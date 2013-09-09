/* FriendsManager.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using Sharparam.SteamLib.Events;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class FriendsManager
    {
        private readonly log4net.ILog _log;

        public event FriendsUpdatedEventHandler FriendsUpdated;

        private Friend[] _friends;

        internal FriendsManager(SteamHelper steamHelper, ISteamUtils005 steamUtils, Client client)
        {
            _log = LogManager.GetLogger(this);
            client.ChatMessageReceived += HandleChatMessage;
        }

        private void OnFriendsUpdated()
        {
            var func = FriendsUpdated;
            if (func != null)
                func(this, null);
        }

        public void UpdateFriends()
        {
            
        }

        private void HandleChatMessage(object sender, ChatMessageEventArgs e)
        {
            var msg = e.Message;
            var friend = _friends.FirstOrDefault(f => f.Id == msg.Sender || f.Id == msg.Receiver);
            if (friend == null)
            {
                _log.Debug("Friend is null (sender nor receiver is friend of current user), aborting");
                return;
            }
            friend.AddChatMessage(msg);
        }
    }
}
