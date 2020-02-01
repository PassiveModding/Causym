using System;
using System.Collections.Generic;
using System.Reflection;
using Causym.Services;
using Disqord;

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

        public static IEnumerable<Type> GetServices(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ServiceAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        public static Color? ColorConvert(string colorHex)
        {
            if (!colorHex.StartsWith('#'))
            {
                colorHex = "#" + colorHex;
            }

            if (colorHex.Length != 7)
            {
                return null;
            }

            var sysColor = System.Drawing.ColorTranslator.FromHtml(colorHex);
            var disColor = new Color(sysColor.R, sysColor.G, sysColor.B);
            return disColor;
        }
    }
}
