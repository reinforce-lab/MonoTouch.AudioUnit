//
//  AudioUnitWithACViewController.h
//  AudioUnitWithAC
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>
#import "AUAudioFilePlayer.h"

@interface AudioUnitWithACViewController : UIViewController {
    AUAudioFilePlayer *player;
    IBOutlet UISlider *slider;
    NSTimer *timer;
}

-(IBAction)play:(id)sender;
-(IBAction)stop:(id)sender;
-(IBAction)sliderAction:(UISlider*)sender;
-(IBAction)loopSwitchAction:(UISwitch*)sender;

-(void)startTimer;
-(void)stopTimer;
@end