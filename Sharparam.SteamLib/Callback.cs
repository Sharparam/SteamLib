namespace Sharparam.SteamLib
{
    using System;
    using System.Runtime.InteropServices;

    internal class Callback<T> : ICallback
    {
        public delegate void DispatchDelegate(T param);

        public event DispatchDelegate OnRun;

        public int iCallback { get; private set; }

        public Callback(int iCallback)
        {
            this.iCallback = iCallback;
            CallbackDispatcher.RegisterCallback(this, iCallback);
        }

        public Callback(DispatchDelegate handler, int iCallback) : this(iCallback)
        {
            OnRun += handler;
        }

        ~Callback()
        {
            UnRegister();
        }

        public void UnRegister()
        {
            CallbackDispatcher.UnregisterCallback(this, iCallback);
        }

        public void Run(IntPtr pubParam)
        {
            var handler = OnRun;
            if (handler != null)
                handler((T)Marshal.PtrToStructure(pubParam, typeof(T)));
        }
    }
}
