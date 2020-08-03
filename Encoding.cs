#region Related components
using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with encoding
	/// </summary>
	public static partial class EncodingService
	{

		#region Constructor
		static EncodingService()
		{
			for (byte @byte = 0; @byte < 255; @byte++)
				EncodingService.HexToByte[EncodingService.ByteToHex[@byte]] = @byte;
			EncodingService.HexToByte["ff"] = 255;
		}

		static string[] ByteToHex { get; } = new[]
		{
			"00", "01", "02", "03", "04", "05", "06", "07",
			"08", "09", "0a", "0b", "0c", "0d", "0e", "0f",
			"10", "11", "12", "13", "14", "15", "16", "17",
			"18", "19", "1a", "1b", "1c", "1d", "1e", "1f",
			"20", "21", "22", "23", "24", "25", "26", "27",
			"28", "29", "2a", "2b", "2c", "2d", "2e", "2f",
			"30", "31", "32", "33", "34", "35", "36", "37",
			"38", "39", "3a", "3b", "3c", "3d", "3e", "3f",
			"40", "41", "42", "43", "44", "45", "46", "47",
			"48", "49", "4a", "4b", "4c", "4d", "4e", "4f",
			"50", "51", "52", "53", "54", "55", "56", "57",
			"58", "59", "5a", "5b", "5c", "5d", "5e", "5f",
			"60", "61", "62", "63", "64", "65", "66", "67",
			"68", "69", "6a", "6b", "6c", "6d", "6e", "6f",
			"70", "71", "72", "73", "74", "75", "76", "77",
			"78", "79", "7a", "7b", "7c", "7d", "7e", "7f",
			"80", "81", "82", "83", "84", "85", "86", "87",
			"88", "89", "8a", "8b", "8c", "8d", "8e", "8f",
			"90", "91", "92", "93", "94", "95", "96", "97",
			"98", "99", "9a", "9b", "9c", "9d", "9e", "9f",
			"a0", "a1", "a2", "a3", "a4", "a5", "a6", "a7",
			"a8", "a9", "aa", "ab", "ac", "ad", "ae", "af",
			"b0", "b1", "b2", "b3", "b4", "b5", "b6", "b7",
			"b8", "b9", "ba", "bb", "bc", "bd", "be", "bf",
			"c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7",
			"c8", "c9", "ca", "cb", "cc", "cd", "ce", "cf",
			"d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7",
			"d8", "d9", "da", "db", "dc", "dd", "de", "df",
			"e0", "e1", "e2", "e3", "e4", "e5", "e6", "e7",
			"e8", "e9", "ea", "eb", "ec", "ed", "ee", "ef",
			"f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7",
			"f8", "f9", "fa", "fb", "fc", "fd", "fe", "ff"
		};

		static Dictionary<string, byte> HexToByte { get; } = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region To Bytes
		/// <summary>
		/// Converts this array segment of bytes to array of bytes
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this ArraySegment<byte> bytes)
			=> bytes.Take();

		/// <summary>
		/// Converts this string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this string @string, Encoding encoding = null)
			=> (encoding ?? Encoding.UTF8).GetBytes(@string);

		/// <summary>
		/// Converts this boolean to array of bytes
		/// </summary>
		/// <param name="bool"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this bool @bool)
			=> BitConverter.GetBytes(@bool);

		/// <summary>
		/// Converts this char to array of bytes
		/// </summary>
		/// <param name="char"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this char @char)
			=> BitConverter.GetBytes(@char);

		/// <summary>
		/// Converts this byte to array of bytes
		/// </summary>
		/// <param name="byte"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this byte @byte)
			=> BitConverter.GetBytes(@byte);

		/// <summary>
		/// Converts this sbyte to array of bytes
		/// </summary>
		/// <param name="sbyte"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this sbyte @sbyte)
			=> BitConverter.GetBytes(@sbyte);

		/// <summary>
		/// Converts this short to array of bytes
		/// </summary>
		/// <param name="short"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this short @short)
			=> BitConverter.GetBytes(@short);

		/// <summary>
		/// Converts this ushort to array of bytes
		/// </summary>
		/// <param name="ushort"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this ushort @ushort)
			=> BitConverter.GetBytes(@ushort);

		/// <summary>
		/// Converts this int to array of bytes
		/// </summary>
		/// <param name="int"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this int @int)
			=> BitConverter.GetBytes(@int);

		/// <summary>
		/// Converts this uint to array of bytes
		/// </summary>
		/// <param name="uint"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this uint @uint)
			=> BitConverter.GetBytes(@uint);

		/// <summary>
		/// Converts this long to array of bytes
		/// </summary>
		/// <param name="long"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this long @long)
			=> BitConverter.GetBytes(@long);

		/// <summary>
		/// Converts this ulong to array of bytes
		/// </summary>
		/// <param name="ulong"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this ulong @ulong)
			=> BitConverter.GetBytes(@ulong);

		/// <summary>
		/// Converts this float to array of bytes
		/// </summary>
		/// <param name="float"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this float @float)
			=> BitConverter.GetBytes(@float);

		/// <summary>
		/// Converts this double to array of bytes
		/// </summary>
		/// <param name="double"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this double @double)
			=> BitConverter.GetBytes(@double);

		/// <summary>
		/// Converts this decimal to array of bytes
		/// </summary>
		/// <param name="decimal"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this decimal @decimal)
		{
			var bytes = new byte[0];
			Decimal.GetBits(@decimal).ForEach(@int => bytes = bytes.Concat(@int.ToBytes()));
			return bytes;
		}

		/// <summary>
		/// Converts this date-time to array of bytes
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this DateTime datetime)
			=> datetime.ToBinary().ToBytes();

		/// <summary>
		/// Converts this big-integer to array of bytes
		/// </summary>
		/// <param name="bigInt"></param>
		/// <param name="toUnsigned"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this BigInteger bigInt, bool toUnsigned = false)
		{
			var bytes = bigInt.ToByteArray();
			if (toUnsigned && bytes[bytes.Length - 1] == 0x00)
				Array.Resize(ref bytes, bytes.Length - 1);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes, 0, bytes.Length);
			return bytes;
		}

		/// <summary>
		/// Converts this big-integer to unsigned array of bytes
		/// </summary>
		/// <param name="bigInt"></param>
		/// <returns></returns>
		public static byte[] ToUnsignedBytes(this BigInteger bigInt)
			=> bigInt.ToBytes(true);

		/// <summary>
		/// Converts this Base64 string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isBase64Url"></param>
		/// <returns></returns>
		public static byte[] Base64ToBytes(this string @string, bool isBase64Url = false)
		{
			if (!isBase64Url)
				return Convert.FromBase64String(@string);

			var base64 = @string.Trim().Replace('-', '+').Replace('_', '/');
			switch (base64.Length % 4)
			{
				case 0:
					break;
				case 2:
					base64 += "==";
					break;
				case 3:
					base64 += "=";
					break;
				default:
					throw new Exception("Illegal base64url string!");
			}
			return Convert.FromBase64String(base64);
		}

		/// <summary>
		/// Converts this Base64Url string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] Base64UrlToBytes(this string @string)
			=> @string.Base64ToBytes(true);
		#endregion

		#region To Hex
		/// <summary>
		/// Converts this array of bytes to hexa string
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToHex(this byte[] bytes)
		{
			var hex = new StringBuilder(bytes.Length * 2);
			foreach (var @byte in bytes)
				hex.Append(EncodingService.ByteToHex[@byte]);
			return hex.ToString();
		}

		/// <summary>
		/// Converts this string to hexa string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isBase64"></param>
		/// <returns></returns>
		public static string ToHex(this string @string, bool isBase64 = false)
			=> (isBase64 ? @string.Base64ToBytes() : @string.ToBytes()).ToHex();

		/// <summary>
		/// Converts this big-integer to hexa string
		/// </summary>
		/// <param name="bigInt"></param>
		/// <returns></returns>
		public static string ToHex(this BigInteger bigInt)
			=> bigInt.ToUnsignedBytes().ToHex();

		/// <summary>
		/// Converts this hexa-string to array of bytes
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static byte[] HexToBytes(this string hex)
		{
			hex = (hex.Length % 2 != 0 ? "0" + hex : hex).ToLower();
			var bytes = new byte[hex.Length / 2];
			for (var index = 0; index < hex.Length / 2; index++)
				bytes[index] = EncodingService.HexToByte[hex.Substring(index * 2, 2)];
			return bytes;
		}
		#endregion

		#region Encode/Decode Base32
		/// <summary>
		/// Encodes this array of bytes to Base32 string
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="addChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string Base32Encode(this byte[] bytes, bool addChecksum = false, string hashAlgorithm = "SHA1")
		{
			if (bytes == null || bytes.Length < 1)
				throw new ArgumentException("Invalid", nameof(bytes));

			var data = addChecksum
				? bytes.Concat(bytes.GetCheckSum(hashAlgorithm, 2))
				: bytes;

			var base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
			var builder = new StringBuilder((data.Length + 7) * 8 / 5);
			int pos = 0, index = 0;
			while (pos < data.Length)
			{
				var current = data[pos];
				int digit;

				// is the current digit going to span a byte boundary?
				if (index > (8 - 5))
				{
					var next = (pos + 1) < data.Length ? data[pos + 1] : 0;
					digit = current & (0xFF >> index);
					index = (index + 5) % 8;
					digit <<= index;
					digit |= next >> (8 - index);
					pos++;
				}
				else
				{
					digit = (current >> (8 - (index + 5))) & 0x1F;
					index = (index + 5) % 8;
					if (index == 0)
						pos++;
				}
				builder.Append(base32Alphabet[digit]);
			}
			return builder.ToString();
		}

		/// <summary>
		/// Decodes this Base32 string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <param name="verifyChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static byte[] Base32Decode(this string @string, bool verifyChecksum = false, string hashAlgorithm = "SHA1")
		{
			if (string.IsNullOrWhiteSpace(@string))
				throw new ArgumentNullException(nameof(@string), "Invalid");

			var base32 = @string.ToUpperInvariant();
			var output = new byte[base32.Length * 5 / 8];
			if (output.Length == 0)
				throw new ArgumentException("The specified string is not valid Base32 format because it doesn't have enough data to construct a complete byte array");

			var pos = 0;
			var subPos = 0;
			var outputPos = 0;
			var outputSubPos = 0;
			var base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

			while (outputPos < output.Length)
			{
				var current = base32Alphabet.IndexOf(base32[pos]);
				if (current < 0)
					throw new FormatException($"Invalid Base32 character \"{@string[pos]}\" at position {pos}");

				var bits = Math.Min(5 - subPos, 8 - outputSubPos);
				output[outputPos] <<= bits;
				output[outputPos] |= (byte)(current >> (5 - (subPos + bits)));
				outputSubPos += bits;

				if (outputSubPos >= 8)
				{
					outputPos++;
					outputSubPos = 0;
				}

				subPos += bits;
				if (subPos >= 5)
				{
					pos++;
					subPos = 0;
				}
			}

			// verify & remove check-sum
			if (verifyChecksum)
			{
				var givenChecksum = output.Take(output.Length - 2);
				output = output.Take(0, output.Length - 2);
				var correctChecksum = output.GetCheckSum(hashAlgorithm, 2);
				return givenChecksum.SequenceEqual(correctChecksum)
					? output
					: null;
			}

			// no check-sum
			return output;
		}

		/// <summary>
		/// Converts this string to Base32 string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="addChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string ToBase32(this string @string, bool addChecksum = false, string hashAlgorithm = "SHA1")
			=> string.IsNullOrWhiteSpace(@string)
				? throw new ArgumentNullException(nameof(@string), "Invalid")
				: @string.ToBytes().Base32Encode(addChecksum, hashAlgorithm);

		/// <summary>
		/// Converts this Base32 string to plain string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="verifyChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string FromBase32(this string @string, bool verifyChecksum = false, string hashAlgorithm = "SHA1")
			=> string.IsNullOrWhiteSpace(@string)
				? throw new ArgumentNullException(nameof(@string), "Invalid")
				: @string.Base32Decode(verifyChecksum, hashAlgorithm).GetString();
		#endregion

		#region Encode/Decode Base58
		/// <summary>
		/// Encodes this array of bytes to Base58 string
		/// </summary>
		/// <param name="bytes">The array of bytes to encode</param>
		/// <param name="addChecksum">true to add checksum</param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string Base58Encode(this byte[] bytes, bool addChecksum = true, string hashAlgorithm = "SHA256")
		{
			// add prefix / surfix
			var data = bytes ?? new byte[0];

			// add check-sum
			if (addChecksum)
				data = data.Concat(data.GetCheckSum(hashAlgorithm, 4));

			// decode byte[] to BigInteger and encode BigInteger to Base58 string
			var bigInt = data.Aggregate<byte, BigInteger>(0, (current, t) => current * 256 + t);
			var base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
			var result = "";
			while (bigInt > 0)
			{
				result = base58Alphabet[(int)(bigInt % 58)] + result;
				bigInt /= 58;
			}

			// append `1` for each leading 0 byte
			for (var index = 0; index < data.Length && data[index] == 0; index++)
				result = '1' + result;

			return result;
		}

		/// <summary>
		/// Decodes this Base58 string to array of bytes
		/// </summary>
		/// <param name="string">The string to decode</param>
		/// <param name="verifyChecksum">true to verify checksum</param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static byte[] Base58Decode(this string @string, bool verifyChecksum = true, string hashAlgorithm = "SHA256")
		{
			// decode Base58 string to BigInteger 
			var base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
			var bigInt = new BigInteger(0);
			for (var index = 0; index < @string.Length; index++)
			{
				var digit = base58Alphabet.IndexOf(@string[index]);
				if (digit < 0)
					throw new FormatException($"Invalid Base58 character \"{@string[index]}\" at position {index}");
				bigInt = bigInt * 58 + digit;
			}

			// encode BigInteger to byte[] - leading zero bytes get encoded as leading `1` characters
			var zeros = Enumerable.Repeat((byte)0, @string.TakeWhile(c => c == '1').Count());
			var output = zeros.Concat(bigInt.ToBytes().Reverse().SkipWhile(b => b == 0)).ToArray();

			// verify & remove check-sum
			if (verifyChecksum)
			{
				var givenChecksum = output.Take(output.Length - 4);
				output = output.Take(0, output.Length - 4);
				var correctChecksum = output.GetCheckSum(hashAlgorithm, 4);
				return givenChecksum.SequenceEqual(correctChecksum)
					? output
					: null;
			}

			// no check-sum
			return output;
		}

		/// <summary>
		/// Converts this string to Base58 string
		/// </summary>
		/// <param name="string">The string to convert</param>
		/// <param name="addChecksum">true to add checksum</param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string ToBase58(this string @string, bool addChecksum = true, string hashAlgorithm = "SHA256")
			=> string.IsNullOrWhiteSpace(@string)
				? null
				: @string.ToBytes().Base58Encode(addChecksum, hashAlgorithm);

		/// <summary>
		/// Converts this Base58 string to plain string
		/// </summary>
		/// <param name="string">The string to convert</param>
		/// <param name="verifyChecksum">true to verify checksum</param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string FromBase58(this string @string, bool verifyChecksum = true, string hashAlgorithm = "SHA256")
			=> string.IsNullOrWhiteSpace(@string)
				? null
				: @string.Base58Decode(verifyChecksum, hashAlgorithm)?.GetString();
		#endregion

		#region Encode/Decode Base64
		/// <summary>
		/// Encodes this array of bytes to Base64 string
		/// </summary>
		/// <param name="bytes">The array of bytes to encode</param>
		/// <param name="addChecksum">true to add checksum</param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string Base64Encode(this byte[] bytes, bool addChecksum = false, string hashAlgorithm = "SHA256")
		{
			if (bytes == null || bytes.Length < 1)
				throw new ArgumentException("Invalid", nameof(bytes));

			var data = addChecksum
				? bytes.Concat(bytes.GetCheckSum(hashAlgorithm, 4))
				: bytes;

			return Convert.ToBase64String(data);
		}

		/// <summary>
		/// Decodes this Base64 string to array of bytes
		/// </summary>
		/// <param name="string">The string to decode</param>
		/// <param name="verifyChecksum">true to verify checksum</param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static byte[] Base64Decode(this string @string, bool verifyChecksum = false, string hashAlgorithm = "SHA256")
		{
			// convert to array of bytes
			var output = Convert.FromBase64String(@string);

			// verify & remove check-sum
			if (verifyChecksum)
			{
				var givenChecksum = output.Take(output.Length - 4);
				output = output.Take(0, output.Length - 4);
				var correctChecksum = output.GetCheckSum(hashAlgorithm, 4);
				return givenChecksum.SequenceEqual(correctChecksum)
					? output
					: null;
			}

			// no check-sum
			return output;
		}

		/// <summary>
		/// Converts this array of bytes to Base64 string
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="addChecksum">true to add checksum</param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string ToBase64(this byte[] bytes, bool addChecksum = false, string hashAlgorithm = "SHA256")
			=> bytes.Base64Encode(addChecksum, hashAlgorithm);

		/// <summary>
		/// Converts this string to Base64 string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isHex"></param>
		/// <param name="isBase64Url"></param>
		/// <param name="addChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string ToBase64(this string @string, bool isHex = false, bool isBase64Url = false, bool addChecksum = false, string hashAlgorithm = "SHA256")
		{
			if (isHex)
				return @string.HexToBytes().ToBase64(addChecksum, hashAlgorithm);

			if (!isBase64Url)
				return @string.ToBytes().ToBase64(addChecksum, hashAlgorithm);

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

		/// <summary>
		/// Converts this array of bytes to Base64Url string
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="addChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string ToBase64Url(this byte[] bytes, bool addChecksum = false, string hashAlgorithm = "SHA256")
			=> bytes.ToBase64(addChecksum, hashAlgorithm).Split('=').First().Replace('+', '-').Replace('/', '_');

		/// <summary>
		/// Converts this string to Base64Url string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isBase64"></param>
		/// <param name="isHex"></param>
		/// <param name="addChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string ToBase64Url(this string @string, bool isBase64 = false, bool isHex = false, bool addChecksum = false, string hashAlgorithm = "SHA256")
			=> (isBase64 ? @string : @string.ToBase64(isHex, false, addChecksum, hashAlgorithm)).Split('=').First().Replace('+', '-').Replace('/', '_');

		/// <summary>
		/// Converts this Base64 string to plain string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="isBase64Url"></param>
		/// <param name="verifyChecksum"></param>
		/// <param name="hashAlgorithm">Name of a hash algorithm (md5, sha1, sha256, sha384, sha512, ripemd/ripemd160, blake128, blake/blake256, blake384, blake512) for working with check-sum</param>
		/// <returns></returns>
		public static string FromBase64(this string @string, bool isBase64Url = false, bool verifyChecksum = false, string hashAlgorithm = "SHA256")
			=> (isBase64Url ? @string.ToBase64(false, true) : @string).Base64Decode(verifyChecksum)?.GetString();

		/// <summary>
		/// Converts this Base64Url string to plain string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string FromBase64Url(this string @string)
			=> @string.FromBase64(true);
		#endregion

		#region Encode/Decode Url
		/// <summary>
		/// Encodes this string to use in url
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string UrlEncode(this string @string)
			=> string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.UrlEncode(@string).Replace("+", "%20").Replace(" ", "%20");

		/// <summary>
		/// Decodes this url-encoded string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string UrlDecode(this string @string)
			=> string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.UrlDecode(@string.Trim().Replace("+", " ").Replace("%20", " "));

		/// <summary>
		/// Encodes this string to Base64Url string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Url64Encode(this string @string)
			=> string.IsNullOrWhiteSpace(@string)
				? ""
				: @string.ToBase64Url();

		/// <summary>
		/// Decodes this Base64Url string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Url64Decode(this string @string)
			=> string.IsNullOrWhiteSpace(@string)
				? ""
				: @string.Base64ToBytes(true).GetString();
		#endregion

		#region Encode/Decode Html
		/// <summary>
		/// Encodes this string to HTML string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string HtmlEncode(this string @string)
			=> string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.HtmlEncode(@string);

		/// <summary>
		/// Decodes this HTML string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string HtmlDecode(this string @string)
			=> string.IsNullOrWhiteSpace(@string)
				? ""
				: WebUtility.HtmlDecode(@string.Trim());
		#endregion

	}
}