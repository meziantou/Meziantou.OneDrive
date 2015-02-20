using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CodeFluent.Runtime.Utilities;
using CodeFluent.Runtime.Web.Utilities;
using Meziantou.OneDrive.Diagnostics;

namespace Meziantou.OneDrive
{
    public class LiveSession
    {
        // https://msdn.microsoft.com/fr-fr/library/dn631817.aspx

        private readonly string _clientId;
        private readonly IRefreshTokenHandler _refreshTokenHandler;
        public string TokenType { get; private set; }
        public DateTimeOffset ExpirationDate { get; private set; }
        public string Scope { get; private set; }
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public string UserId { get; private set; }

        public string ClientId
        {
            get { return _clientId; }
        }

        public bool IsValid
        {
            get { return DateTimeOffset.UtcNow < ExpirationDate; }
        }

        public LiveSession(string clientId, IRefreshTokenHandler refreshTokenHandler)
        {
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));
            _clientId = clientId;
            _refreshTokenHandler = refreshTokenHandler;
        }

        public string BuildLoginUrl(IEnumerable<Scope> scopes)
        {
            EditableUri editableUri = new EditableUri("https://login.live.com/oauth20_authorize.srf");
            editableUri.Parameters["client_id"] = ClientId;
            editableUri.Parameters["scope"] = OneDriveUtilities.GetScopeName(scopes);
            editableUri.Parameters["response_type"] = "code";
            editableUri.Parameters["redirect_uri"] = "https://login.live.com/oauth20_desktop.srf";

            return editableUri.ToString();
        }

        private async Task<bool> GetTokenAsync(string requestContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage message = new HttpRequestMessage())
                {
                    message.Method = new HttpMethod("POST");
                    message.RequestUri = new Uri("https://login.live.com/oauth20_token.srf");
                    message.Content = new StringContent(requestContent);
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    using (var response = await client.SendAsync(message, cancellationToken).ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Logger.Log(LogComponent.Core, LogType.Verbose, value: response.StatusCode + " " + responseContent);

                        await ReadResponseAsync(responseContent).ConfigureAwait(false);
                    }
                }
            }

            return IsValid;
        }

        private async Task ReadResponseAsync(string responseContent)
        {
            var dict = JsonUtilities.Deserialize<Dictionary<string, object>>(responseContent);
            TokenType = dict.GetValue("token_type", (string)null);
            Scope = dict.GetValue("scope", (string)null);
            AccessToken = dict.GetValue("access_token", (string)null);
            RefreshToken = dict.GetValue("refresh_token", (string)null);
            UserId = dict.GetValue("user_id", (string)null);
            long expiresIn = dict.GetValue("expires_in", 0);
            ExpirationDate = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

            if (_refreshTokenHandler != null)
            {
                await _refreshTokenHandler.SaveRefreshTokenAsync(new RefreshTokenInfo(UserId, RefreshToken)).ConfigureAwait(false);
            }
        }

        public Task<bool> AuthenticateAsync(string code, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (code == null) throw new ArgumentNullException(nameof(code));

            string content = string.Format("client_id={0}&redirect_uri=https://login.live.com/oauth20_desktop.srf&code={1}&grant_type=authorization_code", _clientId, code);
            return GetTokenAsync(content, cancellationToken);
        }

        public async Task<bool> RefreshTokenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string refreshToken = null;
            if (_refreshTokenHandler != null)
            {
                var refreshTokenInfo = await _refreshTokenHandler.RetrieveRefreshTokenAsync();
                if (refreshTokenInfo != null)
                {
                    refreshToken = refreshTokenInfo.RefreshToken;
                }
            }

            if (refreshToken == null)
            {
                refreshToken = RefreshToken;
            }

            if (refreshToken == null)
                return IsValid;

            string content = string.Format("client_id={0}&redirect_uri=https://login.live.com/oauth20_desktop.srf&code={1}&grant_type=refresh_token&refresh_token={1}", _clientId, refreshToken);
            return await GetTokenAsync(content, cancellationToken).ConfigureAwait(false);
        }

    }
}