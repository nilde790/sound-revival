namespace SoundRevival.Repository.Exceptions
{
    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException(string message) : base(message, 401)
        {
        }
    }
}
