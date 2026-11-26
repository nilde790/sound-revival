namespace SoundRevival.Core.Exceptions;

public sealed class UniqueConstraintViolationException : Exception
{
    public UniqueConstraintViolationException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
