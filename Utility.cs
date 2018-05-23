#region Related components
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using System.Reflection;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Utility servicing methods
	/// </summary>
	public static partial class UtilityService
	{

		#region UUID
		/// <summary>
		/// Gets the UUID (unique universal identity - in 128 bits)
		/// </summary>
		/// <param name="uuid">The string that presents an UUID</param>
		/// <returns>The string that presents UUID with hyphens (128 bits)</returns>
		public static string GetUUID(string uuid = null)
		{
			if (!string.IsNullOrWhiteSpace(uuid))
			{
				uuid = uuid.Trim();
				if (uuid.IndexOf("-") > -1)
					return uuid;

				var pos = 8;
				uuid = uuid.Insert(pos, "-");
				for (var index = 0; index < 3; index++)
				{
					pos += 5;
					uuid = uuid.Insert(pos, "-");
				}
				return uuid;
			}
			return Guid.NewGuid().ToString("N").ToLower();
		}

		/// <summary>
		/// Generate an UUID from this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="mode">BLAKE or MD5</param>
		/// <returns></returns>
		public static string GenerateUUID(this string @string, string mode = "MD5")
			=> string.IsNullOrWhiteSpace(@string)
				? Guid.NewGuid().ToString("N").ToLower()
				: !string.IsNullOrWhiteSpace(mode) && mode.IsStartsWith("blake")
					? @string.GetBLAKE128()
					: @string.GetMD5();

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

		static Regex HexRegex = new Regex("[^0-9a-fA-F]+");

		/// <summary>
		/// Validates the UUID string
		/// </summary>
		/// <param name="uuid"></param>
		/// <param name="onlyHex">true to only allow hexa characters</param>
		/// <returns>true if it is valid; otherwise false.</returns>
		public static bool IsValidUUID(this string uuid, bool onlyHex = true)
			=> string.IsNullOrWhiteSpace(uuid) || !uuid.Length.Equals(32)
				? false
				: onlyHex
					? UtilityService.HexRegex.Replace(uuid, "").Equals(uuid)
					: !uuid.Contains(" ") && !uuid.Contains(";");
		#endregion

		#region Random number
		static Random _Random = new Random();

		/// <summary>
		/// Gets the random number between min and max
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int GetRandomNumber(int min = 0, int max = Int32.MaxValue) => UtilityService._Random.Next(min, max);

		static RandomBigInteger _RandomBigInteger = new RandomBigInteger();

		/// <summary>
		/// Gets the random of big integer number
		/// </summary>
		/// <param name="length">The number of random bits to generate.</param>
		/// <returns></returns>
		public static BigInteger GetRandomNumber(int length) => UtilityService._RandomBigInteger.Next(length);
		#endregion

		#region Async extensions to support cancellation token
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
				if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
					throw new OperationCanceledException(cancellationToken);
			}
			await task.ConfigureAwait(false);
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
				if (task != await Task.WhenAny(task, tcs.Task).ConfigureAwait(false))
					throw new OperationCanceledException(cancellationToken);
			}
			return await task.ConfigureAwait(false);
		}

		/// <summary>
		/// Performs a web request and get response in async
		/// </summary>
		/// <param name="httpRequest"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest httpRequest, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => httpRequest.Abort(), false))
			{
				try
				{
					return await httpRequest.GetResponseAsync().ConfigureAwait(false) as HttpWebResponse;
				}
				catch (WebException ex)
				{
					if (cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, cancellationToken);
					else
						throw ex;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Downloads the resource as a <see cref="System.Byte"/>  array from the URI specified as an asynchronous operation using a task object.
		/// </summary>
		/// <param name="webclient"></param>
		/// <param name="address"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<byte[]> DownloadDataTaskAsync(this WebClient webclient, string address, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => webclient.CancelAsync(), false))
			{
				try
				{
					return await webclient.DownloadDataTaskAsync(address).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, cancellationToken);
					else
						throw ex;
				}
			}
		}

		/// <summary>
		/// Downloads the resource as a <see cref="System.Byte"/>  array from the URI specified as an asynchronous operation using a task object.
		/// </summary>
		/// <param name="webclient"></param>
		/// <param name="address"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<byte[]> DownloadDataTaskAsync(this WebClient webclient, Uri address, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => webclient.CancelAsync(), false))
			{
				try
				{
					return await webclient.DownloadDataTaskAsync(address).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, cancellationToken);
					else
						throw ex;
				}
			}
		}

		/// <summary>
		/// Downloads the resource as a <see cref="System.String"/>  from the URI specified as an asynchronous operation using a task object.
		/// </summary>
		/// <param name="webclient"></param>
		/// <param name="address"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<string> DownloadStringTaskAsync(this WebClient webclient, string address, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => webclient.CancelAsync(), false))
			{
				try
				{
					return await webclient.DownloadStringTaskAsync(address).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, cancellationToken);
					else
						throw ex;
				}
			}
		}

		/// <summary>
		/// Downloads the resource as a <see cref="System.String"/>  from the URI specified as an asynchronous operation using a task object.
		/// </summary>
		/// <param name="webclient"></param>
		/// <param name="address"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<string> DownloadStringTaskAsync(this WebClient webclient, Uri address, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => webclient.CancelAsync(), false))
			{
				try
				{
					return await webclient.DownloadStringTaskAsync(address).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, cancellationToken);
					else
						throw ex;
				}
			}
		}

		/// <summary>
		/// Downloads the specified resource to a local file as an asynchronous operation using a task object.
		/// </summary>
		/// <param name="webclient"></param>
		/// <param name="address"></param>
		/// <param name="fileName"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task DownloadFileTaskAsync(this WebClient webclient, string address, string fileName, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => webclient.CancelAsync(), false))
			{
				try
				{
					await webclient.DownloadFileTaskAsync(address, fileName).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, cancellationToken);
					else
						throw ex;
				}
			}
		}

		/// <summary>
		/// Downloads the specified resource to a local file as an asynchronous operation using a task object.
		/// </summary>
		/// <param name="webclient"></param>
		/// <param name="address"></param>
		/// <param name="fileName"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task DownloadFileTaskAsync(this WebClient webclient, Uri address, string fileName, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => webclient.CancelAsync(), false))
			{
				try
				{
					await webclient.DownloadFileTaskAsync(address, fileName).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					if (cancellationToken.IsCancellationRequested)
						throw new OperationCanceledException(ex.Message, ex, cancellationToken);
					else
						throw ex;
				}
			}
		}
		#endregion

		#region Working with task in the thread pool
		/// <summary>
		/// Executes an action in the thread pool with cancellation supported
		/// </summary>
		/// <param name="action">The action to run in the thread pool</param>
		/// <param name="onCancel">The action to callback when the operation is canceled</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>An awaitable task</returns>
		public static Task ExecuteTask(Action action, Action onCancel, CancellationToken cancellationToken = default(CancellationToken))
		{
			var tcs = new TaskCompletionSource<object>();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				using (cancellationToken.Register(() =>
				{
					try
					{
						onCancel?.Invoke();
					}
					catch { }
					tcs.TrySetCanceled(cancellationToken);
				}, useSynchronizationContext: false))
				{
					try
					{
						action?.Invoke();
						tcs.TrySetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
			});
			return tcs.Task;
		}

		/// <summary>
		/// Executes an action in the thread pool with cancellation supported
		/// </summary>
		/// <param name="action">The action to run in the thread pool</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>An awaitable task</returns>
		public static Task ExecuteTask(Action action, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.ExecuteTask(action, null, cancellationToken);
		}

		/// <summary>
		/// Executes an action in the thread pool with cancellation supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function to run in the thread pool</param>
		/// <param name="onCancel">The action to callback when the operation is canceled</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>An awaitable task</returns>
		public static Task<T> ExecuteTask<T>(Func<T> func, Action onCancel, CancellationToken cancellationToken = default(CancellationToken))
		{
			var tcs = new TaskCompletionSource<T>();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				using (cancellationToken.Register(() =>
				{
					try
					{
						onCancel?.Invoke();
					}
					catch { }
					tcs.TrySetCanceled(cancellationToken);
				}, useSynchronizationContext: false))
				{
					try
					{
						var result = func != null ? func.Invoke() : default(T);
						tcs.TrySetResult(result);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
			});
			return tcs.Task;
		}

		/// <summary>
		/// Executes an action in the thread pool with cancellation supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function to run in the thread pool</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>An awaitable task</returns>
		public static Task<T> ExecuteTask<T>(Func<T> func, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.ExecuteTask(func, null, cancellationToken);
		}
		#endregion

		#region Get external resource/webpage via HttpWebRequest object
		internal static string[] UserAgents = new string[]
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
			"Mozilla/5.0 (compatible; Yahoo! Slurp; http://help.yahoo.com/help/us/ysearch/slurp)",
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
		public static string MobileUserAgent => "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A356 Safari/604.1";

		/// <summary>
		/// Gets an user-agent as desktop browser
		/// </summary>
		public static string DesktopUserAgent => "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";

		/// <summary>
		/// Gets the web credential
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="account"></param>
		/// <param name="password"></param>
		/// <param name="useSecureProtocol"></param>
		/// <param name="secureProtocol"></param>
		/// <returns></returns>
		public static CredentialCache GetWebCredential(string uri, string account, string password, bool useSecureProtocol = true, SecurityProtocolType secureProtocol = SecurityProtocolType.Ssl3)
		{
			// check
			if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
				return null;

			// remark: not available on OSX
			if (useSecureProtocol && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				ServicePointManager.SecurityProtocol = secureProtocol;

			return new CredentialCache
			{
				{ new Uri(uri), "Basic", new NetworkCredential(account, password) }
			};
		}

		/// <summary>
		/// Gets the web proxy
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="bypass"></param>
		/// <returns></returns>
		public static WebProxy GetWebProxy(string host, int port, string username, string password, string[] bypass = null)
		{
			if (string.IsNullOrWhiteSpace(host))
				return null;

			var uri = new Uri($"{(!host.IsStartsWith("http://") && !host.IsStartsWith("https://") ? "https://" : "")}{host}:{port}");
			var proxy = new WebProxy(uri, true, bypass ?? new string[] { });
			if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
				proxy.Credentials = new NetworkCredential(username, password);

			return proxy;
		}

		/// <summary>
		/// Gets the web response
		/// </summary>
		/// <param name="method"></param>
		/// <param name="uri"></param>
		/// <param name="headers"></param>
		/// <param name="cookies"></param>
		/// <param name="body"></param>
		/// <param name="contentType"></param>
		/// <param name="timeout"></param>
		/// <param name="userAgent"></param>
		/// <param name="referUri"></param>
		/// <param name="credential"></param>
		/// <param name="proxy"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, Dictionary<string, string> headers, Cookie[] cookies, string body, string contentType, int timeout = 90, string userAgent = null, string referUri = null, CredentialCache credential = null, WebProxy proxy = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			// prepare the request object
			var webRequest = WebRequest.Create(uri) as HttpWebRequest;
			webRequest.Method = string.IsNullOrWhiteSpace(method) ? "GET" : method.ToUpper();
			webRequest.Timeout = timeout * 1000;
			webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			webRequest.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? UtilityService.DesktopUserAgent : userAgent;
			webRequest.Referer = string.IsNullOrWhiteSpace(referUri) ? "" : referUri;

			// headers
			headers?.Where(kvp => !kvp.Key.IsEquals("accept-encoding")).ForEach(kvp => webRequest.Headers.Add(kvp.Key, kvp.Value));

			// cookies
			if (webRequest.SupportsCookieContainer)
				cookies?.ForEach(cookie => webRequest.CookieContainer.Add(cookie));

			// compression
			webRequest.Headers.Add("accept-encoding", "deflate,gzip");
			webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

			// credential
			if (credential != null)
			{
				webRequest.Credentials = credential;
				webRequest.PreAuthenticate = true;
			}

			// service point - only available on Windows wit .NET Framework
			if (RuntimeInformation.FrameworkDescription.IsContains(".NET Framework"))
				webRequest.ServicePoint.Expect100Continue = false;

			// proxy
			if (proxy != null)
				webRequest.Proxy = proxy;

			// data to post/put
			if (!string.IsNullOrWhiteSpace(body) && (webRequest.Method.Equals("POST") || webRequest.Method.Equals("PUT")))
			{
				if (!string.IsNullOrWhiteSpace(contentType))
					webRequest.ContentType = contentType;

				using (var writer = new StreamWriter(await webRequest.GetRequestStreamAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false)))
				{
					await writer.WriteAsync(body).WithCancellationToken(cancellationToken).ConfigureAwait(false);
				}
			}

			// switch off certificate validation - not available on OSX
			// source: http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using)
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				webRequest.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

			// make request and return response stream
			try
			{
				return await webRequest.GetResponseAsync(cancellationToken).ConfigureAwait(false);
			}
			catch (SocketException ex)
			{
				if (ex.Message.Contains("did not properly respond after a period of time"))
					throw new ConnectionTimeoutException(ex.InnerException);
				else
					throw ex;
			}
			catch (WebException ex)
			{
				var responseBody = "";
				if (ex.Status.Equals(WebExceptionStatus.ProtocolError))
					using (var stream = (ex.Response as HttpWebResponse).GetResponseStream())
					{
						using (var reader = new StreamReader(stream, true))
						{
							responseBody = await reader.ReadToEndAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
						}
					}
				throw new RemoteServerErrorException("Error occurred at remote server", responseBody, ex?.Response?.ResponseUri.AbsoluteUri ?? uri, ex);
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				webRequest = null;
			}
		}

		/// <summary>
		/// Gets the web response
		/// </summary>
		/// <param name="method"></param>
		/// <param name="uri"></param>
		/// <param name="headers"></param>
		/// <param name="body"></param>
		/// <param name="contentType"></param>
		/// <param name="timeout"></param>
		/// <param name="userAgent"></param>
		/// <param name="referUri"></param>
		/// <param name="credentialAccount"></param>
		/// <param name="credentialPassword"></param>
		/// <param name="useSecureProtocol"></param>
		/// <param name="secureProtocol"></param>
		/// <param name="proxy"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, Dictionary<string, string> headers, string body, string contentType, int timeout = 90, string userAgent = null, string referUri = null, string credentialAccount = null, string credentialPassword = null, bool useSecureProtocol = true, SecurityProtocolType secureProtocol = SecurityProtocolType.Ssl3, WebProxy proxy = null, CancellationToken cancellationToken = default(CancellationToken))
			=> UtilityService.GetWebResponseAsync(method, uri, headers, null, body, contentType, timeout, userAgent, referUri, UtilityService.GetWebCredential(uri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol), proxy, cancellationToken);

		/// <summary>
		/// Gets the web resource stream
		/// </summary>
		/// <param name="method"></param>
		/// <param name="uri"></param>
		/// <param name="headers"></param>
		/// <param name="body"></param>
		/// <param name="contentType"></param>
		/// <param name="timeout"></param>
		/// <param name="userAgent"></param>
		/// <param name="referUri"></param>
		/// <param name="credentialAccount"></param>
		/// <param name="credentialPassword"></param>
		/// <param name="useSecureProtocol"></param>
		/// <param name="secureProtocol"></param>
		/// <param name="proxy"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<Stream> GetWebResourceAsync(string method, string uri, Dictionary<string, string> headers, string body, string contentType, int timeout = 90, string userAgent = null, string referUri = null, string credentialAccount = null, string credentialPassword = null, bool useSecureProtocol = true, SecurityProtocolType secureProtocol = SecurityProtocolType.Ssl3, WebProxy proxy = null, CancellationToken cancellationToken = default(CancellationToken))
			=> (await UtilityService.GetWebResponseAsync(method, uri, headers, body, contentType, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, proxy, cancellationToken).ConfigureAwait(false)).GetResponseStream();

		/// <summary>
		/// Gets the web resource stream
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="referUri"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<Stream> GetWebResourceAsync(string uri, string referUri = null, CancellationToken cancellationToken = default(CancellationToken))
			=> UtilityService.GetWebResourceAsync("GET", uri, null, null, null, 90, UtilityService.SpiderUserAgent, referUri, null, null, true, SecurityProtocolType.Ssl3, null, cancellationToken);

		/// <summary>
		/// Gets the web page
		/// </summary>
		/// <param name="url"></param>
		/// <param name="headers"></param>
		/// <param name="timeout"></param>
		/// <param name="userAgent"></param>
		/// <param name="referUri"></param>
		/// <param name="credentialAccount"></param>
		/// <param name="credentialPassword"></param>
		/// <param name="useSecureProtocol"></param>
		/// <param name="secureProtocol"></param>
		/// <param name="proxy"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<string> GetWebPageAsync(string url, Dictionary<string, string> headers, int timeout = 90, string userAgent = null, string referUri = null, string credentialAccount = null, string credentialPassword = null, bool useSecureProtocol = true, SecurityProtocolType secureProtocol = SecurityProtocolType.Ssl3, WebProxy proxy = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrWhiteSpace(url))
				return null;

			using (var stream = await UtilityService.GetWebResourceAsync("GET", url, headers, null, null, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, proxy, cancellationToken).ConfigureAwait(false))
			{
				using (var reader = new StreamReader(stream, true))
				{
					return (await reader.ReadToEndAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false)).HtmlDecode();
				}
			}
		}

		/// <summary>
		/// Gets the web page
		/// </summary>
		/// <param name="url"></param>
		/// <param name="headers"></param>
		/// <param name="timeout"></param>
		/// <param name="proxyHost"></param>
		/// <param name="proxyPort"></param>
		/// <param name="proxyUsername"></param>
		/// <param name="proxyUserPassword"></param>
		/// <param name="proxyBypassList"></param>
		/// <param name="userAgent"></param>
		/// <param name="referUri"></param>
		/// <param name="credentialAccount"></param>
		/// <param name="credentialPassword"></param>
		/// <param name="useSecureProtocol"></param>
		/// <param name="secureProtocol"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<string> GetWebPageAsync(string url, Dictionary<string, string> headers, int timeout, string proxyHost = null, int proxyPort = 0, string proxyUsername = null, string proxyUserPassword = null, string[] proxyBypassList = null, string userAgent = null, string referUri = null, string credentialAccount = null, string credentialPassword = null, bool useSecureProtocol = true, SecurityProtocolType secureProtocol = SecurityProtocolType.Ssl3, CancellationToken cancellationToken = default(CancellationToken))
			=> UtilityService.GetWebPageAsync(url, headers, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, UtilityService.GetWebProxy(proxyHost, proxyPort, proxyUsername, proxyUserPassword, proxyBypassList), cancellationToken);

		/// <summary>
		/// Gets the web page
		/// </summary>
		/// <param name="url"></param>
		/// <param name="timeout"></param>
		/// <param name="proxyHost"></param>
		/// <param name="proxyPort"></param>
		/// <param name="proxyUsername"></param>
		/// <param name="proxyUserPassword"></param>
		/// <param name="proxyBypassList"></param>
		/// <param name="userAgent"></param>
		/// <param name="referUri"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<string> GetWebPageAsync(string url, int timeout, string proxyHost = null, int proxyPort = 0, string proxyUsername = null, string proxyUserPassword = null, string[] proxyBypassList = null, string userAgent = null, string referUri = null, CancellationToken cancellationToken = default(CancellationToken))
			=> UtilityService.GetWebPageAsync(url, null, timeout, proxyHost, proxyPort, proxyUsername, proxyUserPassword, proxyBypassList, userAgent, referUri, null, null, true, SecurityProtocolType.Ssl3, cancellationToken);

		/// <summary>
		/// Gets the web page
		/// </summary>
		/// <param name="url"></param>
		/// <param name="referUri"></param>
		/// <param name="userAgent"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<string> GetWebPageAsync(string url, string referUri = null, string userAgent = null, CancellationToken cancellationToken = default(CancellationToken))
			=> UtilityService.GetWebPageAsync(url, 90, null, 0, null, null, null, userAgent, referUri, cancellationToken);
		#endregion

		#region Remove/Clear tags
		/// <summary>
		/// Removes HTML/XML tags
		/// </summary>
		/// <param name="input"></param>
		/// <param name="tag"></param>
		/// <param name="attributeValueToClean"></param>
		/// <returns></returns>
		public static string RemoveTag(string input, string tag, string attributeValueToClean = null)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";
			else if (string.IsNullOrWhiteSpace(tag))
				return input;

			var output = input.Trim();
			var start = output.PositionOf("<" + tag);
			while (start > -1)
			{
				var end = output.PositionOf(">", start);
				if (!string.IsNullOrWhiteSpace(attributeValueToClean))
				{
					var tagAttributes = output.Substring(start, end - start);
					if (tagAttributes.PositionOf(attributeValueToClean) > 0)
					{
						end = output.PositionOf("</" + tag, start);
						output = output.Remove(start, end - start + tag.Length + 3);
						start = output.PositionOf("<" + tag);
					}
					else
					{
						output = output.Remove(start, end - start + 1);
						start = output.PositionOf("<" + tag);
					}
				}
				else
				{
					output = output.Remove(start, end - start + 1);
					start = output.PositionOf("<" + tag);
				}
			}

			start = output.PositionOf("</" + tag);
			while (start > -1)
			{
				int end = output.PositionOf(">", start);
				output = output.Remove(start, end - start + 1);
				start = output.PositionOf("</" + tag);
			}

			return output;
		}

		/// <summary>
		/// Removes all HTML/XML tags
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveTags(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";

			var output = input.Trim();
			var start = output.PositionOf("<");
			while (start > -1)
			{
				var end = output.PositionOf(">", start);
				output = output.Remove(start, end - start + 1);
				start = output.PositionOf("<");
			}

			return output;
		}

		/// <summary>
		/// Removes  Microsoft Office tags
		/// </summary>
		/// <param name="input"></param>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string RemoveMsOfficeTags(string input, string[] tags = null)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";

			var output = input.Trim();
			var msoTags = tags == null || tags.Length < 1
				? "w:|o:|v:|m:|st1:|st2:|st3:|st4:|st5:".Split('|')
				: tags;

			msoTags.ForEach(tag => output = UtilityService.RemoveTag(output, tag));
			return output;
		}

		/// <summary>
		/// Removes attributes of a HTML/XML tags
		/// </summary>
		/// <param name="input"></param>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static string RemoveTagAttributes(string input, string tag)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";
			else if (string.IsNullOrWhiteSpace(tag))
				return input;

			var output = input.Trim();
			var start = output.PositionOf("<" + tag + " ");
			while (start > -1)
			{
				var end = output.PositionOf(">", start + 1);
				if (end > 0)
				{
					output = output.Remove(start, end - start + 1);
					output = output.Insert(start, "<" + tag + ">");
				}
				start = output.PositionOf("<" + tag + " ", start + 1);
			}
			return output;
		}

		/// <summary>
		/// Clears HTML/XML tags (with inner)
		/// </summary>
		/// <param name="input"></param>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static string ClearTag(string input, string tag)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";
			else if (string.IsNullOrWhiteSpace(tag))
				return input;

			var output = input.Trim();
			var start = output.PositionOf("<" + tag);
			while (start > -1)
			{
				var end = output.PositionOf("</" + tag + ">", start);
				if (end > 0)
					output = output.Remove(start, end - start + 3 + tag.Length);
				else
				{
					end = output.PositionOf(">", start);
					output = output.Remove(start, end - start + 1);
				}
				start = output.PositionOf("<" + tag);
			}
			return output;
		}

		/// <summary>
		/// Clears comments tags
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string ClearComments(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";

			var output = input.Trim();
			var start = output.PositionOf("<!--");
			while (start > -1)
			{
				var end = output.PositionOf("-->", start);
				if (end > 0)
					output = output.Remove(start, end - start + 3);
				start = output.PositionOf("<!--", start + 1);
			}
			return output;
		}
		#endregion

		#region Removing whitespaces & breaks
		internal static List<object[]> _RegexNormals = null;

		internal static List<object[]> GetRegEx()
		{
			if (UtilityService._RegexNormals == null)
			{
				UtilityService._RegexNormals = new List<object[]>();

				// remove line-breaks
				UtilityService._RegexNormals.Add(new object[] { new Regex(@">\s+\n<", RegexOptions.IgnoreCase), "> <" });
				UtilityService._RegexNormals.Add(new object[] { new Regex(@">\n<", RegexOptions.IgnoreCase), "><" });

				// white-spaces between tags
				UtilityService._RegexNormals.Add(new object[] { new Regex(@"\s+/>", RegexOptions.IgnoreCase), "/>" });
				UtilityService._RegexNormals.Add(new object[] { new Regex(@"/>\s+<", RegexOptions.IgnoreCase), "/><" });
				UtilityService._RegexNormals.Add(new object[] { new Regex(@">\s+<", RegexOptions.IgnoreCase), "> <" });

				// white-spaces before/after special tags
				var tags = "div,/div,section,/section,nav,/nav,main,/main,header,/header,footer,/footer,p,/p,h1,h2,h3,h4,h5,br,hr,input,textarea,table,tr,/tr,td,ul,/ul,li,select,/select,option,script,/script".Split(',');
				foreach (var tag in tags)
				{
					if (!tag[0].Equals('/'))
						UtilityService._RegexNormals.Add(new object[] { new Regex(@">\s+<" + tag, RegexOptions.IgnoreCase), "><" + tag });
					else
					{
						UtilityService._RegexNormals.Add(new object[] { new Regex(@">\s+<" + tag + @">\s+<", RegexOptions.IgnoreCase), "><" + tag + "><" });
						UtilityService._RegexNormals.Add(new object[] { new Regex(@">\s+<" + tag + @">", RegexOptions.IgnoreCase), "><" + tag + ">" });
						UtilityService._RegexNormals.Add(new object[] { new Regex(@"<" + tag + @">\s+<", RegexOptions.IgnoreCase), "<" + tag + "><" });
					}
				}
			}
			return UtilityService._RegexNormals;
		}

		/// <summary>
		/// Removes whitespaces and breaks
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveWhitespaces(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";

			var output = input.Replace("&nbsp;", " ").Trim();
			var regexs = UtilityService.GetRegEx();
			regexs?.ForEach(regex => output = (regex[0] as Regex).Replace(output, regex[1] as string));
			return output;
		}
		#endregion

		#region Working with files & folders
		static List<string> _FileRemovements = new List<string>() { "\\", "/", "*", "?", "<", ">", "|", ":", "\r", "\n", "\t" };
		static List<string[]> _FileReplacements = new List<string[]>() { new string[] { "\"", "'" }, new string[] { "%20", " " }, new string[] { " ft. ", " & " } };

		/// <summary>
		/// Normalizes the name of a file
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string GetNormalizedFilename(string input)
		{
			var output = input.ConvertCompositeUnicodeToUnicode();

			foreach (var str in UtilityService._FileRemovements)
				output = output.Replace(str, "").Trim();

			foreach (var replacement in UtilityService._FileReplacements)
				output = output.Replace(replacement[0], replacement[1]).Trim();

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
		public static string GetFileSize(FileInfo fileInfo)
		{
			return fileInfo == null || !fileInfo.Exists
				? null
				: fileInfo.Length >= (1024 * 1024 * 1024)
					? (fileInfo.Length.CastAs<double>() / (1024 * 1024 * 1024)).ToString("##0.##") + " G"
					: fileInfo.Length >= (1024 * 1024)
						? (fileInfo.Length.CastAs<double>() / (1024 * 1024)).ToString("##0.##") + " M"
						: fileInfo.Length >= 1024
							? (fileInfo.Length.CastAs<double>() / 1024).ToString("##0.##") + " K"
							: fileInfo.Length.ToString("###0") + " B";
		}

		/// <summary>
		/// Gets size of a file in the friendly text
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string GetFileSize(string filePath)
			=> string.IsNullOrWhiteSpace(filePath)
				? null
				: UtilityService.GetFileSize(new FileInfo(filePath));

		/// <summary>
		/// Gets parts of file path (seperate path and file name)
		/// </summary>
		/// <param name="filePath">The string that presents full path (or full URI) of a file</param>
		/// <param name="removeExtension">true to remove file extension</param>
		/// <returns></returns>
		public static Tuple<string, string> GetFileParts(string filePath, bool removeExtension = true)
		{
			string path = "", filename = "";
			try
			{
				var info = new FileInfo(filePath);
				filename = info.Name;
				path = info.FullName;
				path = path.Left(path.Length - filename.Length - 1);
				if (removeExtension)
					filename = filename.Left(filename.Length - info.Extension.Length);
			}
			catch
			{
				filename = filePath.Trim().Replace("\"", "");
				var start = filename.PositionOf(@"\");
				while (start > -1)
				{
					path += (!path.Equals("") ? @"\" : "") + filename.Substring(0, start);
					filename = filename.Remove(0, start + 1);
					start = filename.PositionOf(@"\");
				}

				start = filename.PositionOf("/");
				while (start > -1)
				{
					path += (!path.Equals("") ? "/" : "") + filename.Substring(0, start);
					filename = filename.Remove(0, start + 1);
					start = filename.PositionOf("/");
				}

				start = filename.PositionOf("?");
				if (start > 0)
					filename = filename.Left(start);

				if (removeExtension)
				{
					var pos = -1;
					start = filename.PositionOf(".");
					while (start > -1)
					{
						pos = start;
						start = filename.PositionOf(".", start + 1);
					}
					filename = filename.Remove(pos);
				}
			}

			return new Tuple<string, string>(path, UtilityService.GetNormalizedFilename(filename));
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
				Directory.GetDirectories(path)
					.Where(folderPath =>
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
					})
					.ForEach(folderPath =>
					{
						searchingPatterns.ForEach(searchingPattern =>
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
						});
					});

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
		public static Task<List<FileInfo>> GetFilesAsync(string path, string searchPatterns = null, bool searchInSubFolder = false, List<string> excludedSubFolders = null, string orderBy = "Name", string orderMode = "Ascending", CancellationToken cancellationToken = default(CancellationToken))
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
		public static Task<List<string>> GetFilePathsAsync(string path, string searchPatterns = null, bool searchInSubFolder = false, List<string> excludedSubFolders = null, string orderBy = "Name", string orderMode = "Ascending", CancellationToken cancellationToken = default(CancellationToken))
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

			UtilityService.GetFiles(source, searchPatterns)
				.ForEach(file =>
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
		public static Task MoveFilesAsync(string source, string destination, string searchPatterns, bool deleteOldFilesBeforeMoving = false, CancellationToken cancellationToken = default(CancellationToken))
			=> UtilityService.ExecuteTask(() => UtilityService.MoveFiles(source, destination, searchPatterns, deleteOldFilesBeforeMoving), cancellationToken);
		#endregion

		#region Working with stream & recyclable memory stream
		/// <summary>
		/// Gets a factory to get recyclable memory stream with RecyclableMemoryStreamManager class to limit LOH fragmentation and improve performance
		/// </summary>
		/// <returns></returns>
		public static Func<MemoryStream> GetRecyclableMemoryStreamFactory()
			=> new Microsoft.IO.RecyclableMemoryStreamManager().GetStream;

		/// <summary>
		/// Gets a factory to get recyclable memory stream with RecyclableMemoryStreamManager class to limit LOH fragmentation and improve performance
		/// </summary>
		/// <param name="blockSize"></param>
		/// <param name="largeBufferMultiple"></param>
		/// <param name="maximumBufferSize"></param>
		/// <returns></returns>
		public static Func<MemoryStream> GetRecyclableMemoryStreamFactory(int blockSize, int largeBufferMultiple, int maximumBufferSize)
			=> new Microsoft.IO.RecyclableMemoryStreamManager(blockSize, largeBufferMultiple, maximumBufferSize).GetStream;

		/// <summary>
		/// Creates an instance of <see cref="MemoryStream">MemoryStream</see> using RecyclableMemoryStream to limit LOH fragmentation and improve performance
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static MemoryStream CreateMemoryStream(byte[] buffer = null, int index = 0, int count = 0)
		{
			var stream = UtilityService.GetRecyclableMemoryStreamFactory()();
			if (buffer != null)
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
		/// Converts this memory stream to array segment of byte
		/// </summary>
		/// <param name="stream"></param>
		/// <remarks>
		/// Try to get buffer first to avoid calling ToArray on the MemoryStream because it allocates a new byte array on the heap.
		/// Avoid this by attempting to access the internal memory stream buffer, this works with supported streams like the recyclable memory stream and writable memory streams
		/// </remarks>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this MemoryStream stream)
		{
			return stream.TryGetBuffer(out ArraySegment<byte> buffer)
				? buffer
				: stream.ToArray().ToArraySegment();
		}

		/// <summary>
		/// Converts this memory stream to array of bytes
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this MemoryStream stream)
		{
			return stream.ToArraySegment().ToBytes();
		}

		/// <summary>
		/// Writes the array segment of bytes to this stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		public static void Write(this Stream stream, ArraySegment<byte> buffer)
		{
			stream.Write(buffer.Array, buffer.Offset, buffer.Count);
		}

		/// <summary>
		/// Writes the array segment of bytes to this stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this Stream stream, ArraySegment<byte> buffer, CancellationToken cancellationToken = default(CancellationToken))
		{
			return stream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken);
		}
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

			using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, TextFileReader.BufferSize, false))
			{
				using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			}
		}

		/// <summary>
		/// Reads a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static string ReadTextFile(string filePath, Encoding encoding = null)
		{
			return !string.IsNullOrWhiteSpace(filePath)
				? UtilityService.ReadTextFile(new FileInfo(filePath), encoding)
				: throw new ArgumentException("File path is invalid", nameof(filePath));
		}

		/// <summary>
		/// Reads a text file
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<string> ReadTextFileAsync(FileInfo fileInfo, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (fileInfo == null || !fileInfo.Exists)
				throw new FileNotFoundException($"The file is not found [{(fileInfo == null ? nameof(fileInfo) : fileInfo.FullName)}]");

			using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, TextFileReader.BufferSize, true))
			{
				using (var reader = encoding != null ? new StreamReader(stream, encoding) : new StreamReader(stream, true))
				{
					return await reader.ReadToEndAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Reads a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static Task<string> ReadTextFileAsync(string filePath, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
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

			else if (content == null)
				return;

			using (var stream = new FileStream(fileInfo.FullName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, TextFileReader.BufferSize, false))
			{
				using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
				{
					writer.Write(content);
				}
			}
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
		public static async Task WriteTextFileAsync(FileInfo fileInfo, string content, bool append = false, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (fileInfo == null)
				throw new ArgumentException("File info is invalid", nameof(fileInfo));

			else if (content == null)
				return;

			using (var stream = new FileStream(fileInfo.FullName, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, TextFileReader.BufferSize, true))
			{
				using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
				{
					await writer.WriteAsync(content).WithCancellationToken(cancellationToken).ConfigureAwait(false);
				}
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
		public static Task WriteTextFileAsync(string filePath, string content, bool append = false, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
			=> !string.IsNullOrWhiteSpace(filePath)
				? UtilityService.WriteTextFileAsync(new FileInfo(filePath), content, append, encoding)
				: Task.FromException<string>(new ArgumentException("File path is invalid", nameof(filePath)));
		#endregion

		#region Read/Write text files (multiple lines)
		/// <summary>
		/// Reads the multiple lines of a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="position"></param>
		/// <param name="totalOfLines"></param>
		/// <param name="lines"></param>
		/// <param name="newPosition"></param>
		public static void ReadTextFile(string filePath, long position, int totalOfLines, out List<string> lines, out long newPosition)
		{
			using (var reader = new TextFileReader(filePath))
			{
				reader.Seek(position);
				lines = reader.ReadLines(totalOfLines);
				newPosition = reader.Position;
			}
		}

		/// <summary>
		/// Reads the multiple lines of a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="position"></param>
		/// <param name="totalOfLines"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<Tuple<List<string>, long>> ReadTextFileAsync(string filePath, long position, int totalOfLines, CancellationToken cancellationToken = default(CancellationToken))
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
		/// <returns></returns>
		public static List<string> ReadTextFile(string filePath, int totalOfLines)
		{
			UtilityService.ReadTextFile(filePath, 0, totalOfLines, out List<string> lines, out long newPosition);
			return lines;
		}

		/// <summary>
		/// Reads the multiple lines of a text file
		/// </summary>
		/// <param name="filePath">The path to text file</param>
		/// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<List<string>> ReadTextFileAsync(string filePath, int totalOfLines, CancellationToken cancellationToken = default(CancellationToken))
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

			if (lines != null && lines.Count > 0)
				using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, TextFileReader.BufferSize))
				{
					using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
					{
						lines.Where(line => line != null).ForEach(line => writer.WriteLine(line));
						writer.Flush();
					}
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
		public static async Task WriteTextFileAsync(string filePath, List<string> lines, bool append = true, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("File path is invalid", nameof(filePath));

			if (lines != null && lines.Count > 0)
				using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, TextFileReader.BufferSize, true))
				{
					using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
					{
						await lines.Where(line => line != null).ForEachAsync((line, token) => writer.WriteLineAsync(line).WithCancellationToken(token), cancellationToken, true, false).ConfigureAwait(false);
						await writer.FlushAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
					}
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
				using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, TextFileReader.BufferSize))
				{
					var data = new byte[fileInfo.Length];
					stream.Read(data, 0, fileInfo.Length.CastAs<int>());
					return data;
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
		public static async Task<byte[]> ReadBinaryFileAsync(FileInfo fileInfo, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (fileInfo != null && fileInfo.Exists)
				using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, TextFileReader.BufferSize, true))
				{
					var data = new byte[fileInfo.Length];
					await stream.ReadAsync(data, 0, fileInfo.Length.CastAs<int>(), cancellationToken).ConfigureAwait(false);
					return data;
				}
			throw new ArgumentException("File info is invalid", nameof(fileInfo));
		}

		/// <summary>
		/// Reads a binary file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<byte[]> ReadBinaryFileAsync(string filePath, CancellationToken cancellationToken = default(CancellationToken))
			=> !string.IsNullOrWhiteSpace(filePath)
				? UtilityService.ReadBinaryFileAsync(new FileInfo(filePath), cancellationToken)
				: Task.FromException<byte[]>(new ArgumentException("File path is invalid", nameof(filePath)));

		/// <summary>
		/// Writes a binary file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static void WriteBinaryFile(string filePath, byte[] content)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath), "File path is invalid");

			using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, TextFileReader.BufferSize))
			{
				stream.Write(content ?? new byte[0], 0, content?.Length ?? 0);
				stream.Flush();
			}
		}

		/// <summary>
		/// Writes a binary file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="content"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task WriteBinaryFileAsync(string filePath, byte[] content, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath), "File path is invalid");

			using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, TextFileReader.BufferSize, true))
			{
				await stream.WriteAsync(content ?? new byte[0], 0, content?.Length ?? 0, cancellationToken).ConfigureAwait(false);
				await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Writes a binary file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="content"></param>
		/// <returns></returns>
		public static void WriteBinaryFile(string filePath, Stream content)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath), "File path is invalid");

			using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, TextFileReader.BufferSize))
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
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task WriteBinaryFileAsync(string filePath, Stream content, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath), "File path is invalid");

			using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, TextFileReader.BufferSize, true))
			{
				var buffer = new byte[TextFileReader.BufferSize];
				var read = await content.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
				while (read > 0)
				{
					await stream.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
					await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
					read = await content.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
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
		public static async Task UploadAsync(this Stream stream, string filename, string url, Action<string, string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream), "Data stream is invalid");

			try
			{
				var stopwatch = Stopwatch.StartNew();
				var results = "";
				using (var http = new HttpClient())
				{
					using (var content = new MultipartFormDataContent("VIEAppsNGX----" + DateTime.Now.ToIsoString()))
					{
						content.Add(new StreamContent(stream), "UploadedFile", filename);
						using (var message = await http.PostAsync(url, content, cancellationToken).ConfigureAwait(false))
						{
							results = await message.Content.ReadAsStringAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
						}
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
		public static async Task UploadAsync(byte[] data, string filename, string url, Action<string, string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data), "Data is invalid");

			using (var stream = UtilityService.CreateMemoryStream(data))
			{
				await stream.UploadAsync(filename, url, onCompleted, onError, cancellationToken).ConfigureAwait(false);
			}
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
		public static async Task UploadFileAsync(string filePath, string url, Action<string, string, string, long> onCompleted = null, Action<string, string, Exception> onError = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath), "File path is null");

			else if (!File.Exists(filePath))
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
		/// <returns></returns>
		public static byte[] Download(string url, Action<string, long> onCompleted = null, Action<string, Exception> onError = null)
		{
			if (string.IsNullOrWhiteSpace(url) || (!url.IsStartsWith("http://") && !url.IsStartsWith("https://")))
				throw new ArgumentNullException(nameof(url), "URL is invalid");

			try
			{
				var stopwatch = Stopwatch.StartNew();

				byte[] data = null;
				using (var webclient = new WebClient())
				{
					data = webclient.DownloadData(url);
				}

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
		/// Downloads data from a remote server
		/// </summary>
		/// <param name="url"></param>
		/// <param name="onCompleted"></param>
		/// <param name="onError"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<byte[]> DownloadAsync(string url, Action<string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrWhiteSpace(url) || (!url.IsStartsWith("http://") && !url.IsStartsWith("https://")))
				throw new ArgumentNullException(nameof(url), "URL is invalid");

			try
			{
				var stopwatch = Stopwatch.StartNew();

				byte[] data = null;
				using (var webclient = new WebClient())
				{
					data = await webclient.DownloadDataTaskAsync(url, cancellationToken).ConfigureAwait(false);
				}

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
		/// <param name="referUri"></param>
		/// <param name="onCompleted"></param>
		/// <param name="onError"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task DownloadFileAsync(string url, string filePath, string referUri, Action<string, string, long> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (!string.IsNullOrWhiteSpace(url) && (url.IsStartsWith("http://") || url.IsStartsWith("https://")))
				try
				{
					var stopwatch = Stopwatch.StartNew();

					using (var webStream = await UtilityService.GetWebResourceAsync(url, referUri, cancellationToken).ConfigureAwait(false))
					{
						using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, TextFileReader.BufferSize, true))
						{
							await webStream.CopyToAsync(fileStream, TextFileReader.BufferSize, cancellationToken).ConfigureAwait(false);
						}
					}

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
		public static Task DownloadFileAsync(string url, string filePath, string referUri = null, Action<string, string> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default(CancellationToken))
			=> UtilityService.DownloadFileAsync(url, filePath, referUri, (uri, path, times) => onCompleted?.Invoke(uri, path), onError, cancellationToken);
		#endregion

		#region Compressions
		/// <summary>
		/// Compresses the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static ArraySegment<byte> Compress(this Stream stream, string mode = "deflate")
		{
			using (var output = UtilityService.CreateMemoryStream())
			{
				using (var compressor = "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
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
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<ArraySegment<byte>> CompressAsync(this Stream stream, string mode = "deflate", CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var output = UtilityService.CreateMemoryStream())
			{
				using (var compressor = "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
				{
					if (stream.CanSeek)
						stream.Seek(0, SeekOrigin.Begin);
					var buffer = new byte[TextFileReader.BufferSize];
					var read = stream is MemoryStream
						? stream.Read(buffer, 0, buffer.Length)
						: await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
					while (read > 0)
					{
						await compressor.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
						read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
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
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static ArraySegment<byte> Compress(this ArraySegment<byte> data, string mode = "deflate")
		{
			using (var output = UtilityService.CreateMemoryStream())
			{
				using (var compressor = "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) as Stream : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
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
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static byte[] Compress(this byte[] data, string mode = "deflate") => data.ToArraySegment().Compress(mode).ToBytes();

		/// <summary>
		/// Decompresses the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode">Decompression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static byte[] Decompress(this Stream stream, string mode = "deflate")
		{
			using (var decompressor = "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress) as Stream : new DeflateStream(stream, CompressionMode.Decompress) as Stream)
			{
				var output = new byte[0];
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
		/// <param name="mode">Decompression mode (deflate or gzip)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<byte[]> DecompressAsync(this Stream stream, string mode = "deflate", CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var decompressor = "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress) as Stream : new DeflateStream(stream, CompressionMode.Decompress) as Stream)
			{
				var output = new byte[0];
				var buffer = new byte[TextFileReader.BufferSize];
				var read = stream is MemoryStream
					? decompressor.Read(buffer, 0, buffer.Length)
					: await decompressor.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
				while (read > 0)
				{
					output = output.Concat(buffer.Take(0, read));
					read = await decompressor.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
				}
				return output;
			}
		}

		/// <summary>
		/// Decompresses the array of bytes
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Decompression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static byte[] Decompress(this byte[] data, string mode = "deflate") => data.ToMemoryStream().Decompress(mode);

		/// <summary>
		/// Decompresses the array segment of bytes
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Decompression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static ArraySegment<byte> Decompress(this ArraySegment<byte> data, string mode = "deflate") => data.ToMemoryStream().Decompress(mode).ToArraySegment();
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
		/// <param name="defaultValue">The default value if the setting is not found</param>
		/// <returns></returns>
		public static T GetAppSetting<T>(this IConfiguration configuration, string path, T defaultValue = default(T))
		{
			var section = configuration.GetAppSetting(path);
			return section != null
				? section.Value.CastAs<T>()
				: defaultValue;
		}

		/// <summary>
		/// Gets a setting of the app (from the XML configuration file [app.config/web.config] - section 'appSettings') with special prefix
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="defaultValue">The default value if the setting is not found</param>
		/// <param name="prefix">The special name prefix of the parameter</param>
		/// <returns></returns>
		public static string GetAppSetting(string name, string defaultValue = null, string prefix = "vieapps")
		{
			var value = !string.IsNullOrWhiteSpace(name)
				? ConfigurationManager.AppSettings[(string.IsNullOrWhiteSpace(prefix) ? "" : prefix + ":") + name.Trim()]
				: null;

			return string.IsNullOrWhiteSpace(value)
				? defaultValue
				: value;
		}

		/// <summary>
		/// Gets a parameter of the app (first from header, then second from query)
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="header">The collection of header</param>
		/// <param name="query">The collection of query</param>
		/// <param name="defaultValue">The default value if the parameter is not found</param>
		/// <returns></returns>
		public static string GetAppParameter(string name, NameValueCollection header, NameValueCollection query, string defaultValue = null)
		{
			var value = string.IsNullOrWhiteSpace(name)
				? null
				: header?[name];

			if (value == null)
				value = string.IsNullOrWhiteSpace(name)
					? null
					: query?[name];

			return string.IsNullOrWhiteSpace(value)
				? defaultValue
				: value;
		}

		/// <summary>
		/// Gets a parameter of the app (first from header, then second from query)
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="header">The collection of header</param>
		/// <param name="query">The collection of query</param>
		/// <param name="defaultValue">The default value if the parameter is not found</param>
		/// <returns></returns>
		public static string GetAppParameter(string name, Dictionary<string, string> header, Dictionary<string, string> query, string defaultValue = null)
			=> UtilityService.GetAppParameter(name, header.ToNameValueCollection(), query.ToNameValueCollection(), defaultValue);
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
		public static void KillProcess(Process process, Action<Process> action = null) => ExternalProcess.Kill(process, action);

		/// <summary>
		/// Kills a process by ID
		/// </summary>
		/// <param name="id">The integer that presents the identity of a process</param>
		/// <param name="action">The action to try to close the process before the process be killed</param>
		public static void KillProcess(int id, Action<Process> action = null) => ExternalProcess.Kill(id, action);
		#endregion

	}

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
		{
			this.AndWords = new List<string>();
			this.OrWords = new List<string>();
			this.NotWords = new List<string>();
			this.AndPhrases = new List<string>();
			this.OrPhrases = new List<string>();
			this.NotPhrases = new List<string>();
			this.Parse(query);
		}

		public List<string> AndWords { get; set; }

		public List<string> OrWords { get; set; }

		public List<string> NotWords { get; set; }

		public List<string> AndPhrases { get; set; }

		public List<string> OrPhrases { get; set; }

		public List<string> NotPhrases { get; set; }

		void Parse(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
				return;

			var searchQuery = this.NormalizeKeywords(query);
			var allWords = new List<string>();
			var allPhrases = new List<string>();

			var start = searchQuery.PositionOf("\"");
			var end = searchQuery.PositionOf("\"", start + 1);
			if (start < 0 || end < 1)
				allWords = allWords.Concat(searchQuery.Replace("\"", "").ToArray(' ').Select(i => i.Trim())).ToList();

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

			allWords.Distinct().ForEach(word =>
			{
				if (word[0].Equals('+'))
					this.AndWords.Add(word.Right(word.Length - 1));
				else if (word[0].Equals('-'))
					this.NotWords.Add(word.Right(word.Length - 1));
				else
					this.OrWords.Add(word);
			});

			allPhrases.Distinct().ForEach(phrase =>
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
	public class AppConfigurationSectionHandler : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			this.Section = section;
			return this;
		}

		/// <summary>
		/// Gets the configuration section
		/// </summary>
		public XmlNode Section { get; private set; } = null;
	}
	#endregion

	//  --------------------------------------------------------------------------------------------

	#region Reader of a text file
	/// <summary>
	/// Extends the System.IO.StreamReader that reads lines from a file in a particular encoding
	/// </summary>
	public sealed class TextFileReader : IDisposable
	{
		// by default, one reading block of Windows is 4K (4096), then use 16K(16384)/32K(32768)/64K(65536)/128K(131072)/256K(262144)/512K(524288)
		// for better performance while working with text file has large line of characters
		public static readonly int BufferSize = 16384;
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
			this._reader = new StreamReader(filePath, true);
		}

		~TextFileReader()
		{
			this.Dispose();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this._reader.Close();
			this._reader.Dispose();
			GC.SuppressFinalize(this);
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
					// shift position back from BaseStream.Position by the number of bytes read into internal buffer
					var charLen = (int)this._reader.GetType().InvokeMember("_charLen", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
					var position = this._reader.BaseStream.Position - charLen;

					// if we have consumed chars from the buffer we need to calculate how many bytes they represent in the current encoding and add that to the position
					var charPos = (int)this._reader.GetType().InvokeMember("_charPos", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
					if (charPos > 0)
					{
						var charBuffer = (char[])this._reader.GetType().InvokeMember("_charBuffer", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
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
		public string ReadLine() => this._reader.ReadLine();

		/// <summary>
		/// Reads a line of characters (from the current position)
		/// </summary>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The next line from file, or null if the end of file is reached</returns>
		public Task<string> ReadLineAsync(CancellationToken cancellationToken = default(CancellationToken)) => this._reader.ReadLineAsync().WithCancellationToken(cancellationToken);

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
		public async Task<List<string>> ReadLinesAsync(int totalOfLines, CancellationToken cancellationToken = default(CancellationToken))
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
		public async Task<List<string>> ReadAllLinesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			// jump to first
			this.Seek(0);

			// read all lines
			var lines = new List<string>();
			var line = await this._reader.ReadLineAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
			while (line != null)
			{
				lines.Add(line);
				line = await this._reader.ReadLineAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
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
		public new BigInteger Next(BigInteger minValue, BigInteger maxValue)
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

}