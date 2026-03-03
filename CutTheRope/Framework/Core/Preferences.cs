using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
#if MACOS_AVFOUNDATION
using Foundation;
#endif

namespace CutTheRope.Framework.Core
{
    internal class Preferences : FrameworkTypes
    {
        private const string UnlockedKeyPrefix = "UNLOCKED_";
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
        private const string DynamicBoxSaveFilePrefix = "ctrsave_slot";
        private const string DynamicBoxSaveFileExtension = ".json";
        private const string SaveFolderName = "CutTheRopeDX_SaveData";

        private static string GlobalSaveFilePath => Path.Combine(SaveDirectory, GlobalSaveFileName);

        private static string GetBoxSaveFileName(int slot)
        {
            return $"{DynamicBoxSaveFilePrefix}{slot:D2}{DynamicBoxSaveFileExtension}";
        }

        private static string GetBoxSaveFilePath(int slot)
        {
            return Path.Combine(SaveDirectory, GetBoxSaveFileName(slot));
        }

        private static bool TryParseBoxSlotFromFileName(string fileName, out int slot)
        {
            slot = 0;

            if (string.IsNullOrWhiteSpace(fileName) ||
                !fileName.StartsWith(DynamicBoxSaveFilePrefix, StringComparison.OrdinalIgnoreCase) ||
                !fileName.EndsWith(DynamicBoxSaveFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            int slotPartLength = fileName.Length - DynamicBoxSaveFilePrefix.Length - DynamicBoxSaveFileExtension.Length;
            if (slotPartLength <= 0)
            {
                return false;
            }

            string slotPart = fileName.Substring(DynamicBoxSaveFilePrefix.Length, slotPartLength);
            return int.TryParse(slotPart, NumberStyles.None, CultureInfo.InvariantCulture, out slot) && slot >= 0;
        }

        // Global preferences (PREFS_*, IAP_*, SOUND_ON, etc.)
        private static readonly Dictionary<string, object> GlobalData = [];

        // Per-box game data (STARS_, SCORE_, UNLOCKED_) — indexed by box index
        private static readonly List<Dictionary<string, object>> BoxData = [];

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
            HashSet<string> filesToMigrate = new([GlobalSaveFileName], StringComparer.OrdinalIgnoreCase);

            if (Directory.Exists(oldDir))
            {
                foreach (string oldSlotFilePath in Directory.EnumerateFiles(oldDir, $"{DynamicBoxSaveFilePrefix}*{DynamicBoxSaveFileExtension}"))
                {
                    string fileName = Path.GetFileName(oldSlotFilePath);
                    if (TryParseBoxSlotFromFileName(fileName, out _))
                    {
                        _ = filesToMigrate.Add(fileName);
                    }
                }
            }

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

        // ── Global accessors (PREFS_*, IAP_*, SOUND_ON, etc.) ────────────────────

        /// <summary>
        /// Sets an integer preference and optionally saves to disk.
        /// </summary>
        public static void SetIntForKey(int value, string key, bool commit = false)
        {
            GlobalData[key] = value;
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
            GlobalData[key] = value;
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
            GlobalData[key] = value;
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
            return GlobalData.TryGetValue(key, out object value)
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
            return GlobalData.TryGetValue(key, out object value) && value is bool boolVal && boolVal;
        }

        /// <summary>
        /// Gets a string preference. Returns empty string if not found.
        /// </summary>
        public static string GetStringForKey(string key)
        {
            return GlobalData.TryGetValue(key, out object value) && value is string strVal ? strVal : "";
        }

        /// <summary>
        /// Checks if a global preference key exists in memory.
        /// </summary>
        protected static bool ContainsKey(string key)
        {
            return GlobalData.ContainsKey(key);
        }

        protected static void RemoveKey(string key)
        {
            _ = GlobalData.Remove(key);
        }

        // ── Box-scoped accessors (STARS_, SCORE_, UNLOCKED_ per box) ─────────────

        public static void SetBoxIntForKey(int box, int value, string key, bool commit = false)
        {
            EnsureBoxData(box)[key] = value;
            if (commit)
            {
                RequestSave();
            }
        }

        public static int GetBoxIntForKey(int box, string key)
        {
            return box >= BoxData.Count
                ? 0
                : BoxData[box].TryGetValue(key, out object value)
                ? value switch
                {
                    int intVal => intVal,
                    long longVal => (int)longVal,
                    _ => 0
                }
                : 0;
        }

        public static void SetBoxBoolForKey(int box, bool value, string key, bool commit = false)
        {
            EnsureBoxData(box)[key] = value;
            if (commit)
            {
                RequestSave();
            }
        }

        public static bool GetBoxBoolForKey(int box, string key)
        {
            return box < BoxData.Count && BoxData[box].TryGetValue(key, out object value) && value is bool boolVal && boolVal;
        }

        public static void SetBoxStringForKey(int box, string value, string key, bool commit = false)
        {
            EnsureBoxData(box)[key] = value;
            if (commit)
            {
                RequestSave();
            }
        }

        public static string GetBoxStringForKey(int box, string key)
        {
            return box >= BoxData.Count ? "" : BoxData[box].TryGetValue(key, out object value) && value is string strVal ? strVal : "";
        }

        public static void RemoveBoxKey(int box, string key)
        {
            if (box < BoxData.Count)
            {
                _ = BoxData[box].Remove(key);
            }
        }

        /// <summary>
        /// Clears all in-memory per-box preference dictionaries.
        /// </summary>
        protected static void ClearAllBoxData()
        {
            foreach (Dictionary<string, object> dict in BoxData)
            {
                dict.Clear();
            }
        }

        private static Dictionary<string, object> EnsureBoxData(int box)
        {
            while (BoxData.Count <= box)
            {
                BoxData.Add([]);
            }

            return BoxData[box];
        }

        // ── Serialization ─────────────────────────────────────────────────────────

        /// <summary>
        /// Serializes a preferences dictionary to JSON string (AOT-safe).
        /// </summary>
        private static string SerializeToJson(Dictionary<string, object> data)
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();
                foreach (KeyValuePair<string, object> kvp in data.OrderBy(kvp => kvp.Key, StringComparer.Ordinal))
                {
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
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// Returns true if key belongs to per-box game data (STARS_, SCORE_, UNLOCKED_).
        /// </summary>
        private static bool IsGameDataKey(string key)
        {
            return key.StartsWith("STARS_", StringComparison.Ordinal) ||
            key.StartsWith("SCORE_", StringComparison.Ordinal) ||
            key.StartsWith("UNLOCKED_", StringComparison.Ordinal);
        }

        private static void WritePreferenceFiles()
        {
            File.WriteAllText(GlobalSaveFilePath, SerializeToJson(GlobalData));
            for (int b = 0; b < BoxData.Count; b++)
            {
                File.WriteAllText(GetBoxSaveFilePath(b), SerializeToJson(BoxData[b]));
            }
        }

        /// <summary>
        /// Deserializes JSON into the destination dictionary (AOT-safe).
        /// </summary>
        private static bool DeserializeFromJson(string json, Dictionary<string, object> dest)
        {
            bool didMigrate = false;
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
            {
                if (TryReadJsonValue(prop.Name, prop.Value, out object parsedValue, out bool migrated))
                {
                    dest[prop.Name] = parsedValue;
                    didMigrate |= migrated;
                }
            }

            return didMigrate;
        }

        /// <summary>
        /// Deserializes JSON routing game data keys to <paramref name="gameDataDest"/>
        /// and all other keys to <paramref name="globalDest"/>. Used for migration of old
        /// save files that mixed global prefs and game data in one file.
        /// </summary>
        private static bool DeserializeFromJsonRouted(
            string json,
            Dictionary<string, object> globalDest,
            Dictionary<string, object> gameDataDest)
        {
            bool didMigrate = false;
            using JsonDocument doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
            {
                Dictionary<string, object> dest = IsGameDataKey(prop.Name) ? gameDataDest : globalDest;
                if (TryReadJsonValue(prop.Name, prop.Value, out object parsedValue, out bool migrated))
                {
                    dest[prop.Name] = parsedValue;
                    didMigrate |= migrated;
                }
            }

            return didMigrate;
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
        /// Serializes all preferences to a JSON stream (global prefs only).
        /// </summary>
        public static bool SaveToStream(Stream stream)
        {
            try
            {
                using StreamWriter writer = new(stream);
                writer.Write(SerializeToJson(GlobalData));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: cannot save, {ex}");
                return false;
            }
        }

        /// <summary>
        /// Deserializes all preferences from a JSON stream (global prefs only).
        /// </summary>
        public static bool LoadFromStream(Stream stream)
        {
            try
            {
                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();
                GlobalData.Clear();
                _ = DeserializeFromJson(json, GlobalData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: cannot load, {ex}");
                return false;
            }
        }

        /// <summary>
        /// Loads preferences from disk. Global prefs go to GlobalData; per-box game data
        /// goes to BoxData[b]. Old saves that mixed data in one file are routed automatically.
        /// </summary>
        public static void LoadPreferences()
        {
            GlobalData.Clear();
            foreach (Dictionary<string, object> dict in BoxData)
            {
                dict.Clear();
            }

            bool needsSave = false;

            // Load global prefs file. Route any stale game data (old pre-split saves) to BoxData[0].
            if (File.Exists(GlobalSaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(GlobalSaveFilePath);
                    bool migrated = DeserializeFromJsonRouted(json, GlobalData, EnsureBoxData(0));
                    // If the global file had game data keys in it, we need to rewrite the split files.
                    needsSave |= migrated || BoxData[0].Count > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading global JSON preferences: {ex}");
                }
            }

            // Load dynamic slot save files
            if (Directory.Exists(SaveDirectory))
            {
                foreach (string boxFilePath in Directory.EnumerateFiles(SaveDirectory, $"{DynamicBoxSaveFilePrefix}*{DynamicBoxSaveFileExtension}"))
                {
                    string fileName = Path.GetFileName(boxFilePath);
                    if (!TryParseBoxSlotFromFileName(fileName, out int slot))
                    {
                        continue;
                    }
                    try
                    {
                        string json = File.ReadAllText(boxFilePath);
                        // Route non-game-data keys (IAP_*, SOUND_ON, etc.) to GlobalData for migration compat.
                        bool migrated = DeserializeFromJsonRouted(json, GlobalData, EnsureBoxData(slot));
                        needsSave |= migrated;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading {fileName}: {ex}");
                    }
                }
            }

            if (needsSave)
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
