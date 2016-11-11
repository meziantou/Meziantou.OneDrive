namespace Meziantou.OneDriveClient.Windows
{
    internal enum CredentialType
    {
        Generic = 1,
        DomainPassword,
        DomainCertificate,
        DomainVisiblePassword,
        GenericCertificate,
        DomainExtended,
        Maximum,
        MaximumEx = Maximum + 1000,
    }
}