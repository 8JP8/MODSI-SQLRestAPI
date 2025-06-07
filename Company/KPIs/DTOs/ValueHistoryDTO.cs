// Company/KPIs/DTOs/ValueHistoryDTO.cs
using System;

namespace MODSI_SQLRestAPI.Company.KPIs.DTO
{
    public class ValueHistoryDTO
    {
        public int Id { get; set; }
        public int KPIId { get; set; }
        public int ChangedByUserId { get; set; }
        public string OldValue_1 { get; set; }
        public string NewValue_1 { get; set; }
        public string OldValue_2 { get; set; }
        public string NewValue_2 { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Unit {  get; set; }
    }
}
