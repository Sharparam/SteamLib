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

using System.Text;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public struct Message
    {
        public readonly CSteamID Sender;
        public readonly CSteamID Receiver;
        public readonly EChatEntryType Type;
        public readonly string Content;

        internal Message(CSteamID sender, CSteamID receiver, EChatEntryType type, string content)
        {
            Sender = sender;
            Receiver = receiver;
            Type = type;
            Content = content;
        }

        public Message(Steam steam, CSteamID sender, CSteamID receiver, EChatEntryType type, int chatId)
        {
            var data = new byte[4096];
            var length = steam.SteamFriends002.GetChatMessage(receiver, chatId, data, ref type);
            var content = Encoding.UTF8.GetString(data, 0, length).Replace("\0", "");

            Sender = sender;
            Receiver = receiver;
            Type = type;
            Content = content;
        }

        public Message(Steam steam, CSteamID sender, CSteamID receiver, EChatEntryType type, uint chatId)
            : this(steam, sender, receiver, type, (int) chatId)
        {
            
        }

        public Message(Steam steam, FriendChatMsg_t msg)
            : this(steam, new CSteamID(msg.m_ulSenderID), new CSteamID(msg.m_ulFriendID),
                   (EChatEntryType) msg.m_eChatEntryType, msg.m_iChatID)
        {
            
        }
    }
}
