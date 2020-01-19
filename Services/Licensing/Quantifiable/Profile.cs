namespace Causym.Licensing.Quantifiable
{
    public class Profile
    {
        // Determines the service the profile is tied to
        public string Service { get; set; }

        public string Id { get; set; }

        public int LicensesUsed { get; set; }

        public int LicensesRedeemed { get; set; }

        // Random GUID (can be regenerated via command, unique to each user, used for user LicensesRemaining transfer)
        public string BackupKey { get; set; } = null;

        /*public static void RedeemLicense(string service, string id, string key)
        {
            using (var db = new LicenseContext())
            {
                var user = db.UseUsers.FirstOrDefault(x => x.Service == service && x.Id == id);
                if (user == null)
                {
                    user = new Profile
                    {
                        Id = id,
                        Service = service,
                        BackupKey = null,
                        LicensesUsed = 0,
                        LicensesRedeemed = Uses
                    };
                }
            }
        }*/

        public int LicensesRemaining()
        {
            return LicensesRedeemed - LicensesUsed;
        }
    }
}
