using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTouch.AudioToolbox
{
    public static class AudioUnitUtils
    {
        public const int kAudioUnitSampleFractionBits = 24;

        public static AudioStreamBasicDescription AUCanonicalASBD(double sampleRate, int channel)
        {
            // setting AudioStreamBasicDescription
            int AudioUnitSampleTypeSize = (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR) ? sizeof(float) : sizeof(int);
            AudioStreamBasicDescription audioFormat = new AudioStreamBasicDescription()
            {
                SampleRate = sampleRate,
                Format = AudioFormatType.LinearPCM,
                //kAudioFormatFlagsAudioUnitCanonical = kAudioFormatFlagIsSignedInteger | kAudioFormatFlagsNativeEndian | kAudioFormatFlagIsPacked | kAudioFormatFlagIsNonInterleaved | (kAudioUnitSampleFractionBits << kLinearPCMFormatFlagsSampleFractionShift),
                FormatFlags      = (AudioFormatFlags)((int)AudioFormatFlags.IsSignedInteger | (int)AudioFormatFlags.IsPacked | (int)AudioFormatFlags.IsNonInterleaved | (int)(kAudioUnitSampleFractionBits << (int)AudioFormatFlags.LinearPCMSampleFractionShift)),
                ChannelsPerFrame = channel,
                BytesPerPacket   = AudioUnitSampleTypeSize,
                BytesPerFrame    = AudioUnitSampleTypeSize,
                FramesPerPacket  = 1,
                BitsPerChannel   = 8 * AudioUnitSampleTypeSize,
                Reserved = 0
            };
            return audioFormat;
        }
    }
}
