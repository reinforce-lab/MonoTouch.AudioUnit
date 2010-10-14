using System;
using System.Collections.Generic;
using System.Text;

using MonoTouch.AudioToolbox;

namespace Monotouch_SoundRecording
{
    class AudioQueueRecorder : IDisposable
    {
        #region Variables
        const int _samplingRate = 44100;

        InputAudioQueue _queue;
        
        AudioFile _audioFile;        
        int _numPacketsToWrite;
        int _bufferByteSize;
        long _startingPacketCount;
        bool _isRecording;
        #endregion

        #region Properties
        public bool IsRecording { get { return _isRecording; } }
        #endregion

        #region Constructor
        public AudioQueueRecorder(MonoTouch.CoreFoundation.CFUrl url)
        {
            _isRecording = false;
            PrepareAudioQueue(url);
        }
        #endregion

        #region Public methods
        public void Start()
        {
            _isRecording = true;
            _queue.Start();
        }
        #endregion

        #region Private methods
        void PrepareAudioQueue(MonoTouch.CoreFoundation.CFUrl url)
        {
            AudioStreamBasicDescription audioFormat = new AudioStreamBasicDescription()
            {
                SampleRate = _samplingRate,
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsBigEndian | AudioFormatFlags.LinearPCMIsPacked,
                FramesPerPacket = 1,
                ChannelsPerFrame = 1, // monoral
                BitsPerChannel = 16, // 16-bit
                BytesPerPacket = 2,
                BytesPerFrame = 2,
                Reserved = 0
            };

            _audioFile = AudioFile.Create(url, AudioFileType.AIFF, audioFormat, AudioFileFlags.EraseFlags);
            
            _queue = new InputAudioQueue(audioFormat);
            _queue.InputCompleted += new EventHandler<InputCompletedEventArgs>(_queue_InputCompleted);

            _startingPacketCount = 0;
            _numPacketsToWrite = 1024;
            _bufferByteSize = (int)(_numPacketsToWrite * audioFormat.BytesPerPacket);

            // preparing queue buffer
            IntPtr bufferPtr;
            for (int index = 0; index < 3; index++)
            {
                //_queue.AllocateBuffer(_bufferByteSize, out bufferPtr);
                _queue.AllocateBufferWithPacketDescriptors(_bufferByteSize, _numPacketsToWrite, out bufferPtr);
                _queue.EnqueueBuffer(bufferPtr, _bufferByteSize, null);
            }
        }

        void _queue_InputCompleted(object sender, InputCompletedEventArgs e)
        {
            if (!_isRecording)
                return;

            var buffer = (AudioQueueBuffer)System.Runtime.InteropServices.Marshal.PtrToStructure(e.IntPtrBuffer, typeof(AudioQueueBuffer));
            _audioFile.WritePackets(false, _startingPacketCount, buffer.PacketDescriptions, buffer.AudioData, (int)buffer.AudioDataByteSize);
            _startingPacketCount += _numPacketsToWrite;
            // adding a queue                    
            _queue.EnqueueBuffer(e.IntPtrBuffer, _bufferByteSize, e.PacketDescriptions);            
        }       
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            _isRecording = false; 

            _queue.Stop(true);
            _audioFile.Dispose();
            _queue.Dispose();

            _queue = null;
            _audioFile = null;
        }
        #endregion
    }
}
