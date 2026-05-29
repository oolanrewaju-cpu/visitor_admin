using System.ComponentModel.DataAnnotations.Schema;

namespace visitor_admin.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Firstname { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int DepartmentID { get; set; }
        [ForeignKey("DepartmentID")]
        public Department Department { get; set; } = null!;
        public string Password { get; set; } = string.Empty;
        public int RoleID { get; set; }
        [ForeignKey("RoleID")]
        public RequestRole RequestRole { get; set; } = null!;
        public DateTime LastModifiedBy { get; set; }
    }
}
