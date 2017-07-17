﻿namespace Backend.Fx.Extensions
{
    using System.Text.RegularExpressions;

    public static class StringEx
    {
        public static string Cut(this string s, int length)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            if (s.Length > length)
            {
                s = s.Substring(0, length - 1) + "…";
            }

            return s;
        }

        public static string ToUnixLineEnding(this string s)
        {
            return Regex.Replace(s, @"\r\n?", "\n");
        }
    }
}
