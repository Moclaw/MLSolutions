using Shard.Entities;

namespace Domain.Entities;

/// <summary>
/// Represents the base class for all entities in the domain.
/// </summary>
/// <typeparam name="TKey">The type of the unique identifier for the entity.</typeparam>
public abstract class BaseEntity<TKey> : TrackEntity, IEntity<TKey>
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public TKey Id { get; set; } = default!;
}
