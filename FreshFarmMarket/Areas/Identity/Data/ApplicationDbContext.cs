using FreshFarmMarket.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreshFarmMarket.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<FreshFarmMarketUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.ApplyConfiguration(new FreshFarmMarketUserEntityConfiguration());
    }
    public class FreshFarmMarketUserEntityConfiguration : IEntityTypeConfiguration<FreshFarmMarketUser>
    {
        void IEntityTypeConfiguration<FreshFarmMarketUser>.Configure(EntityTypeBuilder<FreshFarmMarketUser> builder)
        {
            builder.Property(u => u.FullName).HasMaxLength(255);
            builder.Property(u => u.FullName).IsRequired();
            builder.Property(u => u.CreditCardNo).IsRequired();
            builder.Property(u => u.Gender).IsRequired();
            builder.Property(u => u.PhoneNumber).IsRequired();
            builder.Property(u => u.Email).IsRequired();
            builder.Property(u => u.DeliveryAddress).IsRequired();
            builder.Property(u => u.Photo).IsRequired();
            builder.Property(u => u.AboutMe).IsRequired();
        }
    }
}
