using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Webx.Web.Data;
using Webx.Web.Data.Entities;
using Webx.Web.Helpers;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Webx.Web.Data.Repositories;

namespace Webx.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddIdentity<User, IdentityRole>(cfg =>
            {
                cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                cfg.SignIn.RequireConfirmedEmail = true;
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = false;
                cfg.Password.RequireUppercase = false;
                cfg.Password.RequireLowercase = false;
                cfg.Password.RequiredUniqueChars = 0;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequiredLength = 6;
            }).AddDefaultTokenProviders().AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication().AddCookie().AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = this.Configuration["Tokens:Issuer"],
                    ValidAudience = this.Configuration["Tokens:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["Tokens:Key"]))
                };
            });

            services.AddDbContext<DataContext>(cfg =>
            {
                cfg.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });


            services.AddAuthentication().AddFacebook(opts =>
            {
                opts.AppId = "498344415345898";
                opts.AppSecret = "43c3a9dfac8b0263f7600f5df83f4e54";
            }).AddGoogle(opts =>
            {
                opts.ClientId = "237686007342-5agkk9h2abuqcef2ttp383fs0ddcktom.apps.googleusercontent.com";
                opts.ClientSecret = "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";
            });


            services.AddNotyf(cfg =>
            {
                cfg.DurationInSeconds = 5;
                cfg.IsDismissable = true;
                cfg.Position = NotyfPosition.TopRight;
            });

            services.AddTransient<SeedDb>();

            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<IMailHelper, MailHelper>();
            services.AddScoped<IBlobHelper, BlobHelper>();
            services.AddScoped<IConverterHelper, ConverterHelper>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IStatusRepository, StatusRepository>();

            services.AddHttpContextAccessor();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/NotAuthorized";
                options.AccessDeniedPath = "/Account/NotAuthorized";

            });

            services.AddControllersWithViews();
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBPh8sVXJ0S0R+XE9HcFRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS3xTf0RgWH5dc3ZQRmNUUQ==;Mgo+DSMBMAY9C3t2VVhiQlFadVlJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRdkxiWn5fc3dXQ2BZUEQ=");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(new CultureInfo("pt-PT")),
                SupportedCultures = new List<CultureInfo> { new CultureInfo("pt-PT") },
                SupportedUICultures = new List<CultureInfo> { new CultureInfo("pt-PT") }
            });

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseNotyf();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
