using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Customer
{
    public class CreateCustomerDto
    {
        public string CustomerType { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PrimaryPhone { get; init; } = string.Empty;
        public string? SecondPhone { get; init; }
        public string Address { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string? Notes { get; init; }

        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Cedula { get; init; }

        public string? BusinessName { get; init; }
        public string? Rnc { get; init; }
        public string? ContactPerson { get; init; }
    }
}
