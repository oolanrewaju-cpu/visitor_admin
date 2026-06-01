using System.ComponentModel.DataAnnotations.Schema;

namespace visitor_admin.Entities
{
    public class Staff
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int DepartmentID { get; set; }
        public string Password { get; set; } = string.Empty;
        public int StatusID { get; set; }
        public int RoleID { get; set; }
        public int RequestRoleID { get; set; }
        public DateTime LastModifiedBy { get; set; }
    }
}
