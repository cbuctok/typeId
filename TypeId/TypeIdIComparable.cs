namespace TypeId
{
    using System;

    public partial struct TypeId : IComparable, IComparable<TypeId>
    {
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

            return Id.CompareTo(other.Id);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is TypeId id
                && Equals(id);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Id, Type);
        }
    }
}