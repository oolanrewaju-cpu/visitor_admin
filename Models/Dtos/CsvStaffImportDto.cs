namespace visitor_admin.Models.Dtos
{
    public class CsvStaffImportDto
    {
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int StatusID { get; set; }
        public int RoleID { get; set; }
        public string RequestRoleName { get; set; } = string.Empty;
    }
}
