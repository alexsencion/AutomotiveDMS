using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Infrastructure.Identity
{
    public sealed class InvitationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public InvitationTokenProviderOptions()
        {
            Name = InvitationTokenProvider.ProviderName;
            TokenLifespan = TimeSpan.FromDays(3);
        }
    }

    public sealed class InvitationTokenProvider : DataProtectorTokenProvider<ApplicationUser>
    {
        public const string ProviderName = "Invitation";
        public const string Purpose = "SetInitialPassword";

        public InvitationTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<InvitationTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<ApplicationUser>> logger)
            : base(dataProtectionProvider, options, logger)
        { }
    }
}
