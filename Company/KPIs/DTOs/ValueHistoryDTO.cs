// Company/KPIs/DTOs/ValueHistoryDTO.cs
using System;

namespace MODSI_SQLRestAPI.Company.KPIs.DTO
{
    public class ValueHistoryDTO
    {
        public int Id { get; set; }
        public int KPIId { get; set; }
        public int ChangedByUserId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
