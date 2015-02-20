using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeFluent.Runtime.Web.Utilities;
using Meziantou.OneDrive.Design;
using Meziantou.OneDrive.Diagnostics;

namespace Meziantou.OneDrive
{
    public partial class AuthenticationForm : Form
    {
        private readonly LiveSession _liveSession;

        private AuthenticationForm(LiveSession liveSession, string loginUrl)
        {
            _liveSession = liveSession;
            InitializeComponent();

            webBrowser.Navigate(loginUrl);
        }

        private async void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            Logger.Log(LogComponent.Authentication, LogType.Verbose, value: "Navigated to: " + e.Url);

            if (e.Url.ToString() != "about:blank")
            {
                pictureBoxLoading.Visible = false;
            }

            EditableUri editableUri = new EditableUri(e.Url);
            string code = editableUri.Parameters.GetValue("code", (string)null);
            if (code != null)
            {
                await _liveSession.AuthenticateAsync(code);
                if (_liveSession.IsValid)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        public static async Task<LiveSession> CreateSessionAsync(string clientId, IEnumerable<Scope> scopes, IRefreshTokenHandler refreshTokenHandler = null)
        {
            var liveAuthentication = new LiveSession(clientId, refreshTokenHandler);
            if (await liveAuthentication.RefreshTokenAsync())
            {
                return liveAuthentication;
            }

            string loginUrl = liveAuthentication.BuildLoginUrl(scopes);
            Logger.Log(LogComponent.Authentication, LogType.Information, value: string.Format("Login Url: {0}", loginUrl));
            var form = new AuthenticationForm(liveAuthentication, loginUrl);
            DialogResult dialogResult = form.ShowDialog();
            if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
            {
                if (liveAuthentication.IsValid)
                {
                    Logger.Log(LogComponent.Authentication, LogType.Information, value: "Authentication success");
                    return liveAuthentication;
                }
            }

            return null;
        }

        private void webBrowser_NavigateError(object sender, WebBrowserNavigateErrorEventArgs e)
        {
            Logger.Log(LogComponent.Authentication, LogType.Information, value: string.Format("Navigation Error ({0}): {1}", e.StatusCode, e.Url));
            e.Cancel = true;
            MessageBox.Show("Unable to access the network.", "Error");
            Close();
        }
    }
}
