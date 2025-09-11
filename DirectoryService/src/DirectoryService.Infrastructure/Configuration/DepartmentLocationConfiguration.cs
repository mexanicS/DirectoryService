using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configuration;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>

{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");
        
        builder.HasKey(x => x.Id);
        
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new DepartmentLocationId(value));
        
        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new LocationId(value));
        
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new DepartmentId(value));
    }
}