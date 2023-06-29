namespace TypeId
{
    using System;

    /// <summary>
    /// TypeId is UUID that contains a prefix and a UUID suffix. The prefix is used to id the type
    /// of the object. The suffix is used to identify the object.
    /// </summary>
    public struct TypeId : IComparable, IComparable<TypeId>, IEquatable<TypeId>, IFormattable
    {
        private const char _delimeter = '_';
        public readonly Guid Guid => ToGuid();
        public string Id { get; private set; }
        public string Type { get; private set; }

        public static TypeId FromGuid(string prefix, Guid guid)
        {
            return new TypeId
            {
                Type = prefix,
                Id = FromGuid(guid)
            };
        }

        public static bool IsValidPrefix(string prefix)
        {
            if (prefix.Length == 0)
            {
                return false;
            }

            foreach (var c in prefix)
            {
                // false if not ascii or delimeter
                if (c > 127 || c == _delimeter)
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

        public static TypeId NewTypeId(string prefix)
        {
            return new TypeId
            {
                Type = prefix.ToLower(),
                Id = NewB32UUid()
            };
        }

        public static TypeId NewTypeId()
        {
            return new TypeId
            {
                Type = string.Empty,
                Id = NewB32UUid()
            };
        }

        public static bool operator !=(TypeId left, TypeId right)
        {
            return !(left==right);
        }

        public static bool operator <(TypeId left, TypeId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(TypeId left, TypeId right)
        {
            return left.CompareTo(right)<=0;
        }

        public static bool operator ==(TypeId left, TypeId right)
        {
            return left.Equals(right);
        }

        public static bool operator >(TypeId left, TypeId right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(TypeId left, TypeId right)
        {
            return left.CompareTo(right)>= 0;
        }

        public static TypeId Parse(string s)
        {
            var parts = s.Split(_delimeter);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid TypeId format - expected prefix{_delimeter}suffix");
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

            var guid = SimpleBase.Base32.Rfc4648.Decode(parts[1]);
            if (guid.Length != 16)
            {
                throw new ArgumentException("Invalid TypeId format - expected suffix in base32");
            }

            return new TypeId
            {
                Type = parts[0],
                Id = parts[1]
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

        public readonly int CompareTo(object obj)
        {
            if (obj is TypeId other)
                return CompareTo(other);

            throw new ArgumentException("Object is not a TypeId");
        }

        public readonly int CompareTo(TypeId other)
        {
            var typeCompare = string.CompareOrdinal(Type, other.Type);
            if (typeCompare != 0)
            {
                return typeCompare;
            }

            return string.CompareOrdinal(Id, other.Id);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TypeId id
                && Equals(id);
        }

        public readonly bool Equals(TypeId other)
        {
            return Id == other.Id
                && Type == other.Type;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Id, Type);
        }

        public void SetId(Guid guid)
        {
            Id = FromGuid(guid);
        }

        public void SetId(string suffix)
        {
            if (!IsValidSuffix(suffix))
            {
                throw new ArgumentException("Invalid TypeId format - incorrect suffix");
            }

            Id = suffix;
        }

        public void SetType(string prefix)
        {
            if (!IsValidPrefix(prefix))
            {
                throw new ArgumentException("Invalid TypeId format - incorrect prefix");
            }

            Type = prefix;
        }

        // Returns the guid in "registry" format.
        public override readonly string ToString() => ToString("d", null);

        public readonly string ToString(string? format)
        {
            return ToString(format, null);
        }

        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            var registryFormatted = Type + _delimeter + Id;

            if (string.IsNullOrWhiteSpace(format))
            {
                return registryFormatted.ToLowerInvariant();
            }

            formatProvider ??= System.Globalization.CultureInfo.CurrentCulture;

            return formatProvider.ToString().ToLowerInvariant() switch
            {
                "g" => registryFormatted,
                _ => registryFormatted.ToLowerInvariant(),
            };
        }

        private static string FromGuid(Guid guid)
        {
            return SimpleBase.Base32.Rfc4648.Encode(guid.ToByteArray()).ToLowerInvariant();
        }

        private static string NewB32UUid()
        {
            var guid = Guid.NewGuid();
            return FromGuid(guid);
        }

        private readonly Guid ToGuid()
        {
            var guid = SimpleBase.Base32.Rfc4648.Decode(Id);
            if (guid.Length != 16)
            {
                throw new ArgumentException("Invalid TypeId format - expected suffix in base32");
            }

            return new Guid(guid);
        }
    }
}