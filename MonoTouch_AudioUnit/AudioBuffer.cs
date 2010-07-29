using System;
using System.Runtime.InteropServices;

namespace MonoTouch.AudioUnitWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AudioBuffer
    {
        public uint mNumberChannels;
        public uint mDataByteSize;
        public IntPtr mData;
    }
}
