using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs.Customer
{
    public class CreateInteractionNoteDto
    {
        public int CustomerId { get; init; }
        public string Channel { get; init; } = string.Empty;
        public string Note { get; init; } = string.Empty;
    }
}
