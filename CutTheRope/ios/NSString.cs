using System;
using System.Collections.Generic;

namespace CutTheRope.ios
{
    // Token: 0x02000019 RID: 25
    internal class NSString : NSObject
    {
        // Token: 0x060000DE RID: 222 RVA: 0x00005307 File Offset: 0x00003507
        public NSString()
        {
            this.value_ = "";
        }

        // Token: 0x060000DF RID: 223 RVA: 0x0000531A File Offset: 0x0000351A
        public NSString(string rhs)
        {
            this.value_ = rhs;
        }

        // Token: 0x060000E0 RID: 224 RVA: 0x00005329 File Offset: 0x00003529
        public override string ToString()
        {
            return this.value_;
        }

        // Token: 0x060000E1 RID: 225 RVA: 0x00005331 File Offset: 0x00003531
        public int length()
        {
            if (this.value_ == null)
            {
                return 0;
            }
            return this.value_.Length;
        }

        // Token: 0x060000E2 RID: 226 RVA: 0x00005348 File Offset: 0x00003548
        public bool isEqualToString(NSString str)
        {
            return this.isEqualToString(str.value_);
        }

        // Token: 0x060000E3 RID: 227 RVA: 0x00005356 File Offset: 0x00003556
        public bool isEqualToString(string str)
        {
            if (this.value_ == null)
            {
                return str == null;
            }
            return str != null && this.value_ == str;
        }

        // Token: 0x060000E4 RID: 228 RVA: 0x00005376 File Offset: 0x00003576
        public int IndexOf(char c)
        {
            return this.value_.IndexOf(c);
        }

        // Token: 0x060000E5 RID: 229 RVA: 0x00005384 File Offset: 0x00003584
        public NSRange rangeOfString(NSString str)
        {
            return this.rangeOfString(str.value_);
        }

        // Token: 0x060000E6 RID: 230 RVA: 0x00005394 File Offset: 0x00003594
        public NSRange rangeOfString(string str)
        {
            NSRange result = default(NSRange);
            result.length = 0U;
            result.location = 0U;
            if (str.Length > 0)
            {
                int num = this.value_.IndexOf(str);
                if (num > -1)
                {
                    result.length = (uint)str.Length;
                    result.location = (uint)num;
                }
            }
            return result;
        }

        // Token: 0x060000E7 RID: 231 RVA: 0x000053E9 File Offset: 0x000035E9
        public char characterAtIndex(int n)
        {
            return this.value_[n];
        }

        // Token: 0x060000E8 RID: 232 RVA: 0x000053F7 File Offset: 0x000035F7
        public NSString copy()
        {
            return new NSString(this.value_);
        }

        // Token: 0x060000E9 RID: 233 RVA: 0x00005404 File Offset: 0x00003604
        public void getCharacters(char[] to)
        {
            int num = Math.Min(to.Length - 1, this.length());
            for (int i = 0; i < num; i++)
            {
                to[i] = this.value_[i];
            }
            to[num] = '\0';
        }

        // Token: 0x060000EA RID: 234 RVA: 0x00005444 File Offset: 0x00003644
        public char[] getCharacters()
        {
            char[] array = new char[this.length() + 1];
            this.getCharacters(array);
            return array;
        }

        // Token: 0x060000EB RID: 235 RVA: 0x00005467 File Offset: 0x00003667
        public NSString substringWithRange(NSRange range)
        {
            return new NSString(this.value_.Substring((int)range.location, (int)range.length));
        }

        // Token: 0x060000EC RID: 236 RVA: 0x00005485 File Offset: 0x00003685
        public NSString substringFromIndex(int n)
        {
            return new NSString(this.value_.Substring(n));
        }

        // Token: 0x060000ED RID: 237 RVA: 0x00005498 File Offset: 0x00003698
        public NSString substringToIndex(int n)
        {
            return new NSString(this.value_.Substring(0, n));
        }

        // Token: 0x060000EE RID: 238 RVA: 0x000054AC File Offset: 0x000036AC
        public int intValue()
        {
            if (this.value_.Length == 0)
            {
                return 0;
            }
            int num = 0;
            int num2 = 0;
            int num3 = this.value_.Length;
            int num4 = 1;
            while (num2 < num3)
            {
                if (this.value_[num2] == ' ')
                {
                    num2++;
                }
                else if (this.value_[num2] == '-')
                {
                    num4 = -1;
                    num2++;
                }
                else
                {
                    num *= 10;
                    num += (int)(this.value_[num2++] - '0');
                }
            }
            return num * num4;
        }

        // Token: 0x060000EF RID: 239 RVA: 0x0000552C File Offset: 0x0000372C
        public bool boolValue()
        {
            return this.value_.Length != 0 && this.value_.ToLower() == "true";
        }

        // Token: 0x060000F0 RID: 240 RVA: 0x00005554 File Offset: 0x00003754
        public float floatValue()
        {
            if (this.value_.Length == 0)
            {
                return 0f;
            }
            float num = 0f;
            int num2 = 0;
            int num3 = this.value_.Length;
            int num4 = 1;
            int num5 = 10;
            int num6 = 1;
            while (num2 < num3)
            {
                if (this.value_[num2] == ' ')
                {
                    num2++;
                }
                else if (this.value_[num2] == '-')
                {
                    num4 = -1;
                    num2++;
                }
                else if (this.value_[num2] == ',' || this.value_[num2] == '.')
                {
                    num5 = 1;
                    num6 = 10;
                    num2++;
                }
                else
                {
                    num *= (float)num5;
                    num += ((float)this.value_[num2++] - 48f) / (float)num6;
                    if (num6 > 1)
                    {
                        num6 *= 10;
                    }
                }
            }
            return num * (float)num4;
        }

        // Token: 0x060000F1 RID: 241 RVA: 0x0000562C File Offset: 0x0000382C
        public List<NSString> componentsSeparatedByString(char ch)
        {
            List<NSString> list = new();
            char[] separator = new char[] { ch };
            foreach (string rhs in this.value_.Split(separator))
            {
                list.Add(new NSString(rhs));
            }
            return list;
        }

        // Token: 0x060000F2 RID: 242 RVA: 0x00005678 File Offset: 0x00003878
        public bool hasPrefix(NSString prefix)
        {
            return this.value_.StartsWith(prefix.ToString());
        }

        // Token: 0x060000F3 RID: 243 RVA: 0x0000568B File Offset: 0x0000388B
        public bool hasSuffix(string p)
        {
            return this.value_.EndsWith(p);
        }

        // Token: 0x04000092 RID: 146
        private string value_;
    }
}
