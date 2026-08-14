using System;
using System.Reflection;

using Microsoft.Xna.Framework;

using Xunit;

namespace CutTheRopeDX.Desktop.Tests
{
    /// <summary>
    /// Pins the desktop resize wiring as it behaves today. The window and presentation rectangle
    /// update, but the older Core REAL_SCREEN globals remain at their startup values.
    /// </summary>
    public sealed class ScreenSizeManagerCharacterizationTests
    {
        [Fact]
        public void WindowResizeDoesNotRefreshCoreRealScreenGlobals()
        {
            Assembly coreAssembly = Assembly.Load("CutTheRopeDX.Core");
            Type frameworkTypes = coreAssembly.GetType("CutTheRopeDX.Framework.FrameworkTypes");
            Assert.NotNull(frameworkTypes);
            FieldInfo realWidth = frameworkTypes.GetField(
                "REAL_SCREEN_WIDTH",
                BindingFlags.Public | BindingFlags.Static);
            FieldInfo realHeight = frameworkTypes.GetField(
                "REAL_SCREEN_HEIGHT",
                BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(realWidth);
            Assert.NotNull(realHeight);
            float startupWidth = Assert.IsType<float>(realWidth.GetValue(null));
            float startupHeight = Assert.IsType<float>(realHeight.GetValue(null));

            ScreenSizeManager manager = new(2560, 1440);
            MethodInfo windowRectChanged = typeof(ScreenSizeManager).GetMethod(
                "WindowRectChanged",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(windowRectChanged);

            _ = windowRectChanged.Invoke(manager, [new Rectangle(0, 0, 1024, 768)]);

            Assert.Equal(1024, manager.WindowWidth);
            Assert.Equal(768, manager.WindowHeight);
            Assert.Equal(startupWidth, Assert.IsType<float>(realWidth.GetValue(null)));
            Assert.Equal(startupHeight, Assert.IsType<float>(realHeight.GetValue(null)));
        }
    }
}
