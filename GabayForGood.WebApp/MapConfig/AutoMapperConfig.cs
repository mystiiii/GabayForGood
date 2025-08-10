using AutoMapper;
using GabayForGood.DataModel;
using GabayForGood.WebApp.Models;

namespace GabayForGood.WebApp.MapConfig
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Organization, OrgVM>().ReverseMap();
            CreateMap<Project, ProjectVM>().ReverseMap();
        }
    }
}
