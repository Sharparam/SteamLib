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
using System.ComponentModel;
using System.Drawing;
using Sharparam.SteamLib.Events;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public sealed class Friend : IEquatable<Friend>, IEquatable<CSteamID>, IEquatable<LocalUser>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<MessageEventArgs> ChatMessageReceived;

        private readonly log4net.ILog _log;

        private readonly Steam _steam;
        private readonly List<Message> _chatHistory;

        public readonly CSteamID Id;
        public readonly ReadOnlyCollection<Message> ChatHistory;

        public EPersonaState State { get { return _steam.SteamFriends002.GetFriendPersonaState(Id); } }

        public string StateText { get { return Steam.StateToString(State); } }

        public bool Online { get { return State != EPersonaState.k_EPersonaStateOffline; } }

        public string Name { get { return _steam.SteamFriends002.GetFriendPersonaName(Id); } }

        public string Nickname
        {
            get
            {
                string nick;
                try
                {
                    nick = _steam.ClientFriends.GetPlayerNickname(Id);
                }
                catch (ArgumentNullException)
                {
                    nick = null;
                }
                return nick;
            }

            set
            {
                _steam.ClientFriends.SetPlayerNickname(Id, value);
                OnPropertyChanged("Nickname");
            }
        }

        public Bitmap SmallAvatar { get { return _steam.GetSmallAvatar(Id); } }
        public Bitmap MediumAvatar { get { return _steam.GetMediumAvatar(Id); } }
        public Bitmap LargeAvatar { get { return _steam.GetLargeAvatar(Id); } }

        internal Friend(Steam steam, CSteamID id, List<Message> oldChatHistory = null)
        {
            _log = LogManager.GetLogger(this);
            
            _steam = steam;
            Id = id;
            _chatHistory = oldChatHistory ?? new List<Message>();
            ChatHistory = new ReadOnlyCollection<Message>(_chatHistory);
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

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler func = PropertyChanged;
            if (func != null)
                func(this, new PropertyChangedEventArgs(propertyName));
        }

        public static explicit operator CSteamID(Friend friend)
        {
            return friend.Id;
        }

        private void OnChatMessageReceived(Message message)
        {
            var func = ChatMessageReceived;
            if (func != null)
                func(this, new MessageEventArgs(message));
        }

        internal void NotifyChanged()
        {
            NotifyNameChanged();
            NotifyStateChanged();
        }

        internal void NotifyNameChanged()
        {
            OnPropertyChanged("Name");
        }

        internal void NotifyStateChanged()
        {
            OnPropertyChanged("State");
            OnPropertyChanged("StateText");
            OnPropertyChanged("Online");
        }

        internal void AddChatMessage(Message message)
        {
            _chatHistory.Add(message);
            OnChatMessageReceived(message);
        }

        public void SendMessage(string message, EChatEntryType type = EChatEntryType.k_EChatEntryTypeChatMsg)
        {
            if (type == EChatEntryType.k_EChatEntryTypeEmote)
                _log.Warn("Steam no longer supports sending emotes to chat");
            _steam.SendMessage(Id, message, type);
        }
    }
}
