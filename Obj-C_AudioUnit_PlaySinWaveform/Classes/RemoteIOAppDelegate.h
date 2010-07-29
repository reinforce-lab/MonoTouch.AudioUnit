//
//  RemoteIOAppDelegate.h
//  RemoteIO
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>

@class RemoteIOViewController;

@interface RemoteIOAppDelegate : NSObject <UIApplicationDelegate> {
    UIWindow *window;
    RemoteIOViewController *viewController;
}

@property (nonatomic, retain) IBOutlet UIWindow *window;
@property (nonatomic, retain) IBOutlet RemoteIOViewController *viewController;

@end

