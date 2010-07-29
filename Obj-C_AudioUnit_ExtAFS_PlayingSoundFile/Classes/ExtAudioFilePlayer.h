//
//  AUAudioFilePlayer.h
//  ExtAudio
//
//  Created by Norihisa Nagano
//

#import <Foundation/Foundation.h>
#import <AudioUnit/AudioUnit.h>
#import <AudioToolbox/AudioToolbox.h>
#import <AudioToolbox/ExtendedAudioFile.h>
#import "iPhoneCoreAudio.h"

@interface ExtAudioFilePlayer : NSObject{
    AudioUnit audioUnit;    
    AudioStreamBasicDescription outputFormat; //出力フォーマット(Audio Unit正準形)
    BOOL isPlaying;
    SInt64 totalFrames;
    ExtAudioFileRef extAudioFile;
}

@property(readonly)ExtAudioFileRef extAudioFile;
@property(readonly)SInt64 totalFrames; //トータルフレーム数
@property SInt64 currentFrame; //現在のフレーム

-(id)initWithContentsOfURL:(NSURL*)url;

-(void)stop;
-(void)play;

//private
-(void)prepareAudioUnit;
-(void)prepareExtAudio:(NSURL*)fileURL;

@end
