namespace Furion.Application;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength)
    {
        return value?.Length > maxLength ? value[..maxLength] + "..." : value;
    }
}