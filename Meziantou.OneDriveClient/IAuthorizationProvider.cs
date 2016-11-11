namespace Meziantou.OneDriveClient
{
    public interface IAuthorizationProvider
    {
        string GetAuthorizationCode(OneDriveClient client);
    }
}