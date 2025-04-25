using System.Net;

namespace Shard.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
        HResult = (int)HttpStatusCode.BadRequest;
    }
}
