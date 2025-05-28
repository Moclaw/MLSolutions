
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MLSolutions;

/// <summary>
/// Provides extension methods for working with enums, including retrieving display names, descriptions, and metadata.
/// </summary>
public static partial class EnumExtensions
{
    /// <summary>
    /// Retrieves the display name of an enum value using the <see cref="DisplayAttribute"/>.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The display name if defined; otherwise, the enum value as a string.</returns>
    public static string? GetDisplayName(this Enum value)
    {
        var displayName = value.ToString();
        var fieldInfo = value.GetType().GetField(displayName);
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

        return attributes?.Length > 0 ? attributes[0].Description : value.ToString();
    }

    /// <summary>
    /// Retrieves the description of an enum value using the <see cref="DescriptionAttribute"/>.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>The description if defined; otherwise, the enum value as a string.</returns>
    public static string? GetDescription(this Enum value)
    {
        var displayName = value.ToString();
        var fieldInfo = value.GetType().GetField(displayName);
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        return attributes?.Length > 0 ? attributes[0].Description : value.ToString();
    }

    /// <summary>
    /// Retrieves a list of display names for all values of a specified enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="enumType">The type of the enum.</param>
    /// <returns>A list of display names for the enum values.</returns>
    public static List<string> GetDisplayNames<TEnum>(this Type enumType) where TEnum : Enum => Enum.GetValues(enumType)
            .Cast<TEnum>()
            .Select(x => x.GetDisplayName())
            .ToList()!;

    /// <summary>
    /// Retrieves a list of descriptions for all values of a specified enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="enumType">The type of the enum.</param>
    /// <returns>A list of descriptions for the enum values.</returns>
    public static List<string> GetDescriptions<TEnum>(this Type enumType) where TEnum : Enum => Enum.GetValues(enumType)
            .Cast<TEnum>()
            .Select(x => x.GetDescription())
            .ToList()!;

    /// <summary>
    /// Converts an enum type to a read-only list of metadata, including name, display name, description, and value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <returns>
    /// A read-only list of tuples containing the name, display name, description, and integer value of each enum value.
    /// </returns>
    public static IReadOnlyList<(string? Name, string? DisplayName, string? Description, int Value)> ToReadOnlyListWithMetaData<TEnum>() where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum));

        return (
                from object? value in values
                select (Enum.GetName(typeof(TEnum), value),
                    ((TEnum)value).GetDisplayName(),
                    ((TEnum)value).GetDescription(),
                    (int)value)
                ).ToList();
    }
}
