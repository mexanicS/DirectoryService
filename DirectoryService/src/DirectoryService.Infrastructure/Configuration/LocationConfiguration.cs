using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configuration;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("location");

        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new LocationId(value));

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Constants.MAX_LENGTH_LOCATION_NAME)
            .HasConversion(name => name.Value, value => LocationName.Create(value).Value);

        builder.ComplexProperty(p => p.Address, a => {
            a.Property(address => address.Street)
                .IsRequired()
                .HasMaxLength(Constants.Address.MAX_LENGTH_ADDRESS_STREET)
                .HasColumnName("street");

            a.Property(address => address.City)
                .IsRequired()
                .HasMaxLength(Constants.Address.MAX_LENGTH_ADDRESS_CITY)
                .HasColumnName("city");

            a.Property(address => address.HouseNumber)
                .IsRequired()
                .HasMaxLength(Constants.Address.MAX_LENGTH_ADDRESS_HOUSE_NUMBER)
                .HasColumnName("house_number");

            a.Property(address => address.ZipCode)
                .HasMaxLength(Constants.Address.MAX_LENGTH_ADDRESS_ZIP_CODE)
                .HasColumnName("zip_code");
        });
        
        builder.Property(p => p.Timezone)
            .HasColumnName("timezone")
            .HasConversion(
                name => name.Value, 
                value => Timezone.Create(value).Value);
        
        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("update_at");
        
        builder.HasMany(x => x.DepartmentLocations)
            .WithOne()
            .HasForeignKey(x => x.LocationId);
    }
}
