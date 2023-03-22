namespace Datatag
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class StringE
    {
        internal static int IndexOf(this string str, string substring, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex > str.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            int length = str.Length - startIndex;

            if (length < substring.Length)
            {
                return -1;
            }

            for (int i = startIndex; i <= str.Length - substring.Length; i++)
            {
                if (str.Substring(i, substring.Length).Equals(substring))
                {
                    return i;
                }
            }

            return -1;
        }

        internal static int IndexOfAny(this string str, params char[] anyOf)
            => str.IndexOfAny(anyOf);

        internal static string UpToWhitespace(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return "";
            }

            int index = 0;
            while (index < str.Length && !char.IsWhiteSpace(str[index]))
            {
                index++;
            }

            return str.Substring(0, index);
        }

        internal static string Repeat(this string str, int n)
        {
            if (n <= 0)
                return string.Empty;

            if (n == 1)
                return str;

            int len = str.Length;
            int totalLen = len * n;
            char[] chars = new char[totalLen];

            for (int i = 0; i < totalLen; i += len)
            {
                str.CopyTo(0, chars, i, len);
            }

            return new string(chars);
        }

        internal static string UpToNewline(this string str)
        {
            int index = str.IndexOfAny(new char[] { '\r', '\n' });
            if (index == -1)
            {
                return str;
            }
            else if (index > 0 && str[index - 1] == '\r' && index < str.Length - 1 && str[index + 1] == '\n')
            {
                // handle CRLF newline format
                return str.Substring(0, index - 1);
            }
            else
            {
                // handle other newline formats
                return str.Substring(0, index);
            }
        }

        internal static string UpToSubstring(this string str, string substring)
        {
            int index = str.IndexOf(substring);
            if (index > 0)
            {
                return str.Substring(0, index);
            }
            return str;
        }

        internal static string UpToSubstring(this string str, string substring, out string before)
        {
            before = null;
            int index = str.IndexOf(substring);
            if (index > 0)
            {
                before = str.Substring(0, index);
                return str.Substring(index);
            }
            return str;
        }
    }
}