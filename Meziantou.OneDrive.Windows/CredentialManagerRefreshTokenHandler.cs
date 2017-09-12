using System;
using System.Threading;
using System.Threading.Tasks;
using Meziantou.Framework.Win32;

namespace Meziantou.OneDrive.Windows
{
    public class CredentialManagerRefreshTokenHandler : IRefreshTokenHandler
    {
        private readonly string _applicationName;
        private readonly CredentialPersistence _credentialPersistence;

        public CredentialManagerRefreshTokenHandler(string applicationName, CredentialPersistence credentialPersistence)
        {
            if (applicationName == null) throw new ArgumentNullException(nameof(applicationName));

            _applicationName = applicationName;
            _credentialPersistence = credentialPersistence;
        }

        public Task SaveRefreshTokenAsync(RefreshTokenInfo token, CancellationToken ct)
        {
            CredentialManager.WriteCredential(_applicationName, "UserName", token.RefreshToken, _credentialPersistence);
            return Task.CompletedTask;
        }

        public Task<RefreshTokenInfo> RetrieveRefreshTokenAsync(CancellationToken ct)
        {
            Credential cred = CredentialManager.ReadCredential(_applicationName);
            if (cred != null)
                return Task.FromResult(new RefreshTokenInfo(cred.Password));

            return Task.FromResult((RefreshTokenInfo)null);
        }

        public Task DeleteRefreshTokenAsync(CancellationToken ct)
        {
            CredentialManager.DeleteCredential(_applicationName);
            return Task.CompletedTask;
        }
    }
}