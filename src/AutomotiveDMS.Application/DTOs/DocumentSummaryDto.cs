using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Application.DTOs
{
    public class DocumentSummaryDto
    {
        public int Id { get; init; }
        public string DocumentType { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public string BlobUrl { get; init; } = string.Empty;
        public long FileSizeBytes { get; init; }
        public string UploadedBy { get; init; } = string.Empty;
        public DateTime UploadedDate { get; init; }
    }
}
