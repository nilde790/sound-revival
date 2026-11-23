namespace SoundRevival.Infrastructure.Exceptions
{
    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException(string message) : base(message, 401)
        {
        }
    }
}
