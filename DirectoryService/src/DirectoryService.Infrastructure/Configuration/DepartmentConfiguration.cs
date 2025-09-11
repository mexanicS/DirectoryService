using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Path = DirectoryService.Domain.Departments.Path;

namespace DirectoryService.Infrastructure.Configuration;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("department");

        builder.HasKey(d => d.Id);
        
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => new DepartmentId(value));

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(Constants.MAX_LENGTH_DEPARTMENT_NAME)
            .HasConversion(name => name.Value, value => DepartmentName.Create(value).Value);

        builder.Property(p => p.Identifier)
            .HasColumnName("identifier")
            .IsRequired()
            .HasMaxLength(Constants.MAX_LENGTH_DEPARTMENT_IDENTIFIER)
            .HasConversion(
                name => name.Value, 
                value => Identifier.Create(value).Value);
        
        builder.Property(p => p.ParentId)
            .HasColumnName("parent_id")
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value != null ? new DepartmentId(value.Value) : null);
        
        builder.Property(p => p.Path)
            .HasColumnName("path")
            .IsRequired()
            .HasMaxLength(Constants.MAX_LENGTH_DEPARTMENT_PATH)
            .HasConversion(
                name => name.Value, 
                value => Path.Create(value).Value);

        builder.Property(p => p.Depth)
            .HasColumnName("depth")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("update_at");
        
        builder.HasMany(x=>x.DepartmentsChildren)
            .WithOne()
            .IsRequired(false)
            .HasForeignKey(x=>x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DepartmentLocations)
            .WithOne()
            .HasForeignKey(x => x.DepartmentId);
        
        builder.HasMany(x => x.DepartmentPositions)
            .WithOne()
            .HasForeignKey(x => x.DepartmentId);
    }
}
