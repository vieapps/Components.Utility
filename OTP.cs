#region Related components
using System;
using System.Globalization;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// One-time password servicing methods
	/// </summary>
	public static partial class OTPService
	{
		/// <summary>
		/// Generates the counter-based password (RFC 4226)
		/// </summary>
		/// <param name="secret">The secret key to generate password</param>
		/// <param name="counter">The counter to generate password from the secret key</param>
		/// <param name="digits">The number of password digits</param>
		/// <returns></returns>
		public static string GeneratePassword(byte[] secret, long counter, int digits = 6)
		{
			var bytes = counter.ToBytes();
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			var hash = bytes.GetHMACHash(secret, "SHA1");

			bytes = new byte[4];
			Buffer.BlockCopy(hash, hash[hash.Length - 1] & 0xF, bytes, 0, 4);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);

			digits = digits > 4 && digits < 11 ? digits : 6;
			return ((BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF) % (int)Math.Pow(10, digits)).ToString(CultureInfo.InvariantCulture).PadLeft(digits, '0');
		}

		/// <summary>
		/// Generates the counter-based password (RFC 4226)
		/// </summary>
		/// <param name="secret">The secret key (base32-encode) to generate password</param>
		/// <param name="counter">The counter to generate password from the secret key</param>
		/// <param name="digits">The number of password digits</param>
		/// <returns></returns>
		public static string GeneratePassword(string secret, long counter, int digits = 6)
			=> OTPService.GeneratePassword(secret.Base32Decode(), counter, digits);

		/// <summary>
		/// Generates the time-based password (RFC 6238)
		/// </summary>
		/// <param name="secret">The secret key to generate password</param>
		/// <param name="interval">The interval length (seconds) to generate password - Authenticator app (just like Google /Microsoft) uses 30 seconds interval length</param>
		/// <param name="digits">The number of password digits</param>
		/// <returns></returns>
		public static string GeneratePassword(byte[] secret, int interval = 30, int digits = 6)
			=> OTPService.GeneratePassword(secret, DateTime.Now.ToUnixTimestamp() / (interval > 1 ? interval : 30), digits);

		/// <summary>
		/// Generates the time-based password (RFC 6238)
		/// </summary>
		/// <param name="secret">The secret key (base32-encode) to generate password</param>
		/// <param name="interval">The interval length (seconds) to generate password - Authenticator app (just like Google /Microsoft) uses 30 seconds interval length</param>
		/// <param name="digits">The number of password digits</param>
		/// <returns></returns>
		public static string GeneratePassword(string secret, int interval = 30, int digits = 6)
			=> OTPService.GeneratePassword(secret.Base32Decode(), interval, digits);

		/// <summary>
		/// Generates the URI for provisioning
		/// </summary>
		/// <param name="identifier">The string that presents identity (username or email)</param>
		/// <param name="secret">The secret key</param>
		/// <param name="issuer">The string that presents name of issuer</param>
		/// <returns></returns>
		public static string GenerateProvisioningUri(string identifier, byte[] secret, string issuer = null)
			=> $"otpauth://totp/{identifier}?secret={secret.Base32Encode()}&issuer={(string.IsNullOrWhiteSpace(issuer) ? "VIEApps.net" : issuer)}";

		/// <summary>
		/// Generates the URI for provisioning
		/// </summary>
		/// <param name="identifier">The string that presents identity (username or email)</param>
		/// <param name="secret">The secret key</param>
		/// <param name="issuer">The string that presents name of issuer</param>
		/// <returns></returns>
		public static string GenerateProvisioningUri(string identifier, string secret, string issuer = null)
			=> OTPService.GenerateProvisioningUri(identifier, secret.ToBytes(), issuer);
	}
}