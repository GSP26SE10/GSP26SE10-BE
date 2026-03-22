using BookfetSystem.Repositories;
using BookfetSystem.Repositories.DBContext;
using BookfetSystem.Services.Implement;
using BookfetSystem.Services.Interface;
using BookfetSystem.Services.Mappings;
using BookfetSystem.API.Middlewares;
using BookfetSystem.Services.Options;
using BookfetSystem.API.BackgroundJobs;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using BookfetSystem.Services.Interfaces;
using BookfetSystem.Services.Services;
using BookfetSystem.Services.Hubs;
using Microsoft.AspNetCore.SignalR;
using BookfetSystem.Services.Helpers;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "JWT Authorization header using the access token",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };
    options.SwaggerDoc("v1", new() { Title = "Bookfet Management System", Version = "v1" });
    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme, Array.Empty<string>()
                    }
                });
});

// Add DbContext
builder.Services.AddDbContext<GSP26SE10DBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config =>
    config.UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings()
          .UsePostgreSqlStorage(options =>
              options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddHangfireServer();

// Register Mapster mappings
MapsterConfig.RegisterMappings();

// DI for services and repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<StaffGroupRepository>();
builder.Services.AddScoped<StaffGroupMemberRepository>();
builder.Services.AddScoped<ConversationRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<OrderDetailRepository>();
builder.Services.AddScoped<OrderDetailStaffTaskRepository>();
builder.Services.AddScoped<OrderDetailExtraChargeRepository>();
builder.Services.AddScoped<ExtraChargeCatalogRepository>();
builder.Services.AddScoped<FeedbackMenuRepository>();
builder.Services.AddScoped<FeedbackServiceRepository>();
builder.Services.AddScoped<MenuRepository>();
builder.Services.AddScoped<MenuCategoryRepository>();
builder.Services.AddScoped<BlogCategoryRepository>();
builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<PostBlockRepository>();
builder.Services.AddScoped<DishRepository>();
builder.Services.AddScoped<DishCategoryRepository>();
builder.Services.AddScoped<IngredientRepository>();
builder.Services.AddScoped<DishDetailRepository>();
builder.Services.AddScoped<MenuDishRepository>();
builder.Services.AddScoped<PartyCategoryRepository>();
builder.Services.AddScoped<PartyCategoryMenuRepository>();
builder.Services.AddScoped<ServiceRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<ServiceRepository>();
builder.Services.AddScoped<OrderDetailCustomRepository>();
builder.Services.AddScoped<OrderDetailRepository>();
builder.Services.AddScoped<PaymentRepository>();
builder.Services.AddScoped<OrderServiceRepository>();
builder.Services.AddScoped<ContactRequestRepository>();
builder.Services.AddScoped<UserDeviceRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<AISuggestionHandler>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICache, MemoryCacheService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IStaffGroupService, StaffGroupService>();
builder.Services.AddScoped<IStaffGroupMemberService, StaffGroupMemberService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IOrderDetailStaffTaskService, OrderDetailStaffTaskService>();
builder.Services.AddScoped<IOrderDetailExtraChargeService, OrderDetailExtraChargeService>();
builder.Services.AddScoped<IFeedbackMenuService, FeedbackMenuService>();
builder.Services.AddScoped<IFeedbackServiceService, FeedbackServiceService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMenuCategoryService, MenuCategoryService>();
builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IPostBlockService, PostBlockService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IDishCategoryService, DishCategoryService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IDishDetailService, DishDetailService>();
builder.Services.AddScoped<IPartyCategoryService, PartyCategoryService>();
builder.Services.AddScoped<IMenuDishService, MenuDishService>();
builder.Services.AddScoped<IPartyCategoryMenuService, PartyCategoryMenuService>();
builder.Services.AddScoped<ICustomerOrderService, CustomerOrderService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IOrderDetailCustomService, OrderDetailCustomService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();
builder.Services.AddScoped<ISePayWebhookService, SePayWebhookService>();
builder.Services.AddScoped<IOrderServiceManager, OrderServiceManager>();
builder.Services.AddScoped<IOrderStatusTransitionJob, OrderStatusTransitionJob>();
builder.Services.AddScoped<IOrderStatusSchedulerService, OrderStatusSchedulerService>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddScoped<IContactRequestService, ContactRequestService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAIRecommendationService, GeminiRecommendationService>();
builder.Services.AddSignalR();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtConfig");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JwtConfig:Key is missing.");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Custom response when the token is invalid or missing
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },

        OnChallenge = async context =>
        {
            // Skip default behavior
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                Success = false,
                Status = 401,
                Message = "Unauthorized: Token is missing or invalid"
            }));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                Success = false,
                Status = 403,
                Message = "Forbidden: You do not have permission to access this resource"
            }));
        }
    };
});

// CORS configuration from appsettings
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins is { Length: > 0 })
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            throw new InvalidOperationException("CORS configuration is missing: 'Cors:AllowedOrigins' is not set for the current environment.");
        }
    });
});

//builder.WebHost.UseUrls("http://0.0.0.0:5121"); // for public access with tunnel 

var app = builder.Build();

// Trust reverse proxy (Fly, Nginx, etc.) for scheme/host
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

app.UseGlobalException();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Allow dashboard access from deployed environments (not only localhost).
    Authorization = [new AllowAllHangfireDashboardAuthorizationFilter()]
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();

app.MapGet("/", () => Results.Ok("BookfetSystem API is running"));

internal sealed class AllowAllHangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}