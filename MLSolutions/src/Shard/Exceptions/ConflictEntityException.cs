using System.Net;

namespace Shard.Exceptions;

public class ConflictEntityException : Exception
{
    public ConflictEntityException(string message) : base(message)
    {
        HResult = (int)HttpStatusCode.Conflict;
    }
}
