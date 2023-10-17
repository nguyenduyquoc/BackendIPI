using AutoMapper;
using Backend_API.DTOs;
using Backend_API.Entities;
using Backend_API.ViewModels;
using System.Text.RegularExpressions;

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

            CreateMap<CategoryCreateModel, Category>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));
            CreateMap<CategoryEditModel, Category>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // Author
            CreateMap<Author, AuthorDTO>().ReverseMap();
            CreateMap<AuthorCreateModel, Author>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));
            CreateMap<AuthorEditModel, Author>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));


            // Country
            CreateMap<Country, CountryDTO>()
                .ForMember(dest => dest.Provinces, opt => opt.MapFrom(src => src.Provinces != null ? src.Provinces : null));
            CreateMap<CountryDTO, Country>();

            // Coupon
            CreateMap<Coupon, CouponDTO>().ReverseMap();
            CreateMap<CouponCreateModel, Coupon>();
            CreateMap<CouponEditModel, Coupon>();

            // District
            CreateMap<District, DistrictDTO>()
                .ForMember(dest => dest.UserAddresses, opt => opt.MapFrom(src => src.UserAddresses != null ? src.UserAddresses : null))
                .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province != null ? src.Province.Name : null));
            CreateMap<DistrictDTO, District>();

            // Order
            CreateMap<Order, OrderDTO>().ReverseMap();
            CreateMap<OrderCreateModel, Order>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now));

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

            CreateMap<ProductCreateModel, Product>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));
            CreateMap<ProductEditModel, Product>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));


            // ProductImage
            CreateMap<ProductImage, ProductImageDTO>().ReverseMap();
            CreateMap<ProductImageCreateModel, ProductImage>();

            // Province
            CreateMap<Province, ProvinceDTO>()
                .ForMember(dest => dest.Districts, opt => opt.MapFrom(src => src.Districts != null ? src.Districts : null))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : null));
            CreateMap<ProvinceDTO, Province>();

            // Publisher
            CreateMap<Publisher, PublisherDTO>().ReverseMap();
            CreateMap<PublisherEditModel, Publisher>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));
            CreateMap<PublisherCreateModel, Publisher>()
                 .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));

            // ReturnRequest
            CreateMap<ReturnRequest, ReturnRequestDTO>().ReverseMap();

            // Review
            CreateMap<Review, ReviewDTO>().ReverseMap();
            CreateMap<ReviewCreateModel, Review>();
            CreateMap<ReviewEditModel, Review>();

            // Admin
            CreateMap<Admin, AdminDTO>().ReverseMap();

            // Tag
            CreateMap<Tag, TagDTO>().ReverseMap();
            CreateMap<TagCreateModel, Tag>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));

            // User
            CreateMap<User, UserDTO>().ReverseMap();

            // UserAddress
            CreateMap<UserAddress, UserAddressDTO>().ReverseMap();

            CreateMap<UserAddress, UserAddressDTO>()
                .ForMember(dest => dest.DistrictId, opt => opt.MapFrom(src => src.District.Id))
                .ForMember(dest => dest.DistrictName, opt => opt.MapFrom(src => src.District.Name))
                .ForMember(dest => dest.ProvinceId, opt => opt.MapFrom(src => src.District.Province.Id))
                .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.District.Province.Name));
            CreateMap<UserAddressDTO, UserAddress>();
            CreateMap<UserAddressCreateModel, UserAddress>();
            CreateMap<UserAddressEditModel, UserAddress>();

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

        private static string Slugify(string text)
        {
            string slug = text.ToLower(); // Convert text to lowercase
            slug = Regex.Replace(slug, @"\s", "-"); // Replace spaces with dashes
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", ""); // Remove special characters

            return slug;
        }
    }
}

/*
.ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories != null ? src.Categories : null))
                .ForMember(dest => dest.ProductImages, opt => opt.MapFrom(src => src.ProductImages != null ? src.ProductImages : null))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews : null))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null ? src.Tags : null))*/