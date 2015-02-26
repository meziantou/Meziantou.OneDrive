# Meziantou - OneDrive

# How to use the code

To use Microsoft OneDrive API, you must register your application on <https://account.live.com/developers/applications> to get a clientId like `0000000000000000`.

**Authentication**

	LiveSession session = await AuthenticationForm.CreateSessionAsync(clientId, new[]
	{
	    Scope.BasicProfile,
	    Scope.Photos,
	    Scope.ReadOnly,
	    Scope.ReadWrite,
	    Scope.SharedItems,
	    Scope.OfflineAccess
	}, refreshTokenHandler /* may be null */);

The refresh token handler persists the refresh token so you may not need to re-authenticate the next time you need to create a session. We provide 2 refresh token handlers: `InMemory` and [CredentialManager](https://gist.github.com/meziantou/10311113).

Then you can create a `OneDriveClient`:

    OneDriveClient client = new OneDriveClient(session);

**Enumerate items**

    OneDriveFolder root = await client.GetOneDriveRootAsync(cancellationToken);
    foreach (OneDriveItem child in await root.GetChildrenAsync(cancellationToken))
    {
        Console.WriteLine(string.Format("({0}) {1}", child.Type, child.Name));
    }

	// Pagination
 	root.GetChildrenAsync(new GetOptions { Offset = 0, Limit = 20 }, cancellationToken)

	// Filter
	root.GetChildrenAsync(new GetOptions { Filters = ItemTypeFilter.Folder | ItemTypeFilter.Album }, cancellationToken)

	// Search
	client.SearchAsync("sample", cancellationToken)

	// Well-known folders
	client.GetWellKnownFolderAsync(WellKnownFolder.CameraRoll, cancellationToken)
	client.GetWellKnownFolderAsync(WellKnownFolder.Documents, cancellationToken)
	client.GetWellKnownFolderAsync(WellKnownFolder.Pictures, cancellationToken)
	client.GetWellKnownFolderAsync(WellKnownFolder.PublicDocuments, cancellationToken)

**Create a folder**

    OneDriveFolder newFolder = await root.CreateChildDirectoryAsync("New Folder", cancellationToken)

**Download a file**
    
    Stream stream = await file.DownloadAsync(cancellationToken)
    Stream stream = await file.DownloadAsync(rangeStart: 0, rangeEnd: 100, cancellationToken) // in byte

**Upload a file**

	// using REST API (max file size: 100MB)
    OneDriveFile file = await oneDriveFolder.UploadAsync(fileInfo, OverwriteOption.Overwrite, cancellationToken);

	// using BITS
	OneDriveFile file = await oneDriveFolder.BitsUploadAsync(fileInfo, cancellationToken);

	// using BITS (with retry)
    EventHandler<BitsUploadChunckFailedEventArgs> callback = (sender, args) =>
    {
        args.Cancel = args.Attempt > maximumRetryCount;
    };
    oneDriveFile = await folder.BitsUploadAsync(fileInfo, chunckSize, callback, cancellationToken));

**Move or copy item**

    folder.MoveToNewParentAsync(moveToFolder, cancellationToken);
    file.CopyToNewParentAsync(moveToFolder, cancellationToken);

**Get sharing link**

    await file.GetLinkAsync(LinkType.ReadOnly)
	await file.GetLinkAsync(LinkType.ReadWrite)
	await file.GetLinkAsync(LinkType.Embed)

**Quota**

	Quota quota = client.GetQuotaAsync().Result;
    Console.WriteLine("Quota: {0} free of {1} ({2} used)",
    	ConvertUtilities.FormatFileSize(quota.AvailableSpace),
        ConvertUtilities.FormatFileSize(quota.TotalSpace),
        ConvertUtilities.FormatFileSize(quota.TotalSpace - quota.AvailableSpace));

# Dependencies

There is only one dependency:

- [CodeFluent.Runtime.Client](http://www.softfluent.com/products/codefluent-runtime-client) (>=804)
