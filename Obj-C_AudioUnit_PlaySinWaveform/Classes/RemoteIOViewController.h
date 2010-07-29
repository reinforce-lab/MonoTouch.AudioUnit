//
//  RemoteIOViewController.h
//  RemoteIO
//
//  Created by Norihisa Nagano
//

#import <UIKit/UIKit.h>
#import "RemoteOutput.h"
@interface RemoteIOViewController : UIViewController {
    RemoteOutput *remoteOutput;
}

-(IBAction)play:(id)sender;
-(IBAction)stop:(id)sender;
@end