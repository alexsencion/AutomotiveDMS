using AutomotiveDMS.Infrastructure.Identity;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.IntegrationTests.Identity
{
    public class ApplicationUserTests
    {
        [Fact]
        public void FullName_WhenBothNameSet_ReturnsConcatenatedName()
        {
            var user = new ApplicationUser
            {
                FirstName = "Juan",
                LastName = "Perez"
            };

            user.FullName.Should().Be("Juan Perez");
        }

        [Fact]
        public void FullName_WhenFirstNameOnly_ReturnsFirstNameWithoutTrailingSpace()
        {
            var user = new ApplicationUser
            {
                FirstName = "Juan",
                LastName = string.Empty
            };

            user.FullName.Should().Be("Juan");
        }

        [Fact]
        public void FullName_WhenLastNameOnly_ReturnsLastNameWithoutLeadingSpace()
        {
            var user = new ApplicationUser
            {
                FirstName = string.Empty,
                LastName = "Perez"
            };

            user.FullName.Should().Be("Perez");
        }

        [Fact]
        public void FullName_WhenBothNamesEmpty_ReturnsEmptyString()
        {
            var user = new ApplicationUser
            {
                FirstName = string.Empty,
                LastName = string.Empty
            };

            user.FullName.Should().BeEmpty();
        }

        [Fact]
        public void IsActive_DefaultsToTrue()
        {
            var user = new ApplicationUser();

            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void CreatedDate_DefaultsToUtcNow()
        {
            var before = DateTime.UtcNow;
            var user = new ApplicationUser();
            var after = DateTime.UtcNow;

            user.CreatedDate.Should().BeOnOrAfter(before)
                .And.BeOnOrBefore(after);
        }

        [Fact]
        public void LastLoginDate_DefaultsToNull()
        {
            var user = new ApplicationUser();

            user.LastLoginDate.Should().BeNull();
        }

        [Fact]
        public void FirstName_DefaultsToEmptyString()
        {
            var user = new ApplicationUser();

            user.FirstName.Should().BeEmpty();
        }

        [Fact]
        public void LastName_DefaultsToEmptyString()
        {
            var user = new ApplicationUser();

            user.LastName.Should().BeEmpty();
        }

        [Fact]
        public void IsActive_CanBeSetToFalse()
        {
            var user = new ApplicationUser { IsActive = false };

            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void LastLoginDate_CanBeSet()
        {
            var loginTime = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);
            var user = new ApplicationUser { LastLoginDate = loginTime };

            user.LastLoginDate.Should().Be(loginTime);
        }
    }
}
