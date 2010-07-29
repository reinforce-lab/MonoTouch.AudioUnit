//
//  AUAudioFilePlayer.m
//  ExtAudio
//
//  Created by Norihisa Nagano
//

#import "ExtAudioFilePlayer.h"


@implementation ExtAudioFilePlayer

@synthesize extAudioFile;
@synthesize currentFrame;
@synthesize totalFrames;

static void checkError(OSStatus err,const char *message){
    if(err){
        char property[5];
        *(UInt32 *)property = CFSwapInt32HostToBig(err);
        property[4] = '\0';
        NSLog(@"%s = %-4.4s, %d",message, property,err);
        exit(1);
    }
}

static OSStatus renderCallback(
                               void*                       inRefCon,
                               AudioUnitRenderActionFlags* ioActionFlags,
                               const AudioTimeStamp*       inTimeStamp,
                               UInt32                      inBusNumber,
                               UInt32                      inNumberFrames,
                               AudioBufferList*            ioData){
    OSStatus err;
    ExtAudioFilePlayer* def = (ExtAudioFilePlayer*)inRefCon;
    
    UInt32 ioNumberFrames = inNumberFrames;
    //バッファに読み込む
	err = ExtAudioFileRead(def.extAudioFile, &ioNumberFrames, ioData);
	if(err){
		NSLog(@"ExtAudioFileRead err = %d",err);
        return -1;
	}
    //ファイルの末尾に達した
    if(ioNumberFrames != inNumberFrames){
        //先頭に戻す
        ExtAudioFileSeek(def.extAudioFile, 0);
        [def stop];
    }
    return noErr;
}

//再生位置の設定
-(void)setCurrentFrame:(SInt64)frame{
    //frameがtotalFramesを超える場合とマイナスの値の場合、制限する
    if(frame > totalFrames)frame = totalFrames;
    if(frame < 0)frame = 0;
    ExtAudioFileSeek(extAudioFile, frame);
}

//再生位置の取得
-(SInt64)currentFrame{
    SInt64 frame;
    ExtAudioFileTell(extAudioFile, &frame);
    return frame;
}

-(id)initWithContentsOfURL:(NSURL*)url{
    self = [super init];
    if (self != nil){
        [self prepareExtAudio:url];
        [self prepareAudioUnit];
    }
    return self;
}


-(void)prepareExtAudio:(NSURL*)fileURL{
    OSStatus err;    
    //ExAudioFileの作成
    err = ExtAudioFileOpenURL((CFURLRef)fileURL, &extAudioFile);
    checkError(err,"ExtAudioFileOpenURL");
    
	//ファイルデータフォーマットを取得[1]
    AudioStreamBasicDescription inputFormat;
    UInt32 size = sizeof(AudioStreamBasicDescription);
    err = ExtAudioFileGetProperty(extAudioFile, 
                                  kExtAudioFileProperty_FileDataFormat, 
                                  &size,
                                  &inputFormat);
    checkError(err,"kExtAudioFileProperty_FileDataFormat");
    
    //チャンネル数を再生するファイルのチャンネル数に合わせる
    outputFormat = AUCanonicalASBD(44100.0,inputFormat.mChannelsPerFrame);
    
    //読み込むフォーマットをAudio Unit正準形に設定[3]
    err = ExtAudioFileSetProperty(extAudioFile,
                                  kExtAudioFileProperty_ClientDataFormat, 
                                  sizeof(AudioStreamBasicDescription), 
                                  &outputFormat);
    checkError(err,"kExtAudioFileProperty_ClientDataFormat");
    
    //トータルフレーム数を取得しておく
    SInt64 fileLengthFrames;
    size = sizeof(SInt64);
    err = ExtAudioFileGetProperty(extAudioFile, 
                                  kExtAudioFileProperty_FileLengthFrames, 
                                  &size, 
                                  &fileLengthFrames);
    checkError(err,"kExtAudioFileProperty_FileLengthFrames");
    totalFrames = fileLengthFrames;
    
    //位置を0（先頭）に移動
    ExtAudioFileSeek(extAudioFile, 0);
}

- (void)prepareAudioUnit{
    AudioComponentDescription cd;
    cd.componentType = kAudioUnitType_Output;
    cd.componentSubType = kAudioUnitSubType_RemoteIO;
    cd.componentManufacturer = kAudioUnitManufacturer_Apple;
    cd.componentFlags = 0;
    cd.componentFlagsMask = 0;
    
    AudioComponent component = AudioComponentFindNext(NULL, &cd);    
    AudioComponentInstanceNew(component, &audioUnit);    
    AudioUnitInitialize(audioUnit);
    
    AURenderCallbackStruct callbackStruct;
    callbackStruct.inputProc = renderCallback;
    callbackStruct.inputProcRefCon = self;
    
    AudioUnitSetProperty(audioUnit,
                         kAudioUnitProperty_SetRenderCallback, 
                         kAudioUnitScope_Input,
                         0,
                         &callbackStruct,
                         sizeof(AURenderCallbackStruct));
    
    AudioUnitSetProperty(audioUnit,
                         kAudioUnitProperty_StreamFormat,
                         kAudioUnitScope_Input,
                         0,
                         &outputFormat,
                         sizeof(AudioStreamBasicDescription));
}

-(void)play{
    if(!isPlaying){
        isPlaying = YES;        
        AudioOutputUnitStart(audioUnit);
    }
}

-(void)stop{
    if(isPlaying){
        AudioOutputUnitStop(audioUnit);
    }
    isPlaying = NO;
}

- (void)dealloc{
    
    if(isPlaying)AudioOutputUnitStop(audioUnit);
    AudioUnitUninitialize(audioUnit);
    AudioComponentInstanceDispose(audioUnit);
    [super dealloc];
}

@end