using System;

namespace FinSenseAPI.Helpers;

public static class DateHelper
{
    public static int GetDaysElapsedInMonth(DateTime date)
    {
        return date.Day;
    }

    public static int GetDaysRemainingInMonth(DateTime date)
    {
        var last = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        return (last - date).Days;
    }

    public static int GetMonthsBetween(DateTime from, DateTime to)
    {
        return Math.Abs((to.Year - from.Year) * 12 + to.Month - from.Month);
    }

    public static DateTime GetFirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime GetLastDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }
}
