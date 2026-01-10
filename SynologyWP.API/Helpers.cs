using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynologyWP.API
{
  public static class Helpers
  {
    public static DateTime UnixTimeStampToDateTime(ulong unixTimeStamp)
    {
      // Unix timestamp is seconds past epoch
      if (unixTimeStamp == 0)
      {
        return new DateTime();
      }
      DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
      return dateTime;
    }

    public static ulong DateTimeToUnixTimeStamp(DateTime date)
    {
      return (ulong)((DateTimeOffset)date).ToUnixTimeSeconds();
    }

    public static string HumanReadableSize(ulong size)
    {
      string[] sizes = { "B", "KB", "MB", "GB", "TB" };
      int order = 0;
      double len = size;
      while (len >= 1024 && order < sizes.Length - 1)
      {
        order++;
        len = len / 1024;
      }

      // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
      // show a single decimal place, and no space.
      return string.Format("{0:0.##} {1}", len, sizes[order]);
    }
  }
}
