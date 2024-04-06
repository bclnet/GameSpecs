using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GameX.Formats
{
    /// <summary>
    /// AbstractHost
    /// </summary>
    public abstract class AbstractHost
    {
        /// <summary>
        /// Gets the set asynchronous.
        /// </summary>
        /// <param name="shouldThrow">if set to <c>true</c> [should throw].</param>
        /// <returns></returns>
        public abstract Task<HashSet<string>> GetSetAsync(bool shouldThrow = false);

        /// <summary>
        /// Gets the file asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="shouldThrow">if set to <c>true</c> [should throw].</param>
        /// <returns></returns>
        public abstract Task<Stream> GetFileAsync(string filePath, bool shouldThrow = false);
    }

    /// <summary>
    /// HttpHost
    /// </summary>
    /// <seealso cref="GameEstate.Core.AbstractHost" />
    public class HttpHost : AbstractHost
    {
        readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions { });
        readonly HttpClient _hc = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };

        public HttpHost(Uri address, string folder = null)
            => _hc.BaseAddress = folder == null ? address : new UriBuilder(address) { Path = $"{address.LocalPath}{folder}/" }.Uri;

        public static readonly Func<Uri, string, AbstractHost> Factory = (address, folder) => new HttpHost(address, folder);

        public async Task<T> CallAsync<T>(string path, NameValueCollection nvc = null, bool shouldThrow = false)
        {
            var requestUri = ToPathAndQueryString(path, nvc);
            //Log($"query: {requestUri}");
            var r = await _hc.GetAsync(requestUri).ConfigureAwait(false);
            if (!r.IsSuccessStatusCode) return !shouldThrow ? default(T) : throw new InvalidOperationException(r.ReasonPhrase);
            var data = await r.Content.ReadAsByteArrayAsync();
            return FromBytes<T>(data);
        }

        public override async Task<HashSet<string>> GetSetAsync(bool shouldThrow = false)
            => await _cache.GetOrCreate(".set", async x => await CallAsync<HashSet<string>>((string)x.Key));

        public override async Task<Stream> GetFileAsync(string filePath, bool shouldThrow = false)
            => await _cache.GetOrCreateAsync(filePath.Replace('\\', '/'), async x => await CallAsync<Stream>((string)x.Key));

        static string ToPathAndQueryString(string path, NameValueCollection nvc)
        {
            if (nvc == null) return path;
            var array = (
                from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select !string.IsNullOrEmpty(value) ? string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)) : null)
                .Where(x => x != null).ToArray();
            return path + (array.Length > 0 ? "?" + string.Join("&", array) : string.Empty);
        }

        static T FromBytes<T>(byte[] data)
        {
            string path;
            if (typeof(T) == typeof(Stream)) return (T)(object)new MemoryStream(data);
            else if (typeof(T) == typeof(HashSet<string>))
            {
                var d = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                // dir /s/b/a-d > .set
                var lines = Encoding.ASCII.GetString(data)?.Split('\n');
                if (lines?.Length >= 0)
                {
                    var startIndex = Path.GetDirectoryName(lines[0].TrimEnd().Replace('\\', '/')).Length + 1;
                    foreach (var line in lines) if (line.Length >= startIndex && (path = line.Substring(startIndex).TrimEnd().Replace('\\', '/')) != ".set") d.Add(path);
                }
                return (T)(object)d;
            }
            else throw new ArgumentOutOfRangeException(nameof(T), typeof(T).ToString());
        }
    }
}
