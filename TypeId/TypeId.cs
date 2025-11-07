namespace TypeId
{
    using System;

    /// <summary>
    /// TypeId is UUID that contains a prefix and a UUID suffix. The prefix is used to id the type
    /// of the object. The suffix is used to identify the object.
    /// </summary>
    public partial struct TypeId
    {
        public static readonly TypeId Empty = new TypeId(string.Empty, Guid.Empty);
        private const short _maxPrefixLength = 63;
        private const short _suffixLength = 26;

        public TypeId(string prefix, Guid guid) : this()
        {
            if (!string.IsNullOrEmpty(prefix) && !IsValidPrefix(prefix))
            {
                throw new ArgumentException("Invalid prefix", nameof(prefix));
            }

            // Use stackalloc to check version without heap allocation
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes);

            //get version of uuid (guid)
            var version = bytes[7] >> 4;
            if (version != 1)
            {
                guid = GuidToUuid(guid);
                guid.TryWriteBytes(bytes);
            }

            Id = Base32JetPack.Encode(bytes);
            Type = prefix;
            Uuid = guid;
        }

        public string Id { get; private set; }

        public string Type { get; private set; }

        private Guid Uuid { get; set; }

        public static bool IsValidPrefix(string prefix)
        {
            if (prefix.Length > _maxPrefixLength)
            {
                return false;
            }

            foreach (var c in prefix)
            {
                // Fast ASCII lowercase check: a-z are 97-122
                if (c < 'a' || c > 'z' || c == _delimiter)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidSuffix(string suffix)
        {
            if (suffix.Length != _suffixLength)
            {
                return false;
            }

            if (suffix[0] > '7')
            {
                return false;
            }

            // Fast validation: valid base32 chars are 0-9, a-z (excluding i, l, o, u)
            foreach (var c in suffix)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z'))
                {
                    continue;
                }
                return false;
            }

            return true;
        }

        public static TypeId NewTypeId(string prefix)
        {
            var guid = UUIDNext.Uuid.NewSequential();
            guid = GuidToUuid(guid);

            // Use stackalloc and TryWriteBytes to avoid heap allocation
            Span<byte> bytes = stackalloc byte[16];
            guid.TryWriteBytes(bytes);

            return new TypeId
            {
                Id = Base32JetPack.Encode(bytes),
                Type = prefix,
                Uuid = guid,
            };
        }

        public static TypeId Parse(string input)
        {
            string prefix;
            string suffix;

            // Use IndexOf instead of Split to avoid array allocation
            int delimiterIndex = input.IndexOf(_delimiter);

            if (delimiterIndex == -1)
            {
                // No delimiter - just suffix
                prefix = string.Empty;
                suffix = input;
            }
            else
            {
                // Has delimiter - split into prefix and suffix
                prefix = input.Substring(0, delimiterIndex);
                suffix = input.Substring(delimiterIndex + 1);

                if (string.IsNullOrWhiteSpace(prefix))
                {
                    throw new ArgumentException("Invalid TypeId format - if the prefix is empty, the separator should not be there");
                }

                // Check for multiple delimiters
                if (suffix.IndexOf(_delimiter) != -1)
                {
                    throw new ArgumentException($"Invalid TypeId format - expected prefix{_delimiter}suffix or just 26 symbols long UUID");
                }

                // validate prefix
                if (!IsValidPrefix(prefix))
                {
                    throw new ArgumentException("Invalid TypeId format - incorrect prefix");
                }
            }

            // validate suffix
            if (!IsValidSuffix(suffix))
            {
                throw new ArgumentException("Invalid TypeId format - incorrect suffix");
            }

            var guid = SuffixToGuid(suffix);

            return new TypeId
            {
                Type = prefix.ToLowerInvariant(),
                Id = suffix.ToLowerInvariant(),
                Uuid = guid
            };
        }

        public static bool TryParse(string input, out TypeId typeId)
        {
            if (string.IsNullOrEmpty(input))
            {
                typeId = default;
                return false;
            }

            try
            {
                typeId = Parse(input);
                return true;
            }
            catch (ArgumentException)
            {
                typeId = default;
                return false;
            }
        }

        /// <summary>
        /// Endian swap the UUID and return it as a GUID
        /// </summary>
        /// <returns>Returns a GUID</returns>
        public readonly Guid GetGuid()
        {
            Span<byte> bytes = stackalloc byte[16];
            Uuid.TryWriteBytes(bytes);

            // Reverse endianness in-place
            // Reverse bytes 0-3 (first uint)
            (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
            (bytes[1], bytes[2]) = (bytes[2], bytes[1]);

            // Reverse bytes 4-5 (first ushort)
            (bytes[4], bytes[5]) = (bytes[5], bytes[4]);

            // Reverse bytes 6-7 (second ushort)
            (bytes[6], bytes[7]) = (bytes[7], bytes[6]);

            return new Guid(bytes);
        }

        /// <summary>
        /// Use UUID only with other UUIDs
        /// </summary>
        /// <returns>Returns a UUID</returns>
        public readonly Guid GetUuid() => Uuid;

        private static Guid GuidToUuid(Guid guid)
        {
            Span<byte> byteArray = stackalloc byte[16];
            guid.TryWriteBytes(byteArray);

            // Reverse endianness in-place for better performance
            // Reverse bytes 0-3 (first uint)
            (byteArray[0], byteArray[3]) = (byteArray[3], byteArray[0]);
            (byteArray[1], byteArray[2]) = (byteArray[2], byteArray[1]);

            // Reverse bytes 4-5 (first ushort)
            (byteArray[4], byteArray[5]) = (byteArray[5], byteArray[4]);

            // Reverse bytes 6-7 (second ushort)
            (byteArray[6], byteArray[7]) = (byteArray[7], byteArray[6]);

            return new Guid(byteArray);
        }

        /*
         * Disclaimer: This is a GUID, not a UUID.
         *
         * While GUIDs (Microsoft) and UUIDs (RFC4122) look similar and serve similar purposes
         * there are subtle-but-occasionally-important differences.
         * Specifically, Microsoft GUID docs allow GUIDs to contain any hex digit in any position
         * https://learn.microsoft.com/en-us/windows/win32/msi/guid,
         * while RFC4122 requires certain values for the version and variant fields
         * https://www.rfc-editor.org/rfc/rfc4122#section-4 .
         * Also, [per MS docs], GUIDs should be all-upper case,
         * whereas UUIDs should be "output as lower case characters and are case insensitive on input"
         * https://www.rfc-editor.org/rfc/rfc4122#section-3 .
         * This can lead to incompatibilities between code libraries!
         *
         * Therefore GUID is more permissive than UUID.
        */

        private static Guid SuffixToGuid(string suffix)
        {
            var byteArray = Base32JetPack.Decode(suffix);

            // Reverse endianness in-place for better performance
            // Reverse bytes 0-3 (first uint)
            (byteArray[0], byteArray[3]) = (byteArray[3], byteArray[0]);
            (byteArray[1], byteArray[2]) = (byteArray[2], byteArray[1]);

            // Reverse bytes 4-5 (first ushort)
            (byteArray[4], byteArray[5]) = (byteArray[5], byteArray[4]);

            // Reverse bytes 6-7 (second ushort)
            (byteArray[6], byteArray[7]) = (byteArray[7], byteArray[6]);

            return new Guid(byteArray);
        }
    }
}