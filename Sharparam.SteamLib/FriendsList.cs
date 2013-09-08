using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
                
            }
        }
    }
}
