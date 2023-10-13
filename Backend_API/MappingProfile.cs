using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;

namespace Backend_API
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            // Category
            CreateMap<Category, CategoryDTO>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products != null ? src.Products : null))
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
                .ForMember(dest => dest.InverseParent, opt => opt.MapFrom(src => src.InverseParent != null ? src.InverseParent : null));
            CreateMap<CategoryDTO, Category>();

            // Author
            CreateMap<Author, AuthorDTO>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));
            CreateMap<AuthorDTO, Author>();


            // Country
            CreateMap<Country, CountryDTO>()
                .ForMember(dest => dest.Provinces, opt => opt.MapFrom(src => src.Provinces != null ? src.Provinces : null));
            CreateMap<CountryDTO, Country>();

            // Coupon
            CreateMap<Coupon, CountryDTO>().ReverseMap();

            // District
            CreateMap<District, DistrictDTO>().ReverseMap();

            // Order
            CreateMap<Order, OrderDTO>().ReverseMap();

            //OrderProduct
            CreateMap<OrderProduct, OrderProductDTO>().ReverseMap();

            // Product
            CreateMap<Product, ProductDTO>().ReverseMap();

            // ProductImage
            CreateMap<ProductImage, ProductImageDTO>().ReverseMap();

            // Province
            CreateMap<Province, ProvinceDTO>().ReverseMap();

            // Publisher
            CreateMap<Publisher, PublisherDTO>().ReverseMap();

            // ReturnRequest
            CreateMap<ReturnRequest, ReturnRequestDTO>().ReverseMap();

            // Review
            CreateMap<Review, ReviewDTO>().ReverseMap();

            // Admin
            CreateMap<Admin, AdminDTO>().ReverseMap();

            // Tag
            CreateMap<Tag, TagDTO>().ReverseMap();

            // User
            CreateMap<User, UserDTO>().ReverseMap();

            // UserAddress
            CreateMap<UserAddress, UserAddressDTO>().ReverseMap();

        }
    }
}
