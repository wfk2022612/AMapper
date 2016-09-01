using System.Linq;

namespace FastMapper
{
    public static class StringExterntion
    {
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return value == null || value.All(char.IsWhiteSpace);
        }
    }
}
