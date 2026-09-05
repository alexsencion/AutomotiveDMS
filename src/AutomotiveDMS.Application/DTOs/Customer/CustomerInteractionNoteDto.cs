using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Customer
{
    public class CustomerInteractionNoteDto
    {
        public int Id { get; init; }
        public string Channel { get; init; } = string.Empty;
        public string Note { get; init; } = string.Empty;
        public string CreatedBy { get; init; } = string.Empty;
        public DateTime CreatedDate { get; init; }
    }
}
