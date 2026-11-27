using AutomotiveDMS.Application.Security;
using AutomotiveDMS.Domain.Enums;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.UnitTests.Application.Security
{
    public class LoginEventClassifierTests
    {
        [Fact]
        public void Succeeded_KnownUser_IsSucceeded() =>
            Classify(userFound: true, succeeded: true).Should().Be(LoginEventType.Succeeded);

        [Fact]
        public void Failed_KnownUser_IsFailedUnknownUser() =>
            Classify(userFound: false).Should().Be(LoginEventType.FailedUnknownUser);

        [Fact]
        public void Failed_KnownUser_IsFailedPassword() =>
            Classify(userFound: true).Should().Be(LoginEventType.FailedPassword);

        [Fact]
        public void NotAllowed_KnownUser_IsBlockedInactive() =>
            Classify(userFound: true, notAllowed: true).Should().Be(LoginEventType.BlockedInactive);

        [Fact]
        public void LockedOut_NotLockedBefore_IsLockoutTriggered() =>
            Classify(userFound: true, notAllowed: true).Should().Be(LoginEventType.BlockedInactive);

        [Fact]
        public void LockedOut_AlreadyLockedBefore_IsLockoutOuAttempt() =>
            Classify(userFound: true, lockedOut: true, wasLockedBefore: false)
                .Should().Be(LoginEventType.LockoutTriggered);

        [Fact]
        public void WasLockedBefore_ButLockoutExpired_FailedAttempt_IsFailedPassword() =>
            Classify(userFound: true, wasLockedBefore: true).Should().Be(LoginEventType.FailedPassword);

        [Fact]
        public void WasLockedBefore_ButLockoutExpired_SuccessfulAttempt_IsSucceeded() =>
            Classify(userFound: true, succeeded: true, wasLockedBefore: true)
                .Should().Be(LoginEventType.Succeeded);

        [Theory]
        [InlineData(true, true, true, false)]
        [InlineData(true, true, false, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public void ContradictoryOutcome_Throws(bool userFound, bool succeeded, bool lockedOut, bool notAllowed)
        {
            var act = () => Classify(userFound, succeeded, lockedOut, notAllowed);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void NullOutcome_Throws()
        {
            var act = () => LoginEventClassifier.Classify(null!);

            act.Should().Throw<ArgumentNullException>();
        }

        private static LoginEventType Classify(
            bool userFound, bool succeeded = false, bool lockedOut = false,
            bool notAllowed = false, bool wasLockedBefore = false) =>
            LoginEventClassifier.Classify(
                new LoginAttemptOutcome(userFound, succeeded, lockedOut, notAllowed, wasLockedBefore));
    }
}
