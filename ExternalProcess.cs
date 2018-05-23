#region Related components
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
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
		/// <param name="onExited">The action to run when the process is exited (Exited event)</param>
		/// <param name="onOutputDataReceived">The action to run when an output message is received (OutputDataReceived event)</param>
		/// <param name="onErrorDataReceived">The action to run when an error message is received (ErrorDataReceived event)</param>
		/// <param name="captureOutput">true to capture output (standard output and error output)</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file
		/// - Linux: sudo chmod 777 'filename'
		/// - macOS: sudo chmod +x 'filename'
		/// </remarks>
		public static Info Start(string filePath, string arguments, string workingDirectory = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onOutputDataReceived = null, Action<object, DataReceivedEventArgs> onErrorDataReceived = null, bool captureOutput = false)
		{
			// prepare information
			var info = new Info(filePath, arguments);

			// prepare the process
			var psi = new ProcessStartInfo
			{
				FileName = info.FilePath,
				Arguments = info.Arguments,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
				UseShellExecute = false,
				ErrorDialog = false,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			if (string.IsNullOrWhiteSpace(workingDirectory))
			{
				workingDirectory = "";
				if (!filePath.IsStartsWith("cmd.exe") && !filePath.IsStartsWith("/bin/bash") && !filePath.IsStartsWith($".{Path.DirectorySeparatorChar}"))
				{
					var path = filePath;
					var pos = path.IndexOf(Path.DirectorySeparatorChar);
					while (pos > -1)
					{
						workingDirectory += path.Left(pos + 1);
						path = path.Remove(0, pos + 1);
						pos = path.IndexOf(Path.DirectorySeparatorChar);
					}
					if (workingDirectory.IsEndsWith($"{Path.DirectorySeparatorChar}"))
						workingDirectory = workingDirectory.Left(workingDirectory.Length - 1);
				}
			}

			if (!string.IsNullOrWhiteSpace(workingDirectory))
				psi.WorkingDirectory = workingDirectory;

			// initialize the proces
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
			};

			// start the process
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			// return information
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
		/// <param name="onExited">The action to run when the process is exited (Exited event)</param>
		/// <param name="onDataReceived">The method to handle the data receive events (include OutputDataReceived and ErrorDataReceived events)</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file
		/// - Linux: sudo chmod 777 'filename'
		/// - macOS: sudo chmod +x 'filename'
		/// </remarks>
		public static Info Start(string filePath, string arguments, Action<object, EventArgs> onExited, Action<object, DataReceivedEventArgs> onDataReceived = null)
			=> ExternalProcess.Start(filePath, arguments, null, onExited, onDataReceived, onDataReceived, false);

		/// <summary>
		/// Starts to run a command as external process with 'cmd.exe' (Windows) or '/bin/bash' (Linux/macOS)
		/// </summary>
		/// <param name="command">The command to run</param>
		/// <param name="workingDirectory">The working directory</param>
		/// <param name="onExited">The action to run when the process is exited (Exited event)</param>
		/// <param name="onOutputDataReceived">The action to run when an output message is received (OutputDataReceived event)</param>
		/// <param name="onErrorDataReceived">The action to run when an error message is received (ErrorDataReceived event)</param>
		/// <param name="captureOutput">true to capture output (standard output and error output)</param>
		/// <returns></returns>
		/// <remarks>
		/// Remember assign execution permisions to the file
		/// - Linux: sudo chmod 777 'filename'
		/// - macOS: sudo chmod +x 'filename'
		/// </remarks>
		public static Info Start(string command, string workingDirectory = null, Action<object, EventArgs> onExited = null, Action<object, DataReceivedEventArgs> onOutputDataReceived = null, Action<object, DataReceivedEventArgs> onErrorDataReceived = null, bool captureOutput = false)
		{
			var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? $"/c \"{command.Replace("\"", "\"\"\"")}\""
				: $"-c \"{command.Replace("\"", "\\\"")}\"";
			return ExternalProcess.Start(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash", arguments, workingDirectory, onExited, onOutputDataReceived, onErrorDataReceived, captureOutput);
		}

		/// <summary>
		/// Stops an external process
		/// </summary>
		/// <param name="info">The information</param>
		/// <param name="onCompleted">The action to run when completed</param>
		/// <param name="onError">The action to run when got error</param>
		public static void Stop(Info info, Action<Info> onCompleted = null, Action<Exception> onError = null)
		{
			if (info == null || info.Process == null || info.Process.HasExited)
				try
				{
					info?.Process?.Dispose();
					onCompleted?.Invoke(info);
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
			else
				ExternalProcess.Kill(
					info.Process,
					process =>
					{
						info.Process.StandardInput.WriteLine("exit");
						info.Process.StandardInput.Close();
						info.Process.WaitForExit(456);
						info.Process.Refresh();
					},
					process =>
					{
						try
						{
							info.ExitCode = info.Process.ExitCode;
							info.ExitTime = info.Process.ExitTime;
						}
						catch (Exception ex)
						{
							onError?.Invoke(ex);
						}
						try
						{
							info.Process.Dispose();
						}
						catch (Exception ex)
						{
							onError?.Invoke(ex);
						}
						onCompleted?.Invoke(info);
					}
				);
		}

		/// <summary>
		/// Kills an external process
		/// </summary>
		/// <param name="process"></param>
		/// <param name="tryToClose">The action to run to try to close the process before the process be killed</param>
		/// <param name="onKilled">The action to run when process was killed</param>
		/// <param name="onError">The action to run when got error</param>
		public static void Kill(Process process, Action<Process> tryToClose = null, Action<Process> onKilled = null, Action<Exception> onError = null)
		{
			try
			{
				// check
				if (process == null || process.HasExited)
				{
					onKilled?.Invoke(process);
					return;
				}

				// try to close
				try
				{
					tryToClose?.Invoke(process);
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}

				// re-check after trying to close
				try
				{
					if (process.HasExited)
					{
						onKilled?.Invoke(process);
						return;
					}
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}

				// kill
				if (process.StartInfo.RedirectStandardInput)
				{
					try
					{
						process.StandardInput.Close();
						process.CloseMainWindow();
						process.WaitForExit(456);
						process.Refresh();
					}
					catch { }
					if (!process.HasExited)
						process.Kill();
				}
				else if (!process.HasExited)
					process.Kill();

				// callback
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
		/// <param name="tryToClose">The action to try to close the process before the process be killed</param>
		/// <param name="onKilled">The action to run when process was killed</param>
		/// <param name="onError">The action to run when got error</param>
		public static void Kill(int processID, Action<Process> tryToClose = null, Action<Process> onKilled = null, Action<Exception> onError = null)
		{
			try
			{
				using (var process = Process.GetProcessById(processID))
				{
					ExternalProcess.Kill(process, tryToClose, onKilled, onError);
				}
			}
			catch (Exception ex)
			{
				onError?.Invoke(ex);
			}
		}

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
		}
	}
}