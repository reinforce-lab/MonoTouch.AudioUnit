using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Monotouch_SoundRecording
{
	public class Application
	{
		static void Main (string[] args)
		{
			UIApplication.Main (args);
		}
	}

	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{
		MainViewController mainViewController;

		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			mainViewController = new MainViewController ("MainView", null);
			mainViewController.View.Frame = UIScreen.MainScreen.ApplicationFrame;
			window.AddSubview (mainViewController.View);
			window.MakeKeyAndVisible ();
			
			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
		}
	}
}

