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
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
