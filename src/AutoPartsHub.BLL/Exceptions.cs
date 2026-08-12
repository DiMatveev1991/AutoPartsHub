namespace AutoPartsHub.BLL;

public class AppException : Exception
{
    public AppException(string message)
        : base(message)
    {
    }

    public AppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class NotFoundException(string message) : AppException(message);

public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class AuthenticationException(string message) : AppException(message);

public sealed class ForbiddenException(string message) : AppException(message);
