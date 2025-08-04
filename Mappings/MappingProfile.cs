using AutoMapper;
using Ecommerce_APIs.Models.DTOs.BlogPostDtos;
using Ecommerce_APIs.Models.DTOs.ContactMessageDtos;
using Ecommerce_APIs.Models.DTOs.FlashSaleDtos;
using Ecommerce_APIs.Models.DTOs.InternalUserDtos;
using Ecommerce_APIs.Models.DTOs.ProductDtos;
using Ecommerce_APIs.Models.DTOs.StaticPageDtos;
using Ecommerce_APIs.Models.DTOs.UserDtos;
using Ecommerce_APIs.Models.Entites;
using Ecommerce_APIs.Models.Entities;

namespace Ecommerce_APIs.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateInternalUserDto, InternalUser>();
            CreateMap<UpdateInternalUserDto, InternalUser>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AddCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<AddProductDto, Product>();
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());


            CreateMap<ContactMessage, ContactMessageDto>();
            CreateMap<ContactMessageCreateDto, ContactMessage>();

            CreateMap<StaticPage, StaticPageDto>().ReverseMap();
            CreateMap<StaticPageCreateDto, StaticPage>();
            CreateMap<StaticPageUpdateDto, StaticPage>();

            CreateMap<BlogPost, BlogPostDto>();
            CreateMap<BlogPostCreateDto, BlogPost>();

            CreateMap<FlashSaleDto, FlashSale>();

            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        }
    }
}
