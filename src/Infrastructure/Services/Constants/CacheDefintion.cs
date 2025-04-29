namespace Services.Constants;
public abstract record CacheDefintion(CacheType CType)
{
}

public sealed record MemoryCacheDefintion() : CacheDefintion(CacheType.InMemory)
{
}

public sealed record RedisCacheDefintion(string Connection) : CacheDefintion(CacheType.Redis)
{
}