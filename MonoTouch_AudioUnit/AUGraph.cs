//
// AUGraph.cs: AUGraph wrapper class
//
// Author:
//   AKIHIRO Uehara (u-akihiro@reinforce-lab.com)
//
// Copyright 2010 Reinforce Lab.
//

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace MonoTouch.AudioToolbox
{
    public class AUGraph: IDisposable
    {
        #region Variables
        readonly GCHandle _handle;
        readonly IntPtr _auGraph;
        #endregion

        #region Properties
        public event EventHandler<AudioGraphEventArgs> RenderCallback;
        public IntPtr Handler { get { return _auGraph; } }
        #endregion

        #region Constructor
        private AUGraph(IntPtr ptr)
        {
            _auGraph = ptr;

            _handle = GCHandle.Alloc(this);            
            int err = AUGraphAddRenderNotify(_auGraph, renderCallback, GCHandle.ToIntPtr(_handle));
            if (err != 0)
                throw new ArgumentException(String.Format("Error code: {0}", err));
        }
        #endregion

        #region Private methods
        // callback funtion should be static method and be attatched a MonoPInvokeCallback attribute.        
        [MonoTouch.MonoPInvokeCallback(typeof(AudioUnit.AURenderCallback))]
        static int renderCallback(IntPtr inRefCon,
            ref AudioUnit.AudioUnitRenderActionFlags _ioActionFlags,
            ref AudioTimeStamp _inTimeStamp,
            uint _inBusNumber,
            uint _inNumberFrames,
            AudioBufferList _ioData)
        {
            // getting audiounit instance
            var handler = GCHandle.FromIntPtr(inRefCon);
            var inst = (AUGraph)handler.Target;

            // evoke event handler with an argument
            if (inst.RenderCallback != null)
            {
                var args = new AudioGraphEventArgs(
                    _ioActionFlags,
                    _inTimeStamp,
                    _inBusNumber,
                    _inNumberFrames,
                    _ioData);
                inst.RenderCallback(inst, args);
            }

            return 0; // noerror
        }
        #endregion

        #region Public methods
        public static AUGraph CreateInstance()
        { 
            var ptr = new IntPtr();
            int err = NewAUGraph(ref ptr);
            if (err != 0)
                throw new InvalidOperationException(String.Format("Cannot create new AUGraph. Error code:", err));

            err = AUGraphOpen(ptr);
            if (err != 0)
                throw new InvalidOperationException(String.Format("Cannot open AUGraph. Error code:", err));

            return new AUGraph(ptr);
        }
        public int AddNode(AudioComponentDescription cd)
        {
            int node = 0;
            int err = AUGraphAddNode(_auGraph, cd, ref node);
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:", err));
            
            return node;
        }
        public AudioUnit GetNodeInfo(int node)
        {
            int err;
            IntPtr ptr = new IntPtr();
            unsafe
            {
                err = AUGraphNodeInfo(_auGraph, node, null, (IntPtr)(&ptr));                
            }
            if (err != 0)
            {
                throw new ArgumentException(String.Format("Error code:{0}", err));
            }
            if (ptr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Can not get object instance");
            }

            return new AudioUnit(ptr);
        }
        public void ConnnectNodeInput(int inSourceNode, uint inSourceOutputNumber, int inDestNode, uint inDestInputNumber)
        {
            int err = AUGraphConnectNodeInput(_auGraph,                                    
                inSourceNode, inSourceOutputNumber,                                    
                inDestNode, inDestInputNumber                                    
                );
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:", err));            
        }
        public void AUGraphSetNodeInputCallback(int inDestNode, uint inDestInputNumber, AudioUnit.AURenderCallbackStrct inInputCallback)
        {
            int err = AUGraphSetNodeInputCallback(_auGraph,
                inDestNode,inDestInputNumber, inInputCallback);
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:", err));  
        }

        public void Start()
        {
            AUGraphStart(_auGraph);
        }
        public void Stop()
        {
            AUGraphStop(_auGraph);
        }
        public void Initialize()
        {
            int err = AUGraphInitialize(_auGraph);
            if (err != 0)
                throw new ArgumentException(String.Format("Error code:", err));
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            AUGraphUninitialize(_auGraph);
            AUGraphClose(_auGraph);
            DisposeAUGraph(_auGraph);

            _handle.Free();            
        }
        #endregion

        #region Interop
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "NewAUGraph")]
        static extern int NewAUGraph(ref IntPtr outGraph);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphOpen")]
        static extern int AUGraphOpen(IntPtr inGraph);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphAddNode")]
        static extern int AUGraphAddNode(IntPtr inGraph, AudioComponentDescription inDescription, ref int outNode);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphNodeInfo")]
        static extern int AUGraphNodeInfo(IntPtr inGraph, int inNode, AudioComponentDescription outDescription, IntPtr outAudioUnit);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphConnectNodeInput")]
        static extern int AUGraphConnectNodeInput(IntPtr inGraph, int inSourceNode, uint inSourceOutputNumber, int inDestNode, uint inDestInputNumber);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphInitialize")]
        static extern int AUGraphInitialize(IntPtr inGraph);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphAddRenderNotify")]
        static extern int AUGraphAddRenderNotify(IntPtr inGraph, AudioUnit.AURenderCallback inCallback, IntPtr inRefCon );

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphSetNodeInputCallback")]
        static extern int AUGraphSetNodeInputCallback(IntPtr inUnit, Int32 inDestNode, UInt32 inDestInputNumber, AudioUnit.AURenderCallbackStrct inInputCallback);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphStart")]
        static extern int AUGraphStart(IntPtr inGraph);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphStop")]
        static extern int AUGraphStop(IntPtr inGraph);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphUninitialize")]
        static extern int AUGraphUninitialize(IntPtr inGraph);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AUGraphClose")]
        static extern int AUGraphClose(IntPtr inGraph);

        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "DisposeAUGraph")]
        static extern int DisposeAUGraph(IntPtr inGraph);
        #endregion

    }
}
