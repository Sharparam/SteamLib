﻿/* Steam.cs
 *
 * Copyright © 2013 by Adam Hellberg.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class Steam : IDisposable
    {
        private readonly log4net.ILog _log;

        private readonly int _pipe;
        private readonly int _user;

        internal readonly ISteamClient012 SteamClient;
        internal readonly ISteamUtils005 SteamUtils;
        internal readonly ISteamUser016 SteamUser;
        internal readonly ISteamFriends002 SteamFriends002;
        internal readonly ISteamFriends013 SteamFriends013;
        internal readonly IClientEngine ClientEngine;
        internal readonly IClientFriends ClientFriends;

        public bool IsDisposed { get; private set; }

        public readonly LocalUser LocalUser;
        public readonly SteamHelper Helper;
        public readonly FriendsList Friends;

        public Steam()
        {
            _log = LogManager.GetLogger(this);

            _log.Info("Steam initializing...");

            if (!Steamworks.Load())
            {
                _log.Error("Failed to load Steamworks.");
                throw new SteamException("Failed to load Steamworks");
            }

            SteamClient = Steamworks.CreateInterface<ISteamClient012>();

            if (SteamClient == null)
            {
                _log.Error("Failed to create SteamClient interface");
                throw new SteamException("Failed to create SteamClient interface");
            }

            _pipe = SteamClient.CreateSteamPipe();

            if (_pipe == 0)
            {
                _log.Error("Failed to create Steam pipe");
                throw new SteamException("Failed to create Steam pipe");
            }

            _user = SteamClient.ConnectToGlobalUser(_pipe);

            if (_user == 0)
            {
                _log.Error("Failed to connect user");
                throw new SteamException("Failed to connect to global user");
            }

            SteamUtils = SteamClient.GetISteamUtils<ISteamUtils005>(_pipe);

            if (SteamUtils == null)
            {
                _log.Error("Failed to obtain Steam utils");
                throw new SteamException("Failed to obtain Steam utils");
            }

            SteamUser = SteamClient.GetISteamUser<ISteamUser016>(_user, _pipe);

            if (SteamUser == null)
            {
                _log.Error("Failed to obtain Steam user interface");
                throw new SteamException("Failed to obtain Steam user interface");
            }

            SteamFriends002 = SteamClient.GetISteamFriends<ISteamFriends002>(_user, _pipe);

            if (SteamFriends002 == null)
            {
                _log.Error("Failed to obtain Steam friends (002)");
                throw new SteamException("Failed to obtain Steam friends (002)");
            }

            SteamFriends013 = SteamClient.GetISteamFriends<ISteamFriends013>(_user, _pipe);

            if (SteamFriends013 == null)
            {
                _log.Error("Failed to obtain Steam friends (013)");
                throw new SteamException("Failed to obtain Steam friends (013)");
            }

            ClientEngine = Steamworks.CreateInterface<IClientEngine>();

            if (ClientEngine == null)
            {
                _log.Error("Failed to create client engine");
                throw new SteamException("Failed to create client engine");
            }

            ClientFriends = ClientEngine.GetIClientFriends<IClientFriends>(_user, _pipe);

            if (ClientFriends == null)
            {
                _log.Error("Failed to obtain client friends");
                throw new SteamException("Failed to obtain client friends");
            }

            Helper = new SteamHelper(this);
            LocalUser = new LocalUser(this);
            Friends = new FriendsList(this);
        }

        ~Steam()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;


        }
    }
}
