namespace TypeId
{
    using System;

    /// <summary>
    /// Based on base32 implementation in go from jetpack-io. https://github.com/jetpack-io/typeid-go/blob/main/base32/base32.go
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

            // Work directly with string chars - no UTF8 encoding allocation
            for (int i = 0; i < s.Length; i++)
            {
                if (Dec[s[i]] == 0xFF)
                {
                    throw new ArgumentException("Invalid base32 character");
                }
            }

            byte[] id = new byte[16];

            // 6 bytes timestamp (48 bits)
            id[0] = (byte)((Dec[s[0]] << 5) | Dec[s[1]]);
            id[1] = (byte)((Dec[s[2]] << 3) | (Dec[s[3]] >> 2));
            id[2] = (byte)((Dec[s[3]] << 6) | (Dec[s[4]] << 1) | (Dec[s[5]] >> 4));
            id[3] = (byte)((Dec[s[5]] << 4) | (Dec[s[6]] >> 1));
            id[4] = (byte)((Dec[s[6]] << 7) | (Dec[s[7]] << 2) | (Dec[s[8]] >> 3));
            id[5] = (byte)((Dec[s[8]] << 5) | Dec[s[9]]);

            // 10 bytes of entropy (80 bits)
            id[6] = (byte)((Dec[s[10]] << 3) | (Dec[s[11]] >> 2)); // First 4 bits are the version
            id[7] = (byte)((Dec[s[11]] << 6) | (Dec[s[12]] << 1) | (Dec[s[13]] >> 4));
            id[8] = (byte)((Dec[s[13]] << 4) | (Dec[s[14]] >> 1)); // First 2 bits are the variant
            id[9] = (byte)((Dec[s[14]] << 7) | (Dec[s[15]] << 2) | (Dec[s[16]] >> 3));
            id[10] = (byte)((Dec[s[16]] << 5) | Dec[s[17]]);
            id[11] = (byte)((Dec[s[18]] << 3) | Dec[s[19]] >> 2);
            id[12] = (byte)((Dec[s[19]] << 6) | (Dec[s[20]] << 1) | (Dec[s[21]] >> 4));
            id[13] = (byte)((Dec[s[21]] << 4) | (Dec[s[22]] >> 1));
            id[14] = (byte)((Dec[s[22]] << 7) | (Dec[s[23]] << 2) | (Dec[s[24]] >> 3));
            id[15] = (byte)((Dec[s[24]] << 5) | Dec[s[25]]);

            return id;
        }

        public static string Encode(byte[] src)
        {
            if (src.Length != 16)
            {
                throw new ArgumentException("Source array length must be 16.", nameof(src));
            }

            return string.Create(26, src, (dst, bytes) =>
            {
                // 10 byte timestamp
                dst[0] = _alphabet[(bytes[0] & 224) >> 5];
                dst[1] = _alphabet[bytes[0] & 31];
                dst[2] = _alphabet[(bytes[1] & 248) >> 3];
                dst[3] = _alphabet[((bytes[1] & 7) << 2) | ((bytes[2] & 192) >> 6)];
                dst[4] = _alphabet[(bytes[2] & 62) >> 1];
                dst[5] = _alphabet[((bytes[2] & 1) << 4) | ((bytes[3] & 240) >> 4)];
                dst[6] = _alphabet[((bytes[3] & 15) << 1) | ((bytes[4] & 128) >> 7)];
                dst[7] = _alphabet[(bytes[4] & 124) >> 2];
                dst[8] = _alphabet[((bytes[4] & 3) << 3) | ((bytes[5] & 224) >> 5)];
                dst[9] = _alphabet[bytes[5] & 31];

                // 16 bytes of entropy
                dst[10] = _alphabet[(bytes[6] & 248) >> 3];
                dst[11] = _alphabet[((bytes[6] & 7) << 2) | ((bytes[7] & 192) >> 6)];
                dst[12] = _alphabet[(bytes[7] & 62) >> 1];
                dst[13] = _alphabet[((bytes[7] & 1) << 4) | ((bytes[8] & 240) >> 4)];
                dst[14] = _alphabet[((bytes[8] & 15) << 1) | ((bytes[9] & 128) >> 7)];
                dst[15] = _alphabet[(bytes[9] & 124) >> 2];
                dst[16] = _alphabet[((bytes[9] & 3) << 3) | ((bytes[10] & 224) >> 5)];
                dst[17] = _alphabet[bytes[10] & 31];
                dst[18] = _alphabet[(bytes[11] & 248) >> 3];
                dst[19] = _alphabet[((bytes[11] & 7) << 2) | ((bytes[12] & 192) >> 6)];
                dst[20] = _alphabet[(bytes[12] & 62) >> 1];
                dst[21] = _alphabet[((bytes[12] & 1) << 4) | ((bytes[13] & 240) >> 4)];
                dst[22] = _alphabet[((bytes[13] & 15) << 1) | ((bytes[14] & 128) >> 7)];
                dst[23] = _alphabet[(bytes[14] & 124) >> 2];
                dst[24] = _alphabet[((bytes[14] & 3) << 3) | ((bytes[15] & 224) >> 5)];
                dst[25] = _alphabet[bytes[15] & 31];
            });
        }

        public static string Encode(ReadOnlySpan<byte> src)
        {
            if (src.Length != 16)
            {
                throw new ArgumentException("Source span length must be 16.");
            }

            // For span, use inline encoding with stackalloc for intermediate buffer
            Span<char> chars = stackalloc char[26];

            // 10 byte timestamp
            chars[0] = _alphabet[(src[0] & 224) >> 5];
            chars[1] = _alphabet[src[0] & 31];
            chars[2] = _alphabet[(src[1] & 248) >> 3];
            chars[3] = _alphabet[((src[1] & 7) << 2) | ((src[2] & 192) >> 6)];
            chars[4] = _alphabet[(src[2] & 62) >> 1];
            chars[5] = _alphabet[((src[2] & 1) << 4) | ((src[3] & 240) >> 4)];
            chars[6] = _alphabet[((src[3] & 15) << 1) | ((src[4] & 128) >> 7)];
            chars[7] = _alphabet[(src[4] & 124) >> 2];
            chars[8] = _alphabet[((src[4] & 3) << 3) | ((src[5] & 224) >> 5)];
            chars[9] = _alphabet[src[5] & 31];

            // 16 bytes of entropy
            chars[10] = _alphabet[(src[6] & 248) >> 3];
            chars[11] = _alphabet[((src[6] & 7) << 2) | ((src[7] & 192) >> 6)];
            chars[12] = _alphabet[(src[7] & 62) >> 1];
            chars[13] = _alphabet[((src[7] & 1) << 4) | ((src[8] & 240) >> 4)];
            chars[14] = _alphabet[((src[8] & 15) << 1) | ((src[9] & 128) >> 7)];
            chars[15] = _alphabet[(src[9] & 124) >> 2];
            chars[16] = _alphabet[((src[9] & 3) << 3) | ((src[10] & 224) >> 5)];
            chars[17] = _alphabet[src[10] & 31];
            chars[18] = _alphabet[(src[11] & 248) >> 3];
            chars[19] = _alphabet[((src[11] & 7) << 2) | ((src[12] & 192) >> 6)];
            chars[20] = _alphabet[(src[12] & 62) >> 1];
            chars[21] = _alphabet[((src[12] & 1) << 4) | ((src[13] & 240) >> 4)];
            chars[22] = _alphabet[((src[13] & 15) << 1) | ((src[14] & 128) >> 7)];
            chars[23] = _alphabet[(src[14] & 124) >> 2];
            chars[24] = _alphabet[((src[14] & 3) << 3) | ((src[15] & 224) >> 5)];
            chars[25] = _alphabet[src[15] & 31];

            return new string(chars);
        }
    }
}