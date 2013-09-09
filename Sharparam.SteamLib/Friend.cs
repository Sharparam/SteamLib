/* Friend.cs
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
using System.Drawing;
using Sharparam.SteamLib.Events;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class Friend : IEquatable<Friend>, IEquatable<CSteamID>, IEquatable<LocalUser>
    {
        public event ChatMessageReceivedEventHandler ChatMessageReceived;

        private readonly log4net.ILog _log;

        private readonly Steam _steam;
        private readonly List<ChatMessage> _chatHistory;

        public readonly CSteamID Id;
        public readonly Bitmap Avatar;
        public readonly ReadOnlyCollection<ChatMessage> ChatHistory;

        public EPersonaState State { get { return _steam.Helper.GetFriendState(Id); } }

        public string StateText { get { return _steam.Helper.GetFriendStateText(Id); } }

        public bool Online { get { return State != EPersonaState.k_EPersonaStateOffline; } }

        public string Name { get { return _steam.Helper.GetFriendName(Id); } }

        public string Nickname
        {
            get
            {
                string nick;
                try
                {
                    nick = _steam.Helper.GetFriendNickname(Id);
                }
                catch (ArgumentNullException)
                {
                    nick = null;
                }
                return nick;
            }
        }

        internal Friend(Steam steam, CSteamID id, Bitmap avatar = null, List<ChatMessage> oldChatHistory = null)
        {
            _log = LogManager.GetLogger(this);
            
            _steam = steam;
            Id = id;
            Avatar = avatar;
            _chatHistory = oldChatHistory ?? new List<ChatMessage>();
            ChatHistory = new ReadOnlyCollection<ChatMessage>(_chatHistory);
        }

        #region Equality Members

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var id = obj as CSteamID;

            if (id != null && Equals(id))
                return true;

            var user = obj as LocalUser;

            if ((object) user != null && Equals(user))
                return true;

            return obj.GetType() == GetType() && Equals((Friend) obj);
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }

        public bool Equals(Friend other)
        {
            return (object) other != null && Id == other.Id;
        }

        public bool Equals(CSteamID other)
        {
            return other != null && Id == other;
        }

        public bool Equals(LocalUser other)
        {
            return (object) other != null && Id == other.Id;
        }

        public static bool operator ==(Friend left, object right)
        {
            return (object) left != null && left.Equals(right);
        }

        public static bool operator !=(Friend left, object right)
        {
            return !(left == right);
        }

        

        #endregion Equality Members

        public static explicit operator CSteamID(Friend friend)
        {
            return friend.Id;
        }

        private void OnChatMessageReceived(ChatMessage message)
        {
            var func = ChatMessageReceived;
            if (func != null)
                func(this, new ChatMessageEventArgs(message));
        }

        internal void AddChatMessage(ChatMessage message)
        {
            _chatHistory.Add(message);
            OnChatMessageReceived(message);
        }

        public void SendMessage(string message, EChatEntryType type = EChatEntryType.k_EChatEntryTypeChatMsg)
        {
            if (type == EChatEntryType.k_EChatEntryTypeEmote)
                _log.Warn("Steam no longer supports sending emotes to chat");
            _steam.Helper.SendMessage(Id, type, message);
        }
    }
}
