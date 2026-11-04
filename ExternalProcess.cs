#region Related components
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Servicing class for working with external processes
	/// </summary>
	public sealed class ExternalProcess
	{
		/// <summary>
		/// Starts to run an external process directly
		/// </summary>
		/// <param name="filePath">The absolute path to the file of external process</param>
		/// <param name="arguments">The arguments</param>
		/// <param name="workingDirectory">The working directory</param>
		/// <param name="onExited">The action to run when the process was exited (Exited event)</param>
		/// <param name="onOutputDataReceived">The action to run when an output message was received (OutputDataReceived event)</param>
		/// <param name="onErrorDataReceived">The action to run when an error message was received (ErrorDataReceived event)</param>
		/// <param name="captureOutput">true to capture output (standard output and error output) as string</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file (sudo chmod 777 'filename') while running on Linux/macOS
		/// </remarks>
		public static Info Start(string filePath, string arguments, string workingDirectory = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onOutputDataReceived = null, Action<object, DataReceivedEventArgs> onErrorDataReceived = null, bool captureOutput = false)
		{
			var info = new Info(filePath, arguments);
			var psi = new ProcessStartInfo
			{
				FileName = info.FilePath,
				Arguments = info.Arguments,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				UseShellExecute = false,
				ErrorDialog = false,
				RedirectStandardInput = captureOutput,
				RedirectStandardOutput = captureOutput,
				RedirectStandardError = captureOutput
			};

			if (string.IsNullOrWhiteSpace(workingDirectory))
			{
				workingDirectory = "";
				if (!filePath.IsStartsWith("cmd.exe") && !filePath.IsStartsWith("/bin/bash") && !filePath.IsStartsWith("/bin/sh") && !filePath.IsStartsWith("/bin/zsh") && !filePath.IsStartsWith($".{Path.DirectorySeparatorChar}"))
				{
					var path = filePath;
					var pos = path.IndexOf(Path.DirectorySeparatorChar);
					while (pos > -1)
					{
						workingDirectory += path.Left(pos + 1);
						path = path.Remove(0, pos + 1);
						pos = path.IndexOf(Path.DirectorySeparatorChar);
					}
					if (workingDirectory.IsEndsWith(Path.DirectorySeparatorChar))
						workingDirectory = workingDirectory.Left(workingDirectory.Length - 1);
				}
			}

			if (!string.IsNullOrWhiteSpace(workingDirectory))
				psi.WorkingDirectory = workingDirectory;

			var process = new Process
			{
				StartInfo = psi,
				EnableRaisingEvents = true
			};

			process.OutputDataReceived += (sender, args) =>
			{
				if (captureOutput)
					info.StandardOutput += $"\r\n{args.Data}";
				onOutputDataReceived?.Invoke(sender, args);
			};

			process.ErrorDataReceived += (sender, args) =>
			{
				if (captureOutput)
					info.StandardError += $"\r\n{args.Data}";
				onErrorDataReceived?.Invoke(sender, args);
			};

			process.Exited += (sender, args) =>
			{
				try
				{
					info.ExitCode = process.ExitCode;
					info.ExitTime = process.ExitTime;
				}
				catch { }
				onExited?.Invoke(sender, args);
				(sender as IDisposable)?.Dispose();
			};

			process.Start();
			if (captureOutput)
			{
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}

			info.Process = process;
			info.ID = process.Id;
			info.StartTime = process.StartTime;

			return info;
		}

		/// <summary>
		/// Starts to run an external process directly
		/// </summary>
		/// <param name="filePath">The absolute path to the file of external process</param>
		/// <param name="arguments">The arguments</param>
		/// <param name="onExited">The action to run when the process was exited (Exited event)</param>
		/// <param name="onDataReceived">The method to handle the data receive events (include OutputDataReceived and ErrorDataReceived events)</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file (sudo chmod 777 'filename') while running on Linux/macOS
		/// </remarks>
		public static Info Start(string filePath, string arguments, Action<object, EventArgs> onExited, Action<object, DataReceivedEventArgs> onDataReceived = null)
			=> ExternalProcess.Start(filePath, arguments, null, onExited, onDataReceived, onDataReceived, !RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

		/// <summary>
		/// Starts to run a command as external process with 'cmd.exe' (Windows) or '/bin/bash' (Linux/macOS)
		/// </summary>
		/// <param name="command">The command to run</param>
		/// <param name="workingDirectory">The working directory</param>
		/// <param name="onExited">The action to run when the process was exited (Exited event)</param>
		/// <param name="onOutputDataReceived">The action to run when an output message was received (OutputDataReceived event)</param>
		/// <param name="onErrorDataReceived">The action to run when an error message was received (ErrorDataReceived event)</param>
		/// <param name="captureOutput">true to capture output (standard output and error output) as string</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file (sudo chmod 777 'filename') while running on Linux/macOS
		/// </remarks>
		public static Info Start(string command, string workingDirectory = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onOutputDataReceived = null, Action<object, DataReceivedEventArgs> onErrorDataReceived = null, bool captureOutput = false)
		{
			var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? $"/c \"{command.Replace("\"", "\"\"\"")}\""
				: $"-c \"{command.Replace("\"", "\\\"")}\"";
			return ExternalProcess.Start(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash", arguments, workingDirectory, onExited, onOutputDataReceived, onErrorDataReceived, captureOutput);
		}

		/// <summary>
		/// Starts to run a command as external process with 'cmd.exe' (Windows) or '/bin/bash' (Linux/macOS), wait for complete
		/// </summary>
		/// <param name="command">The command to run</param>
		/// <param name="workingDirectory">The working directory</param>
		/// <param name="onExited">The action to run when the process was exited (Exited event)</param>
		/// <param name="onOutputDataReceived">The action to run when an output message was received (OutputDataReceived event)</param>
		/// <param name="onErrorDataReceived">The action to run when an error message was received (ErrorDataReceived event)</param>
		/// <param name="captureOutput">true to capture output (standard output and error output) as string</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file (sudo chmod 777 'filename') while running on Linux/macOS
		/// </remarks>
		public static void Run(string command, string workingDirectory = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onOutputDataReceived = null, Action<object, DataReceivedEventArgs> onErrorDataReceived = null, bool captureOutput = false)
		{
			var info = ExternalProcess.Start(command, workingDirectory, onExited, onOutputDataReceived, onErrorDataReceived, captureOutput);
			info.Process.WaitForExit();
			info.Stop();
		}

#if !NETSTANDARD2_0
		/// <summary>
		/// Starts to run a command as external process with 'cmd.exe' (Windows) or '/bin/bash' (Linux/macOS) and wait for complete
		/// </summary>
		/// <param name="command">The command to run</param>
		/// <param name="workingDirectory">The working directory</param>
		/// <param name="onExited">The action to run when the process was exited (Exited event)</param>
		/// <param name="onOutputDataReceived">The action to run when an output message was received (OutputDataReceived event)</param>
		/// <param name="onErrorDataReceived">The action to run when an error message was received (ErrorDataReceived event)</param>
		/// <param name="captureOutput">true to capture output (standard output and error output) as string</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file (sudo chmod 777 'filename') while running on Linux/macOS
		/// </remarks>
		public static async Task RunAsync(string command, string workingDirectory = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onOutputDataReceived = null, Action<object, DataReceivedEventArgs> onErrorDataReceived = null, bool captureOutput = false, CancellationToken cancellationToken = default)
		{
			var info = ExternalProcess.Start(command, workingDirectory, onExited, onOutputDataReceived, onErrorDataReceived, captureOutput);
			await info.Process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
			info.Stop();
		}

		/// <summary>
		/// Starts to run a command as external process with 'cmd.exe' (Windows) or '/bin/bash' (Linux/macOS) and wait for complete
		/// </summary>
		/// <param name="command">The command to run</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file (sudo chmod 777 'filename') while running on Linux/macOS
		/// </remarks>
		public static Task RunAsync(string command, CancellationToken cancellationToken)
			=> ExternalProcess.RunAsync(command, null, null, null, null, false, cancellationToken);
#endif

		/// <summary>
		/// Stops an external process
		/// </summary>
		/// <param name="info">The information</param>
		/// <param name="onCompleted">The action to run when completed</param>
		/// <param name="onError">The action to run when got error</param>
		public static void Stop(Info info, Action<Info> onCompleted = null, Action<Exception> onError = null)
		{
			if (info?.Process == null)
				onCompleted?.Invoke(info);

			else
				ExternalProcess.Kill
				(
					info.Process,
					process =>
					{
						try
						{
							if (process != null && process.HasExited)
							{
								info.ExitCode = process.ExitCode;
								info.ExitTime = process.ExitTime;
							}
						}
						catch (Exception ex)
						{
							onError?.Invoke(ex);
						}
						finally
						{
							process?.Dispose();
							onCompleted?.Invoke(info);
						}
					},
					onError
				);
		}

		/// <summary>
		/// Kills an external process
		/// </summary>
		/// <param name="process"></param>
		/// <param name="onKilled">The action to run when process was killed</param>
		/// <param name="onError">The action to run when got error</param>
		public static void Kill(Process process, Action<Process> onKilled = null, Action<Exception> onError = null)
		{
			if (process == null)
				onKilled?.Invoke(null);
			else
				try
				{
					if (!process.HasExited)
					{
#if NETSTANDARD2_0
						process.Kill();
#else
						process.Kill(true);
#endif
					}
					process.WaitForExit(456);
					onKilled?.Invoke(process);
				}
				catch (InvalidOperationException)
				{
					onKilled?.Invoke(process);
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
		}

		/// <summary>
		/// Kills an external process that specified by identity
		/// </summary>
		/// <param name="processID">The integer that presents the identity of a process that to be killed</param>
		/// <param name="onKilled">The action to run when process was killed</param>
		/// <param name="onError">The action to run when got error</param>
		public static void Kill(int processID, Action<Process> onKilled = null, Action<Exception> onError = null)
		{
			try
			{
				using (var process = Process.GetProcessById(processID))
					ExternalProcess.Kill(process, onKilled, onError);
			}
			catch (ArgumentException)
			{
				onKilled?.Invoke(null);
			}
			catch (Exception ex)
			{
				onError?.Invoke(ex);
			}
		}

		/// <summary>
		/// Sends a command
		/// </summary>
		/// <param name="process"></param>
		/// <param name="command"></param>
		public static void Send(Process process, string command)
			=> process.StandardInput.WriteLine(command);

		/// <summary>
		/// Sends a command
		/// </summary>
		/// <param name="process"></param>
		/// <param name="command"></param>
		/// <param name="cancellationToken"></param>
		public static Task SendAsync(Process process, string command, CancellationToken cancellationToken = default)
			=> process.StandardInput.WriteLineAsync(command, cancellationToken);

		/// <summary>
		/// Presents information of an external process
		/// </summary>
		public class Info
		{
			/// <summary>
			/// Creates new information of an external process
			/// </summary>
			/// <param name="filePath">The absolute path to the file of external process</param>
			/// <param name="arguments">The arguments</param>
			public Info(string filePath = null, string arguments = null)
			{
				this.FilePath = filePath ?? "";
				this.Arguments = arguments ?? "";
			}

			/// <summary>
			/// Gest the absolute path of file
			/// </summary>
			public string FilePath { get; internal set; }

			/// <summary>
			/// Gets the arguments
			/// </summary>
			public string Arguments { get; internal set; }

			/// <summary>
			/// Ges the standard output (stdout)
			/// </summary>
			public string StandardOutput { get; internal set; } = "";

			/// <summary>
			/// Gets the standard error (stderr)
			/// </summary>
			public string StandardError { get; internal set; } = "";

			/// <summary>
			/// Gets the related process
			/// </summary>
			public Process Process { get; internal set; }

			/// <summary>
			/// Gets the identity
			/// </summary>
			public int? ID { get; internal set; }

			/// <summary>
			/// Gets the start time
			/// </summary>
			public DateTime? StartTime { get; internal set; }

			/// <summary>
			/// Gets the exit time
			/// </summary>
			public DateTime? ExitTime { get; internal set; }

			/// <summary>
			/// Gets the exit code
			/// </summary>
			public int? ExitCode { get; internal set; }

			/// <summary>
			/// Gets the extra information
			/// </summary>
			public Dictionary<string, object> Extra { get; } = new Dictionary<string, object>();

			/// <summary>
			/// Sets the value of a specified key of the extra information
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="key"></param>
			/// <param name="value"></param>
			public void Set<T>(string key, T value)
				=> this.Extra[key] = value;

			/// <summary>
			/// Gets the value of a specified key from the extra information
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="key"></param>
			/// <param name="default"></param>
			/// <returns></returns>
			public T Get<T>(string key, T @default = default)
				=> this.Extra.TryGetValue(key, out object value) && value != null && value is T
					? (T)value
					: @default;

			/// <summary>
			/// Removes the value of a specified key from the extra information
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public bool Remove(string key)
				=> this.Extra.Remove(key);

			/// <summary>
			/// Removes the value of a specified key from the extra information
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="key"></param>
			/// <param name="value"></param>
			/// <returns></returns>
			public bool Remove<T>(string key, out T value)
			{
				value = this.Get<T>(key);
				return this.Remove(key);
			}

			/// <summary>
			/// Stops an external process
			/// </summary>
			/// <param name="onCompleted">The action to run when completed</param>
			/// <param name="onError">The action to run when got error</param>
			public void Stop(Action<Info> onCompleted = null, Action<Exception> onError = null)
				=> ExternalProcess.Stop(this, onCompleted, onError);

			/// <summary>
			/// Kills an external process
			/// </summary>
			/// <param name="onKilled">The action to run when process was killed</param>
			/// <param name="onError">The action to run when got error</param>
			public void Kill(Action<Process> onKilled = null, Action<Exception> onError = null)
				=> ExternalProcess.Kill(this.Process, onKilled, onError);

			/// <summary>
			/// Sends a command
			/// </summary>
			/// <param name="command"></param>
			public void Send(string command)
				=> ExternalProcess.Send(this.Process, command);

			/// <summary>
			/// Sends a command
			/// </summary>
			/// <param name="command"></param>
			public Task SendAsync(string command, CancellationToken cancellationToken = default)
				=> ExternalProcess.SendAsync(this.Process, command, cancellationToken);
		}
	}
}