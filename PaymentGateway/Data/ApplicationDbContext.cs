using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Models;
using PaymentGateway.Models.Tags;

namespace PaymentGateway.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<Store>()
            //    .Property(b => b.Active)
            //    .HasDefaultValue(true);

            //builder.Entity<Merchant>()
            //    .Property(b => b.Active)
            //    .HasDefaultValue(true);
        }

        public DbSet<ServiceProvider> ServiceProvider { get; set; }
        public DbSet<Merchant> Merchant { get; set; }
        public DbSet<Store> Store { get; set; }
        public DbSet<Terminal> Terminal { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<Cryptocurrency> Cryptocurrency { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<TagTerminal> TagTerminals { get; set;}
        public DbSet<TagTerminalConnection> TagTerminalConnections { get; set; }
        public DbSet<TagMerchant> TagMerchant { get; set; }
        public DbSet<TagMerchantConnection> TagMerchantConnection { get; set; }
    }
}
