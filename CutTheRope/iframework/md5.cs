using System;
using System.Runtime.CompilerServices;

namespace CutTheRope.iframework
{
    internal class md5
    {
        private static void GET_UINT32(ref uint n, byte[] b, int dataIndex, int i)
        {
            n = (uint)(b[dataIndex + i] | (b[dataIndex + i + 1] << 8) | (b[dataIndex + i + 2] << 16) | (b[dataIndex + i + 3] << 24));
        }

        private static void PUT_UINT32(uint n, ref byte[] b, int i)
        {
            b[i] = (byte)n;
            b[i + 1] = (byte)(n >> 8);
            b[i + 2] = (byte)(n >> 16);
            b[i + 3] = (byte)(n >> 24);
        }

        private static uint S(uint x, uint n)
        {
            return (x << (int)n) | ((x & uint.MaxValue) >> (int)(32U - n));
        }

        private static void P(ref uint a, uint b, uint c, uint d, uint k, uint s, uint t, uint[] X, FuncF F)
        {
            a += F(b, c, d) + X[(int)k] + t;
            a = S(a, s) + b;
        }

        private static uint F_1(uint x, uint y, uint z)
        {
            return z ^ (x & (y ^ z));
        }

        private static uint F_2(uint x, uint y, uint z)
        {
            return y ^ (z & (x ^ y));
        }

        private static uint F_3(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }

        private static uint F_4(uint x, uint y, uint z)
        {
            return y ^ (x | ~z);
        }

        public static void md5_starts(ref md5_context ctx)
        {
            ctx.total[0] = 0U;
            ctx.total[1] = 0U;
            ctx.state[0] = 1732584193U;
            ctx.state[1] = 4023233417U;
            ctx.state[2] = 2562383102U;
            ctx.state[3] = 271733878U;
        }

        public static void md5_process(ref md5_context ctx, byte[] data, int dataIndex)
        {
            uint[] array = new uint[16];
            GET_UINT32(ref array[0], data, dataIndex, 0);
            GET_UINT32(ref array[1], data, dataIndex, 4);
            GET_UINT32(ref array[2], data, dataIndex, 8);
            GET_UINT32(ref array[3], data, dataIndex, 12);
            GET_UINT32(ref array[4], data, dataIndex, 16);
            GET_UINT32(ref array[5], data, dataIndex, 20);
            GET_UINT32(ref array[6], data, dataIndex, 24);
            GET_UINT32(ref array[7], data, dataIndex, 28);
            GET_UINT32(ref array[8], data, dataIndex, 32);
            GET_UINT32(ref array[9], data, dataIndex, 36);
            GET_UINT32(ref array[10], data, dataIndex, 40);
            GET_UINT32(ref array[11], data, dataIndex, 44);
            GET_UINT32(ref array[12], data, dataIndex, 48);
            GET_UINT32(ref array[13], data, dataIndex, 52);
            GET_UINT32(ref array[14], data, dataIndex, 56);
            GET_UINT32(ref array[15], data, dataIndex, 60);
            uint a = ctx.state[0];
            uint a2 = ctx.state[1];
            uint a3 = ctx.state[2];
            uint a4 = ctx.state[3];
            FuncF f = new FuncF(F_1);
            P(ref a, a2, a3, a4, 0U, 7U, 3614090360U, array, f);
            P(ref a4, a, a2, a3, 1U, 12U, 3905402710U, array, f);
            P(ref a3, a4, a, a2, 2U, 17U, 606105819U, array, f);
            P(ref a2, a3, a4, a, 3U, 22U, 3250441966U, array, f);
            P(ref a, a2, a3, a4, 4U, 7U, 4118548399U, array, f);
            P(ref a4, a, a2, a3, 5U, 12U, 1200080426U, array, f);
            P(ref a3, a4, a, a2, 6U, 17U, 2821735955U, array, f);
            P(ref a2, a3, a4, a, 7U, 22U, 4249261313U, array, f);
            P(ref a, a2, a3, a4, 8U, 7U, 1770035416U, array, f);
            P(ref a4, a, a2, a3, 9U, 12U, 2336552879U, array, f);
            P(ref a3, a4, a, a2, 10U, 17U, 4294925233U, array, f);
            P(ref a2, a3, a4, a, 11U, 22U, 2304563134U, array, f);
            P(ref a, a2, a3, a4, 12U, 7U, 1804603682U, array, f);
            P(ref a4, a, a2, a3, 13U, 12U, 4254626195U, array, f);
            P(ref a3, a4, a, a2, 14U, 17U, 2792965006U, array, f);
            P(ref a2, a3, a4, a, 15U, 22U, 1236535329U, array, f);
            f = new FuncF(F_2);
            P(ref a, a2, a3, a4, 1U, 5U, 4129170786U, array, f);
            P(ref a4, a, a2, a3, 6U, 9U, 3225465664U, array, f);
            P(ref a3, a4, a, a2, 11U, 14U, 643717713U, array, f);
            P(ref a2, a3, a4, a, 0U, 20U, 3921069994U, array, f);
            P(ref a, a2, a3, a4, 5U, 5U, 3593408605U, array, f);
            P(ref a4, a, a2, a3, 10U, 9U, 38016083U, array, f);
            P(ref a3, a4, a, a2, 15U, 14U, 3634488961U, array, f);
            P(ref a2, a3, a4, a, 4U, 20U, 3889429448U, array, f);
            P(ref a, a2, a3, a4, 9U, 5U, 568446438U, array, f);
            P(ref a4, a, a2, a3, 14U, 9U, 3275163606U, array, f);
            P(ref a3, a4, a, a2, 3U, 14U, 4107603335U, array, f);
            P(ref a2, a3, a4, a, 8U, 20U, 1163531501U, array, f);
            P(ref a, a2, a3, a4, 13U, 5U, 2850285829U, array, f);
            P(ref a4, a, a2, a3, 2U, 9U, 4243563512U, array, f);
            P(ref a3, a4, a, a2, 7U, 14U, 1735328473U, array, f);
            P(ref a2, a3, a4, a, 12U, 20U, 2368359562U, array, f);
            f = new FuncF(F_3);
            P(ref a, a2, a3, a4, 5U, 4U, 4294588738U, array, f);
            P(ref a4, a, a2, a3, 8U, 11U, 2272392833U, array, f);
            P(ref a3, a4, a, a2, 11U, 16U, 1839030562U, array, f);
            P(ref a2, a3, a4, a, 14U, 23U, 4259657740U, array, f);
            P(ref a, a2, a3, a4, 1U, 4U, 2763975236U, array, f);
            P(ref a4, a, a2, a3, 4U, 11U, 1272893353U, array, f);
            P(ref a3, a4, a, a2, 7U, 16U, 4139469664U, array, f);
            P(ref a2, a3, a4, a, 10U, 23U, 3200236656U, array, f);
            P(ref a, a2, a3, a4, 13U, 4U, 681279174U, array, f);
            P(ref a4, a, a2, a3, 0U, 11U, 3936430074U, array, f);
            P(ref a3, a4, a, a2, 3U, 16U, 3572445317U, array, f);
            P(ref a2, a3, a4, a, 6U, 23U, 76029189U, array, f);
            P(ref a, a2, a3, a4, 9U, 4U, 3654602809U, array, f);
            P(ref a4, a, a2, a3, 12U, 11U, 3873151461U, array, f);
            P(ref a3, a4, a, a2, 15U, 16U, 530742520U, array, f);
            P(ref a2, a3, a4, a, 2U, 23U, 3299628645U, array, f);
            f = new FuncF(F_4);
            P(ref a, a2, a3, a4, 0U, 6U, 4096336452U, array, f);
            P(ref a4, a, a2, a3, 7U, 10U, 1126891415U, array, f);
            P(ref a3, a4, a, a2, 14U, 15U, 2878612391U, array, f);
            P(ref a2, a3, a4, a, 5U, 21U, 4237533241U, array, f);
            P(ref a, a2, a3, a4, 12U, 6U, 1700485571U, array, f);
            P(ref a4, a, a2, a3, 3U, 10U, 2399980690U, array, f);
            P(ref a3, a4, a, a2, 10U, 15U, 4293915773U, array, f);
            P(ref a2, a3, a4, a, 1U, 21U, 2240044497U, array, f);
            P(ref a, a2, a3, a4, 8U, 6U, 1873313359U, array, f);
            P(ref a4, a, a2, a3, 15U, 10U, 4264355552U, array, f);
            P(ref a3, a4, a, a2, 6U, 15U, 2734768916U, array, f);
            P(ref a2, a3, a4, a, 13U, 21U, 1309151649U, array, f);
            P(ref a, a2, a3, a4, 4U, 6U, 4149444226U, array, f);
            P(ref a4, a, a2, a3, 11U, 10U, 3174756917U, array, f);
            P(ref a3, a4, a, a2, 2U, 15U, 718787259U, array, f);
            P(ref a2, a3, a4, a, 9U, 21U, 3951481745U, array, f);
            ctx.state[0] += a;
            ctx.state[1] += a2;
            ctx.state[2] += a3;
            ctx.state[3] += a4;
        }

        public static void md5_update(ref md5_context ctx, byte[] input, uint length)
        {
            if (length == 0U)
            {
                return;
            }
            uint num = ctx.total[0] & 63U;
            uint num2 = 64U - num;
            ctx.total[0] += length;
            ctx.total[0] &= uint.MaxValue;
            if (ctx.total[0] < length)
            {
                ctx.total[1] += 1U;
            }
            int num3 = 0;
            if (num != 0U && length >= num2)
            {
                Array.Copy(input, num3, ctx.buffer, (int)num, (int)num2);
                md5_process(ref ctx, ctx.buffer, 0);
                length -= num2;
                num3 += (int)num2;
                num = 0U;
            }
            while (length != 0U)
            {
                if (length - 1U <= 62U)
                {
                    Array.Copy(input, num3, ctx.buffer, (int)num, (int)length);
                    return;
                }
                md5_process(ref ctx, input, num3);
                length -= 64U;
                num3 += 64;
            }
        }

        public static void md5_finish(ref md5_context ctx, byte[] digest)
        {
            byte[] b = new byte[8];
            uint num2 = (ctx.total[0] >> 29) | (ctx.total[1] << 3);
            PUT_UINT32(ctx.total[0] << 3, ref b, 0);
            PUT_UINT32(num2, ref b, 4);
            uint num = ctx.total[0] & 63U;
            uint length = (num < 56U) ? (56U - num) : (120U - num);
            md5_update(ref ctx, md5_padding, length);
            md5_update(ref ctx, b, 8U);
            PUT_UINT32(ctx.state[0], ref digest, 0);
            PUT_UINT32(ctx.state[1], ref digest, 4);
            PUT_UINT32(ctx.state[2], ref digest, 8);
            PUT_UINT32(ctx.state[3], ref digest, 12);
        }

        static md5()
        {
            byte[] array = new byte[64];
            array[0] = 128;
            md5_padding = array;
        }

        private static byte[] md5_padding;

        public class md5_context
        {
            public md5_context()
            {
                total = new uint[2];
                state = new uint[4];
                buffer = new byte[64];
            }

            public uint[] total;

            public uint[] state;

            public byte[] buffer;
        }

        // (Invoke) Token: 0x06000664 RID: 1636
        private delegate uint FuncF(uint x, uint y, uint z);
    }
}
