using System;

namespace Meziantou.OneDrive
{
    public class RefreshTokenInfo
    {
        public RefreshTokenInfo(string refreshToken)
        {
            RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
        }

        public string RefreshToken { get; }
    }
}