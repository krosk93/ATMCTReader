using System;

namespace ATMCTReader.CLI;

public static class Extensions
{
    public static string ToPaddedShortDateString(this DateTime dt)
    {
        return dt.ToString("dd/MM/yyyy");
    }

    public static string ToPaddedShortDateTimeString(this DateTime dt)
    {
        return dt.ToString("dd/MM/yyyy HH:mm");
    }
}
