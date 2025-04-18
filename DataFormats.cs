using System;

namespace MODSI_SQLRestAPI
{
    public class Point3D
    {
        public int ID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class PieChart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Value { get; set; }
    }

    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IsActive { get; set; }
    }

}
