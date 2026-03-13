#region Related components
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Newtonsoft.Json.Linq;
#endregion

#if !SIGN
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("VIEApps.Components.XUnitTests")]
#endif

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Utility servicing methods
	/// </summary>
	public static partial class UtilityService
	{

		#region UUID
		/// <summary>
		/// Gets an UUID (128 bits unique universal identity)
		/// </summary>
		/// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
		/// <param name="uuid">The string that presents an UUID</param>
		/// <param name="format">The string that presents the format</param>
		/// <returns>The string that presents an 128 bits UUID</returns>
		public static string GetUUID(bool asBase64Url = false, string uuid = null, string format = null)
		{
			var guid = string.IsNullOrWhiteSpace(uuid) ? Guid.NewGuid() : new Guid(uuid.Trim());
			return asBase64Url ? guid.ToByteArray().ToBase64Url() : guid.ToString(format ?? "N");
		}

		/// <summary>
		/// Gets an UUID (128 bits unique universal identity)
		/// </summary>
		/// <param name="uuid">The array of bytes that presents an UUID</param>
		/// <param name="format">The string that presents the format</param>
		/// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
		/// <returns>The string that presents an 128 bits UUID</returns>
		public static string GetUUID(string uuid, string format = null, bool asBase64Url = false)
			=> UtilityService.GetUUID(asBase64Url, uuid, format);

		/// <summary>
		/// Gets an UUID (128 bits unique universal identity)
		/// </summary>
		/// <param name="uuid">The array of bytes that presents an UUID</param>
		/// <param name="format">The string that presents the format</param>
		/// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
		/// <returns>The string that presents an 128 bits UUID</returns>
		public static string GetUUID(byte[] uuid, string format = null, bool asBase64Url = false)
		{
			var guid = uuid == null || uuid.Length < 1 ? Guid.NewGuid() : new Guid(uuid.Take(16));
			return asBase64Url ? guid.ToByteArray().ToBase64Url() : guid.ToString(format ?? "N");
		}

		/// <summary>
		/// Generate an identity as UUID-format from this string
		/// </summary>
		/// <param name="string"></param>
		/// <param name="format">The string that presents the format</param>
		/// <param name="mode">BLAKE or MD5</param>
		/// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
		/// <returns>The string that presents an 128 bits UUID</returns>
		public static string GenerateUUID(this string @string, string format = null, string mode = null, bool asBase64Url = false)
			=> string.IsNullOrWhiteSpace(@string)
				? UtilityService.GetUUID(asBase64Url, null, format)
				: UtilityService.GetUUID(@string.GetHash(!string.IsNullOrWhiteSpace(mode) && mode.IsStartsWith("blake") ? "blake128" : "md5"), format, asBase64Url);

		/// <summary>
		/// Generate an identity as UUID-format from this array of bytes
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="format">The string that presents the format</param>
		/// <param name="mode">BLAKE or MD5</param>
		/// <param name="asBase64Url">Set to true to encoded as base64-url string</param>
		/// <returns>The string that presents an 128 bits UUID</returns>
		public static string GenerateUUID(this byte[] bytes, string format = null, string mode = null, bool asBase64Url = false)
			=> UtilityService.GetUUID(bytes?.GetHash(!string.IsNullOrWhiteSpace(mode) && mode.IsStartsWith("blake") ? "blake128" : "md5"), format, asBase64Url);

		/// <summary>
		/// Gets a new UUID (universal unique identity - 128 bits or 32 hexa-characters)
		/// </summary>
		public static string NewUUID => UtilityService.GetUUID();

		static string _BlankUUID = null;

		/// <summary>
		/// Gets the blank UUID
		/// </summary>
		/// <returns></returns>
		public static string BlankUUID => UtilityService._BlankUUID ?? (UtilityService._BlankUUID = new string('0', 32));

		static Regex HexRegex { get; } = new Regex("[^0-9a-fA-F]+");

		/// <summary>
		/// Validates the UUID string
		/// </summary>
		/// <param name="uuid"></param>
		/// <param name="onlyHex">true to only allow hexa characters</param>
		/// <returns>true if it is valid; otherwise false.</returns>
		public static bool IsValidUUID(this string uuid, bool onlyHex = true)
			=> !string.IsNullOrWhiteSpace(uuid) && uuid.Length.Equals(32) && (onlyHex ? UtilityService.HexRegex.Replace(uuid, "").Equals(uuid) : !uuid.Contains(" ") && !uuid.Contains(";"));
		#endregion

		#region Random number & code
		static Random Random { get; } = new Random();

		/// <summary>
		/// Gets the random number between min and max
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static int GetRandomNumber(int min = 0, int max = Int32.MaxValue)
			=> UtilityService.Random.Next(min, max);

		static RandomBigInteger RandomBigInteger { get; } = new RandomBigInteger();

		/// <summary>
		/// Gets the random of big integer number
		/// </summary>
		/// <param name="length">The number of random bits to generate.</param>
		/// <returns></returns>
		public static BigInteger GetRandomNumber(int length)
			=> UtilityService.RandomBigInteger.Next(length);

		/// <summary>
		/// Gets a random code
		/// </summary>
		/// <param name="useShortCode">true to use short-code</param>
		/// <param name="useHex">true to use hexa in code</param>
		/// <returns>The string that presents random code</returns>
		public static string GetRandomCode(bool useShortCode = true, bool useHex = false)
		{
			var code = UtilityService.GetUUID();
			var length = 9;
			if (useShortCode)
				length = 4;

			if (!useHex)
			{
				code = UtilityService.GetRandomNumber(1000).ToString() + UtilityService.GetRandomNumber(1000).ToString();
				while (code.Length < length + 5)
					code += UtilityService.GetRandomNumber(1000).ToString();
			}

			var index = UtilityService.GetRandomNumber(0, code.Length);
			if (index > code.Length - length)
				index = code.Length - length;
			code = code.Substring(index, length);

			var random1 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
			var replacement = "O";
			while (replacement.Equals("O"))
				replacement = ((char)UtilityService.GetRandomNumber(71, 90)).ToString();
			code = code.Replace(random1, replacement);

			if (length > 4)
			{
				var random2 = random1;
				while (random2.Equals(random1))
					random2 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
				replacement = "O";
				while (replacement.Equals("O"))
					replacement = ((char)UtilityService.GetRandomNumber(71, 90)).ToString();
				code = code.Replace(random2, replacement);

				var random3 = random1;
				while (random3.Equals(random1))
				{
					random3 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
					if (random3.Equals(random2))
						random3 = ((char)UtilityService.GetRandomNumber(48, 57)).ToString();
				}
				replacement = "O";
				while (replacement.Equals("O"))
					replacement = ((char)UtilityService.GetRandomNumber(71, 90)).ToString();
				code = code.Replace(random3, replacement);
			}

			var hasNumber = false;
			var hasChar = false;
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
				code += ((char)UtilityService.GetRandomNumber(48, 57)).ToString();

			if (!hasChar)
			{
				replacement = "O";
				while (replacement.Equals("O"))
					replacement = ((char)UtilityService.GetRandomNumber(65, 90)).ToString();
				code += replacement;
			}

			return code.Right(length);
		}
		#endregion

		#region Task extensions
		/// <summary>
		/// Executes an action in the thread pool with cancellation supported
		/// </summary>
		/// <param name="action">The action to run in the thread pool</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="creationOptions">The options that controls the behavior of the created task</param>
		/// <param name="scheduler">The scheduler that is used to schedule the created task</param>
		/// <returns>An awaitable task</returns>
		public static Task ExecuteTask(Action action, CancellationToken cancellationToken = default, TaskCreationOptions creationOptions = TaskCreationOptions.DenyChildAttach, TaskScheduler scheduler = null)
			=> Task.Factory.StartNew(action, cancellationToken, creationOptions, scheduler ?? TaskScheduler.Default);

		/// <summary>
		/// Executes an action in the thread pool with cancellation supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function to run in the thread pool</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="creationOptions">The options that controls the behavior of the created task</param>
		/// <param name="scheduler">The scheduler that is used to schedule the created task</param>
		/// <returns>An awaitable task</returns>
		public static Task<T> ExecuteTask<T>(Func<T> func, CancellationToken cancellationToken = default, TaskCreationOptions creationOptions = TaskCreationOptions.DenyChildAttach, TaskScheduler scheduler = null)
			=> Task.Factory.StartNew(func, cancellationToken, creationOptions, scheduler ?? TaskScheduler.Default);

#if NETSTANDARD2_0
		static async Task ExecuteTask(this Task task, Func<Exception, Task> onErrorAsync, int defer, int timeout, CancellationTokenSource cts)
#else
		static async Task ExecuteTask(this Task task, Func<Exception, Task> onErrorAsync, int defer, int timeout)
#endif
		{
			try
			{
				if (defer > 0)
					await Task.Delay(defer).ConfigureAwait(false);
#if NETSTANDARD2_0
				if (timeout > 0 && cts != null && !task.IsCompleted)
				{
					var timeoutTask = Task.Delay(timeout);
					if (await Task.WhenAny(task, timeoutTask).ConfigureAwait(false) != task && !task.IsCompleted)
					{
						cts.Cancel();
						throw new TimeoutException($"Task timeout after {timeout} ms");
					}
				}
				await task.ConfigureAwait(false);
#else
				if (timeout > 0)
					await task.WaitAsync(TimeSpan.FromMilliseconds(timeout)).ConfigureAwait(false);
				else
					await task.ConfigureAwait(false);
#endif
			}
			catch (Exception ex)
			{
				if (onErrorAsync != null)
					try
					{
						await onErrorAsync(ex).ConfigureAwait(false);
					}
					catch { }
			}
		}

		/// <summary>
		/// Executes a task
		/// </summary>
		/// <param name="task"></param>
		/// <param name="waitForCompletion">true to wait for completion of the task</param>
		/// <param name="onErrorAsync">The error handler</param>
		/// <param name="defer">defer in miliseconds</param>
		/// <param name="timeout">timeout in miliseconds</param>
		/// <param name="useWait">true to use Wait() instead of GetAwaiter().GetResult() when wait for task completion</param>
#if NETSTANDARD2_0
		public static void Execute(this Task task, bool waitForCompletion, Func<Exception, Task> onErrorAsync, int defer = 0, int timeout = 0, CancellationTokenSource cts = null, bool useWait = false)
#else
		public static void Execute(this Task task, bool waitForCompletion, Func<Exception, Task> onErrorAsync, int defer = 0, int timeout = 0, bool useWait = false)
#endif
		{
			if (waitForCompletion)
			{
				if (useWait)
#if NETSTANDARD2_0
					task.ExecuteTask(onErrorAsync, defer, timeout, cts).Wait();
#else
					task.ExecuteTask(onErrorAsync, defer, timeout).Wait();
#endif
				else
#if NETSTANDARD2_0
					task.ExecuteTask(onErrorAsync, defer, timeout, cts).GetAwaiter().GetResult();
#else
					task.ExecuteTask(onErrorAsync, defer, timeout).GetAwaiter().GetResult();
#endif
			}
			else
#if NETSTANDARD2_0
				_ = task.ExecuteTask(onErrorAsync, defer, timeout, cts);
#else
				_ = task.ExecuteTask(onErrorAsync, defer, timeout);
#endif
		}

		/// <summary>
		/// Executes a task
		/// </summary>
		/// <param name="task"></param>
		/// <param name="onErrorAsync">The error handler</param>
		/// <param name="defer">defer in miliseconds</param>
		public static void Execute(this Task task, Func<Exception, Task> onErrorAsync, int defer = 0, int timeout = 0)
			=> task.Execute(false, onErrorAsync, defer, timeout);

#if NETSTANDARD2_0
		static async Task ExecuteTask(this Task task, Action<Exception> onError, int defer, int timeout, CancellationTokenSource cts)
#else
		static async Task ExecuteTask(this Task task, Action<Exception> onError, int defer, int timeout)
#endif
		{
			try
			{
				if (defer > 0)
					await Task.Delay(defer).ConfigureAwait(false);
#if NETSTANDARD2_0
				if (timeout > 0 && cts != null && !task.IsCompleted)
				{
					var timeoutTask = Task.Delay(timeout);
					if (await Task.WhenAny(task, timeoutTask).ConfigureAwait(false) != task && !task.IsCompleted)
					{
						cts.Cancel();
						throw new TimeoutException($"Task timeout after {timeout} ms");
					}
				}
				await task.ConfigureAwait(false);
#else
				if (timeout > 0)
					await task.WaitAsync(TimeSpan.FromMilliseconds(timeout)).ConfigureAwait(false);
				else
					await task.ConfigureAwait(false);
#endif
			}
			catch (Exception ex)
			{
				try
				{
					onError?.Invoke(ex);
				}
				catch { }
			}
		}

		/// <summary>
		/// Executes a task
		/// </summary>
		/// <param name="task"></param>
		/// <param name="waitForCompletion">true to wait for completion of the task</param>
		/// <param name="onError">The error handler</param>
		/// <param name="defer">defer in miliseconds</param>
		/// <param name="timeout">timeout in miliseconds</param>
		/// <param name="useWait">true to use Wait() instead of GetAwaiter().GetResult() when wait for task completion</param>
#if NETSTANDARD2_0
		public static void Execute(this Task task, bool waitForCompletion = false, Action<Exception> onError = null, int defer = 0, int timeout = 0, CancellationTokenSource cts = null, bool useWait = false)
#else
		public static void Execute(this Task task, bool waitForCompletion = false, Action<Exception> onError = null, int defer = 0, int timeout = 0, bool useWait = false)
#endif
		{
			if (waitForCompletion)
			{
				if (useWait)
#if NETSTANDARD2_0
					task.ExecuteTask(onError, defer, timeout, cts).Wait();
#else
					task.ExecuteTask(onError, defer, timeout).Wait();
#endif
				else
#if NETSTANDARD2_0
					task.ExecuteTask(onError, defer, timeout, cts).GetAwaiter().GetResult();
#else
					task.ExecuteTask(onError, defer, timeout).GetAwaiter().GetResult();
#endif
			}
			else
#if NETSTANDARD2_0
				_ = task.ExecuteTask(onError, defer, timeout, cts);
#else
				_ = task.ExecuteTask(onError, defer, timeout);
#endif
		}

		/// <summary>
		/// Executes a task
		/// </summary>
		/// <param name="task"></param>
		/// <param name="onError">The error handler</param>
		/// <param name="defer">defer in miliseconds</param>
		public static void Execute(this Task task, Action<Exception> onError, int defer = 0, int timeout = 0)
			=> task.Execute(false, onError, defer, timeout);

		static async Task ExecuteTask(this ValueTask task, Action<Exception> onError, int defer)
		{
			try
			{
				if (defer > 0)
					await Task.Delay(defer).ConfigureAwait(false);
				await task.ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				try
				{
					onError?.Invoke(ex);
				}
				catch { }
			}
		}

		/// <summary>
		/// Executes a task
		/// </summary>
		/// <param name="task"></param>
		/// <param name="waitForCompletion">true to wait for completion of the task</param>
		/// <param name="onError">The error handler</param>
		/// <param name="defer">defer times (in miliseconds)</param>
		/// <param name="useWait">true to use Wait() instead of GetAwaiter().GetResult() when wait for task completion</param>
		public static void Execute(this ValueTask task, bool waitForCompletion = false, Action<Exception> onError = null, int defer = 0, bool useWait = false)
		{
			if (waitForCompletion)
			{
				if (useWait)
					task.ExecuteTask(onError, defer).Wait();
				else
					task.ExecuteTask(onError, defer).GetAwaiter().GetResult();
			}
			else
				_ = task.ExecuteTask(onError, defer);
		}

		/// <summary>
		/// Executes a task
		/// </summary>
		/// <param name="task"></param>
		/// <param name="onError">The error handler</param>
		/// <param name="defer">defer times (in miliseconds)</param>
		public static void Execute(this ValueTask task, Action<Exception> onError, int defer = 0)
			=> task.Execute(false, onError, defer, true);

		/// <summary>
		/// Performs an awaitable task with cancellation token supported
		/// </summary>
		/// <param name="task"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task WithCancellationToken(this Task task, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<bool>();
			using (cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs, false))
			{
				var result = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
				if (result != task)
					throw new OperationCanceledException(cancellationToken);
			}
		}

		/// <summary>
		/// Performs an awaitable task with cancellation token supported
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="task"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<T> WithCancellationToken<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<bool>();
			using (cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetResult(true), tcs, false))
			{
				var result = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
				return result != task
					? throw new OperationCanceledException(cancellationToken)
					: task.Result;
			}
		}
		#endregion

		#region Stream/MemoryStream extensions
		static RecyclableMemoryStreamManager RecyclableMemoryStreamManager { get; } = new RecyclableMemoryStreamManager();

		/// <summary>
		/// Gets the recyclable memory stream manager (with RecyclableMemoryStreamManager class to limit LOH fragmentation and improve performance)
		/// </summary>
		/// <returns></returns>
		public static RecyclableMemoryStreamManager GetRecyclableMemoryStreamManager()
			=> UtilityService.RecyclableMemoryStreamManager;

		/// <summary>
		/// Creates an instance of <see cref="MemoryStream">MemoryStream</see> using RecyclableMemoryStream to limit LOH fragmentation and improve performance
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static MemoryStream CreateMemoryStream(byte[] buffer = null, int index = 0, int count = 0)
		{
			MemoryStream stream;
			try
			{
				stream = UtilityService.RecyclableMemoryStreamManager.GetStream();
			}
			catch
			{
				stream = new MemoryStream();
			}
			if (buffer != null && buffer.Length > 0)
			{
				index = index > -1 && index < buffer.Length ? index : 0;
				count = count > 0 && count < buffer.Length - index ? count : buffer.Length - index;
				stream.Write(buffer, index, count);
				stream.Seek(0, SeekOrigin.Begin);
			}
			return stream;
		}

		/// <summary>
		/// Converts this array of bytes to memory stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static MemoryStream ToMemoryStream(this byte[] buffer, int index = 0, int count = 0)
			=> UtilityService.CreateMemoryStream(buffer, index, count);

		/// <summary>
		/// Converts this array segment of bytes to memory stream
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static MemoryStream ToMemoryStream(this ArraySegment<byte> buffer)
			=> UtilityService.CreateMemoryStream(buffer.Array, buffer.Offset, buffer.Count);

		/// <summary>
		/// Reads this stream and converts to memory stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="onCompleted"></param>
		/// <returns></returns>
		public static MemoryStream ToMemoryStream(this Stream stream, Action<MemoryStream> onCompleted = null)
		{
			var memoryStream = UtilityService.CreateMemoryStream();
			var buffer = new byte[4096];
			var read = stream.Read(buffer, 0, buffer.Length);
			while (read > 0)
			{
				memoryStream.Write(buffer, 0, read);
				read = stream.Read(buffer, 0, buffer.Length);
			}
			memoryStream.Seek(0, SeekOrigin.Begin);
			onCompleted?.Invoke(memoryStream);
			return memoryStream;
		}

		/// <summary>
		/// Reads this stream and converts to memory stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="onCompleted"></param>
		/// <returns></returns>
		public static async Task<MemoryStream> ToMemoryStreamAsync(this Stream stream, CancellationToken cancellationToken = default, Action<MemoryStream> onCompleted = null)
		{
			var memoryStream = UtilityService.CreateMemoryStream();
			var buffer = new byte[4096];
			var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
			while (read > 0)
			{
				await memoryStream.WriteAsync(buffer, read, cancellationToken).ConfigureAwait(false);
				read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
			}
			memoryStream.Seek(0, SeekOrigin.Begin);
			onCompleted?.Invoke(memoryStream);
			return memoryStream;
		}

		/// <summary>
		///  Writes a subarray of characters to the stream
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this StreamWriter writer, char[] buffer, int index, int count, CancellationToken cancellationToken)
			=> writer.WriteAsync(buffer, index, count).WithCancellationToken(cancellationToken);

		/// <summary>
		/// Writes a string to the stream asynchronously
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="string"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this StreamWriter writer, string @string, CancellationToken cancellationToken)
#if NETSTANDARD2_0
			=> writer.WriteAsync(@string).WithCancellationToken(cancellationToken);
#else
			=> writer.WriteAsync(@string == null ? null : @string.AsMemory(), cancellationToken);
#endif

		/// <summary>
		/// Writes a line of string to the stream asynchronously
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="string"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteLineAsync(this StreamWriter writer, string @string, CancellationToken cancellationToken)
#if NETSTANDARD2_0
			=> writer.WriteLineAsync(@string).WithCancellationToken(cancellationToken);
#else
			=> writer.WriteLineAsync(@string == null ? null : @string.AsMemory(), cancellationToken);
#endif

		/// <summary>
		/// Writes the string lines to the stream asynchronously
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="lines"></param>
		/// <returns></returns>
		public static void WriteLines(this StreamWriter writer, IEnumerable<string> lines)
			=> lines?.Where(line => line != null).ForEach(line => writer.WriteLine(line));

		/// <summary>
		/// Writes the string lines to the stream asynchronously
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="lines"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteLinesAsync(this StreamWriter writer, IEnumerable<string> lines, CancellationToken cancellationToken)
			=> lines == null
				? Task.CompletedTask
#if NETSTANDARD2_0
				: lines.Where(line => line != null).ForEachAsync(line => writer.WriteLineAsync(line, cancellationToken), true, false);
#else
				: lines.Where(line => line != null).ForEachAsync(line => writer.WriteLineAsync(line.AsMemory(), cancellationToken), true, false);
#endif

		/// <summary>
		/// Reads a specified maximum number of characters from the current stream
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="buffer"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<int> ReadAsync(this StreamReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken)
			=> reader.ReadAsync(buffer, index, count).WithCancellationToken(cancellationToken);

#if NETSTANDARD2_0
		/// <summary>
		/// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<string> ReadToEndAsync(this StreamReader reader, CancellationToken cancellationToken)
			=> reader.ReadToEndAsync().WithCancellationToken(cancellationToken);

		/// <summary>
		/// Reads a line of characters asynchronously from the current stream and returns the data as a string
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<string> ReadLineAsync(this StreamReader reader, CancellationToken cancellationToken)
			=> reader.ReadLineAsync().WithCancellationToken(cancellationToken);

		public static Task CopyToAsync(this Stream source, Stream destinaion, CancellationToken cancellationToken)
			=> source.CopyToAsync(destinaion).WithCancellationToken(cancellationToken);

		public static Task CopyToAsync(this HttpContent httpContent, Stream stream, CancellationToken cancellationToken)
			=> httpContent.CopyToAsync(stream).WithCancellationToken(cancellationToken);

		public static Task<Stream> ReadAsStreamAsync(this HttpContent httpContent, CancellationToken cancellationToken)
			=> httpContent.ReadAsStreamAsync().WithCancellationToken(cancellationToken);

		public static Task<byte[]> ReadAsByteArrayAsync(this HttpContent httpContent, CancellationToken cancellationToken)
			=> httpContent.ReadAsByteArrayAsync().WithCancellationToken(cancellationToken);

		public static Task<string> ReadAsStringAsync(this HttpContent httpContent, CancellationToken cancellationToken)
			=> httpContent.ReadAsStringAsync().WithCancellationToken(cancellationToken);
#endif

		/// <summary>
		/// Reads all characters from the stream asynchronously and returns them as one string
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="leaveOpen"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<string> ReadAllAsync(this Stream stream, CancellationToken cancellationToken = default, bool leaveOpen = false, Encoding encoding = null)
		{
			if (stream.CanSeek)
				stream.Seek(0, SeekOrigin.Begin);
			using (var streamReader = new StreamReader(stream, encoding ?? Encoding.UTF8, encoding == null, TextFileReader.BufferSize, leaveOpen))
				return await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
		}

#if NETSTANDARD2_0
		/// <summary>
		/// Asynchronously reads a sequence of bytes from the current stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken)
			=> stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		/// <param name="count"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this Stream stream, byte[] buffer, int count = 0, CancellationToken cancellationToken = default)
			=> stream.WriteAsync(buffer, 0, count > 0 ? count : buffer.Length, cancellationToken);

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken)
			=> stream.WriteAsync(buffer, 0, cancellationToken);

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this Stream stream, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
			=> stream.WriteAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken);
#else
		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		/// <param name="count"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this Stream stream, byte[] buffer, int count = 0, CancellationToken cancellationToken = default)
			=> stream.WriteAsync(buffer.AsMemory(0, count > 0 ? count : buffer.Length), cancellationToken).AsTask();

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the current stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="buffer"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task WriteAsync(this Stream stream, ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
			=> stream.WriteAsync(buffer.AsMemory(), cancellationToken).AsTask();
#endif
		#endregion

		#region User agents
		/// <summary>
		/// Gets the collection of spider web-browsers' user-agents
		/// </summary>
		public static List<string> SpiderUserAgents { get; } = new List<string>
		{
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (Linux; Android 6.0.1; Nexus 5X Build/MMB29P) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.96 Mobile Safari/537.36 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"SAMSUNG-SGH-E250/1.0 Profile/MIDP-2.0 Configuration/CLDC-1.1 UP.Browser/6.2.3.3.c.1.101 (GUI) MMP/2.0 (compatible; Googlebot-Mobile/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Google (+https://developers.google.com/+/web/snippet/)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (compatible; Bingbot/2.0; +http://www.bing.com/bingbot.htm)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (compatible; Yahoo! Slurp; +http://help.yahoo.com/help/us/ysearch/slurp)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"DuckDuckBot/1.0; (+http://duckduckgo.com/duckduckbot.html)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (compatible; Baiduspider/2.0; +http://www.baidu.com/search/spider.html)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Sogou web spider/4.0(+http://www.sogou.com/docs/help/webmasters.htm#07)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"Mozilla/5.0 (compatible; Exabot/3.0; +http://www.exabot.com/go/robot)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
			"ia_archiver (+http://www.alexa.com/site/help/webmasters; crawler@alexa.com)",
			"Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)",
		};

		/// <summary>
		/// Gets an user-agent as spider-bot
		/// </summary>
		public static string SpiderUserAgent => UtilityService.SpiderUserAgents[UtilityService.GetRandomNumber(0, UtilityService.SpiderUserAgents.Count - 1)];

		/// <summary>
		/// Gets the collection of mobile web-browsers' user-agents
		/// </summary>
		public static List<string> MobileUserAgents { get; } = new List<string>
		{
			"Mozilla/5.0 (iPhone; CPU iPhone OS 18_5_1 like Mac OS X) AppleWebKit/605.2 (KHTML, like Gecko) Version/18.5 Mobile/15E148 Safari/604.2",
			"Mozilla/5.0 (iPad; CPU iPad OS 18_5_1 like Mac OS X) AppleWebKit/605.2 (KHTML, like Gecko) Version/18.5 Mobile/15E148 Safari/604.2",
			"Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.0.0 Safari/537.36",
			"Mozilla/5.0 (Linux; Android 15) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36"
		};

		/// <summary>
		/// Gets an user-agent as mobile browser
		/// </summary>
		public static string MobileUserAgent => $"{UtilityService.MobileUserAgents[UtilityService.GetRandomNumber(0, UtilityService.MobileUserAgents.Count - 1)]} QNGX/10.10";

		/// <summary>
		/// Gets the collection of desktop web-browsers' user-agents
		/// </summary>
		public static List<string> DesktopUserAgents { get; } = new List<string>
		{
			"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.2 (KHTML, like Gecko) Version/18.5 Safari/605.2",
			"Mozilla/5.0 (Macintosh; Apple Silicon; Mac OS X 10_15_7) AppleWebKit/605.2 (KHTML, like Gecko) Version/18.5 Safari/605.2",
			"Mozilla/5.0 (X11; Ubuntu; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/138.0.0.0 Safari/537.36"
		};

		/// <summary>
		/// Gets an user-agent as desktop browser
		/// </summary>
		public static string DesktopUserAgent => $"{UtilityService.DesktopUserAgents[UtilityService.GetRandomNumber(0, UtilityService.DesktopUserAgents.Count - 1)]} QNGX/10.10";
		#endregion

		#region HTTP requests
		/// <summary>
		/// Converts the collection of cookies to a string for using in HTTP headers
		/// </summary>
		/// <param name="cookies"></param>
		/// <returns></returns>
		public static string GetHttpCookies(this IEnumerable<Cookie> cookies)
			=> cookies.Select(cookie => $"{cookie.Name}={cookie.Value?.UrlEncode()}; path={cookie.Path ?? "/"}; domain={cookie.Domain ?? "*"}; expires={(cookie.Expired ? "-1" : cookie.Expires.ToHttpString())};{(cookie.Secure ? " secure;" : "")}{(cookie.HttpOnly ? " httponly;" : "")}").Join(",");

		/// <summary>
		/// Converts the collection of cookies to a string for using in HTTP headers
		/// </summary>
		/// <param name="cookies"></param>
		/// <returns></returns>
		public static string GetHttpCookies(this CookieCollection cookies)
			=> cookies.ToList().GetHttpCookies();

		/// <summary>
		/// Converts the HTTP cookies string to a collection of cookies
		/// </summary>
		/// <param name="httpCookies"></param>
		/// <param name="domain"></param>
		/// <param name="onAdd"></param>
		/// <returns></returns>
		public static CookieCollection GetCookies(this IEnumerable<string> httpCookies, string domain, Action<Cookie> onAdd = null)
		{
			var index = 0;
			var strCookies = (httpCookies ?? new List<string>()).ToList();
			while (index < strCookies.Count)
			{
				if (strCookies[index].IsContains("expires=") && !strCookies[index].IsContains(","))
				{
					strCookies[index] = $"{strCookies[index]}, {strCookies[index + 1]}";
					strCookies.RemoveAt(index + 1);
				}
				index++;
			}
			var cookies = new CookieCollection();
			strCookies.ForEach(value =>
			{
				var cookie = new Cookie();
				var parts = value.ToList(";");
				for (index = 0; index < parts.Count; index++)
				{
					if (index == 0 && parts[index] != string.Empty)
					{
						var pos = parts[index].IndexOf("=");
						cookie.Name = parts[index].Left(pos);
						cookie.Value = parts[index].Right(parts[index].Length - pos - 1);
					}
					else if (parts[index].IsContains("domain="))
					{
						var values = parts[index].ToList("=");
						if (!string.IsNullOrWhiteSpace(values[1]))
							cookie.Domain = values[1];
					}
					else if (parts[index].IsContains("path="))
					{
						var values = parts[index].ToList("=");
						if (!string.IsNullOrWhiteSpace(values[1]))
							cookie.Path = values[1];
					}
					else if (parts[index].IsContains("expires="))
						try
						{
							var values = parts[index].ToList("=");
							if (!string.IsNullOrWhiteSpace(values[1]))
								cookie.Expires = values[1].FromHttpDateTime(true);
						}
						catch { }
					else if (parts[index].IsContains("secure"))
						cookie.Secure = true;
					else if (parts[index].IsContains("httponly"))
						cookie.HttpOnly = true;
				}
				cookie.Domain = string.IsNullOrWhiteSpace(cookie.Domain) ? domain : cookie.Domain;
				cookie.Path = string.IsNullOrWhiteSpace(cookie.Path) ? "/" : cookie.Path;
				onAdd?.Invoke(cookie);
				cookies.Add(cookie);
			});
			return cookies;
		}

		static CookieCollection GetCookies(this Dictionary<string, string> headers, string domain, Action<Cookie> onAdd)
			=> headers.TryGetValue("Set-Cookie", out var cookies) && !string.IsNullOrWhiteSpace(cookies) ? cookies.ToList().GetCookies(domain, onAdd) : new CookieCollection();

		/// <summary>
		/// Gets the HTTP cookies
		/// </summary>
		/// <param name="response"></param>
		/// <param name="onAdd"></param>
		/// <returns></returns>
		public static CookieCollection GetCookies(this HttpResponseMessage response, Action<Cookie> onAdd = null)
			=> response.GetHeaders().GetCookies(response.RequestMessage.RequestUri.Host, onAdd);

		/// <summary>
		/// Gets the HTTP cookies
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="onAdd"></param>
		/// <returns></returns>
		public static CookieCollection GetCookies(this RemoteServerException exception, Action<Cookie> onAdd = null)
			=> exception.Headers.GetCookies(exception.URI.Host, onAdd);

		/// <summary>
		/// Gets the HTTP headers
		/// </summary>
		/// <param name="response"></param>
		/// <param name="excluded"></param>
		/// <param name="onCompleted"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetHeaders(this HttpResponseMessage response, IEnumerable<string> excluded = null, Action<Dictionary<string, string>> onCompleted = null)
		{
			var headers = response.Content.Headers?.ToDictionary() ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
#if !NETSTANDARD2_0
			response.TrailingHeaders?.ToDictionary().ForEach(kvp => headers[kvp.Key] = kvp.Value);
#endif
			response.Headers?.ToDictionary().ForEach(kvp => headers[kvp.Key] = kvp.Value);
			return headers.Copy(excluded, onCompleted);
		}

		/// <summary>
		/// Copies this HTTP headers (dictionary)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="excluded"></param>
		/// <param name="onCompleted"></param>
		/// <returns></returns>
		public static Dictionary<string, string> Copy(this Dictionary<string, string> @object, IEnumerable<string> excluded = null, Action<Dictionary<string, string>> onCompleted = null)
		{
			var dictionary = new Dictionary<string, string>(@object ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
			excluded?.ForEach(name => dictionary.Remove(name));
			onCompleted?.Invoke(dictionary);
			return dictionary;
		}

		/// <summary>
		/// Copies the response stream asynchronously
		/// </summary>
		/// <param name="response"></param>
		/// <param name="stream"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task CopyToAsync(this HttpResponseMessage response, Stream stream, CancellationToken cancellationToken = default)
			=> response.Content.CopyToAsync(stream, cancellationToken);

		/// <summary>
		/// Reads the response stream asynchronously
		/// </summary>
		/// <param name="response"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<Stream> ReadAsStreamAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
			=> response.Content.ReadAsStreamAsync(cancellationToken);

		/// <summary>
		/// Reads the response stream asynchronously
		/// </summary>
		/// <param name="response"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<byte[]> ReadAsByteArrayAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
			=> response.Content.ReadAsByteArrayAsync(cancellationToken);

		/// <summary>
		/// Reads all characters from the response stream asynchronously and returns them as one string
		/// </summary>
		/// <param name="response"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<string> ReadAsStringAsync(this HttpResponseMessage response, CancellationToken cancellationToken = default)
		{
			var @string = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
			return response.GetHeaders().TryGetValue("Content-Type", out var contentType) && contentType.IsStartsWith("text/html") ? @string?.HtmlDecode() : @string;
		}

		/// <summary>
		/// Gets the pre-configurated web proxy to perform a HTTP request
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="bypass"></param>
		/// <returns></returns>
		public static WebProxy GetWebProxy(Uri uri, string username, string password, IEnumerable<string> bypass = null)
			=> uri != null
				? new WebProxy(uri, true, bypass?.ToArray(), !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password) ? new CredentialCache { { uri, "Basic", new NetworkCredential(username, password) } } : null)
				: null;

		/// <summary>
		/// Gets the pre-configurated web proxy to perform a HTTP request
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="bypass"></param>
		/// <returns></returns>
		public static WebProxy GetWebProxy(string host, int port, string username, string password, IEnumerable<string> bypass = null)
			=> UtilityService.GetWebProxy(string.IsNullOrWhiteSpace(host) ? null : new Uri($"{(!host.IsStartsWith("http://") && !host.IsStartsWith("https://") ? "http://" : "")}{host}:{port}"), username, password, bypass);

		/// <summary>
		/// Assigns the pre-configurated web proxy to perform a HTTP request
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="bypass"></param>
		/// <returns></returns>
		public static WebProxy AssignWebProxy(string host, int port, string username, string password, IEnumerable<string> bypass = null)
		{
			UtilityService.Proxy = UtilityService.GetWebProxy(host, port, username, password, bypass);
			UtilityService.PrepareHttpClient();
			return UtilityService.Proxy;
		}

		/// <summary>
		/// Gets the pre-configurated web proxy to perform a HTTP request
		/// </summary>
		public static WebProxy Proxy { get; private set; }

		/// <summary>
		/// Gets the pre-configurated HttpClient instance to perform a HTTP request
		/// </summary>
		public static HttpClient HttpClient { get; private set; }

#if NETSTANDARD2_0
		static Lazy<HttpClient> LazyHttpClient { get; set; }
#else
		static SocketsHttpHandler HttpHandler { get; set; }
#endif

		static HttpClient PrepareHttpClient()
		{
			UtilityService.HttpClient?.Dispose();
#if NETSTANDARD2_0
			UtilityService.LazyHttpClient = new Lazy<HttpClient>(() =>
			{
				var handler = new HttpClientHandler
				{
					UseCookies = false,
					AllowAutoRedirect = true,
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
					Proxy = UtilityService.Proxy,
					UseProxy = UtilityService.Proxy != null
				};
				try
				{
					handler.AutomaticDecompression |= (DecompressionMethods)4;
				}
				catch { }
				return new HttpClient(handler);
			});
			return UtilityService.HttpClient = UtilityService.LazyHttpClient.Value;
#else
			UtilityService.HttpHandler?.Dispose();
			UtilityService.HttpHandler = new SocketsHttpHandler
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(2),
				MaxConnectionsPerServer = 100,
				UseCookies = false,
				AllowAutoRedirect = true,
				AutomaticDecompression = DecompressionMethods.All,
				Proxy = UtilityService.Proxy,
				UseProxy = UtilityService.Proxy != null
			};
			return UtilityService.HttpClient = new HttpClient(UtilityService.HttpHandler);
#endif
		}

		/// <summary>
		/// Gets the HttpClient instance to perform a HTTP request
		/// </summary>
		/// <param name="credential"></param>
		/// <param name="proxy"></param>
		/// <param name="allowAutoRedirect"></param>
		/// <returns></returns>
		public static HttpClient GetHttpClient(NetworkCredential credential = null, IWebProxy proxy = null, bool allowAutoRedirect = true)
		{
			if (credential != null || (proxy != null && !proxy.Equals(UtilityService.Proxy)) || !allowAutoRedirect)
			{
#if NETSTANDARD2_0
				var httpHandler = new HttpClientHandler
				{
					UseCookies = false,
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
					Proxy = proxy ?? UtilityService.Proxy,
					UseProxy = (proxy ?? UtilityService.Proxy) != null,
					Credentials = credential,
					AllowAutoRedirect = allowAutoRedirect
				};
#else
				var httpHandler = new SocketsHttpHandler
				{
					UseCookies = false,
					AutomaticDecompression = DecompressionMethods.All,
					Proxy = proxy ?? UtilityService.Proxy,
					UseProxy = (proxy ?? UtilityService.Proxy) != null,
					Credentials = credential,
					AllowAutoRedirect = allowAutoRedirect,
					PooledConnectionLifetime = TimeSpan.FromSeconds(60)
				};
#endif
				return new HttpClient(httpHandler);
			}
			return UtilityService.HttpClient ?? UtilityService.PrepareHttpClient();
		}

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="request">The request to perform</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="credential">The credential for sending the request</param>
		/// <param name="proxy">The proxy for sending the request</param>
		/// <param name="allowAutoRedirect">Allow auto-redirect</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<HttpResponseMessage> SendHttpRequestAsync(this HttpRequestMessage request, int timeout = 0, NetworkCredential credential = null, IWebProxy proxy = null, bool allowAutoRedirect = true, CancellationToken cancellationToken = default)
		{
			using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
			{
				cts.CancelAfter(TimeSpan.FromSeconds(timeout > 0 ? timeout : 30));
				var httpClient = UtilityService.GetHttpClient(credential, proxy, allowAutoRedirect);
				try
				{
					var response = await httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
					if (!response.IsSuccessStatusCode)
					{
						var headers = response.GetHeaders();
						var isMoved = response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.MovedPermanently || response.StatusCode == HttpStatusCode.Redirect;
						var isNotModified = response.StatusCode == HttpStatusCode.NotModified;
						var exception = isMoved
							? new RemoteServerMovedException(response.StatusCode, request.Method.ToString(), request.RequestUri, headers, $"Resource on the remote server was moved [{(headers.TryGetValue("Location", out var url) && !string.IsNullOrWhiteSpace(url) ? new Uri((url.IsContains("://") ? "" : $"{request.RequestUri.Scheme}://{request.RequestUri.Host}") + url) : request.RequestUri)}]")
							: new RemoteServerException(response.StatusCode, isNotModified, request.Method.ToString(), request.RequestUri, headers);
						if (!isMoved && !isNotModified)
							try
							{
								exception.Body = await response.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
								exception.Body = string.IsNullOrWhiteSpace(exception.Body) ? null : exception.Body;
							}
							catch { }
						response.Dispose();
						throw exception;
					}
					return response;
				}
				catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
				{
					throw new ConnectionTimeoutException("Request timeout", ex);
				}
				catch (Exception)
				{
					throw;
				}
				finally
				{
					if (credential != null || (proxy != null && !proxy.Equals(UtilityService.Proxy)) || !allowAutoRedirect)
						httpClient.Dispose();
				}
			}
		}

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="request">The request to perform</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(this HttpRequestMessage request, CancellationToken cancellationToken)
			=> request.SendHttpRequestAsync(0, null, null, true, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="method">The HTTP verb to perform request</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="body">The requesting body</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="credential">The credential for sending the request</param>
		/// <param name="proxy">The proxy for sending the request</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="multipartFilename">The name of the file that presents by the body stream</param>
		/// <returns></returns>
		public static async Task<HttpResponseMessage> SendHttpRequestAsync(this Uri uri, string method, Dictionary<string, string> headers, object body, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken, string multipartFilename = null)
		{
			if (string.IsNullOrWhiteSpace(uri?.AbsoluteUri))
				throw new InformationRequiredException("The URI is invalid");

			using (var request = new HttpRequestMessage(new HttpMethod(string.IsNullOrWhiteSpace(method) ? "GET" : method.ToUpper()), uri))
			{
				headers = new Dictionary<string, string>(headers ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
				headers.Copy(UtilityService.ExcludedHttpRequestHeaders).ForEach(kvp =>
				{
					try
					{
						request.Headers.Add(kvp.Key, kvp.Value?.AsciiEncode());
					}
					catch { }
				});

				if (!headers.ContainsKey("User-Agent"))
					request.Headers.Add("User-Agent", UtilityService.DesktopUserAgent);

				if (!headers.ContainsKey("Accept"))
					request.Headers.Add("Accept", string.IsNullOrWhiteSpace(multipartFilename) ? "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" : "application/json,text/plain,*/*");

				if (!headers.ContainsKey("Accept-Language"))
					request.Headers.Add("Accept-Language", "en-US,en;q=0.9,vi;q=0.8");

				if (headers.TryGetValue("Cookie", out var cookies) && !string.IsNullOrWhiteSpace(cookies))
					request.Headers.TryAddWithoutValidation("Cookie", cookies);

#if NETSTANDARD2_0
				request.Headers.Add("Accept-Encoding", "deflate, gzip");

				if (body != null && (request.Method.Equals(HttpMethod.Post) || request.Method.Equals(HttpMethod.Put)))
#else
				request.Headers.Add("Accept-Encoding", "deflate, gzip, br");

				if (body != null && (request.Method.Equals(HttpMethod.Post) || request.Method.Equals(HttpMethod.Put) || request.Method.Equals(HttpMethod.Patch)))
#endif
				{
					if (body is string @string)
						request.Content = new StringContent(@string);

					else if (body is byte[] bytes)
						request.Content = new ByteArrayContent(bytes);

					else if (body is ArraySegment<byte> array)
						request.Content = new ByteArrayContent(array.ToBytes());

					else if (body is Stream stream)
					{
						if (string.IsNullOrWhiteSpace(multipartFilename))
							request.Content = new StreamContent(stream);
						else
						{
							request.Content = new MultipartFormDataContent($"vieapps-ngx---{UtilityService.GetRandomNumber()}-----");
							request.Content.Headers.ContentType.CharSet = "utf-8";
							(request.Content as MultipartFormDataContent).Add(new StreamContent(stream), "files", multipartFilename);
						}
					}

					else
						throw new InvalidRequestException("Body is invalid");

					if (string.IsNullOrWhiteSpace(multipartFilename))
					{
						if (!headers.TryGetValue("Content-Type", out var contenType) || string.IsNullOrWhiteSpace(contenType))
							contenType = "application/octet-stream; charset=utf-8";
						request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contenType);
					}
				}

				return await request.SendHttpRequestAsync(timeout, credential, proxy, !headers.TryGetValue("AllowAutoRedirect", out var allowAutoRedirect) || !"false".IsEquals(allowAutoRedirect), cancellationToken).ConfigureAwait(false);
			}
		}

		static IEnumerable<string> ExcludedHttpRequestHeaders { get; } = new[] { "Accept-Encoding", "Connection", "Content-Type", "Cookie", "Host", "AllowAutoRedirect" };

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="method">The HTTP verb to perform request</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="body">The requesting body</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="credential">The credential for sending the request</param>
		/// <param name="proxy">The proxy for sending the request</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="multipartFilename">The name of the file that presents by the body stream</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(string uri, string method, Dictionary<string, string> headers, object body, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken, string multipartFilename = null)
			=> new Uri(uri).SendHttpRequestAsync(method, headers, body, timeout, credential, proxy, cancellationToken, multipartFilename);

		/// <summary>
		/// Sends a request to a remote end-point via HttpClient
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="method">The HTTP verb to perform request</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="body">The requesting body</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(this Uri uri, string method, Dictionary<string, string> headers, object body, int timeout = 90, CancellationToken cancellationToken = default)
			=> uri.SendHttpRequestAsync(method, headers, body, timeout, null, null, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point via HttpClient
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="method">The HTTP verb to perform request</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="body">The requesting body</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(string uri, string method, Dictionary<string, string> headers, object body, int timeout = 90, CancellationToken cancellationToken = default)
			=> new Uri(uri).SendHttpRequestAsync(method, headers, body, timeout, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(this Uri uri, Dictionary<string, string> headers = null, int timeout = 90, CancellationToken cancellationToken = default)
			=> uri.SendHttpRequestAsync("GET", headers, null, timeout, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(string uri, Dictionary<string, string> headers = null, int timeout = 90, CancellationToken cancellationToken = default)
			=> new Uri(uri).SendHttpRequestAsync(headers, timeout, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(this Uri uri, CancellationToken cancellationToken)
			=> uri.SendHttpRequestAsync(null, 90, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<HttpResponseMessage> SendHttpRequestAsync(string uri, CancellationToken cancellationToken)
			=> new Uri(uri).SendHttpRequestAsync(cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point to fetch and return as a string
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="credential">The credential for sending the request</param>
		/// <param name="proxy">The proxy for sending the request</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<string> FetchHttpAsync(this Uri uri, Dictionary<string, string> headers, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken)
		{
			using (var response = await uri.SendHttpRequestAsync("GET", headers, null, timeout, credential, proxy, cancellationToken).ConfigureAwait(false))
				return await response.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Sends a request to a remote end-point to fetch and return as a string
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="credential">The credential for sending the request</param>
		/// <param name="proxy">The proxy for sending the request</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<string> FetchHttpAsync(string uri, Dictionary<string, string> headers, int timeout, NetworkCredential credential, IWebProxy proxy, CancellationToken cancellationToken)
			=> new Uri(uri).FetchHttpAsync(headers, timeout, credential, proxy, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point to fetch and return as a string
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<string> FetchHttpAsync(this Uri uri, Dictionary<string, string> headers = null, int timeout = 90, CancellationToken cancellationToken = default)
			=> uri.FetchHttpAsync(headers, timeout, null, null, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point to fetch and return as a string
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="headers">The requesting headers</param>
		/// <param name="timeout">The requesting timeout (in seconds)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<string> FetchHttpAsync(string uri, Dictionary<string, string> headers = null, int timeout = 90, CancellationToken cancellationToken = default)
			=> new Uri(uri).FetchHttpAsync(headers, timeout, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point to fetch and return as a string
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<string> FetchHttpAsync(this Uri uri, CancellationToken cancellationToken)
			=> uri.FetchHttpAsync(null, 90, cancellationToken);

		/// <summary>
		/// Sends a request to a remote end-point to fetch and return as a string
		/// </summary>
		/// <param name="uri">The URI to perform request to</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static Task<string> FetchHttpAsync(string uri, CancellationToken cancellationToken)
			=> new Uri(uri).FetchHttpAsync(cancellationToken);
#endregion

		#region Upload/Download a remote end-point file
		/// <summary>
		/// Uploads a stream as a file to a remote end-point
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="uri"></param>
		/// <param name="fileName"></param>
		/// <param name="headers"></param>
		/// <param name="timeout"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="onSuccess"></param>
		/// <param name="onError"></param>
		/// <returns></returns>
		public static async Task UploadAsync(this Stream stream, string uri, string fileName, Dictionary<string, string> headers, int timeout, CancellationToken cancellationToken = default, Action<string, string, string, long> onSuccess = null, Action<string, string, Exception> onError = null)
		{
			try
			{
				var stopwatch = Stopwatch.StartNew();
				using (var response = await new Uri(uri).SendHttpRequestAsync("POST", headers, stream, timeout, null, null, cancellationToken, fileName).ConfigureAwait(false))
				{
					var results = await response.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
					stopwatch.Stop();
					onSuccess?.Invoke(uri, fileName, results, stopwatch.ElapsedMilliseconds);
				}
			}
			catch (Exception ex)
			{
				if (onError != null)
					onError(uri, fileName, ex);
				else
					throw;
			}
		}

		/// <summary>
		/// Uploads a stream as a file to a remote end-point
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="uri"></param>
		/// <param name="fileName"></param>
		/// <param name="headers"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task UploadAsync(this Stream stream, string uri, string fileName, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
			=> stream.UploadAsync(uri, fileName, headers, 600, cancellationToken);

		/// <summary>
		/// Uploads a file to a remote end-point
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="uri"></param>
		/// <param name="headers"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task UploadAsync(this FileInfo fileInfo, string uri, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
		{
			using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
				await fileStream.UploadAsync(uri, fileInfo.Name, headers, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Downloads a file as stream from a remote end-point
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="headers"></param>
		/// <param name="timeout"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<Stream> DownloadAsync(this Uri uri, Dictionary<string, string> headers = null, int timeout = 600, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await uri.SendHttpRequestAsync(headers, timeout, cancellationToken).ConfigureAwait(false);
				return await response.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// Downloads a file as stream from a remote end-point
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="headers"></param>
		/// <param name="timeout"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="onSuccess"></param>
		/// <param name="onError"></param>
		/// <returns></returns>
		public static async Task DownloadAsync(string uri, Dictionary<string, string> headers = null, int timeout = 600, CancellationToken cancellationToken = default, Action<string, Stream, long> onSuccess = null, Action<string, Exception> onError = null)
		{
			try
			{
				var stopwatch = Stopwatch.StartNew();
				using (var response = await new Uri(uri).SendHttpRequestAsync(headers, timeout, cancellationToken).ConfigureAwait(false))
				using (var stream = await response.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
				{
					stopwatch.Stop();
					onSuccess?.Invoke(uri, stream, stopwatch.ElapsedMilliseconds);
				}
			}
			catch (Exception ex)
			{
				if (onError != null)
					onError(uri, ex);
				else
					throw;
			}
		}
		#endregion

		#region Normalize HTML/XML tags & Remove whitespaces/breaks
		/// <summary>
		/// Gets the collection of none-close tags
		/// </summary>
		public static HashSet<string> NoneCloseTags { get; } = "area,base,br,col,command,embed,hr,img,input,keygen,link,meta,param,source,track,wbr".ToHashSet();

		static Regex AllTagsRegex { get; } = new Regex(@"<[^<>]+>", RegexOptions.IgnoreCase);

		static Regex AllAttributesRegex { get; } = new Regex(@"(\S+)=[""]?((?:.(?![""]?\s+(?:\S+)=|[>""]))+.)[""]?", RegexOptions.IgnoreCase);

		/// <summary>
		/// Finds all available tags of XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static List<XTag> FindTags(this string @string)
		{
			// extract all tags
			var tags = new List<XTag>();
			foreach (Match tagMatch in UtilityService.AllTagsRegex.Matches(@string ?? ""))
			{
				var full = tagMatch.Value;
				var isOpen = tagMatch.Value[1] != '/';
				var isClose = tagMatch.Value[1] == '/' || tagMatch.Value[tagMatch.Value.Length - 2] == '/';
				var spacePos = tagMatch.Value.IndexOf(" ");
				var name = isClose
					? isOpen ? spacePos > 0 ? tagMatch.Value.Substring(1, spacePos - 1) : tagMatch.Value.Substring(1, tagMatch.Value.Length - 3) : tagMatch.Value.Substring(2, tagMatch.Value.Length - 3)
					: spacePos > 0 ? tagMatch.Value.Substring(1, spacePos - 1) : tagMatch.Value.Substring(1, tagMatch.Value.Length - 2);

				var attributes = new List<XTagAttribute>();
				foreach (Match attributeMatch in UtilityService.AllAttributesRegex.Matches(tagMatch.Value))
				{
					var start = attributeMatch.Value.IndexOf("=");
					var end = attributeMatch.Value.IndexOf(attributeMatch.Value[start + 1], start + 2);
					attributes.Add(new XTagAttribute
					{
						Full = attributeMatch.Value.Substring(0, end + 1),
						Name = attributeMatch.Value.Substring(0, start),
						Value = start > 0 && end > 0 ? attributeMatch.Value.Substring(start + 2, (end > 0 ? end : attributeMatch.Value.Length) - start - 2).Trim() : ""
					});
				}

				tags.Add(new XTag
				{
					Full = tagMatch.Value,
					Name = name,
					IsOpen = isOpen,
					IsClose = isClose,
					Attributes = attributes
				});
			}

			// prepare positions
			var position = -1;
			tags.ForEach(tag =>
			{
				position = @string.IndexOf(tag.Full, position + 1);
				tag.StartPosition = position;
				tag.EndPosition = position + tag.Full.Length;
			});

			// normalize none-close tags
			tags.Where(tag => !tag.IsClose && UtilityService.NoneCloseTags.Contains(tag.Name)).ForEach(tag => tag.IsClose = true);

			// prepare reelevant tags
			tags.ForEach((current, index) =>
			{
				if (current.RelevantIndex > -1)
					return;

				if (current.IsClose)
				{
					if (current.IsOpen)
						current.RelevantIndex = index;
					return;
				}

				var next = index + 1 < tags.Count ? tags[index + 1] : null;
				if (next == null)
					return;

				if (next.IsOpen)
				{
					var nextOfNext = index + 2 < tags.Count ? tags[index + 2] : null;
					if (nextOfNext != null && nextOfNext.IsClose && nextOfNext.RelevantIndex < 0 && nextOfNext.Name == next.Name)
					{
						next.RelevantIndex = index + 2;
						nextOfNext.RelevantIndex = index + 1;
					}
				}
				else if (next.RelevantIndex < 0 && next.Name == current.Name)
				{
					current.RelevantIndex = index + 1;
					next.RelevantIndex = index;
				}

				if (current.RelevantIndex < 0)
				{
					var idx = tags.FindIndex(index + 1, tag => current.Name == tag.Name && tag.IsClose && tag.RelevantIndex < 0);
					if (idx > 0)
					{
						current.RelevantIndex = idx;
						tags[idx].RelevantIndex = index;
					}
				}
			});

			// prepare outer/inner & next/children
			tags.Where(tag => tag.IsOpen).ForEach(tag =>
			{
				tag.Outer = tag.IsClose ? tag.Full : @string.Substring(tag.StartPosition, tag.RelevantIndex > 0 && tags[tag.RelevantIndex].EndPosition > 0 ? tags[tag.RelevantIndex].EndPosition - tag.StartPosition : @string.Length - tag.StartPosition);
				tag.Inner = tag.IsClose ? null : tag.Outer?.Substring(tag.Full.Length, tag.Outer.Length - tag.Full.Length - tag.Name.Length - 3);
				tag.Next = tag.RelevantIndex > 0 && tag.RelevantIndex + 1 < tags.Count && tags[tag.RelevantIndex + 1].IsOpen ? tags[tag.RelevantIndex + 1] : null;
				tag.Children = tag.RelevantIndex > 0 ? tags.Skip(tags[tag.RelevantIndex].RelevantIndex + 1).Take(tag.RelevantIndex - tags[tag.RelevantIndex].RelevantIndex - 1).Where(xtag => xtag.IsOpen).ToList() : null;
			});

			return tags;
		}

		/// <summary>
		/// Removes tags from XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <param name="beRemovedTags"></param>
		/// <param name="removeInners"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static string RemoveTags(this string @string, IEnumerable<string> beRemovedTags = null, bool removeInners = false, Func<XTag, bool> predicate = null)
		{
			if (!string.IsNullOrWhiteSpace(@string))
			{
				@string.FindTags()
					.Where(tag => (beRemovedTags == null || beRemovedTags.FirstOrDefault(stag => tag.Name.IsEquals(stag)) != null) && (!removeInners || tag.IsOpen && (predicate == null || predicate(tag))))
					.ForEach(tag => @string = @string.Replace(StringComparison.OrdinalIgnoreCase, removeInners ? tag.Outer : tag.Full, ""));
				@string = @string.Trim();
			}
			return @string;
		}

		/// <summary>
		/// Removes HTML/XML tags
		/// </summary>
		/// <param name="string"></param>
		/// <param name="tag"></param>
		/// <param name="removeInner"></param>
		/// <returns></returns>
		public static string RemoveTag(this string @string, string tag, bool removeInner = false)
			=> UtilityService.RemoveTags(@string, string.IsNullOrWhiteSpace(tag) ? null : new[] { tag }, removeInner);

		/// <summary>
		/// Removes tags of Microsoft Office from XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string RemoveMsOfficeTags(this string @string, IEnumerable<string> tags = null)
			=> UtilityService.RemoveTags(@string, tags ?? "w:|o:|v:|m:|st1:|st2:|st3:|st4:|st5:".Split('|'));

		/// <summary>
		/// Gets the default predicate function for removing tag attributes
		/// </summary>
		public static Func<XTag, XTagAttribute, bool> RemoveTagAttributesPredicate { get; } = (tag, attribute) =>
		{
			if (tag.Name.IsEquals("a"))
				return !attribute.Name.IsEquals("href") && !attribute.Name.IsEquals("target") && !attribute.Name.IsEquals("rel");
			if (tag.Name.IsEquals("img") || tag.Name.IsEquals("source"))
				return !attribute.Name.IsEquals("src") && !attribute.Name.IsEquals("srcset") && !attribute.Name.IsEquals("alt") && !attribute.Name.IsEquals("type");
			if (tag.Name.IsEquals("video") || tag.Name.IsEquals("audio"))
				return !attribute.Name.IsEquals("controls") && !attribute.Name.IsEquals("poster") && !attribute.Name.IsEquals("autoplay") && !attribute.Name.IsEquals("muted");
			if (tag.Name.IsEquals("iframe"))
				return !attribute.Name.IsEquals("src") && !attribute.Name.IsEquals("width") && !attribute.Name.IsEquals("height") && !attribute.Name.IsEquals("allowfullscreen") && !attribute.Name.IsContains("-id");
			return attribute.Name.ToLower() != "id";
		};

		/// <summary>
		/// Removes attributes from XHTML/XML tags
		/// </summary>
		/// <param name="string"></param>
		/// <param name="firstPredicate"></param>
		/// <param name="secondPredicate"></param>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string RemoveTagAttributes(this string @string, Func<XTag, XTagAttribute, bool> firstPredicate = null, Func<XTag, XTagAttribute, bool> secondPredicate = null, IEnumerable<string> tags = null)
		{
			if (!string.IsNullOrWhiteSpace(@string))
			{
				@string = @string.HtmlDecode();
				@string.FindTags().Where(tag => tag.IsOpen && tag.Attributes.Any() && (tags == null || tags.FirstOrDefault(stag => tag.Name.IsEquals(stag)) != null)).ForEach(tag =>
				{
					var attributes = tag.Attributes.Select(attribute => new XTagAttribute { Name = attribute.Name, Value = attribute.Value }).ToList();
					var beRemoved = new List<string>();
					tag.Attributes.Where(attribute => firstPredicate != null ? firstPredicate(tag, attribute) : UtilityService.RemoveTagAttributesPredicate(tag, attribute))
						.Where(attribute => secondPredicate == null || secondPredicate(tag, attribute))
						.ForEach(attribute => beRemoved.Add(attribute.Name));
					beRemoved.ForEach(name => tag.Attributes.RemoveAt(tag.Attributes.FindIndex(attribute => attribute.Name.IsEquals(name))));
					if (tag.Attributes.Count != attributes.Count || tag.Attributes.Any(attribute => attribute.Value != attributes.First(attr => attr.Name.IsEquals(attribute.Name)).Value))
						@string = @string.Replace(StringComparison.OrdinalIgnoreCase, tag.Full, tag.ToString());
				});
				@string = @string.Replace(StringComparison.OrdinalIgnoreCase, "allowfullscreen=\"\"", "allowfullscreen");
				@string = @string.Replace(StringComparison.OrdinalIgnoreCase, "autoplay=\"\"", "autoplay");
				@string = @string.Replace(StringComparison.OrdinalIgnoreCase, "muted=\"\"", "muted");
				@string = @string.Trim();
			}
			return @string;
		}

		/// <summary>
		/// Removes attributes from a tag of HTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string RemoveTagAttributes(this string @string, IEnumerable<string> tags)
			=> UtilityService.RemoveTagAttributes(@string, null, null, tags);

		/// <summary>
		/// Removes attributes from a tag of HTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static string RemoveTagAttributes(this string @string, string tag)
			=> UtilityService.RemoveTagAttributes(@string, string.IsNullOrWhiteSpace(tag) ? null : new[] { tag });

		/// <summary>
		/// Removes comments form XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string RemoveComments(this string @string)
		{
			if (!string.IsNullOrWhiteSpace(@string))
			{
				var start = @string.PositionOf("<!--");
				while (start > -1)
				{
					var end = @string.PositionOf("-->", start);
					if (end > 0)
						@string = @string.Remove(start, end - start + 3);
					start = @string.PositionOf("<!--", start + 1);
				}
				@string = @string.Trim();
			}
			return @string;
		}

		/// <summary>
		/// Removes comments from XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string ClearComments(this string @string)
			=> UtilityService.RemoveComments(@string);

		/// <summary>
		/// Clears HTML/XML tags (with inner)
		/// </summary>
		/// <param name="string"></param>
		/// <param name="tag"></param>
		/// <returns></returns>
		public static string ClearTag(this string @string, string tag)
			=> UtilityService.RemoveTags(@string, string.IsNullOrWhiteSpace(tag) ? null : new[] { tag }, true);

		/// <summary>
		/// Cleans format from XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <param name="firstPredicate"></param>
		/// <param name="secondPredicate"></param>
		/// <param name="tags"></param>
		/// <returns></returns>
		public static string ClearFormat(this string @string, Func<XTag, XTagAttribute, bool> firstPredicate = null, Func<XTag, XTagAttribute, bool> secondPredicate = null, IEnumerable<string> tags = null)
			=> UtilityService.RemoveTagAttributes(@string, firstPredicate, secondPredicate, tags);

		/// <summary>
		/// Gets regular expressions for cleaning whitespace and breaks
		/// </summary>
		public static List<Tuple<Regex, string>> WhitespacesAndBreaksRegexs { get; } = new List<Tuple<Regex, string>>
		{
			// line-breaks
			new Tuple<Regex, string>(new Regex(@">\s+\n<", RegexOptions.IgnoreCase), "> <"),
			new Tuple<Regex, string>(new Regex(@">\n<", RegexOptions.IgnoreCase), "><"),

			// white-spaces
			new Tuple<Regex, string>(new Regex(@"\s+/>", RegexOptions.IgnoreCase), "/>"),
			new Tuple<Regex, string>(new Regex(@"/>\s+<", RegexOptions.IgnoreCase), "/><"),
			new Tuple<Regex, string>(new Regex(@">\s+<", RegexOptions.IgnoreCase), "> <"),
			new Tuple<Regex, string>(new Regex(@"""\s+>", RegexOptions.IgnoreCase), "\">"),
			new Tuple<Regex, string>(new Regex(@"'\s+>", RegexOptions.IgnoreCase), "'>"),
			new Tuple<Regex, string>(new Regex(@"\s+"">", RegexOptions.IgnoreCase), "\">"),
			new Tuple<Regex, string>(new Regex(@"\s+'>", RegexOptions.IgnoreCase), "'>"),
			new Tuple<Regex, string>(new Regex(@"""\s+/>", RegexOptions.IgnoreCase), "\"/>"),
			new Tuple<Regex, string>(new Regex(@"'\s+/>", RegexOptions.IgnoreCase), "'/>"),
			new Tuple<Regex, string>(new Regex(@"\s+""/>", RegexOptions.IgnoreCase), "\"/>"),
			new Tuple<Regex, string>(new Regex(@"\s+'/>", RegexOptions.IgnoreCase), "'/>")
		};

		internal static List<Tuple<Regex, string>> _WhitespacesAndBreaksExtendedRegexs = null;

		/// <summary>
		/// Gets extended regular expressions for cleaning whitespace and breaks
		/// </summary>
		public static List<Tuple<Regex, string>> WhitespacesAndBreaksExtendedRegexs
		{
			get
			{
				if (UtilityService._WhitespacesAndBreaksExtendedRegexs == null)
				{
					// white-spaces before/after special tags
					UtilityService._WhitespacesAndBreaksExtendedRegexs = new List<Tuple<Regex, string>>();
					"div,/div,section,/section,nav,/nav,main,/main,header,/header,footer,/footer,p,/p,h1,h2,h3,h4,h5,br,hr,input,textarea,table,tr,/tr,td,ul,/ul,li,select,/select,option,script,/script".ToArray().ForEach(tag =>
					{
						if (!tag[0].Equals('/'))
							UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@">\s+<" + tag, RegexOptions.IgnoreCase), "><" + tag));
						else
						{
							UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@">\s+<" + tag + @">\s+<", RegexOptions.IgnoreCase), "><" + tag + "><"));
							UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@">\s+<" + tag + @">", RegexOptions.IgnoreCase), "><" + tag + ">"));
							UtilityService._WhitespacesAndBreaksExtendedRegexs.Add(new Tuple<Regex, string>(new Regex(@"<" + tag + @">\s+<", RegexOptions.IgnoreCase), "<" + tag + "><"));
						}
					});
				}
				return UtilityService._WhitespacesAndBreaksExtendedRegexs;
			}
		}

		/// <summary>
		/// Removes whitespaces and breaks from HTML code
		/// </summary>
		/// <param name="string">The HTML code</param>
		/// <returns></returns>
		public static string RemoveWhitespaces(this string @string)
		{
			if (!string.IsNullOrWhiteSpace(@string))
			{
				@string = @string.Replace("\r", "").Replace(">\n\t", ">").Replace("\n\t", " ").Replace("\n", "").Replace("\t", "").Trim();
				UtilityService.WhitespacesAndBreaksRegexs.Concat(UtilityService.WhitespacesAndBreaksExtendedRegexs).ForEach(regex => @string = regex.Item1.Replace(@string, regex.Item2));
				@string = @string.Replace("> <", "><").Replace("  </", "</").Replace(" </", "</").Replace("\"> ", "\">").Replace("'> ", "'>").Replace(" class=\"\"", "").Replace(" style=\"\"", "");
			}
			return @string;
		}

		/// <summary>
		/// Removes whitespaces and breaks from HTML code
		/// </summary>
		/// <param name="string">The HTML code</param>
		/// <returns></returns>
		public static string RemoveHTMLWhitespaces(this string @string)
			=> UtilityService.RemoveWhitespaces(@string);

		/// <summary>
		/// Normalizes breaks (BR) of XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <param name="noBreakBetweenTags"></param>
		/// <returns></returns>
		public static string NormalizeBreaks(this string @string, bool noBreakBetweenTags = true)
		{
			if (!string.IsNullOrWhiteSpace(@string))
			{
				@string = @string.Replace("\t", "").Replace("\r", "").Replace("\n", "<br/>");
				UtilityService.WhitespacesAndBreaksRegexs.ForEach(regex => @string = regex.Item1.Replace(@string, regex.Item2));
				if (noBreakBetweenTags)
					@string = @string.Replace("><br/><", "><");
			}
			return @string;
		}

		/// <summary>
		/// Normalizes breaks (BR) of XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <param name="noBreakBetweenTags"></param>
		/// <returns></returns>
		public static string NormalizeHTMLBreaks(this string @string, bool noBreakBetweenTags = true)
			=> UtilityService.NormalizeBreaks(@string, noBreakBetweenTags);

		/// <summary>
		/// Normalizes all none-close tags (change to self-close) of XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string NormalizeNoneCloseTags(this string @string)
		{
			if (!string.IsNullOrWhiteSpace(@string))
				UtilityService.NoneCloseTags.ForEach(tag =>
				{
					var start = @string.PositionOf($"<{tag}");
					while (start > -1)
					{
						var end = @string.IndexOf(">", start);
						if (end > start && @string[end - 1] != '/')
							@string = @string.Substring(0, end) + "/" + @string.Substring(end);
						start = @string.PositionOf($"<{tag}", start + 1);
					}
				});
			return @string;
		}

		/// <summary>
		/// Normalizes all none-close tags (change to self-close) of XHTML/XML
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static string NormalizeHTMLNoneCloseTags(this string @string)
			=> UtilityService.NormalizeNoneCloseTags(@string);
		#endregion

		#region Working with files & directories
		readonly static List<string> _FileRemovements = new List<string> { "\\", "/", "*", "?", "<", ">", "|", ":", "\r", "\n", "\t" };
		readonly static List<string[]> _FileReplacements = new List<string[]> { new[] { "\"", "'" }, new[] { "%20", " " }, new[] { " ft. ", " & " } };

		/// <summary>
		/// Normalizes the name of a file
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string GetNormalizedFilename(this string input)
		{
			var output = input.ConvertCompositeUnicodeToUnicode();
			UtilityService._FileRemovements.ForEach(str => output = output.Replace(str, "").Trim());
			UtilityService._FileReplacements.ForEach(replacement => output = output.Replace(replacement[0], replacement[1]).Trim());
			if (output.IsStartsWith("con."))
				while (output.IndexOf(".") > -1)
					output = output.Replace(".", "");
			return output;
		}

		/// <summary>
		/// Gets size of a file in the friendly text
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static string GetFileSize(this FileInfo fileInfo)
			=> fileInfo == null || !fileInfo.Exists
				? null
				: fileInfo.Length >= (1024 * 1024 * 1024)
					? (fileInfo.Length.CastAs<double>() / (1024 * 1024 * 1024)).ToString("##0.##") + " G"
					: fileInfo.Length >= (1024 * 1024)
						? (fileInfo.Length.CastAs<double>() / (1024 * 1024)).ToString("##0.##") + " M"
						: fileInfo.Length >= 1024
							? (fileInfo.Length.CastAs<double>() / 1024).ToString("##0.##") + " K"
							: fileInfo.Length.ToString("###0") + " B";

		/// <summary>
		/// Gets size of a file in the friendly text
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string GetFileSize(string filePath)
			=> string.IsNullOrWhiteSpace(filePath) ? null : UtilityService.GetFileSize(new FileInfo(filePath));

		/// <summary>
		/// Gets listing of files
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="numberOfFiles"></param>
		/// <param name="searchInSubDirectory"></param>
		/// <param name="excludedSubDirectories"></param>
		/// <param name="orderBy"></param>
		/// <param name="orderMode"></param>
		/// <returns></returns>
		public static List<string> GetFiles(string path, string searchPatterns = null, int numberOfFiles = 0, bool searchInSubDirectory = false, IEnumerable<string> excludedSubDirectories = null, string orderBy = "Name", string orderMode = "Ascending")
		{
			if (!Directory.Exists(path))
				throw new DirectoryNotFoundException($"The directory is not found [{path}]");

			var orderDesc = !string.IsNullOrWhiteSpace(orderMode) && orderMode.IsStartsWith("Desc");
			var orderByTime = !string.IsNullOrWhiteSpace(orderBy) && (orderBy.IsStartsWith("CreationTime") || orderBy.IsStartsWith("LastAccessTime") || orderBy.IsStartsWith("LastWriteTime"));
			var orderByName = !orderByTime && !orderDesc && (string.IsNullOrWhiteSpace(orderBy) || orderBy.IsStartsWith("Name"));

			var directories = new Stack<string>();
			directories.Push(path);

			var patterns = string.IsNullOrWhiteSpace(searchPatterns) ? new[] { "*.*" } : searchPatterns.ToArray("|");
			var results = new List<(string Path, DateTime Time)>();

			while (directories.Count > 0)
			{
				var current = directories.Pop();
				foreach (var pattern in patterns)
					foreach (var filePath in Directory.EnumerateFiles(current, pattern))
					{
						results.Add((filePath, orderByTime ? orderBy.IsStartsWith("CreationTime") ? File.GetCreationTimeUtc(filePath) : orderBy.IsStartsWith("LastAccessTime") ? File.GetLastAccessTimeUtc(filePath) : File.GetLastWriteTimeUtc(filePath) : DateTime.MinValue));
						if (orderByName && numberOfFiles > 0 && results.Count >= numberOfFiles)
							return results.Select(file => file.Path).Take(numberOfFiles).ToList();
					}

				if (searchInSubDirectory)
					foreach (var subDirectory in Directory.EnumerateDirectories(current))
					{
						var excluded = false;
						if (excludedSubDirectories != null)
							foreach (var excludedSubDirectory in excludedSubDirectories)
								if (subDirectory.EndsWith(Path.DirectorySeparatorChar + excludedSubDirectory))
								{
									excluded = true;
									break;
								}
						if (!excluded)
							directories.Push(subDirectory);
					}
			}

			var ordered = orderByTime
				? orderDesc ? results.OrderByDescending(file => file.Time) : results.OrderBy(file => file.Time)
				: orderDesc ? results.OrderByDescending(file => file.Path) : results as IEnumerable<(string Path, DateTime Time)>;
			if (numberOfFiles > 0)
				ordered = ordered.Take(numberOfFiles);
			return ordered.Select(file => file.Path).ToList();
		}

		/// <summary>
		/// Gets listing of files
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="numberOfFiles"></param>
		/// <param name="searchInSubDirectory"></param>
		/// <param name="excludedSubDirectories"></param>
		/// <param name="orderBy"></param>
		/// <param name="orderMode"></param>
		/// <returns></returns>
		public static List<string> GetFiles(this DirectoryInfo directory, string searchPatterns = null, int numberOfFiles = 0, bool searchInSubDirectory = false, IEnumerable<string> excludedSubDirectories = null, string orderBy = "Name", string orderMode = "Ascending")
			=> UtilityService.GetFiles(directory.FullName, searchPatterns, numberOfFiles, searchInSubDirectory, excludedSubDirectories, orderBy, orderMode);

		/// <summary>
		/// Gets listing of files
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="numberOfFiles"></param>
		/// <param name="searchInSubDirectory"></param>
		/// <param name="excludedSubDirectories"></param>
		/// <param name="orderBy"></param>
		/// <param name="orderMode"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<List<string>> GetFilesAsync(string path, string searchPatterns = null, int numberOfFiles = 0, bool searchInSubDirectory = false, IEnumerable<string> excludedSubDirectories = null, string orderBy = "Name", string orderMode = "Ascending", CancellationToken cancellationToken = default)
			=> UtilityService.ExecuteTask(() => UtilityService.GetFiles(path, searchPatterns, numberOfFiles, searchInSubDirectory, excludedSubDirectories, orderBy, orderMode), cancellationToken);

		/// <summary>
		/// Gets listing of files
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="numberOfFiles"></param>
		/// <param name="searchInSubDirectory"></param>
		/// <param name="excludedSubDirectories"></param>
		/// <param name="orderBy"></param>
		/// <param name="orderMode"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<List<string>> GetFilesAsync(this DirectoryInfo directory, string searchPatterns = null, int numberOfFiles = 0, bool searchInSubDirectory = false, IEnumerable<string> excludedSubDirectories = null, string orderBy = "Name", string orderMode = "Ascending", CancellationToken cancellationToken = default)
			=> UtilityService.GetFilesAsync(directory.FullName, searchPatterns, numberOfFiles, searchInSubDirectory, excludedSubDirectories, orderBy, orderMode, cancellationToken);

		/// <summary>
		/// Gets path to a file/folder with 'right' path separator on each OS Platform
		/// </summary>
		/// <param name="paths"></param>
		/// <returns></returns>
		public static string GetPath(params string[] paths)
			=> paths == null || paths.Length < 1 ? null : Path.Combine(paths);

		/// <summary>
		/// Moves file (searched by patterns) of a folder to other folders
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="deleteOldFilesBeforeMoving"></param>
		public static void MoveFiles(string source, string destination, string searchPatterns, bool deleteOldFilesBeforeMoving = false)
		{
			if (!Directory.Exists(source) || !Directory.Exists(destination))
				throw new InformationInvalidException("The paths are invalid");
			if (!source.IsEquals(destination))
				UtilityService.GetFiles(source, searchPatterns).Select(path => new FileInfo(path)).ForEach(file =>
				{
					var path = Path.Combine(destination, file.Name);
					if (deleteOldFilesBeforeMoving && File.Exists(path))
						File.Delete(path);
					File.Move(file.FullName, path);
				});
		}

		/// <summary>
		/// Moves file (searched by patterns) of a folder to other folders
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="searchPatterns"></param>
		/// <param name="deleteOldFilesBeforeMoving"></param>
		public static Task MoveFilesAsync(string source, string destination, string searchPatterns, bool deleteOldFilesBeforeMoving = false, CancellationToken cancellationToken = default)
			=> UtilityService.ExecuteTask(() => UtilityService.MoveFiles(source, destination, searchPatterns, deleteOldFilesBeforeMoving), cancellationToken);

		/// <summary>
		/// Reads this file as text
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static string ReadAsText(string filePath, Encoding encoding = null, Action<string> onCompleted = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
#if NETSTANDARD2_0
			var @string = string.Empty;
			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, FileOptions.SequentialScan))
			using (var streamReader = new StreamReader(fileStream, encoding ?? Encoding.UTF8, encoding == null, TextFileReader.BufferSize, false))
				@string = streamReader.ReadToEnd();
#else
			var @string = File.ReadAllText(filePath, encoding ?? Encoding.UTF8);
#endif
			onCompleted?.Invoke(@string);
			return @string;
		}

		/// <summary>
		/// Reads this file as text
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static string ReadAsText(this FileInfo fileInfo, Encoding encoding = null, Action<string> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsText(fileInfo.FullName, encoding, onCompleted) : throw new ArgumentNullException(nameof(fileInfo));

		/// <summary>
		/// Reads this file as text
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<string> ReadAsTextAsync(string filePath, CancellationToken cancellationToken = default, Encoding encoding = null, Action<string> onCompleted = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
#if NETSTANDARD2_0
			var @string = string.Empty;
			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
			using (var streamReader = new StreamReader(fileStream, encoding ?? Encoding.UTF8, encoding == null, TextFileReader.BufferSize, false))
				@string = await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
			var @string = await File.ReadAllTextAsync(filePath, encoding ?? Encoding.UTF8, cancellationToken).ConfigureAwait(false);
#endif
			onCompleted?.Invoke(@string);
			return @string;
		}

		/// <summary>
		/// Reads this file as text
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static Task<string> ReadAsTextAsync(this FileInfo fileInfo, CancellationToken cancellationToken = default, Encoding encoding = null, Action<string> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsTextAsync(fileInfo.FullName, cancellationToken, encoding, onCompleted) : Task.FromException<string>(new ArgumentNullException(nameof(fileInfo)));

		/// <summary>
		/// Reads this file as XML
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static XmlDocument ReadAsXml(string filePath, Encoding encoding = null, Action<XmlDocument> onCompleted = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			var xml = new XmlDocument();
			xml.LoadXml(UtilityService.ReadAsText(filePath, encoding));
			onCompleted?.Invoke(xml);
			return xml;
		}

		/// <summary>
		/// Reads this file as XML
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static XmlDocument ReadAsXml(this FileInfo fileInfo, Encoding encoding = null, Action<XmlDocument> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsXml(fileInfo.FullName, encoding, onCompleted) : throw new ArgumentNullException(nameof(fileInfo));

		/// <summary>
		/// Reads this file as XML
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<XmlDocument> ReadAsXmlAsync(string filePath, CancellationToken cancellationToken = default, Encoding encoding = null, Action<XmlDocument> onCompleted = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			var xml = new XmlDocument();
			xml.LoadXml(await UtilityService.ReadAsTextAsync(filePath, cancellationToken, encoding).ConfigureAwait(false));
			onCompleted?.Invoke(xml);
			return xml;
		}

		/// <summary>
		/// Reads this file as XML
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static Task<XmlDocument> ReadAsXmlAsync(this FileInfo fileInfo, CancellationToken cancellationToken = default, Encoding encoding = null, Action<XmlDocument> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsXmlAsync(fileInfo.FullName, cancellationToken, encoding, onCompleted) : Task.FromException<XmlDocument>(new ArgumentNullException(nameof(fileInfo)));

		/// <summary>
		/// Reads this file as JSON
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static JToken ReadAsJson(string filePath, Encoding encoding = null, Action<JToken> onCompleted = null)
		{
			var json = JToken.Parse(UtilityService.ReadAsText(filePath, encoding));
			onCompleted?.Invoke(json);
			return json;
		}

		/// <summary>
		/// Reads this file as JSON
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static JToken ReadAsJson(this FileInfo fileInfo, Encoding encoding = null, Action<JToken> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsJson(fileInfo.FullName, encoding, onCompleted) : throw new ArgumentNullException(nameof(fileInfo));

		/// <summary>
		/// Reads this file as JSON
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task<JToken> ReadAsJsonAsync(string filePath, CancellationToken cancellationToken = default, Encoding encoding = null, Action<JToken> onCompleted = null)
		{
			var json = JToken.Parse(await UtilityService.ReadAsTextAsync(filePath, cancellationToken, encoding).ConfigureAwait(false));
			onCompleted?.Invoke(json);
			return json;
		}

		/// <summary>
		/// Reads this file as JSON
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static Task<JToken> ReadAsJsonAsync(this FileInfo fileInfo, CancellationToken cancellationToken = default, Encoding encoding = null, Action<JToken> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsJsonAsync(fileInfo.FullName, cancellationToken, encoding, onCompleted) : Task.FromException<JToken>(new ArgumentNullException(nameof(fileInfo)));

		/// <summary>
		/// Reads multiple lines of this file
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="position"></param>
		/// <param name="totalOfLines"></param>
		/// <returns></returns>
		public static (List<string> Lines, long Position) ReadAsText(this FileInfo fileInfo, long position, int totalOfLines)
		{
			if (fileInfo == null || !fileInfo.Exists)
				throw new FileNotFoundException($"Not found [{(fileInfo == null ? nameof(fileInfo) : fileInfo.FullName)}]");
			using (var reader = new TextFileReader(fileInfo.FullName, position))
				return (reader.ReadLines(totalOfLines), reader.Position);
		}

		/// <summary>
		/// Reads multiple lines of this file
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="position"></param>
		/// <param name="totalOfLines"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<(List<string> Lines, long Position)> ReadAsTextAsync(this FileInfo fileInfo, long position, int totalOfLines, CancellationToken cancellationToken = default)
		{
			if (fileInfo == null || !fileInfo.Exists)
				throw new FileNotFoundException($"Not found [{(fileInfo == null ? nameof(fileInfo) : fileInfo.FullName)}]");
			using (var reader = new TextFileReader(fileInfo.FullName, position))
				return (await reader.ReadLinesAsync(totalOfLines, cancellationToken).ConfigureAwait(false), reader.Position);
		}

		/// <summary>
		/// Reads this file as binary
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static byte[] ReadAsBinary(string filePath, Action<byte[]> onCompleted = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			var buffer = Array.Empty<byte>();
#if NETSTANDARD2_0
			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, FileOptions.SequentialScan))
			{
				var length = fileStream.Length;
				if (length > int.MaxValue)
					throw new IOException($"File too large [{length}]");
				buffer = new byte[(int)length];
				var offset = 0;
				while (offset < buffer.Length)
				{
					var read = fileStream.Read(buffer, offset, buffer.Length - offset);
					if (read == 0)
						throw new EndOfStreamException();
					offset += read;
				}
			}
#else
			buffer = File.ReadAllBytes(filePath);
#endif
			onCompleted?.Invoke(buffer);
			return buffer;
		}

		/// <summary>
		/// Reads this file as binary
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <returns></returns>
		public static byte[] ReadAsBinary(this FileInfo fileInfo, Action<byte[]> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsBinary(fileInfo.FullName, onCompleted) : throw new ArgumentNullException(nameof(fileInfo));

		/// <summary>
		/// Reads this file as binary
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<byte[]> ReadAsBinaryAsync(string filePath, CancellationToken cancellationToken = default, Action<byte[]> onCompleted = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			var buffer = Array.Empty<byte>();
#if NETSTANDARD2_0
			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
			{
				var length = fileStream.Length;
				if (length > int.MaxValue)
					throw new IOException($"File too large [{length}]");
				buffer = new byte[(int)length];
				var offset = 0;
				while (offset < buffer.Length)
				{
					var read = await fileStream.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken).ConfigureAwait(false);
					if (read == 0)
						throw new EndOfStreamException();
					offset += read;
				}
			}
#else
			buffer = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
#endif
			onCompleted?.Invoke(buffer);
			return buffer;
		}

		/// <summary>
		/// Reads this file as binary
		/// </summary>
		/// <param name="fileInfo"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<byte[]> ReadAsBinaryAsync(this FileInfo fileInfo, CancellationToken cancellationToken = default, Action<byte[]> onCompleted = null)
			=> fileInfo != null ? UtilityService.ReadAsBinaryAsync(fileInfo.FullName, cancellationToken, onCompleted) : Task.FromException<byte[]>(new ArgumentNullException(nameof(fileInfo)));

		/// <summary>
		/// Saves this stream as text file
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static async Task SaveAsTextAsync(this Stream stream, string filePath, CancellationToken cancellationToken = default, bool append = false, Encoding encoding = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			if (stream.CanSeek)
				stream.Seek(0, SeekOrigin.Begin);
			encoding = encoding ?? Encoding.UTF8;
			using (var reader = new StreamReader(stream, encoding, true, TextFileReader.BufferSize, true))
			using (var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
			using (var writer = new StreamWriter(fileStream, encoding, TextFileReader.BufferSize, false))
			{
				var buffer = new char[TextFileReader.BufferSize];
				var read = await reader.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
				while (read > 0)
				{
					await writer.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(false);
					read = await reader.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
				}
				await writer.FlushAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Saves this content as text file
		/// </summary>
		/// <param name="content"></param>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static async Task SaveAsTextAsync(this byte[] content, string filePath, CancellationToken cancellationToken = default, bool append = false, Encoding encoding = null)
		{
			if (content != null)
			{
				if (string.IsNullOrWhiteSpace(filePath))
					throw new ArgumentException("Invalid file path", nameof(filePath));
				using (var stream = content.ToMemoryStream())
					await stream.SaveAsTextAsync(filePath, cancellationToken, append, encoding).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Saves this content as text file
		/// </summary>
		/// <param name="content"></param>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public static async Task SaveAsTextAsync(this string content, string filePath, CancellationToken cancellationToken = default, bool append = false, Encoding encoding = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			using (var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
			using (var writer = new StreamWriter(fileStream, encoding ?? Encoding.UTF8, TextFileReader.BufferSize, false))
			{
				await writer.WriteAsync(content, cancellationToken).ConfigureAwait(false);
				await writer.FlushAsync().WithCancellationToken(cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Saves this JSON as text file
		/// </summary>
		/// <param name="json"></param>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static Task SaveAsTextAsync(this JToken json, string filePath, CancellationToken cancellationToken = default, bool append = false, Encoding encoding = null)
		{
			if (json != null)
			{
				if (string.IsNullOrWhiteSpace(filePath))
					throw new ArgumentException("Invalid file path", nameof(filePath));
				return json.ToString(Newtonsoft.Json.Formatting.Indented).SaveAsTextAsync(filePath, cancellationToken, append, encoding);
			}
			return Task.CompletedTask;
		}

		/// <summary>
		/// Saves this stream as binary file
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="stream"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="append"></param>
		/// <returns></returns>
		public static async Task SaveAsBinaryAsync(this Stream stream, string filePath, CancellationToken cancellationToken = default, bool append = false)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			if (stream.CanSeek)
				stream.Seek(0, SeekOrigin.Begin);
			using (var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
			{
				var buffer = new byte[TextFileReader.BufferSize];
				var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
				while (read > 0)
				{
					await fileStream.WriteAsync(buffer, read, cancellationToken).ConfigureAwait(false);
					read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Saves this content as binary file
		/// </summary>
		/// <param name="content"></param>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="append"></param>
		/// <returns></returns>
		public static async Task SaveAsBinaryAsync(this byte[] content, string filePath, CancellationToken cancellationToken = default, bool append = false)
		{
			if (content != null)
			{
				if (string.IsNullOrWhiteSpace(filePath))
					throw new ArgumentException("Invalid file path", nameof(filePath));
				using (var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
					await fileStream.WriteAsync(content, content.Length, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Writes the multiple lines into a text file
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="filePath"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		public static void SaveTo(this IEnumerable<string> lines, string filePath, bool append = true, Encoding encoding = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			using (var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize))
			using (var streamWriter = new StreamWriter(fileStream, encoding ?? Encoding.UTF8, TextFileReader.BufferSize))
				streamWriter.WriteLines(lines);
		}

		/// <summary>
		/// Writes the multiple lines into a text file
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="filePath"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="append"></param>
		/// <param name="encoding"></param>
		public static async Task SaveToAsync(this IEnumerable<string> lines, string filePath, CancellationToken cancellationToken = default, bool append = true, Encoding encoding = null)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Invalid file path", nameof(filePath));
			using (var fileStream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, true))
			using (var streamWriter = new StreamWriter(fileStream, encoding ?? Encoding.UTF8, TextFileReader.BufferSize))
				await streamWriter.WriteLinesAsync(lines, cancellationToken).ConfigureAwait(false);
		}
		#endregion

		#region Compressions
		/// <summary>
		/// Compresses the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode">Compression mode (zstd/br/gzip/deflate)</param>
		/// <returns></returns>
		public static byte[] Compress(this Stream stream, string mode = "zstd")
		{
			using (var output = UtilityService.CreateMemoryStream())
			{
#if NETSTANDARD2_0
				using (var compressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.CompressionStream(output, 10) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(output, CompressionMode.Compress, true) : "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#else
				using (var compressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.CompressionStream(output, 10) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(output, CompressionMode.Compress, true) : "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(output, CompressionLevel.Optimal, true) : "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#endif
				{
					if (stream.CanSeek)
						stream.Seek(0, SeekOrigin.Begin);
					var buffer = new byte[TextFileReader.BufferSize];
					var read = stream.Read(buffer, 0, buffer.Length);
					while (read > 0)
					{
						compressor.Write(buffer, 0, read);
						read = stream.Read(buffer, 0, buffer.Length);
					}
					compressor.Flush();
				}
				return output.ToBytes();
			}
		}

		/// <summary>
		/// Compresses the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode">Compression mode (zstd/br/gzip/deflate)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<byte[]> CompressAsync(this Stream stream, string mode = "zstd", CancellationToken cancellationToken = default)
		{
			using (var output = UtilityService.CreateMemoryStream())
			{
#if NETSTANDARD2_0
				using (var compressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.CompressionStream(output, 10) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(output, CompressionMode.Compress, true) : "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#else
				using (var compressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.CompressionStream(output, 10) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(output, CompressionMode.Compress, true) : "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(output, CompressionLevel.Optimal, true) : "gzip".IsEquals(mode) ? new GZipStream(output, CompressionLevel.Optimal, true) : new DeflateStream(output, CompressionLevel.Optimal, true) as Stream)
#endif
				{
					if (stream.CanSeek)
						stream.Seek(0, SeekOrigin.Begin);
					var buffer = new byte[TextFileReader.BufferSize];
					var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
					while (read > 0)
					{
						await compressor.WriteAsync(buffer, read, cancellationToken).ConfigureAwait(false);
						read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
					}
					await compressor.FlushAsync(cancellationToken).ConfigureAwait(false);
				}
				return output.ToBytes();
			}
		}

		/// <summary>
		/// Compresses the array of bytes
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Compression mode (zstd/br/gzip/deflate)</param>
		/// <returns></returns>
		public static byte[] Compress(this byte[] data, string mode = "zstd")
		{
			using (var stream = data.ToMemoryStream())
				return stream.Compress(mode);
		}

		/// <summary>
		/// Compresses the array segment of bytes
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Compression mode (zstd/br/gzip/deflate)</param>
		/// <returns></returns>
		public static ArraySegment<byte> Compress(this ArraySegment<byte> data, string mode = "zstd")
			=> data.ToBytes().Compress(mode).ToArraySegment();

		/// <summary>
		/// Decompresses the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode">Decompression mode (zstd/br/gzip/deflate)</param>
		/// <returns></returns>
		public static byte[] Decompress(this Stream stream, string mode = "zstd")
		{
#if NETSTANDARD2_0
			using (var decompressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.DecompressionStream(stream) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(stream, CompressionMode.Decompress, true) : "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress, true) : new DeflateStream(stream, CompressionMode.Decompress, true) as Stream)
#else
			using (var decompressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.DecompressionStream(stream) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(stream, CompressionMode.Decompress, true) : "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(stream, CompressionMode.Decompress, true) : "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress, true) : new DeflateStream(stream, CompressionMode.Decompress, true) as Stream)
#endif
			{
				var output = Array.Empty<byte>();
				var buffer = new byte[TextFileReader.BufferSize];
				var read = decompressor.Read(buffer, 0, buffer.Length);
				while (read > 0)
				{
					output = output.Concat(buffer.Take(0, read));
					read = decompressor.Read(buffer, 0, buffer.Length);
				}
				return output;
			}
		}

		/// <summary>
		/// Decompresses the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="mode">Decompression mode (zstd/br/gzip/deflate)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public static async Task<byte[]> DecompressAsync(this Stream stream, string mode = "zstd", CancellationToken cancellationToken = default)
		{
#if NETSTANDARD2_0
			using (var decompressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.DecompressionStream(stream) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(stream, CompressionMode.Decompress, true) : "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress, true) : new DeflateStream(stream, CompressionMode.Decompress, true) as Stream)
#else
			using (var decompressor = string.IsNullOrWhiteSpace(mode) || "zstd".IsEquals(mode) || "zstandard".IsEquals(mode) ? new ZstdSharp.DecompressionStream(stream) : "snap".IsEquals(mode) || "snappy".IsEquals(mode) ? new Snappier.SnappyStream(stream, CompressionMode.Decompress, true) : "br".IsEquals(mode) || "brotli".IsEquals(mode) ? new BrotliStream(stream, CompressionMode.Decompress, true) : "gzip".IsEquals(mode) ? new GZipStream(stream, CompressionMode.Decompress, true) : new DeflateStream(stream, CompressionMode.Decompress, true) as Stream)
#endif
			{
				var output = Array.Empty<byte>();
				var buffer = new byte[TextFileReader.BufferSize];
				var read = await decompressor.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
				while (read > 0)
				{
					output = output.Concat(buffer.Take(0, read));
					read = await decompressor.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
				}
				return output;
			}
		}

		/// <summary>
		/// Decompresses the array of bytes
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Decompression mode (zstd/br/gzip/deflate)</param>
		/// <returns></returns>
		public static byte[] Decompress(this byte[] data, string mode = "zstd")
		{
			using (var stream = data.ToMemoryStream())
				return stream.Decompress(mode);
		}

		/// <summary>
		/// Decompresses the array segment of bytes
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">Decompression mode (zstd/br/gzip/deflate)</param>
		/// <returns></returns>
		public static ArraySegment<byte> Decompress(this ArraySegment<byte> data, string mode = "zstd")
			=> data.ToBytes().Decompress(mode).ToArraySegment();
		#endregion

		#region BigInteger extensions
		/// <summary>
		/// Converts this array of bytes to big-integer
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static BigInteger ToBigInteger(this byte[] bytes)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] clone = new byte[bytes.Length];
				Buffer.BlockCopy(bytes, 0, clone, 0, bytes.Length);
				Array.Reverse(clone);
				return new BigInteger(clone);
			}
			else
				return new BigInteger(bytes);
		}

		public static BigInteger ToUnsignedBigInteger(this byte[] bytes)
		{
			byte[] clone;
			if (BitConverter.IsLittleEndian)
			{
				if (bytes[0] != 0x00)
				{
					clone = new byte[bytes.Length + 1];
					Buffer.BlockCopy(bytes, 0, clone, 1, bytes.Length);
					Array.Reverse(clone);
					return new BigInteger(clone);
				}
				clone = new byte[bytes.Length];
				Buffer.BlockCopy(bytes, 0, clone, 0, bytes.Length);
				Array.Reverse(clone);
				return new BigInteger(clone);
			}

			if (bytes[bytes.Length - 1] == 0x00)
				return new BigInteger(bytes);

			clone = new byte[bytes.Length + 1];
			Buffer.BlockCopy(bytes, 0, clone, 0, bytes.Length);
			return new BigInteger(clone);
		}

		/// <summary>
		/// Converts this hexa-string to big-integer
		/// </summary>
		/// <param name="hex"></param>
		/// <returns></returns>
		public static BigInteger ToBigInteger(this string hex)
		{
			var bytes = hex.HexToBytes();
			if (BitConverter.IsLittleEndian)
				Array.Reverse(bytes);
			Array.Resize(ref bytes, bytes.Length + 1);
			bytes[bytes.Length - 1] = 0x00;
			return new BigInteger(bytes);
		}

		public static BigInteger ModInverse(this BigInteger n, BigInteger p)
		{
			BigInteger x = 1;
			BigInteger y = 0;
			BigInteger a = p;
			BigInteger b = n;

			while (b != 0)
			{
				BigInteger t = b;
				BigInteger q = BigInteger.Divide(a, t);
				b = a - q * t;
				a = t;
				t = x;
				x = y - q * t;
				y = t;
			}

			if (y < 0)
				return y + p;
			//else
			return y;
		}

		public static bool TestBit(this BigInteger i, int n)
		{
			return !(i >> n).IsEven;
		}

		public static int BitLength(this BigInteger i)
		{
			int bitLength = 0;
			do
			{
				bitLength++;
			}
			while ((i >>= 1) != 0);
			return bitLength;
		}

		public static BigInteger Order(this BigInteger b, BigInteger p)
		{
			BigInteger m = 1;
			BigInteger e = 0;

			while (BigInteger.ModPow(b, m, p) != 1)
			{
				m *= 2;
				e++;
			}

			return e;
		}

		private static BigInteger FindS(BigInteger p)
		{
			BigInteger s = p - 1;
			BigInteger e = 0;

			while (s % 2 == 0)
			{
				s /= 2;
				e += 1;
			}

			return s;
		}

		private static BigInteger FindE(BigInteger p)
		{
			BigInteger s = p - 1;
			BigInteger e = 0;

			while (s % 2 == 0)
			{
				s /= 2;
				e += 1;
			}

			return e;
		}

		private static BigInteger TwoExp(BigInteger e)
		{
			BigInteger a = 1;

			while (e > 0)
			{
				a *= 2;
				e--;
			}

			return a;
		}

		public static BigInteger ShanksSqrt(this BigInteger a, BigInteger p)
		{
			if (BigInteger.ModPow(a, (p - 1) / 2, p) == (p - 1))
				return -1;

			if (p % 4 == 3)
				return BigInteger.ModPow(a, (p + 1) / 4, p);

			//Initialize 
			BigInteger s = FindS(p);
			BigInteger e = FindE(p);
			BigInteger n = 2;

			while (BigInteger.ModPow(n, (p - 1) / 2, p) == 1)
				n++;

			BigInteger x = BigInteger.ModPow(a, (s + 1) / 2, p);
			BigInteger b = BigInteger.ModPow(a, s, p);
			BigInteger g = BigInteger.ModPow(n, s, p);
			BigInteger r = e;
			BigInteger m = b.Order(p);

			while (m > 0)
			{
				x = (x * BigInteger.ModPow(g, TwoExp(r - m - 1), p)) % p;
				b = (b * BigInteger.ModPow(g, TwoExp(r - m), p)) % p;
				g = BigInteger.ModPow(g, TwoExp(r - m), p);
				r = m;
				m = b.Order(p);
			}

			return x;
		}
		#endregion

		#region Get setting/parameter of the app
		/// <summary>
		/// Gets a setting section of the app (from the JSON configuration file [appsettings.json])
		/// </summary>
		/// <param name="path">The path from root section (ex: Logging/LogLevel/Default)</param>
		/// <returns></returns>
		public static IConfigurationSection GetAppSetting(this IConfiguration configuration, string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;

			path = path.Trim();
			while (path.StartsWith("/") || path.StartsWith(@"\"))
				path = path.Right(path.Length - 1);
			while (path.EndsWith("/") || path.EndsWith(@"\"))
				path = path.Left(path.Length - 1);

			var paths = path.IndexOf("/") > 0 ? path.ToArray("/", true) : path.ToArray(@"\", true);
			if (string.IsNullOrWhiteSpace(paths[0]))
				return null;

			var section = configuration.GetSection(paths[0]);
			var index = 1;
			while (section != null && index < paths.Length)
			{
				section = section.GetSection(paths[index]);
				index++;
			}

			return section;
		}

		/// <summary>
		/// Gets a setting value of the app (from the JSON configuration file [appsettings.json])
		/// </summary>
		/// <param name="path">The path from root section (ex: Logging/LogLevel/Default)</param>
		/// <param name="default">The default value if the setting is not found</param>
		/// <returns></returns>
		public static T GetAppSetting<T>(this IConfiguration configuration, string path, T @default = default)
		{
			var section = configuration.GetAppSetting(path);
			return section != null
				? section.Value.CastAs<T>()
				: @default;
		}

		/// <summary>
		/// Gets a setting of the app (from the XML configuration file [app.config/web.config] - section 'appSettings') with special prefix
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="default">The default value if the setting is not found</param>
		/// <param name="prefix">The special name prefix of the parameter</param>
		/// <returns></returns>
		public static string GetAppSetting(string name, string @default = null, string prefix = "vieapps")
		{
			var value = !string.IsNullOrWhiteSpace(name)
				? System.Configuration.ConfigurationManager.AppSettings[(string.IsNullOrWhiteSpace(prefix) ? "" : prefix + ":") + name.Trim()]
				: null;

			return string.IsNullOrWhiteSpace(value)
				? @default
				: value;
		}

		/// <summary>
		/// Gets a parameter of the app (first from header, then second from query)
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="header">The collection of header</param>
		/// <param name="query">The collection of query</param>
		/// <param name="default">The default value if the parameter is not found</param>
		/// <returns></returns>
		public static string GetAppParameter(string name, Dictionary<string, string> header, Dictionary<string, string> query, string @default = null)
		{
			string value = null;
			if (!string.IsNullOrWhiteSpace(name))
			{
				if (!(header ?? new Dictionary<string, string>()).TryGetValue(name, out value))
					(query ?? new Dictionary<string, string>()).TryGetValue(name, out value);
			}
			return value ?? @default;
		}

		/// <summary>
		/// Gets a parameter of the app (first from header, then second from query)
		/// </summary>
		/// <param name="name">The name of the setting</param>
		/// <param name="header">The collection of header</param>
		/// <param name="query">The collection of query</param>
		/// <param name="default">The default value if the parameter is not found</param>
		/// <returns></returns>
		public static string GetAppParameter(string name, NameValueCollection header, NameValueCollection query, string @default = null)
			=> UtilityService.GetAppParameter(name, header?.ToDictionary(), query?.ToDictionary(), @default);
		#endregion

		#region Working with external process
		/// <summary>
		/// Runs a process
		/// </summary>
		/// <param name="filePath">The string that presents the path to a file to run</param>
		/// <param name="arguments">The string that presents the arguments to run</param>
		/// <param name="onExited">The method to handle the Exit event</param>
		/// <param name="onDataReceived">The method to handle the data receive events (include OutputDataReceived and ErrorDataReceived events)</param>
		/// <returns></returns>
		public static Process RunProcess(string filePath, string arguments = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onDataReceived = null)
			=> ExternalProcess.Start(filePath, arguments, onExited, onDataReceived).Process;

		/// <summary>
		/// Kills a process
		/// </summary>
		/// <param name="process"></param>
		/// <param name="action">The action to try to close the process before the process be killed</param>
		public static void KillProcess(Process process, Action<Process> action = null)
			=> ExternalProcess.Kill(process, action);

		/// <summary>
		/// Kills a process by ID
		/// </summary>
		/// <param name="id">The integer that presents the identity of a process</param>
		/// <param name="action">The action to try to close the process before the process be killed</param>
		public static void KillProcess(int id, Action<Process> action = null)
			=> ExternalProcess.Kill(id, action);
		#endregion

		#region Get version information
		/// <summary>
		/// Gets the string that presents the version number
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="getInfoVersion"></param>
		/// <returns></returns>
		public static string GetVersion(this Assembly assembly, bool getInfoVersion = true)
		{
			var asmVersion = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
			var asmFileVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
			var asmInfoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
			return (getInfoVersion ? asmInfoVersion?.InformationalVersion : asmVersion?.Version ?? asmFileVersion?.Version) ?? assembly.GetName()?.Version?.ToString() ?? "1.0";
		}
		#endregion

	}

	// -----------------------------------------------------------

	#region Tag information of XHTML/XML
	public class XTag
	{
		public XTag() { }

		/// <summary>
		/// Gets or Sets the full tag with attributes
		/// </summary>
		public string Full { get; set; }

		/// <summary>
		/// Gets or Sets the tag name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or Sets the open state
		/// </summary>
		public bool IsOpen { get; set; }

		/// <summary>
		/// Gets or Sets the close state
		/// </summary>
		public bool IsClose { get; set; }

		/// <summary>
		/// Gets or Sets the collection of attributes
		/// </summary>
		public List<XTagAttribute> Attributes { get; set; }

		/// <summary>
		/// Gets or Sets the relevant index
		/// </summary>
		public int RelevantIndex { get; set; } = -1;

		/// <summary>
		/// Gets or Sets the start position
		/// </summary>
		public int StartPosition { get; set; } = -1;

		/// <summary>
		/// Gets or Sets the end position
		/// </summary>
		public int EndPosition { get; set; } = -1;

		/// <summary>
		/// Gets or Sets the outer (means tag with full attribute, inner content and close mark)
		/// </summary>
		public string Outer { get; set; }

		/// <summary>
		/// Gets or Sets the inner content
		/// </summary>
		public string Inner { get; set; }

		/// <summary>
		/// Gets or Sets the next tag
		/// </summary>
		public XTag Next { get; set; }

		/// <summary>
		/// Gets or Sets the collection of children tags
		/// </summary>
		public List<XTag> Children { get; set; }

		/// <summary>
		/// Converts this tag to string
		/// </summary>
		/// <param name="full"></param>
		/// <returns></returns>
		public string ToString(bool full)
		{
			var tag = "<";
			tag += this.IsOpen ? this.Name : "";
			tag += this.IsOpen && this.Attributes != null && this.Attributes.Any() ? " " + this.Attributes.ToString(" ", attribute => attribute.ToString()) : "";
			tag += this.IsClose ? $"/{(this.IsOpen ? "" : this.Name)}" : "";
			tag += ">";
			tag += full && this.IsOpen && !this.IsClose ? this.Inner + $"</{this.Name}>" : "";
			return tag;
		}

		public override string ToString()
			=> this.ToString(false);
	}

	public class XTagAttribute
	{
		public XTagAttribute() { }

		/// <summary>
		/// Gets or Sets the full attribute with name and value
		/// </summary>
		public string Full { get; set; }

		/// <summary>
		/// Gets or Sets the attribute name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or Sets the attribute value
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Converts this attribute to string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> $"{this.Name}=\"{this.Value}\"";
	}
	#endregion

	// -----------------------------------------------------------

	#region Search Query
	/// <summary>
	/// Presents a parsed query for searching
	/// </summary>
	public class SearchQuery
	{
		/// <summary>
		/// Initializes a searching query
		/// </summary>
		/// <param name="query"></param>
		public SearchQuery(string query = null)
			=> this.Parse(query);

		public List<string> AndWords { get; } = new List<string>();

		public List<string> OrWords { get; } = new List<string>();

		public List<string> NotWords { get; } =	 new List<string>();

		public List<string> AndPhrases { get; } =	new List<string>();

		public List<string> OrPhrases { get; } = new List<string>();

		public List<string> NotPhrases { get; } = new List<string>();

		/// <summary>
		/// Parses the searching query
		/// </summary>
		/// <param name="query"></param>
		public void Parse(string query)
		{
			this.AndWords.Clear();
			this.AndPhrases.Clear();
			this.OrWords.Clear();
			this.OrPhrases.Clear();
			this.NotWords.Clear();
			this.NotPhrases.Clear();

			if (string.IsNullOrWhiteSpace(query))
				return;

			var searchQuery = this.NormalizeKeywords(query);
			var allWords = new List<string>();
			var allPhrases = new List<string>();

			var start = searchQuery.IndexOf("\"");
			var end = start > -1 ? searchQuery.IndexOf("\"", start + 1) : -1;
			if (start < 0 || end < 0)
				allWords = allWords.Concat(searchQuery.Replace("\"", "").ToArray(' ', true)).ToList();

			else
			{
				while (start >= 0 && end > 0)
				{
					var previousCharater = "";
					if (start > 0)
						previousCharater = searchQuery[start - 1].ToString();

					if (previousCharater.Equals("+") || previousCharater.Equals("-"))
						start--;

					var phrase = searchQuery.Substring(start, end - start + 1);
					allPhrases.Add(phrase.Replace(" -\"", "\"").Replace(" +\"", "\""));
					searchQuery = searchQuery.Remove(start, end - start + 1).Trim();
					start = searchQuery.IndexOf("\"");
					end = searchQuery.IndexOf("\"", start + 1);
				}
				allWords = allWords.Concat(this.NormalizeKeywords(searchQuery).Replace("\"", "").ToArray(' ').Select(i => i.Trim())).ToList();
			}

			allWords.Distinct(StringComparer.OrdinalIgnoreCase).Where(word => !string.IsNullOrWhiteSpace(word)).ForEach(word =>
			{
				if (word[0].Equals('+'))
					this.AndWords.Add(word.Right(word.Length - 1));
				else if (word[0].Equals('-'))
					this.NotWords.Add(word.Right(word.Length - 1));
				else
					this.OrWords.Add(word);
			});

			allPhrases.Distinct(StringComparer.OrdinalIgnoreCase).Where(phrase => !string.IsNullOrWhiteSpace(phrase)).ForEach(phrase =>
			{
				if (phrase[0].Equals('+'))
					this.AndPhrases.Add(phrase.Right(phrase.Length - 1).Replace("\"", ""));
				else if (phrase[0].Equals('-'))
					this.NotPhrases.Add(phrase.Right(phrase.Length - 1).Replace("\"", ""));
				else
					this.OrPhrases.Add(phrase.Replace("\"", ""));
			});
		}

		string NormalizeKeywords(string keywords)
		{
			var normalizedKeywords = keywords.Trim().Replace("  ", " ");

			while (normalizedKeywords.Contains("  "))
				normalizedKeywords = normalizedKeywords.Replace("  ", " ");

			normalizedKeywords = normalizedKeywords.Replace("+ ", "+").Replace("- ", "-");

			while (normalizedKeywords.Contains("++\""))
				normalizedKeywords = normalizedKeywords.Replace("++\"", "+\"");

			while (normalizedKeywords.Contains("--\""))
				normalizedKeywords = normalizedKeywords.Replace("--\"", "--\"");

			normalizedKeywords = normalizedKeywords.Replace("'", "").Replace(",", "").Replace(".", "");
			normalizedKeywords = normalizedKeywords.Replace("&", "").Replace("!", "").Replace("%", "");
			normalizedKeywords = normalizedKeywords.Replace("@", "").Replace("$", "").Replace("*", "");
			normalizedKeywords = normalizedKeywords.Replace("|", "").Replace("~", "").Replace("#", "");

			return normalizedKeywords;
		}
	}
	#endregion

	//  --------------------------------------------------------------------------------------------

	#region App configuration section handler
	/// <summary>
	/// The handler for processing a custom configuration section of the app
	/// </summary>
	public class AppConfigurationSectionHandler : System.Configuration.IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			this.Section = section;
			return this;
		}

		/// <summary>
		/// Gets the configuration section
		/// </summary>
		public XmlNode Section { get; private set; }
	}
	#endregion

	//  --------------------------------------------------------------------------------------------

	#region Reader of a text file
	/// <summary>
	/// Extends the System.IO.StreamReader that reads lines from a file in a particular encoding
	/// </summary>
	public sealed class TextFileReader : IDisposable
	{
		// by default, one reading block is 4K (4096), then use 16K(16384)/32K(32768)/64K(65536)/128K(131072)/256K(262144)/512K(524288)
		// for better performance while working with text file has large line of characters
		public static readonly int BufferSize = 16384;
		readonly FileStream _stream = null;
		readonly StreamReader _reader = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="TextFileReader"/> class.
		/// </summary>
		/// <param name="filePath">The path to file</param>
		/// <param name="position">The initializing position</param>
		public TextFileReader(string filePath, long position = 0)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("File path is invalid", nameof(filePath));
			this._stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, TextFileReader.BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
			this._reader = new StreamReader(this._stream, true);
			if (position > 0)
				this.Seek(position);
		}

		~TextFileReader()
			=> this.Dispose();

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			this._reader.Close();
			this._reader.Dispose();
			this._stream.Close();
			this._stream.Dispose();
		}

		/// <summary>
		/// Gets the current encoding of text file.
		/// </summary>
		public Encoding Encoding => this._reader.CurrentEncoding;

		/// <summary>
		/// Gets the length of text file (in bytes).
		/// </summary>
		public long Length => this._reader.BaseStream.Length;

		/// <summary>
		/// Gets the current position
		/// </summary>
		public long Position
		{
			get
			{
				try
				{
					var readerType = this._reader.GetType();

					// shift position back from BaseStream.Position by the number of bytes read into internal buffer
					var charLen = (int)readerType.InvokeMember("_charLen", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
					var position = this._reader.BaseStream.Position - charLen;

					// if we have consumed chars from the buffer we need to calculate how many bytes they represent in the current encoding and add that to the position
					var charPos = (int)readerType.InvokeMember("_charPos", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
					if (charPos > 0)
					{
						var charBuffer = (char[])readerType.InvokeMember("_charBuffer", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, this._reader, null);
						var bytesConsumed = this.Encoding.GetBytes(charBuffer, 0, charPos).Length;
						position += bytesConsumed;
					}

					return position;
				}
				catch
				{
					return this._reader.BaseStream.Position;
				}
			}
		}

		/// <summary>
		/// Seeks to the position (to read next lines from this position)
		/// </summary>
		/// <param name="offset">The offset relative to the origin parameter</param>
		/// <param name="origin">Indicating the reference point used to obtain the new position</param>
		/// <returns>The new position within the current stream</returns>
		public long Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)
		{
			this._reader.DiscardBufferedData();
			return this._reader.BaseStream.Seek(offset > -1 ? offset : 0, origin);
		}

		/// <summary>
		/// Reads a line of characters (from the current position)
		/// </summary>
		/// <returns>The next line from file, or null if the end of file is reached</returns>
		public string ReadLine()
			=> this._reader.ReadLine();

		/// <summary>
		/// Reads a line of characters (from the current position)
		/// </summary>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The next line from file, or null if the end of file is reached</returns>
		public Task<string> ReadLineAsync(CancellationToken cancellationToken = default)
#if NETSTANDARD2_0
			=> this._reader.ReadLineAsync(cancellationToken);
#else
			=> this._reader.ReadLineAsync(cancellationToken).AsTask();
#endif

		/// <summary>
		/// Reads some lines of characters (from the current position)
		/// </summary>
		/// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
		/// <returns>The next lines from the file, or empty collectoin if the end of file is reached</returns>
		public List<string> ReadLines(int totalOfLines)
		{
			// use StreamReader to read all lines (better performance)
			if (totalOfLines < 1 && this.Position < this.Encoding.GetPreamble().Length)
				return this.ReadAllLines();

			// read lines
			var lines = new List<string>();
			var counter = 0;
			var line = this.ReadLine();
			while (line != null)
			{
				lines.Add(line);

				counter++;
				if (totalOfLines > 0 && counter >= totalOfLines)
					break;

				line = this.ReadLine();
			}
			return lines;
		}

		/// <summary>
		/// Reads some lines of characters (from the current position)
		/// </summary>
		/// <param name="totalOfLines">The total number of lines to read (set as 0 to read from current position to end of file)</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns>The next lines from the file, or empty collectoin if the end of file is reached</returns>
		public async Task<List<string>> ReadLinesAsync(int totalOfLines, CancellationToken cancellationToken = default)
		{
			// use StreamReader to read all lines (better performance)
			if (totalOfLines < 1 && this.Position < this.Encoding.GetPreamble().Length)
				return await this.ReadAllLinesAsync(cancellationToken).ConfigureAwait(false);

			// read lines
			var lines = new List<string>();
			var counter = 0;
			var line = await this.ReadLineAsync(cancellationToken).ConfigureAwait(false);

			while (line != null)
			{
				lines.Add(line);

				counter++;
				if (totalOfLines > 0 && counter >= totalOfLines)
					break;

				line = await this.ReadLineAsync(cancellationToken).ConfigureAwait(false);
			}

			return lines;
		}

		/// <summary>
		/// Reads all lines of characters of the files
		/// </summary>
		/// <returns></returns>
		public List<string> ReadAllLines()
		{
			// jump to first
			this.Seek(0);

			// read all lines
			var lines = new List<string>();
			var line = this._reader.ReadLine();
			while (line != null)
			{
				lines.Add(line);
				line = this._reader.ReadLine();
			}

			// return lines
			return lines;
		}

		/// <summary>
		/// Reads all lines of characters of the files
		/// </summary>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <returns></returns>
		public async Task<List<string>> ReadAllLinesAsync(CancellationToken cancellationToken = default)
		{
			// jump to first
			this.Seek(0);

			// read all lines
			var lines = new List<string>();
			var line = await this._reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
			while (line != null)
			{
				lines.Add(line);
				line = await this._reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
			}

			// return lines
			return lines;
		}
	}
	#endregion

	//  --------------------------------------------------------------------------------------------

	#region Random of BigInteger
	/// <summary>
	/// Represents a pseudo-random number generator, which is a device that produces a sequence of numbers that meet certain statistical requirements for randomness.
	/// </summary>
	public class RandomBigInteger : Random
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RandomBigInteger">RandomBigInteger</see> class, using a time-dependent default seed value.
		/// </summary>
		public RandomBigInteger() : base() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="RandomBigInteger">RandomBigInteger</see> class, using the specified seed value.
		/// </summary>
		/// <param name="seed"></param>
		public RandomBigInteger(int seed) : base(seed) { }

		/// <summary>
		/// Generates a random positive BigInteger between 0 and 2^bitLength (non-inclusive).
		/// </summary>
		/// <param name="length">The number of random bits to generate.</param>
		/// <returns>A random positive BigInteger between 0 and 2^bitLength (non-inclusive).</returns>
		public new BigInteger Next(int length)
		{
			if (length < 1)
				return BigInteger.Zero;

			var lengthOfBytes = length / 8;
			var lengthOfBits = length % 8;

			// generates enough random bytes to cover our bits
			var bytes = new byte[lengthOfBytes + 1];
			this.NextBytes(bytes);

			// mask out the unnecessary bits
			var mask = (byte)(0xFF >> (8 - lengthOfBits));
			bytes[bytes.Length - 1] &= mask;

			// return the BigInteger with the bits-length
			return new BigInteger(bytes.Take(1));
		}

		/// <summary>
		/// Generates a random BigInteger between start and end (non-inclusive).
		/// </summary>
		/// <param name="minValue">The lower bound.</param>
		/// <param name="maxValue">The upper bound (non-inclusive).</param>
		/// <returns>A random BigInteger between start and end (non-inclusive)</returns>
		public BigInteger Next(BigInteger minValue, BigInteger maxValue)
		{
			// initialize
			if (minValue == maxValue)
				return minValue;

			var bigInt = maxValue;

			// swap start and end if given in reverse order
			if (minValue > maxValue)
			{
				maxValue = minValue;
				minValue = bigInt;
				bigInt = maxValue - minValue;
			}
			else
				// the distance between start and end to generate a random BigIntger between 0 and (end-start) (non-inclusive)
				bigInt -= minValue;

			// count the number of bits necessary
			var bytes = bigInt.ToBytes();
			var bits = 8;
			byte mask = 0x7F;
			while ((bytes[bytes.Length - 1] & mask) == bytes[bytes.Length - 1])
			{
				bits--;
				mask >>= 1;
			}
			bits += 8 * bytes.Length;

			// generate a random BigInteger that is the first power of 2 larger than the number, 
			// then scale the range down to the size of the number,
			// finally add start back on to shift back to the desired range and return.
			return ((this.Next(bits + 1) * bigInt) / BigInteger.Pow(2, bits + 1)) + minValue;
		}
	}
	#endregion

}