namespace visitor_admin.Models.Dtos
{
    public class PatchStaffDto
    {
        public int UserID { get; set; }
        public string? Username { get; set; }
        public string? Firstname { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public int? DepartmentID { get; set; }
        public string? Password { get; set; }
        public int? StatusID { get; set; }
        public int? RoleID { get; set; }
        public int? RequestRoleID { get; set; }
        public string? RequestRoleName { get; set; }
    }
}
