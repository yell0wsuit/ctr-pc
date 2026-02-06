using System;
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
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TryConnect()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    return OperatingSystem.IsWindows()
                        ? TryConnectWindows(i)
                        : TryConnectUnix(i);
                }
                catch (Exception) when (
                    !System.Diagnostics.Debugger.IsAttached)
                {
                    // Try next pipe index
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeIndex"></param>
        /// <returns></returns>
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
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeIndex"></param>
        /// <returns></returns>
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
