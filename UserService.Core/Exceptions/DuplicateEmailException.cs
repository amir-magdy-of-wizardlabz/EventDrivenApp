namespace UserService.Core.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email, Exception? inner = null)
            : base($"A user with the email '{email}' already exists.", inner)
        {
        }
    }
}
