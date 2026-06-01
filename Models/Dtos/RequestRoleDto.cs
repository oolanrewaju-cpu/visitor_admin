namespace visitor_admin.Models.Dtos
{
    public class RequestRoleDto
    {
        public int RequestRoleID { get; set; }
        public int RequestRoleCode { get; set; }
        public string RequestRoleName { get; set; } = string.Empty;
        public string RequestRoleDescription { get; set; } = string.Empty;
    }
}
