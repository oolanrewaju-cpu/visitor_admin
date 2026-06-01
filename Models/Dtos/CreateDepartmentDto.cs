namespace visitor_admin.Models.Dtos
{
    public class CreateDepartmentDto
    {
        public string DepartmentCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentDescription { get; set; } = string.Empty;
    }
}
