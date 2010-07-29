
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Monotouch_AudioUnit_PlayingSinWaveform
{
	public partial class AudioUnitWithACViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public AudioUnitWithACViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public AudioUnitWithACViewController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public AudioUnitWithACViewController () : base("AudioUnitWithACViewController", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		
		#endregion
	}
}

