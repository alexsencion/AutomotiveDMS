using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Customer
{
    public class UpdateCustomerDto
    {
        public int Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string PrimaryPhone { get; init; } = string.Empty;
        public string? SecondaryPhone { get; init; }
        public string Address { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string? Notes { get; init; }

        public string? FirstName { get; init; }
        public string? LastName { get; init; }

        public string? BusinessName { get; init; }
        public string? ContactPerson { get; init; }
    }
}
