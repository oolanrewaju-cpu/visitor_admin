using AutoMapper;
using visitor_admin.Entities;
using visitor_admin.Models.Dtos;

namespace visitor_admin.Profiles
{
    public class RequestRoleProfile : Profile
    {
        public RequestRoleProfile()
        {
            CreateMap<RequestRole, RequestRoleDto>();
        }
    }
}
