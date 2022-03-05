using System;
using System.Net;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace net.vieapps.Components.Utility
{
	[Serializable]
	public class AppException : Exception
	{
		public AppException() : base() { }

		public AppException(string message) : base (message) { }

		public AppException(string message, Exception innerException) : base (message, innerException) { }

		public AppException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InvalidRequestException : AppException
	{
		public InvalidRequestException() : base("Request is invalid") { }

		public InvalidRequestException(string message) : base(message) { }

		public InvalidRequestException(Exception innerException) : base("Request is invalid", innerException) { }

		public InvalidRequestException(string message, Exception innerException) : base(message, innerException) { }

		public InvalidRequestException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InvalidAppOperationException : AppException
	{
		public InvalidAppOperationException() : base("Operation is invalid") { }

		public InvalidAppOperationException(string message) : base(message) { }

		public InvalidAppOperationException(Exception innerException) : base("Operation is invalid", innerException) { }

		public InvalidAppOperationException(string message, Exception innerException) : base(message, innerException) { }

		public InvalidAppOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InformationRequiredException : AppException
	{
		public InformationRequiredException () : base("Information is required") { }

		public InformationRequiredException (string message) : base(message) { }

		public InformationRequiredException(Exception innerException) : base("Information is required", innerException) { }

		public InformationRequiredException (string message, Exception innerException) : base(message, innerException) { }

		public InformationRequiredException (SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InformationInvalidException : AppException
	{
		public InformationInvalidException() : base("Information is invalid") { }

		public InformationInvalidException(string message) : base(message) { }

		public InformationInvalidException(Exception innerException) : base("Information is invalid", innerException) { }

		public InformationInvalidException(string message, Exception innerException) : base(message, innerException) { }

		public InformationInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InformationExistedException : AppException
	{
		public InformationExistedException() : base("Information is existed") { }

		public InformationExistedException(string message) : base(message) { }

		public InformationExistedException(Exception innerException) : base("Information is existed", innerException) { }

		public InformationExistedException (string message, Exception innerException) : base(message, innerException) { }

		public InformationExistedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InformationNotFoundException : AppException
	{
		public InformationNotFoundException() : base("Information is not found") { }
		
		public InformationNotFoundException(string message) : base(message) { }

		public InformationNotFoundException(Exception innerException) : base("Information is not found", innerException) { }

		public InformationNotFoundException(string message, Exception innerException) : base(message, innerException) { }

		public InformationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class DatabaseOperationException : AppException
	{
		public DatabaseOperationException() : base("Error occured while operating with database") { }

		public DatabaseOperationException(string message) : base(message) { }

		public DatabaseOperationException(Exception innerException) : base("Error occured while operating with database: " + innerException.Message, innerException) { }

		public DatabaseOperationException(string message, Exception innerException) : base(message, innerException) { }

		public DatabaseOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RepositoryOperationException : AppException
	{
		public RepositoryOperationException() : base("Error occured while performing an action of repository") { }

		public RepositoryOperationException(string message) : base(message) { }

		public RepositoryOperationException(Exception innerException) : base($"Error occured while operating with repository: {innerException?.Message}", innerException) { }

		public RepositoryOperationException(string message, Exception innerException) : base(message, innerException) { }

		public RepositoryOperationException(string message, string info, Exception innerException) : base(message, innerException)
		{
			this.Info = info;
		}

		public RepositoryOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public string Info { get; set; } = "";
	}

	[Serializable]
	public class ServiceOperationException : AppException
	{
		public ServiceOperationException() : base("Error occured while operating with service") { }

		public ServiceOperationException(string message) : base(message) { }

		public ServiceOperationException(Exception innerException) : base($"Error occured while operating with service: {innerException?.Message}", innerException) { }

		public ServiceOperationException(string message, Exception innerException) : base(message, innerException) { }

		public ServiceOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class ServiceNotFoundException : AppException
	{
		public ServiceNotFoundException() : base("The requested service is not found") { }

		public ServiceNotFoundException(string message) : base(message) { }

		public ServiceNotFoundException(Exception innerException) : base($"The requested service is not found: {innerException?.Message}", innerException) { }

		public ServiceNotFoundException(string message, Exception innerException) : base(message, innerException) { }

		public ServiceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RemoteServerException : AppException
	{
		public HttpStatusCode StatusCode { get; internal set; } = HttpStatusCode.InternalServerError;

		public bool IsSuccessStatusCode { get; internal set; } = false;

		public Uri URI { get; internal set; }

		public Dictionary<string, string> Headers { get; internal set; }

		public string Body { get; internal set; }

		public RemoteServerException() : base("Error occured while operating with remote server") { }

		public RemoteServerException(HttpStatusCode statusCode, bool isSuccessStatusCode, Uri uri, Dictionary<string, string> headers, string body, string message = null) : base(message ?? $"[HTTP {(int)statusCode}]: Error occurred while operating with the remote server")
		{
			this.StatusCode = statusCode;
			this.IsSuccessStatusCode = isSuccessStatusCode;
			this.URI = uri;
			this.Headers = headers;
			this.Body = body;
		}

		public RemoteServerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RemoteServerMovedException : RemoteServerException
	{
		public RemoteServerMovedException(HttpStatusCode statusCode, Uri uri, string message = null) : base(statusCode, true, uri, null, null, message ?? $"Remote server was moved [{uri}]") { }
	}

	[Serializable]
	public class ConnectionTimeoutException : AppException
	{
		public ConnectionTimeoutException() : base("Connection timeout") { }

		public ConnectionTimeoutException(Exception innerException) : base("Connection timeout", innerException) { }

		public ConnectionTimeoutException(string message, Exception innerException) : base(message, innerException) { }

		public ConnectionTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class MethodNotAllowedException : AppException
	{
		public MethodNotAllowedException() : base("The method is not allowed") { }

		public MethodNotAllowedException(string method) : base($"The method is not allowed: {method}") { }

		public MethodNotAllowedException(Exception innerException) : base("The method is not allowed", innerException) { }

		public MethodNotAllowedException(string message, Exception innerException) : base(message, innerException) { }

		public MethodNotAllowedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}