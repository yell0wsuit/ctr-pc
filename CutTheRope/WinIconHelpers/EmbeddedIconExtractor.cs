#if WINDOWS
using System;
using System.IO;
using System.Reflection;

/// <summary>
/// Provides helper methods for extracting embedded icon resources
/// from the executing assembly to a temporary file on disk.
/// </summary>
/// <remarks>
/// This is primarily intended for Windows-specific scenarios
/// to set an .ico file to the window title bar when used
/// with <c>PublishSingleFile=true</c>.
/// This file will be removed once the project is moved to
/// DesktopGL backend.
/// </remarks>
internal static class EmbeddedIconExtractor
{
    /// <summary>
    /// Extracts an embedded icon resource to a uniquely named temporary
    /// <c>.ico</c> file and returns the file path.
    /// </summary>
    /// <param name="resourceName">
    /// The fully qualified manifest resource name of the embedded icon
    /// (including namespace and file name).
    /// </param>
    /// <returns>
    /// The full file system path to the extracted temporary <c>.ico</c> file.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the specified resource cannot be found in the executing assembly.
    /// </exception>
    /// <remarks>
    /// The caller is responsible for cleaning up the generated temporary file
    /// when it is no longer needed.
    /// </remarks>
    public static string ExtractToTemp(string resourceName)
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream s = asm.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Missing resource: {resourceName}");
        string path = Path.Combine(
            Path.GetTempPath(),
            $"ctr_icon_{Guid.NewGuid():N}.ico"
        );

        using FileStream fs = File.Create(path);
        s.CopyTo(fs);

        return path;
    }
}
#endif
