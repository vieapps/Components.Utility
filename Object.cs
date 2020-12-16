#region Related components
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Dynamic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with objects
	/// </summary>
	public static partial class ObjectService
	{

		#region Attribute info
		/// <summary>
		/// Presents information of an attribute of an objects
		/// </summary>
		[Serializable, DebuggerDisplay("Name = {Name}, IsPublic = {IsPublic}, CanRead = {CanRead}, CanWrite = {CanWrite}")]
		public class AttributeInfo
		{
			/// <summary>
			/// Initializes information of an objects' attribute
			/// </summary>
			public AttributeInfo() : this(null) { }

			/// <summary>
			/// Initializes information of an objects' attribute
			/// </summary>
			/// <param name="info"></param>
			public AttributeInfo(MemberInfo info) : this(info?.Name, info) { }

			/// <summary>
			/// Initializes information of an objects' attribute
			/// </summary>
			/// <param name="name"></param>
			/// <param name="info"></param>
			public AttributeInfo(string name, MemberInfo info)
			{
				this.Name = name ?? info?.Name;
				this.Info = info;
			}

			/// <summary>
			/// Gets the name
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// Gets the information
			/// </summary>
			public MemberInfo Info { get; }

			/// <summary>
			/// Specifies this attribute is public (everyone can access)
			/// </summary>
			public bool IsPublic => this.Info != null && this.Info is PropertyInfo;

			/// <summary>
			/// Specifies this attribute can be read
			/// </summary>
			public bool CanRead => !this.IsPublic || (this.Info as PropertyInfo).CanRead;

			/// <summary>
			/// Specifies this attribute can be written
			/// </summary>
			public bool CanWrite => !this.IsPublic || (this.Info as PropertyInfo).CanWrite;

			/// <summary>
			/// Gets the type of the attribute
			/// </summary>
			public Type Type => this.IsPublic ? (this.Info as PropertyInfo)?.PropertyType : (this.Info as FieldInfo)?.FieldType;
		}
		#endregion

		#region Object meta data
		static ConcurrentDictionary<Type, List<AttributeInfo>> ObjectProperties { get; } = new ConcurrentDictionary<Type, List<AttributeInfo>>();

		static ConcurrentDictionary<Type, List<AttributeInfo>> ObjectFields { get; } = new ConcurrentDictionary<Type, List<AttributeInfo>>();

		/// <summary>
		/// Gets the collection of public properties of the type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="allowDuplicatedName">true to allow duplicated name</param>
		/// <returns>Collection of public properties</returns>
		public static List<AttributeInfo> GetProperties(Type type, Func<AttributeInfo, bool> predicate = null, bool allowDuplicatedName = false)
		{
			if (type == null || !type.IsClassType())
				return new List<AttributeInfo>();

			if (!ObjectService.ObjectProperties.TryGetValue(type, out var properties))
				lock (ObjectService.ObjectProperties)
				{
					if (!ObjectService.ObjectProperties.TryGetValue(type, out properties))
					{
						var defaultMember = type.GetCustomAttributes(typeof(DefaultMemberAttribute))?.FirstOrDefault() as DefaultMemberAttribute;
						if (allowDuplicatedName)
							ObjectService.ObjectProperties[type] = properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
								.Where(info => defaultMember == null || !defaultMember.MemberName.Equals(info.Name))
								.Select(info => new AttributeInfo(info))
								.ToList();
						else
						{
							properties = new List<AttributeInfo>();
							var names = new HashSet<string>();
							type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
								.Where(info => defaultMember == null || !defaultMember.MemberName.Equals(info.Name))
								.Select(info => new AttributeInfo(info))
								.ForEach(attribute =>
								{
									if (!names.Contains(attribute.Name))
									{
										properties.Add(attribute);
										names.Add(attribute.Name);
									}
								});
							ObjectService.ObjectProperties[type] = properties;
						}
					}
				}

			return predicate != null
				? properties.Where(info => predicate(info)).ToList()
				: properties;
		}

		/// <summary>
		/// Gets the collection of public properties of the object's type
		/// </summary>
		/// <param name="predicate">The predicate</param>
		/// <param name="allowDuplicatedName">true to allow duplicated name</param>
		/// <returns>Collection of public properties</returns>
		public static List<AttributeInfo> GetProperties<T>(Func<AttributeInfo, bool> predicate = null, bool allowDuplicatedName = false) where T : class
			=> ObjectService.GetProperties(typeof(T), predicate, allowDuplicatedName);

		/// <summary>
		/// Gets the collection of public properties of the object's type
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="allowDuplicatedName">true to allow duplicated name</param>
		/// <returns>Collection of public properties</returns>
		public static List<AttributeInfo> GetProperties(this object @object, Func<AttributeInfo, bool> predicate = null, bool allowDuplicatedName = false)
			=> @object != null ? ObjectService.GetProperties(@object.GetType(), predicate, allowDuplicatedName) : new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of public attributes of this type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="allowDuplicatedName">true to allow duplicated name</param>
		/// <returns>Collection of public attributes</returns>
		public static List<AttributeInfo> GetPublicAttributes(this Type type, Func<AttributeInfo, bool> predicate = null, bool allowDuplicatedName = false)
			=> type != null ? ObjectService.GetProperties(type, predicate, allowDuplicatedName) : new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of public attributes of the object's type
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <param name="allowDuplicatedName">true to allow duplicated name</param>
		/// <returns>Collection of public attributes</returns>
		public static List<AttributeInfo> GetPublicAttributes(this object @object, Func<AttributeInfo, bool> predicate = null, bool allowDuplicatedName = false)
			=> @object?.GetType().GetPublicAttributes(predicate, allowDuplicatedName) ?? new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of fields (private attributes) of the type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<AttributeInfo> GetFields(Type type, Func<AttributeInfo, bool> predicate = null)
		{
			if (type == null || !type.IsClassType())
				return new List<AttributeInfo>();

			if (!ObjectService.ObjectFields.TryGetValue(type, out var fields))
				lock (ObjectService.ObjectFields)
				{
					if (!ObjectService.ObjectFields.TryGetValue(type, out fields))
						ObjectService.ObjectFields[type] = fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
							.Where(info => !info.Name.StartsWith("<"))
							.Select(info => new AttributeInfo(info))
							.ToList();
				}

			return predicate != null
				? fields.Where(info => predicate(info)).ToList()
				: fields;
		}

		/// <summary>
		/// Gets the collection of fields (private attributes) of the object's type
		/// </summary>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<AttributeInfo> GetFields<T>(Func<AttributeInfo, bool> predicate = null) where T : class
			=> ObjectService.GetFields(typeof(T), predicate);

		/// <summary>
		/// Gets the collection of fields (private attributes) of the object's type
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<AttributeInfo> GetFields(this object @object, Func<AttributeInfo, bool> predicate = null)
			=> @object != null ? ObjectService.GetFields(@object.GetType(), predicate) : new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of private attributes of this type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of private attributes</returns>
		public static List<AttributeInfo> GetPrivateAttributes(this Type type, Func<AttributeInfo, bool> predicate = null)
			=> type != null ? ObjectService.GetFields(type, predicate) : new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of private attributes of this object type
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of private attributes</returns>
		public static List<AttributeInfo> GetPrivateAttributes(this object @object, Func<AttributeInfo, bool> predicate = null)
			=> @object?.GetType().GetPrivateAttributes(predicate) ?? new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of attributes of the type (means contains all public and private attributes)
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of all attributes</returns>
		public static List<AttributeInfo> GetAttributes(Type type, Func<AttributeInfo, bool> predicate = null)
			=> type != null ? type.GetPublicAttributes(predicate).Concat(type.GetPrivateAttributes(predicate)).ToList() : new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of attributes of the object's type (means contains all public and private attributes)
		/// </summary>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of attributes</returns>
		public static List<AttributeInfo> GetAttributes<T>(Func<AttributeInfo, bool> predicate = null) where T : class
			=> ObjectService.GetAttributes(typeof(T), predicate);

		/// <summary>
		/// Gets the collection of attributes of the object's type (means contains all public and private attributes)
		/// </summary>
		/// <param name="object">The object for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of attributes</returns>
		public static List<AttributeInfo> GetAttributes(this object @object, Func<AttributeInfo, bool> predicate = null)
			=> ObjectService.GetAttributes(@object?.GetType(), predicate);

		/// <summary>
		/// Get the full type name (type name with assembly name) of this type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="justName">true to get only name (means last element in full namespace)</param>
		/// <returns>The string that presents type name</returns>
		public static string GetTypeName(this Type type, bool justName = false)
		{
			if (type == null)
				return "";

			if (!justName)
				return type.FullName + "," + type.Assembly.GetName().Name;

			if (!type.IsGenericType)
				return type.FullName.ToArray('.').Last();

			var typeName = type.FullName;
			var pos = typeName.IndexOf("[");
			typeName = typeName.Remove(pos + 1, typeName.LastIndexOf("]") - pos - 1);
			typeName = typeName.Insert(pos + 1, type.GetGenericArguments().Select(gtype => gtype.GetTypeName(true)).Join(","));
			typeName = typeName.ToArray('.').Last();
			pos = typeName.IndexOf("`");
			return typeName.Remove(pos, typeName.IndexOf("[") - pos).Replace("[", "<").Replace("]", ">");
		}

		/// <summary>
		/// Get the full type name (type name with assembly name) of this object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="justName">true to get only name (means last element in full namespace)</param>
		/// <returns></returns>
		public static string GetTypeName(this object @object, bool justName = false)
			=> @object?.GetType().GetTypeName(justName);

		/// <summary>
		/// Gets the state that indicates the custom attribute is defined or not
		/// </summary>
		/// <param name="type">The type for working with</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes</param>
		/// <returns>The first custom attribute</returns>
		public static bool IsDefined<T>(this Type type, bool inherit) where T : class
			=> type != null && type.IsDefined(typeof(T), inherit);

		/// <summary>
		/// Gets the state that indicates the custom attribute is defined or not
		/// </summary>
		/// <param name="attribute">The attribute for working with</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes</param>
		/// <returns>The first custom attribute</returns>
		public static bool IsDefined<T>(this AttributeInfo attribute, bool inherit) where T : class
			=> attribute != null && attribute.Type != null && attribute.Type.IsDefined<T>(inherit);

		/// <summary>
		/// Gets the collection of custom attributes
		/// </summary>
		/// <param name="type">The type for working with</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The collection of custom attributes</returns>
		public static List<T> GetCustomAttributes<T>(this Type type, bool inherit = true) where T : class
			=> type?.GetCustomAttributes(typeof(T), inherit).Select(attribute => attribute as T).ToList();

		/// <summary>
		/// Gets the first custom attribute
		/// </summary>
		/// <param name="type">The type for working with</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The first custom attribute</returns>
		public static T GetCustomAttribute<T>(this Type type, bool inherit = true) where T : class
			=> type?.GetCustomAttributes<T>(inherit).FirstOrDefault();

		/// <summary>
		/// Gets the collection of custom attributes
		/// </summary>
		/// <param name="attribute">The attribute for working with</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The collection of custom attributes</returns>
		public static List<T> GetCustomAttributes<T>(this AttributeInfo attribute, bool inherit = true) where T : class
			=> attribute?.Info?.GetCustomAttributes(typeof(T), inherit).Select(attr => attr as T).ToList();

		/// <summary>
		/// Gets the first custom attribute
		/// </summary>
		/// <param name="attribute">The attribute for working with</param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The first custom attribute</returns>
		public static T GetCustomAttribute<T>(this AttributeInfo attribute, bool inherit = true) where T : class
			=> attribute?.GetCustomAttributes<T>(inherit).FirstOrDefault();

		/// <summary>
		/// Gets the state to determines the type is primitive or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is primitive</returns>
		public static bool IsPrimitiveType(this Type type)
			=> type != null && (type.IsPrimitive || type.IsStringType() || type.IsDateTimeType() || type.IsNumericType());

		/// <summary>
		/// Gets the state to determines the attribute type is primitive or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsPrimitiveType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsPrimitiveType();

		/// <summary>
		/// Gets the state to determines the type is string or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is string</returns>
		public static bool IsStringType(this Type type)
			=> type != null && type.Equals(typeof(string));

		/// <summary>
		/// Gets the state to determines the attribute type is string or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsStringType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsStringType();

		/// <summary>
		/// Gets the state to determines the type is date-time or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is date-time</returns>
		public static bool IsDateTimeType(this Type type)
			=> type != null && (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTimeOffset)) || type.Equals(typeof(DateTime?)) || type.Equals(typeof(DateTimeOffset?)));

		/// <summary>
		/// Gets the state to determines the attribute type is date-time or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsDateTimeType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsDateTimeType();

		/// <summary>
		/// Gets the state to determines the type is integral numeric or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is integral numeric</returns>
		public static bool IsIntegralType(this Type type)
			=> type != null && (type.Equals(typeof(byte)) || type.Equals(typeof(sbyte))
				|| type.Equals(typeof(byte?)) || type.Equals(typeof(sbyte?))
				|| type.Equals(typeof(short)) || type.Equals(typeof(int)) || type.Equals(typeof(long))
				|| type.Equals(typeof(short?)) || type.Equals(typeof(int?)) || type.Equals(typeof(long?))
				|| type.Equals(typeof(ushort)) || type.Equals(typeof(uint)) || type.Equals(typeof(ulong))
				|| type.Equals(typeof(ushort?)) || type.Equals(typeof(uint?)) || type.Equals(typeof(ulong?)));

		/// <summary>
		/// Gets the state to determines the attribute type is integral numeric or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsIntegralType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsIntegralType();

		/// <summary>
		/// Gets the state to determines the type is floating numeric or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is floating numeric</returns>
		public static bool IsFloatingPointType(this Type type)
			=> type != null && (type.Equals(typeof(decimal)) || type.Equals(typeof(double)) || type.Equals(typeof(float)) || type.Equals(typeof(decimal?)) || type.Equals(typeof(double?)) || type.Equals(typeof(float?)));

		/// <summary>
		/// Gets the state to determines the attribute type is floating numeric or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsFloatingPointType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsFloatingPointType();

		/// <summary>
		/// Gets the state to determines the type is numeric or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsNumericType(this Type type)
			=> type != null && (type.IsIntegralType() || type.IsFloatingPointType());

		/// <summary>
		/// Gets the state to determines the attribute type is numeric or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsNumericType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsNumericType();

		/// <summary>
		/// Gets the state to determines the type is a reference of a class or not
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsClassType(this Type type)
			=> type != null && !type.IsPrimitiveType() && type.IsClass;

		/// <summary>
		/// Gets the state to determines the attribute type is a reference of a class or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsClassType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsClassType();

		/// <summary>
		/// Gets the state to determines the attribute type is enumeration or not
		/// </summary>
		/// <param name="attribute">The attribute for checking</param>
		/// <returns>true if type is numeric</returns>
		public static bool IsEnum(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsEnum;

		/// <summary>
		/// Gets the state to determines the attribute type is serializable (got 'Serializable' attribute) or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool IsSerializable(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsSerializable;

		/// <summary>
		/// Gets the state to determines the object is serializable (got 'Serializable' attribute) or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		public static bool IsSerializable(this object @object)
			=> @object != null && @object.GetType().IsSerializable;

		/// <summary>
		/// Gets the nullable type from this type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type GetNullableType(this Type type)
		{
			if (type == null)
				return null;
			type = Nullable.GetUnderlyingType(type) ?? type;
			return type.IsValueType
				? typeof(Nullable<>).MakeGenericType(type)
				: type;
		}

		/// <summary>
		/// Gets the nullable type from this type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Type GetNullableType<T>()
			=> typeof(T).GetNullableType();

		/// <summary>
		/// Gets the state to determines this type is nullable or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsNullable(this Type type)
			=> type != null && Nullable.GetUnderlyingType(type) != null;

		/// <summary>
		/// Gets the state to determines this attribute is nullable or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool IsNullable(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsNullable();

		/// <summary>
		/// Gets the state to determines this type is nullable or not
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool IsNullable<T>()
		{
			var type = typeof(T);
			return type != null && type.IsNullable();
		}

		/// <summary>
		/// Gets the state to determines this object is nullable or not
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static bool IsNullable<T>(this T @object)
			=> @object != null && ObjectService.IsNullable<T>();
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
			if (type == null || genericType == null || !genericType.IsGenericType)
				return false;

			var baseType = type;
			var rootType = typeof(object);
			while (baseType != null && baseType != rootType)
			{
				var current = baseType.IsGenericType
					? baseType.GetGenericTypeDefinition()
					: baseType;

				if (genericType == current)
					return true;

				baseType = baseType.BaseType;
			}

			return false;
		}

		/// <summary>
		/// Gets the generic type arguments of this type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns></returns>
		public static List<Type> GetGenericTypeArguments(this Type type)
			=> type != null && type.IsGenericType ? type.GenericTypeArguments.ToList() : new List<Type>();

		/// <summary>
		/// Gets the generic type arguments of this attribute type
		/// </summary>
		/// <param name="attribute">The attribute for processing</param>
		/// <returns></returns>
		public static List<Type> GetGenericTypeArguments(this AttributeInfo attribute)
			=> attribute?.Type?.GetGenericTypeArguments() ?? new List<Type>();

		/// <summary>
		/// Gets the generic type arguments of this object
		/// </summary>
		/// <param name="object">The type for processing</param>
		/// <returns></returns>
		public static List<Type> GetGenericTypeArguments(this object @object)
			=> @object?.GetType().GetGenericTypeArguments() ?? new List<Type>();

		/// <summary>
		/// Gets the state to determines the type is a generic list
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericList(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(List<>));

		/// <summary>
		/// Gets the state to determines the attribute type is a generic list
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericList(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsGenericList();

		/// <summary>
		/// Gets the state to determines the object's type is a generic list
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic list; otherwise false.</returns>
		public static bool IsGenericList(this object @object)
			=> @object != null && @object.GetType().IsGenericList();

		/// <summary>
		/// Gets the state to determines the type is a generic hash-set
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic hash-set; otherwise false.</returns>
		public static bool IsGenericHashSet(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(HashSet<>));

		/// <summary>
		/// Gets the state to determines the attribute type is a generic hash-set
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericHashSet(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsGenericHashSet();

		/// <summary>
		/// Gets the state to determines the object's type is a generic hash-set
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic hash-set; otherwise false.</returns>
		public static bool IsGenericHashSet(this object @object)
			=> @object != null && @object.GetType().IsGenericHashSet();

		/// <summary>
		/// Gets the state to determines the type is a generic list or generic hash-set
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic list or generic hash-set; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this Type type)
			=> type != null && (type.IsGenericList() || type.IsGenericHashSet());

		/// <summary>
		/// Gets the state to determines the attribute type is a generic list or generic hash-set
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsGenericListOrHashSet();

		/// <summary>
		/// Gets the state to determines the object's type is generic list or generic hash-set
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic list or generic hash-set; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this object @object)
			=> @object != null && @object.GetType().IsGenericListOrHashSet();

		/// <summary>
		/// Gets the state to determines the type is reference of a generic dictionary
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is a reference (or sub-class) of a generic dictionary; otherwise false.</returns>
		public static bool IsGenericDictionary(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(Dictionary<,>));

		/// <summary>
		/// Gets the state to determines the attribute type is a generic dictionary
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericDictionary(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsGenericDictionary();

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a generic dictionary
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is a reference (or sub-class) of a generic dictionary; otherwise false.</returns>
		public static bool IsGenericDictionary(this object @object)
			=> @object != null && @object.GetType().IsGenericDictionary();

		/// <summary>
		/// Gets the state to determines the type is reference of a generic collection
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is sub-class of the generic collection class; otherwise false.</returns>
		public static bool IsGenericCollection(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(Collection<,>));

		/// <summary>
		/// Gets the state to determines the attribute type is a generic collection
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericCollection(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsGenericCollection();

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a generic collection
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is sub-class of the generic collection class; otherwise false.</returns>
		public static bool IsGenericCollection(this object @object)
			=> @object != null && @object.GetType().IsGenericCollection();

		/// <summary>
		/// Gets the state to determines the type is reference of a generic dictionary or a generic collection
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is sub-class of the generic collection class; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this Type type)
			=> type != null && (type.IsGenericDictionary() || type.IsGenericCollection());

		/// <summary>
		/// Gets the state to determines the attribute type is a generic dictionary or a generic collection
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsGenericDictionaryOrCollection();

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a generic dictionary or a generic collection
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is sub-class of the generic collection class; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this object @object)
			=> @object != null && @object.GetType().IsGenericDictionaryOrCollection();

		/// <summary>
		/// Gets the state to determines the type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface; otherwise false.</returns>
		public static bool IsICollection(this Type type)
			=> type != null && (typeof(ICollection).IsAssignableFrom(type) || type.IsGenericHashSet());

		/// <summary>
		/// Gets the state to determines the attribute type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsICollection(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsICollection();

		/// <summary>
		/// Gets the state to determines the type of the object is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface; otherwise false.</returns>
		public static bool IsICollection(this object @object)
			=> @object != null && @object.GetType().IsICollection();

		/// <summary>
		/// Gets the state to determines the type is reference (or sub-class) of the the <see cref="System.Collections.Specialized.Collection">Collection</see> class
		/// </summary>
		/// <param name="type">Type for checking</param>
		/// <returns>true if the type is is reference (or sub-class) of the the <see cref="System.Collections.Specialized.Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsCollection(this Type type)
			=> type != null && typeof(System.Collections.Specialized.Collection).IsAssignableFrom(type);

		/// <summary>
		/// Gets the state to determines the attribute type is reference (or sub-class) of the the <see cref="System.Collections.Specialized.Collection">Collection</see> class
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsCollection(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsCollection();

		/// <summary>
		/// Gets the state to determines the type of the object is reference (or sub-class) of the the Collection class
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is is reference (or sub-class) of the the Collection class; otherwise false.</returns>
		public static bool IsCollection(this object @object)
			=> @object != null && @object.GetType().IsCollection();

		/// <summary>
		/// Gets the state to determines the attribute type is is array or not
		/// </summary>
		/// <param name="attribute">Teh attribute for checking</param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsArray(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsArray;

		/// <summary>
		/// Gets the state to determines the object is array or not
		/// </summary>
		/// <param name="object">The object for checking type</param>
		/// <returns>true if the type of the object is is reference (or sub-class) of the the Collection class; otherwise false.</returns>
		public static bool IsArray(this object @object)
			=> @object != null && @object.GetType().IsArray;
		#endregion

		#region Create new instance & Cast
		static Dictionary<Type, Func<object>> TypeFactories { get; } = new Dictionary<Type, Func<object>>();

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection
		/// </summary>
		/// <param name="type">The type to be created</param>
		/// <returns>The newly created instance</returns>
		public static object CreateInstance(this Type type)
		{
			if (!ObjectService.TypeFactories.TryGetValue(type, out var func))
				lock (ObjectService.TypeFactories)
				{
					if (!ObjectService.TypeFactories.TryGetValue(type, out func))
						ObjectService.TypeFactories[type] = func = Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
				}
			return func();
		}

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection.
		/// </summary>
		/// <typeparam name="T">The type to be created</typeparam>
		/// <returns>The newly created instance</returns>
		public static T CreateInstance<T>()
			=> (T)typeof(T).CreateInstance();

		/// <summary>
		/// Casts the object to other type
		/// </summary>
		/// <param name="object">The object to cast to other type</param>
		/// <param name="type">The type to cast to</param>
		/// <returns></returns>
		public static object CastAs(this object @object, Type type)
			=> @object != null
				? @object.GetType().Equals(type)
					? @object
					: Convert.ChangeType(@object, type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)) ? Nullable.GetUnderlyingType(type) : type)
				: null;

		/// <summary>
		/// Casts the value to other type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The object to cast to other type</param>
		/// <returns></returns>
		public static T CastAs<T>(this object @object)
			=> @object != null
				? (T)@object.CastAs(typeof(T))
				: default;
		#endregion

		#region Manipulations
		/// <summary>
		/// Sets value of an attribute (public/private/static) of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="name">The string that presents the name of the attribute need to get.</param>
		/// <param name="value">The object that presents the value of the attribute need to set.</param>
		/// <returns>true if success, otherwise false</returns>
		public static bool SetAttributeValue(this object @object, string name, object value)
		{
			if (@object == null || string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException(nameof(name));

			var fieldInfo = @object.GetType().GetField(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (fieldInfo != null)
			{
				fieldInfo.SetValue(@object, value);
				if (@object is IPropertyChangedNotifier)
					(@object as IPropertyChangedNotifier).NotifyPropertyChanged(name, @object);
				return true;
			}

			var propertyInfo = @object.GetType().GetProperty(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (propertyInfo != null && propertyInfo.CanWrite)
			{
				propertyInfo.SetValue(@object, value);
				if (@object is IPropertyChangedNotifier)
					(@object as IPropertyChangedNotifier).NotifyPropertyChanged(name, @object);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Sets value of an attribute (public/private/static) of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="attribute">The object that presents information of the attribute need to get</param>
		/// <param name="value">The object that presents the value of the attribute need to set.</param>
		/// <param name="cast">true to cast the type of attribute</param>
		public static bool SetAttributeValue(this object @object, AttributeInfo attribute, object value, bool cast = false)
			=> @object != null && attribute != null ? @object.SetAttributeValue(attribute.Name, cast && value != null ? value.CastAs(attribute.Type) : value) : throw new ArgumentNullException(nameof(attribute));

		/// <summary>
		/// Gets value of an attribute of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="name">The string that presents the name of the attribute need to get.</param>
		/// <returns>The object that presents data of object's attribute</returns>
		public static object GetAttributeValue(this object @object, string name)
		{
			if (@object == null || string.IsNullOrWhiteSpace(name))
				throw new ArgumentException(nameof(name));

			var fieldInfo = @object.GetType().GetField(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (fieldInfo != null)
				return fieldInfo.GetValue(@object);

			var propertyInfo = @object.GetType().GetProperty(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			return propertyInfo != null && propertyInfo.CanRead
				? propertyInfo.GetValue(@object)
				: null;
		}

		/// <summary>
		/// Gets value of an attribute of an object.
		/// </summary>
		/// <typeparam name="T">The type of value to cast to</typeparam>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="name">The string that presents the name of the attribute need to get.</param>
		/// <param name="default">The default value</param>
		/// <returns>The object that presents data of object's attribute</returns>
		public static T GetAttributeValue<T>(this object @object, string name, T @default = default)
		{
			var value = @object?.GetAttributeValue(name);
			return value != null && value is T
				? (T)value
				: @default;
		}

		/// <summary>
		/// Gets value of an attribute of an object.
		/// </summary>
		/// <param name="object">The object need to get data from</param>
		/// <param name="attribute">The object that presents information of the attribute need to get</param>
		/// <returns>The object that presents data of object's attribute</returns>
		public static object GetAttributeValue(this object @object, AttributeInfo attribute)
			=> @object != null && attribute != null && !string.IsNullOrWhiteSpace(attribute.Name)
				? @object.GetAttributeValue(attribute.Name)
				: throw new ArgumentException(nameof(attribute));

		/// <summary>
		/// Gets value of an attribute of an object.
		/// </summary>
		/// <typeparam name="T">The type of value to cast to</typeparam>
		/// <param name="object">The object need to get data from</param>
		/// <param name="attribute">The object that presents information of the attribute need to get</param>
		/// <param name="default">The default value</param>
		/// <returns>The object that presents data of object's attribute</returns>
		public static T GetAttributeValue<T>(this object @object, AttributeInfo attribute, T @default = default)
		{
			var value = @object?.GetAttributeValue(attribute);
			return value != null && value is T
				? (T)value
				: @default;
		}

		/// <summary>
		/// Gets a static object that specified by name
		/// </summary>
		/// <param name="type">The type of the static class that contains the static object</param>
		/// <param name="name">The name of the static object</param>
		/// <returns></returns>
		public static object GetStaticObject(this Type type, string name)
		{
			if (type == null || string.IsNullOrEmpty(name))
				return null;

			var fieldInfo = type.GetField(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (fieldInfo != null)
				return fieldInfo.GetValue(null);

			var propertyInfo = type.GetProperty(name.Trim(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			return propertyInfo != null && propertyInfo.CanRead
				? propertyInfo.GetValue(null, null)
				: null;
		}

		/// <summary>
		/// Gets a static object that specified by name
		/// </summary>
		/// <param name="class">The string that present the type of the static class that contains the static object</param>
		/// <param name="name">The name of the static object</param>
		/// <returns></returns>
		public static object GetStaticObject(string @class, string name)
			=> string.IsNullOrWhiteSpace(@class) ? null : Type.GetType(@class)?.GetStaticObject(name);

		/// <summary>
		/// Trims all string properties
		/// </summary>
		/// <param name="object"></param>
		public static void TrimAll(this object @object)
		{
			if (@object != null && @object.GetType().IsClassType())
				@object.GetPublicAttributes(attribute => attribute.IsStringType()).ForEach(attribute =>
				{
					if (@object.GetAttributeValue(attribute) is string value && value != null)
						@object.SetAttributeValue(attribute, value.Trim());
				});
		}

		/// <summary>
		/// Trims all string properties
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static T TrimAll<T>(this T @object) where T : class
		{
			(@object as object)?.TrimAll();
			return @object;
		}
		#endregion

		#region Copy objects' properties to/from other object
		/// <summary>
		/// Copies data of the object to other object
		/// </summary>
		/// <param name="object">The object to get data from</param>
		/// <param name="destination">The destination object that will be copied to</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <param name="onCompleted">The action to run before completing the copy process</param>
		/// <param name="onError">The action to run when got any error</param>
		public static void CopyTo<T>(this T @object, T destination, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			if (@object == null || destination == null)
				throw new ArgumentNullException(nameof(destination), "The destination object is null");

			var excludedAttributes = new HashSet<string>(excluded ?? new HashSet<string>(), StringComparer.OrdinalIgnoreCase);

			@object.GetPublicAttributes(attribute => attribute.CanWrite && !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
			{
				try
				{
					destination.SetAttributeValue(attribute, @object.GetAttributeValue(attribute));
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
			});

			@object.GetPrivateAttributes(attribute => !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
			{
				try
				{
					destination.SetAttributeValue(attribute, @object.GetAttributeValue(attribute));
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
			});

			onCompleted?.Invoke(destination);
		}

		/// <summary>
		/// Copies data of the source object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="source">Source object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <param name="onCompleted">The action to run before completing the copy process</param>
		/// <param name="onError">The action to run when got any error</param>
		public static void CopyFrom<T>(this T @object, object source, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			if (@object == null || source == null)
				throw new ArgumentNullException(nameof(source), "The source object is null");

			var excludedAttributes = new HashSet<string>(excluded ?? new HashSet<string>(), StringComparer.OrdinalIgnoreCase);

			@object.GetPublicAttributes(attribute => attribute.CanWrite && !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
			{
				try
				{
					@object.SetAttributeValue(attribute, source.GetAttributeValue(attribute));
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
			});

			@object.GetPrivateAttributes(attribute => !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
			{
				try
				{
					@object.SetAttributeValue(attribute, source.GetAttributeValue(attribute));
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
			});

			onCompleted?.Invoke(@object);
		}
		#endregion

		#region Copy objects' properties from JSON
		/// <summary>
		/// Copies data of the JSON object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="json">JSON object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <param name="onCompleted">The action to run before completing the copy process</param>
		/// <param name="onError">The action to run when got any error</param>
		public static void CopyFrom<T>(this T @object, JToken json, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			if (@object == null || json == null)
				throw new ArgumentNullException(nameof(json), "The JSON is null");

			var serializer = new JsonSerializer();
			var excludedAttributes = new HashSet<string>(excluded ?? new HashSet<string>(), StringComparer.OrdinalIgnoreCase);

			foreach (var attribute in @object.GetPublicAttributes())
			{
				// check
				if (!attribute.CanWrite || excludedAttributes.Contains(attribute.Name))
					continue;

				// check token
				JToken token = null;
				try
				{
					token = json[attribute.Name];
				}
				catch { }
				if (token == null)
					continue;

				// array
				if (attribute.IsArray())
				{
					var type = attribute.Type.GetElementType().MakeArrayType();
					@object.SetAttributeValue(attribute, serializer.Deserialize(new JTokenReader(token), type));
				}

				// generic list/hash-set
				else if (attribute.IsGenericListOrHashSet())
					try
					{
						// prepare
						JArray data = null;
						var dataType = attribute.GetGenericTypeArguments().First();

						if (dataType.IsClassType() && attribute.GetAsObjectAttribute() != null && token is JObject)
						{
							data = new JArray();
							if ((token as JObject).Count > 0)
							{
								var gotSpecialAttributes = dataType.GetSpecialSerializeAttributes().Count > 0;
								foreach (var item in token as JObject)
									if (gotSpecialAttributes)
									{
										var child = dataType.CreateInstance();
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
						var type = attribute.IsGenericList()
							? typeof(List<>).MakeGenericType(dataType)
							: typeof(HashSet<>).MakeGenericType(dataType);
						@object.SetAttributeValue(attribute, data != null && data.Count > 0 ? serializer.Deserialize(new JTokenReader(data), type) : type.CreateInstance());
					}
					catch (Exception ex)
					{
						onError?.Invoke(ex);
					}

				// generic dictionary/collection
				else if (attribute.IsGenericDictionaryOrCollection())
					try
					{
						// prepare
						JObject data = null;
						var dataType = attribute.GetGenericTypeArguments().Last();

						if (dataType.IsClassType() && attribute.GetAsArrayAttribute() != null && token is JArray)
						{
							data = new JObject();
							if ((token as JArray).Count > 0)
							{
								var asArray = attribute.GetAsArrayAttribute();
								var keyAttribute = !string.IsNullOrWhiteSpace(asArray.KeyAttribute)
									? asArray.KeyAttribute
									: "ID";
								var gotSpecialAttributes = dataType.GetSpecialSerializeAttributes().Count > 0;
								foreach (JObject item in token as JArray)
									if (gotSpecialAttributes)
									{
										object child = dataType.CreateInstance();
										child.CopyFrom(item);
										var keyValue = child.GetAttributeValue(keyAttribute);
										if (keyValue != null)
											data.Add(keyValue.ToString(), JObject.FromObject(child));
									}
									else
									{
										var keyValue = item[keyAttribute];
										if (keyValue != null && keyValue is JValue && (keyValue as JValue).Value != null)
											data.Add((keyValue as JValue).Value.ToString(), item);
									}
							}
						}
						else
							data = token as JObject;

						// update
						var type = attribute.Type.IsGenericDictionary()
							? typeof(Dictionary<,>).MakeGenericType(attribute.GetGenericTypeArguments().First(), dataType)
							: typeof(Collection<,>).MakeGenericType(attribute.GetGenericTypeArguments().First(), dataType);
						@object.SetAttributeValue(attribute, data != null && data.Count > 0 ? serializer.Deserialize(new JTokenReader(data), type) : type.CreateInstance());
					}
					catch (Exception ex)
					{
						onError?.Invoke(ex);
					}

				// collection
				else if (attribute.IsCollection())
					try
					{
						@object.SetAttributeValue(attribute, token is JObject && (token as JObject).Count > 0 ? serializer.Deserialize(new JTokenReader(token), typeof(System.Collections.Specialized.Collection)) : typeof(System.Collections.Specialized.Collection).CreateInstance());
					}
					catch (Exception ex)
					{
						onError?.Invoke(ex);
					}

				// enum
				else if (attribute.Type.IsEnum && token is JValue && (token as JValue).Value != null)
					try
					{
						@object.SetAttributeValue(attribute, (token as JValue).Value.ToString().ToEnum(attribute.Type));
					}
					catch { }

				// class
				else if (attribute.IsClassType())
					try
					{
						var instance = attribute.Type.CreateInstance();
						if (token is JObject && (token as JObject).Count > 0)
							instance.CopyFrom(token);
						@object.SetAttributeValue(attribute, instance);
					}
					catch (Exception ex)
					{
						onError?.Invoke(ex);
					}

				// primitive or unknown
				else
					try
					{
						@object.SetAttributeValue(attribute, token is JValue ? (token as JValue).Value : null, true);
					}
					catch (Exception ex)
					{
						onError?.Invoke(ex);
					}
			}

			onCompleted?.Invoke(@object);
		}
		#endregion

		#region Copy objects' properties from ExpandoObject
		/// <summary>
		/// Copies data of the ExpandoObject object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="expandoObject">The <see cref="ExpandoObject">ExpandoObject</see> object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <param name="onCompleted">The action to run before completing the copy process</param>
		/// <param name="onError">The action to run when got any error</param>
		public static void CopyFrom<T>(this T @object, ExpandoObject expandoObject, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			if (@object == null)
				return;

			var excludedAttributes = new HashSet<string>(excluded ?? new HashSet<string>(), StringComparer.OrdinalIgnoreCase);
			foreach (var attribute in @object.GetPublicAttributes())
			{
				// check
				if (!attribute.CanWrite || excludedAttributes.Contains(attribute.Name))
					continue;

				if (!expandoObject.TryGet(attribute.Name, out object value))
					continue;

				// normalize the value
				if (value != null)
				{
					// generic list/hash-set
					if (value is List<object> && attribute.IsGenericListOrHashSet())
						value = attribute.IsGenericList()
							? (value as List<object>).ToList(attribute.GetGenericTypeArguments().First())
							: (value as List<object>).ToHashSet(attribute.GetGenericTypeArguments().First());

					// generic dictionary/collection
					else if (value is ExpandoObject && attribute.IsGenericDictionaryOrCollection())
						value = attribute.IsGenericDictionary()
							? (value as ExpandoObject).ToDictionary(attribute.GetGenericTypeArguments().First(), attribute.GetGenericTypeArguments().Last())
							: (value as ExpandoObject).ToCollection(attribute.GetGenericTypeArguments().First(), attribute.GetGenericTypeArguments().Last());

					// class/array
					else if (value is ExpandoObject && attribute.IsClassType() && !attribute.Type.Equals(typeof(ExpandoObject)))
					{
						var obj = attribute.Type.CreateInstance();
						obj.CopyFrom(value as ExpandoObject);
						value = obj;
					}

					// enum
					else if (attribute.Type.IsEnum)
						value = value.ToString().ToEnum(attribute.Type);

					// primitive type
					else if (attribute.Type.IsPrimitiveType())
						try
						{
							value = value.CastAs(attribute.Type);
						}
						catch (Exception ex)
						{
							var exception = new InvalidCastException($"Cannot cast type of \"{attribute.Name}\" ({value.GetType().GetTypeName(true)} => {attribute.Type.GetTypeName(true)}): {ex.Message}", ex);
							if (onError != null)
								onError(exception);
							else
								throw exception;
						}
				}

				// update the value of attribute
				try
				{
					@object.SetAttributeValue(attribute, value);
				}
				catch (Exception ex)
				{
					onError?.Invoke(ex);
				}
			}

			onCompleted?.Invoke(@object);
		}
		#endregion

		#region Copy & Clone objects
		/// <summary>
		/// Creates new an instance of the object and copies data (from this current object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <param name="onCompleted">The action to run before completing the copy process</param>
		/// <param name="onError">The action to run when got any error</param>
		/// <returns></returns>
		public static T Copy<T>(this T @object, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			var instance = ObjectService.CreateInstance<T>();
			instance.CopyFrom(@object, excluded, null, onError);
			onCompleted?.Invoke(instance);
			return instance;
		}

		/// <summary>
		/// Creates new an instance of the object and copies data (from a JSON object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="json">The JSON object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <param name="onCompleted">The action to run before completing the copy process</param>
		/// <param name="onError">The action to run when got any error</param>
		/// <returns></returns>
		public static T Copy<T>(this JToken json, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			var instance = ObjectService.CreateInstance<T>();
			instance.CopyFrom(json, excluded, null, onError);
			onCompleted?.Invoke(instance);
			return instance;
		}

		/// <summary>
		/// Creates new an instance of the object and copies data (from an ExpandoObject object)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expandoObject">The ExpandoObject object to copy data</param>
		/// <param name="excluded">The hash-set of excluded attributes</param>
		/// <param name="onCompleted">The action to run before completing the copy process</param>
		/// <param name="onError">The action to run when got any error</param>
		/// <returns></returns>
		public static T Copy<T>(this ExpandoObject expandoObject, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			var instance = ObjectService.CreateInstance<T>();
			instance.CopyFrom(expandoObject, excluded, null, onError);
			onCompleted?.Invoke(instance);
			return instance;
		}

		/// <summary>
		/// Clones the object (perform a deep copy of the object)
		/// </summary>
		/// <typeparam name="T">The type of object being copied</typeparam>
		/// <param name="object">The object instance to copy</param>
		/// <param name="onCompleted">The action to run before completing the clone process</param>
		/// <param name="onError">The action to run when got any error</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(this T @object, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			// initialize the object
			var instance = default(T);
			if (@object == null)
				return instance;

			// the object is serializable
			if (@object.IsSerializable())
				using (var stream = UtilityService.CreateMemoryStream())
				{
					try
					{
						using (var writer = new BsonDataWriter(stream))
						{
							new JsonSerializer().Serialize(writer, @object);
							stream.Seek(0, SeekOrigin.Begin);
							using (var reader = new BsonDataReader(stream))
							{
								instance = new JsonSerializer().Deserialize<T>(reader);
							}
						}
					}
					catch (Exception ex)
					{
						if (onError != null)
							onError.Invoke(ex);
						else
							throw;
					}
				}

			// cannot serialize, then create new instance and copy data
			else
				instance = @object.Copy(null, null, onError);

			// return the new instance of object
			onCompleted?.Invoke(instance);
			return instance;
		}
		#endregion

		#region JSON conversions
		internal static List<AttributeInfo> GetSpecialSerializeAttributes(this Type type)
			=> type?.GetPublicAttributes()
				.Where(attribute => (attribute.IsGenericDictionaryOrCollection() && attribute.GetAsArrayAttribute() != null) || (attribute.IsGenericListOrHashSet() && attribute.GetAsObjectAttribute() != null))
				.ToList() ?? new List<AttributeInfo>();

		internal static AsArrayAttribute GetAsArrayAttribute(this AttributeInfo attribute)
			=> attribute?.GetCustomAttribute<AsArrayAttribute>();

		internal static AsObjectAttribute GetAsObjectAttribute(this AttributeInfo attribute)
			=> attribute?.GetCustomAttribute<AsObjectAttribute>();

		/// <summary>
		/// Converts this XmlNode object to JSON object
		/// </summary>
		/// <param name="node">The XmlNode object to convert to JSON</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static JObject ToJson(this XmlNode node, Action<JObject> onCompleted = null)
		{
			var json = new JObject();
			if (node != null && node.Attributes != null)
				foreach (XmlAttribute attribute in node.Attributes)
					json[attribute.Name] = attribute.Value;
			onCompleted?.Invoke(json);
			return json;
		}

		/// <summary>
		/// Serializes this string to JSON object (with default settings of Json.NET Serializer)
		/// </summary>
		/// <param name="json">The JSON string to serialize to JSON object</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static JToken ToJson(this string json, Action<JToken> onCompleted = null)
		{
			var token = JToken.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json.Trim());
			onCompleted?.Invoke(token);
			return token;
		}

		/// <summary>
		/// Serializes this object to JSON object (with default settings of Json.NET Serializer)
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="object">The object to serialize to JSON</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static JToken ToJson<T>(this T @object, Action<JToken> onCompleted = null)
		{
			// check
			if (@object == null)
				return null;

			// by-pass on JSON Token
			if (@object is JToken)
			{
				onCompleted?.Invoke(@object as JToken);
				return @object as JToken;
			}

			// generate
			JToken json = null;

			// array or generict list/hash-set
			if (@object.IsArray() || @object.IsGenericListOrHashSet())
			{
				var type = @object.IsArray()
					? @object.GetType().GetElementType()
					: @object.GetGenericTypeArguments().First();

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
				if (@object.GetGenericTypeArguments().Last().IsClassType())
				{
					json = new JObject();
					var enumerator = (@object as IDictionary).GetEnumerator();
					while (enumerator.MoveNext())
						(json as JObject)[enumerator.Key.ToString()] = enumerator.Value?.ToJson();
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
					if (attribute.IsGenericListOrHashSet() && attribute.GetGenericTypeArguments().First().IsClassType() && attribute.GetAsObjectAttribute() != null)
					{
						var jsonObject = new JObject();

						if (@object.GetAttributeValue(attribute.Name) is IEnumerable items)
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
										jsonObject[key.ToString()] = item?.ToJson();
								}
						}

						json[attribute.Name] = jsonObject;
					}
					else if (attribute.IsGenericDictionaryOrCollection() && attribute.GetGenericTypeArguments().Last().IsClassType() && attribute.GetAsArrayAttribute() != null)
					{
						var jsonArray = new JArray();
						if (@object.GetAttributeValue(attribute.Name) is IEnumerable items)
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
			onCompleted?.Invoke(json);
			return json;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON object that contains information for deserializing</param>
		/// <param name="copy">true to create new instance and copy data; false to deserialize object</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T FromJson<T>(this JToken json, bool copy = false, Action<T, JToken> onCompleted = null)
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
				@object = typeof(T).IsPrimitiveType() && json is JValue
					? (json as JValue).Value.CastAs<T>()
					: new JsonSerializer().Deserialize<T>(new JTokenReader(json));

			// run the handler
			onCompleted?.Invoke(@object, json);

			// return object
			return @object;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON string that contains information for deserializing</param>
		/// <param name="copy">true to create new instance and copy data; false to deserialize object</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T FromJson<T>(this string json, bool copy = false, Action<T, JToken> onCompleted = null)
			=> (json ?? "").ToJson().FromJson(copy, onCompleted);

		/// <summary>
		/// Gets the <see cref="JToken">JToken</see> with the specified key converted to the specified type
		/// </summary>
		/// <typeparam name="T">The type to convert the token to</typeparam>
		/// <param name="json"></param>
		/// <param name="key">The token key</param>
		/// <param name="default">The default value</param>
		/// <returns>The converted token value</returns>
		public static T Value<T>(this JToken json, object key, T @default)
		{
			var value = json.Value<object>(key);
			if (value != null)
				try
				{
					return value.GetType().Equals(typeof(T)) ? (T)value : value.CastAs<T>();
				}
				catch { }
			return @default;
		}

		/// <summary>
		/// Gets the <see cref="JToken">JToken</see> with the specified key converted to the specified type
		/// </summary>
		/// <typeparam name="T">The type to convert the token to</typeparam>
		/// <param name="json"></param>
		/// <param name="key">The token key</param>
		/// <param name="default">The default value</param>
		/// <returns>The converted token value</returns>
		public static T Get<T>(this JToken json, object key, T @default = default)
			=> json.Value(key, @default);
		#endregion

		#region XML conversions
		/// <summary>
		/// Serializes this object to XML object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="object"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XElement ToXml<T>(this T @object, Action<XElement> onCompleted = null) where T : class
		{
			if (@object == null)
				return null;

			if (@object is JToken)
				return JsonConvert.DeserializeXNode(@object.ToString())?.Root;

			using (var stream = UtilityService.CreateMemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					new XmlSerializer(typeof(T)).Serialize(writer, @object);
					var xml = XElement.Parse(stream.ToBytes().GetString());
					onCompleted?.Invoke(xml);
					return xml;
				}
			}
		}

		/// <summary>
		/// Converts this string object to XML object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XElement ToXml(this string @object, Action<XElement> onCompleted = null)
		{
			var xml = XDocument.Parse(@object);
			onCompleted?.Invoke(xml?.Root);
			return xml?.Root;
		}

		/// <summary>
		/// Converts this JSON object to XML object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="rootElementName"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XElement ToXml(this JObject @object, string rootElementName = null, Action<XElement> onCompleted = null)
		{
			var xml = JsonConvert.DeserializeXNode(@object?.ToString(), rootElementName)?.Root;
			onCompleted?.Invoke(xml);
			return xml;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this XML object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="xml">The XML object that contains information for deserializing</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T FromXml<T>(this XContainer xml, Action<T, XContainer> onCompleted = null) where T : class
		{
			// deserialize
			var @object = (T)new XmlSerializer(typeof(T)).Deserialize(xml.CreateReader());

			// run the handler
			onCompleted?.Invoke(@object, xml);

			// return the object
			return @object;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this XML string
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="xml">The XML string that contains information for deserializing</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T FromXml<T>(this string xml, Action<T> onCompleted = null) where T : class
		{
			// deserialize
			T @object;
			using (var stringReader = new StringReader(xml))
			{
				using (var xmlReader = new XmlTextReader(stringReader))
				{
					@object = (T)new XmlSerializer(typeof(T)).Deserialize(xmlReader);
				}
			}

			// run the handler
			onCompleted?.Invoke(@object);

			// return the object
			return @object;
		}

		/// <summary>
		/// Converts this XmlNode object to XElement object
		/// </summary>
		/// <param name="node"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XElement ToXElement(this XmlNode node, Action<XElement> onCompleted = null)
		{
			var doc = new XDocument();
			using (var writer = doc.CreateWriter())
			{
				node.WriteTo(writer);
				onCompleted?.Invoke(doc.Root);
				return doc.Root;
			}
		}

		/// <summary>
		/// Converts this XElement object to XmlNode object
		/// </summary>
		/// <param name="element"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XmlNode ToXmlNode(this XElement element, Action<XmlNode> onCompleted = null)
		{
			using (var reader = element.CreateReader())
			{
				var node = new XmlDocument().ReadNode(reader);
				onCompleted?.Invoke(node);
				return node;
			}
		}

		/// <summary>
		/// Converts this XElement object to XmlDocument object
		/// </summary>
		/// <param name="element"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XmlDocument ToXmlDocument(this XElement element, Action<XmlDocument> onCompleted = null)
		{
			using (var reader = element.CreateReader())
			{
				var doc = new XmlDocument();
				doc.Load(reader);
				doc.DocumentElement.Attributes.RemoveAll();
				onCompleted?.Invoke(doc);
				return doc;
			}
		}

		/// <summary>
		/// Converts this XmlDocument object to XDocument object
		/// </summary>
		/// <param name="document"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XDocument ToXDocument(this XmlDocument document, Action<XDocument> onCompleted = null)
		{
			var doc = new XDocument();
			using (var writer = doc.CreateWriter())
			{
				document.WriteTo(writer);
			}
			var declaration = document.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
			if (declaration != null)
				doc.Declaration = new XDeclaration(declaration.Version, declaration.Encoding, declaration.Standalone);
			onCompleted?.Invoke(doc);
			return doc;
		}

		/// <summary>
		/// Converts this XDocument object to XmlDocument object
		/// </summary>
		/// <param name="document"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static XmlDocument ToXmlDocument(this XDocument document, Action<XmlDocument> onCompleted = null)
		{
			using (var reader = document.CreateReader())
			{
				var doc = new XmlDocument();
				doc.Load(reader);
				if (document.Declaration != null)
					doc.InsertBefore(doc.CreateXmlDeclaration(document.Declaration.Version, document.Declaration.Encoding, document.Declaration.Standalone), doc.FirstChild);
				onCompleted?.Invoke(doc);
				return doc;
			}
		}
		#endregion

		#region ExpandoObject conversions & manipulations
		/// <summary>
		/// Creates (Deserializes) an <see cref="ExpandoObject">ExpandoObject</see> object from this JSON string
		/// </summary>
		/// <param name="json">The string that presents serialized data to create object</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject(this string json, Action<ExpandoObject> onCompleted = null)
		{
			var expando = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
			onCompleted?.Invoke(expando);
			return expando;
		}

		/// <summary>
		/// Creates (Deserializes) an <see cref="ExpandoObject">ExpandoObject</see> object from this JSON
		/// </summary>
		/// <param name="json">The string that presents serialized data to create object</param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject(this JToken json, Action<ExpandoObject> onCompleted = null)
		{
			var expando = new JsonSerializer().Deserialize<ExpandoObject>(new JTokenReader(json));
			onCompleted?.Invoke(expando);
			return expando;
		}

		/// <summary>
		/// Creates an <see cref="ExpandoObject">ExpandoObject</see> object from this dictionary object
		/// </summary>
		/// <param name="object"></param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject(this IDictionary<string, object> @object, Action<ExpandoObject> onCompleted = null)
		{
			var expando = new ExpandoObject();
			@object.ForEach(kvp => (expando as IDictionary<string, object>)[kvp.Key] = kvp.Value is IDictionary<string, object> dictionary ? dictionary.ToExpandoObject() : kvp.Value);
			onCompleted?.Invoke(expando);
			return expando;
		}

		/// <summary>
		/// Creates an <see cref="ExpandoObject">ExpandoObject</see> object from this object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject<T>(this T @object, Action<ExpandoObject> onCompleted = null) where T : class
			=> (@object is JToken ? @object as JToken : @object?.ToJson())?.ToExpandoObject(onCompleted);

		/// <summary>
		/// Creates (Deserializes) an object from this <see cref="ExpandoObject">ExpandoObject</see> object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T FromExpandoObject<T>(this ExpandoObject @object, Action<T, ExpandoObject> onCompleted = null) where T : class
		{
			var instance = JObject.FromObject(@object).FromJson<T>();
			onCompleted?.Invoke(instance, @object);
			return instance;
		}

		/// <summary>
		/// Merges this <see cref="ExpandoObject">ExpandoObject</see> object with other
		/// </summary>
		/// <param name="object"></param>
		/// <param name="other"></param>
		/// <param name="onCompleted">The action to run when the merging process is completed</param>
		/// <returns></returns>
		public static ExpandoObject Merge(this ExpandoObject @object, ExpandoObject other, Action<ExpandoObject> onCompleted = null)
		{
			other?.ForEach(kvp =>
			{
				if (@object.TryGet(kvp.Key, out var current))
				{
					if (current == null)
						@object.Set(kvp.Key, kvp.Value);
					else if (current is ExpandoObject currentAsExpandoObject)
					{
						if (kvp.Value is ExpandoObject valueAsExpandoObject)
							currentAsExpandoObject.Merge(valueAsExpandoObject);
						else
							currentAsExpandoObject.Set(kvp.Key, kvp.Value);
					}
				}
				else
					@object.Set(kvp.Key, kvp.Value);
			});
			onCompleted?.Invoke(@object);
			return @object;
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
			if (@object == null || string.IsNullOrWhiteSpace(name))
				return false;

			// prepare
			var dictionary = @object as IDictionary<string, object>;
			var names = name.IndexOf(".") > 0
				? name.ToArray('.', true, true)
				: new[] { name };

			// no multiple
			if (names.Length < 2)
				return dictionary.TryGetValue(name, out value);

			// got multiple
			var index = 0;
			while (index < names.Length - 1 && dictionary != null)
			{
				dictionary = dictionary.ContainsKey(names[index])
					? dictionary[names[index]] as IDictionary<string, object>
					: null;
				index++;
			}

			return dictionary != null && dictionary.TryGetValue(names[names.Length - 1], out value);
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
			value = default;
			if (@object == null)
				return false;

			// get value & normalize
			if (@object.TryGet(name, out var tempValue))
			{
				// get type
				var type = typeof(T);

				// generic list/hash-set
				if (tempValue is List<object> tempList && type.IsGenericListOrHashSet())
					tempValue = type.IsGenericList()
						? tempList.ToList<T>()
						: tempList.ToHashSet<T>();

				// generic dictionary/collection or object
				else if (tempValue is ExpandoObject tempExpando)
				{
					if (type.IsGenericDictionaryOrCollection())
						tempValue = type.IsGenericDictionary()
							? tempExpando.ToDictionary<T>()
							: tempExpando.ToCollection<T>();

					else if (type.IsClassType() && !type.Equals(typeof(ExpandoObject)))
					{
						var tempObject = type.CreateInstance();
						tempObject.CopyFrom(tempExpando);
						tempValue = tempObject;
					}
				}

				// other (primitive or other)
				else
					tempValue = tempValue.CastAs(type);

				// cast the value & return state
				value = (T)tempValue;
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
			=> @object.TryGet(name, out object value)
				? value
				: null;

		/// <summary>
		/// Gets the value of an attribute of this <see cref="ExpandoObject">ExpandoObject</see> object (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <param name="default">Default value when the attribute is not found</param>
		/// <returns>The value of an attribute (if the object got it); otherwise null.</returns>
		public static T Get<T>(this ExpandoObject @object, string name, T @default = default)
			=> @object.TryGet(name, out T value)
				? value
				: @default;

		/// <summary>
		/// Gets the value of an attribute of this <see cref="ExpandoObject">ExpandoObject</see> object (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <param name="default">Default value when the attribute is not found</param>
		/// <returns>The value of an attribute (if the object got it); otherwise null.</returns>
		public static T Value<T>(this ExpandoObject @object, string name, T @default = default)
			=> @object.Get(name, @default);

		/// <summary>
		/// Checks to see the <see cref="ExpandoObject">ExpandoObject</see> object is got an attribute by specified name (accept the dot (.) to get check of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute for checking, accept the dot (.) to get check of child object</param>
		/// <returns>true if the object got an attribute with the name</returns>
		public static bool Has(this ExpandoObject @object, string name)
			=> @object != null && (name.IndexOf(".") < 0 ? (@object as IDictionary<string, object>).ContainsKey(name) : @object.TryGet(name, out var value));

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
				return dictionary.ContainsKey(name) && dictionary.Remove(name);

			// got multiple
			var index = 0;
			while (index < names.Length - 1 && dictionary != null)
			{
				dictionary = dictionary.ContainsKey(names[index])
					? dictionary[names[index]] as IDictionary<string, object>
					: null;
				index++;
			}

			return dictionary != null && dictionary.Remove(names[names.Length - 1], out var value);
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

	#region Property changed notifier
	/// <summary>
	/// Presents the notifier for notifying an event when a property value was change
	/// </summary>
	public interface IPropertyChangedNotifier : INotifyPropertyChanged
	{
		/// <summary>
		/// Calls for notifying an event when a property value was change
		/// </summary>
		/// <param name="name"></param>
		/// <param name="sender"></param>
		void NotifyPropertyChanged([CallerMemberName] string name = "", object sender = null);

		/// <summary>
		/// Fires automatically when receive an event of property value was changed
		/// </summary>
		/// <param name="name"></param>
		void ProcessPropertyChanged(string name);
	}
	#endregion

}