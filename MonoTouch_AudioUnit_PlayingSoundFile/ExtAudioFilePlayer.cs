using System;
using System.Collections.Generic;
using System.Text;

using MonoTouch.AudioUnitWrapper;
using MonoTouch.AudioToolbox;
using MonoTouch.CoreFoundation;

namespace Monotouch_AudioUnit_PlayingSoundFile
{
    class ExtAudioFilePlayer : IDisposable
    {
        #region Variables        
        const int kAudioUnitSampleFractionBits = 24;
        readonly CFUrl _url;
        readonly int _sampleRate;

        AudioComponent _audioComponent;
        AudioUnit _audioUnit;        
        ExtAudioFile _extAudioFile;

        AudioStreamBasicDescription _srcFormat;
        AudioStreamBasicDescription _dstFormat;        

        //long _startingPacketCount;
        long _totalFrames;                
        #endregion

        #region Properties        
        public long TotalFrames { get { return _totalFrames; } }
        public long CurrentPosition
        {
            set
            {
                long frame = value;
                frame = Math.Min(frame, _totalFrames);
                frame = Math.Max(frame, 0);
                
                _extAudioFile.Seek(frame);
            }
            get
            {
                return _extAudioFile.FileTell();
            }
        }
        #endregion

        #region Constructor
        public ExtAudioFilePlayer(CFUrl url)
        {
            _sampleRate = 44100;
            _url = url;

            prepareExtAudioFile();
            prepareAudioUnit();
        }
        #endregion

        #region private methods
        void _audioUnit_RenderCallback(object sender, AudioUnitEventArgs e)
        {
            // reading buffer
            uint numberFrames = e.NumberFrames;
            numberFrames = _extAudioFile.Read(numberFrames, e.Data);            
            // is EOF?
            if (numberFrames != e.NumberFrames)
            {
                // loop back to file head
                _extAudioFile.Seek(0);                
                Stop();
            }
        }

        AudioStreamBasicDescription AUCanonicalASBD(double sampleRate, int channel)
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
                //kAudioFormatFlagsAudioUnitCanonical = kAudioFormatFlagIsSignedInteger | kAudioFormatFlagsNativeEndian | kAudioFormatFlagIsPacked | kAudioFormatFlagIsNonInterleaved | (kAudioUnitSampleFractionBits << kLinearPCMFormatFlagsSampleFractionShift),
                FormatFlags = (AudioFormatFlags)((int)AudioFormatFlags.IsSignedInteger | (int)AudioFormatFlags.IsPacked | (int)AudioFormatFlags.IsNonInterleaved | (int)(kAudioUnitSampleFractionBits << (int)AudioFormatFlags.LinearPCMSampleFractionShift)),
                ChannelsPerFrame = channel,
                BytesPerPacket = AudioUnitSampleTypeSize,
                BytesPerFrame = AudioUnitSampleTypeSize,
                FramesPerPacket = 1,
                BitsPerChannel = 8 * AudioUnitSampleTypeSize,
                Reserved = 0
            };
            return audioFormat;
        }

        void prepareExtAudioFile()
        {
            // Opening Audio File
            _extAudioFile = ExtAudioFile.OpenURL(_url);

            // Getting file data format
            _srcFormat = _extAudioFile.FileDataFormat;

            // Setting the channel number of the output format same to the input format
            _dstFormat = AUCanonicalASBD(_sampleRate, _srcFormat.ChannelsPerFrame);

            // setting reading format as audio unit cannonical format
            _extAudioFile.ClientDataFormat = _dstFormat;

            // getting total frame
            _totalFrames = _extAudioFile.FileLengthFrames;
            
            // Seeking to the file head
            _extAudioFile.Seek(0);
        }

        void prepareAudioUnit()
        {
            // creating an AudioComponentDescription of the RemoteIO AudioUnit
            AudioComponentDescription cd = new AudioComponentDescription()
            {
                componentType = AudioComponentDescription.AudioComponentType.kAudioUnitType_Output,
                componentSubType = AudioComponentDescription.AudioComponentSubType.kAudioUnitSubType_RemoteIO,
                componentManufacturer = AudioComponentDescription.AudioComponentManufacturerType.kAudioUnitManufacturer_Apple,
                componentFlags = 0,
                componentFlagsMask = 0
            };

            // Getting AudioComponent using the audio component description
            _audioComponent = AudioComponent.FindComponent(cd);

            // creating an audio unit instance
            _audioUnit = AudioUnit.CreateInstance(_audioComponent);

            // setting audio format
            _audioUnit.SetAudioFormat(AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input, _dstFormat);

            // setting callback method
            _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(_audioUnit_RenderCallback);
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
            Stop();
            _audioUnit.Dispose();
            _extAudioFile.Dispose();            
        }
        #endregion
    }
}
