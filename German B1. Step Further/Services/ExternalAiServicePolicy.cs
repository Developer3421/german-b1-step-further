using System;

namespace German_B1._Step_Further.Services
{
    public static class ExternalAiServicePolicy
    {
        public static bool RequiresUserAgreement(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Currently, this is used only for online tools opened inside Instruments.
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }
    }
}

