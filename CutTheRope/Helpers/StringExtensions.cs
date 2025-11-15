using System;
using System.Collections.Generic;

namespace CutTheRope.Helpers
{
    internal static class StringExtensions
    {
        private static string SafeValue(string value)
        {
            return value ?? string.Empty;
        }

        public static int Length(this string value)
        {
            return SafeValue(value).Length;
        }

        public static bool IsEqualToString(this string value, string other)
        {
            return value == null ? other == null : other != null && value.Equals(other, StringComparison.Ordinal);
        }

        public static int IndexOf(this string value, char c)
        {
            return SafeValue(value).IndexOf(c);
        }

        public static char CharacterAtIndex(this string value, int n)
        {
            return SafeValue(value)[n];
        }

        public static string Copy(this string value)
        {
            return value ?? string.Empty;
        }

        public static void GetCharacters(this string value, char[] destination)
        {
            if (destination == null || destination.Length == 0)
            {
                return;
            }

            string safeValue = SafeValue(value);
            int copyLength = Math.Min(destination.Length - 1, safeValue.Length);
            for (int i = 0; i < copyLength; i++)
            {
                destination[i] = safeValue[i];
            }

            destination[copyLength] = '\0';
        }

        public static char[] GetCharacters(this string value)
        {
            char[] buffer = new char[SafeValue(value).Length + 1];
            value?.GetCharacters(buffer);
            return buffer;
        }

        public static string SubstringFromIndex(this string value, int index)
        {
            return SafeValue(value)[index..];
        }

        public static string SubstringToIndex(this string value, int index)
        {
            return SafeValue(value)[..index];
        }

        public static int IntValue(this string value)
        {
            string safeValue = SafeValue(value);
            if (safeValue.Length == 0)
            {
                return 0;
            }

            int result = 0;
            int sign = 1;
            for (int i = 0; i < safeValue.Length; i++)
            {
                if (safeValue[i] == ' ')
                {
                    continue;
                }

                if (safeValue[i] == '-')
                {
                    sign = -1;
                    continue;
                }

                result *= 10;
                result += safeValue[i] - '0';
            }

            return result * sign;
        }

        public static bool BoolValue(this string value)
        {
            string safeValue = SafeValue(value);
            return safeValue.Length != 0 && safeValue.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public static float FloatValue(this string value)
        {
            string safeValue = SafeValue(value);
            if (safeValue.Length == 0)
            {
                return 0f;
            }

            float number = 0f;
            int sign = 1;
            int mul = 10;
            int fractionMul = 1;
            for (int i = 0; i < safeValue.Length; i++)
            {
                if (safeValue[i] == ' ')
                {
                    continue;
                }

                if (safeValue[i] == '-')
                {
                    sign = -1;
                    continue;
                }

                if (safeValue[i] is ',' or '.')
                {
                    mul = 1;
                    fractionMul = 10;
                    continue;
                }

                number *= mul;
                number += (safeValue[i] - 48f) / fractionMul;
                if (fractionMul > 1)
                {
                    fractionMul *= 10;
                }
            }

            return number * sign;
        }

        public static List<string> ComponentsSeparatedByString(this string value, char separator)
        {
            List<string> list = [];
            char[] separators = [separator];
            foreach (string part in SafeValue(value).Split(separators))
            {
                list.Add(part);
            }

            return list;
        }

        public static bool HasPrefix(this string value, string prefix)
        {
            return SafeValue(value).StartsWith(SafeValue(prefix), StringComparison.Ordinal);
        }

        public static bool HasSuffix(this string value, string suffix)
        {
            return SafeValue(value).EndsWith(SafeValue(suffix), StringComparison.Ordinal);
        }
    }
}
