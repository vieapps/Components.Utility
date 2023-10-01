#region Related components
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Presents the instance of an Android device (connect and manipulate by ADB Shell)
	/// </summary>
	public class AndroidDevice : IDisposable
	{
		/// <summary>
		/// Gets the path to file of a platform tool (that related to 'ANDROID_HOME' environment variable)
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="useEnvironmentVariable"></param>
		/// <returns></returns>
		public static string GetPlatformToolFilePath(string fileName, bool useEnvironmentVariable = true)
		{
			if (useEnvironmentVariable)
			{
				var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME", EnvironmentVariableTarget.Machine);
				return string.IsNullOrWhiteSpace(androidHome) ? fileName : Path.Combine(androidHome, "platform-tools", fileName);
			}
			return fileName;
		}

		/// <summary>
		/// Starts new a process
		/// </summary>
		/// <param name="arguments"></param>
		/// <param name="onExited"></param>
		/// <param name="onDataReceived"></param>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static Process Start(string arguments, Action<EventArgs> onExited = null, Action<string> onDataReceived = null, string filename = null)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					UseShellExecute = false,
					ErrorDialog = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					FileName = filename ?? AndroidDevice.GetPlatformToolFilePath("adb"),
					Arguments = string.IsNullOrWhiteSpace(filename)
						? arguments ?? ""
						: filename.IsEquals("cmd.exe")
							? $"/c \"{(arguments ?? "").Replace("\"", "\"\"\"")}\""
							: filename.IsStartsWith("/bin/")
								? $"-c \"{(arguments ?? "").Replace("\"", "\\\"")}\""
								: arguments ?? ""
				},
				EnableRaisingEvents = true
			};
			process.Exited += (sender, args) => onExited?.Invoke(args);
			if (onDataReceived != null)
			{
				process.OutputDataReceived += (sender, args) => onDataReceived.Invoke(args.Data);
				process.ErrorDataReceived += (sender, args) => onDataReceived.Invoke(args.Data);
			}
			process.Start();
			if (onDataReceived != null)
			{
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}
			return process;
		}

		/// <summary>
		/// Gets the attached/connected devices, one string present 'deviceID|state'
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<List<string>> GetDevicesAsync(CancellationToken cancellationToken = default)
		{
			using (var process = AndroidDevice.Start("devices"))
			{
				var results = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
				var devices = results.Replace("\r", "").ToArray("\n").Where(info => !info.StartsWith("*") && !info.IsContains("devices attached")).Select(info => info.Trim()).ToList();
				return devices.Select(info => $"{(info.IndexOf("\t") > 0 ? info.Left(info.IndexOf("\t")).Trim() : info.Trim().Replace("device", "").Replace("offline", "").Replace("\t", "").Trim())}|{(info.IsContains("offline") ? "offline" : "online")}").ToList();
			}
		}

		/// <summary>
		/// Connects to an Android device using ADB shell
		/// </summary>
		/// <param name="deviceID"></param>
		/// <param name="onConnected"></param>
		/// <param name="onDisconnected"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<AndroidDevice> ConnectAsync(string deviceID, Action<AndroidDevice> onConnected, Action<AndroidDevice> onDisconnected, CancellationToken cancellationToken = default)
		{
			var device = new AndroidDevice(deviceID, cancellationToken, onConnected, onDisconnected);
			await device.ConnectAsync(cancellationToken).ConfigureAwait(false);
			return device;
		}

		/// <summary>
		/// Connects to an Android device using ADB shell
		/// </summary>
		/// <param name="deviceID"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static Task<AndroidDevice> ConnectAsync(string deviceID, CancellationToken cancellationToken = default)
			=> AndroidDevice.ConnectAsync(deviceID, null, null, cancellationToken);

		/// <summary>
		/// Gets the device's identity
		/// </summary>
		public string ID { get; private set; }

		/// <summary>
		/// Gets or Sets the device's friendly name
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the device's shell
		/// </summary>
		public Process Shell { get; private set; }

		/// <summary>
		/// Gets the device's shell output
		/// </summary>
		public string Output { get; private set; }

		/// <summary>
		/// Gets the device's connection state
		/// </summary>
		public bool Connected { get; private set; } = false;

		/// <summary>
		/// Sets the handler to call when device is connected
		/// </summary>
		public Action<AndroidDevice> OnConnected { private get; set; }

		/// <summary>
		/// Sets the handler to call when device is disconnected
		/// </summary>
		public Action<AndroidDevice> OnDisconnected { private get; set; }

		/// <summary>
		/// Gets the screen resolution
		/// </summary>
		public Size ScreenResolution { get; private set; } = new Size(1080, 1920);

		/// <summary>
		/// Gets or Sets the cancellation token source for sending cancel/stop signals from this device to all related processes
		/// </summary>
		public CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();

		/// <summary>
		/// Gets the cancellation token for handling cancel/stop signals from this device to all related processes
		/// </summary>
		public CancellationToken CancellationToken => this.CancellationTokenSource.Token;

		/// <summary>
		/// Gets or Sets the available state for running a scenario that provided by the pool (means able to running an app with a specified scenario)
		/// </summary>
		public bool Available { get; set; } = true;

		/// <summary>
		/// Gets or Sets the current running state of an app on this device
		/// </summary>
		public string Running { get; set; }

		/// <summary>
		/// Gets or Sets the session identity of an associated worker
		/// </summary>
		public string SessionID { get; set; }

		/// <summary>
		/// Gets the locker for working safe in multi-threads
		/// </summary>
		public SemaphoreSlim Locker { get; } = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Gets the state bag of the device
		/// </summary>
		public List<string> State { get; } = new List<string>();

		/// <summary>
		/// Gets the extra information of the device
		/// </summary>
		public ConcurrentDictionary<string, object> Extra { get; } = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Initializes a device's shell
		/// </summary>
		/// <param name="deviceID"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="onConnected"></param>
		/// <param name="onDisconnected"></param>
		public AndroidDevice(string deviceID = null, CancellationToken cancellationToken = default, Action<AndroidDevice> onConnected = null, Action<AndroidDevice> onDisconnected = null)
		{
			this.ID = deviceID ?? "";
			this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			this.OnConnected = onConnected;
			this.OnDisconnected = onDisconnected;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			this.DisconnectAsync().Run(true);
			this.Locker.Dispose();
		}
		/// <summary>
		/// Sets the value of a specified key of the extra information
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Set<T>(string key, T value)
		{
			this.Extra[key] = value;
			return true;
		}

		/// <summary>
		/// Gets the value of a specified key from the extra information
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="default"></param>
		/// <returns></returns>
		public T Get<T>(string key, T @default = default)
			=> this.Extra.TryGetValue(key, out object value) && value != null && value is T valueIsT ? valueIsT : @default;

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
			if (this.Extra.Remove(key, out var val) && val is T valueIsT)
			{
				value = valueIsT;
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Connects the device's shell
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task ConnectAsync(CancellationToken cancellationToken = default)
		{
			if (this.Connected)
			{
				this.OnConnected?.Invoke(this);
				return;
			}

			this.Output = "Connecting...";
			this.Shell = AndroidDevice.Start
			(
				$"-s {this.ID} shell",
				_ =>
				{
					if (this.Connected)
					{
						this.Connected = false;
						this.OnDisconnected?.Invoke(this);
					}
				},
				text => this.Output += text ?? ""
			);

			using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, new CancellationTokenSource(345).Token))
				while (this.Output == "Connecting...")
					try
					{
						await Task.Delay(13, cts.Token).ConfigureAwait(false);
					}
					catch
					{
						break;
					}

			this.Connected = !this.Output.IsContains("not found") && !this.Output.IsContains("device offline");
			if (this.Connected)
			{
				this.Output = $"Connected to '{this.ID}'";
				this.OnConnected?.Invoke(this);
				await this.SetScreenPortraitOrientationAsync(cancellationToken).ConfigureAwait(false);
			}
			else
			{
				this.Output = this.Output.Replace("Connecting...", "").Replace("adb.exe:", "").Replace("adb:", "").Trim();
				this.Shell.Dispose();
				this.Shell = null;
			}
		}

		/// <summary>
		/// Disconnects the device's shell
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task DisconnectAsync(CancellationToken cancellationToken = default)
		{
			await this.SendAsync("exit", 123, cancellationToken).ConfigureAwait(false);
			this.Shell?.Dispose();
			this.Shell = null;
		}

		/// <summary>
		/// Reboots the device
		/// </summary>
		/// <param name="reconnectTimeout"></param>
		/// <param name="onRebootCompleted"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task RebootAsync(int reconnectTimeout, Action<AndroidDevice> onRebootCompleted, CancellationToken cancellationToken = default)
		{
			await this.DisconnectAsync(cancellationToken).ConfigureAwait(false);
			await this.DoAsync("reboot", cancellationToken).ConfigureAwait(false);

			this.Connected = false;
			using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, new CancellationTokenSource(reconnectTimeout > 0 ? reconnectTimeout : 180000).Token))
				while (!this.Connected)
					try
					{
						await Task.Delay(2345, cts.Token).ConfigureAwait(false);
						await this.ConnectAsync(cts.Token).ConfigureAwait(false);
					}
					catch
					{
						break;
					}

			if (this.Connected)
			{
				while (await this.RunAsync("getprop sys.boot_completed", cancellationToken).ConfigureAwait(false) != "1")
					await Task.Delay(456, cancellationToken).ConfigureAwait(false);
				onRebootCompleted?.Invoke(this);
			}
		}

		/// <summary>
		/// Reboots the device
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task RebootAsync(CancellationToken cancellationToken = default)
			=> this.RebootAsync(0, null, cancellationToken);

		/// <summary>
		/// Initializes new a device's connection, sends a command and gets the results (as text)
		/// </summary>
		/// <param name="command"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<string> DoAsync(string command, CancellationToken cancellationToken = default)
		{
			using (var process = Start($"-s {this.ID} {command}"))
				return (await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).Trim();
		}

		/// <summary>
		/// Initializes new a device's shell, runs a command and gets the results (as text)
		/// </summary>
		/// <param name="command"></param>
		/// <param name="asSuperUser"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> RunAsync(string command, bool asSuperUser, CancellationToken cancellationToken = default)
			=> this.DoAsync($"shell {(asSuperUser ? $"su -c \"{command.Replace("\"", "\\\"")}\"" : command)}", cancellationToken);

		/// <summary>
		/// Initializes new a device's shell, runs a command and gets the results (as text)
		/// </summary>
		/// <param name="command"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> RunAsync(string command, CancellationToken cancellationToken = default)
			=> this.RunAsync(command, false, cancellationToken);

		/// <summary>
		/// Sends a command to the device's shell and gets the results (as text)
		/// </summary>
		/// <param name="command"></param>
		/// <param name="timeout"></param>
		/// <param name="asSuperUser"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<string> SendAsync(string command, int timeout, bool asSuperUser, CancellationToken cancellationToken = default)
		{
			async Task sendCommandAsync()
			{
				await Task.WhenAll
				(
					this.Connected && this.Shell != null ? this.Shell.StandardInput.WriteLineAsync(asSuperUser ? $"su -c \"{command.Replace("\"", "\\\"")}\"" : command, cancellationToken) : Task.CompletedTask,
					this.Connected && this.Shell != null && timeout > 0 ? Task.Delay(timeout, cancellationToken) : Task.CompletedTask
				).ConfigureAwait(false);
			}
			this.Output = "";
			try
			{
				await sendCommandAsync().ConfigureAwait(false);
			}
			catch (InvalidOperationException)
			{
				await Task.Delay(345, cancellationToken).ConfigureAwait(false);
				await sendCommandAsync().ConfigureAwait(false);
			}
			catch (Exception)
			{
				throw;
			}
			return this.Output;
		}

		/// <summary>
		/// Sends a command to the device's shell and gets the results (as text)
		/// </summary>
		/// <param name="command"></param>
		/// <param name="timeout"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> SendAsync(string command, int timeout, CancellationToken cancellationToken = default)
			=> this.SendAsync(command, timeout, false, cancellationToken);

		/// <summary>
		/// Sends a command to the device's shell and gets the results (as text)
		/// </summary>
		/// <param name="command"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> SendAsync(string command, CancellationToken cancellationToken = default)
			=> this.SendAsync(command, 345, cancellationToken);

		/// <summary>
		/// Gets the device's name
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<string> GetDeviceNameAsync(CancellationToken cancellationToken = default)
		{
			this.Name = await this.RunAsync("getprop net.hostname", cancellationToken).ConfigureAwait(false);
			if (string.IsNullOrWhiteSpace(this.Name))
				this.Name = await this.RunAsync("settings get global device_name", cancellationToken).ConfigureAwait(false);
			return this.Name;
		}

		/// <summary>
		/// Sets the device's name
		/// </summary>
		/// <param name="name"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task SetDeviceNameAsync(string name, CancellationToken cancellationToken = default)
		{
			this.Name = name;
			await this.RunAsync($"setprop net.hostname \"{this.Name}\"", true, cancellationToken).ConfigureAwait(false);
			await this.RunAsync($"settings put global device_name \"{this.Name}\"", true, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets the screen solution
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<Size> GetScreenResolutionAsync(CancellationToken cancellationToken = default)
		{
			var results = await this.RunAsync($"dumpsys display | grep mCurrentDisplayRect", cancellationToken).ConfigureAwait(false);
			if (results != "")
				try
				{
					results = results.Right(results.Length - results.PositionOf("=")).Trim();
					results = results.Right(results.Length - results.PositionOf("-")).Trim();
					var size = results.Replace(")", "").Replace("-", "").Replace(" ", "").ToArray(",");
					this.ScreenResolution = new Size(size[0].As<int>(), size[1].As<int>());
				}
				catch { }
			return this.ScreenResolution;
		}

		/// <summary>
		/// Sets the screen automatic rotation
		/// </summary>
		/// <param name="state"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SetScreenAutomaticRotationAsync(string state, CancellationToken cancellationToken = default)
			=> this.RunAsync($"content insert --uri content://settings/system --bind name:s:accelerometer_rotation --bind value:i:{(state == null || state.IsEquals("false") || state.IsEquals("off") || state == "0" ? "0" : "1")}", true, cancellationToken);

		/// <summary>
		/// Sets the screen orientation mode
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task SetScreenOrientationAsync(string mode, CancellationToken cancellationToken = default)
		{
			await this.SetScreenAutomaticRotationAsync("off", cancellationToken).ConfigureAwait(false);
			await this.RunAsync($"content insert --uri content://settings/system --bind name:s:user_rotation --bind value:i:{(mode == null || mode == "0" || mode.IsEquals("portrait") ? "0" : "1")}", true, cancellationToken).ConfigureAwait(false);
			await this.GetScreenResolutionAsync(cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Sets the screen orientation mode is landscape
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SetScreenLandscapeOrientationAsync(CancellationToken cancellationToken = default)
			=> this.SetScreenOrientationAsync("landscape", cancellationToken);

		/// <summary>
		/// Sets the screen orientation mode is portrait
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SetScreenPortraitOrientationAsync(CancellationToken cancellationToken = default)
			=> this.SetScreenOrientationAsync("portrait", cancellationToken);

		/// <summary>
		/// Opens an app
		/// </summary>
		/// <param name="appID"></param>
		/// <param name="waiting"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> OpenAppAsync(string appID, int waiting = 1234, bool asSuperUser = false, CancellationToken cancellationToken = default)
			=> this.SendAsync($"monkey -p {appID} -c android.intent.category.LAUNCHER 1", waiting, asSuperUser, cancellationToken);

		/// <summary>
		/// Forces stop a package (means force stop an app)
		/// </summary>
		/// <param name="appID"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> ForceStopAppAsync(string appID, CancellationToken cancellationToken = default)
			=> this.SendAsync($"am force-stop {appID}", cancellationToken);

		/// <summary>
		/// Clears data of a package (means clear cache and storage of an app)
		/// </summary>
		/// <param name="appID"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> ClearAppDataAsync(string appID, CancellationToken cancellationToken = default)
			=> this.SendAsync($"pm clear {appID}", cancellationToken);

		/// <summary>
		/// Installs an app
		/// </summary>
		/// <param name="apkFilePath"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<string> InstallAppAsync(string apkFilePath, CancellationToken cancellationToken = default)
		{
			await this.SendAsync("settings put global verifier_verify_adb_installs 0", cancellationToken).ConfigureAwait(false);
			return await this.DoAsync($"install {apkFilePath}", cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Uninstalls an app
		/// </summary>
		/// <param name="appID"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> UninstallAppAsync(string appID, CancellationToken cancellationToken = default)
			=> this.DoAsync($"uninstall {appID}", cancellationToken);

		/// <summary>
		/// Gets the version number of an app
		/// </summary>
		/// <param name="appID"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<string> GetAppVersionAsync(string appID, CancellationToken cancellationToken = default)
		{
			var versionInfo = await this.RunAsync($"dumpsys package {appID} | grep versionName", cancellationToken).ConfigureAwait(false);
			return string.IsNullOrWhiteSpace(versionInfo) ? null : versionInfo.ToArray("=").Last().Trim();
		}

		/// <summary>
		/// Sends text into current focused control
		/// </summary>
		/// <param name="text"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task<string> InputTextAsync(string text, CancellationToken cancellationToken = default)
		{
			new[] { "&", "<", ">", "?", ":", "{", "}", "[", "]", "|", "'" }.ForEach(@char => text = text.Replace(@char, $"\\{@char}"));
			return this.SendAsync($"input text \"{text.Replace("\"", "\\\"").Replace(" ", "%s")}\"", 456, cancellationToken);
		}

		/// <summary>
		/// Sends a key event
		/// </summary>
		/// <param name="keyEvent"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SendKeyEventAsync(string keyEvent, CancellationToken cancellationToken = default)
			=> this.SendAsync($"input keyevent {keyEvent}", cancellationToken);

		/// <summary>
		/// Swipes the device screen
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="duration"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SwipeAsync(Point start, Point end, int duration, CancellationToken cancellationToken = default)
			=> this.SendAsync($"input swipe {start.X} {start.Y} {end.X} {end.Y} {duration}", cancellationToken);

		/// <summary>
		/// Swipes the device screen
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SwipeAsync(Point start, Point end,  CancellationToken cancellationToken = default)
			=> this.SwipeAsync(start, end, 456, cancellationToken);

		/// <summary>
		/// Swipes the device screen (by percentage)
		/// </summary>
		/// <param name="startX"></param>
		/// <param name="startY"></param>
		/// <param name="endX"></param>
		/// <param name="endY"></param>
		/// <param name="duration"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SwipeByPercentageAsync(double startX, double startY, double endX, double endY, int duration = 456, CancellationToken cancellationToken = default)
			=> this.SwipeAsync(new Point((int)(startX * (this.ScreenResolution.Width * 1.0 / 100.0)), (int)(startY * (this.ScreenResolution.Height * 1.0 / 100.0))), new Point((int)(endX * (this.ScreenResolution.Width * 1.0 / 100.0)), (int)(endY * (this.ScreenResolution.Height * 1.0 / 100.0))), duration, cancellationToken);

		/// <summary>
		/// Swipes the device screen up
		/// </summary>
		/// <param name="start"></param>
		/// <param name="distance"></param>
		/// <param name="duration"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SwipeUpAsync(Point start, int distance, int duration = 456, CancellationToken cancellationToken = default)
			=> this.SwipeAsync(start, new Point(start.X, start.Y - distance), duration, cancellationToken);

		/// <summary>
		/// Swipes the device screen down
		/// </summary>
		/// <param name="start"></param>
		/// <param name="distance"></param>
		/// <param name="duration"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task SwipeDownAsync(Point start, int distance, int duration = 456, CancellationToken cancellationToken = default)
			=> this.SwipeAsync(start, new Point(start.X, start.Y + distance), duration, cancellationToken);

		/// <summary>
		/// Taps the screen
		/// </summary>
		/// <param name="postion"></param>
		/// <param name="tapTimes"></param>
		/// <param name="waitingTimes"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task TapAsync(Point postion, int tapTimes, int waitingTimes, CancellationToken cancellationToken = default)
		{
			await this.SendAsync($"input tap {postion.X} {postion.Y}", cancellationToken).ConfigureAwait(false);
			for (var counter = 1; counter < tapTimes; counter++)
			{
				await Task.Delay(waitingTimes, cancellationToken).ConfigureAwait(false);
				await this.SendAsync($"input tap {postion.X} {postion.Y}", cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Taps the screen
		/// </summary>
		/// <param name="postion"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task TapAsync(Point postion, CancellationToken cancellationToken = default)
			=> this.TapAsync(postion, 1, 0, cancellationToken);

		/// <summary>
		/// Taps the screen (by percentage)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="tapTimes"></param>
		/// <param name="waitingTimes"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task TapByPercentageAsync(double x, double y, int tapTimes, int waitingTimes, CancellationToken cancellationToken = default)
			=> this.TapAsync(new Point((int)(x * (this.ScreenResolution.Width * 1.0 / 100.0)), (int)(y * (this.ScreenResolution.Height * 1.0 / 100.0))), tapTimes, waitingTimes, cancellationToken);

		/// <summary>
		/// Taps the screen (by percentage)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task TapByPercentageAsync(double x, double y, CancellationToken cancellationToken = default)
			=> this.TapByPercentageAsync(x, y, 1, 0, cancellationToken);

		/// <summary>
		/// Holds the screen (means tap and hold in a certain duration)
		/// </summary>
		/// <param name="postion"></param>
		/// <param name="duration"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task HoldAsync(Point postion, int duration, CancellationToken cancellationToken = default)
			=> this.SwipeAsync(postion, postion, duration, cancellationToken);

		/// <summary>
		/// Holds the screen (means tap and hold in a certain duration)
		/// </summary>
		/// <param name="postion"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task HoldAsync(Point postion, CancellationToken cancellationToken = default)
			=> this.HoldAsync(postion, 456, cancellationToken);

		/// <summary>
		/// Holds the screen (means tap and hold in a certain duration - by percentage)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="duration"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task HoldByPercentageAsync(double x, double y, int duration, CancellationToken cancellationToken = default)
			=> this.HoldAsync(new Point((int)(x * (this.ScreenResolution.Width * 1.0 / 100.0)), (int)(y * (this.ScreenResolution.Height * 1.0 / 100.0))), duration, cancellationToken);

		/// <summary>
		/// Holds the screen (means tap and hold in a certain duration - by percentage)
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public Task HoldByPercentageAsync(double x, double y, CancellationToken cancellationToken = default)
			=> this.HoldByPercentageAsync(x, y, 456, cancellationToken);

		static HashSet<string> BeRemoved { get; } = new[] { "index", "class", "package", "checkable", "checked", "enabled", "focusable", "focused", "scrollable", "long-clickable", "password", "selected" }.ToHashSet();

		static XElement Normalize(XElement control)
		{
			var name = control.Attribute("class")?.Value;
			name = string.IsNullOrWhiteSpace(name) ? control.Elements()?.FirstOrDefault()?.Attribute("class")?.Value : name;
			name = string.IsNullOrWhiteSpace(name) ? control.Name.LocalName	: name;
			control.Name = name;
			var attributes = control.Attributes().Where(attribute => BeRemoved.Contains(attribute.Name.LocalName)).ToList();
			attributes.ForEach(attribute => attribute.Remove());
			control.Elements().ForEach(child => Normalize(child));
			return control;
		}

		/// <summary>
		/// Dumps the device UI to XML
		/// </summary>
		/// <param name="toLowerCase"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="MethodNotAllowedException"></exception>
		public async Task<XElement> DumpAsync(bool toLowerCase, CancellationToken cancellationToken = default)
		{
			var results = await this.RunAsync("uiautomator dump", cancellationToken).ConfigureAwait(false);
			if (results.IsContains("Killed"))
				throw new MethodNotAllowedException("UIAutomator was be killed");
			results = await this.RunAsync("cat sdcard/window_dump.xml", cancellationToken).ConfigureAwait(false);
			results = string.IsNullOrWhiteSpace(results) ? null : toLowerCase ? results.Trim().ToLower() : results.Trim();
			return results != null ? Normalize(results.ToXml(xml => xml.Add(new XAttribute("class", "android.UI")))) : null;
		}

		/// <summary>
		/// Dumps the device UI to XML
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="MethodNotAllowedException"></exception>
		public Task<XElement> DumpAsync(CancellationToken cancellationToken = default)
			=> this.DumpAsync(true, cancellationToken);

		/// <summary>
		/// Initializes new a device's shell, captures the screen and gets the PNG image
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<MemoryStream> CaptureAsync(CancellationToken cancellationToken = default)
		{
			var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
			var filePath = isWindows ? null : Path.Combine(".", $"_screencap_{DateTime.Now:yyyyMMddHHmmss}_{UtilityService.GetRandomNumber()}.png");
			using (var process = AndroidDevice.Start
			(
				isWindows ? $"-s {this.ID} shell screencap -p" : $"{AndroidDevice.GetPlatformToolFilePath("adb")} -s {this.ID} exec-out screencap -p > {filePath}",
				null,
#if NETSTANDARD2_0
				isWindows ? null as Action<string> : _ => { },
#else
				isWindows ? null : _ => { },
#endif
				isWindows ? null : "/bin/bash"
			))
			{
				var bytes = Array.Empty<byte>();
				if (isWindows)
				{
					bytes = new byte[10240];
					var data = new List<byte>();
					var isCR = false;
					var read = 0;
					do
					{
						read = await process.StandardOutput.BaseStream.ReadAsync(bytes, cancellationToken).ConfigureAwait(false);
						for (var index = 0; index < read; index++) // convert CRLF to LF
						{
							if (isCR && bytes[index] == 0x0A)
							{
								isCR = false;
								data.RemoveAt(data.Count - 1);
								data.Add(bytes[index]);
								continue;
							}
							isCR = bytes[index] == 0x0D;
							data.Add(bytes[index]);
						}
					}
					while (read > 0);
					bytes = data.ToArray();
				}
				else
				{
#if NETSTANDARD2_0
					process.WaitForExit();
#else
					await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
#endif
					var fileInfo = new FileInfo(filePath);
					if (fileInfo.Exists)
					{
						bytes = await fileInfo.ReadAsBinaryAsync(cancellationToken).ConfigureAwait(false);
						fileInfo.Delete();
					}
				}
				return bytes.ToMemoryStream();
			}
		}
	}
}