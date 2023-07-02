namespace TypeId
{
    using System;

    public partial struct TypeId : IEquatable<TypeId>
    {
        public readonly bool Equals(TypeId other)
        {
            return Id == other.Id
                && Type == other.Type;
        }
    }
}