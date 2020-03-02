using System;
using Utf8Json;
using System.Net;
using System.Text;
using System.Net.Http;
using EdgeUpdateAPI.Classes;
using System.Threading.Tasks;
using EdgeUpdateAPI.Responses;
using Utf8Json.Formatters;
using System.Collections.Generic;
using System.Linq;

namespace EdgeUpdateAPI
{
    public sealed class EdgeUpdate
    {
        #region Properties

        [Obsolete("Outdated, this will return an old update")]
        public static Guid CanaryGuid { get; } = new Guid("65C35B14-6C1D-4122-AC46-7148CC9D6497");
        [Obsolete("Outdated, this will return an old update")]
        public static Guid DevGuid { get; } = new Guid("0D50BFEC-CD6A-4F9A-964C-C7416E3ACB10");
        [Obsolete("Outdated, this will return an old update")]
        public static Guid BetaGuid { get; } = new Guid("2CD8A007-E189-409D-A2C8-9AF4EF3C72AA");
        [Obsolete("Outdated, this will return an old update")]
        public static Guid StableGuid { get; } = new Guid("56EB18F8-B008-4CBD-B6D2-8C97FE7E9062");
        [Obsolete("Outdated, this will return an old update")]
        public static Guid SetupGuid { get; } = new Guid("F3C4FE00-EFD5-403B-9569-398A20F1BA4A");

        public static string Canary_Win_x64 { get; } = "msedge-canary-win-x64";
        public static string Canary_Win_x86 { get; } = "msedge-canary-win-x86";
        public static string Canary_Win_arm64 { get; } = "msedge-canary-win-arm64";

        public static string Beta_Win_x64 { get; } = "msedge-beta-win-x64";
        public static string Beta_Win_x86 { get; } = "msedge-beta-win-x86";
        public static string Beta_Win_arm64 { get; } = "msedge-beta-win-arm64";

        public static string Stable_Win_x64 { get; } = "msedge-stable-win-x64";
        public static string Stable_Win_x86 { get; } = "msedge-stable-win-x86";
        public static string Stable_Win_arm64 { get; } = "msedge-stable-win-arm64";

        public static string Dev_Win_x64 { get; } = "msedge-dev-win-x64";
        public static string Dev_Win_x86 { get; } = "msedge-dev-win-x86";
        public static string Dev_Win_arm64 { get; } = "msedge-dev-win-arm64";

        #endregion

        #region Fields

        private readonly HttpClient _hc;
        private const string URL = "msedge.api.cdp.microsoft.com";

        #endregion

        //Ctor
        public EdgeUpdate()
        {
            //GZip or deflate.
            var filter = new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.All };

            //Create a new instance of HttpClient.
            _hc = new HttpClient(filter);
            _hc.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Microsoft Edge Update/1.3.101.7;winhttp"); //Not needed but... who cares
            _hc.DefaultRequestHeaders.Connection.Add("keep-alive");
        }

        #region Public methods

        public async Task<IResult<EdgeFiles>> GetLatestFiles(string name)
        {
            var res = await GetLatestVersion(name);
            if (res.Success)
            {
                return await GetFiles(name, res.Value.ContentId.Version);                
            }
            else
            {
                return new Result<EdgeFiles>(false, res.Message, res.ResultType);
            }
        }

        public async Task<IResult<EdgeFiles>> GetFiles(string name, string version)
        {
            var res2 = await GetGeneratedLink(name, version);
            if (res2.Success)
            {
                EdgeFiles edgeFiles = new EdgeFiles
                {
                    Version = version
                };

                if (res2.Value.Count > 0)
                {
                    edgeFiles.EdgeFile = new EdgeFile[res2.Value.Count];

                    for (int i = 0; i < res2.Value.Count; i++)
                    {
                        //if it's a full update == 2 underscores
                        //3 underscores == delta update

                        var splitted = res2.Value[i].FileId.Replace(".exe", "", StringComparison.InvariantCultureIgnoreCase).Split('_');
                        bool isDeltaUpdate = splitted.Length >= 4;

                        //var edgeName = splitted[0];
                        //var arch = splitted[1];
                        //var version = splitted[2];
                        //var deltaUpdate = splitted[3];

                        edgeFiles.EdgeFile[i] = new EdgeFile
                        {
                            FileName = res2.Value[i].FileId,
                            Sha1 = Convert.FromBase64String(res2.Value[i].Hashes.Sha1),
                            Sha256 = Convert.FromBase64String(res2.Value[i].Hashes.Sha256),
                            Size = res2.Value[i].SizeInBytes,
                            Url = res2.Value[i].Url,
                            EdgeFileUpdateType = isDeltaUpdate ? EdgeFileUpdateType.Delta : EdgeFileUpdateType.Full,
                            DeltaVersion = isDeltaUpdate ? splitted[3] : null,
                            Arch = Enum.Parse<Arch>(splitted[1], true)
                        };
                    }

                    return new Result<EdgeFiles>(true, edgeFiles);
                }
                else
                {
                    return new Result<EdgeFiles>(false, "Missing files", ResultType.Other);
                }
            }
            else
            {
                return new Result<EdgeFiles>(false, res2.Message, res2.ResultType);
            }
        }

        #endregion

        //ABSTRACTION LEVEL OVER 9000

        #region Private methods

        private async Task<IResult<EdgeFiles>> GetLatestFiles(Guid guidRing)
            => await GetLatestFiles(guidRing.ToString());

        private async Task<IResult<LatestResponse>> GetLatestVersion(string name)
            => await SendRequestAndDeserialize<LatestResponse>($"api/v1/contents/Browser/namespaces/Default/names/{name}/versions/latest?action=select",
                HttpMethod.Post, "{\"targetingAttributes\":{}}");

        private async Task<IResult<LatestResponse>> GetLatestVersion(Guid guidRing) 
            => await GetLatestVersion(guidRing.ToString());

        private async Task<IResult<List<FilesResponse>>> GetVersionDetails(string name, string version)
            => await SendRequestAndDeserialize<List<FilesResponse>>($"api/v1/contents/Browser/namespaces/Default/names/{name}/versions/{version}/files",
                HttpMethod.Get);
        private async Task<IResult<List<FilesResponse>>> GetVersionDetails(Guid guidRing, string version)
            => await GetVersionDetails(guidRing.ToString(), version);

        private async Task<IResult<List<FilesResponse>>> GetGeneratedLink(string name, string version)
            => await SendRequestAndDeserialize<List<FilesResponse>>($"api/v1/contents/Browser/namespaces/Default/names/{name}/versions/{version}/files?action=GenerateDownloadInfo",
                HttpMethod.Post, "{\"targetingAttributes\":{}}");
        private async Task<IResult<List<FilesResponse>>> GetGeneratedLink(Guid guidRing, string version)
            => await GetGeneratedLink(guidRing.ToString(), version);

        private async Task<IResult<T>> SendRequestAndDeserialize<T>(string path, HttpMethod reqMethod, string content = "")
        {
            var req = await SendRequest(path, reqMethod, content);

            if (req.Success)
            {
                try
                {
                    var serialized = JsonSerializer.Deserialize<T>(req.Value);
                    return new Result<T>(true, serialized);
                }
                catch (Exception ex)
                {
                    return new Result<T>(false, ex.Message, ResultType.Exception);
                }
            }
            else
                return new Result<T>(false, "Request failure.\n" + req.Message, req.ResultType);

        }

        private async Task<IResult<string>> SendRequest(string path, HttpMethod reqMethod, string content = "")
        {
            var req = CreateRequestHeader(reqMethod, $"https://{URL}/{path}");

            if (reqMethod == HttpMethod.Post)
                req.Content = new StringContent(content, Encoding.UTF8, "application/json");

            try
            {
                var response = await _hc.SendAsync(req);

                if (response.IsSuccessStatusCode)
                    return new Result<string>(true, await response.Content.ReadAsStringAsync());
                else
                    return new Result<string>(false, "BOH", ResultType.Other); //TODO: replace BOH with something more useful

            }
            catch (Exception ex)
            {
                return new Result<string>(false, ex.Message, ResultType.Exception);
            }
        }

        private static HttpRequestMessage CreateRequestHeader(HttpMethod method, string url)
            => new HttpRequestMessage(method, new Uri(url));

        #endregion
    }
}
