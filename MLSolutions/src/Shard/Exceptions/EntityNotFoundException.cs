using System.Net;

namespace Shard.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string message) : base(message)
    {
        HResult = (int)HttpStatusCode.NoContent;
    }
}
