using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DeviceManagementPortal.Infrastructure;
using DeviceManagementPortal.Infrastructure.API;
using DeviceManagementPortal.Infrastructure.API.Converters;
using DeviceManagementPortal.Infrastructure.API.Filters;
using DeviceManagementPortal.Infrastructure.Contracts;
using DeviceManagementPortal.Infrastructure.EF;
using DeviceManagementPortal.Utility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;

namespace DeviceManagementPortal
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        private readonly IConfiguration Configuration = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region --- Database ---

            services.AddDbContext<DeviceManagementIdentityDbContext>(options =>
            {
                options.UseSqlServer(Configuration["ConnectionStrings:Identity"]);
                options.EnableSensitiveDataLogging(false);
            });
            services.AddDbContext<DeviceManagementDbContext>(options =>
            {
                options.UseSqlServer(Configuration["ConnectionStrings:Application"]);
                options.EnableSensitiveDataLogging(false);
            });
            
            #endregion --- Database ---

            #region --- Dependency Injection ---

            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<DeviceManagementIdentityDbContext>();
            services.AddScoped<IDeviceManagementDbContext, DeviceManagementDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            #endregion --- Dependency Injection ---

            #region --- Identity ---

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
            });

            services.AddAuthentication().AddJwtBearer(options => 
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["JWTSecret"])),
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx => {
                        var userManager = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                        var signInManager = ctx.HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityUser>>();
                        string userName = ctx.Principal.FindFirst(ClaimTypes.Name)?.Value;
                        IdentityUser user = await userManager.FindByNameAsync(userName);
                        ctx.Principal = await signInManager.CreateUserPrincipalAsync(user);
                    }
                };
            });

            #endregion --- Identity ---

            #region --- Web API ---

            services.AddControllers(options =>
            {
                options.Filters.Add(new GlobalExceptionFilterAttribute());
            })
            .ConfigureApiBehaviorOptions(options => 
            {
                // Executes when Model Binding fails.
                options.InvalidModelStateResponseFactory = context =>
                {
                    BadRequestObjectResult result = new BadRequestObjectResult(context.ModelState);
                    result.ContentTypes.Add(MediaTypeNames.Application.Json);

                    return result;
                };
            })
            .AddNewtonsoftJson(options => 
            { 
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new DateTimeConverter(Configuration));
                options.SerializerSettings.Converters.Add(new BadCharacterConverter());
            });
            services.AddRouting();

            #endregion --- Web API ---
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }

            var defaultFileOptions = new DefaultFilesOptions();
            defaultFileOptions.DefaultFileNames.Clear();
            defaultFileOptions.DefaultFileNames.Add("Login.html");

            app.UseDefaultFiles(defaultFileOptions);
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/Error.html");
            });

            SeedData.EnsurePopulated(app);
        }
    }
}
