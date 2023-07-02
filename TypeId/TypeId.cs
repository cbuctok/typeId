namespace TypeId
{
    using System;
    using UUIDNext;

    /// <summary>
    /// TypeId is UUID that contains a prefix and a UUID suffix. The prefix is used to id the type
    /// of the object. The suffix is used to identify the object.
    /// </summary>
    public partial struct TypeId
    {
        public static readonly TypeId Empty = new TypeId(string.Empty, Guid.Empty);
        private const short _maxPrefixLength = 63;

        public TypeId(string prefix, Guid guid) : this()
        {
            Type = prefix;
            Id = guid;
        }

        public Guid Id { get; private set; }
        public string Type { get; private set; }

        public static TypeId FromGuid(string prefix, Guid guid)
        {
            return new TypeId
            {
                Type = prefix,
                Id = guid
            };
        }

        public static bool IsValidPrefix(string? prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return true;
            }

            if (prefix.Length > _maxPrefixLength)
            {
                return false;
            }

            foreach (var c in prefix)
            {
                // false if not ascii or delimiter
                if (c > 127 || c == _delimiter)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidSuffix(string suffix)
        {
            if (suffix.Length != 26)
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

        public static TypeId New()
        {
            return new TypeId
            {
                Type = string.Empty,
                Id = Uuid.NewSequential()
            };
        }

        public static TypeId NewTypeId(string prefix)
        {
            return new TypeId
            {
                Type = prefix.ToLower(),
                Id = Uuid.NewSequential()
            };
        }

        public static TypeId NewTypeId()
        {
            return new TypeId
            {
                Type = string.Empty,
                Id = Uuid.NewSequential()
            };
        }

        public static TypeId Parse(string s)
        {
            var parts = s.Split(_delimiter);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid TypeId format - expected prefix{_delimiter}suffix");
            }

            // validate prefix
            if (!IsValidPrefix(parts[0]))
            {
                throw new ArgumentException("Invalid TypeId format - incorrect prefix");
            }

            // validate suffix
            if (!IsValidSuffix(parts[1]))
            {
                throw new ArgumentException("Invalid TypeId format - incorrect suffix");
            }

            // decode parts[1] to guid
            var guidBytes = SimpleBase.Base32.Crockford.Decode(parts[1]);
            var guid = new Guid(guidBytes);

            return new TypeId
            {
                Type = parts[0],
                Id = guid
            };
        }

        public static bool TryParse(string s, out TypeId typeId)
        {
            try
            {
                typeId = Parse(s);
                return true;
            }
            catch (ArgumentException)
            {
                typeId = default;
                return false;
            }
        }

        public void SetId(Guid guid)
        {
            Id = guid;
        }

        public void SetType(string prefix)
        {
            if (!IsValidPrefix(prefix))
            {
                throw new ArgumentException("Invalid TypeId format - incorrect prefix");
            }

            Type = prefix;
        }
    }
}