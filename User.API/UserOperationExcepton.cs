using System;

namespace User.API
{
    public class UserOperationExcepton : Exception
    {
        public UserOperationExcepton() { }

        public UserOperationExcepton(string message) : base(message) { }

        public UserOperationExcepton(string message, Exception innerException) : base(message, innerException) { }
    }
}
