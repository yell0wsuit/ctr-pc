using CutTheRope.ios;
using System;
using System.Collections.Generic;
using System.IO;

namespace CutTheRope.iframework.core
{
    internal class Preferences : NSObject
    {
        public override NSObject init()
        {
            if (base.init() == null)
            {
                return null;
            }
            Preferences._loadPreferences();
            return this;
        }

        public virtual void setIntforKey(int v, string k, bool comit)
        {
            Preferences._setIntforKey(v, k, comit);
        }

        public virtual void setBooleanforKey(bool v, string k, bool comit)
        {
            Preferences._setBooleanforKey(v, k, comit);
        }

        public virtual void setStringforKey(string v, string k, bool comit)
        {
            Preferences._setStringforKey(v, k, comit);
        }

        public virtual int getIntForKey(string k)
        {
            return Preferences._getIntForKey(k);
        }

        public virtual float getFloatForKey(string k)
        {
            return 0f;
        }

        public virtual bool getBooleanForKey(string k)
        {
            return Preferences._getBooleanForKey(k);
        }

        public virtual string getStringForKey(string k)
        {
            return Preferences._getStringForKey(k);
        }

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

        public static int _getIntForKey(string k)
        {
            int value;
            if (Preferences.data_.TryGetValue(k, out value))
            {
                return value;
            }
            return 0;
        }

        private static float _getFloatForKey(string k)
        {
            return 0f;
        }

        public static bool _getBooleanForKey(string k)
        {
            return Preferences._getIntForKey(k) != 0;
        }

        public static void _setBooleanforKey(bool v, string k, bool comit)
        {
            Preferences._setIntforKey(v ? 1 : 0, k, comit);
        }

        private static string _getStringForKey(string k)
        {
            string value;
            if (Preferences.dataStrings_.TryGetValue(k, out value))
            {
                return value;
            }
            return "";
        }

        public virtual void savePreferences()
        {
            Preferences._savePreferences();
        }

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

        public virtual void loadPreferences()
        {
            Preferences._loadPreferences();
        }

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

        private static Dictionary<string, int> data_ = new();

        private static Dictionary<string, string> dataStrings_ = new();

        public static bool GameSaveRequested = false;
    }
}
