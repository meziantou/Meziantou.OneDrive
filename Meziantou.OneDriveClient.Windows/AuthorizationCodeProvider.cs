using System;
using System.Windows.Forms;

namespace Meziantou.OneDriveClient.Windows
{
    public class AuthorizationCodeProvider : IAuthorizationProvider
    {
        public string LoginUrl { get; set; } = "https://login.live.com/oauth20_authorize.srf";

        public string GetAuthorizationCode(OneDriveClient client)
        {
            var scope = client.Scopes != null ? string.Join(" ", client.Scopes) : string.Empty;
            string loginUrl = $"{LoginUrl}?client_id={Uri.EscapeDataString(client.ApplicationId)}&scope={Uri.EscapeDataString(scope)}&response_type=code&redirect_uri={Uri.EscapeDataString(client.ReturnUrl)}";

            var authenticationForm = new AuthenticationForm(loginUrl);
            Application.EnableVisualStyles();
            Application.Run(authenticationForm);
            return authenticationForm.AuthorizationCode;
        }
    }
}
