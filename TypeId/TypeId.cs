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
            // Empty prefix is valid
            if (prefix.Length == 0)
            {
                return true;
            }

            if (prefix.Length > _maxPrefixLength)
            {
                return false;
            }

            // Must start with a lowercase letter (not underscore)
            if (prefix[0] < 'a' || prefix[0] > 'z')
            {
                return false;
            }

            // Must end with a lowercase letter (not underscore)
            if (prefix[prefix.Length - 1] < 'a' || prefix[prefix.Length - 1] > 'z')
            {
                return false;
            }

            // Check middle characters: can be lowercase letters or underscores
            for (int i = 1; i < prefix.Length - 1; i++)
            {
                var c = prefix[i];
                // Fast ASCII lowercase check: a-z are 97-122, underscore is 95
                if ((c < 'a' || c > 'z') && c != _delimiter)
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

            // First character must be 0-7 to ensure the value doesn't exceed 128 bits
            if (suffix[0] > '7')
            {
                return false;
            }

            // Valid base32 chars are 0-9, a-z (excluding i, l, o, u)
            // The alphabet is: 0123456789abcdefghjkmnpqrstvwxyz
            foreach (var c in suffix)
            {
                if (c >= '0' && c <= '9')
                {
                    continue;
                }
                if (c >= 'a' && c <= 'z')
                {
                    // Reject i, l, o, u as they're not in the base32 alphabet
                    if (c == 'i' || c == 'l' || c == 'o' || c == 'u')
                    {
                        return false;
                    }
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

            // The suffix is always exactly 26 characters
            // If there's a prefix, it's separated by '_'
            // Since prefixes can now contain underscores (0.3.0 spec), we need to find the separator
            // by checking if the last 26 chars are a valid suffix preceded by '_'

            if (input.Length == _suffixLength)
            {
                // No prefix - just 26 character suffix
                prefix = string.Empty;
                suffix = input;
            }
            else if (input.Length > _suffixLength)
            {
                // Extract the last 26 characters as the suffix
                suffix = input.Substring(input.Length - _suffixLength);

                // The character before the suffix should be the separator
                int separatorIndex = input.Length - _suffixLength - 1;

                if (separatorIndex < 0 || input[separatorIndex] != _delimiter)
                {
                    throw new ArgumentException($"Invalid TypeId format - expected prefix{_delimiter}suffix or just 26 symbols long UUID");
                }

                // Everything before the separator is the prefix
                prefix = input.Substring(0, separatorIndex);

                if (string.IsNullOrWhiteSpace(prefix))
                {
                    throw new ArgumentException("Invalid TypeId format - if the prefix is empty, the separator should not be there");
                }

                // validate prefix
                if (!IsValidPrefix(prefix))
                {
                    throw new ArgumentException("Invalid TypeId format - incorrect prefix");
                }
            }
            else
            {
                // Input is shorter than 26 characters - invalid
                throw new ArgumentException("Invalid TypeId format - suffix must be exactly 26 characters");
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