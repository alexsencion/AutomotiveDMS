using AutomotiveDMS.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AutomotiveDMS.Domain.ValueObjects
{
    public sealed record Vin
    {
        public const int Length = 17;

        public string Value { get; }

        private Vin(string value) => Value = value;

        public static bool TryCreate(
            string? input,
            [NotNullWhen(true)] out Vin? vin,
            [NotNullWhen(false)] out string? error)
        {
            vin = null;
            var normalized = Normalize(input);

            error = normalized switch
            {
                "" => "VIN is required.",
                _ when !normalized.All(char.IsAsciiLetterOrDigit) => "VIN can contain only letters and digits.",
                _ when normalized.IndexOfAny(['I', 'O', 'Q']) >= 0 => "VIN cannot contain the letters I, O or Q.",
                _ when normalized.Length != Length => $"VIN must be exactly {Length} characters (found {normalized.Length}).",
                _ => null
            };

            if (error is not null)
                return false;

            vin = new Vin(normalized);
            return true;
        }

        public static Vin Create(string? input) =>
            TryCreate(input, out var vin, out var error) ? vin : throw new DomainException(error);

        private static string Normalize(string? input) =>
            input is null
                ? string.Empty
                : new string(input.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()).ToUpperInvariant();

        public override string ToString() => Value;
    }
}
