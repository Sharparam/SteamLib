/* ChatMessage.cs
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

using Sharparam.SteamLib.Events;
using Sharparam.SteamLib.Logging;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class Client
    {
        // Events
        public event ChatMessageReceivedEventHandler ChatMessageReceived;

        private readonly log4net.ILog _log;

        private readonly ISteamUtils005 _steamUtils;

        private readonly SteamHelper _steamHelper;

        private readonly FriendsManager _friendsManager;

        private int _pipe;
        private int _user;

        // Callbacks
        private Callback<FriendChatMsg_t> _friendChatCallback;
        private Callback<FriendAdded_t> _friendAddedCallback;
        private Callback<PersonaStateChange_t> _personaStateChangeCallback;
        private Callback<FriendProfileInfoResponse_t> _friendProfileInfoResponseCallback;

        public Client()
        {
            _log = LogManager.GetLogger(this);

            _log.Debug("Creating friends manager");
            _friendsManager = new FriendsManager(_steamHelper, _steamUtils, this);
            
            // Set up callbacks
            _log.Debug("Setting up callbacks");
            _friendChatCallback = new Callback<FriendChatMsg_t>(HandleFriendChatMessage);
            _friendAddedCallback = new Callback<FriendAdded_t>(HandleFriendAdded);
            _personaStateChangeCallback = new Callback<PersonaStateChange_t>(HandlePersonaStateChange);
            _friendProfileInfoResponseCallback = new Callback<FriendProfileInfoResponse_t>(HandleFriendProfileInfoResponse);
            
            _log.Debug("CallbackDispatcher is spawning dispatch thread");
            CallbackDispatcher.SpawnDispatchThread(_pipe);

            _log.Info("Steam client initialization complete!");
            _log.Debug("<< Client())");
        }

        private void OnChatMessageReceived(ChatMessage message)
        {
            _log.Debug(">> OnChatMessageReceived([message])");
            var func = ChatMessageReceived;
            if (func == null)
                return;
            func(this, new ChatMessageEventArgs(message));
            _log.Debug("<< OnChatMessageReceived()");
        }

        private void HandleFriendChatMessage(FriendChatMsg_t callback)
        {
            _log.Debug(">> HandleFriendChatMessage([callback])");
            
            var message = _steamHelper.GetChatMessage(callback); // Construct the message class
            if (message.Type != EChatEntryType.k_EChatEntryTypeChatMsg && message.Type != EChatEntryType.k_EChatEntryTypeEmote)
            {
                _log.Debug("Not a chat or emote message, aborting.");
                _log.Debug("<< HandleFriendChatMessage()");
                return;
            }

            _log.Debug("Handling friend chat message");
            OnChatMessageReceived(message); // Throw the message event
            _log.Debug("<< HandleFriendChatMessage()");
        }

        private void HandleFriendAdded(FriendAdded_t callback)
        {
            _log.Debug(">> HandleFriendAdded([callback])");
            _friendsManager.UpdateFriends(); // A friend was added, so we need to update the local friends list
            _log.Debug("<< HandleFriendAdded()");
        }

        private void HandlePersonaStateChange(PersonaStateChange_t callback)
        {
            _log.Debug(">> HandlePersonaStateChange([callback])");
            // TODO: Just refresh the entire friends list for now, make it more efficient later
            var changeType = callback.m_nChangeFlags;
            if (changeType == EPersonaChange.k_EPersonaChangeComeOnline ||
                changeType == EPersonaChange.k_EPersonaChangeGoneOffline ||
                changeType == EPersonaChange.k_EPersonaChangeName ||
                changeType == EPersonaChange.k_EPersonaChangeNameFirstSet ||
                changeType == EPersonaChange.k_EPersonaChangeRelationshipChanged ||
                changeType == EPersonaChange.k_EPersonaChangeStatus)
            {
                _friendsManager.UpdateFriends();
            }
            _log.Debug("<< HandlePersonaStateChange()");
        }

        private void HandleFriendProfileInfoResponse(FriendProfileInfoResponse_t callback)
        {
            _log.Debug(">> HandleFriendProfileInfoResponse([callback])");
            // TODO: Just refresh the entire friends list for now, make it more efficient later
            _friendsManager.UpdateFriends();
            _log.Debug("<< HandleFriendProfileInfoResponse()");
        }
    }
}
