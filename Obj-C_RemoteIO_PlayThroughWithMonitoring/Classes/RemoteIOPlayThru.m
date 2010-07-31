//
//  RemoteIOPlayThru.m
//  RemoteIOPlayThru
//
//  Created by Norihisa Nagano
//

#import "RemoteIOPlayThru.h"

@implementation RemoteIOPlayThru

static OSStatus renderCallback(
                               void *						inRefCon,
                               AudioUnitRenderActionFlags 		*ioActionFlags,
                               const AudioTimeStamp *			inTimeStamp,
                               UInt32						inBusNumber,
                               UInt32						inNumberFrames,
                               AudioBufferList *				ioData){
    //Post Render時のみ処理を行う
    if (*ioActionFlags & kAudioUnitRenderAction_PostRender){
        ExtAudioFileRef extAudioFile = (ExtAudioFileRef)inRefCon;
        //バッファを書き込む
        ExtAudioFileWriteAsync(extAudioFile, inNumberFrames, ioData);
    }
    return noErr;
}


- (id) init{
    self = [super init];
    if (self != nil) {
        [self prepareAUGraph];
    }
    return self;
}

-(void)prepareAUGraph{
    OSStatus err;
    AUNode remoteIONode;
    AudioUnit remoteIOUnit;
    
    NewAUGraph(&mAuGraph);
    AUGraphOpen(mAuGraph);
    
    AudioComponentDescription cd;
	cd.componentType = kAudioUnitType_Output;
	cd.componentSubType = kAudioUnitSubType_RemoteIO;
	cd.componentManufacturer = kAudioUnitManufacturer_Apple;
	cd.componentFlags = cd.componentFlagsMask = 0;
	
	AUGraphAddNode(mAuGraph, &cd, &remoteIONode);
    AUGraphNodeInfo(mAuGraph, remoteIONode, NULL, &remoteIOUnit);
    
    //マイク入力をオンにする
    UInt32 flag = 1;
    AudioUnitSetProperty(remoteIOUnit,
                         kAudioOutputUnitProperty_EnableIO,
                         kAudioUnitScope_Input,
                         1, //Remote Input
                         &flag,
                         sizeof(flag));
    //オーディオ正準形
    AudioStreamBasicDescription audioFormat = CanonicalASBD(44100.0, 1);
    AudioUnitSetProperty(remoteIOUnit,
                         kAudioUnitProperty_StreamFormat,
                         kAudioUnitScope_Output, //Remote inputのアウトプットバス
                         1, //Remote input
                         &audioFormat,
                         sizeof(AudioStreamBasicDescription));
    
    AudioUnitSetProperty(remoteIOUnit,
                         kAudioUnitProperty_StreamFormat,
                         kAudioUnitScope_Input,
                         0, //Remote output
                         &audioFormat,
                         sizeof(AudioStreamBasicDescription));
    
    AUGraphConnectNodeInput(mAuGraph,
                            remoteIONode, 1, //Remote Inputと 
                            remoteIONode, 0  //Remote Outputを接続
                            );
    
    UInt32 size;
    AudioUnitGetProperty(remoteIOUnit,
                         kAudioUnitProperty_StreamFormat,
                         kAudioUnitScope_Output, //Remote oupputのアウトプットバス
                         0, //Remote output
                         &audioUnitOutputFormat,
                         &size);
    
    AUGraphInitialize(mAuGraph);
}

-(void)recording:(NSURL*)toURL{
    if(isRecording)return;
    
    //変換するフォーマット(AIFF)
    AudioStreamBasicDescription outputFormat;
    outputFormat.mSampleRate         = 44100.0;
    outputFormat.mFormatID			= kAudioFormatLinearPCM;
    outputFormat.mFormatFlags		= kAudioFormatFlagIsBigEndian 
	| kLinearPCMFormatFlagIsSignedInteger 
	| kLinearPCMFormatFlagIsPacked;
    outputFormat.mFramesPerPacket	= 1;
    outputFormat.mChannelsPerFrame	= 1;
    outputFormat.mBitsPerChannel    = 16;
    outputFormat.mBytesPerPacket    = 2;
    outputFormat.mBytesPerFrame		= 2;
    outputFormat.mReserved			= 0;
    
    ExtAudioFileCreateWithURL((CFURLRef)toURL, 
                              kAudioFileAIFFType,//AIFFで保存する, 
                              &outputFormat,
                              NULL, 
                              kAudioFileFlags_EraseFile, 
                              &extAudioFile);
    
    //Remote OutputのアウトプットのASBDが入力
    ExtAudioFileSetProperty(extAudioFile,
                            kExtAudioFileProperty_ClientDataFormat, 
                            sizeof(AudioStreamBasicDescription), 
                            &audioUnitOutputFormat);
    
    ExtAudioFileSeek(extAudioFile, 0);
    
    AUGraphAddRenderNotify(mAuGraph, renderCallback, extAudioFile);
    isRecording = YES;
    
}

-(void)stopRecording{
    //通知を受けない
    AUGraphRemoveRenderNotify(mAuGraph, renderCallback, extAudioFile);
    //Ext Audioを閉じる
    ExtAudioFileDispose(extAudioFile);
    isRecording = NO;
}

-(void)play{
    if(!isPlaying)AUGraphStart(mAuGraph);
    isPlaying = YES;
}

-(void)stop{
    if(isPlaying)AUGraphStop(mAuGraph);
    isPlaying = NO;
}


- (void)dealloc{
    [self stop];
    AUGraphUninitialize(mAuGraph);
	AUGraphClose(mAuGraph);
    DisposeAUGraph(mAuGraph);
    [super dealloc];
}

@end
