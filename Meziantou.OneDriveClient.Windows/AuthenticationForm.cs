using System;
using System.Windows.Forms;

namespace Meziantou.OneDriveClient.Windows
{
    internal partial class AuthenticationForm : Form
    {
        public string AuthorizationCode { get; private set; }

        public AuthenticationForm(string loginUrl)
        {
            InitializeComponent();
            webBrowser.Navigate(loginUrl);
        }

        private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.ToString() != "about:blank")
            {
                pictureBoxLoading.Visible = false;
            }

            var query = e.Url.Query;
            if (query == null)
                return;

            query = query.Substring(1); // remove starting '?'
            var parts = query.Split('&');
            foreach (var part in parts)
            {
                const string codeToken = "code=";
                if (!part.StartsWith(codeToken, StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = Uri.UnescapeDataString(part.Substring(codeToken.Length));
                AuthorizationCode = value;
                Close();
            }
        }

        private void webBrowser_NavigateError(object sender, WebBrowserNavigateErrorEventArgs e)
        {
            e.Cancel = true;
            MessageBox.Show("Unable to access the network.", "Error");
            Close();
        }
    }
}
