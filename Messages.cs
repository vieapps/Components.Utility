#region Related components
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Presents an email message
	/// </summary>
	[Serializable, DebuggerDisplay("Subject = {Subject}")]
	public class EmailMessage
	{
		/// <summary>
		/// Initializes a new email message
		/// </summary>
		/// <param name="encryptedMessage"></param>
		public EmailMessage(string encryptedMessage = null)
		{
			this.ID = UtilityService.NewUUID;
			this.SendingTime = DateTime.Now;
			this.From = "";
			this.ReplyTo = "";
			this.To = "";
			this.Cc = "";
			this.Bcc = "";
			this.Subject = "";
			this.Body = "";
			this.Footer = "";
			this.Attachment = "";
			this.Priority = MailPriority.Normal;
			this.IsHtmlFormat = true;
			this.Encoding = System.Text.Encoding.UTF8.CodePage;
			this.SmtpServer = "";
			this.SmtpServerPort = 25;
			this.SmtpUsername = "";
			this.SmtpPassword = "";
			this.SmtpServerEnableSsl = false;
			this.SmtpStartTls = false;

			if (!string.IsNullOrWhiteSpace(encryptedMessage))
				try
				{
					this.CopyFrom(encryptedMessage.Decrypt(EmailMessage.EncryptionKey).FromJson<EmailMessage>());
				}
				catch { }
		}

		#region Properties
		public string From { get; set; }
		public string ReplyTo { get; set; }
		public string To { get; set; }
		public string Cc { get; set; }
		public string Bcc { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string Footer { get; set; }
		public string Attachment { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		public MailPriority Priority { get; set; }
		public bool IsHtmlFormat { get; set; }
		public int Encoding { get; set; }
		public string SmtpServer { get; set; }
		public int SmtpServerPort { get; set; }
		public string SmtpUsername { get; set; }
		public string SmtpPassword { get; set; }
		public bool SmtpServerEnableSsl { get; set; }
		public bool SmtpStartTls { get; set; }
		#endregion

		#region Helper properties
		static string _EncryptionKey = null;

		internal static string EncryptionKey
		{
			get
			{
				return EmailMessage._EncryptionKey ?? (EmailMessage._EncryptionKey = UtilityService.GetAppSetting("Keys:MessageEncryption", "VIE-Apps-9D17C42D-Core-AE9F-Components-4D72-Email-586D-Encryption-277D9E606F1F-Keys"));
			}
		}

		/// <summary>
		/// Gets or sets the identity of the email message.
		/// </summary>
		/// <remarks>
		/// If other message had same Id, that message will be overried by is message
		/// </remarks>
		public string ID { get; set; }

		/// <summary>
		/// Gets or sets time to start to send this message via email.
		/// </summary>
		/// <remarks>
		/// Set a specifict time to tell mailer send this message from this time
		/// </remarks>
		public DateTime SendingTime { get; set; }
		#endregion

		#region Working with files
		/// <summary>
		/// Loads message from file and deserialize as object.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		public static EmailMessage Load(string filePath)
		{
			return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)
				? new EmailMessage(UtilityService.ReadTextFile(filePath))
				: null;
		}

		/// <summary>
		/// Loads message from file and deserialize as object.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		public static async Task<EmailMessage> LoadAsync(string filePath)
		{
			return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)
				? new EmailMessage(await UtilityService.ReadTextFileAsync(filePath).ConfigureAwait(false))
				: null;
		}

		/// <summary>
		/// Serializes and saves message into file.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="folderPath">The path to folder that stores queue of email messages.</param>
		public static void Save(EmailMessage message, string folderPath)
		{
			if (message != null && Directory.Exists(folderPath))
				try
				{
					UtilityService.WriteTextFile(Path.Combine(folderPath, message.ID + ".msg"), message.Encrypted);
				}
				catch { }
		}

		/// <summary>
		/// Serializes and saves message into file.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="folderPath">The path to folder that stores queue of email messages.</param>
		public static async Task SaveAsync(EmailMessage message, string folderPath)
		{
			if (message != null && Directory.Exists(folderPath))
				try
				{
					await UtilityService.WriteTextFileAsync(Path.Combine(folderPath, message.ID + ".msg"), message.Encrypted).ConfigureAwait(false);
				}
				catch { }
		}

		/// <summary>
		/// Gets the string that presents the encrypted messages
		/// </summary>
		[JsonIgnore]
		public string Encrypted
		{
			get
			{
				return this.ToJson().ToString(Formatting.None).Encrypt(EmailMessage.EncryptionKey);
			}
		}
		#endregion

	}

	/// <summary>
	/// Presents a web-hook message
	/// </summary>
	[Serializable, DebuggerDisplay("Endpoint = {EndpointURL}")]
	public class WebHookMessage
	{
		/// <summary>
		/// Initializes a new web-hook message
		/// </summary>
		/// <param name="encryptedMessage"></param>
		public WebHookMessage(string encryptedMessage = null)
		{
			this.ID = UtilityService.NewUUID;
			this.SendingTime = DateTime.Now;
			this.EndpointURL = "";
			this.Body = "";
			this.Header = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			this.Query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (!string.IsNullOrWhiteSpace(encryptedMessage))
				try
				{
					this.CopyFrom(encryptedMessage.Decrypt(WebHookMessage.EncryptionKey).FromJson<WebHookMessage>());
				}
				catch { }
		}

		#region Properties
		/// <summary>
		/// Gets or Sets the url of webhook's endpoint
		/// </summary>
		public string EndpointURL { get; set; }

		/// <summary>
		/// Gets or Sets the body of the webhook message
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Gets or Sets header of webhook message
		/// </summary>
		[JsonIgnore]
		public Dictionary<string, string> Header { get; set; }

		/// <summary>
		/// Gets or Sets query-string of webhook message
		/// </summary>
		[JsonIgnore]
		public Dictionary<string, string> Query { get; set; }
		#endregion

		#region Helper properties
		static string _EncryptionKey = null;

		internal static string EncryptionKey
		{
			get
			{
				return WebHookMessage._EncryptionKey ?? (WebHookMessage._EncryptionKey = UtilityService.GetAppSetting("Keys:MessageEncryption", "VIE-Apps-5D659BA4-Core-23BE-Components-4E43-WebHook-81E4-Encryption-EACD7EDE222A-Keys"));
			}
		}

		/// <summary>
		/// Gets or sets identity of the message.
		/// </summary>
		public string ID { get; set; }

		/// <summary>
		/// Gets or sets time to start to send this message.
		/// </summary>
		public DateTime SendingTime { get; set; }
		#endregion

		#region Working with files
		/// <summary>
		/// Loads message from file and deserialize as object.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		public static WebHookMessage Load(string filePath)
		{
			return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)
				? new WebHookMessage(UtilityService.ReadTextFile(filePath))
				: null;
		}

		/// <summary>
		/// Loads message from file and deserialize as object.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		public static async Task<WebHookMessage> LoadAsync(string filePath)
		{
			return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)
				? new WebHookMessage(await UtilityService.ReadTextFileAsync(filePath).ConfigureAwait(false))
				: null;
		}

		/// <summary>
		/// Serializes and saves message into file.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="folderPath">The path to folder that stores queue of messages.</param>
		public static void Save(WebHookMessage message, string folderPath)
		{
			if (message != null && Directory.Exists(folderPath))
				try
				{
					UtilityService.WriteTextFile(Path.Combine(folderPath, message.ID + ".msg"), message.Encrypted);
				}
				catch { }
		}

		/// <summary>
		/// Serializes and saves message into file.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="folderPath">The path to folder that stores queue of messages.</param>
		public static async Task SaveAsync(WebHookMessage message, string folderPath)
		{
			if (message != null && Directory.Exists(folderPath))
				try
				{
					await UtilityService.WriteTextFileAsync(Path.Combine(folderPath, message.ID + ".msg"), message.Encrypted).ConfigureAwait(false);
				}
				catch { }
		}

		/// <summary>
		/// Gets the string that presents the encrypted messages
		/// </summary>
		[JsonIgnore]
		public string Encrypted
		{
			get
			{
				return this.ToJson().ToString(Formatting.None).Encrypt(WebHookMessage.EncryptionKey);
			}
		}
		#endregion

	}

	// ------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Collection of global methods for sending a message (email and web hook)
	/// </summary>
	public static partial class MessageService
	{

		#region Helper methods
		/// <summary>
		/// Prepares a valid email address
		/// </summary>
		/// <param name="emailInfo">The string that presents information of an email adress before validating</param>
		/// <param name="convertNameToANSI">true to convert display name as ANSI</param>
		/// <returns><see cref="System.Net.Mail.MailAddress">MailAddress</see> object that contains valid email address</returns>
		public static MailAddress GetMailAddress(this string emailInfo, bool convertNameToANSI = false)
		{
			if (string.IsNullOrWhiteSpace(emailInfo))
				return null;

			string email = "", displayName = "";

			var emails = emailInfo.ToArray('<');
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
		#endregion

		#region Methods to send an email
		/// <summary>
		/// Send an e-mail within VIE Portal software using System.Net.Mail namespace.
		/// </summary>
		/// <param name="from">Sender name and e-mail address.</param>
		/// <param name="replyTo">Address will be replied to.</param>
		/// <param name="to">Recipients. Seperate multiple by comma (,).</param>
		/// <param name="cc">CC recipients. Seperate multiple by comma (,).</param>
		/// <param name="bcc">BCC recipients. Seperate multiple by comma (,).</param>
		/// <param name="subject">Mail subject.</param>
		/// <param name="body">Mail body.</param>
		/// <param name="attachment">Path to attachment file.</param>
		/// <param name="priority">Priority. See <c>System.Net.Mail.MailPriority</c> class for more information.</param>
		/// <param name="isHtmlFormat">TRUE if the message body is HTML formated.</param>
		/// <param name="encoding">Encoding of body message. See <c>System.Web.Mail.MailEncoding</c> class for more information.</param>
		/// <param name="smtpServer">IP address or host name of SMTP server.</param>
		/// <param name="smtpServerPort">Port number for SMTP service on the SMTP server.</param>
		/// <param name="smtpUsername">Username of the SMTP server use for sending mail.</param>
		/// <param name="smtpPassword">Password of user on the SMTP server use for sending mail.</param>
		/// <param name="additionalFooter">Additional content will be added into email as footer.</param>
		/// <param name="preventDomains">Collection of harmful domains need to prevent.</param>
		public static void SendMail(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, System.Net.Mail.MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, string additionalFooter, HashSet<string> preventDomains)
		{
			MessageService.SendMail(from, replyTo, to, cc, bcc, subject, body, attachment, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, false, additionalFooter, preventDomains);
		}

		/// <summary>
		/// Send an e-mail within VIE Portal software using System.Net.Mail namespace.
		/// </summary>
		/// <param name="from">Sender name and e-mail address.</param>
		/// <param name="replyTo">Address will be replied to.</param>
		/// <param name="to">Recipients. Seperate multiple by comma (,).</param>
		/// <param name="cc">CC recipients. Seperate multiple by comma (,).</param>
		/// <param name="bcc">BCC recipients. Seperate multiple by comma (,).</param>
		/// <param name="subject">Mail subject.</param>
		/// <param name="body">Mail body.</param>
		/// <param name="attachment">Path to attachment file.</param>
		/// <param name="priority">Priority. See <c>System.Net.Mail.MailPriority</c> class for more information.</param>
		/// <param name="isHtmlFormat">TRUE if the message body is HTML formated.</param>
		/// <param name="encoding">Encoding of body message. See <c>System.Web.Mail.MailEncoding</c> class for more information.</param>
		/// <param name="smtpServer">IP address or host name of SMTP server.</param>
		/// <param name="smtpServerPort">Port number for SMTP service on the SMTP server.</param>
		/// <param name="smtpUsername">Username of the SMTP server use for sending mail.</param>
		/// <param name="smtpPassword">Password of user on the SMTP server use for sending mail.</param>
		/// <param name="smtpEnableSsl">TRUE if the SMTP server requires SSL.</param>
		/// <param name="additionalFooter">Additional content will be added into email as footer.</param>
		/// <param name="preventDomains">Collection of harmful domains need to prevent.</param>
		public static void SendMail(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, System.Net.Mail.MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl, string additionalFooter, HashSet<string> preventDomains)
		{
			// check sender
			if (from.Equals(""))
				throw new InvalidDataException("No sender information for the message!");

			// validate recipients
			var toEmails = string.IsNullOrWhiteSpace(to)
				? ""
				: to.Trim();

			var ccEmails = string.IsNullOrWhiteSpace(cc)
				? ""
				: cc.Trim();

			var bccEmails = string.IsNullOrWhiteSpace(bcc)
				? ""
				: bcc.Trim();

			// remove all harmful domains
			if (preventDomains != null && preventDomains.Count > 0)
			{
				string[] emails = null;

				// to
				if (!toEmails.Equals(""))
				{
					emails = toEmails.ToArray(';');
					toEmails = "";
					emails.ForEach(email => toEmails += (!toEmails.Equals("") ? ";" : "") + (!preventDomains.Contains(email.GetDomain()) ? email : ""));
				}

				// cc
				if (!ccEmails.Equals(""))
				{
					emails = ccEmails.ToArray(';');
					ccEmails = "";
					emails.ForEach(email => ccEmails += (!ccEmails.Equals("") ? ";" : "") + (!preventDomains.Contains(email.GetDomain()) ? email : ""));
				}

				// bcc
				if (!bccEmails.Equals(""))
				{
					emails = bccEmails.ToArray(';');
					bccEmails = "";
					emails.ForEach(email => bccEmails += (!bccEmails.Equals("") ? ";" : "") + (!preventDomains.Contains(email.GetDomain()) ? email : ""));
				}
			}

			// check recipients
			if (toEmails.Equals("") && ccEmails.Equals("") && bccEmails.Equals(""))
				throw new InvalidDataException("No recipients for the message!");

			// get sender information
			MailAddress fromAddress = null;
			try
			{
				fromAddress = from.ConvertUnicodeToANSI().GetMailAddress();
			}
			catch
			{
				fromAddress = "VIEApps NGX <vieapps.net@gmail.com>".GetMailAddress();
			}

			// reply to
			MailAddress replyToAddress = null;
			if (!string.IsNullOrWhiteSpace(replyTo))
				try
				{
					replyToAddress = replyTo.GetMailAddress();
				}
				catch { }

			// recipients
			List<MailAddress> toAddresses = null;
			if (!string.IsNullOrWhiteSpace(toEmails))
			{
				toAddresses = new List<MailAddress>();
				toEmails.ToArray(';').ForEach(email =>
				{
					try
					{
						toAddresses.Add(email.GetMailAddress());
					}
					catch { }
				});
			}

			List<MailAddress> ccAddresses = null;
			if (!string.IsNullOrWhiteSpace(ccEmails))
			{
				ccAddresses = new List<MailAddress>();
				ccEmails.ToArray(';').ForEach(email =>
				{
					try
					{
						ccAddresses.Add(email.GetMailAddress());
					}
					catch { }
				});
			}

			List<MailAddress> bccAddresses = null;
			if (!string.IsNullOrWhiteSpace(bccEmails))
			{
				bccAddresses = new List<MailAddress>();
				bccEmails.ToArray(';').ForEach(email =>
				{
					try
					{
						bccAddresses.Add(email.GetMailAddress());
					}
					catch { }
				});
			}

			// prepare attachments
			var attachments = attachment != null && File.Exists(attachment)
				? new List<string>() { attachment }
				:  null;

			// send mail
			MessageService.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl);
		}

		/// <summary>
		/// Send an e-mail within VIE Portal software using System.Net.Mail namespace.
		/// </summary>
		/// <param name="fromAddress">Sender address.</param>
		/// <param name="replyToAddress">Address will be replied to.</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject.</param>
		/// <param name="body">The message body.</param>
		/// <param name="attachments">Collection of attachment files (all are full path of attachments).</param>
		/// <param name="additionalFooter">The data will be added into email as footer.</param>
		/// <param name="priority">Priority. See <c>System.Net.Mail.MailPriority</c> class for more information.</param>
		/// <param name="isHtmlFormat">TRUE if the message body is HTML formated.</param>
		/// <param name="encoding">Encoding of body message. See <c>System.Web.Mail.MailEncoding</c> class for more information.</param>
		/// <param name="smtpServer">IP address or host name of SMTP server.</param>
		/// <param name="smtpServerPort">Port number for SMTP service on the SMTP server.</param>
		/// <param name="smtpUsername">Username of the SMTP server use for sending mail.</param>
		/// <param name="smtpPassword">Password of user on the SMTP server use for sending mail.</param>
		/// <param name="smtpEnableSsl">TRUE if the SMTP server requires SSL.</param>
		public static void SendMail(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl)
		{
			// prepare SMTP server
			var smtp = new SmtpClient();

			// host name (IP of DNS of SMTP server)
			smtp.Host = !string.IsNullOrWhiteSpace(smtpServer)
				? smtpServer
				: "127.0.0.1";

			// port
			try
			{
				smtp.Port = Convert.ToInt32(smtpServerPort);
			}
			catch
			{
				smtp.Port = 25;
			}

			// credential (username/password)
			if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
				smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

			// SSL
			smtp.EnableSsl = smtpEnableSsl;

			// delivery method
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

			// send email
			MessageService.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtp);
		}

		/// <summary>
		/// Send an e-mail within VIE Portal software using System.Net.Mail namespace.
		/// </summary>
		/// <param name="fromAddress">Sender address.</param>
		/// <param name="replyToAddress">Address will be replied to.</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject.</param>
		/// <param name="body">The message body.</param>
		/// <param name="attachments">Collection of attachment files (all are full path of attachments).</param>
		/// <param name="additionalFooter">The data will be added into email as footer.</param>
		/// <param name="priority">Priority. See <c>System.Net.Mail.MailPriority</c> class for more information.</param>
		/// <param name="isHtmlFormat">TRUE if the message body is HTML formated.</param>
		/// <param name="encoding">Encoding of body message. See <c>System.Web.Mail.MailEncoding</c> class for more information.</param>
		/// <param name="smtp">Informaiton of SMTP server for sending email.</param>
		public static void SendMail(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, SmtpClient smtp)
		{
			// check
			if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
				throw new InvalidDataException("The email must have subject and body");

			if (fromAddress == null || string.IsNullOrWhiteSpace(fromAddress.Address))
				throw new InvalidDataException("The email must have sender address");

			if ((toAddresses == null || toAddresses.Count < 1)
				&& (ccAddresses == null || ccAddresses.Count < 1)
				&& (bccAddresses == null || bccAddresses.Count < 1))
				throw new InvalidDataException("The email must have at least one recipients");

			if (smtp == null)
				throw new InvalidDataException("You must provide SMTP information for sending an email.");

			// create new message object
			var message = new MailMessage();

			// sender
			message.From = new MailAddress(fromAddress.Address, fromAddress.DisplayName.ConvertUnicodeToANSI(), Encoding.UTF8);

			// reply to
			if (replyToAddress != null)
				message.ReplyToList.Add(replyToAddress);

			// recipients (TO)
			if (toAddresses != null)
				foreach (MailAddress emailAddress in toAddresses)
					message.To.Add(emailAddress);

			// recipients (CC)
			if (ccAddresses != null)
				foreach (MailAddress emailAddress in ccAddresses)
					message.CC.Add(emailAddress);

			// recipients (BCC)
			if (bccAddresses != null)
				foreach (MailAddress emailAddress in bccAddresses)
					message.Bcc.Add(emailAddress);

			// format
			message.Priority = priority;
			message.BodyEncoding = message.SubjectEncoding = encoding;
			message.IsBodyHtml = isHtmlFormat;

			// subject
			message.Subject = subject;

			// body
			message.Body = body + (!string.IsNullOrWhiteSpace(additionalFooter) ? additionalFooter : "");

			// attachment
			if (attachments != null && attachments.Count > 0)
				attachments.ForEach(attachment =>
				{
					if (!string.IsNullOrWhiteSpace(attachment) && File.Exists(attachment))
						message.Attachments.Add(new System.Net.Mail.Attachment(attachment));
				});

			// additional headers
			message.Headers.Add("x-mailer", "VIEApps NGX Mailer");

			// switch off certificate validation (http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using)
			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
			{
				return true;
			};

			// send message
			smtp.Send(message);
		}
		#endregion

		#region Methods to send an email in asynchronous way
		/// <summary>
		/// Send an e-mail within VIE Portal software using System.Net.Mail namespace.
		/// </summary>
		/// <param name="from">Sender name and e-mail address.</param>
		/// <param name="replyTo">Address will be replied to.</param>
		/// <param name="to">Recipients. Seperate multiple by comma (,).</param>
		/// <param name="cc">CC recipients. Seperate multiple by comma (,).</param>
		/// <param name="bcc">BCC recipients. Seperate multiple by comma (,).</param>
		/// <param name="subject">Mail subject.</param>
		/// <param name="body">Mail body.</param>
		/// <param name="attachment">Path to attachment file.</param>
		/// <param name="priority">Priority. See <c>System.Net.Mail.MailPriority</c> class for more information.</param>
		/// <param name="isHtmlFormat">TRUE if the message body is HTML formated.</param>
		/// <param name="encoding">Encoding of body message. See <c>System.Web.Mail.MailEncoding</c> class for more information.</param>
		/// <param name="smtpServer">IP address or host name of SMTP server.</param>
		/// <param name="smtpServerPort">Port number for SMTP service on the SMTP server.</param>
		/// <param name="smtpUsername">Username of the SMTP server use for sending mail.</param>
		/// <param name="smtpPassword">Password of user on the SMTP server use for sending mail.</param>
		/// <param name="smtpEnableSsl">TRUE if the SMTP server requires SSL.</param>
		/// <param name="cancellationToken">Token for cancelling this task.</param>
		public static Task SendMailAsync(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, System.Net.Mail.MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl = true, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.ExecuteTask(() => MessageService.SendMail(from, replyTo, to, cc, bcc, subject, body, attachment, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl, null, null), cancellationToken);
		}

		/// <summary>
		/// Send an e-mail within VIE Portal software using System.Net.Mail namespace.
		/// </summary>
		/// <param name="fromAddress">Sender address.</param>
		/// <param name="replyToAddress">Address will be replied to.</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject.</param>
		/// <param name="body">The message body.</param>
		/// <param name="attachments">Collection of attachment files (all are full path of attachments).</param>
		/// <param name="additionalFooter">The data will be added into email as footer.</param>
		/// <param name="priority">Priority. See <c>System.Net.Mail.MailPriority</c> class for more information.</param>
		/// <param name="isHtmlFormat">TRUE if the message body is HTML formated.</param>
		/// <param name="encoding">Encoding of body message. See <c>System.Web.Mail.MailEncoding</c> class for more information.</param>
		/// <param name="smtpServer">IP address or host name of SMTP server.</param>
		/// <param name="smtpServerPort">Port number for SMTP service on the SMTP server.</param>
		/// <param name="smtpUsername">Username of the SMTP server use for sending mail.</param>
		/// <param name="smtpPassword">Password of user on the SMTP server use for sending mail.</param>
		/// <param name="smtpEnableSsl">TRUE if the SMTP server requires SSL.</param>
		/// <param name="cancellationToken">Token for cancelling this task.</param>
		public static Task SendMailAsync(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.ExecuteTask(() => MessageService.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl), cancellationToken);
		}

		/// <summary>
		/// Send an e-mail within VIE Portal software using System.Net.Mail namespace.
		/// </summary>
		/// <param name="fromAddress">Sender address.</param>
		/// <param name="replyToAddress">Address will be replied to.</param>
		/// <param name="toAddresses">Collection of recipients</param>
		/// <param name="ccAddresses">Collection of CC recipients</param>
		/// <param name="bccAddresses">Collection of BCC recipients</param>
		/// <param name="subject">The message subject.</param>
		/// <param name="body">The message body.</param>
		/// <param name="attachments">Collection of attachment files (all are full path of attachments).</param>
		/// <param name="additionalFooter">The data will be added into email as footer.</param>
		/// <param name="priority">Priority. See <c>System.Net.Mail.MailPriority</c> class for more information.</param>
		/// <param name="isHtmlFormat">TRUE if the message body is HTML formated.</param>
		/// <param name="encoding">Encoding of body message. See <c>System.Web.Mail.MailEncoding</c> class for more information.</param>
		/// <param name="smtp">Informaiton of SMTP server for sending email.</param>
		/// <param name="cancellationToken">Token for cancelling this task.</param>
		public static Task SendMailAsync(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, SmtpClient smtp, CancellationToken cancellationToken = default(CancellationToken))
		{
			return UtilityService.ExecuteTask(() => MessageService.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtp), cancellationToken);
		}
		#endregion

	}

}