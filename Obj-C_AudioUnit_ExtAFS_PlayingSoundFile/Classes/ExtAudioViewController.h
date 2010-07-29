//
//  ExtAudioViewController.h
//  ExtAudio
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>
#import <AudioToolbox/AudioToolbox.h>
#import <AudioToolbox/ExtendedAudioFile.h>
#import "ExtAudioFilePlayer.h"

@interface ExtAudioViewController : UIViewController {
    ExtAudioFilePlayer *player;
    
    IBOutlet UISlider *slider;
    NSTimer *timer;
}

-(IBAction)play:(id)sender;
-(IBAction)stop:(id)sender;
-(IBAction)sliderAction:(UISlider*)sender;

-(void)startTimer;
-(void)stopTimer;
@end