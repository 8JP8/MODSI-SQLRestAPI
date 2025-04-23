using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODSI_SQLRestAPI.UserAuth.Models
{
    public class Roles
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Roles() { }

        public Roles(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Role name cannot be null or empty.");

            Id = id;
            Name = name;
        }

        public string GetName() => Name;
        public void SetName(string name) => Name = name;
    }
}
