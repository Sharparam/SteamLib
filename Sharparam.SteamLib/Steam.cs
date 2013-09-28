/* Steam.cs
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
using System.Drawing;
using System.Text;
using Sharparam.SteamLib.Events;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class Steam : IDisposable
    {
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

        private readonly int _pipe;
        private readonly int _user;

        internal readonly ISteamClient012 SteamClient;
        internal readonly ISteamUtils005 SteamUtils;
        internal readonly ISteamUser016 SteamUser;
        internal readonly ISteamFriends002 SteamFriends002;
        internal readonly ISteamFriends013 SteamFriends013;
        internal readonly IClientEngine ClientEngine;
        internal readonly IClientFriends ClientFriends;

        private static readonly Dictionary<EPersonaState, string> StateMapping = new Dictionary<EPersonaState, string>
        {
            {EPersonaState.k_EPersonaStateAway, "Away"},
            {EPersonaState.k_EPersonaStateBusy, "Busy"},
            {EPersonaState.k_EPersonaStateLookingToPlay, "Looking To Play"},
            {EPersonaState.k_EPersonaStateLookingToTrade, "Looking To Trade"},
            {EPersonaState.k_EPersonaStateOffline, "Offline"},
            {EPersonaState.k_EPersonaStateOnline, "Online"},
            {EPersonaState.k_EPersonaStateSnooze, "Snooze"}
        };

        public bool IsDisposed { get; private set; }

        public readonly LocalUser LocalUser;
        public readonly Friends Friends;
 
        private Callback<PersonaStateChange_t> _personaStateChange;
        private Callback<FriendProfileInfoResponse_t> _friendProfileInfoResponse;
        private Callback<FriendAdded_t> _friendAdded; 
        private Callback<FriendChatMsg_t> _friendChatMessage; 

        #region Constructor

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

            // Set up callbacks
            _personaStateChange = new Callback<PersonaStateChange_t>(HandlePersonaStateChange);
            _friendProfileInfoResponse = new Callback<FriendProfileInfoResponse_t>(HandleFriendProfileInfoResponse);
            _friendAdded = new Callback<FriendAdded_t>(HandleFriendAdded);
            _friendChatMessage = new Callback<FriendChatMsg_t>(HandleFriendChatMessage);

            LocalUser = new LocalUser(this);
            Friends = new Friends(this);

            // Spawn dispatch thread
            CallbackDispatcher.SpawnDispatchThread(_pipe);
        }

        #endregion Constructor

        #region Finalizer / Dispose methods

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

            CallbackDispatcher.StopDispatchThread(_pipe);

            IsDisposed = true;
        }

        #endregion Finalizer / Dispose methods

        #region Event Dispatchers

        private void OnMessage(Message message)
        {
            OnMessage(new MessageEventArgs(message));
        }

        private void OnMessage(MessageEventArgs args)
        {
            var func = Message;
            if (func != null)
                func(this, args);
        }

        private void OnChatMessage(Message message)
        {
            OnChatMessage(new MessageEventArgs(message));
        }

        private void OnChatMessage(MessageEventArgs args)
        {
            var func = ChatMessage;
            if (func != null)
                func(this, args);
        }

        private void OnTypingMessage(Message message)
        {
            OnTypingMessage(new MessageEventArgs(message));
        }

        private void OnTypingMessage(MessageEventArgs args)
        {
            var func = TypingMessage;
            if (func != null)
                func(this, args);
        }

        #endregion Event Dispatchers

        #region Steam Event Handlers

        private void HandlePersonaStateChange(PersonaStateChange_t param)
        {
            var id = new CSteamID(param.m_ulSteamID);
            if (id == LocalUser)
                LocalUser.NotifyStateChanged();
            else
                Friends.NotifyStateChanged(new CSteamID(param.m_ulSteamID));
        }

        private void HandleFriendProfileInfoResponse(FriendProfileInfoResponse_t param)
        {
            Friends.NotifyChanged(new CSteamID(param.m_steamIDFriend));
        }

        private void HandleFriendAdded(FriendAdded_t param)
        {
            Friends.Add(new CSteamID(param.m_ulSteamID));
        }

        private void HandleFriendChatMessage(FriendChatMsg_t param)
        {
            var message = new Message(this, param);
            var type = message.Type;
            var sent = message.Sender == LocalUser;

            var msgFunc = sent ? MessageSent : MessageReceived;

            var args = new MessageEventArgs(message);

            OnMessage(args);
            if (msgFunc != null)
                msgFunc(this, args);
            Friends.HandleMessageEvent(sent ? message.Receiver : message.Sender, args);
            if (sent)
                Friends.HandleMessageSentEvent(args);
            else
                Friends.HandleMessageReceivedEvent(args);

            switch (type)
            {
                case EChatEntryType.k_EChatEntryTypeChatMsg:
                    OnChatMessage(args);
                    var chatFunc = sent ? ChatMessageSent : ChatMessageReceived;
                    if (chatFunc != null)
                        chatFunc(this, args);
                    Friends.HandleChatMessageEvent(sent ? message.Receiver : message.Sender, args);
                    if (sent)
                        Friends.HandleChatMessageSentEvent(args);
                    else
                        Friends.HandleChatMessageReceivedEvent(args);
                    break;
                case EChatEntryType.k_EChatEntryTypeTyping:
                    OnTypingMessage(args);
                    var typingFunc = sent ? TypingMessageSent : TypingMessageReceived;
                    if (typingFunc != null)
                        typingFunc(this, args);
                    Friends.HandleTypingMessageEvent(sent ? message.Receiver : message.Sender, args);
                    if (sent)
                        Friends.HandleTypingMessageSentEvent(args);
                    else
                        Friends.HandleTypingMessageReceivedEvent(args);
                    break;
            }
        }

        #endregion Steam Event Handlers

        #region Steam Methods

        public static string StateToString(EPersonaState state)
        {
            return StateMapping.ContainsKey(state) ? StateMapping[state] : "Unknown";
        }

        public void SendMessage(CSteamID receiver, string message, EChatEntryType type = EChatEntryType.k_EChatEntryTypeChatMsg)
        {
            if (type == EChatEntryType.k_EChatEntryTypeEmote)
                _log.Warn("Steam no longer supports sending emotes to chat");
            SteamFriends002.SendMsgToFriend(receiver, type, Encoding.UTF8.GetBytes(message));
        }

        #endregion Steam Methods

        #region Avatar Methods

        // All credit to kimoto on GitHub
        // https://gist.github.com/986866/b2dc849940d4bdba858f3b305bf68afda641e21e

        private Bitmap GetAvatarFromHandle(int handle)
        {
            uint width = 0;
            uint height = 0;
            if (!SteamUtils.GetImageSize(handle, ref width, ref height))
                return null;

            var size = 4 * width * height;
            var buffer = new byte[size];
            if (!SteamUtils.GetImageRGBA(handle, buffer, (int)size))
                return null;

            var bitmap = new Bitmap((int)width, (int)height);
            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    var index = (y * (int)width + x) * 4;
                    int r = buffer[index + 0];
                    int g = buffer[index + 1];
                    int b = buffer[index + 2];
                    int a = buffer[index + 3];
                    var color = Color.FromArgb(a, r, g, b);
                    bitmap.SetPixel(x, y, color);
                }

            return bitmap;
        }

        private Bitmap GetAvatar(CSteamID id, EAvatarSize size)
        {
            int handle;
            switch (size)
            {
                case EAvatarSize.k_EAvatarSize32x32:
                    handle = SteamFriends013.GetSmallFriendAvatar(id);
                    break;
                case EAvatarSize.k_EAvatarSize64x64:
                    handle = SteamFriends013.GetMediumFriendAvatar(id);
                    break;
                case EAvatarSize.k_EAvatarSize184x184:
                    handle = SteamFriends013.GetLargeFriendAvatar(id);
                    break;
                default:
                    handle = SteamFriends013.GetLargeFriendAvatar(id);
                    break;
            }
            var avatar = GetAvatarFromHandle(handle);
            return avatar;
        }

        public Bitmap GetSmallAvatar(CSteamID id)
        {
            return GetAvatar(id, EAvatarSize.k_EAvatarSize32x32);
        }

        public Bitmap GetMediumAvatar(CSteamID id)
        {
            return GetAvatar(id, EAvatarSize.k_EAvatarSize64x64);
        }

        public Bitmap GetLargeAvatar(CSteamID id)
        {
            return GetAvatar(id, EAvatarSize.k_EAvatarSize184x184);
        }

        #endregion Avatar Methods
    }
}
