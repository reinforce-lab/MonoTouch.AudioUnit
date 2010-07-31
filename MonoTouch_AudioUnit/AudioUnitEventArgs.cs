using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTouch.AudioToolbox
{
    public class AudioUnitEventArgs : EventArgs
    {
        #region Variables
        public readonly AudioUnit.AudioUnitRenderActionFlags ActionFlags;
        public readonly MonoTouch.AudioToolbox.AudioTimeStamp TimeStamp;
        public readonly uint BusNumber;
        public readonly uint NumberFrames;
        public readonly AudioBufferList Data;
        #endregion

        #region Constructor
        public AudioUnitEventArgs(AudioUnit.AudioUnitRenderActionFlags _ioActionFlags,
            MonoTouch.AudioToolbox.AudioTimeStamp _inTimeStamp,
            uint _inBusNumber,
            uint _inNumberFrames,
            AudioBufferList _ioData)
        {
            ActionFlags = _ioActionFlags;
            this.TimeStamp = _inTimeStamp;
            BusNumber = _inBusNumber;
            NumberFrames = _inNumberFrames;
            Data = _ioData;
        }
        #endregion
    }
}
