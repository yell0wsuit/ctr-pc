using System;
using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the order of work inside <see cref="ViewController.ShowView"/>. Showing a view is
    /// also when the root controller takes the picture it crossfades in, and that picture has to
    /// be of the finished scene: anything captured earlier is played over the screen for the
    /// length of the fade before the live view takes over.
    /// </summary>
    public sealed class ViewShowOrderTests
    {
        [Fact]
        public void TheCrossfadePictureIsTakenAfterTheViewIsLaidOut()
        {
            _ = HeadlessGame.Boot();

            RootController root = Application.SharedRootController();
            FieldInfo previousView = typeof(RootController).GetField(
                "previousView",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(previousView);

            int restoreTransition = root.viewTransition;
            object restorePrevious = previousView.GetValue(root);
            ProbeController controller = new();

            try
            {
                // What the real game has on hand when it crossfades between two screens: a
                // transition chosen, and an outgoing view to fade from.
                root.viewTransition = 0;
                previousView.SetValue(root, new View());

                // Taking the picture is a full draw of the incoming view, which a headless run
                // cannot do - so reaching the render backend is exactly the moment of capture,
                // and what the controller has done by then is what the picture shows.
                _ = Assert.Throws<NotSupportedException>(() => controller.ShowView(0));

                Assert.True(controller.Shown, "the view was captured before it was shown");
                Assert.Equal(1, controller.Relayouts);
            }
            finally
            {
                root.viewTransition = restoreTransition;
                previousView.SetValue(root, restorePrevious);
                controller.Dispose();
            }
        }

        /// <summary>Controller that records the layout pass its own view receives.</summary>
        private sealed class ProbeController : ViewController
        {
            public ProbeController()
            {
                AddViewwithID(new ProbeView(this), 0);
            }

            /// <summary>How many times this controller has been laid out.</summary>
            public int Relayouts { get; private set; }

            /// <summary>Whether the view has been shown.</summary>
            public bool Shown { get; private set; }

            /// <inheritdoc />
            protected override void Relayout(ViewportLayoutSnapshot snapshot)
            {
                Relayouts++;
                base.Relayout(snapshot);
            }

            private void OnViewShown()
            {
                Shown = true;
            }

            /// <summary>View that reports being shown to the controller that owns it.</summary>
            private sealed class ProbeView(ProbeController owner) : View
            {
                /// <inheritdoc />
                public override void Show()
                {
                    owner.OnViewShown();
                    base.Show();
                }
            }
        }
    }
}
