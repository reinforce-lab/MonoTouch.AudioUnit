//
// AudioBuffer.cs: AudioBuffer wrapper class
//
// Author:
//   AKIHIRO Uehara (u-akihiro@reinforce-lab.com)
//
// Copyright 2010 Reinforce Lab.
//

using System;
using System.Runtime.InteropServices;

namespace MonoTouch.AudioToolbox
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AudioBuffer
    {
        public uint mNumberChannels;
        public uint mDataByteSize;
        public IntPtr mData;
    }
}
