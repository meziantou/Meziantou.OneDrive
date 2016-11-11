namespace Meziantou.OneDrive
{
    public interface IAuthorizationProvider
    {
        string GetAuthorizationCode(OneDriveClient client);
    }
}