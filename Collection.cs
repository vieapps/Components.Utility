#region Related components
using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Numerics;
using System.Xml;
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using net.vieapps.Components.Utility;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with collections
	/// </summary>
	public static partial class CollectionService
	{

		#region LINQ Extensions
		/// <summary>
		/// Performs the specified action on each element of the collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
		{
			foreach (var item in enumerable)
				action(item);
		}

		/// <summary>
		/// Performs the specified action on each element of the collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
		{
			var index = -1;
			foreach (var item in enumerable)
			{
				index++;
				action(item, index);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<TValue> action)
		{
			foreach (var kvp in dictionary)
				action(kvp.Value);
		}

		/// <summary>
		/// Performs the specified action on each element of the collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<TValue, int> action)
		{
			var index = -1;
			foreach (var kvp in dictionary)
			{
				index++;
				action(kvp.Value, index);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<KeyValuePair<TKey, TValue>> action)
		{
			foreach (var kvp in dictionary)
				action(kvp);
		}

		/// <summary>
		/// Performs the specified action on each element of the collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<KeyValuePair<TKey, TValue>, int> action)
		{
			var index = -1;
			foreach (var kvp in dictionary)
			{
				index++;
				action(kvp, index);
			}
		}

		/// <summary>
		///  Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, Task> actionAsync, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			if (!parallelExecutions)
				foreach (var item in enumerable)
					await actionAsync(item).ConfigureAwait(captureContext);

			else
			{
				var tasks = enumerable.Select(item => actionAsync(item)).ToList();
				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		///  Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, CancellationToken, Task> actionAsync, CancellationToken cancellationToken, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			if (!parallelExecutions)
				foreach (var item in enumerable)
					await actionAsync(item, cancellationToken).ConfigureAwait(captureContext);

			else
			{
				var tasks = enumerable.Select(item => actionAsync(item, cancellationToken)).ToList();
				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<T>(this IEnumerable<T> dictionary, Func<T, int, Task> actionAsync, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
				{
					index++;
					await actionAsync(kvp, index).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var kvp in dictionary)
				{
					index++;
					tasks.Add(actionAsync(kvp, index));
				}

				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<T>(this IEnumerable<T> dictionary, Func<T, int, CancellationToken, Task> actionAsync, CancellationToken cancellationToken, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
				{
					index++;
					await actionAsync(kvp, index, cancellationToken).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var kvp in dictionary)
				{
					index++;
					tasks.Add(actionAsync(kvp, index, cancellationToken));
				}

				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		///  Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TValue, Task> actionAsync, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
					await actionAsync(kvp.Value).ConfigureAwait(captureContext);

			else
			{
				var tasks = dictionary.Select(kvp => actionAsync(kvp.Value)).ToList();
				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		///  Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TValue, CancellationToken, Task> actionAsync, CancellationToken cancellationToken, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
					await actionAsync(kvp.Value, cancellationToken).ConfigureAwait(captureContext);

			else
			{
				var tasks = dictionary.Select(kvp => actionAsync(kvp.Value, cancellationToken)).ToList();
				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TValue, int, Task> actionAsync, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
				{
					index++;
					await actionAsync(kvp.Value, index).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var kvp in dictionary)
				{
					index++;
					tasks.Add(actionAsync(kvp.Value, index));
				}

				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TValue, int, CancellationToken, Task> actionAsync, CancellationToken cancellationToken, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
				{
					index++;
					await actionAsync(kvp.Value, index, cancellationToken).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var kvp in dictionary)
				{
					index++;
					tasks.Add(actionAsync(kvp.Value, index, cancellationToken));
				}

				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		///  Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, Task> actionAsync, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
					await actionAsync(kvp).ConfigureAwait(captureContext);

			else
			{
				var tasks = dictionary.Select(kvp => actionAsync(kvp)).ToList();
				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		///  Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, CancellationToken, Task> actionAsync, CancellationToken cancellationToken, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
					await actionAsync(kvp, cancellationToken).ConfigureAwait(captureContext);

			else
			{
				var tasks = dictionary.Select(kvp => actionAsync(kvp, cancellationToken)).ToList();
				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, int, Task> actionAsync, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
				{
					index++;
					await actionAsync(kvp, index).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var kvp in dictionary)
				{
					index++;
					tasks.Add(actionAsync(kvp, index));
				}

				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, int, CancellationToken, Task> actionAsync, CancellationToken cancellationToken, bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var kvp in dictionary)
				{
					index++;
					await actionAsync(kvp, index, cancellationToken).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var kvp in dictionary)
				{
					index++;
					tasks.Add(actionAsync(kvp, index, cancellationToken));
				}

				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}
		#endregion

		#region String conversions
		/// <summary>
		/// Converts this string to an array
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static string[] ToArray(this string @string, string separator = ",", bool removeEmptyElements = false, bool trim = true)
			=> @string == null
				? Array.Empty<string>()
				: (trim ? @string.Trim() : @string).Split(new[] { separator ?? "," }, removeEmptyElements ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
					.Where(e => !removeEmptyElements || !string.IsNullOrWhiteSpace(e))
					.Select(e => trim ? e.Trim() : e)
					.ToArray();

		/// <summary>
		/// Converts this string to an array
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static string[] ToArray(this string @string, char separator, bool removeEmptyElements = false, bool trim = true)
			=> @string == null
				? Array.Empty<string>()
				: @string.ToArray($"{separator}", removeEmptyElements, trim);

		/// <summary>
		/// Converts this string to an array
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static List<string> ToList(this string @string, string separator = ",", bool removeEmptyElements = false, bool trim = true)
			=> @string == null
				? new List<string>()
				: @string.ToArray(separator, removeEmptyElements, trim).ToList();

		/// <summary>
		/// Converts this string to an array
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static List<string> ToList(this string @string, char separator, bool removeEmptyElements = false, bool trim = true)
			=> @string.ToList(separator.ToString(), removeEmptyElements, trim);

		/// <summary>
		/// Converts this string to a hash-set
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public static HashSet<string> ToHashSet(this string @string, string separator = ",", bool removeEmptyElements = false, bool trim = true, IEqualityComparer<string> comparer = null)
			=> new HashSet<string>(@string.ToArray(separator, removeEmptyElements, trim), comparer ?? StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Converts this string to a hash-set
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public static HashSet<string> ToHashSet(this string @string, char separator, bool removeEmptyElements = false, bool trim = true, IEqualityComparer<string> comparer = null)
			=> @string.ToHashSet(separator.ToString(), removeEmptyElements, trim, comparer);

		/// <summary>
		/// Converts this collection to string
		/// </summary>
		/// <param name="object"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string ToString(this IEnumerable<string> @object, string separator)
			=> string.Join(separator ?? "", @object);

		/// <summary>
		/// Converts this collection to string
		/// </summary>
		/// <param name="object"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string Join(this IEnumerable<string> @object, string separator)
			=> string.Join(separator ?? "", @object);

		/// <summary>
		/// Converts this collection to string
		/// </summary>
		/// <param name="object"></param>
		/// <param name="separator"></param>
		/// <param name="converter"></param>
		/// <returns></returns>
		public static string ToString<T>(this IEnumerable<T> @object, string separator, Func<T, string> converter = null)
			=> @object.Select(obj => obj != null ? converter != null ? converter(obj) : obj.ToString() : "null").ToString(separator);

		/// <summary>
		/// Converts this collection to string
		/// </summary>
		/// <param name="object"></param>
		/// <param name="elementSeparator"></param>
		/// <param name="valueSeparator"></param>
		/// <returns></returns>
		public static string ToString(this NameValueCollection @object, string elementSeparator, string valueSeparator = null)
		{
			var @string = "";
			foreach (string key in @object)
				@string += (!@string.Equals("") ? (elementSeparator ?? "") : "") + key + (valueSeparator ?? ": ") + @object[key];
			return @string;
		}
		#endregion

		#region Get first/last element
		/// <summary>
		/// Gets the first element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T First<T>(this T[] @object)
			=> @object.Length > 0
				? @object[0]
				: default;

		/// <summary>
		/// Gets the first element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T First<T>(this IList<T> @object)
			=> @object.Count > 0
				? @object[0]
				: default;

		/// <summary>
		/// Gets the first element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T First<T>(this Collection @object)
			=> @object.Count > 0
				? (T)@object[0]
				: default;

		/// <summary>
		/// Gets the last element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T Last<T>(this T[] @object)
			=> @object.Length > 0
				? @object[@object.Length - 1]
				: default;

		/// <summary>
		/// Gets the last element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T Last<T>(this IList<T> @object)
			=> @object.Count > 0
				? @object[@object.Count - 1]
				: default;

		/// <summary>
		/// Gets the last element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T Last<T>(this Collection @object)
			=> @object.Count > 0
				? (T)@object[@object.Count - 1]
				: default;
		#endregion

		#region Manipulations
		/// <summary>
		/// Concats other arrays with this array object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="arrays"></param>
		/// <returns></returns>
		public static T[] Concat<T>(this T[] @object, params T[][] arrays)
		{
			if (typeof(T).IsPrimitive)
			{
				var data = arrays.Where(array => array != null);
				var result = new T[@object.Length + data.Sum(array => array.Length)];
				if (@object.Length > 0)
#pragma warning disable CA2018 // 'Buffer.BlockCopy' expects the number of bytes to be copied for the 'count' argument
                    Buffer.BlockCopy(@object, 0, result, 0, @object.Length);
#pragma warning restore CA2018 // 'Buffer.BlockCopy' expects the number of bytes to be copied for the 'count' argument
                var offset = @object.Length;
				data.ForEach(array =>
				{
#pragma warning disable CA2018 // 'Buffer.BlockCopy' expects the number of bytes to be copied for the 'count' argument
                    Buffer.BlockCopy(array, 0, result, offset, array.Length);
#pragma warning restore CA2018 // 'Buffer.BlockCopy' expects the number of bytes to be copied for the 'count' argument
                    offset += array.Length;
				});
				return result;
			}
			else
			{
				var result = @object.Select(array => array);
				arrays.ForEach(array => result = result.Concat(array));
				return result.ToArray();
			}
		}

		/// <summary>
		/// Concats all elements of this queue object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static IEnumerable<T> Concat<T>(this Queue<T[]> @object)
		{
			var result = new List<T>().Select(o => o);
			if (@object != null)
				while (@object.Count > 0)
					result = result.Concat(@object.Dequeue());
			return result;
		}

		/// <summary>
		/// Concats all elements of this concurrent queue object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static IEnumerable<T> Concat<T>(this ConcurrentQueue<T[]> @object)
		{
			var result = new List<T>().Select(o => o);
			if (@object != null)
				while (!@object.IsEmpty)
					if (@object.TryDequeue(out var array))
						result = result.Concat(array);
			return result;
		}

		/// <summary>
		/// Takes a sub-array from this array object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static T[] Take<T>(this T[] @object, int offset, int count = 0)
		{
			if (@object.Length < 1 || (offset == 0 && count == @object.Length))
				return @object;

			offset = offset > -1 && offset < @object.Length ? offset : 0;
			count = count > 0 && count < @object.Length - offset ? count : @object.Length - offset;
			if (offset == 0 && count == @object.Length)
				return @object;

			if (typeof(T).IsPrimitive)
			{
				var result = new T[count];
				Buffer.BlockCopy(@object, offset, result, 0, count);
				return result;
			}
			else
				return @object.Skip(offset).Take(count).ToArray();
		}

		/// <summary>
		/// Takes the array from this array segment
		/// </summary>
		/// <param name="segment"></param>
		/// <returns></returns>
		public static T[] Take<T>(this ArraySegment<T> segment)
			=> segment.Array.Take(segment.Offset, segment.Count);

		/// <summary>
		/// Splits this array to sub-arrays
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="size">The size (length) of one sub-array</param>
		/// <returns></returns>
		public static List<T[]> Split<T>(this T[] @object, int size)
		{
			if (@object.Length < 1 || size >= @object.Length)
				return new List<T[]> { @object };

			else if (size < 2)
				return @object.Select(e => new[] { e }).ToList();

			var arrays = new List<T[]>();
			var offset = 0;
			do
			{
				arrays.Add(@object.Take(offset, size));
				offset += size;
				if (offset + size > @object.Length - 1)
				{
					arrays.Add(@object.Take(offset, @object.Length - offset));
					offset += @object.Length - offset;
				}
			}
			while (offset < @object.Length - 1);

			return arrays;
		}

		/// <summary>
		/// Splits this array segment to sub-arrays
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="segment"></param>
		/// <param name="size">The size (length) of one sub-array</param>
		/// <returns></returns>
		public static List<ArraySegment<T>> Split<T>(this ArraySegment<T> segment, int size)
			=> segment.Take().Split(size).Select(array => new ArraySegment<T>(array)).ToList();

#if NETSTANDARD2_0
		/// <summary>
		/// Attempts to add the specified key and value to the dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="key">The key of the element to add</param>
		/// <param name="value">The value of the element to add. It can be null</param>
		/// <returns>true if the key/value pair was added to the dictionary successfully; otherwise false.</returns>
		public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> @object, TKey key, TValue value)
		{
			if (@object != null && key != null && !@object.ContainsKey(key))
				try
				{
					@object.Add(key, value);
					return true;
				}
				catch { }
			return key == null ? throw new ArgumentNullException(nameof(key), "The key is null") : false;
		}

		/// <summary>
		/// Removes the value with the specified key from the dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="key">The key of the element to remove</param>
		/// <param name="value">The element that be removed from dictionary</param>
		/// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the dictionary.</returns>
		public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> @object, TKey key, out TValue value)
		{
			if (@object != null && key != null && @object.ContainsKey(key))
			{
				value = @object[key];
				return @object.Remove(key);
			}
			value = default;
			return false;
		}
#endif

		/// <summary>
		/// Attempts to add the value that has the specified key into this dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="key">The key of the element to remove</param>
		/// <returns>true if the object was removed successfully; otherwise, false.</returns>
		public static bool Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @object, TKey key, TValue value)
			=> @object != null && @object.TryAdd(key, value);

		/// <summary>
		/// Attempts to remove the value that has the specified key from this dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="key">The key of the element to remove</param>
		/// <returns>true if the object was removed successfully; otherwise, false.</returns>
		public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @object, TKey key)
			=> @object != null && @object.TryRemove(key, out var value);

		/// <summary>
		/// Gets the value associated with the specified key from this dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="key">The key of the value to get</param>
		/// <param name="default"></param>
		/// <returns>When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter</returns>
		public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> @object, TKey key, TValue @default = default)
			=> @object != null && key != null && @object.TryGetValue(key, out var value)
				? value
				: @default;

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="allowDuplicated">true to allow duplicated item existed in the collection</param>
		/// <param name="items">Items to append</param>
		public static void Append<T>(this IList<T> @object, bool allowDuplicated, IEnumerable<T> items)
			=> items?.ForEach(item =>
			{
				if (allowDuplicated)
					@object.Add(item);
				else if (@object.IndexOf(item) < 0)
					@object.Add(item);
			});

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="items">Items to append</param>
		public static void Append<T>(this IList<T> @object, IEnumerable<T> items)
			=> @object.Append(true, items);

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="allowDuplicated">true to allow items can be duplicated, false if otherwise</param>
		/// <param name="lists"></param>
		public static void Append<T>(this IList<T> @object, bool allowDuplicated, params IEnumerable<T>[] lists)
			=> lists?.ForEach(list => @object.Append(allowDuplicated, list));

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="lists"></param>
		public static void Append<T>(this IList<T> @object, params IEnumerable<T>[] lists)
			=> @object.Append(true, lists);

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="item"></param>
		public static void Append<T>(this ISet<T> @object, T item)
		{
			if (!@object.Contains(item))
				@object.Add(item);
		}

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="items"></param>
		public static void Append<T>(this ISet<T> @object, IEnumerable<T> items)
			=> items?.ForEach(item => @object.Append(item));

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="lists"></param>
		public static void Append<T>(this ISet<T> @object, params IEnumerable<T>[] lists)
			=> lists?.ForEach(list => @object.Append(list));

		/// <summary>
		/// Swaps the two-elements by the specified indexes
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="first">The zero-based index of the first element to swap</param>
		/// <param name="second">The zero-based index of the second element to swap</param>
		/// <returns>true if swap success.</returns>
		public static bool Swap<T>(this IList<T> @object, int first, int second)
		{
			if (first != second && first > -1 && first < @object.Count && second > -1 && second < @object.Count)
			{
				var element = @object[second];
				@object[second] = @object[first];
				@object[first] = element;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Randomizes (swap items with random indexes)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		public static IList<T> Randomize<T>(this IList<T> @object)
		{
			if (@object.Count.Equals(2))
				@object.Swap(0, 1);

			else
			{
				var random = new SortedList();
				@object.ForEach((o, i) => random.Add(UtilityService.GetRandomNumber(), i));

				var maxIndex = random.Count / 2;
				if (random.Count % 2 == 1)
					maxIndex++;
				var oldIndex = 0;
				while (oldIndex < maxIndex)
				{
					var newIndex = (int)random.GetByIndex(oldIndex);
					@object.Swap(oldIndex, newIndex);
					oldIndex++;
				}
			}

			return @object;
		}
		#endregion

		#region Conversions
		/// <summary>
		/// Creates a list from this collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static List<T> ToList<T>(this Collection @object)
			=> @object?.AsEnumerableDictionaryEntry.Select(entry => (T)entry.Value).ToList();

		/// <summary>
		/// Creates a list of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static List<T> ToList<T>(this JArray json, Func<JToken, T> converter = null)
			=> json?.Select(token => converter != null ? converter(token) : token.FromJson<T>())?.ToList() ?? new List<T>();

		/// <summary>
		/// Creates a list of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static List<T> ToList<T>(this JObject json, Func<JToken, T> converter = null)
		{
			var list = new List<T>();
			json?.ForEach(token => list.Add(converter != null ? converter(token) : token.FromJson<T>()));
			return list;
		}

		/// <summary>
		/// Creates a list of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static List<T> ToList<T>(this JObject json, Func<KeyValuePair<string, JToken>, T> converter = null)
		{
			var list = new List<T>();
			json?.ForEach(kvp => list.Add(converter != null ? converter(kvp) : kvp.Value.FromJson<T>()));
			return list;
		}

		/// <summary>
		/// Converts a list of XML nodes to list of XML nodes
		/// </summary>
		/// <param name="xmlNodes"></param>
		/// <returns></returns>
		public static List<XmlNode> ToList(this XmlNodeList xmlNodes)
		{
			var nodes = new List<XmlNode>();
			if (xmlNodes != null)
				foreach (XmlNode xmlNode in xmlNodes)
					nodes.Add(xmlNode);
			return nodes;
		}

		/// <summary>
		/// Converts a list of XML attributes to list of XML attributes
		/// </summary>
		/// <param name="xmlAttributes"></param>
		/// <returns></returns>
		public static List<XmlAttribute> ToList(this XmlAttributeCollection xmlAttributes)
		{
			var attributes = new List<XmlAttribute>();
			if (xmlAttributes != null)
				foreach (XmlAttribute xmlAttribute in xmlAttributes)
					attributes.Add(xmlAttribute);
			return attributes;
		}

		/// <summary>
		/// Creates a hash-set from this collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="checkDuplicated"></param>
		/// <returns></returns>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> @object, bool checkDuplicated = true)
		{
			if (@object is HashSet<T>)
				return @object as HashSet<T>;

			var set = new HashSet<T>(checkDuplicated ? Array.Empty<T>() : @object);
			if (checkDuplicated)
				set.Append(@object);

			return set;
		}

		/// <summary>
		/// Creates a hash-set from this collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="checkDuplicated"></param>
		/// <returns></returns>
		public static HashSet<T> ToHashSet<T>(this Collection @object, bool checkDuplicated = true)
			=> @object?.AsEnumerableDictionaryEntry.Select(entry => (T)entry.Value).ToHashSet(checkDuplicated);

		/// <summary>
		/// Creates a dictionary from this collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="onPreCompleted"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this Collection @object, Action<Dictionary<TKey, TValue>> onPreCompleted = null)
		{
			var dictionary = new Dictionary<TKey, TValue>();
			if (@object != null)
			{
				var enumerator = @object.GetEnumerator();
				while (enumerator.MoveNext())
					dictionary.Add((TKey)enumerator.Entry.Key, (TValue)enumerator.Entry.Value);
			}
			onPreCompleted?.Invoke(dictionary);
			return dictionary;
		}

		/// <summary>
		/// Creates a dictionary from this collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="onPreCompleted"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this Collection<TKey, TValue> @object, Action<Dictionary<TKey, TValue>> onPreCompleted = null)
		{
			var dictionary = new Dictionary<TKey, TValue>();
			if (@object != null)
			{
				var enumerator = @object.GetEnumerator();
				while (enumerator.MoveNext())
					dictionary.Add(enumerator.Current.Key, enumerator.Current.Value);
			}
			onPreCompleted?.Invoke(dictionary);
			return dictionary;
		}

		/// <summary>
		/// Creates a dictionary from this collection with all keys in lower case
		/// </summary>
		/// <param name="nvCollection"></param>
		/// <param name="onCompleted"></param>
		/// <param name="stringComparer"></param>
		/// <returns></returns>
		public static Dictionary<string, string> ToDictionary(this NameValueCollection nvCollection, Action<Dictionary<string, string>> onCompleted = null, StringComparer stringComparer = null)
		{
			var dictionary = new Dictionary<string, string>(stringComparer ?? StringComparer.OrdinalIgnoreCase);
			if (nvCollection != null)
				foreach (string key in nvCollection)
					dictionary[key.ToLower()] = nvCollection[key];
			onCompleted?.Invoke(dictionary);
			return dictionary;
		}

		/// <summary>
		/// Creates a dictionary from from this JSON
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this JObject json, string keyAttribute, Func<JToken, TValue> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");

			var dictionary = new Dictionary<TKey, TValue>();
			json?.ForEach(token =>
			{
				var @object = converter != null ? converter(token) : token.FromJson<TValue>();
				var key = (TKey)@object.GetAttributeValue(keyAttribute);
				if (key != null)
					dictionary.TryAdd(key, @object);
			});
			return dictionary;
		}

		/// <summary>
		/// Creates a dictionary of objects from this JSON
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this JArray json, string keyAttribute, Func<JToken, TValue> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException("keyAttribute", "The name of key attribute is null");

			var dictionary = new Dictionary<TKey, TValue>();
			json?.ForEach(token =>
			{
				var @object = converter != null ? converter(token) : token.FromJson<TValue>();
				var key = (TKey)@object.GetAttributeValue(keyAttribute);
				if (key != null && !dictionary.ContainsKey(key))
					dictionary.Add(key, @object);
			});
			return dictionary;
		}

		/// <summary>
		/// Creates a dictionary from from this JSON
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this JObject json, string keyAttribute, Func<KeyValuePair<string, JToken>, TValue> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");

			var dictionary = new Dictionary<TKey, TValue>();
			json?.ForEach(kvp =>
			{
				var @object = converter != null ? converter(kvp) : kvp.Value.FromJson<TValue>();
				var key = (TKey)@object.GetAttributeValue(keyAttribute);
				if (key != null)
					dictionary.TryAdd(key, @object);
			});
			return dictionary;
		}

		/// <summary>
		/// Converts this header to a dictionary of string
		/// </summary>
		/// <param name="headers"></param>
		/// <param name="onCompleted"></param>
		/// <returns></returns>
		public static Dictionary<string, string> ToDictionary(this HttpHeaders headers, Action<Dictionary<string, string>> onCompleted = null)
		{
			var dictionary = headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Join(","), StringComparer.OrdinalIgnoreCase);
			onCompleted?.Invoke(dictionary);
			return dictionary;
		}

		/// <summary>
		/// Converts this dictionary to the name and value collection
		/// </summary>
		/// <param name="dictionary"></param>
		/// <returns></returns>
		public static NameValueCollection ToNameValueCollection(this Dictionary<string, string> dictionary)
		{
			var nvCollection = new NameValueCollection();
			dictionary?.ForEach(entry => nvCollection.Add(entry.Key, entry.Value));
			return nvCollection;
		}

		/// <summary>
		/// Creates a name-value collection from JSON object
		/// </summary>
		/// <param name="json"></param>
		public static NameValueCollection ToNameValueCollection(this JObject json)
		{
			var nvCollection = new NameValueCollection();
			json?.ForEach(kvp =>
			{
				if (kvp.Value != null && kvp.Value is JValue && (kvp.Value as JValue).Value != null)
				{
					if (nvCollection[kvp.Key] != null)
						nvCollection.Set(kvp.Key, (kvp.Value as JValue).Value.ToString());
					else
						nvCollection.Add(kvp.Key, (kvp.Value as JValue).Value.ToString());
				}
			});
			return nvCollection;
		}

		/// <summary>
		/// Creates a collection of enums from this collection of enum-strings
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enums"></param>
		/// <returns></returns>
		public static IEnumerable<T> ToEnums<T>(this IEnumerable<string> enums)
			=> enums?.Select(@enum => @enum.ToEnum<T>());

		/// <summary>
		/// Converts the array to a enumerable collection of objects
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static IEnumerable<object> ToEnumerable(this Array array)
		{
			var enumerable = new List<object>();
			if (array != null)
				foreach (var @object in array)
					enumerable.Add(@object);
			return enumerable;
		}

		/// <summary>
		/// Creates a collection from this hash-set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this HashSet<T> @object, string keyAttribute)
			=> (@object as IEnumerable<T>)?.ToCollection(keyAttribute);

		/// <summary>
		/// Creates a collection from this hash-set
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <param name="onPreCompleted"></param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this HashSet<TValue> @object, string keyAttribute, Action<Collection<TKey, TValue>> onPreCompleted = null)
			=> (@object as IEnumerable<TValue>)?.ToCollection(keyAttribute, onPreCompleted);

		/// <summary>
		/// Creates a collection from this list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this List<T> @object, string keyAttribute)
			=> (@object as IEnumerable<T>)?.ToCollection(keyAttribute);

		/// <summary>
		/// Creates a collection from this hash-set
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <param name="onPreCompleted"></param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this List<TValue> @object, string keyAttribute, Action<Collection<TKey, TValue>> onPreCompleted = null)
			=> (@object as IEnumerable<TValue>)?.ToCollection(keyAttribute, onPreCompleted);

		/// <summary>
		/// Creates a collection from this list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this IEnumerable<T> @object, string keyAttribute)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException("keyAttribute", "The name of key attribute is null");

			var collection = new Collection();
			@object?.ForEach(item =>
			{
				try
				{
					var key = item.GetAttributeValue(keyAttribute);
					if (key != null && !collection.Contains(key))
						collection.Add(key, item);
				}
				catch { }
			});
			return collection;
		}

		/// <summary>
		/// Creates a collection from this list
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <param name="onPreCompleted"></param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this IEnumerable<TValue> @object, string keyAttribute, Action<Collection<TKey, TValue>> onPreCompleted = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");
			var collection = new Collection<TKey, TValue>(@object?.ToDictionary(item => (TKey)item.GetAttributeValue(keyAttribute)));
			onPreCompleted?.Invoke(collection);
			return collection;
		}

		/// <summary>
		/// Creates a collection from this dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="onPreCompleted"></param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this Dictionary<TKey, TValue> @object, Action<Collection<TKey, TValue>> onPreCompleted = null)
		{
			var collection = new Collection<TKey, TValue>(@object);
			onPreCompleted?.Invoke(collection);
			return collection;
		}

		/// <summary>
		/// Creates a collection of objects from this JSON
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this JArray json, string keyAttribute, Func<JToken, TValue> converter = null)
			=> new Collection<TKey, TValue>(json?.ToDictionary<TKey, TValue>(keyAttribute, converter));

		/// <summary>
		/// Creates a collection of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this JArray json, string keyAttribute, Func<JToken, T> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");

			var collection = new Collection();
			json?.ForEach(token =>
			{
				var @object = converter != null ? converter(token) : token.FromJson<T>();
				var key = @object.GetAttributeValue(keyAttribute);
				if (key != null && !collection.Contains(key))
					collection.Add(key, @object);
			});
			return collection;
		}

		/// <summary>
		/// Creates a collection of objects from this JSON
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this JObject json, string keyAttribute, Func<JToken, TValue> converter = null)
			=> new Collection<TKey, TValue>(json?.ToDictionary<TKey, TValue>(keyAttribute, converter));

		/// <summary>
		/// Creates a collection of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this JObject json, string keyAttribute, Func<JToken, T> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");

			var collection = new Collection();
			json?.ForEach(token =>
			{
				var @object = converter != null ? converter(token) : token.FromJson<T>();
				var key = @object.GetAttributeValue(keyAttribute);
				if (key != null && !collection.Contains(key))
					collection.Add(key, @object);
			});
			return collection;
		}

		/// <summary>
		/// Creates a collection of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute that their value will be used a the key of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this JObject json, string keyAttribute, Func<KeyValuePair<string, JToken>, T> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");

			var collection = new Collection();
			json?.ForEach(kvp =>
			{
				var @object = converter != null ? converter(kvp) : kvp.Value.FromJson<T>();
				var key = @object.GetAttributeValue(keyAttribute);
				if (key != null && !collection.Contains(key))
					collection.Add(key, @object);
			});
			return collection;
		}
		#endregion

		#region Conversions (Special)
		/// <summary>
		/// Converts the collection of objects to the generic list of strong-typed objects
		/// </summary>
		/// <param name="object"></param>
		/// <param name="type">The type of elements</param>
		/// <returns></returns>
		public static IEnumerable ToList(this List<object> @object, Type type)
		{
			var list = typeof(List<>).MakeGenericType(type).CreateInstance<IList>();
			@object.ForEach(element =>
			{
				// assign value
				var value = element;
				if (value != null)
				{
					// value is list/hash-set
					if (type.IsGenericListOrHashSet() && value is List<object>)
						value = type.IsGenericList()
							? (value as List<object>).ToList(type.GenericTypeArguments[0])
							: (value as List<object>).ToHashSet(type.GenericTypeArguments[0]);

					// value is dictionary/collection or object
					else if (value is ExpandoObject)
					{
						if (type.IsGenericDictionaryOrCollection())
							value = type.IsGenericDictionary()
								? (value as ExpandoObject).ToDictionary(type.GenericTypeArguments[0], type.GenericTypeArguments[1])
								: (value as ExpandoObject).ToCollection(type.GenericTypeArguments[0], type.GenericTypeArguments[1]);

						else if (type.IsClassType() && !type.Equals(typeof(ExpandoObject)))
						{
							var temp = type.CreateInstance();
							temp.CopyFrom(value as ExpandoObject);
							value = temp;
						}
					}

					// value is primitive type
					else if (type.IsPrimitiveType() && !type.Equals(value.GetType()))
						value = Convert.ChangeType(value, type);
				}

				// update into the collection
				list.Add(value);
			});
			return list;
		}

		/// <summary>
		/// Converts the collection of objects to the generic list of strong-typed objects
		/// </summary>
		/// <typeparam name="T">The strong-typed generic type</typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T ToList<T>(this List<object> @object)
			=> typeof(T).IsGenericList()
				? (T)@object.ToList(typeof(T).GenericTypeArguments[0])
				: default;

#if NETSTANDARD2_0
		/// <summary>
		/// Converts the collection of cookies to a list
		/// </summary>
		/// <param name="cookies"></param>
		/// <returns></returns>
		public static List<Cookie> ToList(this CookieCollection cookies)
		{
			var list = new List<Cookie>();
			foreach (Cookie cookie in cookies)
				list.Add(cookie);
			return list;
		}
#endif

		/// <summary>
		/// Converts the collection of objects to the generic hash-set of strong-typed objects
		/// </summary>
		/// <param name="object"></param>
		/// <param name="type">The type of elements</param>
		/// <returns></returns>
		public static IEnumerable ToHashSet(this List<object> @object, Type type)
		{
			var ctor = typeof(HashSet<>).MakeGenericType(type).GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(type) });
			return ctor.Invoke(new object[] { @object.ToList(type) }) as IEnumerable;
		}

		/// <summary>
		/// Converts the generic list of objects to the generic hash-set of strong-typed objects
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T ToHashSet<T>(this List<object> @object)
			=> typeof(T).IsGenericHashSet()
				? (T)@object.ToHashSet(typeof(T).GenericTypeArguments[0])
				: default;

		/// <summary>
		/// Converts the collection of objects to the generic dictionary of strong-typed objects
		/// </summary>
		/// <param name="object"></param>
		/// <param name="keyType"></param>
		/// <param name="valueType"></param>
		/// <returns></returns>
		public static IDictionary ToDictionary(this ExpandoObject @object, Type keyType, Type valueType)
		{
			var dictionary = typeof(Dictionary<,>).MakeGenericType(keyType, valueType).CreateInstance() as IDictionary;
			@object.ForEach(kvp =>
			{
				// key
				object key = kvp.Key;
				if (!keyType.Equals(key.GetType()))
					key = Convert.ChangeType(key, keyType);

				// value
				object value = kvp.Value;
				if (value != null)
				{
					// value is list/hash-set
					if (valueType.IsGenericListOrHashSet() && value is List<object>)
						value = valueType.IsGenericList()
							? (value as List<object>).ToList(valueType.GenericTypeArguments[0])
							: (value as List<object>).ToHashSet(valueType.GenericTypeArguments[0]);

					// value is dictionary/collection or object
					else if (value is ExpandoObject)
					{
						if (valueType.IsGenericDictionaryOrCollection())
							value = valueType.IsGenericDictionary()
								? (value as ExpandoObject).ToDictionary(valueType.GenericTypeArguments[0], valueType.GenericTypeArguments[1])
								: (value as ExpandoObject).ToCollection(valueType.GenericTypeArguments[0], valueType.GenericTypeArguments[1]);

						else if (valueType.IsClassType() && !valueType.Equals(typeof(ExpandoObject)))
						{
							var temp = valueType.CreateInstance();
							temp.CopyFrom(value as ExpandoObject);
							value = temp;
						}
					}

					// value is primitive
					else if (valueType.IsPrimitiveType() && !valueType.Equals(value.GetType()))
						value = Convert.ChangeType(value, valueType);
				}

				// update into the collection
				dictionary.Add(key, value);
			});
			return dictionary;
		}

		/// <summary>
		/// Converts the collection of objects to the generic dictionary of strong-typed objects
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T ToDictionary<T>(this ExpandoObject @object)
			=> typeof(T).IsGenericDictionary()
				? (T)@object.ToDictionary(typeof(T).GenericTypeArguments[0], typeof(T).GenericTypeArguments[1])
				: default;

		/// <summary>
		/// Converts the collection of objects to the generic collection of strong-typed objects
		/// </summary>
		/// <param name="object"></param>
		/// <param name="keyType"></param>
		/// <param name="valueType"></param>
		/// <returns></returns>
		public static IDictionary ToCollection(this ExpandoObject @object, Type keyType, Type valueType)
		{
			var ctor = typeof(Collection<,>).MakeGenericType(keyType, valueType).GetConstructor(new[] { typeof(Dictionary<,>).MakeGenericType(keyType, valueType) });
			return ctor.Invoke(new object[] { @object.ToDictionary(keyType, valueType) }) as IDictionary;
		}

		/// <summary>
		/// Converts the collection of objects to the generic collection of strong-typed objects
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T ToCollection<T>(this ExpandoObject @object)
			=> typeof(T).IsGenericCollection()
				? (T)@object.ToCollection(typeof(T).GenericTypeArguments[0], typeof(T).GenericTypeArguments[1])
				: default;
		#endregion

		#region Conversions (JSON - JArray & JObject) 
		/// <summary>
		/// Creates a JArray object from this collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static JArray ToJArray<T>(this IEnumerable<T> @object, Func<T, JToken> converter = null)
		{
			var type = typeof(T);
			if (type.IsPrimitiveType() || type.IsClassType())
			{
				var json = new JArray();
				@object.ForEach(item => json.Add(converter != null ? converter(item) : item?.ToJson()));
				return json;
			}
			else
				return JArray.FromObject(@object);
		}

		/// <summary>
		/// Creates a JArray object from this collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static JArray ToJArray<TKey, TValue>(this IDictionary<TKey, TValue> @object, Func<TValue, JToken> converter = null)
			=> @object.Select(kvp => converter != null ? converter(kvp.Value) : kvp.Value?.ToJson()).ToJArray();

		/// <summary>
		/// Creates a JArray object from this collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static JArray ToJArray<TKey, TValue>(this IDictionary<TKey, TValue> @object, Func<KeyValuePair<TKey, TValue>, JToken> converter = null)
			=> @object.Select(kvp => converter != null ? converter(kvp) : kvp.Value?.ToJson()).ToJArray();

		/// <summary>
		/// Creates a JArray object from this collection
		/// </summary>
		/// <param name="object"></param>
		/// <param name="converter"></param>
		/// <returns></returns>
		public static JArray ToJArray(this Collection @object, Func<object, JToken> converter = null)
		{
			var json = new JArray();
			var enumerator = @object.GetEnumerator();
			while (enumerator.MoveNext())
				json.Add(converter != null ? converter?.Invoke(enumerator.Current) : enumerator.Current?.ToJson());
			return json;
		}

		/// <summary>
		/// Creates a JObject object from this collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="keyAttribute">The string that presents name of attribute to use their value as key</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static JObject ToJObject<T>(this IEnumerable<T> @object, string keyAttribute, Func<T, JToken> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");
			var json = new JObject();
			@object.Where(item => item != null).ForEach(item => json[(item?.GetAttributeValue(keyAttribute) ?? item?.GetHashCode()).ToString()] = converter != null ? converter(item) : item?.ToJson());
			return json;
		}

		/// <summary>
		/// Creates a JObject object from this collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static JObject ToJObject<TKey, TValue>(this IDictionary<TKey, TValue> @object, Func<TValue, JToken> converter = null)
		{
			var json = new JObject();
			var enumerator = @object.GetEnumerator();
			while (enumerator.MoveNext())
				json[enumerator.Current.Key.ToString()] = converter != null ? converter(enumerator.Current.Value) : enumerator.Current.Value?.ToJson();
			return json;
		}

		/// <summary>
		/// Creates a JObject object from this collection
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		public static JObject ToJObject(this Collection @object, Func<object, JToken> converter = null)
		{
			var json = new JObject();
			var enumerator = @object.AsEnumerableDictionaryEntry.GetEnumerator();
			while (enumerator.MoveNext())
				json[enumerator.Current.Key.ToString()] = converter != null ? converter(enumerator.Current.Value) : enumerator.Current.Value?.ToJson();
			return json;
		}

		/// <summary>
		/// Creates a JObject object from this collection
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		public static JObject ToJObject(this NameValueCollection @object)
		{
			var json = new JObject();
			foreach (string key in @object.Keys)
				json[key] = @object[key]?.ToJson();
			return json;
		}
		#endregion

		#region Conversions (ArraySegment)
		/// <summary>
		/// Converts this list to array segment
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static ArraySegment<T> ToArraySegment<T>(this List<T> list, int offset = 0, int count = 0)
		{
			offset = offset > -1 && offset < list.Count ? offset : 0;
			count = count > 0 && count < list.Count - offset ? count : list.Count - offset;
			return new ArraySegment<T>(list.ToArray(), offset, count);
		}

		/// <summary>
		/// Converts this array to array segment
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="array"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static ArraySegment<T> ToArraySegment<T>(this T[] array, int offset = 0, int count = 0)
		{
			offset = offset > -1 && offset < array.Length ? offset : 0;
			count = count > 0 && count < array.Length - offset ? count : array.Length - offset;
			return new ArraySegment<T>(array, offset, count);
		}

		/// <summary>
		/// Converts this string to array segment of bytes
		/// </summary>
		/// <param name="string"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this string @string, Encoding encoding = null)
			=> @string.ToBytes(encoding).ToArraySegment();

		/// <summary>
		/// Converts this boolean to array segment of bytes
		/// </summary>
		/// <param name="bool"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this bool @bool)
			=> @bool.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this char to array segment of bytes
		/// </summary>
		/// <param name="char"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this char @char)
			=> @char.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this byte to array segment of bytes
		/// </summary>
		/// <param name="byte"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this byte @byte)
			=> @byte.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this sbyte to array segment of bytes
		/// </summary>
		/// <param name="sbyte"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this sbyte @sbyte)
			=> @sbyte.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this short to array segment of bytes
		/// </summary>
		/// <param name="short"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this short @short)
			=> @short.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this ushort to array segment of bytes
		/// </summary>
		/// <param name="ushort"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this ushort @ushort)
			=> @ushort.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this int to array segment of bytes
		/// </summary>
		/// <param name="int"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this int @int)
			=> @int.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this uint to array segment of bytes
		/// </summary>
		/// <param name="uint"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this uint @uint)
			=> @uint.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this long to array segment of bytes
		/// </summary>
		/// <param name="long"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this long @long)
			=> @long.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this ulong to array segment of bytes
		/// </summary>
		/// <param name="ulong"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this ulong @ulong)
			=> @ulong.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this float to array segment of bytes
		/// </summary>
		/// <param name="float"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this float @float)
			=> @float.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this double to array segment of bytes
		/// </summary>
		/// <param name="double"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this double @double)
			=> @double.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this decimal to array segment of bytes
		/// </summary>
		/// <param name="decimal"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this decimal @decimal)
			=> @decimal.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this date-time to array segment of bytes
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this DateTime datetime)
			=> datetime.ToBytes().ToArraySegment();

		/// <summary>
		/// Converts this big-integer to array segment of bytes
		/// </summary>
		/// <param name="bigInt"></param>
		/// <returns></returns>
		public static ArraySegment<byte> ToArraySegment(this BigInteger bigInt)
			=> bigInt.ToUnsignedBytes().ToArraySegment();
		#endregion

	}
}

namespace System.Collections.Specialized
{
	/// <summary>
	/// Represents a collection of key/value pairs that are accessible by key or index
	/// </summary>
	[Serializable, DebuggerDisplay("Count = {Count}")]
	public class Collection : IDictionary
	{
		readonly OrderedDictionary _collection = new OrderedDictionary();

		/// <summary>
		/// Initializes a new instance of the <see cref="Collection"/> class
		/// </summary>
		public Collection() { }

		#region Properties
		/// <summary>
		/// Gets the number of key/values pairs contained in the collection
		/// </summary>
		public int Count => this._collection.Count;

		/// <summary>
		/// Gets a value indicating whether the collection is read-only
		/// </summary>
		public bool IsReadOnly => this._collection.IsReadOnly;

		/// <summary>
		/// Gets a value indicating whether access to the collection is synchronized
		/// </summary>
		public bool IsSynchronized => (this._collection as ICollection).IsSynchronized;

		/// <summary>
		/// Gets an object that can be used to synchronize access to the collection
		/// </summary>
		public object SyncRoot => (this._collection as ICollection).SyncRoot;

		/// <summary>
		/// Gets a value indicating whether the collection has a fixed size
		/// </summary>
		public bool IsFixedSize => (this._collection as IDictionary).IsFixedSize;

		/// <summary>
		/// Gets an object containing the keys in the collection
		/// </summary>
		public ICollection Keys => this._collection.Keys;

		/// <summary>
		/// Gets an object containing the values in the collection
		/// </summary>
		public ICollection Values => this._collection.Values;

		/// <summary>
		/// Gets or sets the value with the specified key
		/// </summary>
		/// <param name="key">The key of the value to get or set</param>
		/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new element using the specified key</returns>
		public object this[object key]
		{
			get => this._collection[key];
			set => this._collection[key] = value;
		}

		/// <summary>
		/// Gets or sets the value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the value to get or set</param>
		/// <returns>The value of the item at the specified index</returns>
		public object this[int index]
		{
			get => this._collection[index];
			set => this._collection[index] = value;
		}

		/// <summary>
		/// Gets the object that cast as enumerable of dictionary entry
		/// </summary>
		public IEnumerable<DictionaryEntry> AsEnumerableDictionaryEntry => this._collection.Cast<DictionaryEntry>();
		#endregion

		#region Methods
		/// <summary>
		/// Copies the elements of the collection to an array, starting at a particular index
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in array at which copying begins</param>
		public void CopyTo(Array array, int index)
			=> (this._collection as ICollection).CopyTo(array, index);

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool Contains(object key)
			=> key != null && this._collection.Contains(key);

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool ContainsKey(object key)
			=> this.Contains(key);

		/// <summary>
		/// Adds an element with the specified key and value into the collection with the lowest available index
		/// </summary>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Add(object key, object value)
			=> this._collection.Add(key, value);

		/// <summary>
		/// Inserts a new element into the collection with the specified key and value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted</param>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Insert(int index, object key, object value)
			=> this._collection.Insert(index, key, value);

		void IDictionary.Remove(object key)
			=> this.Remove(key);

		/// <summary>
		/// Removes the element with the specified key from the collection
		/// </summary>
		/// <param name="key">The key of the element to remove</param>
		/// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the collection</returns>
		public bool Remove(object key)
		{
			if (this.Contains(key))
			{
				this._collection.Remove(key);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes the element at the specified index from the collection
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove</param>
		public void RemoveAt(int index)
			=> this._collection.RemoveAt(index);

		/// <summary>
		/// Removes all elements from the collection
		/// </summary>
		public void Clear()
			=> this._collection.Clear();

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire the collection
		/// </summary>
		/// <param name="object">The object to locate in the collections. The value can be null for reference types</param>
		/// <returns>The zero-based index of the first occurrence of item within the entire the collections if found; otherwise, –1.</returns>
		public int IndexOf(object @object)
		{
			var index = -1;
			foreach (var value in this.Values)
			{
				index++;
				if (object.ReferenceEquals(@object, value))
					return index;
			}
			return -1;
		}

		/// <summary>
		/// Searches for the specified key and returns the zero-based index of the first occurrence within the entire the collection
		/// </summary>
		/// <param name="object">The key to locate in the collections. The value cannot be null</param>
		/// <returns>The zero-based index of the first occurrence of item within the entire the collections if found; otherwise, –1.</returns>
		public int IndexOfKey(object @object)
		{
			if (@object == null)
			{
				var index = -1;
				foreach (var key in this.Keys)
				{
					index++;
					if (object.ReferenceEquals(@object, key))
						return index;
				}
			}
			return -1;
		}

		/// <summary>
		/// Gets the key of the element at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the element to get the key</param>
		/// <returns>The key object at the specified index</returns>
		public object GetKeyAt(int index)
			=> index > -1 && index < this.Count
				? this.AsEnumerableDictionaryEntry.ElementAt(index).Key
				: null;

		/// <summary>
		/// Gets value of the element at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the element</param>
		/// <returns>The value object at the specified index</returns>
		public object GetByIndex(int index)
			=> index > -1 && index < this.Count
				? this._collection[index]
				: null;

		/// <summary>
		/// Gets value of the element by specified key
		/// </summary>
		/// <param name="key">The object that presents the key of the element</param>
		/// <returns>The value object that specified by the key</returns>
		public object GetByKey(object key)
			=> key != null
				? this._collection[key]
				: null;
		#endregion

		#region Enumerator
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

		/// <summary>
		/// Returns an enumerator that iterates through the collection
		/// </summary>
		/// <returns></returns>
		public IDictionaryEnumerator GetEnumerator()
			=> new Enumerator(this);

		public class Enumerator : IDictionaryEnumerator
		{
			int _index = -1;
			readonly Collection _collection = null;
			readonly List<object> _keys = new List<object>();

			public Enumerator(Collection collection = null)
			{
				this._collection = collection;
				if (this._collection != null)
					foreach (var key in this._collection.Keys)
						this._keys.Add(key);
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection</returns>
			public bool MoveNext()
			{
				this._index++;
				return this._collection != null && this._index < this._collection.Count;
			}

			/// <summary>
			/// Resets the enumerator to its initial position, which is before the first element in the collection
			/// </summary>
			public void Reset()
				=> this._index = -1;

			/// <summary>
			/// Gets the value of current element in the collection
			/// </summary>
			public object Current => this.Value;

			/// <summary>
			/// Gets the key of the current element
			/// </summary>
			public object Key => this._collection != null && this._index > -1 && this._index < this._collection.Count ? this._keys[this._index] : null;

			/// <summary>
			/// Gets the value of the current element
			/// </summary>
			public object Value => this._collection != null && this._index > -1 && this._index < this._collection.Count ? this._collection[this._index] : null;

			/// <summary>
			/// Gets both the key and the value of the current element
			/// </summary>
			public DictionaryEntry Entry => new DictionaryEntry(this.Key, this.Value);
		}
		#endregion

	}
}

namespace System.Collections.Generic
{
	/// <summary>
	/// Represents a generic collection of key/value pairs that are accessible by key or index
	/// </summary>
	/// <typeparam name="TKey">The type of all keys</typeparam>
	/// <typeparam name="TValue">The type of all values</typeparam>
	[Serializable, DebuggerDisplay("Count = {Count}")]
	public class Collection<TKey, TValue> : Collection, IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Collection">Collection</see> class
		/// </summary>
		/// <param name="dictionary">The initialized values</param>
		public Collection(IDictionary<TKey, TValue> dictionary = null) : base()
			=> dictionary?.ForEach(kvp => this.Add(kvp));

		#region Properties
		/// <summary>
		/// Gets or sets the value with the specified key
		/// </summary>
		/// <param name="key">The key of the value to get or set</param>
		/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new element using the specified key</returns>
		public TValue this[TKey key]
		{
			get => (TValue)base[key];
			set => base[key] = value;
		}

		/// <summary>
		/// Gets or sets the value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the value to get or set</param>
		/// <returns>The value of the item at the specified index</returns>
		public new TValue this[int index]
		{
			get => (TValue)base[index];
			set => base[index] = value;
		}

		/// <summary>
		/// Gets an object that containing the keys of the collection
		/// </summary>
		public new ICollection<TKey> Keys
		{
			get
			{
				var keys = new List<TKey>();
				foreach (TKey key in base.Keys)
					keys.Add(key);
				return keys;
			}
		}

		/// <summary>
		/// Gets an object that containing the values in the collection
		/// </summary>
		public new ICollection<TValue> Values
		{
			get
			{
				var values = new List<TValue>();
				foreach (TValue value in base.Values)
					values.Add(value);
				return values;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Copies the elements of the collection to an array, starting at a particular index
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in array at which copying begins</param>
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
			=> base.CopyTo(array, index);

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool Contains(TKey key)
			=> base.Contains(key);

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool ContainsKey(TKey key)
			=> this.Contains(key);

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="element">The element that contains both the key and the value</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool Contains(KeyValuePair<TKey, TValue> element)
			=> this.Contains(element.Key);

		/// <summary>
		/// Adds an element with the specified key and value into the collection with the lowest available index
		/// </summary>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Add(TKey key, TValue value)
			=> base.Add(key, value);

		/// <summary>
		/// Adds an element into the collection with the lowest available index
		/// </summary>
		/// <param name="element">The element that contains both the key and the value</param>
		public void Add(KeyValuePair<TKey, TValue> element)
			=> this.Add(element.Key, element.Value);

		/// <summary>
		/// Inserts a new element into the collection with the specified key and value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted</param>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Insert(int index, TKey key, TValue value)
			=> base.Insert(index, key, value);

		/// <summary>
		/// Removes the element with the specified key from the collection
		/// </summary>
		/// <param name="key">The key of the element to remove</param>
		/// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the collection</returns>
		public bool Remove(TKey key)
			=> base.Remove(key);

		/// <summary>
		/// Removes the element with the specified key from the collection
		/// </summary>
		/// <param name="element">The element that contains both the key and the value</param>
		/// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the collection.</returns>
		public bool Remove(KeyValuePair<TKey, TValue> element)
			=> this.Remove(element.Key);

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire the collection
		/// </summary>
		/// <param name="object">The object to locate in the collections. The value can be null for reference types</param>
		/// <returns>The zero-based index of the first occurrence of item within the entire the collections if found; otherwise, –1.</returns>
		public int IndexOf(TValue @object)
			=> base.IndexOf(@object);

		/// <summary>
		/// Searches for the specified key and returns the zero-based index of the first occurrence within the entire the collection
		/// </summary>
		/// <param name="key">The key to locate in the collections. The value cannot be null</param>
		/// <returns>The zero-based index of the first occurrence of item within the entire the collections if found; otherwise, –1.</returns>
		public int IndexOfKey(TKey key)
			=> base.IndexOfKey(key);

		/// <summary>
		/// Gets a key at the specified index
		/// </summary>
		/// <param name="index">Index of the key</param>
		/// <returns>The key object at the specified index</returns>
		public new TKey GetKeyAt(int index)
			=> (TKey)base.GetKeyAt(index);

		/// <summary>
		/// Gets the value associated with the specified key
		/// </summary>
		/// <param name="key">The key whose value to get</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter</param>
		/// <returns>true if the object that contains an element with the specified key; otherwise, false.</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			if (this.Contains(key))
			{
				value = this[key];
				return true;
			}
			value = default;
			return false;
		}

		/// <summary>
		/// Gets value of the element at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the element</param>
		/// <returns>The value object at the specified index</returns>
		public new TValue GetByIndex(int index)
			=> index > -1 && index < this.Count
				? (TValue)base.GetByIndex(index)
				: default;

		/// <summary>
		/// Gets value of the element by specified key
		/// </summary>
		/// <param name="key">The object that presents the key of the element</param>
		/// <returns>The value object that specified by the key</returns>
		public TValue GetByKey(TKey key)
			=> (TValue)base.GetByKey(key);

		/// <summary>
		/// Gets value of the first element
		/// </summary>
		/// <returns></returns>
		public TValue First()
			=> this.GetByIndex(0);

		/// <summary>
		/// Gets value of the last element
		/// </summary>
		/// <returns></returns>
		public TValue Last()
			=> this.GetByIndex(this.Count - 1);
		#endregion

		#region Enumerator
		/// <summary>
		/// Returns an enumerator that iterates through the collection
		/// </summary>
		/// <returns></returns>
		public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
			=> new Enumerator(this);

		public new class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			int _index = -1;
			Collection<TKey, TValue> _collection = null;
			readonly List<TKey> _keys = new List<TKey>();

			public Enumerator() : this(null) { }

			public Enumerator(Collection<TKey, TValue> collection)
			{
				this._collection = collection;
				if (this._collection != null)
					this._keys = this._collection.Keys.ToList();
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection
			/// </summary>
			/// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection</returns>
			public bool MoveNext()
			{
				this._index++;
				return this._collection != null && this._index < this._collection.Count;
			}

			/// <summary>
			/// Resets the enumerator to its initial position, which is before the first element in the collection
			/// </summary>
			public void Reset()
				=> this._index = -1;

			public void Dispose()
				=> this._collection = null;

			/// <summary>
			/// Gets the key of the current element
			/// </summary>
			public TKey Key => this._collection != null && this._index > -1 && this._index < this._collection.Count ? this._keys[this._index] : default;

			/// <summary>
			/// Gets the value of the current element
			/// </summary>
			public TValue Value => this._collection != null && this._index > -1 && this._index < this._collection.Count ? this._collection[this._index] : default;

			/// <summary>
			/// Gets the value of current element in the collection
			/// </summary>
			public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(this.Key, this.Value);

			object IEnumerator.Current => this.Current;
		}
		#endregion

	}
}

namespace System.Collections.Concurrent
{
	/// <summary>
	/// Represents a thread-safe hash-based unique collection (original: https://github.com/i3arnon/ConcurrentHashSet)
	/// </summary>
	/// <typeparam name="T">The type of the items in the collection.</typeparam>
	/// <remarks>All public members of <see cref="ConcurrentHashSet{T}"/> are thread-safe and may be used concurrently from multiple threads.</remarks>
	[Serializable, DebuggerDisplay("Count = {Count}")]
	public class ConcurrentHashSet<T> : IReadOnlyCollection<T>, ICollection<T>
	{

		#region Helper classes
		[Serializable]
		class Tables
		{
			public readonly Node[] Buckets;
			public readonly object[] Locks;

			public volatile int[] CountPerLock;

			public Tables(Node[] buckets, object[] locks, int[] countPerLock)
			{
				Buckets = buckets;
				Locks = locks;
				CountPerLock = countPerLock;
			}
		}

		[Serializable]
		class Node
		{
			public readonly T Item;
			public readonly int Hashcode;

			public volatile Node Next;

			public Node(T item, int hashcode, Node next)
			{
				Item = item;
				Hashcode = hashcode;
				Next = next;
			}
		}
		#endregion

		#region Static attributes
		const int DefaultCapacity = 31;
		const int MaxLockNumber = 1024;
		const int ProcessorCountRefreshIntervalMs = 30000;

		static volatile int _ProcessorCount;
		static volatile int _LastProcessorCountRefreshTicks;

		static int DefaultConcurrencyLevel
		{
			get
			{
				var now = Environment.TickCount;
				if (_ProcessorCount == 0 || now - _LastProcessorCountRefreshTicks >= ProcessorCountRefreshIntervalMs)
				{
					_ProcessorCount = Environment.ProcessorCount;
					_LastProcessorCountRefreshTicks = now;
				}
				return _ProcessorCount;
			}
		}
		#endregion

		#region Attributes & Properties
		readonly IEqualityComparer<T> _comparer;
		readonly bool _growLockArray;

		int _budget;
		volatile Tables _tables;

		/// <summary>
		/// Gets the number of items contained in the <see
		/// cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <value>The number of items contained in the <see
		/// cref="ConcurrentHashSet{T}"/>.</value>
		/// <remarks>Count has snapshot semantics and represents the number of items in the <see
		/// cref="ConcurrentHashSet{T}"/>
		/// at the moment when Count was accessed.</remarks>
		public int Count
		{
			get
			{
				var count = 0;
				var acquiredLocks = 0;
				try
				{
					this.AcquireAllLocks(ref acquiredLocks);
					for (var i = 0; i < this._tables.CountPerLock.Length; i++)
						count += this._tables.CountPerLock[i];
				}
				finally
				{
					this.ReleaseLocks(0, acquiredLocks);
				}
				return count;
			}
		}

		/// <summary>
		/// Gets a value that indicates whether the <see cref="ConcurrentHashSet{T}"/> is empty.
		/// </summary>
		/// <value>true if the <see cref="ConcurrentHashSet{T}"/> is empty; otherwise,
		/// false.</value>
		public bool IsEmpty
		{
			get
			{
				var acquiredLocks = 0;
				try
				{
					this.AcquireAllLocks(ref acquiredLocks);
					for (var i = 0; i < this._tables.CountPerLock.Length; i++)
						if (this._tables.CountPerLock[i] != 0)
							return false;
				}
				finally
				{
					this.ReleaseLocks(0, acquiredLocks);
				}
				return true;
			}
		}

		bool ICollection<T>.IsReadOnly => false;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see
		/// cref="ConcurrentHashSet{T}"/>
		/// class that is empty, has the default concurrency level, has the default initial capacity, and
		/// uses the default comparer for the item type.
		/// </summary>
		public ConcurrentHashSet() : this(DefaultConcurrencyLevel, DefaultCapacity, true, null) { }

		/// <summary>
		/// Initializes a new instance of the <see
		/// cref="ConcurrentHashSet{T}"/>
		/// class that is empty, has the specified concurrency level and capacity, and uses the default
		/// comparer for the item type.
		/// </summary>
		/// <param name="concurrencyLevel">The estimated number of threads that will update the
		/// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
		/// <param name="capacity">The initial number of elements that the <see
		/// cref="ConcurrentHashSet{T}"/>
		/// can contain.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="concurrencyLevel"/> is
		/// less than 1.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"> <paramref name="capacity"/> is less than
		/// 0.</exception>
		public ConcurrentHashSet(int concurrencyLevel, int capacity) : this(concurrencyLevel, capacity, false, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
		/// class that contains elements copied from the specified <see
		/// cref="T:System.Collections.IEnumerable{T}"/>, has the default concurrency
		/// level, has the default initial capacity, and uses the default comparer for the item type.
		/// </summary>
		/// <param name="collection">The <see
		/// cref="T:System.Collections.IEnumerable{T}"/> whose elements are copied to
		/// the new
		/// <see cref="ConcurrentHashSet{T}"/>.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is a null reference.</exception>
		public ConcurrentHashSet(IEnumerable<T> collection) : this(collection, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
		/// class that is empty, has the specified concurrency level and capacity, and uses the specified
		/// <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
		/// </summary>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>
		/// implementation to use when comparing items.</param>
		public ConcurrentHashSet(IEqualityComparer<T> comparer) : this(DefaultConcurrencyLevel, DefaultCapacity, true, comparer) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
		/// class that contains elements copied from the specified <see
		/// cref="T:System.Collections.IEnumerable"/>, has the default concurrency level, has the default
		/// initial capacity, and uses the specified
		/// <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
		/// </summary>
		/// <param name="collection">The <see
		/// cref="T:System.Collections.IEnumerable{T}"/> whose elements are copied to
		/// the new
		/// <see cref="ConcurrentHashSet{T}"/>.</param>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>
		/// implementation to use when comparing items.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="collection"/> is a null reference
		/// (Nothing in Visual Basic).
		/// </exception>
		public ConcurrentHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer) : this(comparer)
			=> this.Initialize(collection);

		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> 
		/// class that contains elements copied from the specified <see cref="T:System.Collections.IEnumerable"/>, 
		/// has the specified concurrency level, has the specified initial capacity, and uses the specified 
		/// <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
		/// </summary>
		/// <param name="concurrencyLevel">The estimated number of threads that will update the 
		/// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
		/// <param name="collection">The <see cref="T:System.Collections.IEnumerable{T}"/> whose elements are copied to the new 
		/// <see cref="ConcurrentHashSet{T}"/>.</param>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/> implementation to use 
		/// when comparing items.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="collection"/> is a null reference.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="concurrencyLevel"/> is less than 1.
		/// </exception>
		public ConcurrentHashSet(int concurrencyLevel, IEnumerable<T> collection, IEqualityComparer<T> comparer) : this(concurrencyLevel, DefaultCapacity, false, comparer)
			=> this.Initialize(collection);

		/// <summary>
		/// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/>
		/// class that is empty, has the specified concurrency level, has the specified initial capacity, and
		/// uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>.
		/// </summary>
		/// <param name="concurrencyLevel">The estimated number of threads that will update the
		/// <see cref="ConcurrentHashSet{T}"/> concurrently.</param>
		/// <param name="capacity">The initial number of elements that the <see
		/// cref="ConcurrentHashSet{T}"/>
		/// can contain.</param>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer{T}"/>
		/// implementation to use when comparing items.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="concurrencyLevel"/> is less than 1. -or-
		/// <paramref name="capacity"/> is less than 0.
		/// </exception>
		public ConcurrentHashSet(int concurrencyLevel, int capacity, IEqualityComparer<T> comparer) : this(concurrencyLevel, capacity, false, comparer) { }

		private ConcurrentHashSet(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<T> comparer)
		{
			if (concurrencyLevel < 1)
				throw new ArgumentOutOfRangeException(nameof(concurrencyLevel));
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));

			// The capacity should be at least as large as the concurrency level. Otherwise, we would have locks that don't guard
			// any buckets.
			if (capacity < concurrencyLevel)
				capacity = concurrencyLevel;

			var locks = new object[concurrencyLevel];
			for (var i = 0; i < locks.Length; i++)
				locks[i] = new object();

			var countPerLock = new int[locks.Length];
			var buckets = new Node[capacity];
			this._tables = new Tables(buckets, locks, countPerLock);

			this._growLockArray = growLockArray;
			this._budget = buckets.Length / locks.Length;
			this._comparer = comparer ?? EqualityComparer<T>.Default;
		}

		void Initialize(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			foreach (var item in collection)
				this.AddInternal(item, this._comparer.GetHashCode(item), false);
			if (this._budget == 0)
				this._budget = this._tables.Buckets.Length / this._tables.Locks.Length;
		}
		#endregion

		#region Locking mechanism
		static int GetBucket(int hashcode, int bucketCount)
		{
			var bucketNo = (hashcode & 0x7fffffff) % bucketCount;
			Debug.Assert(bucketNo >= 0 && bucketNo < bucketCount);
			return bucketNo;
		}

		static void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
		{
			bucketNo = (hashcode & 0x7fffffff) % bucketCount;
			lockNo = bucketNo % lockCount;
			Debug.Assert(bucketNo >= 0 && bucketNo < bucketCount);
			Debug.Assert(lockNo >= 0 && lockNo < lockCount);
		}

		void GrowTable(Tables tables)
		{
			const int maxArrayLength = 0X7FEFFFFF;
			var locksAcquired = 0;
			try
			{
				// The thread that first obtains _locks[0] will be the one doing the resize operation
				AcquireLocks(0, 1, ref locksAcquired);

				// Make sure nobody resized the table while we were waiting for lock 0:
				if (tables != this._tables)
				{
					// We assume that since the table reference is different, it was already resized (or the budget
					// was adjusted). If we ever decide to do table shrinking, or replace the table for other reasons,
					// we will have to revisit this logic.
					return;
				}

				// Compute the (approx.) total size. Use an Int64 accumulation variable to avoid an overflow.
				long approxCount = 0;
				for (var i = 0; i < tables.CountPerLock.Length; i++)
				{
					approxCount += tables.CountPerLock[i];
				}

				//
				// If the bucket array is too empty, double the budget instead of resizing the table
				//
				if (approxCount < tables.Buckets.Length / 4)
				{
					this._budget = 2 * this._budget;
					if (this._budget < 0)
					{
						this._budget = int.MaxValue;
					}
					return;
				}

				// Compute the new table size. We find the smallest integer larger than twice the previous table size, and not divisible by
				// 2,3,5 or 7. We can consider a different table-sizing policy in the future.
				var newLength = 0;
				var maximizeTableSize = false;
				try
				{
					checked
					{
						// Double the size of the buckets table and add one, so that we have an odd integer.
						newLength = tables.Buckets.Length * 2 + 1;

						// Now, we only need to check odd integers, and find the first that is not divisible
						// by 3, 5 or 7.
						while (newLength % 3 == 0 || newLength % 5 == 0 || newLength % 7 == 0)
						{
							newLength += 2;
						}

						Debug.Assert(newLength % 2 != 0);

						if (newLength > maxArrayLength)
						{
							maximizeTableSize = true;
						}
					}
				}
				catch (OverflowException)
				{
					maximizeTableSize = true;
				}

				if (maximizeTableSize)
				{
					newLength = maxArrayLength;

					// We want to make sure that GrowTable will not be called again, since table is at the maximum size.
					// To achieve that, we set the budget to int.MaxValue.
					//
					// (There is one special case that would allow GrowTable() to be called in the future: 
					// calling Clear() on the ConcurrentHashSet will shrink the table and lower the budget.)
					this._budget = int.MaxValue;
				}

				// Now acquire all other locks for the table
				AcquireLocks(1, tables.Locks.Length, ref locksAcquired);

				var newLocks = tables.Locks;

				// Add more locks
				if (this._growLockArray && tables.Locks.Length < MaxLockNumber)
				{
					newLocks = new object[tables.Locks.Length * 2];
					Array.Copy(tables.Locks, 0, newLocks, 0, tables.Locks.Length);
					for (var i = tables.Locks.Length; i < newLocks.Length; i++)
					{
						newLocks[i] = new object();
					}
				}

				var newBuckets = new Node[newLength];
				var newCountPerLock = new int[newLocks.Length];

				// Copy all data into a new table, creating new nodes for all elements
				for (var i = 0; i < tables.Buckets.Length; i++)
				{
					var current = tables.Buckets[i];
					while (current != null)
					{
						var next = current.Next;
						GetBucketAndLockNo(current.Hashcode, out int newBucketNo, out int newLockNo, newBuckets.Length, newLocks.Length);

						newBuckets[newBucketNo] = new Node(current.Item, current.Hashcode, newBuckets[newBucketNo]);

						checked
						{
							newCountPerLock[newLockNo]++;
						}

						current = next;
					}
				}

				// Adjust the budget
				this._budget = Math.Max(1, newBuckets.Length / newLocks.Length);

				// Replace tables with the new versions
				this._tables = new Tables(newBuckets, newLocks, newCountPerLock);
			}
			finally
			{
				// Release all locks that we took earlier
				ReleaseLocks(0, locksAcquired);
			}
		}

		void AcquireAllLocks(ref int locksAcquired)
		{
			// First, acquire lock 0
			AcquireLocks(0, 1, ref locksAcquired);

			// Now that we have lock 0, the _locks array will not change (i.e., grow),
			// and so we can safely read _locks.Length.
			AcquireLocks(1, this._tables.Locks.Length, ref locksAcquired);
			Debug.Assert(locksAcquired == this._tables.Locks.Length);
		}

		void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
		{
			Debug.Assert(fromInclusive <= toExclusive);
			var locks = this._tables.Locks;

			for (var index = fromInclusive; index < toExclusive; index++)
			{
				var lockTaken = false;
				try
				{
					Monitor.Enter(locks[index], ref lockTaken);
				}
				finally
				{
					if (lockTaken)
					{
						locksAcquired++;
					}
				}
			}
		}

		void ReleaseLocks(int fromInclusive, int toExclusive)
		{
			Debug.Assert(fromInclusive <= toExclusive);
			for (var index = fromInclusive; index < toExclusive; index++)
			{
				Monitor.Exit(this._tables.Locks[index]);
			}
		}
		#endregion

		#region Manipulates
		bool AddInternal(T item, int hashcode, bool acquireLock)
		{
			while (true)
			{
				var tables = this._tables;

				GetBucketAndLockNo(hashcode, out int bucketNo, out int lockNo, tables.Buckets.Length, tables.Locks.Length);

				var resizeDesired = false;
				var lockTaken = false;
				try
				{
					if (acquireLock)
						Monitor.Enter(tables.Locks[lockNo], ref lockTaken);

					// If the table just got resized, we may not be holding the right lock, and must retry.
					// This should be a rare occurrence.
					if (tables != this._tables)
						continue;

					// Try to find this item in the bucket
					Node previous = null;
					for (var current = tables.Buckets[bucketNo]; current != null; current = current.Next)
					{
						Debug.Assert(previous == null && current == tables.Buckets[bucketNo] || previous.Next == current);
						if (hashcode == current.Hashcode && this._comparer.Equals(current.Item, item))
							return false;
						previous = current;
					}

					// The item was not found in the bucket. Insert the new item.
					Volatile.Write(ref tables.Buckets[bucketNo], new Node(item, hashcode, tables.Buckets[bucketNo]));
					checked
					{
						tables.CountPerLock[lockNo]++;
					}

					//
					// If the number of elements guarded by this lock has exceeded the budget, resize the bucket table.
					// It is also possible that GrowTable will increase the budget but won't resize the bucket table.
					// That happens if the bucket table is found to be poorly utilized due to a bad hash function.
					//
					if (tables.CountPerLock[lockNo] > this._budget)
						resizeDesired = true;
				}
				finally
				{
					if (lockTaken)
						Monitor.Exit(tables.Locks[lockNo]);
				}

				//
				// The fact that we got here means that we just performed an insertion. If necessary, we will grow the table.
				//
				// Concurrency notes:
				// - Notice that we are not holding any locks at when calling GrowTable. This is necessary to prevent deadlocks.
				// - As a result, it is possible that GrowTable will be called unnecessarily. But, GrowTable will obtain lock 0
				//   and then verify that the table we passed to it as the argument is still the current table.
				//
				if (resizeDesired)
					this.GrowTable(tables);

				return true;
			}
		}

		/// <summary>
		/// Adds the specified item to the <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>true if the items was added to the <see cref="ConcurrentHashSet{T}"/>
		/// successfully; false if it already exists.</returns>
		/// <exception cref="T:System.OverflowException">The <see cref="ConcurrentHashSet{T}"/>
		/// contains too many items.</exception>
		public bool Add(T item)
			=> this.AddInternal(item, this._comparer.GetHashCode(item), true);

		void ICollection<T>.Add(T item)
			=> Add(item);

		/// <summary>
		/// Attempts to remove the item from the <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		/// <param name="item">The item to remove.</param>
		/// <returns>true if an item was removed successfully; otherwise, false.</returns>
		public bool TryRemove(T item)
		{
			var hashcode = this._comparer.GetHashCode(item);
			while (true)
			{
				var tables = this._tables;

				GetBucketAndLockNo(hashcode, out int bucketNo, out int lockNo, tables.Buckets.Length, tables.Locks.Length);

				lock (tables.Locks[lockNo])
				{
					// If the table just got resized, we may not be holding the right lock, and must retry.
					// This should be a rare occurrence.
					if (tables != this._tables)
						continue;

					Node previous = null;
					for (var current = tables.Buckets[bucketNo]; current != null; current = current.Next)
					{
						Debug.Assert((previous == null && current == tables.Buckets[bucketNo]) || previous.Next == current);

						if (hashcode == current.Hashcode && this._comparer.Equals(current.Item, item))
						{
							if (previous == null)
								Volatile.Write(ref tables.Buckets[bucketNo], current.Next);
							else
								previous.Next = current.Next;
							tables.CountPerLock[lockNo]--;
							return true;
						}
						previous = current;
					}
				}
				return false;
			}
		}

		bool ICollection<T>.Remove(T item)
			=> this.TryRemove(item);

		/// <summary>
		/// Removes all items from the <see cref="ConcurrentHashSet{T}"/>.
		/// </summary>
		public void Clear()
		{
			var locksAcquired = 0;
			try
			{
				this.AcquireAllLocks(ref locksAcquired);
				var newTables = new Tables(new Node[DefaultCapacity], this._tables.Locks, new int[this._tables.CountPerLock.Length]);
				this._tables = newTables;
				this._budget = Math.Max(1, newTables.Buckets.Length / newTables.Locks.Length);
			}
			finally
			{
				this.ReleaseLocks(0, locksAcquired);
			}
		}

		/// <summary>
		/// Determines whether the <see cref="ConcurrentHashSet{T}"/> contains the specified
		/// item.
		/// </summary>
		/// <param name="item">The item to locate in the <see cref="ConcurrentHashSet{T}"/>.</param>
		/// <returns>true if the <see cref="ConcurrentHashSet{T}"/> contains the item; otherwise, false.</returns>
		public bool Contains(T item)
		{
			var hashcode = this._comparer.GetHashCode(item);

			// We must capture the _buckets field in a local variable. It is set to a new table on each table resize.
			var tables = this._tables;

			var bucketNo = GetBucket(hashcode, tables.Buckets.Length);

			// We can get away w/out a lock here.
			// The Volatile.Read ensures that the load of the fields of 'n' doesn't move before the load from buckets[i].
			var current = Volatile.Read(ref tables.Buckets[bucketNo]);

			while (current != null)
			{
				if (hashcode == current.Hashcode && this._comparer.Equals(current.Item, item))
					return true;
				current = current.Next;
			}
			return false;
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			var locksAcquired = 0;
			try
			{
				this.AcquireAllLocks(ref locksAcquired);
				var count = 0;
				for (var index = 0; index < this._tables.Locks.Length && count >= 0; index++)
					count += this._tables.CountPerLock[index];

				if (array.Length - count < arrayIndex || count < 0) //"count" itself or "count + arrayIndex" can overflow
					throw new ArgumentException("The index is equal to or greater than the length of the array, or the number of elements in the set is greater than the available space from index to the end of the destination array.");

				var buckets = this._tables.Buckets;
				for (var index = 0; index < buckets.Length; index++)
				{
					for (var current = buckets[index]; current != null; current = current.Next)
					{
						array[arrayIndex] = current.Item;
						arrayIndex++; //this should never flow, CopyToItems is only called when there's no overflow risk
					}
				}
			}
			finally
			{
				this.ReleaseLocks(0, locksAcquired);
			}
		}
		#endregion

		#region Enumerators
		/// <summary>Returns an enumerator that iterates through the <see cref="ConcurrentHashSet{T}"/>.</summary>
		/// <returns>An enumerator for the <see cref="ConcurrentHashSet{T}"/>.</returns>
		/// <remarks>
		/// The enumerator returned from the collection is safe to use concurrently with
		/// reads and writes to the collection, however it does not represent a moment-in-time snapshot
		/// of the collection.  The contents exposed through the enumerator may contain modifications
		/// made to the collection after <see cref="GetEnumerator"/> was called.
		/// </remarks>
		public IEnumerator<T> GetEnumerator()
		{
			var buckets = this._tables.Buckets;
			for (var index = 0; index < buckets.Length; index++)
			{
				// The Volatile.Read ensures that the load of the fields of 'current' doesn't move before the load from buckets[i].
				var current = Volatile.Read(ref buckets[index]);
				while (current != null)
				{
					yield return current.Item;
					current = current.Next;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		#endregion

	}
}