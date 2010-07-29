//
//  ConverterOutput.m
//  AudioUnitWithAC
//
//  Created by Norihisa Nagano
//

#import "AUAudioFilePlayer.h"

@implementation AUAudioFilePlayer

@synthesize isLoop;

static void checkError(OSStatus err,const char *message){
    if(err){
        char property[5];
        *(UInt32 *)property = CFSwapInt32HostToBig(err);
        NSLog(@"%s = %-4.4s",message, property);
        exit(1);
    }
}

static void writeDecompressionMagicCookie(AudioConverterRef audioConverter, AudioFileID infile){
    UInt32 propertySize;
    OSStatus err = AudioFileGetPropertyInfo(infile, 
                                            kAudioFilePropertyMagicCookieData, 
                                            &propertySize, 
                                            NULL);
    
    if (err == noErr && propertySize > 0){
        UInt8*  magicCookie = malloc(sizeof(UInt8) * propertySize);
        UInt32	magicCookieSize = propertySize;
        AudioFileGetProperty(infile,
                             kAudioFilePropertyMagicCookieData,
                             &propertySize,
                             magicCookie);
        
        AudioConverterSetProerty(audioConverter,
                                  kAudioConverterDecompressionMagicCookie,
                                  magicCookieSize,
                                  magicCookie);
        free(magicCookie);
    }
}


OSStatus EncoderDataProc(
    AudioConverterRef				inAudioConverter, 
    UInt32*                         ioNumberDataPackets,
    AudioBufferList*				ioData,
    AudioStreamPacketDescription**	outDataPacketDescription,
    void*							inUserData)
{
    AudioFileIO* audioFileIO = (AudioFileIO*)inUserData;
    if(audioFileIO->isDone){
        *ioNumberDataPackets =
        return -1; //任意のエラーを返す
    }
    UInt32 maxPackets 
    = audioFileIO->srcBufferSize / audioFileIO->srcSizePerPacket;
    if (*ioNumberDataPackets > maxPackets)
        *ioNumberDataPackets = maxPackets;
    
    UInt32 outNumBytes;
    OSStatus err;
    err = AudioFileReadPackets(audioFileIO->audioFileID,
                               NO, 
                               &outNumBytes, 
                               audioFileIO->packetDescs, 
                               audioFileIO->startingPacketCount,
                               ioNumberDataPackets, 
                               audioFileIO->srcBuffer); 
    if(err)return err;
    
    audioFileIO->startingPacketCount += *ioNumberDataPackets;
    
    ioData->mBuffers[0].mNumberChannels 
            = audioFileIO->srcFormat.mChannelsPerFrame;
    ioData->mBuffers[0].mDataByteSize = outNumBytes;
    ioData->mBuffers[0].mData = audioFileIO->srcBuffer;
    
    if (outDataPacketDescription) {
        if (audioFileIO->packetDescs){
            *outDataPacketDescription = audioFileIO->packetDescs;
        }else{
            *outDataPacketDescription = NULL;
        }
    }
    
    if(audioFileIO->startingPacketCount == audioFileIO->totalPacketCount){
        if(audioFileIO->isLoop){
            audioFileIO->startingPacketCount = 0;
        }else{
            audioFileIO->isDone = YES;
        }
    }
    return err;
}

static OSStatus renderCallback(
                               void*                       inRefCon,
                               AudioUnitRenderActionFlags* ioActionFlags,
                               const AudioTimeStamp*       inTimeStamp,
                               UInt32                      inBusNumber,
                               UInt32                      inNumberFrames,
                               AudioBufferList*            ioData){
    //fillBufferを呼び出すだけ
    AUAudioFilePlayer* def = (AUAudioFilePlayer*)inRefCon;
    [def fillBuffer:ioData inNumberFrames:inNumberFrames];
    return noErr;
}

-(void)fillBuffer:(AudioBufferList*)ioData inNumberFrames:(UInt32)inNumberFrames{
    UInt32 ioOutputDataPackets = inNumberFrames;
    OSStatus err;
    err = AudioConverterFillComplexBuffer(
                      audioConverter, 
                      EncoderDataProc, 
                      &audioFileIO, 
                      &ioOutputDataPackets, 
                      ioData, 
                      audioFileIO.packetDescs);
    if(err || ioOutputDataPackets == 0) {
        audioFileIO.startingPacketCount = 0;
        [self stop];
    }
}

- (id)initWithContentsOfURL:(NSURL*)url{
    self = [super init];
    if (self != nil){
        [self prepareAudioUnit];
        [self prepareAudioConverter:url];
    }
    return self;
}



//変更無し
-(void)prepareAudioUnit{
    //RemoteIO Audio UnitのAudioComponentDescriptionを作成
    AudioComponentDescription cd;
    cd.componentType = kAudioUnitType_Output;
    cd.componentSubType = kAudioUnitSubType_RemoteIO;
    cd.componentManufacturer = kAudioUnitManufacturer_Apple;
    cd.componentFlags = 0;
    cd.componentFlagsMask = 0;
    
    //AudioComponentDescriptionからAudioComponentを取得
    AudioComponent component = AudioComponentFindNext(NULL, &cd);
    
    //AudioComponentとAudioUnitのアドレスを渡してAudioUnitを取得
    AudioComponentInstanceNew(component, &audioUnit);
    
    //AudioUnitを初期化
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
    
    AudioStreamBasicDescription audioFormat;
    audioFormat.mSampleRate         = 44100.0;
    audioFormat.mFormatID           = kAudioFormatLinearPCM;
    audioFormat.mFormatFlags        = kAudioFormatFlagsAudioUnitCanonical;
    audioFormat.mChannelsPerFrame   = 2;
    audioFormat.mBytesPerPacket     = sizeof(AudioUnitSampleType);
    audioFormat.mBytesPerFrame      = sizeof(AudioUnitSampleType);
    audioFormat.mFramesPerPacket    = 1;
    audioFormat.mBitsPerChannel     = 8 * sizeof(AudioUnitSampleType);
    audioFormat.mReserved           = 0;
        
    AudioUnitSetProperty(audioUnit,
                         kAudioUnitProperty_StreamFormat,
                         kAudioUnitScope_Input,
                         0,
                         &audioFormat,
                         sizeof(audioFormat));
    
    outputFormat = audioFormat;
}

-(void)setIsLoop:(BOOL)flag{
    audioFileIO.isLoop = flag;
}

-(void)setCurrentPosition:(UInt64)position{
    audioFileIO.startingPacketCount = position;
}

-(UInt64)currentPosition{
    return audioFileIO.startingPacketCount;
}

-(UInt64)totalPacketCount{
    return audioFileIO.totalPacketCount - 1;
}

-(void)prepareAudioConverter:(NSURL*)url{
    //Audio Fileの準備
    AudioStreamBasicDescription inputFormat;
    
    OSStatus err = AudioFileOpenURL((CFURLRef)url, 
                                    kAudioFileReadPermission, 
                                    0, 
                                    &audioFileIO.audioFileID);
    checkError (err, "AudioFileOpen");
    
    UInt32 size = sizeof(inputFormat);
    AudioFileGetProperty(audioFileIO.audioFileID,
                         kAudioFilePropertyDataFormat, 
                         &size, 
                         &inputFormat);
    
    err = AudioConverterNew(&inputFormat, &outputFormat, &audioConverter);
    checkError (err, "AudioConverterNew");
    
    //バッファの用意
    audioFileIO.srcBufferSize = 32768;
    audioFileIO.srcBuffer = malloc(sizeof(char) *audioFileIO.srcBufferSize);
    audioFileIO.startingPacketCount = 0;
    audioFileIO.srcFormat = inputFormat;
    
    //pakcketDescriptionを用意する
    if(inputFormat.mBytesPerPacket == 0){
        size = sizeof(audioFileIO.srcSizePerPacket);
        AudioFileGetProperty(audioFileIO.audioFileID, 
                             kAudioFilePropertyPacketSizeUpperBound, 
                             &size, 
                             &audioFileIO.srcSizePerPacket);
        
        audioFileIO.numPacketsToRead = audioFileIO.srcBufferSize / audioFileIO.srcSizePerPacket;
        audioFileIO.packetDescs = malloc(sizeof(AudioStreamPacketDescription) * audioFileIO.numPacketsToRead);
    }else{
        audioFileIO.srcSizePerPacket = inputFormat.mBytesPerPacket;
        audioFileIO.numPacketsToRead = audioFileIO.srcBufferSize / audioFileIO.srcSizePerPacket;
        audioFileIO.packetDescs = NULL;
    }
    
    //総パケット数を取得
    UInt64 totalPacketCount;
    UInt32 propertySize = sizeof(UInt64);
    AudioFileGetProperty(audioFileIO.audioFileID,
                         kAudioFilePropertyAudioDataPacketCount, 
                         &propertySize, 
                         &totalPacketCount);
    audioFileIO.totalPacketCount = totalPacketCount;
    
    //マジッククッキーの処理
    writeDecompressionMagicCookie(audioConverter, audioFileIO.audioFileID);    
}

-(void)play{
    if(!isPlaying){
        AudioOutputUnitStart(audioUnit);
    }
    audioFileIO.isDone = NO;
    isPlaying = YES;
}

-(void)stop{
    if(isPlaying)AudioOutputUnitStop(audioUnit);
    isPlaying = NO;
}

- (void)dealloc{
    if(isPlaying)AudioOutputUnitStop(audioUnit);
    AudioConverterDispose(audioConverter);
    
    if(audioFileIO.packetDescs)free(audioFileIO.packetDescs);
    if(audioFileIO.srcBuffer)free(audioFileIO.srcBuffer);
    if(audioFileIO.audioFileID)AudioFileClose(audioFileIO.audioFileID);
    
    
    AudioUnitUninitialize(audioUnit);
    AudioComponentInstanceDispose(audioUnit);
    [super dealloc];
}


@end