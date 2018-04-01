#region Related components
using System;
using System.Net;
using System.Text;
using System.Linq;
using System.IO;
using System.IO.Compression;
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

		#region To Bytes
		/// <summary>
		/// Converts this string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this string @string, Encoding encoding = null)
		{
			return (encoding ?? Encoding.UTF8).GetBytes(@string);
		}

		/// <summary>
		/// Converts this hexa-string to array of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static byte[] HexToBytes(this string @string)
		{
			var bytes = new byte[@string.Length / 2];
			for (var index = 0; index < @string.Length; index += 2)
				bytes[index / 2] = Convert.ToByte(@string.Substring(index, 2), 16);
			return bytes;
		}

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
		{
			return @string.Base64ToBytes(true);
		}

		/// <summary>
		/// Converts this boolean to array of bytes
		/// </summary>
		/// <param name="bool"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this bool @bool)
		{
			return BitConverter.GetBytes(@bool);
		}

		/// <summary>
		/// Converts this char to array of bytes
		/// </summary>
		/// <param name="char"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this char @char)
		{
			return BitConverter.GetBytes(@char);
		}

		/// <summary>
		/// Converts this byte to array of bytes
		/// </summary>
		/// <param name="byte"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this byte @byte)
		{
			return BitConverter.GetBytes(@byte);
		}

		/// <summary>
		/// Converts this sbyte to array of bytes
		/// </summary>
		/// <param name="sbyte"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this sbyte @sbyte)
		{
			return BitConverter.GetBytes(@sbyte);
		}

		/// <summary>
		/// Converts this short to array of bytes
		/// </summary>
		/// <param name="short"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this short @short)
		{
			return BitConverter.GetBytes(@short);
		}

		/// <summary>
		/// Converts this ushort to array of bytes
		/// </summary>
		/// <param name="ushort"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this ushort @ushort)
		{
			return BitConverter.GetBytes(@ushort);
		}

		/// <summary>
		/// Converts this int to array of bytes
		/// </summary>
		/// <param name="int"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this int @int)
		{
			return BitConverter.GetBytes(@int);
		}

		/// <summary>
		/// Converts this uint to array of bytes
		/// </summary>
		/// <param name="uint"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this uint @uint)
		{
			return BitConverter.GetBytes(@uint);
		}

		/// <summary>
		/// Converts this long to array of bytes
		/// </summary>
		/// <param name="long"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this long @long)
		{
			return BitConverter.GetBytes(@long);
		}

		/// <summary>
		/// Converts this ulong to array of bytes
		/// </summary>
		/// <param name="ulong"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this ulong @ulong)
		{
			return BitConverter.GetBytes(@ulong);
		}

		/// <summary>
		/// Converts this float to array of bytes
		/// </summary>
		/// <param name="float"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this float @float)
		{
			return BitConverter.GetBytes(@float);
		}

		/// <summary>
		/// Converts this double to array of bytes
		/// </summary>
		/// <param name="double"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this double @double)
		{
			return BitConverter.GetBytes(@double);
		}

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
		{
			return datetime.ToBinary().ToBytes();
		}

		/// <summary>
		/// Converts this big-integer to array of bytes
		/// </summary>
		/// <param name="bigInt"></param>
		/// <returns></returns>
		public static byte[] ToBytes(this BigInteger bigInt)
		{
			return bigInt.ToByteArray();
		}
		#endregion

		#region To Hexa
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
		/// Converts this big-integer to hexa string
		/// </summary>
		/// <param name="bigInt"></param>
		/// <returns></returns>
		public static string ToHexa(this BigInteger bigInt)
		{
			return bigInt.ToBytes().ToHexa();
		}
		#endregion

		#region To Big Integer
		/// <summary>
		/// Converts this array of bytes to big-integer
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static BigInteger ToBigInteger(this byte[] bytes)
		{
			return new BigInteger(bytes);
		}

		/// <summary>
		/// Converts this hexa-string to big-integer
		/// </summary>
		/// <param name="string"></param>
		/// <remarks>https://stackoverflow.com/questions/30119174/converting-a-hex-string-to-its-biginteger-equivalent-negates-the-value</remarks>
		/// <returns></returns>
		public static BigInteger ToBigInteger(this string @string)
		{
			return BigInteger.Parse(@string, System.Globalization.NumberStyles.AllowHexSpecifier);
		}
		#endregion

		#region Encode/Decode Base32
		/// <summary>
		/// Encodes this array of bytes to Base32 string
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string Base32Encode(this byte[] bytes)
		{
			var base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
			var builder = new StringBuilder((bytes.Length + 7) * 8 / 5);
			int pos = 0, index = 0;
			while (pos < bytes.Length)
			{
				var current = bytes[pos];
				int digit;

				// is the current digit going to span a byte boundary?
				if (index > (8 - 5))
				{
					var next = (pos + 1) < bytes.Length ? bytes[pos + 1] : 0;
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
		/// <returns></returns>
		public static byte[] Base32Decode(this string @string)
		{
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

			return output;
		}

		/// <summary>
		/// Converts this string to Base32 string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ToBase32(this string @string)
		{
			return @string.ToBytes().Base32Encode();
		}

		/// <summary>
		/// Converts this Base32 string to plain string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string FromBase32(this string @string)
		{
			return @string.Base32Decode().GetString();
		}
		#endregion

		#region Encode/Decode Base58
		/// <summary>
		/// Encodes this array of bytes to Base58 string
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="addChecksum"></param>
		/// <param name="prefix"></param>
		/// <param name="postfix"></param>
		/// <returns></returns>
		public static string Base58Encode(this byte[] bytes, bool addChecksum = true, byte[] prefix = null, byte[] postfix = null)
		{
			// add prefix / surfix
			var data = (prefix ?? new byte[0]).Concat(bytes, postfix ?? new byte[0]);

			// add check-sum
			if (addChecksum)
				data = data.Concat(data.GetCheckSum());

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
		/// <param name="string"></param>
		/// <param name="verifyChecksum"></param>
		/// <returns></returns>
		public static byte[] Base58Decode(this string @string, bool verifyChecksum = true)
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
			var bytes = zeros.Concat(bigInt.ToBytes().Reverse().SkipWhile(b => b == 0)).ToArray();

			// verify & remove check-sum
			if (verifyChecksum)
			{
				var givenChecksum = bytes.Sub(bytes.Length - 4);
				bytes = bytes.Sub(0, bytes.Length - 4);
				var correctChecksum = bytes.GetCheckSum();
				return givenChecksum.SequenceEqual(correctChecksum)
					? bytes
					: null;
			}

			// no check-sum
			return bytes;
		}

		/// <summary>
		/// Converts this string to Base58 string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="addChecksum"></param>
		/// <param name="prefix"></param>
		/// <param name="postfix"></param>
		/// <returns></returns>
		public static string ToBase58(this string @string, bool addChecksum = true, byte[] prefix = null, byte[] postfix = null)
		{
			return string.IsNullOrWhiteSpace(@string)
				? null
				: @string.ToBytes().Base58Encode(addChecksum, prefix, postfix);
		}

		/// <summary>
		/// Converts this Base58 string to plain string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="verifyChecksum"></param>
		/// <returns></returns>
		public static string FromBase58(this string @string, bool verifyChecksum = true)
		{
			return string.IsNullOrWhiteSpace(@string)
				? null
				: @string.Base58Decode(verifyChecksum)?.GetString();
		}
		#endregion

		#region Encode/Decode Base64
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
		/// Converts this array of bytes to Base64Url string
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string ToBase64Url(this byte[] bytes)
		{
			return bytes.ToBase64().Split('=').First().Replace('+', '-').Replace('/', '_');
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
			return (isBase64 ? @string : @string.ToBase64(isHexa, false)).Split('=').First().Replace('+', '-').Replace('/', '_');
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

		#region Encode/Decode Url
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
				: @string.Base64ToBytes(true).GetString();
		}
		#endregion

		#region Encode/Decode Html
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

		#region Compressions
		/// <summary>
		/// Compresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static byte[] Compress(this byte[] data, string mode = "deflate")
		{
			using (var stream = new MemoryStream())
			{
				using (var compressor = !string.IsNullOrWhiteSpace(mode) && mode.IsEquals("gzip")
					? new GZipStream(stream, CompressionMode.Compress) as Stream
					: new DeflateStream(stream, CompressionMode.Compress) as Stream
				)
				{
					compressor.Write(data, 0, data.Length);
				}
				return stream.GetBuffer();
			}
		}

		/// <summary>
		/// Compresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<byte[]> CompressAsync(this byte[] data, string mode = "deflate", CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var stream = new MemoryStream())
			{
				using (var compressor = !string.IsNullOrWhiteSpace(mode) && mode.IsEquals("gzip")
					? new GZipStream(stream, CompressionMode.Compress) as Stream
					: new DeflateStream(stream, CompressionMode.Compress) as Stream
				)
				{
					await compressor.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
				}
				return stream.GetBuffer();
			}
		}

		/// <summary>
		/// Decompresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <returns></returns>
		public static byte[] Decompress(this byte[] data, string mode = "deflate")
		{
			using (var input = new MemoryStream(data))
			{
				using (var decompressor = !string.IsNullOrWhiteSpace(mode) && mode.IsEquals("gzip")
					? new GZipStream(input, CompressionMode.Decompress) as Stream
					: new DeflateStream(input, CompressionMode.Decompress) as Stream
				)
				{
					using (var output = new MemoryStream())
					{
						var buffer = new byte[64];
						var read = decompressor.Read(buffer, 0, buffer.Length);
						while (read > 0)
						{
							output.Write(buffer, 0, read);
							buffer = new byte[64];
							read = decompressor.Read(buffer, 0, buffer.Length);
						}
						return output.GetBuffer();
					}
				}
			}
		}

		/// <summary>
		/// Decompresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Compression mode (deflate or gzip)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<byte[]> DecompressAsync(this byte[] data, string mode = "deflate", CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var input = new MemoryStream(data))
			{
				using (var decompressor = !string.IsNullOrWhiteSpace(mode) && mode.IsEquals("gzip")
					? new GZipStream(input, CompressionMode.Decompress) as Stream
					: new DeflateStream(input, CompressionMode.Decompress) as Stream
				)
				{
					using (var output = new MemoryStream())
					{
						var buffer = new byte[64];
						var read = await decompressor.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
						while (read > 0)
						{
							await output.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
							buffer = new byte[64];
							read = await decompressor.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
						}
						return output.GetBuffer();
					}
				}
			}
		}
		#endregion

	}

}