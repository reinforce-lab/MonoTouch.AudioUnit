using System;

using MonoTouch.CoreFoundation;
using MonoTouch.AudioToolbox;

namespace Monotouch_RemoteIO_PlayThroughWithMonitoring
{
    class RemoteIOPlayThrough
    {
        #region Variables
        AUGraph _auGraph;
        bool _isPlaying;
        bool _isRecording;
        AudioStreamBasicDescription _audioUnitOutputFormat;
        ExtAudioFile _extAudioFile;
        #endregion

        #region Constructor
        public RemoteIOPlayThrough()
        {
            _isRecording = false;
            _isPlaying = false;

            prepareAUGraph();
        }
        #endregion

        #region Private methods
        void _auGraph_RenderCallback(object sender, AudioGraphEventArgs e)
        {
            if (!_isRecording)
                return;

            // is Post Render ?
            if ((e.ActionFlags & AudioUnit.AudioUnitRenderActionFlags.kAudioUnitRenderAction_PostRender) != 0)
            {
                // reading buffer
                _extAudioFile.WriteAsync(e.NumberFrames, e.Data);
            }
            throw new NotImplementedException();
        }

        void prepareAUGraph()
        {
            // Creating audio graph instance
            _auGraph = AUGraph.CreateInstance();

            // getting audio node and audio unit
            AudioComponentDescription cd = new AudioComponentDescription()
            {
                componentType = AudioComponentDescription.AudioComponentType.kAudioUnitType_Output,
                componentSubType = AudioComponentDescription.AudioComponentSubType.kAudioUnitSubType_RemoteIO,
                componentManufacturer = AudioComponentDescription.AudioComponentManufacturerType.kAudioUnitManufacturer_Apple,
                componentFlags = 0,
                componentFlagsMask = 0
            };
            int remoteIONode = _auGraph.AddNode(cd);
            AudioUnit remoteIOUnit = _auGraph.GetNodeInfo(remoteIONode);

            // turning on microphone         
            /*
            remoteIOUnit.SetEnableIO(true, 
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input,
                1 // remote input
                );*/
            
            // audio canonical format
            AudioStreamBasicDescription audioFormat = CanonicalASBD(44100, 1);
            remoteIOUnit.SetAudioFormat(audioFormat,
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Output, // output bus of Remote input
                1 // Remote input
                );
            remoteIOUnit.SetAudioFormat(audioFormat,
                 AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input,
                 0 // Remote output,
                 );

            // Connecting Remote Input to Remote Output
            _auGraph.ConnnectNodeInput(
                remoteIONode, 1,
                remoteIONode, 0);

            // getting output audio format
            _audioUnitOutputFormat = remoteIOUnit.GetAudioFormat(
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Output,  // Remote output bus
                0 // Remote output
                );

            // 
            _auGraph.RenderCallback += new EventHandler<AudioGraphEventArgs>(_auGraph_RenderCallback);
            // graph initialization
            _auGraph.Initialize();
        }

        AudioStreamBasicDescription CanonicalASBD(double sampleRate, int channel)
        {
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
                SampleRate = sampleRate,
                Format = AudioFormatType.LinearPCM,
                //    kAudioFormatFlagsCanonical  = kAudioFormatFlagIsSignedInteger | kAudioFormatFlagsNativeEndian | kAudioFormatFlagIsPacked,
                FormatFlags = (AudioFormatFlags)((int)AudioFormatFlags.IsSignedInteger | (int)AudioFormatFlags.IsPacked),
                ChannelsPerFrame = channel,
                BytesPerPacket = AudioUnitSampleTypeSize * channel,
                BytesPerFrame = AudioUnitSampleTypeSize * channel,
                FramesPerPacket = 1,
                BitsPerChannel = 8 * AudioUnitSampleTypeSize,
                Reserved = 0
            };
            
            return audioFormat;
        }
        #endregion

        #region Public methods
        public void StartRecording(CFUrl url)
        {
            //  convertion audio format (AIFF)
            AudioStreamBasicDescription outputFormat = new AudioStreamBasicDescription()
            {
                SampleRate = 44100,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.IsBigEndian | AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsPacked,
                FramesPerPacket = 1,
                ChannelsPerFrame = 1,
                BitsPerChannel = 16,
                BytesPerPacket = 2,
                BytesPerFrame = 2,
                Reserved = 0
            };

            _extAudioFile = ExtAudioFile.CreateWithURL(url, AudioFileType.AIFF, outputFormat, null, AudioFileFlags.EraseFlags);                             
            _extAudioFile.ClientDataFormat = _audioUnitOutputFormat;
            _extAudioFile.Seek(0);

            _isRecording = true;

        }
        public void StopRecording()
        {
            _isRecording = false;
            _extAudioFile.Dispose();
        }
        public void Play()
        {
            if (!_isPlaying)
            {
                _auGraph.Start();
            }
            _isPlaying = true;
        }
        public void Stop()
        {
            if (_isPlaying)
            {
                _auGraph.Stop();
            }
            _isPlaying = false;
        }
        #endregion

    }
}
