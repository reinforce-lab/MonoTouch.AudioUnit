using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MonoTouch.AudioToolbox;

namespace MonoTouch.AudioUnitWrapper
{
    public class _AudioConverter : IDisposable
    {
        #region Variables
        readonly GCHandle _handle;
        IntPtr _audioConverter;
        #endregion

        #region Properties
        public event EventHandler<_AudioConverterEventArgs> EncoderCallback;
        public Byte[] DecompressionMagicCookie
        {
            set
            { 
                byte[] data = value;
                if (null != data)
                {
                    AudioConverterSetProperty(_audioConverter,
                        AudioConverterPropertyIDType.kAudioConverterDecompressionMagicCookie,
                        (uint)data.Length,
                        data);
                }
            }
        }
        #endregion

        #region Constructor
        private _AudioConverter()
        {
            _handle = GCHandle.Alloc(this);
            _audioConverter = new IntPtr();
        }
        #endregion

        #region Private methods
        [MonoTouch.MonoPInvokeCallback(typeof(AudioConverterComplexInputDataProc))]
        static int complexInputDataProc(
            IntPtr inAudioConverrter,
            ref uint ioNumberDataPackets,
            AudioBufferList ioData,
            ref AudioStreamPacketDescription[] outDataPacketDescription, //AudioStreamPacketDescription**
            IntPtr inUserData
            )
        {
            // getting audiounit instance
            var handler = GCHandle.FromIntPtr(inUserData);
            var inst = (_AudioConverter)handler.Target;

            // evoke event handler with an argument
            if (inst.EncoderCallback != null)
            {
                var args = new _AudioConverterEventArgs(
                    ioNumberDataPackets,
                    ioData,
                    outDataPacketDescription);
                inst.EncoderCallback(inst, args);
            }

            return 0; // noerror
        }
        #endregion

        #region Public methods
        public static _AudioConverter CreateInstance(AudioStreamBasicDescription srcFormat, AudioStreamBasicDescription destFormat)            
        {
            _AudioConverter inst = new _AudioConverter();
            int err_code;
            unsafe{
                IntPtr ptr = inst._audioConverter;
                IntPtr pptr =(IntPtr)(&ptr);
                err_code = AudioConverterNew(ref srcFormat, ref destFormat, pptr);
            }
            if (err_code != 0)
            {
                throw new ArgumentException(String.Format("Error code:{0}", err_code));
            }
            return inst;
        }
        public void FillBuffer(AudioBufferList data, uint numberFrames, AudioStreamPacketDescription[] packetDescs)
        {
            uint numPackets = numberFrames;
            int err =AudioConverterFillComplexBuffer(                
                _audioConverter,
                complexInputDataProc,
                GCHandle.ToIntPtr(_handle),
                ref numPackets,
                data,
                packetDescs);
            if(err != 0 || numPackets == 0) {
                throw new InvalidOperationException(String.Format("Error code:{0}", err));
            }            
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            _handle.Free();            
        }
        #endregion

        #region Interop
        delegate int AudioConverterComplexInputDataProc(
            IntPtr inAudioConverrter,
            ref uint ioNumberDataPackets,
            AudioBufferList ioData,
            ref AudioStreamPacketDescription[] outDataPacketDescription, 
            IntPtr inUserData
            );

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioConverterNew")]
        static extern int AudioConverterFillComplexBuffer(
            IntPtr 		inAudioConverter,
            AudioConverterComplexInputDataProc	inInputDataProc,
            IntPtr inInputDataProcUserData,
            ref uint ioOutputDataPacketSize,
            AudioBufferList outOutputData,
            AudioStreamPacketDescription[] outPacketDescription);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioConverterNew")]
        static extern int AudioConverterNew(
            ref MonoTouch.AudioToolbox.AudioStreamBasicDescription inSourceFormat,
            ref MonoTouch.AudioToolbox.AudioStreamBasicDescription inDestinationFormat,
            IntPtr outAudioConverter);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioConverterSetProperty")]
        static extern int AudioConverterSetProperty(IntPtr inAudioConverter,
            [MarshalAs(UnmanagedType.U4)] AudioConverterPropertyIDType inID,
            uint inDataSize,
            IntPtr inPrppertyData            
            );
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioConverterSetProperty")]
        static extern int AudioConverterSetProperty(IntPtr inAudioConverter,
            [MarshalAs(UnmanagedType.U4)] AudioConverterPropertyIDType inID,
            uint inDataSize,
            byte[] inPrppertyData            
            );

        enum AudioConverterPropertyIDType // typedef UInt32 AudioConverterPropertyID
        {
	        //kAudioConverterPropertyMinimumInputBufferSize		= 'mibs',
	        //kAudioConverterPropertyMinimumOutputBufferSize		= 'mobs',
	        //kAudioConverterPropertyMaximumInputBufferSize		= 'xibs',
	        //kAudioConverterPropertyMaximumInputPacketSize		= 'xips',
	        //kAudioConverterPropertyMaximumOutputPacketSize		= 'xops',
	        //kAudioConverterPropertyCalculateInputBufferSize		= 'cibs',
	        //kAudioConverterPropertyCalculateOutputBufferSize	= 'cobs',
	        //kAudioConverterPropertyInputCodecParameters			= 'icdp',
	        //kAudioConverterPropertyOutputCodecParameters		= 'ocdp',
	        //kAudioConverterSampleRateConverterAlgorithm			= 'srci',
	        //kAudioConverterSampleRateConverterComplexity		= 'srca',
	        //kAudioConverterSampleRateConverterQuality			= 'srcq',
	        //kAudioConverterSampleRateConverterInitialPhase		= 'srcp',
	        //kAudioConverterCodecQuality							= 'cdqu',
	        //kAudioConverterPrimeMethod							= 'prmm',
	        //kAudioConverterPrimeInfo							= 'prim',
	        //kAudioConverterChannelMap							= 'chmp',
	        kAudioConverterDecompressionMagicCookie				=  0x646d6763, //'dmgc',
	        //kAudioConverterCompressionMagicCookie				= 'cmgc',
	        //kAudioConverterEncodeBitRate						= 'brat',
	        //kAudioConverterEncodeAdjustableSampleRate			= 'ajsr',
	        //kAudioConverterInputChannelLayout					= 'icl ',
	        //kAudioConverterOutputChannelLayout					= 'ocl ',
	        //kAudioConverterApplicableEncodeBitRates				= 'aebr',
	        //kAudioConverterAvailableEncodeBitRates				= 'vebr',
	        //kAudioConverterApplicableEncodeSampleRates			= 'aesr',
	        //kAudioConverterAvailableEncodeSampleRates			= 'vesr',
	        //kAudioConverterAvailableEncodeChannelLayoutTags		= 'aecl',
	        //kAudioConverterCurrentOutputStreamDescription		= 'acod',
	        //kAudioConverterCurrentInputStreamDescription		= 'acid',
	        //kAudioConverterPropertySettings						= 'acps',
	        //kAudioConverterPropertyBitDepthHint					= 'acbd',
	        //kAudioConverterPropertyFormatList					= 'flst'
        };    
        #endregion


    }
}
