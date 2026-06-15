using CsvHelper.Configuration;

namespace visitor_admin.Models.Dtos
{
    public sealed class CsvStaffImportDtoMap : ClassMap<CsvStaffImportDto>
    {
        public CsvStaffImportDtoMap()
        {
            Map(m => m.Username).Name("Username");
            Map(m => m.Firstname).Name("Firstname");
            Map(m => m.Surname).Name("Surname");
            Map(m => m.Email).Name("Email");
            Map(m => m.Department).Name("Department");
            Map(m => m.StatusID).Name("StatusID");
            Map(m => m.RoleID).Name("RoleID");
            Map(m => m.RequestRoleName).Name("RequestRoleName");
        }
    }
}
