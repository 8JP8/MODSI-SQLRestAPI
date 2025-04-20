using System;

namespace MODSI_SQLRestAPI
{
    internal class LoginRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    internal class Point3D
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    internal class PieChart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
    }

    internal class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Group { get; set; }
        public string Salt { get; set; }
        public bool Encrypted { get; set; }
    }
}
