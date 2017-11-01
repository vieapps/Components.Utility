﻿#region Related components
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Xml.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with objects
	/// </summary>
	public static class ObjectService
	{

		#region Attribute info
		/// <summary>
		/// Presents information of an attribute of an objects
		/// </summary>
		[Serializable, DebuggerDisplay("Name = {Name}")]
		public class AttributeInfo
		{
			public AttributeInfo() : this(null, null) { }

			public AttributeInfo(string name, MemberInfo info)
			{
				this.Name = name;
				this.Info = info;
			}

			public string Name { get; internal set; }

			public MemberInfo Info { get; internal set; }

			/// <summary>
			/// Specifies this attribute can be read
			/// </summary>
			public bool CanRead
			{
				get
				{
					return this.Info is PropertyInfo
						? (this.Info as PropertyInfo).CanRead
						: true;
				}
			}

			/// <summary>
			/// Specifies this attribute can be written
			/// </summary>
			public bool CanWrite
			{
				get
				{
					return this.Info is PropertyInfo
						? (this.Info as PropertyInfo).CanWrite
						: true;
				}
			}

			/// <summary>
			/// Specifies this attribute is public (everyone can access)
			/// </summary>
			public bool IsPublic
			{
				get
				{
					return this.Info is PropertyInfo;
				}
			}

			/// <summary>
			/// Gets the type of the attribute
			/// </summary>
			public Type Type
			{
				get
				{
					return this.Info is PropertyInfo
						? (this.Info as PropertyInfo).PropertyType
						: (this.Info as FieldInfo).FieldType;
				}
			}
		}
		#endregion

		#region Object meta data
		static Dictionary<Type, List<AttributeInfo>> ObjectProperties = new Dictionary<Type, List<AttributeInfo>>();
		static Dictionary<Type, List<AttributeInfo>> ObjectFields = new Dictionary<Type, List<AttributeInfo>>();

		/// <summary>
		/// Gets the collection of public properties of the type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns>Collection of public properties</returns>
		public static List<AttributeInfo> GetProperties(Type type)
		{
			if (!ObjectService.ObjectProperties.TryGetValue(type, out List<AttributeInfo> properties))
				lock (ObjectService.ObjectProperties)
				{
					if (!ObjectService.ObjectProperties.TryGetValue(type, out properties))
					{
						var defaultMembers = type.GetCustomAttributes(typeof(DefaultMemberAttribute));
						var defaultMember = defaultMembers != null
							? defaultMembers.FirstOrDefault() as DefaultMemberAttribute
							: null;
						properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
							.Where(info => defaultMember == null || !defaultMember.MemberName.Equals(info.Name))
							.Select(info => new AttributeInfo(info.Name, info))
							.ToList();
						ObjectService.ObjectProperties.Add(type, properties);
					}
				}
			return properties;
		}

		/// <summary>
		/// Gets the collection of public properties of the object's type
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <returns>Collection of public properties</returns>
		public static List<AttributeInfo> GetProperties(this object @object)
		{
			return ObjectService.GetProperties(@object.GetType());
		}

		/// <summary>
		/// Gets the collection of fields (private attributes) of the type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<AttributeInfo> GetFields(Type type)
		{
			if (!ObjectService.ObjectFields.TryGetValue(type, out List<AttributeInfo> attributes))
				lock (ObjectService.ObjectFields)
				{
					if (!ObjectService.ObjectFields.TryGetValue(type, out attributes))
					{
						attributes = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
							.Where(info => !info.Name.StartsWith("<"))
							.Select(info => new AttributeInfo(info.Name, info))
							.ToList();
						ObjectService.ObjectFields.Add(type, attributes);
					}
				}
			return attributes;
		}

		/// <summary>
		/// Gets the collection of fields (private attributes) of the object's type
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<AttributeInfo> GetFields(this object @object)
		{
			return ObjectService.GetFields(@object.GetType());
		}

		/// <summary>
		/// Gets the collection of attributes of the type (means contains all public properties and private fields)
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns>Collection of attributes</returns>
		public static List<AttributeInfo> GetAttributes(Type type)
		{
			return ObjectService.GetProperties(type)
				.Concat(ObjectService.GetFields(type))
				.ToList();
		}

		/// <summary>
		/// Gets the collection of attributes of the object's type (means contains all public properties and private fields)
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <returns>Collection of attributes</returns>
		public static List<AttributeInfo> GetAttributes(this object @object)
		{
			return ObjectService.GetAttributes(@object.GetType());
		}

		/// <summary>
		/// Get the full type name (type name with assembly name) of this type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="justName">true to get only name (means last element in full namespace)</param>
		/// <returns>The string that presents type name</returns>
		public static string GetTypeName(this Type type, bool justName = false)
		{
			return justName
				? type.FullName.ToArray('.').Last()
				: type.FullName + "," + type.Assembly.GetName().Name;
		}

		/// <summary>
		/// Gets the state to determines the type is primitive or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is primitive</returns>
		public static bool IsPrimitiveType(this Type type)
		{
			return type.IsPrimitive || type.IsStringType() || type.IsDateTimeType() || type.IsNumericType();
		}

		/// <summary>
		/// Gets the state to determines the type is string or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is string</returns>
		public static bool IsStringType(this Type type)
		{
			return type.Equals(typeof(String));
		}

		/// <summary>
		/// Gets the state to determines the type is date-time or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is date-time</returns>
		public static bool IsDateTimeType(this Type type)
		{
			return type.Equals(typeof(DateTime));
		}

		/// <summary>
		/// Gets the state to determines the type is integral numeric or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is integral numeric</returns>
		public static bool IsIntegralType(this Type type)
		{
			return type.Equals(typeof(Byte)) || type.Equals(typeof(SByte))
				|| type.Equals(typeof(Int16)) || type.Equals(typeof(Int32)) || type.Equals(typeof(Int64))
				|| type.Equals(typeof(UInt16)) || type.Equals(typeof(UInt32)) || type.Equals(typeof(UInt64));
		}

		/// <summary>
		/// Gets the state to determines the type is floating numeric or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is floating numeric</returns>
		public static bool IsFloatingPointType(this Type type)
		{
			return type.Equals(typeof(Decimal)) || type.Equals(typeof(Double)) || type.Equals(typeof(Single));
		}

		/// <summary>
		/// Gets the state to determines the type is numeric or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsNumericType(this Type type)
		{
			return type.IsIntegralType() || type.IsFloatingPointType();
		}

		/// <summary>
		/// Gets the state to determines the type is a reference of a class or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsClassType(this Type type)
		{
			return !type.IsPrimitiveType() && type.IsClass;
		}
		#endregion

		#region Collection meta data
		/// <summary>
		/// Gets the state to determines this type is sub-class of a generic type
		/// </summary>
		/// <param name="type">The type for checking</param>
		/// <param name="genericType">The generic type for checking</param>
		/// <returns>true if the checking type is sub-class of the generic type</returns>
		public static bool IsSubclassOfGeneric(this Type type, Type genericType)
		{
			if (genericType == null || !genericType.IsGenericType)
				return false;

			while (type != null && type != typeof(object))
			{
				var current = type.IsGenericType
					? type.GetGenericTypeDefinition()
					: type;

				if (genericType == current)
					return true;

				type = type.BaseType;
			}

			return false;
		}

		/// <summary>
		/// Gets the state to determines the type is a generic list
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericList(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(List<>));
		}

		/// <summary>
		/// Gets the state to determines the object's type is a generic list
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic list; otherwise false.</returns>
		public static bool IsGenericList(this object @object)
		{
			return @object.GetType().IsGenericList();
		}

		/// <summary>
		/// Gets the state to determines the type is a generic hash-set
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic hash-set; otherwise false.</returns>
		public static bool IsGenericHashSet(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(HashSet<>));
		}

		/// <summary>
		/// Gets the state to determines the object's type is a generic hash-set
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic hash-set; otherwise false.</returns>
		public static bool IsGenericHashSet(this object @object)
		{
			return @object.GetType().IsGenericHashSet();
		}

		/// <summary>
		/// Gets the state to determines the type is generic list or generic hash-set
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic list or generic hash-set; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this Type type)
		{
			return type.IsGenericList() || type.IsGenericHashSet();
		}

		/// <summary>
		/// Gets the state to determines the object's type is generic list or generic hash-set
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic list or generic hash-set; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this object @object)
		{
			return @object.GetType().IsGenericListOrHashSet();
		}

		/// <summary>
		/// Gets the state to determines the type is reference of a generic dictionary
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic dictionary; otherwise false.</returns>
		public static bool IsGenericDictionary(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(Dictionary<,>));
		}

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a generic dictionary
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic dictionary; otherwise false.</returns>
		public static bool IsGenericDictionary(this object @object)
		{
			return @object.GetType().IsGenericDictionary();
		}

		/// <summary>
		/// Gets the state to determines the type is reference of a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericCollection(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(Collection<,>));
		}

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericCollection(this object @object)
		{
			return @object.GetType().IsGenericCollection();
		}

		/// <summary>
		/// Gets the state to determines the type is reference of a generic dictionary or a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this Type type)
		{
			return type.IsGenericDictionary() || type.IsGenericCollection();
		}

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a generic dictionary or a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this object @object)
		{
			return @object.GetType().IsGenericDictionaryOrCollection();
		}

		/// <summary>
		/// Gets the state to determines the type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface; otherwise false.</returns>
		public static bool IsICollection(this Type type)
		{
			return typeof(ICollection).IsAssignableFrom(type) || type.IsGenericHashSet();
		}

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface; otherwise false.</returns>
		public static bool IsICollection(this object @object)
		{
			return @object.GetType().IsICollection();
		}

		/// <summary>
		/// Gets the state to determines the type is reference (or sub-class) of the the <see cref="Collection">Collection</see> class
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is is reference (or sub-class) of the the <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsCollection(this Type type)
		{
			return typeof(Collection).IsAssignableFrom(type);
		}

		/// <summary>
		/// Gets the state to determines the type of the object is reference (or sub-class) of the the <see cref="Collection">Collection</see> class
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is is reference (or sub-class) of the the <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsCollection(this object @object)
		{
			return @object.GetType().IsCollection();
		}

		/// <summary>
		/// Gets the state to determines the the object is array or not
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is is reference (or sub-class) of the the <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsArray(this object @object)
		{
			return @object.GetType().IsArray;
		}
		#endregion

		#region Create new instance & Cast
		static Dictionary<Type, Func<object>> TypeFactoryCache = new Dictionary<Type, Func<object>>();

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection
		/// </summary>
		/// <param name="type">The type to be created</param>
		/// <returns>The newly created instance</returns>
		public static object CreateInstance(this Type type)
		{
			if (!ObjectService.TypeFactoryCache.TryGetValue(type, out Func<object> func))
				lock (ObjectService.TypeFactoryCache)
				{
					if (!ObjectService.TypeFactoryCache.TryGetValue(type, out func))
						ObjectService.TypeFactoryCache[type] = func = Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
				}
			return func();
		}

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection.
		/// </summary>
		/// <typeparam name="T">The type to be created</typeparam>
		/// <returns>The newly created instance</returns>
		public static T CreateInstance<T>()
		{
			return (T)typeof(T).CreateInstance();
		}

		/// <summary>
		/// Casts the value to other type
		/// </summary>
		/// <param name="value">The value to cast to other type</param>
		/// <param name="type">The type to cast type of value to</param>
		/// <returns></returns>
		public static object CastAs(this object value, Type type)
		{
			return value != null
				? value.GetType().Equals(type)
					? value
					: Convert.ChangeType(value, type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) ? Nullable.GetUnderlyingType(type) : type)
				: null;
		}

		/// <summary>
		/// Casts the value to other type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The value to cast to other type</param>
		/// <returns></returns>
		public static T CastAs<T>(this object value)
		{
			var type = typeof(T);
			return value != null
				? value.GetType().Equals(type)
					? (T)value
					: (T)Convert.ChangeType(value, type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) ? Nullable.GetUnderlyingType(type) : type)
				: default(T);
		}
		#endregion

		#region Manipulations
		/// <summary>
		/// Sets value of an attribute (public/private/static) of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="name">The string that presents the name of the attribute need to get.</param>
		/// <param name="value">The object that presents the value of the attribute need to set.</param>
		public static void SetAttributeValue(this object @object, string name, object value)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException(nameof(name));

			var fieldInfo = @object.GetType().GetField(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (fieldInfo != null)
				fieldInfo.SetValue(@object, value);

			else
			{
				var propertyInfo = @object.GetType().GetProperty(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				if (propertyInfo != null && propertyInfo.CanWrite)
					propertyInfo.SetValue(@object, value);
			}
		}

		/// <summary>
		/// Sets value of an attribute (public/private/static) of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="attribute">The object that presents information of the attribute need to get</param>
		/// <param name="value">The object that presents the value of the attribute need to set.</param>
		/// <param name="cast">true to cast the type of attribute</param>
		public static void SetAttributeValue(this object @object, AttributeInfo attribute, object value, bool cast = false)
		{
			if (attribute == null)
				throw new ArgumentException(nameof(attribute));
			@object.SetAttributeValue(attribute.Name, cast && value != null ? value.CastAs(attribute.Type) : value);
		}

		/// <summary>
		/// Gets value of an attribute of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="name">The string that presents the name of the attribute need to get.</param>
		/// <returns>The object that presents data of object's attribute</returns>
		public static object GetAttributeValue(this object @object, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException(nameof(name));

			var fieldInfo = @object.GetType().GetField(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (fieldInfo != null)
				return fieldInfo.GetValue(@object);

			else
			{
				var propertyInfo = @object.GetType().GetProperty(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				return propertyInfo != null && propertyInfo.CanRead
					? propertyInfo.GetValue(@object)
					: null;
			}
		}

		/// <summary>
		/// Gets value of an attribute of an object.
		/// </summary>
		/// <param name="object">The object need to get data from</param>
		/// <param name="attribute">The object that presents information of the attribute need to get</param>
		/// <returns>The object that presents data of object's attribute</returns>
		public static object GetAttributeValue(this object @object, AttributeInfo attribute)
		{
			if (attribute == null || string.IsNullOrWhiteSpace(attribute.Name))
				throw new ArgumentException(nameof(attribute));

			return @object.GetAttributeValue(attribute.Name);
		}

		/// <summary>
		/// Gets a static object that specified by name
		/// </summary>
		/// <param name="type">The type of the static class that contains the static object</param>
		/// <param name="name">The name of the static object</param>
		/// <returns></returns>
		public static object GetStaticObject(this Type type, string name)
		{
			if (string.IsNullOrEmpty(name))
				return null;

			var fieldInfo = type.GetField(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (fieldInfo != null)
				return fieldInfo.GetValue(null);

			else
			{
				var propertyInfo = type.GetProperty(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				return propertyInfo != null && propertyInfo.CanRead
					? propertyInfo.GetValue(null, null)
					: null;
			}
		}

		/// <summary>
		/// Gets a static object that specified by name
		/// </summary>
		/// <param name="class">The string that present the type of the static class that contains the static object</param>
		/// <param name="name">The name of the static object</param>
		/// <returns></returns>
		public static object GetStaticObject(string @class, string name)
		{
			var type = Type.GetType(@class);
			return type != null
				? type.GetStaticObject(name)
				: null;
		}
		#endregion

		#region Copy & Clone
		/// <summary>
		/// Copies data of the object to other object
		/// </summary>
		/// <param name="object">The object to get data from</param>
		/// <param name="destination">The destination object that will be copied to</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <returns>The object that presents data of object's attribute</returns>
		public static void CopyTo(this object @object, object destination, HashSet<string> excluded = null)
		{
			if (object.ReferenceEquals(destination, null))
				throw new ArgumentNullException(nameof(destination), "The destination is null");

			@object.GetProperties().ForEach(attribute =>
			{
				if (attribute.CanWrite && (excluded == null || excluded.Count < 1 || !excluded.Contains(attribute.Name)))
					try
					{
						destination.SetAttributeValue(attribute, @object.GetAttributeValue(attribute));
					}
					catch { }
			});

			@object.GetFields().ForEach(attribute =>
			{
				if (excluded == null || excluded.Count < 1 || !excluded.Contains(attribute.Name))
					try
					{
						destination.SetAttributeValue(attribute, @object.GetAttributeValue(attribute));
					}
					catch { }
			});
		}

		/// <summary>
		/// Copies data of the source object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="source">Source object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		public static void CopyFrom(this object @object, object source, HashSet<string> excluded = null)
		{
			if (object.ReferenceEquals(source, null))
				throw new ArgumentNullException(nameof(source), "The source is null");

			@object.GetProperties().ForEach(attribute =>
			{
				if (attribute.CanWrite && (excluded == null || excluded.Count < 1 || !excluded.Contains(attribute.Name)))
					try
					{
						@object.SetAttributeValue(attribute, source.GetAttributeValue(attribute));
					}
					catch { }
			});

			@object.GetFields().ForEach(attribute =>
			{
				if (excluded == null || excluded.Count < 1 || !excluded.Contains(attribute.Name))
					try
					{
						@object.SetAttributeValue(attribute, source.GetAttributeValue(attribute));
					}
					catch { }
			});
		}

		/// <summary>
		/// Copies data of the JSON object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="json">JSON object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		public static void CopyFrom(this object @object, JToken json, HashSet<string> excluded = null)
		{
			if (object.ReferenceEquals(json, null))
				throw new ArgumentNullException(nameof(json), "The JSON is null");

			var serializer = new JsonSerializer();
			foreach (var attribute in @object.GetProperties())
			{
				// check excluded
				if (!attribute.CanWrite || (excluded != null && excluded.Contains(attribute.Name)))
					continue;

				// check token
				var token = json[attribute.Name];
				if (token == null)
					continue;

				// array
				if (attribute.Type.IsArray)
				{
					var type = attribute.Type.GetElementType().MakeArrayType();
					var instance = serializer.Deserialize(new JTokenReader(token), type);
					@object.SetAttributeValue(attribute, instance);
				}

				// generic list/hash-set
				else if (attribute.Type.IsGenericListOrHashSet())
					try
					{
						// prepare
						JArray data = null;
						if (attribute.Type.GenericTypeArguments[0].IsClassType() && attribute.GetAsObjectAttribute() != null && token is JObject)
						{
							data = new JArray();
							if ((token as JObject).Count > 0)
							{
								var gotSpecialAttributes = attribute.Type.GenericTypeArguments[0].GetSpecialSerializeAttributes().Count > 0;
								foreach (var item in token as JObject)
									if (gotSpecialAttributes)
									{
										object child = attribute.Type.GenericTypeArguments[0].CreateInstance();
										child.CopyFrom(item.Value);
										data.Add(JObject.FromObject(child));
									}
									else
										data.Add(item.Value);
							}
						}
						else
							data = token as JArray;

						// update
						var type = attribute.Type.IsGenericList()
							? (typeof(List<>)).MakeGenericType(attribute.Type.GenericTypeArguments[0])
							: (typeof(HashSet<>)).MakeGenericType(attribute.Type.GenericTypeArguments[0]);

						var instance = data != null && data.Count > 0
							? serializer.Deserialize(new JTokenReader(data), type)
							: type.CreateInstance();

						@object.SetAttributeValue(attribute, instance);
					}
					catch { }

				// generic dictionary/collection
				else if (attribute.Type.IsGenericDictionaryOrCollection())
					try
					{
						// prepare
						JObject data = null;
						if (attribute.Type.GenericTypeArguments[1].IsClassType() && attribute.GetAsArrayAttribute() != null && token is JArray)
						{
							data = new JObject();
							if ((token as JArray).Count > 0)
							{
								var asArray = attribute.GetAsArrayAttribute();
								var keyAttribute = !string.IsNullOrWhiteSpace(asArray.KeyAttribute)
									? asArray.KeyAttribute
									: "ID";
								var gotSpecialAttributes = attribute.Type.GenericTypeArguments[1].GetSpecialSerializeAttributes().Count > 0;
								foreach (JObject item in token as JArray)
									if (gotSpecialAttributes)
									{
										object child = attribute.Type.GenericTypeArguments[1].CreateInstance();
										child.CopyFrom(item);
										var keyValue = child.GetAttributeValue(keyAttribute);
										if (keyValue != null)
											data.Add(new JProperty(keyValue.ToString(), JObject.FromObject(child)));
									}
									else
									{
										var keyValue = item[keyAttribute];
										if (keyValue != null && keyValue is JValue && (keyValue as JValue).Value != null)
											data.Add(new JProperty((keyValue as JValue).Value.ToString(), item));
									}
							}
						}
						else
							data = token as JObject;

						// update
						var type = attribute.Type.IsGenericDictionary()
							? (typeof(Dictionary<,>)).MakeGenericType(attribute.Type.GenericTypeArguments[0], attribute.Type.GenericTypeArguments[1])
							: (typeof(Collection<,>)).MakeGenericType(attribute.Type.GenericTypeArguments[0], attribute.Type.GenericTypeArguments[1]);

						var instance = data != null && data.Count > 0
							? serializer.Deserialize(new JTokenReader(data), type)
							: type.CreateInstance();

						@object.SetAttributeValue(attribute, instance);
					}
					catch { }

				// collection
				else if (attribute.Type.IsCollection())
					try
					{
						var instance = token is JObject && (token as JObject).Count > 0
							? serializer.Deserialize(new JTokenReader(token), typeof(Collection))
							: typeof(Collection).CreateInstance();
						@object.SetAttributeValue(attribute, instance);
					}
					catch { }

				// enum
				else if (attribute.Type.IsEnum && token is JValue && (token as JValue).Value != null)
					try
					{
						@object.SetAttributeValue(attribute, (token as JValue).Value.ToString().ToEnum(attribute.Type));
					}
					catch { }

				// class
				else if (attribute.Type.IsClassType())
					try
					{
						var instance = attribute.Type.CreateInstance();
						if (token is JObject && (token as JObject).Count > 0)
							instance.CopyFrom(token);
						@object.SetAttributeValue(attribute, instance);
					}
					catch { }

				// primitive or unknown
				else
					try
					{
						@object.SetAttributeValue(attribute, token is JValue ? (token as JValue).Value : null, true);
					}
					catch { }
			}
		}

		/// <summary>
		/// Copies data of the ExpandoObject object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="expandoObject">The <see cref="ExpandoObject">ExpandoObject</see> object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		public static void CopyFrom(this object @object, ExpandoObject expandoObject, HashSet<string> excluded = null)
		{
			foreach (var attribute in @object.GetProperties())
			{
				// check excluded
				if (!attribute.CanWrite || (excluded != null && excluded.Contains(attribute.Name)))
					continue;

				// get and check the value
				object value = null;
				if (!expandoObject.TryGet(attribute.Name, out value))
					continue;

				// normalize the value
				if (value != null)
				{
					// value is generic list/hash-set
					if (value is List<object> && attribute.Type.IsGenericListOrHashSet())
						value = attribute.Type.IsGenericList()
							? (value as List<object>).ToList(attribute.Type.GenericTypeArguments[0])
							: (value as List<object>).ToHashSet(attribute.Type.GenericTypeArguments[0]);

					// value is generic dictionary/collection or object
					else if (value is ExpandoObject)
					{
						if (attribute.Type.IsGenericDictionaryOrCollection())
							value = attribute.Type.IsGenericDictionary()
								? (value as ExpandoObject).ToDictionary(attribute.Type.GenericTypeArguments[0], attribute.Type.GenericTypeArguments[1])
								: (value as ExpandoObject).ToCollection(attribute.Type.GenericTypeArguments[0], attribute.Type.GenericTypeArguments[1]);

						else if (attribute.Type.IsClassType() && !attribute.Type.Equals(typeof(ExpandoObject)))
						{
							var obj = attribute.Type.CreateInstance();
							obj.CopyFrom(value as ExpandoObject);
							value = obj;
						}
					}

					// value is primitive type
					else if (attribute.Type.IsPrimitiveType())
						value = value.CastAs(attribute.Type);
				}

				// update the value of attribute
				try
				{
					@object.SetAttributeValue(attribute, value);
				}
				catch { }
			}
		}

		/// <summary>
		/// Creates new an instance of the object and copies data (from this current object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <returns></returns>
		public static T Copy<T>(this T @object, HashSet<string> excluded = null)
		{
			var instance = ObjectService.CreateInstance<T>();
			instance.CopyFrom(@object, excluded);
			return instance;
		}

		/// <summary>
		/// Creates new an instance of the object and copies data (from a JSON object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <returns></returns>
		public static T Copy<T>(this JToken json, HashSet<string> excluded = null)
		{
			var instance = ObjectService.CreateInstance<T>();
			instance.CopyFrom(json, excluded);
			return instance;
		}

		/// <summary>
		/// Creates new an instance of the object and copies data (from an ExpandoObject object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expandoObject">The ExpandoObject object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <returns></returns>
		public static T Copy<T>(this ExpandoObject expandoObject, HashSet<string> excluded = null)
		{
			var instance = ObjectService.CreateInstance<T>();
			instance.CopyFrom(expandoObject, excluded);
			return instance;
		}

		/// <summary>
		/// Clones the object (perform a deep copy of the object)
		/// </summary>
		/// <typeparam name="T">The type of object being copied</typeparam>
		/// <param name="object">The object instance to copy</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(this T @object)
		{
			// initialize the object
			T instance;

			// the object is serializable
			if (@object.GetType().IsSerializable)
				using (var stream = new MemoryStream())
				{
					var formatter = new BinaryFormatter();
					formatter.Serialize(stream, @object);
					stream.Seek(0, SeekOrigin.Begin);
					instance = (T)formatter.Deserialize(stream);
				}

			// cannot serialize, then copy data
			else
			{
				instance = ObjectService.CreateInstance<T>();
				@object.CopyTo(instance);
			}

			// return the new instance of object
			return instance;
		}
		#endregion

		#region JSON Conversions
		internal static List<AttributeInfo> GetSpecialSerializeAttributes(this Type type)
		{
			return ObjectService.GetProperties(type)
				.Where(attribute => (attribute.Type.IsGenericDictionaryOrCollection() && attribute.GetAsArrayAttribute() != null) || (attribute.Type.IsGenericListOrHashSet() && attribute.GetAsObjectAttribute() != null))
				.ToList();
		}

		internal static AsArrayAttribute GetAsArrayAttribute(this AttributeInfo attribute)
		{
			var attributes = attribute.Info.GetCustomAttributes(typeof(AsArrayAttribute), true);
			return attributes.Length > 0
				? attributes[0] as AsArrayAttribute
				: null;
		}

		internal static AsObjectAttribute GetAsObjectAttribute(this AttributeInfo attribute)
		{
			var attributes = attribute.Info.GetCustomAttributes(typeof(AsObjectAttribute), true);
			return attributes.Length > 0
				? attributes[0] as AsObjectAttribute
				: null;
		}

		/// <summary>
		/// Serializes this object to JSON object (with default settings of Json.NET Serializer)
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="object">The object to serialize to JSON</param>
		/// <returns></returns>
		public static JToken ToJson<T>(this T @object)
		{
			// by-pass on JObject or JArray
			if (@object is JObject || @object is JArray)
				return @object as JToken;

			// generate
			JToken json = null;

			// array or generict list/hash-set
			if (@object.IsArray() || @object.IsGenericListOrHashSet())
			{
				var type = @object.IsArray()
					? @object.GetType().GetElementType()
					: @object.GetType().GenericTypeArguments[0];

				if (type.IsClassType())
				{
					json = new JArray();
					foreach (var item in @object as IEnumerable)
						(json as JArray).Add(item?.ToJson());
				}
				else
					json = JArray.FromObject(@object);
			}

			// generict dictionary/collection
			else if (@object.IsGenericDictionaryOrCollection())
			{
				if (@object.GetType().GenericTypeArguments[1].IsClassType())
				{
					json = new JObject();
					var enumerator = (@object as IDictionary).GetEnumerator();
					while (enumerator.MoveNext())
						(json as JObject).Add(new JProperty(enumerator.Key.ToString(), enumerator.Value?.ToJson()));
				}
				else
					json = JObject.FromObject(@object);
			}

			// object
			else if (@object.GetType().IsClassType())
			{
				json = JObject.FromObject(@object);
				@object.GetType().GetSpecialSerializeAttributes().ForEach(attribute =>
				{
					if (attribute.Type.IsGenericListOrHashSet() && attribute.Type.GenericTypeArguments[0].IsClassType() && attribute.GetAsObjectAttribute() != null)
					{
						var jsonObject = new JObject();

						var items = @object.GetAttributeValue(attribute.Name) as IEnumerable;
						if (items != null)
						{
							var asObject = attribute.GetAsObjectAttribute();
							var keyAttribute = !string.IsNullOrWhiteSpace(asObject.KeyAttribute)
								? asObject.KeyAttribute
								: "ID";

							foreach (var item in items)
								if (item != null)
								{
									var key = item.GetAttributeValue(keyAttribute);
									if (key != null)
										jsonObject.Add(new JProperty(key.ToString(), item?.ToJson()));
								}
						}

						json[attribute.Name] = jsonObject;
					}
					else if (attribute.Type.IsGenericDictionaryOrCollection() && attribute.Type.GenericTypeArguments[1].IsClassType() && attribute.GetAsArrayAttribute() != null)
					{
						var jsonArray = new JArray();

						var items = @object.GetAttributeValue(attribute.Name) as IEnumerable;
						if (items != null)
							foreach (var item in items)
								jsonArray.Add(item?.ToJson());

						json[attribute.Name] = jsonArray;
					}
				});
			}

			// primitive or unknown
			else
				json = new JValue(@object);

			// return the JSON
			return json;
		}

		/// <summary>
		/// Serializes this object to JSON object (with default settings of Json.NET Serializer)
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="object">The object to serialize to JSON</param>
		/// <param name="onPreCompleted">The action to run on pre-completed</param>
		/// <returns></returns>
		public static JToken ToJson<T>(this T @object, Action<JToken> onPreCompleted)
		{
			// serialize
			var json = @object.ToJson();

			// run the handler
			onPreCompleted?.Invoke(json);

			// return the JSON
			return json;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON object that contains information for deserializing</param>
		/// <param name="copy">true to create new instance and copy data; false to deserialize object</param>
		/// <param name="onPreCompleted">The action to run on pre-completed</param>
		/// <returns></returns>
		public static T FromJson<T>(this JToken json, bool copy = false, Action<T, JToken> onPreCompleted = null)
		{
			// initialize the object
			T @object;

			// got special, then create new instance and copy data from JSON
			if (copy || typeof(T).GetSpecialSerializeAttributes().Count > 0)
			{
				@object = ObjectService.CreateInstance<T>();
				@object.CopyFrom(json);
			}

			// deserialize the object from JSON
			else
				@object = (new JsonSerializer()).Deserialize<T>(new JTokenReader(json));

			// run the handler
			onPreCompleted?.Invoke(@object, json);

			// return object
			return @object;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON string that contains information for deserializing</param>
		/// <param name="copy">true to create new instance and copy data; false to deserialize object</param>
		/// <param name="onPreCompleted">The action to run on pre-completed</param>
		/// <returns></returns>
		public static T FromJson<T>(this string json, bool copy = false, Action<T, JToken> onPreCompleted = null)
		{
			// deserialize
			var token = json.Trim().StartsWith("[")
				? JArray.Parse(json) as JToken
				: JObject.Parse(json) as JToken;
			var @object = token.FromJson<T>(copy);

			// run the handler
			onPreCompleted?.Invoke(@object, token);

			// return object
			return @object;
		}
		#endregion

		#region XML Conversions
		/// <summary>
		/// Serializes this object to XML object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="object"></param>
		/// <param name="onPreCompleted">The action to run on pre-completed</param>
		/// <returns></returns>
		public static XElement ToXml<T>(this T @object, Action<XElement> onPreCompleted = null)
		{
			// serialize
			XElement xml = null;
			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					(new XmlSerializer(typeof(T))).Serialize(writer, @object);
					xml = XElement.Parse(stream.ToArray().GetString());
				}
			}

			// run the handler
			onPreCompleted?.Invoke(xml);

			// return the XML
			return xml;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this XML object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="xml">The XML object that contains information for deserializing</param>
		/// <param name="onPreCompleted">The action to run on pre-completed</param>
		/// <returns></returns>
		public static T FromXml<T>(this XContainer xml, Action<T, XContainer> onPreCompleted = null)
		{
			// deserialize
			var @object = (T)(new XmlSerializer(typeof(T))).Deserialize(xml.CreateReader());

			// run the handler
			onPreCompleted?.Invoke(@object, xml);

			// return the object
			return @object;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this XML string
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="xml">The XML string that contains information for deserializing</param>
		/// <param name="onPreCompleted">The action to run on pre-completed</param>
		/// <returns></returns>
		public static T FromXml<T>(this string xml, Action<T> onPreCompleted = null)
		{
			// deserialize
			T @object;
			using (var stringReader = new StringReader(xml))
			{
				using (var xmlReader = new XmlTextReader(stringReader))
				{
					@object = (T)(new XmlSerializer(typeof(T))).Deserialize(xmlReader);
				}
			}

			// run the handler
			onPreCompleted?.Invoke(@object);

			// return the object
			return @object;
		}

		/// <summary>
		/// Converts this XmlNode object to XElement object
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static XElement ToXElement(this XmlNode node)
		{
			var doc = new XDocument();
			using (var writer = doc.CreateWriter())
			{
				node.WriteTo(writer);
				return doc.Root;
			}
		}

		/// <summary>
		/// Converts this XElement object to XmlNode object
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static XmlNode ToXmlNode(XElement element)
		{
			using (var reader = element.CreateReader())
			{
				var doc = new XmlDocument();
				return doc.ReadNode(reader);
			}
		}

		/// <summary>
		/// Converts this XElement object to XmlDocument object
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static XmlDocument ToXmlDocument(this XElement element)
		{
			using (var reader = element.CreateReader())
			{
				var doc = new XmlDocument();
				doc.Load(reader);
				doc.DocumentElement.Attributes.RemoveAll();
				return doc;
			}
		}

		/// <summary>
		/// Converts this XmlDocument object to XDocument object
		/// </summary>
		/// <param name="document"></param>
		/// <returns></returns>
		public static XDocument ToXDocument(this XmlDocument document)
		{
			var doc = new XDocument();
			using (var writer = doc.CreateWriter())
			{
				document.WriteTo(writer);
			}
			var declaration = document.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
			if (declaration != null)
				doc.Declaration = new XDeclaration(declaration.Version, declaration.Encoding, declaration.Standalone);
			return doc;
		}

		/// <summary>
		/// Converts this XDocument object to XmlDocument object
		/// </summary>
		/// <param name="document"></param>
		/// <returns></returns>
		public static XmlDocument ToXmlDocument(this XDocument document)
		{
			using (var reader = document.CreateReader())
			{
				var doc = new XmlDocument();
				doc.Load(reader);
				if (document.Declaration != null)
					doc.InsertBefore(doc.CreateXmlDeclaration(document.Declaration.Version, document.Declaration.Encoding, document.Declaration.Standalone), doc.FirstChild);
				return doc;
			}
		}

		/// <summary>
		/// Transforms this XmlDocument object by the specified stylesheet to XHTML/XML string
		/// </summary>
		/// <param name="document"></param>
		/// <param name="xslTransfrom">The stylesheet for transfroming</param>
		/// <returns></returns>
		public static string Transfrom(this XmlDocument document, XslCompiledTransform xslTransfrom)
		{
			// transform
			string results = "";
			if (xslTransfrom == null)
				return results;

			using (var writer = new StringWriter())
			{
				try
				{
					xslTransfrom.Transform(document, null, writer);
					results = writer.ToString();
				}
				catch (Exception)
				{
					throw;
				}
			}

			// refine characters
			results = results.Replace(StringComparison.OrdinalIgnoreCase, "&#xD;", "").Replace(StringComparison.OrdinalIgnoreCase, "&#xA;", "").Replace(StringComparison.OrdinalIgnoreCase, "\t", "").Replace(StringComparison.OrdinalIgnoreCase, "&#x9;", "").Replace(StringComparison.OrdinalIgnoreCase, "<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");

			// remove "xmlns:" attributes
			var start = results.PositionOf("xmlns:");
			while (start > 0)
			{
				var end = results.PositionOf("\"", start);
				end = results.PositionOf("\"", end + 1);
				results = results.Remove(start, end - start + 1);
				start = results.PositionOf("xmlns:");
			}

			// return the result
			return results;
		}

		/// <summary>
		/// Transforms this XmlDocument object by the specified stylesheet to XHTML/XML string
		/// </summary>
		/// <param name="document"></param>
		/// <param name="xmlStylesheet">The stylesheet for transfroming</param>
		/// <param name="enableScript">true to enable inline script (like C#) while processing</param>
		/// <returns></returns>
		public static string Transfrom(this XmlDocument document, string xmlStylesheet, bool enableScript = true)
		{
			using (var stream = new MemoryStream(xmlStylesheet.ToBytes()))
			{
				using (var reader = new XmlTextReader(stream))
				{
					try
					{
						var xslTransform = new XslCompiledTransform();
						xslTransform.Load(reader, enableScript ? new XsltSettings() { EnableScript = true } : null, null);
						return document.Transfrom(xslTransform);
					}
					catch (Exception)
					{
						throw;
					}
				}
			}
		}
		#endregion

		#region ExpandoObject Conversions & Manipulations
		/// <summary>
		/// Creates (Deserializes) an <see cref="ExpandoObject">ExpandoObject</see> object from this JSON string
		/// </summary>
		/// <param name="json">The string that presents serialized data to create object</param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject(this string json)
		{
			return JsonConvert.DeserializeObject<ExpandoObject>(json);
		}

		/// <summary>
		/// Creates (Deserializes) an <see cref="ExpandoObject">ExpandoObject</see> object from this JSON
		/// </summary>
		/// <param name="json">The string that presents serialized data to create object</param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject(this JToken json)
		{
			return (new JsonSerializer()).Deserialize<ExpandoObject>(new JTokenReader(json));
		}

		/// <summary>
		/// Creates an <see cref="ExpandoObject">ExpandoObject</see> object from this dictionary object
		/// </summary>
		/// <param name="object"></param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject(this IDictionary<string, object> @object)
		{
			var expando = new ExpandoObject();
			@object.ForEach(entry =>
			{
				(expando as IDictionary<string, object>)[entry.Key] = entry.Value is IDictionary<string, object>
					? (entry.Value as IDictionary<string, object>).ToExpandoObject()
					: entry.Value;
			});
			return expando;
		}

		/// <summary>
		/// Creates an <see cref="ExpandoObject">ExpandoObject</see> object from this object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject<T>(this T @object) where T : class
		{
			return (@object is JToken ? @object as JToken : @object.ToJson<T>()).ToExpandoObject();
		}

		/// <summary>
		/// Creates (Deserializes) an object from this <see cref="ExpandoObject">ExpandoObject</see> object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T FromExpandoObject<T>(this ExpandoObject @object) where T : class
		{
			return JObject.FromObject(@object).FromJson<T>();
		}

		/// <summary>
		/// Tries to get value of an attribute of the <see cref="ExpandoObject">ExpandoObject</see> object by specified name (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <param name="value">The value (if got); otherwise null.</param>
		/// <returns>true if the object has an attribute; otherwise false.</returns>
		public static bool TryGet(this ExpandoObject @object, string name, out object value)
		{
			// assign & check
			value = null;
			if (string.IsNullOrWhiteSpace(name))
				return false;

			// prepare
			var dictionary = @object as IDictionary<string, object>;
			var names = name.IndexOf(".") > 0
				? name.ToArray('.', true, true)
				: new[] { name };

			// no multiple
			if (names.Length < 2)
			{
				if (!dictionary.ContainsKey(name))
					return false;

				value = dictionary[name];
				return true;
			}

			// got multiple
			var index = 0;
			while (index < names.Length - 1 && dictionary != null)
			{
				dictionary = dictionary.ContainsKey(names[index])
					? dictionary[names[index]] as IDictionary<string, object>
					: null;
				index++;
			}

			if (dictionary == null || !dictionary.ContainsKey(names[names.Length - 1]))
				return false;

			value = dictionary[names[names.Length - 1]];
			return true;
		}

		/// <summary>
		/// Tries to get value of an attribute of the <see cref="ExpandoObject">ExpandoObject</see> object by specified name (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <param name="value">The value (if got); otherwise default of T.</param>
		/// <returns>true if the object has an attribute; otherwise false.</returns>
		public static bool TryGet<T>(this ExpandoObject @object, string name, out T value)
		{
			// assign default
			value = default(T);

			// get value & normalize
			object theValue = null;
			if (@object.TryGet(name, out theValue))
			{
				// get type
				var type = typeof(T);

				// generic list/hash-set
				if (theValue is List<object> && type.IsGenericListOrHashSet())
					theValue = type.IsGenericList()
						? (theValue as List<object>).ToList<T>()
						: (theValue as List<object>).ToHashSet<T>();

				// generic dictionary/collection or object
				else if (theValue is ExpandoObject)
				{
					if (type.IsGenericDictionaryOrCollection())
						theValue = type.IsGenericDictionary()
							? (theValue as ExpandoObject).ToDictionary<T>()
							: (theValue as ExpandoObject).ToCollection<T>();

					else if (type.IsClassType() && !type.Equals(typeof(ExpandoObject)))
					{
						var obj = type.CreateInstance();
						obj.CopyFrom(theValue as ExpandoObject);
						theValue = obj;
					}
				}

				// other (primitive or other)
				else
					theValue = theValue.CastAs(type);

				// cast the value & return state
				value = (T)theValue;
				return true;
			}

			// return the default state
			return false;
		}

		/// <summary>
		/// Gets the value of an attribute of this <see cref="ExpandoObject">ExpandoObject</see> object (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <returns>The value of an attribute (if the object got it); otherwise null.</returns>
		public static object Get(this ExpandoObject @object, string name)
		{
			return @object.TryGet(name, out object value)
				? value
				: null;
		}

		/// <summary>
		/// Gets the value of an attribute of this <see cref="ExpandoObject">ExpandoObject</see> object (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <param name="defaultValue">Default value when the attribute is not found</param>
		/// <returns>The value of an attribute (if the object got it); otherwise null.</returns>
		public static T Get<T>(this ExpandoObject @object, string name, T defaultValue = default(T))
		{
			return @object.TryGet<T>(name, out T value)
				? value
				: defaultValue;
		}

		/// <summary>
		/// Checks to see the <see cref="ExpandoObject">ExpandoObject</see> object is got an attribute by specified name (accept the dot (.) to get check of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute for checking, accept the dot (.) to get check of child object</param>
		/// <returns>true if the object got an attribute with the name</returns>
		public static bool Has(this ExpandoObject @object, string name)
		{
			return @object.TryGet(name, out object value);
		}

		/// <summary>
		/// Sets the value of an attribute of the <see cref="ExpandoObject">ExpandoObject</see> object by specified name (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <param name="value">The value to set</param>
		/// <returns>true if the attribute has been setted; otherwise false.</returns>
		public static bool Set(this ExpandoObject @object, string name, object value)
		{
			// check
			if (string.IsNullOrWhiteSpace(name))
				return false;

			// prepare
			var dictionary = @object as IDictionary<string, object>;
			var names = name.IndexOf(".") > 0
				? name.ToArray('.', true, true)
				: new[] { name };

			// no multiple
			if (names.Length < 2)
			{
				if (!dictionary.ContainsKey(name))
					dictionary.Add(name, value);
				else
					dictionary[name] = value;
				return true;
			}

			// got multiple
			var index = 0;
			while (index < names.Length - 1 && dictionary != null)
			{
				dictionary = dictionary.ContainsKey(names[index])
					? dictionary[names[index]] as IDictionary<string, object>
					: null;
				index++;
			}

			if (dictionary == null)
				return false;

			if (!dictionary.ContainsKey(names[names.Length - 1]))
				dictionary.Add(names[names.Length - 1], value);
			else
				dictionary[names[names.Length - 1]] = value;
			return true;
		}

		/// <summary>
		/// Removes an attribute of the <see cref="ExpandoObject">ExpandoObject</see> object by specified name (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <returns>true if the attribute has been removed from the object; otherwise false.</returns>
		public static bool Remove(this ExpandoObject @object, string name)
		{
			// check
			if (string.IsNullOrWhiteSpace(name))
				return false;

			// prepare
			var dictionary = @object as IDictionary<string, object>;
			var names = name.IndexOf(".") > 0
				? name.ToArray('.', true, true)
				: new[] { name };

			// no multiple
			if (names.Length < 2)
			{
				if (!dictionary.ContainsKey(name))
					return false;

				dictionary.Remove(name);
				return true;
			}

			// got multiple
			var index = 0;
			while (index < names.Length - 1 && dictionary != null)
			{
				dictionary = dictionary.ContainsKey(names[index])
					? dictionary[names[index]] as IDictionary<string, object>
					: null;
				index++;
			}

			if (dictionary == null || !dictionary.ContainsKey(names[names.Length - 1]))
				return false;

			dictionary.Remove(names[names.Length - 1]);
			return true;
		}
		#endregion

	}

	#region Attributes of object serialization
	/// <summary>
	/// Specifies this property is serialized as an object (JObject) instead as an array (JArray) while serializing/deserializing via Json.NET
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	[DebuggerDisplay("Key = {KeyAttribute}")]
	public class AsObjectAttribute : Attribute
	{
		public AsObjectAttribute() { }

		/// <summary>
		/// Gets or sets the name of attribute to use as the key (if not value is provided, the name 'ID' will be used while processing)
		/// </summary>
		public string KeyAttribute { get; set; }
	}

	/// <summary>
	/// Specifies this property is serialized as an array (JArray) instead as an object (JObject) while serializing/deserializing via Json.NET
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	[DebuggerDisplay("Key = {KeyAttribute}")]
	public class AsArrayAttribute : Attribute
	{
		public AsArrayAttribute() { }

		/// <summary>
		/// Gets or sets the name of attribute to use as the key (if not value is provided, the name 'ID' will be used while processing)
		/// </summary>
		public string KeyAttribute { get; set; }
	}
	#endregion

}