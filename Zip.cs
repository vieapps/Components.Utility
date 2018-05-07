#region Related components
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Servicing methods for working with ZIP archive
	/// </summary>
	public static partial class ZipService
	{
		/// <summary>
		/// Zips a collection of files
		/// </summary>
		/// <param name="files"></param>
		/// <param name="zipFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		public static void Zip(IEnumerable<FileInfo> files, string zipFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null)
		{
			if (files == null || files.Count() < 1)
				throw new ArgumentException("Source files are invalid", nameof(files));
			else if (string.IsNullOrWhiteSpace(zipFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(zipFilePath));

			if (File.Exists(zipFilePath))
				File.Delete(zipFilePath);

			using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create, encoding ?? Encoding.UTF8))
			{
				files.Where(file => file.Exists).ForEach(file => zipArchive.CreateEntryFromFile(file.FullName, file.Name, compressionLevel));
			}
		}

		/// <summary>
		/// Zips a collection of files
		/// </summary>
		/// <param name="files"></param>
		/// <param name="zipFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		/// <param name="cancellationToken"></param>
		public static async Task ZipAsync(IEnumerable<FileInfo> files, string zipFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			await UtilityService.ExecuteTask(() => ZipService.Zip(files, zipFilePath, compressionLevel, encoding), cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Zips a collection of files
		/// </summary>
		/// <param name="files"></param>
		/// <param name="zipFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		public static void Zip(IEnumerable<string> files, string zipFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null)
		{
			if (files == null || files.Count() < 1)
				throw new ArgumentException("Source files are invalid", nameof(files));
			else if (string.IsNullOrWhiteSpace(zipFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(zipFilePath));
			ZipService.Zip(files.Select(path => new FileInfo(path)), zipFilePath, compressionLevel, encoding);
		}

		/// <summary>
		/// Zips a collection of files
		/// </summary>
		/// <param name="files"></param>
		/// <param name="zipFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		/// <param name="cancellationToken"></param>
		public static async Task ZipAsync(IEnumerable<string> files, string zipFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			await UtilityService.ExecuteTask(() => ZipService.Zip(files, zipFilePath, compressionLevel, encoding), cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Zips a folder (with all child contents)
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="zipFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		public static void Zip(string sourcePath, string zipFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null)
		{
			if (!Directory.Exists(sourcePath))
				throw new ArgumentException("Source path to the folder for zipping is invalid", nameof(sourcePath));
			else if (string.IsNullOrWhiteSpace(zipFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(zipFilePath));

			if (File.Exists(zipFilePath))
				File.Delete(zipFilePath);

			ZipFile.CreateFromDirectory(sourcePath, zipFilePath, compressionLevel, false, encoding ?? Encoding.UTF8);
		}

		/// <summary>
		/// Zips a folder (with all child contents)
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="zipFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		/// <param name="cancellationToken"></param>
		public static async Task ZipAsync(string sourcePath, string zipFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
		{
			await UtilityService.ExecuteTask(() => ZipService.Zip(sourcePath, zipFilePath, compressionLevel, encoding), cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Unzips a. ZIP archive file to a folder
		/// </summary>
		/// <param name="zipFilePath"></param>
		/// <param name="destinationPath"></param>
		public static void Unzip(string zipFilePath, string destinationPath)
		{
			if (string.IsNullOrWhiteSpace(zipFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(zipFilePath));
			else if (!Directory.Exists(destinationPath))
				throw new ArgumentException("Destination path to the folder for unzipping is invalid", nameof(destinationPath));
			ZipFile.ExtractToDirectory(zipFilePath, destinationPath);
		}

		/// <summary>
		/// Unzips a. ZIP archive file to a folder
		/// </summary>
		/// <param name="zipFilePath"></param>
		/// <param name="destinationPath"></param>
		/// <param name="cancellationToken"></param>
		public static async Task UnzipAsync(string zipFilePath, string destinationPath, CancellationToken cancellationToken = default(CancellationToken))
		{
			await UtilityService.ExecuteTask(() => ZipService.Unzip(zipFilePath, destinationPath), cancellationToken).ConfigureAwait(false);
		}
	}
}