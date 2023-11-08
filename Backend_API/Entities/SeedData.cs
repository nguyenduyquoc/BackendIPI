using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace Backend_API.Entities
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new BookstoreContext(serviceProvider.GetRequiredService<DbContextOptions<BookstoreContext>>()))
            {
                // Check if tables has any data
                if (!HasData(context))
                {
                    SeedUsers(context);
                    SeedUserAddresses(context);
                    SeedAdmins(context);
                    SeedOrders(context);
                }
            }
        }

        //Check if any tables has data (if just 1 table has data then dont seed)
        private static bool HasData(BookstoreContext context)
        {
            bool hasData =
                context.Set<User>().Any() ||
                context.Set<UserAddress>().Any() ||
                context.Set<Admin>().Any();
            return hasData;
        }


        private static void SeedAdmins(BookstoreContext context)
        {
            ///Seed 1 manager 
            var admin = new Admin
            {
                Fname = "John",
                Lname = "Doe",
                Password = HashPassword("password"),
                Avatar = "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147696/sem3/avatar_ga15mg.png",
                Email = "admin@email.com",
                Phone = "0999888777",
                RoleId = 1
            };
            context.Admins.Add(admin);

            //Seed 1 staffs
            var staff = new Admin
            {
                Fname = "Jenny",
                Lname = "Smith",
                Password = HashPassword("password"),
                Avatar = "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147437/sem3/avatar_2_l9le4z.jpg",
                Email = "staff@email.com",
                Phone = "0111333555",
                RoleId = 2
            };
            context.Admins.Add(staff);

            context.SaveChanges();
        }
        private static void SeedUsers(BookstoreContext context)
        {
            var avatarUrls = new[]
            {
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147437/sem3/avatar_2_l9le4z.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147437/sem3/avatar_4_h2p5ud.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147437/sem3/avatar_3_rroosa.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147437/sem3/avatar_5_jfhcwk.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147436/sem3/avatar_10_majbfp.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147436/sem3/avatar_8_ktllet.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147436/sem3/avatar_6_uoskk1.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147436/sem3/avatar_1_m3cbz1.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147436/sem3/avatar_7_tkiwbv.jpg",
                "https://res.cloudinary.com/dxcyeb8km/image/upload/v1689147436/sem3/avatar_9_wghzit.jpg"
            };
            var random = new Random();

            //Seed 50 users (id = 1 - 50)
            for (int i = 0; i < 50; i++)
            {
                var user = new User
                {
                    Fname = Faker.Name.First(),
                    Lname = Faker.Name.Last(),
                    Password = HashPassword("password"),
                    Avatar = avatarUrls[random.Next(avatarUrls.Length)],
                    Email = Faker.Internet.Email(),
                    Phone = Faker.Identification.UsPassportNumber(),
                    Subscribe = false,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(user);
            }
            context.SaveChanges();
        }

        private static void SeedUserAddresses(BookstoreContext context)
        {
            var random = new Random();
            for (int i = 1; i <= 50; i++)
            {
                var addresses = new[]
                {
                   new UserAddress
                   {
                       UserId = i,
                       Address = "Số 1 Nguyễn Văn An",
                       DistrictId = random.Next(1, 705),
                   },
                   new UserAddress
                   {
                       UserId = i,
                       Address = "Tầng 12 Landmark Resident",
                       DistrictId = random.Next(1, 705),
                   }
                };
                context.UserAddresses.AddRange(addresses);
            }
        }


        private static void SeedOrders(BookstoreContext context)
        {
            var random = new Random();
            var paymentMethods = new[]
            {
                "PAYPAL",
                "VNPAY",
                "COD"
            };


            // Calculate the date 7 months ago from the current date
            var sevenMonthsAgo = DateTime.UtcNow.AddMonths(-7);

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 50; j++)
                {
                    var daysToAdd = i * 27 + j; // Increase the days for each iteration
                    var createdDate = sevenMonthsAgo.AddDays(daysToAdd);

                    var paymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
                    var paymentMethodCode = GetPaymentMethodCode(paymentMethod);

                    var uniqueOrderCode = GenerateUniqueOrderCode(context, paymentMethodCode, createdDate);

                    var subtotal = Math.Round((decimal)70.55 + (decimal)random.NextDouble() * (decimal)(320.68 - 70.55), 2);

                    var orders = new Order
                    {
                        Code = uniqueOrderCode,
                        Status = 4,
                        Name = Faker.Name.FullName(),
                        Email = Faker.Internet.Email(),
                        Phone = Faker.Identification.UsPassportNumber(),
                        Address = Faker.Address.StreetAddress(),
                        District = Faker.Address.City(),
                        Province = Faker.Address.UsState(),
                        Country = Faker.Address.Country(),
                        Subtotal = subtotal,
                        DeliveryFee = 0,
                        GrandTotal = subtotal,
                        PaymentMethod = paymentMethod,
                        UserId = random.Next(7, 20),
                        CreatedAt = createdDate,
                    };
                    context.Orders.Add(orders);
                }
            }
            context.SaveChanges();
        }

        private static string GetPaymentMethodCode(string paymentMethod)
        {
            switch (paymentMethod)
            {
                case "PAYPAL":
                    return "PPL";
                case "VNPAY":
                    return "VNP";
                case "COD":
                    return "COD";
                default:
                    return string.Empty;
            }
        }


        private static string GenerateUniqueOrderCode(BookstoreContext context, string paymentMethodCode, DateTime createdDate)
        {
            string uniqueOrderCode;
            do
            {
                // Generate a random unique string (alphanumeric)
                var uniqueString = GenerateRandomString();
                uniqueOrderCode = $"{paymentMethodCode}{createdDate:yyMMddHHmmss}{uniqueString}";
            } while (context.Orders.Any(o => o.Code == uniqueOrderCode));

            return uniqueOrderCode;
        }

        private static string GenerateRandomString(int length = 4)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var uniqueString = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return uniqueString;
        }

        /*private static void SeedOrderProducts(BookstoreContext context)
        {
            var random = new Random();
            for (int i = 1; i <= 350; i++)
            {
                var orderProducts = new[]
                {
                    new OrderProduct
                    {
                        OrderId = i,
                        ProductId = random.Next(1, 22),
                        Price = (decimal)55.32,
                        Quantity = random.Next(1, 3),
                    },
                    new OrderProduct
                    {
                        OrderId = i,
                        ProductId = random.Next(1, 22),
                        Price = (decimal)55.32,
                        Quantity = random.Next(1, 3),
                    }
                };
                context.OrderProducts.AddRange(orderProducts);
            }

            context.SaveChanges();
        }

        private static void SeedReviews(BookstoreContext context)
        {
            var random = new Random();
            for (int i = 1; i <= 22; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    var review = new Review
                    {
                        ProductId = i,
                        OrderId = random.Next(1, 350),
                        Rating = random.Next(1, 5),
                        Comment = Faker.Lorem.Sentences(2).ToString(),
                    };
                    context.Reviews.Add(review);
                }
            }
            context.SaveChanges();
        }*/

        private static string HashPassword(string password)
        {
            // Hash and salt the password
            var salt = BCrypt.Net.BCrypt.GenerateSalt(6);
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
            return hashedPassword;
        }

        private static string Slugify(string text)
        {
            string slug = text.ToLower(); // Convert text to lowercase
            slug = Regex.Replace(slug, @"\s", "-"); // Replace spaces with dashes
            slug = Regex.Replace(slug, @"[^a-z0-9\-]", ""); // Remove special characters

            return slug;
        }

    }
}
