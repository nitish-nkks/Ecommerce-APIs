using AutoMapper;
using Ecommerce_APIs.Models.DTOs.BlogPostDtos;
using Ecommerce_APIs.Models.DTOs.ContactMessageDtos;
using Ecommerce_APIs.Models.DTOs.FlashSaleDtos;
using Ecommerce_APIs.Models.DTOs.StaticPageDtos;
using Ecommerce_APIs.Models.Entites;

namespace Ecommerce_APIs.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ContactMessage, ContactMessageDto>();
            CreateMap<ContactMessageCreateDto, ContactMessage>();

            CreateMap<StaticPage, StaticPageDto>().ReverseMap();
            CreateMap<StaticPageCreateDto, StaticPage>();
            CreateMap<StaticPageUpdateDto, StaticPage>();

            CreateMap<BlogPost, BlogPostDto>();
            CreateMap<BlogPostCreateDto, BlogPost>();

            CreateMap<FlashSaleDto, FlashSale>();


        }
    }
}
