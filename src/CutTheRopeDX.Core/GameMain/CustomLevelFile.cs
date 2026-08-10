using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Reads and validates externally supplied level XML files.
    /// </summary>
    internal static class CustomLevelFile
    {
        /// <summary>
        /// Attempts to read a level XML file from disk.
        /// </summary>
        /// <param name="path">Absolute path to the level file.</param>
        /// <param name="map">Receives the parsed root element on success; otherwise <see langword="null"/>.</param>
        /// <param name="error">Receives a human-readable failure reason on failure; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when the file was read and parsed; otherwise <see langword="false"/>.</returns>
        public static bool TryLoad(string path, out XElement map, out string error)
        {
            map = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                error = "No level path was supplied.";
                return false;
            }

            if (!File.Exists(path))
            {
                error = "Level file not found: " + path;
                return false;
            }

            try
            {
                map = XDocument.Load(path).Root;
            }
            catch (Exception ex) when (ex is XmlException or IOException or UnauthorizedAccessException)
            {
                error = "Could not read level file " + path + ": " + ex.Message;
                return false;
            }

            if (map == null)
            {
                error = "Level file " + path + " contains no root element.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
