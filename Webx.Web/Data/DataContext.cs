using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Webx.Web.Data.Entities;

namespace Webx.Web.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Store> Stores { get; set; }

        public DbSet<Stock> Stocks { get; set; }

        public DbSet<ProductImages> ProductImages { get; set; }

        public DbSet<Brand> Brands { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<OrderDetailTemp> OrderDetailTemps { get; set; }

        public DbSet<Appointment> Appointments { get; set; }


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {


        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);          

            builder.Entity<User>().HasIndex(u => u.NIF).IsUnique();
        }


    }


   
}
