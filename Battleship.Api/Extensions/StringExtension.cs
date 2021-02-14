using System;
using System.Collections.Generic;

namespace Battleship.Api.Extensions
{
    public static class StringExtension
    {
        public static IEnumerable<string> TrimSplit(this string value, char separator)
        {
            if (value == null)
                yield break;

            foreach (var i in value.Split(separator, StringSplitOptions.RemoveEmptyEntries)) 
                yield return i.Trim();
        }
    }
}
