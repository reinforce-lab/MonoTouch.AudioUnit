using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace MonoTouch.AudioUnitWrapper
{
    public class AudioComponent : IDisposable
    {
        #region Variables
        readonly IntPtr _handler;
        #endregion

        #region Properties
        public IntPtr Handler { get { return _handler; } }
        #endregion

        #region Constructor
        private AudioComponent(IntPtr handler)
        { 
            _handler = handler;
        }
        #endregion

        #region public methods
        public static AudioComponent FindNextComponent(AudioComponent cmp, AudioComponentDescription cd)
        {
            // Getting component hanlder
            IntPtr handler;
            if (cmp == null)
                handler = AudioComponentFindNext(IntPtr.Zero, cd);
            else
                handler = AudioComponentFindNext(cmp.Handler, cd);

            // creating an instance
            if (handler != IntPtr.Zero)
                return new AudioComponent(handler);
            else
                return null;

        }
        public static AudioComponent FindComponent(AudioComponentDescription cd)
        {
            return FindNextComponent(null, cd);
        }
        #endregion

        #region IDisposable メンバ
        public void Dispose()
        {
            AudioComponentInstanceDispose(_handler);            
        }
        #endregion

        #region Inteop
        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioComponentFindNext")]
        static extern IntPtr AudioComponentFindNext(IntPtr inComponent, AudioComponentDescription inDesc);


        [DllImport(MonoTouch.Constants.AudioToolboxLibrary, EntryPoint = "AudioComponentInstanceDispose")]
        static extern int AudioComponentInstanceDispose(IntPtr inInstance);
        #endregion

    }
}
