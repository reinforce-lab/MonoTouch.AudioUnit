//
//  RemoteIOAppDelegate.m
//  RemoteIO
//
//  Created by Norihisa Nagano
//

#import "RemoteIOAppDelegate.h"
#import "RemoteIOViewController.h"

@implementation RemoteIOAppDelegate

@synthesize window;
@synthesize viewController;


- (void)applicationDidFinishLaunching:(UIApplication *)application {    
    
    // Override point for customization after app launch    
    [window addSubview:viewController.view];
    [window makeKeyAndVisible];
}


- (void)dealloc {
    [viewController release];
    [window release];
    [super dealloc];
}


@end
