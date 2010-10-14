
using MonoTouch.UIKit;
using System.Drawing;
using System;
using MonoTouch.Foundation;


namespace Monotouch_SoundRecording
{

	public partial class FlipsideViewController : UIViewController
	{
		public FlipsideViewController (string nibName, NSBundle bundle) : base(nibName, bundle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.ViewFlipsideBackgroundColor;
		}

		/*
		// Override to allow orientations other than the default portrait orientation.
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (InterfaceOrientation == UIInterfaceOrientation.Portrait);
		}
		*/

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidUnload ()
		{
			// Release any retained subviews of the main view.
			// e.g. this.myOutlet = null;
		}

		partial void done (UIBarButtonItem sender)
		{
			if (Done != null)
				Done (this, EventArgs.Empty);
		}

		public event EventHandler Done;
	}
}

