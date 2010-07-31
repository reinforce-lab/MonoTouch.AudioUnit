/*
 *  iPhoneCoreAudio.h
 *
 *  Created by Norihisa Nagano
 *
 */

#if !defined(__iPhoneCoreAudio_h__)
#define __iPhoneCoreAudio_h__
#include <AudioToolbox/AudioToolbox.h>

static void checkError(OSStatus err,const char *message){
    if(err){
        char property[5];
        *(UInt32 *)property = CFSwapInt32HostToBig(err);
        property[4] = '\0';
        NSLog(@"%s = %-4.4s, %d",message, property,err);
    }
}


extern AudioStreamBasicDescription AUCanonicalASBD(Float64 sampleRate, 
                                                   UInt32 channel);
extern AudioStreamBasicDescription CanonicalASBD(Float64 sampleRate, 
                                                 UInt32 channel);
extern void printASBD(AudioStreamBasicDescription audioFormat);
#endif  //  __iPhoneCoreAudio_h__