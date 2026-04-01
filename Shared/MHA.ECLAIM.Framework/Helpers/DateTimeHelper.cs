using MHA.ECLAIM.Framework.JSONConstants;
using System.Globalization;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class DateTimeHelper
    {
        private static readonly JSONAppSettings appSettings;

        static DateTimeHelper()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        #region Current Date and Time
        public static DateTime GetCurrentUtcDateTime()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            currentDateTime = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Unspecified);
            return currentDateTime;
        }

        public static DateTime GetCurrentDateTime()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            currentDateTime = ConvertToLocalDateTime(currentDateTime);
            return currentDateTime;
        }
        #endregion

        #region Date and Time Conversion
        public static DateTime ConvertToLocalDateTime(DateTime? dateTime)
        {
            DateTime localDateTime = DateTime.MinValue;
            if (dateTime != null)
                localDateTime = ConvertToLocalDateTime((DateTime)dateTime);
            return localDateTime;
        }

        public static DateTime ConvertToLocalDateTime(DateTime dateTime)
        {
            DateTime currentDateTime = dateTime;
            currentDateTime = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Unspecified);
            string timeZoneName = appSettings.CentralTimeZone;
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(currentDateTime, timeZoneInfo);

            return currentDateTime;
        }

        public static DateTime ConvertToUTCDateTime(DateTime dateTime)
        {
            DateTime currentDateTime = dateTime;
            currentDateTime = DateTime.SpecifyKind(currentDateTime, DateTimeKind.Unspecified);
            string timeZoneName = appSettings.CentralTimeZone;
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
            currentDateTime = TimeZoneInfo.ConvertTimeToUtc(currentDateTime, timeZoneInfo);
            return currentDateTime;
        }

        public static DateTime ConvertStringToDate(string dateString)
        {
            DateTime.TryParseExact(dateString, appSettings.DefaultDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateResult);
            return dateResult;
        }
        #endregion

        #region Date and Time Formatting
        public static string FormatDate(DateTime? date)
        {
            return date?.ToString(appSettings.DefaultDateFormat) ?? "";
        }

        #endregion

        #region Next and Previous Date
        public static DateTime GetNextWorkingDate(List<DayOfWeek> workingDaysOfWeek, DateTime currentDate, int dueDays)
        {
            while (dueDays > 0)
            {
                currentDate = currentDate.AddDays(1);

                if (workingDaysOfWeek.Contains(currentDate.DayOfWeek))
                {
                    dueDays--;
                }
            }

            return currentDate;
        }
        #endregion
    }
}
