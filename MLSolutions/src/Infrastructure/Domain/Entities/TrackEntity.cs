namespace Domain.Entities;

/// <summary>
/// Represents a base entity that tracks creation, update, and deletion metadata.
/// </summary>
public abstract class TrackEntity
{
    /// <summary>
    /// Gets or sets the creation date of the entity.
    /// This property is automatically set when the entity is created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date of the entity.
    /// This property is updated whenever the entity is modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is marked as deleted.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date when the entity was deleted.
    /// This property is <c>null</c> if the entity has not been deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
