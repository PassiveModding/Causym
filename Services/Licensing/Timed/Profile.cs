using System;

namespace Causym.Licensing.Timed
{
    public class Profile
    {
        // Determines the service the profile is tied to
        public string Service { get; set; }

        public string Id { get; set; }

        public DateTime ExpiryDate { get; set; }

        // Random GUID (can be regenerated via command, unique to each user, used for user LicensesRemaining transfer)
        public string BackupKey { get; set; } = null;

        public TimeSpan? TimeRemaining()
        {
            var now = DateTime.UtcNow;
            if (now > ExpiryDate)
            {
                return null;
            }

            return ExpiryDate - now;
        }

        public bool IsExpired()
        {
            var remaining = TimeRemaining();
            if (remaining == null) return true;
            return TimeRemaining() < TimeSpan.Zero;
        }
    }
}
