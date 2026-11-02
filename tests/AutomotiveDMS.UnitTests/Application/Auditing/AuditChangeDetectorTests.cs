using AutomotiveDMS.Application.Auditing;
using AutomotiveDMS.Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AutomotiveDMS.UnitTests.Application.Auditing
{
    public class AuditChangeDetectorTests
    {
        private readonly AuditChangeDetector _detector = new();

        [Fact]
        public void Added_ProducesSingleCreatedEntry_WithoutFieldDetails()
        {
            var snapshot = new EntitySnapshot("Vehicle", "42", EntityChangeState.Added,
                [P("Make", null, "Toyota"), P("ListPrice", null, 18500m)]);

            var changes = _detector.Detect(snapshot);

            changes.Should().ContainSingle().Which.Should().Be(
                new AuditChange("Vehicle", "42", AuditAction.Created, null, null, null));
        }

        [Fact]
        public void Deleted_ProducesSingleDeleteEntry_WithoutFieldDetails()
        {
            var snapshot = new EntitySnapshot("Guarantor", "7", EntityChangeState.Deleted,
                [P("FirstName", "Luis", "Luis")]);

            var changes = _detector.Detect(snapshot);

            changes.Should().ContainSingle().Which.Should().Be(
                new AuditChange("Guarantor", "7", AuditAction.Deleted, null, null, null));
        }

        [Fact]
        public void Modified_OneChangedProperty_ProducesOneUpdatedEntry()
        {
            var changes = _detector.Detect(Modified(P("Color", "Red", "Blue")));

            changes.Should().ContainSingle().Which.Should().Be(
                new AuditChange("Vehicle", "42", AuditAction.Updated, "Color", "Red", "Blue"));
        }

        [Fact]
        public void Modified_SeveralChangedProperties_ProducesOneEntryEach_InPropertyOrder()
        {
            var changes = _detector.Detect(Modified(
                P("Make", "Toyota", "Honda"),
                P("Model", "Corolla", "Civic")));

            changes.Select(c => c.FieldName).Should().Equal("Make", "Model");
            changes.Should().AllSatisfy(c =>
            {
                c.EntityName.Should().Be("Vehicle");
                c.EntityId.Should().Be("42");
                c.Action.Should().Be(AuditAction.Updated);
            });
        }

        [Fact]
        public void Modified_PropertyReassignedSameValue_ProducesNothing()
        {
            var changes = _detector.Detect(Modified(P("Make", "Toyota", "Toyota")));

            changes.Should().BeEmpty();
        }

        [Fact]
        public void Modified_OnlyUnchangedAndExcludedProperties_ProducesNothing()
        {
            var changes = _detector.Detect(Modified(
                P("Make", "Toyota", "Toyota"),
                P("ModifiedDate", null, new DateTime(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc))));

            changes.Should().BeEmpty();
        }

        [Theory]
        [InlineData("CreatedBy")]
        [InlineData("CreatedDate")]
        [InlineData("ModifiedBy")]
        [InlineData("ModifiedDate")]
        [InlineData("PasswordHash")]
        [InlineData("SecurityStamp")]
        [InlineData("ConcurrencyStamp")]
        [InlineData("NormalizedEmail")]
        [InlineData("NormalizedUserName")]
        [InlineData("LastLoginDate")]
        [InlineData("AccessFailedCount")]
        [InlineData("LockoutEnd")]
        public void DefaultOptions_NeverRecordSensitiveOrBookkeepingFields(string property)
        {
            var changes = _detector.Detect(Modified(P(property, "old", "new")));

            changes.Should().BeEmpty();
        }

        [Fact]
        public void CustomExclusions_AreHonoured_AndReplaceTheDefauls()
        {
            var detector = new AuditChangeDetector(new AuditOptions
            {
                ExcludedProperties = new HashSet<string> { "Notes" }
            });

            var changes = detector.Detect(Modified(
                P("Notes", "a", "b"),
                P("ModifiedBy", "u1", "u2")));

            changes.Should().ContainSingle().Which.FieldName.Should().Be("ModifiedBy");
        }

        [Fact]
        public void NullToValue_IsRecorded_WithNullOldValue()
        {
            var changes = _detector.Detect(Modified(P("Trim", null, "LE")));

            changes.Should().ContainSingle().Which.Should().Be(
                new AuditChange("Vehicle", "42", AuditAction.Updated, "Trim", null, "LE"));
        }

        [Fact]
        public void ValueToNull_IsRecorded_WithNullNewValue()
        {
            var changes = _detector.Detect(Modified(P("Trim", "LE", null)));

            changes.Should().ContainSingle().Which.NewValue.Should().BeNull();
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", null)]
        public void NullAndEmptyString_AreTreatedAsEqual(string? before, string? after)
        {
            var changes = _detector.Detect(Modified(P("Notes", before, after)));
            
            changes.Should().BeEmpty();
        }

        [Fact]
        public void Decimal_SameValueDifferentScale_IsNotAChange()
        {
            var changes = _detector.Detect(Modified(P("ListPrice", 18500.00m, 18500m)));

            changes.Should().BeEmpty();
        }

        [Fact]
        public void Decimal_IsFormattedWithTwoDecimals_RegardlessOfCurrentCulture()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var change = _detector.Detect(Modified(P("ListPrice", 18500m, 17999.5m))).Single();

                change.OldValue.Should().Be("18500.00");
                change.NewValue.Should().Be("17999.50");
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void Enum_IsRecordedByName()
        {
            var change = _detector.Detect(Modified(
                P("Status", VehicleStatus.Available, VehicleStatus.Sold))).Single();

            change.OldValue.Should().Be("Available");
            change.NewValue.Should().Be("Sold");
        }

        [Fact]
        public void DateTime_IsRecordedAsIso8601()
        {
            var before = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
            var after = new DateTime(2026, 10, 27, 23, 56, 0, DateTimeKind.Utc);

            var change = _detector.Detect(Modified(P("StartDate", before, after))).Single();

            change.OldValue.Should().Be("2026-10-01T12:00:00.0000000Z");
            change.NewValue.Should().Be("2026-10-27T23:56:00.0000000Z");
        }

        [Fact]
        public void Bool_IsRecordedInLowercase()
        {
            var change = _detector.Detect(Modified(P("IsPaid", false, true))).Single();

            change.OldValue.Should().Be("false");
            change.NewValue.Should().Be("true");
        }

        [Fact]
        public void Integer_IsRecordedInvariant()
        {
            var change = _detector.Detect(Modified(P("Mileage", 15000, 1234567))).Single();

            change.OldValue.Should().Be("15000");
            change.NewValue.Should().Be("1234567");
        }

        [Fact]
        public void String_CaseOnlyChange_IsAChange()
        {
            var changes = _detector.Detect(Modified(P("Make", "toyota", "Toyota")));

            changes.Should().ContainSingle();
        }

        [Fact]
        public void LongValue_IsTruncatedToMaxLength_EndingWithEllipsis()
        {
            var detector = new AuditChangeDetector(new AuditOptions { MaxValueLength = 10 });

            var change = detector.Detect(Modified(P("Notes", "short", new string('x', 50)))).Single();

            change.OldValue.Should().Be("short");
            change.NewValue.Should().HaveLength(10).And.EndWith("…");
            change.NewValue.Should().HaveLength(10).And.EndWith("\u2026");
        }

        [Fact]
        public void IsActive_TrueToFalse_IsRecordedAsDeleted()
        {
            var change = _detector.Detect(Modified(P("IsActive", true, false))).Single();

            change.Should().Be(new AuditChange(
                "Vehicle", "42", AuditAction.Deleted, "IsActive", "true", "false"));
        }

        [Fact]
        public void IsActive_FalseToTrue_IsRecordedAsRestored()
        {
            var change = _detector.Detect(Modified(P("IsActive", false, true))).Single();

            change.Action.Should().Be(AuditAction.Restored);
        }

        [Fact]
        public void SoftDeleteTogetherWithOtherChanges_ProducesDeletedPlusUpdatedEntries()
        {
            var changes = _detector.Detect(Modified(
                P("Notes", "ok", "sold elsewhere"),
                P("IsActive", true, false)));

            changes.Select(c => (c.FieldName, c.Action)).Should().Equal(
                ("Notes", AuditAction.Updated),
                ("IsActive", AuditAction.Deleted));
        }

        [Fact]
        public void CustomerSoftDeleteProperty_IsHonoured()
        {
            var detector = new AuditChangeDetector(new AuditOptions { SoftDeleteProperty = "IsEnabled" });

            var changes = detector.Detect(Modified(
                P("IsEnabled", true, false),
                P("IsActive", true, false)));

            changes.Select(c => (c.FieldName, c.Action)).Should().Equal(
                ("IsEnabled", AuditAction.Deleted),
                ("IsActive", AuditAction.Updated));
        }

        [Fact]
        public void NullSnapshot_Throws()
        {
            var act = () => _detector.Detect(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        private static PropertySnapshot P(string name, object? before, object? after) =>
            new(name, before, after);

        private static EntitySnapshot Modified(params PropertySnapshot[] properties) =>
            new("Vehicle", "42", EntityChangeState.Modified, properties);
    }
}
