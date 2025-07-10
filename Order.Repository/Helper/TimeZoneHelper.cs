using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Order.Repository.Helper
{
    public static class TimeZoneHelper
    {
        private static readonly TimeZoneInfo AustraliaTimeZone = GetAustraliaTimeZone();

        private static TimeZoneInfo GetAustraliaTimeZone()
        {
            string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "AUS Eastern Standard Time"      // Windows ID
                : "Australia/Sydney";              // IANA ID (Linux/macOS)

            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public static DateTime GetCurrentAustraliaTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AustraliaTimeZone);
        }

        public static DateTime ConvertToAustraliaTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, AustraliaTimeZone);
        }

        public static DateTime ConvertFromAustraliaTime(DateTime australiaTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(australiaTime, AustraliaTimeZone);
        }
    }
}
