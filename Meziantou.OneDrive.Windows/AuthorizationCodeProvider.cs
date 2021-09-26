using System;
using System.Threading;
using System.Windows.Forms;

namespace Meziantou.OneDrive.Windows
{
    public sealed class AuthorizationCodeProvider : IAuthorizationProvider
    {
        public string LoginUrl { get; set; } = "https://login.live.com/oauth20_authorize.srf";

        public string GetAuthorizationCode(OneDriveClient client)
        {
            var scope = client.Scopes != null ? string.Join(" ", client.Scopes) : string.Empty;
            string loginUrl = $"{LoginUrl}?client_id={Uri.EscapeDataString(client.ApplicationId)}&scope={Uri.EscapeDataString(scope)}&response_type=code&redirect_uri={Uri.EscapeDataString(client.ReturnUrl)}";

            AuthenticationForm authenticationForm = null;
            var t = new Thread(_ =>
            {
                authenticationForm = new AuthenticationForm(loginUrl);
                Application.EnableVisualStyles();
                Application.Run(authenticationForm);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
            return authenticationForm?.AuthorizationCode;
        }
    }
}
