using System;
using System.Collections.Generic;
using System.Linq;

namespace Causym.Licensing.Quantifiable
{
    public class License
    {
        // Determines the service the license is tied to
        public string Service { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RedeemedBy { get; set; } = null;

        public DateTime? RedeemedOn { get; set; } = null;

        public int Uses { get; set; }

        public string Key { get; set; }

        public static List<License> GenerateLicenses(int count, int uses, string service)
        {
            var newLicenses = new List<License>();
            using (var db = new DataContext())
            {
                for (int i = 0; i < count; i++)
                {
                    var newLicense = new License
                    {
                        CreatedAt = DateTime.UtcNow,
                        Key = LicenseService.GenerateRandomKey(),
                        RedeemedBy = null,
                        RedeemedOn = null,
                        Uses = uses,
                        Service = service
                    };

                    // Ensure ONLY unique keys are added
                    // TODO: Use while loop here rather than just re-looping the for loop in order to avoid class allocations of newLicense
                    if (db.UseLicenses.Any(x => x.Key == newLicense.Key))
                    {
                        i--;
                    }
                    else
                    {
                        db.UseLicenses.Add(newLicense);
                        newLicenses.Add(newLicense);
                    }
                }

                db.SaveChanges();
            }

            return newLicenses;
        }

        public bool IsRedeemed()
        {
            return RedeemedBy != null;
        }
    }
}
