using System;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public sealed class LocalUser : IEquatable<CSteamID>, IEquatable<Friend>, IEquatable<LocalUser>
    {
        private readonly Steam _steam;

        public readonly CSteamID Id;

        public string Name { get { return _steam.Helper.GetMyName(); } }
        
        public EPersonaState State
        {
            get { return _steam.Helper.GetMyState(); }
            set { _steam.Helper.SetMyState(value); }
        }

        public string StateText { get { return _steam.Helper.GetMyStateText(); } }

        internal LocalUser(Steam steam)
        {
            _steam = steam;
            Id = _steam.SteamUser.GetSteamID();
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
            return (object) left != null && left.Equals(right);
        }

        public static bool operator !=(LocalUser left, object right)
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
