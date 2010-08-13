using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Monotouch_AudioUnit_SoundTriggeredPlayingSoundMemoryBased
{
	public partial class MainView : UIViewController
    {
        #region Variables
        ExtAudioBufferPlayer _player;
        NSTimer _timer;
        bool _isTimerAvailable;
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

            var url = MonoTouch.CoreFoundation.CFUrl.FromFile("loop_stereo.aif");
            _player = new ExtAudioBufferPlayer(url);

            // setting audio session
            _slider.ValueChanged += new EventHandler(_slider_ValueChanged);
            _playButton.TouchDown += new EventHandler(_playButton_TouchDown);
            _stopButton.TouchDown += new EventHandler(_stopButton_TouchDown);

            _slider.MaxValue = _player.TotalFrames;

            _isTimerAvailable = true;
            _timer = NSTimer.CreateRepeatingTimer(TimeSpan.FromMilliseconds(100),
                delegate {
                    if (_isTimerAvailable)
                    {
                        long pos = _player.CurrentPosition;
                        _slider.Value = pos;
                        _signalLevelLabel.Text = _player.SignalLevel.ToString("0.00E0");
                        //System.Diagnostics.Debug.WriteLine("CurPos: " + _player.CurrentPosition.ToString());
                    }                    
                }
                );
            NSRunLoop.Current.AddTimer(_timer, "NSDefaultRunLoopMode");            
        }

        void _slider_ValueChanged(object sender, EventArgs e)
        {
            _isTimerAvailable = false; 
            _player.CurrentPosition = (long)_slider.Value;            
            _isTimerAvailable = true;             
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            _player.Dispose();
        }

        void _stopButton_TouchDown(object sender, EventArgs e)
        {
            //_player.Stop();            
        }

        void _playButton_TouchDown(object sender, EventArgs e)
        {
            //_player.Play();            
        }
        #endregion
    }
}

