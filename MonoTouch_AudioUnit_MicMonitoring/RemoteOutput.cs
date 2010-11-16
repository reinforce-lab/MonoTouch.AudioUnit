using System;
using System.Runtime.InteropServices;

using MonoTouch.AudioToolbox;

namespace Monotouch_AudioUnit_MicMonitoring
{
    class RemoteOutput : IDisposable
    {
        #region Variables
        const int _sampleRate = 44100;
        const float _lpf_constant = (1 / (float)(4 * 1024));

        const int kAudioUnitSampleFractionBits = 24;

        AudioComponent _component;
        AudioUnit _audioUnit;

        float _sigLevel, _dcLevel;
        #endregion

        #region Properties
        public float SignalLevel
        {
            get
            {
                lock (this)
                {
                    return _sigLevel;
                }
            }
        }
        #endregion

        #region Constructor
        public RemoteOutput()
        {            
            _sigLevel = 0;
            _dcLevel  = 0;

            prepareAudioUnit();
        }
        #endregion

        #region Private methods
        void _callback(object sender, AudioUnitEventArgs args)
        {
            // getting microphone
            _audioUnit.Render(args.ActionFlags,
                args.TimeStamp,
                1, // Remote input
                args.NumberFrames,
                args.Data);

            // Getting a pointer to a buffer to be filled
            IntPtr outL = args.Data.mBuffers[0].mData;
            IntPtr outR = args.Data.mBuffers[1].mData;

            // level monitor            
            float diff;
            float sig_level = _sigLevel;
            unsafe
            {
                var outLPtr = (Int32*)outL.ToPointer();
                var outRPtr = (Int32*)outR.ToPointer();
                for (int i = 0; i < args.NumberFrames; i++)
                {
                    float val = *outLPtr;
                    
                    _dcLevel += (val - _dcLevel) * _lpf_constant;
                    diff = Math.Abs(val - _dcLevel);
                    sig_level += (diff - sig_level) * _lpf_constant;
                    
                    outLPtr++;
                }
            }
            //System.Diagnostics.Debug.WriteLine(String.Format("AD{0}: DC:{1}", sig_level, _dcLevel));
            lock (this)
            {
                _sigLevel = sig_level;
            }
        }
        void prepareAudioUnit()
        {
            // AudioSession
            AudioSession.Initialize();
            AudioSession.SetActive(true);
            AudioSession.Category = AudioSessionCategory.PlayAndRecord;
            AudioSession.PreferredHardwareIOBufferDuration = 0.01f;            

            // Creating AudioComponentDescription instance of RemoteIO Audio Unit
            var cd = new AudioComponentDescription()
            {
                componentType = AudioComponentDescription.AudioComponentType.kAudioUnitType_Output,
                componentSubType = AudioComponentDescription.AudioComponentSubType.kAudioUnitSubType_RemoteIO,
                componentManufacturer = AudioComponentDescription.AudioComponentManufacturerType.kAudioUnitManufacturer_Apple,
                componentFlags = 0,
                componentFlagsMask = 0
            };

            // Getting AudioComponent from the description
            _component = AudioComponent.FindComponent(cd);

            // Getting Audiounit
            _audioUnit = AudioUnit.CreateInstance(_component);

            // turning on microphone
            _audioUnit.SetEnableIO(true,
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input,
                1 // Remote Input
                );

            // setting AudioStreamBasicDescription
            int AudioUnitSampleTypeSize = (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR) ? sizeof(float) : sizeof(uint);
            AudioStreamBasicDescription audioFormat = new AudioStreamBasicDescription()
            {
                SampleRate = _sampleRate,
                Format = AudioFormatType.LinearPCM,
                //kAudioFormatFlagsAudioUnitCanonical = kAudioFormatFlagIsSignedInteger | kAudioFormatFlagsNativeEndian | kAudioFormatFlagIsPacked | kAudioFormatFlagIsNonInterleaved | (kAudioUnitSampleFractionBits << kLinearPCMFormatFlagsSampleFractionShift),
                FormatFlags = (AudioFormatFlags)((int)AudioFormatFlags.IsSignedInteger | (int)AudioFormatFlags.IsPacked | (int)AudioFormatFlags.IsNonInterleaved | (int)(kAudioUnitSampleFractionBits << (int)AudioFormatFlags.LinearPCMSampleFractionShift)),
                ChannelsPerFrame = 2,
                BytesPerPacket = AudioUnitSampleTypeSize,
                BytesPerFrame = AudioUnitSampleTypeSize,
                FramesPerPacket = 1,
                BitsPerChannel = 8 * AudioUnitSampleTypeSize,
                Reserved = 0
            };
            _audioUnit.SetAudioFormat(audioFormat, 
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input, 
                0 // Remote output
                );
            _audioUnit.SetAudioFormat(audioFormat, 
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Output, 
                1 // Remote input
                );

            // setting callback
            /*
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(simulator_callback);
            else
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(device_callback);
            */
            _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(_callback);
            // initialize
            _audioUnit.Initialize();
        }
        #endregion

        #region Public methods
        public void Play()
        {
            _audioUnit.Start();
        }
        public void Stop()
        {
            _audioUnit.Stop();
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            _audioUnit.Dispose();
            _component.Dispose();
        }
        #endregion
    }
}
