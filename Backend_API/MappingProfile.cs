﻿using AutoMapper;
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
                .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province != null ? src.Province.Name : null))
                .ForMember(dest => dest.DeliveryServices, opt => opt.MapFrom(src => src.DeliveryServices != null ? src.DeliveryServices : null));
            CreateMap<DistrictDTO, District>();

            // Order
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.ReturnRequestId, opt => opt.MapFrom(src => src.ReturnRequest.Id))
                .ForMember(dest => dest.ReturnRequestStatus, opt => opt.MapFrom(src => src.ReturnRequest.Status));
            CreateMap<OrderDTO, Order>();
            CreateMap<OrderCreateModel, Order>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.Now));

            //OrderProduct
            CreateMap<OrderProduct, OrderProductDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductSlug, opt => opt.MapFrom(src => src.Product.Slug))
                .ForMember(dest => dest.ProductThumbnail, opt => opt.MapFrom(src => src.Product.Thumbnail));

            CreateMap<OrderProductDTO, OrderProduct>();
            CreateMap<OrderProductCreateModel, OrderProduct>();

            // Product
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src =>
                    src.OrderProducts.Where(op => op.Review != null).Select(op => op.Review).ToList()))
                .ForMember(dest => dest.Rating, opt =>
                {
                    opt.MapFrom(src => CalculateAverageRating(
                        src.OrderProducts.Where(op => op.Review != null).Select(op => op.Review).ToList()));
                })
                .ForMember(dest => dest.SoldQuantity, opt =>
                {
                    opt.MapFrom(src => src.OrderProducts
                        .Where(op => op.Order != null && op.Order.Status == 5)
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
            CreateMap<ReturnRequest, ReturnRequestDTO>()
                .ForMember(dest => dest.ReturnProducts, opt => opt.MapFrom(src => src.Order.OrderProducts.Where(op => op.ReturnQuantity > 0)));
            CreateMap<ReturnRequestDTO, ReturnRequest>();

            // Return Request Image
            CreateMap<ReturnRequestImage, ReturnRequestImageDTO>();

            // Review
            CreateMap<Review, ReviewDTO>()
                .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.OrderProduct.Order.Id))
                .ForMember(dest => dest.Fname, opt => opt.MapFrom(src => src.OrderProduct.Order.User.Fname))
                .ForMember(dest => dest.Lname, opt => opt.MapFrom(src => src.OrderProduct.Order.User.Lname))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.OrderProduct.Order.User.Avatar))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.OrderProduct.Order.User.Email));
            CreateMap<ReviewDTO, Review>();
            CreateMap<ReviewCreateModel, Review>();

            // Admin
            CreateMap<Admin, AdminDTO>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : null));
            CreateMap<AdminDTO, Admin>();


            // Tag
            CreateMap<Tag, TagDTO>().ReverseMap();
            CreateMap<TagCreateModel, Tag>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => Slugify(src.Name)));

            // User
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<UserProfileEdit, User>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.Now)); ;

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

            CreateMap<DeliveryService, DeliveryServiceDTO>().ReverseMap();

            // Admin
            /*CreateMap<Admin, AdminDTO>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.Name))
                .ForMember(dest => dest.MenuItems, opt => opt.MapFrom(src => src.Role.MenuItems))
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Role.Permissions));*/
            // ROLE
            CreateMap<Role, RoleDTO>().ReverseMap();
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
