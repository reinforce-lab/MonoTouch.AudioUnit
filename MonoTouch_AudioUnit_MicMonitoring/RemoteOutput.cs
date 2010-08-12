using System;
using System.Runtime.InteropServices;

using MonoTouch.AudioToolbox;

namespace Monotouch_AudioUnit_MicMonitoring
{
    class RemoteOutput : IDisposable
    {
        #region Variables
        const int kAudioUnitSampleFractionBits = 24;
        readonly int _sampleRate;

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
            // getting microphone
            _audioUnit.Render(args.ActionFlags, 
                args.TimeStamp,
                1, // Remote input
                args.NumberFrames,
                args.Data);
        }
        // AudioUnit callback function uses this method to use instance variables. 
        // In the static callback method is not convienient because instance variables can not used.
        void device_callback(object sender, AudioUnitEventArgs args)
        {
            // getting microphone
            _audioUnit.Render(args.ActionFlags,
                args.TimeStamp,
                1, // Remote input
                args.NumberFrames,
                args.Data);
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
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(simulator_callback);
            else
                _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(device_callback);

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
