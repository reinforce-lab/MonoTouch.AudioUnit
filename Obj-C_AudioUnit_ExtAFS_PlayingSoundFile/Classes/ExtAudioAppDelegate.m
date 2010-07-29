//
//  ExtAudioAppDelegate.m
//  ExtAudio
//
//  Created by Norihisa Nagano
//

#import "ExtAudioAppDelegate.h"
#import "ExtAudioViewController.h"

@implementation ExtAudioAppDelegate

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
