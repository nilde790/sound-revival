namespace SoundRevival.Repository.Exceptions
{
    public class BusinessRuleException : AppException
    {
        public BusinessRuleException(string message) : base(message, 400)
        {
        }
    }
}
