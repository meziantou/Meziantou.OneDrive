using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeFluent.Runtime.Utilities;
using CodeFluent.Runtime.Web.Utilities;
using Meziantou.OneDrive.Diagnostics;
using Meziantou.OneDrive.Utilities;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;

namespace Meziantou.OneDrive
{
    public class OneDriveClient : IDisposable
    {
        private const string ApiEndpoint = "https://apis.live.net/v5.0/";
        private readonly object _lock = new object();
        private readonly LiveSession _session;
        private HttpClient _httpClient;

        protected LiveSession Session
        {
            get { return _session; }
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }

            Logger.Log(LogComponent.Core, LogType.Verbose, value: "OneDriveClient disposed.");
        }

        public OneDriveClient(LiveSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            _session = session;
        }

        public Task<User> GetCurrentUserAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetUserAsync(null, cancellationToken);
        }

        public Task<User> GetUserAsync(string id = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetAsync<User>(string.Format("/{0}", id ?? "me"), null, cancellationToken);
        }

        public Task<IEnumerable<User>> GetFriendsAsync(GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetCollectionAsync<User>("/me/friends", options, cancellationToken);
        }

        public Task<Quota> GetQuotaAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetAsync<Quota>("/me/skydrive/quota", null, cancellationToken);
        }

        public Task<OneDriveFolder> GetOneDriveRootAsync(string userId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetAsync<OneDriveFolder>(string.Format("/{0}/skydrive", userId ?? "me"), null, cancellationToken);
        }

        public async Task<OneDriveItem> GetItemByPathAsync(string path, string userId = null, bool createIfNotExist = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var parts = path.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var item = await GetOneDriveRootAsync(userId, cancellationToken).ConfigureAwait(false);

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                if (i < parts.Length - 1) // folder
                {
                    bool found = false;
                    var children = await item.GetChildrenAsync(new GetOptions { Filters = ItemTypeFilter.Folder | ItemTypeFilter.Album }, cancellationToken);
                    foreach (var child in children.OfType<OneDriveFolder>())
                    {
                        if (string.Equals(child.Name, part, StringComparison.OrdinalIgnoreCase))
                        {
                            item = child;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        if (createIfNotExist)
                        {
                            item = await item.CreateChildDirectoryAsync(part, cancellationToken);
                        }
                        else
                        {
                            throw new DirectoryNotFoundException();
                        }
                    }
                }
                else // Last item => File or folder
                {
                    var children = await item.GetChildrenAsync(null, cancellationToken);
                    foreach (var child in children)
                    {
                        if (string.Equals(child.Name, part, StringComparison.OrdinalIgnoreCase))
                            return child;
                    }

                    if (createIfNotExist)
                    {
                        return await item.CreateChildDirectoryAsync(part, cancellationToken);
                    }

                    throw new DirectoryNotFoundException();
                }
            }

            return null;
        }

        public Task<IEnumerable<OneDriveItem>> SearchAsync(string query, GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetCollectionAsync<OneDriveItem>("/me/skydrive/search?q=" + Uri.EscapeDataString(query), options, cancellationToken);
        }

        public Task<IEnumerable<OneDriveItem>> GetSharedItemsAsync(GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetCollectionAsync<OneDriveItem>("/me/skydrive/shared", options, cancellationToken);
        }

        public Task<IEnumerable<OneDriveItem>> GetRecentItemsAsync(GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetCollectionAsync<OneDriveItem>("/me/skydrive/recent_docs", options, cancellationToken);
        }


        public Task<IEnumerable<OneDriveAlbum>> GetAlbumsAsync(GetOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetCollectionAsync<OneDriveAlbum>("/me/albums", options, cancellationToken);
        }

        public Task<OneDriveAlbum> CreateAlbumsAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var dict = new Dictionary<string, object>();
            dict.Add("name", name);
            return PostAsync<OneDriveAlbum>("/me/albums", dict, cancellationToken);
        }

        public Task<OneDriveItem> GetWellKnownFolderAsync(WellKnownFolder folder, string userId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetAsync<OneDriveItem>("/" + (userId ?? "me") + OneDriveUtilities.GetWellKnowFolderPath(folder), null, cancellationToken);
        }

        public Task<OneDriveItem> GetItemByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetAsync<OneDriveItem>(string.Format("/{0}", id), null, cancellationToken);
        }

        public Task<Tag> GetTagByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return GetAsync<Tag>(string.Format("/{0}", id), null, cancellationToken);
        }

        protected string GetResourceUri(string path, GetOptions options)
        {
            if ((path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
               !path.StartsWith(ApiEndpoint, StringComparison.OrdinalIgnoreCase)) ||
                path.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            EditableUri editableUri;
            if (path.StartsWith(ApiEndpoint, StringComparison.OrdinalIgnoreCase))
            {
                editableUri = new EditableUri(path);
            }
            else
            {
                editableUri = new EditableUri(ApiEndpoint + path);
            }

            if (options == null)
                return editableUri.ToString();

            //editableUri.Parameters["suppress_response_codes"] = "true";
            if (options.SuppressRedirect.HasValue)
            {
                editableUri.Parameters["suppress_redirects"] = options.SuppressRedirect.Value ? "true" : "false";
            }
            if (options.ReturnSslResources.HasValue)
            {
                editableUri.Parameters["return_ssl_resources"] = options.ReturnSslResources.Value ? "true" : "false";
            }
            if (options.Pretty.HasValue)
            {
                editableUri.Parameters["pretty"] = options.Pretty.Value ? "true" : "false";
            }
            if (options.Locale != null)
            {
                editableUri.Parameters["locale"] = options.Locale.Name;
            }
            if (options.Filters != ItemTypeFilter.None)
            {
                var filters = ConvertUtilities.SplitEnumValues(options.Filters);
                editableUri.Parameters["filter"] = string.Join(",", filters.Select(OneDriveUtilities.GetFilterName));
            }
            if (options.Offset.HasValue)
            {
                editableUri.Parameters["offset"] = options.Offset.Value;
            }
            if (options.Limit.HasValue)
            {
                editableUri.Parameters["limit"] = options.Limit.Value;
            }
            if (options.SortOrder != SortProperty.Default)
            {
                editableUri.Parameters["sort_by"] = OneDriveUtilities.GetSortPropertyName(options.SortOrder);
                editableUri.Parameters["sort_order"] = options.SortDirection == SortDirection.Ascending ? "ascending" : "descending";
            }

            return editableUri.ToString();
        }

        public async Task<string> SendJsonAsync(string uri, string method, string json, CancellationToken cancellationToken)
        {
            HttpWebRequest request = await CreateJsonRequestAsync(uri, cancellationToken);
            request.Method = method;
            request.AllowReadStreamBuffering = false;
            var bytes = Encoding.UTF8.GetBytes(json);
            LogRequestMessage(request, json);
            await SetRequestStreamAsync(request, bytes, 0, bytes.Length, cancellationToken);
            using (HttpWebResponse response = await GetResponseAsync(request, cancellationToken))
            {
                await EnsureSuccess(response);
                return await ReadReponseAsString(response);
            }
        }

        private async Task<string> ReadReponseAsString(HttpWebResponse response, [CallerMemberName]string methodName = null)
        {
            using (Stream stream = response.GetResponseStream())
            {
                if (stream == null)
                    return null;

                Encoding encoding = response.CharacterSet != null ? Encoding.GetEncoding(response.CharacterSet) : Encoding.UTF8;
                using (TextReader reader = new StreamReader(stream, encoding))
                {
                    var content = await reader.ReadToEndAsync();
                    LogResponseMessage(response, content, methodName);
                    return content;
                }
            }
        }

        protected virtual async Task EnsureSuccess(HttpWebResponse response, [CallerMemberName]string methodName = null)
        {
            if (IsSuccessStatusCode(response.StatusCode))
                return;

            string content = await ReadReponseAsString(response);
            LogResponseMessage(response, content, methodName);
            var dict = Deserialize<Dictionary<string, object>>(content);

            /*
             * "error": {
             *     "code": "request_token_expired", 
             *     "message": "The provided access token has expired."
             * }
             */

            string reasonPhrase = response.StatusDescription;
            HttpStatusCode statusCode = response.StatusCode;
            string code = null;
            string message = null;

            if (dict != null)
            {
                object errorObject;
                dict.TryGetValue("error", out errorObject);
                if (errorObject == null)
                {
                    errorObject = dict;
                }

                var errorDict = errorObject as Dictionary<string, object>;
                if (errorDict != null)
                {
                    object o;
                    if (errorDict.TryGetValue("code", out o))
                    {
                        code = o as string;
                    }
                    if (errorDict.TryGetValue("message", out o))
                    {
                        message = o as string;
                    }
                }
            }

            throw new OneDriveException(code, message, statusCode, reasonPhrase);
        }

        protected virtual bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return statusCode >= HttpStatusCode.OK && statusCode <= (HttpStatusCode)299;
        }

        protected async virtual Task<HttpWebRequest> CreateRequestAsync(string uri, CancellationToken cancellationToken)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            if (!Session.IsValid)
            {
                await Session.RefreshTokenAsync(cancellationToken);
            }

            HttpWebRequest webRequest = WebRequest.CreateHttp(uri);
            webRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + Session.AccessToken);
            return webRequest;
        }

        protected virtual void AddJsonHeader(HttpWebRequest webRequest)
        {
            if (webRequest == null) throw new ArgumentNullException(nameof(webRequest));

            webRequest.ContentType = "application/json; charset=" + Encoding.UTF8.EncodingName;
        }

        protected async virtual Task<HttpWebRequest> CreateJsonRequestAsync(string uri, CancellationToken cancellationToken)
        {
            HttpWebRequest webRequest = await CreateRequestAsync(uri, cancellationToken);
            AddJsonHeader(webRequest);
            return webRequest;
        }

        private static async Task<HttpWebResponse> GetResponseAsync(HttpWebRequest request, CancellationToken ct)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            using (ct.Register(request.Abort, useSynchronizationContext: false))
            {
                try
                {
                    var response = await request.GetResponseAsync();
                    ct.ThrowIfCancellationRequested();
                    return (HttpWebResponse)response;
                }
                catch (WebException ex)
                {
                    if (ct.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(ex.Message, ex, ct);
                    }

                    throw;
                }
            }
        }

        private static async Task SetRequestStreamAsync(HttpWebRequest request, Stream s, CancellationToken ct)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            using (ct.Register(request.Abort, useSynchronizationContext: false))
            {
                try
                {
                    using (var response = await request.GetRequestStreamAsync())
                    {
                        await s.CopyToAsync(response, 4096, ct);
                    }
                }
                catch (WebException ex)
                {
                    if (ct.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(ex.Message, ex, ct);
                    }

                    throw;
                }
            }
        }

        private static async Task SetRequestStreamAsync(HttpWebRequest request, byte[] bytes, int offset, int count, CancellationToken ct)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            using (ct.Register(request.Abort, useSynchronizationContext: false))
            {
                try
                {
                    using (var response = await request.GetRequestStreamAsync())
                    {
                        await response.WriteAsync(bytes, offset, count, ct);
                    }
                }
                catch (WebException ex)
                {
                    if (ct.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(ex.Message, ex, ct);
                    }

                    throw;
                }
            }
        }

        internal async Task<T> GetAsync<T>(string url, GetOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await GetAsync(url, options, cancellationToken);
            return Deserialize<T>(result);
        }
        internal async Task<IEnumerable<T>> GetCollectionAsync<T>(string url, GetOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await GetAsync(url, options, cancellationToken);
            return DeserializeCollectionResponse<T>(result);
        }

        internal async Task<string> GetAsync(string url, GetOptions options, CancellationToken cancellationToken = default(CancellationToken))
        {
            url = GetResourceUri(url, options);
            HttpWebRequest request = await CreateRequestAsync(url, cancellationToken);
            LogRequestMessage(request);
            using (HttpWebResponse response = await GetResponseAsync(request, cancellationToken))
            {
                await EnsureSuccess(response);
                return await ReadReponseAsString(response);
            }
        }

        internal async Task<T> PostAsync<T>(string url, object data, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await PostAsync(url, data, cancellationToken);
            return Deserialize<T>(result);
        }

        internal Task<string> PostAsync(string url, object data, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = Serialize(data);
            url = GetResourceUri(url, null);
            return SendJsonAsync(url, "POST", json, cancellationToken);
        }

        internal async Task<T> PutAsync<T>(string url, object data, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await PutAsync(url, data, cancellationToken);
            return Deserialize<T>(result);
        }

        internal Task<string> PutAsync(string url, object data, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = Serialize(data);
            url = GetResourceUri(url, null);
            return SendJsonAsync(url, "PUT", json, cancellationToken);
        }

        internal async Task<T> CopyAsync<T>(string url, string destination, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await CopyAsync(url, destination, cancellationToken);
            return Deserialize<T>(result);
        }

        internal Task<string> CopyAsync(string url, string destination, CancellationToken cancellationToken = default(CancellationToken))
        {
            return CopyOrMoveAsync(url, "COPY", destination, cancellationToken);
        }

        internal async Task<T> MoveAsync<T>(string url, string destination, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await MoveAsync(url, destination, cancellationToken);
            return Deserialize<T>(result);
        }

        internal Task<string> MoveAsync(string url, string destination, CancellationToken cancellationToken = default(CancellationToken))
        {
            return CopyOrMoveAsync(url, "MOVE", destination, cancellationToken);
        }

        private Task<string> CopyOrMoveAsync(string url, string method, string destination, CancellationToken cancellationToken = default(CancellationToken))
        {
            string json = "{destination:" + Serialize(destination) + "}";
            url = GetResourceUri(url, null);
            return SendJsonAsync(url, method, json, cancellationToken);
        }

        internal async Task<T> DeleteAsync<T>(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await DeleteAsync(url, cancellationToken).ConfigureAwait(false);
            return Deserialize<T>(result);
        }

        internal async Task<string> DeleteAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            url = GetResourceUri(url, null);
            HttpWebRequest request = await CreateRequestAsync(url, cancellationToken);
            request.Method = "DELETE";
            LogRequestMessage(request);
            using (HttpWebResponse response = await GetResponseAsync(request, cancellationToken))
            {
                await EnsureSuccess(response);
                return await ReadReponseAsString(response);
            }
        }

        internal async Task<T> UploadAsync<T>(string url, string name, Stream stream, OverwriteOption overwriteOption, bool resizePhoto = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            string result = await UploadAsync(url, name, stream, overwriteOption, resizePhoto, cancellationToken).ConfigureAwait(false);
            return Deserialize<T>(result);
        }

        internal async Task<string> UploadAsync(string url, string name, Stream stream, OverwriteOption overwriteOption, bool resizePhoto = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            url = GetResourceUri(url, null);
            EditableUri editableUri = new EditableUri(url);
            editableUri.AbsolutePath += "/" + Uri.EscapeDataString(name);
            editableUri.Parameters["downsize_photo_uploads"] = resizePhoto ? "true" : "false";
            switch (overwriteOption)
            {
                case OverwriteOption.DoNotOverwrite:
                    editableUri.Parameters["overwrite"] = "false";
                    break;
                case OverwriteOption.Overwrite:
                    editableUri.Parameters["overwrite"] = "true";
                    break;
                case OverwriteOption.Rename:
                    editableUri.Parameters["overwrite"] = "ChooseNewName";
                    break;
            }

            url = editableUri.ToString();

            HttpWebRequest request = await CreateRequestAsync(url, cancellationToken);
            request.Method = "PUT";
            request.AllowWriteStreamBuffering = false;
            request.ContentLength = stream.Length;
            LogRequestMessage(request);
            await SetRequestStreamAsync(request, stream, cancellationToken);
            using (HttpWebResponse response = await GetResponseAsync(request, cancellationToken))
            {
                await EnsureSuccess(response);
                return await ReadReponseAsString(response);
            }
        }

        internal async Task<Stream> DowloadAsync(string url, long? rangeStart = null, long? rangeEnd = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            url = GetResourceUri(url, null);
            HttpWebRequest request = await CreateRequestAsync(url, cancellationToken);
            request.Method = "GET";
            if (rangeStart.HasValue || rangeEnd.HasValue)
            {
                string range = string.Format("bytes={0}-{1}", rangeStart, rangeEnd);
                request.Headers.Add(HttpRequestHeader.Range, range);
            }
            request.AllowReadStreamBuffering = false;
            HttpWebResponse response = await GetResponseAsync(request, cancellationToken);
            try
            {
                await EnsureSuccess(response);

                Stream responseStream = response.GetResponseStream();
                if (responseStream == null)
                    return null;

                var streamWithEvents = new StreamWithDisposeEvents(responseStream);
                streamWithEvents.Disposed += (sender, args) =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    response.Dispose();
                };

                return responseStream;

            }
            catch
            {
                response?.Dispose();
                throw;
            }
        }

        private void LogRequestMessage(HttpWebRequest message, string content = null, [CallerMemberName]string methodName = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("---->: " + message.Method + " " + message.RequestUri.AbsoluteUri);
            if (message.Headers.Count > 0)
            {
                sb.AppendLine();
                DumpHttpHeaders(sb, message.Headers);
            }

            if (content != null)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(content);
            }

            // ReSharper disable once ExplicitCallerInfoArgument
            Logger.Log(LogComponent.Core, LogType.Verbose, methodName: methodName, value: sb.ToString());
        }

        private void LogResponseMessage(HttpWebResponse message, string content = null, [CallerMemberName]string methodName = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<----: " + message.Method + " " + message.ResponseUri.AbsoluteUri);
            sb.AppendLine(string.Format("StatusCode: {0} ({1}) {2}", (int)message.StatusCode, message.StatusCode, message.StatusDescription));
            if (message.Headers.Count > 0)
            {
                sb.AppendLine();
                DumpHttpHeaders(sb, message.Headers);
            }

            if (content != null)
            {
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine(Format(content));
            }

            // ReSharper disable once ExplicitCallerInfoArgument
            Logger.Log(LogComponent.Core, LogType.Verbose, methodName: methodName, value: sb.ToString());
        }

        private static void DumpHttpHeaders(StringBuilder sb, NameValueCollection headers)
        {
            foreach (var key in headers.AllKeys)
            {
                string[] values = headers.GetValues(key);
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        sb.AppendLine(key + ": " + value);
                    }
                }
            }
        }
        private static void DumpHttpHeaders(StringBuilder sb, HttpHeaders headers)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> key in headers)
            {
                if (key.Value != null)
                {
                    foreach (var value in key.Value)
                    {
                        sb.AppendLine(key + ": " + value);
                    }
                }
            }
        }

        private async Task<string> BitsCreateSessionAsync(string url, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpWebRequest request = await CreateRequestAsync(url, cancellationToken);
            request.Method = "POST";
            request.ContentLength = 0;
            request.Headers.Add("X-Http-Method-Override", "BITS_POST");
            request.Headers.Add("BITS-Packet-Type", "Create-Session");
            request.Headers.Add("BITS-Supported-Protocols", "{7df0354d-249b-430f-820d-3d2a9bef4931}");
            using (HttpWebResponse response = await GetResponseAsync(request, cancellationToken))
            {
                await EnsureSuccess(response);
                string packetType = response.GetResponseHeader("BITS-Packet-Type");
                if (string.IsNullOrEmpty(packetType) || !packetType.Equals("Ack", StringComparison.OrdinalIgnoreCase))
                    throw new OneDriveException("Response was not an ACK");

                string sessionId = response.GetResponseHeader("BITS-Session-Id");
                if (string.IsNullOrEmpty(sessionId))
                    throw new OneDriveException("No session id found in ACK");

                return sessionId;
            }
        }

        private async Task<string> BitsCommitSessionAsync(string url, string sessionId, string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpWebRequest request = await CreateRequestAsync(url, cancellationToken);
            request.Method = "POST";
            request.ContentLength = 0;
            request.Headers.Add("X-Http-Method-Override", "BITS_POST");
            request.Headers.Add("BITS-Packet-Type", "Close-Session");
            request.Headers.Add("BITS-Session-Id", sessionId);
            using (HttpWebResponse response = await GetResponseAsync(request, cancellationToken))
            {
                await EnsureSuccess(response);
                string packetType = response.GetResponseHeader("BITS-Packet-Type");
                if (string.IsNullOrEmpty(packetType) || !packetType.Equals("Ack", StringComparison.OrdinalIgnoreCase))
                    throw new OneDriveException("Response was not an ACK");

                string resourceId = response.GetResponseHeader("X-Resource-Id");
                if (string.IsNullOrEmpty(resourceId))
                    throw new OneDriveException("No resource id found in ACK");

                return "file." + userId + "." + resourceId;
            }
        }

        private async Task BitsWriteFragmentsAsync(string url, string sessionId, string folderId, string name, Stream stream, int fragmentLength, EventHandler<BitsUploadChunckFailedEventArgs> chunckFailedCallback, CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] fragment = new byte[fragmentLength];
            int byteRead = 0;
            long position = 0;
            long totalLength = stream.Length;
            int attempt = 0;

            while (attempt > 0 || (byteRead = await stream.ReadAsync(fragment, 0, fragmentLength, cancellationToken)) > 0)
            {
                HttpWebRequest request = await CreateRequestAsync(url, cancellationToken);
                request.Method = "POST";
                request.Headers.Add("X-Http-Method-Override", "BITS_POST");
                request.Headers.Add("BITS-Packet-Type", "Fragment");
                request.Headers.Add("BITS-Session-Id", sessionId);
                request.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", position, position + byteRead - 1, totalLength));
                request.ContentLength = byteRead;
                request.AllowWriteStreamBuffering = false;
                await SetRequestStreamAsync(request, fragment, 0, byteRead, cancellationToken);
                using (HttpWebResponse response = await GetResponseAsync(request, cancellationToken))
                {
                    await EnsureSuccess(response);
                    string packetType = response.GetResponseHeader("BITS-Packet-Type");
                    if (string.IsNullOrEmpty(packetType) || !packetType.Equals("Ack", StringComparison.OrdinalIgnoreCase))
                    {
                        if (chunckFailedCallback != null)
                        {
                            attempt++;
                            BitsUploadChunckFailedEventArgs args = new BitsUploadChunckFailedEventArgs(folderId, name, position, position + byteRead - 1, totalLength, attempt);
                            chunckFailedCallback(this, args);
                            if (args.Cancel)
                            {
                                throw new InvalidOperationException("Response was not an ACK");
                            }
                        }
                    }
                    else
                    {
                        attempt = 0;
                    }
                }

                position += byteRead;
            }
        }

        internal async Task<string> BitsUploadAsync(string userId, string folderId, string name, Stream stream, int fragmentLength, EventHandler<BitsUploadChunckFailedEventArgs> chunckFailedCallback, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Change long id to short id
            string shortId = folderId;
            if (shortId.StartsWith("folder."))
            {
                shortId = shortId.Substring("folder.".Length);
            }

            int index = shortId.IndexOf(".", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                shortId = shortId.Substring(index + 1);
            }

            string url = string.Format("https://cid-{0}.users.storage.live.com/items/{1}/{2}", userId, shortId, Uri.EscapeDataString(name));

            string sessionId = await BitsCreateSessionAsync(url, cancellationToken);
            await BitsWriteFragmentsAsync(url, sessionId, folderId, name, stream, fragmentLength, chunckFailedCallback, cancellationToken);
            return await BitsCommitSessionAsync(url, sessionId, userId, cancellationToken);
        }

        protected virtual string Serialize(object obj)
        {
            return JsonUtilities.Serialize(obj, CreateJsonOptions());
        }

        protected virtual T Deserialize<T>(string json)
        {
            return JsonUtilities.Deserialize<T>(json, CreateJsonOptions());
        }

        //protected internal virtual T DeserializeDataResponse<T>(string json)
        //{
        //    // { data: ... }
        //    JsonUtilitiesOptions options = CreateJsonOptions();
        //    Dictionary<string, object> result = JsonUtilities.Deserialize<Dictionary<string, object>>(json, options);
        //    if (result == null)
        //        return default(T);

        //    object data;
        //    if (!result.TryGetValue("data", out data))
        //        return default(T);

        //    return (T)JsonUtilities.ChangeType(data, typeof(T), options);
        //}

        protected internal virtual IEnumerable<T> DeserializeCollectionResponse<T>(string json)
        {
            // [{}, {}]
            // { data: [{}, {}] }
            JsonUtilitiesOptions options = CreateJsonOptions();
            object result = JsonUtilities.Deserialize<object>(json, options);
            if (result == null)
                return null;

            var dict = result as IDictionary<string, object>;
            if (dict != null)
            {
                object o;
                if (dict.TryGetValue("data", out o))
                {
                    return (IEnumerable<T>)JsonUtilities.ChangeType(o, typeof(T[]), options);
                }
            }

            IEnumerable collection = result as IEnumerable;
            if (collection != null)
            {
                return (IEnumerable<T>)JsonUtilities.ChangeType(collection, typeof(T[]), options);
            }

            return (IEnumerable<T>)JsonUtilities.ChangeType(result, typeof(T[]), options);
        }

        protected virtual JsonUtilitiesOptions CreateJsonOptions()
        {
            JsonUtilitiesOptions options = new JsonUtilitiesOptions();
            options.SerializationOptions = JsonSerializationOptions.Default | JsonSerializationOptions.UseJsonAttributeForApply | JsonSerializationOptions.UseJsonAttribute;
            options.CreateInstanceCallback = args =>
            {
                Type type = args.Value as Type;
                if (type == null)
                {
                    return;
                }

                var instance = CreateInstanceFromType(type, args.ObjectGraph);
                if (instance != null)
                {
                    args.Value = instance;
                    args.Handled = true;
                }
            };
            return options;
        }

        protected virtual object CreateInstanceFromType(Type type, IDictionary<object, object> additionalData)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition.IsAssignableFrom(typeof(List<>)))
                {
                    return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GenericTypeArguments));
                }

                if (genericTypeDefinition == typeof(IEnumerable<>))
                {
                    return Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GenericTypeArguments));
                }
            }

            if (type.IsAssignableFrom(typeof(Tag)))
            {
                return new Tag(this);
            }

            if (type.IsAssignableFrom(typeof(OneDriveItem)) || type.IsAssignableFrom(typeof(ITaggable)))
            {
                string itemType = null;
                object o;
                if (additionalData != null && additionalData.TryGetValue("value", out o))
                {
                    var dict = o as IDictionary<string, object>;
                    if (dict != null)
                    {
                        itemType = dict.GetValue("type", (string)null);
                    }
                }

                switch (OneDriveUtilities.GetItemType(itemType))
                {
                    case ItemType.Folder:
                        return new OneDriveFolder(this);
                    case ItemType.Album:
                        return new OneDriveAlbum(this);
                    case ItemType.File:
                        return new OneDriveFile(this);
                    case ItemType.Photo:
                        return new OneDrivePhoto(this);
                    case ItemType.Video:
                        return new OneDriveVideo(this);
                    case ItemType.Audio:
                        return new OneDriveAudio(this);
                    //case ItemType.Unknown:
                    default:
                        return new OneDriveItem(this);
                }
            }

            if (type.IsAssignableFrom(typeof(OneDriveAlbum)))
            {
                return new OneDriveAlbum(this);
            }

            return null;
        }

        protected internal virtual void Apply(object input, object target)
        {
            JsonUtilities.Apply(input, target, CreateJsonOptions());
        }

        protected virtual string Format(string json)
        {
            try
            {
                return JsonUtilities.SerializeFormatted(JsonUtilities.Deserialize(json));
            }
            catch
            {
                return json;
            }
        }
    }
}
