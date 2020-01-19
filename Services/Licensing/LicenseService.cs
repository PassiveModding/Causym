using System;

namespace Causym.Licensing
{
    public class LicenseService
    {
        private static readonly Random Random = new Random();

        public static string GenerateRandomKey()
        {
            return $"{GenerateRandomNo()}-{GenerateRandomNo()}-{GenerateRandomNo()}-{GenerateRandomNo()}";
        }

        private static string GenerateRandomNo()
        {
            return Random.Next(0, 9999).ToString("D4");
        }
    }
}
