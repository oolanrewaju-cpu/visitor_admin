namespace visitor_admin.Entities
{
    public class RequestRole
    {
        public int RequestRoleID { get; set; }
        public int RequestRoleCode { get; set; }
        public string RequestRoleName { get; set; } = string.Empty;
        public string RequestRoleDescription { get; set; } = string.Empty;
    }
}
