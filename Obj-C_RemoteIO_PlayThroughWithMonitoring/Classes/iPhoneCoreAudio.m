/*
 *  iPhoneCoreAudio.c
 *
 *  Created by nagano on 09/07/20.
 *
 */

#include "iPhoneCoreAudio.h"

AudioStreamBasicDescription AUCanonicalASBD(Float64 sampleRate, 
                                            UInt32 channel){
    AudioStreamBasicDescription audioFormat;
    audioFormat.mSampleRate         = sampleRate;
    audioFormat.mFormatID           = kAudioFormatLinearPCM;
    audioFormat.mFormatFlags        = kAudioFormatFlagsAudioUnitCanonical;
    audioFormat.mChannelsPerFrame   = channel;
    audioFormat.mBytesPerPacket     = sizeof(AudioUnitSampleType);
    audioFormat.mBytesPerFrame      = sizeof(AudioUnitSampleType);
    audioFormat.mFramesPerPacket    = 1;
    audioFormat.mBitsPerChannel     = 8 * sizeof(AudioUnitSampleType);
    audioFormat.mReserved           = 0;
    return audioFormat;
}


AudioStreamBasicDescription CanonicalASBD(Float64 sampleRate, 
                                                 UInt32 channel){
    AudioStreamBasicDescription audioFormat;
    audioFormat.mSampleRate         = sampleRate;
    audioFormat.mFormatID           = kAudioFormatLinearPCM;
    audioFormat.mFormatFlags        = kAudioFormatFlagsCanonical;
    audioFormat.mChannelsPerFrame   = channel;
    audioFormat.mBytesPerPacket     = sizeof(AudioSampleType) * channel;
    audioFormat.mBytesPerFrame      = sizeof(AudioSampleType) * channel;
    audioFormat.mFramesPerPacket    = 1;
    audioFormat.mBitsPerChannel     = 8 * sizeof(AudioSampleType);
    audioFormat.mReserved           = 0;
    return audioFormat;
}


void printASBD(AudioStreamBasicDescription audioFormat){
    UInt32 mFormatFlags = audioFormat.mFormatFlags;
    NSMutableArray *flags = [NSMutableArray array];
    
    if(mFormatFlags & kAudioFormatFlagIsFloat)[flags addObject:@"kAudioFormatFlagIsFloat"];
    if(mFormatFlags & kAudioFormatFlagIsBigEndian)[flags addObject:@"kAudioFormatFlagIsBigEndian"];
    if(mFormatFlags & kAudioFormatFlagIsSignedInteger)[flags addObject:@"kAudioFormatFlagIsSignedInteger"];
    if(mFormatFlags & kAudioFormatFlagIsPacked)[flags addObject:@"kAudioFormatFlagIsPacked"];
    if(mFormatFlags & kAudioFormatFlagIsNonInterleaved)[flags addObject:@"kAudioFormatFlagIsNonInterleaved"];
    
    if(mFormatFlags & kAudioFormatFlagIsAlignedHigh)[flags addObject:@"kAudioFormatFlagIsAlignedHigh"];
    if(mFormatFlags & kAudioFormatFlagIsNonMixable)[flags addObject:@"kAudioFormatFlagIsNonMixable"];
    if(mFormatFlags & (kAudioUnitSampleFractionBits << kLinearPCMFormatFlagsSampleFractionShift))[flags addObject:@"(kAudioUnitSampleFractionBits << kLinearPCMFormatFlagsSampleFractionShift)"];
    
    NSMutableString *flagsString = [NSMutableString string];
    for(int i = 0; i < [flags count]; i++){
        [flagsString appendString:[flags objectAtIndex:i]];
        if(i != [flags count] - 1)[flagsString appendString:@"|"];
    }
    if([flags count] == 0)[flagsString setString:@"0"];
    
    char formatID[5];
	*(UInt32 *)formatID = CFSwapInt32HostToBig(audioFormat.mFormatID);
	formatID[4] = '\0';
    
    printf("\n");
    printf("audioFormat.mSampleRate       = %.2f;\n",audioFormat.mSampleRate);
    printf("audioFormat.mFormatID         = '%-4.4s;'\n",formatID);
    printf("audioFormat.mFormatFlags      = %s;\n",[flagsString UTF8String]);
    printf("audioFormat.mBytesPerPacket   = %d;\n",audioFormat.mBytesPerPacket);
    printf("audioFormat.mFramesPerPacket  = %d;\n",audioFormat.mFramesPerPacket);
    printf("audioFormat.mBytesPerFrame    = %d;\n",audioFormat.mBytesPerFrame);
    printf("audioFormat.mChannelsPerFrame = %d;\n",audioFormat.mChannelsPerFrame);
    printf("audioFormat.mBitsPerChannel   = %d;\n",audioFormat.mBitsPerChannel);
    printf("\n");
}