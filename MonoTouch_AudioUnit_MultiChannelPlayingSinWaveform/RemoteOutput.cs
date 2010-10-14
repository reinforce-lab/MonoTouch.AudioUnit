using System;
using System.Runtime.InteropServices;

using MonoTouch.AudioToolbox;

namespace Monotouch_AudioUnit_MultiChannelPlayingSinWaveform
{
    class RemoteOutput :IDisposable
    {        
        #region Variables        
        readonly GCHandle _handle;
        SineWaveDef[] _waveDef;
        
        AUGraph _auGraph;        
        #endregion

        #region Constructor
        public RemoteOutput()
        {
            _handle = GCHandle.Alloc(this);
            
            _waveDef = new SineWaveDef[2];
            for (int i = 0; i < _waveDef.Length; i++)
            {
                _waveDef[i] = new SineWaveDef()
                {
                    sampleRate = 44100.0,
                    frequency = 440.0 * (i +1)
                };
            }
                
            prepareAUGraph();            
        }
        #endregion

        #region Private methods       
        // AudioUnit callback function uses this method to use instance variables. 
        // In the static callback method is not convienient because instance variables can not used.
        // callback funtion should be static method and be attatched a MonoPInvokeCallback attribute.        
        [MonoTouch.MonoPInvokeCallback(typeof(AudioUnit.AURenderCallback))]
        static int device_renderCallback(IntPtr inRefCon,
            ref AudioUnit.AudioUnitRenderActionFlags _ioActionFlags,
            ref AudioTimeStamp _inTimeStamp,
            uint _inBusNumber,
            uint _inNumberFrames,
            AudioBufferList _ioData)
        {
            var handler = GCHandle.FromIntPtr(inRefCon);
            var inst = (RemoteOutput)handler.Target;
            var waveDef = inst._waveDef[_inBusNumber];

            double dphai = 2 * Math.PI * waveDef.frequency / waveDef.sampleRate;
            double phase = waveDef.phase;

            // Getting a pointer to a buffer to be filled
            IntPtr outL = _ioData.mBuffers[0].mData;
            IntPtr outR = _ioData.mBuffers[1].mData;

            // filling sin waveform.
            // AudioUnitSampleType is different between a simulator (float32) and a real device (int32).
            unsafe
            {
                var outLPtr = (int*)outL.ToPointer();
                var outRPtr = (int*)outR.ToPointer();
                for (int i = 0; i < _inNumberFrames; i++)
                {
                    int sample = (int)(Math.Sin(phase) * int.MaxValue / 128); // signal waveform format is fixed-point (8.24)
                    *outLPtr++ = sample;
                    *outRPtr++ = sample;
                    phase += dphai;
                }
            }
            waveDef.phase = phase % (2 * Math.PI);
            return 0;
        }

        void prepareAUGraph()
        {
            // Creating audio graph instance
            _auGraph = AUGraph.CreateInstance();

            // Adding Remote IO node  to AUGraph
            AudioComponentDescription cd = new AudioComponentDescription()
            {
                componentType = AudioComponentDescription.AudioComponentType.kAudioUnitType_Output,
                componentSubType = AudioComponentDescription.AudioComponentSubType.kAudioUnitSubType_RemoteIO,
                componentManufacturer = AudioComponentDescription.AudioComponentManufacturerType.kAudioUnitManufacturer_Apple,
                componentFlags = 0,
                componentFlagsMask = 0
            };
            int remoteIONode = _auGraph.AddNode(cd);

            // Preparing AudioComponentDescrption of MultiChannelMixer
            cd.componentType = AudioComponentDescription.AudioComponentType.kAudioUnitType_Mixer;
            cd.componentSubType = AudioComponentDescription.AudioComponentSubType.kAudioUnitSubType_MultiChannelMixer;
            int multiChannelMixerNode = _auGraph.AddNode(cd);

            // Setting callback method as the case of Audio Unit            
            for (int i = 0; i < _waveDef.Length; i++)
            {
                var callbackStruct = new AudioUnit.AURenderCallbackStrct();
                callbackStruct.inputProc = device_renderCallback; // setting callback function
                callbackStruct.inputProcRefCon = GCHandle.ToIntPtr(_handle); // a pointer that passed to the renderCallback (IntPtr inRefCon) 
                _auGraph.AUGraphSetNodeInputCallback(
                    multiChannelMixerNode,
                    (uint)i, // bus number
                    callbackStruct);
            }            

            var remoteIOAudioUnit = _auGraph.GetNodeInfo(remoteIONode);
            var multiChannelMixerAudioUnit = _auGraph.GetNodeInfo(multiChannelMixerNode);

            // Getting an AudioUnit canonical description
            var audioFormat = AudioUnitUtils.AUCanonicalASBD(44100.0, 2);

            // applying the audio format to each audio units
            remoteIOAudioUnit.SetAudioFormat(audioFormat, AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input, 0);
            multiChannelMixerAudioUnit.SetAudioFormat(audioFormat, AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input, 0);
            
            // connecting multiChannelMixerNode(bus:0) to remoteIONode(bus:0)
            _auGraph.ConnnectNodeInput(
                multiChannelMixerNode, 
                0, // output bus                
                remoteIONode, 
                0  // input bus
                );
            
            // graph initialization
            _auGraph.Initialize();
        }
        #endregion

        #region Public methods
        public void Play()
        {
            _auGraph.Start();            
        }
        public void Stop()
        {
            _auGraph.Stop();            
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            _auGraph.Dispose();            
            _handle.Free();                
        }
        #endregion
    }
    internal class SineWaveDef
    {
        public double phase;
        public double sampleRate;
        public double frequency;
        //public double freqz;
    }
}
