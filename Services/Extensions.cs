using System.Linq;

namespace Causym
{
    public static class Extensions
    {
        public static string FixLength(this string value, int length = 1023)
        {
            if (value.Length > length)
            {
                value = value.Substring(0, length - 3) + "...";
            }

            return value;
        }

        /*
        public static int Length(this Disqord.LocalEmbedBuilder builder)
        {
            int titleLength = builder.Title?.Length ?? 0;
            int authorLength = builder.Author?.Name?.Length ?? 0;
            int descriptionLength = builder.Description?.Length ?? 0;
            int footerLength = builder.Footer?.Text?.Length ?? 0;
            int fieldSum = builder.Fields?.Sum(x => x.Name?.Length + x.Value?.Length) ?? 0;
            return titleLength + authorLength + descriptionLength + footerLength + fieldSum;
        }
        */
    }
}
