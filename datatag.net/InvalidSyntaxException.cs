namespace Datatag;

public class InvalidSyntaxException : Exception
{
    public InvalidSyntaxException() : base(null) { }
    public InvalidSyntaxException(string? message) : base(message) { }
}
