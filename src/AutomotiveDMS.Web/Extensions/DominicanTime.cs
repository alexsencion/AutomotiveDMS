namespace AutomotiveDMS.Web.Extensions
{
    public static class DominicanTime
    {
        private static readonly TimeZoneInfo Zone = Resolve();

        public static DateTime FromUtc(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);

        private static TimeZoneInfo Resolve()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.CreateCustomTimeZone(
                    "AST", TimeSpan.FromHours(-4), "Atlantic Standard Time", "AST");
            }
        } 

    }
}
