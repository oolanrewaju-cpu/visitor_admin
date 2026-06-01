using AutoMapper;
using visitor_admin.Entities;
using visitor_admin.Models.Dtos;

namespace visitor_admin.Profiles
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {
            CreateMap<Department, DepartmentDto>();
            CreateMap<CreateDepartmentDto, Department>();
            CreateMap<Department, CreateDepartmentDto>();
            CreateMap<UpdateDepartmentDto, Department>();
            CreateMap<PatchDepartmentDto, Department>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
