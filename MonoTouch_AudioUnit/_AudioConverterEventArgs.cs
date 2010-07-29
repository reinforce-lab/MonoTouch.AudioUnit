using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTouch.AudioUnitWrapper
{
    public class _AudioConverterEventArgs : EventArgs
    {
        #region Variables
        public uint NumberDataPackets;
        public readonly AudioBufferList Data;            
        public readonly MonoTouch.AudioToolbox.AudioStreamPacketDescription[] DataPacketDescription;
        #endregion

        #region Constructor
        public _AudioConverterEventArgs(
            uint _NumberDataPackets,
            AudioBufferList _Data,
            MonoTouch.AudioToolbox.AudioStreamPacketDescription[] _DataPacketDescription)
        {
            NumberDataPackets = _NumberDataPackets;
            Data = _Data;
            DataPacketDescription = _DataPacketDescription;
        }
        #endregion
    }
}
