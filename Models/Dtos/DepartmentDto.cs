namespace visitor_admin.Models.Dtos
{
    public class DepartmentDto
    {
        public int DepartmentID { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentDescription { get; set; } = string.Empty;
    }
}
