namespace Sharparam.SteamLib.Utils
{
    internal class DownloadItem
    {
        public readonly string Url;

        public readonly string Target;

        public Downloader.DownloadFinishedCallback Callback;

        public DownloadItem(string url, string target, Downloader.DownloadFinishedCallback callback)
        {
            Url = url;
            Target = target;
            Callback = callback;
        }
    }
}
