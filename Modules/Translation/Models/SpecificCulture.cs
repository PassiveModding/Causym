using System.Globalization;

namespace Causym.Translation
{
    public class SpecificCulture
    {
        public SpecificCulture(CultureInfo culture)
        {
            BaseCulture = culture;
            string specName = "(none)";
            try
            {
                specName = CultureInfo.CreateSpecificCulture(BaseCulture.Name).Name;
            }
            catch
            {
            }

            SpecificName = specName;
        }

        public CultureInfo BaseCulture { get; }

        public string SpecificName { get; }
    }
}
