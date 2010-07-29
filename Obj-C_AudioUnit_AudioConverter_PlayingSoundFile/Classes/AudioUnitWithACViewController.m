//
//  AudioUnitWithACViewController.m
//  AudioUnitWithAC
//
//  Created by Norihisa Nagano
//

#import "AudioUnitWithACViewController.h"

@implementation AudioUnitWithACViewController


- (void)viewDidLoad {
    NSString *path;
    path = [[NSBundle mainBundle] pathForResource:@"loop" ofType:@"mp3"];
    //path = [[NSBundle mainBundle] pathForResource:@"loop" ofType:@"wav"];
    
    NSURL *url = [NSURL fileURLWithPath:path];
    player = [[AUAudioFilePlayer alloc]initWithContentsOfURL:url];
    
    UInt64 totalPacketCount = [player totalPacketCount];
    slider.maximumValue = totalPacketCount;
    
    [self startTimer];
}


-(void)updateSlider{
    [self stopTimer];
    slider.value = [player currentPosition];
    [self startTimer];
}

-(IBAction)loopSwitchAction:(UISwitch*)sender{
    player.isLoop = sender.isOn;
}

-(IBAction)sliderAction:(UISlider*)sender{
    player.currentPosition = (SInt64)sender.value;
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
    [player stop];
    [player release];
    [self stopTimer];
    [super dealloc];
}

@end