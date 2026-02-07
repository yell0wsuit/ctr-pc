using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;

namespace CutTheRope.Helpers.Discord
{
    /// <summary>
    /// Cross-platform connection to a Discord IPC pipe.
    /// Windows uses named pipes; Unix/macOS uses Unix domain sockets.
    /// </summary>
    internal sealed class DiscordIpcConnection : IDisposable
    {
        private NamedPipeClientStream _pipeStream;
        private Socket _unixSocket;
        private NetworkStream _networkStream;

        public Stream Stream { get; private set; }

        /// <summary>
        /// Attempts to connect to Discord IPC pipe indices 0 through 9.
        /// </summary>
        /// <returns><see langword="true"/> on the first successful connection; otherwise <see langword="false"/>.</returns>
        public bool TryConnect()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    bool connected = OperatingSystem.IsWindows()
                        ? TryConnectWindows(i)
                        : TryConnectUnix(i);

                    if (connected)
                    {
                        return true;
                    }
                }
                catch (Exception) when (
                    !Debugger.IsAttached)
                {
                    // Try next pipe index
                }
            }

            return false;
        }

        /// <summary>
        /// Connects via Windows named pipe (<c>\\.\pipe\discord-ipc-{pipeIndex}</c>).
        /// </summary>
        /// <param name="pipeIndex">The pipe index to try (0-9).</param>
        /// <returns><see langword="true"/> if the connection succeeded.</returns>
        private bool TryConnectWindows(int pipeIndex)
        {
            NamedPipeClientStream pipe = new(
                ".", $"discord-ipc-{pipeIndex}", PipeDirection.InOut, PipeOptions.Asynchronous);

            try
            {
                pipe.Connect(TimeSpan.FromSeconds(1));
                _pipeStream = pipe;
                Stream = pipe;
                return true;
            }
            catch
            {
                pipe.Dispose();
                if (Debugger.IsAttached)
                {
                    throw;
                }

                return false;
            }
        }

        /// <summary>
        /// Connects via Unix domain socket at standard paths (XDG_RUNTIME_DIR, TMPDIR, /tmp).
        /// </summary>
        /// <param name="pipeIndex">The pipe index to try (0-9).</param>
        /// <returns><see langword="true"/> if the connection succeeded.</returns>
        private bool TryConnectUnix(int pipeIndex)
        {
            string pipeName = $"discord-ipc-{pipeIndex}";

            // Try standard paths in order of preference
            string[] basePaths =
            [
                Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR"),
                Environment.GetEnvironmentVariable("TMPDIR"),
                "/tmp"
            ];

            foreach (string basePath in basePaths)
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    continue;
                }

                string socketPath = Path.Combine(basePath, pipeName);
                if (!File.Exists(socketPath))
                {
                    continue;
                }

                Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                try
                {
                    socket.Connect(new UnixDomainSocketEndPoint(socketPath));
                    _unixSocket = socket;
                    _networkStream = new NetworkStream(socket, ownsSocket: false);
                    Stream = _networkStream;
                    return true;
                }
                catch
                {
                    socket.Dispose();
                }
            }

            return false;
        }

        public void Dispose()
        {
            _networkStream?.Dispose();
            _networkStream = null;

            _unixSocket?.Dispose();
            _unixSocket = null;

            _pipeStream?.Dispose();
            _pipeStream = null;

            Stream = null;
        }
    }
}
