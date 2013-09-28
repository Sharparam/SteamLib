/* LocalUser.cs
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
using Steam4NET;

namespace Sharparam.SteamLib
{
    public sealed class LocalUser : IEquatable<CSteamID>, IEquatable<Friend>, IEquatable<LocalUser>
    {
        public event EventHandler StateChanged;

        private readonly Steam _steam;

        public readonly CSteamID Id;

        public string Name
        {
            get { return _steam.SteamFriends002.GetPersonaName(); }
            set { _steam.SteamFriends002.SetPersonaName(value); }
        }

        public EPersonaState State
        {
            get { return _steam.SteamFriends002.GetPersonaState(); }
            set { _steam.SteamFriends002.SetPersonaState(value); }
        }

        public string StateText { get { return Steam.StateToString(State); } }

        internal LocalUser(Steam steam)
        {
            _steam = steam;
            Id = _steam.SteamUser.GetSteamID();
        }

        private void OnStateChanged()
        {
            var func = StateChanged;
            if (func != null)
                func(this, null);
        }

        internal void NotifyStateChanged()
        {
            OnStateChanged();
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
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

            var friend = obj as Friend;

            if ((object) friend != null && Equals(friend))
                return true;

            return obj.GetType() == GetType() && Equals((LocalUser)obj);
        }

        public bool Equals(LocalUser other)
        {
            return (object) other != null && Id == other.Id;
        }

        public bool Equals(CSteamID other)
        {
            return other != null && Id == other;
        }

        public bool Equals(Friend other)
        {
            return (object) other != null && Id == other.Id;
        }

        public static bool operator ==(LocalUser left, object right)
        {
            return ((object) left != null && left.Equals(right)) || ((object) left == null && right == null);
        }

        public static bool operator !=(LocalUser left, object right)
        {
            return !(left == right);
        }

        public static bool operator ==(CSteamID left, LocalUser right)
        {
            return (left != null && (object) right != null && right.Equals(left)) || (left == null && (object) right == null);
        }

        public static bool operator !=(CSteamID left, LocalUser right)
        {
            return !(left == right);
        }

        #endregion Equality Members

        public static explicit operator CSteamID(LocalUser user)
        {
            return user.Id;
        }
    }
}
