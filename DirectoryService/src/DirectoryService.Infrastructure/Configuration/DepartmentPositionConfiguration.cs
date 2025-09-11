using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configuration;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>

{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");
        
        builder.HasKey(x => x.Id);
        
        
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new DepartmentPositionId(value));
        
        builder.Property(x => x.PositionId)
            .HasColumnName("position_id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new PositionId(value));
        
        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new DepartmentId(value));
    }
}