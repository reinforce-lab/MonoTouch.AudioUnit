//
//  RemoteIOViewController.m
//  RemoteIO
//
//  Created by Norihisa Nagano
//

#import "RemoteIOViewController.h"

@implementation RemoteIOViewController

- (void)viewDidLoad {
    remoteOutput = [[RemoteOutput alloc]init];
}

-(IBAction)play:(id)sender{
    [remoteOutput play];
}
-(IBAction)stop:(id)sender{
    [remoteOutput stop];
}

- (void)dealloc {
    [remoteOutput release];
    [super dealloc];
}

@end