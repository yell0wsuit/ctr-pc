using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace CutTheRopeDX.Browser
{
    /// <summary>Thin managed wrapper over the storage.js localStorage module.</summary>
    internal static partial class StorageInterop
    {
        /// <summary>Imports storage.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("storage", "../storage.js");
        }

        /// <summary>Reads a value, or null when the key is absent.</summary>
        [JSImport("read", "storage")]
        public static partial string Read(string key);

        /// <summary>Writes a value.</summary>
        [JSImport("write", "storage")]
        public static partial void Write(string key, string value);

        /// <summary>Returns every key beginning with a prefix.</summary>
        [JSImport("keysWithPrefix", "storage")]
        public static partial string[] KeysWithPrefix(string prefix);
    }
}
