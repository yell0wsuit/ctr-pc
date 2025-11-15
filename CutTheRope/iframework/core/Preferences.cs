using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CutTheRope.iframework.core
{
    internal class Preferences : FrameworkTypes
    {
        private static readonly Dictionary<string, object> PreferencesData = [];
        private const string SaveFileName = "ctr_preferences.json";
        private const string LegacyBinaryFileName = "ctr_save.bin";
        private const string MigratedBinaryFileName = "ctr_bin_candeletethis.bin";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static bool GameSaveRequested { get; set; }

        public Preferences()
        {
            LoadPreferences();
        }

        /// <summary>
        /// Sets an integer preference and optionally saves to disk.
        /// </summary>
        public static void SetIntForKey(int value, string key, bool commit = false)
        {
            PreferencesData[key] = value;
            if (commit)
            {
                RequestSave();
            }
        }

        /// <summary>
        /// Sets a boolean preference and optionally saves to disk.
        /// </summary>
        public static void SetBooleanForKey(bool value, string key, bool commit = false)
        {
            SetIntForKey(value ? 1 : 0, key, commit);
        }

        /// <summary>
        /// Sets a string preference and optionally saves to disk.
        /// </summary>
        public static void SetStringForKey(string value, string key, bool commit = false)
        {
            PreferencesData[key] = value;
            if (commit)
            {
                RequestSave();
            }
        }

        /// <summary>
        /// Gets an integer preference. Returns 0 if not found.
        /// </summary>
        public static int GetIntForKey(string key)
        {
            return PreferencesData.TryGetValue(key, out object value)
                ? value switch
                {
                    int intVal => intVal,
                    long longVal => (int)longVal,
                    JsonElement jsonElement => jsonElement.GetInt32(),
                    _ => 0
                }
                : 0;
        }

        /// <summary>
        /// Gets a boolean preference. Returns false if not found.
        /// </summary>
        public static bool GetBooleanForKey(string key)
        {
            return GetIntForKey(key) != 0;
        }

        /// <summary>
        /// Gets a string preference. Returns empty string if not found.
        /// </summary>
        public static string GetStringForKey(string key)
        {
            return PreferencesData.TryGetValue(key, out object value)
                ? value switch
                {
                    string strVal => strVal,
                    JsonElement jsonElement => jsonElement.GetString() ?? "",
                    _ => ""
                }
                : "";
        }

        /// <summary>
        /// Requests the preferences to be saved on the next Update call.
        /// </summary>
        public static void RequestSave()
        {
            if (!GameSaveRequested)
            {
                GameSaveRequested = true;
            }
        }

        /// <summary>
        /// Saves pending preferences to disk if requested.
        /// Called once per frame by the game loop.
        /// </summary>
        public static void Update()
        {
            if (!GameSaveRequested)
            {
                return;
            }

            try
            {
                string json = JsonSerializer.Serialize(PreferencesData, JsonOptions);
                File.WriteAllText(SaveFileName, json);
                GameSaveRequested = false;
            }
            catch (Exception ex)
            {
                LOG($"Error saving preferences: {ex}");
                GameSaveRequested = false;
            }
        }

        /// <summary>
        /// Serializes all preferences to a JSON stream.
        /// </summary>
        public static bool SaveToStream(Stream stream)
        {
            try
            {
                string json = JsonSerializer.Serialize(PreferencesData, JsonOptions);
                using StreamWriter writer = new(stream);
                writer.Write(json);
                return true;
            }
            catch (Exception ex)
            {
                LOG($"Error: cannot save, {ex}");
                return false;
            }
        }

        /// <summary>
        /// Deserializes all preferences from a JSON stream.
        /// </summary>
        public static bool LoadFromStream(Stream stream)
        {
            try
            {
                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();
                Dictionary<string, object> data = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);

                if (data != null)
                {
                    PreferencesData.Clear();
                    foreach (KeyValuePair<string, object> kvp in data)
                    {
                        PreferencesData[kvp.Key] = kvp.Value;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LOG($"Error: cannot load, {ex}");
                return false;
            }
        }

        /// <summary>
        /// Loads preferences from disk if the save file exists.
        /// Supports migration from legacy binary format to JSON.
        /// </summary>
        public static void LoadPreferences()
        {
            PreferencesData.Clear();

            // Try to load from JSON first (preferred format)
            if (File.Exists(SaveFileName))
            {
                try
                {
                    string json = File.ReadAllText(SaveFileName);
                    Dictionary<string, object> data = JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions);

                    if (data != null)
                    {
                        foreach (KeyValuePair<string, object> kvp in data)
                        {
                            PreferencesData[kvp.Key] = kvp.Value;
                        }
                    }
                    return;
                }
                catch (Exception ex)
                {
                    LOG($"Error loading JSON preferences: {ex}");
                }
            }

            // Fall back to legacy binary format
            if (File.Exists(LegacyBinaryFileName))
            {
                try
                {
                    using FileStream fileStream = File.OpenRead(LegacyBinaryFileName);
                    if (LoadLegacyBinaryFormat(fileStream))
                    {
                        LOG("Successfully migrated preferences from binary to JSON format");

                        // Save as JSON
                        try
                        {
                            string json = JsonSerializer.Serialize(PreferencesData, JsonOptions);
                            File.WriteAllText(SaveFileName, json);
                        }
                        catch (Exception ex)
                        {
                            LOG($"Error saving migrated preferences as JSON: {ex}");
                        }

                        // Rename old binary file
                        try
                        {
                            if (File.Exists(MigratedBinaryFileName))
                            {
                                File.Delete(MigratedBinaryFileName);
                            }

                            File.Move(LegacyBinaryFileName, MigratedBinaryFileName);
                            LOG($"Moved legacy binary to {MigratedBinaryFileName}");
                        }
                        catch (Exception ex)
                        {
                            LOG($"Error renaming legacy binary file: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LOG($"Error loading legacy binary preferences: {ex}");
                }
            }
        }

        /// <summary>
        /// Loads preferences from legacy binary format.
        /// </summary>
        private static bool LoadLegacyBinaryFormat(Stream stream)
        {
            try
            {
                using BinaryReader reader = new(stream);

                // Load integers
                int intCount = reader.ReadInt32();
                for (int i = 0; i < intCount; i++)
                {
                    string key = reader.ReadString();
                    int value = reader.ReadInt32();
                    PreferencesData[key] = value;
                }

                // Load strings
                int stringCount = reader.ReadInt32();
                for (int i = 0; i < stringCount; i++)
                {
                    string key = reader.ReadString();
                    string value = reader.ReadString();
                    PreferencesData[key] = value;
                }

                return true;
            }
            catch (Exception ex)
            {
                LOG($"Error: cannot load legacy binary format, {ex}");
                return false;
            }
        }

        /// <summary>
        /// Instance method for compatibility. Loads preferences.
        /// </summary>
        public virtual void loadPreferences()
        {
            LoadPreferences();
        }

        /// <summary>
        /// Instance method for compatibility. Requests save.
        /// </summary>
        public virtual void SavePreferences()
        {
            RequestSave();
        }

    }
}
