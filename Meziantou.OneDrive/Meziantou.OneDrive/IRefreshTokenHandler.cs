using System.Threading.Tasks;

namespace Meziantou.OneDrive
{
    public interface IRefreshTokenHandler
    {
        Task SaveRefreshTokenAsync(RefreshTokenInfo token);

        Task<RefreshTokenInfo> RetrieveRefreshTokenAsync();
    }
}
