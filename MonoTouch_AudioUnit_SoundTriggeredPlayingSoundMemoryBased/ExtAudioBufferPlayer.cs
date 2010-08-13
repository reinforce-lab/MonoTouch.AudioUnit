using System;
using System.Collections.Generic;
using System.Text;

using MonoTouch.AudioToolbox;
using MonoTouch.CoreFoundation;

namespace Monotouch_AudioUnit_SoundTriggeredPlayingSoundMemoryBased
{
    class ExtAudioBufferPlayer : IDisposable
    {
        #region Variables        
        const int _playingDuration = 44100 * 2; // 2sec
        const int _threshold = 100000;
        
        readonly CFUrl _url;
        readonly int _sampleRate;

        AudioComponent _audioComponent;
        AudioUnit _audioUnit;        
        ExtAudioFile _extAudioFile;
        AudioBufferList _buffer;

        AudioStreamBasicDescription _srcFormat;
        AudioStreamBasicDescription _dstFormat;        

        //long _startingPacketCount;
        long _totalFrames;
        uint _currentFrame;
        int  _numberOfChannels;
        int  _triggered;
        float _signalLevel;
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
                _currentFrame = (uint)frame;                
            }
            get
            {
                return _currentFrame;
            }
        }
        public float SignalLevel
        {
            get { return _signalLevel; }
        }
        #endregion

        #region Constructor
        public ExtAudioBufferPlayer(CFUrl url)
        {
            _sampleRate = 44100;
            _url = url;

            _currentFrame = 0;

            prepareExtAudioFile();
            prepareAudioUnit();
        }
        #endregion

        #region private methods
        void _audioUnit_RenderCallback(object sender, AudioUnitEventArgs args)
        {
            // getting microphone input signal
            _audioUnit.Render(args.ActionFlags,
                args.TimeStamp,
                1, // Remote input
                args.NumberFrames,
                args.Data);

            // Getting a pointer to a buffer to be filled
            IntPtr outL = args.Data.mBuffers[0].mData;
            IntPtr outR = args.Data.mBuffers[1].mData;

            // Getting signal level and trigger detection
            unsafe
            {
                var outLPtr = (int*)outL.ToPointer();
                for (int i = 0; i < args.NumberFrames; i++)
                {
                    // LPF
                    float diff = Math.Abs(*outLPtr) - _signalLevel;
                    if (diff > 0)
                        _signalLevel += diff / 1000f;
                    else
                        _signalLevel += diff / 10000f;
                    
                    diff = Math.Abs(diff);
                    
                    // sound triger detection
                    if (_triggered <= 0 && diff > _threshold) 
                    {
                        _triggered = _playingDuration;
                    }
                }
            }                        

            // playing sound
            unsafe
            {
                var outLPtr = (int*)outL.ToPointer();
                var outRPtr = (int*)outR.ToPointer();                
                
                var buf0 = (int*)_buffer.mBuffers[0].mData;
                int *buf1= (_numberOfChannels == 2) ? (int*)_buffer.mBuffers[1].mData : buf0;

                for (int i = 0; i < args.NumberFrames; i++)
                {                    
                    _triggered = Math.Max(0, _triggered -1);

                    if (_triggered <= 0)
                    {
                        // 0-filling
                        *outLPtr++ = 0;
                        *outRPtr++ = 0;
                    }
                    else 
                    {
                        if (_currentFrame >= _totalFrames)
                        {
                            _currentFrame = 0;
                        }
                        
                        *outLPtr++ = buf0[++_currentFrame];
                        *outRPtr++ = buf1[_currentFrame];
                    }
                }
            }
        }

        void prepareExtAudioFile()
        {
            // Opening Audio File
            _extAudioFile = ExtAudioFile.OpenURL(_url);

            // Getting file data format
            _srcFormat = _extAudioFile.FileDataFormat;

            // Setting the channel number of the output format same to the input format
            _dstFormat = AudioUnitUtils.AUCanonicalASBD(_sampleRate, _srcFormat.ChannelsPerFrame);

            // setting reading format as audio unit cannonical format
            _extAudioFile.ClientDataFormat = _dstFormat;

            // getting total frame
            _totalFrames = _extAudioFile.FileLengthFrames;

            // Aloocating AudoBufferList
            _buffer = new AudioBufferList((uint)_srcFormat.ChannelsPerFrame, (uint)(sizeof(uint) * _totalFrames));
            _numberOfChannels = _srcFormat.ChannelsPerFrame;

            // Reading all frame into the buffer
            _extAudioFile.Read((uint)_totalFrames, _buffer);
        }

        void prepareAudioUnit()
        {
            // AudioSession
            AudioSession.Initialize();
            AudioSession.SetActive(true);
            AudioSession.Category = AudioSessionCategory.PlayAndRecord;
            AudioSession.PreferredHardwareIOBufferDuration = 0.005f;            

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

            // turning on microphone
            _audioUnit.SetEnableIO(true,
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input,
                1 // Remote Input
                );

            // setting audio format
            _audioUnit.SetAudioFormat(_dstFormat, 
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input, 
                0 // Remote Output
                );            
            _audioUnit.SetAudioFormat( AudioUnitUtils.AUCanonicalASBD(_sampleRate, 2),                  
                AudioUnit.AudioUnitScopeType.kAudioUnitScope_Output,                     
                1 // Remote input                     
                );


            // setting callback method
            _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(_audioUnit_RenderCallback);

            _audioUnit.Initialize();
            _audioUnit.Start();
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            _audioUnit.Stop(); 
            _audioUnit.Dispose();
            _extAudioFile.Dispose();            
        }
        #endregion
    }
}
