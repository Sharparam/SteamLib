namespace Sharparam.SteamLib
{
    using System;

    internal interface ICallback
    {
        void Run(IntPtr param);
    }
}
