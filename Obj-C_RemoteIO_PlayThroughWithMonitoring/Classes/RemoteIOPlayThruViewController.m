//
//  RemoteIOPlayThruViewController.m
//  RemoteIOPlayThru
//
//  Created by Norihisa Nagano
//

#import "RemoteIOPlayThruViewController.h"

@implementation RemoteIOPlayThruViewController

- (void)viewDidLoad {
    playThru = [[RemoteIOPlayThru alloc]init];
    [playThru play];    
}

-(IBAction)startRecording:(id)sender{
    //書き出すAIFFのパスを作成
    NSArray *filePaths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,
                                                             NSUserDomainMask,YES);
    NSString *documentDirectory = [filePaths objectAtIndex:0];
    NSString *outputPath = [documentDirectory stringByAppendingPathComponent:@"output.aif"];
    recordingURL = [NSURL fileURLWithPath:outputPath];
    [recordingURL retain];
    
    [playThru recording:recordingURL];
}

-(IBAction)stopRecording:(id)sender{
    [playThru stopRecording];
    AVAudioPlayer *player = [[AVAudioPlayer alloc]initWithContentsOfURL:recordingURL error:nil];
    player.delegate = self;
    [player play];
}

- (void)audioPlayerDidFinishPlaying:(AVAudioPlayer *)player successfully:(BOOL)flag{
    [player release];
}

- (void)dealloc {
    [super dealloc];
}

@end
