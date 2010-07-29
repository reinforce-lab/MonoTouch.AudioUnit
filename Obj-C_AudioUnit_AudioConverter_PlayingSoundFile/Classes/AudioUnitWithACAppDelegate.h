//
//  AudioUnitWithACAppDelegate.h
//  AudioUnitWithAC
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>

@class AudioUnitWithACViewController;

@interface AudioUnitWithACAppDelegate : NSObject <UIApplicationDelegate> {
    UIWindow *window;
    AudioUnitWithACViewController *viewController;
}

@property (nonatomic, retain) IBOutlet UIWindow *window;
@property (nonatomic, retain) IBOutlet AudioUnitWithACViewController *viewController;

@end

