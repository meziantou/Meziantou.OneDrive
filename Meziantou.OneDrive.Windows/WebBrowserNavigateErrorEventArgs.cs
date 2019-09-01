namespace Meziantou.OneDrive.Windows
{
    internal sealed class WebBrowserNavigateErrorEventArgs : CancelEventArgs
    {
        public WebBrowserNavigateErrorEventArgs(string url, string frame, int statusCode)
        {
            Url = url;
            Frame = frame;
            StatusCode = statusCode;
        }

        public string Url { get; }
        public string Frame { get; }
        public int StatusCode { get; }
    }
}
