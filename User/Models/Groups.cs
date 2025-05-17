using System;

namespace MODSI_SQLRestAPI.UserAuth.Models
{
    public class Groups
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Groups() { }

        public Groups(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Group name cannot be null or empty.");

            Id = id;
            Name = name;
        }

        public string GetName() => Name;
        public void SetName(string name) => Name = name;
    }
}
