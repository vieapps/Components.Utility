#region Related components
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using SharpCompress.Common;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Writers;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Servicing methods for working with archiver (Zip/Tar)
	/// </summary>
	public static partial class ArchivingService
	{
		/// <summary>
		/// Archives a collection of files using ZIP
		/// </summary>
		/// <param name="files"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		public static void Zip(IEnumerable<FileInfo> files, string archiveFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null)
		{
			if (files == null || !files.Any())
				throw new ArgumentException("Source files are invalid", nameof(files));
			
			if (string.IsNullOrWhiteSpace(archiveFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(archiveFilePath));

			if (File.Exists(archiveFilePath))
				File.Delete(archiveFilePath);

			using (var archiver = ZipFile.Open(archiveFilePath, ZipArchiveMode.Create, encoding ?? Encoding.UTF8))
				files.Where(file => file.Exists).ForEach(file => archiver.CreateEntryFromFile(file.FullName, file.Name, compressionLevel));
		}

		/// <summary>
		/// Archives a collection of files using ZIP
		/// </summary>
		/// <param name="files"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		/// <param name="cancellationToken"></param>
		public static Task ZipAsync(IEnumerable<FileInfo> files, string archiveFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null, CancellationToken cancellationToken = default)
			=> UtilityService.ExecuteTask(() => ArchivingService.Zip(files, archiveFilePath, compressionLevel, encoding), cancellationToken);

		/// <summary>
		/// Archives a collection of files using ZIP
		/// </summary>
		/// <param name="files"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		public static void Zip(IEnumerable<string> files, string archiveFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null)
		{
			if (files == null || !files.Any())
				throw new ArgumentException("Source files are invalid", nameof(files));
			
			if (string.IsNullOrWhiteSpace(archiveFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(archiveFilePath));
			
			ArchivingService.Zip(files.Select(path => new FileInfo(path)), archiveFilePath, compressionLevel, encoding);
		}

		/// <summary>
		/// Archives a collection of files using ZIP
		/// </summary>
		/// <param name="files"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		/// <param name="cancellationToken"></param>
		public static Task ZipAsync(IEnumerable<string> files, string archiveFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null, CancellationToken cancellationToken = default)
			=> UtilityService.ExecuteTask(() => ArchivingService.Zip(files, archiveFilePath, compressionLevel, encoding), cancellationToken);

		/// <summary>
		/// Archives a directory (means all files) using ZIP
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		public static void Zip(string sourcePath, string archiveFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null)
		{
			if (!Directory.Exists(sourcePath))
				throw new ArgumentException("Source path to the directory for archiving is invalid", nameof(sourcePath));

			if (string.IsNullOrWhiteSpace(archiveFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(archiveFilePath));

			if (File.Exists(archiveFilePath))
				File.Delete(archiveFilePath);

			ZipFile.CreateFromDirectory(sourcePath, archiveFilePath, compressionLevel, false, encoding ?? Encoding.UTF8);
		}

		/// <summary>
		/// Archives a directory (means all files) using ZIP
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="encoding"></param>
		/// <param name="cancellationToken"></param>
		public static Task ZipAsync(string sourcePath, string archiveFilePath, CompressionLevel compressionLevel = CompressionLevel.Optimal, Encoding encoding = null, CancellationToken cancellationToken = default)
			=> UtilityService.ExecuteTask(() => ArchivingService.Zip(sourcePath, archiveFilePath, compressionLevel, encoding), cancellationToken);

		/// <summary>
		/// UnArchives to a directory using ZIP
		/// </summary>
		/// <param name="archiveFilePath"></param>
		/// <param name="destinationPath"></param>
		public static void Unzip(string archiveFilePath, string destinationPath)
		{
			if (string.IsNullOrWhiteSpace(archiveFilePath))
				throw new ArgumentException("Path of .ZIP file is invalid", nameof(archiveFilePath));

			if (!Directory.Exists(destinationPath))
				throw new ArgumentException("Destination path to the directory for unarchiving is invalid", nameof(destinationPath));

			ZipFile.ExtractToDirectory(archiveFilePath, destinationPath);
		}

		/// <summary>
		/// UnArchives to a directory using ZIP
		/// </summary>
		/// <param name="archiveFilePath"></param>
		/// <param name="destinationPath"></param>
		/// <param name="cancellationToken"></param>
		public static Task UnzipAsync(string archiveFilePath, string destinationPath, CancellationToken cancellationToken = default)
			=> UtilityService.ExecuteTask(() => ArchivingService.Unzip(archiveFilePath, destinationPath), cancellationToken);

		/// <summary>
		/// Archives a directory (means all files) using TAR/ZSTD
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="includeSubDirectories"></param>
		/// <param name="compressionLevel"></param>
		/// <exception cref="ArgumentException"></exception>
		public static void Tar(string sourcePath, string archiveFilePath, bool includeSubDirectories = true, int compressionLevel = 10)
		{
			if (!Directory.Exists(sourcePath))
				throw new ArgumentException("Source path to the directory for archiving is invalid", nameof(sourcePath));

			var files = Directory.EnumerateFiles(sourcePath, "*.*", includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
				.OrderBy(filePath =>
				{
					var ext = Path.GetExtension(filePath).ToLowerInvariant();
					return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp" || ext == ".mp4" || ext == ".mkv" || ext == ".avi" || ext == ".zip" || ext == ".rar" || ext == ".7z" ? 1 : 0;
				})
				.Select(filePath => new FileInfo(filePath))
				.ToList();

			using (var fileStream = File.Create(archiveFilePath))
			using (var zstdStream = new ZstdSharp.CompressionStream(fileStream, level: compressionLevel, leaveOpen: false))
			using (var archiver = TarArchive.Create())
			{
				files.ForEach(file => archiver.AddEntry(file.Name, file.FullName));
				archiver.SaveTo(zstdStream, new WriterOptions(CompressionType.None) { LeaveStreamOpen = false });
			}
		}

		/// <summary>
		/// Archives a directory (means all files) using TAR/ZSTD
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="archiveFilePath"></param>
		/// <param name="includeSubDirectories"></param>
		/// <param name="compressionLevel"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task TarAsync(string sourcePath, string archiveFilePath, bool includeSubDirectories = true, int compressionLevel = 10, CancellationToken cancellationToken = default)
		{
			if (!Directory.Exists(sourcePath))
				throw new ArgumentException("Source path to the directory for archiving is invalid", nameof(sourcePath));

			var files = Directory.EnumerateFiles(sourcePath, "*.*", includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
				.OrderBy(filePath =>
				{
					var ext = Path.GetExtension(filePath).ToLowerInvariant();
					return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp" || ext == ".mp4" || ext == ".mkv" || ext == ".avi" || ext == ".zip" || ext == ".rar" || ext == ".7z" ? 1 : 0;
				})
				.Select(filePath => new FileInfo(filePath))
				.ToList();

			using (var fileStream = File.Create(archiveFilePath))
			using (var zstdStream = new ZstdSharp.CompressionStream(fileStream, level: compressionLevel, leaveOpen: false))
			using (var archiver = TarArchive.Create())
			{
				files.ForEach(file => archiver.AddEntry(file.Name, file.FullName));
				await archiver.SaveToAsync(zstdStream, new WriterOptions(CompressionType.None) { LeaveStreamOpen = false }, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// UnArchives to a directory using TAR/ZSTD
		/// </summary>
		/// <param name="archiveFilePath"></param>
		/// <param name="destinationPath"></param>
		/// <exception cref="ArgumentException"></exception>
		public static void UnTar(string archiveFilePath, string destinationPath)
		{
			if (string.IsNullOrWhiteSpace(archiveFilePath))
				throw new ArgumentException("Path of .TAR.ZST file is invalid", nameof(archiveFilePath));

			if (!Directory.Exists(destinationPath))
				throw new ArgumentException("Destination path to the directory for unarchiving is invalid", nameof(destinationPath));

			using (var fileStream = File.OpenRead(archiveFilePath))
			using (var zstdStream = new ZstdSharp.DecompressionStream(fileStream))
			using (var archiver = TarArchive.Open(zstdStream))
				foreach (var entry in archiver.Entries)
				{
					if (!entry.IsDirectory)
						entry.WriteToDirectory(destinationPath, new ExtractionOptions
						{
							ExtractFullPath = true,
							Overwrite = true
						});
				}
		}

		/// <summary>
		/// UnArchives to a directory using TAR/ZSTD
		/// </summary>
		/// <param name="archiveFilePath"></param>
		/// <param name="destinationPath"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task UnTarAsync(string archiveFilePath, string destinationPath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(archiveFilePath))
				throw new ArgumentException("Path of .TAR.ZST file is invalid", nameof(archiveFilePath));

			if (!Directory.Exists(destinationPath))
				throw new ArgumentException("Destination path to the directory for unarchiving is invalid", nameof(destinationPath));

			using (var fileStream = File.OpenRead(archiveFilePath))
			using (var zstdStream = new ZstdSharp.DecompressionStream(fileStream))
			using (var archiver = TarArchive.Open(zstdStream))
				foreach (var entry in archiver.Entries)
				{
					if (!entry.IsDirectory)
						await entry.WriteToDirectoryAsync(destinationPath, new ExtractionOptions
						{
							ExtractFullPath = true,
							Overwrite = true
						}, cancellationToken).ConfigureAwait(false);
				}
		}
	}
}