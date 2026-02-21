using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
#if MACOS_AVFOUNDATION
using Foundation;
#endif

namespace CutTheRope.Framework.Core
{
    internal class Preferences : FrameworkTypes
    {
        private const string UnlockedKeyPrefix = "UNLOCKED_";
        private const string GlobalPreferencesPrefix = "PREFS_";
        private static readonly Dictionary<string, object> PreferencesData = [];
        private static readonly HashSet<string> BooleanPreferenceKeys =
        [
            "PREFS_EXIST",
            "PREFS_CANDY_WAS_CHANGED",
            "PREFS_GAME_CENTER_ENABLED",
            "PREFS_WINDOW_FULLSCREEN",
            "PREFS_RPC_ENABLED",
            "PREFS_UPDATE_CHECK",
            "PREFS_CLICK_TO_CUT",
            "SOUND_ON",
            "MUSIC_ON",
            "IAP_SHAREWARE",
            "IAP_UNLOCK",
            "IAP_BANNERS"
        ];
        private const string GlobalSaveFileName = "ctr_preferences.json";
        private const string OriginalSaveFileName = "ctroriginal_savefile.json";
        private const string SaveFolderName = "CutTheRopeDX_SaveData";
        private static string GlobalSaveFilePath => Path.Combine(SaveDirectory, GlobalSaveFileName);
        private static string OriginalSaveFilePath => Path.Combine(SaveDirectory, OriginalSaveFileName);
        public static bool GameSaveRequested { get; set; }

        /// <summary>
        /// Gets the save directory with the following fallback priority:
        /// <list type="bullet">
        /// <item>
        /// <description>Next to the executable (preferred for portability)</description>
        /// </item>
        /// <item>
        /// <description>User's Documents folder</description>
        /// </item>
        /// <item>
        /// <description>LocalApplicationData (final fallback)</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// Todo: Add custom save directory when setting UI is implemented.
        /// </remarks>
        private static string SaveDirectory
        {
            get
            {
                if (field == null)
                {
                    field = DetermineSaveDirectory();
                    Console.WriteLine($"[Preferences] Using save directory: {field}");
                }
                return field;
            }
        }

        /// <summary>
        /// Determines the best available save directory based on writability and platform constraints.
        /// </summary>
        /// <returns>The path to the save directory.</returns>
        private static string DetermineSaveDirectory()
        {
#if MACOS_AVFOUNDATION
            // On macOS, if not in .app bundle (dev mode), try executable directory
            if (!IsInsideMacAppBundle())
            {
                string exeDir = AppContext.BaseDirectory;
                string exeSaveDir = Path.Combine(exeDir, SaveFolderName);
                if (TryCreateDirectory(exeSaveDir))
                {
                    MigrateOldSaveFiles(exeDir, exeSaveDir);
                    return exeSaveDir;
                }
            }
            // Otherwise fall through to Documents folder below
#else
            // On non-macOS, try executable directory first (excluding macOS .app bundle)
            string exeDir = AppContext.BaseDirectory;
            if (!IsInsideMacAppBundle(exeDir))
            {
                string exeSaveDir = Path.Combine(exeDir, SaveFolderName);
                if (TryCreateDirectory(exeSaveDir))
                {
                    MigrateOldSaveFiles(exeDir, exeSaveDir);
                    return exeSaveDir;
                }
            }
#endif

            // Fallback to Documents/{SaveFolderName}
            string documentsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                SaveFolderName);
            if (TryCreateDirectory(documentsDir))
            {
                return documentsDir;
            }

            // Final fallback to LocalApplicationData/{SaveFolderName}
            string localAppDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SaveFolderName);
            if (TryCreateDirectory(localAppDataDir))
            {
                return localAppDataDir;
            }

            // Last resort: current directory
            Console.WriteLine("[Preferences] Warning: All save directory options failed, using current directory");
            return ".";
        }

        /// <summary>
        /// Migrates save files from an old location to a new directory.
        /// Only moves files that exist in the old location and don't exist in the new location.
        /// </summary>
        /// <param name="oldDir">The old directory containing save files.</param>
        /// <param name="newDir">The new directory to move save files to.</param>
        private static void MigrateOldSaveFiles(string oldDir, string newDir)
        {
            string[] filesToMigrate = [GlobalSaveFileName, OriginalSaveFileName];

            foreach (string fileName in filesToMigrate)
            {
                string oldPath = Path.Combine(oldDir, fileName);
                string newPath = Path.Combine(newDir, fileName);

                if (File.Exists(oldPath) && !File.Exists(newPath))
                {
                    try
                    {
                        File.Move(oldPath, newPath);
                        Console.WriteLine($"[Preferences] Migrated {fileName} to new save directory");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Preferences] Failed to migrate {fileName}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Tests whether a directory is writable by creating and deleting a temporary file.
        /// </summary>
        /// <param name="path">The directory path to test.</param>
        /// <returns><c>true</c> if the directory is writable; otherwise, <c>false</c>.</returns>
        private static bool IsDirectoryWritable(string path)
        {
            try
            {
                string testFile = Path.Combine(path, ".write_test_" + Guid.NewGuid().ToString("N"));
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to create a directory and verifies it is writable.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        /// <returns><c>true</c> if the directory exists and is writable; otherwise, <c>false</c>.</returns>
        private static bool TryCreateDirectory(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    _ = Directory.CreateDirectory(path);
                }
                return IsDirectoryWritable(path);
            }
            catch
            {
                return false;
            }
        }

#if MACOS_AVFOUNDATION
        /// <summary>
        /// Determines whether the app is running from inside a macOS .app bundle using NSBundle.
        /// </summary>
        /// <returns><c>true</c> if running from a .app bundle; otherwise, <c>false</c>.</returns>
        private static bool IsInsideMacAppBundle()
        {
            string bundlePath = NSBundle.MainBundle.BundlePath;
            return bundlePath.EndsWith(".app", StringComparison.OrdinalIgnoreCase);
        }

#else
        /// <summary>
        /// Determines whether the given path is inside a macOS .app bundle.
        /// Checks for the standard bundle structure: *.app/Contents/MacOS/
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns><c>true</c> if the path is inside a macOS .app bundle; otherwise, <c>false</c>.</returns>
        private static bool IsInsideMacAppBundle(string path)
        {
            DirectoryInfo dir = new(path);

            while (dir != null)
            {
                if (dir.Name.Equals("MacOS", StringComparison.OrdinalIgnoreCase) &&
                    dir.Parent?.Name.Equals("Contents", StringComparison.OrdinalIgnoreCase) == true &&
                    dir.Parent.Parent?.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }

                dir = dir.Parent;
            }

            return false;
        }
#endif

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
            PreferencesData[key] = value;
            if (commit)
            {
                RequestSave();
            }
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
                    _ => 0
                }
                : 0;
        }

        /// <summary>
        /// Gets a boolean preference. Returns false if not found.
        /// </summary>
        public static bool GetBooleanForKey(string key)
        {
            return PreferencesData.TryGetValue(key, out object value) && value is bool boolVal && boolVal;
        }

        /// <summary>
        /// Gets a string preference. Returns empty string if not found.
        /// </summary>
        public static string GetStringForKey(string key)
        {
            return PreferencesData.TryGetValue(key, out object value) && value is string strVal ? strVal : "";
        }

        /// <summary>
        /// Checks if a preference key exists in memory.
        /// This might be removed once the setting UI is implemented.
        /// </summary>
        protected static bool ContainsKey(string key)
        {
            return PreferencesData.ContainsKey(key);
        }

        protected static void RemoveKey(string key)
        {
            _ = PreferencesData.Remove(key);
        }

        /// <summary>
        /// Serializes preferences dictionary to JSON string (AOT-safe).
        /// </summary>
        private static string SerializeToJson(Func<string, bool> includeKey)
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                foreach (KeyValuePair<string, object> kvp in PreferencesData)
                {
                    if (!includeKey(kvp.Key))
                    {
                        continue;
                    }

                    writer.WritePropertyName(kvp.Key);
                    switch (kvp.Value)
                    {
                        case int intVal:
                            writer.WriteNumberValue(intVal);
                            break;
                        case long longVal:
                            writer.WriteNumberValue(longVal);
                            break;
                        case bool boolVal:
                            writer.WriteBooleanValue(boolVal);
                            break;
                        case string strVal:
                            writer.WriteStringValue(strVal);
                            break;
                        default:
                            writer.WriteNullValue();
                            break;
                    }
                }
                writer.WriteEndObject();
            }
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }

        private static bool IsGlobalPreferenceKey(string key)
        {
            return key.StartsWith(GlobalPreferencesPrefix, StringComparison.Ordinal);
        }

        private static bool JsonContainsNonGlobalKeys(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
            {
                if (!IsGlobalPreferenceKey(prop.Name))
                {
                    return true;
                }
            }

            return false;
        }

        private static void WritePreferenceFiles()
        {
            File.WriteAllText(GlobalSaveFilePath, SerializeToJson(IsGlobalPreferenceKey));
            File.WriteAllText(OriginalSaveFilePath, SerializeToJson(key => !IsGlobalPreferenceKey(key)));
        }

        /// <summary>
        /// Deserializes JSON string into PreferencesData dictionary (AOT-safe).
        /// </summary>
        private static bool DeserializeFromJson(string json)
        {
            bool didMigrateBooleanValues = false;
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
            {
                if (TryReadJsonValue(prop.Name, prop.Value, out object parsedValue, out bool migratedBooleanValue))
                {
                    PreferencesData[prop.Name] = parsedValue;
                    didMigrateBooleanValues |= migratedBooleanValue;
                }
            }

            return didMigrateBooleanValues;
        }

        private static bool TryReadJsonValue(string key, JsonElement element, out object parsedValue, out bool migratedBooleanValue)
        {
            migratedBooleanValue = false;

            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    if (key.StartsWith(UnlockedKeyPrefix, StringComparison.Ordinal) &&
                        element.TryGetInt64(out long legacyUnlockedState))
                    {
                        parsedValue = legacyUnlockedState > 0;
                        migratedBooleanValue = true;
                        return true;
                    }

                    if (BooleanPreferenceKeys.Contains(key) && element.TryGetInt64(out long boolNumeric) && (boolNumeric == 0 || boolNumeric == 1))
                    {
                        parsedValue = boolNumeric == 1;
                        migratedBooleanValue = true;
                        return true;
                    }
                    if (element.TryGetInt32(out int intVal))
                    {
                        parsedValue = intVal;
                        return true;
                    }
                    if (element.TryGetInt64(out long longVal))
                    {
                        parsedValue = longVal;
                        return true;
                    }
                    break;
                case JsonValueKind.String:
                    parsedValue = element.GetString() ?? "";
                    return true;
                case JsonValueKind.True:
                    parsedValue = true;
                    return true;
                case JsonValueKind.False:
                    parsedValue = false;
                    return true;
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                case JsonValueKind.Null:
                default:
                    break;
            }

            parsedValue = null;
            return false;
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
                WritePreferenceFiles();
                GameSaveRequested = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving preferences: {ex}");
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
                using StreamWriter writer = new(stream);
                writer.Write(SerializeToJson(_ => true));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: cannot save, {ex}");
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
                PreferencesData.Clear();
                _ = DeserializeFromJson(json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: cannot load, {ex}");
                return false;
            }
        }

        /// <summary>
        /// Loads preferences from disk if the save file exists.
        /// </summary>
        public static void LoadPreferences()
        {
            PreferencesData.Clear();
            bool migratedBooleanValues = false;
            bool needsSaveSplitMigration = false;
            bool hasOriginalSaveFile = File.Exists(OriginalSaveFilePath);

            if (File.Exists(GlobalSaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(GlobalSaveFilePath);
                    migratedBooleanValues |= DeserializeFromJson(json);
                    needsSaveSplitMigration = JsonContainsNonGlobalKeys(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading global JSON preferences: {ex}");
                }
            }

            if (hasOriginalSaveFile)
            {
                try
                {
                    string json = File.ReadAllText(OriginalSaveFilePath);
                    migratedBooleanValues |= DeserializeFromJson(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading original JSON save file: {ex}");
                }
            }

            if (migratedBooleanValues || (needsSaveSplitMigration && !hasOriginalSaveFile))
            {
                try
                {
                    WritePreferenceFiles();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing migrated preference files: {ex}");
                    RequestSave();
                }
            }
        }
    }
}
