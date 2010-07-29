//
//  ExtAudioViewController.m
//  ExtAudio
//
//  Created by Norihisa Nagano
//

#import "ExtAudioViewController.h"

@implementation ExtAudioViewController

- (void)viewDidLoad {
    NSString *path;
    path = [[NSBundle mainBundle] pathForResource:@"loop_stereo" ofType:@"aif"];
    //path = [[NSBundle mainBundle] pathForResource:@"loop_mono" ofType:@"wav"];
    NSURL *fileURL = [NSURL fileURLWithPath:path];
    
    player = [[ExtAudioFilePlayer alloc]initWithContentsOfURL:fileURL];
    slider.maximumValue = player.totalFrames;
    [self startTimer];
}


-(void)updateSlider{
    slider.value = player.currentFrame;
}


-(IBAction)sliderAction:(UISlider*)sender{
    [self stopTimer];
    player.currentFrame = (SInt64)sender.value;
    [self startTimer];
}

-(IBAction)play:(id)sender{
    [player play];
}
-(IBAction)stop:(id)sender{
    [player stop];
}

-(void)startTimer{
    timer = [NSTimer scheduledTimerWithTimeInterval:0.1 
                                             target:self 
                                           selector:@selector(updateSlider) 
                                           userInfo:nil 
                                            repeats:YES];
    [timer retain];
}

-(void)stopTimer{
    [timer invalidate];
}

- (void)dealloc {
    [self stopTimer];
    [player release];
    [super dealloc];
}

@end
