namespace SIGID.Domain.Exceptions;

public class AuthenticationException : Exception
{
    public AuthenticationException() : base("Authentication failed") { }
    public AuthenticationException(string message) : base(message) { }
    public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
}

public class InvalidCredentialsException : AuthenticationException
{
    public InvalidCredentialsException() : base("Invalid username or password") { }
    public InvalidCredentialsException(string message) : base(message) { }
}

public class UserNotFoundException : AuthenticationException
{
    public UserNotFoundException() : base("User not found") { }
    public UserNotFoundException(string username) : base($"User '{username}' not found") { }
}