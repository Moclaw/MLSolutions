
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MLSolutions;

public static partial class EnumExtensions
{
    public static string? GetDisplayName(this Enum value)
    {
        var displayName = value.ToString();
        var fieldInfo = value.GetType().GetField(displayName);
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

        return attributes?.Length > 0 ? attributes[0].Description : value.ToString();
    }

    public static string? GetDescription(this Enum value)
    {
        var displayName = value.ToString();
        var fieldInfo = value.GetType().GetField(displayName);
        var attributes = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        return attributes?.Length > 0 ? attributes[0].Description : value.ToString();
    }

    public static List<string> GetDisplayNames<TEnum>(this Type enumType) where TEnum : Enum
    {
        return Enum.GetValues(enumType)
            .Cast<TEnum>()
            .Select(x => x.GetDisplayName())
            .ToList()!;
    }

    public static List<string> GetDescriptions<TEnum>(this Type enumType) where TEnum : Enum
    {
        return Enum.GetValues(enumType)
            .Cast<TEnum>()
            .Select(x => x.GetDescription())
            .ToList()!;
    }

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
