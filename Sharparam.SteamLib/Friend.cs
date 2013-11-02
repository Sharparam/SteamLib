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
using System.Linq;
using Sharparam.SteamLib.Events;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public sealed class Friend : IEquatable<Friend>, IEquatable<CSteamID>, IEquatable<LocalUser>, INotifyPropertyChanged
    {
        private const int MessageHistoryLimit = 256;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<MessageEventArgs> Message;
        public event EventHandler<MessageEventArgs> ChatMessage;
        public event EventHandler<MessageEventArgs> TypingMessage;

        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> ChatMessageReceived;
        public event EventHandler<MessageEventArgs> TypingMessageReceived;

        public event EventHandler<MessageEventArgs> MessageSent;
        public event EventHandler<MessageEventArgs> ChatMessageSent;
        public event EventHandler<MessageEventArgs> TypingMessageSent;

        private readonly log4net.ILog _log;

        private readonly Steam _steam;
        private readonly ObservableCollection<Message> _messageHistory;

        public readonly CSteamID Id;
        public readonly ReadOnlyObservableCollection<Message> MessageHistory;

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
                catch (ArgumentNullException) // Nick has not been set
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

        public IEnumerable<Message> ChatMessageHistory
        {
            get { return MessageHistory.Where(m => m.Type == EChatEntryType.k_EChatEntryTypeChatMsg); }
        }

        public bool InGame
        {
            get
            {
                try
                {
                    ulong gameId = 0;
                    uint gameIp = 0;
                    ushort gamePort = 0;
                    ushort queryPort = 0;
                    return
                        _steam.SteamFriends002.GetFriendGamePlayed(Id, ref gameId, ref gameIp, ref gamePort,
                                                                   ref queryPort) &&
                        gameId != 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        internal Friend(Steam steam, CSteamID id, ObservableCollection<Message> oldChatHistory = null)
        {
            _log = LogManager.GetLogger(this);
            
            _steam = steam;
            Id = id;
            _messageHistory = oldChatHistory ?? new ObservableCollection<Message>();
            MessageHistory = new ReadOnlyObservableCollection<Message>(_messageHistory);
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
            return ((object) left != null && left.Equals(right)) || ((object) left == null && right == null);
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

        #region Event Dispatchers

        private void OnPropertyChanged(string propertyName)
        {
            var func = PropertyChanged;
            if (func != null)
                func(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OnMessage(MessageEventArgs args)
        {
            var func = Message;
            if (func != null)
                func(this, args);
        }

        internal void OnChatMessage(MessageEventArgs args)
        {
            var func = ChatMessage;
            if (func != null)
                func(this, args);
        }
        
        internal void OnTypingMessage(MessageEventArgs args)
        {
            var func = TypingMessage;
            if (func != null)
                func(this, args);
        }

        internal void OnMessageReceived(MessageEventArgs args)
        {
            var func = MessageReceived;
            if (func != null)
                func(this, args);
        }

        internal void OnChatMessageReceived(MessageEventArgs args)
        {
            var func = ChatMessageReceived;
            if (func != null)
                func(this, args);
        }

        internal void OnTypingMessageReceived(MessageEventArgs args)
        {
            var func = TypingMessageReceived;
            if (func != null)
                func(this, args);
        }

        internal void OnMessageSent(MessageEventArgs args)
        {
            var func = MessageSent;
            if (func != null)
                func(this, args);
        }

        internal void OnChatMessageSent(MessageEventArgs args)
        {
            var func = ChatMessageSent;
            if (func != null)
                func(this, args);
        }

        internal void OnTypingMessageSent(MessageEventArgs args)
        {
            var func = TypingMessageSent;
            if (func != null)
                func(this, args);
        }

        #endregion Event Dispatchers

        #region Notify Methods

        internal void NotifyChanged()
        {
            NotifyNameChanged();
            NotifyStateChanged();
            NotifyAvatarChanged();
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

        internal void NotifyAvatarChanged()
        {
            OnPropertyChanged("SmallAvatar");
            OnPropertyChanged("MediumAvatar");
            OnPropertyChanged("LargeAvatar");
        }

        #endregion Notify Methods

        internal void AddMessage(Message message)
        {
            if (_messageHistory.Count >= MessageHistoryLimit)
                _messageHistory.RemoveAt(0);

            _messageHistory.Add(message);
        }

        public void SendMessage(string message, EChatEntryType type = EChatEntryType.k_EChatEntryTypeChatMsg)
        {
            if (type == EChatEntryType.k_EChatEntryTypeEmote)
                _log.Warn("Steam no longer supports sending emotes to chat");
            _steam.SendMessage(Id, message, type);
        }
    }
}
