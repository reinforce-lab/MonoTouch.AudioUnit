/*
using System;
using System.Collections.Generic;
using System.Text;

using MonoTouch.AudioUnitWrapper;
using MonoTouch.AudioToolbox;
using MonoTouch.CoreFoundation;

namespace Monotouch_AudioUnit_PlayingSoundFile
{
    class _AUAudioFilePlayer : IDisposable
    {
        #region Variables
        AudioFile _audioFile;
        AudioComponent _audioComponent;
        AudioUnit _audioUnit;
        _AudioConverter _audioConverter;

        CFUrl _url;
        int _sampleRate;
        
        AudioStreamBasicDescription _srcFormat;
        AudioStreamBasicDescription _dstFormat;
        AudioStreamPacketDescription[] _packetDescs;

        long _startingPacketCount;
        long _totalPacketCount;
        int _numPacketsToRead;
        
        byte[] _srcBuffer;
        int _srcSizePerPacket;

        bool _isLoop;
        bool _isDone;
        bool _isPlaying;
        #endregion

        #region Properties
        public bool isLoop { get; set; }
        public UInt64 totalPacketCount { get; }
        public UInt64 currentPosition { get; set; }
        #endregion

        #region Constructor
        public _AUAudioFilePlayer()
        {
            _sampleRate = 44100;

            prepareAudioUnit();
            prepareAudioConverter();
        }
        #endregion

        #region private methods
        void writeDecompressingMagicCookie(_AudioConverter audioConverter, AudioFile infile)
        {
            audioConverter.DecompressionMagicCookie = infile.MagicCookie;          
        }
        
        void _audioUnit_BufferCompleted(object sender, AudioUnitEventArgs e)
        {
            // copying audio data from the audio converter to the audio unit
            uint ioOutputDataPackets = e.NumberFrames;
            _audioConverter.FillBuffer(e.Data, ioOutputDataPackets, _packetDescs);
        }

        void _audioConverter_EncoderCallback(object sender, _AudioConverterEventArgs e)
        {
            if (_isDone)
            {
                e.NumberDataPackets = 0;
                return;
            }

            int maxPackets = _srcBuffer.Length / _srcSizePerPacket;
            if (e.NumberDataPackets > maxPackets)
            {
                e.NumberDataPackets = maxPackets;
            }

            _audioFile.ReadPacketData(false, _startingPacketCount, (int)e.NumberDataPackets, _srcBuffer, 0, _srcBuffer.Length);
            _startingPacketCount += e.NumberDataPackets;
            
            e.Data.mBuffers[0].mNumberChannels = (uint)_srcFormat.ChannelsPerFrame;
            e.Data.mBuffers[0].mDataByteSize = _srcBuffer.Length;
            e.Data.mBuffers[0].mData = _srcBuffer;

            if (e.DataPacketDescription != null)
            {
                if (_packetDescs != null)
                {
                    e.DataPacketDescription = _packetDescs;
                }
                else
                {
                    e.DataPacketDescription = null;
                }
            }
            if (_startingPacketCount == _totalPacketCount)
            {
                if (_isLoop)
                {
                    _startingPacketCount = 0;
                }
                else
                {
                    _isDone = true;
                }
            }
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
            int AudioUnitSampleTypeSize;
            if (MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR)
            {
                AudioUnitSampleTypeSize = sizeof(float);
            }
            else
            {
                AudioUnitSampleTypeSize = sizeof(int);
            }
            _dstFormat = new AudioStreamBasicDescription()
            {
                SampleRate = _sampleRate,
                Format = AudioFormatType.LinearPCM,
                //kAudioFormatFlagsAudioUnitCanonical = kAudioFormatFlagIsSignedInteger | kAudioFormatFlagsNativeEndian | kAudioFormatFlagIsPacked | kAudioFormatFlagIsNonInterleaved | (kAudioUnitSampleFractionBits << kLinearPCMFormatFlagsSampleFractionShift),
                FormatFlags = (AudioFormatFlags)((int)AudioFormatFlags.IsSignedInteger | (int)AudioFormatFlags.IsPacked | (int)AudioFormatFlags.IsNonInterleaved | (int)(kAudioUnitSampleFractionBits << (int)AudioFormatFlags.LinearPCMSampleFractionShift)),
                ChannelsPerFrame = 2,
                BytesPerPacket = AudioUnitSampleTypeSize,
                BytesPerFrame  = AudioUnitSampleTypeSize,
                FramesPerPacket = 1,
                BitsPerChannel = 8 * AudioUnitSampleTypeSize,
                Reserved = 0
            };
            _audioUnit.SetAudioFormat(AudioUnit.AudioUnitScopeType.kAudioUnitScope_Input, _dstFormat);

            // setting callback method
            _audioUnit.RenderCallback += new EventHandler<AudioUnitEventArgs>(_audioUnit_BufferCompleted);
        }
        void prepareAudioConverter()
        {
            // Opening Audio File
            _audioFile = AudioFile.Open(_url, AudioFilePermission.Read, AudioFileType.WAVE);
            _srcFormat = _audioFile.StreamBasicDescription;

            // Creating audio converter
            _audioConverter = _AudioConverter.CreateInstance(_audioFile.StreamBasicDescription, _dstFormat);
            _audioConverter.EncoderCallback += new EventHandler<_AudioConverterEventArgs>(_audioConverter_EncoderCallback);

            // Allocating buffer
            _srcBuffer = new byte[32768];            
            _startingPacketCount = 0;

            // preparing packet descriptions
            if (_srcFormat.BytesPerPacket == 0)
            {
                _srcSizePerPacket = _audioFile.PacketSizeUpperBound;
                _numPacketsToRead = _srcBuffer.Length / _audioFile.PacketSizeUpperBound;
                _packetDescs = new AudioStreamPacketDescription[_numPacketsToRead];
            }
            else
            {
                _srcSizePerPacket = _srcFormat.BytesPerPacket;
                _numPacketsToRead = _srcBuffer.Length / _srcFormat.BytesPerPacket;
                _packetDescs = null;
            }
            
            // getting number of total packet
            _totalPacketCount = _audioFile.DataPacketCount;
            
            // dispose magic key            
            writeDecompressingMagicCookie(_audioConverter, _audioFile);           
        }

        #endregion

        #region Public methods
        public void Play()
        { }
        public void Stop()
        {
        }
        public void prepareAudioConverter(String url)
        { 
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            Stop();
            _audioUnit.Dispose();
            _audioConverter.Dispose();            
        }
        #endregion
    }
}
*/