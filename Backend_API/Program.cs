using Backend_API;
using Backend_API.Entities;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add CORS:
builder.Services.AddCors(options =>
{
    // chinh sach bao mat trong trinh duyet web
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
        }
    );
});

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling 
    = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper configuration
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add connection to databbase.
var connectionString = builder.Configuration.GetConnectionString("Bookstore");
builder.Services.AddDbContext<BookstoreContext>(
    options => options.UseSqlServer(connectionString)
);

// Add Authentication JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

// ADD POLICY:
builder.Services.AddAuthorization(options =>
{
    // -- Simple policy (Gia su chi co "admin@email.com" moi co the create Category)
    options.AddPolicy("SuperAdmin", policy => policy.RequireClaim(ClaimTypes.Email, "admin@email.com")); //Phai co email va email phai la admin@email.com
    options.AddPolicy("RoleRequire", policy => policy.RequireClaim(ClaimTypes.Role)); //Phai co role (khong quan trong role gi)

    
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1 - Them CORS:
app.UseCors();

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

app.MapControllers();

app.Run();
