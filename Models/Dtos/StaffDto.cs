namespace visitor_admin.Models.Dtos
{
    public class StaffDto
    {
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public int RequestRoleID { get; set; }
        public DateTime LastModifiedBy { get; set; }
    }
}