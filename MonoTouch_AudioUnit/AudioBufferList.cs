//
// AudioBufferList.cs: AudioBufferList wrapper class
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
    public class AudioBufferList : IDisposable
    {
        #region Variables
        public uint mNumberBuffers;                        
        
        // mBuffers array size is variable. But here we uses fixed size of 2, because iPhone phone terminal two (L/R) channels.        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public AudioBuffer[] mBuffers;

        bool _allocated;
        #endregion

        #region Constructor
        public AudioBufferList() 
        {
            _allocated = false;
        }
        public AudioBufferList(uint numBuffers, uint bufferSize)
        {
            _allocated = false;

            mNumberBuffers = numBuffers;
            mBuffers = new AudioBuffer[mNumberBuffers];
            for (int i = 0; i < mNumberBuffers; i++)
            {
                mBuffers[i].mNumberChannels = 1;
                mBuffers[i].mDataByteSize = bufferSize;
                mBuffers[i].mData = Marshal.AllocHGlobal((int)bufferSize);
            }
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            if (_allocated)
            {
                foreach (var mbuf in mBuffers)
                    Marshal.FreeHGlobal(mbuf.mData);
                _allocated = false;
            }
        }
        #endregion
    }
}
