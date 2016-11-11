using System;

namespace Meziantou.OneDriveClient
{
    public class RefreshTokenInfo
    {
        public RefreshTokenInfo(string refreshToken)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));
            RefreshToken = refreshToken;
        }

        public string RefreshToken { get; }
    }
}