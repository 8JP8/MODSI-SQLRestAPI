using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.Rooms.Models
{
    public class Room
    {
        public string Id { get; set; } // 5 dígitos alfanuméricos
        public string JsonData { get; set; }
    }
}
