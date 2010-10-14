
using MonoTouch.UIKit;
using System.Drawing;
using System;
using MonoTouch.Foundation;
using MonoTouch.AudioToolbox;
using MonoTouch.CoreFoundation;

namespace Monotouch_SoundRecording
{
	public partial class MainViewController : UIViewController
	{
        CFUrl _url;
        AudioQueuePlayer _player;
        AudioQueueRecorder _recorder;

		public MainViewController (string nibName, NSBundle bundle) : base(nibName, bundle)
		{
			// Custom initialization
		}

		
		// Implement viewDidLoad to do additional setup after loading the view, typically from a nib.
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            // setting button stat
            _recordingButton.Enabled = true;
            _playBackButton.Enabled = false;

            // binding event handlers
            _recordingButton.TouchUpInside += new EventHandler(_recordingButton_TouchCancel);
            _playBackButton.TouchUpInside  += new EventHandler(_playBackButton_TouchDown);

            // getting local sound file path
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            path = System.IO.Path.Combine(path, "recording.aiff");
			_url = CFUrl.FromFile(path);
			
            // setting audio session
            AudioSession.Initialize();
            AudioSession.SetActive(true);

            
        }

        void _recordingButton_TouchCancel(object sender, EventArgs e)
        {
            if (_recorder == null)
            {
                // during playing, it can not record.
                stopPlayback();

                _playBackButton.Enabled = false;

                // setting audio session
                AudioSession.Category = AudioSessionCategory.RecordAudio;

                _recorder = new AudioQueueRecorder(_url);
                _recorder.Start();
                _recordingButton.SetTitle("Stop Recording", UIControlState.Normal);
            }
            else
            {
                _recorder.Dispose();
                _recorder = null;
                _recordingButton.SetTitle("Start Recording", UIControlState.Normal);
                _playBackButton.Enabled = true;

                // setting audio session
                AudioSession.Category = AudioSessionCategory.MediaPlayback;
            }
        }

        void _playBackButton_TouchDown(object sender, EventArgs e)
        {
            if (_player == null && _recorder == null)
            {
                _player = new AudioQueuePlayer(_url);
                _player.Play();
                _playBackButton.SetTitle("Stop", UIControlState.Normal);
            }
            else
            {
                stopPlayback();
            }         
        }        

        void stopPlayback()
        {
            if (_player == null) return;

            _player.Stop();
            _player = null;
            _playBackButton.SetTitle("Playback", UIControlState.Normal);
        }
		/*
		partial void showInfo (UIButton sender)
		{
			var controller = new FlipsideViewController ("FlipsideView", null);
			controller.Done += delegate { this.DismissModalViewControllerAnimated (true); };
			controller.ModalTransitionStyle = UIModalTransitionStyle.FlipHorizontal;
			this.PresentModalViewController (controller, true);
		}*/


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

	}
}

