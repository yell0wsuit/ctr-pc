using System;
using System.Collections.Generic;

namespace CutTheRope.ios
{
    internal sealed class NSString : NSObject
    {
        public NSString()
        {
            value_ = "";
        }

        public NSString(string rhs)
        {
            value_ = rhs;
        }

        public override string ToString()
        {
            return value_;
        }

        public int Length()
        {
            return value_ == null ? 0 : value_.Length;
        }

        public bool IsEqualToString(NSString str)
        {
            return IsEqualToString(str.value_);
        }

        public bool IsEqualToString(string str)
        {
            return value_ == null ? str == null : str != null && value_ == str;
        }

        public int IndexOf(char c)
        {
            return value_.IndexOf(c);
        }

        public NSRange RangeOfString(NSString str)
        {
            return RangeOfString(str.value_);
        }

        public NSRange RangeOfString(string str)
        {
            NSRange result = default;
            result.length = 0U;
            result.location = 0U;
            if (str.Length > 0)
            {
                int num = value_.IndexOf(str, StringComparison.Ordinal);
                if (num > -1)
                {
                    result.length = (uint)str.Length;
                    result.location = (uint)num;
                }
            }
            return result;
        }

        public char CharacterAtIndex(int n)
        {
            return value_[n];
        }

        public NSString Copy()
        {
            return new NSString(value_);
        }

        public void GetCharacters(char[] to)
        {
            int num = Math.Min(to.Length - 1, Length());
            for (int i = 0; i < num; i++)
            {
                to[i] = value_[i];
            }
            to[num] = '\0';
        }

        public char[] GetCharacters()
        {
            char[] array = new char[Length() + 1];
            GetCharacters(array);
            return array;
        }

        public NSString SubstringWithRange(NSRange range)
        {
            return new NSString(value_.Substring((int)range.location, (int)range.length));
        }

        public NSString SubstringFromIndex(int n)
        {
            return new NSString(value_[n..]);
        }

        public NSString SubstringToIndex(int n)
        {
            return new NSString(value_[..n]);
        }

        public int IntValue()
        {
            if (value_.Length == 0)
            {
                return 0;
            }
            int num = 0;
            int num2 = 0;
            int num3 = value_.Length;
            int num4 = 1;
            while (num2 < num3)
            {
                if (value_[num2] == ' ')
                {
                    num2++;
                }
                else if (value_[num2] == '-')
                {
                    num4 = -1;
                    num2++;
                }
                else
                {
                    num *= 10;
                    num += value_[num2++] - '0';
                }
            }
            return num * num4;
        }

        public bool BoolValue()
        {
            return value_.Length != 0 && value_.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public float FloatValue()
        {
            if (value_.Length == 0)
            {
                return 0f;
            }
            float num = 0f;
            int num2 = 0;
            int num3 = value_.Length;
            int num4 = 1;
            int num5 = 10;
            int num6 = 1;
            while (num2 < num3)
            {
                if (value_[num2] == ' ')
                {
                    num2++;
                }
                else if (value_[num2] == '-')
                {
                    num4 = -1;
                    num2++;
                }
                else if (value_[num2] is ',' or '.')
                {
                    num5 = 1;
                    num6 = 10;
                    num2++;
                }
                else
                {
                    num *= num5;
                    num += (value_[num2++] - 48f) / num6;
                    if (num6 > 1)
                    {
                        num6 *= 10;
                    }
                }
            }
            return num * num4;
        }

        public List<NSString> ComponentsSeparatedByString(char ch)
        {
            List<NSString> list = [];
            char[] separator = [ch];
            foreach (string rhs in value_.Split(separator))
            {
                list.Add(new NSString(rhs));
            }
            return list;
        }

        public bool HasPrefix(NSString prefix)
        {
            return value_.StartsWith(prefix.ToString(), StringComparison.Ordinal);
        }

        public bool HasSuffix(string p)
        {
            return value_.EndsWith(p, StringComparison.Ordinal);
        }

        private readonly string value_;
    }
}
