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
            CreateMap<District, DistrictDTO>()
                .ForMember(dest => dest.UserAddresses, opt => opt.MapFrom(src => src.UserAddresses != null ? src.UserAddresses : null))
                .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province != null ? src.Province.Name : null));
            CreateMap<DistrictDTO, District>();
            // Order
            CreateMap<Order, OrderDTO>().ReverseMap();

            //OrderProduct
            CreateMap<OrderProduct, OrderProductDTO>().ReverseMap();

            // Product
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : null))
                .ForMember(dest => dest.PublisherName, opt => opt.MapFrom(src => src.Publisher != null ? src.Publisher.Name : null))
                .ForMember(dest => dest.Rating, opt =>
                {
                    opt.MapFrom(src => CalculateAverageRating(src.Reviews.ToList()));
                })
                .ForMember(dest => dest.SoldQuantity, opt =>
                {
                    opt.MapFrom(src => src.OrderProducts
                        .Where(op => op.Order != null && op.Order.Status == 4)
                        .Sum(op => op.Quantity));
                });
            CreateMap<ProductDTO, Product>();


            // ProductImage
            CreateMap<ProductImage, ProductImageDTO>().ReverseMap();

            // Province
            CreateMap<Province, ProvinceDTO>()
                .ForMember(dest => dest.Districts, opt => opt.MapFrom(src => src.Districts != null ? src.Districts : null))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null));
            CreateMap<ProvinceDTO, Province>();

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


        private decimal CalculateAverageRating(List<Review> reviews)
        {
            if (reviews == null || reviews.Count == 0)
            {
                return 0; // or another default value
            }

            decimal totalRating = reviews.Sum(r => r.Rating);
            return totalRating / reviews.Count;
        }

        /*private int CalculateSoldQuantity(List<OrderProduct> orderProducts)
        {
            if (orderProducts == null || orderProducts.Count == 0)
            {
                return 0; // or another default value
            }

            // Filter the order products by the order's status
            int soldQuantity = orderProducts
                .Where(op => op.Order.Status == 4)
                .Sum(op => op.Quantity);

            return soldQuantity;
        }*/
    }
}

/*
.ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories != null ? src.Categories : null))
                .ForMember(dest => dest.ProductImages, opt => opt.MapFrom(src => src.ProductImages != null ? src.ProductImages : null))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews : null))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null ? src.Tags : null))*/