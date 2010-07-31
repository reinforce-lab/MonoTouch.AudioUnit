//
//  RemoteIOPlayThruViewController.h
//  RemoteIOPlayThru
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>
#import "RemoteIOPlayThru.h"
#import <AVFoundation/AVFoundation.h>

@interface RemoteIOPlayThruViewController : UIViewController {
    RemoteIOPlayThru *playThru;
    NSURL *recordingURL;
}

-(IBAction)startRecording:(id)sender;
-(IBAction)stopRecording:(id)sender;
@end