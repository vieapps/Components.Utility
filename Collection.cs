#region Related components
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with collections
	/// </summary>
	public static partial class CollectionService
	{

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
		{
			return @string == null
				? new string[] { }
				: (trim ? @string.Trim() : @string).Split(new[] { separator ?? "," }, removeEmptyElements ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
					.Where(e => removeEmptyElements ? !string.IsNullOrWhiteSpace(e) : true)
					.Select(e => trim ? e.Trim() : e)
					.ToArray();
		}

		/// <summary>
		/// Converts this string to an array
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static string[] ToArray(this string @string, char separator, bool removeEmptyElements = false, bool trim = true)
		{
			return @string == null
				? new string[] { }
				: @string.ToArray(separator.ToString(), removeEmptyElements, trim);
		}

		/// <summary>
		/// Converts this string to an array
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static List<string> ToList(this string @string, string separator = ",", bool removeEmptyElements = false, bool trim = true)
		{
			return @string == null
				? new List<string>()
				: (trim ? @string.Trim() : @string).Split(new[] { separator ?? "," }, removeEmptyElements ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None)
					.Where(e => removeEmptyElements ? !string.IsNullOrWhiteSpace(e) : true)
					.Select(e => trim ? e.Trim() : e)
					.ToList();
		}

		/// <summary>
		/// Converts this string to an array
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static List<string> ToList(this string @string, char separator, bool removeEmptyElements = false, bool trim = true)
		{
			return @string.ToList(separator.ToString(), removeEmptyElements, trim);
		}

		/// <summary>
		/// Converts this string to a hash-set
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static HashSet<string> ToHashSet(this string @string, string separator = ",", bool removeEmptyElements = false, bool trim = true)
		{
			return new HashSet<string>(@string.ToArray(separator, removeEmptyElements, trim));
		}

		/// <summary>
		/// Converts this string to a hash-set
		/// </summary>
		/// <param name="string"></param>
		/// <param name="separator"></param>
		/// <param name="removeEmptyElements"></param>
		/// <param name="trim"></param>
		/// <returns></returns>
		public static HashSet<string> ToHashSet(this string @string, char separator, bool removeEmptyElements = false, bool trim = true)
		{
			return @string.ToHashSet(separator.ToString(), removeEmptyElements, trim);
		}

		/// <summary>
		/// Converts this collection to string
		/// </summary>
		/// <param name="object"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string ToString(this IEnumerable<string> @object, string separator)
		{
			return string.Join(separator ?? "", @object);
		}

		/// <summary>
		/// Converts this collection to string
		/// </summary>
		/// <param name="object"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static string ToString<T>(this IEnumerable<T> @object, string separator)
		{
			return @object.Select(o => o != null ? o.ToString() : "null").ToString(separator);
		}

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
		{
			return @object.Length > 0
				? @object[0]
				: default(T);
		}

		/// <summary>
		/// Gets the first element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T First<T>(this IList<T> @object)
		{
			return @object.Count > 0
				? @object[0]
				: default(T);
		}

		/// <summary>
		/// Gets the first element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T First<T>(this Collection @object)
		{
			return @object.Count > 0
				? (T)@object[0]
				: default(T);
		}

		/// <summary>
		/// Gets the last element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T Last<T>(this T[] @object)
		{
			return @object.Length > 1
				? @object[@object.Length - 1]
				: default(T);
		}

		/// <summary>
		/// Gets the last element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T Last<T>(this IList<T> @object)
		{
			return @object.Count > 1
				? @object[@object.Count - 1]
				: default(T);
		}

		/// <summary>
		/// Gets the last element
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T Last<T>(this Collection @object)
		{
			return @object.Count > 1
				? (T)@object[@object.Count - 1]
				: default(T);
		}
		#endregion

		#region Manipulations
		/// <summary>
		/// Removes an element by index
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static T[] RemoveAt<T>(this T[] @object, int index)
		{
			if (index > -1 && index < @object.Length)
			{
				var array = new T[@object.Length - 1];
				if (index > 0)
					Array.Copy(@object, 0, array, 0, index);
				if (index < @object.Length - 1)
					Array.Copy(@object, index + 1, array, index, @object.Length - index - 1);
				return array;
			}
			else
				return @object;
		}

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="allowDuplicated">true to allow duplicated item existed in the collection</param>
		/// <param name="items">Items to append</param>
		public static void Append<T>(this IList<T> @object, bool allowDuplicated, IEnumerable<T> items)
		{
			items?.ForEach(item =>
			{
				if (allowDuplicated)
					@object.Add(item);
				else if (@object.IndexOf(item) < 0)
					@object.Add(item);
			});
		}

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="items">Items to append</param>
		public static void Append<T>(this IList<T> @object, IEnumerable<T> items)
		{
			@object.Append(true, items);
		}

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="allowDuplicated">true to allow items can be duplicated, false if otherwise</param>
		/// <param name="lists"></param>
		public static void Append<T>(this IList<T> @object, bool allowDuplicated, params IEnumerable<T>[] lists)
		{
			lists?.ForEach(list => @object.Append(allowDuplicated, list));
		}

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="lists"></param>
		public static void Append<T>(this IList<T> @object, params IEnumerable<T>[] lists)
		{
			@object.Append(true, lists);
		}

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
		{
			items?.ForEach(item => @object.Append(item));
		}

		/// <summary>
		/// Appends items into collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="lists"></param>
		public static void Append<T>(this ISet<T> @object, params IEnumerable<T>[] lists)
		{
			lists?.ForEach(list => @object.Append(list));
		}

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
		public static void Randomize<T>(this IList<T> @object)
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
		}
		#endregion

		#region Conversions
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

			var set = new HashSet<T>(checkDuplicated ? new T[] { } : @object);
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
		{
			return @object.AsEnumerableDictionaryEntry
				.Select(entry => (T)entry.Value)
				.ToHashSet(checkDuplicated);
		}

		/// <summary>
		/// Creates a collection from this hash-set
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this HashSet<T> @object, string keyAttribute)
		{
			return (@object as IEnumerable<T>).ToCollection(keyAttribute);
		}

		/// <summary>
		/// Creates a collection from this hash-set
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this HashSet<TValue> @object, string keyAttribute)
		{
			return (@object as IEnumerable<TValue>).ToCollection<TKey, TValue>(keyAttribute);
		}

		/// <summary>
		/// Creates a list from this collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static List<T> ToList<T>(this Collection @object)
		{
			return @object.AsEnumerableDictionaryEntry
				.Select(entry => (T)entry.Value)
				.ToList();
		}

		/// <summary>
		/// Creates a collection from this list
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <returns></returns>
		public static Collection ToCollection<T>(this List<T> @object, string keyAttribute)
		{
			return (@object as IEnumerable<T>).ToCollection(keyAttribute);
		}

		/// <summary>
		/// Creates a collection from this hash-set
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="keyAttribute">The string that presents name of the attribute to get value that will be used as key</param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this List<TValue> @object, string keyAttribute)
		{
			return (@object as IEnumerable<TValue>).ToCollection<TKey, TValue>(keyAttribute);
		}

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
			@object.ForEach(item =>
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
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this IEnumerable<TValue> @object, string keyAttribute)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");
			return new Collection<TKey, TValue>(@object.ToDictionary(item => (TKey)item.GetAttributeValue(keyAttribute)));
		}

		/// <summary>
		/// Creates a dictionary from this collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this Collection @object)
		{
			var dictionary = new Dictionary<TKey, TValue>();
			var enumerator = @object.GetEnumerator();
			while (enumerator.MoveNext())
				dictionary.Add((TKey)enumerator.Entry.Key, (TValue)enumerator.Entry.Value);
			return dictionary;
		}

		/// <summary>
		/// Creates a dictionary from this collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this Collection<TKey, TValue> @object)
		{
			var dictionary = new Dictionary<TKey, TValue>();
			var enumerator = @object.GetEnumerator();
			while (enumerator.MoveNext())
				dictionary.Add(enumerator.Current.Key, enumerator.Current.Value);
			return dictionary;
		}

		/// <summary>
		/// Creates a collection from this dictionary
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <returns></returns>
		public static Collection<TKey, TValue> ToCollection<TKey, TValue>(this Dictionary<TKey, TValue> @object)
		{
			return new Collection<TKey, TValue>(@object);
		}

		/// <summary>
		/// Creates a collection of enums from this collection of enum-strings
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enums"></param>
		/// <returns></returns>
		public static IEnumerable<T> ToEnums<T>(this IEnumerable<string> enums)
		{
			return enums == null
				? null
				: enums.Select(@enum => @enum.ToEnum<T>());
		}

		/// <summary>
		/// Converts this name and value collection to dictionary with all keys in lower case
		/// </summary>
		/// <param name="nvCollection"></param>
		/// <returns></returns>
		public static Dictionary<string, string> ToDictionary(this NameValueCollection nvCollection)
		{
			var dictionary = new Dictionary<string, string>();
			foreach (string key in nvCollection)
				if (!dictionary.ContainsKey(key.ToLower()))
					dictionary.Add(key.ToLower(), nvCollection[key]);
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
			dictionary.ForEach(entry => nvCollection.Add(entry.Key, entry.Value));
			return nvCollection;
		}
		#endregion

		#region Special conversions
		/// <summary>
		/// Converts the collection of objects to the generic list of strong-typed objects
		/// </summary>
		/// <param name="object"></param>
		/// <param name="type">The type of elements</param>
		/// <returns></returns>
		public static IEnumerable ToList(this List<object> @object, Type type)
		{
			var list = typeof(List<>).MakeGenericType(type).CreateInstance() as IList;
			@object.ForEach(element =>
			{
				// assign value
				object value = element;

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
						var obj = type.CreateInstance();
						obj.CopyFrom(value as ExpandoObject);
						value = obj;
					}
				}

				// value is primitive type
				else if (type.IsPrimitiveType() && !type.Equals(value.GetType()))
					value = Convert.ChangeType(value, type);

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
		{
			return typeof(T).IsGenericList()
				? (T)@object.ToList(typeof(T).GenericTypeArguments[0])
				: default(T);
		}

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
		{
			return typeof(T).IsGenericHashSet()
				? (T)@object.ToHashSet(typeof(T).GenericTypeArguments[0])
				: default(T);
		}

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
			@object.ForEach(element =>
			{
				// key
				object key = element.Key;
				if (!keyType.Equals(key.GetType()))
					key = Convert.ChangeType(key, keyType);

				// value
				object value = element.Value;

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
						var obj = valueType.CreateInstance();
						obj.CopyFrom(value as ExpandoObject);
						value = obj;
					}
				}

				// value is primitive
				else if (valueType.IsPrimitiveType() && !valueType.Equals(value.GetType()))
					value = Convert.ChangeType(value, valueType);

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
		{
			return typeof(T).IsGenericDictionary()
				? (T)@object.ToDictionary(typeof(T).GenericTypeArguments[0], typeof(T).GenericTypeArguments[1])
				: default(T);
		}

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
		{
			return typeof(T).IsGenericCollection()
				? (T)@object.ToCollection(typeof(T).GenericTypeArguments[0], typeof(T).GenericTypeArguments[1])
				: default(T);
		}
		#endregion

		#region JSON (JArray) Conversions
		/// <summary>
		/// Creates a JArray object from this collection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static JArray ToJArray<T>(this IEnumerable<T> @object, Func<T, JToken> converter = null)
		{
			if (typeof(T).IsPrimitiveType() || typeof(T).IsClassType())
			{
				var array = new JArray();
				@object.ForEach(item => array.Add(converter != null ? converter(item) : item?.ToJson()));
				return array;
			}
			else
				return JArray.FromObject(@object);
		}

		/// <summary>
		/// Creates a collection of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static List<T> ToList<T>(this JArray json, Func<JToken, T> converter = null)
		{
			var list = new List<T>();
			json.ForEach(token => list.Add(converter != null ? converter(token) : token.FromJson<T>()));
			return list;
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
		{
			var json = new JArray();
			var enumerator = @object.GetEnumerator();
			while (enumerator.MoveNext())
				json.Add(converter != null ? converter(enumerator.Current.Value) : enumerator.Current.Value?.ToJson());
			return json;
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
			json.ForEach(token =>
			{
				var @object = converter != null ? converter(token) : token.FromJson<TValue>();
				var key = (TKey)@object.GetAttributeValue(keyAttribute);
				if (key != null && !dictionary.ContainsKey(key))
					dictionary.Add(key, @object);
			});
			return dictionary;
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
		{
			return new Collection<TKey, TValue>(json.ToDictionary<TKey, TValue>(keyAttribute, converter));
		}

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
				json.Add(converter != null ? converter(enumerator.Current) : enumerator.Current?.ToJson());
			return json;
		}

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
			json.ForEach(token =>
			{
				var @object = converter != null ? converter(token) : token.FromJson<T>();
				var key = @object.GetAttributeValue(keyAttribute);
				if (key != null && !collection.Contains(key))
					collection.Add(key, @object);
			});
			return collection;
		}
		#endregion

		#region JSON (JObject) Conversions
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
			@object.ForEach(item =>
			{
				var key = item.GetAttributeValue(keyAttribute) ?? item.GetHashCode();
				json.Add(new JProperty(key.ToString(), converter != null ? converter(item) : item.ToJson<T>()));
			});
			return json;
		}

		/// <summary>
		/// Creates a collection of objects from this JSON
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object that presents the serialized data of collection of objects</param>
		/// <param name="converter">The conversion</param>
		/// <returns></returns>
		public static List<T> ToList<T>(this JObject json, Func<JToken, T> converter = null)
		{
			var list = new List<T>();
			json.ForEach(token => list.Add(converter != null ? converter(token) : token.FromJson<T>()));
			return list;
		}

		/// <summary>
		/// Creates a JArray object from this collection
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
				json.Add(new JProperty(enumerator.Current.Key.ToString(), converter != null ? converter(enumerator.Current.Value) : enumerator.Current.Value?.ToJson()));
			return json;
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
		public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this JObject json, string keyAttribute, Func<JToken, TValue> converter = null)
		{
			if (string.IsNullOrWhiteSpace(keyAttribute))
				throw new ArgumentNullException(nameof(keyAttribute), "The name of key attribute is null");

			var dictionary = new Dictionary<TKey, TValue>();
			foreach (var token in json)
			{
				var @object = converter != null ? converter(token.Value) : token.Value.FromJson<TValue>();
				var key = (TKey)@object.GetAttributeValue(keyAttribute);
				if (key != null && !dictionary.ContainsKey(key))
					dictionary.Add(key, @object);
			}
			return dictionary;
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
		{
			return new Collection<TKey, TValue>(json.ToDictionary<TKey, TValue>(keyAttribute, converter));
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
				json.Add(new JProperty(enumerator.Current.Key.ToString(), converter != null ? converter(enumerator.Current.Value) : enumerator.Current.Value?.ToJson()));
			return json;
		}

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
			foreach (var token in json)
			{
				var @object = converter != null ? converter(token.Value) : token.Value.FromJson<T>();
				var key = @object.GetAttributeValue(keyAttribute);
				if (key != null && !collection.Contains(key))
					collection.Add(key, @object);
			}
			return collection;
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
				json.Add(new JProperty(key, @object[key]));
			return json;
		}

		/// <summary>
		/// Creates a name-value collection from JSON object
		/// </summary>
		/// <param name="json"></param>
		public static NameValueCollection ToNameValueCollection(this JObject json)
		{
			var nvCollection = new NameValueCollection();
			foreach (var token in json)
				if (token.Value != null && token.Value is JValue && (token.Value as JValue).Value != null)
				{
					if (nvCollection[token.Key] != null)
						nvCollection.Set(token.Key, (token.Value as JValue).Value as string);
					else
						nvCollection.Add(token.Key, (token.Value as JValue).Value as string);
				}
			return nvCollection;
		}
		#endregion

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
		/// <param name="enumerable"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> enumerable, Action<TValue> action)
		{
			foreach (var item in enumerable)
				action(item.Value);
		}

		/// <summary>
		/// Performs the specified action on each element of the collection
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="action">The delegated action to perform on each element of the collection</param>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> enumerable, Action<TValue, int> action)
		{
			var index = -1;
			foreach (var item in enumerable)
			{
				index++;
				action(item.Value, index);
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
		public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, CancellationToken, Task> actionAsync, CancellationToken cancellationToken = default(CancellationToken), bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
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
		/// <param name="enumerable"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<T>(this IEnumerable<T> enumerable, Func<T, int, CancellationToken, Task> actionAsync, CancellationToken cancellationToken = default(CancellationToken), bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var item in enumerable)
				{
					index++;
					await actionAsync(item, index, cancellationToken).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var item in enumerable)
				{
					index++;
					tasks.Add(actionAsync(item, index, cancellationToken));
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
		/// <param name="enumerable"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> enumerable, Func<TValue, CancellationToken, Task> actionAsync, CancellationToken cancellationToken = default(CancellationToken), bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			if (!parallelExecutions)
				foreach (var item in enumerable)
					await actionAsync(item.Value, cancellationToken).ConfigureAwait(captureContext);

			else
			{
				var tasks = enumerable.Select(item => actionAsync(item.Value, cancellationToken)).ToList();
				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}

		/// <summary>
		/// Performs the specified action on each element of the collection (in asynchronous way)
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="actionAsync">The delegated action to perform on each element of the collection</param>
		/// <param name="cancellationToken">The cancellation token</param>
		/// <param name="waitForAllCompleted">true to wait for all tasks are completed before leaving; otherwise false to fire-and-forget.</param>
		/// <param name="parallelExecutions">true to execute all tasks in parallel; otherwise false to execute in sequence.</param>
		/// <param name="captureContext">true to capture/return back to calling context.</param>
		/// <returns></returns>
		public static async Task ForEachAsync<TKey, TValue>(this IDictionary<TKey, TValue> enumerable, Func<TValue, int, CancellationToken, Task> actionAsync, CancellationToken cancellationToken = default(CancellationToken), bool waitForAllCompleted = true, bool parallelExecutions = true, bool captureContext = false)
		{
			var index = -1;
			if (!parallelExecutions)
				foreach (var item in enumerable)
				{
					index++;
					await actionAsync(item.Value, index, cancellationToken).ConfigureAwait(captureContext);
				}

			else
			{
				var tasks = new List<Task>();
				foreach (var item in enumerable)
				{
					index++;
					tasks.Add(actionAsync(item.Value, index, cancellationToken));
				}

				if (waitForAllCompleted)
					await Task.WhenAll(tasks).ConfigureAwait(captureContext);
			}
		}
		#endregion

	}

	// ---------------------------------------------------------------------------

	/// <summary>
	/// Represents a collection of key/value pairs that are accessible by key or index
	/// </summary>
	[Serializable, DebuggerDisplay("Count = {Count}")]
	public class Collection : IDictionary
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="net.vieapps.Components.Utility.Collection"/> class
		/// </summary>
		public Collection() { }

		OrderedDictionary _collection = new OrderedDictionary();

		#region Properties
		/// <summary>
		/// Gets the number of key/values pairs contained in the collection
		/// </summary>
		public int Count
		{
			get
			{
				return this._collection.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection is read-only
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return this._collection.IsReadOnly;
			}
		}

		/// <summary>
		/// Gets a value indicating whether access to the collection is synchronized
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return (this._collection as ICollection).IsSynchronized;
			}
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the collection
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return (this._collection as ICollection).SyncRoot;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection has a fixed size
		/// </summary>
		public bool IsFixedSize
		{
			get
			{
				return (this._collection as IDictionary).IsFixedSize;
			}
		}

		/// <summary>
		/// Gets an object containing the keys in the collection
		/// </summary>
		public ICollection Keys
		{
			get
			{
				return this._collection.Keys;
			}
		}

		/// <summary>
		/// Gets an object containing the values in the collection
		/// </summary>
		public ICollection Values
		{
			get
			{
				return this._collection.Values;
			}
		}

		/// <summary>
		/// Gets or sets the value with the specified key
		/// </summary>
		/// <param name="key">The key of the value to get or set</param>
		/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new element using the specified key</returns>
		public object this[object key]
		{
			get
			{
				return this._collection[key];
			}
			set
			{
				this._collection[key] = value;
			}
		}

		/// <summary>
		/// Gets or sets the value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the value to get or set</param>
		/// <returns>The value of the item at the specified index</returns>
		public object this[int index]
		{
			get
			{
				return this._collection[index];
			}
			set
			{
				this._collection[index] = value;
			}
		}

		/// <summary>
		/// Gets the object that cast as enumerable of dictionary entry
		/// </summary>
		public IEnumerable<DictionaryEntry> AsEnumerableDictionaryEntry
		{
			get
			{
				return this._collection.Cast<DictionaryEntry>();
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Copies the elements of the collection to an array, starting at a particular index
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in array at which copying begins</param>
		public void CopyTo(Array array, int index)
		{
			(this._collection as ICollection).CopyTo(array, index);
		}

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool Contains(object key)
		{
			return key != null && this._collection.Contains(key);
		}

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool ContainsKey(object key)
		{
			return this.Contains(key);
		}

		/// <summary>
		/// Adds an element with the specified key and value into the collection with the lowest available index
		/// </summary>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Add(object key, object value)
		{
			this._collection.Add(key, value);
		}

		/// <summary>
		/// Inserts a new element into the collection with the specified key and value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted</param>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Insert(int index, object key, object value)
		{
			this._collection.Insert(index, key, value);
		}

		void IDictionary.Remove(object key)
		{
			this.Remove(key);
		}

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
			else
				return false;
		}

		/// <summary>
		/// Removes the element at the specified index from the collection
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove</param>
		public void RemoveAt(int index)
		{
			this._collection.RemoveAt(index);
		}

		/// <summary>
		/// Removes all elements from the collection
		/// </summary>
		public void Clear()
		{
			this._collection.Clear();
		}

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
			if (!object.ReferenceEquals(@object, null))
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
		{
			return index > -1 && index < this.Count
				? this.AsEnumerableDictionaryEntry.ElementAt(index).Key
				: null;
		}
		#endregion

		#region Enumerator
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection
		/// </summary>
		/// <returns></returns>
		public IDictionaryEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public class Enumerator : IDictionaryEnumerator
		{
			int _index = -1;
			Collection _collection = null;
			List<object> _keys = new List<object>();

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
			{
				this._index = -1;
			}

			/// <summary>
			/// Gets the value of current element in the collection
			/// </summary>
			public object Current
			{
				get
				{
					return this.Value;
				}
			}

			/// <summary>
			/// Gets the key of the current element
			/// </summary>
			public object Key
			{
				get
				{
					return this._collection != null && this._index > -1 && this._index < this._collection.Count
						? this._keys[this._index]
						: null;
				}
			}

			/// <summary>
			/// Gets the value of the current element
			/// </summary>
			public object Value
			{
				get
				{
					return this._collection != null && this._index > -1 && this._index < this._collection.Count
						? this._collection[this._index]
						: null;
				}
			}

			/// <summary>
			/// Gets both the key and the value of the current element
			/// </summary>
			public DictionaryEntry Entry
			{
				get
				{
					return new DictionaryEntry(this.Key, this.Value);
				}
			}
		}
		#endregion

	}

	/// <summary>
	/// Represents a generic collection of key/value pairs that are accessible by key or index
	/// </summary>
	/// <typeparam name="TKey">The type of all keys</typeparam>
	/// <typeparam name="TValue">The type of all values</typeparam>
	[Serializable, DebuggerDisplay("Count = {Count}")]
	public class Collection<TKey, TValue> : Collection, IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="net.vieapps.Components.Utility.Collection">Collection</see> class
		/// </summary>
		/// <param name="dictionary">Initialized values</param>
		public Collection(IDictionary<TKey, TValue> dictionary = null) : base()
		{
			dictionary?.ForEach(kvp => this.Add(kvp));
		}

		#region Properties
		/// <summary>
		/// Gets or sets the value with the specified key
		/// </summary>
		/// <param name="key">The key of the value to get or set</param>
		/// <returns>The value associated with the specified key. If the specified key is not found, attempting to get it returns null, and attempting to set it creates a new element using the specified key</returns>
		public TValue this[TKey key]
		{
			get
			{
				return (TValue)base[key];
			}
			set
			{
				base[key] = value;
			}
		}

		/// <summary>
		/// Gets or sets the value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index of the value to get or set</param>
		/// <returns>The value of the item at the specified index</returns>
		public new TValue this[int index]
		{
			get
			{
				return (TValue)base[index];
			}
			set
			{
				base[index] = value;
			}
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
		{
			base.CopyTo(array, index);
		}

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool Contains(TKey key)
		{
			return base.Contains(key);
		}

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="key">The key to locate in the collection</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool ContainsKey(TKey key)
		{
			return this.Contains(key);
		}

		/// <summary>
		/// Determines whether the collection contains a specific key
		/// </summary>
		/// <param name="element">The element that contains both the key and the value</param>
		/// <returns>true if the collection contains an element with the specified key; otherwise, false.</returns>
		public bool Contains(KeyValuePair<TKey, TValue> element)
		{
			return this.Contains(element.Key);
		}

		/// <summary>
		/// Adds an element with the specified key and value into the collection with the lowest available index
		/// </summary>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Add(TKey key, TValue value)
		{
			base.Add(key, value);
		}

		/// <summary>
		/// Adds an element into the collection with the lowest available index
		/// </summary>
		/// <param name="element">The element that contains both the key and the value</param>
		public void Add(KeyValuePair<TKey, TValue> element)
		{
			this.Add(element.Key, element.Value);
		}

		/// <summary>
		/// Inserts a new element into the collection with the specified key and value at the specified index
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted</param>
		/// <param name="key">The key of the element to add. Key must be not null.</param>
		/// <param name="value">The value of element to add. Value can be null.</param>
		public void Insert(int index, TKey key, TValue value)
		{
			base.Insert(index, key, value);
		}

		/// <summary>
		/// Removes the element with the specified key from the collection
		/// </summary>
		/// <param name="key">The key of the element to remove</param>
		/// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the collection</returns>
		public bool Remove(TKey key)
		{
			return base.Remove(key);
		}

		/// <summary>
		/// Removes the element with the specified key from the collection
		/// </summary>
		/// <param name="element">The element that contains both the key and the value</param>
		/// <returns>true if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the collection.</returns>
		public bool Remove(KeyValuePair<TKey, TValue> element)
		{
			return this.Remove(element.Key);
		}

		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first occurrence within the entire the collection
		/// </summary>
		/// <param name="object">The object to locate in the collections. The value can be null for reference types</param>
		/// <returns>The zero-based index of the first occurrence of item within the entire the collections if found; otherwise, –1.</returns>
		public int IndexOf(TValue @object)
		{
			return base.IndexOf(@object);
		}

		/// <summary>
		/// Searches for the specified key and returns the zero-based index of the first occurrence within the entire the collection
		/// </summary>
		/// <param name="key">The key to locate in the collections. The value cannot be null</param>
		/// <returns>The zero-based index of the first occurrence of item within the entire the collections if found; otherwise, –1.</returns>
		public int IndexOfKey(TKey key)
		{
			return base.IndexOfKey(key);
		}

		/// <summary>
		/// Gets a key at the specified index
		/// </summary>
		/// <param name="index">Index of the key</param>
		/// <returns>The key object at the specified index</returns>
		public new TKey GetKeyAt(int index)
		{
			return (TKey)base.GetKeyAt(index);
		}

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
			else
			{
				value = default(TValue);
				return false;
			}
		}

		/// <summary>
		/// Gets the first element
		/// </summary>
		/// <returns></returns>
		public TValue First()
		{
			return this.Count > 0
				? (TValue)base[0]
				: default(TValue);
		}

		/// <summary>
		/// Gets the last element
		/// </summary>
		/// <returns></returns>
		public TValue Last()
		{
			return this.Count > 0
				? (TValue)base[this.Count - 1]
				: default(TValue);
		}
		#endregion

		#region Enumerator
		/// <summary>
		/// Returns an enumerator that iterates through the collection
		/// </summary>
		/// <returns></returns>
		public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return new Enumerator(this);
		}

		public new class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
		{
			int _index = -1;
			Collection<TKey, TValue> _collection = null;
			List<TKey> _keys = new List<TKey>();

			public Enumerator() : this(null) { }

			public Enumerator(Collection<TKey, TValue> collection)
			{
				this._collection = collection;
				if (this._collection != null)
					this._keys = this._collection.Keys as List<TKey>;
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
			{
				this._index = -1;
			}

			public void Dispose()
			{
				this._collection = null;
			}

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			/// <summary>
			/// Gets the value of current element in the collection
			/// </summary>
			public KeyValuePair<TKey, TValue> Current
			{
				get
				{
					return new KeyValuePair<TKey, TValue>(this.Key, this.Value);
				}
			}

			/// <summary>
			/// Gets the key of the current element
			/// </summary>
			public TKey Key
			{
				get
				{
					return this._collection != null && this._index > -1 && this._index < this._collection.Count
						? this._keys[this._index]
						: default(TKey);
				}
			}

			/// <summary>
			/// Gets the value of the current element
			/// </summary>
			public TValue Value
			{
				get
				{
					return this._collection != null && this._index > -1 && this._index < this._collection.Count
						? this._collection[this._index]
						: default(TValue);
				}
			}
		}
		#endregion

	}
}