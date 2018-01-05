#region Related components
using System;
using System.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.Tasks;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// One-time password servicing methods
	/// </summary>
	public static class OTPService
	{
		static string UrlEncodeAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
		static string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
		static int InByteSize = 8;
		static int OutByteSize = 5;
		static int SizeOfInt32 = 4;

		/// <summary>
		/// Generates the time-based one-time password (RFC 6238)
		/// </summary>
		/// <param name="secret">The secret key to generate OTP</param>
		/// <param name="interval">The interval length (seconds) to generate OTP - Authenticator app (just like Google /Microsoft) uses 30 seconds interval length</param>
		/// <param name="digits">The number of OTP digits</param>
		/// <returns></returns>
		public static string GeneratePassword(byte[] secret, int interval = 30, int digits = 6)
		{
			return OTPService.GeneratePassword(secret, DateTime.Now.ToUnixTimestamp() / (interval > 0 ? interval : 30), digits);
		}

		/// <summary>
		/// Generates the counter-based one-time password (RFC 4226)
		/// </summary>
		/// <param name="secret">The secret key to generate OTP</param>
		/// <param name="counter">The counter to generate OTP from the secret key</param>
		/// <param name="digits">The number of OTP digits</param>
		/// <returns></returns>
		public static string GeneratePassword(byte[] secret, long counter, int digits = 6)
		{
			var bytes = BitConverter.GetBytes(counter);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			var hash = bytes.GetHMACHash(secret, "SHA1");
			bytes = new byte[OTPService.SizeOfInt32];
			Buffer.BlockCopy(hash, hash[hash.Length - 1] & 0xF, bytes, 0, OTPService.SizeOfInt32);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			digits = digits > 0 && digits < 10 ? digits : 6;
			return ((BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF) % (int)Math.Pow(10, digits)).ToString(CultureInfo.InvariantCulture).PadLeft(digits, '0');
		}

		/// <summary>
		/// Generates the QR Code image for provisioning
		/// </summary>
		/// <param name="identifier">The string that presents identity, usually is username/account or email</param>
		/// <param name="secret">The secret key</param>
		/// <param name="size">The number that presents the size of image</param>
		/// <param name="issuer">The string that presents name of issuer</param>
		/// <returns>The array of bytes that presents data of QR Code image (PNG) for provisioning</returns>
		public static Task<byte[]> GenerateProvisioningImageAsync(string identifier, byte[] secret, int size = 300, string issuer = null)
		{
			// encode the secret with Base32 string
			int pos = 0, index = 0;
			var builder = new StringBuilder((secret.Length + 7) * OTPService.InByteSize / OTPService.OutByteSize);
			while (pos < secret.Length)
			{
				var current = secret[pos];
				int digit;

				//Is the current digit going to span a byte boundary?
				if (index > (OTPService.InByteSize - OTPService.OutByteSize))
				{
					var next = (pos + 1) < secret.Length ? secret[pos + 1] : 0;
					digit = current & (0xFF >> index);
					index = (index + OTPService.OutByteSize) % OTPService.InByteSize;
					digit <<= index;
					digit |= next >> (OTPService.InByteSize - index);
					pos++;
				}
				else
				{
					digit = (current >> (OTPService.InByteSize - (index + OTPService.OutByteSize))) & 0x1F;
					index = (index + OTPService.OutByteSize) % OTPService.InByteSize;
					if (index == 0)
						pos++;
				}
				builder.Append(OTPService.Base32Alphabet[digit]);
			}
			var data = builder.ToString();

			// encode the uri to build QR Code
			var uri = $"otpauth://totp/{identifier}?secret={data}&issuer={issuer ?? "VIEApps.net"}";
			builder = new StringBuilder();
			for (index = 0; index < uri.Length; index++)
			{
				var symbol = uri[index];
				if (OTPService.UrlEncodeAlphabet.IndexOf(symbol) != -1)
					builder.Append(symbol);
				else
				{
					builder.Append('%');
					builder.Append(((int)symbol).ToString("X2"));
				}
			}
			data = builder.ToString();

			// download data of QR Code image
			return UtilityService.DownloadAsync($"https://chart.apis.google.com/chart?cht=qr&chs={size}x{size}&chl={data}");
		}
	}
}