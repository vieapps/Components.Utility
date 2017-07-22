using System;
using System.Runtime.Serialization;

namespace net.vieapps.Components.Utility
{
	[Serializable]
	public abstract class AppException : Exception
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

		public InformationInvalidException(string message, Exception innerException) : base(message, innerException) { }

		public InformationInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InformationExistedException : AppException
	{
		public InformationExistedException(string message) : base(message) { }

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

		public DatabaseOperationException(string message, Exception innerException) : base(message, innerException) { }

		public DatabaseOperationException(Exception innerException) : base("Error occured while operating with database: " + innerException.Message, innerException) { }

		public DatabaseOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RepositoryOperationException : AppException
	{
		public RepositoryOperationException() : base("Error occured while operating with repository") { }

		public RepositoryOperationException(string message) : base(message) { }

		public RepositoryOperationException(string message, Exception innerException) : base(message, innerException) { }

		public RepositoryOperationException(Exception innerException) : base("Error occured while operating with repository: " + innerException.Message, innerException) { }

		public RepositoryOperationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RemoteServerErrorException : AppException
	{
		public string ResponseBody { get; internal set; }

		public string ResponseUri { get; internal set; }

		public RemoteServerErrorException() : base("Error occured while operating with remote server") { }

		public RemoteServerErrorException(Exception innerException) : base("Error occurred while operating with the remote server", innerException) { }

		public RemoteServerErrorException(string message, Exception innerException) : base(message, innerException) { }

		public RemoteServerErrorException(string message, string responseBody, Exception innerException) : this(message, responseBody, "", innerException) { }

		public RemoteServerErrorException(string message, string responseBody, string responseUri, Exception innerException) : base(message, innerException)
		{
			this.ResponseBody = responseBody;
			this.ResponseUri = responseUri;
		}

		public RemoteServerErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class ConnectionTimeoutException : AppException
	{
		public ConnectionTimeoutException() : base("Connection timeout") { }

		public ConnectionTimeoutException(Exception innerException) : base("Connection timeout", innerException) { }

		public ConnectionTimeoutException(string message, Exception innerException) : base(message, innerException) { }

		public ConnectionTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}