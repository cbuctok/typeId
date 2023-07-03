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

            //get version of uuid (guid)
            var version = guid.ToByteArray()[7] >> 4;
            if (version != 1)
            {
                guid = GuidToUuid(guid);
            }

            Id = Base32JetPack.Encode(guid.ToByteArray());
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
                // false if not ascii lowercase or delimiter
                if (!char.IsLower(c) || c > 127 || c == _delimiter)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidPrefixParsing(string? prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                // "prefix cannot be empty when there's a separator"
                return false;
            }

            return IsValidPrefix(prefix);
        }

        public static bool IsValidSuffix(string suffix)
        {
            if (suffix.Length != _suffixLength)
            {
                return false;
            }

            foreach (var c in suffix)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }

            return true;
        }

        public static TypeId NewTypeId(string prefix)
        {
            var guid = UUIDNext.Uuid.NewSequential();
            guid = GuidToUuid(guid);

            return new TypeId
            {
                Id = Base32JetPack.Encode(guid.ToByteArray()),
                Type = prefix,
                Uuid = guid,
            };
        }

        public static TypeId Parse(string input)
        {
            string prefix;
            string suffix;

            var parts = input.Split(_delimiter);
            var length = parts.Length;
            if (length != 2 && length != 1)
            {
                throw new ArgumentException($"Invalid TypeId format - expected prefix{_delimiter}suffix or just 26 symbols long UUID");
            }

            var hasPrefix = length == 2;

            if (!hasPrefix)
            {
                prefix = string.Empty;
                suffix = parts[0];
            }
            else
            {
                prefix = parts[0];

                if (string.IsNullOrWhiteSpace(prefix))
                {
                    throw new ArgumentException("Invalid TypeId format - if the prefix is empty, the separator should not be there");
                }

                suffix = parts[1];
            }

            // validate prefix
            if (hasPrefix && !IsValidPrefix(prefix))
            {
                throw new ArgumentException("Invalid TypeId format - incorrect prefix");
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
            // endian swap
            var bytes = Uuid.ToByteArray();

            var guidBytes = new byte[16];
            guidBytes[0] = bytes[3];
            guidBytes[1] = bytes[2];
            guidBytes[2] = bytes[1];
            guidBytes[3] = bytes[0];
            guidBytes[4] = bytes[5];
            guidBytes[5] = bytes[4];
            guidBytes[6] = bytes[7];
            guidBytes[7] = bytes[6];
            guidBytes[8] = bytes[8];
            guidBytes[9] = bytes[9];
            guidBytes[10] = bytes[10];
            guidBytes[11] = bytes[11];
            guidBytes[12] = bytes[12];
            guidBytes[13] = bytes[13];
            guidBytes[14] = bytes[14];
            guidBytes[15] = bytes[15];

            return new Guid(guidBytes);
        }

        /// <summary>
        /// Use UUID only with other UUIDs
        /// </summary>
        /// <returns>Returns a UUID</returns>
        public readonly Guid GetUuid() => Uuid;

        private static Guid GuidToUuid(Guid guid)
        {
            byte[] byteArray = guid.ToByteArray();

            // Create temporary variables and correct the byte order to be little-endian
            uint tempUint = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(byteArray[0..4]));
            Array.Copy(BitConverter.GetBytes(tempUint), 0, byteArray, 0, 4);

            ushort tempUshort = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(byteArray[4..6]));
            Array.Copy(BitConverter.GetBytes(tempUshort), 0, byteArray, 4, 2);

            tempUshort = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(byteArray[6..8]));
            Array.Copy(BitConverter.GetBytes(tempUshort), 0, byteArray, 6, 2);

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
            // Create temporary variables and correct the byte order to be big-endian
            uint tempUint = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(byteArray[0..4]));
            Array.Copy(BitConverter.GetBytes(tempUint), 0, byteArray, 0, 4);

            ushort tempUshort = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(byteArray[4..6]));
            Array.Copy(BitConverter.GetBytes(tempUshort), 0, byteArray, 4, 2);

            tempUshort = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(byteArray[6..8]));
            Array.Copy(BitConverter.GetBytes(tempUshort), 0, byteArray, 6, 2);

            return new Guid(byteArray);
        }
    }
}