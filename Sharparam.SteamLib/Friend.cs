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
    public class Friend
    {
        public event ChatMessageReceivedEventHandler ChatMessageReceived;

        private readonly log4net.ILog _log;

        private readonly SteamHelper _steamHelper;
        private readonly CSteamID _id;
        private readonly List<ChatMessage> _chatHistory;

        public ReadOnlyCollection<ChatMessage> ChatHistory;

        public CSteamID Id { get { return _id; } }
        public Bitmap Avatar { get; internal set; }

        public bool Online { get { return GetState() != EPersonaState.k_EPersonaStateOffline; } }

        internal Friend(SteamHelper steamHelper, CSteamID id, List<ChatMessage> oldChatHistory = null)
        {
            _log = LogManager.GetLogger(this);
            //_log.DebugFormat(">> Friend([clientFriends], {0}, {1})", id.Render(), oldChatHistory == null ? "null" : "[oldChatHistory]");
            _steamHelper = steamHelper;
            _id = id;
            _chatHistory = oldChatHistory ?? new List<ChatMessage>();
            ChatHistory = new ReadOnlyCollection<ChatMessage>(_chatHistory);
            //_log.Debug("<< Friend()");
        }

        private void OnChatMessageReceived(ChatMessage message)
        {
            _log.Debug(">> OnChatMessageReceived([message])");
            var func = ChatMessageReceived;
            if (func != null)
                func(this, new ChatMessageEventArgs(message));
            _log.Debug("<< OnChatMessageReceived()");
        }

        internal void AddChatMessage(ChatMessage message)
        {
            _log.Debug(">> AddChatMessage([message])");
            _chatHistory.Add(message);
            OnChatMessageReceived(message);
            _log.Debug("<< AddChatMessage()");
        }

        public string GetName()
        {
            //_log.Debug(">< GetName()");
            return _steamHelper.GetFriendName(_id);
        }

        public string GetNickname()
        {
            _log.Debug(">> GetNickName()");
            string nick;
            try
            {
                nick = _steamHelper.GetFriendNickname(_id);
            }
            catch (ArgumentNullException) // No nickname set for this friend
            {
                nick = null;
            }
            _log.Debug("<< GetNickName()");
            return nick;
        }

        public EPersonaState GetState()
        {
            //_log.Debug(">< GetState()");
            return _steamHelper.GetFriendState(_id);
        }

        public string GetStateText()
        {
            //_log.Debug(">< GetStateText()");
            return _steamHelper.GetFriendStateText(_id);
        }

        public void SendType(string message, EChatEntryType type)
        {
            _log.DebugFormat(">> SendType([message], {0})", type);
            if (type == EChatEntryType.k_EChatEntryTypeEmote)
                _log.Warn("Steam no longer supports sending emotes to chat");
            _steamHelper.SendMessage(_id, type, message);
            _log.Debug("<< SendType()");
        }

        public void SendMessage(string message)
        {
            _log.Debug(">> SendMessage([message])");
            SendType(message, EChatEntryType.k_EChatEntryTypeChatMsg);
            _log.Debug("<< SendMessage()");
        }

        [Obsolete("Steam no longer supports sending emotes to chat")]
        public void SendEmote(string message)
        {
            _log.Debug(">> SendEmote([message]) [OBSOLETE]");
            SendType(message, EChatEntryType.k_EChatEntryTypeEmote);
            _log.Debug("<< SendEmote()");
        }
    }
}
