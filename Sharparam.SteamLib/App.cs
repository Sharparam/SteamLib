/* App.cs
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

namespace Sharparam.SteamLib
{
    using System;
    using System.ComponentModel;
    using System.Text;

    using Sharparam.SteamLib.Logging;

    using Steam4NET;

    public sealed class App : IEquatable<App>, IEquatable<UInt32>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly log4net.ILog _log;

        private readonly Steam _steam;

        private readonly bool _isGame;

        public readonly UInt32 Id;

        private EAppState _state;

        public string Name { get { return _steam.GetAppData(Id, "name"); } }

        public EAppState State
        {
            get
            {
                return _state;
            }
            internal set
            {
                var oldInstalled = Installed;
                var oldPlayable = Playable;
                _state = value;
                NotifyStateChanged();
                if (oldInstalled != Installed)
                    NotifyInstalledChanged();
                if (oldPlayable != Playable)
                    NotifyPlayableChanged();
            }
        }

        public bool Installed { get { return _steam.SteamApps006.BIsAppInstalled(Id); } }

        public bool Playable
        {
            get
            {
                // This prevents this property returning true if an update is required or similar,
                // in which case BIsAppInstalled still returns true (because app is installed but requires
                // an update)
                return Installed && (State & EAppState.k_EAppStateFullyInstalled) != 0 && _steam.GetAppData(Id, "state") != "eStateUnavailable";
            }
        }

        public bool IsGame { get { return _isGame; } }

        internal App(Steam steam, UInt32 id, EAppState state = EAppState.k_EAppStateInvalid)
        {
            _log = LogManager.GetLogger(this);
            
            _steam = steam;
            Id = id;
            State = state;

            _isGame = true;

            if (_steam.GetAppData(Id, "state") == "eStateTool" ||
                !string.IsNullOrEmpty(_steam.GetAppData(Id, "DLCForAppID")) ||
                _steam.GetAppData(Id, "IsMediaFile") == "1")
                _isGame = false;

            _log.DebugFormat("New app created with id {0} ({1}) and state {2}, installed == {3}, playable == {4}", Id, Name, State, Installed, Playable);
        }

        public bool Equals(App other)
        {
            return Id == other.Id;
        }

        public bool Equals(uint other)
        {
            return Id == other;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static explicit operator uint(App app)
        {
            return app.Id;
        }

        public void Launch()
        {
            System.Diagnostics.Process.Start("steam://run/" + Id);
        }

        private void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }

        internal void NotifyChanged()
        {
            NotifyNameChanged();
            NotifyStateChanged();
            NotifyInstalledChanged();
            NotifyPlayableChanged();
        }

        internal void NotifyNameChanged()
        {
            OnPropertyChanged("Name");
        }

        internal void NotifyStateChanged()
        {
            OnPropertyChanged("State");
        }

        internal void NotifyInstalledChanged()
        {
            OnPropertyChanged("Installed");
            _log.DebugFormat("NotifyInstalledChanged, new value: {0}", Installed);
        }

        internal void NotifyPlayableChanged()
        {
            OnPropertyChanged("Playable");
            _log.DebugFormat("NotifyPlayableChanged, new value: {0}", Playable);
        }
    }
}
