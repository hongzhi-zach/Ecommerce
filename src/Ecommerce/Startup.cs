using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Ecommerce.Data;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Data.Repositories;
using Ecommerce.Data.Interfaces;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Data.SqlClient;

namespace Ecommerce
{
    public class Startup
    {
        //iconfigurationroot represent the entry to configuration data
        private IConfigurationRoot _configurationRoot;
        public Startup(IHostingEnvironment hostingEnvironment)
        {
            _configurationRoot = new ConfigurationBuilder()
                //setbase method point the root of the application
                .SetBasePath(hostingEnvironment.ContentRootPath)
                //point the json config file 
                .AddJsonFile("appsettings.json")
                //pass the configurations using build method
                .Build();
        }
        public void ConfigureServices(IServiceCollection services)
        {
            //Server configuration, import appdbcontext & entity framework
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_configurationRoot.GetConnectionString("DefaultConnection")));
            //Authentication, Identity config
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<IDrinkRepository, DrinkRepository>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //request object allows multiple users requesting the same time.
            services.AddScoped(sp => ShoppingCart.GetCart(sp));
            services.AddTransient<IOrderRepository, OrderRepository>();

            services.AddMvc();
            services.AddMemoryCache();
            services.AddSession();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseSession();
            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                   name: "drinkdetails",
                   template: "Drink/Details/{drinkId?}",
                   defaults: new { Controller = "Drink", action = "Details"});

                routes.MapRoute(
                    name: "categoryfilter",
                    template: "Drink/{action}/{category?}",
                    defaults: new { Controller = "Drink", action = "List" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{Id?}");
            });

            //check if data is in database using dbinitializer
            DbInitializer.Seed(app);
        }
    }
}
