using CutTheRopeDX.GameMain;

namespace CutTheRopeDX.Tests
{
    /// <summary>Records win/lose callbacks so tests can assert on outcomes, not internals.</summary>
    internal sealed class RecordingSceneDelegate : IGameSceneDelegate
    {
        /// <summary>Number of times the level was won.</summary>
        public int WonCount { get; private set; }

        /// <summary>Number of times the level was lost.</summary>
        public int LostCount { get; private set; }

        /// <inheritdoc />
        public void GameWon()
        {
            WonCount++;
        }

        /// <inheritdoc />
        public void GameLost()
        {
            LostCount++;
        }
    }
}
