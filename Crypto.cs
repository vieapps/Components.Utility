#region Related components
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Konscious.Security.Cryptography;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with cryptos
	/// </summary>
	public static partial class CryptoService
	{
		/// <summary>
		/// The default passphrase for generating a key
		/// </summary>
		public const string DEFAULT_PASS_PHRASE = "C804BE43-VIEApps-0B43-Core-442B-Components-B635-Service-FD0616D11B01";

		/// <summary>
		/// The default key (256 bits - hash key from DEFAULT_PASS_PHRASE) for encrypting/decrypting with AES
		/// </summary>
		public static readonly byte[] DEFAULT_ENCRYPTION_KEY = DEFAULT_PASS_PHRASE.GenerateHashKey(256);

		/// <summary>
		/// The default initialization vector (128 bits - hash key from DEFAULT_PASS_PHRASE) for encrypting/decrypting with AES
		/// </summary>
		public static readonly byte[] DEFAULT_ENCRYPTION_IV = DEFAULT_PASS_PHRASE.GenerateHashKey(128);

		#region Generate keys
		/// <summary>
		/// Generates a key using RNGCryptoServiceProvider with random bytes
		/// </summary>
		/// <param name="length">The byte-length of the key (means number of total bytes :: 256 bytes = 2048 bits)</param>
		/// <returns>An array of bytes that presents the key</returns>
		public static byte[] GenerateRandomKey(int length = 256)
		{
			var key = new byte[length > 0 ? length : 256];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(key);
			}
			return key;
		}

		/// <summary>
		/// Generates a key using hash from this array of bytes
		/// </summary>
		/// <param name="bytes">The passphrase</param>
		/// <param name="length">The bit-length of the key (means number of total bits :: 256 bits = 32 bytes)</param>
		/// <returns>An array of bytes that presents the key</returns>
		public static byte[] GenerateHashKey(this byte[] bytes, int length = 256)
		{
			using (var hasher = new HMACBlake2B(length > 0 ? length : 256))
			{
				return hasher.ComputeHash(hasher.ComputeHash(bytes));
			}
		}

		/// <summary>
		/// Generates a key using hash from this passphrase
		/// </summary>
		/// <param name="passphrase">The passphrase</param>
		/// <param name="length">The bit-length of the key (means number of total bits :: 256 bits = 32 bytes)</param>
		/// <returns>An array of bytes that presents the key</returns>
		public static byte[] GenerateHashKey(this string passphrase, int length = 256) => passphrase.ToBytes().GenerateHashKey(length);
		#endregion

		#region Hash an array of bytes or a string
		static Dictionary<string, Func<HashAlgorithm>> HashFactories = new Dictionary<string, Func<HashAlgorithm>>(StringComparer.OrdinalIgnoreCase)
		{
			{ "md5", () => MD5.Create() },
			{ "sha1", () => SHA1.Create() },
			{ "sha256", () => SHA256.Create() },
			{ "sha384", () => SHA384.Create() },
			{ "sha512", () => SHA512.Create() },
			{ "ripemd", () => RIPEMD160.Create() },
			{ "ripemd160", () => RIPEMD160.Create() },
			{ "blake", () => new HMACBlake2B(256) },
			{ "blake128", () => new HMACBlake2B(128) },
			{ "blake256", () => new HMACBlake2B(256) },
			{ "blake384", () => new HMACBlake2B(384) },
			{ "blake512", () => new HMACBlake2B(512) },
		};

		/// <summary>
		/// Gets a hashser
		/// </summary>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static HashAlgorithm GetHasher(string mode = "SHA256")
		{
			if (!CryptoService.HashFactories.TryGetValue(mode, out Func<HashAlgorithm> func))
				func = () => SHA256.Create();
			return func();
		}

		/// <summary>
		/// Gets hash of this array of bytes
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static byte[] GetHash(this byte[] bytes, string mode = "SHA256")
		{
			if (bytes == null || bytes.Length < 1)
				throw new ArgumentException("Invalid", nameof(bytes));

			using (var hasher = CryptoService.GetHasher(mode))
			{
				return hasher.ComputeHash(bytes);
			}
		}

		/// <summary>
		/// Gets hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static byte[] GetHash(this string @string, string mode = "SHA256") => string.IsNullOrWhiteSpace(@string) ? new byte[0] : @string.ToBytes().GetHash(mode);

		/// <summary>
		/// Gets MD5 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetMD5Hash(this string @string) => @string.GetHash("MD5");

		/// <summary>
		/// Gets MD5 hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetMD5(this string @string, bool toHex = true) => toHex ? @string.GetMD5Hash().ToHex() : @string.GetMD5Hash().ToBase64();

		/// <summary>
		/// Gets SHA hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHAHash(this string @string) => @string.GetSHA256Hash();

		/// <summary>
		/// Gets SHA hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetSHA(this string @string, bool toHex = true) => @string.GetSHA256(toHex);

		/// <summary>
		/// Gets SHA hash of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA1Hash(this string @string) => @string.GetHash("SHA1");

		/// <summary>
		/// Gets SHA hash of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetSHA1(this string @string, bool toHex = true) => toHex ? @string.GetSHA1Hash().ToHex() : @string.GetSHA1Hash().ToBase64();

		/// <summary>
		/// Gets SHA hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA256Hash(this string @string) => @string.GetHash("SHA256");

		/// <summary>
		/// Gets SHA hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetSHA256(this string @string, bool toHex = true) => toHex ? @string.GetSHA256Hash().ToHex() : @string.GetSHA256Hash().ToBase64();

		/// <summary>
		/// Gets SHA hash of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA384Hash(this string @string) => @string.GetHash("SHA384");

		/// <summary>
		/// Gets SHA hash of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetSHA384(this string @string, bool toHex = true) => toHex ? @string.GetSHA384Hash().ToHex() : @string.GetSHA384Hash().ToBase64();

		/// <summary>
		/// Gets SHA hash of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetSHA512Hash(this string @string) => @string.GetHash("SHA512");

		/// <summary>
		/// Gets SHA hash of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetSHA512(this string @string, bool toHex = true) => toHex ? @string.GetSHA512Hash().ToHex() : @string.GetSHA512Hash().ToBase64();

		/// <summary>
		/// Gets BLAKE2 hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetBLAKEHash(this string @string) => @string.GetBLAKE256Hash();

		/// <summary>
		/// Gets BLAKE2 hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetBLAKE(this string @string, bool toHex = true) => @string.GetBLAKE256(toHex);

		/// <summary>
		/// Gets BLAKE2 hash of this string (128 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetBLAKE128Hash(this string @string) => @string.GetHash("BLAKE128");

		/// <summary>
		/// Gets BLAKE2 hash of this string (128 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetBLAKE128(this string @string, bool toHex = true) => toHex ? @string.GetBLAKE128Hash().ToHex() : @string.GetBLAKE128Hash().ToBase64();

		/// <summary>
		/// Gets BLAKE2 hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetBLAKE256Hash(this string @string) => @string.GetHash("BLAKE256");

		/// <summary>
		/// Gets BLAKE2 hash of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetBLAKE256(this string @string, bool toHex = true) => toHex ? @string.GetBLAKE256Hash().ToHex() : @string.GetBLAKE256Hash().ToBase64();

		/// <summary>
		/// Gets BLAKE2 hash of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetBLAKE384Hash(this string @string) => @string.GetHash("BLAKE384");

		/// <summary>
		/// Gets BLAKE2 hash of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetBLAKE384(this string @string, bool toHex = true) => toHex ? @string.GetBLAKE384Hash().ToHex() : @string.GetBLAKE384Hash().ToBase64();

		/// <summary>
		/// Gets BLAKE2 hash of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetBLAKE512Hash(this string @string) => @string.GetHash("BLAKE512");

		/// <summary>
		/// Gets BLAKE2 hash of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetBLAKE512(this string @string, bool toHex = true) => toHex ? @string.GetBLAKE512Hash().ToHex() : @string.GetBLAKE512Hash().ToBase64();

		/// <summary>
		/// Gets RIPEMD160 hash of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetRIPEMD160Hash(this string @string) => @string.GetHash("RIPEMD160");

		/// <summary>
		/// Gets RIPEMD160 hash of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetRIPEMD160(this string @string, bool toHex = true) => toHex ? @string.GetRIPEMD160Hash().ToHex() : @string.GetRIPEMD160Hash().ToBase64();
		#endregion

		#region Double Hash an array of bytes or a string
		/// <summary>
		/// Gets the double-hash of this array of bytes
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="firstMode">Mode of the first hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <param name="secondMode">Mode of the second hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static byte[] GetDoubleHash(this byte[] bytes, string firstMode = "SHA256", string secondMode = null)
		{
			if (bytes == null || bytes.Length < 1)
				throw new ArgumentException("Invalid", nameof(bytes));

			using (var firstHasher = CryptoService.GetHasher(firstMode))
			{
				var firstHash = firstHasher.ComputeHash(bytes);
				if (string.IsNullOrWhiteSpace(secondMode) || secondMode.IsEquals(firstMode))
					return firstHasher.ComputeHash(firstHash);
				else
					using (var secondHasher = CryptoService.GetHasher(secondMode))
					{
						return secondHasher.ComputeHash(firstHash);
					}
			}
		}

		/// <summary>
		/// Gets the double-hash of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="firstMode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <param name="secondMode">Mode of the second hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static byte[] GetDoubleHash(this string @string, string firstMode = "SHA256", string secondMode = null) => string.IsNullOrWhiteSpace(@string) ? new byte[0] : @string.ToBytes().GetDoubleHash(firstMode, secondMode);

		/// <summary>
		/// Gets the double-hash of this array of bytes with first hash is SHA256, second hash is RIPEMD160
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static byte[] GetHash160(this byte[] bytes) => bytes == null || bytes.Length < 1 ? throw new ArgumentException("Invalid", nameof(bytes)) : bytes.GetDoubleHash("SHA256", "RIPEMD160");

		/// <summary>
		/// Gets the double-hash of this string with first hash is SHA256, second hash is RIPEMD160
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] GetHash160(this string @string) => string.IsNullOrWhiteSpace(@string) ? new byte[0] : @string.ToBytes().GetHash160();
		#endregion

		#region HMAC Hash an array of bytes or string
		static byte[] GetBlakeKey(this byte[] key)
		{
			if (key.Length < 64)
				return key;

			using (var hasher = new HMACBlake2B(64))
			{
				return hasher.ComputeHash(key);
			}
		}

		static Dictionary<string, Func<byte[], HMAC>> HmacHashFactories = new Dictionary<string, Func<byte[], HMAC>>(StringComparer.OrdinalIgnoreCase)
		{
			{ "md5", (key) => new HMACMD5(key) },
			{ "sha1", (key) => new HMACSHA1(key) },
			{ "sha256", (key) => new HMACSHA256(key) },
			{ "sha384", (key) => new HMACSHA384(key) },
			{ "sha512", (key) => new HMACSHA512(key) },
			{ "ripemd", (key) => new HMACRIPEMD160(key) },
			{ "ripemd160", (key) => new HMACRIPEMD160(key) },
			{ "blake", (key) => new HMACBlake2B(key.GetBlakeKey(), 256) },
			{ "blake128", (key) => new HMACBlake2B(key.GetBlakeKey(), 128) },
			{ "blake256", (key) => new HMACBlake2B(key.GetBlakeKey(), 256) },
			{ "blake384", (key) => new HMACBlake2B(key.GetBlakeKey(), 384) },
			{ "blake512", (key) => new HMACBlake2B(key.GetBlakeKey(), 512) },
		};

		/// <summary>
		/// Gets a HMAC hashser
		/// </summary>
		/// <param name="key"></param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static HMAC GetHMACHasher(byte[] key, string mode = "SHA256")
		{
			if (!CryptoService.HmacHashFactories.TryGetValue(mode, out Func<byte[], HMAC> func))
				func = (k) => new HMACSHA256(k);
			return func(key);
		}

		/// <summary>
		/// Gets HMAC of this array of bytes
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="key">Keys for hashing (means salt)</param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static byte[] GetHMACHash(this byte[] bytes, byte[] key, string mode = "SHA256")
		{
			if (bytes == null || bytes.Length < 1)
				throw new ArgumentException("Invalid", nameof(bytes));
			else if (key == null || key.Length < 1)
				throw new ArgumentException("Invalid", nameof(key));

			using (var hasher = CryptoService.GetHMACHasher(key, mode))
			{
				return hasher.ComputeHash(bytes);
			}
		}

		/// <summary>
		/// Gets HMAC of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key">Keys for hashing (means salt)</param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static byte[] GetHMACHash(this string @string, byte[] key, string mode = "SHA256") => string.IsNullOrWhiteSpace(@string) ? new byte[0] : @string.ToBytes().GetHMACHash(key, mode);

		/// <summary>
		/// Gets HMAC of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key">Keys for hashing (means salt)</param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, blake128, blake/blake256, blake384, blake512)</param>
		/// <returns></returns>
		public static byte[] GetHMACHash(this string @string, string key, string mode = "SHA256") => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), mode);

		/// <summary>
		/// Gets HMAC of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key">Keys for hashing (means salt)</param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, blake128, blake/blake256, blake384, blake512)</param>
		/// <param name="toHex">true to get hexa-string, otherwise get base64-string</param>
		/// <returns></returns>
		public static string GetHMAC(this string @string, string key, string mode = null, bool toHex = true) => toHex ? @string.GetHMACHash(key, mode).ToHex() : @string.GetHMACHash(key, mode).ToBase64();

		/// <summary>
		/// Gets MD5 HMAC of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACMD5Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "MD5");

		/// <summary>
		/// Gets MD5 HMAC of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACMD5(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "MD5", toHex);

		/// <summary>
		/// Gets MD5 HMAC of this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACMD5(this string @string, bool toHex = true) => @string.GetHMACMD5(null, toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACSHAHash(this string @string, string key) => @string.GetHMACSHA256Hash(key);

		/// <summary>
		/// Gets SHA HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA(this string @string, string key, bool toHex = true) => @string.GetHMACSHA256(key, toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA(this string @string, bool toHexa = true) => @string.GetHMACSHA256(toHexa);

		/// <summary>
		/// Gets SHA HMAC of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACSHA1Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "SHA1");

		/// <summary>
		/// Gets SHA HMAC of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA1(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "SHA1", toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACSHA1(this string @string, bool toHexa = true) => @string.GetHMACSHA1(null, toHexa);

		/// <summary>
		/// Gets SHA HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACSHA256Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "SHA256");

		/// <summary>
		/// Gets SHA HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA256(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "SHA256", toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA256(this string @string, bool toHex = true) => @string.GetHMACSHA256(null, toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACSHA384Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "SHA384");

		/// <summary>
		/// Gets SHA HMAC of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA384(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "SHA384", toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA384(this string @string, bool toHex = true) => @string.GetHMACSHA384(null, toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACSHA512Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "SHA512");

		/// <summary>
		/// Gets SHA HMAC of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA512(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "SHA512", toHex);

		/// <summary>
		/// Gets SHA HMAC of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACSHA512(this string @string, bool toHex = true) => @string.GetHMACSHA512(null, toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACBLAKEHash(this string @string, string key) => @string.GetHMACBLAKE256Hash(key);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE(this string @string, string key, bool toHex = true) => @string.GetHMACBLAKE256(key, toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE(this string @string, bool toHex = true) => @string.GetHMACBLAKE256(toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (128 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACBLAKE128Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "BLAKE128");

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (128 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE128(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "BLAKE128", toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (128 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHexa"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE128(this string @string, bool toHexa = true) => @string.GetHMACBLAKE128(null, toHexa);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACBLAKE256Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "BLAKE256");

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE256(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "BLAKE256", toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (256 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE256(this string @string, bool toHex = true) => @string.GetHMACBLAKE256(null, toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACBLAKE384Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "BLAKE384");

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE384(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "BLAKE384", toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (384 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE384(this string @string, bool toHex = true) => @string.GetHMACBLAKE384(null, toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACBLAKE512Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "BLAKE512");

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE512(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "BLAKE512", toHex);

		/// <summary>
		/// Gets BLAKE2 HMAC of this string (512 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACBLAKE512(this string @string, bool toHex = true) => @string.GetHMACBLAKE512(null, toHex);

		/// <summary>
		/// Gets RIPEMD HMAC of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] GetHMACRIPEMD160Hash(this string @string, string key) => @string.GetHMACHash((key ?? DEFAULT_PASS_PHRASE).ToBytes(), "RIPEMD160");

		/// <summary>
		/// Gets RIPEMD HMAC of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACRIPEMD160(this string @string, string key, bool toHex = true) => @string.GetHMAC(key, "RIPEMD160", toHex);

		/// <summary>
		/// Gets RIPEMD HMAC of this string (160 bits)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string GetHMACRIPEMD160(this string @string, bool toHex = true) => @string.GetHMACRIPEMD160(null, toHex);
		#endregion

		#region Check-Sum of an array of bytes or a string
		/// <summary>
		/// Gets the check-sum of this array of bytes using double-hash
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <param name="length">Length of the check-sum</param>
		/// <returns></returns>
		public static byte[] GetCheckSum(this byte[] bytes, string mode = "SHA256", int length = 4) => bytes == null || bytes.Length < 1 ? throw new ArgumentException("Invalid", nameof(bytes)) : bytes.GetDoubleHash(mode).Take(0, length);

		/// <summary>
		/// Gets the check-sum of this string using double-hash
		/// </summary>
		/// <param name="string"></param>
		/// <param name="mode">Mode of the hasher (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512)</param>
		/// <param name="length">Length of the check-sum</param>
		/// <returns></returns>
		public static byte[] GetCheckSum(this string @string, string mode = "SHA256", int length = 4) => string.IsNullOrWhiteSpace(@string) ? new byte[0] : @string.ToBytes().GetCheckSum(mode, length);
		#endregion

		#region Encrypt/Decrypt (using AES)
		/// <summary>
		/// Encrypts by specific key and initialization vector using AES
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <returns></returns>
		public static byte[] Encrypt(this byte[] data, byte[] key = null, byte[] iv = null)
		{
			if (data == null || data.Length < 1)
				return null;

			using (var crypto = new AesCryptoServiceProvider())
			{
				using (var encryptor = crypto.CreateEncryptor(key ?? DEFAULT_ENCRYPTION_KEY, iv ?? DEFAULT_ENCRYPTION_IV))
				{
					return encryptor.TransformFinalBlock(data, 0, data.Length);
				}
			}
		}

		/// <summary>
		/// Encrypts this string by specific key and initialization vector using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string Encrypt(this string @string, byte[] key, byte[] iv, bool toHex = false) => string.IsNullOrWhiteSpace(@string) ? "" : toHex ? @string.ToBytes().Encrypt(key, iv).ToHex() : @string.ToBytes().Encrypt(key, iv).ToBase64();

		/// <summary>
		/// Encrypts this string by specific pass-phrase using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="passPhrase"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string Encrypt(this string @string, string passPhrase = null, bool toHex = false) => @string.Encrypt(passPhrase?.GenerateHashKey(256), passPhrase?.GenerateHashKey(128), toHex);

		/// <summary>
		/// Decrypts by specific key and initialization vector using AES
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <returns></returns>
		public static byte[] Decrypt(this byte[] data, byte[] key = null, byte[] iv = null)
		{
			if (data == null || data.Length < 1)
				return null;

			using (var crypto = new AesCryptoServiceProvider())
			{
				using (var decryptor = crypto.CreateDecryptor(key ?? DEFAULT_ENCRYPTION_KEY, iv ?? DEFAULT_ENCRYPTION_IV))
				{
					return decryptor.TransformFinalBlock(data, 0, data.Length);
				}
			}
		}

		/// <summary>
		/// Decrypts this encrypted string by specific key and initialization vector using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="key"></param>
		/// <param name="iv"></param>
		/// <param name="isHex"></param>
		/// <returns></returns>
		public static string Decrypt(this string @string, byte[] key, byte[] iv, bool isHex = false) => string.IsNullOrWhiteSpace(@string) ? "" : isHex ? @string.HexToBytes().Decrypt(key, iv).GetString() : @string.Base64ToBytes().Decrypt(key, iv).GetString();

		/// <summary>
		/// Decrypts this encrypted string by specific pass-phrase using AES
		/// </summary>
		/// <param name="string"></param>
		/// <param name="passPhrase"></param>
		/// <param name="isHex"></param>
		/// <returns></returns>
		public static string Decrypt(this string @string, string passPhrase = null, bool isHex = false) => @string.Decrypt(passPhrase?.GenerateHashKey(256), passPhrase?.GenerateHashKey(128), isHex);
		#endregion

		#region Encrypt/Decrypt (using RSA)
		/// <summary>
		/// Encrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Encrypt(this RSA rsa, byte[] data)
			=> data == null || data.Length < 1
				? new byte[0]
				: rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);

		/// <summary>
		/// Encrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string Encrypt(this RSA rsa, string data, bool toHex = false)
			=> string.IsNullOrWhiteSpace(data)
				? ""
				: toHex
					? rsa.Encrypt(data.ToBytes()).ToHex()
					: rsa.Encrypt(data.ToBytes()).ToBase64();

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
				return rsa.Encrypt(data);
			}
		}

		/// <summary>
		/// Encrypts the data by RSA
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="toHex"></param>
		/// <returns></returns>
		public static string RSAEncrypt(string key, string data, bool toHex = false)
		{
			using (var rsa = CryptoService.CreateRSAInstance(key))
			{
				return rsa.Encrypt(data, toHex);
			}
		}

		/// <summary>
		/// Decrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Decrypt(this RSA rsa, byte[] data)
			=> data == null || data.Length < 1
				? new byte[0]
				: rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);

		/// <summary>
		/// Decrypts the data by RSA
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="data"></param>
		/// <param name="isHex"></param>
		/// <returns></returns>
		public static string Decrypt(this RSA rsa, string data, bool isHex = false)
			=> string.IsNullOrWhiteSpace(data)
				? ""
				: isHex
					? rsa.Decrypt(data.HexToBytes()).GetString()
					: rsa.Decrypt(data.Base64ToBytes()).GetString();

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
				return rsa.Decrypt(data);
			}
		}

		/// <summary>
		/// Decrypts the data by RSA
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="isHex"></param>
		/// <returns></returns>
		public static string RSADecrypt(string key, string data, bool isHex = false)
		{
			using (var rsa = CryptoService.CreateRSAInstance(key))
			{
				return rsa.Decrypt(data, isHex);
			}
		}
		#endregion

		#region Create instance & Generate key-pair of RSA
		/// <summary>
		/// Creates an instance of RSA Algorithm
		/// </summary>
		/// <param name="key">Key for the RSA instance, must be formated in JSON or PEM</param>
		/// <returns>An instance of RSA</returns>
		public static RSA CreateRSAInstance(string key)
		{
			// check key
			if (string.IsNullOrWhiteSpace(key))
				return null;

			// create new instance
			var rsa = RSA.Create();

			// import PEM
			if (key.StartsWith(PEM_PRIVATE_KEY_BEGIN) || key.StartsWith(PEM_PUBLIC_KEY_BEGIN))
				rsa.ImportPemParameters(key);

			// import JSON
			else
				rsa.ImportJsonParameters(key);

			// return the RSA instance
			return rsa;
		}

		/// <summary>
		/// Generates the key-pairs form this RSA instance
		/// </summary>
		/// <returns>
		/// Collection of strings that presents key-pairs, indexes is:
		/// - 0: parameters (private included) in JSON format,
		/// - 1: parameters (private included) in encrypted JSON format,
		/// - 2: parameters (only public) in JSON format,
		/// - 3: parameters (only public) in encrypted JSON format,
		/// - 4: parameters (private included) in PEM format,
		/// - 5: parameters (only public) in PEM format,
		/// - 6: Exponent parameter in HEX format
		/// - 7: Modulus parameter in HEX format
		/// </returns>
		public static List<string> GenerateKeyPairs(this RSA rsa)
		{
			// create collection of keys
			var keyPairs = new List<string>();

			// JSON
			var json = rsa.ExportJsonParameters(true);
			keyPairs.Add(json);
			keyPairs.Add(json.Encrypt());

			json = rsa.ExportJsonParameters(false);
			keyPairs.Add(json);
			keyPairs.Add(json.Encrypt());

			// PEM
			keyPairs.Add(rsa.ExportPemParameters(true));
			keyPairs.Add(rsa.ExportPemParameters(false));

			// exponent & modulus
			var parameters = rsa.ExportParameters(false);
			keyPairs.Add(parameters.Modulus.ToHex());
			keyPairs.Add(parameters.Exponent.ToHex());

			// return the collection of keys
			return keyPairs;
		}

		/// <summary>
		/// Generates the key-pairs form this RSA instance
		/// </summary>
		/// <returns>
		/// Collection of strings that presents key-pairs, indexes is:
		/// - 0: parameters (private included) in JSON format,
		/// - 1: parameters (private included) in encrypted JSON format,
		/// - 2: parameters (only public) in JSON format,
		/// - 3: parameters (only public) in encrypted JSON format,
		/// - 4: parameters (private included) in PEM format,
		/// - 5: parameters (only public) in PEM format,
		/// - 6: Exponent parameter in HEX format
		/// - 7: Modulus parameter in HEX format
		/// </returns>
		public static List<string> GenerateRSAKeyPairs()
		{
			using (var rsa = RSA.Create())
			{
				return rsa.GenerateKeyPairs();
			}
		}
		#endregion

		#region Export/Import parameters of RSA with JSON format
		/// <summary>
		/// Exports the parameters of this RSA instance to string with JSON format
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="includePrivateParameters">true to export private parameters (private key)</param>
		/// <returns>The JSON string that presents exported parameters</returns>
		public static string ExportJsonParameters(this RSA rsa, bool includePrivateParameters)
		{
			var rsaParameters = rsa.ExportParameters(includePrivateParameters);
			var jsonParameters = includePrivateParameters
				? new JObject
				{
					{ "Modulus", rsaParameters.Modulus?.ToBase64() },
					{ "Exponent", rsaParameters.Exponent?.ToBase64() },
					{ "P", rsaParameters.P?.ToBase64() },
					{ "Q", rsaParameters.Q?.ToBase64() },
					{ "DP", rsaParameters.DP?.ToBase64() },
					{ "DQ", rsaParameters.DQ?.ToBase64() },
					{ "InverseQ", rsaParameters.InverseQ?.ToBase64() },
					{ "D", rsaParameters.D?.ToBase64() }
				}
				: new JObject
				{
					{ "Modulus", rsaParameters.Modulus?.ToBase64() },
					{ "Exponent", rsaParameters.Exponent?.ToBase64() },
				};
			return jsonParameters.ToString(Formatting.None);
		}

		/// <summary>
		/// Imports the  parameters of RSA from JSON s
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="jsonParameters">The JSON string that presents exported parameters</param>
		public static void ImportJsonParameters(this RSA rsa, string jsonParameters)
		{
			try
			{
				var parameters = JObject.Parse(jsonParameters).ToExpandoObject();
				rsa.ImportParameters(new RSAParameters
				{
					Modulus = parameters.Get<string>("Modulus")?.Base64ToBytes(),
					Exponent = parameters.Get<string>("Exponent")?.Base64ToBytes(),
					P = parameters.Get<string>("P")?.Base64ToBytes(),
					Q = parameters.Get<string>("Q")?.Base64ToBytes(),
					DP = parameters.Get<string>("DP")?.Base64ToBytes(),
					DQ = parameters.Get<string>("DQ")?.Base64ToBytes(),
					InverseQ = parameters.Get<string>("InverseQ")?.Base64ToBytes(),
					D = parameters.Get<string>("D")?.Base64ToBytes()
				});
			}
			catch
			{
				throw new InformationInvalidException("Invalid RSA key");
			}
		}
		#endregion

		#region Export/Import parameters of RSA with PEM format
		public const string PEM_PRIVATE_KEY_BEGIN = "-----BEGIN RSA PRIVATE KEY-----";
		public const string PEM_PRIVATE_KEY_END = "-----END RSA PRIVATE KEY-----";
		public const string PEM_PUBLIC_KEY_BEGIN = "-----BEGIN PUBLIC KEY-----";
		public const string PEM_PUBLIC_KEY_END = "-----END PUBLIC KEY-----";

		/// <summary>
		/// Exports the parameters of this RSA instance to string with PEM format
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="includePrivateParameters">true to export private parameters (private key)</param>
		/// <returns>The JSON string that presents exported parameters</returns>
		public static string ExportPemParameters(this RSA rsa, bool includePrivateParameters)
		{
			return includePrivateParameters
				? rsa.ExportPemPrivateParameters()
				: rsa.ExportPemPublicParameters();
		}

		// -------------------------------------------------------
		// Methods to export key to PEM format - http://stackoverflow.com/questions/28406888/c-sharp-rsa-public-key-output-not-correct/28407693#28407693
		/// <summary>
		/// Exports the private key of RSA to PEM format
		/// </summary>
		/// <param name="rsa">Object to export</param>
		/// <returns></returns>
		static string ExportPemPrivateParameters(this RSA rsa)
		{
			using (var results = new StringWriter())
			{
				using (var stream = new MemoryStream())
				{
					var writer = new BinaryWriter(stream);
					writer.Write((byte)0x30); // SEQUENCE
					using (var innerStream = new MemoryStream())
					{
						var parameters = rsa.ExportParameters(true);
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
						writer.Write(innerStream.ToArray(), 0, length);
					}

					// begin
					results.WriteLine(PEM_PRIVATE_KEY_BEGIN);

					// output as Base64 with lines chopped at 64 characters
					var base64 = stream.ToArraySegment().Take().ToBase64().ToCharArray();
					for (var index = 0; index < base64.Length; index += 64)
						results.WriteLine(base64, index, Math.Min(64, base64.Length - index));

					// end
					results.Write(PEM_PRIVATE_KEY_END);
				}

				return results.ToString();
			}
		}

		// -------------------------------------------------------
		// Methods to export public key to PEM format - http://stackoverflow.com/questions/23734792/c-sharp-export-private-public-rsa-key-from-rsacryptoserviceprovider-to-pem-strin

		/// <summary>
		/// Exports the public key of RSA to PEM format
		/// </summary>
		/// <param name="rsa">Object to export</param>
		/// <returns></returns>
		static string ExportPemPublicParameters(this RSA rsa)
		{
			using (var results = new StringWriter())
			{
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
								var parameters = rsa.ExportParameters(false);
								var paramsWriter = new BinaryWriter(paramsStream);
								CryptoService.EncodeIntegerBigEndian(paramsWriter, parameters.Modulus); // Modulus
								CryptoService.EncodeIntegerBigEndian(paramsWriter, parameters.Exponent); // Exponent
								var paramsLength = (int)paramsStream.Length;
								CryptoService.EncodeLength(bitStringWriter, paramsLength);
								bitStringWriter.Write(paramsStream.ToArray(), 0, paramsLength);
							}
							var bitStringLength = (int)bitStringStream.Length;
							CryptoService.EncodeLength(innerWriter, bitStringLength);
							innerWriter.Write(bitStringStream.ToArray(), 0, bitStringLength);
						}

						var length = (int)innerStream.Length;
						CryptoService.EncodeLength(writer, length);
						writer.Write(innerStream.ToArray(), 0, length);
					}

					// begin
					results.WriteLine(PEM_PUBLIC_KEY_BEGIN);

					// output as Base64 with lines chopped at 64 characters
					var base64 = stream.ToArraySegment().Take().ToBase64().ToCharArray();
					for (var index = 0; index < base64.Length; index += 64)
						results.WriteLine(base64, index, Math.Min(64, base64.Length - index));

					// end
					results.Write(PEM_PUBLIC_KEY_END);
				}

				return results.ToString();
			}
		}

		/// <summary>
		/// Imports the  parameters of RSA from JSON s
		/// </summary>
		/// <param name="rsa"></param>
		/// <param name="pemParameters">The PEM string that presents exported parameters</param>
		public static void ImportPemParameters(this RSA rsa, string pemParameters)
		{
			if (pemParameters.StartsWith(PEM_PRIVATE_KEY_BEGIN))
				rsa.ImportPemPrivateParameters(pemParameters);
			else
				rsa.ImportPemPublicParameters(pemParameters);
		}

		// from http://www.jensign.com/opensslkey/
		static void ImportPemPrivateParameters(this RSA rsa, string pemParameters)
		{
			// prepare key
			var stringBuilder = new StringBuilder(pemParameters.Trim());
			stringBuilder.Replace(PEM_PRIVATE_KEY_BEGIN, "");
			stringBuilder.Replace(PEM_PRIVATE_KEY_END, "");
			byte[] key = null;
			try
			{
				key = Convert.FromBase64String(stringBuilder.ToString().Trim());
			}
			catch (Exception ex)
			{
				throw new InvalidDataException("Invalid PEM key to import (not Base64 string)", ex);
			}

			// set up stream to decode the asn.1 encoded RSA key
			using (var stream = new MemoryStream(key))
			{
				// wrap Memory Stream with BinaryReader for easy reading
				using (var reader = new BinaryReader(stream))
				{
					byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

					byte @byte = 0;
					ushort twoBytes = 0;
					int elems = 0;
					try
					{
						twoBytes = reader.ReadUInt16();
						if (twoBytes == 0x8130)                      // data read as little endian order (actual data order for Sequence is 30 81)
							reader.ReadByte();                           // advance 1 byte
						else if (twoBytes == 0x8230)
							reader.ReadInt16();                          // advance 2 bytes
						else
							return;

						twoBytes = reader.ReadUInt16();
						if (twoBytes != 0x0102)                           // version number
							return;
						@byte = reader.ReadByte();
						if (@byte != 0x00)
							return;

						// all key components are Integer sequences
						elems = CryptoService.GetIntegerSize(reader);
						MODULUS = reader.ReadBytes(elems);

						elems = CryptoService.GetIntegerSize(reader);
						E = reader.ReadBytes(elems);

						elems = CryptoService.GetIntegerSize(reader);
						D = reader.ReadBytes(elems);

						elems = CryptoService.GetIntegerSize(reader);
						P = reader.ReadBytes(elems);

						elems = CryptoService.GetIntegerSize(reader);
						Q = reader.ReadBytes(elems);

						elems = CryptoService.GetIntegerSize(reader);
						DP = reader.ReadBytes(elems);

						elems = CryptoService.GetIntegerSize(reader);
						DQ = reader.ReadBytes(elems);

						elems = CryptoService.GetIntegerSize(reader);
						IQ = reader.ReadBytes(elems);

						// import parameters
						rsa.ImportParameters(new RSAParameters()
						{
							Modulus = MODULUS,
							Exponent = E,
							D = D,
							P = P,
							Q = Q,
							DP = DP,
							DQ = DQ,
							InverseQ = IQ
						});
					}
					catch (Exception)
					{
						throw;
					}
				}
			}
		}

		// from http://www.jensign.com/opensslkey/
		static void ImportPemPublicParameters(this RSA rsa, string pemParameters)
		{
			// prepare key
			var stringBuilder = new StringBuilder(pemParameters.Trim());
			stringBuilder.Replace(PEM_PUBLIC_KEY_BEGIN, "");
			stringBuilder.Replace(PEM_PUBLIC_KEY_END, "");
			byte[] key = null;
			try
			{
				key = Convert.FromBase64String(stringBuilder.ToString().Trim());
			}
			catch (Exception ex)
			{
				throw new InvalidDataException("Invalid PEM key to import (not Base64 string)", ex);
			}

			// set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob
			using (var stream = new MemoryStream(key))
			{
				// wrap Memory Stream with BinaryReader for easy reading
				using (var reader = new BinaryReader(stream))
				{
					// encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
					byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
					byte[] seq = new byte[15];

					byte @byte = 0;
					ushort twoBytes = 0;

					try
					{
						twoBytes = reader.ReadUInt16();
						if (twoBytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
							reader.ReadByte();   //advance 1 byte
						else if (twoBytes == 0x8230)
							reader.ReadInt16();  //advance 2 bytes
						else
							return;

						seq = reader.ReadBytes(15);      //read the Sequence OID
						if (!seq.SequenceEqual(SeqOID))  //make sure Sequence for OID is correct
							return;

						twoBytes = reader.ReadUInt16();
						if (twoBytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
							reader.ReadByte();   //advance 1 byte
						else if (twoBytes == 0x8203)
							reader.ReadInt16();  //advance 2 bytes
						else
							return;

						@byte = reader.ReadByte();
						if (@byte != 0x00)     //expect null byte next
							return;

						twoBytes = reader.ReadUInt16();
						if (twoBytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
							reader.ReadByte();   //advance 1 byte
						else if (twoBytes == 0x8230)
							reader.ReadInt16();  //advance 2 bytes
						else
							return;

						twoBytes = reader.ReadUInt16();
						byte lowbyte = 0x00;
						byte highbyte = 0x00;

						if (twoBytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
							lowbyte = reader.ReadByte(); // read next bytes which is bytes in modulus
						else if (twoBytes == 0x8202)
						{
							highbyte = reader.ReadByte();    //advance 2 bytes
							lowbyte = reader.ReadByte();
						}
						else
							return;

						byte[] modInt = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
						var modSize = BitConverter.ToInt32(modInt, 0);

						var firstByte = reader.ReadByte();
						reader.BaseStream.Seek(-1, SeekOrigin.Current);

						//if first byte (highest order) of modulus is zero, don't include it
						if (firstByte == 0x00)
						{
							reader.ReadByte();   //skip this null byte
							modSize -= 1;   //reduce modulus buffer size by 1
						}

						var modulus = reader.ReadBytes(modSize);  //read the modulus bytes
						if (reader.ReadByte() != 0x02)           //expect an Integer for the exponent data
							return;

						var expbytes = (int)reader.ReadByte();       // should only need one byte for actual exponent data (for all useful values)
						var exponent = reader.ReadBytes(expbytes);

						// import
						rsa.ImportParameters(new RSAParameters()
						{
							Modulus = modulus,
							Exponent = exponent
						});
					}
					catch (Exception)
					{
						throw;
					}
				}
			}
		}

		static void EncodeLength(BinaryWriter writer, int length)
		{
			// check
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative");

			// short form
			if (length < 0x80)
				writer.Write((byte)length);

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
				writer.Write((byte)(bytesRequired | 0x80));
				for (var index = bytesRequired - 1; index >= 0; index--)
					writer.Write((byte)(length >> (8 * index) & 0xff));
			}
		}

		static void EncodeIntegerBigEndian(BinaryWriter writer, byte[] value, bool forceUnsigned = true)
		{
			writer.Write((byte)0x02); // INTEGER
			var prefixZeros = 0;
			for (var index = 0; index < value.Length; index++)
			{
				if (value[index] != 0) break;
				prefixZeros++;
			}
			if (value.Length - prefixZeros == 0)
			{
				CryptoService.EncodeLength(writer, 1);
				writer.Write((byte)0);
			}
			else
			{
				if (forceUnsigned && value[prefixZeros] > 0x7f)
				{
					// Add a prefix zero to force unsigned if the MSB is 1
					CryptoService.EncodeLength(writer, value.Length - prefixZeros + 1);
					writer.Write((byte)0);
				}
				else
					CryptoService.EncodeLength(writer, value.Length - prefixZeros);

				for (var index = prefixZeros; index < value.Length; index++)
					writer.Write(value[index]);
			}
		}

		static int GetIntegerSize(BinaryReader reader)
		{
			byte @byte = 0;
			byte lowByte = 0x00;
			byte highByte = 0x00;
			int count = 0;
			@byte = reader.ReadByte();
			if (@byte != 0x02)     //expect integer
				return 0;
			@byte = reader.ReadByte();

			if (@byte == 0x81)
				count = reader.ReadByte();    // data size in next byte

			else if (@byte == 0x82)
			{
				highByte = reader.ReadByte(); // data size in next 2 bytes
				lowByte = reader.ReadByte();
				byte[] modint = { lowByte, highByte, 0x00, 0x00 };
				count = BitConverter.ToInt32(modint, 0);
			}

			// we already have the data size
			else
				count = @byte;

			//remove high order zeros in data
			while (reader.ReadByte() == 0x00)
				count -= 1;

			//last ReadByte wasn't a removed zero, so back up a byte
			reader.BaseStream.Seek(-1, SeekOrigin.Current);

			return count;
		}
		#endregion

		#region Encrypt/Decrypt (using ECC)
		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data to encrypt</param>
		/// <param name="publicKey">The public key used to encrypt data</param>
		/// <returns></returns>
		public static byte[] ECCEncrypt(this byte[] data, ECCsecp256k1.Point publicKey) => ECCsecp256k1.Encrypt(publicKey, data);

		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data to encrypt</param>
		/// <param name="publicKey">The public key used to encrypt data</param>
		/// <returns></returns>
		public static byte[] ECCEncrypt(this byte[] data, byte[] publicKey) => ECCsecp256k1.Encrypt(publicKey, data);

		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data to encrypt</param>
		/// <param name="publicKey">The public key used to encrypt data</param>
		/// <returns></returns>
		public static byte[] ECCEncrypt(this byte[] data, string publicKey) => ECCsecp256k1.Encrypt(publicKey, data);

		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data to encrypt</param>
		/// <param name="publicKey">The public key used to encrypt data</param>
		/// <param name="toHex">true to get hexa string, false to get base64 string</param>
		/// <returns></returns>
		public static string ECCEncrypt(this string data, ECCsecp256k1.Point publicKey, bool toHex = false) => toHex ? data.ToBytes().ECCEncrypt(publicKey).ToHex() : data.ToBytes().ECCEncrypt(publicKey).ToBase64();

		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data to encrypt</param>
		/// <param name="publicKey">The public key used to encrypt data</param>
		/// <param name="toHex">true to get hexa string, false to get base64 string</param>
		/// <returns></returns>
		public static string ECCEncrypt(this string data, byte[] publicKey, bool toHex = false) => toHex ? data.ToBytes().ECCEncrypt(publicKey).ToHex() : data.ToBytes().ECCEncrypt(publicKey).ToBase64();

		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data to encrypt</param>
		/// <param name="publicKey">The public key used to encrypt data</param>
		/// <param name="toHex">true to get hexa string, false to get base64 string</param>
		/// <returns></returns>
		public static string ECCEncrypt(this string data, string publicKey, bool toHex = false) => toHex ? data.ToBytes().ECCEncrypt(publicKey).ToHex() : data.ToBytes().ECCEncrypt(publicKey).ToBase64();

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data the decrypt</param>
		/// <param name="privateKey">The private key used to decrypt</param>
		/// <returns></returns>
		public static byte[] ECCDecrypt(this byte[] data, BigInteger privateKey) => ECCsecp256k1.Decrypt(privateKey, data);

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data the decrypt</param>
		/// <param name="privateKey">The private key used to decrypt</param>
		/// <returns></returns>
		public static byte[] ECCDecrypt(this byte[] data, byte[] privateKey) => ECCsecp256k1.Decrypt(privateKey, data);

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data the decrypt</param>
		/// <param name="privateKey">The private key used to decrypt</param>
		/// <returns></returns>
		public static byte[] ECCDecrypt(this byte[] data, string privateKey) => ECCsecp256k1.Decrypt(privateKey, data);

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data the decrypt</param>
		/// <param name="privateKey">The private key used to decrypt</param>
		/// <param name="isHex">true to specified that the data is hexa string, false is base64 string</param>
		/// <returns></returns>
		public static string ECCDecrypt(this string data, BigInteger privateKey, bool isHex = false) => isHex ? data.HexToBytes().ECCDecrypt(privateKey).GetString() : data.Base64ToBytes().ECCDecrypt(privateKey).GetString();

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data the decrypt</param>
		/// <param name="privateKey">The private key used to decrypt</param>
		/// <param name="isHex">true to specified that the data is hexa string, false is base64 string</param>
		/// <returns></returns>
		public static string ECCDecrypt(this string data, byte[] privateKey, bool isHex = false) => isHex ? data.HexToBytes().ECCDecrypt(privateKey).GetString() : data.Base64ToBytes().ECCDecrypt(privateKey).GetString();

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="data">The data the decrypt</param>
		/// <param name="privateKey">The private key used to decrypt</param>
		/// <param name="isHex">true to specified that the data is hexa string, false is base64 string</param>
		/// <returns></returns>
		public static string ECCDecrypt(this string data, string privateKey, bool isHex = false) => isHex ? data.HexToBytes().ECCDecrypt(privateKey).GetString() : data.Base64ToBytes().ECCDecrypt(privateKey).GetString();
		#endregion

		#region Sign/Verify (using ECC)
		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="hash">The hashed-data to sign</param>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <returns></returns>
		public static BigInteger[] ECCSign(this byte[] hash, BigInteger privateKey) => ECCsecp256k1.Sign(privateKey, hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="hash">The hashed-data to sign</param>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <returns></returns>
		public static BigInteger[] ECCSign(this byte[] hash, byte[] privateKey) => ECCsecp256k1.Sign(privateKey, hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="hash">The hashed-data to sign</param>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <returns></returns>
		public static BigInteger[] ECCSign(this byte[] hash, string privateKey) => ECCsecp256k1.Sign(privateKey, hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="hash">The hashed-data to sign</param>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <returns></returns>
		public static string ECCSignAsHex(this byte[] hash, BigInteger privateKey) => ECCsecp256k1.SignAsHex(privateKey, hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="hash">The hashed-data to sign</param>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <returns></returns>
		public static string ECCSignAsHex(this byte[] hash, byte[] privateKey) => ECCsecp256k1.SignAsHex(privateKey, hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="hash">The hashed-data to sign</param>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <returns></returns>
		public static string ECCSignAsHex(this byte[] hash, string privateKey) => ECCsecp256k1.SignAsHex(privateKey.HexToBytes(), hash);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="signature">The signature to verify</param>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <returns></returns>
		public static bool ECCVerify(this BigInteger[] signature, ECCsecp256k1.Point publicKey, byte[] hash) => ECCsecp256k1.Verify(publicKey, hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="signature">The signature to verify</param>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <returns></returns>
		public static bool ECCVerify(this BigInteger[] signature, byte[] publicKey, byte[] hash) => ECCsecp256k1.Verify(publicKey, hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="signature">The signature to verify</param>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <returns></returns>
		public static bool ECCVerify(this string signature, byte[] publicKey, byte[] hash) => ECCsecp256k1.Verify(publicKey, hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="signature">The signature to verify</param>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <returns></returns>
		public static bool ECCVerify(this string signature, string publicKey, byte[] hash) => ECCsecp256k1.Verify(publicKey, hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="signature">The signature to verify</param>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <returns></returns>
		public static bool ECCVerify(this string signature, string publicKey, string hash) => ECCsecp256k1.Verify(publicKey, hash.HexToBytes(), signature);
		#endregion

		#region Generate keys of ECC
		/// <summary>
		/// Generates key-pair of Elliptic Curve Cryptography that follow secp256k1 specs (Bitcoin)
		/// </summary>
		/// <param name="length">The byte-length of the key (means number of total bytes :: 256 bytes = 2048 bits)</param>
		/// <returns></returns>
		public static Tuple<BigInteger, ECCsecp256k1.Point> GenerateECCKeyPair(int length = 256)
		{
			var privateKey = ECCsecp256k1.GeneratePrivateKey(length);
			var publicKey = ECCsecp256k1.GeneratePublicKey(privateKey);
			return new Tuple<BigInteger, ECCsecp256k1.Point>(privateKey, publicKey);
		}

		/// <summary>
		/// Generates a random private key of Elliptic Curve Cryptography that follow secp256k1 specs (Bitcoin)
		/// </summary>
		/// <param name="length">The byte-length of the key (means number of total bytes :: 256 bytes = 2048 bits)</param>
		/// <returns></returns>
		public static BigInteger GenerateECCPrivateKey(int length = 256) => ECCsecp256k1.GeneratePrivateKey(length);

		/// <summary>
		/// Generates the public key of Elliptic Curve Cryptography that follow secp256k1 specs (Bitcoin)
		/// </summary>
		/// <param name="privateKey"></param>
		/// <returns></returns>
		public static ECCsecp256k1.Point GenerateECCPublicKey(this BigInteger privateKey) => ECCsecp256k1.GeneratePublicKey(privateKey);

		/// <summary>
		/// Generates the public key of Elliptic Curve Cryptography that follow secp256k1 specs (Bitcoin)
		/// </summary>
		/// <param name="privateKey"></param>
		/// <returns></returns>
		public static byte[] GenerateECCPublicKey(this byte[] privateKey) => ECCsecp256k1.GeneratePublicKey(privateKey);

		/// <summary>
		/// Generates the public key of Elliptic Curve Cryptography that follow secp256k1 specs (Bitcoin)
		/// </summary>
		/// <param name="privateKey"></param>
		/// <returns></returns>
		public static string GenerateECCPublicKey(this string privateKey) => ECCsecp256k1.GeneratePublicKey(privateKey).ToHex();
		#endregion

	}

	// -------------------------------------------------------------------------------
	// Replacement of System.Security.Cryptography.RIPEMD160 => [SshNet.Security.Cryptography - https://github.com/sshnet/Cryptography]

	#region RIPEMD160 Base
	internal interface IHashProvider : IDisposable
	{
		/// <summary>
		/// Gets the size, in bits, of the computed hash code.
		/// </summary>
		/// <returns>
		/// The size, in bits, of the computed hash code.
		/// </returns>
		int HashSize { get; }

		/// <summary>
		/// Gets the input block size.
		/// </summary>
		/// <returns>
		/// The input block size.
		/// </returns>
		int InputBlockSize { get; }

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		/// <returns>
		/// The output block size.
		/// </returns>
		int OutputBlockSize { get; }

		/// <summary>
		/// Gets the value of the computed hash code.
		/// </summary>
		/// <value>
		/// The current value of the computed hash code.
		/// </value>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		byte[] Hash { get; }

		/// <summary>
		/// Resets an implementation of the <see cref="IHashProvider"/> to its initial state.
		/// </summary>
		void Reset();

		/// <summary>
		/// Routes data written to the object into the hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The input to compute the hash code for.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
		void HashCore(byte[] array, int ibStart, int cbSize);

		/// <summary>
		/// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
		/// </summary>
		/// <returns>
		/// The computed hash code.
		/// </returns>
		byte[] HashFinal();

		/// <summary>
		/// Computes the hash value for the specified region of the input byte array and copies the specified
		/// region of the input byte array to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input to compute the hash code for.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">A copy of the part of the input array used to compute the hash code.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>
		/// The number of bytes written.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <para><paramref name="inputCount"/> uses an invalid value.</para>
		/// <para>-or-</para>
		/// <para><paramref name="inputBuffer"/> has an invalid length.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="inputOffset"/> is out of range. This parameter requires a non-negative number.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);

		/// <summary>
		/// Computes the hash value for the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input to compute the hash code for.</param>
		/// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
		/// <returns>
		/// An array that is a copy of the part of the input that is hashed.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <para><paramref name="inputCount"/> uses an invalid value.</para>
		/// <para>-or-</para>
		/// <para><paramref name="inputBuffer"/> has an invalid length.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="inputOffset"/> is out of range. This parameter requires a non-negative number.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);

		/// <summary>
		/// Computes the hash value for the input data.
		/// </summary>
		/// <param name="buffer">The input to compute the hash code for.</param>
		/// <returns>
		/// The computed hash code.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		byte[] ComputeHash(byte[] buffer);
	}

	internal abstract class HashProviderBase : IHashProvider
	{
		bool _disposed;
		byte[] _hashValue;

		/// <summary>
		/// Gets the value of the computed hash code.
		/// </summary>
		/// <value>
		/// The current value of the computed hash code.
		/// </value>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public byte[] Hash
		{
			get
			{
				if (this._disposed)
					throw new ObjectDisposedException(GetType().FullName);
				return (byte[])this._hashValue.Clone();
			}
		}

		/// <summary>
		/// Computes the hash value for the specified region of the input byte array and copies the specified
		/// region of the input byte array to the specified region of the output byte array.
		/// </summary>
		/// <param name="inputBuffer">The input to compute the hash code for.</param>
		/// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
		/// <param name="outputBuffer">A copy of the part of the input array used to compute the hash code.</param>
		/// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
		/// <returns>
		/// The number of bytes written.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <para><paramref name="inputCount"/> uses an invalid value.</para>
		/// <para>-or-</para>
		/// <para><paramref name="inputBuffer"/> has an invalid length.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="inputOffset"/> is out of range. This parameter requires a non-negative number.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (this._disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (inputBuffer == null)
				throw new ArgumentNullException("inputBuffer");
			if (inputOffset < 0)
				throw new ArgumentOutOfRangeException("inputOffset");
			if (inputCount < 0 || (inputCount > inputBuffer.Length))
				throw new ArgumentException("XX");
			if ((inputBuffer.Length - inputCount) < inputOffset)
				throw new ArgumentException("xx");

			this.HashCore(inputBuffer, inputOffset, inputCount);

			// todo: optimize this by taking into account that inputBuffer and outputBuffer can be the same
			Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
			return inputCount;
		}

		/// <summary>
		/// Computes the hash value for the specified region of the specified byte array.
		/// </summary>
		/// <param name="inputBuffer">The input to compute the hash code for.</param>
		/// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
		/// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
		/// <returns>
		/// An array that is a copy of the part of the input that is hashed.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <para><paramref name="inputCount"/> uses an invalid value.</para>
		/// <para>-or-</para>
		/// <para><paramref name="inputBuffer"/> has an invalid length.</para>
		/// </exception>
		/// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="inputOffset"/> is out of range. This parameter requires a non-negative number.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (this._disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (inputBuffer == null)
				throw new ArgumentNullException("inputBuffer");
			if (inputOffset < 0)
				throw new ArgumentOutOfRangeException("inputOffset");
			if (inputCount < 0 || (inputCount > inputBuffer.Length))
				throw new ArgumentException("XX");
			if ((inputBuffer.Length - inputCount) < inputOffset)
				throw new ArgumentException("xx");

			this.HashCore(inputBuffer, inputOffset, inputCount);
			this._hashValue = this.HashFinal();

			// from the MSDN docs:
			// the return value of this method is not the hash value, but only a copy of the hashed part of the input data
			var outputBytes = new byte[inputCount];
			Buffer.BlockCopy(inputBuffer, inputOffset, outputBytes, 0, inputCount);
			return outputBytes;
		}

		/// <summary>
		/// Computes the hash value for the input data.
		/// </summary>
		/// <param name="buffer">The input to compute the hash code for.</param>
		/// <returns>
		/// The computed hash code.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
		/// <exception cref="ObjectDisposedException">The object has already been disposed.</exception>
		public byte[] ComputeHash(byte[] buffer)
		{
			if (this._disposed)
				throw new ObjectDisposedException(GetType().FullName);
			if (buffer == null)
				throw new ArgumentNullException("buffer");

			this.HashCore(buffer, 0, buffer.Length);
			this._hashValue = HashFinal();
			this.Reset();
			return this.Hash;
		}

		/// <summary>
		/// Releases all resources used by the current instance of the <see cref="HashProviderBase"/> class.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			this.Dispose(true);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="HashProviderBase"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				this._hashValue = null;
			this._disposed = true;
		}

		/// <summary>
		/// Gets the size, in bits, of the computed hash code.
		/// </summary>
		/// <returns>
		/// The size, in bits, of the computed hash code.
		/// </returns>
		public abstract int HashSize { get; }

		/// <summary>
		/// Gets the input block size.
		/// </summary>
		/// <returns>
		/// The input block size.
		/// </returns>
		public abstract int InputBlockSize { get; }

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		/// <returns>
		/// The output block size.
		/// </returns>
		public abstract int OutputBlockSize { get; }

		/// <summary>
		/// Resets an implementation of <see cref="HashProviderBase"/> to its initial state.
		/// </summary>
		public abstract void Reset();

		/// <summary>
		/// Routes data written to the object into the hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The input to compute the hash code for.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
		public abstract void HashCore(byte[] array, int ibStart, int cbSize);

		/// <summary>
		/// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
		/// </summary>
		/// <returns>
		/// The computed hash code.
		/// </returns>
		public abstract byte[] HashFinal();
	}

	internal class RIPEMD160HashProvider : HashProviderBase
	{
		const int DigestSize = 20;

		readonly byte[] _buffer;
		int _bufferOffset;
		long _byteCount;
		int _offset;
		int H0, H1, H2, H3, H4; // IV's

		/// <summary>
		/// The word buffer.
		/// </summary>
		readonly int[] X;

		/// <summary>
		/// Initializes a new instance of the <see cref="RIPEMD160HashProvider" /> class.
		/// </summary>
		public RIPEMD160HashProvider()
		{
			this._buffer = new byte[4];
			this.X = new int[16];
			this.InitializeHashValue();
		}

		/// <summary>
		/// Gets the size, in bits, of the computed hash code.
		/// </summary>
		/// <returns>
		/// The size, in bits, of the computed hash code.
		/// </returns>
		public override int HashSize
		{
			get
			{
				return RIPEMD160HashProvider.DigestSize * 8;
			}
		}

		/// <summary>
		/// Gets the input block size.
		/// </summary>
		/// <returns>
		/// The input block size.
		/// </returns>
		public override int InputBlockSize
		{
			get
			{
				return 64;
			}
		}

		/// <summary>
		/// Gets the output block size.
		/// </summary>
		/// <returns>
		/// The output block size.
		/// </returns>
		public override int OutputBlockSize
		{
			get
			{
				return 64;
			}
		}

		/// <summary>
		/// Routes data written to the object into the hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The input to compute the hash code for.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
		public override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			//
			// fill the current word
			//
			while ((this._bufferOffset != 0) && (cbSize > 0))
			{
				this.Update(array[ibStart]);
				ibStart++;
				cbSize--;
			}

			//
			// process whole words.
			//
			while (cbSize > this._buffer.Length)
			{
				this.ProcessWord(array, ibStart);

				ibStart += this._buffer.Length;
				cbSize -= this._buffer.Length;
				this._byteCount += this._buffer.Length;
			}

			//
			// load in the remainder.
			//
			while (cbSize > 0)
			{
				this.Update(array[ibStart]);

				ibStart++;
				cbSize--;
			}
		}

		/// <summary>
		/// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
		/// </summary>
		/// <returns>
		/// The computed hash code.
		/// </returns>
		public override byte[] HashFinal()
		{
			var output = new byte[RIPEMD160HashProvider.DigestSize];
			var bitLength = (this._byteCount << 3);

			//
			// add the pad bytes.
			//
			this.Update(128);

			while (this._bufferOffset != 0)
				this.Update(0);
			this.ProcessLength(bitLength);
			this.ProcessBlock();

			this.UnpackWord(H0, output, 0);
			this.UnpackWord(H1, output, 4);
			this.UnpackWord(H2, output, 8);
			this.UnpackWord(H3, output, 12);
			this.UnpackWord(H4, output, 16);

			return output;
		}

		/// <summary>
		/// Resets <see cref="RIPEMD160HashProvider"/> to its initial state.
		/// </summary>
		public override void Reset()
		{
			this.InitializeHashValue();

			this._byteCount = 0;
			this._bufferOffset = 0;
			for (var i = 0; i < this._buffer.Length; i++)
			{
				this._buffer[i] = 0;
			}

			this._offset = 0;

			for (var i = 0; i != this.X.Length; i++)
			{
				this.X[i] = 0;
			}
		}

		void InitializeHashValue()
		{
			H0 = unchecked(0x67452301);
			H1 = unchecked((int)0xefcdab89);
			H2 = unchecked((int)0x98badcfe);
			H3 = unchecked(0x10325476);
			H4 = unchecked((int)0xc3d2e1f0);
		}

		void ProcessWord(byte[] input, int inOff)
		{
			this.X[this._offset++] = (input[inOff] & 0xff) | ((input[inOff + 1] & 0xff) << 8) | ((input[inOff + 2] & 0xff) << 16) | ((input[inOff + 3] & 0xff) << 24);
			if (this._offset == 16)
				this.ProcessBlock();
		}

		void ProcessLength(long bitLength)
		{
			if (this._offset > 14)
				this.ProcessBlock();
			this.X[14] = (int)(bitLength & 0xffffffff);
			this.X[15] = (int)((ulong)bitLength >> 32);
		}

		void UnpackWord(int word, byte[] outBytes, int outOff)
		{
			outBytes[outOff] = (byte)word;
			outBytes[outOff + 1] = (byte)((uint)word >> 8);
			outBytes[outOff + 2] = (byte)((uint)word >> 16);
			outBytes[outOff + 3] = (byte)((uint)word >> 24);
		}

		void Update(byte input)
		{
			this._buffer[this._bufferOffset++] = input;

			if (this._bufferOffset == this._buffer.Length)
			{
				this.ProcessWord(this._buffer, 0);
				this._bufferOffset = 0;
			}

			this._byteCount++;
		}

		int RL(int x, int n)
		{
			return (x << n) | (int)((uint)x >> (32 - n));
		}

		/// <summary>
		/// Rounds 0-15
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <returns></returns>
		int F1(int x, int y, int z)
		{
			return x ^ y ^ z;
		}

		/// <summary>
		/// Rounds 16-31
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <returns></returns>
		int F2(int x, int y, int z)
		{
			return (x & y) | (~x & z);
		}

		/// <summary>
		/// ounds 32-47
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <returns></returns>
		int F3(int x, int y, int z)
		{
			return (x | ~y) ^ z;
		}

		/// <summary>
		/// Rounds 48-63
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <returns></returns>
		int F4(int x, int y, int z)
		{
			return (x & z) | (y & ~z);
		}

		/// <summary>
		/// ounds 64-79
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <param name="z">The z.</param>
		/// <returns></returns>
		int F5(int x, int y, int z)
		{
			return x ^ (y | ~z);
		}

		void ProcessBlock()
		{
			int aa;
			int bb;
			int cc;
			int dd;
			int ee;

			var a = aa = H0;
			var b = bb = H1;
			var c = cc = H2;
			var d = dd = H3;
			var e = ee = H4;

			//
			// Rounds 1 - 16
			//
			// left
			a = RL(a + F1(b, c, d) + X[0], 11) + e; c = RL(c, 10);
			e = RL(e + F1(a, b, c) + X[1], 14) + d; b = RL(b, 10);
			d = RL(d + F1(e, a, b) + X[2], 15) + c; a = RL(a, 10);
			c = RL(c + F1(d, e, a) + X[3], 12) + b; e = RL(e, 10);
			b = RL(b + F1(c, d, e) + X[4], 5) + a; d = RL(d, 10);
			a = RL(a + F1(b, c, d) + X[5], 8) + e; c = RL(c, 10);
			e = RL(e + F1(a, b, c) + X[6], 7) + d; b = RL(b, 10);
			d = RL(d + F1(e, a, b) + X[7], 9) + c; a = RL(a, 10);
			c = RL(c + F1(d, e, a) + X[8], 11) + b; e = RL(e, 10);
			b = RL(b + F1(c, d, e) + X[9], 13) + a; d = RL(d, 10);
			a = RL(a + F1(b, c, d) + X[10], 14) + e; c = RL(c, 10);
			e = RL(e + F1(a, b, c) + X[11], 15) + d; b = RL(b, 10);
			d = RL(d + F1(e, a, b) + X[12], 6) + c; a = RL(a, 10);
			c = RL(c + F1(d, e, a) + X[13], 7) + b; e = RL(e, 10);
			b = RL(b + F1(c, d, e) + X[14], 9) + a; d = RL(d, 10);
			a = RL(a + F1(b, c, d) + X[15], 8) + e; c = RL(c, 10);

			// right
			aa = RL(aa + F5(bb, cc, dd) + X[5] + unchecked((int)0x50a28be6), 8) + ee; cc = RL(cc, 10);
			ee = RL(ee + F5(aa, bb, cc) + X[14] + unchecked((int)0x50a28be6), 9) + dd; bb = RL(bb, 10);
			dd = RL(dd + F5(ee, aa, bb) + X[7] + unchecked((int)0x50a28be6), 9) + cc; aa = RL(aa, 10);
			cc = RL(cc + F5(dd, ee, aa) + X[0] + unchecked((int)0x50a28be6), 11) + bb; ee = RL(ee, 10);
			bb = RL(bb + F5(cc, dd, ee) + X[9] + unchecked((int)0x50a28be6), 13) + aa; dd = RL(dd, 10);
			aa = RL(aa + F5(bb, cc, dd) + X[2] + unchecked((int)0x50a28be6), 15) + ee; cc = RL(cc, 10);
			ee = RL(ee + F5(aa, bb, cc) + X[11] + unchecked((int)0x50a28be6), 15) + dd; bb = RL(bb, 10);
			dd = RL(dd + F5(ee, aa, bb) + X[4] + unchecked((int)0x50a28be6), 5) + cc; aa = RL(aa, 10);
			cc = RL(cc + F5(dd, ee, aa) + X[13] + unchecked((int)0x50a28be6), 7) + bb; ee = RL(ee, 10);
			bb = RL(bb + F5(cc, dd, ee) + X[6] + unchecked((int)0x50a28be6), 7) + aa; dd = RL(dd, 10);
			aa = RL(aa + F5(bb, cc, dd) + X[15] + unchecked((int)0x50a28be6), 8) + ee; cc = RL(cc, 10);
			ee = RL(ee + F5(aa, bb, cc) + X[8] + unchecked((int)0x50a28be6), 11) + dd; bb = RL(bb, 10);
			dd = RL(dd + F5(ee, aa, bb) + X[1] + unchecked((int)0x50a28be6), 14) + cc; aa = RL(aa, 10);
			cc = RL(cc + F5(dd, ee, aa) + X[10] + unchecked((int)0x50a28be6), 14) + bb; ee = RL(ee, 10);
			bb = RL(bb + F5(cc, dd, ee) + X[3] + unchecked((int)0x50a28be6), 12) + aa; dd = RL(dd, 10);
			aa = RL(aa + F5(bb, cc, dd) + X[12] + unchecked((int)0x50a28be6), 6) + ee; cc = RL(cc, 10);

			//
			// Rounds 16-31
			//
			// left
			e = RL(e + F2(a, b, c) + X[7] + unchecked((int)0x5a827999), 7) + d; b = RL(b, 10);
			d = RL(d + F2(e, a, b) + X[4] + unchecked((int)0x5a827999), 6) + c; a = RL(a, 10);
			c = RL(c + F2(d, e, a) + X[13] + unchecked((int)0x5a827999), 8) + b; e = RL(e, 10);
			b = RL(b + F2(c, d, e) + X[1] + unchecked((int)0x5a827999), 13) + a; d = RL(d, 10);
			a = RL(a + F2(b, c, d) + X[10] + unchecked((int)0x5a827999), 11) + e; c = RL(c, 10);
			e = RL(e + F2(a, b, c) + X[6] + unchecked((int)0x5a827999), 9) + d; b = RL(b, 10);
			d = RL(d + F2(e, a, b) + X[15] + unchecked((int)0x5a827999), 7) + c; a = RL(a, 10);
			c = RL(c + F2(d, e, a) + X[3] + unchecked((int)0x5a827999), 15) + b; e = RL(e, 10);
			b = RL(b + F2(c, d, e) + X[12] + unchecked((int)0x5a827999), 7) + a; d = RL(d, 10);
			a = RL(a + F2(b, c, d) + X[0] + unchecked((int)0x5a827999), 12) + e; c = RL(c, 10);
			e = RL(e + F2(a, b, c) + X[9] + unchecked((int)0x5a827999), 15) + d; b = RL(b, 10);
			d = RL(d + F2(e, a, b) + X[5] + unchecked((int)0x5a827999), 9) + c; a = RL(a, 10);
			c = RL(c + F2(d, e, a) + X[2] + unchecked((int)0x5a827999), 11) + b; e = RL(e, 10);
			b = RL(b + F2(c, d, e) + X[14] + unchecked((int)0x5a827999), 7) + a; d = RL(d, 10);
			a = RL(a + F2(b, c, d) + X[11] + unchecked((int)0x5a827999), 13) + e; c = RL(c, 10);
			e = RL(e + F2(a, b, c) + X[8] + unchecked((int)0x5a827999), 12) + d; b = RL(b, 10);

			// right
			ee = RL(ee + F4(aa, bb, cc) + X[6] + unchecked((int)0x5c4dd124), 9) + dd; bb = RL(bb, 10);
			dd = RL(dd + F4(ee, aa, bb) + X[11] + unchecked((int)0x5c4dd124), 13) + cc; aa = RL(aa, 10);
			cc = RL(cc + F4(dd, ee, aa) + X[3] + unchecked((int)0x5c4dd124), 15) + bb; ee = RL(ee, 10);
			bb = RL(bb + F4(cc, dd, ee) + X[7] + unchecked((int)0x5c4dd124), 7) + aa; dd = RL(dd, 10);
			aa = RL(aa + F4(bb, cc, dd) + X[0] + unchecked((int)0x5c4dd124), 12) + ee; cc = RL(cc, 10);
			ee = RL(ee + F4(aa, bb, cc) + X[13] + unchecked((int)0x5c4dd124), 8) + dd; bb = RL(bb, 10);
			dd = RL(dd + F4(ee, aa, bb) + X[5] + unchecked((int)0x5c4dd124), 9) + cc; aa = RL(aa, 10);
			cc = RL(cc + F4(dd, ee, aa) + X[10] + unchecked((int)0x5c4dd124), 11) + bb; ee = RL(ee, 10);
			bb = RL(bb + F4(cc, dd, ee) + X[14] + unchecked((int)0x5c4dd124), 7) + aa; dd = RL(dd, 10);
			aa = RL(aa + F4(bb, cc, dd) + X[15] + unchecked((int)0x5c4dd124), 7) + ee; cc = RL(cc, 10);
			ee = RL(ee + F4(aa, bb, cc) + X[8] + unchecked((int)0x5c4dd124), 12) + dd; bb = RL(bb, 10);
			dd = RL(dd + F4(ee, aa, bb) + X[12] + unchecked((int)0x5c4dd124), 7) + cc; aa = RL(aa, 10);
			cc = RL(cc + F4(dd, ee, aa) + X[4] + unchecked((int)0x5c4dd124), 6) + bb; ee = RL(ee, 10);
			bb = RL(bb + F4(cc, dd, ee) + X[9] + unchecked((int)0x5c4dd124), 15) + aa; dd = RL(dd, 10);
			aa = RL(aa + F4(bb, cc, dd) + X[1] + unchecked((int)0x5c4dd124), 13) + ee; cc = RL(cc, 10);
			ee = RL(ee + F4(aa, bb, cc) + X[2] + unchecked((int)0x5c4dd124), 11) + dd; bb = RL(bb, 10);

			//
			// Rounds 32-47
			//
			// left
			d = RL(d + F3(e, a, b) + X[3] + unchecked((int)0x6ed9eba1), 11) + c; a = RL(a, 10);
			c = RL(c + F3(d, e, a) + X[10] + unchecked((int)0x6ed9eba1), 13) + b; e = RL(e, 10);
			b = RL(b + F3(c, d, e) + X[14] + unchecked((int)0x6ed9eba1), 6) + a; d = RL(d, 10);
			a = RL(a + F3(b, c, d) + X[4] + unchecked((int)0x6ed9eba1), 7) + e; c = RL(c, 10);
			e = RL(e + F3(a, b, c) + X[9] + unchecked((int)0x6ed9eba1), 14) + d; b = RL(b, 10);
			d = RL(d + F3(e, a, b) + X[15] + unchecked((int)0x6ed9eba1), 9) + c; a = RL(a, 10);
			c = RL(c + F3(d, e, a) + X[8] + unchecked((int)0x6ed9eba1), 13) + b; e = RL(e, 10);
			b = RL(b + F3(c, d, e) + X[1] + unchecked((int)0x6ed9eba1), 15) + a; d = RL(d, 10);
			a = RL(a + F3(b, c, d) + X[2] + unchecked((int)0x6ed9eba1), 14) + e; c = RL(c, 10);
			e = RL(e + F3(a, b, c) + X[7] + unchecked((int)0x6ed9eba1), 8) + d; b = RL(b, 10);
			d = RL(d + F3(e, a, b) + X[0] + unchecked((int)0x6ed9eba1), 13) + c; a = RL(a, 10);
			c = RL(c + F3(d, e, a) + X[6] + unchecked((int)0x6ed9eba1), 6) + b; e = RL(e, 10);
			b = RL(b + F3(c, d, e) + X[13] + unchecked((int)0x6ed9eba1), 5) + a; d = RL(d, 10);
			a = RL(a + F3(b, c, d) + X[11] + unchecked((int)0x6ed9eba1), 12) + e; c = RL(c, 10);
			e = RL(e + F3(a, b, c) + X[5] + unchecked((int)0x6ed9eba1), 7) + d; b = RL(b, 10);
			d = RL(d + F3(e, a, b) + X[12] + unchecked((int)0x6ed9eba1), 5) + c; a = RL(a, 10);

			// right
			dd = RL(dd + F3(ee, aa, bb) + X[15] + unchecked((int)0x6d703ef3), 9) + cc; aa = RL(aa, 10);
			cc = RL(cc + F3(dd, ee, aa) + X[5] + unchecked((int)0x6d703ef3), 7) + bb; ee = RL(ee, 10);
			bb = RL(bb + F3(cc, dd, ee) + X[1] + unchecked((int)0x6d703ef3), 15) + aa; dd = RL(dd, 10);
			aa = RL(aa + F3(bb, cc, dd) + X[3] + unchecked((int)0x6d703ef3), 11) + ee; cc = RL(cc, 10);
			ee = RL(ee + F3(aa, bb, cc) + X[7] + unchecked((int)0x6d703ef3), 8) + dd; bb = RL(bb, 10);
			dd = RL(dd + F3(ee, aa, bb) + X[14] + unchecked((int)0x6d703ef3), 6) + cc; aa = RL(aa, 10);
			cc = RL(cc + F3(dd, ee, aa) + X[6] + unchecked((int)0x6d703ef3), 6) + bb; ee = RL(ee, 10);
			bb = RL(bb + F3(cc, dd, ee) + X[9] + unchecked((int)0x6d703ef3), 14) + aa; dd = RL(dd, 10);
			aa = RL(aa + F3(bb, cc, dd) + X[11] + unchecked((int)0x6d703ef3), 12) + ee; cc = RL(cc, 10);
			ee = RL(ee + F3(aa, bb, cc) + X[8] + unchecked((int)0x6d703ef3), 13) + dd; bb = RL(bb, 10);
			dd = RL(dd + F3(ee, aa, bb) + X[12] + unchecked((int)0x6d703ef3), 5) + cc; aa = RL(aa, 10);
			cc = RL(cc + F3(dd, ee, aa) + X[2] + unchecked((int)0x6d703ef3), 14) + bb; ee = RL(ee, 10);
			bb = RL(bb + F3(cc, dd, ee) + X[10] + unchecked((int)0x6d703ef3), 13) + aa; dd = RL(dd, 10);
			aa = RL(aa + F3(bb, cc, dd) + X[0] + unchecked((int)0x6d703ef3), 13) + ee; cc = RL(cc, 10);
			ee = RL(ee + F3(aa, bb, cc) + X[4] + unchecked((int)0x6d703ef3), 7) + dd; bb = RL(bb, 10);
			dd = RL(dd + F3(ee, aa, bb) + X[13] + unchecked((int)0x6d703ef3), 5) + cc; aa = RL(aa, 10);

			//
			// Rounds 48-63
			//
			// left
			c = RL(c + F4(d, e, a) + X[1] + unchecked((int)0x8f1bbcdc), 11) + b; e = RL(e, 10);
			b = RL(b + F4(c, d, e) + X[9] + unchecked((int)0x8f1bbcdc), 12) + a; d = RL(d, 10);
			a = RL(a + F4(b, c, d) + X[11] + unchecked((int)0x8f1bbcdc), 14) + e; c = RL(c, 10);
			e = RL(e + F4(a, b, c) + X[10] + unchecked((int)0x8f1bbcdc), 15) + d; b = RL(b, 10);
			d = RL(d + F4(e, a, b) + X[0] + unchecked((int)0x8f1bbcdc), 14) + c; a = RL(a, 10);
			c = RL(c + F4(d, e, a) + X[8] + unchecked((int)0x8f1bbcdc), 15) + b; e = RL(e, 10);
			b = RL(b + F4(c, d, e) + X[12] + unchecked((int)0x8f1bbcdc), 9) + a; d = RL(d, 10);
			a = RL(a + F4(b, c, d) + X[4] + unchecked((int)0x8f1bbcdc), 8) + e; c = RL(c, 10);
			e = RL(e + F4(a, b, c) + X[13] + unchecked((int)0x8f1bbcdc), 9) + d; b = RL(b, 10);
			d = RL(d + F4(e, a, b) + X[3] + unchecked((int)0x8f1bbcdc), 14) + c; a = RL(a, 10);
			c = RL(c + F4(d, e, a) + X[7] + unchecked((int)0x8f1bbcdc), 5) + b; e = RL(e, 10);
			b = RL(b + F4(c, d, e) + X[15] + unchecked((int)0x8f1bbcdc), 6) + a; d = RL(d, 10);
			a = RL(a + F4(b, c, d) + X[14] + unchecked((int)0x8f1bbcdc), 8) + e; c = RL(c, 10);
			e = RL(e + F4(a, b, c) + X[5] + unchecked((int)0x8f1bbcdc), 6) + d; b = RL(b, 10);
			d = RL(d + F4(e, a, b) + X[6] + unchecked((int)0x8f1bbcdc), 5) + c; a = RL(a, 10);
			c = RL(c + F4(d, e, a) + X[2] + unchecked((int)0x8f1bbcdc), 12) + b; e = RL(e, 10);

			// right
			cc = RL(cc + F2(dd, ee, aa) + X[8] + unchecked((int)0x7a6d76e9), 15) + bb; ee = RL(ee, 10);
			bb = RL(bb + F2(cc, dd, ee) + X[6] + unchecked((int)0x7a6d76e9), 5) + aa; dd = RL(dd, 10);
			aa = RL(aa + F2(bb, cc, dd) + X[4] + unchecked((int)0x7a6d76e9), 8) + ee; cc = RL(cc, 10);
			ee = RL(ee + F2(aa, bb, cc) + X[1] + unchecked((int)0x7a6d76e9), 11) + dd; bb = RL(bb, 10);
			dd = RL(dd + F2(ee, aa, bb) + X[3] + unchecked((int)0x7a6d76e9), 14) + cc; aa = RL(aa, 10);
			cc = RL(cc + F2(dd, ee, aa) + X[11] + unchecked((int)0x7a6d76e9), 14) + bb; ee = RL(ee, 10);
			bb = RL(bb + F2(cc, dd, ee) + X[15] + unchecked((int)0x7a6d76e9), 6) + aa; dd = RL(dd, 10);
			aa = RL(aa + F2(bb, cc, dd) + X[0] + unchecked((int)0x7a6d76e9), 14) + ee; cc = RL(cc, 10);
			ee = RL(ee + F2(aa, bb, cc) + X[5] + unchecked((int)0x7a6d76e9), 6) + dd; bb = RL(bb, 10);
			dd = RL(dd + F2(ee, aa, bb) + X[12] + unchecked((int)0x7a6d76e9), 9) + cc; aa = RL(aa, 10);
			cc = RL(cc + F2(dd, ee, aa) + X[2] + unchecked((int)0x7a6d76e9), 12) + bb; ee = RL(ee, 10);
			bb = RL(bb + F2(cc, dd, ee) + X[13] + unchecked((int)0x7a6d76e9), 9) + aa; dd = RL(dd, 10);
			aa = RL(aa + F2(bb, cc, dd) + X[9] + unchecked((int)0x7a6d76e9), 12) + ee; cc = RL(cc, 10);
			ee = RL(ee + F2(aa, bb, cc) + X[7] + unchecked((int)0x7a6d76e9), 5) + dd; bb = RL(bb, 10);
			dd = RL(dd + F2(ee, aa, bb) + X[10] + unchecked((int)0x7a6d76e9), 15) + cc; aa = RL(aa, 10);
			cc = RL(cc + F2(dd, ee, aa) + X[14] + unchecked((int)0x7a6d76e9), 8) + bb; ee = RL(ee, 10);

			//
			// Rounds 64-79
			//
			// left
			b = RL(b + F5(c, d, e) + X[4] + unchecked((int)0xa953fd4e), 9) + a; d = RL(d, 10);
			a = RL(a + F5(b, c, d) + X[0] + unchecked((int)0xa953fd4e), 15) + e; c = RL(c, 10);
			e = RL(e + F5(a, b, c) + X[5] + unchecked((int)0xa953fd4e), 5) + d; b = RL(b, 10);
			d = RL(d + F5(e, a, b) + X[9] + unchecked((int)0xa953fd4e), 11) + c; a = RL(a, 10);
			c = RL(c + F5(d, e, a) + X[7] + unchecked((int)0xa953fd4e), 6) + b; e = RL(e, 10);
			b = RL(b + F5(c, d, e) + X[12] + unchecked((int)0xa953fd4e), 8) + a; d = RL(d, 10);
			a = RL(a + F5(b, c, d) + X[2] + unchecked((int)0xa953fd4e), 13) + e; c = RL(c, 10);
			e = RL(e + F5(a, b, c) + X[10] + unchecked((int)0xa953fd4e), 12) + d; b = RL(b, 10);
			d = RL(d + F5(e, a, b) + X[14] + unchecked((int)0xa953fd4e), 5) + c; a = RL(a, 10);
			c = RL(c + F5(d, e, a) + X[1] + unchecked((int)0xa953fd4e), 12) + b; e = RL(e, 10);
			b = RL(b + F5(c, d, e) + X[3] + unchecked((int)0xa953fd4e), 13) + a; d = RL(d, 10);
			a = RL(a + F5(b, c, d) + X[8] + unchecked((int)0xa953fd4e), 14) + e; c = RL(c, 10);
			e = RL(e + F5(a, b, c) + X[11] + unchecked((int)0xa953fd4e), 11) + d; b = RL(b, 10);
			d = RL(d + F5(e, a, b) + X[6] + unchecked((int)0xa953fd4e), 8) + c; a = RL(a, 10);
			c = RL(c + F5(d, e, a) + X[15] + unchecked((int)0xa953fd4e), 5) + b; e = RL(e, 10);
			b = RL(b + F5(c, d, e) + X[13] + unchecked((int)0xa953fd4e), 6) + a; d = RL(d, 10);

			// right
			bb = RL(bb + F1(cc, dd, ee) + X[12], 8) + aa; dd = RL(dd, 10);
			aa = RL(aa + F1(bb, cc, dd) + X[15], 5) + ee; cc = RL(cc, 10);
			ee = RL(ee + F1(aa, bb, cc) + X[10], 12) + dd; bb = RL(bb, 10);
			dd = RL(dd + F1(ee, aa, bb) + X[4], 9) + cc; aa = RL(aa, 10);
			cc = RL(cc + F1(dd, ee, aa) + X[1], 12) + bb; ee = RL(ee, 10);
			bb = RL(bb + F1(cc, dd, ee) + X[5], 5) + aa; dd = RL(dd, 10);
			aa = RL(aa + F1(bb, cc, dd) + X[8], 14) + ee; cc = RL(cc, 10);
			ee = RL(ee + F1(aa, bb, cc) + X[7], 6) + dd; bb = RL(bb, 10);
			dd = RL(dd + F1(ee, aa, bb) + X[6], 8) + cc; aa = RL(aa, 10);
			cc = RL(cc + F1(dd, ee, aa) + X[2], 13) + bb; ee = RL(ee, 10);
			bb = RL(bb + F1(cc, dd, ee) + X[13], 6) + aa; dd = RL(dd, 10);
			aa = RL(aa + F1(bb, cc, dd) + X[14], 5) + ee; cc = RL(cc, 10);
			ee = RL(ee + F1(aa, bb, cc) + X[0], 15) + dd; bb = RL(bb, 10);
			dd = RL(dd + F1(ee, aa, bb) + X[3], 13) + cc; aa = RL(aa, 10);
			cc = RL(cc + F1(dd, ee, aa) + X[9], 11) + bb; ee = RL(ee, 10);
			bb = RL(bb + F1(cc, dd, ee) + X[11], 11) + aa; dd = RL(dd, 10);

			dd += c + H1;
			H1 = H2 + d + ee;
			H2 = H3 + e + aa;
			H3 = H4 + a + bb;
			H4 = H0 + b + cc;
			H0 = dd;

			//
			// reset the offset and clean out the word buffer.
			//
			this._offset = 0;
			for (var i = 0; i < this.X.Length; i++)
				this.X[i] = 0;
		}
	}
	#endregion

	#region RIPEMD160
	/// <summary>
	/// Cryptographic hash function based upon the Merkle–Damgård construction.
	/// </summary>
	public sealed class RIPEMD160 : HashAlgorithm
	{
		/// <summary>
		/// Creates a new instance of the <see cref="RIPEMD160"/> class.
		/// </summary>
		/// <returns></returns>
		public new static RIPEMD160 Create()
		{
			return new RIPEMD160();
		}

		/// <summary>
		/// Creates a new instance of the <see cref="RIPEMD160"/> class.
		/// </summary>
		/// <param name="hashName"></param>
		/// <returns></returns>
		public new static RIPEMD160 Create(string hashName)
		{
			return new RIPEMD160();
		}

		IHashProvider _hashProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="RIPEMD160"/> class.
		/// </summary>
		public RIPEMD160()
		{
			this._hashProvider = new RIPEMD160HashProvider();
		}

		/// <summary>
		/// Gets the size, in bits, of the computed hash code.
		/// </summary>
		/// <returns>
		/// The size, in bits, of the computed hash code.
		/// </returns>
		public override int HashSize
		{
			get
			{
				return this._hashProvider.HashSize;
			}
		}

		/// <summary>
		/// Routes data written to the object into the hash algorithm for computing the hash.
		/// </summary>
		/// <param name="array">The input to compute the hash code for.</param>
		/// <param name="ibStart">The offset into the byte array from which to begin using data.</param>
		/// <param name="cbSize">The number of bytes in the byte array to use as data.</param>
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			this._hashProvider.HashCore(array, ibStart, cbSize);
		}

		/// <summary>
		/// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
		/// </summary>
		/// <returns>
		/// The computed hash code.
		/// </returns>
		protected override byte[] HashFinal()
		{
			return this._hashProvider.HashFinal();
		}

		/// <summary>
		/// Initializes an implementation of the <see cref="HashAlgorithm"/> class.
		/// </summary>
		public override void Initialize()
		{
			this._hashProvider.Reset();
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="RIPEMD160"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				this._hashProvider.Dispose();
				this._hashProvider = null;
			}
		}
	}
	#endregion

	#region HMAC RIPEMD160
	// SshNet.Security.Cryptography - https://github.com/sshnet/Cryptography
	/// <summary>
	/// Computes a Hash-based Message Authentication Code (HMAC) by using the <see cref="RIPEMD160"/> hash function.
	/// </summary>
	public sealed class HMACRIPEMD160 : HMAC
	{
		IHashProvider _hashProvider;
		byte[] _innerPadding;
		byte[] _outerPadding;
		readonly int _hashSize;

		/// <summary>
		/// Holds value indicating whether the inner padding was already written.
		/// </summary>
		bool _innerPaddingWritten;

		/// <summary>
		/// Gets or sets the block size, in bytes, to use in the hash value.
		/// </summary>
		/// <value>
		/// The block size to use in the hash value. For <see cref="HMACRIPEMD160"/> this is 64 bytes.
		/// </value>
		protected int BlockSize
		{
			get { return 64; }
		}

		/// <summary>
		/// Gets the size, in bits, of the computed hash code.
		/// </summary>
		/// <value>
		/// The size, in bits, of the computed hash code.
		/// </value>
		public override int HashSize
		{
			get { return this._hashSize; }
		}

		/// <summary>
		/// Gets or sets the key to use in the hash algorithm.
		/// </summary>
		/// <returns>
		/// The key to use in the hash algorithm.
		/// </returns>
		public override byte[] Key
		{
			get
			{
				return base.Key;
			}
			set
			{
				this.SetKey(value);
			}
		}

		/// <summary>
		/// Initializes a <see cref="HMACRIPEMD160"/> with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		public HMACRIPEMD160(byte[] key)
		{
			this._hashProvider = new RIPEMD160HashProvider();
			this._hashSize = this._hashProvider.HashSize;
			this.SetKey(key);
		}

		/// <summary>
		/// Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.
		/// </summary>
		public override void Initialize()
		{
			this._hashProvider.Reset();
			this._innerPaddingWritten = false;
		}

		/// <summary>
		/// Hashes the core.
		/// </summary>
		/// <param name="rgb">The RGB.</param>
		/// <param name="ib">The ib.</param>
		/// <param name="cb">The cb.</param>
		protected override void HashCore(byte[] rgb, int ib, int cb)
		{
			if (!this._innerPaddingWritten)
			{
				// write the inner padding
				this._hashProvider.TransformBlock(this._innerPadding, 0, BlockSize, _innerPadding, 0);

				// ensure we only write inner padding once
				this._innerPaddingWritten = true;
			}

			this._hashProvider.HashCore(rgb, ib, cb);
		}

		/// <summary>
		/// Finalizes the hash computation after the last data is processed by the cryptographic stream object.
		/// </summary>
		/// <returns>
		/// The computed hash code.
		/// </returns>
		protected override byte[] HashFinal()
		{
			// finalize the original hash
			var hashValue = this._hashProvider.ComputeHash(new byte[0]);

			// write the outer padding
			this._hashProvider.TransformBlock(this._outerPadding, 0, BlockSize, _outerPadding, 0);

			// write the inner hash and finalize the hash
			this._hashProvider.TransformFinalBlock(hashValue, 0, hashValue.Length);

			var hash = this._hashProvider.Hash;

			return this.GetTruncatedHash(hash);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (this._hashProvider != null)
			{
				this._hashProvider.Dispose();
				this._hashProvider = null;
			}
		}

		byte[] GetTruncatedHash(byte[] hash)
		{
			var hashSizeBytes = this.HashSize / 8;
			if (hash.Length == hashSizeBytes)
				return hash;

			var truncatedHash = new byte[hashSizeBytes];
			Buffer.BlockCopy(hash, 0, truncatedHash, 0, hashSizeBytes);
			return truncatedHash;
		}

		void SetKey(byte[] value)
		{
			var shortenedKey = value.Length > this.BlockSize
				? this._hashProvider.ComputeHash(value)
				: value;

			this._innerPadding = new byte[this.BlockSize];
			this._outerPadding = new byte[this.BlockSize];

			// compute inner and outer padding.
			for (var i = 0; i < shortenedKey.Length; i++)
			{
				this._innerPadding[i] = (byte)(0x36 ^ shortenedKey[i]);
				this._outerPadding[i] = (byte)(0x5C ^ shortenedKey[i]);
			}

			for (var i = shortenedKey.Length; i < BlockSize; i++)
			{
				this._innerPadding[i] = 0x36;
				this._outerPadding[i] = 0x5C;
			}

			// no need to explicitly clone as this is already done in the setter
			base.Key = shortenedKey;
		}
	}
	#endregion

	// -------------------------------------------------------------------------------
	// Elliptic Curve Cryptography that follow secp256k1 specs (Bitcoin) => [TangibleCryptography - https://github.com/TangibleCryptography/Secp256k1]

	/// <summary>
	/// Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
	/// </summary>
	public sealed class ECCsecp256k1
	{
		public static readonly BigInteger P = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F".ToBigInteger();
		public static readonly Point G = Point.Decode("0479BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8".HexToBytes());
		public static readonly BigInteger N = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141".ToBigInteger();

		public static DSA ECCDSA = new DSA();
		public static Encryption ECCEncryption = new Encryption();

		#region Point
		public class Point : ICloneable
		{
			private Point()
			{
				this.IsInfinity = true;
			}

			public Point(BigInteger x, BigInteger y, bool isInfinity = false)
			{
				this.X = x;
				this.Y = y;
				this.IsInfinity = isInfinity;
			}

			public BigInteger X { get; private set; }

			public BigInteger Y { get; private set; }

			public static Point Infinity { get { return new Point(); } }

			public bool IsInfinity { get; }

			public object Clone()
			{
				return new Point(this.X, this.Y);
			}

			public byte[] Encode(bool compressed)
			{
				if (this.IsInfinity)
					return new byte[1];

				byte[] x = this.X.ToUnsignedBytes();
				byte[] encoded;
				if (!compressed)
				{
					byte[] y = this.Y.ToUnsignedBytes();
					encoded = new byte[65];
					encoded[0] = 0x04;
					Buffer.BlockCopy(y, 0, encoded, 33 + (32 - y.Length), y.Length);
				}
				else
				{
					encoded = new byte[33];
					encoded[0] = (byte)(this.Y.TestBit(0) ? 0x03 : 0x02);
				}

				Buffer.BlockCopy(x, 0, encoded, 1 + (32 - x.Length), x.Length);
				return encoded;
			}

			public static Point Decode(byte[] encoded)
			{
				if (encoded == null || ((encoded.Length != 33 && encoded[0] != 0x02 && encoded[0] != 0x03) && (encoded.Length != 65 && encoded[0] != 0x04)))
					throw new FormatException("Invalid encoded point");

				var unsigned = new byte[32];
				Buffer.BlockCopy(encoded, 1, unsigned, 0, 32);
				BigInteger x = unsigned.ToUnsignedBigInteger();
				BigInteger y;
				var prefix = encoded[0];

				// uncompressed PubKey
				if (prefix == 0x04)
				{
					Buffer.BlockCopy(encoded, 33, unsigned, 0, 32);
					y = unsigned.ToUnsignedBigInteger();
				}
				// compressed PubKey
				else
				{
					// solve y
					y = ((x * x * x + 7) % ECCsecp256k1.P).ShanksSqrt(ECCsecp256k1.P);

					if (y.IsEven ^ prefix == 0x02)	// negate y for prefix (0x02 indicates y is even, 0x03 indicates y is odd)
						y = -y + ECCsecp256k1.P;      // TODO:  DRY replace this and body of Negate() with call to static method
				}
				return new Point(x, y);
			}

			public Point Negate()
			{
				var r = (Point)this.Clone();
				r.Y = -r.Y + ECCsecp256k1.P;
				return r;
			}

			public Point Subtract(Point point)
			{
				return this.Add(point.Negate());
			}

			public Point Add(Point point)
			{
				BigInteger m;

				if (this.IsInfinity)
					return point;

				if (point.IsInfinity)
					return this;

				if (this.X - point.X == 0)
				{
					if (this.Y - point.Y == 0)
						m = 3 * this.X * this.X * (2 * this.Y).ModInverse(ECCsecp256k1.P);
					else
						return Infinity;
				}
				else
				{
					var mx = (this.X - point.X);
					if (mx < 0)
						mx += ECCsecp256k1.P;
					m = (this.Y - point.Y) * mx.ModInverse(ECCsecp256k1.P);
				}

				m = m % ECCsecp256k1.P;

				var v = this.Y - m * this.X;
				var x3 = (m * m - this.X - point.X);
				x3 = x3 % ECCsecp256k1.P;
				if (x3 < 0)
					x3 += ECCsecp256k1.P;
				var y3 = -(m * x3 + v);
				y3 = y3 % ECCsecp256k1.P;
				if (y3 < 0)
					y3 += ECCsecp256k1.P;

				return new Point(x3, y3);
			}

			public Point Twice()
			{
				return this.Add(this);
			}

			public Point Multiply(BigInteger bigInt)
			{
				if (bigInt.Sign == -1)
					throw new FormatException("The multiplicator cannot be negative");

				bigInt = bigInt % ECCsecp256k1.N;

				Point result = Point.Infinity;
				Point temp = null;
				do
				{
					temp = temp == null
						? this
						: temp.Twice();

					if (!bigInt.IsEven)
						result = result.Add(temp);
				}
				while ((bigInt >>= 1) != 0);

				return result;
			}
		}
		#endregion

		#region Elgamal
		public class Elgamal
		{
			public Point GenerateKey(Point publicKey, out byte[] key, BigInteger? tempKey = null)
			{
				while (tempKey == null || tempKey.Value.IsZero || tempKey.Value >= ECCsecp256k1.N)
					tempKey = new BigInteger(CryptoService.GenerateRandomKey(32).Concat(new byte[1] { 0 }));

				for (int counter = 0; counter < 100; counter++)
				{
					var tag = ECCsecp256k1.G.Multiply(tempKey.Value);
					var keyPoint = publicKey.Multiply(tempKey.Value);

					if (keyPoint.IsInfinity || tag.IsInfinity)
						continue;

					key = keyPoint.Encode(false).GetDoubleHash("SHA256");
					return tag;
				}

				throw new Exception("Unable to generate key");
			}

			public byte[] DecipherKey(BigInteger privateKey, Point tag)
			{
				return tag.Multiply(privateKey).Encode(false).GetDoubleHash("SHA256");
			}
		}
		#endregion

		#region Encryption
		public class Encryption
		{
			Elgamal Elgamal { get; set; }  = new Elgamal();
			RijndaelManaged Rijndael { get; set; } = new RijndaelManaged();

			public Encryption()
			{
				this.Rijndael.KeySize = 256;
				this.Rijndael.BlockSize = 128;
				this.Rijndael.Mode = CipherMode.CBC;
				this.Rijndael.Padding = PaddingMode.PKCS7;
			}

			~Encryption()
			{
				this.Rijndael.Dispose();
			}

			public byte[] Encrypt(Point publicKey, string message)
			{
				return this.Encrypt(publicKey, message.ToBytes());
			}

			public byte[] Encrypt(Point publicKey, byte[] data)
			{
				var tag = this.Elgamal.GenerateKey(publicKey, out byte[] key);
				var tagBytes = tag.Encode(false);

				this.Rijndael.Key = key;
				this.Rijndael.IV = CryptoService.GenerateRandomKey(16);

				using (var cryptor = this.Rijndael.CreateEncryptor())
				{
					var cipherData = cryptor.TransformFinalBlock(data, 0, data.Length);

					var cipher = new byte[cipherData.Length + 65 + 16];
					Buffer.BlockCopy(tagBytes, 0, cipher, 0, 65);
					Buffer.BlockCopy(Rijndael.IV, 0, cipher, 65, 16);
					Buffer.BlockCopy(cipherData, 0, cipher, 65 + 16, cipherData.Length);
					return cipher;
				}
			}

			public byte[] Decrypt(BigInteger privateKey, byte[] cipherData)
			{
				var tagBytes = new byte[65];
				Buffer.BlockCopy(cipherData, 0, tagBytes, 0, tagBytes.Length);
				var keyPoint = Point.Decode(tagBytes);

				var iv = new byte[16];
				Buffer.BlockCopy(cipherData, 65, iv, 0, iv.Length);

				var cipher = new byte[cipherData.Length - 16 - 65];
				Buffer.BlockCopy(cipherData, 65 + 16, cipher, 0, cipher.Length);

				var key = this.Elgamal.DecipherKey(privateKey, keyPoint);

				this.Rijndael.IV = iv;
				this.Rijndael.Key = key;

				using (var decryptor = this.Rijndael.CreateDecryptor())
				{
					return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
				}
			}
		}
		#endregion

		#region DSA
		public class DSA
		{
			public BigInteger[] Sign(BigInteger privateKey, byte[] hash, BigInteger? tempKey = null)
			{
				var hashBigIn = hash.ToUnsignedBigInteger();
				while (tempKey == null || tempKey.Value.IsZero || tempKey >= ECCsecp256k1.N)
					tempKey = new BigInteger(CryptoService.GenerateRandomKey(32).Concat(new byte[1] { 0 }));

				for (int counter = 0; counter < 100; counter++)
				{
					var r = ECCsecp256k1.G.Multiply(tempKey.Value).X % ECCsecp256k1.N;
					if (r.IsZero)
						continue;

					var ss = (hashBigIn + r * privateKey);
					var s = (ss * (tempKey.Value.ModInverse(ECCsecp256k1.N))) % ECCsecp256k1.N;
					if (s.IsZero)
						continue;

					return new BigInteger[] { r, s };
				}

				throw new Exception("Unable to sign");
			}

			public bool Verify(Point publicKey, byte[] hash, BigInteger r, BigInteger s)
			{
				if (r >= ECCsecp256k1.N || r.IsZero || s >= ECCsecp256k1.N || s.IsZero)
					return false;

				var z = hash.ToUnsignedBigInteger();
				var w = s.ModInverse(ECCsecp256k1.N);
				var u1 = (z * w) % ECCsecp256k1.N;
				var u2 = (r * w) % ECCsecp256k1.N;
				var pt = ECCsecp256k1.G.Multiply(u1).Add(publicKey.Multiply(u2));
				var pmod = pt.X % ECCsecp256k1.N;

				return pmod == r;
			}
		}
		#endregion

		#region Generate keys
		/// <summary>
		/// Generates a random private key for using with Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="length">The byte-length of the key (means number of total bytes :: 256 bytes = 2048 bits)</param>
		/// <returns></returns>
		public static BigInteger GeneratePrivateKey(int length = 256) => CryptoService.GenerateRandomKey(length).ToUnsignedBigInteger();

		/// <summary>
		/// Generates the public key from the private key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key</param>
		/// <returns></returns>
		public static Point GeneratePublicKey(BigInteger privateKey) => ECCsecp256k1.G.Multiply(privateKey);

		/// <summary>
		/// Generates the public key from the private key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key</param>
		/// <returns></returns>
		public static byte[] GeneratePublicKey(byte[] privateKey) => ECCsecp256k1.GeneratePublicKey(privateKey.ToUnsignedBigInteger()).Encode(true);

		/// <summary>
		/// Generates the public key from the private key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key</param>
		/// <returns></returns>
		public static byte[] GeneratePublicKey(string privateKey) => ECCsecp256k1.GeneratePublicKey(privateKey.HexToBytes().ToUnsignedBigInteger()).Encode(true);

		/// <summary>
		/// Generates (Decodes) the public key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key</param>
		/// <returns></returns>
		public static Point GetPublicKey(byte[] publicKey) => ECCsecp256k1.Point.Decode(publicKey);

		/// <summary>
		/// Generates (Decodes) the public key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key</param>
		/// <returns></returns>
		public static Point GetPublicKey(string publicKey) => ECCsecp256k1.Point.Decode(publicKey.HexToBytes());

		/// <summary>
		/// Generates (Encodes) the public key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key</param>
		/// <returns></returns>
		public static byte[] GetPublicKey(Point publicKey, bool compress = true) => publicKey.Encode(compress);

		/// <summary>
		/// Generates the public address from the public key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key to generate public address</param>
		/// <param name="compressed">Do compress the public address</param>
		/// <param name="prefix">The byte that presents prefix of the public address</param>
		/// <returns></returns>
		public static string GeneratePublicAddress(Point publicKey, bool compressed = true, byte prefix = 0) => new[] { prefix }.Concat(publicKey.Encode(compressed).GetHash160()).Base58Encode();

		/// <summary>
		/// Generates the public address from the public key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key to generate public address</param>
		/// <param name="compressed">Do compress the address</param>
		/// <param name="prefix">The byte that presents prefix of the public address</param>
		/// <returns></returns>
		public static string GeneratePublicAddress(byte[] publicKey, bool compressed = true, byte prefix = 0) => ECCsecp256k1.GeneratePublicAddress(ECCsecp256k1.GetPublicKey(publicKey), compressed, prefix);

		/// <summary>
		/// Generates the public address from the public key using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key to generate public address</param>
		/// <param name="compressed">Do compress the address</param>
		/// <param name="prefix">The byte that presents prefix of the public address</param>
		/// <returns></returns>
		public static string GeneratePublicAddress(string publicKey, bool compressed = true, byte prefix = 0) => ECCsecp256k1.GeneratePublicAddress(ECCsecp256k1.GetPublicKey(publicKey), compressed, prefix);
		#endregion

		#region Encrypt
		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key to encrypt data</param>
		/// <param name="data">The array of bytes that contains data to encrypt</param>
		/// <returns></returns>
		public static byte[] Encrypt(Point publicKey, byte[] data) => ECCsecp256k1.ECCEncryption.Encrypt(publicKey, data);

		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key to encrypt data</param>
		/// <param name="data">The array of bytes that contains data to encrypt</param>
		/// <returns></returns>
		public static byte[] Encrypt(byte[] publicKey, byte[] data) => ECCsecp256k1.ECCEncryption.Encrypt(ECCsecp256k1.GetPublicKey(publicKey), data);

		/// <summary>
		/// Encrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key to encrypt data</param>
		/// <param name="data">The array of bytes that contains data to encrypt</param>
		/// <returns></returns>
		public static byte[] Encrypt(string publicKey, byte[] data) => ECCsecp256k1.ECCEncryption.Encrypt(ECCsecp256k1.GetPublicKey(publicKey), data);
		#endregion

		#region Decrypt
		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key to decrypt data</param>
		/// <param name="data">The array of bytes that contains data to decrypt</param>
		/// <returns></returns>
		public static byte[] Decrypt(BigInteger privateKey, byte[] data) => ECCsecp256k1.ECCEncryption.Decrypt(privateKey, data);

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key to decrypt data</param>
		/// <param name="data">The array of bytes that contains data to decrypt</param>
		/// <returns></returns>
		public static byte[] Decrypt(byte[] privateKey, byte[] data) => ECCsecp256k1.ECCEncryption.Decrypt(privateKey.ToUnsignedBigInteger(), data);

		/// <summary>
		/// Decrypts the data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key to decrypt data</param>
		/// <param name="data">The array of bytes that contains data to decrypt</param>
		/// <returns></returns>
		public static byte[] Decrypt(string privateKey, byte[] data) => ECCsecp256k1.ECCEncryption.Decrypt(privateKey.HexToBytes().ToUnsignedBigInteger(), data);
		#endregion

		#region Sign
		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="hash">The hashed-data to sign</param>
		/// <returns></returns>
		public static BigInteger[] Sign(BigInteger privateKey, byte[] hash) => ECCsecp256k1.ECCDSA.Sign(privateKey, hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="hash">The hashed-data to sign</param>
		/// <returns></returns>
		public static BigInteger[] Sign(byte[] privateKey, byte[] hash) => ECCsecp256k1.Sign(privateKey.ToUnsignedBigInteger(), hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="hash">The hashed-data to sign</param>
		/// <returns></returns>
		public static BigInteger[] Sign(string privateKey, byte[] hash) => ECCsecp256k1.Sign(privateKey.HexToBytes().ToUnsignedBigInteger(), hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="data">The data to compute hash</param>
		/// <param name="hashMode">The hash mode</param>
		/// <returns></returns>
		public static BigInteger[] Sign(BigInteger privateKey, byte[] data, string hashMode) => ECCsecp256k1.Sign(privateKey, data.GetHash(hashMode));

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="data">The data to compute hash</param>
		/// <param name="hashMode">The hash mode</param>
		/// <returns></returns>
		public static BigInteger[] Sign(byte[] privateKey, byte[] data, string hashMode) => ECCsecp256k1.Sign(privateKey.ToUnsignedBigInteger(), data, hashMode);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="data">The data to compute hash</param>
		/// <param name="hashMode">The hash mode</param>
		/// <returns></returns>
		public static BigInteger[] Sign(string privateKey, byte[] data, string hashMode) => ECCsecp256k1.Sign(privateKey.HexToBytes().ToUnsignedBigInteger(), data, hashMode);
		#endregion

		#region Sign (hex)
		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="hash">The hashed-data to sign</param>
		/// <returns></returns>
		public static string SignAsHex(BigInteger privateKey, byte[] hash)
		{
			var signature = ECCsecp256k1.Sign(privateKey, hash);
			return $"{signature[0].ToHex()}{signature[1].ToHex()}";
		}

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="hash">The hashed-data to sign</param>
		/// <returns></returns>
		public static string SignAsHex(byte[] privateKey, byte[] hash) => ECCsecp256k1.SignAsHex(privateKey.ToUnsignedBigInteger(), hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="hash">The hashed-data to sign</param>
		/// <returns></returns>
		public static string SignAsHex(string privateKey, byte[] hash) => ECCsecp256k1.SignAsHex(privateKey.HexToBytes().ToUnsignedBigInteger(), hash);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="data">The data to compute hash</param>
		/// <param name="hashMode">The hash mode</param>
		/// <returns></returns>
		public static string SignAsHex(BigInteger privateKey, byte[] data, string hashMode) => ECCsecp256k1.SignAsHex(privateKey, data.GetHash(hashMode));

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="data">The data to compute hash</param>
		/// <param name="hashMode">The hash mode</param>
		/// <returns></returns>
		public static string SignAsHex(byte[] privateKey, byte[] data, string hashMode) => ECCsecp256k1.SignAsHex(privateKey.ToUnsignedBigInteger(), data, hashMode);

		/// <summary>
		/// Signs data using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="privateKey">The private key used to to sign</param>
		/// <param name="data">The data to compute hash</param>
		/// <param name="hashMode">The hash mode</param>
		/// <returns></returns>
		public static string SignAsHex(string privateKey, byte[] data, string hashMode) => ECCsecp256k1.SignAsHex(privateKey.HexToBytes().ToUnsignedBigInteger(), data, hashMode);
		#endregion

		#region Verify
		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(Point publicKey, byte[] hash, BigInteger[] signature) => publicKey == null || signature == null || signature.Length < 2 ? false : ECCsecp256k1.ECCDSA.Verify(publicKey, hash, signature[0], signature[1]);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(byte[] publicKey, byte[] hash, BigInteger[] signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(string publicKey, byte[] hash, BigInteger[] signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="data">The data to compute hash to verify with signature</param>
		/// <param name="hashMode">The hash mode</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(Point publicKey, byte[] data, string hashMode, BigInteger[] signature) => ECCsecp256k1.Verify(publicKey, data.GetHash(hashMode), signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="data">The data to compute hash to verify with signature</param>
		/// <param name="hashMode">The hash mode</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(byte[] publicKey, byte[] data, string hashMode, BigInteger[] signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), data, hashMode, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="data">The data to compute hash to verify with signature</param>
		/// <param name="hashMode">The hash mode</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(string publicKey, byte[] data, string hashMode, BigInteger[] signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), data, hashMode, signature);
		#endregion

		#region Verify (hex)
		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(Point publicKey, byte[] hash, string signature) => publicKey == null || string.IsNullOrWhiteSpace(signature) || !signature.Length.Equals(128) ? false : ECCsecp256k1.ECCDSA.Verify(publicKey, hash, signature.Left(64).ToBigInteger(), signature.Right(64).ToBigInteger());

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(byte[] publicKey, byte[] hash, string signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="hash">The hashed-data to veriry with signature</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(string publicKey, byte[] hash, string signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), hash, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="data">The data to compute hash to verify with signature</param>
		/// <param name="hashMode">The hash mode</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(Point publicKey, byte[] data, string hashMode, string signature) => ECCsecp256k1.Verify(publicKey, data.GetHash(hashMode), signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="data">The data to compute hash to verify with signature</param>
		/// <param name="hashMode">The hash mode</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(byte[] publicKey, byte[] data, string hashMode, string signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), data, hashMode, signature);

		/// <summary>
		/// Verifys the signature using Elliptic Curve Cryptography (follow Secp256k1 specs - Bitcoin)
		/// </summary>
		/// <param name="publicKey">The public key used to verify</param>
		/// <param name="data">The data to compute hash to verify with signature</param>
		/// <param name="hashMode">The hash mode</param>
		/// <param name="signature">The signature to verify</param>
		/// <returns></returns>
		public static bool Verify(string publicKey, byte[] data, string hashMode, string signature) => ECCsecp256k1.Verify(ECCsecp256k1.GetPublicKey(publicKey), data, hashMode, signature);
		#endregion

	}
}