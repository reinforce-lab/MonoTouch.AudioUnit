//
//  RemoteOutput.m
//  RemoteIO
//
//  Created by Norihisa Nagano
//

#import "RemoteOutput.h"

@implementation RemoteOutput

@synthesize phase;
@synthesize sampleRate;

static OSStatus renderCallback(void*                       inRefCon,
                               AudioUnitRenderActionFlags* ioActionFlags,
                               const AudioTimeStamp*       inTimeStamp,
                               UInt32                      inBusNumber,
                               UInt32                      inNumberFrames,
                               AudioBufferList*            ioData){
    //[1]RemoteOutputのインスタンスにキャストする
    RemoteOutput* def = (RemoteOutput*)inRefCon;    
    //サイン波の計算に使う数値の用意
    float freq = 440 * 2.0 * M_PI / def.sampleRate;
    //phaseはサウンドの再生中に継続して使うため、RemoteOutputのプロパティとする
    double phase = def.phase;
    //[2]値を書き込むポインタ
    AudioUnitSampleType *outL = ioData->mBuffers[0].mData;
    AudioUnitSampleType *outR = ioData->mBuffers[1].mData;
    
    for (int i = 0; i< inNumberFrames; i++){
        //[3]サイン波を計算
        float wave = sin(phase);
        //[4] 8.24固定小数点に変換
        AudioUnitSampleType sample = wave * (1 << kAudioUnitSampleFractionBits);
        *outL++ = sample;
        *outR++ = sample;
        phase = phase + freq;
    }
    def.phase = phase;
    return noErr;
}

- (id)init{
    self = [super init];
    if (self != nil)[self prepareAudioUnit];
    return self;
}

-(BOOL)isPlaying{
    return isPlaying;
}

- (void)prepareAudioUnit{    
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
    callbackStruct.inputProc = renderCallback;//コールバック関数の設定
    callbackStruct.inputProcRefCon = self;//コールバック関数内で参照するデータのポインタ
    
    AudioUnitSetProperty(audioUnit, 
                         kAudioUnitProperty_SetRenderCallback, 
                         //サイン波の値はAudioUnitに入ってくるものなのでScopeはInput
                         kAudioUnitScope_Input,
                         0,   // 0 == スピーカー
                         &callbackStruct,
                         sizeof(AURenderCallbackStruct));
    sampleRate = 44100.0;
    
    AudioStreamBasicDescription audioFormat;
    audioFormat.mSampleRate         = sampleRate;
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
}

-(void)play{
    if(!isPlaying)AudioOutputUnitStart(audioUnit);
    isPlaying = YES;
}

-(void)stop{
    if(isPlaying)AudioOutputUnitStop(audioUnit);
    isPlaying = NO;
}

- (void)dealloc{
    if(isPlaying)AudioOutputUnitStop(audioUnit);
    AudioUnitUninitialize(audioUnit);
    AudioComponentInstanceDispose(audioUnit);
    [super dealloc];
}

@end