using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Steam4NET;

namespace Sharparam.SteamLib
{
    public class FriendsList : ReadOnlyObservableCollection<Friend>
    {
        private readonly Steam _steam;

        private readonly ObservableCollection<Friend> _list;

        private bool _updating;

        private FriendsList(Steam steam, ObservableCollection<Friend> list) : base(list)
        {
            _steam = steam;
            _list = list;

            Update();
        }

        internal FriendsList(Steam steam) : this(steam, new ObservableCollection<Friend>())
        {
            
        }

        public void Update(bool force = false)
        {
            if (_updating && !force)
                return;

            _updating = true;

            var old = new Friend[_list.Count];
            _list.CopyTo(old, 0);

            _list.Clear();

            var numFriends = _steam.SteamFriends002.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

            for (var i = 0; i < numFriends; i++)
            {
                var friendId = _steam.SteamFriends002.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                var oldFriend = old.FirstOrDefault(f => f.Id == friendId);
                if (oldFriend != null)
                    _list.Add(oldFriend);
                else
                {
                    var avatar = _steam.Helper.GetLargeFriendAvatar(friendId);
                    _list.Add(new Friend(_steam, friendId, avatar));
                }
            }
        }

        public bool Contains(CSteamID id)
        {
            return this.Any(f => f.Id == id);
        }

        #region Get Methods

        public Friend GetFriendById(CSteamID id)
        {
            return this.FirstOrDefault(f => f.Id == id);
        }

        public Friend GetFriendByName(string name, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public Friend GetFriendByPartialName(string search, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Name, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public IEnumerable<Friend> GetFriendsByName(string name, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Friend> GetFriendsByPartialName(string search, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Name, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public Friend GetFriendByNickname(string name, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public Friend GetFriendByPartialNickname(string search, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Nickname, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public IEnumerable<Friend> GetFriendsByNickname(string name, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<Friend> GetFriendsByPartialNickname(string search, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Nickname, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public IEnumerable<Friend> GetFriendsByState(EPersonaState state)
        {
            return this.Where(f => f.State == state);
        }

        public IEnumerable<Friend> GetFriendsByStates(IEnumerable<EPersonaState> states)
        {
            return this.Where(f => states.Contains(f.State));
        }

        #endregion Get Methods
    }
}
