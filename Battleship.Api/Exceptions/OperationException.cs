using System;

namespace Battleship.Api.Exceptions
{
    public class OperationException : Exception
    {
        public OperationException(ErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public OperationException(ErrorCode errorCode, string message, Exception inner)
            : base(message, inner)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; }
    }
}
