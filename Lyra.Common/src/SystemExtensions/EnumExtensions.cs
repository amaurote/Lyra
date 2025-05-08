using System.ComponentModel;
using System.Reflection;

namespace Lyra.Common.SystemExtensions;

public static class EnumExtensions
{
    public static string Description(this Enum value)
    {
        return value
                   .GetType()
                   .GetField(value.ToString())?
                   .GetCustomAttribute<DescriptionAttribute>()?
                   .Description
               ?? ToDisplayString(value);
    }

    public static string ToDisplayString(this Enum value)
    {
        return System.Text.RegularExpressions.Regex
            .Replace(value.ToString(), "(\\B[A-Z])", " $1");
    }
}