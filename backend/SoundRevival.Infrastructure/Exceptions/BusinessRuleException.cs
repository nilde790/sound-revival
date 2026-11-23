namespace SoundRevival.Infrastructure.Exceptions
{
    public class BusinessRuleException : AppException
    {
        public BusinessRuleException(string message) : base(message, 400)
        {
        }
    }
}
