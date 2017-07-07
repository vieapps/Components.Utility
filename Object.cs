#region Related components
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
		[Serializable]
		[DebuggerDisplay("Name = {Name}")]
		public class AttributeInfo
		{
			public AttributeInfo() { }

			public string Name { get; internal set; }
			public MemberInfo Info { get; internal set; }
			internal bool NotNull { get; set; }
			internal string Column { get; set; }
			internal int MaxLength { get; set; }
			internal bool IsCLOB { get; set; }
			internal bool IsDateTimeString { get; set; }
			internal bool IsCustom { get; set; }

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
		/// <summary>
		/// Gets collection of public properties of the object's type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns>Collection of public properties</returns>
		public static List<AttributeInfo> GetProperties(Type type)
		{
			var defaultMembers = type.GetCustomAttributes(typeof(DefaultMemberAttribute));
			var defaultMember = defaultMembers != null
				? defaultMembers.FirstOrDefault() as DefaultMemberAttribute
				: null;

			var properties = new List<AttributeInfo>();
			type.GetProperties(BindingFlags.Instance | BindingFlags.Public).ForEach(info =>
			{
				if (defaultMember == null || !defaultMember.MemberName.Equals(info.Name))
					properties.Add(new AttributeInfo()
					{
						Name = info.Name,
						Info = info,
						NotNull = false,
						Column = null,
						MaxLength = 0,
						IsCLOB = false,
						IsCustom = false
					});
			});
			return properties;
		}

		/// <summary>
		/// Gets collection of public properties of the object
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <returns>Collection of public properties</returns>
		public static List<AttributeInfo> GetProperties(this object @object)
		{
			return ObjectService.GetProperties(@object.GetType());
		}

		/// <summary>
		/// Gets collection of private fields/attributes of the object's type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<AttributeInfo> GetFields(Type type)
		{
			var attributes = new List<AttributeInfo>();
			type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).ForEach(info =>
			{
				if (!info.Name.StartsWith("<"))
					attributes.Add(new AttributeInfo()
					{
						Name = info.Name,
						Info = info,
						NotNull = false,
						Column = null,
						MaxLength = 0,
						IsCLOB = false,
						IsCustom = false
					});
			});
			return attributes;
		}

		/// <summary>
		/// Gets collection of private fields/attributes of the object's type
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<AttributeInfo> GetFields(this object @object)
		{
			return ObjectService.GetFields(@object.GetType());
		}

		/// <summary>
		/// Gets collection of attributes of the object's type (means contains all public properties and private fields)
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
		/// Gets collection of attributes of the object's type (means contains all public properties and private fields)
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <returns>Collection of attributes</returns>
		public static List<AttributeInfo> GetAttributes(this object @object)
		{
			return ObjectService.GetAttributes(@object.GetType());
		}

		/// <summary>
		/// Get full type name (type name with assembly name) of this type
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
		/// Gets state to determines the type is primitive
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is primitive</returns>
		public static bool IsPrimitiveType(this Type type)
		{
			return type.IsPrimitive
				? type.IsPrimitive
				: type.IsStringType() || type.IsDateTimeType() || type.IsNumericType();
		}

		/// <summary>
		/// Gets state to determines the type is string
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is string</returns>
		public static bool IsStringType(this Type type)
		{
			return type.Equals(typeof(System.String));
		}

		/// <summary>
		/// Gets state to determines the type is date-time
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is date-time</returns>
		public static bool IsDateTimeType(this Type type)
		{
			return type.Equals(typeof(System.DateTime));
		}

		/// <summary>
		/// Gets state to determines the type is integral numeric
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is integral numeric</returns>
		public static bool IsIntegralType(this Type type)
		{
			return type.Equals(typeof(System.Int16)) || type.Equals(typeof(System.Int32)) || type.Equals(typeof(System.Int64))
							|| type.Equals(typeof(System.UInt16)) || type.Equals(typeof(System.UInt32)) || type.Equals(typeof(System.UInt64))
							|| type.Equals(typeof(System.Byte)) || type.Equals(typeof(System.SByte));
		}

		/// <summary>
		/// Gets state to determines the type is floating numeric
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is floating numeric</returns>
		public static bool IsFloatingPointType(this Type type)
		{
			return type.Equals(typeof(System.Decimal)) || type.Equals(typeof(System.Double)) || type.Equals(typeof(System.Single));
		}

		/// <summary>
		/// Gets state to determines the type is numeric
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsNumericType(this Type type)
		{
			return type.IsIntegralType() || type.IsFloatingPointType();
		}

		/// <summary>
		/// Gets state to determines the type is a reference of a class
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
		/// Gets state to determines this type is sub-class of a generic type
		/// </summary>
		/// <param name="type">The type for checking</param>
		/// <param name="genericType">The generic type for checking</param>
		/// <returns>true if the checking type is sub-class of the generic type</returns>
		public static bool IsSubclassOfGeneric(this Type type, Type genericType)
		{
			if (!genericType.IsGenericType)
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
		/// Gets state to determines the type is reference of a generic list
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericList(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(List<>));
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference of a generic list
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic list; otherwise false.</returns>
		public static bool IsGenericList(this object @object)
		{
			return @object.GetType().IsGenericList();
		}

		/// <summary>
		/// Gets state to determines the type is reference of a generic hash-set
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic hash-set; otherwise false.</returns>
		public static bool IsGenericHashSet(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(HashSet<>));
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference of a generic hash-set
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic hash-set; otherwise false.</returns>
		public static bool IsGenericHashSet(this object @object)
		{
			return @object.GetType().IsGenericHashSet();
		}

		/// <summary>
		/// Gets state to determines the type is reference of a generic list or generic hash-set
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic list or generic hash-set; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this Type type)
		{
			return type.IsGenericList() || type.IsGenericHashSet();
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference of a generic list or generic hash-set
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic list or generic hash-set; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this object @object)
		{
			return @object.GetType().IsGenericListOrHashSet();
		}

		/// <summary>
		/// Gets state to determines the type is reference of a generic dictionary
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic dictionary; otherwise false.</returns>
		public static bool IsGenericDictionary(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(Dictionary<,>));
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference of a generic dictionary
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic dictionary; otherwise false.</returns>
		public static bool IsGenericDictionary(this object @object)
		{
			return @object.GetType().IsGenericDictionary();
		}

		/// <summary>
		/// Gets state to determines the type is reference of a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericCollection(this Type type)
		{
			return type.IsGenericType && type.IsSubclassOfGeneric(typeof(Collection<,>));
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference of a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericCollection(this object @object)
		{
			return @object.GetType().IsGenericCollection();
		}

		/// <summary>
		/// Gets state to determines the type is reference of a generic dictionary or a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this Type type)
		{
			return type.IsGenericDictionary() || type.IsGenericCollection();
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference of a generic dictionary or a generic collection (the <see cref="Collection">Collection</see> class)
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is sub-class of the generic <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this object @object)
		{
			return @object.GetType().IsGenericDictionaryOrCollection();
		}

		/// <summary>
		/// Gets state to determines the type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface; otherwise false.</returns>
		public static bool IsICollection(this Type type)
		{
			return typeof(ICollection).IsAssignableFrom(type) || type.IsGenericHashSet();
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface; otherwise false.</returns>
		public static bool IsICollection(this object @object)
		{
			return @object.GetType().IsICollection();
		}

		/// <summary>
		/// Gets state to determines the type is reference (or sub-class) of the the <see cref="Collection">Collection</see> class
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is is reference (or sub-class) of the the <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsCollection(this Type type)
		{
			return typeof(Collection).IsAssignableFrom(type);
		}

		/// <summary>
		/// Gets state to determines the type of the object is reference (or sub-class) of the the <see cref="Collection">Collection</see> class
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is is reference (or sub-class) of the the <see cref="Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsCollection(this object @object)
		{
			return @object.GetType().IsCollection();
		}
		#endregion

		#region Fast create new instance & cast type
		static Dictionary<Type, Func<object>> TypeFactoryCache = new Dictionary<Type, Func<object>>();

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection
		/// </summary>
		/// <param name="type">The type to be created</param>
		/// <returns>The newly created instance</returns>
		public static object CreateInstance(this Type type)
		{
			Func<object> func;

			if (!ObjectService.TypeFactoryCache.TryGetValue(type, out func))
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

		static Dictionary<Type, Func<object, object>> CastFactoryCache = new Dictionary<Type, Func<object, object>>()
		{
			{ typeof(Byte), value => Convert.ToByte(value) },
			{ typeof(SByte), value => Convert.ToSByte(value) },
			{ typeof(Int16), value => Convert.ToInt16(value) },
			{ typeof(UInt16), value => Convert.ToUInt16(value) },
			{ typeof(Int32), value => Convert.ToInt32(value) },
			{ typeof(UInt32), value => Convert.ToUInt32(value) },
			{ typeof(Int64), value => Convert.ToInt64(value) },
			{ typeof(UInt64), value => Convert.ToUInt64(value) },
			{ typeof(Single), value => Convert.ToSingle(value) },
			{ typeof(Double), value => Convert.ToDouble(value) },
			{ typeof(Decimal), value => Convert.ToDecimal(value) },
			{ typeof(Boolean), value => Convert.ToBoolean(value) },
			{ typeof(Char), value => Convert.ToChar(value) },
			{ typeof(String), value => Convert.ToString(value) },
			{ typeof(DateTime), value => Convert.ToDateTime(value) }
		};

		/// <summary>
		/// Casts the type of a primitive object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="type">The type to cast to</param>
		/// <returns></returns>
		public static object CastType(this object @object, Type type)
		{
			return ObjectService.CastFactoryCache.ContainsKey(type)
				? ObjectService.CastFactoryCache[type](@object)
				: @object;
		}

		/// <summary>
		/// Casts the type of a primitive object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T CastType<T>(this object @object)
		{
			return (T)@object.CastType(typeof(T));
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
				throw new ArgumentException("name");

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
				throw new ArgumentException("attribute");
			@object.SetAttributeValue(attribute.Name, !cast ? value : value != null ? value.CastType(attribute.Type) : Convert.ChangeType(value, attribute.Type));
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
				throw new ArgumentException("name");

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
				throw new ArgumentException("attribute");

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
				throw new ArgumentNullException("destination", "The destination is null");

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
				throw new ArgumentNullException("source", "The source is null");

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
				throw new ArgumentNullException("json", "The JSON is null");

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
						if (attribute.Type.GenericTypeArguments[0].IsClassType() && attribute.GetSerializeAsObjectAttribute() != null && token is JObject)
						{
							data = new JArray();
							if ((token as JObject).Count > 0)
							{
								var gotSpecialAttributes = attribute.Type.GenericTypeArguments[0].GetSpecialSerializeAttributes().Count > 0;
								foreach (var item in token as JObject)
									if (gotSpecialAttributes)
									{
										object child = Activator.CreateInstance(attribute.Type.GenericTypeArguments[0]);
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
							: Activator.CreateInstance(type);

						@object.SetAttributeValue(attribute, instance);
					}
					catch { }

				// generic dictionary/collection
				else if (attribute.Type.IsGenericDictionaryOrCollection())
					try
					{
						// prepare
						JObject data = null;
						if (attribute.Type.GenericTypeArguments[1].IsClassType() && attribute.GetSerializeAsArrayAttribute() != null && token is JArray)
						{
							data = new JObject();
							if ((token as JArray).Count > 0)
							{
								var asArray = attribute.GetSerializeAsArrayAttribute();
								var keyAttribute = !string.IsNullOrWhiteSpace(asArray.KeyAttribute)
									? asArray.KeyAttribute
									: "ID";
								var gotSpecialAttributes = attribute.Type.GenericTypeArguments[1].GetSpecialSerializeAttributes().Count > 0;
								foreach (JObject item in token as JArray)
									if (gotSpecialAttributes)
									{
										object child = Activator.CreateInstance(attribute.Type.GenericTypeArguments[1]);
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
							: Activator.CreateInstance(type);

						@object.SetAttributeValue(attribute, instance);
					}
					catch { }

				// collection
				else if (attribute.Type.IsCollection())
					try
					{
						var instance = token is JObject && (token as JObject).Count > 0
							? serializer.Deserialize(new JTokenReader(token), typeof(Collection))
							: Activator.CreateInstance<Collection>();
						@object.SetAttributeValue(attribute, instance);
					}
					catch { }

				// class
				else if (attribute.Type.IsClassType())
					try
					{
						var instance = Activator.CreateInstance(attribute.Type);
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
							var obj = Activator.CreateInstance(attribute.Type);
							obj.CopyFrom(value as ExpandoObject);
							value = obj;
						}
					}

					// value is primitive type
					else if (attribute.Type.IsPrimitiveType())
						value = value.CastType(attribute.Type);
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
			var instance = Activator.CreateInstance<T>();
			instance.CopyFrom(@object, excluded);
			return instance;
		}

		/// <summary>
		/// Creates new an instance of the object and copies data (from a JSON object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="json">The JSON object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <returns></returns>
		public static T Copy<T>(this T @object, JToken json, HashSet<string> excluded = null)
		{
			var instance = Activator.CreateInstance<T>();
			instance.CopyFrom(json, excluded);
			return instance;
		}

		/// <summary>
		/// Creates new an instance of the object and copies data (from an ExpandoObject object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="expandoObject">The ExpandoObject object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <returns></returns>
		public static T Copy<T>(this T @object, ExpandoObject expandoObject, HashSet<string> excluded = null)
		{
			var instance = Activator.CreateInstance<T>();
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
			var instance = default(T);

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
				instance = Activator.CreateInstance<T>();
				@object.CopyTo(instance);
			}

			// return the new instance of object
			return instance;
		}
		#endregion

		#region JSON Conversions
		static List<AttributeInfo> GetSpecialSerializeAttributes(this Type type)
		{
			return ObjectService.GetProperties(type)
				.Where(attribute => (attribute.Type.IsGenericDictionaryOrCollection() && attribute.GetSerializeAsArrayAttribute() != null) || (attribute.Type.IsGenericListOrHashSet() && attribute.GetSerializeAsObjectAttribute() != null))
				.ToList();
		}

		static SerializeAsArrayAttribute GetSerializeAsArrayAttribute(this AttributeInfo attribute)
		{
			var attributes = attribute.Info.GetCustomAttributes(typeof(SerializeAsArrayAttribute), true);
			return attributes.Length > 0
				? attributes[0] as SerializeAsArrayAttribute
				: null;
		}

		static SerializeAsObjectAttribute GetSerializeAsObjectAttribute(this AttributeInfo attribute)
		{
			var attributes = attribute.Info.GetCustomAttributes(typeof(SerializeAsObjectAttribute), true);
			return attributes.Length > 0
				? attributes[0] as SerializeAsObjectAttribute
				: null;
		}

		/// <summary>
		/// Serializes this object to JSON object (with default settings of Json.NET Serializer)
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static JObject ToJson<T>(this T @object)
		{
			// deserialize as JSON
			var json = JObject.FromObject(@object);

			// update special attributes
			@object.GetType().GetSpecialSerializeAttributes().ForEach(attribute =>
			{
				if (attribute.Type.IsGenericListOrHashSet() && attribute.Type.GenericTypeArguments[0].IsClassType() && attribute.GetSerializeAsObjectAttribute() != null)
				{
					var jsonObject = new JObject();

					var items = @object.GetAttributeValue(attribute.Name) as IEnumerable;
					if (items != null)
					{
						var asObject = attribute.GetSerializeAsObjectAttribute();
						var keyAttribute = !string.IsNullOrWhiteSpace(asObject.KeyAttribute)
							? asObject.KeyAttribute
							: "ID";

						foreach (var item in items)
							if (item != null)
							{
								var key = item.GetAttributeValue(keyAttribute);
								if (key != null)
									jsonObject.Add(new JProperty(key.ToString(), item.ToJson()));
							}
					}

					json[attribute.Name] = jsonObject;
				}
				else if (attribute.Type.IsGenericDictionaryOrCollection() && attribute.Type.GenericTypeArguments[1].IsClassType() && attribute.GetSerializeAsArrayAttribute() != null)
				{
					var jsonArray = new JArray();

					var items = @object.GetAttributeValue(attribute.Name) as IEnumerable;
					if (items != null)
						foreach (var item in items)
							if (item != null)
								jsonArray.Add(item.ToJson());

					json[attribute.Name] = jsonArray;
				}
			});

			// return the JSON
			return json;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON object that contains information for deserializing</param>
		/// <param name="copy">true to create new instance and copy data; false to deserialize object</param>
		/// <returns></returns>
		public static T FromJson<T>(this JToken json, bool copy = false)
		{
			// initialize object
			var @object = default(T);

			// got special, then create new instance and copy data from JSON
			if (copy || typeof(T).GetSpecialSerializeAttributes().Count > 0)
			{
				@object = Activator.CreateInstance<T>();
				@object.CopyFrom(json);
			}

			// deserialize the object from JSON
			else
				@object = (new JsonSerializer()).Deserialize<T>(new JTokenReader(json));

			// return object
			return @object;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON string that contains information for deserializing</param>
		/// <returns></returns>
		public static T FromJson<T>(this string json)
		{
			return (json.Trim().StartsWith("[") ? JArray.Parse(json) as JToken : JObject.Parse(json) as JToken).FromJson<T>();
		}
		#endregion

		#region XML Conversions
		/// <summary>
		/// Serializes this object to XML object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static XElement ToXml<T>(this T @object)
		{
			using (var stream = new MemoryStream())
			{
				using (TextWriter writer = new StreamWriter(stream))
				{
					(new XmlSerializer(typeof(T))).Serialize(writer, @object);
					return XElement.Parse(Encoding.UTF8.GetString(stream.ToArray()));
				}
			}
		}

		/// <summary>
		/// Creates (Deserializes) an object from this XML object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="xml">The XML object that contains information for deserializing</param>
		/// <returns></returns>
		public static T FromXml<T>(this XContainer xml)
		{
			return (T)(new XmlSerializer(typeof(T))).Deserialize(xml.CreateReader());
		}
		/// <summary>
		/// Creates (Deserializes) an object from this XML string
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="xml">The XML string that contains information for deserializing</param>
		/// <returns></returns>
		public static T FromXml<T>(this string xml)
		{
			using (StringReader stringReader = new StringReader(xml))
			{
				using (XmlReader xmlReader = new XmlTextReader(stringReader))
				{
					return (T)(new XmlSerializer(typeof(T))).Deserialize(xmlReader);
				}
			}
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
			using (XmlWriter writer = doc.CreateWriter())
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
			XslCompiledTransform xslTransform = null;
			using (var stream = new MemoryStream(xmlStylesheet.ToBytes()))
			{
				using (var reader = new XmlTextReader(stream))
				{
					try
					{
						XsltSettings xsltSettings = null;
						if (enableScript)
						{
							xsltSettings = new XsltSettings();
							xsltSettings.EnableScript = true;
						}
						xslTransform = new XslCompiledTransform();
						xslTransform.Load(reader, xsltSettings, null);
					}
					catch (Exception)
					{
						throw;
					}
				}
			}
			return document.Transfrom(xslTransform);
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
		public static ExpandoObject ToExpandoObject(this JObject json)
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
			@object.ForEach(p =>
			{
				(expando as IDictionary<string, object>)[p.Key] = p.Value is IDictionary<string, object>
					? (p.Value as IDictionary<string, object>).ToExpandoObject()
					: p.Value;
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
			return @object.ToJson<T>().ToExpandoObject();
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
		/// Gets the value of an attribute of this <see cref="ExpandoObject">ExpandoObject</see> object (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <returns>The value of an attribute (if the object got it); otherwise null.</returns>
		public static object Get(this ExpandoObject @object, string name)
		{
			object value;
			return @object.TryGet(name, out value)
				? value
				: null;
		}

		/// <summary>
		/// Gets the value of an attribute of this <see cref="ExpandoObject">ExpandoObject</see> object (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <returns>The value of an attribute (if the object got it); otherwise null.</returns>
		public static T Get<T>(this ExpandoObject @object, string name)
		{
			T value;
			return @object.TryGet<T>(name, out value)
				? value
				: default(T);
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
			var theObject = @object as IDictionary<string, object>;
			var names = name.IndexOf(".") > 0
				? name.ToArray('.', true, true)
				: new string[] { name };

			// no multiple
			if (names.Length < 2)
			{
				if (!theObject.ContainsKey(name))
					return false;

				value = theObject[name];
				return true;
			}

			// got multiple
			var index = 0;
			while (index < names.Length - 1 && theObject != null)
			{
				theObject = theObject.ContainsKey(names[index])
					? theObject[names[index]] as IDictionary<string, object>
					: null;
				index++;
			}

			if (theObject == null || !theObject.ContainsKey(names[names.Length - 1]))
				return false;

			value = theObject[names[names.Length - 1]];
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
				Type type = typeof(T);

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
						var obj = Activator.CreateInstance(type);
						obj.CopyFrom(theValue as ExpandoObject);
						theValue = obj;
					}
				}

				// other (primitive or other)
				else
					theValue = theValue != null
						? theValue.CastType(type)
						: Convert.ChangeType(theValue, type);

				// cast the value & return state
				value = (T)theValue;
				return true;
			}

			// return the default state
			return false;
		}

		/// <summary>
		/// Checks to see the <see cref="ExpandoObject">ExpandoObject</see> object is got an attribute by specified name (accept the dot (.) to get check of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute for checking, accept the dot (.) to get check of child object</param>
		/// <returns>true if the object got an attribute with the name</returns>
		public static bool Has(this ExpandoObject @object, string name)
		{
			object value;
			return @object.TryGet(name, out value);
		}
		#endregion

	}

	#region Attributes of object serialization
	/// <summary>
	/// Specifies this property is be serialized as an object (JObject) instead as an array (JArray) while serializing/deserializing via Json.NET
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	[DebuggerDisplay("Key = {KeyAttribute}")]
	public class SerializeAsObjectAttribute : Attribute
	{
		public SerializeAsObjectAttribute() { }

		/// <summary>
		/// Gets or sets the name of attribute to use as the key (if not value is provided, the name 'ID' will be used while processing)
		/// </summary>
		public string KeyAttribute { get; set; }
	}

	/// <summary>
	/// Specifies this property is be serialized as an array (JArray) instead as an object (JObject) while serializing/deserializing via Json.NET
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	[DebuggerDisplay("Key = {KeyAttribute}")]
	public class SerializeAsArrayAttribute : Attribute
	{
		public SerializeAsArrayAttribute() { }

		/// <summary>
		/// Gets or sets the name of attribute to use as the key (if not value is provided, the name 'ID' will be used while processing)
		/// </summary>
		public string KeyAttribute { get; set; }
	}
	#endregion

}