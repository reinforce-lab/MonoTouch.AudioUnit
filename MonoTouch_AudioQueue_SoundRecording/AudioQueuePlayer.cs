using System;
using MonoTouch.AudioToolbox;
using MonoTouch.CoreFoundation;

namespace Monotouch_SoundRecording
{
    class AudioQueuePlayer
    {
        #region Variables
        CFUrl _url;
        AudioFile _audioFile;
        OutputAudioQueue _audioQueue;        
        long _startingPacketCount;
        int _numPacketsToRead;
        int _bufferByteSize;
		bool _isPlaying;
        bool _isPrepared;

        #endregion

        #region Properties
        public bool IsPlaying { get { return _isPlaying; } }
        #endregion

        #region Constructor
        public AudioQueuePlayer(CFUrl url)
		{            
            _url = url; 
			_isPlaying  = false;
            _isPrepared = false;
        }
        #endregion

        #region Private methods
        void prepareAudioQueue()
        {
            _audioFile = AudioFile.Open(_url, AudioFilePermission.Read, AudioFileType.AIFF);

            // Getting AudioStreamBasicDescription
            var audioFormat = _audioFile.StreamBasicDescription;

            // Creating an audio output queue object instance            
            _audioQueue = new OutputAudioQueue(audioFormat);
            _audioQueue.OutputCompleted += new EventHandler<OutputCompletedEventArgs>(_audioQueue_OutputCompleted);

            // Getting packet size
            int maxPacketSize = _audioFile.MaximumPacketSize;
            _startingPacketCount = 0;

            _numPacketsToRead = 1024;
            _bufferByteSize = _numPacketsToRead * maxPacketSize;

            // enqueue buffers
            IntPtr bufferPtr;
            for (int index = 0; index < 3; index++)
            {
                _audioQueue.AllocateBuffer(_bufferByteSize, out bufferPtr);
                outputCallback(bufferPtr);
            }
            _isPrepared = true;
        }

        void _audioQueue_OutputCompleted(object sender, OutputCompletedEventArgs e)
        {
            if (!_isPlaying)
                return;

            outputCallback(e.IntPtrBuffer);
        }
        
        void outputCallback(IntPtr bufPtr)
        {	
            // reading packets
            var buffer = new byte[_bufferByteSize];
            var descs = _audioFile.ReadPacketData(_startingPacketCount, _numPacketsToRead, buffer);            
            if (_startingPacketCount < _audioFile.DataPacketCount)
            {
                unsafe
                {
                    fixed (byte* ptr = buffer)
                    {
                        OutputAudioQueue.FillAudioData(bufPtr, 0, new IntPtr((void*)ptr), 0, _bufferByteSize);
                    }
                }
                
                _startingPacketCount += _numPacketsToRead;
                // enqueue a buffer
                _audioQueue.EnqueueBuffer(bufPtr, _bufferByteSize, null);                
            }
            else
            {
                if (_isPlaying)
                {
                    Stop();
                }
            }
        }
        #endregion

        #region Public methods
        public void Play()
        {
            if (!_isPrepared)
            {
                prepareAudioQueue();
            }
            _isPlaying = true;
            _audioQueue.Start();            
        }
        public void Stop()
        {
            _isPlaying = false;

            _audioQueue.Stop(true);
            _audioFile.Dispose();
            _audioQueue.Dispose();
            _isPrepared = false;            
        }
        #endregion
    }
}
