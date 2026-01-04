using System;

namespace German_B1._Step_Further.Models
{
    public class UserSettings
    {
        public bool UserAgreementAccepted { get; set; }

        public DateTime? UserAgreementAcceptedAtUtc { get; set; }

        // Simple versioning in case we update the agreement text later.
        public string UserAgreementVersion { get; set; } = "1.0";
    }
}
