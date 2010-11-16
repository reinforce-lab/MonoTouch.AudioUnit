using System;
using System.Runtime.InteropServices;

using MonoTouch.AudioToolbox;

namespace Monotouch_AudioUnit_PlayingSinWaveform
{
    enum WaveFormType { Sin, Square, Impulse };

    class RemoteOutput :IDisposable
    {        
        #region Variables
        const int kAudioUnitSampleFractionBits = 24;
        const int _sampleRate = 44100;
                
        const bool _isSquareWave = false;

        AudioComponent _component;
        AudioUnit _audioUnit;
        int _period;
        WaveFormType _waveFormType;
        int _phase;
        #endregion

        #region Properties
        public WaveFormType WaveFormType
        {
            get { return _waveFormType; }
            set { lock (this) { _waveFormType = value; } }
        }
        public int Period
        {
            get { return _period; }
            set { lock (this) { _period = value; } }
        }        
        #endregion

        #region Constructor
        public RemoteOutput()
        {
            prepareAudioUnit();
        }
        #endregion

        #region Private methods
        /*
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
        }*/
        // AudioUnit callback function uses this method to use instance variables. 
        // In the static callback method is not convienient because instance variables can not used.
        void device_callback(object sender, AudioUnitEventArgs args)
        {
            // Getting a pointer to a buffer to be filled
            IntPtr outL = args.Data.mBuffers[0].mData;
            IntPtr outR = args.Data.mBuffers[1].mData;

            var buf = new int[args.NumberFrames];
            // generating signal waveform 
            switch (_waveFormType)
            {
                case Monotouch_AudioUnit_PlayingSinWaveform.WaveFormType.Sin:                    
                    for (int i = 0; i < buf.Length; i++)
                    {
                        buf[i] = (int)(Math.Sin(2.0 * Math.PI * _phase / (double)_period) * int.MaxValue / 128.0);
                        _phase = (_phase + 1) % _period;
                    }
                    break;
                case Monotouch_AudioUnit_PlayingSinWaveform.WaveFormType.Square:
                    for (int i = 0; i < buf.Length; i++)
                    {
                        buf[i] = _phase < _period /2 ? -1 * int.MaxValue / 128 :  int.MaxValue / 128;
                        _phase = (_phase + 1) % _period;
                    }
                    break;
                case Monotouch_AudioUnit_PlayingSinWaveform.WaveFormType.Impulse: 
                    for (int i = 0; i < buf.Length; i++)
                    {
                        buf[i] = _phase == 0 ? int.MaxValue / 128 : 0;
                        _phase = (_phase + 1) % _period;
                    }
                    break;
            }
            
            // filling buffer
            unsafe
            {
                var outLPtr = (int*)outL.ToPointer();
                var outRPtr = (int*)outR.ToPointer();
                for(int i=0; i< buf.Length; i++)
                {
                    *outLPtr++ = buf[i];
                    *outRPtr++ = buf[i];
                }
            }
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
            /*
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(simulator_callback);
            else
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(device_callback);
             * */
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
