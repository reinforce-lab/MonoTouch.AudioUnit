//
// ExtAudioFile.cs: ExtAudioFile wrapper class
//
// Author:
//   AKIHIRO Uehara (u-akihiro@reinforce-lab.com)
//
// Copyright 2010 Reinforce Lab.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using MonoTouch.AudioToolbox;

namespace MonoTouch.AudioToolbox
{
    public class ExtAudioFile : IDisposable
    {
        #region Variables
        const int kAudioUnitSampleFractionBits = 24;
        readonly IntPtr _extAudioFile;
        #endregion

        #region Property        
        public long FileLengthFrames
        {
            get {
                long length = 0;
                uint size   = (uint)Marshal.SizeOf(typeof(long));
                
                int err = ExtAudioFileGetProperty(_extAudioFile,
                    ExtAudioFilePropertyIDType.kExtAudioFileProperty_FileLengthFrames,
                    ref size, ref length);
                if (err != 0)
                {
                    throw new InvalidOperationException(String.Format("Error code:{0}", err));
                }

                return length;
            }
        }

        public AudioStreamBasicDescription FileDataFormat
        {
            get
            {
                AudioStreamBasicDescription dc = new AudioStreamBasicDescription();
                uint size = (uint)Marshal.SizeOf(typeof(AudioStreamBasicDescription));
                int err = ExtAudioFileGetProperty(_extAudioFile,
                    ExtAudioFilePropertyIDType.kExtAudioFileProperty_FileDataFormat,
                    ref size, ref dc);
                if (err != 0)
                {
                    throw new InvalidOperationException(String.Format("Error code:{0}", err));
                }

                return dc;
            }
        }

        public AudioStreamBasicDescription ClientDataFormat
        {
            set
            {                
                int err = ExtAudioFileSetProperty(_extAudioFile,
                    ExtAudioFilePropertyIDType.kExtAudioFileProperty_ClientDataFormat,
                    (uint)Marshal.SizeOf(value), ref value);
                if (err != 0)
                {
                    throw new InvalidOperationException(String.Format("Error code:{0}", err));
                }
            }
        }           
        #endregion

        #region Constructor
        private ExtAudioFile(IntPtr ptr)
        {
            _extAudioFile = ptr;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public methods        
        public static ExtAudioFile OpenURL(MonoTouch.CoreFoundation.CFUrl url)
        { 
            int err;
            IntPtr ptr = new IntPtr();
            unsafe {                
                err = ExtAudioFileOpenURL(url.Handle, (IntPtr)(&ptr));
            }            
            if (err != 0)
            {
                throw new ArgumentException(String.Format("Error code:{0}", err));
            }
            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Can not get object instance");
            }
            
            return new ExtAudioFile(ptr);
        }
        public static ExtAudioFile CreateWithURL(MonoTouch.CoreFoundation.CFUrl url,
            AudioFileType fileType, 
            AudioStreamBasicDescription inStreamDesc, 
            //AudioChannelLayout channelLayout, 
            AudioFileFlags flag)
        {             
            int err;
            IntPtr ptr = new IntPtr();
            unsafe {                
                err = ExtAudioFileCreateWithURL(url.Handle, fileType, ref inStreamDesc, IntPtr.Zero, (uint)flag,
                    (IntPtr)(&ptr));
            }            
            if (err != 0)
            {
                throw new ArgumentException(String.Format("Error code:{0}", err));
            }
            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Can not get object instance");
            }
            
            return new ExtAudioFile(ptr);         
        }
        public void Seek(long frameOffset)
        {
            int err = ExtAudioFileSeek(_extAudioFile, frameOffset);
            if (err != 0)
            {
                throw new ArgumentException(String.Format("Error code:{0}", err));
            }
        }
        public long FileTell()
        {
            long frame = 0;
            
            int err = ExtAudioFileTell(_extAudioFile, ref frame);
            if (err != 0)
            {
                throw new ArgumentException(String.Format("Error code:{0}", err));
            }
            
            return frame;
        }
        public uint Read(uint numberFrames, AudioBufferList data)
        {            
            int err = ExtAudioFileRead(_extAudioFile, ref numberFrames, data);
            if (err != 0)
            {
                throw new ArgumentException(String.Format("Error code:{0}", err));
            }

            return numberFrames;
        }
        public void WriteAsync(uint numberFrames, AudioBufferList data)
        {
            int err = ExtAudioFileWriteAsync(_extAudioFile, numberFrames, data);
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:{0}", err));            
        }
        public void Write(uint numberFrames, AudioBufferList data)
        {
            int err = ExtAudioFileWrite(_extAudioFile, numberFrames, data);
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:{0}", err));
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            ExtAudioFileDispose(_extAudioFile);            
        }
        #endregion


        #region Interop
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileOpenURL")]
        static extern int ExtAudioFileOpenURL(IntPtr inUrl, IntPtr outExtAudioFile); // caution

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileRead")]
        static extern int ExtAudioFileRead(IntPtr  inExtAudioFile, ref uint ioNumberFrames, AudioBufferList ioData);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileWriteAsync")]
        static extern int ExtAudioFileWriteAsync(IntPtr inExtAudioFile, uint inNumberFrames, AudioBufferList ioData);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileWrite")]
        static extern int ExtAudioFileWrite(IntPtr inExtAudioFile, uint inNumberFrames, AudioBufferList ioData);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileDispose")]
        static extern int ExtAudioFileDispose(IntPtr inExtAudioFile);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileSeek")]
        static extern int ExtAudioFileSeek(IntPtr inExtAudioFile, long inFrameOffset);
        
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileTell")]
        static extern int ExtAudioFileTell(IntPtr inExtAudioFile, ref long outFrameOffset);
        
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileCreateWithURL")]
        static extern int ExtAudioFileCreateWithURL(IntPtr inURL,
            [MarshalAs(UnmanagedType.U4)] AudioFileType inFileType,
            ref AudioStreamBasicDescription inStreamDesc,
            IntPtr inChannelLayout, //AudioChannelLayout inChannelLayout, AudioChannelLayout results in compilation error (error code 134.)
            UInt32 flags,
            IntPtr outExtAudioFile);            
        
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileGetProperty")]
        static extern int ExtAudioFileGetProperty(
            IntPtr inExtAudioFile, 
            ExtAudioFilePropertyIDType inPropertyID,
            ref uint ioPropertyDataSize,
            IntPtr outPropertyData);
        
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileGetProperty")]
        static extern int ExtAudioFileGetProperty(
            IntPtr inExtAudioFile,
            ExtAudioFilePropertyIDType inPropertyID,
            ref uint ioPropertyDataSize,
            ref AudioStreamBasicDescription outPropertyData);
        

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileGetProperty")]
        static extern int ExtAudioFileGetProperty(
            IntPtr inExtAudioFile,
            ExtAudioFilePropertyIDType inPropertyID,
            ref uint ioPropertyDataSize,
            ref long outPropertyData);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileSetProperty")]
        static extern int ExtAudioFileSetProperty(
            IntPtr inExtAudioFile,
            ExtAudioFilePropertyIDType inPropertyID,
            uint ioPropertyDataSize,
            IntPtr outPropertyData);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "ExtAudioFileSetProperty")]
        static extern int ExtAudioFileSetProperty(
            IntPtr inExtAudioFile,
            ExtAudioFilePropertyIDType inPropertyID,
            uint ioPropertyDataSize,
            ref AudioStreamBasicDescription outPropertyData);
        
        enum ExtAudioFilePropertyIDType {                 
	        kExtAudioFileProperty_FileDataFormat		= 0x66666d74, //'ffmt',   // AudioStreamBasicDescription
	        //kExtAudioFileProperty_FileChannelLayout		= 'fclo',   // AudioChannelLayout

            kExtAudioFileProperty_ClientDataFormat = 0x63666d74, //'cfmt',   // AudioStreamBasicDescription
	        //kExtAudioFileProperty_ClientChannelLayout	= 'cclo',   // AudioChannelLayout
	        //kExtAudioFileProperty_CodecManufacturer		= 'cman',	// UInt32
	
	        // read-only:
	        //kExtAudioFileProperty_AudioConverter		= 'acnv',	// AudioConverterRef
	        //kExtAudioFileProperty_AudioFile				= 'afil',	// AudioFileID
	        //kExtAudioFileProperty_FileMaxPacketSize		= 'fmps',	// UInt32
	        //kExtAudioFileProperty_ClientMaxPacketSize	= 'cmps',	// UInt32
	        kExtAudioFileProperty_FileLengthFrames		= 0x2366726d,//'#frm',	// SInt64
	
	        // writable:
	        //kExtAudioFileProperty_ConverterConfig		= 'accf',   // CFPropertyListRef
	        //kExtAudioFileProperty_IOBufferSizeBytes		= 'iobs',	// UInt32
	        //kExtAudioFileProperty_IOBuffer				= 'iobf',	// void *
	        //kExtAudioFileProperty_PacketTable			= 'xpti'	// AudioFilePacketTableInfo             
        };
        #endregion
    }
}
