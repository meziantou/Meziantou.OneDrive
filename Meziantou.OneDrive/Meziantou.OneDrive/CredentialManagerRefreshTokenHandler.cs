using System.Threading.Tasks;
using Meziantou.OneDrive.Utilities;

namespace Meziantou.OneDrive
{
    public class CredentialManagerRefreshTokenHandler : IRefreshTokenHandler
    {
        private readonly string _applicationName;

        public CredentialManagerRefreshTokenHandler(string applicationName)
        {
            _applicationName = applicationName;
        }

        public Task SaveRefreshTokenAsync(RefreshTokenInfo token)
        {
            if (_applicationName != null)
            {
                CredentialManager.WriteCredential(_applicationName, token.UserId, token.RefreshToken);
            }

            return Task.FromResult(0);
        }

        public Task<RefreshTokenInfo> RetrieveRefreshTokenAsync()
        {
            if (_applicationName != null)
            {
                Credential cred = CredentialManager.ReadCredential(_applicationName);
                if (cred != null)
                {
                    return Task.FromResult(new RefreshTokenInfo(cred.UserName, cred.Password));
                }
            }
            return Task.FromResult((RefreshTokenInfo)null);
        }
    }
}