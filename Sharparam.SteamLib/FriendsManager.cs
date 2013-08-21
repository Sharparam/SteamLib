﻿/* FriendsManager.cs
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
        private readonly ILog _log;

        public event FriendsUpdatedEventHandler FriendsUpdated;

        private readonly ISteamUtils005 _steamUtils;

        private readonly SteamFriends _steamFriends;

        private Friend[] _friends;

        private int _friendCount;

        private DateTime _lastUpdate;
        private static readonly TimeSpan UpdateInterval = new TimeSpan(0, 0, 10);

        public ReadOnlyCollection<Friend> Friends;

        internal FriendsManager(SteamFriends steamFriends, ISteamUtils005 steamUtils, Client client)
        {
            _log = LogManager.GetLogger(this);
            _log.Debug(">> FriendsManager([clientFriends])");
            _log.Info("FriendsManager is initializing");
            _steamFriends = steamFriends;
            _steamUtils = steamUtils;
            UpdateFriends();
            client.ChatMessageReceived += HandleChatMessage;
            _log.Debug("<< FriendsManager()");
        }

        private void OnFriendsUpdated()
        {
            _log.Debug(">> OnFriendsUpdated()");
            var func = FriendsUpdated;
            if (func != null)
                func(this, null);
            _log.Debug("<< OnFriendsUpdated()");
        }

        public void UpdateFriends()
        {
            _log.Debug(">> UpdateFriends()");

            var now = DateTime.Now;
            if ((now - _lastUpdate) < UpdateInterval)
            {
                _log.Debug("Less than _updateInterval since last update, aborting");
                _log.Debug("<< UpdateFriends()");
                return;
            }

            var oldFriends = _friends == null ? null : (Friend[]) _friends.Clone();
            var newFriendCount = _steamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            if (_friends == null || newFriendCount != _friendCount)
            {
                _friendCount = newFriendCount;
                _friends = new Friend[_friendCount];
            }
            
            for (int i = 0; i < _friendCount; i++)
            {
                var friend = _steamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                Friend oldFriend = oldFriends == null ? null : oldFriends.FirstOrDefault(f => f.SteamID == friend);
                _friends[i] = new Friend(_steamFriends, friend, oldFriend == null ? null :  oldFriend.ChatHistory.ToList());
                var avatar = _steamFriends.GetLargeFriendAvatar(_friends[i].SteamID);
                if (avatar != null)
                    _friends[i].Avatar = avatar;
            }

            Friends = new ReadOnlyCollection<Friend>(_friends.ToList());
            OnFriendsUpdated();
            _lastUpdate = now;
            _log.Debug("<< UpdateFriends()");
        }

        public bool IsFriend(CSteamID id)
        {
            _log.DebugFormat(">< IsFriend({0})", id.Render());
            return _friends.Any(f => f.SteamID == id);
        }

        public Friend GetFriendBySteamId(CSteamID id)
        {
            _log.DebugFormat(">< GetFriendBySteamId({0})", id.Render());
            return _friends.FirstOrDefault(f => f.SteamID == id);
        }

        public Friend GetFriendByName(string name, bool caseSensitive = true)
        {
            _log.DebugFormat(">< GetFriendByName({0}, {1})", name, caseSensitive ? "true" : "false");
            return _friends.FirstOrDefault(f => (caseSensitive ? f.GetName() : f.GetName().ToLower()) == (caseSensitive ? name : name.ToLower()));
        }

        public Friend GetFriendByMatching(string name, bool caseSensitive = true)
        {
            _log.DebugFormat(">< GetFriendByMatching({0}, {1})", name, caseSensitive ? "true" : "false");
            return _friends.FirstOrDefault(f => (caseSensitive ? f.GetName() : f.GetName().ToLower()).Contains(caseSensitive ? name : name.ToLower()));
        }

        private void HandleChatMessage(object sender, ChatMessageEventArgs e)
        {
            _log.Debug(">> HandleChatMessage([sender], [e])");
            var msg = e.Message;
            var friend = _friends.FirstOrDefault(f => f.SteamID == msg.Sender || f.SteamID == msg.Receiver);
            if (friend == null)
            {
                _log.Debug("Friend is null (sender nor receiver is friend of current user), aborting");
                _log.Debug("<< HandleChatMessage()");
                return;
            }
            friend.AddChatMessage(msg);
            _log.Debug("<< HandleChatMessage()");
        }
    }
}