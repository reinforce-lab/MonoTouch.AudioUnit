//
//  ExtAudioAppDelegate.h
//  ExtAudio
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>

@class ExtAudioViewController;

@interface ExtAudioAppDelegate : NSObject <UIApplicationDelegate> {
    UIWindow *window;
    ExtAudioViewController *viewController;
}

@property (nonatomic, retain) IBOutlet UIWindow *window;
@property (nonatomic, retain) IBOutlet ExtAudioViewController *viewController;

@end

