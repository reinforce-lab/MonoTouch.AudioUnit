using System;
using System.Runtime.InteropServices;

namespace MonoTouch.AudioUnitWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    public class AudioBufferList
    {
        public uint mNumberBuffers;                        
        
        // mBuffers array size is variable. But here we uses fixed size of 2, because iPhone phone terminal two (L/R) channels.        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public AudioBuffer[] mBuffers;
        
        public AudioBufferList() { }
    }
}
