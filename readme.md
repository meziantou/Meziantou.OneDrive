# Meziantou - OneDrive

# How to use the code

To use Microsoft OneDrive API, you must register your application on <https://account.live.com/developers/applications> to get a clientId like `0000000000000000`.

# Usage

1. Install the NuGet package:

````
Install-Package Meziantou.OneDriveClient.Windows
````

2. Initialize the client:

```
// Initialize the client
var client = new OneDriveClient();
client.ApplicationId = "000000004418B915";
client.AuthorizationProvider = new AuthorizationCodeProvider(); // Provider to display the login window

// Optional
// Store the refresh token, so the application can re-authenticate automatically when the token expires
client.RefreshTokenHandler = new MemoryRefreshTokenHandler();
client.RefreshTokenHandler = new CredentialManagerRefreshTokenHandler("appname");
```

3. Use the client:

```
var item = await Client.CreateDirectoryAsync("sample");
var children = await item.GetChildrenAsync();
var newItem = await item.CreateFileAsync("sample.txt", stream, length: 1000, chunckSize: 100, OnChunkErrorHandler);
var stream = await newItem.DownloadAsync();
```
