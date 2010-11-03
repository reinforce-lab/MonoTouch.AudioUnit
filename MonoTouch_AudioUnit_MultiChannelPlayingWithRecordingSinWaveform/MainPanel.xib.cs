
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ToneGen
{
	public partial class MainPanel : UIViewController
	{
        AudioQueueSynth _synth;

		#region Constructors

		// The IntPtr and initWithCoder constructors are required for controllers that need 
		// to be able to be created from a xib rather than from managed code

		public MainPanel (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public MainPanel (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public MainPanel () : base("MainPanel", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}		
		#endregion

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _playButton.TouchUpInside += new EventHandler(_playButton_TouchUpInside);
            _stopButton.TouchUpInside += new EventHandler(_stopButton_TouchUpInside);
        }

        void _stopButton_TouchUpInside(object sender, EventArgs e)
        {
            if (null == _synth)
                return;

            _messageTextField.Text = string.Empty;
            try
            {
                _synth.stop(true);
                _synth = null;
            }
            catch (Exception ex)
            {
                _messageTextField.Text = ex.Message + "\n" + ex.StackTrace;
            }            
        }

        void _playButton_TouchUpInside(object sender, EventArgs e)
        {
            if (null != _synth)
                return;

            _messageTextField.Text = String.Empty;
            try
            {
                _synth = new AudioQueueSynth(int.Parse(_baseFreqTextField.Text), int.Parse(_bufferLengthTextField.Text));
                _synth.play();
            }
            catch (Exception ex)
            {
                _messageTextField.Text = ex.Message + "\n" + ex.StackTrace;
            }
        }
		
	}
}

