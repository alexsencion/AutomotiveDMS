using AutomotiveDMS.Domain.Common;
using AutomotiveDMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Entities
{
    public class Document : BaseEntity
    {
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string BlobUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }

        public int? VehicleId { get; set; }
        public int? CustomerId { get; set; }
        public int? ContractId { get; set; }

        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }

        public Vehicle? Vehicle { get; set; }
        public Customer? Customer { get; set; }
        public FinancingContract? Contract { get; set; }
    }
}
