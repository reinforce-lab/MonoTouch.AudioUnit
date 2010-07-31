//
//  RemoteIOPlayThru.h
//  RemoteIOPlayThru
//
//  Created by Norihisa Nagano
//

#import <Foundation/Foundation.h>
#import "iPhoneCoreAudio.h"
#import <AudioUnit/AudioUnit.h>
#import <AudioToolbox/ExtendedAudioFile.h>

@interface RemoteIOPlayThru : NSObject {
    AUGraph mAuGraph;
    BOOL isPlaying;
    
    BOOL isRecording;
    ExtAudioFileRef extAudioFile;
    //Remote OutputのアウトプットのASBD
    AudioStreamBasicDescription audioUnitOutputFormat;
}

-(void)recording:(NSURL*)toURL;
-(void)stopRecording;

-(void)prepareAUGraph;
-(void)play;
-(void)stop;
@end