using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Financing
{
    public class GuarantorDto
    {
        public int Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Cedula { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Address { get; init; }
        public string Relationship { get; init; } = string.Empty;
    }
}
