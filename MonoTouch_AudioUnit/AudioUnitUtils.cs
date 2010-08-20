using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

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

        public static void SetOverrideCategoryDefaultToSpeaker(bool isSpeaker)
        {
            int val = isSpeaker ? 1 : 0;
            int err = AudioSessionSetProperty(
                0x6373706b, //'cspk'
                (UInt32)sizeof(UInt32),
                ref val);
            if (err != 0)
                throw new ArgumentException();            
        }
             
        /*
        public static void SetOverrideAudioRoute(OverrideAudioRouteType route)
        {
            int err = AudioSessionSetProperty(
                AudioSessionProperty.OverrideAudioRoute,
                (UInt32)sizeof(UInt32),
                ref route);
            if (err != 0)
                throw new ArgumentException();
        }        
        */

        /*
Int32 doChangeDefaultRoute = 1;
         * AudioSessionSetProperty 
         * (kAudioSessionProperty_OverrideCategoryDefaultToSpeaker,
         * sizeof (doChangeDefaultRoute),
         * &doChangeDefaultRou         */
        #region Interop
        public enum OverrideAudioRouteType
        {
            None = 0,
            Speaker = 0x73706d72 // 'spkr'
        }
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioSessionSetProperty")]
        static extern int AudioSessionSetProperty(
            AudioSessionProperty inID,
            UInt32 inDataSize,
            ref OverrideAudioRouteType inData);
        
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioSessionSetProperty")]
        static extern int AudioSessionSetProperty(
            UInt32 inID,
            UInt32 inDataSize,
            ref int inData);
        #endregion
    }
}
