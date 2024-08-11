using System;

namespace UserService.Infrastructure.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException()
        {
        }

        public DuplicateEmailException(string message)
            : base(message)
        {
        }

        public DuplicateEmailException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
