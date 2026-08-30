using System;
using System.IO;
using System.Text;

namespace CutTheRopeDX.Browser
{
    /// <summary>
    /// Forwards the game's standard error to the editor during a playtest, and onward to the browser
    /// console as before.
    /// </summary>
    /// <remarks>
    /// Core reports a level it cannot load by writing the reason to <see cref="Console.Error"/>, and
    /// the desktop editor reads exactly those bytes off the child process's stderr pipe. A browser tab
    /// has no pipe, so this stands in for one, which is how the browser reaches the same diagnostics
    /// without Core needing to know that a second reporting channel exists.
    /// </remarks>
    /// <param name="inner">The writer installed before this one; keeps receiving everything.</param>
    internal sealed class PlaytestErrorWriter(TextWriter inner) : TextWriter
    {
        /// <summary>Characters written since the last line break.</summary>
        private readonly StringBuilder _line = new();

        /// <inheritdoc />
        public override Encoding Encoding => Encoding.UTF8;

        /// <inheritdoc />
        /// <remarks>
        /// Overriding the single-character write is enough: every other <see cref="TextWriter"/>
        /// overload funnels into it.
        /// </remarks>
        public override void Write(char value)
        {
            inner.Write(value);

            if (value == '\n')
            {
                EmitLine();
            }
            else if (value != '\r')
            {
                _ = _line.Append(value);
            }
        }

        /// <summary>Sends the completed line to the editor, if it carries anything.</summary>
        private void EmitLine()
        {
            if (_line.Length > 0)
            {
                PlaytestSession.ReportError(_line.ToString());
                _ = _line.Clear();
            }
        }
    }
}
