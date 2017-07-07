#region Related components
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Diagnostics;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods
	/// </summary>
	public static class Utility
	{

		#region UUID & Random number
		/// <summary>
		/// Gets the UUID (unique universal identity - in 128 bits)
		/// </summary>
		/// <param name="uuid">The string that presents an UUID</param>
		/// <returns>The string that presents UUID with hyphens (128 bits)</returns>
		public static string GetUUID(string uuid = null)
		{
			var id = string.IsNullOrWhiteSpace(uuid) ? Guid.NewGuid().ToString("N").ToLower() : uuid.Trim();
			if (!string.IsNullOrWhiteSpace(uuid) && uuid.IndexOf("-") < 0)
			{
				var pos = 8;
				id = id.Insert(pos, "-");
				for (var index = 0; index < 3; index++)
				{
					pos += 5;
					id = id.Insert(pos, "-");
				}
			}
			return id;
		}

		/// <summary>
		/// Gets a new UUID (unique universal identity - in 128 bits)
		/// </summary>
		public static string NewUID
		{
			get
			{
				return Utility.GetUUID();
			}
		}

		static string _BlankUUID = null;

		/// <summary>
		/// Gets the blank UUID
		/// </summary>
		/// <returns></returns>
		public static string GetBlankUUID()
		{
			if (Utility._BlankUUID == null)
				Utility._BlankUUID = new string('0', 32);
			return Utility._BlankUUID;
		}

		/// <summary>
		/// Gets the blank UUID
		/// </summary>
		/// <returns></returns>
		public static string BlankUID
		{
			get
			{
				return Utility.GetBlankUUID();
			}
		}

		static Regex Hexa = new Regex("[^0-9a-fA-F]+");

		/// <summary>
		/// Validates the UUID string
		/// </summary>
		/// <param name="uuid"></param>
		/// <param name="onlyHexa">true to only allow hexa characters</param>
		/// <returns>true if it is valid; otherwise false.</returns>
		public static bool IsValidUUID(this string uuid, bool onlyHexa = true)
		{
			return string.IsNullOrWhiteSpace(uuid) || !uuid.Length.Equals(32)
				? false
				: onlyHexa
					? Utility.Hexa.Replace(uuid, "").Equals(uuid)
					: !uuid.Contains(" ") && !uuid.Contains(";");
		}

		/// <summary>
		/// Converts an identity number to identity string
		/// </summary>
		/// <param name="identity">The number that presents the identity</param>
		/// <returns>Identity number</returns>
		public static string ToIdentity(this long identity)
		{
			// first, convert to hexa-decimal string with max-length is 10
			var hexValue = (Utility.BlankUID + identity.ToString("X").ToLower()).Right(19);

			// second, refine zero characters
			hexValue = hexValue.Replace("00000", "j");
			hexValue = hexValue.Replace("0000", "M");
			hexValue = hexValue.Replace("000", "o");
			hexValue = hexValue.Replace("00", "P");

			// and then, replace numeric characters
			var replacements = new string[] { "Z", "r", "S", "t", "U", "v", "w", "X", "y", "Q" };
			var newIdentity = "";
			for (var index = 0; index < hexValue.Length; index++)
				if (hexValue[index] >= '0' && hexValue[index] <= '9')
				{
					var replacementIndex = Convert.ToInt32(hexValue[index] + "");
					newIdentity += replacements[replacementIndex];
				}
				else
					newIdentity += hexValue[index].ToString();

			// return new identity
			return newIdentity;
		}

		/// <summary>
		/// Converts this identity string to identity number.
		/// </summary>
		/// <param name="identity">The identity string that presents an identity number</param>
		/// <returns>Identity number</returns>
		public static long ToIdentity(this string identity)
		{
			// check first
			if (string.IsNullOrWhiteSpace(identity))
				return 0;

			// then, refine replacements
			var replacements = new string[] { "z", "r", "s", "t", "u", "v", "w", "x", "y", "q" };
			var newIdentity = "";
			var hexValue = identity.ToLower();
			for (var index = 0; index < hexValue.Length; index++)
				if (hexValue[index] >= 'q' && hexValue[index] <= 'z')
				{
					var replacement = hexValue[index] + "";
					var replacementValue = "";
					for (var replacementIndex = 0; replacementIndex < replacements.Length; replacementIndex++)
					{
						if (replacement.Equals(replacements[replacementIndex]))
						{
							replacementValue = replacementIndex.ToString();
							break;
						}
					}
					newIdentity += replacementValue;
				}
				else
					newIdentity += hexValue[index].ToString();

			if (newIdentity.Equals(""))
				return 0;

			// finaly, refine zero characters
			newIdentity = newIdentity.Replace("j", "00000");
			newIdentity = newIdentity.Replace("m", "0000");
			newIdentity = newIdentity.Replace("o", "000");
			newIdentity = newIdentity.Replace("p", "00");
			while (newIdentity[0].Equals('0'))
				newIdentity = newIdentity.Right(newIdentity.Length - 1);

			// convert to long and return value
			return Int64.Parse(newIdentity, System.Globalization.NumberStyles.AllowHexSpecifier);
		}

		/// <summary>
		/// Converts this UUID to identity string
		/// </summary>
		/// <param name="uuid">The GUID value in string that presents the identity</param>
		/// <returns>Identity string</returns>
		public static string ToIdentityString(this string uuid)
		{
			return Int64.Parse(uuid.Left(16), System.Globalization.NumberStyles.HexNumber).ToIdentity() + "." + Int64.Parse(uuid.Right(16), System.Globalization.NumberStyles.HexNumber).ToIdentity();
		}

		/// <summary>
		/// Converts this identity string to UUID string
		/// </summary>
		/// <param name="identity">The string that presents an identity</param>
		/// <returns>UUID string that presents the identity string</returns>
		public static string ToUUIDString(string identity)
		{
			// check first
			if (string.IsNullOrWhiteSpace(identity) || identity.IndexOf(".") < 0)
				return null;

			var pos = identity.IndexOf(".");
			var left = identity.Left(pos).ToIdentity();
			var right = identity.Right(identity.Length - pos - 1).ToIdentity();

			return left.ToString("X") + right.ToString("X");
		}

		static Random _Random = new Random();

		/// <summary>
		/// Gets the random number between min and max
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int GetRandomNumber(int min = 0, int max = Int32.MaxValue)
		{
			return Utility._Random.Next(min, max);
		}
		#endregion

		#region Get external resource/webpage via HttpWebRequest object
		internal static string[] UserAgents = new string[] {
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

		public static string SpiderUserAgent
		{
			get
			{
				return Utility.UserAgents[Utility.GetRandomNumber(0, Utility.UserAgents.Length - 1)];
			}
		}

		public static string MobileUserAgent
		{
			get
			{
				return "Mozilla/5.0 (iPhone; CPU iPhone OS 7_0_4 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11B554a Safari/9537.53";
			}
		}

		public static string DesktopUserAgent
		{
			get
			{
				return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.98 Safari/537.36";
			}
		}

		public static CredentialCache GetWebCredential(string uri, string account, string password, bool useSecureProtocol, SecurityProtocolType secureProtocol)
		{
			if (useSecureProtocol)
				ServicePointManager.SecurityProtocol = secureProtocol;

			var credentialCache = new CredentialCache();
			credentialCache.Add(new Uri(uri), "Basic", new NetworkCredential(account, password));
			return credentialCache;
		}

		public static WebProxy GetWebProxy(string proxyHost, int proxyPort, string proxyUsername, string proxyUserPassword, string[] proxyBypassList)
		{
			WebProxy proxy = null;
			if (!string.IsNullOrWhiteSpace(proxyHost))
			{
				if (proxyBypassList == null || proxyBypassList.Length < 1)
					proxy = new WebProxy(proxyHost, proxyPort);
				else
				{
					var proxyAddress = new Uri("http://" + proxyHost + ":" + proxyPort.ToString());
					proxy = new WebProxy(proxyAddress, true, proxyBypassList);
				}
				if (!string.IsNullOrWhiteSpace(proxyUsername) && !string.IsNullOrWhiteSpace(proxyUserPassword))
					proxy.Credentials = new NetworkCredential(proxyUsername, proxyUserPassword);
			}
			return proxy;
		}

		public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest httpRequest, CancellationToken cancellationToken)
		{
			using (cancellationToken.Register(() => httpRequest.Abort(), useSynchronizationContext: false))
			{
				try
				{
					return await httpRequest.GetResponseAsync() as HttpWebResponse;
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

		public static async Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, Dictionary<string, string> headers, Cookie[] cookies, string body, string contentType, int timeout, string userAgent, string referUri, CredentialCache credential, WebProxy proxy, CancellationToken cancellationToken)
		{
			// get the request object to handle on the remote resource
			var webRequest = WebRequest.Create(uri) as HttpWebRequest;

			// set properties
			webRequest.Method = string.IsNullOrWhiteSpace(method) ? "GET" : method.ToUpper();
			webRequest.Timeout = timeout * 1000;
			webRequest.ServicePoint.Expect100Continue = false;
			webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			webRequest.UserAgent = !string.IsNullOrWhiteSpace(userAgent) ? userAgent : Utility.DesktopUserAgent;
			if (!string.IsNullOrWhiteSpace(referUri))
				webRequest.Referer = referUri;

			// headers
			if (headers != null)
				foreach (var header in headers)
					if (!header.Key.Equals("Accept-Encoding"))
						webRequest.Headers.Add(header.Key, header.Value);

			// cookies
			if (cookies != null && cookies.Length > 0 && webRequest.SupportsCookieContainer)
				foreach (var cookie in cookies)
					webRequest.CookieContainer.Add(cookie);

			// compression
			webRequest.Headers.Add("Accept-Encoding", "deflate,gzip");
			webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

			// credential
			if (credential != null)
			{
				webRequest.Credentials = credential;
				webRequest.PreAuthenticate = true;
			}

			// proxy
			if (proxy != null)
				webRequest.Proxy = proxy;

			// data to post/put
			if (!string.IsNullOrWhiteSpace(body) && (webRequest.Method.Equals("POST") || webRequest.Method.Equals("PUT")))
			{
				if (!string.IsNullOrWhiteSpace(contentType))
					webRequest.ContentType = contentType;

				using (var requestWriter = new StreamWriter(await webRequest.GetRequestStreamAsync()))
				{
					await requestWriter.WriteAsync(body);
				}
			}

			// switch off certificate validation (http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using)
			ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
			{
				return true;
			};

			// make request and return response stream
			try
			{
				return await webRequest.GetResponseAsync(cancellationToken);
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
				string responseBody = "";
				if (ex.Status.Equals(WebExceptionStatus.ProtocolError))
				{
					using (var stream = (ex.Response as HttpWebResponse).GetResponseStream())
					{
						using (var reader = new StreamReader(stream, true))
						{
							responseBody = await reader.ReadToEndAsync();
						}
					}
				}
				throw new RemoteServerErrorException("Error occurred at remote server", responseBody, ex.Response != null && ex.Response.ResponseUri != null ? ex.Response.ResponseUri.AbsoluteUri : uri, ex);
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

		public static async Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, Dictionary<string, string> headers, string body, string contentType, int timeout, string userAgent, string referUri, string credentialAccount, string credentialPassword, bool useSecureProtocol, SecurityProtocolType secureProtocol, WebProxy proxy, CancellationToken cancellationToken)
		{
			// credential
			CredentialCache credential = null;
			if (!string.IsNullOrWhiteSpace(credentialAccount) && !string.IsNullOrWhiteSpace(credentialPassword))
				credential = Utility.GetWebCredential(uri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol);

			// make request
			return await Utility.GetWebResponseAsync(method, uri, headers, null, body, contentType, timeout, userAgent, referUri, credential, proxy, cancellationToken);
		}

		public static async Task<Stream> GetWebResourceAsync(string method, string uri, Dictionary<string, string> headers, string body, string contentType, int timeout, string userAgent, string referUri, string credentialAccount, string credentialPassword, bool useSecureProtocol, SecurityProtocolType secureProtocol, WebProxy proxy, CancellationToken cancellationToken)
		{
			var webResponse = await Utility.GetWebResponseAsync(method, uri, headers, body, contentType, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, proxy, cancellationToken);
			return webResponse.GetResponseStream();
		}

		public static Task<Stream> GetWebResourceAsync(string uri, string referUri, CancellationToken cancellationToken)
		{
			return Utility.GetWebResourceAsync("GET", uri, null, null, null, 90, Utility.SpiderUserAgent, referUri, null, null, true, SecurityProtocolType.Ssl3, null, cancellationToken);
		}

		public static Task<Stream> GetWebResourceAsync(string uri, string referUri)
		{
			return Utility.GetWebResourceAsync(uri, referUri, CancellationToken.None);
		}

		public static Task<Stream> GetWebResourceAsync(string uri)
		{
			return Utility.GetWebResourceAsync(uri, null);
		}

		public static async Task<string> GetWebPageAsync(string url, Dictionary<string, string> headers, int timeout, string userAgent, string referUri, string credentialAccount, string credentialPassword, bool useSecureProtocol, SecurityProtocolType secureProtocol, WebProxy proxy, CancellationToken cancellationToken)
		{
			// check uri
			if (string.IsNullOrWhiteSpace(url))
				return null;

			// get stream of external resource as HTML
			string html = "";
			using (var webResponse = await Utility.GetWebResponseAsync("GET", url, headers, null, "text/html", timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, proxy, cancellationToken))
			{
				using (var stream = webResponse.GetResponseStream())
				{
					using (var reader = new StreamReader(stream, true))
					{
						html = await reader.ReadToEndAsync();
					}
				}
			}

			// decode and return HTML
			return html.HtmlDecode();
		}

		public static Task<string> GetWebPageAsync(string url, Dictionary<string, string> headers, int timeout, string proxyHost, int proxyPort, string proxyUsername, string proxyUserPassword, string[] proxyBypassList, string userAgent, string referUri, string credentialAccount, string credentialPassword, bool useSecureProtocol, SecurityProtocolType secureProtocol, CancellationToken cancellationToken)
		{
			return Utility.GetWebPageAsync(url, headers, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, Utility.GetWebProxy(proxyHost, proxyPort, proxyUsername, proxyUserPassword, proxyBypassList), cancellationToken);
		}

		public static Task<string> GetWebPageAsync(string url, Dictionary<string, string> headers, int timeout, string proxyHost, int proxyPort, string proxyUsername, string proxyUserPassword, string[] proxyBypassList, string userAgent, string referUri, CancellationToken cancellationToken)
		{
			return Utility.GetWebPageAsync(url, headers, timeout, proxyHost, proxyPort, proxyUsername, proxyUserPassword, proxyBypassList, userAgent, referUri, null, null, true, SecurityProtocolType.Ssl3, cancellationToken);
		}

		public static Task<string> GetWebPageAsync(string url, int timeout, string proxyHost = null, int proxyPort = 0, string proxyUsername = null, string proxyUserPassword = null, string[] proxyBypassList = null, string userAgent = null, string referUri = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Utility.GetWebPageAsync(url, null, timeout, proxyHost, proxyPort, proxyUsername, proxyUserPassword, proxyBypassList, userAgent, referUri, cancellationToken);
		}

		public static Task<string> GetWebPageAsync(string url, string referUri = null, string userAgent = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Utility.GetWebPageAsync(url, 45, null, 0, null, null, null, userAgent, referUri, cancellationToken);
		}
		#endregion

		#region Write file/stream directly to HttpResponse output stream
		/// <summary>
		/// Gets max request length (defined in 'system.web/httpRuntime' section of web.config file)
		/// </summary>
		public static int MaxRequestLength
		{
			get
			{
				var maxRequestLength = 30;
				var httpRuntime = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
				if (httpRuntime != null)
					maxRequestLength = httpRuntime.MaxRequestLength / 1024;
				return maxRequestLength;
			}
		}

		internal static long MinSmallFileSize = 1024 * 40;                              // 40 KB
		internal static long MaxSmallFileSize = 1024 * 1024 * 2;                // 02 MB
		internal static long MaxAllowedSize = Utility.MaxRequestLength * 1024 * 1024;

		static string GetRequestETag(HttpContext context)
		{
			// IE or common browser
			var requestETag = context.Request.Headers["If-Range"];

			// FireFox
			if (string.IsNullOrWhiteSpace(requestETag))
				requestETag = context.Request.Headers["If-Match"];

			// normalize
			if (!string.IsNullOrWhiteSpace(requestETag))
			{
				while (requestETag.StartsWith("\""))
					requestETag = requestETag.Right(requestETag.Length - 1);
				while (requestETag.EndsWith("\""))
					requestETag = requestETag.Left(requestETag.Length - 1);
			}

			// return the request ETag for resume downloading
			return requestETag;
		}

		/// <summary>
		/// Writes the content of the file directly to output stream
		/// </summary>
		/// <param name="context"></param>
		/// <param name="filePath">The path to file</param>
		/// <param name="contentType">The MIME type</param>
		/// <param name="eTag">The entity tag</param>
		/// <returns></returns>
		public static async Task WriteFileToOutputAsync(this HttpContext context, string filePath, string contentType, string eTag)
		{
			await context.WriteFileToOutputAsync(filePath, contentType, null, eTag);
		}

		/// <summary>
		/// Writes the content of the file directly to output stream
		/// </summary>
		/// <param name="context"></param>
		/// <param name="filePath">The path to file</param>
		/// <param name="contentType">The MIME type</param>
		/// <param name="contentDisposition">The string that presents name of attachment file, let it empty/null for writting showing/displaying (not for downloading attachment file)</param>
		/// <param name="eTag">The entity tag</param>
		/// <returns></returns>
		public static async Task WriteFileToOutputAsync(this HttpContext context, string filePath, string contentType, string contentDisposition, string eTag)
		{
			var fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
				throw new FileNotFoundException("File is not found: [" + Utility.GetFileParts(filePath, false)[1] + "]");

			using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				await context.WriteStreamToOutputAsync(stream, contentType, contentDisposition, eTag, fileInfo.LastWriteTime.ToHttpString(), (int)Utility.MinSmallFileSize);
			}
		}

		/// <summary>
		/// Writes the binary data directly to output stream
		/// </summary>
		/// <param name="context"></param>
		/// <param name="data">The data to write</param>
		/// <param name="contentType">The MIME type</param>
		/// <param name="contentDisposition">The string that presents name of attachment file, let it empty/null for writting showing/displaying (not for downloading attachment file)</param>
		/// <param name="eTag">The entity tag</param>
		/// <param name="lastModified">The last-modified time in HTTP date-time format</param>
		/// <returns></returns>
		public static async Task WriteDataToOutputAsync(this HttpContext context, byte[] data, string contentType, string contentDisposition, string eTag, string lastModified)
		{
			using (MemoryStream stream = new MemoryStream(data))
			{
				await context.WriteStreamToOutputAsync(stream, contentType, contentDisposition, eTag, lastModified, (int)Utility.MinSmallFileSize);
			}
		}

		/// <summary>
		/// Writes the stream directly to output stream
		/// </summary>
		/// <param name="context"></param>
		/// <param name="stream">The stream to write</param>
		/// <param name="contentType">The MIME type</param>
		/// <param name="contentDisposition">The string that presents name of attachment file, let it empty/null for writting showing/displaying (not for downloading attachment file)</param>
		/// <param name="eTag">The entity tag</param>
		/// <param name="lastModified">The last-modified time in HTTP date-time format</param>
		/// <param name="blockSize">Size of one block to write</param>
		/// <returns></returns>
		public static async Task WriteStreamToOutputAsync(this HttpContext context, Stream stream, string contentType, string contentDisposition, string eTag, string lastModified, int blockSize)
		{
			// validate whether the file is too large
			var totalBytes = stream.Length;
			if (totalBytes > Utility.MaxAllowedSize)
			{
				context.Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
				context.Response.StatusDescription = "Request Entity Too Large";
				return;
			}

			// check ETag for supporting resumeable downloaders
			if (!string.IsNullOrWhiteSpace(eTag))
			{
				var requestETag = Utility.GetRequestETag(context);
				if (!string.IsNullOrWhiteSpace(requestETag) && !eTag.Equals(requestETag))
				{
					context.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
					context.Response.StatusDescription = "Precondition Failed";
					return;
				}
			}

			// prepare position for flushing as partial blocks
			var flushAsPartialContent = false;
			long startBytes = 0, endBytes = totalBytes - 1;
			if (context.Request.Headers["Range"] != null)
			{
				var requestedRange = context.Request.Headers["Range"];
				var range = requestedRange.Split(new char[] { '=', '-' });

				startBytes = Convert.ToInt64(range[1]);
				if (startBytes >= totalBytes)
				{
					context.Response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
					context.Response.StatusDescription = "Precondition Failed";
					return;
				}

				flushAsPartialContent = true;

				if (startBytes < 0)
					startBytes = 0;

				try
				{
					endBytes = Convert.ToInt64(range[2]);
				}
				catch { }
				if (endBytes > totalBytes - 1)
					endBytes = totalBytes - 1;
			}

			// prepare headers
			var headers = new List<string[]>();

			if (!string.IsNullOrWhiteSpace(lastModified))
				headers.Add(new string[] { "Last-Modified", lastModified });

			if (!string.IsNullOrWhiteSpace(eTag))
			{
				headers.Add(new string[] { "Accept-Ranges", "bytes" });
				headers.Add(new string[] { "ETag", "\"" + eTag + "\"" });
			}

			if (flushAsPartialContent && startBytes > -1)
				headers.Add(new string[] { "Content-Range", string.Format(" bytes {0}-{1}/{2}", startBytes, endBytes, totalBytes) });

			headers.Add(new string[] { "Content-Length", ((endBytes - startBytes) + 1).ToString() });

			if (!string.IsNullOrWhiteSpace(contentDisposition))
				headers.Add(new string[] { "Content-Disposition", "Attachment; Filename=\"" + contentDisposition + "\"" });

			// flush headers to HttpResponse output stream
			try
			{
				context.Response.Clear();
				context.Response.Buffer = false;
				context.Response.ContentEncoding = Encoding.UTF8;
				context.Response.ContentType = contentType;

				// status code of partial content
				if (flushAsPartialContent)
				{
					context.Response.StatusCode = (int)HttpStatusCode.PartialContent;
					context.Response.StatusDescription = "Partial Content";
				}

				foreach (var header in headers)
					context.Response.AppendHeader(header[0], header[1]);

				await context.Response.FlushAsync();
			}
			catch (HttpException ex)
			{
				var isDisconnected = ex.Message.Contains("0x800704CD") || ex.Message.Contains("0x800703E3") || ex.Message.Contains("The remote host closed the connection");
				if (!isDisconnected)
					throw ex;
			}
			catch (Exception)
			{
				throw;
			}

			// write small file directly to output stream
			if (!flushAsPartialContent && totalBytes <= Utility.MaxSmallFileSize)
			{
				try
				{
					var isDisconnected = false;
					var data = new byte[totalBytes];
					var readBytes = totalBytes <= Utility.MinSmallFileSize ? stream.Read(data, 0, (int)totalBytes) : await stream.ReadAsync(data, 0, (int)totalBytes);
					try
					{
						await context.Response.OutputStream.WriteAsync(data, 0, readBytes);
					}
					catch (HttpException ex)
					{
						isDisconnected = ex.Message.Contains("0x800704CD") || ex.Message.Contains("0x800703E3") || ex.Message.Contains("The remote host closed the connection");
						if (!isDisconnected)
							throw ex;
					}
					catch (Exception ex)
					{
						throw ex;
					}

					// flush the written buffer to client and update cache
					if (!isDisconnected)
					{
						try
						{
							await context.Response.FlushAsync();
						}
						catch (Exception)
						{
							throw;
						}
					}
				}
				catch (Exception)
				{
					throw;
				}
			}

			// flush to output stream
			else
			{
				// prepare blocks for writing
				var packSize = blockSize;
				if (packSize > (endBytes - startBytes))
					packSize = (int)(endBytes - startBytes) + 1;
				var totalBlocks = (int)Math.Ceiling((endBytes - startBytes + 0.0) / packSize);

				// jump to requested position
				stream.Seek(startBytes > 0 ? startBytes : 0, SeekOrigin.Begin);

				// read and flush stream data to response stream
				var isDisconnected = false;
				var readBlocks = 0;
				while (readBlocks < totalBlocks)
				{
					// the client is still connected
					if (context.Response.IsClientConnected)
						try
						{
							var buffer = new byte[packSize];
							var readBytes = await stream.ReadAsync(buffer, 0, packSize);
							if (readBytes > 0)
							{
								// write data to output stream
								try
								{
									await context.Response.OutputStream.WriteAsync(buffer, 0, readBytes);
								}
								catch (HttpException ex)
								{
									isDisconnected = ex.Message.Contains("0x800704CD") || ex.Message.Contains("0x800703E3") || ex.Message.Contains("The remote host closed the connection");
									if (!isDisconnected)
										throw ex;
									else
										break;
								}
								catch (Exception)
								{
									throw;
								}

								// flush the written buffer to client
								if (!isDisconnected)
									try
									{
										await context.Response.FlushAsync();
									}
									catch (Exception ex)
									{
										throw ex;
									}
							}
							readBlocks++;
						}
						catch (Exception ex)
						{
							throw ex;
						}

					// the client is disconnected
					else
					{
						isDisconnected = true;
						break;
					}
				}
			}
		}
		#endregion

		#region Remove/Clear tags
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

		public static string RemoveMsOfficeTags(string input, string[] tags = null)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";

			var output = input.Trim();
			var msoTags = tags == null || tags.Length < 1 ? "w:|o:|v:|m:|st1:".Split('|') : tags;
			foreach (var tag in msoTags)
				output = Utility.RemoveTag(output, tag);
			return output;
		}

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

		public static string ClearComments(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";

			var output = input.Trim();
			var start = output.PositionOf("<!--");
			while (start > -1)
			{
				int end = output.PositionOf("-->", start);
				if (end > 0)
					output = output.Remove(start, end - start + 3);
				start = output.PositionOf("<!--", start + 1);
			}
			return output;
		}
		#endregion

		#region Removing white-space methods
		internal static List<object[]> _RegexNormals = null;

		internal static List<object[]> GetRegEx()
		{
			if (Utility._RegexNormals == null)
			{
				Utility._RegexNormals = new List<object[]>();

				// remove line-breaks
				Utility._RegexNormals.Add(new object[] { new Regex(@">\s+\n<", RegexOptions.IgnoreCase), "> <" });
				Utility._RegexNormals.Add(new object[] { new Regex(@">\n<", RegexOptions.IgnoreCase), "><" });

				// white-spaces between tags
				Utility._RegexNormals.Add(new object[] { new Regex(@"\s+/>", RegexOptions.IgnoreCase), "/>" });
				Utility._RegexNormals.Add(new object[] { new Regex(@"/>\s+<", RegexOptions.IgnoreCase), "/><" });
				Utility._RegexNormals.Add(new object[] { new Regex(@">\s+<", RegexOptions.IgnoreCase), "> <" });

				// white-spaces before/after special tags
				var tags = "div,/div,section,/section,nav,/nav,main,/main,header,/header,footer,/footer,p,/p,h1,h2,h3,h4,h5,br,hr,input,textarea,table,tr,/tr,td,ul,/ul,li,select,/select,option,script,/script".Split(',');
				foreach (var tag in tags)
				{
					if (!tag[0].Equals('/'))
						Utility._RegexNormals.Add(new object[] { new Regex(@">\s+<" + tag, RegexOptions.IgnoreCase), "><" + tag });
					else
					{
						Utility._RegexNormals.Add(new object[] { new Regex(@">\s+<" + tag + @">\s+<", RegexOptions.IgnoreCase), "><" + tag + "><" });
						Utility._RegexNormals.Add(new object[] { new Regex(@">\s+<" + tag + @">", RegexOptions.IgnoreCase), "><" + tag + ">" });
						Utility._RegexNormals.Add(new object[] { new Regex(@"<" + tag + @">\s+<", RegexOptions.IgnoreCase), "<" + tag + "><" });
					}
				}
			}
			return Utility._RegexNormals;
		}

		public static string RemoveWhitespaces(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return "";

			var output = input.Replace("&nbsp;", " ").Trim();
			var regexs = Utility.GetRegEx();
			if (regexs != null)
				foreach (var regex in regexs)
					output = (regex[0] as Regex).Replace(output, regex[1] as string);
			return output;
		}
		#endregion

		#region Working with process
		public static int RunProcess(string filePath, string arguments, EventHandler onCompleted = null)
		{
			var psi = new ProcessStartInfo()
			{
				FileName = filePath,
				Arguments = string.IsNullOrWhiteSpace(arguments) ? "" : "\"" + arguments + "\"",
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = false,
				RedirectStandardError = false,
				UseShellExecute = false,
				ErrorDialog = false,
			};

			var process = new Process() { StartInfo = psi };
			if (onCompleted != null)
			{
				process.EnableRaisingEvents = true;
				process.Exited += new EventHandler(onCompleted);
			}
			process.Start();

			return process.Id;
		}

		public static void KillProcess(int id)
		{
			var process = Process.GetProcessById(id);
			if (process != null)
				process.Kill();
		}
		#endregion

		#region Working with files & folders
		static List<string> _FileRemovements = new List<string>() { "\\", "/", "*", "?", "<", ">", "|", ":", "\r", "\n", "\t" };
		static List<string[]> _FileReplacements = new List<string[]>() { new string[] { "\"", "'" }, new string[] { "%20", " " }, new string[] { " ft. ", " & " } };

		public static string GetNormalizedFilename(string input)
		{
			var output = input.ConvertCompositeUnicodeToUnicode();

			foreach (var str in Utility._FileRemovements)
				output = output.Replace(str, "").Trim();

			foreach (var replacement in Utility._FileReplacements)
				output = output.Replace(replacement[0], replacement[1]).Trim();

			if (output.IsStartsWith("con."))
				while (output.IndexOf(".") > -1)
					output = output.Replace(".", "");

			return output;
		}

		public static string[] GetFileParts(string filePath, bool removeExtension = true)
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
				var start = filename.PositionOf("\\");
				while (start > -1)
				{
					path += (!path.Equals("") ? "\\" : "") + filename.Substring(0, start);
					filename = filename.Remove(0, start + 1);
					start = filename.PositionOf("\\");
				}

				start = filename.PositionOf("/");
				while (start > -1)
				{
					path += (!path.Equals("") ? "\\" : "") + filename.Substring(0, start);
					filename = filename.Remove(0, start + 1);
					start = filename.PositionOf("/");
				}

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

			return new string[] { path, Utility.GetNormalizedFilename(filename) };
		}

		public static List<FileInfo> GetFiles(string path, string searchPatterns, bool searchInSubFolder = false, List<string> excludedSubFolders = null)
		{
			if (!Directory.Exists(path))
				throw new DirectoryNotFoundException("The folder is not found [" + path + "]");

			var fileInfos = new List<FileInfo>();
			var searchingPatterns = string.IsNullOrWhiteSpace(searchPatterns)
				? new string[] { "*.*" }
				: searchPatterns.ToArray('|', true);

			searchingPatterns.ForEach(searchingPattern =>
			{
				var files = Directory.GetFiles(path, searchingPattern)
					.Select(p => new FileInfo(p))
					.OrderBy(f => f.Name)
					.ToList();
				fileInfos.Append(files);
			});

			if (searchInSubFolder)
			{
				var folderPaths = Directory.GetDirectories(path);
				if (folderPaths != null && folderPaths.Length > 0)
					foreach (var folderPath in folderPaths)
					{
						var isExcluded = false;
						if (excludedSubFolders != null && excludedSubFolders.Count > 0)
							foreach (var excludedFolder in excludedSubFolders)
							{
								isExcluded = folderPath.IsEndsWith("\\" + excludedFolder);
								if (isExcluded)
									break;
							}

						if (!isExcluded)
							searchingPatterns.ForEach(searchingPattern =>
							{
								var files = Directory.GetFiles(folderPath, searchingPattern)
									.Select(p => new FileInfo(p))
									.OrderBy(f => f.Name)
									.ToList();
								fileInfos.Append(files);
							});
					}
			}

			return fileInfos;
		}

		public static List<string> GetFilePaths(string path, string searchPatterns, bool searchInSubFolder = false, List<string> excludedSubFolders = null)
		{
			return Utility.GetFiles(path, searchPatterns, searchInSubFolder, excludedSubFolders)
				.Select(f => f.FullName)
				.ToList();
		}
		#endregion

		#region Read/Write text files
		public static string ReadTextFile(string filePath, Encoding encoding = default(UTF8Encoding))
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("The file is not found [" + filePath + "]");

			using (var reader = new StreamReader(filePath, encoding != null ? encoding : Encoding.UTF8, true))
			{
				try
				{
					return reader.ReadToEnd();
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		public static async Task<string> ReadTextFileAsync(string filePath, Encoding encoding = default(UTF8Encoding))
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("The file is not found [" + filePath + "]");

			using (var reader = new StreamReader(filePath, encoding != null ? encoding : Encoding.UTF8, true))
			{
				try
				{
					return await reader.ReadToEndAsync();
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
		}

		public static void WriteTextFile(string filePath, string content, bool append = false, Encoding encoding = default(UTF8Encoding))
		{
			if (string.IsNullOrWhiteSpace(filePath) || content == null)
				return;

			using (var file = new StreamWriter(filePath, append, encoding != null ? encoding : Encoding.UTF8))
			{
				try
				{
					file.Write(content);
				}
				catch { }
			}
		}

		public static async Task WriteTextFileAsync(string filePath, string content, bool append = false, Encoding encoding = default(UTF8Encoding))
		{
			if (string.IsNullOrWhiteSpace(filePath) || content == null)
				return;

			using (var file = new StreamWriter(filePath, append, encoding != null ? encoding : Encoding.UTF8))
			{
				try
				{
					await file.WriteAsync(content);
				}
				catch { }
			}
		}
		#endregion

		#region Read/Write text files (multiple lines)

		#region Helper class for reading text file
		/// <summary>
		/// Implements a System.IO.BinaryReader that reads block of characters from a file stream in a particular encoding.
		/// </summary>
		public sealed class TextFileReader : IDisposable
		{

			// by default, one reading block of Windows is 4K (4096), then use 64K(65536)/128K(131072)/256K(262144)/512K(524288)
			// for better performance while working with text file has large line of characters
			public static readonly int BufferSize = 65536;

			#region Information of one line after reading
			internal sealed class TextLine
			{
				public TextLine() { }

				string _line = "";
				public string Line
				{
					get { return _line; }
					set { _line = value; }
				}

				long _position = 0;
				public long Position
				{
					get { return _position; }
					set { _position = value; }
				}
			}
			#endregion

			FileStream _fileStream = null;
			BinaryReader _binReader = null;
			StreamReader _streamReader = null;

			Queue<TextLine> _lines = null;
			StringBuilder _builder = null;

			Encoding _encoding = Encoding.UTF8;
			long _length = -1;
			long _position = 0;

			/// <summary>
			/// Initializes a new instance of the <see cref="TextFileReader"/> class.
			/// </summary>
			/// <param name="filePath">The path to text file.</param>
			/// <param name="detectEncoding">if set to <c>true</c>, then detect encoding of text file.</param>
			public TextFileReader(string filePath, bool detectEncoding = true)
			{
				// check existed
				this.Existed(filePath);

				// default encoding is UTF-8
				Encoding encoding = Encoding.UTF8;

				// detect encoding
				if (detectEncoding)
				{
					// create streams to detect encoding
					this._fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
					this._streamReader = new StreamReader(this._fileStream, true);

					// get encoding
					encoding = this._streamReader.CurrentEncoding;

					// reset stream (jump to first)
					this._streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
				}

				// initialize reader
				this.InitializeReader(filePath, encoding);
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="TextFileReader"/> class.
			/// </summary>
			/// <param name="filePath">The path to text file.</param>
			/// <param name="encoding">The encoding of text file.</param>
			public TextFileReader(string filePath, Encoding encoding)
			{
				// check existed
				this.Existed(filePath);

				// initialize reader
				this.InitializeReader(filePath, encoding);
			}

			/// <summary>
			/// Checks existed of the file.
			/// </summary>
			/// <param name="filePath">The path to text file.</param>
			void Existed(string filePath)
			{
				if (string.IsNullOrWhiteSpace(filePath))
					throw new ApplicationException("No path");

				else if (!File.Exists(filePath))
					throw new FileNotFoundException("File (" + filePath + ") is not found.");
			}

			/// <summary>
			/// Initializes the reader.
			/// </summary>
			/// <param name="filePath">The path to text file.</param>
			/// <param name="encoding">The encoding of text file.</param>
			void InitializeReader(string filePath, Encoding encoding)
			{
				// create streams and builders
				if (this._fileStream == null)
					this._fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

				if (this._binReader == null)
					this._binReader = new BinaryReader(this._fileStream, encoding);

				if (this._builder == null)
					this._builder = new StringBuilder();

				if (this._lines == null)
					this._lines = new Queue<TextLine>();

				// assign some helper attributes
				this._length = this._fileStream.Length;
				this._encoding = encoding;
			}

			/// <summary>
			/// Gets the position of file after reading last line.
			/// </summary>
			public long Position { get { return this._position; } }

			/// <summary>
			/// Gets the current encoding of text file.
			/// </summary>
			public Encoding Encoding { get { return this._encoding; } }

			/// <summary>
			/// Gets the length of text file (in bytes).
			/// </summary>
			public long Length { get { return this._length; } }

			/// <summary>
			/// Sets the position within the current stream (from the beginning position)
			/// </summary>
			/// <param name="offset">A byte offset relative to the origin parameter</param>
			/// <returns>The new position within the current stream</returns>
			public long Seek(long offset)
			{
				return this.Seek(offset, SeekOrigin.Begin);
			}

			/// <summary>
			/// Sets the position within the current stream (read next line from this position)
			/// </summary>
			/// <param name="offset">A byte offset relative to the origin parameter</param>
			/// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position</param>
			/// <returns>The new position within the current stream</returns>
			public long Seek(long offset, SeekOrigin origin)
			{
				// seek and reset
				this._position = this._binReader.BaseStream.Seek(offset, origin);
				this._builder = new StringBuilder();
				this._lines = new Queue<TextLine>();

				// return new position
				return this._position;
			}

			/// <summary>
			/// Reads all lines of characters from the current stream (from the begin to end) and returns the data as a collection of string.
			/// </summary>
			/// <returns>The next lines from the input stream, or empty collectoin if the end of the input stream is reached</returns>
			public List<string> ReadAllLines()
			{
				// create new stream if need
				if (this._streamReader == null)
					this._streamReader = new StreamReader(this._fileStream, this._encoding);

				// jump to first
				this._streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

				// read all lines
				List<string> lines = new List<string>();
				string line = this._streamReader.ReadLine();
				while (line != null)
				{
					// normalize UTF-16 BOM of all lines
					if (line.Length > 0 && line[0] == '\uFEFF')
						line = line.Substring(1);

					// add the line into collection
					lines.Add(line);
					line = this._streamReader.ReadLine();
				}

				// assign position to end of file
				this._position = _length;

				// return lines
				return lines;
			}

			/// <summary>
			/// Reads some lines of characters from the current stream at the current position and returns the data as a collection of string.
			/// </summary>
			/// <param name="totalLines">The total number of lines to read (set as 0 to read from current position to end of file).</param>
			/// <returns>The next lines from the input stream, or empty collectoin if the end of the input stream is reached</returns>
			public List<string> ReadLines(int totalLines)
			{
				// use StreamReader to read all lines (better performance)
				if (totalLines < 1 && this.Position < this._encoding.GetPreamble().Length)
					return this.ReadAllLines();

				// read lines
				List<string> lines = new List<string>();
				int counter = 0;
				string line = this.ReadLine();
				while (line != null)
				{
					// normalize UTF-16 BOM of all lines
					if (line.Length > 0 && line[0] == '\uFEFF')
						line = line.Substring(1);

					// add the line into collection
					lines.Add(line);

					// check counter and read next line
					counter++;
					if (totalLines > 0 && counter >= totalLines)
						break;
					line = this.ReadLine();
				}
				return lines;
			}

			/// <summary>
			/// Reads a line of characters from the current stream at the current position and returns the data as a string.
			/// </summary>
			/// <returns>The next line from the input stream, or null if the end of the input stream is reached</returns>
			public string ReadLine()
			{
				// check to read next block
				if (this._lines == null || this._lines.Count < 1)
					this.ReadBlockOfLines();

				// get first line from queue and assign position
				TextLine line = null;
				if (this._lines != null && this._lines.Count > 0)
				{
					line = this._lines.Dequeue();
					this._position = line.Position;
				}
				else
					this._position = this._length;

				// return line of characters
				return line != null ? line.Line : null;
			}

			void ReadBlockOfLines()
			{
				// read one block
				char[] data = new char[TextFileReader.BufferSize];
				int readBytes = this._binReader.Read(data, 0, TextFileReader.BufferSize);

				// build block of lines and continue read other blocks until reach end-of-line (\n)
				while (readBytes > 0)
				{
					this._builder.Append(data);
					if (!this.BuildBlockOfLines())
						readBytes = this._binReader.Read(data, 0, TextFileReader.BufferSize);
					else
						readBytes = 0;
				}
			}

			bool BuildBlockOfLines()
			{
				// get current string and find the end-of-line (\n)
				string theString = this._builder.ToString();
				int eolPosition = theString.IndexOf("\n");
				bool endOfLineIsFound = eolPosition > -1;

				// stop process if end-of-line is not found
				if (!endOfLineIsFound)
					return endOfLineIsFound;

				// end-of-file flag
				bool endOfFileIsFound = false;

				// process lines of characters and check end-of-file
				while (eolPosition > -1)
				{
					// get line of characters
					string line = theString.Substring(0, eolPosition + 1);

					// prepare reading position of stream
					this._position += this.Encoding.GetByteCount(line);

					// refine line of characters
					while (line.EndsWith("\n"))
						line = line.Substring(0, line.Length - 1);
					while (line.EndsWith("\r"))
						line = line.Substring(0, line.Length - 1);

					// normalize UTF-16 BOM of all lines
					if (line.Length > 0 && line[0] == '\uFEFF')
						line = line.Substring(1);

					// update line of characters into queue
					TextLine textLine = new TextLine();
					textLine.Line = line;
					textLine.Position = this._position;
					this._lines.Enqueue(textLine);

					// stop process if end of file
					if (endOfFileIsFound)
					{
						theString = null;
						break;
					}

					// remove the processed line
					else
						theString = theString.Remove(0, eolPosition + 1);

					// check end-of-file position (\0)
					if (theString.StartsWith("\0"))
					{
						endOfFileIsFound = true;
						theString = null;
						eolPosition = -1;
					}

					// if the string is not started by end-of-file, then check next end-of-line position
					else
					{
						// check next end-of-line position
						eolPosition = theString.IndexOf("\n");

						// if next end-of-line position is not found, then check end-of-file position (\0)
						if (eolPosition < 0)
						{
							eolPosition = theString.IndexOf("\0");
							if (eolPosition > 0)
							{
								eolPosition--;
								endOfFileIsFound = true;
							}
						}
					}
				}

				// update builder
				this._builder = new StringBuilder();
				if (theString != null)
					this._builder.Append(theString);

				// return the flag
				return endOfLineIsFound;
			}

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				if (this._binReader != null)
				{
					this._binReader.Close();
					this._binReader.Dispose();
				}

				if (this._streamReader != null)
				{
					this._streamReader.Close();
					this._streamReader.Dispose();
				}

				if (this._fileStream != null)
				{
					this._fileStream.Close();
					this._fileStream.Dispose();
				}
			}

			~TextFileReader()
			{
				this.Dispose();
			}
		}
		#endregion

		public static void ReadTextFile(string filePath, long position, int totalOfLines, out List<string> lines, out long newPosition)
		{
			using (var fileReader = new TextFileReader(filePath))
			{
				try
				{
					fileReader.Seek(position);
					lines = fileReader.ReadLines(totalOfLines);
					newPosition = fileReader.Position;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		public static List<string> ReadTextFile(string filePath, int totalOfLines)
		{
			List<string> lines;
			long newPosition;
			Utility.ReadTextFile(filePath, 0, totalOfLines, out lines, out newPosition);
			return lines;
		}

		public static void WriteTextFile(string filePath, List<string> lines, bool append = true, Encoding encoding = default(UTF8Encoding))
		{
			if (!string.IsNullOrWhiteSpace(filePath) && lines != null && lines.Count > 0)
				using (var file = new StreamWriter(filePath, append, encoding != null ? encoding : Encoding.UTF8))
				{
					try
					{
						lines.ForEach(line =>
						{
							if (line != null)
								file.WriteLine(line);
						});
						file.Flush();
					}
					catch { }
				}
		}
		#endregion

		#region Read/Download binary files
		public static byte[] ReadFile(FileInfo fileInfo)
		{
			if (fileInfo.Exists)
				using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var buffer = new byte[fileInfo.Length];
					fileStream.Read(buffer, 0, (int)fileInfo.Length);
					return buffer;
				}
			else
				return null;
		}

		public static byte[] ReadFile(string filePath)
		{
			return Utility.ReadFile(new FileInfo(filePath));
		}

		public static async Task<byte[]> ReadFileAsync(string filePath)
		{
			var fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
				return null;

			else if (fileInfo.Length < 1024 * 1024)
				return Utility.ReadFile(fileInfo);

			else
				using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 10240, true))
				{
					var data = new byte[fileInfo.Length];
					await fileStream.ReadAsync(data, 0, (int)fileInfo.Length);
					return data;
				}
		}

		public static async Task DownloadFileAsync(string url, string filePath, string referUri = null, CancellationToken cancellationToken = default(CancellationToken), Action<string, string> onCompleted = null, Action<string, Exception> onError = null)
		{
			if (string.IsNullOrWhiteSpace(url) || !url.IsStartsWith("http"))
			{
				if (onCompleted != null)
					onCompleted(url, null);
			}
			else
				try
				{
					using (var webStream = await Utility.GetWebResourceAsync(url, referUri, cancellationToken))
					{
						using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 1024, true))
						{
							await webStream.CopyToAsync(fileStream, 1024, cancellationToken);
						}
					}
					if (onCompleted != null)
						onCompleted(url, filePath);
				}
				catch (Exception ex)
				{
					if (onError != null)
						onError(url, ex);
				}
		}
		#endregion

		#region Evaluate a JavaScript expression
		/// <summary>
		/// Evaluates an expression and return value (just like JavaScript does)
		/// </summary>
		/// <param name="expression">The JavaScript expression for evaluating</param>
		/// <returns></returns>
		public static object Eval(string expression)
		{
			var value = JsEval.Eval(expression);
			if (value != null && value is Microsoft.JScript.DateObject)
			{
				var datetime = value.ToString().ToArray(' ');
				return DateTime.Parse(datetime[5] + "/" + datetime[1].GetMonthFromHttpString().ToString("00") + "/" + datetime[2] + " " + datetime[3]);
			}
			else
				return value;
		}

		/// <summary>
		/// Evaluates an expression and return value (just like JavaScript does)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expression">The JavaScript expression for evaluating</param>
		/// <returns></returns>
		public static T Eval<T>(string expression)
		{
			return (T)Utility.Eval(expression);
		}
		#endregion

	}

	// -----------------------------------------------------------

	#region Search Query
	public class SearchQuery
	{
		public List<string> AndWords { get; set; }
		public List<string> OrWords { get; set; }
		public List<string> NotWords { get; set; }
		public List<string> AndPhrases { get; set; }
		public List<string> OrPhrases { get; set; }
		public List<string> NotPhrases { get; set; }

		public SearchQuery()
		{
			this.AndWords = new List<string>();
			this.OrWords = new List<string>();
			this.NotWords = new List<string>();
			this.AndPhrases = new List<string>();
			this.OrPhrases = new List<string>();
			this.NotPhrases = new List<string>();
		}

		public static SearchQuery Parse(string searchingQuery)
		{
			var searchQuery = new SearchQuery();
			if (string.IsNullOrWhiteSpace(searchingQuery))
				return searchQuery;

			var query = SearchQuery.NormalizeKeywords(searchingQuery);
			int start = -1, end = -1;
			var allWords = new List<string>();
			var allPhrases = new List<string>();

			start = query.PositionOf("\"");
			end = query.PositionOf("\"", start + 1);
			if (start < 0 || end < 1)
				SearchQuery.ExtractWords(query.Replace("\"", ""), ref allWords);

			else
			{
				while (start >= 0 && end > 0)
				{
					var previousCharater = "";
					if (start > 0)
						previousCharater = query[start - 1].ToString();

					if (previousCharater.Equals("+") || previousCharater.Equals("-"))
						start--;

					var phrase = query.Substring(start, end - start + 1);
					allPhrases.Add(phrase.Replace(" -\"", "\"").Replace(" +\"", "\""));
					query = query.Remove(start, end - start + 1).Trim();
					start = query.IndexOf("\"");
					end = query.IndexOf("\"", start + 1);
				}

				SearchQuery.ExtractWords(query.Replace("\"", ""), ref allWords);
			}

			foreach (var word in allWords)
			{
				if (word[0].Equals('+'))
					searchQuery.AndWords.Add(word.Right(word.Length - 1));
				else if (word[0].Equals('-'))
					searchQuery.NotWords.Add(word.Right(word.Length - 1));
				else
					searchQuery.OrWords.Add(word);
			}

			foreach (var phrase in allPhrases)
			{
				if (phrase[0].Equals('+'))
					searchQuery.AndPhrases.Add(phrase.Right(phrase.Length - 1).Replace("\"", ""));
				else if (phrase[0].Equals('-'))
					searchQuery.NotPhrases.Add(phrase.Right(phrase.Length - 1).Replace("\"", ""));
				else
					searchQuery.OrPhrases.Add(phrase.Replace("\"", ""));
			}

			return searchQuery;
		}

		static void ExtractWords(string query, ref List<string> words)
		{
			if (string.IsNullOrWhiteSpace(query))
				return;

			var allWords = SearchQuery.NormalizeKeywords(query).Replace("\"", "").ToArray(' ');
			foreach (var word in allWords)
				if (!string.IsNullOrWhiteSpace(word))
					words.Add(word.Trim());
		}

		static string NormalizeKeywords(string keywords)
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

}