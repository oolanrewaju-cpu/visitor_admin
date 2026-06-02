namespace visitor_admin.Entities
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentDescription { get; set; } = string.Empty;
        public DateTime LastModifiedBy { get; set; }
    }
}
