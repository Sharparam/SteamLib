﻿/* SteamFriends.cs
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

using System.Drawing;
using System.Text;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class SteamHelper
    {
        private readonly log4net.ILog _log;

        private readonly Steam _steam;

        internal SteamHelper(Steam steam)
        {
            _steam = steam;
            _log = LogManager.GetLogger(this);	
        }

        public string GetMyName()
        {
            return _steam.SteamFriends002.GetPersonaName();
        }

        public EPersonaState GetMyState()
        {
            return _steam.SteamFriends002.GetPersonaState();
        }

        public string GetMyStateText()
        {
            return Utils.StateToString(GetMyState());
        }

        public void SetMyState(EPersonaState state)
        {
            if (GetMyState() == state)
                return;

            _steam.SteamFriends002.SetPersonaState(state);
        }

        public int GetFriendCount(EFriendFlags flags = EFriendFlags.k_EFriendFlagImmediate)
        {
            return _steam.SteamFriends002.GetFriendCount(flags);
        }

        public CSteamID GetFriendByIndex(int index, EFriendFlags flags = EFriendFlags.k_EFriendFlagImmediate)
        {
            return _steam.SteamFriends002.GetFriendByIndex(index, flags);
        }

        public string GetFriendName(CSteamID id)
        {
            return _steam.SteamFriends002.GetFriendPersonaName(id);
        }

        public string GetFriendNickname(CSteamID id)
        {
            return _steam.ClientFriends.GetPlayerNickname(id);
        }

        public EPersonaState GetFriendState(CSteamID id)
        {
            return _steam.SteamFriends002.GetFriendPersonaState(id);
        }

        public string GetFriendStateText(CSteamID id)
        {
            return Utils.StateToString(GetFriendState(id));
        }

        public Bitmap GetFriendAvatar(CSteamID id, EAvatarSize size)
        {
            int handle;
            switch (size)
            {
                case EAvatarSize.k_EAvatarSize32x32:
                    handle = _steam.SteamFriends013.GetSmallFriendAvatar(id);
                    break;
                case EAvatarSize.k_EAvatarSize64x64:
                    handle = _steam.SteamFriends013.GetMediumFriendAvatar(id);
                    break;
                case EAvatarSize.k_EAvatarSize184x184:
                    handle = _steam.SteamFriends013.GetLargeFriendAvatar(id);
                    break;
                default:
                    handle = _steam.SteamFriends013.GetLargeFriendAvatar(id);
                    break;
            }
            var avatar = Utils.GetAvatarFromHandle(handle, _steam.SteamUtils);
            return avatar;
        }

        public Bitmap GetSmallFriendAvatar(CSteamID id)
        {
            return GetFriendAvatar(id, EAvatarSize.k_EAvatarSize32x32);
        }

        public Bitmap GetMediumFriendAvatar(CSteamID id)
        {
            return GetFriendAvatar(id, EAvatarSize.k_EAvatarSize64x64);
        }

        public Bitmap GetLargeFriendAvatar(CSteamID id)
        {
            return GetFriendAvatar(id, EAvatarSize.k_EAvatarSize184x184);
        }

        public ChatMessage GetChatMessage(CSteamID sender, CSteamID receiver, int chatId)
        {
            var data = new byte[4096];
            var type = EChatEntryType.k_EChatEntryTypeChatMsg;
            var length = _steam.SteamFriends002.GetChatMessage(receiver, chatId, data, ref type);
            var msg = Encoding.UTF8.GetString(data, 0, length - 1);
            var message = new ChatMessage(sender, receiver, type, msg);
            return message;
        }

        public ChatMessage GetChatMessage(CSteamID sender, CSteamID receiver, uint chatId)
        {
            return GetChatMessage(sender, receiver, (int) chatId);
        }

        public ChatMessage GetChatMessage(FriendChatMsg_t callback)
        {
            var sender = new CSteamID(callback.m_ulSenderID);
            var receiver = new CSteamID(callback.m_ulFriendID);
            var id = callback.m_iChatID;
            return GetChatMessage(sender, receiver, id);
        }

        public void SendMessage(CSteamID receiver, EChatEntryType type, string message)
        {
            if (type == EChatEntryType.k_EChatEntryTypeEmote)
                _log.Warn("Steam no longer supports sending emotes to chat");
            _steam.SteamFriends002.SendMsgToFriend(receiver, type, Encoding.UTF8.GetBytes(message));
        }

        public void SendChatMessage(CSteamID receiver, string message)
        {
            SendMessage(receiver, EChatEntryType.k_EChatEntryTypeChatMsg, message);
        }
    }
}