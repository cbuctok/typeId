namespace TypeId
{
    using System;

    public partial struct TypeId : IFormattable
    {
        private const char _delimiter = '_';

        // Returns the guid in "registry" format.
        public override readonly string ToString() => ToString("d", null);

        public readonly string ToString(string? format)
        {
            return ToString(format, null);
        }

        public readonly string ToString(string? format, IFormatProvider? formatProvider)
        {
            var registryFormatted = Type + _delimiter + Id;

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
    }
}