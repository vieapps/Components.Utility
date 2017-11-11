#region Related components
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with cryptos
	/// </summary>
	public static class CryptoService
	{

		#region Conversions
		/// <summary>
		/// Converts this string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this string @string)
		{
			return UTF8Encoding.UTF8.GetBytes(@string);
		}

		/// <summary>
		/// Converts this hexa-string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] HexToBytes(this string @string)
		{
			var length = @string.Length;
			var bytes = new byte[length / 2];
			for (var index = 0; index < length; index += 2)
				bytes[index / 2] = Convert.ToByte(@string.Substring(index, 2), 16);
			return bytes;
		}

		/// <summary>
		/// Converts this Base64 string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] Base64ToBytes(this string @string)
		{
			return Convert.FromBase64String(@string);
		}

		/// <summary>
		/// Converts this array of bytes to hexa string
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToHexa(this byte[] bytes)
		{
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}

		/// <summary>
		/// Converts this string to hexa string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isBase64"></param>
		/// <returns></returns>
		public static string ToHexa(this string @string, bool isBase64 = false)
		{
			return (isBase64 ? @string.Base64ToBytes() : @string.ToBytes()).ToHexa();
		}

		/// <summary>
		/// Converts this array of bytes to Base64 string
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToBase64(this byte[] bytes)
		{
			return Convert.ToBase64String(bytes);
		}

		/// <summary>
		/// Converts this string to Base64 string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isHexa"></param>
		/// <param name="isBase64Url"></param>
		/// <returns></returns>
		public static string ToBase64(this string @string, bool isHexa = false, bool isBase64Url = false)
		{
			if (isHexa)
				return @string.HexToBytes().ToBase64();

			else if (!isBase64Url)
				return @string.ToBytes().ToBase64();

			else
			{
				var output = @string.Trim().Replace('-', '+').Replace('_', '/');
				switch (output.Length % 4)
				{
					case 0:
						break;
					case 2:
						output += "==";
						break;
					case 3:
						output += "=";
						break;
					default:
						throw new Exception("Illegal base64url string!");
				}
				return output;
			}
		}

		/// <summary>
		/// Converts this string to Base64Url string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isBase64"></param>
		/// <param name="isHexa"></param>
		/// <returns></returns>
		public static string ToBase64Url(this string @string, bool isBase64 = false, bool isHexa = false)
		{
			var output = isBase64
				? @string
				: @string.ToBase64(isHexa, false);
			output = output.Split('=')[0];
			return output.Replace('+', '-').Replace('/', '_');
		}

		/// <summary>
		/// Converts this Base64 string to plain string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isBase64Url"></param>
		/// <returns></returns>
		public static string FromBase64(this string @string, bool isBase64Url = false)
		{
			return Convert.FromBase64String(isBase64Url ? @string.ToBase64(false, true) : @string).GetString();
		}

		/// <summary>
		/// Converts this Base64Url string to plain string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string FromBase64Url(this string @string)
		{
			return @string.FromBase64(true);
		}
		#endregion

		#region Url/Html encoding/decoding
		/// <summary>
		/// Encodes this string to use in url
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string UrlEncode(this string @string)
		{
			return string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.UrlEncode(@string).Replace("+", "%20").Replace(" ", "%20");
		}

		/// <summary>
		/// Decodes this url-encoded string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string UrlDecode(this string @string)
		{
			return string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.UrlDecode(@string.Trim());
		}

		/// <summary>
		/// Encodes this string to Base64Url string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Url64Encode(this string @string)
		{
			return string.IsNullOrWhiteSpace(@string)
				? ""
				: @string.ToBase64Url();
		}

		/// <summary>
		/// Decodes this Base64Url string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Url64Decode(this string @string)
		{
			return string.IsNullOrWhiteSpace(@string)
				? ""
				: @string.ToBase64(false, true).Base64ToBytes().GetString();
		}

		/// <summary>
		/// Encodes this string to HTML string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string HtmlEncode(this string @string)
		{
			return string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.HtmlEncode(@string);
		}

		/// <summary>
		/// Decodes this HTML string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string HtmlDecode(this string @string)
		{
			return string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.HtmlDecode(@string.Trim());
		}
		#endregion

		#region Encryption key/initialize vector
		/// <summary>
		/// Gets the default key for encrypting/decrypting data
		/// </summary>
		public static string DefaultEncryptionKey
		{
			get
			{
				return "C804BE43-VIEApps-0B43-Core-442B-Components-B635-Service-FD0616D11B01";
			}
		}

		/// <summary>
		/// Generates a key from this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="reverse"></param>
		/// <param name="hash"></param>
		/// <param name="keySize"></param>
		/// <returns></returns>
		public static byte[] GenerateEncryptionKey(this string @string, bool reverse, bool hash, int keySize)
		{
			var passPhrase = reverse
				? @string.Reverse()
				: @string;

			var fullKey = hash
				? passPhrase.GetMD5Hash()
				: passPhrase.ToBytes();

			var maxIndex = 0;
			if (keySize > 7)
				maxIndex = keySize / 8;

			else
			{
				var sizeOfBytes = fullKey.Length;
				var bytes = 1;
				var bits = bytes * 8;
				while (bytes <= sizeOfBytes)
				{
					bits = bytes * 8;
					if (bytes < 2)
						bytes++;
					else
						bytes = bytes * 2;
				}
				maxIndex = bits / 8;
			}

			var keys = new byte[maxIndex];
			for (var index = 0; index < maxIndex; index++)
				keys[index] = fullKey[index];

			return keys;
		}

		/// <summary>
		/// Generates a key from this string (for using with AES)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GenerateEncryptionKey(this string @string)
		{
			return @string.GenerateEncryptionKey(true, false, 256);
		}

		/// <summary>
		/// Generates an IV (initialize vector) from this string (for using with AES)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GenerateEncryptionIV(this string @string)
		{
			return @string.GenerateEncryptionKey(false, true, 128);
		}
		#endregion

		#region Hashing
		/// <summary>
		/// Gets hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public static byte[] GetHash(this string @string, string mode = "MD5")
		{
			HashAlgorithm hasher = null;
			try
			{
				if ("MD5".IsEquals(mode))
					hasher = MD5.Create();
				else if ("SHA1".IsEquals(mode))
					hasher = SHA1.Create();
				else if ("SHA256".IsEquals(mode))
					hasher = SHA256.Create();
				else if ("SHA384".IsEquals(mode))
					hasher = SHA384.Create();
				else
					hasher = SHA512.Create();
				return hasher.ComputeHash(@string.ToBytes());
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				hasher?.Clear();
				hasher?.Dispose();
			}
		}

		/// <summary>
		/// Gets MD5 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetMD5Hash(this string @string)
		{
			return @string.GetHash("MD5");
		}

		/// <summary>
		/// Gets MD5 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toBase64"></param>
		/// <returns></returns>
		public static string GetMD5(this string @string, bool toBase64 = false)
		{
			return toBase64
				? @string.GetMD5Hash().ToBase64()
				: @string.GetMD5Hash().ToHexa();
		}

		/// <summary>
		/// Gets SHA1 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA1Hash(this string @string)
		{
			return @string.GetHash("SHA1");
		}

		/// <summary>
		/// Gets SHA1 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toBase64"></param>
		/// <returns></returns>
		public static string GetSHA1(this string @string, bool toBase64 = false)
		{
			return toBase64
				? @string.GetSHA1Hash().ToBase64()
				: @string.GetSHA1Hash().ToHexa();
		}

		/// <summary>
		/// Gets SHA256 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA256Hash(this string @string)
		{
			return @string.GetHash("SHA256");
		}

		/// <summary>
		/// Gets SHA256 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toBase64"></param>
		/// <returns></returns>
		public static string GetSHA256(this string @string, bool toBase64 = false)
		{
			return toBase64
				? @string.GetSHA256Hash().ToBase64()
				: @string.GetSHA256Hash().ToHexa();
		}

		/// <summary>
		/// Gets SHA384 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA384Hash(this string @string)
		{
			return @string.GetHash("SHA384");
		}

		/// <summary>
		/// Gets SHA384 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toBase64"></param>
		/// <returns></returns>
		public static string GetSHA384(this string @string, bool toBase64 = false)
		{
			return toBase64
				? @string.GetSHA384Hash().ToBase64()
				: @string.GetSHA384Hash().ToHexa();
		}

		/// <summary>
		/// Gets SHA512 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA512Hash(this string @string)
		{
			return @string.GetHash("SHA512");
		}

		/// <summary>
		/// Gets SHA512 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toBase64"></param>
		/// <returns></returns>
		public static string GetSHA512(this string @string, bool toBase64 = false)
		{
			return toBase64
				? @string.GetSHA512Hash().ToBase64()
				: @string.GetSHA512Hash().ToHexa();
		}

		/// <summary>
		/// Gets HMAC hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public static byte[] GetHMACHash(this string @string, byte[] key, string mode = "SHA256")
		{
			HMAC hasher = null;
			try
			{
				if ("MD5".IsEquals(mode))
					hasher = new HMACMD5(key);
				else if ("SHA1".IsEquals(mode))
					hasher = new HMACSHA1(key);
				else if ("SHA256".IsEquals(mode))
					hasher = new HMACSHA256(key);
				else if ("SHA384".IsEquals(mode))
					hasher = new HMACSHA384(key);
				else
					hasher = new HMACSHA512(key);
				return hasher.ComputeHash(@string.ToBytes());
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				hasher?.Clear();
				hasher?.Dispose();
			}
		}

		/// <summary>
		/// Gets HMAC hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		public static byte[] GetHMACHash(this string @string, string key, string mode = "SHA256")
		{
			return @string.GetHMACHash(key.ToBytes(), mode);
		}

		/// <summary>
		/// Gets HMAC hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="mode"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMAC(this string @string, string key, string mode = null, bool toHexa = true)
		{
			var hash = @string.GetHMACHash(key, mode);
			return toHexa
				? hash.ToHexa()
				: hash.ToBase64();
		}

		/// <summary>
		/// Gets HMAC MD5 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACMD5(this string @string, string key, bool toHexa = true)
		{
			return @string.GetHMAC(key, "MD5", toHexa);
		}

		/// <summary>
		/// Gets HMAC MD5 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACMD5(this string @string, bool toHexa = true)
		{
			return @string.GetHMACMD5(CryptoService.DefaultEncryptionKey, toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA1 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA1(this string @string, string key, bool toHexa = true)
		{
			return @string.GetHMAC(key, "SHA1", toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA1 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA1(this string @string, bool toHexa = true)
		{
			return @string.GetHMACSHA1(CryptoService.DefaultEncryptionKey, toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA256 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA256(this string @string, string key, bool toHexa = true)
		{
			return @string.GetHMAC(key, "SHA256", toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA256 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA256(this string @string, bool toHexa = true)
		{
			return @string.GetHMACSHA256(CryptoService.DefaultEncryptionKey, toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA384 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA384(this string @string, string key, bool toHexa = true)
		{
			return @string.GetHMAC(key, "SHA384", toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA384 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA384(this string @string, bool toHexa = true)
		{
			return @string.GetHMACSHA384(CryptoService.DefaultEncryptionKey, toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA512 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA512(this string @string, string key, bool toHexa = true)
		{
			return @string.GetHMAC(key, "SHA512", toHexa);
		}

		/// <summary>
		/// Gets HMAC SHA512 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA512(this string @string, bool toHexa = true)
		{
			return @string.GetHMACSHA512(CryptoService.DefaultEncryptionKey, toHexa);
		}
		#endregion

		#region Encrypt/Decrypt (using AES)
		/// <summary>
		/// Encrypts this string by specific key and initialize vector using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string Encrypt(this string @string, byte[] key, byte[] iv, bool toHexa = false)
		{
			if (string.IsNullOrWhiteSpace(@string))
				return "";

			using (var crypto = new AesCryptoServiceProvider())
			{
				crypto.Key = key;
				crypto.IV = iv;
				using (var encryptor = crypto.CreateEncryptor())
				{
					var encrypted = @string.ToBytes();
					encrypted = encryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
					return toHexa
						? encrypted.ToHexa()
						: encrypted.ToBase64();
				}
			}
		}

		/// <summary>
		/// Encrypts this string by specific pass-phrase using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="passPhrase"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string Encrypt(this string @string, string passPhrase = null, bool toHexa = false)
		{
			var key = (string.IsNullOrWhiteSpace(passPhrase) ? CryptoService.DefaultEncryptionKey : passPhrase).GenerateEncryptionKey();
			var iv = (string.IsNullOrWhiteSpace(passPhrase) ? CryptoService.DefaultEncryptionKey : passPhrase).GenerateEncryptionIV();
			return @string.Encrypt(key, iv, toHexa);
		}

		/// <summary>
		/// Decrypts this encrypted string by specific key and initialize vector using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <param name="isHexa"></param>
		/// <returns></returns>
		public static string Decrypt(this string @string, byte[] key, byte[] iv, bool isHexa = false)
		{
			if (string.IsNullOrWhiteSpace(@string))
				return "";

			using (var crypto = new AesCryptoServiceProvider())
			{
				crypto.Key = key;
				crypto.IV = iv;
				using (var decryptor = crypto.CreateDecryptor())
				{
					var decrypted = isHexa
						? @string.HexToBytes()
						: @string.Base64ToBytes();
					return decryptor.TransformFinalBlock(decrypted, 0, decrypted.Length).GetString();
				}
			}
		}

		/// <summary>
		/// Decrypts this encrypted string by specific pass-phrase using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="passPhrase"></param>
		/// <param name="isHexa"></param>
		/// <returns></returns>
		public static string Decrypt(this string @string, string passPhrase = null, bool isHexa = false)
		{
			var key = (string.IsNullOrWhiteSpace(passPhrase) ? CryptoService.DefaultEncryptionKey : passPhrase).GenerateEncryptionKey();
			var iv = (string.IsNullOrWhiteSpace(passPhrase) ? CryptoService.DefaultEncryptionKey : passPhrase).GenerateEncryptionIV();
			return @string.Decrypt(key, iv, isHexa);
		}
		#endregion

		#region Encrypt/Decrypt (using RSA)
		/// <summary>
		/// Encrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] RSAEncrypt(RSACryptoServiceProvider rsa, byte[] data)
		{
			return rsa.Encrypt(data, false);
		}

		/// <summary>
		/// Encrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string RSAEncrypt(RSACryptoServiceProvider rsa, string data)
		{
			return Convert.ToBase64String(CryptoService.RSAEncrypt(rsa, data.ToBytes()));
		}

		/// <summary>
		/// Encrypts the data by RSA
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] RSAEncrypt(string key, byte[] data)
		{
			using (var rsa = CryptoService.CreateRSAInstance(key))
			{
				return CryptoService.RSAEncrypt(rsa, data);
			}
		}

		/// <summary>
		/// Encrypts the data by RSA
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string RSAEncrypt(string key, string data)
		{
			return Convert.ToBase64String(CryptoService.RSAEncrypt(key, data.ToBytes()));
		}

		/// <summary>
		/// Decrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] RSADecrypt(RSACryptoServiceProvider rsa, byte[] data)
		{
			return rsa.Decrypt(data, false);
		}

		/// <summary>
		/// Decrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string RSADecrypt(RSACryptoServiceProvider rsa, string data)
		{
			return CryptoService.RSADecrypt(rsa, Convert.FromBase64String(data)).GetString();
		}

		/// <summary>
		/// Decrypts the data by RSA
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] RSADecrypt(string key, byte[] data)
		{
			using (var rsa = CryptoService.CreateRSAInstance(key))
			{
				return CryptoService.RSADecrypt(rsa, data);
			}
		}

		/// <summary>
		/// Decrypts the data by RSA
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string RSADecrypt(string key, string data)
		{
			using (var rsa = CryptoService.CreateRSAInstance(key))
			{
				return CryptoService.RSADecrypt(rsa, data);
			}
		}
		#endregion

		#region Create new instance of RSA Algorithm by a specific key
		const string PEM_PRIVATE_KEY_BEGIN = "-----BEGIN RSA PRIVATE KEY-----";
		const string PEM_PRIVATE_KEY_END = "-----END RSA PRIVATE KEY-----";
		const string PEM_PUBLIC_KEY_BEGIN = "-----BEGIN PUBLIC KEY-----";
		const string PEM_PUBLIC_KEY_END = "-----END PUBLIC KEY-----";

		/// <summary>
		/// Creates an instance of RSA Algorithm
		/// </summary>
		/// <param name="key">The specifict key for the RSA instance, might be private or public key, and must be formated in XML or PEM</param>
		/// <returns>An instance of RSA</returns>
		public static RSACryptoServiceProvider CreateRSAInstance(string key)
		{
			return CryptoService.CreateRSAInstance(key, null);
		}

		/// <summary>
		/// Creates an instance of RSA Algorithm
		/// </summary>
		/// <param name="key">Key for the RSA instance, might be private or public key, and must be formated in XML or PEM</param>
		/// <param name="name">Additional for name of key-container. Default name of key-container is 'VIEPortalNGRSAContainer'. Ex: if the additional name is 'Passport', then the name of the key-container will be 'VIEPortalNGPassportRSAContainer'.</param>
		/// <returns>An instance of RSA</returns>
		public static RSACryptoServiceProvider CreateRSAInstance(string key, string name)
		{
			// check key
			if (key == null || key.Trim().Equals("") || (!key.StartsWith("<RSAKeyValue>") && !key.StartsWith(CryptoService.PEM_PRIVATE_KEY_BEGIN) && !key.StartsWith(CryptoService.PEM_PUBLIC_KEY_BEGIN)))
				throw new InvalidDataException("Invalid key to create new instance of RSA. Key must be formated in XML or PEM.");

			// RSA instance
			RSACryptoServiceProvider rsa = null;

			// create RSACryptoServiceProvider instance and initialize with XML key
			if (key.StartsWith("<RSAKeyValue>"))
				try
				{
					rsa = CryptoService.CreateRSAInstance(key, 2048, name);
				}
				catch (Exception)
				{
					throw;
				}

			// create RSACryptoServiceProvider instance and initialize with PEM private key
			else if (key.StartsWith(CryptoService.PEM_PRIVATE_KEY_BEGIN))
			{
				// prepare key
				var stringBuilder = new StringBuilder(key.Trim());
				stringBuilder.Replace(CryptoService.PEM_PRIVATE_KEY_BEGIN, "");
				stringBuilder.Replace(CryptoService.PEM_PRIVATE_KEY_END, "");
				byte[] binKey = null;
				try
				{
					binKey = Convert.FromBase64String(stringBuilder.ToString().Trim());
				}
				catch (Exception ex)
				{
					throw new InvalidDataException("Invalid private PEM key to create new instance of RSA (not Base64 string).", ex);
				}

				// create new instance of RSA with private key
				try
				{
					rsa = CryptoService.CreateRSAInstanceWithPrivateKey(binKey);
				}
				catch (Exception)
				{
					throw;
				}
			}

			// create RSACryptoServiceProvider instance and initialize with PEM public key
			else if (key.StartsWith(CryptoService.PEM_PUBLIC_KEY_BEGIN))
			{
				// prepare key
				var stringBuilder = new StringBuilder(key.Trim());
				stringBuilder.Replace(CryptoService.PEM_PUBLIC_KEY_BEGIN, "");
				stringBuilder.Replace(CryptoService.PEM_PUBLIC_KEY_END, "");
				byte[] binKey = null;
				try
				{
					binKey = Convert.FromBase64String(stringBuilder.ToString().Trim());
				}
				catch (Exception ex)
				{
					throw new InvalidDataException("Invalid public PEM key to create new instance of RSA (not Base64 string).", ex);
				}

				// create new instance of RSA with private key
				try
				{
					rsa = CryptoService.CreateRSAInstanceWithPublicKey(binKey);
				}
				catch (Exception)
				{
					throw;
				}
			}

			// return the RSA instance
			return rsa;
		}

		/// <summary>
		/// Creates an instance of RSA Algorithm from XML key
		/// </summary>
		/// <param name="key">Key (in XML format) for the RSA instance</param>
		/// <param name="size">The size of key</param>
		/// <param name="name">Additional for name of key-container. Default name of key-container is 'VIEPortalNGRSAContainer'. Ex: if the additional name is 'Passport', then the name of the key-container will be 'VIEPortalNGPassportRSAContainer'.</param>
		/// <returns>An instance of RSA</returns>
		public static RSACryptoServiceProvider CreateRSAInstance(string key, int size, string name)
		{
			// check format of key
			if (key == null || key.Trim().Equals("") || !key.StartsWith("<RSAKeyValue>"))
				throw new InvalidDataException("Invalid key to create new instance of RSA (key must be formated in XML)");

			// preapare name of key container
			if (name == null)
				name = "";
			string containerName = string.Format("VIEPortalNG{0}RSAContainer", name);

			// create new instance of RSA
			try
			{
				// create parameters of key-store and container
				var parameters = new CspParameters(1, "Microsoft Strong Cryptographic Provider");
				parameters.Flags = CspProviderFlags.UseMachineKeyStore;
				parameters.KeyContainerName = containerName;

				// create new instance of RSA and store in the machine key-store
				var rsa = new RSACryptoServiceProvider(size, parameters);
				rsa.FromXmlString(key);

				// store in Windows key-stores (C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys)
				rsa.PersistKeyInCsp = true;

				return rsa;
			}
			catch (CryptographicException ex)
			{
				/*	Object already exists:
						- Update settings of IIS and impersonate user: https://pwnedcode.wordpress.com/2008/11/10/fixing-cryptographicexception-%E2%80%9Cobject-already-exists%E2%80%9D/
						- Allow account of processes can modify folder C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys
				 */
				if (ex.Message.Contains("Object already exists"))
					throw new CryptographicException("Cannot create new instance of RSA. Please config your machine to allow to access to container '" + containerName + "' of machine key store (FAST & DANGEROUS method: change security of folder " + @"'C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys' to allow account of processes can ascess and modify).", ex);
				else
					throw new CryptographicException("Cannot create new instance of RSA in container '" + containerName + "' of the machine key store.", ex);
			}
			catch (Exception)
			{
				throw;
			}
		}

		// from http://www.jensign.com/opensslkey/
		static RSACryptoServiceProvider CreateRSAInstanceWithPrivateKey(byte[] key)
		{
			byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

			// ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
			MemoryStream memStream = new MemoryStream(key);
			BinaryReader binReader = new BinaryReader(memStream);    //wrap Memory Stream with BinaryReader for easy reading
			byte bt = 0;
			ushort twobytes = 0;
			int elems = 0;
			try
			{
				twobytes = binReader.ReadUInt16();
				if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
					binReader.ReadByte();        //advance 1 byte
				else if (twobytes == 0x8230)
					binReader.ReadInt16();       //advance 2 bytes
				else
					return null;

				twobytes = binReader.ReadUInt16();
				if (twobytes != 0x0102) //version number
					return null;
				bt = binReader.ReadByte();
				if (bt != 0x00)
					return null;

				//------  all private key components are Integer sequences ----
				elems = GetIntegerSize(binReader);
				MODULUS = binReader.ReadBytes(elems);

				elems = GetIntegerSize(binReader);
				E = binReader.ReadBytes(elems);

				elems = GetIntegerSize(binReader);
				D = binReader.ReadBytes(elems);

				elems = GetIntegerSize(binReader);
				P = binReader.ReadBytes(elems);

				elems = GetIntegerSize(binReader);
				Q = binReader.ReadBytes(elems);

				elems = GetIntegerSize(binReader);
				DP = binReader.ReadBytes(elems);

				elems = GetIntegerSize(binReader);
				DQ = binReader.ReadBytes(elems);

				elems = GetIntegerSize(binReader);
				IQ = binReader.ReadBytes(elems);

				// ------- create RSACryptoServiceProvider instance and initialize with public key -----
				RSAParameters parameters = new RSAParameters();
				parameters.Modulus = MODULUS;
				parameters.Exponent = E;
				parameters.D = D;
				parameters.P = P;
				parameters.Q = Q;
				parameters.DP = DP;
				parameters.DQ = DQ;
				parameters.InverseQ = IQ;

				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
				rsa.PersistKeyInCsp = false;
				rsa.ImportParameters(parameters);
				return rsa;
			}
			catch (Exception)
			{
				return null;
			}
			finally
			{
				binReader.Close();
			}
		}

		static int GetIntegerSize(BinaryReader binr)
		{
			byte bt = 0;
			byte lowbyte = 0x00;
			byte highbyte = 0x00;
			int count = 0;
			bt = binr.ReadByte();
			if (bt != 0x02)     //expect integer
				return 0;
			bt = binr.ReadByte();

			if (bt == 0x81)
				count = binr.ReadByte();    // data size in next byte
			else
				if (bt == 0x82)
			{
				highbyte = binr.ReadByte(); // data size in next 2 bytes
				lowbyte = binr.ReadByte();
				byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
				count = BitConverter.ToInt32(modint, 0);
			}

			// we already have the data size
			else
				count = bt;

			//remove high order zeros in data
			while (binr.ReadByte() == 0x00)
				count -= 1;

			//last ReadByte wasn't a removed zero, so back up a byte
			binr.BaseStream.Seek(-1, SeekOrigin.Current);

			return count;
		}

		// from http://www.jensign.com/opensslkey/
		static RSACryptoServiceProvider CreateRSAInstanceWithPublicKey(byte[] key)
		{
			// encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
			byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
			byte[] seq = new byte[15];

			// ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
			MemoryStream memStream = new MemoryStream(key);
			BinaryReader binReader = new BinaryReader(memStream);    //wrap Memory Stream with BinaryReader for easy reading
			byte bt = 0;
			ushort twobytes = 0;

			try
			{
				twobytes = binReader.ReadUInt16();
				if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
					binReader.ReadByte();   //advance 1 byte
				else if (twobytes == 0x8230)
					binReader.ReadInt16();  //advance 2 bytes
				else
					return null;

				seq = binReader.ReadBytes(15);      //read the Sequence OID
				if (!CryptoService.CompareByteArrays(seq, SeqOID))  //make sure Sequence for OID is correct
					return null;

				twobytes = binReader.ReadUInt16();
				if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
					binReader.ReadByte();   //advance 1 byte
				else if (twobytes == 0x8203)
					binReader.ReadInt16();  //advance 2 bytes
				else
					return null;

				bt = binReader.ReadByte();
				if (bt != 0x00)     //expect null byte next
					return null;

				twobytes = binReader.ReadUInt16();
				if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
					binReader.ReadByte();   //advance 1 byte
				else if (twobytes == 0x8230)
					binReader.ReadInt16();  //advance 2 bytes
				else
					return null;

				twobytes = binReader.ReadUInt16();
				byte lowbyte = 0x00;
				byte highbyte = 0x00;

				if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
					lowbyte = binReader.ReadByte(); // read next bytes which is bytes in modulus
				else if (twobytes == 0x8202)
				{
					highbyte = binReader.ReadByte();    //advance 2 bytes
					lowbyte = binReader.ReadByte();
				}
				else
					return null;
				byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
				int modsize = BitConverter.ToInt32(modint, 0);

				byte firstbyte = binReader.ReadByte();
				binReader.BaseStream.Seek(-1, SeekOrigin.Current);

				//if first byte (highest order) of modulus is zero, don't include it
				if (firstbyte == 0x00)
				{
					binReader.ReadByte();   //skip this null byte
					modsize -= 1;   //reduce modulus buffer size by 1
				}

				byte[] modulus = binReader.ReadBytes(modsize);  //read the modulus bytes

				if (binReader.ReadByte() != 0x02)           //expect an Integer for the exponent data
					return null;

				int expbytes = (int)binReader.ReadByte();       // should only need one byte for actual exponent data (for all useful values)
				byte[] exponent = binReader.ReadBytes(expbytes);

				// ------- create RSACryptoServiceProvider instance and initialize with public key -----
				RSAParameters parameters = new RSAParameters();
				parameters.Modulus = modulus;
				parameters.Exponent = exponent;

				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
				rsa.PersistKeyInCsp = false;
				rsa.ImportParameters(parameters);
				return rsa;
			}
			catch (Exception)
			{
				return null;
			}
			finally
			{
				binReader.Close();
			}
		}

		static bool CompareByteArrays(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;

			int i = 0;
			foreach (byte c in a)
			{
				if (c != b[i])
					return false;
				i++;
			}
			return true;
		}
		#endregion

		#region Generate key-pairs for RSA Algorithm
		/// <summary>
		/// Generates the RSA key-pairs (with 2048 bits length).
		/// </summary>
		/// <returns>
		/// Collection of strings that presents key-pairs, indexes is:
		/// - 0: private key in XML format,
		/// - 1: private key in XML format (encrypted by default AES encryption), 
		/// - 2: public key in XML format, 
		/// - 3: public key in XML format (encrypted by default AES encryption), 
		/// - 4: private key in PEM format,
		/// - 5: private key in PEM format (encrypted by default AES encryption),
		/// - 6: public key in PEM format,
		/// - 7: public key in PEM format (encrypted by default AES encryption),
		/// - 8: exponent of public key in HEX format
		/// - 9: modulus of public key in HEX format,
		/// </returns>
		public static List<string> GenerateRSAKeyPairs()
		{
			// generate new container name for new key-pair
			var cspParams = new CspParameters(1, "Microsoft Strong Cryptographic Provider");
			cspParams.Flags = CspProviderFlags.UseArchivableKey;
			cspParams.KeyContainerName = "VIEAppsRSAContainer-" + UtilityService.GetUUID();

			// generate key pair
			using (var rsa = new RSACryptoServiceProvider(2048, cspParams))
			{
				// to not store
				rsa.PersistKeyInCsp = false;

				// create collection of keys
				var keyPairs = new List<string>();

				// add private key in XML format
				var key = rsa.ToXmlString(true);
				keyPairs.Append(new List<string>() { key, key.Encrypt() });

				// add public key in XML format
				var publicKey = rsa.ToXmlString(false);
				keyPairs.Append(new List<string>() { publicKey, publicKey.Encrypt() });

				// add private key in PEM format
				key = CryptoService.ExportPrivateKeyToPEMFormat(rsa);
				keyPairs.Append(new List<string>() { key, key.Encrypt() });

				// add public key in PEM format
				key = CryptoService.ExportPublicKeyToPEMFormat(rsa);
				keyPairs.Append(new List<string>() { key, key.Encrypt() });

				// add modulus and exponent of public key in HEX format
				var xmlDoc = new System.Xml.XmlDocument();
				xmlDoc.LoadXml(publicKey);
				keyPairs.Append(new List<string>() { xmlDoc.DocumentElement.ChildNodes[0].InnerText.ToHexa(true), xmlDoc.DocumentElement.ChildNodes[1].InnerText.ToHexa(true) });

				// return the collection of keys
				return keyPairs;
			}
		}

		// -------------------------------------------------------
		// Methods to export private key to PEM format - http://stackoverflow.com/questions/28406888/c-sharp-rsa-public-key-output-not-correct/28407693#28407693
		/// <summary>
		/// Exports the private key of RSA to PEM format
		/// </summary>
		/// <param name="rsa">Object to export</param>
		/// <returns></returns>
		public static string ExportPrivateKeyToPEMFormat(RSACryptoServiceProvider rsa)
		{
			// check
			if (rsa.PublicOnly)
				throw new ArgumentException("CSP does not contain a private key", "csp");

			var outputStream = new StringWriter();
			var parameters = rsa.ExportParameters(true);
			using (var stream = new MemoryStream())
			{
				var writer = new BinaryWriter(stream);
				writer.Write((byte)0x30); // SEQUENCE
				using (var innerStream = new MemoryStream())
				{
					var innerWriter = new BinaryWriter(innerStream);
					CryptoService.EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.D);
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.P);
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.Q);
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.DP);
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.DQ);
					CryptoService.EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
					var length = (int)innerStream.Length;
					CryptoService.EncodeLength(writer, length);
					writer.Write(innerStream.GetBuffer(), 0, length);
				}

				// begin
				outputStream.WriteLine(CryptoService.PEM_PRIVATE_KEY_BEGIN);

				// output as Base64 with lines chopped at 64 characters
				var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
				for (var index = 0; index < base64.Length; index += 64)
					outputStream.WriteLine(base64, index, Math.Min(64, base64.Length - index));

				// end
				outputStream.Write(CryptoService.PEM_PRIVATE_KEY_END);
			}

			return outputStream.ToString();
		}

		// -------------------------------------------------------
		// Methods to export public key to PEM format - http://stackoverflow.com/questions/23734792/c-sharp-export-private-public-rsa-key-from-rsacryptoserviceprovider-to-pem-strin

		/// <summary>
		/// Exports the public key of RSA to PEM format
		/// </summary>
		/// <param name="rsa">Object to export</param>
		/// <returns></returns>
		public static String ExportPublicKeyToPEMFormat(RSACryptoServiceProvider rsa)
		{
			var outputStream = new StringWriter();
			var parameters = rsa.ExportParameters(false);
			using (var stream = new MemoryStream())
			{
				var writer = new BinaryWriter(stream);
				writer.Write((byte)0x30); // SEQUENCE
				using (var innerStream = new MemoryStream())
				{
					var innerWriter = new BinaryWriter(innerStream);
					innerWriter.Write((byte)0x30); // SEQUENCE
					CryptoService.EncodeLength(innerWriter, 13);
					innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER
					var rsaEncryptionOid = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
					CryptoService.EncodeLength(innerWriter, rsaEncryptionOid.Length);
					innerWriter.Write(rsaEncryptionOid);
					innerWriter.Write((byte)0x05); // NULL
					CryptoService.EncodeLength(innerWriter, 0);
					innerWriter.Write((byte)0x03); // BIT STRING
					using (var bitStringStream = new MemoryStream())
					{
						var bitStringWriter = new BinaryWriter(bitStringStream);
						bitStringWriter.Write((byte)0x00); // # of unused bits
						bitStringWriter.Write((byte)0x30); // SEQUENCE
						using (var paramsStream = new MemoryStream())
						{
							var paramsWriter = new BinaryWriter(paramsStream);
							CryptoService.EncodeIntegerBigEndian(paramsWriter, parameters.Modulus); // Modulus
							CryptoService.EncodeIntegerBigEndian(paramsWriter, parameters.Exponent); // Exponent
							var paramsLength = (int)paramsStream.Length;
							CryptoService.EncodeLength(bitStringWriter, paramsLength);
							bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);
						}
						var bitStringLength = (int)bitStringStream.Length;
						CryptoService.EncodeLength(innerWriter, bitStringLength);
						innerWriter.Write(bitStringStream.GetBuffer(), 0, bitStringLength);
					}
					var length = (int)innerStream.Length;
					CryptoService.EncodeLength(writer, length);
					writer.Write(innerStream.GetBuffer(), 0, length);
				}

				// begin
				outputStream.WriteLine(CryptoService.PEM_PUBLIC_KEY_BEGIN);

				// output as Base64 with lines chopped at 64 characters
				var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
				for (var index = 0; index < base64.Length; index += 64)
					outputStream.WriteLine(base64, index, Math.Min(64, base64.Length - index));

				// end
				outputStream.Write(CryptoService.PEM_PUBLIC_KEY_END);
			}

			return outputStream.ToString();
		}

		static void EncodeLength(BinaryWriter stream, int length)
		{
			// check
			if (length < 0)
				throw new ArgumentOutOfRangeException("length", "Length must be non-negative");

			// short form
			if (length < 0x80)
				stream.Write((byte)length);

			// long form
			else
			{
				var temp = length;
				var bytesRequired = 0;
				while (temp > 0)
				{
					temp >>= 8;
					bytesRequired++;
				}
				stream.Write((byte)(bytesRequired | 0x80));
				for (var index = bytesRequired - 1; index >= 0; index--)
					stream.Write((byte)(length >> (8 * index) & 0xff));
			}
		}

		static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
		{
			stream.Write((byte)0x02); // INTEGER
			var prefixZeros = 0;
			for (var index = 0; index < value.Length; index++)
			{
				if (value[index] != 0) break;
				prefixZeros++;
			}
			if (value.Length - prefixZeros == 0)
			{
				CryptoService.EncodeLength(stream, 1);
				stream.Write((byte)0);
			}
			else
			{
				if (forceUnsigned && value[prefixZeros] > 0x7f)
				{
					// Add a prefix zero to force unsigned if the MSB is 1
					CryptoService.EncodeLength(stream, value.Length - prefixZeros + 1);
					stream.Write((byte)0);
				}
				else
					CryptoService.EncodeLength(stream, value.Length - prefixZeros);
				for (var index = prefixZeros; index < value.Length; index++)
					stream.Write(value[index]);
			}
		}
		#endregion

	}

}