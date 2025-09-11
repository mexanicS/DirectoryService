using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configuration;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new PositionId(value));

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Constants.MAX_LENGTH_LOCATION_NAME)
            .HasConversion(name => name.Value, value => PositionName.Create(value).Value);

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(Constants.MAX_LENGTH_DESCRIPTION)
            .HasConversion(name => name.Value, value => Description.Create(value).Value);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("update_at");
    }
}
