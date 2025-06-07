// Company/KPIs/Models/ValueHistory.cs
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODSI_SQLRestAPI.Company.KPIs.Models
{
    [Table("ValueHistory")]
    public class ValueHistory
    {
        public int Id { get; set; }
        public int KPIId { get; set; }
        public KPI KPI { get; set; }
        public int ChangedByUserId { get; set; }
        public string OldValue_1 { get; set; }
        public string NewValue_1 { get; set; }
        public string OldValue_2 { get; set; }
        public string NewValue_2 { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Unit { get; set; }

    }
}
