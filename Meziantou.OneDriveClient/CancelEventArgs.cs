using System;

namespace Meziantou.OneDriveClient
{
    public class CancelEventArgs : EventArgs
    {
        public CancelEventArgs() : this(false)
        {
        }

        public CancelEventArgs(bool cancel)
        {
            this.Cancel = cancel;
        }

        public bool Cancel { get; set; }
    }
}