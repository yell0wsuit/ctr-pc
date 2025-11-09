using System;
using System.Collections.Generic;

namespace CutTheRope.ios
{
    internal class NSString : NSObject
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

        public int length()
        {
            if (value_ == null)
            {
                return 0;
            }
            return value_.Length;
        }

        public bool isEqualToString(NSString str)
        {
            return isEqualToString(str.value_);
        }

        public bool isEqualToString(string str)
        {
            if (value_ == null)
            {
                return str == null;
            }
            return str != null && value_ == str;
        }

        public int IndexOf(char c)
        {
            return value_.IndexOf(c);
        }

        public NSRange rangeOfString(NSString str)
        {
            return rangeOfString(str.value_);
        }

        public NSRange rangeOfString(string str)
        {
            NSRange result = default(NSRange);
            result.length = 0U;
            result.location = 0U;
            if (str.Length > 0)
            {
                int num = value_.IndexOf(str);
                if (num > -1)
                {
                    result.length = (uint)str.Length;
                    result.location = (uint)num;
                }
            }
            return result;
        }

        public char characterAtIndex(int n)
        {
            return value_[n];
        }

        public NSString copy()
        {
            return new NSString(value_);
        }

        public void getCharacters(char[] to)
        {
            int num = Math.Min(to.Length - 1, length());
            for (int i = 0; i < num; i++)
            {
                to[i] = value_[i];
            }
            to[num] = '\0';
        }

        public char[] getCharacters()
        {
            char[] array = new char[length() + 1];
            getCharacters(array);
            return array;
        }

        public NSString substringWithRange(NSRange range)
        {
            return new NSString(value_.Substring((int)range.location, (int)range.length));
        }

        public NSString substringFromIndex(int n)
        {
            return new NSString(value_.Substring(n));
        }

        public NSString substringToIndex(int n)
        {
            return new NSString(value_.Substring(0, n));
        }

        public int intValue()
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

        public bool boolValue()
        {
            return value_.Length != 0 && value_.ToLower() == "true";
        }

        public float floatValue()
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
                else if (value_[num2] == ',' || value_[num2] == '.')
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

        public List<NSString> componentsSeparatedByString(char ch)
        {
            List<NSString> list = new();
            char[] separator = [ch];
            foreach (string rhs in value_.Split(separator))
            {
                list.Add(new NSString(rhs));
            }
            return list;
        }

        public bool hasPrefix(NSString prefix)
        {
            return value_.StartsWith(prefix.ToString());
        }

        public bool hasSuffix(string p)
        {
            return value_.EndsWith(p);
        }

        private string value_;
    }
}
