namespace MODSI_SQLRestAPI.Company.Departments.Models
{
    public class RoleDepartmentPermission
    {
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }

        public RoleDepartmentPermission() { }

        public RoleDepartmentPermission(int roleId, int departmentId, bool canRead, bool canWrite)
        {
            RoleId = roleId;
            DepartmentId = departmentId;
            CanRead = canRead;
            CanWrite = canWrite;
        }

        // Getters and setters
        public bool GetCanRead() => CanRead;
        public void SetCanRead(bool canRead) => CanRead = canRead;

        public bool GetCanWrite() => CanWrite;
        public void SetCanWrite(bool canWrite) => CanWrite = canWrite;
    }
}
