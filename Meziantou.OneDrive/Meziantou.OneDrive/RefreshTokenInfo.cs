using System;

namespace Meziantou.OneDrive
{
    public class RefreshTokenInfo
    {
        public RefreshTokenInfo(string userId, string refreshToken)
        {
            if (userId == null) throw new ArgumentNullException(nameof(userId));
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));
            UserId = userId;
            RefreshToken = refreshToken;
        }

        public string UserId { get;  }
        public string RefreshToken { get; }
        
    }
}