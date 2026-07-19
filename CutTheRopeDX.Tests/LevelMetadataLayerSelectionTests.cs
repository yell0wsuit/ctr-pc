using System.Linq;
using System.Xml.Linq;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LevelMetadataLayerSelectionTests
    {
        [Fact]
        public void SelectLayers_UsesOnlyFirstCaseInsensitiveSettingsLayer()
        {
            XElement map = XElement.Parse("""
                <map>
                  <layer name="settings">
                    <map width="320" height="480" />
                    <gameDesign special="1" />
                  </layer>
                  <layer name="Objects">
                    <candy x="10" y="20" />
                  </layer>
                  <layer name="settings">
                    <map width="999" height="999" />
                    <gameDesign special="9" />
                    <candy x="90" y="90" />
                  </layer>
                  <layer name="Settings">
                    <map width="640" height="960" />
                    <candy x="30" y="40" />
                  </layer>
                </map>
                """);

            XElement[] selected = [.. LevelMetadataLayerSelection.SelectLayers(map)];

            Assert.Equal(["settings", "Objects"],
                selected.Select(layer => layer.Attribute("name")?.Value));
            Assert.Equal("320", selected[0].Element("map")?.Attribute("width")?.Value);
            Assert.Equal("10", selected[1].Element("candy")?.Attribute("x")?.Value);
        }
    }
}
