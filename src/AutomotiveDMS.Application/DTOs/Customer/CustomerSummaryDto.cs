using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Customer
{
    public class CustomerSummaryDto
    {
        public int Id { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string IdNumber { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PrimaryPhone { get; init; } = string.Empty;
    }
}
