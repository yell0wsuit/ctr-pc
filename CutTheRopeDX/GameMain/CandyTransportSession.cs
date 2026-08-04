using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Transporter kind responsible for temporarily hiding a whole candy.</summary>
    internal enum CandyTransportKind
    {
        /// <summary>A bamboo tube carries the candy.</summary>
        Bamboo,

        /// <summary>A paired magic hat or seasonal sock carries the candy.</summary>
        Sock,
    }

    /// <summary>
    /// Exact delayed-dispatch payload for one hidden <see cref="CandyLifecycle"/> transport operation.
    /// </summary>
    internal sealed class CandyTransportSession : FrameworkTypes
    {
        private CandyTransportSession(
            CandyTransportKind kind,
            CandyContext candy,
            BambooTube bambooTube,
            Sock sock,
            float savedExitSpeed)
        {
            Kind = kind;
            Candy = candy;
            BambooTube = bambooTube;
            Sock = sock;
            SavedExitSpeed = savedExitSpeed;
        }

        /// <summary>Gets the transporter kind for this session.</summary>
        public CandyTransportKind Kind { get; }

        /// <summary>Gets the logical candy being transported, or <see langword="null"/> in lifecycle-only state tests.</summary>
        public CandyContext Candy { get; }

        /// <summary>Gets the bamboo tube payload, or <see langword="null"/> for a sock or lifecycle-only session.</summary>
        public BambooTube BambooTube { get; }

        /// <summary>Gets the magic hat or sock payload, or <see langword="null"/> for a bamboo or lifecycle-only session.</summary>
        public Sock Sock { get; }

        /// <summary>Gets the saved exit speed for a sock session.</summary>
        public float SavedExitSpeed { get; }

        /// <summary>Creates a bamboo transport session for one logical candy.</summary>
        /// <param name="candy">The logical candy being transported, or <see langword="null"/> for lifecycle-only tests.</param>
        /// <param name="tube">The bamboo tube carrying the candy, or <see langword="null"/> for lifecycle-only tests.</param>
        /// <returns>A new bamboo transport session containing the exact dispatcher payload.</returns>
        public static CandyTransportSession ForBamboo(CandyContext candy, BambooTube tube)
        {
            return new(CandyTransportKind.Bamboo, candy, tube, null, 0f);
        }

        /// <summary>Creates a magic-hat or sock transport session for one logical candy.</summary>
        /// <param name="candy">The logical candy being transported, or <see langword="null"/> for lifecycle-only tests.</param>
        /// <param name="sock">The magic hat or seasonal sock carrying the candy, or <see langword="null"/> for lifecycle-only tests.</param>
        /// <param name="savedExitSpeed">The candy speed to restore when it exits transport.</param>
        /// <returns>A new sock transport session containing the exact dispatcher payload.</returns>
        public static CandyTransportSession ForSock(CandyContext candy, Sock sock, float savedExitSpeed)
        {
            return new(CandyTransportKind.Sock, candy, null, sock, savedExitSpeed);
        }
    }
}
