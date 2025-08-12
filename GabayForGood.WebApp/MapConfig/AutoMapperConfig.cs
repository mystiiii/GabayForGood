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
            CreateMap<Donation, DonationVM>().ReverseMap();

            // Project to ProjectVM mapping
            CreateMap<Project, ProjectVM>()
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore()); // ImageFile is only for form uploads

            // ProjectVM to Project mapping
            CreateMap<ProjectVM, Project>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // Handle ImageUrl manually in controller
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Handle timestamps manually
                .ForMember(dest => dest.ModifiedAt, opt => opt.Ignore()) // Handle timestamps manually
                .ForMember(dest => dest.Organization, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.ProjectUpdates, opt => opt.Ignore()) // Navigation property
                .ForMember(dest => dest.Donations, opt => opt.Ignore()) // Navigation property
                .ForSourceMember(src => src.ImageFile, opt => opt.DoNotValidate()); // Ignore source property that doesn't exist in destination
        }
    }
}