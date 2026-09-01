using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using CutTheRopeDX.GameMain.Tutorials;
using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    /// <summary>
    /// The shipped corpus is entirely on the authored schema. Every tutorial element in every map
    /// goes through the production parser here, so a typo in any locale copy of any level fails
    /// the suite rather than showing up as a prompt that silently never plays.
    /// </summary>
    public sealed class TutorialContentSchemaTests
    {
        [Fact]
        public void EveryShippedMapValidatesThroughTheProductionParser()
        {
            List<string> maps = [.. MapPaths()];
            Assert.NotEmpty(maps);

            foreach (string path in maps)
            {
                XElement map = XElement.Load(path);
                List<XElement> nodes = [.. TutorialNodes(map)];
                if (nodes.Count == 0)
                {
                    continue;
                }

                TutorialPromptLoader loader = new(
                    new TutorialDirector(new EmptyWorld()),
                    new FakeVisualFactory(),
                    source: Path.GetFileName(path),
                    locale: "en",
                    twoParts: IsTwoParts(map),
                    scale: 3f,
                    offsetX: 0f,
                    offsetY: 0f,
                    mapOffsetX: 0,
                    mapOffsetY: 0);

                _ = loader.LoadAll(nodes);
            }
        }

        [Fact]
        public void NoMapKeepsTheLegacySpecialAttribute()
        {
            List<string> offenders =
            [
                .. from path in MapPaths()
                   where XElement.Load(path).DescendantsAndSelf().Any(node => node.Attribute("special") != null)
                   select Path.GetFileName(path)
            ];

            Assert.Empty(offenders);
        }

        /// <summary>1_5 stages two region-scoped bubbled prompts in every locale.</summary>
        [Fact]
        public void BubbledRegionLevelStagesTwoScopedPromptsPerLocale()
        {
            foreach (IGrouping<string, XElement> locale in NodesByLocale("1_5.xml"))
            {
                List<XElement> triggered =
                [
                    .. locale.Where(node => node.Attribute("showOn")?.Value == "bubbled")
                ];

                Assert.Equal(2, triggered.Count);
                Assert.All(triggered, node =>
                    Assert.Equal("133,0,186,133", node.Attribute("inArea")?.Value));
            }
        }

        /// <summary>1_1 keeps one swipe per locale and the ten-second hold the loader hardcoded.</summary>
        [Fact]
        public void SwipeLevelKeepsOneSwipePerLocaleAndItsAuthoredHold()
        {
            foreach (IGrouping<string, XElement> locale in NodesByLocale("1_1.xml"))
            {
                _ = Assert.Single(locale, node => node.Attribute("anim")?.Value == "swipe");
                Assert.All(locale, node => Assert.Equal("10", node.Attribute("duration")?.Value));
            }
        }

        /// <summary>14_1 triggers its second prompt on lantern capture in every locale.</summary>
        [Fact]
        public void LanternLevelTriggersItsSecondPromptOnCapture()
        {
            foreach (IGrouping<string, XElement> locale in NodesByLocale("14_1.xml"))
            {
                _ = Assert.Single(locale, node => node.Attribute("showOn")?.Value == "lanternCatch");
            }
        }

        /// <summary>15_1 triggers both of its prompts on the mouse grab, in every locale.</summary>
        [Fact]
        public void MouseLevelTriggersBothPromptsOnGrab()
        {
            foreach (IGrouping<string, XElement> locale in NodesByLocale("15_1.xml"))
            {
                Assert.Equal(2, locale.Count(node => node.Attribute("showOn")?.Value == "mouseGrab"));
            }
        }

        /// <summary>17_1's dead id-5 metadata is gone and none of its prompts gained a trigger.</summary>
        [Fact]
        public void DeadMetadataLevelCarriesNoTriggerAtAll()
        {
            XElement map = XElement.Load(MapPath("17_1.xml"));

            Assert.Null(map.Descendants("gameDesign").Single().Attribute("special"));
            Assert.All(TutorialNodes(map), node => Assert.Null(node.Attribute("showOn")));
        }

        private static List<IGrouping<string, XElement>> NodesByLocale(string mapFileName)
        {
            XElement map = XElement.Load(MapPath(mapFileName));
            List<IGrouping<string, XElement>> locales =
            [
                .. TutorialNodes(map).GroupBy(node => node.Attribute("locale")?.Value ?? string.Empty)
            ];

            Assert.NotEmpty(locales);
            return locales;
        }

        private static string MapPath(string mapFileName)
        {
            return ContentPaths.GetMapPath(mapFileName);
        }

        private static IEnumerable<string> MapPaths()
        {
            return Directory.EnumerateFiles(
                ContentPaths.GetContentPath(ContentPaths.MapsDirectory),
                "*.xml");
        }

        private static IEnumerable<XElement> TutorialNodes(XElement map)
        {
            return map.Descendants().Where(node => IsTutorialElement(node.Name.LocalName));
        }

        private static bool IsTutorialElement(string name)
        {
            return name == "tutorialText"
                || (name.StartsWith("tutorial", StringComparison.Ordinal)
                    && name.Length > 8
                    && name[8..].All(char.IsAsciiDigit));
        }

        private static bool IsTwoParts(XElement map)
        {
            return map.Descendants("gameDesign")
                .Any(node => node.Attribute("twoParts")?.Value == "true");
        }
    }
}
