using CutTheRope.ios;
using System;
using System.Collections.Generic;
using System.IO;

namespace CutTheRope.iframework.core
{
    // Token: 0x02000066 RID: 102
    internal class Preferences : NSObject
    {
        // Token: 0x060003BD RID: 957 RVA: 0x00014DF8 File Offset: 0x00012FF8
        public override NSObject init()
        {
            if (base.init() == null)
            {
                return null;
            }
            Preferences._loadPreferences();
            return this;
        }

        // Token: 0x060003BE RID: 958 RVA: 0x00014E0A File Offset: 0x0001300A
        public virtual void setIntforKey(int v, string k, bool comit)
        {
            Preferences._setIntforKey(v, k, comit);
        }

        // Token: 0x060003BF RID: 959 RVA: 0x00014E14 File Offset: 0x00013014
        public virtual void setBooleanforKey(bool v, string k, bool comit)
        {
            Preferences._setBooleanforKey(v, k, comit);
        }

        // Token: 0x060003C0 RID: 960 RVA: 0x00014E1E File Offset: 0x0001301E
        public virtual void setStringforKey(string v, string k, bool comit)
        {
            Preferences._setStringforKey(v, k, comit);
        }

        // Token: 0x060003C1 RID: 961 RVA: 0x00014E28 File Offset: 0x00013028
        public virtual int getIntForKey(string k)
        {
            return Preferences._getIntForKey(k);
        }

        // Token: 0x060003C2 RID: 962 RVA: 0x00014E30 File Offset: 0x00013030
        public virtual float getFloatForKey(string k)
        {
            return 0f;
        }

        // Token: 0x060003C3 RID: 963 RVA: 0x00014E37 File Offset: 0x00013037
        public virtual bool getBooleanForKey(string k)
        {
            return Preferences._getBooleanForKey(k);
        }

        // Token: 0x060003C4 RID: 964 RVA: 0x00014E3F File Offset: 0x0001303F
        public virtual string getStringForKey(string k)
        {
            return Preferences._getStringForKey(k);
        }

        // Token: 0x060003C5 RID: 965 RVA: 0x00014E48 File Offset: 0x00013048
        public static void _setIntforKey(int v, string key, bool comit)
        {
            int value;
            if (Preferences.data_.TryGetValue(key, out value))
            {
                Preferences.data_[key] = v;
            }
            else
            {
                Preferences.data_.Add(key, v);
            }
            if (comit)
            {
                Preferences._savePreferences();
            }
        }

        // Token: 0x060003C6 RID: 966 RVA: 0x00014E88 File Offset: 0x00013088
        private static void _setStringforKey(string v, string k, bool comit)
        {
            string value;
            if (Preferences.dataStrings_.TryGetValue(k, out value))
            {
                Preferences.dataStrings_[k] = v;
            }
            else
            {
                Preferences.dataStrings_.Add(k, v);
            }
            if (comit)
            {
                Preferences._savePreferences();
            }
        }

        // Token: 0x060003C7 RID: 967 RVA: 0x00014EC8 File Offset: 0x000130C8
        public static int _getIntForKey(string k)
        {
            int value;
            if (Preferences.data_.TryGetValue(k, out value))
            {
                return value;
            }
            return 0;
        }

        // Token: 0x060003C8 RID: 968 RVA: 0x00014EE7 File Offset: 0x000130E7
        private static float _getFloatForKey(string k)
        {
            return 0f;
        }

        // Token: 0x060003C9 RID: 969 RVA: 0x00014EEE File Offset: 0x000130EE
        public static bool _getBooleanForKey(string k)
        {
            return Preferences._getIntForKey(k) != 0;
        }

        // Token: 0x060003CA RID: 970 RVA: 0x00014EF9 File Offset: 0x000130F9
        public static void _setBooleanforKey(bool v, string k, bool comit)
        {
            Preferences._setIntforKey((v > false) ? 1 : 0, k, comit);
        }

        // Token: 0x060003CB RID: 971 RVA: 0x00014F08 File Offset: 0x00013108
        private static string _getStringForKey(string k)
        {
            string value;
            if (Preferences.dataStrings_.TryGetValue(k, out value))
            {
                return value;
            }
            return "";
        }

        // Token: 0x060003CC RID: 972 RVA: 0x00014F2B File Offset: 0x0001312B
        public virtual void savePreferences()
        {
            Preferences._savePreferences();
        }

        // Token: 0x060003CD RID: 973 RVA: 0x00014F34 File Offset: 0x00013134
        public static void _savePreferences()
        {
            try
            {
                if (!Preferences.GameSaveRequested)
                {
                    Preferences.GameSaveRequested = true;
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x060003CE RID: 974 RVA: 0x00014F64 File Offset: 0x00013164
        public static void Update()
        {
            try
            {
                if (Preferences.GameSaveRequested)
                {
                    FileStream fileStream = File.Create("ctr_save.bin");
                    Preferences.SaveToStream(fileStream);
                    fileStream.Close();
                    Preferences.GameSaveRequested = false;
                }
            }
            catch (Exception)
            {
                Preferences.GameSaveRequested = false;
            }
        }

        // Token: 0x060003CF RID: 975 RVA: 0x00014FB4 File Offset: 0x000131B4
        public static bool SaveToStream(Stream stream)
        {
            bool flag = false;
            bool flag2;
            try
            {
                BinaryWriter binaryWriter = new(stream);
                binaryWriter.Write(Preferences.data_.Count);
                foreach (KeyValuePair<string, int> item in Preferences.data_)
                {
                    binaryWriter.Write(item.Key);
                    binaryWriter.Write(item.Value);
                }
                binaryWriter.Write(Preferences.dataStrings_.Count);
                foreach (KeyValuePair<string, string> item2 in Preferences.dataStrings_)
                {
                    binaryWriter.Write(item2.Key);
                    binaryWriter.Write(item2.Value);
                }
                binaryWriter.Close();
                flag = true;
                flag2 = flag;
            }
            catch (Exception ex)
            {
                FrameworkTypes._LOG("Error: cannot save, " + ex.ToString());
                flag2 = flag;
            }
            return flag2;
        }

        // Token: 0x060003D0 RID: 976 RVA: 0x000150D0 File Offset: 0x000132D0
        public static bool LoadFromStream(Stream stream)
        {
            bool flag = false;
            bool flag2;
            try
            {
                BinaryReader binaryReader = new(stream);
                int num = binaryReader.ReadInt32();
                for (int i = 0; i < num; i++)
                {
                    string key = binaryReader.ReadString();
                    int value = binaryReader.ReadInt32();
                    Preferences.data_.Add(key, value);
                }
                num = binaryReader.ReadInt32();
                for (int j = 0; j < num; j++)
                {
                    string key2 = binaryReader.ReadString();
                    string value2 = binaryReader.ReadString();
                    Preferences.dataStrings_.Add(key2, value2);
                }
                binaryReader.Close();
                flag = true;
                flag2 = flag;
            }
            catch (Exception ex)
            {
                FrameworkTypes._LOG("Error: cannot load, " + ex.ToString());
                flag2 = flag;
            }
            return flag2;
        }

        // Token: 0x060003D1 RID: 977 RVA: 0x00015188 File Offset: 0x00013388
        public virtual void loadPreferences()
        {
            Preferences._loadPreferences();
        }

        // Token: 0x060003D2 RID: 978 RVA: 0x00015190 File Offset: 0x00013390
        internal static void _loadPreferences()
        {
            string file = "ctr_save.bin";
            if (File.Exists(file))
            {
                FileStream fileStream = File.OpenRead(file);
                Preferences.LoadFromStream(fileStream);
                fileStream.Close();
            }
        }

        // Token: 0x040002A5 RID: 677
        private static Dictionary<string, int> data_ = new();

        // Token: 0x040002A6 RID: 678
        private static Dictionary<string, string> dataStrings_ = new();

        // Token: 0x040002A7 RID: 679
        public static bool GameSaveRequested = false;
    }
}
