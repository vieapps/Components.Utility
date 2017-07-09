#region Related components
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Presents a email message
	/// </summary>
	[Serializable]
	[DebuggerDisplay("Subject = {Subject}")]
	public class EmailMessage
	{

		internal static string EncryptionKey = "VIE-Apps-9D17C42D-Core-AE9F-Components-4D72-Email-586D-Encryption-277D9E606F1F-Keys";

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
		public MailPriority Priority { get; set; }
		public bool IsHtmlFormat { get; set; }
		public int Encoding { get; set; }
		public string SmtpServer { get; set; }
		public int SmtpServerPort { get; set; }
		public string SmtpUsername { get; set; }
		public string SmtpPassword { get; set; }
		public bool SmtpEnableSSL { get; set; }
		public bool SmtpStartTLS { get; set; }
		#endregion

		#region Constructors
		public EmailMessage() : this(null) { }

		public EmailMessage(string encryptedMessage)
		{
			this.Id = Utility.GetUUID();
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
			this.SmtpEnableSSL = false;
			this.SmtpStartTLS = false;

			if (!string.IsNullOrWhiteSpace(encryptedMessage))
				try
				{
					this.CopyFrom(EmailMessage.FromJson(encryptedMessage.Decrypt(EmailMessage.EncryptionKey)));
				}
				catch { }
		}
		#endregion

		#region Helper properties
		/// <summary>
		/// Gets or sets the identity of the email message.
		/// </summary>
		/// <remarks>
		/// If other message had same Id, that message will be overried by is message
		/// </remarks>
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets time to start to send this message via email.
		/// </summary>
		/// <remarks>
		/// Set a specifict time to tell mailer send this message from this time
		/// </remarks>
		public DateTime SendingTime { get; set; }

		/// <summary>
		/// Gets the encrypted message 
		/// </summary>
		[JsonIgnore]
		public string EncryptedMessage
		{
			get
			{
				if (string.IsNullOrWhiteSpace(this.Id))
					this.Id = Utility.GetUUID();
				return this.ToString().Encrypt(EmailMessage.EncryptionKey);
			}
		}
		#endregion

		#region Working with JSON
		public JObject ToJson()
		{
			return this.ToJson<EmailMessage>() as JObject;
		}

		public override string ToString()
		{
			return this.ToJson().ToString(Formatting.None);
		}

		public static EmailMessage FromJson(string json)
		{
			return JsonConvert.DeserializeObject<EmailMessage>(json);
		}
		#endregion

		#region Working with files
		/// <summary>
		/// Loads message from file and deserialize as object.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		public static EmailMessage Load(string filePath)
		{
			return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath) ? new EmailMessage(Utility.ReadTextFile(filePath)) : null;
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
					Utility.WriteTextFile(folderPath + "\\" + message.Id + ".msg", message.EncryptedMessage, false);
				}
				catch { }
		}
		#endregion

	}

	/// <summary>
	/// Presents a web-hook message
	/// </summary>
	[Serializable]
	[DebuggerDisplay("Endpoint = {EndpointURL}")]
	public class WebHookMessage
	{

		internal static string EncryptionKey = "VIE-Apps-5D659BA4-Core-23BE-Components-4E43-WebHook-81E4-Encryption-EACD7EDE222A-Keys";

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
		public NameValueCollection Headers { get; set; }

		/// <summary>
		/// Gets or Sets query-string of webhook message
		/// </summary>
		[JsonIgnore]
		public NameValueCollection QueryString { get; set; }
		#endregion

		#region Constructors
		public WebHookMessage() : this(null) { }

		public WebHookMessage(string encryptedMessage)
		{
			this.Id = Utility.GetUUID();
			this.SendingTime = DateTime.Now;
			this.EndpointURL = "";
			this.Body = "";
			this.Headers = new NameValueCollection();
			this.QueryString = new NameValueCollection();

			if (!string.IsNullOrWhiteSpace(encryptedMessage))
				try
				{
					this.CopyFrom(WebHookMessage.FromJson(encryptedMessage.Decrypt(WebHookMessage.EncryptionKey)));
				}
				catch { }
		}
		#endregion

		#region Helper properties
		/// <summary>
		/// Gets or sets identity of the message.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets time to start to send this message.
		/// </summary>
		public DateTime SendingTime { get; set; }

		/// <summary>
		/// Gets encrypted message 
		/// </summary>
		[JsonIgnore]
		public string EncryptedMessage
		{
			get
			{
				if (string.IsNullOrWhiteSpace(this.Id))
					this.Id = Utility.GetUUID();
				return this.ToString().Encrypt(WebHookMessage.EncryptionKey);
			}
		}
		#endregion

		#region Working with JSON
		public JObject ToJson()
		{
			var json = this.ToJson<WebHookMessage>() as JObject;
			json.Add(new JProperty("Headers", this.Headers.ToJObject()));
			json.Add(new JProperty("QueryString", this.QueryString.ToJObject()));
			return json;
		}

		public override string ToString()
		{
			return this.ToJson().ToString(Formatting.None);
		}

		public static WebHookMessage FromJson(string json)
		{
			var message = json.FromJson<WebHookMessage>();

			var jsonObject = JObject.Parse(json);
			if (jsonObject["Headers"] != null && jsonObject["Headers"] is JObject)
				message.Headers = (jsonObject["Headers"] as JObject).CreateNameValueCollection();
			if (jsonObject["QueryString"] != null && jsonObject["QueryString"] is JObject)
				message.QueryString = (jsonObject["QueryString"] as JObject).CreateNameValueCollection();

			return message;
		}
		#endregion

		#region Working with files
		/// <summary>
		/// Loads message from file and deserialize as object.
		/// </summary>
		/// <param name="filePath">The file path.</param>
		/// <returns></returns>
		public static WebHookMessage Load(string filePath)
		{
			return !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath) ? new WebHookMessage(Utility.ReadTextFile(filePath)) : null;
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
					Utility.WriteTextFile(folderPath + "\\" + message.Id + ".msg", message.EncryptedMessage, false);
				}
				catch { }
		}
		#endregion

	}

	// ------------------------------------------------------------------------------------------------------

	/// <summary>
	/// Collection of global methods for sending a message (email and web hook)
	/// </summary>
	public static class MessageUtility
	{

		#region Helper methods
		/// <summary>
		/// Prepares a valid email address
		/// </summary>
		/// <param name="emailInfo">The string that presents information of an email adress before validating</param>
		/// <returns><see cref="System.Net.Mail.MailAddress">MailAddress</see> object that contains valid email address</returns>
		public static MailAddress PrepareMailAddress(string emailInfo)
		{
			return MessageUtility.PrepareMailAddress(emailInfo, false);
		}

		/// <summary>
		/// Prepares a valid email address
		/// </summary>
		/// <param name="emailInfo">The string that presents information of an email adress before validating</param>
		/// <param name="convertNameToANSI">true to convert display name as ANSI</param>
		/// <returns><see cref="System.Net.Mail.MailAddress">MailAddress</see> object that contains valid email address</returns>
		public static MailAddress PrepareMailAddress(string emailInfo, bool convertNameToANSI)
		{
			if (emailInfo == null || emailInfo.Equals(""))
				return null;

			string email = "", displayName = "";

			string[] emails = emailInfo.Split('<');
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

			return email.Equals("") ? null : new MailAddress(email, displayName, Encoding.UTF8);
		}

		static string GetEmailDomain(string emailAddress)
		{
			string domain = "";
			if (emailAddress != null && !emailAddress.Equals(""))
			{
				int pos = emailAddress.IndexOf("@");
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
			MessageUtility.SendMail(from, replyTo, to, cc, bcc, subject, body, attachment, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, false, additionalFooter, preventDomains);
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
			string toEmails = "";
			if (to != null)
				toEmails = to.Trim();
			string ccEmails = "";
			if (cc != null)
				ccEmails = cc.Trim();
			string bccEmails = "";
			if (bcc != null)
				bccEmails = bcc.Trim();

			// remove all harmful domains
			if (preventDomains != null && preventDomains.Count > 0)
			{
				// variable for checing
				string[] emails = null;

				// to
				if (!toEmails.Equals(""))
				{
					emails = toEmails.Split(';');
					toEmails = "";
					foreach (string email in emails)
					{
						if (!preventDomains.Contains(MessageUtility.GetEmailDomain(email)))
							toEmails += email + ";";
					}
					if (!toEmails.Equals(""))
						toEmails = toEmails.Left(toEmails.Length - 1);
				}

				// cc
				if (!ccEmails.Equals(""))
				{
					emails = ccEmails.Split(';');
					ccEmails = "";
					foreach (string email in emails)
					{
						if (!preventDomains.Contains(MessageUtility.GetEmailDomain(email)))
							ccEmails += email + ";";
					}
					if (!ccEmails.Equals(""))
						ccEmails = ccEmails.Left(ccEmails.Length - 1);
				}

				// bcc
				if (!bccEmails.Equals(""))
				{
					emails = bccEmails.Split(';');
					bccEmails = "";
					foreach (string email in emails)
					{
						if (!preventDomains.Contains(MessageUtility.GetEmailDomain(email)))
							bccEmails += email + ";";
					}
					if (!bccEmails.Equals(""))
						bccEmails = bccEmails.Left(bccEmails.Length - 1);
				}
			}

			// check recipients
			if (toEmails.Equals("") && ccEmails.Equals("") && bccEmails.Equals(""))
				throw new InvalidDataException("No recipients for the message!");

			// get sender information
			MailAddress fromAddress = null;
			try
			{
				fromAddress = MessageUtility.PrepareMailAddress(from.ConvertUnicodeToANSI());
			}
			catch
			{
				fromAddress = MessageUtility.PrepareMailAddress("VIE Portal NG <no-reply@vieportal.net>");
			}

			// reply to
			MailAddress replyToAddress = null;
			if (replyTo != null && !replyTo.Equals(""))
				try
				{
					replyToAddress = MessageUtility.PrepareMailAddress(replyTo);
				}
				catch { }

			// recipients
			List<MailAddress> toAddresses = null;
			if (toEmails != null && !toEmails.Equals(""))
			{
				toAddresses = new List<MailAddress>();
				string[] emails = toEmails.Split(';');
				foreach (string email in emails)
				{
					MailAddress emailAddress = null;
					try
					{
						emailAddress = MessageUtility.PrepareMailAddress(email);
					}
					catch { }
					if (emailAddress != null)
						toAddresses.Add(emailAddress);
				}
			}

			List<MailAddress> ccAddresses = null;
			if (ccEmails != null && !ccEmails.Equals(""))
			{
				ccAddresses = new List<MailAddress>();
				string[] emails = ccEmails.Split(';');
				foreach (string email in emails)
				{
					MailAddress emailAddress = null;
					try
					{
						emailAddress = MessageUtility.PrepareMailAddress(email);
					}
					catch { }
					if (emailAddress != null)
						ccAddresses.Add(emailAddress);
				}
			}

			List<MailAddress> bccAddresses = null;
			if (bccEmails != null && !bccEmails.Equals(""))
			{
				bccAddresses = new List<MailAddress>();
				string[] emails = bccEmails.Split(';');
				foreach (string email in emails)
				{
					MailAddress emailAddress = null;
					try
					{
						emailAddress = MessageUtility.PrepareMailAddress(email);
					}
					catch { }
					if (emailAddress != null)
						bccAddresses.Add(emailAddress);
				}
			}

			// prepare attachments
			List<string> attachments = null;
			if (attachment != null && File.Exists(attachment))
			{
				attachments = new List<string>();
				attachments.Add(attachment);
			}

			// send mail
			MessageUtility.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl);
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
			SmtpClient smtp = new SmtpClient();

			// host name (IP of DNS of SMTP server)
			if (smtpServer != null && !smtpServer.Equals(""))
				smtp.Host = smtpServer;
			else
				smtp.Host = "127.0.0.1";        // local host SMTP

			// port
			int serverPort = 25;
			try { serverPort = Convert.ToInt32(smtpServerPort); }
			catch { serverPort = 25; }
			smtp.Port = serverPort;

			// credential (username/password)
			if (smtpUsername != null && !smtpUsername.Equals("") && smtpPassword != null && !smtpPassword.Equals(""))
				smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

			// SSL
			smtp.EnableSsl = smtpEnableSsl;

			// delivery method
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

			// send email
			MessageUtility.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtp);
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
			if (subject == null || subject.Trim().Equals("") || body == null || body.Trim().Equals(""))
				throw new InvalidDataException("The email must have subject and body");

			if (fromAddress == null || fromAddress.Address == null || fromAddress.Equals(""))
				throw new InvalidDataException("The email must have sender address");

			if ((toAddresses == null || toAddresses.Count < 1)
				&& (ccAddresses == null || ccAddresses.Count < 1)
				&& (bccAddresses == null || bccAddresses.Count < 1))
				throw new InvalidDataException("The email must have at least one recipients");

			if (smtp == null)
				throw new InvalidDataException("You must provide SMTP information for sending an email.");

			// create new message object
			MailMessage message = new MailMessage();

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
			string messageBody = body;
			if (additionalFooter != null && !additionalFooter.Equals(""))
				messageBody += additionalFooter;
			message.Body = messageBody;

			// attachment
			if (attachments != null && attachments.Count > 0)
				foreach (string attachment in attachments)
				{
					if (attachment.Trim().Equals(""))
						continue;
					if (!File.Exists(attachment))
						continue;
					message.Attachments.Add(new System.Net.Mail.Attachment(attachment));
				}

			// additional headers
			message.Headers.Add("X-Mailer", "VIE Portal NG Mailer (System)");

			// switch off certificate validation (http://stackoverflow.com/questions/777607/the-remote-certificate-is-invalid-according-to-the-validation-procedure-using)
			ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

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
		/// <param name="additionalFooter">Additional content will be added into email as footer.</param>
		/// <param name="preventDomains">Collection of harmful domains need to prevent.</param>
		public static Task SendMailAsync(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, System.Net.Mail.MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, string additionalFooter, HashSet<string> preventDomains)
		{
			return MessageUtility.SendMailAsync(from, replyTo, to, cc, bcc, subject, body, attachment, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, additionalFooter, preventDomains, CancellationToken.None);
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
		/// <param name="additionalFooter">Additional content will be added into email as footer.</param>
		/// <param name="preventDomains">Collection of harmful domains need to prevent.</param>
		/// <param name="cancellationToken">Token for cancelling this task.</param>
		public static Task SendMailAsync(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, System.Net.Mail.MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, string additionalFooter, HashSet<string> preventDomains, CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				MessageUtility.SendMail(from, replyTo, to, cc, bcc, subject, body, attachment, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, additionalFooter, preventDomains);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException(ex);
			}
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
		public static Task SendMailAsync(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, System.Net.Mail.MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl, string additionalFooter, HashSet<string> preventDomains)
		{
			return MessageUtility.SendMailAsync(from, replyTo, to, cc, bcc, subject, body, attachment, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl, additionalFooter, preventDomains, CancellationToken.None);
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
		/// <param name="cancellationToken">Token for cancelling this task.</param>
		public static Task SendMailAsync(string from, string replyTo, string to, string cc, string bcc, string subject, string body, string attachment, System.Net.Mail.MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl, string additionalFooter, HashSet<string> preventDomains, CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				MessageUtility.SendMail(from, replyTo, to, cc, bcc, subject, body, attachment, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl, additionalFooter, preventDomains);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException(ex);
			}
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
		public static Task SendMailAsync(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl)
		{
			return MessageUtility.SendMailAsync(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl, CancellationToken.None);
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
		public static Task SendMailAsync(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, string smtpServer, string smtpServerPort, string smtpUsername, string smtpPassword, bool smtpEnableSsl, CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				MessageUtility.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtpServer, smtpServerPort, smtpUsername, smtpPassword, smtpEnableSsl);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException(ex);
			}
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
		public static Task SendMailAsync(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, SmtpClient smtp)
		{
			return MessageUtility.SendMailAsync(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtp, CancellationToken.None);
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
		public static Task SendMailAsync(MailAddress fromAddress, MailAddress replyToAddress, List<MailAddress> toAddresses, List<MailAddress> ccAddresses, List<MailAddress> bccAddresses, string subject, string body, List<string> attachments, string additionalFooter, MailPriority priority, bool isHtmlFormat, Encoding encoding, SmtpClient smtp, CancellationToken cancellationToken)
		{
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				MessageUtility.SendMail(fromAddress, replyToAddress, toAddresses, ccAddresses, bccAddresses, subject, body, attachments, additionalFooter, priority, isHtmlFormat, encoding, smtp);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException(ex);
			}
		}
		#endregion

	}

}