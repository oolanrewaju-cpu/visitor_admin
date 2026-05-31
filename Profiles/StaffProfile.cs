using AutoMapper;
using visitor_admin.Entities;
using visitor_admin.Models.Dtos;

namespace visitor_admin.Profiles
{
    public class StaffProfile : Profile
    {
        public StaffProfile()
        {
            CreateMap<Staff, StaffDto>();
            CreateMap<CreateStaffDto, Staff>();
            CreateMap<Staff, CreateStaffDto>();
            CreateMap<UpdateStaffDto, Staff>();
            CreateMap<PatchStaffDto, Staff>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
