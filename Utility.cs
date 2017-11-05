#region Related components
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Configuration;

using Newtonsoft.Json.Linq;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods
	/// </summary>
	public static partial class UtilityService
	{

		#region UUID & Random number
		/// <summary>
		/// Gets the UUID (unique universal identity - in 128 bits)
		/// </summary>
		/// <param name="uuid">The string that presents an UUID</param>
		/// <returns>The string that presents UUID with hyphens (128 bits)</returns>
		public static string GetUUID(string uuid = null)
		{
			var id = string.IsNullOrWhiteSpace(uuid)
				? Guid.NewGuid().ToString("N").ToLower()
				: uuid.Trim();

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
				return UtilityService.GetUUID();
			}
		}

		static string _BlankUUID = null;

		/// <summary>
		/// Gets the blank UUID
		/// </summary>
		/// <returns></returns>
		public static string GetBlankUUID()
		{
			if (UtilityService._BlankUUID == null)
				UtilityService._BlankUUID = new string('0', 32);
			return UtilityService._BlankUUID;
		}

		/// <summary>
		/// Gets the blank UUID
		/// </summary>
		/// <returns></returns>
		public static string BlankUID
		{
			get
			{
				return UtilityService.GetBlankUUID();
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
					? UtilityService.Hexa.Replace(uuid, "").Equals(uuid)
					: !uuid.Contains(" ") && !uuid.Contains(";");
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
			return UtilityService._Random.Next(min, max);
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

		/// <summary>
		/// Gets an user-agent as spider-bot
		/// </summary>
		public static string SpiderUserAgent
		{
			get
			{
				return UtilityService.UserAgents[UtilityService.GetRandomNumber(0, UtilityService.UserAgents.Length - 1)];
			}
		}

		/// <summary>
		/// Gets an user-agent as mobile browser
		/// </summary>
		public static string MobileUserAgent
		{
			get
			{
				return "Mozilla/5.0 (iPhone; CPU iPhone OS 7_0_4 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11B554a Safari/9537.53";
			}
		}

		/// <summary>
		/// Gets an user-agent as desktop browser
		/// </summary>
		public static string DesktopUserAgent
		{
			get
			{
				return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.98 Safari/537.36";
			}
		}

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
			if (useSecureProtocol)
				ServicePointManager.SecurityProtocol = secureProtocol;

			var credentialCache = new CredentialCache
			{
				{ new Uri(uri), "Basic", new NetworkCredential(account, password) }
			};
			return credentialCache;
		}

		/// <summary>
		/// Gets the web proxy
		/// </summary>
		/// <param name="proxyHost"></param>
		/// <param name="proxyPort"></param>
		/// <param name="proxyUsername"></param>
		/// <param name="proxyUserPassword"></param>
		/// <param name="proxyBypassList"></param>
		/// <returns></returns>
		public static WebProxy GetWebProxy(string proxyHost, int proxyPort, string proxyUsername, string proxyUserPassword, string[] proxyBypassList = null)
		{
			WebProxy proxy = null;
			if (!string.IsNullOrWhiteSpace(proxyHost))
			{
				proxy = proxyBypassList == null || proxyBypassList.Length < 1
					? new WebProxy(proxyHost, proxyPort)
					: new WebProxy(new Uri("http://" + proxyHost + ":" + proxyPort.ToString()), true, proxyBypassList);

				if (!string.IsNullOrWhiteSpace(proxyUsername) && !string.IsNullOrWhiteSpace(proxyUserPassword))
					proxy.Credentials = new NetworkCredential(proxyUsername, proxyUserPassword);
			}
			return proxy;
		}

		/// <summary>
		/// Performs a web request and get response in async
		/// </summary>
		/// <param name="httpRequest"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest httpRequest, CancellationToken cancellationToken = default(CancellationToken))
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
			// get the request object to handle on the remote resource
			var webRequest = WebRequest.Create(uri) as HttpWebRequest;

			// set properties
			webRequest.Method = string.IsNullOrWhiteSpace(method)
				? "GET"
				: method.ToUpper();
			webRequest.Timeout = timeout * 1000;
			webRequest.ServicePoint.Expect100Continue = false;
			webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			webRequest.UserAgent = string.IsNullOrWhiteSpace(userAgent)
				? UtilityService.DesktopUserAgent
				: userAgent;
			webRequest.Referer = string.IsNullOrWhiteSpace(referUri)
				? ""
				: referUri;

			// headers
			if (headers != null)
				foreach (var header in headers)
					if (!header.Key.IsEquals("accept-encoding"))
						webRequest.Headers.Add(header.Key, header.Value);

			// cookies
			if (cookies != null && cookies.Length > 0 && webRequest.SupportsCookieContainer)
				foreach (var cookie in cookies)
					webRequest.CookieContainer.Add(cookie);

			// compression
			webRequest.Headers.Add("accept-encoding", "deflate,gzip");
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
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
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
				var responseBody = "";
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
		public static async Task<HttpWebResponse> GetWebResponseAsync(string method, string uri, Dictionary<string, string> headers, string body, string contentType, int timeout = 90, string userAgent = null, string referUri = null, string credentialAccount = null, string credentialPassword = null, bool useSecureProtocol = true, SecurityProtocolType secureProtocol = SecurityProtocolType.Ssl3, WebProxy proxy = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			// credential
			var credential = !string.IsNullOrWhiteSpace(credentialAccount) && !string.IsNullOrWhiteSpace(credentialPassword)
				? UtilityService.GetWebCredential(uri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol)
				: null;

			// make request
			return await UtilityService.GetWebResponseAsync(method, uri, headers, null, body, contentType, timeout, userAgent, referUri, credential, proxy, cancellationToken);
		}

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
		{
			var webResponse = await UtilityService.GetWebResponseAsync(method, uri, headers, body, contentType, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, proxy, cancellationToken);
			return webResponse.GetResponseStream();
		}

		/// <summary>
		/// Gets the web resource stream
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="referUri"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<Stream> GetWebResourceAsync(string uri, string referUri = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.GetWebResourceAsync("GET", uri, null, null, null, 90, UtilityService.SpiderUserAgent, referUri, null, null, true, SecurityProtocolType.Ssl3, null, cancellationToken);
		}

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
			// check uri
			if (string.IsNullOrWhiteSpace(url))
				return null;

			// get stream of external resource as HTML
			var html = "";
			using (var webResponse = await UtilityService.GetWebResponseAsync("GET", url, headers, null, null, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, proxy, cancellationToken))
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
		{
			return UtilityService.GetWebPageAsync(url, headers, timeout, userAgent, referUri, credentialAccount, credentialPassword, useSecureProtocol, secureProtocol, UtilityService.GetWebProxy(proxyHost, proxyPort, proxyUsername, proxyUserPassword, proxyBypassList), cancellationToken);
		}

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
		{
			return UtilityService.GetWebPageAsync(url, null, timeout, proxyHost, proxyPort, proxyUsername, proxyUserPassword, proxyBypassList, userAgent, referUri, null, null, true, SecurityProtocolType.Ssl3, cancellationToken);
		}

		/// <summary>
		/// Gets the web page
		/// </summary>
		/// <param name="url"></param>
		/// <param name="referUri"></param>
		/// <param name="userAgent"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<string> GetWebPageAsync(string url, string referUri = null, string userAgent = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.GetWebPageAsync(url, 90, null, 0, null, null, null, userAgent, referUri, cancellationToken);
		}
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

		#region Working with process
		/// <summary>
		/// Runs a process
		/// </summary>
		/// <param name="filePath">The string that presents the path to a file to run</param>
		/// <param name="arguments">The string that presents the arguments to run</param>
		/// <param name="onExited">The method to handle the Exit event</param>
		/// <param name="onDataReceived">The method to handle the data receive events (include OutputDataReceived and ErrorDataReceived events)</param>
		/// <returns></returns>
		public static Process RunProcess(string filePath, string arguments = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onDataReceived = null)
		{
			// initialize info
			var psi = new ProcessStartInfo()
			{
				FileName = filePath,
				Arguments = arguments ?? "",
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				ErrorDialog = false,
				UseShellExecute = false,
				RedirectStandardInput = true
			};

			if (onDataReceived != null)
			{
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
			}
			else
			{
				psi.RedirectStandardOutput = false;
				psi.RedirectStandardError = false;
			}

			// initialize proces
			var process = new Process() { StartInfo = psi };
			if (onExited != null || onDataReceived != null)
				process.EnableRaisingEvents = true;

			// assign event handlers
			if (onExited != null)
				process.Exited += new EventHandler(onExited);

			if (onDataReceived != null)
			{
				process.OutputDataReceived += new DataReceivedEventHandler(onDataReceived);
				process.ErrorDataReceived += new DataReceivedEventHandler(onDataReceived);
			}

			// start and return process
			process.Start();
			if (onDataReceived != null)
			{
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}

			return process;
		}

		/// <summary>
		/// Kills a process
		/// </summary>
		/// <param name="process"></param>
		/// <param name="action">The action to try to close the process before the process be killed</param>
		public static void KillProcess(Process process, Action<Process> action = null)
		{
			if (process != null)
			{
				try
				{
					action?.Invoke(process);
				}
				catch { }

				if (process.StartInfo.RedirectStandardInput)
				{
					process.StandardInput.Close();
					process.Refresh();
					if (!process.HasExited)
					{
						if (!process.WaitForExit(567))
							process.Kill();
					}
				}
				else if (!process.HasExited)
					process.Kill();
			}
		}

		/// <summary>
		/// Kills a process by ID
		/// </summary>
		/// <param name="id">The integer that presents the identity of a process</param>
		/// <param name="action">The action to try to close the process before the process be killed</param>
		public static void KillProcess(int id, Action<Process> action = null)
		{
			UtilityService.KillProcess(Process.GetProcessById(id));
		}
		#endregion

		#region Working with task in the thread pool
		/// <summary>
		/// Executes a task the the thread pool with cancellation token
		/// </summary>
		/// <param name="action"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task ExecuteTask(Action action, CancellationToken cancellationToken = default(CancellationToken))
		{
			var tcs = new TaskCompletionSource<object>();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				if (cancellationToken == null)
					cancellationToken = default(CancellationToken);
				cancellationToken.Register(() =>
				{
					tcs.SetCanceled();
					return;
				});

				try
				{
					action?.Invoke();
					tcs.SetResult(null);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});
			return tcs.Task;
		}

		/// <summary>
		/// Executes a task the the thread pool with cancellation token
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="func"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<TResult> ExecuteTask<TResult>(Func<TResult> func, CancellationToken cancellationToken = default(CancellationToken))
		{
			var tcs = new TaskCompletionSource<TResult>();
			ThreadPool.QueueUserWorkItem(_ =>
			{
				if (cancellationToken == null)
					cancellationToken = default(CancellationToken);
				cancellationToken.Register(() =>
				{
					tcs.SetCanceled();
					return;
				});

				try
				{
					var result = func != null ? func.Invoke() : default(TResult);
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});
			return tcs.Task;
		}
		#endregion

		#region Working with files & folders
		static List<string> _FileRemovements = new List<string>() { "\\", "/", "*", "?", "<", ">", "|", ":", "\r", "\n", "\t" };
		static List<string[]> _FileReplacements = new List<string[]>() { new string[] { "\"", "'" }, new string[] { "%20", " " }, new string[] { " ft. ", " & " } };

		/// <summary>
		/// Normalizes the file name
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
		/// Gets parts of file path (seperate path and file name)
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="removeExtension"></param>
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

			return new Tuple<string, string>(path, UtilityService.GetNormalizedFilename(filename));
		}

		/// <summary>
		/// Searchs and gets listing of files by searching pattern
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="searchInSubFolder"></param>
		/// <param name="excludedSubFolders"></param>
		/// <returns></returns>
		public static List<FileInfo> GetFiles(string path, string searchPatterns, bool searchInSubFolder = false, List<string> excludedSubFolders = null)
		{
			if (!Directory.Exists(path))
				throw new DirectoryNotFoundException("The folder is not found [" + path + "]");

			var files = new List<FileInfo>();
			var searchingPatterns = string.IsNullOrWhiteSpace(searchPatterns)
				? new string[] { "*.*" }
				: searchPatterns.ToArray('|', true);

			searchingPatterns.ForEach(searchingPattern => files.Append(Directory.GetFiles(path, searchingPattern).Select(f => new FileInfo(f)).OrderBy(f => f.Name)));

			if (searchInSubFolder)
			{
				var folderPaths = Directory.GetDirectories(path);
				if (folderPaths != null)
					folderPaths.ForEach(folderPath =>
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
							searchingPatterns.ForEach(searchingPattern => files.Append(Directory.GetFiles(folderPath, searchingPattern).Select(f => new FileInfo(f)).OrderBy(f => f.Name)));
					});
			}

			return files;
		}

		/// <summary>
		/// Searchs and gets listing of files by searching pattern
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="searchInSubFolder"></param>
		/// <param name="excludedSubFolders"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<List<FileInfo>> GetFilesAsync(string path, string searchPatterns, bool searchInSubFolder = false, List<string> excludedSubFolders = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.ExecuteTask<List<FileInfo>>(() => UtilityService.GetFiles(path, searchPatterns, searchInSubFolder, excludedSubFolders), cancellationToken);
		}

		/// <summary>
		/// Searchs and gets the listing of file paths by searching pattern
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="searchInSubFolder"></param>
		/// <param name="excludedSubFolders"></param>
		/// <returns></returns>
		public static List<string> GetFilePaths(string path, string searchPatterns, bool searchInSubFolder = false, List<string> excludedSubFolders = null)
		{
			return UtilityService.GetFiles(path, searchPatterns, searchInSubFolder, excludedSubFolders)
				.Select(f => f.FullName)
				.ToList();
		}

		/// <summary>
		/// Searchs and gets the listing of file paths by searching pattern
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="searchInSubFolder"></param>
		/// <param name="excludedSubFolders"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<List<string>> GetFilePathsAsync(string path, string searchPatterns, bool searchInSubFolder = false, List<string> excludedSubFolders = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.ExecuteTask<List<string>>(() => UtilityService.GetFilePaths(path, searchPatterns, searchInSubFolder, excludedSubFolders), cancellationToken);
		}
		#endregion

		#region Read/Write text files
		/// <summary>
		/// Reads a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static string ReadTextFile(string filePath, Encoding encoding = default(UTF8Encoding))
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("The file is not found [" + filePath + "]");

			using (var reader = new StreamReader(filePath, encoding ?? Encoding.UTF8, true))
			{
				try
				{
					return reader.ReadToEnd();
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Reads a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<string> ReadTextFileAsync(string filePath, Encoding encoding = default(UTF8Encoding))
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("The file is not found [" + filePath + "]");

			using (var reader = new StreamReader(filePath, encoding ?? Encoding.UTF8, true))
			{
				try
				{
					return await reader.ReadToEndAsync();
				}
				catch (Exception)
				{
					throw;
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
		public static void WriteTextFile(string filePath, string content, bool append = false, Encoding encoding = default(UTF8Encoding))
		{
			if (string.IsNullOrWhiteSpace(filePath) || content == null)
				return;

			using (var writer = new StreamWriter(filePath, append, encoding ?? Encoding.UTF8))
			{
				try
				{
					writer.Write(content);
				}
				catch { }
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
		public static async Task WriteTextFileAsync(string filePath, string content, bool append = false, Encoding encoding = default(UTF8Encoding))
		{
			if (string.IsNullOrWhiteSpace(filePath) || content == null)
				return;

			using (var writer = new StreamWriter(filePath, append, encoding ?? Encoding.UTF8))
			{
				try
				{
					await writer.WriteAsync(content);
				}
				catch { }
			}
		}
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
				try
				{
					reader.Seek(position);
					lines = reader.ReadLines(totalOfLines);
					newPosition = reader.Position;
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		/// <summary>
		/// Reads the multiple lines of a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="position"></param>
		/// <param name="totalOfLines"></param>
		/// <returns></returns>
		public static async Task<Tuple<List<string>, long>> ReadTextFileAsync(string filePath, long position, int totalOfLines)
		{
			using (var reader = new TextFileReader(filePath))
			{
				try
				{
					reader.Seek(position);
					var lines = await reader.ReadLinesAsync(totalOfLines);
					return new Tuple<List<string>, long>(lines, reader.Position);
				}
				catch (Exception)
				{
					throw;
				}
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
		/// <returns></returns>
		public static async Task<List<string>> ReadTextFileAsync(string filePath, int totalOfLines)
		{
			return (await UtilityService.ReadTextFileAsync(filePath, 0, totalOfLines)).Item1;
		}

		/// <summary>
		/// Writes the multiple lines of a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="lines"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		public static void WriteTextFile(string filePath, List<string> lines, bool append = true, Encoding encoding = default(UTF8Encoding))
		{
			if (!string.IsNullOrWhiteSpace(filePath) && lines != null && lines.Count > 0)
				using (var writer = new StreamWriter(filePath, append, encoding ?? Encoding.UTF8))
				{
					try
					{
						lines.Where(line => line != null).ForEach(line => writer.WriteLine(line));
						writer.Flush();
					}
					catch { }
				}
		}

		/// <summary>
		/// Writes the multiple lines of a text file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="lines"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		public static async Task WriteTextFileAsync(string filePath, List<string> lines, bool append = true, Encoding encoding = default(UTF8Encoding))
		{
			if (!string.IsNullOrWhiteSpace(filePath) && lines != null && lines.Count > 0)
				using (var stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read, TextFileReader.BufferSize, true))
				{
					using (var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8))
					{
						try
						{
							await lines.Where(line => line != null).ForEachAsync(async (line, cancellationToken) => await writer.WriteLineAsync(line), CancellationToken.None, true, false);
							writer.Flush();
						}
						catch { }
					}
				}
		}
		#endregion

		#region Read/Download binary files
		/// <summary>
		/// Reads a binary file
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static byte[] ReadFile(FileInfo fileInfo)
		{
			if (fileInfo != null && fileInfo.Exists)
				using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					var buffer = new byte[fileInfo.Length];
					stream.Read(buffer, 0, (int)fileInfo.Length);
					return buffer;
				}
			return null;
		}

		/// <summary>
		/// Reads a binary file
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] ReadFile(string filePath)
		{
			return UtilityService.ReadFile(new FileInfo(filePath));
		}

		/// <summary>
		/// Reads a binary file
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static async Task<byte[]> ReadFileAsync(string filePath)
		{
			var fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists)
				return null;

			else if (fileInfo.Length < 1024 * 1024)
				return UtilityService.ReadFile(fileInfo);

			else
				using (var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 10240, true))
				{
					var data = new byte[fileInfo.Length];
					await stream.ReadAsync(data, 0, (int)fileInfo.Length);
					return data;
				}
		}

		/// <summary>
		/// Downloads a file from a remote uri
		/// </summary>
		/// <param name="url"></param>
		/// <param name="filePath"></param>
		/// <param name="referUri"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="onCompleted"></param>
		/// <param name="onError"></param>
		/// <returns></returns>
		public static async Task DownloadFileAsync(string url, string filePath, string referUri = null, Action<string, string> onCompleted = null, Action<string, Exception> onError = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (string.IsNullOrWhiteSpace(url) || !url.IsStartsWith("http"))
				onCompleted?.Invoke(url, null);

			else
				try
				{
					using (var webStream = await UtilityService.GetWebResourceAsync(url, referUri, cancellationToken))
					{
						using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 1024, true))
						{
							await webStream.CopyToAsync(fileStream, 1024, cancellationToken);
						}
					}
					onCompleted?.Invoke(url, filePath);
				}
				catch (Exception ex)
				{
					onError?.Invoke(url, ex);
				}
		}
		#endregion

		#region Get setting/parameter of the app
		/// <summary>
		/// Gets a setting of the app (from the 'appSettings' section of configuration file with prefix 'vieapps:')
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="defaultValue">The default value if the setting is not found</param>
		/// <returns></returns>
		public static string GetAppSetting(string name, string defaultValue = null)
		{
			var value = string.IsNullOrEmpty(name)
				? null
				: ConfigurationManager.AppSettings["vieapps:" + name.Trim()];

			return string.IsNullOrEmpty(value)
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

			return string.IsNullOrEmpty(value)
				? defaultValue
				: value;
		}
		#endregion

	}

	// -----------------------------------------------------------

	#region Search Query
	/// <summary>
	/// Presents a parsed query for searching
	/// </summary>
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
			this._section = section;
			return this;
		}

		XmlNode _section = null;

		/// <summary>
		/// Gets the configuration section
		/// </summary>
		public XmlNode Section { get { return this._section; } }
	}
	#endregion

	//  --------------------------------------------------------------------------------------------

	#region Reader of a text file
	/// <summary>
	/// Implements a System.IO.BinaryReader that reads block of characters from a file stream in a particular encoding.
	/// </summary>
	public sealed class TextFileReader : IDisposable
	{
		// by default, one reading block of Windows is 4K (4096), then use 64K(65536)/128K(131072)/256K(262144)/512K(524288)
		// for better performance while working with text file has large line of characters
		public static readonly int BufferSize = 65536;

		internal sealed class TextLine
		{
			public TextLine() { }

			public string Line { get; set; } = "";

			public long Position { get; set; } = 0;
		}

		FileStream _fileStream = null;
		BinaryReader _binReader = null;
		StreamReader _streamReader = null;

		Queue<TextLine> _lines = null;
		StringBuilder _builder = null;

		Encoding _encoding = Encoding.UTF8;
		long _length = -1, _position = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="TextFileReader"/> class.
		/// </summary>
		/// <param name="filePath">The path to text file.</param>
		/// <param name="detectEncoding">if set to <c>true</c>, then detect encoding of text file.</param>
		/// <param name="useAsync">true to use async</param>
		public TextFileReader(string filePath, bool detectEncoding = true, bool useAsync = false)
		{
			// check existed
			this.Existed(filePath);

			// default encoding is UTF-8
			var encoding = Encoding.UTF8;

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
			this.Initialize(filePath, encoding, useAsync);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TextFileReader"/> class.
		/// </summary>
		/// <param name="filePath">The path to text file.</param>
		/// <param name="encoding">The encoding of text file.</param>
		/// <param name="useAsync">true to use async</param>
		public TextFileReader(string filePath, Encoding encoding, bool useAsync = false)
		{
			// check existed
			this.Existed(filePath);

			// initialize reader
			this.Initialize(filePath, encoding, useAsync);
		}

		/// <summary>
		/// Checks existed of the file.
		/// </summary>
		/// <param name="filePath">The path to text file.</param>
		void Existed(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("No file path", nameof(filePath));

			else if (!File.Exists(filePath))
				throw new FileNotFoundException("File (" + filePath + ") is not found.");
		}

		/// <summary>
		/// Initializes the reader.
		/// </summary>
		/// <param name="filePath">The path to text file.</param>
		/// <param name="encoding">The encoding of text file.</param>
		/// <param name="useAsync">true to use async</param>
		void Initialize(string filePath, Encoding encoding, bool useAsync = false)
		{
			// create streams and builders
			if (this._fileStream == null)
			{
				if (useAsync)
					this._fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, TextFileReader.BufferSize, true);
				else
					this._fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			}

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
		/// <param name="origin">A value for indicating the reference point used to obtain the new position</param>
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
			var lines = new List<string>();
			var line = this._streamReader.ReadLine();
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
			this._position = this._length;

			// return lines
			return lines;
		}

		/// <summary>
		/// Reads all lines of characters from the current stream (from the begin to end) and returns the data as a collection of string.
		/// </summary>
		/// <returns>The next lines from the input stream, or empty collectoin if the end of the input stream is reached</returns>
		public async Task<List<string>> ReadAllLinesAsync()
		{
			// create new stream if need
			if (this._streamReader == null)
				this._streamReader = new StreamReader(this._fileStream, this._encoding);

			// jump to first
			this._streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

			// read all lines
			var lines = new List<string>();
			var line = await this._streamReader.ReadLineAsync();
			while (line != null)
			{
				// normalize UTF-16 BOM of all lines
				if (line.Length > 0 && line[0] == '\uFEFF')
					line = line.Substring(1);

				// add the line into collection
				lines.Add(line);
				line = await this._streamReader.ReadLineAsync();
			}

			// assign position to end of file
			this._position = this._length;

			// return lines
			return lines;
		}

		/// <summary>
		/// Reads some lines of characters from the current stream at the current position and returns the data as a collection of string.
		/// </summary>
		/// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
		/// <returns>The next lines from the input stream, or empty collectoin if the end of the input stream is reached</returns>
		public List<string> ReadLines(int totalOfLines)
		{
			// use StreamReader to read all lines (better performance)
			if (totalOfLines < 1 && this.Position < this._encoding.GetPreamble().Length)
				return this.ReadAllLines();

			// read lines
			var lines = new List<string>();
			var counter = 0;
			var line = this.ReadLine();
			while (line != null)
			{
				// normalize UTF-16 BOM of all lines
				if (line.Length > 0 && line[0] == '\uFEFF')
					line = line.Substring(1);

				// add the line into collection
				lines.Add(line);

				// check counter and read next line
				counter++;
				if (totalOfLines > 0 && counter >= totalOfLines)
					break;

				line = this.ReadLine();
			}
			return lines;
		}

		/// <summary>
		/// Reads some lines of characters from the current stream at the current position and returns the data as a collection of string.
		/// </summary>
		/// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
		/// <returns>The next lines from the input stream, or empty collectoin if the end of the input stream is reached</returns>
		public async Task<List<string>> ReadLinesAsync(int totalOfLines)
		{
			// use StreamReader to read all lines (better performance)
			if (totalOfLines < 1 && this.Position < this._encoding.GetPreamble().Length)
				return await this.ReadAllLinesAsync();

			// read lines
			var lines = new List<string>();
			var counter = 0;
			var line = await this.ReadLineAsync();
			while (line != null)
			{
				// normalize UTF-16 BOM of all lines
				if (line.Length > 0 && line[0] == '\uFEFF')
					line = line.Substring(1);

				// add the line into collection
				lines.Add(line);

				// check counter and read next line
				counter++;
				if (totalOfLines > 0 && counter >= totalOfLines)
					break;

				line = await this.ReadLineAsync();
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
			var line = this._lines != null && this._lines.Count > 0
				? this._lines.Dequeue()
				: null;
			this._position = line != null
				? line.Position
				: this._position = this._length;

			// return line of characters
			return line != null
				? line.Line
				: null;
		}

		/// <summary>
		/// Reads a line of characters from the current stream at the current position and returns the data as a string.
		/// </summary>
		/// <returns>The next line from the input stream, or null if the end of the input stream is reached</returns>
		public Task<string> ReadLineAsync()
		{
			return UtilityService.ExecuteTask<string>(() => this.ReadLine());
		}

		void ReadBlockOfLines()
		{
			// read one block
			var data = new char[TextFileReader.BufferSize];
			var readBytes = this._binReader.Read(data, 0, TextFileReader.BufferSize);

			// build block of lines and continue read other blocks until reach end-of-line (\n)
			while (readBytes > 0)
			{
				this._builder.Append(data);
				readBytes = !this.BuildBlockOfLines()
					? this._binReader.Read(data, 0, TextFileReader.BufferSize)
					: 0;
			}
		}

		bool BuildBlockOfLines()
		{
			// get current string and find the end-of-line (\n)
			var theString = this._builder.ToString();
			var eolPosition = theString.IndexOf("\n");
			var endOfLineIsFound = eolPosition > -1;

			// stop process if end-of-line is not found
			if (!endOfLineIsFound)
				return endOfLineIsFound;

			// end-of-file flag
			var endOfFileIsFound = false;

			// process lines of characters and check end-of-file
			while (eolPosition > -1)
			{
				// get line of characters
				var line = theString.Substring(0, eolPosition + 1);

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
				this._lines.Enqueue(new TextLine()
				{
					Line = line,
					Position = this._position
				});

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
			this._binReader?.Close();
			this._binReader?.Dispose();

			this._streamReader?.Close();
			this._streamReader?.Dispose();

			this._fileStream?.Close();
			this._fileStream?.Dispose();

			GC.SuppressFinalize(this);
		}

		~TextFileReader()
		{
			this.Dispose();
		}
	}
	#endregion

}