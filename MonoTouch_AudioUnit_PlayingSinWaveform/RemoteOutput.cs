using System;
using System.Runtime.InteropServices;

using MonoTouch.AudioToolbox;

namespace Monotouch_AudioUnit_PlayingSinWaveform
{
    class RemoteOutput :IDisposable
    {        
        #region Variables
        const int kAudioUnitSampleFractionBits = 24;
        readonly int _sampleRate;
        const int _frequency = 15000;
        const bool _isSquareWave = false;

        AudioComponent _component;
        AudioUnit _audioUnit;
        double _phase;        
        #endregion

        #region Constructor
        public RemoteOutput()
        {            
            _sampleRate = 44100;            

            prepareAudioUnit();
        }
        #endregion

        #region Private methods
        void simulator_callback(object sender, AudioUnitEventArgs args)
        {
            // Generating sin waveform
            double dphai = _frequency * 2.0 * Math.PI / _sampleRate;

            // Getting a pointer to a buffer to be filled
            IntPtr outL = args.Data.mBuffers[0].mData;
            IntPtr outR = args.Data.mBuffers[1].mData;

            // filling sin waveform.
            // AudioUnitSampleType is different between a simulator (float32) and a real device (int32).
            unsafe
            {
                var outLPtr = (float*)outL.ToPointer();
                var outRPtr = (float*)outR.ToPointer();
                for (int i = 0; i < args.NumberFrames; i++)
                {
                    float sample = (float)Math.Sin(_phase) / 2048;
                    if (_isSquareWave)
                    {
                        if (sample > 0)
                            sample = 1.0f / 2048.0f;
                        else
                            sample = -1.0f / 2048.0f;
                    }
                    *outLPtr++ = sample;
                    *outRPtr++ = sample;
                    _phase += dphai;
                }
            }
            _phase %= 2 * Math.PI;
        }
        // AudioUnit callback function uses this method to use instance variables. 
        // In the static callback method is not convienient because instance variables can not used.
        void device_callback(object sender, AudioUnitEventArgs args)
        {
            // Generating sin waveform
            double dphai = _frequency * 2.0 * Math.PI / _sampleRate;

            // Getting a pointer to a buffer to be filled
            IntPtr outL = args.Data.mBuffers[0].mData;
            IntPtr outR = args.Data.mBuffers[1].mData;

            // filling sin waveform.
            // AudioUnitSampleType is different between a simulator (float32) and a real device (int32).
            unsafe
            {
                var outLPtr = (int*)outL.ToPointer();
                var outRPtr = (int*)outR.ToPointer();
                for (int i = 0; i < args.NumberFrames; i++)
                {
                    int sample = (int)(Math.Sin(_phase) * int.MaxValue / 128); // signal waveform format is fixed-point (8.24)
                    if (_isSquareWave)
                    {
                        if (sample > 0)
                            sample = int.MaxValue / 128;
                        else
                            sample = -1 * int.MaxValue / 128;
                    }
                    *outLPtr++ = sample;
                    *outRPtr++ = sample;
                    _phase += dphai;
                }
            }
            _phase %= 2 * Math.PI;
        }
        void prepareAudioUnit()
        {
            // Creating AudioComponentDescription instance of RemoteIO Audio Unit
            AudioComponentDescription cd = new AudioComponentDescription()
            {
                componentType    = AudioComponentDescription.AudioComponentType.kAudioUnitType_Output,
                componentSubType = AudioComponentDescription.AudioComponentSubType.kAudioUnitSubType_RemoteIO,
                componentManufacturer = AudioComponentDescription.AudioComponentManufacturerType.kAudioUnitManufacturer_Apple,
                componentFlags = 0,
                componentFlagsMask = 0
            };
            
            // Getting AudioComponent from the description
            _component = AudioComponent.FindComponent(cd);
           
            // Getting Audiounit
            _audioUnit = AudioUnit.CreateInstance(_component);

            // setting AudioStreamBasicDescription
            int AudioUnitSampleTypeSize;
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
            {
                AudioUnitSampleTypeSize = sizeof(float);
            }
            else
            {
                AudioUnitSampleTypeSize = sizeof(int);
            }
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
            _audioUnit.SetAudioFormat(audioFormat, AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input, 0);            

            // setting callback
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(simulator_callback);
            else
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(device_callback);
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
