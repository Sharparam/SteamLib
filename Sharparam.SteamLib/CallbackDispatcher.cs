namespace Sharparam.SteamLib
{
    using System.Collections.Generic;
    using System.Threading;

    using Steam4NET;

    internal static class CallbackDispatcher
    {
        private static readonly Dictionary<int, ICallback> RegisteredCallbacks = new Dictionary<int, ICallback>();

        private static readonly Dictionary<int, Thread> ManagedThreads = new Dictionary<int, Thread>();

        internal static void RegisterCallback(ICallback callback, int iCallback)
        {
            RegisteredCallbacks.Add(iCallback, callback);
        }

        internal static void UnregisterCallback(ICallback callback, int iCallback)
        {
            if (RegisteredCallbacks[iCallback] == callback)
                RegisteredCallbacks.Remove(iCallback);
        }

        internal static void RunCallbacks(int pipe)
        {
            var callbackMsg = new CallbackMsg_t();

            if (Steamworks.GetCallback(pipe, ref callbackMsg))
            {
                ICallback callback;
                if (RegisteredCallbacks.TryGetValue(callbackMsg.m_iCallback, out callback))
                    callback.Run(callbackMsg.m_pubParam);
                Steamworks.FreeLastCallback(pipe);
            }
        }

        private static void DispatchThread(object param)
        {
            var pipe = (int)param;

            while (true)
            {
                RunCallbacks(pipe);
                Thread.Sleep(1);
            }
        }

        internal static void SpawnDispatchThread(int pipe)
        {
            if (ManagedThreads.ContainsKey(pipe))
                return;

            var dispatchThread = new Thread(DispatchThread);
            dispatchThread.Start(pipe);
            ManagedThreads[pipe] = dispatchThread;
        }

        internal static void StopDispatchThread(int pipe)
        {
            Thread dispatchThread;

            if (ManagedThreads.TryGetValue(pipe, out dispatchThread))
            {
                dispatchThread.Abort();
                dispatchThread.Join(2500);

                ManagedThreads.Remove(pipe);
            }
        }
    }
}
