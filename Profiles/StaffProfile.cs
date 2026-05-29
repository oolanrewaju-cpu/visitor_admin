using AutoMapper;
using visitor_admin.Entities;
using visitor_admin.Models.Dtos;

namespace visitor_admin.Profiles
{
    public class StaffProfile : Profile
    {
        public StaffProfile()
        {
            CreateMap<Staff, StaffDto>(); // Map from Staff entity to StaffDto
            CreateMap<StaffDto, Staff>(); // Map from StaffDto to Staff entity
            CreateMap<Staff, UpdateStaffDto>(); // Map from Staff entity to StaffUpdateDto>
            CreateMap<UpdateStaffDto, Staff>(); // Map from StaffUpdateDto to Staff entity>
            CreateMap<Staff, CreateStaffDto>(); // Map from Staff entity to StaffCreateDto>
            CreateMap<CreateStaffDto, Staff>(); // Map from StaffCreateDto to Staff entity>
        }
    }
}
