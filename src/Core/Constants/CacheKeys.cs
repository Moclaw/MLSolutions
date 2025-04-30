namespace Core.Constants;
public enum CacheType
{
    InMemory = 0,
    Redis = 1
}

public enum CacheKeySearchOperator
{
    StartsWith = 0,
    EndsWith = 1,
    Contains = 2,
}