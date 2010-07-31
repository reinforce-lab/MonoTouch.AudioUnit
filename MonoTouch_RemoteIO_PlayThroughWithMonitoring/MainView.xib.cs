using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.AVFoundation;
using MonoTouch.UIKit;

namespace Monotouch_RemoteIO_PlayThroughWithMonitoring
{
	public partial class MainView : UIViewController
    {
        #region Variables
        RemoteIOPlayThrough _player;
        MonoTouch.CoreFoundation.CFUrl _url;
        #endregion

        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public MainView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public MainView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public MainView () : base("MainView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		
		#endregion

        #region Public methods
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }
        public override void LoadView()
        {
            base.LoadView();

        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
     
            /*
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            path = System.IO.Path.Combine(path, "loop_stereo.aif"); // loop_mono.wav
            if (!System.IO.File.Exists(path))
                throw new ArgumentException("file not found; " + path);*/

            _url = MonoTouch.CoreFoundation.CFUrl.FromFile("output.aif");
            _player = new RemoteIOPlayThrough();

            // setting audio session
            _playButton.TouchDown += new EventHandler(_playButton_TouchDown);
            _stopButton.TouchDown += new EventHandler(_stopButton_TouchDown);
        }


        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
        }

        void _stopButton_TouchDown(object sender, EventArgs e)
        {
            _player.StopRecording();


            var player = AVAudioPlayer.FromUrl(MonoTouch.Foundation.NSUrl.FromFilename("output.aif"));
            player.Play();
        }

        void _playButton_TouchDown(object sender, EventArgs e)
        {
            _player.StartRecording(_url);            
        }
        #endregion
    }
}

