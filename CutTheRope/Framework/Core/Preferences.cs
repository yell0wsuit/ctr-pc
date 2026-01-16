using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CutTheRope.Framework.Core
{
    internal class Preferences : FrameworkTypes
    {
        private static readonly Dictionary<string, object> PreferencesData = [];
        private const string SaveFileName = "ctr_preferences.json";
        private const string LegacyBinaryFileName = "ctr_save.bin";
        private const string MigratedBinaryFileName = "ctr_bin_candeletethis.bin";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
        private const string SaveFolderName = "CutTheRopeDX_SaveData";
        private static string SaveFilePath => Path.Combine(SaveDirectory, SaveFileName);
        private static string LegacyBinaryFilePath => Path.Combine(SaveDirectory, LegacyBinaryFileName);
        private static string MigratedBinaryFilePath => Path.Combine(SaveDirectory, MigratedBinaryFileName);
        public static bool GameSaveRequested { get; set; }

        private static string _saveDirectory;

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
                if (_saveDirectory == null)
                {
                    _saveDirectory = DetermineSaveDirectory();
                    Console.WriteLine($"[Preferences] Using save directory: {_saveDirectory}");
                }
                return _saveDirectory;
            }
        }

        /// <summary>
        /// Determines the best available save directory based on writability and platform constraints.
        /// </summary>
        /// <returns>The path to the save directory.</returns>
        private static string DetermineSaveDirectory()
        {
            // 1. Try executable directory first (excluding macOS .app bundle)
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

            // 2. Fallback to Documents/{SaveFolderName}
            string documentsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                SaveFolderName);
            if (TryCreateDirectory(documentsDir))
            {
                return documentsDir;
            }

            // 3. Final fallback to LocalApplicationData/{SaveFolderName}
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
            string[] filesToMigrate = [SaveFileName, LegacyBinaryFileName, MigratedBinaryFileName];

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
        /// Checks if a preference key exists in memory.
        /// This might be removed once the setting UI is implemented.
        /// </summary>
        protected static bool ContainsKey(string key)
        {
            return PreferencesData.ContainsKey(key);
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
                File.WriteAllText(SaveFilePath, json);
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
                string json = JsonSerializer.Serialize(PreferencesData, JsonOptions);
                using StreamWriter writer = new(stream);
                writer.Write(json);
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
                Console.WriteLine($"Error: cannot load, {ex}");
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
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
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
                    Console.WriteLine($"Error loading JSON preferences: {ex}");
                }
            }

            // Fall back to legacy binary format
            if (File.Exists(LegacyBinaryFilePath))
            {
                try
                {
                    using FileStream fileStream = File.OpenRead(LegacyBinaryFilePath);
                    if (LoadLegacyBinaryFormat(fileStream))
                    {
                        Console.WriteLine("Successfully migrated preferences from binary to JSON format");

                        // Save as JSON
                        try
                        {
                            string json = JsonSerializer.Serialize(PreferencesData, JsonOptions);
                            File.WriteAllText(SaveFilePath, json);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error saving migrated preferences as JSON: {ex}");
                        }

                        // Rename old binary file
                        try
                        {
                            if (File.Exists(MigratedBinaryFilePath))
                            {
                                File.Delete(MigratedBinaryFilePath);
                            }

                            File.Move(LegacyBinaryFilePath, MigratedBinaryFilePath);
                            Console.WriteLine($"Moved legacy binary to {MigratedBinaryFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error renaming legacy binary file: {ex}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading legacy binary preferences: {ex}");
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
                Console.WriteLine($"Error: cannot load legacy binary format, {ex}");
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
