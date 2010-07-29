//
//  RemoteOutput.h
//  RemoteIO
//
//  Created by Norihisa Nagano
//

#import <AudioUnit/AudioUnit.h>

@interface RemoteOutput : NSObject {
    AudioUnit audioUnit;
    double phase;
    Float64 sampleRate;
    BOOL isPlaying;    
}

-(void)play;
-(void)stop;

@property(nonatomic) double phase;
@property(nonatomic) Float64 sampleRate;

- (void)prepareAudioUnit;
@end