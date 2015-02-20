using System.Threading.Tasks;

namespace Meziantou.OneDrive
{
    public class InMemoryRefreshTokenHandler : IRefreshTokenHandler
    {
        private RefreshTokenInfo _token;

        public Task SaveRefreshTokenAsync(RefreshTokenInfo token)
        {
            _token = token;
            return Task.FromResult(0);
        }

        public Task<RefreshTokenInfo> RetrieveRefreshTokenAsync()
        {
            return Task.FromResult(_token);
        }
    }
}