/* FriendsList.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Sharparam.SteamLib.Events;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class FriendsList : ReadOnlyObservableCollection<Friend>
    {
        private readonly Steam _steam;

        private readonly ObservableCollection<Friend> _list;

        private bool _updating;

        private FriendsList(Steam steam, ObservableCollection<Friend> list) : base(list)
        {
            _steam = steam;
            _list = list;

            Update();
        }

        internal FriendsList(Steam steam) : this(steam, new ObservableCollection<Friend>())
        {
            
        }

        internal void Add(CSteamID id)
        {
            if (GetFriendById(id) != null)
                return;

            var friend = new Friend(_steam, id);
            _list.Add(friend);
        }

        public void Update(bool force = false)
        {
            if (_updating && !force)
                return;

            _updating = true;

            var old = new Friend[_list.Count];
            _list.CopyTo(old, 0);

            _list.Clear();

            var numFriends = _steam.SteamFriends002.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

            for (var i = 0; i < numFriends; i++)
            {
                var friendId = _steam.SteamFriends002.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                var oldFriend = old.FirstOrDefault(f => f.Id == friendId);
                _list.Add(oldFriend ?? new Friend(_steam, friendId));
            }

            _updating = false;
        }

        public bool Contains(CSteamID id)
        {
            return this.Any(f => f.Id == id);
        }

        #region Get Methods

        public Friend GetFriendById(CSteamID id)
        {
            return this.FirstOrDefault(f => f.Id == id);
        }

        public Friend GetFriendByName(string name, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public Friend GetFriendByPartialName(string search, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Name, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public IEnumerable<Friend> GetFriendsByName(string name, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Friend> GetFriendsByPartialName(string search, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Name, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public Friend GetFriendByNickname(string name, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public Friend GetFriendByPartialNickname(string search, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Nickname, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public IEnumerable<Friend> GetFriendsByNickname(string name, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Friend> GetFriendsByPartialNickname(string search, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Nickname, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public IEnumerable<Friend> GetFriendsByState(EPersonaState state)
        {
            return this.Where(f => f.State == state);
        }

        public IEnumerable<Friend> GetFriendsByStates(IEnumerable<EPersonaState> states)
        {
            return this.Where(f => states.Contains(f.State));
        }

        public IEnumerable<Friend> GetOnlineFriends()
        {
            return this.Where(f => f.Online);
        }

        #endregion Get Methods

        #region Notify methods

        internal void NotifyChanged(CSteamID id)
        {
            var friend = GetFriendById(id);
            if (friend != null)
                friend.NotifyChanged();
        }

        internal void NotifyNameChanged(CSteamID id)
        {
            var friend = GetFriendById(id);
            if (friend != null)
                friend.NotifyNameChanged();
        }

        internal void NotifyStateChanged(CSteamID id)
        {
            var friend = GetFriendById(id);
            if (friend != null)
                friend.NotifyStateChanged();
        }

        #endregion Notify methods

        #region Internal Handler Methods

        internal void HandleMessageEvent(CSteamID id, MessageEventArgs args)
        {
            var friend = GetFriendById(id);
            if (friend == null)
                return;
            friend.OnMessage(args);
            friend.AddMessage(args.Message);
        }

        internal void HandleChatMessageEvent(CSteamID id, MessageEventArgs args)
        {
            var friend = GetFriendById(id);
            if (friend != null)
                friend.OnChatMessage(args);
        }

        internal void HandleTypingMessageEvent(CSteamID id, MessageEventArgs args)
        {
            var friend = GetFriendById(id);
            if (friend != null)
                friend.OnTypingMessage(args);
        }

        internal void HandleMessageReceivedEvent(MessageEventArgs args)
        {
            var friend = GetFriendById(args.Message.Sender);
            if (friend != null)
                friend.OnMessageReceived(args);
        }

        internal void HandleChatMessageReceivedEvent(MessageEventArgs args)
        {
            var friend = GetFriendById(args.Message.Sender);
            if (friend != null)
                friend.OnChatMessageReceived(args);
        }

        internal void HandleTypingMessageReceivedEvent(MessageEventArgs args)
        {
            var friend = GetFriendById(args.Message.Sender);
            if (friend != null)
                friend.OnTypingMessageReceived(args);
        }

        internal void HandleMessageSentEvent(MessageEventArgs args)
        {
            var friend = GetFriendById(args.Message.Receiver);
            if (friend != null)
                friend.OnMessageSent(args);
        }

        internal void HandleChatMessageSentEvent(MessageEventArgs args)
        {
            var friend = GetFriendById(args.Message.Receiver);
            if (friend != null)
                friend.OnChatMessageSent(args);
        }

        internal void HandleTypingMessageSentEvent(MessageEventArgs args)
        {
            var friend = GetFriendById(args.Message.Receiver);
            if (friend != null)
                friend.OnTypingMessageSent(args);
        }

        #endregion Internal Handler Methods
    }
}
