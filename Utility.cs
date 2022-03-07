#region Related components
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.IO;
using Microsoft.Extensions.Configuration;
#endregion

#if !SIGN
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VIEApps.Components.XUnitTests")]
#endif

namespace net.vieapps.Components.Utility
{
    /// <summary>
    /// Utility servicing methods
    /// </summary>
    public static partial class UtilityService
    {

        #region Constructor
        static UtilityService()
        {
            // global settings of service point - not available on macOS
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // default security protocol is TLS 1.2
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // switch off certificate validation - http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
        }
        #endregion

        #region UUID
        /// <summary>
        /// Gets an UUID (128 bits unique universal identity)
        /// </summary>
        /// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
        /// <param name="uuid">The string that presents an UUID</param>
        /// <param name="format">The string that presents the format</param>
        /// <returns>The string that presents an 128 bits UUID</returns>
        public static string GetUUID(bool asBase64Url = false, string uuid = null, string format = null)
        {
            var guid = string.IsNullOrWhiteSpace(uuid) ? Guid.NewGuid() : new Guid(uuid.Trim());
            return asBase64Url ? guid.ToByteArray().ToBase64Url() : guid.ToString(format ?? "N");
        }

        /// <summary>
        /// Gets an UUID (128 bits unique universal identity)
        /// </summary>
        /// <param name="uuid">The array of bytes that presents an UUID</param>
        /// <param name="format">The string that presents the format</param>
        /// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
        /// <returns>The string that presents an 128 bits UUID</returns>
        public static string GetUUID(string uuid, string format = null, bool asBase64Url = false)
            => UtilityService.GetUUID(asBase64Url, uuid, format);

        /// <summary>
        /// Gets an UUID (128 bits unique universal identity)
        /// </summary>
        /// <param name="uuid">The array of bytes that presents an UUID</param>
        /// <param name="format">The string that presents the format</param>
        /// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
        /// <returns>The string that presents an 128 bits UUID</returns>
        public static string GetUUID(byte[] uuid, string format = null, bool asBase64Url = false)
        {
            var guid = uuid == null || !uuid.Any() ? Guid.NewGuid() : new Guid(uuid.Take(16));
            return asBase64Url ? guid.ToByteArray().ToBase64Url() : guid.ToString(format ?? "N");
        }

        /// <summary>
        /// Generate an identity as UUID-format from this string
        /// </summary>
        /// <param name="string"></param>
        /// <param name="format">The string that presents the format</param>
        /// <param name="mode">BLAKE or MD5</param>
        /// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
        /// <returns>The string that presents an 128 bits UUID</returns>
        public static string GenerateUUID(this string @string, string format = null, string mode = null, bool asBase64Url = false)
            => string.IsNullOrWhiteSpace(@string)
                ? UtilityService.GetUUID(asBase64Url, null, format)
                : UtilityService.GetUUID(@string.GetHash(!string.IsNullOrWhiteSpace(mode) && mode.IsStartsWith("blake") ? "blake128" : "md5"), format, asBase64Url);

        /// <summary>
        /// Generate an identity as UUID-format from this array of bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="format">The string that presents the format</param>
        /// <param name="mode">BLAKE or MD5</param>
        /// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
        /// <returns>The string that presents an 128 bits UUID</returns>
        public static string GenerateUUID(this byte[] bytes, string format = null, string mode = null, bool asBase64Url = false)
            => UtilityService.GetUUID(bytes?.GetHash(!string.IsNullOrWhiteSpace(mode) && mode.IsStartsWith("blake") ? "blake128" : "md5"), format, asBase64Url);

        /// <summary>
        /// Gets a new UUID (universal unique identity - 128 bits or 32 hexa-characters)
        /// </summary>
        public static string NewUUID => UtilityService.GetUUID();

        static string _BlankUUID = null;

        /// <summary>
        /// Gets the blank UUID
        /// </summary>
        /// <returns></returns>
        public static string BlankUUID => UtilityService._BlankUUID ?? (UtilityService._BlankUUID = new string('0', 32));

        static Regex HexRegex => new Regex("[^0-9a-fA-F]+");

        /// <summary>
        /// Validates the UUID string
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="onlyHex">true to only allow hexa characters</param>
        /// <returns>true if it is valid; otherwise false.</returns>
        public static bool IsValidUUID(this string uuid, bool onlyHex = true)
            => !string.IsNullOrWhiteSpace(uuid) && uuid.Length.Equals(32) && (onlyHex ? UtilityService.HexRegex.Replace(uuid, "").Equals(uuid) : !uuid.Contains(" ") && !uuid.Contains(";"));
        #endregion

        #region Random number & code
        readonly static Random _Random = new Random();

        /// <summary>
        /// Gets the random number between min and max
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int min = 0, int max = Int32.MaxValue)
            => UtilityService._Random.Next(min, max);

        readonly static RandomBigInteger _RandomBigInteger = new RandomBigInteger();

        /// <summary>
        /// Gets the random of big integer number
        /// </summary>
        /// <param name="length">The number of random bits to generate.</param>
        /// <returns></returns>
        public static BigInteger GetRandomNumber(int length)
            => UtilityService._RandomBigInteger.Next(length);

        /// <summary>
        /// Gets a random code
        /// </summary>
        /// <param name="useShortCode">true to use short-code</param>
        /// <param name="useHex">true to use hexa in code</param>
        /// <returns>The string that presents random code</returns>
        public static string GetRandomCode(bool useShortCode = true, bool useHex = false)
        {
            var code = UtilityService.GetUUID();
            var length = 9;
            if (useShortCode)
                length = 4;

            if (!useHex)
            {
                code = UtilityService.GetRandomNumber(1000).ToString() + UtilityService.GetRandomNumber(1000).ToString();
                while (code.Length < length + 5)
                    code += UtilityService.GetRandomNumber(1000).ToString();
            }

            var index = UtilityService.GetRandomNumber(0, code.Length);
            if (index > code.Length - length)
                index = code.Length - length;
            code = code.Substring(index, length);

            var random1 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
            var replacement = "O";
            while (replacement.Equals("O"))
                replacement = ((char)UtilityService.GetRandomNumber(71, 90)).ToString();
            code = code.Replace(random1, replacement);

            if (length > 4)
            {
                var random2 = random1;
                while (random2.Equals(random1))
                    random2 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
                replacement = "O";
                while (replacement.Equals("O"))
                    replacement = ((char)UtilityService.GetRandomNumber(71, 90)).ToString();
                code = code.Replace(random2, replacement);

                var random3 = random1;
                while (random3.Equals(random1))
                {
                    random3 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
                    if (random3.Equals(random2))
                        random3 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
                }
                replacement = "O";
                while (replacement.Equals("O"))
                    replacement = ((char)UtilityService.GetRandomNumber(71, 90)).ToString();
                code = code.Replace(random3, replacement);
            }

            var hasNumber = false;
            var hasChar = false;
            for (int charIndex = 0; charIndex < code.Length; charIndex++)
            {
                if (code[charIndex] >= '0' && code[charIndex] <= '9')
                    hasNumber = true;
                if (code[charIndex] >= 'A' && code[charIndex] <= 'Z')
                    hasChar = true;
                if (hasNumber && hasChar)
                    break;
            }

            if (!hasNumber)
                code += ((char)UtilityService.GetRandomNumber(48, 57)).ToString();

            if (!hasChar)
            {
                replacement = "O";
                while (replacement.Equals("O"))
                    replacement = ((char)UtilityService.GetRandomNumber(65, 90)).ToString();
                code += replacement;
            }

            return code.Right(length);
        }
        #endregion

        #region Task/Cancellation Token extensions
        /// <summary>
        /// Executes an action in the thread pool with cancellation supported
        /// </summary>
        /// <param name="action">The action to run in the thread pool</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="creationOptions">The options that controls the behavior of the created task</param>
        /// <param name="scheduler">The scheduler that is used to schedule the created task</param>
        /// <returns>An awaitable task</returns>
        public static Task ExecuteTask(Action action, CancellationToken cancellationToken = default, TaskCreationOptions creationOptions = TaskCreationOptions.DenyChildAttach, TaskScheduler scheduler = null)
            => Task.Factory.StartNew(action, cancellationToken, creationOptions, scheduler ?? TaskScheduler.Default);

        /// <summary>
        /// Executes an action in the thread pool with cancellation supported
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function to run in the thread pool</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="creationOptions">The options that controls the behavior of the created task</param>
        /// <param name="scheduler">The scheduler that is used to schedule the created task</param>
        /// <returns>An awaitable task</returns>
        public static Task<T> ExecuteTask<T>(Func<T> func, CancellationToken cancellationToken = default, TaskCreationOptions creationOptions = TaskCreationOptions.DenyChildAttach, TaskScheduler scheduler = null)
            => Task.Factory.StartNew(func, cancellationToken, creationOptions, scheduler ?? TaskScheduler.Default);

        /// <summary>
        /// Runs a task and just forget it (or wait for completion)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="onError">The error handler</param>
        /// <param name="waitForCompletion">true to wait for completion of the task</param>
        public static void Run(this Task task, Action<Exception> onError = null, bool waitForCompletion = false)
        {
            var instance = Task.Run(async () =>
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
            });
            if (waitForCompletion)
                instance.Wait();
            else
                instance.ConfigureAwait(false);
        }

        /// <summary>
        /// Runs a task and just forget it (or wait for completion)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="waitForCompletion">true to wait for completion of the task</param>
        public static void Run(this Task task, bool waitForCompletion)
            => task.Run(null, waitForCompletion);

        /// <summary>
        /// Runs a task and just forget it (or wait for completion)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="onError">The error handler</param>
        /// <param name="waitForCompletion">true to wait for completion of the task</param>
        public static void Run(this ValueTask task, Action<Exception> onError = null, bool waitForCompletion = false)
            => task.AsTask().Run(onError, waitForCompletion);

        /// <summary>
        /// Runs a task and just forget it (or wait for completion)
        /// </summary>
        /// <param name="task"></param>
        /// <param name="waitForCompletion">true to wait for completion of the task</param>
        public static void Run(this ValueTask task, bool waitForCompletion)
            => task.Run(null, waitForCompletion);

        /// <summary>
        /// Performs an awaitable task with cancellation token supported
        /// </summary>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task WithCancellationToken(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs, false))
            {
                var result = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
                if (result != task)
                    throw new OperationCanceledException(cancellationToken);
            }
        }

        /// <summary>
        /// Performs an awaitable task with cancellation token supported
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> WithCancellationToken<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs, false))
            {
                var result = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
                return result != task
                    ? throw new OperationCanceledException(cancellationToken)
                    : task.Result;
            }
        }

        /// <summary>
        /// Writes a string to the stream asynchronously
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="string"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WriteAsync(this StreamWriter writer, string @string, CancellationToken cancellationToken)
            => writer.WriteAsync(@string).WithCancellationToken(cancellationToken);

        /// <summary>
        /// Writes a line of string to the stream asynchronously
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="string"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WriteLineAsync(this StreamWriter writer, string @string, CancellationToken cancellationToken)
            => writer.WriteLineAsync(@string).WithCancellationToken(cancellationToken);

        /// <summary>
        /// Clears all buffers for this stream asynchronously and causes any buffered data to be written to the underlying device
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task FlushAsync(this StreamWriter writer, CancellationToken cancellationToken)
            => writer.FlushAsync().WithCancellationToken(cancellationToken);

        /// <summary>
        /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> ReadToEndAsync(this StreamReader reader, CancellationToken cancellationToken)
            => reader.ReadToEndAsync().WithCancellationToken(cancellationToken);

        /// <summary>
        /// Reads a line of characters asynchronously from the current stream and returns the data as a string
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> ReadLineAsync(this StreamReader reader, CancellationToken cancellationToken)
            => reader.ReadLineAsync().WithCancellationToken(cancellationToken);
        #endregion

        #region Web/Http request extensions
        internal static string[] UserAgents { get; } = new[]
        {
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.96 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "SAMSUNG-SGH-E250/1.0 Profile/MIDP-2.0 Configuration/CLDC-1.1 UP.Browser/6.2.3.3.c.1.101 (GUI) MMP/2.0 (compatible; Googlebot-Mobile/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Google (+https://developers.google.com/+/web/snippet/)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; Bingbot/2.0; +http://www.bing.com/bingbot.htm)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; Yahoo! Slurp; +http://help.yahoo.com/help/us/ysearch/slurp)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "DuckDuckBot/1.0; (+http://duckduckgo.com/duckduckbot.html)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Sogou web spider/4.0(+http://www.sogou.com/docs/help/webmasters.htm#07)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "Mozilla/5.0 (compatible; Exabot/3.0; +http://www.exabot.com/go/robot)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
            "ia_archiver (+http://www.alexa.com/site/help/webmasters; crawler@alexa.com)",
            "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
        };

        /// <summary>
        /// Gets an user-agent as spider-bot
        /// </summary>
        public static string SpiderUserAgent => UtilityService.UserAgents[UtilityService.GetRandomNumber(0, UtilityService.UserAgents.Length - 1)];

        /// <summary>
        /// Gets an user-agent as mobile browser
        /// </summary>
        public static string MobileUserAgent => "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1";

        /// <summary>
        /// Gets an user-agent as desktop browser
        /// </summary>
        public static string DesktopUserAgent => "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36";

        /// <summary>
        /// Gets the request stream
        /// </summary>
        /// <param name="httpWebRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest httpWebRequest, CancellationToken cancellationToken)
            => httpWebRequest.GetRequestStreamAsync().WithCancellationToken(cancellationToken);

        /// <summary>
        /// Performs a web request and get response
        /// </summary>
        /// <param name="httpWebRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest httpWebRequest, CancellationToken cancellationToken)
        {
            using (cancellationToken.Register(() => httpWebRequest.Abort(), false))
            {
                try
                {
                    return await httpWebRequest.GetResponseAsync().ConfigureAwait(false) as HttpWebResponse;
                }
                catch (Exception ex)
                {
                    throw cancellationToken.IsCancellationRequested ? new OperationCanceledException(ex.Message, ex, cancellationToken) : ex;
                }
            }
        }

        /// <summary>
        /// Copies the response stream asynchronously
        /// </summary>
        /// <param name="response"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task CopyToAsync(this HttpWebResponse response, Stream stream, CancellationToken cancellationToken = default)
        {
            using (var source = response.GetResponseStream())
            {
                await source.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Copies the response stream asynchronously
        /// </summary>
        /// <param name="response"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task CopyToAsync(this HttpResponseMessage response, Stream stream, CancellationToken cancellationToken = default)
            => response.Content.CopyToAsync(stream, cancellationToken);

        /// <summary>
        /// Reads all characters from the response stream asynchronously and returns them as one string
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<string> ReadAsStringAsync(this HttpWebResponse response, CancellationToken cancellationToken = default)
        {
            using (var stream = response.GetResponseStream())
            {
                return await stream.ReadAllAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Reads all characters from the response stream asynchronously and returns them as one string
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> ReadAsStringAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
            => response.Content.ReadAsStringAsync(cancellationToken);

        /// <summary>
        /// Reads the response stream asynchronously
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> ReadAsStreamAsync(this HttpWebResponse response, CancellationToken cancellationToken = default)
            => Task.FromResult(response.GetResponseStream());

        /// <summary>
        /// Reads the response stream asynchronously
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<Stream> ReadAsStreamAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
            => response.Content.ReadAsStreamAsync(cancellationToken);

        /// <summary>
        /// Reads the response stream asynchronously
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadAsByteArrayAsync(this HttpWebResponse response, CancellationToken cancellationToken = default)
        {
            using (var stream = response.GetResponseStream())
            {
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                return buffer;
            }
        }

        /// <summary>
        /// Reads the response stream asynchronously
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> ReadAsByteArrayAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
            => response.Content.ReadAsByteArrayAsync(cancellationToken);

        /// <summary>
        /// Gets the HTTP headers
        /// </summary>
        /// <param name="response"></param>
        /// <param name="excluded"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHeaders(this HttpWebResponse response, IEnumerable<string> excluded = null, Action<Dictionary<string, string>> onCompleted = null)
            => response.Headers?.ToDictionary(headers =>
            {
                excluded?.ForEach(name => headers.Remove(name));
                onCompleted?.Invoke(headers);
            }) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the HTTP headers
        /// </summary>
        /// <param name="response"></param>
        /// <param name="excluded"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHeaders(this HttpResponseMessage response, IEnumerable<string> excluded = null, Action<Dictionary<string, string>> onCompleted = null)
        {
            var headers = response.Content?.GetHeaders() ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            response.Headers?.ToDictionary()?.ForEach(kvp => headers[kvp.Key] = kvp.Value);
            excluded?.ForEach(name => headers.Remove(name));
            onCompleted?.Invoke(headers);
            return headers;
        }

        /// <summary>
        /// Gets the HTTP headers
        /// </summary>
        /// <param name="response"></param>
        /// <param name="excluded"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetHeaders(this HttpContent response, IEnumerable<string> excluded = null, Action<Dictionary<string, string>> onCompleted = null)
            => response.Headers?.ToDictionary(headers =>
            {
                excluded?.ForEach(name => headers.Remove(name));
                onCompleted?.Invoke(headers);
            }) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the HTTP cookies
        /// </summary>
        /// <param name="httpCookies"></param>
        /// <param name="domain"></param>
        /// <param name="onAdd"></param>
        /// <returns></returns>
        public static CookieCollection GetCookies(this IEnumerable<string> httpCookies, string domain, Action<Cookie> onAdd = null)
        {
            var index = 0;
            var strCookies = (httpCookies ?? new List<string>()).ToList();
            while (index < strCookies.Count)
            {
                if (strCookies[index].IsContains("expires=") && !strCookies[index].IsContains(","))
                {
                    strCookies[index] = strCookies[index] + ", " + strCookies[index + 1];
                    strCookies.RemoveAt(index + 1);
                }
                index++;
            }
            var cookies = new CookieCollection();
            strCookies.ForEach(value =>
            {
                try
                {
                    var cookie = new Cookie();
                    var parts = value.Split(';');
                    for (index = 0; index < parts.Length; index++)
                    {
                        if (index == 0)
                        {
                            if (parts[index] != string.Empty)
                            {
                                var pos = parts[index].IndexOf("=");
                                cookie.Name = parts[index].Substring(0, pos);
                                cookie.Value = parts[index].Substring(pos + 1, parts[index].Length - (pos + 1));
                            }
                        }

                        else if (parts[index].IsContains("domain="))
                        {
                            var values = parts[index].ToList('=');
                            cookie.Domain = string.IsNullOrWhiteSpace(values[1]) ? domain : values[1];
                        }

                        else if (parts[index].IsContains("path="))
                        {
                            var values = parts[index].ToList('=');
                            cookie.Path = string.IsNullOrWhiteSpace(values[1]) ? "/" : values[1];
                        }

                        else if (parts[index].IsContains("expires="))
                            try
                            {
                                var values = parts[index].ToList('=');
                                if (!string.IsNullOrWhiteSpace(values[1]))
                                    cookie.Expires = values[1].FromHttpDateTime(true);
                            }
                            catch { }

                        else if (parts[index].IsContains("secure"))
                            cookie.Secure = true;

                        else if (parts[index].IsContains("httponly"))
                            cookie.HttpOnly = true;
                    }
                    cookie.Domain = string.IsNullOrWhiteSpace(cookie.Domain) ? domain : cookie.Domain;
                    cookie.Path = string.IsNullOrWhiteSpace(cookie.Path) ? "/" : cookie.Path;
                    onAdd?.Invoke(cookie);
                    cookies.Add(cookie);
                }
                catch { }
            });
            return cookies;
        }

        /// <summary>
        /// Gets the HTTP cookies
        /// </summary>
        /// <param name="response"></param>
        /// <param name="onAdd"></param>
        /// <returns></returns>
        public static CookieCollection GetCookies(this HttpWebResponse response, Action<Cookie> onAdd = null)
            => response.Cookies != null && response.Cookies.Count > 0
                ? response.Cookies
                : response.Headers["Set-Cookie"].ToList(",").GetCookies(response.ResponseUri.Host, onAdd);

        /// <summary>
        /// Gets the HTTP cookies
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="domain"></param>
        /// <param name="name"></param>
        /// <param name="onAdd"></param>
        /// <returns></returns>
        public static CookieCollection GetCookies(this HttpHeaders headers, string domain, string name = "Set-Cookie", Action<Cookie> onAdd = null)
            => (headers.TryGetValues(name, out var cookieValues) && cookieValues != null ? cookieValues.ToList() : new List<string>()).GetCookies(domain, onAdd);

        /// <summary>
        /// Gets the HTTP cookies
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static CookieCollection GetCookies(this HttpRequestMessage message, Action<Cookie> onAdd = null)
            => message.Headers.GetCookies(message.RequestUri.Host, "Cookie", onAdd);

        /// <summary>
        /// Gets the HTTP cookies
        /// </summary>
        /// <param name="message"></param>
        /// <param name="useContentHeaders"></param>
        /// <param name="onAdd"></param>
        /// <returns></returns>
        public static CookieCollection GetCookies(this HttpResponseMessage message, bool useContentHeaders = false, Action<Cookie> onAdd = null)
            => (useContentHeaders ? message.Content.Headers : message.Headers as HttpHeaders).GetCookies(message.RequestMessage.RequestUri.Host, "Set-Cookie", onAdd);

        /// <summary>
        /// Gets the HTTP cookies
        /// </summary>
        /// <param name="content"></param>
        /// <param name="domain"></param>
        /// <param name="onAdd"></param>
        /// <returns></returns>
        public static CookieCollection GetCookies(this HttpContent content, string domain, Action<Cookie> onAdd = null)
            => content.Headers.GetCookies(domain, "Set-Cookie", onAdd);

        /// <summary>
        /// Gets the web proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="bypass"></param>
        /// <returns></returns>
        public static WebProxy GetWebProxy(Uri uri, string username, string password, IEnumerable<string> bypass = null)
            => uri != null
                ? new WebProxy(uri, true, bypass?.ToArray(), !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) ? new CredentialCache { { uri, "Basic", new NetworkCredential(username, password) } } : null)
                : null;

        /// <summary>
        /// Gets the web proxy
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="bypass"></param>
        /// <returns></returns>
        public static WebProxy GetWebProxy(string host, int port, string username, string password, IEnumerable<string> bypass = null)
            => UtilityService.GetWebProxy(string.IsNullOrWhiteSpace(host) ? null : new Uri($"{(!host.IsStartsWith("http://") && !host.IsStartsWith("https://") ? "http://" : "")}{host}:{port}"), username, password, bypass);

        /// <summary>
        /// Gets the pre-configurated web proxy
        /// </summary>
        public static WebProxy Proxy { get; private set; }

        /// <summary>
        /// Assigns the web-proxy
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="bypass"></param>
        /// <returns></returns>
        public static WebProxy AssignWebProxy(string host, int port, string username, string password, IEnumerable<string> bypass = null)
            => UtilityService.Proxy ?? (UtilityService.Proxy = UtilityService.GetWebProxy(host, port, username, password, bypass));
        #endregion

        #region Send requests via Web/Http
        /// <summary>
        /// Sends a request to a remote end-point via HttpWebRequest
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="method">The HTTP verb to perform request</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="body">The requesting body (text or binary - array/array segment of bytes)</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="credential">The credential for marking request</param>
        /// <param name="proxy">The proxy for marking request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> SendHttpWebRequestAsync(this Uri uri, string method, Dictionary<string, string> headers, object body, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken)
        {
            if (uri == null || string.IsNullOrWhiteSpace(uri.AbsoluteUri))
                throw new InformationRequiredException("The URI to send the request to is required");

            headers = new Dictionary<string, string>(headers ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
#pragma warning disable SYSLIB0014
            var request = WebRequest.Create(uri) as HttpWebRequest;
#pragma warning restore SYSLIB0014
            request.Method = string.IsNullOrWhiteSpace(method) ? "GET" : method.ToUpper();
            request.Timeout = timeout * 1000;
            request.AllowAutoRedirect = headers.TryGetValue("AllowAutoRedirect", out var allowAutoRedirect) && "true".IsEquals(allowAutoRedirect);

            headers.Where(kvp => !kvp.Key.IsEquals("Accept-Encoding") && !kvp.Key.IsEquals("Host") && !kvp.Key.IsEquals("Connection") && !kvp.Key.IsEquals("AllowAutoRedirect") && !kvp.Key.IsEquals("Cookie")).ForEach(kvp =>
            {
                try
                {
                    request.Headers.Add(kvp.Key, kvp.Value);
                }
                catch { }
            });

            if (headers.ContainsKey("Cookie"))
                request.Headers.Add("Cookie", headers["Cookie"].ToList().Join("; "));

            if (!headers.ContainsKey("Accept"))
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            if (!headers.ContainsKey("Accept-Language"))
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9,vi;q=0.8");

            if (!headers.ContainsKey("User-Agent"))
                request.UserAgent = UtilityService.DesktopUserAgent;

#if NETSTANDARD2_0
            request.Headers.Add("Accept-Encoding", "deflate, gzip");
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
#else
            request.Headers.Add("Accept-Encoding", "deflate, gzip, br");
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;
#endif

            request.Proxy = proxy ?? UtilityService.Proxy;

            if (credential != null)
            {
                request.Credentials = credential;
                request.UseDefaultCredentials = false;
                request.PreAuthenticate = true;
            }

            if (body != null && (request.Method.Equals("POST") || request.Method.Equals("PUT") || request.Method.Equals("PATCH")))
            {
                if (body is string @string)
                {
                    if (!headers.ContainsKey("Content-Type"))
                        request.ContentType = "application/json; charset=utf-8";
                    using (var writer = new StreamWriter(await request.GetRequestStreamAsync(cancellationToken).ConfigureAwait(false)))
                        await writer.WriteAsync(@string, cancellationToken).ConfigureAwait(false);
                }
                else if (body is byte[] bytes)
                    using (var stream = await request.GetRequestStreamAsync(cancellationToken).ConfigureAwait(false))
                        await stream.WriteAsync(bytes, 0, cancellationToken).ConfigureAwait(false);
                else if (body is ArraySegment<byte> array)
                    using (var stream = await request.GetRequestStreamAsync(cancellationToken).ConfigureAwait(false))
                        await stream.WriteAsync(array, cancellationToken).ConfigureAwait(false);
                else
                    throw new InvalidRequestException("Body is invalid");
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // switch off certificate validation (http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using) - not available on macOS
                request.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

#if NETSTANDARD2_0
                // service point - only available on Windows with .NET Framework
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.FrameworkDescription.IsContains(".NET Framework"))
                    request.ServicePoint.Expect100Continue = false;
#endif
            }

            try
            {
                return await request.GetResponseAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                if (ex.Message.Contains("did not properly respond after a period of time"))
                    throw new ConnectionTimeoutException(ex.InnerException);
                else
                    throw;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse response)
                {
                    var heads = response.GetHeaders();
                    var isMoved = response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Redirect;
                    var isNotModified = response.StatusCode == HttpStatusCode.NotModified;
                    var exception = isMoved
                        ? new RemoteServerMovedException(response.StatusCode, uri, heads, $"Resource on the remote server was moved [{(heads.TryGetValue("Location", out var url) && !string.IsNullOrWhiteSpace(url) ? new Uri((url.IsContains("://") ? "" : $"{uri.Scheme}://{uri.Host}") + url) : uri)}]")
                        : new RemoteServerException(response.StatusCode, isNotModified, uri, heads, null, ex.Message, ex);
                    if (!isMoved && !isNotModified)
                        try
                        {
                            exception.Body = await response.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                            exception.Body = String.IsNullOrWhiteSpace(exception.Body) ? null : exception.Body;
                        }
                        catch { }
                    response.Dispose();
                    throw exception;
                }
                else if (ex.Status == WebExceptionStatus.Timeout)
                    throw new ConnectionTimeoutException(ex);
                else
                    throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Sends a request to a remote end-point via HttpClient
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="method">The HTTP verb to perform request</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="body">The requesting body (text or binary (array of bytes or array segment of bytes)</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="credential">The credential for marking request</param>
        /// <param name="proxy">The proxy for marking request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendHttpClientRequestAsync(this Uri uri, string method, Dictionary<string, string> headers, object body, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken)
        {
            if (uri == null || string.IsNullOrWhiteSpace(uri.AbsoluteUri))
                throw new InformationRequiredException("The URI is invalid");

            headers = new Dictionary<string, string>(headers ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
            using (var request = new HttpRequestMessage(new HttpMethod(string.IsNullOrWhiteSpace(method) ? "GET" : method.ToUpper()), uri))
            {
#if NETSTANDARD2_0
                request.Headers.Add("Accept-Encoding", "deflate, gzip");
#else
                request.Headers.Add("Accept-Encoding", "deflate, gzip, br");
#endif
                headers.Where(kvp => !kvp.Key.IsEquals("Accept-Encoding") && !kvp.Key.IsEquals("Host") && !kvp.Key.IsEquals("Connection") && !kvp.Key.IsEquals("AllowAutoRedirect") && !kvp.Key.IsEquals("Cookie")).ForEach(kvp =>
                {
                    try
                    {
                        request.Headers.Add(kvp.Key, kvp.Value.ToList());
                    }
                    catch { }
                });

                if (!headers.ContainsKey("Accept"))
                    request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");

                if (!headers.ContainsKey("Accept-Language"))
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.9,vi;q=0.8");

                if (!headers.ContainsKey("User-Agent"))
                    request.Headers.Add("User-Agent", UtilityService.DesktopUserAgent);

#if NETSTANDARD2_0
                if (body != null && (request.Method.Equals(HttpMethod.Post) || request.Method.Equals(HttpMethod.Put)))
#else
                if (body != null && (request.Method.Equals(HttpMethod.Post) || request.Method.Equals(HttpMethod.Put) || request.Method.Equals(HttpMethod.Patch)))
#endif
                {
                    if (body is string @string)
                        request.Content = new StringContent(@string);
                    else if (body is byte[] bytes)
                        request.Content = new ByteArrayContent(bytes);
                    else if (body is ArraySegment<byte> array)
                        request.Content = new ByteArrayContent(array.ToBytes());
                    else if (body is Stream stream)
                        request.Content = new StreamContent(stream);
                    else
                        throw new InvalidRequestException("Body is invalid");
                    if (!headers.TryGetValue("Content-Type", out var contenType) || string.IsNullOrWhiteSpace(contenType))
                        contenType = "application/octet-stream; charset=utf-8";
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contenType);
                }

                using (var handler = new HttpClientHandler { UseCookies = true })
                {
                    if (headers.ContainsKey("Cookie"))
                    {
                        handler.CookieContainer = new CookieContainer();
                        handler.CookieContainer.Add(new Uri($"{uri.Scheme}://{uri.Host}"), headers["Cookie"].ToList(";").Join(",").ToList().GetCookies(uri.Host));
                    }

                    if (credential != null)
                    {
                        handler.PreAuthenticate = true;
                        handler.UseDefaultCredentials = false;
                        handler.Credentials = credential;
                    }

                    proxy = proxy ?? UtilityService.Proxy;
                    if (proxy != null)
                    {
                        handler.Proxy = proxy;
                        handler.UseProxy = true;
                    }

                    handler.AllowAutoRedirect = headers.TryGetValue("AllowAutoRedirect", out var allowAutoRedirect) && "true".IsEquals(allowAutoRedirect);
                    handler.ServerCertificateCustomValidationCallback = (requestMsg, certificate, chain, sslPolicyErrors) => true;
#if NETSTANDARD2_0
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
#else
                    handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;
#endif
                    using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeout) })
                        try
                        {
                            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
                            if (!response.IsSuccessStatusCode)
                            {
                                var heads = response.GetHeaders();
                                var isMoved = response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Redirect;
                                var isNotModified = response.StatusCode == HttpStatusCode.NotModified;
                                var exception = isMoved
                                    ? new RemoteServerMovedException(response.StatusCode, uri, heads, $"Resource on the remote server was moved [{(heads.TryGetValue("Location", out var url) && !string.IsNullOrWhiteSpace(url) ? new Uri((url.IsContains("://") ? "" : $"{uri.Scheme}://{uri.Host}") + url) : uri)}]")
                                    : new RemoteServerException(response.StatusCode, isNotModified, uri, heads);
                                if (!isMoved && !isNotModified)
                                    try
                                    {
                                        exception.Body = await response.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                                        exception.Body = String.IsNullOrWhiteSpace(exception.Body) ? null : exception.Body;
                                    }
                                    catch { }
                                response.Dispose();
                                throw exception;
                            }
                            return response;
                        }
                        catch (TaskCanceledException ex)
                        {
                            if (ex.Message.IsContains("HttpClient.Timeout"))
                                throw new ConnectionTimeoutException(ex);
                            else
                                throw;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                }
            }
        }
        #endregion

        #region Send requests to remote end-point
        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="method">The HTTP verb to perform request</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="body">The requesting body (text or binary (array of bytes or array segment of bytes)</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="credential">The credential for marking request</param>
        /// <param name="proxy">The proxy for marking request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static async Task<RemoteServerResponse> SendHttpRequestAsync(this Uri uri, string method, Dictionary<string, string> headers, object body, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken = default, bool useHttpClient = true)
        {
            if (useHttpClient)
                using (var response = await uri.SendHttpClientRequestAsync(method, headers, body, timeout, credential, proxy, cancellationToken).ConfigureAwait(false))
                using (var stream = await response.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    return await RemoteServerResponse.CreateAsync(response.StatusCode, uri, response.GetHeaders(), stream).ConfigureAwait(false);
            else
                using (var response = await uri.SendHttpWebRequestAsync(method, headers, body, timeout, credential, proxy, cancellationToken).ConfigureAwait(false))
                using (var stream = await response.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                    return await RemoteServerResponse.CreateAsync(response.StatusCode, uri, response.GetHeaders(), stream).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="method">The HTTP verb to perform request</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="body">The requesting body (text or binary (array of bytes or array segment of bytes)</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="credential">The credential for marking request</param>
        /// <param name="proxy">The proxy for marking request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<RemoteServerResponse> SendHttpRequestAsync(string uri, string method, Dictionary<string, string> headers, object body, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => string.IsNullOrWhiteSpace(uri)
                ? Task.FromException<RemoteServerResponse>(new InformationInvalidException("The URI is invalid"))
                : new Uri(uri).SendHttpRequestAsync(method, headers, body, timeout, credential, proxy, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="method">The HTTP verb to perform request</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="body">The requesting body (text or binary (array of bytes or array segment of bytes)</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<RemoteServerResponse> SendHttpRequestAsync(this Uri uri, string method, Dictionary<string, string> headers, object body, int timeout, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => uri.SendHttpRequestAsync(method, headers, body, timeout, null, null, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="method">The HTTP verb to perform request</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="body">The requesting body (text or binary (array of bytes or array segment of bytes)</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<RemoteServerResponse> SendHttpRequestAsync(string uri, string method, Dictionary<string, string> headers, object body, int timeout, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => string.IsNullOrWhiteSpace(uri)
                ? Task.FromException<RemoteServerResponse>(new InformationInvalidException("The URI is invalid"))
                : new Uri(uri).SendHttpRequestAsync(method, headers, body, timeout, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<RemoteServerResponse> SendHttpRequestAsync(this Uri uri, Dictionary<string, string> headers, int timeout, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => uri.SendHttpRequestAsync("GET", headers, null, timeout, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<RemoteServerResponse> SendHttpRequestAsync(string uri, Dictionary<string, string> headers, int timeout, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => string.IsNullOrWhiteSpace(uri)
                ? Task.FromException<RemoteServerResponse>(new InformationInvalidException("The URI is invalid"))
                : new Uri(uri).SendHttpRequestAsync(headers, timeout, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<RemoteServerResponse> SendHttpRequestAsync(this Uri uri, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => uri.SendHttpRequestAsync(null, 90, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<RemoteServerResponse> SendHttpRequestAsync(string uri, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => string.IsNullOrWhiteSpace(uri)
                ? Task.FromException<RemoteServerResponse>(new InformationInvalidException("The URI is invalid"))
                : new Uri(uri).SendHttpRequestAsync(cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point to fetch and return as a string
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="credential">The credential for marking request</param>
        /// <param name="proxy">The proxy for marking request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static async Task<string> FetchHttpAsync(this Uri uri, Dictionary<string, string> headers, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken = default, bool useHttpClient = true)
        {
            using (var response = await uri.SendHttpRequestAsync("GET", headers, null, timeout, credential, proxy, cancellationToken, useHttpClient).ConfigureAwait(false))
            {
                return await response.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends a request to a remote end-point to fetch and return as a string
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="credential">The credential for marking request</param>
        /// <param name="proxy">The proxy for marking request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<string> FetchHttpAsync(string uri, Dictionary<string, string> headers, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => string.IsNullOrWhiteSpace(uri)
                ? Task.FromException<string>(new InformationInvalidException("The URI is invalid"))
                : new Uri(uri).FetchHttpAsync(headers, timeout, credential, proxy, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point to fetch and return as a string
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<string> FetchHttpAsync(this Uri uri, Dictionary<string, string> headers, int timeout, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => uri.FetchHttpAsync(headers, timeout, null, null, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point to fetch and return as a string
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="headers">The requesting headers</param>
        /// <param name="timeout">The requesting time-out</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<string> FetchHttpAsync(string uri, Dictionary<string, string> headers, int timeout, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => string.IsNullOrWhiteSpace(uri)
                ? Task.FromException<string>(new InformationInvalidException("The URI is invalid"))
                : new Uri(uri).FetchHttpAsync(headers, timeout, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point to fetch a string
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<string> FetchHttpAsync(this Uri uri, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => uri.FetchHttpAsync(null, 90, cancellationToken, useHttpClient);

        /// <summary>
        /// Sends a request to a remote end-point to fetch a string
        /// </summary>
        /// <param name="uri">The URI to perform request to</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="useHttpClient">true to use HttpClient; false to use HttpWebRequest</param>
        /// <returns></returns>
        public static Task<string> FetchHttpAsync(string uri, CancellationToken cancellationToken = default, bool useHttpClient = true)
            => string.IsNullOrWhiteSpace(uri)
                ? Task.FromException<string>(new InformationInvalidException("The URI is invalid"))
                : new Uri(uri).FetchHttpAsync(cancellationToken, useHttpClient);
        #endregion

        #region Normalize HTML/XML tags & Remove whitespaces/breaks
        /// <summary>
        /// Gets the collection of none-close tags
        /// </summary>
        public static List<string> NoneCloseTags { get; } = new List<string> { "img", "br", "hr", "input", "embed", "source" };

        static Regex AllTagsRegex { get; } = new Regex(@"<[^<>]+>", RegexOptions.IgnoreCase);

        static Regex AllAttributesRegex { get; } = new Regex(@"(\S+)=[""]?((?:.(?![""]?\s+(?:\S+)=|[>""]))+.)[""]?", RegexOptions.IgnoreCase);

        /// <summary>
        /// Finds all available tags of XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static List<XTag> FindTags(this string @string)
        {
            // extract all tags
            var tags = new List<XTag>();
            foreach (Match tagMatch in UtilityService.AllTagsRegex.Matches(@string ?? ""))
            {
                var full = tagMatch.Value;
                var isOpen = tagMatch.Value[1] != '/';
                var isClose = tagMatch.Value[1] == '/' || tagMatch.Value[tagMatch.Value.Length - 2] == '/';
                var spacePos = tagMatch.Value.IndexOf(" ");
                var name = isClose
                    ? isOpen ? spacePos > 0 ? tagMatch.Value.Substring(1, spacePos - 1) : tagMatch.Value.Substring(1, tagMatch.Value.Length - 3) : tagMatch.Value.Substring(2, tagMatch.Value.Length - 3)
                    : spacePos > 0 ? tagMatch.Value.Substring(1, spacePos - 1) : tagMatch.Value.Substring(1, tagMatch.Value.Length - 2);

                var attributes = new List<XTagAttribute>();
                foreach (Match attributeMatch in UtilityService.AllAttributesRegex.Matches(tagMatch.Value))
                {
                    var start = attributeMatch.Value.IndexOf("=");
                    var end = attributeMatch.Value.IndexOf(attributeMatch.Value[start + 1], start + 2);
                    attributes.Add(new XTagAttribute
                    {
                        Full = attributeMatch.Value.Substring(0, end + 1),
                        Name = attributeMatch.Value.Substring(0, start),
                        Value = start > 0 && end > 0 ? attributeMatch.Value.Substring(start + 2, (end > 0 ? end : attributeMatch.Value.Length) - start - 2).Trim() : ""
                    });
                }

                tags.Add(new XTag
                {
                    Full = tagMatch.Value,
                    Name = name,
                    IsOpen = isOpen,
                    IsClose = isClose,
                    Attributes = attributes
                });
            }

            // prepare positions
            var position = -1;
            tags.ForEach(tag =>
            {
                position = @string.IndexOf(tag.Full, position + 1);
                tag.StartPosition = position;
                tag.EndPosition = position + tag.Full.Length;
            });

            // normalize non-close tags
            tags.Where(tag => !tag.IsClose && UtilityService.NoneCloseTags.IndexOf(tag.Name) > -1).ForEach(tag => tag.IsClose = true);

            // prepare reelevant tags
            tags.ForEach((current, index) =>
            {
                if (current.RelevantIndex > -1)
                    return;

                if (current.IsClose)
                {
                    if (current.IsOpen)
                        current.RelevantIndex = index;
                    return;
                }

                var next = index + 1 < tags.Count ? tags[index + 1] : null;
                if (next == null)
                    return;

                if (next.IsOpen)
                {
                    var nextOfNext = index + 2 < tags.Count ? tags[index + 2] : null;
                    if (nextOfNext != null && nextOfNext.IsClose && nextOfNext.RelevantIndex < 0 && nextOfNext.Name == next.Name)
                    {
                        next.RelevantIndex = index + 2;
                        nextOfNext.RelevantIndex = index + 1;
                    }
                }
                else if (next.RelevantIndex < 0 && next.Name == current.Name)
                {
                    current.RelevantIndex = index + 1;
                    next.RelevantIndex = index;
                }

                if (current.RelevantIndex < 0)
                {
                    var idx = tags.FindIndex(index + 1, tag => current.Name == tag.Name && tag.IsClose && tag.RelevantIndex < 0);
                    if (idx > 0)
                    {
                        current.RelevantIndex = idx;
                        tags[idx].RelevantIndex = index;
                    }
                }
            });

            // prepare outer/inner & next/children
            tags.Where(tag => tag.IsOpen).ForEach(tag =>
            {
                tag.Outer = tag.IsClose ? tag.Full : @string.Substring(tag.StartPosition, tag.RelevantIndex > 0 && tags[tag.RelevantIndex].EndPosition > 0 ? tags[tag.RelevantIndex].EndPosition - tag.StartPosition : @string.Length - tag.StartPosition);
                tag.Inner = tag.IsClose ? null : tag.Outer?.Substring(tag.Full.Length, tag.Outer.Length - tag.Full.Length - tag.Name.Length - 3);
                tag.Next = tag.RelevantIndex > 0 && tag.RelevantIndex + 1 < tags.Count && tags[tag.RelevantIndex + 1].IsOpen ? tags[tag.RelevantIndex + 1] : null;
                tag.Children = tag.RelevantIndex > 0 ? tags.Skip(tags[tag.RelevantIndex].RelevantIndex + 1).Take(tag.RelevantIndex - tags[tag.RelevantIndex].RelevantIndex - 1).Where(xtag => xtag.IsOpen).ToList() : null;
            });

            return tags;
        }

        /// <summary>
        /// Removes tags from XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <param name="beRemovedTags"></param>
        /// <param name="removeInners"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static string RemoveTags(this string @string, IEnumerable<string> beRemovedTags = null, bool removeInners = false, Func<XTag, bool> predicate = null)
        {
            if (!string.IsNullOrWhiteSpace(@string))
            {
                @string.FindTags()
                    .Where(tag => (beRemovedTags == null || beRemovedTags.FirstOrDefault(stag => tag.Name.IsEquals(stag)) != null) && (!removeInners || tag.IsOpen && (predicate == null || predicate(tag))))
                    .ForEach(tag => @string = @string.Replace(StringComparison.OrdinalIgnoreCase, removeInners ? tag.Outer : tag.Full, ""));
                @string = @string.Trim();
            }
            return @string;
        }

        /// <summary>
        /// Removes HTML/XML tags
        /// </summary>
        /// <param name="string"></param>
        /// <param name="tag"></param>
        /// <param name="removeInner"></param>
        /// <returns></returns>
        public static string RemoveTag(this string @string, string tag, bool removeInner = false)
            => UtilityService.RemoveTags(@string, string.IsNullOrWhiteSpace(tag) ? null : new[] { tag }, removeInner);

        /// <summary>
        /// Removes tags of Microsoft Office from XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string RemoveMsOfficeTags(this string @string, IEnumerable<string> tags = null)
            => UtilityService.RemoveTags(@string, tags ?? "w:|o:|v:|m:|st1:|st2:|st3:|st4:|st5:".Split('|'));

        /// <summary>
        /// Gets the default predicate function for removing tag attributes
        /// </summary>
        public static Func<XTag, XTagAttribute, bool> RemoveTagAttributesPredicate { get; } = (tag, attribute) =>
        {
            if (tag.Name.IsEquals("a"))
                return !attribute.Name.IsEquals("href") && !attribute.Name.IsEquals("target") && !attribute.Name.IsEquals("rel");
            if (tag.Name.IsEquals("img") || tag.Name.IsEquals("source"))
                return !attribute.Name.IsEquals("src") && !attribute.Name.IsEquals("srcset") && !attribute.Name.IsEquals("alt") && !attribute.Name.IsEquals("type");
            if (tag.Name.IsEquals("video") || tag.Name.IsEquals("audio"))
                return !attribute.Name.IsEquals("controls") && !attribute.Name.IsEquals("poster") && !attribute.Name.IsEquals("autoplay") && !attribute.Name.IsEquals("muted");
            if (tag.Name.IsEquals("iframe"))
                return !attribute.Name.IsEquals("src") && !attribute.Name.IsEquals("width") && !attribute.Name.IsEquals("height") && !attribute.Name.IsEquals("allowfullscreen") && !attribute.Name.IsContains("-id");
            return attribute.Name.ToLower() != "id";
        };

        /// <summary>
        /// Removes attributes from XHTML/XML tags
        /// </summary>
        /// <param name="string"></param>
        /// <param name="firstPredicate"></param>
        /// <param name="secondPredicate"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string RemoveTagAttributes(this string @string, Func<XTag, XTagAttribute, bool> firstPredicate = null, Func<XTag, XTagAttribute, bool> secondPredicate = null, IEnumerable<string> tags = null)
        {
            if (!string.IsNullOrWhiteSpace(@string))
            {
                @string = @string.HtmlDecode();
                @string.FindTags().Where(tag => tag.IsOpen && tag.Attributes.Any() && (tags == null || tags.FirstOrDefault(stag => tag.Name.IsEquals(stag)) != null)).ForEach(tag =>
                {
                    var attributes = tag.Attributes.Select(attribute => new XTagAttribute { Name = attribute.Name, Value = attribute.Value }).ToList();
                    var beRemoved = new List<string>();
                    tag.Attributes.Where(attribute => firstPredicate != null ? firstPredicate(tag, attribute) : UtilityService.RemoveTagAttributesPredicate(tag, attribute))
                        .Where(attribute => secondPredicate == null || secondPredicate(tag, attribute))
                        .ForEach(attribute => beRemoved.Add(attribute.Name));
                    beRemoved.ForEach(name => tag.Attributes.RemoveAt(tag.Attributes.FindIndex(attribute => attribute.Name.IsEquals(name))));
                    if (tag.Attributes.Count != attributes.Count || tag.Attributes.Any(attribute => attribute.Value != attributes.First(attr => attr.Name.IsEquals(attribute.Name)).Value))
                        @string = @string.Replace(StringComparison.OrdinalIgnoreCase, tag.Full, tag.ToString());
                });
                @string = @string.Replace(StringComparison.OrdinalIgnoreCase, "allowfullscreen=\"\"", "allowfullscreen");
                @string = @string.Replace(StringComparison.OrdinalIgnoreCase, "autoplay=\"\"", "autoplay");
                @string = @string.Replace(StringComparison.OrdinalIgnoreCase, "muted=\"\"", "muted");
                @string = @string.Trim();
            }
            return @string;
        }

        /// <summary>
        /// Removes attributes from a tag of HTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string RemoveTagAttributes(this string @string, IEnumerable<string> tags)
            => UtilityService.RemoveTagAttributes(@string, null, null, tags);

        /// <summary>
        /// Removes attributes from a tag of HTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string RemoveTagAttributes(this string @string, string tag)
            => UtilityService.RemoveTagAttributes(@string, string.IsNullOrWhiteSpace(tag) ? null : new[] { tag });

        /// <summary>
        /// Removes comments form XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string RemoveComments(this string @string)
        {
            if (!string.IsNullOrWhiteSpace(@string))
            {
                var start = @string.PositionOf("<!--");
                while (start > -1)
                {
                    var end = @string.PositionOf("-->", start);
                    if (end > 0)
                        @string = @string.Remove(start, end - start + 3);
                    start = @string.PositionOf("<!--", start + 1);
                }
                @string = @string.Trim();
            }
            return @string;
        }

        /// <summary>
        /// Removes comments from XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string ClearComments(this string @string)
            => UtilityService.RemoveComments(@string);

        /// <summary>
        /// Clears HTML/XML tags (with inner)
        /// </summary>
        /// <param name="string"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string ClearTag(this string @string, string tag)
            => UtilityService.RemoveTags(@string, string.IsNullOrWhiteSpace(tag) ? null : new[] { tag }, true);

        /// <summary>
        /// Cleans format from XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <param name="firstPredicate"></param>
        /// <param name="secondPredicate"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string ClearFormat(this string @string, Func<XTag, XTagAttribute, bool> firstPredicate = null, Func<XTag, XTagAttribute, bool> secondPredicate = null, IEnumerable<string> tags = null)
            => UtilityService.RemoveTagAttributes(@string, firstPredicate, secondPredicate, tags);

        /// <summary>
        /// Gets regular expressions for cleaning whitespace and breaks
        /// </summary>
        public static List<Tuple<Regex, string>> WhitespacesAndBreaksRegexs { get; } = new List<Tuple<Regex, string>>
        {
			// line-breaks
			new Tuple<Regex, string>(new Regex(@">\s+\n<", RegexOptions.IgnoreCase), "> <"),
            new Tuple<Regex, string>(new Regex(@">\n<", RegexOptions.IgnoreCase), "><"),

			// white-spaces
			new Tuple<Regex, string>(new Regex(@"\s+/>", RegexOptions.IgnoreCase), "/>"),
            new Tuple<Regex, string>(new Regex(@"/>\s+<", RegexOptions.IgnoreCase), "/><"),
            new Tuple<Regex, string>(new Regex(@">\s+<", RegexOptions.IgnoreCase), "> <"),
            new Tuple<Regex, string>(new Regex(@"""\s+>", RegexOptions.IgnoreCase), "\">"),
            new Tuple<Regex, string>(new Regex(@"'\s+>", RegexOptions.IgnoreCase), "'>"),
            new Tuple<Regex, string>(new Regex(@"\s+"">", RegexOptions.IgnoreCase), "\">"),
            new Tuple<Regex, string>(new Regex(@"\s+'>", RegexOptions.IgnoreCase), "'>"),
            new Tuple<Regex, string>(new Regex(@"""\s+/>", RegexOptions.IgnoreCase), "\"/>"),
            new Tuple<Regex, string>(new Regex(@"'\s+/>", RegexOptions.IgnoreCase), "'/>"),
            new Tuple<Regex, string>(new Regex(@"\s+""/>", RegexOptions.IgnoreCase), "\"/>"),
            new Tuple<Regex, string>(new Regex(@"\s+'/>", RegexOptions.IgnoreCase), "'/>")
        };

        internal static List<Tuple<Regex, string>> _WhitespacesAndBreaksExtendedRegexs = null;

        /// <summary>
        /// Gets extended regular expressions for cleaning whitespace and breaks
        /// </summary>
        public static List<Tuple<Regex, string>> WhitespacesAndBreaksExtendedRegexs
        {
            get
            {
                if (UtilityService._WhitespacesAndBreaksExtendedRegexs == null)
                {
                    // white-spaces before/after special tags
                    UtilityService._WhitespacesAndBreaksExtendedRegexs = new List<Tuple<Regex, string>>();
                    "div,/div,section,/section,nav,/nav,main,/main,header,/header,footer,/footer,p,/p,h1,h2,h3,h4,h5,br,hr,input,textarea,table,tr,/tr,td,ul,/ul,li,select,/select,option,script,/script".ToArray().ForEach(tag =>
                    {
                        if (!tag[0].Equals('/'))
                            UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@">\s+<" + tag, RegexOptions.IgnoreCase), "><" + tag));
                        else
                        {
                            UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@">\s+<" + tag + @">\s+<", RegexOptions.IgnoreCase), "><" + tag + "><"));
                            UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@">\s+<" + tag + @">", RegexOptions.IgnoreCase), "><" + tag + ">"));
                            UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@"<" + tag + @">\s+<", RegexOptions.IgnoreCase), "<" + tag + "><"));
                        }
                    });
                }
                return UtilityService._WhitespacesAndBreaksExtendedRegexs;
            }
        }

        /// <summary>
        /// Removes whitespaces and breaks from HTML code
        /// </summary>
        /// <param name="string">The HTML code</param>
        /// <returns></returns>
        public static string RemoveWhitespaces(this string @string)
        {
            if (!string.IsNullOrWhiteSpace(@string))
            {
                @string = @string.Replace("\r", "").Replace(">\n\t", ">").Replace("\n\t", " ").Replace("\n", "").Replace("\t", "").Trim();
                UtilityService.WhitespacesAndBreaksRegexs.Concat(UtilityService.WhitespacesAndBreaksExtendedRegexs).ForEach(regex => @string = regex.Item1.Replace(@string, regex.Item2));
                @string = @string.Replace("> <", "><").Replace("  </", "</").Replace(" </", "</").Replace("\"> ", "\">").Replace("'> ", "'>");
            }
            return @string;
        }

        /// <summary>
        /// Removes whitespaces and breaks from HTML code
        /// </summary>
        /// <param name="string">The HTML code</param>
        /// <returns></returns>
        public static string RemoveHTMLWhitespaces(this string @string)
            => UtilityService.RemoveWhitespaces(@string);

        /// <summary>
        /// Normalizes breaks (BR) of XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <param name="noBreakBetweenTags"></param>
        /// <returns></returns>
        public static string NormalizeBreaks(this string @string, bool noBreakBetweenTags = true)
        {
            if (!string.IsNullOrWhiteSpace(@string))
            {
                @string = @string.Replace("\t", "").Replace("\r", "").Replace("\n", "<br/>");
                UtilityService.WhitespacesAndBreaksRegexs.ForEach(regex => @string = regex.Item1.Replace(@string, regex.Item2));
                if (noBreakBetweenTags)
                    @string = @string.Replace("><br/><", "><");
            }
            return @string;
        }

        /// <summary>
        /// Normalizes breaks (BR) of XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <param name="noBreakBetweenTags"></param>
        /// <returns></returns>
        public static string NormalizeHTMLBreaks(this string @string, bool noBreakBetweenTags = true)
            => UtilityService.NormalizeBreaks(@string, noBreakBetweenTags);

        /// <summary>
        /// Normalizes all none-close tags (change to self-close) of XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string NormalizeNoneCloseTags(this string @string)
        {
            if (!string.IsNullOrWhiteSpace(@string))
                UtilityService.NoneCloseTags.ForEach(tag =>
                {
                    var start = @string.PositionOf($"<{tag}");
                    while (start > -1)
                    {
                        var end = @string.IndexOf(">", start);
                        if (end > start && @string[end - 1] != '/')
                            @string = @string.Substring(0, end) + "/" + @string.Substring(end);
                        start = @string.PositionOf($"<{tag}", start + 1);
                    }
                });
            return @string;
        }

        /// <summary>
        /// Normalizes all none-close tags (change to self-close) of XHTML/XML
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static string NormalizeHTMLNoneCloseTags(this string @string)
            => UtilityService.NormalizeNoneCloseTags(@string);
        #endregion

        #region Working with files & folders
        readonly static List<string> _FileRemovements = new List<string> { "\\", "/", "*", "?", "<", ">", "|", ":", "\r", "\n", "\t" };
        readonly static List<string[]> _FileReplacements = new List<string[]> { new[] { "\"", "'" }, new[] { "%20", " " }, new[] { " ft. ", " & " } };

        /// <summary>
        /// Normalizes the name of a file
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetNormalizedFilename(this string input)
        {
            var output = input.ConvertCompositeUnicodeToUnicode();
            UtilityService._FileRemovements.ForEach(str => output = output.Replace(str, "").Trim());
            UtilityService._FileReplacements.ForEach(replacement => output = output.Replace(replacement[0], replacement[1]).Trim());
            if (output.IsStartsWith("con."))
                while (output.IndexOf(".") > -1)
                    output = output.Replace(".", "");
            return output;
        }

        /// <summary>
        /// Gets size of a file in the friendly text
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static string GetFileSize(this FileInfo fileInfo)
            => fileInfo == null || !fileInfo.Exists
                ? null
                : fileInfo.Length >= (1024 * 1024 * 1024)
                    ? (fileInfo.Length.CastAs<double>() / (1024 * 1024 * 1024)).ToString("##0.##") + " G"
                    : fileInfo.Length >= (1024 * 1024)
                        ? (fileInfo.Length.CastAs<double>() / (1024 * 1024)).ToString("##0.##") + " M"
                        : fileInfo.Length >= 1024
                            ? (fileInfo.Length.CastAs<double>() / 1024).ToString("##0.##") + " K"
                            : fileInfo.Length.ToString("###0") + " B";

        /// <summary>
        /// Gets size of a file in the friendly text
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileSize(string filePath)
            => string.IsNullOrWhiteSpace(filePath) ? null : UtilityService.GetFileSize(new FileInfo(filePath));

        /// <summary>
        /// Searchs and gets listing of files by searching pattern
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPatterns"></param>
        /// <param name="searchInSubFolder"></param>
        /// <param name="excludedSubFolders"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderMode"></param>
        /// <returns></returns>
        public static List<FileInfo> GetFiles(string path, string searchPatterns = null, bool searchInSubFolder = false, List<string> excludedSubFolders = null, string orderBy = "Name", string orderMode = "Ascending")
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"The folder is not found [{path}]");

            var files = new List<FileInfo>();
            var searchingPatterns = string.IsNullOrWhiteSpace(searchPatterns)
                ? new[] { "*.*" }
                : searchPatterns.ToArray('|', true);

            searchingPatterns.ForEach(searchingPattern =>
            {
                var results = Directory.GetFiles(path, searchingPattern).Select(filePath => new FileInfo(filePath));
                if (!string.IsNullOrWhiteSpace(orderBy) && (orderBy.IsStartsWith("Name") || orderBy.IsStartsWith("LastWriteTime")))
                    results = !string.IsNullOrWhiteSpace(orderMode) && orderMode.IsStartsWith("Asc")
                        ? orderBy.IsStartsWith("Name")
                            ? results.OrderBy(file => file.Name).ThenByDescending(file => file.LastWriteTime)
                            : results.OrderBy(file => file.LastWriteTime).ThenBy(file => file.Name)
                        : orderBy.IsStartsWith("Name")
                            ? results.OrderByDescending(file => file.Name).ThenByDescending(file => file.LastWriteTime)
                            : results.OrderByDescending(file => file.LastWriteTime).ThenBy(file => file.Name);
                files = files.Concat(results).ToList();
            });

            if (searchInSubFolder)
                Directory.GetDirectories(path).Where(folderPath =>
                {
                    var isExcluded = false;
                    if (excludedSubFolders != null && excludedSubFolders.Count > 0)
                        foreach (var excludedFolder in excludedSubFolders)
                        {
                            isExcluded = folderPath.IsEndsWith(Path.DirectorySeparatorChar.ToString() + excludedFolder);
                            if (isExcluded)
                                break;
                        }
                    return !isExcluded;
                }).ForEach(folderPath => searchingPatterns.ForEach(searchingPattern =>
                {
                    var results = Directory.GetFiles(folderPath, searchingPattern).Select(filePath => new FileInfo(filePath));
                    if (!string.IsNullOrWhiteSpace(orderBy) && (orderBy.IsStartsWith("Name") || orderBy.IsStartsWith("LastWriteTime")))
                        results = !string.IsNullOrWhiteSpace(orderMode) && orderMode.IsStartsWith("Asc")
                            ? orderBy.IsStartsWith("Name")
                                ? results.OrderBy(file => file.Name).ThenByDescending(file => file.LastWriteTime)
                                : results.OrderBy(file => file.LastWriteTime).ThenBy(file => file.Name)
                            : orderBy.IsStartsWith("Name")
                                ? results.OrderByDescending(file => file.Name).ThenByDescending(file => file.LastWriteTime)
                                : results.OrderByDescending(file => file.LastWriteTime).ThenBy(file => file.Name);
                    files = files.Concat(results).ToList();
                }));

            return files;
        }

        /// <summary>
        /// Searchs and gets listing of files by searching pattern
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPatterns"></param>
        /// <param name="searchInSubFolder"></param>
        /// <param name="excludedSubFolders"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderMode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<List<FileInfo>> GetFilesAsync(string path, string searchPatterns = null, bool searchInSubFolder = false, List<string> excludedSubFolders = null, string orderBy = "Name", string orderMode = "Ascending", CancellationToken cancellationToken = default)
            => UtilityService.ExecuteTask(() => UtilityService.GetFiles(path, searchPatterns, searchInSubFolder, excludedSubFolders, orderBy, orderMode), cancellationToken);

        /// <summary>
        /// Searchs and gets the listing of file paths by searching pattern
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPatterns"></param>
        /// <param name="searchInSubFolder"></param>
        /// <param name="excludedSubFolders"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderMode"></param>
        /// <returns></returns>
        public static List<string> GetFilePaths(string path, string searchPatterns = null, bool searchInSubFolder = false, List<string> excludedSubFolders = null, string orderBy = "Name", string orderMode = "Ascending")
            => UtilityService.GetFiles(path, searchPatterns, searchInSubFolder, excludedSubFolders, orderBy, orderMode)
                .Select(file => file.FullName)
                .ToList();

        /// <summary>
        /// Searchs and gets the listing of file paths by searching pattern
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPatterns"></param>
        /// <param name="searchInSubFolder"></param>
        /// <param name="excludedSubFolders"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderMode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<List<string>> GetFilePathsAsync(string path, string searchPatterns = null, bool searchInSubFolder = false, List<string> excludedSubFolders = null, string orderBy = "Name", string orderMode = "Ascending", CancellationToken cancellationToken = default)
            => UtilityService.ExecuteTask(() => UtilityService.GetFilePaths(path, searchPatterns, searchInSubFolder, excludedSubFolders, orderBy, orderMode), cancellationToken);

        /// <summary>
        /// Gets path to a file/folder with 'right' path separator on each OS Platform
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string GetPath(params string[] paths)
            => paths == null || paths.Length < 1
                ? null
                : Path.Combine(paths);

        /// <summary>
        /// Moves file (searched by patterns) of a folder to other folders
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="searchPatterns"></param>
        /// <param name="deleteOldFilesBeforeMoving"></param>
        public static void MoveFiles(string source, string destination, string searchPatterns, bool deleteOldFilesBeforeMoving = false)
        {
            if (!Directory.Exists(source) || !Directory.Exists(destination))
                throw new InformationInvalidException("The paths are invalid");
            else if (source.IsEquals(destination))
                return;

            UtilityService.GetFiles(source, searchPatterns).ForEach(file =>
            {
                var path = Path.Combine(destination, file.Name);
                if (deleteOldFilesBeforeMoving && File.Exists(path))
                    File.Delete(path);
                File.Move(file.FullName, path);
            });
        }

        /// <summary>
        /// Moves file (searched by patterns) of a folder to other folders
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="searchPatterns"></param>
        /// <param name="deleteOldFilesBeforeMoving"></param>
        public static Task MoveFilesAsync(string source, string destination, string searchPatterns, bool deleteOldFilesBeforeMoving = false, CancellationToken cancellationToken = default)
            => UtilityService.ExecuteTask(() => UtilityService.MoveFiles(source, destination, searchPatterns, deleteOldFilesBeforeMoving), cancellationToken);
        #endregion

        #region Working with stream & recyclable memory stream
        static RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; } = new RecyclableMemoryStreamManager();

        /// <summary>
        /// Gets the recyclable memory stream manager (with RecyclableMemoryStreamManager class to limit LOH fragmentation and improve performance)
        /// </summary>
        /// <returns></returns>
        public static RecyclableMemoryStreamManager GetRecyclableMemoryStreamManager()
            => UtilityService.RecyclableMemoryStreamManager;

        /// <summary>
        /// Gets the recyclable memory stream manager
        /// </summary>
        /// <param name="blockSize"></param>
        /// <param name="largeBufferMultiple"></param>
        /// <param name="maximumBufferSize"></param>
        /// <returns></returns>
        public static RecyclableMemoryStreamManager GetRecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            => new RecyclableMemoryStreamManager(blockSize, largeBufferMultiple, maximumBufferSize);

        /// <summary>
        /// Creates an instance of <see cref="MemoryStream">MemoryStream</see> using RecyclableMemoryStream to limit LOH fragmentation and improve performance
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static MemoryStream CreateMemoryStream(byte[] buffer = null, int index = 0, int count = 0)
        {
            MemoryStream stream;
            try
            {
                stream = UtilityService.RecyclableMemoryStreamManager.GetStream();
            }
            catch
            {
                stream = new MemoryStream();
            }
            if (buffer != null && buffer.Any())
            {
                index = index > -1 && index < buffer.Length ? index : 0;
                count = count > 0 && count < buffer.Length - index ? count : buffer.Length - index;
                stream.Write(buffer, index, count);
                stream.Seek(0, SeekOrigin.Begin);
            }
            return stream;
        }

        /// <summary>
        /// Converts this array of bytes to memory stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static MemoryStream ToMemoryStream(this byte[] buffer, int index = 0, int count = 0)
            => UtilityService.CreateMemoryStream(buffer, index, count);

        /// <summary>
        /// Converts this array segment of bytes to memory stream
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static MemoryStream ToMemoryStream(this ArraySegment<byte> buffer)
            => UtilityService.CreateMemoryStream(buffer.Array, buffer.Offset, buffer.Count);

        /// <summary>
        /// Reads this stream and converts to memory stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static MemoryStream ToMemoryStream(this Stream stream, Action<MemoryStream> onCompleted = null)
        {
            var memoryStream = UtilityService.CreateMemoryStream();
            var buffer = new byte[4096];
            var read = stream.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                memoryStream.Write(buffer, 0, read);
                read = stream.Read(buffer, 0, buffer.Length);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            onCompleted?.Invoke(memoryStream);
            return memoryStream;
        }

        /// <summary>
        /// Reads this stream and converts to memory stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="onCompleted"></param>
        /// <returns></returns>
        public static async Task<MemoryStream> ToMemoryStreamAsync(this Stream stream, CancellationToken cancellationToken = default, Action<MemoryStream> onCompleted = null)
        {
            var memoryStream = UtilityService.CreateMemoryStream();
            var buffer = new byte[4096];
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            while (read > 0)
            {
                await memoryStream.WriteAsync(buffer, read, cancellationToken).ConfigureAwait(false);
                read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            onCompleted?.Invoke(memoryStream);
            return memoryStream;
        }

        /// <summary>
        /// Converts this memory stream to array segment of byte
        /// </summary>
        /// <param name="stream"></param>
        /// <remarks>
        /// Try to get buffer first to avoid calling ToArray on the MemoryStream because it allocates a new byte array on the heap.
        /// Avoid this by attempting to access the internal memory stream buffer, this works with supported streams like the recyclable memory stream and writable memory streams
        /// </remarks>
        /// <returns></returns>
        public static ArraySegment<byte> ToArraySegment(this MemoryStream stream)
            => stream.TryGetBuffer(out var buffer)
                ? buffer
                : new ArraySegment<byte>(stream.ToArray());

        /// <summary>
        /// Converts this memory stream to array of bytes
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this MemoryStream stream)
        {
            if (stream.TryGetBuffer(out var buffer))
            {
                var array = new byte[buffer.Count];
                Buffer.BlockCopy(buffer.Array, buffer.Offset, array, 0, buffer.Count);
                return array;
            }
            return stream.ToArray();
        }

        /// <summary>
        /// Writes the array segment of bytes to this stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        public static void Write(this Stream stream, ArraySegment<byte> buffer)
            => stream.Write(buffer.Array, buffer.Offset, buffer.Count);

        /// <summary>
        /// Writes the array segment of bytes to this stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WriteAsync(this Stream stream, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
#if NETSTANDARD2_0
            => stream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken);
#else
            => stream.WriteAsync(buffer.AsMemory(), cancellationToken).AsTask();
#endif

        /// <summary>
        /// Reads all characters from the stream asynchronously and returns them as one string
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="leaveOpen"></param>
        /// <returns></returns>
        public static async Task<string> ReadAllAsync(this Stream stream, CancellationToken cancellationToken = default, bool leaveOpen = false)
        {
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);
            using (var reader = leaveOpen ? new StreamReader(stream, null, true, -1, true) : new StreamReader(stream, true))
            {
                return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            }
        }

#if NETSTANDARD2_0
        public static Task CopyToAsync(this Stream source, Stream destinaion, CancellationToken cancellationToken)
            => source.CopyToAsync(destinaion).WithCancellationToken(cancellationToken);

        public static Task CopyToAsync(this HttpContent httpContent, Stream stream, CancellationToken cancellationToken)
            => httpContent.CopyToAsync(stream).WithCancellationToken(cancellationToken);

        public static Task<Stream> ReadAsStreamAsync(this HttpContent httpContent, CancellationToken cancellationToken)
            => httpContent.ReadAsStreamAsync().WithCancellationToken(cancellationToken);

        public static Task<byte[]> ReadAsByteArrayAsync(this HttpContent httpContent, CancellationToken cancellationToken)
            => httpContent.ReadAsByteArrayAsync().WithCancellationToken(cancellationToken);

        public static Task<string> ReadAsStringAsync(this HttpContent httpContent, CancellationToken cancellationToken)
            => httpContent.ReadAsStringAsync().WithCancellationToken(cancellationToken);

        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken)
            => stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        public static Task WriteAsync(this Stream stream, byte[] buffer, int count = 0, CancellationToken cancellationToken = default)
            => stream.WriteAsync(buffer, 0, count > 0 ? count : buffer.Length, cancellationToken);
#else
        public static Task WriteAsync(this Stream stream, byte[] buffer, int count = 0, CancellationToken cancellationToken = default)
            => stream.WriteAsync(buffer.AsMemory(0, count > 0 ? count : buffer.Length), cancellationToken).AsTask();
#endif
        #endregion

        #region Read/Write text files
        /// <summary>
        /// Reads a text file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadTextFile(FileInfo fileInfo, Encoding encoding = null)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new FileNotFoundException($"The file is not found [{(fileInfo == null ? nameof(fileInfo) : fileInfo.FullName)}]");

            using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, false))
            using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
                return reader.ReadToEnd();
        }

        /// <summary>
        /// Reads a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string ReadTextFile(string filePath, Encoding encoding = null)
            => !string.IsNullOrWhiteSpace(filePath)
                ? UtilityService.ReadTextFile(new FileInfo(filePath), encoding)
                : throw new ArgumentException("File path is invalid", nameof(filePath));

        /// <summary>
        /// Reads a text file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task<string> ReadTextFileAsync(FileInfo fileInfo, Encoding encoding = null, CancellationToken cancellationToken = default)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new FileNotFoundException($"The file is not found [{(fileInfo == null ? nameof(fileInfo) : fileInfo.FullName)}]");

            using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
            using (var reader = encoding != null ? new StreamReader(stream, encoding) : new StreamReader(stream, true))
                return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Task<string> ReadTextFileAsync(string filePath, Encoding encoding = null, CancellationToken cancellationToken = default)
            => !string.IsNullOrWhiteSpace(filePath)
                ? UtilityService.ReadTextFileAsync(new FileInfo(filePath), encoding, cancellationToken)
                : Task.FromException<string>(new ArgumentException("File path is invalid", nameof(filePath)));

        /// <summary>
        /// Writes a text file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public static void WriteTextFile(FileInfo fileInfo, string content, bool append = false, Encoding encoding = null)
        {
            if (fileInfo == null)
                throw new ArgumentException("File info is invalid", nameof(fileInfo));

            if (content == null)
                return;

            using (var stream = new FileStream(fileInfo.FullName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, false))
            using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
                writer.Write(content);
        }

        /// <summary>
        /// Writes a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public static void WriteTextFile(string filePath, string content, bool append = false, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is invalid", nameof(filePath));
            UtilityService.WriteTextFile(new FileInfo(filePath), content, append, encoding);
        }

        /// <summary>
        /// Writes a text file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static async Task WriteTextFileAsync(FileInfo fileInfo, string content, bool append = false, Encoding encoding = null, CancellationToken cancellationToken = default)
        {
            if (fileInfo == null)
                throw new ArgumentException("File info is invalid", nameof(fileInfo));

            else if (content == null)
                return;

            using (var stream = new FileStream(fileInfo.FullName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
            using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
            {
                await writer.WriteAsync(content, cancellationToken).ConfigureAwait(false);
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Task WriteTextFileAsync(string filePath, string content, bool append = false, Encoding encoding = null, CancellationToken cancellationToken = default)
            => !string.IsNullOrWhiteSpace(filePath)
                ? UtilityService.WriteTextFileAsync(new FileInfo(filePath), content, append, encoding, cancellationToken)
                : Task.FromException<string>(new ArgumentException("File path is invalid", nameof(filePath)));
        #endregion

        #region Read/Write text files (multiple lines)
        /// <summary>
        /// Reads the multiple lines of a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="position"></param>
        /// <param name="totalOfLines"></param>
        public static Tuple<List<string>, long> ReadTextFile(string filePath, long position, int totalOfLines)
        {
            using (var reader = new TextFileReader(filePath))
            {
                reader.Seek(position);
                return new Tuple<List<string>, long>(reader.ReadLines(totalOfLines), reader.Position);
            }
        }

        /// <summary>
        /// Reads the multiple lines of a text file
        /// </summary>
        /// <param name="filePath">The path to text file</param>
        /// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
        /// <returns></returns>
        public static List<string> ReadTextFile(string filePath, int totalOfLines)
            => UtilityService.ReadTextFile(filePath, 0, totalOfLines).Item1;

        /// <summary>
        /// Reads the multiple lines of a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="position"></param>
        /// <param name="totalOfLines"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Tuple<List<string>, long>> ReadTextFileAsync(string filePath, long position, int totalOfLines, CancellationToken cancellationToken = default)
        {
            using (var reader = new TextFileReader(filePath))
            {
                reader.Seek(position);
                return new Tuple<List<string>, long>(await reader.ReadLinesAsync(totalOfLines, cancellationToken).ConfigureAwait(false), reader.Position);
            }
        }

        /// <summary>
        /// Reads the multiple lines of a text file
        /// </summary>
        /// <param name="filePath">The path to text file</param>
        /// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public static async Task<List<string>> ReadTextFileAsync(string filePath, int totalOfLines, CancellationToken cancellationToken = default)
            => (await UtilityService.ReadTextFileAsync(filePath, 0, totalOfLines, cancellationToken).ConfigureAwait(false)).Item1;

        /// <summary>
        /// Writes the multiple lines of a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lines"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        public static void WriteTextFile(string filePath, List<string> lines, bool append = true, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is invalid", nameof(filePath));

            if (lines != null && lines.Any())
                using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize))
                using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
                {
                    lines.Where(line => line != null).ForEach(line => writer.WriteLine(line));
                    writer.Flush();
                }
        }

        /// <summary>
        /// Writes the multiple lines of a text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lines"></param>
        /// <param name="append"></param>
        /// <param name="encoding"></param>
        /// <param name="cancellationToken"></param>
        public static async Task WriteTextFileAsync(string filePath, List<string> lines, bool append = true, Encoding encoding = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is invalid", nameof(filePath));

            if (lines != null && lines.Any())
                using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
                using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
                {
                    await lines.Where(line => line != null).ForEachAsync(async line => await writer.WriteLineAsync(line, cancellationToken).ConfigureAwait(false), true, false).ConfigureAwait(false);
                    await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
        }
        #endregion

        #region Read/Write binary files
        /// <summary>
        /// Reads a binary file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static byte[] ReadBinaryFile(FileInfo fileInfo)
        {
            if (fileInfo != null && fileInfo.Exists)
                using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize))
                {
                    var buffer = new byte[fileInfo.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
            throw new ArgumentException("File info is invalid", nameof(fileInfo));
        }

        /// <summary>
        /// Reads a binary file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ReadBinaryFile(string filePath)
            => !string.IsNullOrWhiteSpace(filePath)
                ? UtilityService.ReadBinaryFile(new FileInfo(filePath))
                : throw new ArgumentException("File path is invalid", nameof(filePath));

        /// <summary>
        /// Reads a binary file
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadBinaryFileAsync(FileInfo fileInfo, CancellationToken cancellationToken = default)
        {
            if (fileInfo != null && fileInfo.Exists)
                using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
                {
                    var buffer = new byte[fileInfo.Length];
                    await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    return buffer;
                }
            throw new ArgumentException("File info is invalid", nameof(fileInfo));
        }

        /// <summary>
        /// Reads a binary file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> ReadBinaryFileAsync(string filePath, CancellationToken cancellationToken = default)
            => !string.IsNullOrWhiteSpace(filePath)
                ? UtilityService.ReadBinaryFileAsync(new FileInfo(filePath), cancellationToken)
                : Task.FromException<byte[]>(new ArgumentException("File path is invalid", nameof(filePath)));

        /// <summary>
        /// Writes a binary file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public static void WriteBinaryFile(string filePath, byte[] content, bool append = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path is invalid");

            using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize))
            {
                stream.Write(content ?? Array.Empty<byte>(), 0, content != null ? content.Length : 0);
                stream.Flush();
            }
        }

        /// <summary>
        /// Writes a binary file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task WriteBinaryFileAsync(string filePath, byte[] content, bool append = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path is invalid");

            using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
            {
                await stream.WriteAsync(content ?? Array.Empty<byte>(), 0, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Writes a binary file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public static void WriteBinaryFile(string filePath, Stream content, bool append = false)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path is invalid");

            if (content.CanSeek)
                content.Seek(0, SeekOrigin.Begin);

            using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize))
            {
                var buffer = new byte[TextFileReader.BufferSize];
                var read = content.Read(buffer, 0, buffer.Length);
                while (read > 0)
                {
                    stream.Write(buffer, 0, read);
                    stream.Flush();
                    read = content.Read(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// Writes a binary file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        /// <param name="append"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task WriteBinaryFileAsync(string filePath, Stream content, bool append = false, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path is invalid");

            if (content.CanSeek)
                content.Seek(0, SeekOrigin.Begin);

            using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
            {
                var buffer = new byte[TextFileReader.BufferSize];
                var read = await content.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                while (read > 0)
                {
                    await stream.WriteAsync(buffer, read, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    read = await content.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        #endregion

        #region Upload/Download
        /// <summary>
        /// Uploads the data stream as file to a remote server
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="filename"></param>
        /// <param name="url"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onError"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task UploadAsync(this Stream stream, string filename, string url, Action<string, string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), "Data stream is invalid");

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var results = "";
                using (var httpClient = new HttpClient())
                using (var content = new MultipartFormDataContent("VIEAppsNGX----" + DateTime.Now.ToIsoString()))
                {
                    content.Add(new StreamContent(stream), "files", filename);
                    using (var message = await httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false))
                    {
                        results = await message.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    }
                }
                stopwatch.Stop();
                onCompleted?.Invoke(url, results, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                onError?.Invoke(url, ex);
            }
        }

        /// <summary>
        /// Uploads data as file to a remote server
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <param name="url"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onError"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task UploadAsync(byte[] data, string filename, string url, Action<string, string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data is invalid");

            using (var stream = UtilityService.CreateMemoryStream(data))
                await stream.UploadAsync(filename, url, onCompleted, onError, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Upload a file to a remote server
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="url"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onError"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task UploadFileAsync(string filePath, string url, Action<string, string, string, long> onCompleted = null, Action<string, string, Exception> onError = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path is null");

            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            var fileInfo = new FileInfo(filePath);
            using (var stream = UtilityService.CreateMemoryStream(await UtilityService.ReadBinaryFileAsync(fileInfo, cancellationToken).ConfigureAwait(false)))
            {
                var stopwatch = Stopwatch.StartNew();
                await stream.UploadAsync(
                    fileInfo.Name,
                    url,
                    (uri, results, times) =>
                    {
                        stopwatch.Stop();
                        onCompleted?.Invoke(filePath, url, results, stopwatch.ElapsedMilliseconds);
                    },
                    (uri, ex) =>
                    {
                        stopwatch.Stop();
                        onError?.Invoke(filePath, url, ex);
                    },
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Downloads data from a remote server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onError"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<byte[]> DownloadAsync(string url, Action<string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url) || (!url.IsStartsWith("http://") && !url.IsStartsWith("https://")))
                throw new ArgumentNullException(nameof(url), "URL is invalid");

            try
            {
                var stopwatch = Stopwatch.StartNew();
                byte[] data = null;
                using (var webResponse = await new Uri(url).SendHttpClientRequestAsync("GET", null, null, 90, null, null, cancellationToken).ConfigureAwait(false))
                    data = await webResponse.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();
                onCompleted?.Invoke(url, stopwatch.ElapsedMilliseconds);
                return data;
            }
            catch (Exception ex)
            {
                onError?.Invoke(url, ex);
                return null;
            }
        }

        /// <summary>
        /// Downloads a file from a remote server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="refererURL"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onError"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task DownloadFileAsync(string url, string filePath, string refererURL, Action<string, string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(url) && (url.IsStartsWith("http://") || url.IsStartsWith("https://")))
                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    using (var webResponse = await new Uri(url).SendHttpClientRequestAsync("GET", null, null, 90, null, null, cancellationToken).ConfigureAwait(false))
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
                        await webResponse.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
                    stopwatch.Stop();
                    onCompleted?.Invoke(url, filePath, stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    onError?.Invoke(url, ex);
                }
            else
                onCompleted?.Invoke(url, null, 0);
        }

        /// <summary>
        /// Downloads a file from a remote server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        /// <param name="referUri"></param>
        /// <param name="onCompleted"></param>
        /// <param name="onError"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task DownloadFileAsync(string url, string filePath, string referUri = null, Action<string, string> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default)
            => UtilityService.DownloadFileAsync(url, filePath, referUri, (uri, path, times) => onCompleted?.Invoke(uri, path), onError, cancellationToken);
        #endregion

        #region Compressions
        /// <summary>
        /// Compresses the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mode">Compression mode (br/gzip/deflate)</param>
        /// <returns></returns>
        public static ArraySegment<byte> Compress(this Stream stream, string mode = "deflate")
        {
            using (var output = UtilityService.CreateMemoryStream())
            {
#if NETSTANDARD2_0
                using (var compressor = "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#else
                using (var compressor = "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(output, CompressionLevel.Optimal, true) as Stream : "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#endif
                {
                    if (stream.CanSeek)
                        stream.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[TextFileReader.BufferSize];
                    var read = stream.Read(buffer, 0, buffer.Length);
                    while (read > 0)
                    {
                        compressor.Write(buffer, 0, read);
                        read = stream.Read(buffer, 0, buffer.Length);
                    }
                    compressor.Flush();
                }
                return output.ToArraySegment();
            }
        }

        /// <summary>
        /// Compresses the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mode">Compression mode (br/gzip/deflate)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public static async Task<ArraySegment<byte>> CompressAsync(this Stream stream, string mode = "deflate", CancellationToken cancellationToken = default)
        {
            using (var output = UtilityService.CreateMemoryStream())
            {
#if NETSTANDARD2_0
                using (var compressor = "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#else
                using (var compressor = "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(output, CompressionLevel.Optimal, true) as Stream : "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#endif
                {
                    if (stream.CanSeek)
                        stream.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[TextFileReader.BufferSize];
                    var read = stream is MemoryStream
                        ? stream.Read(buffer, 0, buffer.Length)
                        : await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    while (read > 0)
                    {
                        await compressor.WriteAsync(buffer, read, cancellationToken).ConfigureAwait(false);
                        read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    }
                    await compressor.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                return output.ToArraySegment();
            }
        }

        /// <summary>
        /// Compresses the array segment of bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mode">Compression mode (br/gzip/deflate)</param>
        /// <returns></returns>
        public static ArraySegment<byte> Compress(this ArraySegment<byte> data, string mode = "deflate")
        {
            using (var output = UtilityService.CreateMemoryStream())
            {
#if NETSTANDARD2_0
                using (var compressor = "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#else
                using (var compressor = "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(output, CompressionLevel.Optimal, true) as Stream : "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#endif
                {
                    compressor.Write(data.Array, data.Offset, data.Count);
                    compressor.Flush();
                }
                return output.ToArraySegment();
            }
        }

        /// <summary>
        /// Compresses the array of bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mode">Compression mode (br/gzip/deflate)</param>
        /// <returns></returns>
        public static byte[] Compress(this byte[] data, string mode = "deflate")
            => data.ToArraySegment().Compress(mode).ToBytes();

        /// <summary>
        /// Decompresses the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mode">Decompression mode (br/gzip/deflate)</param>
        /// <returns></returns>
        public static byte[] Decompress(this Stream stream, string mode = "deflate")
        {
#if NETSTANDARD2_0
            using (var decompressor = "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress) as Stream : new DeflateStream(stream, CompressionMode.Decompress) as Stream)
#else
            using (var decompressor = "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(stream, CompressionMode.Decompress) as Stream : "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress) as Stream : new DeflateStream(stream, CompressionMode.Decompress) as Stream)
#endif
            {
                var output = Array.Empty<byte>();
                var buffer = new byte[TextFileReader.BufferSize];
                var read = decompressor.Read(buffer, 0, buffer.Length);
                while (read > 0)
                {
                    output = output.Concat(buffer.Take(0, read));
                    read = decompressor.Read(buffer, 0, buffer.Length);
                }
                return output;
            }
        }

        /// <summary>
        /// Decompresses the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="mode">Decompression mode (br/gzip/deflate)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public static async Task<byte[]> DecompressAsync(this Stream stream, string mode = "deflate", CancellationToken cancellationToken = default)
        {
#if NETSTANDARD2_0
            using (var decompressor = "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress) as Stream : new DeflateStream(stream, CompressionMode.Decompress) as Stream)
#else
            using (var decompressor = "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(stream, CompressionMode.Decompress) as Stream : "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress) as Stream : new DeflateStream(stream, CompressionMode.Decompress) as Stream)
#endif
            {
                var output = Array.Empty<byte>();
                var buffer = new byte[TextFileReader.BufferSize];
                var read = stream is MemoryStream
                    ? decompressor.Read(buffer, 0, buffer.Length)
                    : await decompressor.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                while (read > 0)
                {
                    output = output.Concat(buffer.Take(0, read));
                    read = await decompressor.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
                return output;
            }
        }

        /// <summary>
        /// Decompresses the array of bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mode">Decompression mode (br/gzip/deflate)</param>
        /// <returns></returns>
        public static byte[] Decompress(this byte[] data, string mode = "deflate")
            => data.ToMemoryStream().Decompress(mode);

        /// <summary>
        /// Decompresses the array segment of bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="mode">Decompression mode (br/gzip/deflate)</param>
        /// <returns></returns>
        public static ArraySegment<byte> Decompress(this ArraySegment<byte> data, string mode = "deflate")
            => data.ToMemoryStream().Decompress(mode).ToArraySegment();
        #endregion

        #region BigInteger extensions
        /// <summary>
        /// Converts this array of bytes to big-integer
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static BigInteger ToBigInteger(this byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] clone = new byte[bytes.Length];
                Buffer.BlockCopy(bytes, 0, clone, 0, bytes.Length);
                Array.Reverse(clone);
                return new BigInteger(clone);
            }
            else
                return new BigInteger(bytes);
        }

        public static BigInteger ToUnsignedBigInteger(this byte[] bytes)
        {
            byte[] clone;
            if (BitConverter.IsLittleEndian)
            {
                if (bytes[0] != 0x00)
                {
                    clone = new byte[bytes.Length + 1];
                    Buffer.BlockCopy(bytes, 0, clone, 1, bytes.Length);
                    Array.Reverse(clone);
                    return new BigInteger(clone);
                }
                clone = new byte[bytes.Length];
                Buffer.BlockCopy(bytes, 0, clone, 0, bytes.Length);
                Array.Reverse(clone);
                return new BigInteger(clone);
            }

            if (bytes[bytes.Length - 1] == 0x00)
                return new BigInteger(bytes);

            clone = new byte[bytes.Length + 1];
            Buffer.BlockCopy(bytes, 0, clone, 0, bytes.Length);
            return new BigInteger(clone);
        }

        /// <summary>
        /// Converts this hexa-string to big-integer
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static BigInteger ToBigInteger(this string hex)
        {
            var bytes = hex.HexToBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            Array.Resize(ref bytes, bytes.Length + 1);
            bytes[bytes.Length - 1] = 0x00;
            return new BigInteger(bytes);
        }

        public static BigInteger ModInverse(this BigInteger n, BigInteger p)
        {
            BigInteger x = 1;
            BigInteger y = 0;
            BigInteger a = p;
            BigInteger b = n;

            while (b != 0)
            {
                BigInteger t = b;
                BigInteger q = BigInteger.Divide(a, t);
                b = a - q * t;
                a = t;
                t = x;
                x = y - q * t;
                y = t;
            }

            if (y < 0)
                return y + p;
            //else
            return y;
        }

        public static bool TestBit(this BigInteger i, int n)
        {
            return !(i >> n).IsEven;
        }

        public static int BitLength(this BigInteger i)
        {
            int bitLength = 0;
            do
            {
                bitLength++;
            }
            while ((i >>= 1) != 0);
            return bitLength;
        }

        public static BigInteger Order(this BigInteger b, BigInteger p)
        {
            BigInteger m = 1;
            BigInteger e = 0;

            while (BigInteger.ModPow(b, m, p) != 1)
            {
                m *= 2;
                e++;
            }

            return e;
        }

        private static BigInteger FindS(BigInteger p)
        {
            BigInteger s = p - 1;
            BigInteger e = 0;

            while (s % 2 == 0)
            {
                s /= 2;
                e += 1;
            }

            return s;
        }

        private static BigInteger FindE(BigInteger p)
        {
            BigInteger s = p - 1;
            BigInteger e = 0;

            while (s % 2 == 0)
            {
                s /= 2;
                e += 1;
            }

            return e;
        }

        private static BigInteger TwoExp(BigInteger e)
        {
            BigInteger a = 1;

            while (e > 0)
            {
                a *= 2;
                e--;
            }

            return a;
        }

        public static BigInteger ShanksSqrt(this BigInteger a, BigInteger p)
        {
            if (BigInteger.ModPow(a, (p - 1) / 2, p) == (p - 1))
                return -1;

            if (p % 4 == 3)
                return BigInteger.ModPow(a, (p + 1) / 4, p);

            //Initialize 
            BigInteger s = FindS(p);
            BigInteger e = FindE(p);
            BigInteger n = 2;

            while (BigInteger.ModPow(n, (p - 1) / 2, p) == 1)
                n++;

            BigInteger x = BigInteger.ModPow(a, (s + 1) / 2, p);
            BigInteger b = BigInteger.ModPow(a, s, p);
            BigInteger g = BigInteger.ModPow(n, s, p);
            BigInteger r = e;
            BigInteger m = b.Order(p);

            while (m > 0)
            {
                x = (x * BigInteger.ModPow(g, TwoExp(r - m - 1), p)) % p;
                b = (b * BigInteger.ModPow(g, TwoExp(r - m), p)) % p;
                g = BigInteger.ModPow(g, TwoExp(r - m), p);
                r = m;
                m = b.Order(p);
            }

            return x;
        }
        #endregion

        #region Get setting/parameter of the app
        /// <summary>
        /// Gets a setting section of the app (from the JSON configuration file [appsettings.json])
        /// </summary>
        /// <param name="path">The path from root section (ex: Logging/LogLevel/Default)</param>
        /// <returns></returns>
        public static IConfigurationSection GetAppSetting(this IConfiguration configuration, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path.Trim();
            while (path.StartsWith("/") || path.StartsWith(@"\"))
                path = path.Right(path.Length - 1);
            while (path.EndsWith("/") || path.EndsWith(@"\"))
                path = path.Left(path.Length - 1);

            var paths = path.IndexOf("/") > 0 ? path.ToArray("/", true) : path.ToArray(@"\", true);
            if (string.IsNullOrWhiteSpace(paths[0]))
                return null;

            var section = configuration.GetSection(paths[0]);
            var index = 1;
            while (section != null && index < paths.Length)
            {
                section = section.GetSection(paths[index]);
                index++;
            }

            return section;
        }

        /// <summary>
        /// Gets a setting value of the app (from the JSON configuration file [appsettings.json])
        /// </summary>
        /// <param name="path">The path from root section (ex: Logging/LogLevel/Default)</param>
        /// <param name="default">The default value if the setting is not found</param>
        /// <returns></returns>
        public static T GetAppSetting<T>(this IConfiguration configuration, string path, T @default = default)
        {
            var section = configuration.GetAppSetting(path);
            return section != null
                ? section.Value.CastAs<T>()
                : @default;
        }

        /// <summary>
        /// Gets a setting of the app (from the XML configuration file [app.config/web.config] - section 'appSettings') with special prefix
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="default">The default value if the setting is not found</param>
        /// <param name="prefix">The special name prefix of the parameter</param>
        /// <returns></returns>
        public static string GetAppSetting(string name, string @default = null, string prefix = "vieapps")
        {
            var value = !string.IsNullOrWhiteSpace(name)
                ? System.Configuration.ConfigurationManager.AppSettings[(string.IsNullOrWhiteSpace(prefix) ? "" : prefix + ":") + name.Trim()]
                : null;

            return string.IsNullOrWhiteSpace(value)
                ? @default
                : value;
        }

        /// <summary>
        /// Gets a parameter of the app (first from header, then second from query)
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="header">The collection of header</param>
        /// <param name="query">The collection of query</param>
        /// <param name="default">The default value if the parameter is not found</param>
        /// <returns></returns>
        public static string GetAppParameter(string name, Dictionary<string, string> header, Dictionary<string, string> query, string @default = null)
        {
            string value = null;
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!(header ?? new Dictionary<string, string>()).TryGetValue(name, out value))
                    (query ?? new Dictionary<string, string>()).TryGetValue(name, out value);
            }
            return value ?? @default;
        }

        /// <summary>
        /// Gets a parameter of the app (first from header, then second from query)
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="header">The collection of header</param>
        /// <param name="query">The collection of query</param>
        /// <param name="default">The default value if the parameter is not found</param>
        /// <returns></returns>
        public static string GetAppParameter(string name, NameValueCollection header, NameValueCollection query, string @default = null)
            => UtilityService.GetAppParameter(name, header?.ToDictionary(), query?.ToDictionary(), @default);
        #endregion

        #region Working with external process
        /// <summary>
        /// Runs a process
        /// </summary>
        /// <param name="filePath">The string that presents the path to a file to run</param>
        /// <param name="arguments">The string that presents the arguments to run</param>
        /// <param name="onExited">The method to handle the Exit event</param>
        /// <param name="onDataReceived">The method to handle the data receive events (include OutputDataReceived and ErrorDataReceived events)</param>
        /// <returns></returns>
        public static Process RunProcess(string filePath, string arguments = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onDataReceived = null)
            => ExternalProcess.Start(filePath, arguments, onExited, onDataReceived).Process;

        /// <summary>
        /// Kills a process
        /// </summary>
        /// <param name="process"></param>
        /// <param name="action">The action to try to close the process before the process be killed</param>
        public static void KillProcess(Process process, Action<Process> action = null)
            => ExternalProcess.Kill(process, action);

        /// <summary>
        /// Kills a process by ID
        /// </summary>
        /// <param name="id">The integer that presents the identity of a process</param>
        /// <param name="action">The action to try to close the process before the process be killed</param>
        public static void KillProcess(int id, Action<Process> action = null)
            => ExternalProcess.Kill(id, action);
        #endregion

        #region Get version information
        /// <summary>
        /// Gets the string that presents the version number
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="getInfoVersion"></param>
        /// <returns></returns>
        public static string GetVersion(this Assembly assembly, bool getInfoVersion = true)
        {
            var asmVersion = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            var asmFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            var asmInfoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return (getInfoVersion ? asmInfoVersion?.InformationalVersion : asmVersion?.Version ?? asmFileVersion?.Version) ?? assembly.GetName()?.Version?.ToString() ?? "1.0";
        }
        #endregion

    }

    // -----------------------------------------------------------

    #region Tag information of XHTML/XML
    public class XTag
    {
        public XTag() { }

        /// <summary>
        /// Gets or Sets the full tag with attributes
        /// </summary>
        public string Full { get; set; }

        /// <summary>
        /// Gets or Sets the tag name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the open state
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Gets or Sets the close state
        /// </summary>
        public bool IsClose { get; set; }

        /// <summary>
        /// Gets or Sets the collection of attributes
        /// </summary>
        public List<XTagAttribute> Attributes { get; set; }

        /// <summary>
        /// Gets or Sets the relevant index
        /// </summary>
        public int RelevantIndex { get; set; } = -1;

        /// <summary>
        /// Gets or Sets the start position
        /// </summary>
        public int StartPosition { get; set; } = -1;

        /// <summary>
        /// Gets or Sets the end position
        /// </summary>
        public int EndPosition { get; set; } = -1;

        /// <summary>
        /// Gets or Sets the outer (means tag with full attribute, inner content and close mark)
        /// </summary>
        public string Outer { get; set; }

        /// <summary>
        /// Gets or Sets the inner content
        /// </summary>
        public string Inner { get; set; }

        /// <summary>
        /// Gets or Sets the next tag
        /// </summary>
        public XTag Next { get; set; }

        /// <summary>
        /// Gets or Sets the collection of children tags
        /// </summary>
        public List<XTag> Children { get; set; }

        /// <summary>
        /// Converts this tag to string
        /// </summary>
        /// <param name="full"></param>
        /// <returns></returns>
        public string ToString(bool full)
        {
            var tag = "<";
            tag += this.IsOpen ? this.Name : "";
            tag += this.IsOpen && this.Attributes != null && this.Attributes.Any() ? " " + this.Attributes.ToString(" ", attribute => attribute.ToString()) : "";
            tag += this.IsClose ? $"/{(this.IsOpen ? "" : this.Name)}" : "";
            tag += ">";
            tag += full && this.IsOpen && !this.IsClose ? this.Inner + $"</{this.Name}>" : "";
            return tag;
        }

        public override string ToString()
            => this.ToString(false);
    }

    public class XTagAttribute
    {
        public XTagAttribute() { }

        /// <summary>
        /// Gets or Sets the full attribute with name and value
        /// </summary>
        public string Full { get; set; }

        /// <summary>
        /// Gets or Sets the attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the attribute value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Converts this attribute to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => $"{this.Name}=\"{this.Value}\"";
    }
    #endregion

    // -----------------------------------------------------------

    #region Search Query
    /// <summary>
    /// Presents a parsed query for searching
    /// </summary>
    [Serializable]
    public class SearchQuery
    {
        /// <summary>
        /// Initializes a searching query
        /// </summary>
        /// <param name="query"></param>
        public SearchQuery(string query = null)
            => this.Parse(query);

        public List<string> AndWords { get; } = new List<string>();

        public List<string> OrWords { get; } = new List<string>();

        public List<string> NotWords { get; } = new List<string>();

        public List<string> AndPhrases { get; } = new List<string>();

        public List<string> OrPhrases { get; } = new List<string>();

        public List<string> NotPhrases { get; } = new List<string>();

        /// <summary>
        /// Parses the searching query
        /// </summary>
        /// <param name="query"></param>
        public void Parse(string query)
        {
            this.AndWords.Clear();
            this.AndPhrases.Clear();
            this.OrWords.Clear();
            this.OrPhrases.Clear();
            this.NotWords.Clear();
            this.NotPhrases.Clear();

            if (string.IsNullOrWhiteSpace(query))
                return;

            var searchQuery = this.NormalizeKeywords(query);
            var allWords = new List<string>();
            var allPhrases = new List<string>();

            var start = searchQuery.IndexOf("\"");
            var end = start > -1 ? searchQuery.IndexOf("\"", start + 1) : -1;
            if (start < 0 || end < 0)
                allWords = allWords.Concat(searchQuery.Replace("\"", "").ToArray(' ', true)).ToList();

            else
            {
                while (start >= 0 && end > 0)
                {
                    var previousCharater = "";
                    if (start > 0)
                        previousCharater = searchQuery[start - 1].ToString();

                    if (previousCharater.Equals("+") || previousCharater.Equals("-"))
                        start--;

                    var phrase = searchQuery.Substring(start, end - start + 1);
                    allPhrases.Add(phrase.Replace(" -\"", "\"").Replace(" +\"", "\""));
                    searchQuery = searchQuery.Remove(start, end - start + 1).Trim();
                    start = searchQuery.IndexOf("\"");
                    end = searchQuery.IndexOf("\"", start + 1);
                }
                allWords = allWords.Concat(this.NormalizeKeywords(searchQuery).Replace("\"", "").ToArray(' ').Select(i => i.Trim())).ToList();
            }

            allWords.Distinct(StringComparer.OrdinalIgnoreCase).Where(word => !string.IsNullOrWhiteSpace(word)).ForEach(word =>
            {
                if (word[0].Equals('+'))
                    this.AndWords.Add(word.Right(word.Length - 1));
                else if (word[0].Equals('-'))
                    this.NotWords.Add(word.Right(word.Length - 1));
                else
                    this.OrWords.Add(word);
            });

            allPhrases.Distinct(StringComparer.OrdinalIgnoreCase).Where(phrase => !string.IsNullOrWhiteSpace(phrase)).ForEach(phrase =>
            {
                if (phrase[0].Equals('+'))
                    this.AndPhrases.Add(phrase.Right(phrase.Length - 1).Replace("\"", ""));
                else if (phrase[0].Equals('-'))
                    this.NotPhrases.Add(phrase.Right(phrase.Length - 1).Replace("\"", ""));
                else
                    this.OrPhrases.Add(phrase.Replace("\"", ""));
            });
        }

        string NormalizeKeywords(string keywords)
        {
            var normalizedKeywords = keywords.Trim().Replace("  ", " ");

            while (normalizedKeywords.Contains("  "))
                normalizedKeywords = normalizedKeywords.Replace("  ", " ");

            normalizedKeywords = normalizedKeywords.Replace("+ ", "+").Replace("- ", "-");

            while (normalizedKeywords.Contains("++\""))
                normalizedKeywords = normalizedKeywords.Replace("++\"", "+\"");

            while (normalizedKeywords.Contains("--\""))
                normalizedKeywords = normalizedKeywords.Replace("--\"", "--\"");

            normalizedKeywords = normalizedKeywords.Replace("'", "").Replace(",", "").Replace(".", "");
            normalizedKeywords = normalizedKeywords.Replace("&", "").Replace("!", "").Replace("%", "");
            normalizedKeywords = normalizedKeywords.Replace("@", "").Replace("$", "").Replace("*", "");
            normalizedKeywords = normalizedKeywords.Replace("|", "").Replace("~", "").Replace("#", "");

            return normalizedKeywords;
        }
    }
    #endregion

    //  --------------------------------------------------------------------------------------------

    #region App configuration section handler
    /// <summary>
    /// The handler for processing a custom configuration section of the app
    /// </summary>
    public class AppConfigurationSectionHandler : System.Configuration.IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            this.Section = section;
            return this;
        }

        /// <summary>
        /// Gets the configuration section
        /// </summary>
        public XmlNode Section { get; private set; }
    }
    #endregion

    //  --------------------------------------------------------------------------------------------

    #region Reader of a text file
    /// <summary>
    /// Extends the System.IO.StreamReader that reads lines from a file in a particular encoding
    /// </summary>
    public sealed class TextFileReader : IDisposable
    {
        // by default, one reading block is 4K (4096), then use 16K(16384)/32K(32768)/64K(65536)/128K(131072)/256K(262144)/512K(524288)
        // for better performance while working with text file has large line of characters
        public static readonly int BufferSize = 16384;
        readonly FileStream _stream = null;
        readonly StreamReader _reader = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFileReader"/> class.
        /// </summary>
        /// <param name="filePath">The path to file</param>
        public TextFileReader(string filePath)
        {
            // check
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is invalid", nameof(filePath));

            else if (!File.Exists(filePath))
                throw new FileNotFoundException($"File is not found ({filePath})");

            // initialize
            this._stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true);
            this._reader = new StreamReader(this._stream, true);
        }

        ~TextFileReader()
            => this.Dispose();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this._reader.Close();
            this._reader.Dispose();
            this._stream.Close();
            this._stream.Dispose();
        }

        /// <summary>
        /// Gets the current encoding of text file.
        /// </summary>
        public Encoding Encoding => this._reader.CurrentEncoding;

        /// <summary>
        /// Gets the length of text file (in bytes).
        /// </summary>
        public long Length => this._reader.BaseStream.Length;

        /// <summary>
        /// Gets the current position
        /// </summary>
        public long Position
        {
            get
            {
                try
                {
                    var readerType = this._reader.GetType();

                    // shift position back from BaseStream.Position by the number of bytes read into internal buffer
                    var charLen = (int)readerType.InvokeMember("_charLen", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
                    var position = this._reader.BaseStream.Position - charLen;

                    // if we have consumed chars from the buffer we need to calculate how many bytes they represent in the current encoding and add that to the position
                    var charPos = (int)readerType.InvokeMember("_charPos", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
                    if (charPos > 0)
                    {
                        var charBuffer = (char[])readerType.InvokeMember("_charBuffer", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
                        var bytesConsumed = this.Encoding.GetBytes(charBuffer, 0, charPos).Length;
                        position += bytesConsumed;
                    }

                    return position;
                }
                catch
                {
                    return this._reader.BaseStream.Position;
                }
            }
        }

        /// <summary>
        /// Seeks to the position (to read next lines from this position)
        /// </summary>
        /// <param name="offset">The offset relative to the origin parameter</param>
        /// <param name="origin">Indicating the reference point used to obtain the new position</param>
        /// <returns>The new position within the current stream</returns>
        public long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            this._reader.DiscardBufferedData();
            return this._reader.BaseStream.Seek(offset > -1 ? offset : 0, origin);
        }

        /// <summary>
        /// Reads a line of characters (from the current position)
        /// </summary>
        /// <returns>The next line from file, or null if the end of file is reached</returns>
        public string ReadLine()
            => this._reader.ReadLine();

        /// <summary>
        /// Reads a line of characters (from the current position)
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The next line from file, or null if the end of file is reached</returns>
        public Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
            => this._reader.ReadLineAsync(cancellationToken);

        /// <summary>
        /// Reads some lines of characters (from the current position)
        /// </summary>
        /// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
        /// <returns>The next lines from the file, or empty collectoin if the end of file is reached</returns>
        public List<string> ReadLines(int totalOfLines)
        {
            // use StreamReader to read all lines (better performance)
            if (totalOfLines < 1 && this.Position < this.Encoding.GetPreamble().Length)
                return this.ReadAllLines();

            // read lines
            var lines = new List<string>();
            var counter = 0;
            var line = this.ReadLine();
            while (line != null)
            {
                lines.Add(line);

                counter++;
                if (totalOfLines > 0 && counter >= totalOfLines)
                    break;

                line = this.ReadLine();
            }
            return lines;
        }

        /// <summary>
        /// Reads some lines of characters (from the current position)
        /// </summary>
        /// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The next lines from the file, or empty collectoin if the end of file is reached</returns>
        public async Task<List<string>> ReadLinesAsync(int totalOfLines, CancellationToken cancellationToken = default)
        {
            // use StreamReader to read all lines (better performance)
            if (totalOfLines < 1 && this.Position < this.Encoding.GetPreamble().Length)
                return await this.ReadAllLinesAsync(cancellationToken).ConfigureAwait(false);

            // read lines
            var lines = new List<string>();
            var counter = 0;
            var line = await this.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            while (line != null)
            {
                lines.Add(line);

                counter++;
                if (totalOfLines > 0 && counter >= totalOfLines)
                    break;

                line = await this.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }

            return lines;
        }

        /// <summary>
        /// Reads all lines of characters of the files
        /// </summary>
        /// <returns></returns>
        public List<string> ReadAllLines()
        {
            // jump to first
            this.Seek(0);

            // read all lines
            var lines = new List<string>();
            var line = this._reader.ReadLine();
            while (line != null)
            {
                lines.Add(line);
                line = this._reader.ReadLine();
            }

            // return lines
            return lines;
        }

        /// <summary>
        /// Reads all lines of characters of the files
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns></returns>
        public async Task<List<string>> ReadAllLinesAsync(CancellationToken cancellationToken = default)
        {
            // jump to first
            this.Seek(0);

            // read all lines
            var lines = new List<string>();
            var line = await this._reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            while (line != null)
            {
                lines.Add(line);
                line = await this._reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            }

            // return lines
            return lines;
        }
    }
    #endregion

    //  --------------------------------------------------------------------------------------------

    #region Random of BigInteger
    /// <summary>
    /// Represents a pseudo-random number generator, which is a device that produces a sequence of numbers that meet certain statistical requirements for randomness.
    /// </summary>
    public class RandomBigInteger : Random
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomBigInteger">RandomBigInteger</see> class, using a time-dependent default seed value.
        /// </summary>
        public RandomBigInteger() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomBigInteger">RandomBigInteger</see> class, using the specified seed value.
        /// </summary>
        /// <param name="seed"></param>
        public RandomBigInteger(int seed) : base(seed) { }

        /// <summary>
        /// Generates a random positive BigInteger between 0 and 2^bitLength (non-inclusive).
        /// </summary>
        /// <param name="length">The number of random bits to generate.</param>
        /// <returns>A random positive BigInteger between 0 and 2^bitLength (non-inclusive).</returns>
        public new BigInteger Next(int length)
        {
            if (length < 1)
                return BigInteger.Zero;

            var lengthOfBytes = length / 8;
            var lengthOfBits = length % 8;

            // generates enough random bytes to cover our bits
            var bytes = new byte[lengthOfBytes + 1];
            this.NextBytes(bytes);

            // mask out the unnecessary bits
            var mask = (byte)(0xFF >> (8 - lengthOfBits));
            bytes[bytes.Length - 1] &= mask;

            // return the BigInteger with the bits-length
            return new BigInteger(bytes.Take(1));
        }

        /// <summary>
        /// Generates a random BigInteger between start and end (non-inclusive).
        /// </summary>
        /// <param name="minValue">The lower bound.</param>
        /// <param name="maxValue">The upper bound (non-inclusive).</param>
        /// <returns>A random BigInteger between start and end (non-inclusive)</returns>
        public BigInteger Next(BigInteger minValue, BigInteger maxValue)
        {
            // initialize
            if (minValue == maxValue)
                return minValue;

            var bigInt = maxValue;

            // swap start and end if given in reverse order
            if (minValue > maxValue)
            {
                maxValue = minValue;
                minValue = bigInt;
                bigInt = maxValue - minValue;
            }
            else
                // the distance between start and end to generate a random BigIntger between 0 and (end-start) (non-inclusive)
                bigInt -= minValue;

            // count the number of bits necessary
            var bytes = bigInt.ToBytes();
            var bits = 8;
            byte mask = 0x7F;
            while ((bytes[bytes.Length - 1] & mask) == bytes[bytes.Length - 1])
            {
                bits--;
                mask >>= 1;
            }
            bits += 8 * bytes.Length;

            // generate a random BigInteger that is the first power of 2 larger than the number, 
            // then scale the range down to the size of the number,
            // finally add start back on to shift back to the desired range and return.
            return ((this.Next(bits + 1) * bigInt) / BigInteger.Pow(2, bits + 1)) + minValue;
        }
    }
    #endregion

    //  --------------------------------------------------------------------------------------------

    #region Remote server response
    public class RemoteServerResponse : IDisposable
    {
        internal static async Task<RemoteServerResponse> CreateAsync(HttpStatusCode statusCode, Uri uri, Dictionary<string, string> headers, Stream stream, CancellationToken cancellationToken = default)
        {
            var response = new RemoteServerResponse(statusCode, uri, headers) { Body = UtilityService.CreateMemoryStream() };
            await stream.CopyToAsync(response.Body, cancellationToken).ConfigureAwait(false);
            return response;
        }

        CookieCollection _cookies = null;

        public HttpStatusCode StatusCode { get; internal set; } = HttpStatusCode.OK;

        public Uri URI { get; internal set; }

        public Dictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();

        public MemoryStream Body { get; internal set; }

        public CookieCollection Cookies => this.GetCookies();

        public string ContentType
            => this.GetHeaders().TryGetValue("Content-Type", out var contentType) && !string.IsNullOrWhiteSpace(contentType)
                ? contentType
                : null;

        public RemoteServerResponse(HttpStatusCode statusCode = HttpStatusCode.OK, Uri uri = null, Dictionary<string, string> headers = null)
        {
            this.StatusCode = statusCode;
            this.URI = uri;
            this.Headers = headers;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Body?.Dispose();
        }

        public Task<MemoryStream> ReadAsStreamAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(this.Body);

        public async Task<byte[]> ReadAsByteArrayAsync(CancellationToken cancellationToken = default)
        {
            var buffer = Array.Empty<byte>();
            if (this.Body != null)
            {
                this.Body.Seek(0, SeekOrigin.Begin);
                buffer = new byte[this.Body.Length];
                await this.Body.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            return buffer;
        }

        public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            var @string = this.Body != null ? await this.Body.ReadAllAsync(cancellationToken, true).ConfigureAwait(false) : null;
            return (this.ContentType ?? "").IsStartsWith("text/html") ? @string?.HtmlDecode() : @string;
        }

        public async Task CopyToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (this.Body != null)
            {
                this.Body.Seek(0, SeekOrigin.Begin);
                await this.Body.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            if (this.Body != null)
            {
                this.Body.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[TextFileReader.BufferSize];
                var count = await this.Body.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                while (count > 0)
                {
                    await stream.WriteAsync(buffer, count, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    count = await this.Body.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public Dictionary<string, string> GetHeaders(IEnumerable<string> excluded = null, Action<Dictionary<string, string>> onCompleted = null)
        {
            var headers = new Dictionary<string, string>(this.Headers ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
            excluded?.ForEach(name => headers.Remove(name));
            onCompleted?.Invoke(headers);
            return headers;
        }

        public CookieCollection GetCookies(Action<Cookie> onAdd = null)
            => this._cookies ?? (this._cookies = this.GetHeaders().TryGetValue("Set-Cookie", out var cookies) && !string.IsNullOrWhiteSpace(cookies) ? cookies.ToList().GetCookies(this.URI?.Host ?? "vieapps.net", onAdd) : new CookieCollection());
    }
    #endregion

}