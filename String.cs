#region Related components
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Threading.Tasks;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with string
	/// </summary>
	public static class StringService
	{

		#region Manipulations
		/// <summary>
		/// Gets left-side sub-string (just like VB does)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Left(this string @string, int length)
		{
			if (length < 0)
				throw new ArgumentException($"Argument '{nameof(length)}' must be greater or equal to zero");

			return @string.Equals("")
				? string.Empty
				: length >= @string.Length
					? @string
					: @string.Substring(0, length);
		}

		/// <summary>
		/// Gets right-side sub-string (just like VB does)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Right(this string @string, int length)
		{
			if (length < 0)
				throw new ArgumentException($"Argument '{nameof(length)}' must be greater or equal to zero");

			return @string.Equals("")
				? string.Empty : length >= @string.Length
					? @string
					: @string.Substring(@string.Length - length);
		}

		/// <summary>
		/// Replaces
		/// </summary>
		/// <param name="string"></param>
		/// <param name="comparisonType"></param>
		/// <param name="pattern"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string Replace(this string @string, StringComparison comparisonType, string pattern, string replacement)
		{
			if (string.IsNullOrWhiteSpace(@string))
				return string.Empty;
			else if (string.IsNullOrWhiteSpace(pattern))
				return @string;

			var result = @string;
			var pos = result.IndexOf(pattern, comparisonType);
			while (pos > -1)
			{
				result = result.Remove(pos, pattern.Length);
				if (!string.IsNullOrWhiteSpace(replacement))
					result = result.Insert(pos, replacement);
				pos += string.IsNullOrWhiteSpace(replacement)
					? 1
					: replacement.Length;
				pos = pos < result.Length
					? result.IndexOf(pattern, pos, comparisonType)
					: -1;
			}
			return result;
		}
		#endregion

		#region Comparisions
		/// <summary>
		/// Check to see its equals
		/// </summary>
		/// <param name="string"></param>
		/// <param name="compareTo"></param>
		/// <param name="comparisonType"></param>
		/// <returns></returns>
		public static bool IsEquals(this string @string, string compareTo, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return string.IsNullOrWhiteSpace(compareTo)
				? string.IsNullOrWhiteSpace(@string)
				: @string.Equals(compareTo, comparisonType);
		}

		/// <summary>
		/// Check to see its contains
		/// </summary>
		/// <param name="string"></param>
		/// <param name="substring"></param>
		/// <param name="comparisonType"></param>
		/// <returns></returns>
		public static bool IsContains(this string @string, string substring, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return string.IsNullOrWhiteSpace(substring)
				? false
				: @string.PositionOf(substring, 0, comparisonType) > -1;
		}

		/// <summary>
		/// Check to see its starts with
		/// </summary>
		/// <param name="string"></param>
		/// <param name="substring"></param>
		/// <param name="comparisonType"></param>
		/// <returns></returns>
		public static bool IsStartsWith(this string @string, string substring, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return string.IsNullOrWhiteSpace(substring)
				? false
				: @string.StartsWith(substring, comparisonType);
		}

		/// <summary>
		/// Check to see its ends with
		/// </summary>
		/// <param name="string"></param>
		/// <param name="substring"></param>
		/// <param name="comparisonType"></param>
		/// <returns></returns>
		public static bool IsEndsWith(this string @string, string substring, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return string.IsNullOrWhiteSpace(substring)
				? false
				: @string.EndsWith(substring, comparisonType);
		}

		/// <summary>
		/// Gets position of sub-string (index of sub-string)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="substring"></param>
		/// <param name="startIndex"></param>
		/// <param name="comparisonType"></param>
		/// <returns></returns>
		public static int PositionOf(this string @string, string substring, int startIndex = 0, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			return string.IsNullOrWhiteSpace(substring) || startIndex >= @string.Length
				? -1
				: @string.IndexOf(substring, startIndex < 0 ? 0 : startIndex, comparisonType);
		}

		/// <summary>
		/// Counts the number of appearances of the sub-string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="substring"></param>
		/// <param name="startIndex"></param>
		/// <param name="comparisonType"></param>
		/// <returns></returns>
		public static int Count(this string @string, string substring, int startIndex = 0, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
		{
			if (string.IsNullOrWhiteSpace(substring))
				return 0;

			var count = 0;
			var start = @string.PositionOf(substring, startIndex < 0 ? 0 : startIndex, comparisonType);
			while (start > -1)
			{
				count++;
				start = @string.PositionOf(substring, start + 1, comparisonType);
			}
			return count;
		}
		#endregion

		#region Compressions
		/// <summary>
		/// Compresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Compress(this byte[] data)
		{
			using (var stream = new MemoryStream())
			{
				using (var deflate = new DeflateStream(stream, CompressionMode.Compress))
				{
					deflate.Write(data, 0, data.Length);
				}
				return stream.GetBuffer();
			}
		}

		/// <summary>
		/// Compresses the string using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns>The compressed-string in Base64 format</returns>
		public static string Compress(this string data)
		{
			return data.ToBytes().Compress().ToBase64();
		}

		/// <summary>
		/// Compresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static async Task<byte[]> CompressAsync(this byte[] data)
		{
			using (var stream = new MemoryStream())
			{
				using (var deflate = new DeflateStream(stream, CompressionMode.Compress))
				{
					await deflate.WriteAsync(data, 0, data.Length);
				}
				return stream.GetBuffer();
			}
		}

		/// <summary>
		/// Compresses the string using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns>The compressed-string in Base64 format</returns>
		public static async Task<string> CompressAsync(this string data)
		{
			return (await data.ToBytes().CompressAsync()).ToBase64();
		}

		/// <summary>
		/// Decompresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Decompress(this byte[] data)
		{
			using (var input = new MemoryStream(data))
			{
				using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
				{
					using (var output = new MemoryStream())
					{
						var buffer = new byte[64];
						var readBytes = deflate.Read(buffer, 0, buffer.Length);
						while (readBytes > 0)
						{
							output.Write(buffer, 0, readBytes);
							readBytes = deflate.Read(buffer, 0, buffer.Length);
						}
						return output.GetBuffer();
					}
				}
			}
		}

		/// <summary>
		/// Decompresses the Base64 string using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string Decompress(this string data)
		{
			return data.Base64ToBytes().Decompress().GetString();
		}

		/// <summary>
		/// Decompresses the array of bytes using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static async Task<byte[]> DecompressAsync(this byte[] data)
		{
			using (var input = new MemoryStream(data))
			{
				using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
				{
					using (var output = new MemoryStream())
					{
						var buffer = new byte[64];
						var readBytes = await deflate.ReadAsync(buffer, 0, buffer.Length);
						while (readBytes > 0)
						{
							await output.WriteAsync(buffer, 0, readBytes);
							readBytes = await deflate.ReadAsync(buffer, 0, buffer.Length);
						}
						return output.GetBuffer();
					}
				}
			}
		}

		/// <summary>
		/// Decompresses the Base64 string using Deflate compression method
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static async Task<string> DecompressAsync(this string data)
		{
			return (await data.Base64ToBytes().DecompressAsync()).GetString();
		}
		#endregion

		#region Conversions
		/// <summary>
		/// Gets reversed string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string Reverse(this string @string)
		{
			if (@string.Equals("") || @string.Length < 2)
				return @string;

			var chars = @string.ToCharArray();
			Array.Reverse(chars);
			return new string(chars);
		}

		/// <summary>
		/// Gets the string with first-letter is capitalized
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string GetCapitalizedFirstLetter(this string @string)
		{
			var chars = @string.ToLower().ToCharArray();
			chars[0] = char.ToUpper(chars[0]);
			return new string(chars);
		}

		/// <summary>
		/// Gets the string with first-letter of all words is capitalized
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string GetCapitalizedWords(this string @string)
		{
			return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(@string);
		}

		/// <summary>
		/// Converts this enum-string to enum type value
		/// </summary>
		/// <param name="string"></param>
		/// <param name="type">The type of enum</param>
		/// <returns></returns>
		public static object ToEnum(this string @string, Type type)
		{
			if (type == null || !type.IsEnum)
				throw new ArgumentException($"The type '{nameof(type)}' is not enum");

			return Enum.Parse(type, @string);
		}

		/// <summary>
		/// Converts this enum-string to enum type value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="string"></param>
		/// <returns></returns>
		public static T ToEnum<T>(this string @string)
		{
			return (T)@string.ToEnum(typeof(T));
		}

		/// <summary>
		/// Gets the string from array of bytes with UTF8 encoding
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static string GetString(this byte[] bytes, int count = 0)
		{
			return count > 0
				? Encoding.UTF8.GetString(bytes, 0, count)
				: Encoding.UTF8.GetString(bytes);
		}
		#endregion

		#region Conversions (Vietnamese)
		internal static string ConvertVietnamese(this string @string, int mode)
		{
			if (string.IsNullOrWhiteSpace(@string))
				return string.Empty;

			var utf8Literal = "Ã  Ã¡ áº£ Ã£ áº¡ Ã€ Ã áº¢ Ãƒ áº  Ã¢ áº§ áº¥ áº© áº« áº­ Ã‚ áº¦ áº¤ áº¨ áºª áº¬ Äƒ áº± áº¯ áº³ áºµ áº· Ä‚ áº° áº® áº² áº´ áº¶ "
				+ "Ã² Ã³ á» Ãµ á» Ã’ Ã“ á»Ž Ã• á»Œ Ã´ á»“ á»‘ á»• á»— á»™ Ã” á»’ á» á»” á»– á»˜ Æ¡ á» á»› á»Ÿ á»¡ á»£ Æ  á»œ á»š á»ž á»  á»¢ "
				+ "Ã¨ Ã© áº» áº½ áº¹ Ãˆ Ã‰ áºº áº¼ áº¸ Ãª á» áº¿ á»ƒ á»… á»‡ ÃŠ á»€ áº¾ á»‚ á»„ á»† "
				+ "Ã¹ Ãº á»§ Å© á»¥ Ã™ Ãš á»¦ Å¨ á»¤ Æ° á»« á»© á»­ á»¯ á»± Æ¯ á»ª á»¨ á»¬ á»® á»° "
				+ "Ã¬ Ã­ á»‰ Ä© á»‹ ÃŒ Ã á»ˆ Ä¨ á»Š á»³ Ã½ á»· á»¹ á»µ á»² Ã á»¶ á»¸ á»´ Ä‘ Ä "
				+ "â€œ â€ â€“ Ã â€™ Á ÇŽ â€¦ aI";
			var utf8Literals = utf8Literal.Split(' ');

			var utf8Unicode = "à á ả ã ạ À Á Ả Ã Ạ â ầ ấ ẩ ẫ ậ Â Ầ Ấ Ẩ Ẫ Ậ ă ằ ắ ẳ ẵ ặ Ă Ằ Ắ Ẳ Ẵ Ặ "
				+ "ò ó ỏ õ ọ Ò Ó Ỏ Õ Ọ ô ồ ố ổ ỗ ộ Ô Ồ Ố Ổ Ỗ Ộ ơ ờ ớ ở ỡ ợ Ơ Ờ Ớ Ở Ỡ Ợ "
				+ "è é ẻ ẽ ẹ È É Ẻ Ẽ Ẹ ê ề ế ể ễ ệ Ê Ề Ế Ể Ễ Ệ "
				+ "ù ú ủ ũ ụ Ù Ú Ủ Ũ Ụ ư ừ ứ ử ữ ự Ư Ừ Ứ Ử Ữ Ự "
				+ "ì í ỉ ĩ ị Ì Í Ỉ Ĩ Ị ỳ ý ỷ ỹ ỵ Ỳ Ý Ỷ Ỹ Ỵ đ Đ "
				+ "“ ” – Á ’ Đ ă &nbsp; á";
			var utf8Unicodes = utf8Unicode.Split(' ');

			var utf8UnicodeComposite = "à á ả ã ạ À Á Ả Ã Ạ â ầ ấ ẩ ẫ ậ Â Ầ Ấ Ẩ Ẫ Ậ ă ằ ắ ẳ ẵ ặ Ă Ằ Ắ Ẳ Ẵ Ặ "
				+ "ò ó ỏ õ ọ Ò Ó Ỏ Õ Ọ ô ồ ố ổ ỗ ộ Ô Ồ Ố Ổ Ỗ Ộ ơ ờ ớ ở ỡ ợ Ơ Ờ Ớ Ở Ỡ Ợ "
				+ "è é ẻ ẽ ẹ È É Ẻ Ẽ Ẹ ê ề ế ể ễ ệ Ê Ề Ế Ể Ễ Ệ "
				+ "ù ú ủ ũ ụ Ù Ú Ủ Ũ Ụ ư ừ ứ ử ữ ự Ư Ừ Ứ Ử Ữ Ự "
				+ "ì í ỉ ĩ ị Ì Í Ỉ Ĩ Ị ỳ ý ỷ ỹ ỵ Ỳ Ý Ỷ Ỹ Ỵ đ Đ "
				+ "“ ” – Á ’ Đ ă &nbsp; á";
			var utf8UnicodeComposites = utf8UnicodeComposite.Split(' ');

			var ansi = "a a a a a A A A A A a a a a a a A A A A A A a a a a a a A A A A A A "
				+ "o o o o o O O O O O o o o o o o O O O O O O o o o o o o O O O O O O "
				+ "e e e e e E E E E E e e e e e e E E E E E E "
				+ "u u u u u U U U U U u u u u u u U U U U U U "
				+ "i i i i i I I I I I y y y y y Y Y Y Y Y d D "
				+ "\" \" - A ' D a &nbsp; a";
			var ansis = ansi.Split(' ');

			var decimalUnicode = "&#224; &#225; &#7843; &#227; &#7841; &#192; &#193; &#7842; &#195; &#7840; &#226; &#7847; &#7845; &#7849; &#7851; &#7853; &#194; &#7846; &#7844; &#7848; &#7850; &#7852; &#259; &#7857; &#7855; &#7859; &#7861; &#7863; &#258; &#7856; &#7854; &#7858; &#7860; &#7862; "
				+ "&#242; &#243; &#7887; &#245; &#7885; &#210; &#211; &#7886; &#213; &#7884; &#244; &#7891; &#7889; &#7893; &#7895; &#7897; &#212; &#7890; &#7888; &#7892; &#7894; &#7896; &#417; &#7901; &#7899; &#7903; &#7905; &#7907; &#416; &#7900; &#7898; &#7902; &#7904; &#7906; "
				+ "&#232; &#233; &#7867; &#7869; &#7865; &#200; &#201; &#7866; &#7868; &#7864; &#234; &#7873; &#7871; &#7875; &#7877; &#7879; &#202; &#7872; &#7870; &#7874; &#7876; &#7878; "
				+ "&#249; &#250; &#7911; &#361; &#7909; &#217; &#218; &#7910; &#360; &#7908; &#432; &#7915; &#7913; &#7917; &#7919; &#7921; &#431; &#7914; &#7912; &#7916; &#7918; &#7920; "
				+ "&#236; &#237; &#7881; &#297; &#7883; &#204; &#205; &#7880; &#296; &#7882; &#7923; &#253; &#7927; &#7929; &#7925; &#7922; &#221; &#7926; &#7928; &#7924; &#273; &#272; "
				+ "&#34; &#34; &#45; &#224; &#39; &#272; &#259; &#32; &#225;";
			var decimalUnicodes = decimalUnicode.Split(' ');

			var tcvn3 = "µ ¸ ¶ · ¹ µ ¸ ¶ · ¹ © Ç Ê È É Ë ¢ Ç Ê È É Ë ¨ » ¾ ¼ ½ Æ ¡ » ¾ ¼ ½ Æ "
				+ "ß ã á â ä ß ã á â ä « å è æ ç é ¤ å è æ ç é ¬ ê í ë ì î ¥ ê í ë ì î "
				+ "Ì Ð Î Ï Ñ Ì Ð Î Ï Ñ ª Ò Õ Ó Ô Ö £ Ò Õ Ó Ô Ö "
				+ "ï ó ñ ò ô ï ó ñ ò ô ­ õ ø ö ÷ ù ¦ õ ø ö ÷ ù "
				+ "× Ý Ø Ü Þ × Ý Ø Ü Þ ú ý û ü þ ú ý û ü þ ® § "
				+ "“ ” – ¸ ’ § ¨ &nbsp; ¸";
			var tcvn3s = tcvn3.Split(' ');

			var decodeLength = -1;
			var result = @string.Trim();

			// convert
			switch (mode)
			{
				case 0:             // UTF8 Literal to unicode
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < utf8Literals.Length)
							result = result.Replace(utf8Literals[index], utf8Unicodes[index]);
					break;

				case 1:             // Unicode to UTF8 Literal
					decodeLength = utf8Literals.Length;
					for (int index = 0; index < decodeLength; index++)
						result = result.Replace(utf8Unicodes[index], utf8Literals[index]);
					break;

				case 2:             // Unicode to ANSI
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < ansis.Length)
							result = result.Replace(utf8Unicodes[index], ansis[index]);
					break;

				case 3:             // UTF8 Literal to ANSI
					decodeLength = utf8Literals.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < ansis.Length)
							result = result.Replace(utf8Literals[index], ansis[index]);
					break;

				case 4:             // Unicode to Decimal
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < decimalUnicodes.Length)
							result = result.Replace(utf8Unicodes[index], decimalUnicodes[index]);
					result = result.Replace("Ð", "D");
					break;

				case 5:             // Unicode Composite to ANSI
					decodeLength = utf8UnicodeComposites.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < ansis.Length)
							result = result.Replace(utf8UnicodeComposites[index], ansis[index]);
					result = result.Replace("Ð", "D");
					break;

				case 6:             // Decimal to Unicode
					decodeLength = decimalUnicodes.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < utf8Unicodes.Length)
							result = result.Replace(decimalUnicodes[index], utf8Unicodes[index]);
					break;

				case 7:             // TCVN3 to Unicode
									// first, convert to decimal
					decodeLength = tcvn3s.Length;
					for (int index = 0; index < decodeLength; index++)
						if (!tcvn3s[index].Equals("") && index < decimalUnicodes.Length)
							result = result.Replace(tcvn3s[index], decimalUnicodes[index]);
					// and then, convert from decimal to unicode
					decodeLength = decimalUnicodes.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < utf8Unicodes.Length)
							result = result.Replace(decimalUnicodes[index], utf8Unicodes[index]);
					break;

				case 8:             // Unicode to Composite Unicode
					decodeLength = utf8Unicodes.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < utf8UnicodeComposites.Length)
							result = result.Replace(utf8Unicodes[index], utf8UnicodeComposites[index]);
					break;

				case 9:             // Composite Unicode to Unicode
					decodeLength = utf8UnicodeComposites.Length;
					for (int index = 0; index < decodeLength; index++)
						if (index < utf8Unicodes.Length)
							result = result.Replace(utf8UnicodeComposites[index], utf8Unicodes[index]);
					break;

				default:
					break;
			}

			return result.Replace("&nbsp;", " ");
		}

		/// <summary>
		/// Converts this Vietnamese string from UTF8 to Unicode
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertUTF8ToUnicode(this string @string)
		{
			return @string.ConvertVietnamese(0);
		}

		/// <summary>
		/// Converts this Vietnamese string from Unicode to UTF8
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertUnicodeToUTF8(this string @string)
		{
			return @string.ConvertVietnamese(1);
		}

		/// <summary>
		/// Converts this Vietnamese string from Unicode to ANSI
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertUnicodeToANSI(this string @string)
		{
			return @string.ConvertVietnamese(2).ConvertVietnamese(5);
		}

		/// <summary>
		/// Converts this Vietnamese string from Unicode to Decimal
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertUnicodeToDecimal(this string @string)
		{
			return @string.ConvertVietnamese(4);
		}

		/// <summary>
		/// Converts this Vietnamese string from Decimal to Unicode
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertDecimalToUnicode(this string @string)
		{
			return @string.ConvertVietnamese(6);
		}

		/// <summary>
		/// Converts this Vietnamese string from TCVN3 to Unicode
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertTCVN3ToUnicode(this string @string)
		{
			return @string.ConvertVietnamese(7);
		}

		/// <summary>
		/// Converts this Vietnamese string from Pre-composed Unicode to Composite Unicode
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertUnicodeToCompositeUnicode(this string @string)
		{
			return @string.ConvertVietnamese(8);
		}

		/// <summary>
		/// Converts this Vietnamese string from Composite Unicode to Pre-composed Unicode
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ConvertCompositeUnicodeToUnicode(this string @string)
		{
			return @string.ConvertVietnamese(9);
		}
		#endregion

		#region  Conversions (Uri)
		static Regex Normal = new Regex("[^a-zA-Z0-9_-]+");
		static Regex Numberic = new Regex("[^0-9]+");

		/// <summary>
		/// Generates the ANSI uri from this string (means remove all white spaces and special characters)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="toLowerCase">true to return lower case</param>
		/// <returns></returns>
		public static string GetANSIUri(this string @string, bool toLowerCase = true)
		{
			// convert Vietnamese characters
			var result = @string.Trim().ConvertUnicodeToANSI();

			// remove all special characters
			result = result.Replace(StringComparison.OrdinalIgnoreCase, "C#", "CSharp").Replace(" ", "-");
			result = StringService.Normal.Replace(result, "");

			// remove duplicate characters
			while (result.Contains("--"))
				result = result.Replace("--", "-");
			while (result.Contains("__"))
				result = result.Replace("__", "_");

			// normalize first & last characters
			while (result.StartsWith("-") || result.StartsWith("_"))
				result = result.Right(result.Length - 1);
			while (result.EndsWith("-") || result.EndsWith("_"))
				result = result.Left(result.Length - 1);

			// add timestamp if has no value
			if (result.Equals(""))
				result = "v" + DateTime.UtcNow.ToUnixTimestamp();

			// numeric
			else if (StringService.Numberic.Replace(result, "").Equals(result))
				result = "v" + DateTime.UtcNow.ToUnixTimestamp() + result;

			return toLowerCase
				? result.ToLower()
				: result;
		}

		/// <summary>
		/// Validate the ANSI uri
		/// </summary>
		/// <param name="ansiUri">The string that presents an ANSI uri to check</param>
		/// <returns>true if valid; otherwise false.</returns>
		public static bool IsValidANSIUri(this string ansiUri)
		{
			return string.IsNullOrWhiteSpace(ansiUri)
				? false
				: ansiUri.IsEquals(StringService.Normal.Replace(ansiUri, ""));
		}
		#endregion

	}
}