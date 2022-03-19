#region Related components
using System;
using System.IO;
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
using Newtonsoft.Json;
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

		#region Meta of objects
		/// <summary>
		/// Presents meta information of object's field/property/method
		/// </summary>
		[Serializable, DebuggerDisplay("Name = {Name}, IsPublic = {IsPublic}, IsStatic = {IsStatic}")]
		public class MetaInfo
		{
			internal readonly bool? _isPublic, _isStatic;

			/// <summary>
			/// Initializes information of an object' meta data
			/// </summary>
			public MetaInfo() : this(null) { }

			/// <summary>
			/// Initializes information of an object' meta data
			/// </summary>
			/// <param name="info"></param>
			/// <param name="isPublic"></param>
			/// <param name="isStatic"></param>
			public MetaInfo(MemberInfo info, bool? isPublic = null, bool? isStatic = null)
			{
				this.Info = info;
				if (isPublic != null && isPublic.Value)
					this._isPublic = true;
				if (isStatic != null && isStatic.Value)
					this._isStatic = true;
			}

			/// <summary>
			/// Gets the detail information
			/// </summary>
			public MemberInfo Info { get; }

			/// <summary>
			/// Gets the name
			/// </summary>
			public string Name => this.Info?.Name;

			/// <summary>
			/// Gets the detail information (when this is meta information of a field)
			/// </summary>
			public FieldInfo FieldInfo => this.Info is FieldInfo fieldInfo ? fieldInfo : null;

			/// <summary>
			/// Specifies this meta is information of a field
			/// </summary>
			public bool IsField => this.FieldInfo != null;

			/// <summary>
			/// Gets the detail information (when this is meta information of a property)
			/// </summary>
			public PropertyInfo PropertyInfo => this.Info is PropertyInfo propertyInfo ? propertyInfo : null;

			/// <summary>
			/// Specifies this meta is information of a property
			/// </summary>
			public bool IsProperty => this.PropertyInfo != null;

			/// <summary>
			/// Gets the detail information (when this is meta information of a method)
			/// </summary>
			public MethodInfo MethodInfo => this.Info is MethodInfo methodInfo ? methodInfo : null;

			/// <summary>
			/// Specifies this meta is information of a method
			/// </summary>
			public bool IsMethod => this.MethodInfo != null;

			/// <summary>
			/// Specifies is public or not
			/// </summary>
			public bool IsPublic => (this.IsField && this.FieldInfo.IsPublic) || (this.IsMethod && this.MethodInfo.IsPublic) || (this._isPublic != null && this._isPublic.Value);

			/// <summary>
			/// Specifies is static or not
			/// </summary>
			public bool IsStatic => (this.IsField && this.FieldInfo.IsStatic) || (this.IsMethod && this.MethodInfo.IsStatic) || (this._isStatic != null && this._isStatic.Value);

			/// <summary>
			/// Gets the type of the member
			/// </summary>
			public Type Type => this.FieldInfo?.FieldType ?? this.PropertyInfo?.PropertyType ?? this.MethodInfo?.ReturnType;
		}

		/// <summary>
		/// Presents information of an object's attribute (field or property)
		/// </summary>
		[Serializable, DebuggerDisplay("Name = {Name}, IsPublic = {IsPublic}, CanRead = {CanRead}, CanWrite = {CanWrite}")]
		public class AttributeInfo : MetaInfo
		{
			/// <summary>
			/// Initializes information of an objects' attribute
			/// </summary>
			public AttributeInfo() : base(null) { }

			/// <summary>
			/// Initializes information of an objects' attribute
			/// </summary>
			/// <param name="info"></param>
			/// <param name="isPublic"></param>
			/// <param name="isStatic"></param>
			public AttributeInfo(MemberInfo info, bool? isPublic = null, bool? isStatic = null) : base(info, isPublic, isStatic) { }

			/// <summary>
			/// Specifies this attribute can be read or not
			/// </summary>
			public bool CanRead => !this.IsMethod && (this.IsField || this.PropertyInfo.CanRead);

			/// <summary>
			/// Specifies this attribute can be written or not
			/// </summary>
			public bool CanWrite => !this.IsMethod && (this.IsField || this.PropertyInfo.CanWrite);
		}

		static ConcurrentDictionary<Type, List<MetaInfo>> TypeMeta { get; } = new ConcurrentDictionary<Type, List<MetaInfo>>();

		/// <summary>
		/// Gets the meta information of a specified type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetMetaInfo(this Type type, Func<MetaInfo, bool> predicate = null)
		{
			if (type == null || !type.IsClassType())
				return new List<MetaInfo>();

			if (!ObjectService.TypeMeta.TryGetValue(type, out var metaInfo) || metaInfo == null)
				lock (ObjectService.TypeMeta)
				{
					if (!ObjectService.TypeMeta.TryGetValue(type, out metaInfo) || metaInfo == null)
					{
						metaInfo = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(field => !field.Name.StartsWith("<")).Select(field => new MetaInfo(field))
							.Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(property => new MetaInfo(property, true)))
							.Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Select(property => new MetaInfo(property, false)))
							.Concat(type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(method => !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_") && !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove_")).Select(method => new MetaInfo(method)))
							.ToList();
						metaInfo = metaInfo
							.Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).Where(property => metaInfo.FirstOrDefault(info => info.Name == property.Name) == null).Select(property => new MetaInfo(property, true, true)))
							.Concat(type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static).Where(property => metaInfo.FirstOrDefault(info => info.Name == property.Name) == null).Select(property => new MetaInfo(property, false, true)))
							.ToList();
						ObjectService.TypeMeta.TryAdd(type, metaInfo);
					}
				}

			return predicate != null
				? metaInfo.Where(info => predicate(info)).ToList()
				: metaInfo;
		}

		/// <summary>
		/// Gets the meta information of a specified type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetMetaInfo<T>(Func<MetaInfo, bool> predicate = null) where T : class
			=> ObjectService.GetMetaInfo(typeof(T), predicate);

		/// <summary>
		/// Gets the meta information of this object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		/// <returns></returns>
		public static List<MetaInfo> GetMetaInfo(this object @object, Func<MetaInfo, bool> predicate = null)
			=> ObjectService.GetMetaInfo(@object?.GetType(), predicate);

		/// <summary>
		/// Gets the collection of fields
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <param name="predicate">The predicate</param>
		/// <returns>Collection of private fields/attributes</returns>
		public static List<MetaInfo> GetFields(Type type, Func<MetaInfo, bool> predicate = null)
			=> type != null && type.IsClassType()
				? predicate != null
					? type.GetMetaInfo(info => info.IsField).Where(info => predicate(info)).ToList()
					: type.GetMetaInfo(info => info.IsField).ToList()
				: new List<MetaInfo>();

		/// <summary>
		/// Gets the collection of fields
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetFields<T>(Func<MetaInfo, bool> predicate = null) where T : class
			=> ObjectService.GetFields(typeof(T), predicate);

		/// <summary>
		/// Gets the collection of fields
		/// </summary>
		/// <param name="object"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		/// <returns></returns>
		public static List<MetaInfo> GetFields(this object @object, Func<MetaInfo, bool> predicate = null)
			=> ObjectService.GetFields(@object?.GetType(), predicate);

		/// <summary>
		/// Gets the collection of properties
		/// </summary>
		/// <param name="type"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetProperties(this Type type, Func<MetaInfo, bool> predicate = null)
			=> type != null && type.IsClassType()
				? predicate != null
					? type.GetMetaInfo(info => info.IsProperty).Where(info => predicate(info)).ToList()
					: type.GetMetaInfo(info => info.IsProperty).ToList()
				: new List<MetaInfo>();

		/// <summary>
		/// Gets the collection of properties
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetProperties<T>(Func<MetaInfo, bool> predicate = null) where T : class
			=> ObjectService.GetProperties(typeof(T), predicate);

		/// <summary>
		/// Gets the collection of properties
		/// </summary>
		/// <param name="object"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetProperties(this object @object, Func<MetaInfo, bool> predicate = null)
			=> ObjectService.GetProperties(@object?.GetType(), predicate);

		/// <summary>
		/// Gets the collection of methods
		/// </summary>
		/// <param name="type"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetMethods(this Type type, Func<MetaInfo, bool> predicate = null)
			=> type != null && type.IsClassType()
				? predicate != null
					? type.GetMetaInfo().Where(info => info.IsMethod).Where(info => predicate(info)).ToList()
					: type.GetMetaInfo().Where(info => info.IsMethod).ToList()
				: new List<MetaInfo>();

		/// <summary>
		/// Gets the collection of methods
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetMethods<T>(Func<MetaInfo, bool> predicate = null) where T : class
			=> ObjectService.GetMethods(typeof(T), predicate);

		/// <summary>
		/// Gets the collection of methods
		/// </summary>
		/// <param name="object"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<MetaInfo> GetMethods(this object @object, Func<MetaInfo, bool> predicate = null)
			=> ObjectService.GetMethods(@object?.GetType(), predicate);

		/// <summary>
		/// Gets the collection of all attributes (means all fields and all properties)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<AttributeInfo> GetAttributes(this Type type, Func<AttributeInfo, bool> predicate = null)
			=> type != null && type.IsClassType()
				? predicate != null
					? type.GetMetaInfo(info => info.IsField || info.IsProperty).Select(info => new AttributeInfo(info.Info, info._isPublic, info._isStatic)).Where(info => predicate(info)).ToList()
					: type.GetMetaInfo(info => info.IsField || info.IsProperty).Select(info => new AttributeInfo(info.Info, info._isPublic, info._isStatic)).ToList()
				: new List<AttributeInfo>();

		/// <summary>
		/// Gets the collection of all attributes (means all fields and all properties)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<AttributeInfo> GetAttributes(this object @object, Func<AttributeInfo, bool> predicate = null)
			=> ObjectService.GetAttributes(@object?.GetType(), predicate);

		/// <summary>
		/// Gets the collection of public attributes (means public fields and public properties)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<AttributeInfo> GetPublicAttributes(this Type type, Func<AttributeInfo, bool> predicate = null)
			=> ObjectService.GetAttributes(type, predicate).Where(info => info.IsPublic).ToList();

		/// <summary>
		/// Gets the collection of public attributes (means public fields and public properties)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<AttributeInfo> GetPublicAttributes(this object @object, Func<AttributeInfo, bool> predicate = null)
			=> ObjectService.GetPublicAttributes(@object?.GetType(), predicate);

		/// <summary>
		/// Gets the collection of private attributes (means private fields and private properties)
		/// </summary>
		/// <param name="type"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<AttributeInfo> GetPrivateAttributes(this Type type, Func<AttributeInfo, bool> predicate = null)
			=> ObjectService.GetAttributes(type, predicate).Where(info => !info.IsPublic).ToList();

		/// <summary>
		/// Gets the collection of private attributes (means private fields and private properties)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static List<AttributeInfo> GetPrivateAttributes(this object @object, Func<AttributeInfo, bool> predicate = null)
			=> ObjectService.GetPrivateAttributes(@object?.GetType(), predicate);

		/// <summary>
		/// Gets a static attribute (means static field or static property) of the type that specified by name
		/// </summary>
		/// <param name="type">The type of the static class that contains the static object</param>
		/// <param name="name">The name of the static attribute</param>
		/// <returns></returns>
		public static object GetStaticObject(this Type type, string name)
		{
			try
			{
				var attribute = type != null && !string.IsNullOrWhiteSpace(name) ? type.GetAttributes(attr => attr.IsStatic && attr.CanRead).FirstOrDefault(attr => attr.Name.IsEquals(name)) : null;
				return attribute?.FieldInfo?.GetValue(null) ?? attribute?.PropertyInfo?.GetValue(null);
			}
			catch
			{
				return null;
			}
		}

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
		/// Gets the collection of custom attributes
		/// </summary>
		/// <param name="type"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The collection of custom attributes</returns>
		public static List<T> GetCustomAttributes<T>(this Type type, bool inherit = true) where T : class
			=> type?.GetCustomAttributes(typeof(T), inherit).Select(attr => attr as T).ToList();

		/// <summary>
		/// Gets the collection of custom attributes
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The collection of custom attributes</returns>
		public static List<T> GetCustomAttributes<T>(this AttributeInfo attribute, bool inherit = true) where T : class
			=> attribute?.Info?.GetCustomAttributes(typeof(T), inherit).Select(attr => attr as T).ToList();

		/// <summary>
		/// Gets the collection of custom attributes
		/// </summary>
		/// <param name="object"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The collection of custom attributes</returns>
		public static List<T> GetCustomAttributes<T>(this object @object, bool inherit = true) where T : class
			=> @object?.GetType().GetCustomAttributes<T>(inherit);

		/// <summary>
		/// Gets the first custom attribute
		/// </summary>
		/// <param name="type"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The first custom attribute</returns>
		public static T GetCustomAttribute<T>(this Type type, bool inherit = true) where T : class
			=> type?.GetCustomAttributes<T>(inherit).FirstOrDefault();

		/// <summary>
		/// Gets the first custom attribute
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The first custom attribute</returns>
		public static T GetCustomAttribute<T>(this AttributeInfo attribute, bool inherit = true) where T : class
			=> attribute?.GetCustomAttributes<T>(inherit).FirstOrDefault();

		/// <summary>
		/// Gets the first custom attribute
		/// </summary>
		/// <param name="object"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes (default is true)</param>
		/// <returns>The first custom attribute</returns>
		public static T GetCustomAttribute<T>(this object @object, bool inherit = true) where T : class
			=> @object?.GetType().GetCustomAttribute<T>(inherit);

		internal static AsArrayAttribute GetAsArrayAttribute(this AttributeInfo attribute)
			=> attribute?.GetCustomAttribute<AsArrayAttribute>();

		internal static AsObjectAttribute GetAsObjectAttribute(this AttributeInfo attribute)
			=> attribute?.GetCustomAttribute<AsObjectAttribute>();

		internal static List<AttributeInfo> GetSpecialSerializeAttributes(this Type type)
			=> type?.GetPublicAttributes(attribute => !attribute.IsStatic)
				.Where(attribute => (attribute.IsGenericDictionaryOrCollection() && attribute.GetAsArrayAttribute() != null) || (attribute.IsGenericListOrHashSet() && attribute.GetAsObjectAttribute() != null))
				.ToList() ?? new List<AttributeInfo>();

		internal static List<AttributeInfo> GetSpecialSerializeAttributes(this object @object)
			=> @object?.GetType().GetSpecialSerializeAttributes();

		internal static bool GotSpecialSerializeAttributes(this Type type)
			=> type != null && type.IsClassType() && type.GetSpecialSerializeAttributes().Any();

		/// <summary>
		/// Gets the state that indicates the custom attribute is defined or not
		/// </summary>
		/// <param name="type"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes</param>
		/// <returns>The first custom attribute</returns>
		public static bool IsDefined<T>(this Type type, bool inherit) where T : class
			=> type != null && type.IsDefined(typeof(T), inherit);

		/// <summary>
		/// Gets the state that indicates the custom attribute is defined or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes</param>
		/// <returns>The first custom attribute</returns>
		public static bool IsDefined<T>(this AttributeInfo attribute, bool inherit) where T : class
			=> attribute != null && attribute.Type != null && attribute.Type.IsDefined<T>(inherit);

		/// <summary>
		/// Gets the state that indicates the custom attribute is defined or not
		/// </summary>
		/// <param name="object"></param>
		/// <param name="inherit">true to search this member's inheritance chain to find the attributes</param>
		/// <returns>The first custom attribute</returns>
		public static bool IsDefined<T>(this object @object, bool inherit) where T : class
			=> @object != null && @object.GetType().IsDefined<T>(inherit);

		/// <summary>
		/// Gets the state to determines the type is primitive or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if type is primitive</returns>
		public static bool IsPrimitiveType(this Type type)
			=> type != null && (type.IsPrimitive || type.IsStringType() || type.IsDateTimeType() || type.IsNumericType());

		/// <summary>
		/// Gets the state to determines the attribute's type is primitive or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsPrimitiveType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsPrimitiveType();

		/// <summary>
		/// Gets the state to determines the object's type is primitive or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsPrimitiveType(this object @object)
			=> @object != null && @object.GetType().IsPrimitiveType();

		/// <summary>
		/// Gets the state to determines the type is string or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if type is string</returns>
		public static bool IsStringType(this Type type)
			=> type != null && type.Equals(typeof(string));

		/// <summary>
		/// Gets the state to determines the attribute's type is string or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsStringType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsStringType();

		/// <summary>
		/// Gets the state to determines the object's type is string or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsStringType(this object @object)
			=> @object != null && @object.GetType().IsStringType();

		/// <summary>
		/// Gets the state to determines the type is date-time or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if type is date-time</returns>
		public static bool IsDateTimeType(this Type type)
			=> type != null && (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTimeOffset)) || type.Equals(typeof(DateTime?)) || type.Equals(typeof(DateTimeOffset?)));

		/// <summary>
		/// Gets the state to determines the attribute's type is date-time or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsDateTimeType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsDateTimeType();

		/// <summary>
		/// Gets the state to determines the object's type is date-time or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsDateTimeType(this object @object)
			=> @object != null && @object.GetType().IsDateTimeType();

		/// <summary>
		/// Gets the state to determines the type is integral numeric or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if type is integral numeric</returns>
		public static bool IsIntegralType(this Type type)
			=> type != null && (type.Equals(typeof(byte)) || type.Equals(typeof(sbyte))
				|| type.Equals(typeof(byte?)) || type.Equals(typeof(sbyte?))
				|| type.Equals(typeof(short)) || type.Equals(typeof(int)) || type.Equals(typeof(long))
				|| type.Equals(typeof(short?)) || type.Equals(typeof(int?)) || type.Equals(typeof(long?))
				|| type.Equals(typeof(ushort)) || type.Equals(typeof(uint)) || type.Equals(typeof(ulong))
				|| type.Equals(typeof(ushort?)) || type.Equals(typeof(uint?)) || type.Equals(typeof(ulong?)));

		/// <summary>
		/// Gets the state to determines the attribute's type is integral numeric or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsIntegralType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsIntegralType();

		/// <summary>
		/// Gets the state to determines the object's type is integral numeric or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsIntegralType(this object @object)
			=> @object != null && @object.GetType().IsIntegralType();

		/// <summary>
		/// Gets the state to determines the type is floating numeric or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if type is floating numeric</returns>
		public static bool IsFloatingPointType(this Type type)
			=> type != null && (type.Equals(typeof(decimal)) || type.Equals(typeof(decimal?))
				|| type.Equals(typeof(double)) || type.Equals(typeof(double?))
				|| type.Equals(typeof(float)) || type.Equals(typeof(float?)));

		/// <summary>
		/// Gets the state to determines the attribute's type is floating numeric or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsFloatingPointType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsFloatingPointType();

		/// <summary>
		/// Gets the state to determines the object's type is floating numeric or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsFloatingPointType(this object @object)
			=> @object != null && @object.GetType().IsFloatingPointType();

		/// <summary>
		/// Gets the state to determines the type is numeric or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsNumericType(this Type type)
			=> type != null && (type.IsIntegralType() || type.IsFloatingPointType());

		/// <summary>
		/// Gets the state to determines the attribute's type is numeric or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsNumericType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsNumericType();

		/// <summary>
		/// Gets the state to determines the object's type is numeric or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsNumericType(this object @object)
			=> @object != null && @object.GetType().IsNumericType();

		/// <summary>
		/// Gets the state to determines the type is a reference of a class or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsClassType(this Type type)
			=> type != null && !type.IsPrimitiveType() && type.IsClass;

		/// <summary>
		/// Gets the state to determines the attribute's type is a reference of a class or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsClassType(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsClassType();

		/// <summary>
		/// Gets the state to determines the object's type is a reference of a class or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsClassType(this object @object)
			=> @object != null && @object.GetType().IsClassType();

		/// <summary>
		/// Gets the state to determines the attribute's type is enumeration or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsEnum(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsEnum;

		/// <summary>
		/// Gets the state to determines the object's type is enumeration or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsEnum(this object @object)
			=> @object != null && @object.GetType().IsEnum;

		/// <summary>
		/// Gets the state that determines the type is got Json.NET string enumeration or not
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsStringEnum(this Type type)
			=> type != null && type.IsEnum && typeof(StringEnumConverter).Equals(type.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType);

		/// <summary>
		/// Gets the state that determines the attribute's type is got Json.NET string enumeration or not
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool IsStringEnum(this AttributeInfo attribute)
			=> attribute != null && attribute.Type != null && attribute.Type.IsStringEnum();

		/// <summary>
		/// Gets the state to determines the object's type is got Json.NET string enumeration or not
		/// </summary>
		/// <param name="object"></param>
		/// <returns>true if type is numeric</returns>
		public static bool IsStringEnum(this object @object)
			=> @object != null && @object.GetType().IsStringEnum();

		/// <summary>
		/// Gets the state to determines the attribute's type is serializable (got 'Serializable' attribute) or not
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
		/// Gets the state to determines this object is nullable or not
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns></returns>
		public static bool IsNullable<T>(this T @object)
			=> @object != null && @object.GetType().IsNullable();
		#endregion

		#region Meta of collections
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
		/// <param name="type"></param>
		/// <returns></returns>
		public static List<Type> GetGenericTypeArguments(this Type type)
			=> type != null && type.IsGenericType ? type.GenericTypeArguments.ToList() : new List<Type>();

		/// <summary>
		/// Gets the generic type arguments of this attribute type
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static List<Type> GetGenericTypeArguments(this AttributeInfo attribute)
			=> attribute?.Type.GetGenericTypeArguments() ?? new List<Type>();

		/// <summary>
		/// Gets the generic type arguments of this object
		/// </summary>
		/// <param name="object">The type for processing</param>
		/// <returns></returns>
		public static List<Type> GetGenericTypeArguments(this object @object)
			=> @object?.GetType().GetGenericTypeArguments() ?? new List<Type>();

		/// <summary>
		/// Gets the first element of the generic type arguments of this type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns></returns>
		public static Type GetFirstGenericTypeArgument(this Type type)
			=> type != null && type.IsGenericType ? type.GenericTypeArguments.First() : null;

		/// <summary>
		/// Gets the first element of the generic type arguments of this attribute
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static Type GetFirstGenericTypeArgument(this AttributeInfo attribute)
			=> attribute?.Type.GetFirstGenericTypeArgument();

		/// <summary>
		/// Gets the first element of the generic type arguments of this object
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		public static Type GetFirstGenericTypeArgument(this object @object)
			=> @object?.GetType().GetFirstGenericTypeArgument();

		/// <summary>
		/// Gets the last element of the generic type arguments of this type
		/// </summary>
		/// <param name="type">The type for processing</param>
		/// <returns></returns>
		public static Type GetLastGenericTypeArgument(this Type type)
			=> type != null && type.IsGenericType ? type.GenericTypeArguments.Last() : null;

		/// <summary>
		/// Gets the last element of the generic type arguments of this attribute
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static Type GetLastGenericTypeArgument(this AttributeInfo attribute)
			=> attribute?.Type.GetLastGenericTypeArgument();

		/// <summary>
		/// Gets the last element of the generic type arguments of this object
		/// </summary>
		/// <param name="object"></param>
		/// <returns></returns>
		public static Type GetLastGenericTypeArgument(this object @object)
			=> @object?.GetType().GetLastGenericTypeArgument();

		/// <summary>
		/// Gets the state to determines the type is a generic list
		/// </summary>
		/// <param name="type"></param>
		/// <returns>true if the type is a generic list; otherwise false.</returns>
		public static bool IsGenericList(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(List<>));

		/// <summary>
		/// Gets the state to determines the attribute's type is a generic list
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
		/// <param name="type"></param>
		/// <returns>true if the type is a reference (or sub-class) of a generic hash-set; otherwise false.</returns>
		public static bool IsGenericHashSet(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(HashSet<>));

		/// <summary>
		/// Gets the state to determines the attribute's type is a generic hash-set
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
		/// <param name="type"></param>
		/// <returns>true if the type is a reference (or sub-class) of a generic list or generic hash-set; otherwise false.</returns>
		public static bool IsGenericListOrHashSet(this Type type)
			=> type != null && (type.IsGenericList() || type.IsGenericHashSet());

		/// <summary>
		/// Gets the state to determines the attribute's type is a generic list or generic hash-set
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
		/// <param name="type"></param>
		/// <returns>true if the type is a reference (or sub-class) of a generic dictionary; otherwise false.</returns>
		public static bool IsGenericDictionary(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(Dictionary<,>));

		/// <summary>
		/// Gets the state to determines the attribute's type is a generic dictionary
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
		/// <param name="type"></param>
		/// <returns>true if the type is sub-class of the generic collection class; otherwise false.</returns>
		public static bool IsGenericCollection(this Type type)
			=> type != null && type.IsGenericType && type.IsSubclassOfGeneric(typeof(Collection<,>));

		/// <summary>
		/// Gets the state to determines the attribute's type is a generic collection
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
		/// <param name="type"></param>
		/// <returns>true if the type is sub-class of the generic collection class; otherwise false.</returns>
		public static bool IsGenericDictionaryOrCollection(this Type type)
			=> type != null && (type.IsGenericDictionary() || type.IsGenericCollection());

		/// <summary>
		/// Gets the state to determines the attribute's type is a generic dictionary or a generic collection
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
		/// <param name="type"></param>
		/// <returns>true if the type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface; otherwise false.</returns>
		public static bool IsICollection(this Type type)
			=> type != null && (typeof(ICollection).IsAssignableFrom(type) || type.IsGenericHashSet());

		/// <summary>
		/// Gets the state to determines the attribute's type is reference of a class that is sub-class of the <see cref="ICollection">ICollection</see> interface
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
		/// <param name="type"></param>
		/// <returns>true if the type is is reference (or sub-class) of the the <see cref="System.Collections.Specialized.Collection">Collection</see> class; otherwise false.</returns>
		public static bool IsCollection(this Type type)
			=> type != null && typeof(System.Collections.Specialized.Collection).IsAssignableFrom(type);

		/// <summary>
		/// Gets the state to determines the attribute's type is reference (or sub-class) of the the <see cref="System.Collections.Specialized.Collection">Collection</see> class
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
		/// Gets the state to determines the attribute's type is is array or not
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

		#region Create new instance
		static ConcurrentDictionary<Type, Func<object>> TypeFactories { get; } = new ConcurrentDictionary<Type, Func<object>>();

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection
		/// </summary>
		/// <param name="type">The type to be created</param>
		/// <returns>The newly created instance</returns>
		public static object CreateInstance(this Type type)
		{
			if (!ObjectService.TypeFactories.TryGetValue(type, out var func) || func == null)
				lock (ObjectService.TypeFactories)
				{
					if (!ObjectService.TypeFactories.TryGetValue(type, out func) || func == null)
					{
						func = Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
						ObjectService.TypeFactories.TryAdd(type, func);
					}
				}
			return func();
		}

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns>The newly created instance</returns>
		public static T CreateInstance<T>(this Type type)
			=> (T)type.CreateInstance();

		/// <summary>
		/// Creates an instance of the specified type using a generated factory to avoid using Reflection
		/// </summary>
		/// <typeparam name="T">The type to be created</typeparam>
		/// <returns>The newly created instance</returns>
		public static T CreateInstance<T>()
			=> typeof(T).CreateInstance<T>();
		#endregion

		#region Object casts/conversions
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
		/// Casts the object to other type
		/// </summary>
		/// <param name="object">The object to cast to other type</param>
		/// <param name="type">The type to cast to</param>
		/// <returns></returns>
		public static object As(this object @object, Type type)
			=> @object?.CastAs(type);

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

		/// <summary>
		/// Casts the value to other type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object">The object to cast to other type</param>
		/// <returns></returns>
		public static T As<T>(this object @object)
			=> @object.CastAs<T>();
		#endregion

		#region Object manipulations
		/// <summary>
		/// Sets value of an attribute of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="name">The string that presents the name of the attribute need to get.</param>
		/// <param name="value">The object that presents the value of the attribute need to set.</param>
		/// <returns>true if success, otherwise false</returns>
		public static bool SetAttributeValue(this object @object, string name, object value)
		{
			if (@object == null || string.IsNullOrWhiteSpace(name))
				throw new ArgumentException(nameof(name));

			var attribute = @object.GetAttributes(attr => !attr.IsStatic && attr.CanWrite).FirstOrDefault(attr => attr.Name.IsEquals(name));
			if (attribute != null)
			{
				if (attribute.IsProperty)
					attribute.PropertyInfo.SetValue(@object, value);
				else
					attribute.FieldInfo.SetValue(@object, value);
				if (@object is IPropertyChangedNotifier notifier)
					notifier.NotifyPropertyChanged(name, @object);
			}
			return attribute != null;
		}

		/// <summary>
		/// Sets value of an attribute (public/private/static) of an object.
		/// </summary>
		/// <param name="object">The object need to get data from.</param>
		/// <param name="attribute">The object that presents information of the attribute need to get</param>
		/// <param name="value">The object that presents the value of the attribute need to set.</param>
		/// <param name="cast">true to cast the type of attribute</param>
		public static bool SetAttributeValue(this object @object, AttributeInfo attribute, object value, bool cast = false)
			=> @object != null && attribute != null
				? @object.SetAttributeValue(attribute.Name, cast && value != null ? value.CastAs(attribute.Type) : value)
				: throw new ArgumentException(nameof(attribute));

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

			try
			{
				var attribute = @object.GetAttributes(attr => !attr.IsStatic && attr.CanRead).FirstOrDefault(attr => attr.Name.IsEquals(name));
				return attribute?.FieldInfo?.GetValue(@object) ?? attribute?.PropertyInfo?.GetValue(@object);
			}
			catch
			{
				return null;
			}
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
			return value != null && value is T valueIsT ? valueIsT : @default;
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
			return value != null && value is T valueIsT ? valueIsT : @default;
		}

		/// <summary>
		/// Trims all string attributes
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <param name="onCompleted"></param>
		/// <returns></returns>
		public static T TrimAll<T>(this T @object, Action<T> onCompleted = null) where T : class
		{
			if (@object != null && @object.IsClassType())
				@object.GetPublicAttributes(attribute => !attribute.IsStatic && attribute.CanWrite && attribute.IsStringType()).ForEach(attribute =>
				{
					if (@object.GetAttributeValue(attribute) is string value)
						@object.SetAttributeValue(attribute, value.Trim());
				});
			onCompleted?.Invoke(@object);
			return @object;
		}

		/// <summary>
		/// Calls a method of this object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="info"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static object CallMethod(this object @object, MethodInfo info, object[] parameters = null)
			=> info?.Invoke(info.IsStatic ? null : @object, parameters);

		/// <summary>
		/// Calls a method of this object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="info"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static object CallMethod(this object @object, MetaInfo info, object[] parameters = null)
			=> @object?.CallMethod(info?.MethodInfo, parameters);

		/// <summary>
		/// Calls a method of this object
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		public static object CallMethod(this object @object, string name, object[] parameters = null)
			=> @object?.CallMethod(@object?.GetMethods().FirstOrDefault(info => info.Name == name && info.IsMethod)?.MethodInfo, parameters);

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
						tempValue = type.CreateInstance().CopyFrom(tempExpando);
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
			=> @object != null && @object.TryGet(name, out object value) ? value : null;

		/// <summary>
		/// Gets the value of an attribute of this <see cref="ExpandoObject">ExpandoObject</see> object (accept the dot (.) to get attribute of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute, accept the dot (.) to get attribute of child object</param>
		/// <param name="default">Default value when the attribute is not found</param>
		/// <returns>The value of an attribute (if the object got it); otherwise null.</returns>
		public static T Get<T>(this ExpandoObject @object, string name, T @default = default)
			=> @object != null && @object.TryGet(name, out T value) ? value : @default;

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

		/// <summary>
		/// Checks to see the <see cref="ExpandoObject">ExpandoObject</see> object is got an attribute by specified name (accept the dot (.) to get check of child object)
		/// </summary>
		/// <param name="object"></param>
		/// <param name="name">The string that presents the name of the attribute for checking, accept the dot (.) to get check of child object</param>
		/// <returns>true if the object got an attribute with the name</returns>
		public static bool Has(this ExpandoObject @object, string name)
			=> @object != null && (name.IndexOf(".") < 0 ? (@object as IDictionary<string, object>).ContainsKey(name) : @object.TryGet(name, out var value));
		#endregion

		#region Copy objects' properties from other object
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

			@object.GetPublicAttributes(attribute => !attribute.IsStatic && attribute.CanWrite && !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
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

			@object.GetPrivateAttributes(attribute => !attribute.IsStatic && !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
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
		public static T CopyFrom<T>(this T @object, object source, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			if (@object == null || source == null)
				throw new ArgumentNullException(nameof(source), "The source object is null");

			var excludedAttributes = new HashSet<string>(excluded ?? new HashSet<string>(), StringComparer.OrdinalIgnoreCase);

			@object.GetPublicAttributes(attribute => !attribute.IsStatic && attribute.CanWrite && !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
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

			@object.GetPrivateAttributes(attribute => !attribute.IsStatic && !excludedAttributes.Contains(attribute.Name)).ForEach(attribute =>
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
			return @object;
		}

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
			=> typeof(T).CreateInstance<T>().CopyFrom(@object, excluded, onCompleted, onError);

		/// <summary>
		/// Clones the object (perform a deep copy of the object)
		/// </summary>
		/// <typeparam name="T">The type of object being copied</typeparam>
		/// <param name="object">The object instance to copy</param>
		/// <param name="onCompleted">The action to run before completing the clone process</param>
		/// <param name="onError">The action to run when got any error</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(this T @object, Action<T> onCompleted = null, Action<Exception> onError = null)
			=> @object == null ? default : @object.Copy(null, onCompleted, onError);
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
		public static T CopyFrom<T>(this T @object, JToken json, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			if (@object == null || json == null)
				throw new ArgumentNullException(nameof(json), "The objects were null");

			var serializer = new JsonSerializer();
			var excludedAttributes = new HashSet<string>(excluded ?? new HashSet<string>(), StringComparer.OrdinalIgnoreCase);

			foreach (var attribute in @object.GetPublicAttributes(attribute => !attribute.IsStatic && attribute.CanWrite && !excludedAttributes.Contains(attribute.Name)))
			{
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
						var dataType = attribute.GetFirstGenericTypeArgument();

						if (dataType.IsClassType() && attribute.GetAsObjectAttribute() != null && token is JObject jobject)
						{
							data = new JArray();
							if (jobject.Count > 0)
							{
								var gotSpecialAttributes = dataType.GetSpecialSerializeAttributes().Count > 0;
								foreach (var item in jobject)
									if (gotSpecialAttributes)
										data.Add(JObject.FromObject(dataType.CreateInstance().CopyFrom(item.Value)));
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
						var dataType = attribute.GetLastGenericTypeArgument();

						if (dataType.IsClassType() && attribute.GetAsArrayAttribute() != null && token is JArray jarray)
						{
							data = new JObject();
							if (jarray.Count > 0)
							{
								var asArray = attribute.GetAsArrayAttribute();
								var keyAttribute = !string.IsNullOrWhiteSpace(asArray.KeyAttribute)
									? asArray.KeyAttribute
									: "ID";
								var gotSpecialAttributes = dataType.GetSpecialSerializeAttributes().Count > 0;
								foreach (JObject item in jarray)
									if (gotSpecialAttributes)
									{
										var child = dataType.CreateInstance().CopyFrom(item);
										var keyValue = child.GetAttributeValue(keyAttribute);
										if (keyValue != null)
											data.Add(keyValue.ToString(), JObject.FromObject(child));
									}
									else
									{
										var keyValue = item[keyAttribute];
										if (keyValue != null && keyValue is JValue jvalue && jvalue.Value != null)
											data.Add(jvalue.Value.ToString(), item);
									}
							}
						}
						else
							data = token as JObject;

						// update
						var type = attribute.Type.IsGenericDictionary()
							? typeof(Dictionary<,>).MakeGenericType(attribute.GetFirstGenericTypeArgument(), dataType)
							: typeof(Collection<,>).MakeGenericType(attribute.GetFirstGenericTypeArgument(), dataType);
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
						@object.SetAttributeValue(attribute, token is JObject jobject && jobject.Count > 0 ? serializer.Deserialize(new JTokenReader(token), typeof(System.Collections.Specialized.Collection)) : typeof(System.Collections.Specialized.Collection).CreateInstance());
					}
					catch (Exception ex)
					{
						onError?.Invoke(ex);
					}

				// enum
				else if (attribute.Type.IsEnum && token is JValue jvalue && jvalue.Value != null)
					try
					{
						@object.SetAttributeValue(attribute, jvalue.Value.ToString().ToEnum(attribute.Type));
					}
					catch { }

				// class
				else if (attribute.IsClassType())
					try
					{
						var instance = attribute.Type.CreateInstance();
						if (token is JObject jobject && jobject.Count > 0)
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
			return @object;
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
			=> typeof(T).CreateInstance<T>().CopyFrom(json, excluded, onCompleted, onError);
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
		public static T CopyFrom<T>(this T @object, ExpandoObject expandoObject, HashSet<string> excluded = null, Action<T> onCompleted = null, Action<Exception> onError = null)
		{
			@object?.GetPublicAttributes(attribute => !attribute.IsStatic && attribute.CanWrite && (excluded == null || !excluded.Contains(attribute.Name))).ForEach(attribute =>
			{
				// by-pass
				if (!expandoObject.TryGet(attribute.Name, out var value))
					return;

				// normalize the value
				if (value != null)
				{
					// generic list/hash-set
					if (value is List<object> values && attribute.IsGenericListOrHashSet())
						value = attribute.IsGenericList()
							? values.ToList(attribute.GetFirstGenericTypeArgument())
							: values.ToHashSet(attribute.GetFirstGenericTypeArgument());

					// generic dictionary/collection
					else if (value is ExpandoObject expando && attribute.IsGenericDictionaryOrCollection())
						value = attribute.IsGenericDictionary()
							? expando.ToDictionary(attribute.GetFirstGenericTypeArgument(), attribute.GetLastGenericTypeArgument())
							: expando.ToCollection(attribute.GetFirstGenericTypeArgument(), attribute.GetLastGenericTypeArgument());

					// class/array
					else if (value is ExpandoObject expandoObj && attribute.IsClassType() && !attribute.Type.Equals(typeof(ExpandoObject)))
						value = attribute.Type.CreateInstance().CopyFrom(expandoObj);

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
							var exception = new InvalidCastException($"Cannot cast type of \"{attribute.Name}\" ({value.GetTypeName(true)} => {attribute.GetTypeName(true)}): {ex.Message}", ex);
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
			});
			onCompleted?.Invoke(@object);
			return @object;
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
			=> typeof(T).CreateInstance<T>().CopyFrom(expandoObject, excluded, onCompleted, onError);
		#endregion

		#region JSON conversions
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
			if (@object is JToken jsonObj)
			{
				onCompleted?.Invoke(jsonObj);
				return jsonObj;
			}

			// generate
			JToken json = null;

			// array or generict list/hash-set
			if (@object.IsArray() || @object.IsGenericListOrHashSet())
			{
				var type = @object.IsArray() ? @object.GetType().GetElementType() : @object.GetFirstGenericTypeArgument();
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
				if (@object.GetLastGenericTypeArgument().IsClassType())
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
			else if (@object.IsClassType())
			{
				json = JObject.FromObject(@object);
				@object.GetSpecialSerializeAttributes().ForEach(attribute =>
				{
					if (attribute.IsGenericListOrHashSet() && attribute.GetFirstGenericTypeArgument().IsClassType() && attribute.GetAsObjectAttribute() != null)
					{
						var jsonObject = new JObject();
						if (@object.GetAttributeValue(attribute.Name) is IEnumerable items)
						{
							var asObject = attribute.GetAsObjectAttribute();
							var keyAttribute = !string.IsNullOrWhiteSpace(asObject.KeyAttribute) ? asObject.KeyAttribute : "ID";
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
					else if (attribute.IsGenericDictionaryOrCollection() && attribute.GetLastGenericTypeArgument().IsClassType() && attribute.GetAsArrayAttribute() != null)
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
			var type = typeof(T);
			var @object = type.IsPrimitiveType() && json is JValue value
				? value.Value != null ? value.Value.CastAs<T>() : default
				: (copy || type.GotSpecialSerializeAttributes()) && type.IsClassType() && !type.IsGenericListOrHashSet() && !type.IsGenericDictionaryOrCollection()
					? type.CreateInstance<T>().CopyFrom(json)
					: new JsonSerializer().Deserialize<T>(new JTokenReader(json));
			onCompleted?.Invoke(@object, json);
			return @object;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON object that contains information for deserializing</param>
		/// <param name="copy">true to create new instance and copy data; false to deserialize object</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T As<T>(this JToken json, bool copy = false, Action<T, JToken> onCompleted = null)
			=> json != null ? json.FromJson(copy, onCompleted) : default;

		/// <summary>
		/// Creates (Deserializes) an object from this JSON object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="json">The JSON string that contains information for deserializing</param>
		/// <param name="copy">true to create new instance and copy data; false to deserialize object</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T FromJson<T>(this string json, bool copy = false, Action<T, JToken> onCompleted = null)
			=> string.IsNullOrWhiteSpace(json) ? default : json.ToJson().As(copy, onCompleted);

		/// <summary>
		/// Gets the <see cref="JToken">JToken</see> with the specified key converted to the specified type
		/// </summary>
		/// <typeparam name="T">The type to convert the token to</typeparam>
		/// <param name="json"></param>
		/// <param name="key">The token key</param>
		/// <param name="default">The default value</param>
		/// <returns>The converted token value</returns>
		public static T Get<T>(this JToken json, string key, T @default = default)
		{
			var value = @default;
			if (json != null)
				try
				{
					var jsonvalue = json.Value<object>(key);
					value = jsonvalue != null
						? jsonvalue is T valueIsT ? valueIsT : jsonvalue.CastAs<T>()
						: @default;
				}
				catch
				{
					value = @default;
				}
			return value;
		}
		#endregion

		#region ExpandoObject conversions
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
		/// Creates an <see cref="ExpandoObject">ExpandoObject</see> object from this object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="object"></param>
		/// <returns>An <see cref="ExpandoObject">ExpandoObject</see> object</returns>
		public static ExpandoObject ToExpandoObject<T>(this T @object, Action<ExpandoObject> onCompleted = null)
			=> (@object is JToken json ? json : @object?.ToJson())?.ToExpandoObject(onCompleted);

		/// <summary>
		/// Creates (Deserializes) an object from this <see cref="ExpandoObject">ExpandoObject</see> object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expando"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T FromExpandoObject<T>(this ExpandoObject expando, Action<T, ExpandoObject> onCompleted = null)
		{
			var @object = JObject.FromObject(expando).As<T>();
			onCompleted?.Invoke(@object, expando);
			return @object;
		}

		/// <summary>
		/// Creates (Deserializes) an object from this <see cref="ExpandoObject">ExpandoObject</see> object
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="expando"></param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T As<T>(this ExpandoObject expando, Action<T, ExpandoObject> onCompleted = null)
			=> expando != null ? expando.FromExpandoObject(onCompleted) : default;
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
			var xml = @object == null ? null : JsonConvert.DeserializeXNode(@object.ToString(), rootElementName)?.Root;
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
		/// Creates (Deserializes) an object from this XML object
		/// </summary>
		/// <typeparam name="T">Type of the object</typeparam>
		/// <param name="xml">The XML object that contains information for deserializing</param>
		/// <param name="onCompleted">The action to run when the conversion process is completed</param>
		/// <returns></returns>
		public static T As<T>(this XContainer xml, Action<T, XContainer> onCompleted = null) where T : class
			=> xml.FromXml(onCompleted);

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