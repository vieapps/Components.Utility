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

	[Serializable]
	public class InvalidSessionException : AppException
	{
		public InvalidSessionException() : base("Session is invalid") { }

		public InvalidSessionException(string message) : base(message) { }

		public InvalidSessionException(Exception innerException) : base("Session is invalid", innerException) { }

		public InvalidSessionException(string message, Exception innerException) : base(message, innerException) { }

		public InvalidSessionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class SessionNotFoundException : AppException
	{
		public SessionNotFoundException() : base("Session is not found") { }

		public SessionNotFoundException(string message) : base(message) { }

		public SessionNotFoundException(string message, Exception innerException) : base (message, innerException) { }

		public SessionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class SessionInformationRequiredException : AppException
	{
		public SessionInformationRequiredException() : base("Required information of the session is not found") { }

		public SessionInformationRequiredException(string message) : base(message) { }

		public SessionInformationRequiredException(Exception innerException) : base("Required information of the session is not found", innerException) { }

		public SessionInformationRequiredException(string message, Exception innerException) : base(message, innerException) { }

		public SessionInformationRequiredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class SessionExpiredException : AppException
	{
		public SessionExpiredException() : base("Session is expired") { }

		public SessionExpiredException(string message) : base(message) { }

		public SessionExpiredException(string message, Exception innerException) : base (message, innerException) { }

		public SessionExpiredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InvalidTokenException : AppException
	{
		public InvalidTokenException() : base("Token is invalid") { }

		public InvalidTokenException(string message) : base(message) { }

		public InvalidTokenException(Exception innerException) : base("Token is invalid", innerException) { }

		public InvalidTokenException(string message, Exception innerException) : base(message, innerException) { }

		public InvalidTokenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class TokenNotFoundException : AppException
	{
		public TokenNotFoundException() : base("Token is not found") { }

		public TokenNotFoundException(string message) : base(message) { }

		public TokenNotFoundException(string message, Exception innerException) : base (message, innerException) { }

		public TokenNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class TokenExpiredException : AppException
	{
		public TokenExpiredException() : base("Token is expired") { }

		public TokenExpiredException(string message) : base(message) { }

		public TokenExpiredException(string message, Exception innerException) : base (message, innerException) { }

		public TokenExpiredException(Exception innerException) : base("Token is expired", innerException) { }

		public TokenExpiredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class TokenRevokedException : AppException
	{
		public TokenRevokedException() : base("The access token has been revoked") { }

		public TokenRevokedException(string message) : base(message) { }

		public TokenRevokedException(string message, Exception innerException) : base(message, innerException) { }

		public TokenRevokedException(Exception innerException) : base("The access token has been revoked", innerException) { }

		public TokenRevokedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InvalidTokenSignatureException : AppException
	{
		public InvalidTokenSignatureException() : base("Token signature is invalid") { }

		public InvalidTokenSignatureException(string message) : base(message) { }

		public InvalidTokenSignatureException(Exception innerException) : base("Token signature is invalid", innerException) { }

		public InvalidTokenSignatureException(string message, Exception innerException) : base(message, innerException) { }

		public InvalidTokenSignatureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class UnauthorizedException : AppException
	{
		public UnauthorizedException() : base("Unauthorized. Sign-in please!") { }

		public UnauthorizedException(string message) : base(message) { }

		public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }

		public UnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class AccessDeniedException : AppException
	{
		public AccessDeniedException() : base("Sorry! You don't have enough permission to complete this request!") { }

		public AccessDeniedException(string message) : base(message) { }

		public AccessDeniedException(string message, Exception innerException) : base(message, innerException) { }

		public AccessDeniedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class WrongAccountException : AppException
	{
		public WrongAccountException() : base("Wrong account or password") { }

		public WrongAccountException(string message) : base(message) { }

		public WrongAccountException(Exception innerException) : base("Wrong account or password", innerException) { }

		public WrongAccountException(string message, Exception innerException) : base(message, innerException) { }

		public WrongAccountException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class LockedAccountException : AppException
	{
		public LockedAccountException() : base("Account is locked") { }

		public LockedAccountException(string message) : base(message) { }

		public LockedAccountException(Exception innerException) : base("Account is locked", innerException) { }

		public LockedAccountException(string message, Exception innerException) : base(message, innerException) { }

		public LockedAccountException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InvalidActivateInformationException : AppException
	{
		public InvalidActivateInformationException() : base("The information for activating is invalid") { }

		public InvalidActivateInformationException(string message) : base(message) { }

		public InvalidActivateInformationException(Exception innerException) : base("The information for activating is invalid", innerException) { }

		public InvalidActivateInformationException(string message, Exception innerException) : base(message, innerException) { }

		public InvalidActivateInformationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class ActivateInformationExpiredException : AppException
	{
		public ActivateInformationExpiredException() : base("The information for activating is expired") { }

		public ActivateInformationExpiredException(string message) : base(message) { }

		public ActivateInformationExpiredException(Exception innerException) : base("The information for activating is expired", innerException) { }

		public ActivateInformationExpiredException(string message, Exception innerException) : base(message, innerException) { }

		public ActivateInformationExpiredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

}