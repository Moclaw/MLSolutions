namespace Shared.Entities;

/// <summary>
/// Represents a base interface for all entities.
/// </summary>
public interface IEntity { }

/// <summary>
/// Represents a generic interface for entities with a strongly-typed identifier.
/// </summary>
/// <typeparam name="TKey">The type of the identifier.</typeparam>
public interface IEntity<TKey> : IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    TKey Id { get; set; }
}
