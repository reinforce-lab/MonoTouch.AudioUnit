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
            _player.WaveFormType = WaveFormType.Sin;
            _player.Period = 44;

            _playButton.TouchDown += new EventHandler(_playButton_TouchDown);
            _stopButton.TouchDown += new EventHandler(_stopButton_TouchDown);

            _playButton.BackgroundColor = UIColor.White;
            _playButton.Enabled = true;

            _stopButton.BackgroundColor = UIColor.Blue;
            _stopButton.Enabled = false;

            _sinButton.TouchDown += new EventHandler(_sinButton_TouchDown);
            _squareButton.TouchDown += new EventHandler(_squareButton_TouchDown);
            _impulseButton.TouchDown += new EventHandler(_impulseButton_TouchDown);

            _periodLabel.EditingDidEnd += new EventHandler(_periodLabel_EditingDidEnd);
            _periodLabel.ShouldReturn = (tf) =>
            {
                tf.ResignFirstResponder();
                return true;
            };
            _periodLabel.Text = _player.Period.ToString();

            _sinButton.BackgroundColor = UIColor.Blue;
            _squareButton.BackgroundColor = UIColor.White;
            _impulseButton.BackgroundColor = UIColor.White;
        }

        void _periodLabel_EditingDidEnd(object sender, EventArgs e)
        {
            _player.Period = Int32.Parse(_periodLabel.Text);
        }

        void _squareButton_TouchDown(object sender, EventArgs e)
        {
            _sinButton.BackgroundColor = UIColor.White;
            _squareButton.BackgroundColor = UIColor.Blue;
            _impulseButton.BackgroundColor = UIColor.White;

            _player.WaveFormType = WaveFormType.Square;
        }


        void _sinButton_TouchDown(object sender, EventArgs e)
        {
            _sinButton.BackgroundColor = UIColor.Blue;
            _squareButton.BackgroundColor = UIColor.White; 
            _impulseButton.BackgroundColor = UIColor.White;            

            _player.WaveFormType = WaveFormType.Sin;
        }

        void _impulseButton_TouchDown(object sender, EventArgs e)
        {
            _sinButton.BackgroundColor = UIColor.White;
            _squareButton.BackgroundColor = UIColor.White;
            _impulseButton.BackgroundColor = UIColor.Blue; 

            _player.WaveFormType = WaveFormType.Impulse;
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
            _playButton.BackgroundColor = UIColor.White;
            _stopButton.Enabled = false;
            _stopButton.BackgroundColor = UIColor.Blue;
        }

        void _playButton_TouchDown(object sender, EventArgs e)
        {
            _player.Play();

            _playButton.Enabled = false;
            _playButton.BackgroundColor = UIColor.Blue;
            _stopButton.Enabled = true;
            _stopButton.BackgroundColor = UIColor.White;
        }
        #endregion
    }
}

