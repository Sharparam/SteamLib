namespace Sharparam.SteamLib.Utils
{
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;

    internal class Downloader
    {
        public delegate void DownloadFinishedCallback(DownloadItem item, bool success);

        private readonly Queue<DownloadItem> _queue;

        private readonly WebClient _client;

        private bool _working;

        public Downloader()
        {
            _queue = new Queue<DownloadItem>();
            _client = new WebClient();
        }

        public void Add(string url, string target, DownloadFinishedCallback callback)
        {
            Add(new DownloadItem(url, target, callback));
        }

        public void Add(DownloadItem item)
        {
            _queue.Enqueue(item);

            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (_working || _queue.Count == 0)
                return;

            ThreadPool.QueueUserWorkItem(Work, _queue.Dequeue());
        }

        private void Work(object state)
        {
            _working = true;

            var item = state as DownloadItem;
            if (item == null)
            {
                _working = false;
                ProcessQueue();
                return;
            }

            bool success;

            try
            {
                _client.DownloadFile(item.Url, item.Target);
                success = true;
            }
            catch (WebException)
            {
                success = false;
            }

            item.Callback(item, success);

            _working = false;

            ProcessQueue();
        }
    }
}
