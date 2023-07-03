namespace TypeId
{
    using System;

    /// <summary>
    /// Based on base32 implementation in go from jetpack-io.
    /// https://github.com/jetpack-io/typeid-go/blob/main/base32/base32.go
    /// </summary>
    internal static class Base32JetPack
    {
        private const string _alphabet = "0123456789abcdefghjkmnpqrstvwxyz";

        private static readonly byte[] Dec = new byte[]
        {
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x01,
        0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x0A, 0x0B, 0x0C,
        0x0D, 0x0E, 0x0F, 0x10, 0x11, 0xFF, 0x12, 0x13, 0xFF, 0x14,
        0x15, 0xFF, 0x16, 0x17, 0x18, 0x19, 0x1A, 0xFF, 0x1B, 0x1C,
        0x1D, 0x1E, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        };

        public static byte[] Decode(string s)
        {
            if (s.Length != 26)
            {
                throw new ArgumentException("Invalid length");
            }

            var v = System.Text.Encoding.UTF8.GetBytes(s);

            for (int i = 0; i < v.Length; i++)
            {
                if (Dec[v[i]] == 0xFF)
                {
                    throw new ArgumentException("Invalid base32 character");
                }
            }

            byte[] id = new byte[16];

            // 6 bytes timestamp (48 bits)
            id[0] = (byte)((Dec[v[0]] << 5) | Dec[v[1]]);
            id[1] = (byte)((Dec[v[2]] << 3) | (Dec[v[3]] >> 2));
            id[2] = (byte)((Dec[v[3]] << 6) | (Dec[v[4]] << 1) | (Dec[v[5]] >> 4));
            id[3] = (byte)((Dec[v[5]] << 4) | (Dec[v[6]] >> 1));
            id[4] = (byte)((Dec[v[6]] << 7) | (Dec[v[7]] << 2) | (Dec[v[8]] >> 3));
            id[5] = (byte)((Dec[v[8]] << 5) | Dec[v[9]]);

            // 10 bytes of entropy (80 bits)
            id[6] = (byte)((Dec[v[10]] << 3) | (Dec[v[11]] >> 2)); // First 4 bits are the version
            id[7] = (byte)((Dec[v[11]] << 6) | (Dec[v[12]] << 1) | (Dec[v[13]] >> 4));
            id[8] = (byte)((Dec[v[13]] << 4) | (Dec[v[14]] >> 1)); // First 2 bits are the variant
            id[9] = (byte)((Dec[v[14]] << 7) | (Dec[v[15]] << 2) | (Dec[v[16]] >> 3));
            id[10] = (byte)((Dec[v[16]] << 5) | Dec[v[17]]);
            id[11] = (byte)((Dec[v[18]] << 3) | Dec[v[19]] >> 2);
            id[12] = (byte)((Dec[v[19]] << 6) | (Dec[v[20]] << 1) | (Dec[v[21]] >> 4));
            id[13] = (byte)((Dec[v[21]] << 4) | (Dec[v[22]] >> 1));
            id[14] = (byte)((Dec[v[22]] << 7) | (Dec[v[23]] << 2) | (Dec[v[24]] >> 3));
            id[15] = (byte)((Dec[v[24]] << 5) | Dec[v[25]]);

            return id;
        }

        public static string Encode(byte[] src)
        {
            if (src.Length != 16)
            {
                throw new ArgumentException("Source array length must be 16.", nameof(src));
            }

            char[] dst = new char[26];

            // 10 byte timestamp
            dst[0] = _alphabet[(src[0] & 224) >> 5];
            dst[1] = _alphabet[src[0] & 31];
            dst[2] = _alphabet[(src[1] & 248) >> 3];
            dst[3] = _alphabet[((src[1] & 7) << 2) | ((src[2] & 192) >> 6)];
            dst[4] = _alphabet[(src[2] & 62) >> 1];
            dst[5] = _alphabet[((src[2] & 1) << 4) | ((src[3] & 240) >> 4)];
            dst[6] = _alphabet[((src[3] & 15) << 1) | ((src[4] & 128) >> 7)];
            dst[7] = _alphabet[(src[4] & 124) >> 2];
            dst[8] = _alphabet[((src[4] & 3) << 3) | ((src[5] & 224) >> 5)];
            dst[9] = _alphabet[src[5] & 31];

            // 16 bytes of entropy
            dst[10] = _alphabet[(src[6] & 248) >> 3];
            dst[11] = _alphabet[((src[6] & 7) << 2) | ((src[7] & 192) >> 6)];
            dst[12] = _alphabet[(src[7] & 62) >> 1];
            dst[13] = _alphabet[((src[7] & 1) << 4) | ((src[8] & 240) >> 4)];
            dst[14] = _alphabet[((src[8] & 15) << 1) | ((src[9] & 128) >> 7)];
            dst[15] = _alphabet[(src[9] & 124) >> 2];
            dst[16] = _alphabet[((src[9] & 3) << 3) | ((src[10] & 224) >> 5)];
            dst[17] = _alphabet[src[10] & 31];
            dst[18] = _alphabet[(src[11] & 248) >> 3];
            dst[19] = _alphabet[((src[11] & 7) << 2) | ((src[12] & 192) >> 6)];
            dst[20] = _alphabet[(src[12] & 62) >> 1];
            dst[21] = _alphabet[((src[12] & 1) << 4) | ((src[13] & 240) >> 4)];
            dst[22] = _alphabet[((src[13] & 15) << 1) | ((src[14] & 128) >> 7)];
            dst[23] = _alphabet[(src[14] & 124) >> 2];
            dst[24] = _alphabet[((src[14] & 3) << 3) | ((src[15] & 224) >> 5)];
            dst[25] = _alphabet[src[15] & 31];

            return new string(dst);
        }
    }
}