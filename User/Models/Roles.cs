using MODSI_SQLRestAPI.UserAuth.Models;
using System.Collections.Generic;
using System;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; }
    public ICollection<RoleDepartmentPermission> RoleDepartmentPermissions { get; set; }

    public Role()
    {
        Users = new List<User>();
        RoleDepartmentPermissions = new List<RoleDepartmentPermission>();
    }

    public Role(int id, string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Role name cannot be null or empty.");
        Id = id;
        Name = name;
        Users = new List<User>();
        RoleDepartmentPermissions = new List<RoleDepartmentPermission>();
    }

    public string GetName() => Name;
    public void SetName(string name) => Name = name;
}