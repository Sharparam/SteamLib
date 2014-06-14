/* Apps.cs
 *
 * Copyright © 2014 by Adam Hellberg.
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
using System.Globalization;
using System.Linq;
using Sharparam.SteamLib.Events;
using Steam4NET;

namespace Sharparam.SteamLib
{
    using System.Diagnostics.Eventing.Reader;
    using System.Runtime.InteropServices;

    using Sharparam.SteamLib.Logging;

    using VDF;

    public class Apps : ReadOnlyObservableCollection<App>
    {
        private readonly log4net.ILog _log;
        private readonly Steam _steam;

        private readonly ObservableCollection<App> _list;

        private bool _updating;

        private Apps(Steam steam, ObservableCollection<App> list)
            : base(list)
        {
            _log = LogManager.GetLogger(this);
            _steam = steam;
            _list = list;

            Update();
        }

        internal Apps(Steam steam)
            : this(steam, new ObservableCollection<App>())
        {

        }

        internal void Add(UInt32 id, EAppState state = EAppState.k_EAppStateInvalid)
        {
            if (GetAppById(id) != null)
                return;

            var app = new App(_steam, id, state);
            _list.Add(app);
        }

        internal void Add(App app)
        {
            if (GetAppById(app.Id) != null)
                return;

            _list.Add(app);
        }

        public void Update(bool force = false)
        {
            if (_updating && !force)
                return;

            _updating = true;

            var old = new App[_list.Count];
            _list.CopyTo(old, 0);

            _list.Clear();

            var steamConfig = new SteamConfigFile(_steam.SteamConfigPath);

            if (!steamConfig.SteamElement.Children.ContainsKey("apps"))
            {
                _log.Info("No apps in the Steam config, aborting update");
                _updating = false;
                return;
            }

            var apps = steamConfig.SteamElement.Children["apps"];

            foreach (var app in apps.Children)
            {
                uint id;
                var valid = uint.TryParse(app.Key, out id);
                if (!valid)
                    continue;
                var oldApp = old.FirstOrDefault(a => a.Id == id);
                if (oldApp != null)
                {
                    Add(oldApp);
                    continue;
                }
                var data = app.Value;
                var hasAllLocalContent = data.Children.ContainsKey("HasAllLocalContent") && data.Children["HasAllLocalContent"].Value == "1";
                var upToDate = data.Children.ContainsKey("UpToDate") && data.Children["UpToDate"].Value == "1";
                var addGameExplorerEntry = data.Children.ContainsKey("AddGameExplorerEntry")
                                           && data.Children["AddGameExplorerEntry"].Value == "1";
                var state = ((hasAllLocalContent && upToDate) || addGameExplorerEntry)
                                ? EAppState.k_EAppStateFullyInstalled
                                : EAppState.k_EAppStateUninstalled;
                Add(id, state);
            }

            _updating = false;
        }

        public bool Contains(CSteamID id)
        {
            return this.Any(f => f.Id == id);
        }

        #region Get Methods

        public App GetAppById(uint id)
        {
            return this.FirstOrDefault(f => f.Id == id);
        }

        public App GetAppByName(string name, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public App GetAppByPartialName(string search, bool caseSensitive = true)
        {
            return
                this.FirstOrDefault(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Name, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        public IEnumerable<App> GetAppsByName(string name, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    f.Name.Equals(name,
                                  caseSensitive
                                      ? StringComparison.InvariantCulture
                                      : StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<App> GetAppsByPartialName(string search, bool caseSensitive = true)
        {
            return
                this.Where(
                    f =>
                    CultureInfo.InvariantCulture.CompareInfo.IndexOf(f.Name, search,
                                                                     caseSensitive
                                                                         ? CompareOptions.None
                                                                         : CompareOptions.IgnoreCase) >= 0);
        }

        #endregion Get Methods

        #region Notify methods

        internal void NotifyChanged(uint id)
        {
            var app = GetAppById(id);
            if (app != null)
                app.NotifyChanged();
        }

        internal void NotifyNameChanged(uint id)
        {
            var app = GetAppById(id);
            if (app != null)
                app.NotifyNameChanged();
        }

        #endregion Notify methods

        #region Internal Handler Methods

        internal void HandleAppEventStateChangeEvent(uint id, uint state)
        {
            var app = GetAppById(id);
            if (app == null)
                Add(id, (EAppState)state);
            else
                app.State = (EAppState)state;
        }
        
        #endregion Internal Handler Methods
    }
}
