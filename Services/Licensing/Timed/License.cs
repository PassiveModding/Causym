using System;
using System.Collections.Generic;
using System.Linq;

namespace Causym.Licensing.Timed
{
    public class License
    {
        // Determines the service the license is tied to
        public string Service { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RedeemedBy { get; set; } = null;

        public DateTime? RedeemedOn { get; set; } = null;

        public TimeSpan ValidFor { get; set; }

        public string Key { get; set; }

        public static List<License> GenerateLicenses(int count, TimeSpan time, string service)
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
                        ValidFor = time,
                        Service = service
                    };

                    // Ensure ONLY unique keys are added
                    // TODO: Use while loop here rather than just re-looping the for loop in order to avoid class allocations of newLicense
                    if (db.TimeLicenses.Any(x => x.Key == newLicense.Key))
                    {
                        i--;
                    }
                    else
                    {
                        db.TimeLicenses.Add(newLicense);
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
