//
// AudioUnit.cs: AudioUnit wrapper class
//
// Author:
//   AKIHIRO Uehara (u-akihiro@reinforce-lab.com)
//
// Copyright 2010 Reinforce Lab.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MonoTouch.AudioToolbox
{
    public class AudioUnit : IDisposable
    {
        #region Variables
        readonly GCHandle _handle;
        readonly IntPtr _audioUnit;
        bool _isPlaying;
        #endregion

        #region Properties
        public event EventHandler<AudioUnitEventArgs> RenderCallback;
        //public event EventHandler<AudioUnitEventArgs> InputCallback;
        public bool IsPlaying { get { return _isPlaying; } }
        #endregion

        #region Constructor
        public AudioUnit(IntPtr handler)
        {
            _audioUnit = handler;
            _isPlaying = false;

            _handle = GCHandle.Alloc(this);
            var callbackStruct = new AURenderCallbackStrct();
            callbackStruct.inputProc = renderCallback; // setting callback function            
            callbackStruct.inputProcRefCon = GCHandle.ToIntPtr(_handle); // a pointer that passed to the renderCallback (IntPtr inRefCon) 
            int err = AudioUnitSetProperty(_audioUnit,
                AudioUnitPropertyIDType.kAudioUnitProperty_SetRenderCallback,
                AudioUnitScopeType.kAudioUnitScope_Input,
                0, // 0 == speaker                
                callbackStruct,
                (uint)Marshal.SizeOf(callbackStruct));
            if (err != 0)
                throw new ArgumentException(String.Format("Error code: {0}", err));           
        }
        #endregion

        #region Private methods
        // callback funtion should be static method and be attatched a MonoPInvokeCallback attribute.        
        [MonoTouch.MonoPInvokeCallback(typeof(AURenderCallback))]
        static int renderCallback(IntPtr inRefCon,
            ref AudioUnitRenderActionFlags _ioActionFlags,
            ref AudioTimeStamp _inTimeStamp,
            uint _inBusNumber,
            uint _inNumberFrames,
            AudioBufferList _ioData)
        {
            // getting audiounit instance
            var handler = GCHandle.FromIntPtr(inRefCon);
            var inst = (AudioUnit)handler.Target;
            
            // evoke event handler with an argument
            if (inst.RenderCallback != null) 
            {
                var args = new AudioUnitEventArgs(
                    _ioActionFlags,
                    _inTimeStamp,
                    _inBusNumber,
                    _inNumberFrames,
                    _ioData);
                inst.RenderCallback(inst, args);
            }

            return 0; // noerror
        }
        #endregion

        #region Setter/Getter
        public void SetAudioFormat(MonoTouch.AudioToolbox.AudioStreamBasicDescription audioFormat, AudioUnitScopeType scope, uint audioUnitElement)
        {
            int err = AudioUnitSetProperty(_audioUnit,
                AudioUnitPropertyIDType.kAudioUnitProperty_StreamFormat,
                scope,
                audioUnitElement, 
                ref audioFormat,
                (uint)Marshal.SizeOf(audioFormat));
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:{0}", err));
        }
        public MonoTouch.AudioToolbox.AudioStreamBasicDescription GetAudioFormat(AudioUnitScopeType scope, uint audioUnitElement)
        {
            MonoTouch.AudioToolbox.AudioStreamBasicDescription audioFormat = new AudioStreamBasicDescription();
            uint size = (uint)Marshal.SizeOf(audioFormat);
            
            int err = AudioUnitGetProperty(_audioUnit,
                AudioUnitPropertyIDType.kAudioUnitProperty_StreamFormat,
                scope,
                audioUnitElement,
                ref audioFormat,
                ref size);
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:{0}", err));

            return audioFormat;
        }
        
        public void SetEnableIO(bool enableIO, AudioUnitScopeType scope, uint audioUnitElement)
        {                                   
            uint flag = (uint)(enableIO ? 1 : 0);
            int err = AudioUnitSetProperty(_audioUnit,
                AudioUnitPropertyIDType.kAudioOutputUnitProperty_EnableIO,
                scope,
                audioUnitElement,
                ref flag,
                (uint)Marshal.SizeOf(typeof(uint)));
            if (err != 0)
                throw new InvalidOperationException(String.Format("Error code:{0}", err));
        }
        #endregion

        #region Public methods
        public static AudioUnit CreateInstance(AudioComponent cmp)
        {
            var ptr = new IntPtr();
            int err = AudioComponentInstanceNew(cmp.Handler, ref ptr);
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:", err));

            if (ptr != IntPtr.Zero)
            {
                return new AudioUnit(ptr);
            }
            else
            {
                return null;
            }
        }
        public void Initialize()
        {
            AudioUnitInitialize(_audioUnit);
        }
        public void Start()
        {
            if (! _isPlaying) {
                AudioOutputUnitStart(_audioUnit);
                _isPlaying = true;
            }
        }
        public void Stop()
        {
            if (_isPlaying)
            {
                AudioOutputUnitStop(_audioUnit);
                _isPlaying = false;
            }
        }
        public void Render(AudioUnitRenderActionFlags flags, AudioTimeStamp timeStamp, UInt32 outputBusnumber, UInt32 numberFrames, AudioBufferList data)
        {
            int err = AudioUnitRender(_audioUnit,
                ref flags,
                ref timeStamp,
                outputBusnumber,
                numberFrames,
                data);
            if (err != 0)
                throw new InvalidOperationException(String.Format("Error code:{0}", err));
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            Stop();
            AudioUnitUnInitialize(_audioUnit);
            _handle.Free();            
        }
        #endregion

        #region Inteop
        /// <summary>
        /// AudioUnit call back method declaration
        /// </summary>
        public delegate int AURenderCallback(IntPtr inRefCon,
           ref AudioUnitRenderActionFlags ioActionFlags,
           ref AudioTimeStamp inTimeStamp,
           uint inBusNumber,
           uint inNumberFrames,
           AudioBufferList ioData);

        [StructLayout(LayoutKind.Sequential)]
        public class AURenderCallbackStrct
        {
            public AURenderCallback inputProc;
            public IntPtr inputProcRefCon;

            public AURenderCallbackStrct() { }
        }    

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioComponentInstanceNew")]
        static extern int AudioComponentInstanceNew(IntPtr inComponent, ref IntPtr inDesc);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitInitialize")]
        static extern int AudioUnitInitialize(IntPtr inUnit);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitUnInitialize")]
        static extern int AudioUnitUnInitialize(IntPtr inUnit);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioOutputUnitStart")]
        static extern int AudioOutputUnitStart(IntPtr ci);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioOutputUnitStop")]
        static extern int AudioOutputUnitStop(IntPtr ci);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitRender")]
        static extern int AudioUnitRender(IntPtr inUnit,
            ref AudioUnitRenderActionFlags ioActionFlags,
            ref AudioTimeStamp inTimeStamp,
            UInt32 inOutputBusNumber,
            UInt32 inNumberFrames,
            AudioBufferList ioData
            );

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitSetProperty")]
        static extern int AudioUnitSetProperty(IntPtr inUnit,
            [MarshalAs(UnmanagedType.U4)] AudioUnitPropertyIDType inID,
            [MarshalAs(UnmanagedType.U4)] AudioUnitScopeType inScope,
            [MarshalAs(UnmanagedType.U4)] uint inElement,
            AURenderCallbackStrct inData,
            uint inDataSize
            );

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitSetProperty")]
        static extern int AudioUnitSetProperty(IntPtr inUnit,
            [MarshalAs(UnmanagedType.U4)] AudioUnitPropertyIDType inID,
            [MarshalAs(UnmanagedType.U4)] AudioUnitScopeType inScope,
            [MarshalAs(UnmanagedType.U4)] uint inElement,
            ref MonoTouch.AudioToolbox.AudioStreamBasicDescription inData,
            uint inDataSize
            );
        
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitSetProperty")]
        static extern int AudioUnitSetProperty(IntPtr inUnit,
            [MarshalAs(UnmanagedType.U4)] AudioUnitPropertyIDType inID,
            [MarshalAs(UnmanagedType.U4)] AudioUnitScopeType inScope,
            [MarshalAs(UnmanagedType.U4)] uint inElement,
            ref uint flag,
            uint inDataSize
            );
        
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitGetProperty")]
        static extern int AudioUnitGetProperty(IntPtr inUnit,
            [MarshalAs(UnmanagedType.U4)] AudioUnitPropertyIDType inID,
            [MarshalAs(UnmanagedType.U4)] AudioUnitScopeType inScope,
            [MarshalAs(UnmanagedType.U4)] uint inElement,
            ref MonoTouch.AudioToolbox.AudioStreamBasicDescription outData,
            ref uint ioDataSize
            );

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioUnitGetProperty")]
        static extern int AudioUnitGetProperty(IntPtr inUnit,
            [MarshalAs(UnmanagedType.U4)] AudioUnitPropertyIDType inID,
            [MarshalAs(UnmanagedType.U4)] AudioUnitScopeType inScope,
            [MarshalAs(UnmanagedType.U4)] uint inElement,
            ref uint flag,
            ref uint ioDataSize
            );

        public enum AudioUnitPropertyIDType
        {
            // range (0 -> 999)
            kAudioUnitProperty_ClassInfo = 0,
            kAudioUnitProperty_MakeConnection = 1,
            kAudioUnitProperty_SampleRate = 2,
            kAudioUnitProperty_ParameterList = 3,
            kAudioUnitProperty_ParameterInfo = 4,
            kAudioUnitProperty_StreamFormat = 8,
            kAudioUnitProperty_ElementCount = 11,
            kAudioUnitProperty_Latency = 12,
            kAudioUnitProperty_SupportedNumChannels = 13,
            kAudioUnitProperty_MaximumFramesPerSlice = 14,
            kAudioUnitProperty_AudioChannelLayout = 19,
            kAudioUnitProperty_TailTime = 20,
            kAudioUnitProperty_BypassEffect = 21,
            kAudioUnitProperty_LastRenderError = 22,
            kAudioUnitProperty_SetRenderCallback = 23,
            kAudioUnitProperty_FactoryPresets = 24,
            kAudioUnitProperty_RenderQuality = 26,
            kAudioUnitProperty_InPlaceProcessing = 29,
            kAudioUnitProperty_ElementName = 30,
            kAudioUnitProperty_SupportedChannelLayoutTags = 32,
            kAudioUnitProperty_PresentPreset = 36,
            kAudioUnitProperty_ShouldAllocateBuffer = 51,

            //Output property
	        kAudioOutputUnitProperty_CurrentDevice			= 2000,
	        kAudioOutputUnitProperty_ChannelMap				= 2002, // this will also work with AUConverter
	        kAudioOutputUnitProperty_EnableIO				= 2003,
	        kAudioOutputUnitProperty_StartTime				= 2004,
	        kAudioOutputUnitProperty_SetInputCallback		= 2005,
	        kAudioOutputUnitProperty_HasIO					= 2006,
	        kAudioOutputUnitProperty_StartTimestampsAtZero  = 2007	// this will also work with AUConverter
        };
        
        public enum AudioUnitScopeType
        {
            kAudioUnitScope_Global = 0,
            kAudioUnitScope_Input = 1,
            kAudioUnitScope_Output = 2
        }

        [Flags]
        public enum AudioUnitRenderActionFlags
        {
            kAudioUnitRenderAction_PreRender = (1 << 2),
            kAudioUnitRenderAction_PostRender = (1 << 3),
            kAudioUnitRenderAction_OutputIsSilence = (1 << 4),
            kAudioOfflineUnitRenderAction_Preflight = (1 << 5),
            kAudioOfflineUnitRenderAction_Render = (1 << 6),
            kAudioOfflineUnitRenderAction_Complete = (1 << 7),
            kAudioUnitRenderAction_PostRenderError = (1 << 8)
        };
        #endregion    
    }
}
