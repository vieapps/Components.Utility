﻿#region Related components
using System;
using System.Globalization;
using System.Diagnostics;
#endregion

namespace net.vieapps.Components.Utility
{
	/// <summary>
	/// Static servicing methods for working with date-time
	/// </summary>
	public static partial class DateTimeService
	{
		/// <summary>
		/// The Unix Epoch
		/// </summary>
		public static DateTime UnixEpoch { get; } = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Gets the default date-time for checking (1/1/1900)
		/// </summary>
		public static DateTime CheckingDateTime { get; } = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		/// <summary>
		/// Gets the string that presents elapsed times (means times for processing)
		/// </summary>
		/// <param name="elapsedTimes">The value that presents total elapsed times in mili-seconds</param>
		/// <param name="computeMinutes">true to compute minutes in elapsed times</param>
		/// <param name="addString">true to add string (like minute(s), second(s), mili-second(s), ...) into the ending-point</param>
		/// <param name="strDays">The string that presents the label of day(s)</param>
		/// <param name="strHours">The string that presents the label of hour(s)</param>
		/// <param name="strMinutes">The string that presents the label of minute(s)</param>
		/// <param name="strSeconds">The string that presents the label of second(s)</param>
		/// <param name="strMiliseconds">The string that presents the label of mili-second(s)</param>
		/// <param name="strNanoseconds">The string that presents the label of nano-second(s)</param>
		/// <returns>The string that presents elapsed times</returns>
		public static string GetElapsedTimes(this long elapsedTimes, bool computeMinutes = true, bool addString = true, string strDays = "day(s)", string strHours = "hour(s)", string strMinutes = "minute(s)", string strSeconds = "second(s)", string strMiliseconds = "milisecond(s)", string strNanoseconds = "nanosecond(s)")
		{
			if (elapsedTimes < 1)
				return "1" + (addString ? $" {strNanoseconds?.Trim() ?? "nanosecond(s)"}" : "");

			int days = 0, hours = 0, minutes = 0, seconds = 0;

			var miliseconds = elapsedTimes;
			while (miliseconds > 999)
			{
				seconds++;
				miliseconds -= 1000;
			}

			if (computeMinutes && seconds > 59)
			{
				while (seconds > 59)
				{
					minutes++;
					seconds -= 60;
				}
				while (minutes > 59)
				{
					hours++;
					minutes -= 60;
				}
				while (hours > 23)
				{
					days++;
					hours -= 24;
				}
			}

			var times = "";

			if (miliseconds > 0)
				times = miliseconds.ToString() + (addString ? $" {strMiliseconds?.Trim() ?? "milisecond(s)"}" : "") + times;

			if (seconds > 0)
				times = seconds.ToString() + (addString ? $" {strSeconds?.Trim() ?? "second(s)"}" + (!times.Equals("") ? ", " : "") : "") + times;

			if (minutes > 0)
				times = minutes.ToString() + (addString ? $" {strMinutes?.Trim() ?? "minute(s)"}" + (!times.Equals("") ? ", " : "") : "") + times;

			if (hours > 0)
				times = hours.ToString() + (addString ? $" {strHours?.Trim() ?? "hour(s)"}" + (!times.Equals("") ? ", " : "") : "") + times;

			if (days > 0)
				times = days.ToString() + (addString ? $" {strDays?.Trim() ?? "day(s)"}" + (!times.Equals("") ? ", " : "") : "") + times;

			return times;
		}

		/// <summary>
		/// Gets the string that presents elapsed times (means times for processing)
		/// </summary>
		/// <param name="stopwatch">The <see cref="Stopwatch">Stopwatch</see> object that presents elapsed times</param>
		/// <param name="computeMinutes">true to compute minutes in elapsed times</param>
		/// <param name="addString">true to add string (like minute(s), second(s), mili-second(s), ...) into the ending-point</param>
		/// <param name="strDays">The string that presents the label of day(s)</param>
		/// <param name="strHours">The string that presents the label of hour(s)</param>
		/// <param name="strMinutes">The string that presents the label of minute(s)</param>
		/// <param name="strSeconds">The string that presents the label of second(s)</param>
		/// <param name="strMiliseconds">The string that presents the label of mili-second(s)</param>
		/// <param name="strNanoseconds">The string that presents the label of nano-second(s)</param>
		/// <returns>The string that presents elapsed times</returns>
		public static string GetElapsedTimes(this Stopwatch stopwatch, bool computeMinutes = true, bool addString = true, string strDays = "day(s)", string strHours = "hour(s)", string strMinutes = "minute(s)", string strSeconds = "second(s)", string strMiliseconds = "milisecond(s)", string strNanoseconds = "nanosecond(s)")
			=> stopwatch.ElapsedMilliseconds.GetElapsedTimes(computeMinutes, addString, strDays, strHours, strMinutes, strSeconds, strMiliseconds, strNanoseconds);

		/// <summary>
		/// Gets the string that presents elapsed times (means times for processing)
		/// </summary>
		/// <param name="startTime">The <see cref="DateTime">DateTime</see> object that presents the starting time</param>
		/// <param name="computeMinutes">true to compute minutes in elapsed times</param>
		/// <param name="addString">true to add string (like minute(s), second(s), mili-second(s), ...) into the ending-point</param>
		/// <param name="strDays">The string that presents the label of day(s)</param>
		/// <param name="strHours">The string that presents the label of hour(s)</param>
		/// <param name="strMinutes">The string that presents the label of minute(s)</param>
		/// <param name="strSeconds">The string that presents the label of second(s)</param>
		/// <param name="strMiliseconds">The string that presents the label of mili-second(s)</param>
		/// <param name="strNanoseconds">The string that presents the label of nano-second(s)</param>
		/// <returns>The string that presents elapsed times</returns>
		public static string GetElapsedTimes(this DateTime startTime, bool computeMinutes = true, bool addString = true, string strDays = "day(s)", string strHours = "hour(s)", string strMinutes = "minute(s)", string strSeconds = "second(s)", string strMiliseconds = "milisecond(s)", string strNanoseconds = "nanosecond(s)")
			=> (DateTime.Now > startTime ? (DateTime.Now - startTime).TotalMilliseconds : 0).As<long>().GetElapsedTimes(computeMinutes, addString, strDays, strHours, strMinutes, strSeconds, strMiliseconds, strNanoseconds);

		/// <summary>
		/// Gets the name of weekday from this date-time
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static string GetWeekDayName(this DateTime datetime)
		{
			var weekDay = "";
			switch (datetime.DayOfWeek)
			{
				case DayOfWeek.Sunday:
					weekDay = "Sun";
					break;

				case DayOfWeek.Monday:
					weekDay = "Mon";
					break;

				case DayOfWeek.Tuesday:
					weekDay = "Tue";
					break;

				case DayOfWeek.Wednesday:
					weekDay = "Wed";
					break;

				case DayOfWeek.Thursday:
					weekDay = "Thu";
					break;

				case DayOfWeek.Friday:
					weekDay = "Fri";
					break;

				case DayOfWeek.Saturday:
					weekDay = "Sat";
					break;

				default:
					break;
			}
			return weekDay;
		}

		/// <summary>
		/// Gets name of month from this date-time
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static string GetMonthName(this DateTime datetime)
		{
			var month = "";
			switch (datetime.Month)
			{
				case 1:
					month = "Jan";
					break;

				case 2:
					month = "Feb";
					break;

				case 3:
					month = "Mar";
					break;

				case 4:
					month = "Apr";
					break;

				case 5:
					month = "May";
					break;

				case 6:
					month = "Jun";
					break;

				case 7:
					month = "Jul";
					break;

				case 8:
					month = "Aug";
					break;

				case 9:
					month = "Sep";
					break;

				case 10:
					month = "Oct";
					break;

				case 11:
					month = "Nov";
					break;

				case 12:
					month = "Dec";
					break;

				default:
					break;
			}

			return month;
		}

		/// <summary>
		/// Gets the month from this this HTTP date-time string
		/// </summary>
		/// <param name="string"></param>
		/// <returns></returns>
		public static int GetMonthFromHttpString(this string @string)
		{
			var month = 1;
			switch (@string.ToLower())
			{
				case "jan":
				case "january":
					month = 1;
					break;
				case "feb":
				case "february":
					month = 2;
					break;
				case "mar":
				case "march":
					month = 3;
					break;
				case "apr":
				case "april":
					month = 4;
					break;
				case "may":
					month = 5;
					break;
				case "jun":
				case "june":
					month = 6;
					break;
				case "jul":
				case "july":
					month = 7;
					break;
				case "aug":
				case "august":
					month = 8;
					break;
				case "sep":
				case "september":
					month = 9;
					break;
				case "oct":
				case "october":
					month = 10;
					break;
				case "nov":
				case "november":
					month = 11;
					break;
				case "dec":
				case "december":
					month = 12;
					break;
				default:
					break;
			}
			return month;
		}

		/// <summary>
		/// Converts this HTTP date-time string to date-time
		/// </summary>
		/// <param name="string"></param>
		/// <param name="useUTC"></param>
		/// <returns></returns>
		public static DateTime FromHttpDateTime(this string @string, bool useUTC = false)
		{
			if (string.IsNullOrWhiteSpace(@string))
				return DateTimeService.CheckingDateTime;
			else if (!@string.IsEndsWith(" GMT"))
				return DateTimeService.CheckingDateTime;

			// get date-time
			var datetime = DateTimeService.CheckingDateTime;
			try
			{
				var httpDate = @string.ToLower().ToArray(' ');
				var day = httpDate[1];
				var month = httpDate[2];
				var year = httpDate[3];
				var times = httpDate[4].ToArray(':');
				var hour = times[0];
				var minute = times[1];
				var second = times[2];
				datetime = new DateTime(year.As<int>(), month.GetMonthFromHttpString(), day.As<int>(), hour.As<int>(), minute.As<int>(), second.As<int>());
				datetime = useUTC
					? datetime.ToUniversalTime()
					: datetime.ToLocalTime();
			}
			catch { }

			return datetime;
		}

		/// <summary>
		/// Gets the first-day-of-week of this date-time
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static DateTime GetFirstDayOfWeek(this DateTime datetime)
		{
			var firstDay = datetime;
			while (firstDay.DayOfWeek != DayOfWeek.Monday)
				firstDay = firstDay.AddDays(-1);
			return new DateTime(firstDay.Year, firstDay.Month, firstDay.Day, 0, 0, 0);
		}

		/// <summary>
		/// Gets the end-day-of-week of this date-time
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static DateTime GetEndDayOfWeek(this DateTime datetime)
		{
			var endDay = datetime.GetFirstDayOfWeek().AddDays(6);
			return new DateTime(endDay.Year, endDay.Month, endDay.Day, 23, 59, 59);
		}

		/// <summary>
		/// Gets the first-day-of-month of this date-time
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static DateTime GetFirstDayOfMonth(this DateTime datetime)
			=> new DateTime(datetime.Year, datetime.Month, 1, 0, 0, 0);

		/// <summary>
		/// Gets the end-day-of-month of this date-time
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static DateTime GetEndDayOfMonth(this DateTime datetime)
		{
			var daysInMonth = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(datetime.Year, datetime.Month);
			var endDay = datetime.GetFirstDayOfMonth().AddDays(daysInMonth - 1);
			return new DateTime(endDay.Year, endDay.Month, endDay.Day, 23, 59, 59);
		}

		/// <summary>
		/// Checks to see this date-time is in current week
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static bool IsInCurrentWeek(this DateTime datetime)
			=> datetime >= DateTime.Now.GetFirstDayOfWeek() && datetime <= DateTime.Now.GetEndDayOfWeek();

		/// <summary>
		/// Checks to see this date-time is in current month
		/// </summary>
		/// <param name="datetime"></param>
		/// <returns></returns>
		public static bool IsInCurrentMonth(this DateTime datetime)
			=> datetime >= DateTime.Now.GetFirstDayOfMonth() && datetime <= DateTime.Now.GetEndDayOfMonth();

		/// <summary>
		/// Converts this date-time to UNIX timestamp
		/// </summary>
		/// <param name="datetime"></param>
		/// <param name="useUTC"></param>
		/// <returns></returns>
		public static long ToUnixTimestamp(this DateTime datetime, bool useUTC = true)
			=> ((useUTC ? datetime.ToUniversalTime() : datetime) - DateTimeService.UnixEpoch).TotalSeconds.As<long>();

		/// <summary>
		/// Converts this UNIX timestamp to date-time
		/// </summary>
		/// <param name="unixTimestamp"></param>
		/// <param name="useUTC"></param>
		/// <returns></returns>
		public static DateTime FromUnixTimestamp(this long unixTimestamp, bool useUTC = true)
			=> useUTC
				? DateTimeService.UnixEpoch.AddSeconds(unixTimestamp)
				: DateTimeService.UnixEpoch.AddSeconds(unixTimestamp).ToLocalTime();

		/// <summary>
		/// Converts this date-time to string in format 'yyyy/MM/dd HH:mm:ss'
		/// </summary>
		/// <param name="datetime"></param>
		/// <param name="useUTC">true to use UTC date-time</param>
		/// <param name="allowTime">true to allow time, false to use date onlye</param>
		/// <returns></returns>
		public static string ToDTString(this DateTime datetime, bool useUTC = false, bool allowTime = true)
			=> (useUTC ? datetime.ToUniversalTime() : datetime).ToString(allowTime ? "yyyy/MM/dd HH:mm:ss" : "yyyy/MM/dd");

		/// <summary>
		/// Converts this date-time to string in format 'yyyy-MM-ddTHH:mm:ssZzzzz'
		/// </summary>
		/// <param name="datetime"></param>
		/// <param name="useUTC"></param>
		/// <returns></returns>
		public static string ToUtcString(this DateTime datetime, bool useUTC = false)
			=> (useUTC ? datetime.ToUniversalTime() : datetime).ToString("yyyy-MM-ddTHH:mm:ssZzzzz");

		/// <summary>
		/// Converts this date-time to string in format 'yyyy-MM-ddTHH:mm:ssZzzzz'
		/// </summary>
		/// <param name="datetime"></param>
		/// <param name="useUTC"></param>
		/// <returns></returns>
		public static string ToIsoString(this DateTime datetime, bool useUTC = false)
			=> (useUTC ? datetime.ToUniversalTime() : datetime).ToString("yyyy-MM-ddTHH:mm:ss.fffzzzz");

		/// <summary>
		/// Converts this date-time to HTTP string with GMT
		/// </summary>
		/// <param name="datetime"></param>
		/// <param name="useUTC"></param>
		/// <returns></returns>
		public static string ToHttpString(this DateTime datetime, bool useUTC = true)
		{
			var time = useUTC ? datetime.ToUniversalTime() : datetime;
			return $"{time.GetWeekDayName()}, {time.Day.ToString().PadLeft(2, '0')} {time.GetMonthName()} {time.Year} {time.Hour.ToString().PadLeft(2, '0')}:{time.Minute.ToString().PadLeft(2, '0')}:{time.Second.ToString().PadLeft(2, '0')} GMT";
		}
	}
}