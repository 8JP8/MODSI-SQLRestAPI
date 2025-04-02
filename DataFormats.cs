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

}
