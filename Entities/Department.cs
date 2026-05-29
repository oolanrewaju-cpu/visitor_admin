using System.ComponentModel.DataAnnotations;

namespace visitor_admin.Entities
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public int DepartmentCode { get; set; }
        [StringLength(100)]
        public string DepartmentName { get; set; } = string.Empty;
        [StringLength(500)]
        public string DepartmentDescription { get; set;} = string.Empty;
    }
}
