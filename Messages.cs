#region Related components
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
#if NETSTANDARD2_0
using System.Runtime.InteropServices;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Presents an email message
	/// </summary>
	[DebuggerDisplay("Subject = {Subject}")]
	public class EmailMessage
	{
		/// <summary>
		/// Initializes a new email message
		/// </summary>
		public EmailMessage() : this(null) { }

		/// <summary>
		/// Initializes a new email message
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message</param>
		public EmailMessage(string encryptedMessage)
		{
			if (!string.IsNullOrWhiteSpace(encryptedMessage))
				try
				{
					this.CopyFrom(encryptedMessage.Decrypt(EmailMessage.EncryptionKey).FromJson<EmailMessage>());
				}
				catch { }
		}

		#region Properties
		public string From { get; set; } = "";

		public string ReplyTo { get; set; }

		public string To { get; set; } = "";

		public string Cc { get; set; } = "";

		public string Bcc { get; set; } = "";

		public string Subject { get; set; } = "";

		public string Body { get; set; } = "";

		public string Footer { get; set; }

		public string Attachment { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public MailPriority Priority { get; set; } = MailPriority.Normal;

		public bool IsBodyHtml { get; set; } = true;

		public int Encoding { get; set; } = System.Text.Encoding.UTF8.CodePage;

		public string SmtpServer { get; set; }

		public int SmtpServerPort { get; set; } = 25;

		public string SmtpUsername { get; set; }

		public string SmtpPassword { get; set; }

		public bool SmtpServerEnableSsl { get; set; } = true;

		static string _EncryptionKey = null;

		internal static string EncryptionKey => EmailMessage._EncryptionKey ?? (EmailMessage._EncryptionKey = UtilityService.GetAppSetting("Keys:MessageEncryption", "VIE-Apps-9D17C42D-Core-AE9F-Components-4D72-Email-586D-Encryption-277D9E606F1F-Keys"));

		/// <summary>
		/// Gets or sets the identity of the message
		/// </summary>
		public string ID { get; set; } = UtilityService.NewUUID;

		/// <summary>
		/// Gets or sets the correlation identity of the message
		/// </summary>
		public string CorrelationID { get; set; } = UtilityService.NewUUID;

		/// <summary>
		/// Gets or sets time to start to send this message via email
		/// </summary>
		/// <remarks>
		/// Set a specifict time to tell mailer send this message from this time
		/// </remarks>
		public DateTime SendingTime { get; set; } = DateTime.Now;
		#endregion

		#region Working with files
		/// <summary>
		/// Loads message from file and deserialize as object
		/// </summary>
		/// <param name="filePath">The full path to a file that contains the encrypted message</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<EmailMessage> LoadAsync(string filePath, CancellationToken cancellationToken = default)
			=> !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)
				? new EmailMessage(await new FileInfo(filePath).ReadAsTextAsync(cancellationToken).ConfigureAwait(false))
				: null;

		/// <summary>
		/// Serializes and saves message into file
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="directory">The path to a directory that stores the queue of messages</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static async Task SaveAsync(EmailMessage message, string directory, CancellationToken cancellationToken = default)
		{
			if (message != null && Directory.Exists(directory))
				try
				{
					await message.Encrypted.ToBytes().SaveAsTextAsync(Path.Combine(directory, message.ID + ".msg"), cancellationToken).ConfigureAwait(false);
				}
				catch { }
		}

		/// <summary>
		/// Gets the string that presents the encrypted messages
		/// </summary>
		[JsonIgnore]
		public string Encrypted => this.ToJson().ToString(Formatting.None).Encrypt(EmailMessage.EncryptionKey);
		#endregion

	}

	// ------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Presents a web-hook message
	/// </summary>
	[DebuggerDisplay("Endpoint = {EndpointURL}")]
	public class WebHookMessage
	{
		/// <summary>
		/// Initializes a new web-hook message
		/// </summary>
		public WebHookMessage() : this(null) { }

		/// <summary>
		/// Initializes a new web-hook message
		/// </summary>
		/// <param name="encryptedMessage">The encrypted message</param>
		public WebHookMessage(string encryptedMessage)
		{
			if (!string.IsNullOrWhiteSpace(encryptedMessage))
				try
				{
					this.CopyFrom(encryptedMessage.Decrypt(WebHookMessage.EncryptionKey).FromJson<WebHookMessage>());
					this.Header.Select(kvp => kvp.Key).ToList().ForEach(key => this.Header[key] = this.Header[key]?.UrlDecode());
					this.Query.Select(kvp => kvp.Key).ToList().ForEach(key => this.Query[key] = this.Query[key]?.UrlDecode());
				}
				catch { }
		}

		#region Properties
		/// <summary>
		/// Gets or Sets the url of webhook's endpoint
		/// </summary>
		public string EndpointURL { get; set; } = "";

		/// <summary>
		/// Gets or Sets the body of the webhook message
		/// </summary>
		public string Body { get; set; } = "";

		/// <summary>
		/// Gets or Sets header of webhook message
		/// </summary>
		public Dictionary<string, string> Header { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gets or Sets query-string of webhook message
		/// </summary>
		public Dictionary<string, string> Query { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		static string _EncryptionKey = null;

		internal static string EncryptionKey => WebHookMessage._EncryptionKey ?? (WebHookMessage._EncryptionKey = UtilityService.GetAppSetting("Keys:MessageEncryption", "VIE-Apps-5D659BA4-Core-23BE-Components-4E43-WebHook-81E4-Encryption-EACD7EDE222A-Keys"));

		/// <summary>
		/// Gets or sets identity of the message
		/// </summary>
		public string ID { get; set; } = UtilityService.NewUUID;

		/// <summary>
		/// Gets or sets the correlation identity of the message
		/// </summary>
		public string CorrelationID { get; set; } = UtilityService.NewUUID;

		/// <summary>
		/// Gets or sets time to start to send this message
		/// </summary>
		public DateTime SendingTime { get; set; } = DateTime.Now;
		#endregion

		#region Working with files
		/// <summary>
		/// Loads message from file and deserialize as object
		/// </summary>
		/// <param name="filePath">The full path to a file that contains the encrypted message</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<WebHookMessage> LoadAsync(string filePath, CancellationToken cancellationToken = default)
			=> !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)
				? new WebHookMessage(await new FileInfo(filePath).ReadAsTextAsync(cancellationToken).ConfigureAwait(false))
				: null;

		/// <summary>
		/// Serializes and saves message into file
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="directory">The path to a directory that stores the queue of messages</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static async Task SaveAsync(WebHookMessage message, string directory, CancellationToken cancellationToken = default)
		{
			if (message != null && Directory.Exists(directory))
				try
				{
					await message.Encrypted.ToBytes().SaveAsTextAsync(Path.Combine(directory, message.ID + ".msg"), cancellationToken).ConfigureAwait(false);
				}
				catch { }
		}

		/// <summary>
		/// Gets the string that presents the encrypted messages
		/// </summary>
		[JsonIgnore]
		public string Encrypted => this.ToJson().ToString(Formatting.None).Encrypt(WebHookMessage.EncryptionKey);
		#endregion

	}

	// ------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Collection of global methods for sending a message (email and web hook)
	/// </summary>
	public static partial class MessageService
	{

		#region Working with a email message
		/// <summary>
		/// Gets the collection of harmful domains need to prevent while sending email messages
		/// </summary>
		public static HashSet<string> HarmfulDomains { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Prepares a valid email address
		/// </summary>
		/// <param name="emailAddress">The string that presents information of an email adress before validating</param>
		/// <param name="convertNameToANSI">true to convert display name as ANSI</param>
		/// <returns><see cref="System.Net.Mail.MailAddress">MailAddress</see> object that contains valid email address</returns>
		public static MailAddress GetMailAddress(this string emailAddress, bool convertNameToANSI = false)
		{
			if (string.IsNullOrWhiteSpace(emailAddress))
				return null;

			string email, displayName = "";

			var emails = emailAddress.ToArray('<');
			if (emails.Length > 1)
			{
				email = emails[1];
				displayName = emails[0];
				if (convertNameToANSI)
					displayName = displayName.ConvertUnicodeToANSI();
			}
			else
				email = emails[0];

			email = email.Replace("<", "").Replace(">", "").Replace(" ", "").ToLower().Trim();
			while (email.StartsWith("/") || email.StartsWith(@"\"))
				email = email.Right(email.Length - 1);

			return email.Equals("")
				? null
				: new MailAddress(email, displayName, Encoding.UTF8);
		}

		/// <summary>
		/// Gets the domain name from the email address
		/// </summary>
		/// <param name="emailAddress"></param>
		/// <returns></returns>
		public static string GetDomain(this string emailAddress)
		{
			var domain = "";
			if (!string.IsNullOrWhiteSpace(emailAddress))
			{
				var pos = emailAddress.PositionOf("@");
				if (pos > 0)
					domain = emailAddress.Right(emailAddress.Length - pos - 1);
				if (emailAddress.EndsWith(">"))
					domain = emailAddress.Right(emailAddress.Length - 1);
			}
			return domain.ToLower();
		}

		/// <summary>
		/// Gets an e-mail message
		/// </summary>
		/// <param name="fromAddress">Sender address</param>
		/// <param name="replyToAddress">Address will be replied to</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachments">Collection of attachment files (means the collection of files with full path)</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		public static MailMessage GetMailMessage(MailAddress fromAddress, MailAddress replyToAddress, IEnumerable<MailAddress> toAddresses, IEnumerable<MailAddress> ccAddresses, IEnumerable<MailAddress> bccAddresses, string subject, string body, IEnumerable<string> attachments, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null)
		{
			// check
			if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
				throw new InvalidDataException("The email must have subject and body");

			if (fromAddress == null || string.IsNullOrWhiteSpace(fromAddress.Address))
				throw new InvalidDataException("The email must have sender address");

			if ((toAddresses == null || toAddresses.Count() < 1)
				&& (ccAddresses == null || ccAddresses.Count() < 1)
				&& (bccAddresses == null || bccAddresses.Count() < 1))
				throw new InvalidDataException("The email must have at least one recipient");

			// create new mail message
			var message = new MailMessage
			{
				From = new MailAddress(fromAddress.Address, fromAddress.DisplayName.ConvertUnicodeToANSI(), encoding ?? Encoding.UTF8),
				Priority = priority,
				Subject = subject.Trim(),
				SubjectEncoding = encoding ?? Encoding.UTF8,
				Body = $"{body.Trim()}{footer ?? ""}",
				BodyEncoding = encoding ?? Encoding.UTF8,
				IsBodyHtml = isBodyHtml
			};

			// reply to
			if (replyToAddress != null)
				message.ReplyToList.Add(replyToAddress);

			// recipients
			toAddresses?.Where(emailAddress => emailAddress != null).ForEach(emailAddress => message.To.Add(emailAddress));
			ccAddresses?.Where(emailAddress => emailAddress != null).ForEach(emailAddress => message.CC.Add(emailAddress));
			bccAddresses?.Where(emailAddress => emailAddress != null).ForEach(emailAddress => message.Bcc.Add(emailAddress));

			// attachments
			attachments?.Where(attachment => File.Exists(attachment)).ForEach(attachment => message.Attachments.Add(new Attachment(attachment)));

			// final
			message.Headers.Add("x-mailer", mailer ?? "VIEApps NGX Mailer");
			return message;
		}

		/// <summary>
		/// Gets an e-mail message
		/// </summary>
		/// <param name="from">Sender name and e-mail address</param>
		/// <param name="replyTo">Address will be replied to</param>
		/// <param name="to">Recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="cc">CC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="bcc">BCC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachment">The full path to an attachment file</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		public static MailMessage GetMailMessage(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null)
		{
			// check & validate
			if (from.Equals(""))
				throw new InvalidDataException("No sender information for the message");

			var toEmails = string.IsNullOrWhiteSpace(to)
				? ""
				: to.Trim();

			var ccEmails = string.IsNullOrWhiteSpace(cc)
				? ""
				: cc.Trim();

			var bccEmails = string.IsNullOrWhiteSpace(bcc)
				? ""
				: bcc.Trim();

			if (toEmails.Equals("") && ccEmails.Equals("") && bccEmails.Equals(""))
				throw new InvalidDataException("No recipient for the message");

			// prepare
			MailAddress fromAddress = null;
			try
			{
				fromAddress = from.ConvertUnicodeToANSI().GetMailAddress();
			}
			catch
			{
				fromAddress = UtilityService.GetAppSetting("Email:DefaultSender", "VIEApps NGX <vieapps.net@gmail.com>").GetMailAddress();
			}

			MailAddress replyToAddress = null;
			if (!string.IsNullOrWhiteSpace(replyTo))
				try
				{
					replyToAddress = replyTo.GetMailAddress();
				}
				catch { }

			var toAddresses = string.IsNullOrWhiteSpace(toEmails)
				? null
				: toEmails.ToArray(';', true, true).Select(email =>
				{
					try
					{
						return email.GetMailAddress();
					}
					catch
					{
						return null;
					}
				});

			var ccAddresses = string.IsNullOrWhiteSpace(toEmails)
				? null
				: ccEmails.ToArray(';', true, true).Select(email =>
				{
					try
					{
						return email.GetMailAddress();
					}
					catch
					{
						return null;
					}
				});

			var bccAddresses = string.IsNullOrWhiteSpace(toEmails)
				? null
				: bccEmails.ToArray(';', true, true).Select(email =>
				{
					try
					{
						return email.GetMailAddress();
					}
					catch
					{
						return null;
					}
				});

			// get the mail message
			return MessageService.GetMailMessage(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, new[] { attachment }, footer, priority, isBodyHtml, encoding, mailer);
		}

		/// <summary>
		/// Normalizes the email message
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static MailMessage Normalize(this MailMessage message)
		{
			if (message == null)
				throw new InformationInvalidException("The message is invalid");

			if (MessageService.HarmfulDomains.Count > 0)
			{
				if (message.To != null && message.To.Count > 0)
				{
					var index = 0;
					while (index < message.To.Count)
					{
						if (MessageService.HarmfulDomains.Contains(message.To[index].Address.GetDomain()))
							message.To.RemoveAt(index);
						else
							index++;
					}
				}

				if (message.CC != null && message.CC.Count > 0)
				{
					var index = 0;
					while (index < message.CC.Count)
					{
						if (MessageService.HarmfulDomains.Contains(message.CC[index].Address.GetDomain()))
							message.CC.RemoveAt(index);
						else
							index++;
					}
				}

				if (message.Bcc != null && message.Bcc.Count > 0)
				{
					var index = 0;
					while (index < message.Bcc.Count)
					{
						if (MessageService.HarmfulDomains.Contains(message.Bcc[index].Address.GetDomain()))
							message.Bcc.RemoveAt(index);
						else
							index++;
					}
				}
			}

			if ((message.To == null || message.To.Count < 1)
				&& (message.CC == null || message.CC.Count < 1)
				&& (message.Bcc == null || message.Bcc.Count < 1))
				throw new InvalidDataException("The message must have at least one recipient");

			return message;
		}
		#endregion

		#region Working with a SMTP client
		/// <summary>
		/// Gets the Smtp client for sending email messages
		/// </summary>
		/// <param name="host">The host address of the SMTP server (IP or host name)</param>
		/// <param name="port">The port number of SMTP service on the SMTP server</param>
		/// <param name="user">The name of user for connecting with SMTP server</param>
		/// <param name="password">The password of user for connecting with SMTP server</param>
		/// <param name="enableSsl">true if the SMTP server requires SSL</param>
		public static SmtpClient GetSmtpClient(string host, int port, string user, string password, bool enableSsl)
		{
			var smtp = new SmtpClient
			{
				Host = host ?? "127.0.0.1",
				Port = port,
				EnableSsl = enableSsl,
				DeliveryMethod = SmtpDeliveryMethod.Network
			};

			// credential
			if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
				smtp.Credentials = new NetworkCredential(user, password);

#if NETSTANDARD2_0
			// service point - only available on Windows with .NET Framework
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.FrameworkDescription.IsContains(".NET Framework"))
				smtp.ServicePoint.Expect100Continue = false;
#endif

			return smtp;
		}

		/// <summary>
		/// Gets the SMTP client for sending email messages
		/// </summary>
		/// <param name="host">The host address of the SMTP server (IP or host name)</param>
		/// <param name="port">The port number of SMTP service on the SMTP server</param>
		/// <param name="user">The name of user for connecting with SMTP server</param>
		/// <param name="password">The password of user for connecting with SMTP server</param>
		/// <param name="enableSsl">true if the SMTP server requires SSL</param>
		public static SmtpClient GetSmtpClient(string host, string port, string user, string password, bool enableSsl)
		{
			enableSsl = !string.IsNullOrWhiteSpace(host) ? enableSsl : "true".IsEquals(UtilityService.GetAppSetting("Email:SmtpServerEnableSsl"));
			host = !string.IsNullOrWhiteSpace(host) ? host : UtilityService.GetAppSetting("Email:SmtpServer");
			port = !string.IsNullOrWhiteSpace(port) ? port : UtilityService.GetAppSetting("Email:SmtpPort");
			user = !string.IsNullOrWhiteSpace(user) ? user : UtilityService.GetAppSetting("Email:SmtpUser");
			password = !string.IsNullOrWhiteSpace(password) ? password : UtilityService.GetAppSetting("Email:SmtpUserPassword");
			return MessageService.GetSmtpClient(host, Int32.TryParse(port, out var smtpPort) ? smtpPort : 25, user, password, enableSsl);
		}
		#endregion

		#region Send email messages
		/// <summary>
		/// Sends an email message using a SMTP client
		/// </summary>
		/// <param name="smtp">The SMTP client for sending email</param>
		/// <param name="message">The email message</param>
		public static void SendMail(this SmtpClient smtp, MailMessage message)
		{
			// check
			if (smtp == null)
				throw new InformationInvalidException("The SMTP client is invalid");
			if (message == null)
				throw new InformationInvalidException("The message is invalid");

			// send
			smtp.Send(message.Normalize());
		}

		/// <summary>
		/// Sends an email message using a SMTP client
		/// </summary>
		/// <param name="smtp">The SMTP client for sending email</param>
		/// <param name="message">The email message</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static Task SendMailAsync(this SmtpClient smtp, MailMessage message, CancellationToken cancellationToken)
			=> smtp == null
				? Task.FromException(new InformationInvalidException("The SMTP client is invalid"))
				: message == null
					? Task.FromException(new InformationInvalidException("The message is invalid"))
#if NETSTANDARD2_0
					: smtp.SendMailAsync(message.Normalize()).WithCancellationToken(cancellationToken);
#else
					: smtp.SendMailAsync(message.Normalize(), cancellationToken);
#endif

		/// <summary>
		/// Sends an email message using a SMTP client
		/// </summary>
		/// <param name="smtp">The SMTP client for sending email</param>
		/// <param name="fromAddress">Sender address</param>
		/// <param name="replyToAddress">Address will be replied to</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachments">Collection of attachment files (means the collection of files with full path)</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		public static void SendMail(this SmtpClient smtp, MailAddress fromAddress, MailAddress replyToAddress, IEnumerable<MailAddress> toAddresses, IEnumerable<MailAddress> ccAddresses, IEnumerable<MailAddress> bccAddresses, string subject, string body, IEnumerable<string> attachments, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null)
		{
			if (smtp == null)
				throw new InformationInvalidException("The SMTP client is invalid");
			smtp.SendMail(MessageService.GetMailMessage(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, footer, priority, isBodyHtml, encoding, mailer));
		}

		/// <summary>
		/// Sends an email message using a SMTP client
		/// </summary>
		/// <param name="smtp">The SMTP client for sending email</param>
		/// <param name="fromAddress">Sender address</param>
		/// <param name="replyToAddress">Address will be replied to</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachments">Collection of attachment files (means the collection of files with full path)</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isHtmlFormat">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static Task SendMailAsync(this SmtpClient smtp, MailAddress fromAddress, MailAddress replyToAddress, IEnumerable<MailAddress> toAddresses, IEnumerable<MailAddress> ccAddresses, IEnumerable<MailAddress> bccAddresses, string subject, string body, IEnumerable<string> attachments, string footer = null, MailPriority priority = MailPriority.Normal, bool isHtmlFormat = true, Encoding encoding = null, string mailer = null, CancellationToken cancellationToken = default)
			=> smtp == null
				? Task.FromException(new InformationInvalidException("The SMTP client is invalid"))
				: smtp.SendMailAsync(MessageService.GetMailMessage(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, footer, priority, isHtmlFormat, encoding, mailer), cancellationToken);

		/// <summary>
		/// Sends an email message using the default SMTP client
		/// </summary>
		/// <param name="fromAddress">Sender address</param>
		/// <param name="replyToAddress">Address will be replied to</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachments">Collection of attachment files (means the collection of files with full path)</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		/// <param name="smtpServerHost">The host address of the SMTP server (IP or host name)</param>
		/// <param name="smtpServerPort">The port number of SMTP service on the SMTP server</param>
		/// <param name="smtpServerUser">The name of user for connecting with SMTP server</param>
		/// <param name="smtpServerPassword">The password of user for connecting with SMTP server</param>
		/// <param name="smtpServerEnableSsl">true if the SMTP server requires SSL</param>
		public static void SendMail(MailAddress fromAddress, MailAddress replyToAddress, IEnumerable<MailAddress> toAddresses, IEnumerable<MailAddress> ccAddresses, IEnumerable<MailAddress> bccAddresses, string subject, string body, IEnumerable<string> attachments, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null, string smtpServerHost = null, string smtpServerPort = null, string smtpServerUser = null, string smtpServerPassword = null, bool smtpServerEnableSsl = true)
		{
			using (var smtp = MessageService.GetSmtpClient(smtpServerHost, smtpServerPort, smtpServerUser, smtpServerPassword, smtpServerEnableSsl))
			{
				smtp.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, footer, priority, isBodyHtml, encoding, mailer);
			}
		}

		/// <summary>
		/// Sends an email message using the default SMTP client
		/// </summary>
		/// <param name="fromAddress">Sender address</param>
		/// <param name="replyToAddress">Address will be replied to</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachments">Collection of attachment files (means the collection of files with full path)</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		/// <param name="smtpServerHost">The host address of the SMTP server (IP or host name)</param>
		/// <param name="smtpServerPort">The port number of SMTP service on the SMTP server</param>
		/// <param name="smtpServerUser">The name of user for connecting with SMTP server</param>
		/// <param name="smtpServerPassword">The password of user for connecting with SMTP server</param>
		/// <param name="smtpServerEnableSsl">true if the SMTP server requires SSL</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static async Task SendMailAsync(MailAddress fromAddress, MailAddress replyToAddress, IEnumerable<MailAddress> toAddresses, IEnumerable<MailAddress> ccAddresses, IEnumerable<MailAddress> bccAddresses, string subject, string body, IEnumerable<string> attachments, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null, string smtpServerHost = null, string smtpServerPort = null, string smtpServerUser = null, string smtpServerPassword = null, bool smtpServerEnableSsl = true, CancellationToken cancellationToken = default)
		{
			using (var smtp = MessageService.GetSmtpClient(smtpServerHost, smtpServerPort, smtpServerUser, smtpServerPassword, smtpServerEnableSsl))
			{
				await smtp.SendMailAsync(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, footer, priority, isBodyHtml, encoding, mailer, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Sends an email message using a SMTP client
		/// </summary>
		/// <param name="smtp">The SMTP client for sending email</param>
		/// <param name="from">Sender name and e-mail address</param>
		/// <param name="replyTo">Address will be replied to</param>
		/// <param name="to">Recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="cc">CC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="bcc">BCC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachment">The full path to an attachment file</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		public static void SendMail(this SmtpClient smtp, string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null)
		{
			if (smtp == null)
				throw new InformationInvalidException("The SMTP client is invalid");
			smtp.SendMail(MessageService.GetMailMessage(from, replyTo, to, cc, bcc, subject, body, attachment, footer, priority, isBodyHtml, encoding, mailer));
		}

		/// <summary>
		/// Sends an email message using a SMTP client
		/// </summary>
		/// <param name="smtp">The SMTP client for sending email</param>
		/// <param name="from">Sender name and e-mail address</param>
		/// <param name="replyTo">Address will be replied to</param>
		/// <param name="to">Recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="cc">CC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="bcc">BCC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachment">The full path to an attachment file</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static Task SendMailAsync(this SmtpClient smtp, string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null, CancellationToken cancellationToken = default)
			=> smtp == null
				? Task.FromException(new InformationInvalidException("The SMTP client is invalid"))
				: smtp.SendMailAsync(MessageService.GetMailMessage(from, replyTo, to, cc, bcc, subject, body, attachment, footer, priority, isBodyHtml, encoding, mailer), cancellationToken);

		/// <summary>
		/// Sends an email message using the default SMTP client
		/// </summary>
		/// <param name="from">Sender name and e-mail address</param>
		/// <param name="replyTo">Address will be replied to</param>
		/// <param name="to">Recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="cc">CC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="bcc">BCC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachment">The full path to an attachment file</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		/// <param name="smtpServerHost">The host address of the SMTP server (IP or host name)</param>
		/// <param name="smtpServerPort">The port number of SMTP service on the SMTP server</param>
		/// <param name="smtpServerUser">The name of user for connecting with SMTP server</param>
		/// <param name="smtpServerPassword">The password of user for connecting with SMTP server</param>
		/// <param name="smtpServerEnableSsl">true if the SMTP server requires SSL</param>
		public static void SendMail(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null, string smtpServerHost = null, string smtpServerPort = null, string smtpServerUser = null, string smtpServerPassword = null, bool smtpServerEnableSsl = true)
		{
			using (var smtp = MessageService.GetSmtpClient(smtpServerHost, smtpServerPort, smtpServerUser, smtpServerPassword, smtpServerEnableSsl))
			{
				smtp.SendMail(from, replyTo, to, cc, bcc, subject, body, attachment, footer, priority, isBodyHtml, encoding, mailer);
			}
		}

		/// <summary>
		/// Sends an email message using the default SMTP client
		/// </summary>
		/// <param name="from">Sender name and e-mail address</param>
		/// <param name="replyTo">Address will be replied to</param>
		/// <param name="to">Recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="cc">CC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="bcc">BCC recipients, seperated multiple by semi-colon (;)</param>
		/// <param name="subject">The message subject</param>
		/// <param name="body">The message body</param>
		/// <param name="attachment">The full path to an attachment file</param>
		/// <param name="footer">The additional footer (will be placed at the bottom of the body)</param>
		/// <param name="priority">The priority</param>
		/// <param name="isBodyHtml">true if the message body is HTML formated</param>
		/// <param name="encoding">Encoding of subject and body message</param>
		/// <param name="mailer">The name of mailer agent (means 'x-mailer' header)</param>
		/// <param name="smtpServerHost">The host address of the SMTP server (IP or host name)</param>
		/// <param name="smtpServerPort">The port number of SMTP service on the SMTP server</param>
		/// <param name="smtpServerUser">The name of user for connecting with SMTP server</param>
		/// <param name="smtpServerPassword">The password of user for connecting with SMTP server</param>
		/// <param name="smtpServerEnableSsl">true if the SMTP server requires SSL</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static async Task SendMailAsync(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, string footer = null, MailPriority priority = MailPriority.Normal, bool isBodyHtml = true, Encoding encoding = null, string mailer = null, string smtpServerHost = null, string smtpServerPort = null, string smtpServerUser = null, string smtpServerPassword = null, bool smtpServerEnableSsl = true, CancellationToken cancellationToken = default)
		{
			using (var smtp = MessageService.GetSmtpClient(smtpServerHost, smtpServerPort, smtpServerUser, smtpServerPassword, smtpServerEnableSsl))
			{
				await smtp.SendMailAsync(from, replyTo, to, cc, bcc, subject, body, attachment, footer, priority, isBodyHtml, encoding, mailer, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Sends the collection of email messages using a SMTP client
		/// </summary>
		/// <param name="smtp">The SMTP client for sending email</param>
		/// <param name="messages">The collection of email messages</param>
		public static void SendMails(this SmtpClient smtp, IEnumerable<MailMessage> messages)
		{
			if (smtp == null)
				throw new InformationInvalidException("The SMTP client is invalid");
			messages?.Where(message => message != null).ForEach(message => smtp.Send(message.Normalize()));
		}

		/// <summary>
		/// Sends the collection of email messages using a SMTP client
		/// </summary>
		/// <param name="smtp"></param>
		/// <param name="messages"></param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static Task SendMailsAsync(this SmtpClient smtp, IEnumerable<MailMessage> messages, CancellationToken cancellationToken = default)
			=> smtp == null
				? Task.FromException(new InformationInvalidException("The SMTP client is invalid"))
				: messages == null
					? Task.CompletedTask
					: messages.Where(message => message != null).ForEachAsync((message, token) => smtp.SendMailAsync(message, token), cancellationToken);

		/// <summary>
		/// Sends this email message
		/// </summary>
		/// <param name="message">The email message</param>
		public static void SendMessage(this EmailMessage message)
		{
			if (message == null)
				throw new InformationInvalidException("The message is invalid");
			MessageService.SendMail(message.From, message.ReplyTo, message.To, message.Cc, message.Bcc, message.Subject, message.Body, message.Attachment, null, message.Priority, message.IsBodyHtml, Encoding.GetEncoding(message.Encoding), null, message.SmtpServer, message.SmtpServerPort.ToString(), message.SmtpUsername, message.SmtpPassword, message.SmtpServerEnableSsl);
		}

		/// <summary>
		/// Sends this email message
		/// </summary>
		/// <param name="message">The email message</param>
		/// <param name="cancellationToken">The cancellation token</param>
		public static Task SendMessageAsync(this EmailMessage message, CancellationToken cancellationToken = default)
			=> message == null
				? Task.FromException(new InformationInvalidException("The message is invalid"))
				: MessageService.SendMailAsync(message.From, message.ReplyTo, message.To, message.Cc, message.Bcc, message.Subject, message.Body, message.Attachment, null, message.Priority, message.IsBodyHtml, Encoding.GetEncoding(message.Encoding), null, message.SmtpServer, message.SmtpServerPort.ToString(), message.SmtpUsername, message.SmtpPassword, message.SmtpServerEnableSsl, cancellationToken);
		#endregion

		#region Send webhook messages
		/// <summary>
		/// Normalizes the web-hook message
		/// </summary>
		/// <param name="message">The web-hook message</param>
		/// <param name="signAlgorithm">The HMAC algorithm to sign the body with the specified key (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <param name="signKey">The key that use to sign</param>
		/// <param name="signatureName">The name of the signature parameter, default is combination of algorithm and the string 'Signature', ex: HmacSha256Signature</param>
		/// <param name="signatureAsHex">true to use signature as hex, false to use as Base64</param>
		/// <param name="signatureInQuery">true to place the signature in query string, false to place in header, default is false</param>
		/// <param name="additionalQuery">The additional query string</param>
		/// <param name="additionalHeader">The additional header</param>
		/// <returns></returns>
		public static WebHookMessage Normalize(this WebHookMessage message, string signAlgorithm = "SHA256", string signKey = null, string signatureName = null, bool signatureAsHex = true, bool signatureInQuery = false, Dictionary<string, string> additionalQuery = null, Dictionary<string, string> additionalHeader = null)
		{
			if (message == null)
				throw new InformationInvalidException("The message is invalid (null)");
			else if (string.IsNullOrWhiteSpace(message.EndpointURL) || string.IsNullOrWhiteSpace(message.Body))
				throw new InformationInvalidException("The message is invalid (no end-point or no body)");

			if (string.IsNullOrWhiteSpace(signAlgorithm) || !CryptoService.HmacHashAlgorithmFactories.ContainsKey(signAlgorithm))
				signAlgorithm = "SHA256";

			if (string.IsNullOrWhiteSpace(signatureName))
				signatureName = $"Hmac{signAlgorithm.GetCapitalizedFirstLetter()}Signature";

			var query = new Dictionary<string, string>(additionalQuery ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
			message.Query?.ForEach(kvp => query[kvp.Key] = kvp.Value);

			var header = new Dictionary<string, string>(additionalHeader ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
			message.Header?.ForEach(kvp => header[kvp.Key] = kvp.Value);

			using (var hasher = CryptoService.GetHMACHashAlgorithm((signKey ?? CryptoService.DEFAULT_PASS_PHRASE).ToBytes(), signAlgorithm))
			{
				var signature = signatureAsHex
					? hasher.ComputeHash(message.Body.ToBytes()).ToHex()
					: hasher.ComputeHash(message.Body.ToBytes()).ToBase64();
				if (signatureInQuery)
					query[signatureName] = signature;
				else
					header[signatureName] = signature;
			}

			message.Query = query;
			message.Header = header;

			return message;
		}

		/// <summary>
		/// Sends a web-hook message (means post a JSON document to a specified URL)
		/// </summary>
		/// <param name="message">The well-formed webhook message to send</param>
		/// <param name="userAgent">The additional name to add to user agent string, default value is 'VIEApps NGX WebHook Sender'</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendMessageAsync(this WebHookMessage message, string userAgent, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(message?.EndpointURL) || string.IsNullOrWhiteSpace(message?.Body))
				return Task.FromException<HttpResponseMessage>(new InformationInvalidException(message == null ? "The message is invalid (null)" : "The message is invalid (no end-point or no body)"));
			var uri = new Uri($"{message.EndpointURL}{(message.Query.Any() ? message.EndpointURL.IndexOf("?") > 0 ? "&" : "?" : "")}{message.Query.ToString("&", kvp => $"{kvp.Key}={kvp.Value?.UrlEncode()}")}");
			var headers = new Dictionary<string, string>(message.Header ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
			{
				["User-Agent"] = $"{UtilityService.DesktopUserAgent} {userAgent ?? $"VIEApps NGX WebHook Sender/{Assembly.GetCallingAssembly().GetVersion(false)}"}",
				["Content-Type"] = "application/json; charset=utf-8"
			};
			return uri.SendHttpRequestAsync("POST", headers, message.Body, 120, cancellationToken);
		}

		/// <summary>
		/// Sends a web-hook message (means post a JSON document to a specified URL)
		/// </summary>
		/// <param name="message">The well-formed webhook message to send</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendMessageAsync(this WebHookMessage message, CancellationToken cancellationToken = default)
			=> message.SendMessageAsync(null, cancellationToken);
		#endregion

	}
}