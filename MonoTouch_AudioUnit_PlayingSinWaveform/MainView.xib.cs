using System;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Monotouch_AudioUnit_PlayingSinWaveform
{
	public partial class MainView : UIViewController
    {
        #region Variables
        RemoteOutput _player;
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

            _player = new RemoteOutput();

            _playButton.TouchDown += new EventHandler(_playButton_TouchDown);
            _stopButton.TouchDown += new EventHandler(_stopButton_TouchDown);

            _playButton.Enabled = true;
            _stopButton.Enabled = false;
        }
        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            _player.Dispose();
        }
        void _stopButton_TouchDown(object sender, EventArgs e)
        {
            _player.Stop();

            _playButton.Enabled = true;
            _stopButton.Enabled = false;
        }

        void _playButton_TouchDown(object sender, EventArgs e)
        {
            _player.Play();

            _playButton.Enabled = false;
            _stopButton.Enabled = true;
        }
        #endregion
    }
}

