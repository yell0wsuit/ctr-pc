using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Runs every string the game ships, in every language, through the text wrapper at the range
    /// of widths a window can produce.
    /// </summary>
    /// <remarks>
    /// A language change rebuilds every menu in turn, so one string the wrapper cannot lay out
    /// takes the whole rebuild down with it and leaves the menus half made. That is what the
    /// Chinese credits did: they are written as a line break followed by an indent, which used to
    /// be able to describe a line of negative length.
    /// </remarks>
    public sealed class LocalizedTextWrapTests
    {
        [Theory]
        [MemberData(nameof(Locales))]
        public void EveryStringWrapsAtEveryWidth(string locale)
        {
            _ = HeadlessGame.Boot();

            foreach ((string key, string value) in StringsIn(locale))
            {
                foreach (float width in new[] { 20f, 60f, 140f, 300f, 700f, 1300f })
                {
                    Text block = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
                    block.wrapLongWords = true;

                    Exception failure = Record.Exception(() => block.SetStringandWidth(value, width));

                    Assert.True(
                        failure == null,
                        $"{locale}/{key} at width {width}: {failure?.GetType().Name}: {failure?.Message}");
                }
            }
        }

        public static TheoryData<string> Locales()
        {
            TheoryData<string> data = [];
            foreach (string file in Directory.GetFiles(LocaleDirectory, "*.json"))
            {
                data.Add(Path.GetFileNameWithoutExtension(file));
            }

            return data;
        }

        /// <summary>Reads the strings a locale file holds.</summary>
        /// <param name="locale">Locale to read.</param>
        /// <returns>Each key and its string.</returns>
        private static IEnumerable<(string Key, string Value)> StringsIn(string locale)
        {
            using JsonDocument document = JsonDocument.Parse(
                File.ReadAllText(Path.Combine(LocaleDirectory, locale + ".json")));

            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    yield return (property.Name, property.Value.GetString());
                }
            }
        }

        /// <summary>Where the shipped locale files sit next to the test binary.</summary>
        private static string LocaleDirectory =>
            Path.Combine(AppContext.BaseDirectory, "content", "locales");
    }
}
