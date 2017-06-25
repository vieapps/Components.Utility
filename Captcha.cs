#region Related components
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web;
#endregion

namespace net.vieapps.Components.Utility
{
	public static class CaptchaService
	{

		#region Generate new code & validate
		/// <summary>
		/// Generates new code of the captcha
		/// </summary>
		/// <param name="salt">The string to use as salt</param>
		/// <returns>The encrypted string that contains code of captcha</returns>
		public static string GenerateCode(string salt)
		{
			return (DateTime.Now.ToUnixTimestamp().ToString() + (string.IsNullOrWhiteSpace(salt) ? "" : "-" + salt) + "-" + CaptchaService.GenerateRandomCode()).Encrypt(CryptoService.DefaultEncryptionKey, true);
		}

		/// <summary>
		/// Validates captcha code
		/// </summary>
		/// <param name="captchaCode">The string that presents encrypted code</param>
		/// <param name="inputCode">The code that inputed by user</param>
		/// <returns>true if valid</returns>
		public static bool IsCodeValid(string captchaCode, string inputCode)
		{
			if (string.IsNullOrWhiteSpace(captchaCode) || string.IsNullOrWhiteSpace(inputCode))
				return false;

			string code = "";
			try
			{
				code = captchaCode.Decrypt(CryptoService.DefaultEncryptionKey, true);
				string[] codes = code.ToArray('-');
				code = codes[codes.Length - 1];

				DateTime datetime = Convert.ToInt64(codes[0]).FromUnixTimestamp(false);
				if ((DateTime.Now - datetime).TotalMinutes > 5.0)
					return false;
			}
			catch
			{
				return false;
			}

			return code.IsEquals(inputCode.Trim());
		}
		#endregion

		#region Generate captcha image and flush into HttpResponse object
		/// <summary>
		/// Generates captcha images
		/// </summary>
		/// <param name="output">The HttResponse object to export captcha image to</param>
		/// <param name="code">The string that presents encrypted code for generating</param>
		public static void GenerateCaptchaImage(HttpResponse output, string code)
		{
			CaptchaService.GenerateCaptchaImage(output, code, true, null);
		}

		/// <summary>
		/// Generates captcha images
		/// </summary>
		/// <param name="output">The HttResponse object to export captcha image to</param>
		/// <param name="code">The string that presents encrypted code for generating</param>
		/// <param name="useSmallImage">true to use small image</param>
		/// <param name="noises">The collection of noise texts</param>
		public static void GenerateCaptchaImage(HttpResponse output, string code, bool useSmallImage, List<string> noises)
		{
			// check code
			if (!code.Equals(""))
			{
				try
				{
					code = code.Decrypt(CryptoService.DefaultEncryptionKey, true);
					string[] codes = code.ToArray('-');
					code = codes[codes.Length - 1];
					string tempCode = "";
					string space = " ";
					string spaceP = "";
					if (code.Length <= 5 && !useSmallImage)
						spaceP = "  ";
					else if (useSmallImage)
						space = "";
					for (int index = 0; index < code.Length; index++)
						tempCode += spaceP + code[index].ToString() + space;
					code = tempCode;
				}
				catch
				{
					code = "I-n-valid";
				}
			}
			else
				code = "Invalid";

			// prepare sizae of security image
			int width = 220, height = 48;
			if (useSmallImage)
			{
				width = 110;
				height = 24;
			}

			// create new graphic from the bitmap with random background color
			Color[] backgroundColors = new Color[] { Color.Orange, Color.Thistle, Color.LightSeaGreen, Color.Violet, Color.Yellow, Color.YellowGreen, Color.NavajoWhite, Color.LightGray, Color.Tomato, Color.LightGreen, Color.White };
			if (useSmallImage)
				backgroundColors = new Color[] { Color.Orange, Color.Thistle, Color.LightSeaGreen, Color.Yellow, Color.YellowGreen, Color.NavajoWhite, Color.White };

			Bitmap securityBitmap = CreateBackroundImage(width, height, new Color[] { backgroundColors[Utility.GetRandomNumber(0, backgroundColors.Length)], backgroundColors[Utility.GetRandomNumber(0, backgroundColors.Length)], backgroundColors[Utility.GetRandomNumber(0, backgroundColors.Length)], backgroundColors[Utility.GetRandomNumber(0, backgroundColors.Length)] });
			Graphics securityGraph = Graphics.FromImage(securityBitmap);
			securityGraph.SmoothingMode = SmoothingMode.AntiAlias;

			// add noise texts (for big image)
			if (!useSmallImage)
			{
				// comuting noise texts for the image
				List<string> texts =
					noises != null && noises.Count > 0
					? noises
					: new List<string>() { "VIEApps", "vieapps.net", "VIEApps REST", "Tyrion Q. Nguyen" };

				List<string> noiseTexts = new List<string>() { "Winners never quit", "Quitters never win", "Vietnam - The hidden charm", "Don't be evil", "Keep moving", "Connecting People", "Information at your fingertips", "No sacrifice no victory", "No paint no gain", "Enterprise Web Services", "On-Demand Services for Enterprise", "Cloud Computing Enterprise Services", "Where do you want to go today?", "Make business easier", "Simplify business process", "VIEApps", "vieapps.net" };
				noiseTexts.Append(texts);

				string noiseText = noiseTexts[Utility.GetRandomNumber(0, noiseTexts.Count)];
				noiseText += " " + noiseText + " " + noiseText + " " + noiseText;

				// write noise texts
				securityGraph.DrawString(noiseTexts[Utility.GetRandomNumber(0, noiseTexts.Count)] + " - " + noiseTexts[Utility.GetRandomNumber(0, noiseTexts.Count)], new Font("Verdana", 10, FontStyle.Underline), new SolidBrush(Color.White), new PointF(0, 3));
				securityGraph.DrawString(noiseTexts[Utility.GetRandomNumber(0, noiseTexts.Count)] + " - " + noiseTexts[Utility.GetRandomNumber(0, noiseTexts.Count)], new Font("Verdana", 12, FontStyle.Bold), new SolidBrush(Color.White), new PointF(5, 7));
				securityGraph.DrawString(noiseTexts[Utility.GetRandomNumber(0, noiseTexts.Count)] + " - " + noiseTexts[Utility.GetRandomNumber(0, noiseTexts.Count)], new Font("Arial", 11, FontStyle.Italic), new SolidBrush(Color.White), new PointF(-5, 20));
				securityGraph.DrawString(noiseText, new Font("Arial", 12, FontStyle.Bold), new SolidBrush(Color.White), new PointF(20, 28));
			}

			// add noise lines (for small image)
			else
			{
				// randrom index to make noise lines
				int randomIndex = Utility.GetRandomNumber(0, backgroundColors.Length);

				// first two lines
				Pen noisePen = new Pen(new SolidBrush(Color.Gray), 2);
				securityGraph.DrawLine(noisePen, new Point(width, randomIndex), new Point(randomIndex, height / 2 - randomIndex));
				securityGraph.DrawLine(noisePen, new Point(width / 3 - randomIndex, randomIndex), new Point(width / 2 + randomIndex, height - randomIndex));

				// second two lines
				noisePen = new Pen(new SolidBrush(Color.Yellow), 1);
				securityGraph.DrawLine(noisePen, new Point(((width / 4) * 3) - randomIndex, randomIndex), new Point(width / 3 + randomIndex, height - randomIndex));
				if (randomIndex % 2 == 1)
					securityGraph.DrawLine(noisePen, new Point(width - randomIndex * 2, randomIndex), new Point(randomIndex, height - randomIndex));
				else
					securityGraph.DrawLine(noisePen, new Point(randomIndex, randomIndex), new Point(width - randomIndex * 2, height - randomIndex));

				// third two lines
				randomIndex = Utility.GetRandomNumber(0, backgroundColors.Length);
				noisePen = new Pen(new SolidBrush(Color.Magenta), 1);
				securityGraph.DrawLine(noisePen, new Point(((width / 6) * 3) - randomIndex, randomIndex), new Point(width / 5 + randomIndex, height - randomIndex + 3));
				if (randomIndex % 2 == 1)
					securityGraph.DrawLine(noisePen, new Point(width - randomIndex * 2, randomIndex - 1), new Point(randomIndex, height - randomIndex - 3));
				else
					securityGraph.DrawLine(noisePen, new Point(randomIndex, randomIndex + 1), new Point(width - randomIndex * 2, height - randomIndex + 4));

				// fourth two lines
				randomIndex = Utility.GetRandomNumber(0, backgroundColors.Length);
				noisePen = new Pen(new SolidBrush(backgroundColors[Utility.GetRandomNumber(0, backgroundColors.Length)]), 1);
				securityGraph.DrawLine(noisePen, new Point(((width / 10) * 3) - randomIndex, randomIndex), new Point(width / 6 + randomIndex, height - randomIndex + 3));
				if (randomIndex % 2 == 1)
					securityGraph.DrawLine(noisePen, new Point(width - randomIndex * 3, randomIndex - 2), new Point(randomIndex, height - randomIndex - 2));
				else
					securityGraph.DrawLine(noisePen, new Point(randomIndex, randomIndex + 2), new Point(width - randomIndex * 3, height - randomIndex + 2));
			}

			// put the security code into the image with random font and brush
			string[] fonts = new string[] {
					"Verdana", "Arial", "Times New Roman", "Courier", "Courier New", "Comic Sans MS"
				};

			Brush[] brushs = new Brush[] {
				new SolidBrush(Color.Black), new SolidBrush(Color.Blue), new SolidBrush(Color.DarkBlue), new SolidBrush(Color.DarkGreen),
				new SolidBrush(Color.Magenta), new SolidBrush(Color.Red), new SolidBrush(Color.DarkRed), new SolidBrush(Color.Black),
				new SolidBrush(Color.Firebrick), new SolidBrush(Color.DarkGreen), new SolidBrush(Color.Green), new SolidBrush(Color.DarkViolet)
			};

			if (useSmallImage)
			{
				int step = 0;
				for (int index = 0; index < code.Length; index++)
				{
					float x = (index * 7) + step + Utility.GetRandomNumber(-1, 9);
					float y = Utility.GetRandomNumber(-2, 0);

					string writtenCode = code.Substring(index, 1);
					if (writtenCode.Equals("I") || (Utility.GetRandomNumber() % 2 == 1 && !writtenCode.Equals("L")))
						writtenCode = writtenCode.ToLower();

					int addedX = Utility.GetRandomNumber(-3, 5);
					securityGraph.DrawString(writtenCode, new Font(fonts[Utility.GetRandomNumber(0, fonts.Length)], Utility.GetRandomNumber(13, 19), FontStyle.Bold), brushs[Utility.GetRandomNumber(0, brushs.Length)], new PointF(x + addedX, y));
					step += Utility.GetRandomNumber(13, 23);
				}
			}
			else
			{
				// write code
				int step = 0;
				for (int index = 0; index < code.Length; index++)
				{
					string font = fonts[Utility.GetRandomNumber(0, fonts.Length)];
					float x = 2 + step, y = 10;
					step += 9;
					float fontSize = 15;
					if (index > 1 && index < 4)
					{
						fontSize = 25;
						x -= 10;
						y -= 5;
					}
					else if (index > 3 && index < 6)
					{
						y -= Utility.GetRandomNumber(3, 5);
						fontSize += index;
						step += index / 5;
						if (index == 4)
						{
							if (Utility.GetRandomNumber() % 2 == 1)
								y += Utility.GetRandomNumber(8, 12);
							else if (Utility.GetRandomNumber() % 2 == 2)
							{
								y -= Utility.GetRandomNumber(2, 6);
								fontSize += Utility.GetRandomNumber(1, 4);
							}
						}
					}
					else if (index > 5)
					{
						x += Utility.GetRandomNumber(0, 4);
						y -= Utility.GetRandomNumber(0, 4);
						fontSize += index - 7;
						step += index / 3 + 1;
						if (index == 10)
						{
							if (Utility.GetRandomNumber() % 2 == 1)
								y += Utility.GetRandomNumber(7, 14);
							else if (Utility.GetRandomNumber() % 2 == 2)
							{
								y -= Utility.GetRandomNumber(1, 3);
								fontSize += Utility.GetRandomNumber(2, 5);
							}
						}
					}
					string writtenCode = code.Substring(index, 1);
					if (writtenCode.Equals("I") || (Utility.GetRandomNumber() % 2 == 1 && !writtenCode.Equals("L")))
						writtenCode = writtenCode.ToLower();
					securityGraph.DrawString(writtenCode, new Font(font, fontSize, FontStyle.Bold), brushs[Utility.GetRandomNumber(0, brushs.Length)], new PointF(x + 2, y + 2));
					securityGraph.DrawString(writtenCode, new Font(font, fontSize, FontStyle.Bold), brushs[Utility.GetRandomNumber(0, brushs.Length)], new PointF(x, y));
				}

				// fill it randomly with pixels
				int maxX = width, maxY = height, startX = 0, startY = 0;
				int random = Utility.GetRandomNumber(1, 100);
				if (random > 80)
				{
					maxX -= maxX / 3;
					maxY = maxY / 2;
				}
				else if (random > 60)
				{
					startX = maxX / 3;
					startY = maxY / 2;
				}
				else if (random > 30)
				{
					startX = maxX / 7;
					startY = maxY / 4;
					maxX -= maxX / 5;
					maxY -= maxY / 8;
				}

				for (int iX = startX; iX < maxX; iX++)
					for (int iY = startY; iY < maxY; iY++)
						if ((iX % 3 == 1) && (iY % 4 == 1))
							securityBitmap.SetPixel(iX, iY, Color.DarkGray);
			}

			// add random noise into image (use SIN)
			double divideTo = 64.0d + Utility.GetRandomNumber(1, 10);
			int distortion = Utility.GetRandomNumber(5, 11);
			if (useSmallImage)
				distortion = Utility.GetRandomNumber(1, 5);

			Bitmap noisedBitmap = new Bitmap(width, height, PixelFormat.Format16bppRgb555);
			for (int y = 0; y < height; y++)
				for (int x = 0; x < width; x++)
				{
					int newX = (int)(x + (distortion * Math.Sin(Math.PI * y / divideTo)));
					if (newX < 0 || newX >= width)
						newX = 0;

					int newY = (int)(y + (distortion * Math.Cos(Math.PI * x / divideTo)));
					if (newY < 0 || newY >= height)
						newY = 0;

					noisedBitmap.SetPixel(x, y, securityBitmap.GetPixel(newX, newY));
				}

			// export as JPEG image
			output.Cache.SetNoStore();
			output.ContentType = "image/jpeg";
			output.Clear();
			noisedBitmap.Save(output.OutputStream, ImageFormat.Jpeg);

			// destroy temporary objects
			securityGraph.Dispose();
			securityBitmap.Dispose();
			noisedBitmap.Dispose();
		}

		static Bitmap CreateBackroundImage(int width, int height, Color[] backgroundColors)
		{
			// create element bitmaps
			int bmpWidth = Utility.GetRandomNumber(Utility.GetRandomNumber(5, 10), Utility.GetRandomNumber(20, width / 2));
			int bmpHeight = Utility.GetRandomNumber(height / 4, height / 2);
			if (height > 20)
				bmpHeight = Utility.GetRandomNumber(Utility.GetRandomNumber(1, 10), Utility.GetRandomNumber(12, height));
			Bitmap bitmap1 = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format16bppRgb555);
			Graphics graph = Graphics.FromImage(bitmap1);
			graph.SmoothingMode = SmoothingMode.AntiAlias;
			graph.Clear(backgroundColors[0]);

			bmpWidth = Utility.GetRandomNumber(Utility.GetRandomNumber(15, width / 3), Utility.GetRandomNumber(width / 3, width / 2));
			bmpHeight = Utility.GetRandomNumber(5, height / 3);
			if (height > 20)
				bmpHeight = Utility.GetRandomNumber(Utility.GetRandomNumber(5, height / 4), Utility.GetRandomNumber(height / 4, height / 2));
			Bitmap bitmap2 = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format16bppRgb555);
			graph = Graphics.FromImage(bitmap2);
			graph.SmoothingMode = SmoothingMode.AntiAlias;
			graph.Clear(backgroundColors[1]);

			bmpWidth = Utility.GetRandomNumber(Utility.GetRandomNumber(width / 4, width / 2), Utility.GetRandomNumber(width / 2, width));
			bmpHeight = Utility.GetRandomNumber(height / 2, height);
			if (height > 20)
				bmpHeight = Utility.GetRandomNumber(Utility.GetRandomNumber(height / 5, height / 2), Utility.GetRandomNumber(height / 2, height));
			Bitmap bitmap3 = new Bitmap(bmpWidth, bmpHeight, PixelFormat.Format16bppRgb555);
			graph = Graphics.FromImage(bitmap3);
			graph.SmoothingMode = SmoothingMode.AntiAlias;
			graph.Clear(backgroundColors[2]);

			Bitmap backroundBitmap = new Bitmap(width, height, PixelFormat.Format16bppRgb555);
			graph = Graphics.FromImage(backroundBitmap);
			graph.SmoothingMode = SmoothingMode.AntiAlias;
			graph.Clear(backgroundColors[3]);
			graph.DrawImage(bitmap1, Utility.GetRandomNumber(0, width / 2), Utility.GetRandomNumber(0, height / 2));
			graph.DrawImage(bitmap2, Utility.GetRandomNumber(width / 5, width / 2), Utility.GetRandomNumber(height / 5, height / 2));
			graph.DrawImage(bitmap3, Utility.GetRandomNumber(width / 4, width / 3), Utility.GetRandomNumber(0, height / 3));
			
			return backroundBitmap;
		}
		#endregion

		#region Generate random code
		/// <summary>
		/// Generates random code for using with captcha or other purpose
		/// </summary>
		/// <param name="useShortCode">true to use short-code</param>
		/// <param name="useHex">true to use hexa in code</param>
		/// <returns>The string that presents random code</returns>
		public static string GenerateRandomCode(bool useShortCode = true, bool useHex = false)
		{
			string code = Utility.GetUUID();
			int length = 9;
			if (useShortCode)
				length = 4;

			if (!useHex)
			{
				code = Utility.GetRandomNumber(1000).ToString() + Utility.GetRandomNumber(1000).ToString();
				while (code.Length < length + 5)
					code += Utility.GetRandomNumber(1000).ToString();
			}

			int index = Utility.GetRandomNumber(0, code.Length);
			if (index > code.Length - length)
				index = code.Length - length;
			code = code.Substring(index, length);

			string random1 = ((char)Utility.GetRandomNumber(48, 57)).ToString();
			string replacement = "O";
			while (replacement.Equals("O"))
				replacement = ((char)Utility.GetRandomNumber(71, 90)).ToString();
			code = code.Replace(random1, replacement);

			if (length > 4)
			{
				string random2 = random1;
				while (random2.Equals(random1))
					random2 = ((char)Utility.GetRandomNumber(48, 57)).ToString();
				replacement = "O";
				while (replacement.Equals("O"))
					replacement = ((char)Utility.GetRandomNumber(71, 90)).ToString();
				code = code.Replace(random2, replacement);

				string random3 = random1;
				while (random3.Equals(random1))
				{
					random3 = ((char)Utility.GetRandomNumber(48, 57)).ToString();
					if (random3.Equals(random2))
						random3 = ((char)Utility.GetRandomNumber(48, 57)).ToString();
				}
				replacement = "O";
				while (replacement.Equals("O"))
					replacement = ((char)Utility.GetRandomNumber(71, 90)).ToString();
				code = code.Replace(random3, replacement);
			}

			bool hasNumber = false, hasChar = false;
			for (int charIndex = 0; charIndex < code.Length; charIndex++)
			{
				if (code[charIndex] >= '0' && code[charIndex] <= '9')
					hasNumber = true;
				if (code[charIndex] >= 'A' && code[charIndex] <= 'Z')
					hasChar = true;
				if (hasNumber && hasChar)
					break;
			}

			if (!hasNumber)
				code += ((char)Utility.GetRandomNumber(48, 57)).ToString();

			if (!hasChar)
			{
				replacement = "O";
				while (replacement.Equals("O"))
					replacement = ((char)Utility.GetRandomNumber(65, 90)).ToString();
				code += replacement;
			}

			return code.Right(length);
		}
		#endregion

	}
}