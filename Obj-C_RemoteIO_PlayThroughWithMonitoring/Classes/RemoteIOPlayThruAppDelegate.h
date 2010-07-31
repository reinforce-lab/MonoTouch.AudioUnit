//
//  RemoteIOPlayThruAppDelegate.h
//  RemoteIOPlayThru
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>

@class RemoteIOPlayThruViewController;

@interface RemoteIOPlayThruAppDelegate : NSObject <UIApplicationDelegate> {
    UIWindow *window;
    RemoteIOPlayThruViewController *viewController;
}

@property (nonatomic, retain) IBOutlet UIWindow *window;
@property (nonatomic, retain) IBOutlet RemoteIOPlayThruViewController *viewController;

@end

