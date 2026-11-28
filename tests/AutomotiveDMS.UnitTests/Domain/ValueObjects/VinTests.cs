using AutomotiveDMS.Domain.Exceptions;
using AutomotiveDMS.Domain.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.UnitTests.Domain.ValueObjects
{
    public class VinTests
    {
        private const string ValidVin = "1HGCM82633A004352";

        [Fact]
        public void ValidVin_IsAccepted_Unchanged()
        {
            Vin.TryCreate(ValidVin, out var vin, out var error).Should().BeTrue();

            vin!.Value.Should().Be(ValidVin);
            error.Should().BeNull();
        }

        [Fact]
        public void Lowercase_IsNormalizedToUppercase() =>
            Vin.Create("1hgcm82633a004352").Value.Should().Be(ValidVin);

        [Theory]
        [InlineData(" 1HGCM82633A004352 ")]
        [InlineData("1HG-CM826-33A004352")]
        [InlineData("1hg cm826 33a004352")]
        public void SpacesAndDashes_AreRemoved(string input) =>
            Vin.Create(input).Value.Should().Be(ValidVin);

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void WrongLength_IsRejected(string? input) =>
            ErrorFor(input).Should().Be("VIN is required.");

        [Theory]
        [InlineData("1HGCM82633A00435", 16)]
        [InlineData("1HGCM82633A0043521", 18)]
        public void WrongLength_IsRejected_WithActualLength(string input, int length) =>
            ErrorFor(input).Should().Be($"VIN must be exactly 17 characters (found {length}).");

        [Theory]
        [InlineData("1HGCM82633AI04352")]
        [InlineData("1HGCM82633AO04352")]
        [InlineData("1HGCM82633AQ04352")]
        public void LettersIOQ_AreRejected(string input) =>
            ErrorFor(input).Should().Be("VIN cannot contain the letters I, O or Q.");

        [Theory]
        [InlineData("1HGCM82633A00435@")]
        [InlineData("1HGCM82633A00435.")]
        [InlineData("1HGCM82633A00435_")]
        public void Symbols_AreRejected(string input) =>
            ErrorFor(input).Should().Be("VIN can contain only letters and digits.");

        [Fact]
        public void IOQ_IsReportedBeforeLength()
        {
            ErrorFor("1HGCM82633AOO435").Should().Be("VIN cannot contain the letters I, O or Q.");
        }

        [Fact]
        public void Equality_IgnoresOriginalFormatting() =>
            Vin.Create("1hg-cm826-33a004352").Should().Be(Vin.Create(ValidVin));

        [Fact]
        public void ToString_ReturnsValue() =>
            Vin.Create(ValidVin).ToString().Should().Be(ValidVin);

        [Fact]
        public void Create_Invalid_ThrowsDomainException_WithReason()
        {
            var act = () => Vin.Create("A12345678");

            act.Should().Throw<DomainException>().WithMessage("VIN must be exactly 17 characters*");
        }

        [Fact]
        public void TryCreate_Invalid_ReturnsFalse_AndNoVin()
        {
            Vin.TryCreate("bad", out var vin, out _).Should().BeFalse();

            vin.Should().BeNull();
        }


        private static string? ErrorFor(string? input)
        {
            Vin.TryCreate(input, out _, out var error);
            return error;
        }

    }
}
