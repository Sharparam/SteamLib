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
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;

    using Sharparam.SteamLib.Logging;

    using Steam4NET;

    public sealed class App : IEquatable<App>, IEquatable<UInt32>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const string ImageUrlFormat = "http://media.steampowered.com/steamcommunity/public/images/apps/{0}/{1}.jpg";

        private static readonly string CacheDirectory = Path.Combine(Environment.CurrentDirectory, "cache");

        private static readonly string CachePathFormat = Path.Combine(CacheDirectory, "{0}_{1}.jpg");

        private readonly log4net.ILog _log;

        private readonly Steam _steam;

        private readonly bool _isGame;

        public readonly UInt32 Id;

        private EAppState _state;

        private Bitmap _iconBitmap;

        private Bitmap _logoBitmap;

        private Bitmap _smallLogoBitmap;

        public string Name { get { return _steam.GetAppData(Id, "name"); } }

        public string Icon { get { return _steam.GetAppData(Id, "icon"); } }

        public string Logo { get { return _steam.GetAppData(Id, "logo"); } }

        public string SmallLogo { get { return _steam.GetAppData(Id, "logo_small"); } }

        public string IconUrl
        {
            get
            {
                return string.Format(ImageUrlFormat, Id, Icon);
            }
        }

        public string LogoUrl
        {
            get
            {
                return string.Format(ImageUrlFormat, Id, Logo);
            }
        }

        public string SmallLogoUrl
        {
            get
            {
                return string.Format(ImageUrlFormat, Id, SmallLogo);
            }
        }

        public Bitmap IconBitmap
        {
            private set
            {
                _iconBitmap = value;
                NotifyIconBitmapChanged();
            }
            get
            {
                if (_iconBitmap != null)
                    return _iconBitmap;

                var file = string.Format(CachePathFormat, Id, Icon);
                if (File.Exists(file))
                {
                    _iconBitmap = (Bitmap)Image.FromFile(file);
                    return _iconBitmap;
                }
                
                ThreadPool.QueueUserWorkItem(o => DownloadIcon());
                return null;
            }
        }

        public Bitmap LogoBitmap
        {
            private set
            {
                _logoBitmap = value;
                NotifyLogoBitmapChanged();
            }
            get
            {
                if (_logoBitmap != null)
                    return _logoBitmap;

                var file = string.Format(CachePathFormat, Id, Logo);
                if (File.Exists(file))
                {
                    _logoBitmap = (Bitmap)Image.FromFile(file);
                    return _logoBitmap;
                }

                ThreadPool.QueueUserWorkItem(o => DownloadLogo());
                return null;
            }
        }

        public Bitmap SmallLogoBitmap
        {
            private set
            {
                _smallLogoBitmap = value;
                NotifySmallLogoBitmapChanged();
            }
            get
            {
                if (_smallLogoBitmap != null)
                    return _smallLogoBitmap;

                var file = string.Format(CachePathFormat, Id, SmallLogo);
                if (File.Exists(file))
                {
                    _smallLogoBitmap = (Bitmap)Image.FromFile(file);
                    return _smallLogoBitmap;
                }

                ThreadPool.QueueUserWorkItem(o => DownloadSmallLogo());
                return null;
            }
        }

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

        private void CheckDownloadCache()
        {
            if (!Directory.Exists(CacheDirectory))
                Directory.CreateDirectory(CacheDirectory);
        }

        private void DownloadIcon()
        {
            _log.DebugFormat("Downloading icon for app {0}", Id);

            CheckDownloadCache();

            var file = string.Format(CachePathFormat, Id, Icon);
            var url = string.Format(ImageUrlFormat, Id, Icon);

            var client = new WebClient();
            try
            {
                client.DownloadFile(url, file);
                IconBitmap = (Bitmap)Image.FromFile(file);
            }
            catch (WebException)
            {
                IconBitmap = null;
            }
        }

        private void DownloadLogo()
        {
            
        }

        private void DownloadSmallLogo()
        {
            
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
            NotifyIconBitmapChanged();
            NotifyLogoBitmapChanged();
            NotifySmallLogoBitmapChanged();
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

        internal void NotifyIconBitmapChanged()
        {
            OnPropertyChanged("IconBitmap");
        }

        internal void NotifyLogoBitmapChanged()
        {
            OnPropertyChanged("LogoBitmap");
        }

        internal void NotifySmallLogoBitmapChanged()
        {
            OnPropertyChanged("SmallLogoBitmap");
        }
    }
}
