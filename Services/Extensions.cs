using Causym.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

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
    }
}
