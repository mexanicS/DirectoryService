using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.BackgroundServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DirectoryService.IntegrationTests;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class SoftDeletePurgeBackgroundServiceTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    protected IServiceProvider Services { get; set; }

    public SoftDeletePurgeBackgroundServiceTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    [Fact]
    public async Task Purge_should_permanently_delete_expired_soft_deleted_records_only()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // 1. Create location: active (should not be deleted)
        var activeLocationId = await CreateLocationInDb("Active Loc", isDeleted: false, ageDays: 0);

        // 2. Create location: soft-deleted but young (should not be deleted)
        var youngLocationId = await CreateLocationInDb("Young Deleted Loc", isDeleted: true, ageDays: 5);

        // 3. Create location: soft-deleted and expired (should be permanently deleted)
        var expiredLocationId = await CreateLocationInDb("Expired Deleted Loc", isDeleted: true, ageDays: 45);

        // 4. Create position: active
        var activePositionId = await CreatePositionInDb("Active Pos", isDeleted: false, ageDays: 0);

        // 5. Create position: expired soft-deleted
        var expiredPositionId = await CreatePositionInDb("Expired Pos", isDeleted: true, ageDays: 45);

        // Instantiate background service
        var service = new SoftDeletePurgeBackgroundService(
            Services.GetRequiredService<IServiceScopeFactory>(),
            Services.GetRequiredService<IConfiguration>(),
            Services.GetRequiredService<ILogger<SoftDeletePurgeBackgroundService>>()
        );

        // act
        await service.PurgeAllDeletedRecordsAsync(cancellationToken);

        // assert
        await ExecuteContext(async context =>
        {
            // Verify locations
            var activeLocation = await context.Locations.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == activeLocationId);
            Assert.NotNull(activeLocation);

            var youngLocation = await context.Locations.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == youngLocationId);
            Assert.NotNull(youngLocation);

            var expiredLocation = await context.Locations.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == expiredLocationId);
            Assert.Null(expiredLocation);

            // Verify positions
            var activePosition = await context.Positions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == activePositionId);
            Assert.NotNull(activePosition);

            var expiredPosition = await context.Positions.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == expiredPositionId);
            Assert.Null(expiredPosition);
        });
    }

    [Fact]
    public async Task Purge_should_delete_departments_hierarchically_bottom_up()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // Create parent and child departments, both soft-deleted and expired
        var (parentId, childId) = await CreateDepartmentHierarchyInDb(isDeleted: true, ageDays: 45);

        // Instantiate background service
        var service = new SoftDeletePurgeBackgroundService(
            Services.GetRequiredService<IServiceScopeFactory>(),
            Services.GetRequiredService<IConfiguration>(),
            Services.GetRequiredService<ILogger<SoftDeletePurgeBackgroundService>>()
        );

        // act
        await service.PurgeAllDeletedRecordsAsync(cancellationToken);

        // assert
        await ExecuteContext(async context =>
        {
            var parent = await context.Departments.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == parentId, cancellationToken);
            Assert.Null(parent);

            var child = await context.Departments.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == childId, cancellationToken);
            Assert.Null(child);
        });
    }

    private async Task<T> ExecuteContext<T>(Func<Infrastructure.DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Infrastructure.DirectoryServiceDbContext>();
        return await action(context);
    }

    private async Task ExecuteContext(Func<Infrastructure.DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Infrastructure.DirectoryServiceDbContext>();
        await action(context);
    }

    private async Task<LocationId> CreateLocationInDb(string name, bool isDeleted, int ageDays)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create(name).Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);
            
            if (isDeleted)
            {
                location.SoftDelete();
                // Set soft-deleted date via reflection since it has a private setter
                var prop = typeof(Location).GetProperty(nameof(Location.SoftDeletedAt));
                prop?.SetValue(location, DateTime.UtcNow.AddDays(-ageDays));
            }

            context.Locations.Add(location);
            await context.SaveChangesAsync();
            return locationId;
        });
    }

    private async Task<PositionId> CreatePositionInDb(string name, bool isDeleted, int ageDays)
    {
        return await ExecuteContext(async context =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create(name).Value,
                Description.Create("Description").Value).Value;

            if (isDeleted)
            {
                position.SoftDelete();
                var prop = typeof(Position).GetProperty(nameof(Position.SoftDeletedAt));
                prop?.SetValue(position, DateTime.UtcNow.AddDays(-ageDays));
            }

            context.Positions.Add(position);
            await context.SaveChangesAsync();
            return positionId;
        });
    }

    private async Task<(DepartmentId ParentId, DepartmentId ChildId)> CreateDepartmentHierarchyInDb(bool isDeleted, int ageDays)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create("Test Location").Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);

            var parentId = new DepartmentId(Guid.NewGuid());
            var parentLocation = DepartmentLocation.Create(parentId, locationId).Value;
            var parent = Department.CreateParent(
                DepartmentName.Create("Parent").Value,
                Identifier.Create("parent").Value,
                [parentLocation],
                parentId).Value;

            var childId = new DepartmentId(Guid.NewGuid());
            var childLocation = DepartmentLocation.Create(childId, locationId).Value;
            var child = Department.CreateChild(
                DepartmentName.Create("Child").Value,
                Identifier.Create("child").Value,
                parent,
                [childLocation],
                childId).Value;

            if (isDeleted)
            {
                parent.SoftDelete();
                child.SoftDelete();

                var parentProp = typeof(Department).GetProperty(nameof(Department.SoftDeletedAt));
                parentProp?.SetValue(parent, DateTime.UtcNow.AddDays(-ageDays));

                var childProp = typeof(Department).GetProperty(nameof(Department.SoftDeletedAt));
                childProp?.SetValue(child, DateTime.UtcNow.AddDays(-ageDays));
            }

            context.Departments.Add(parent);
            context.Departments.Add(child);
            await context.SaveChangesAsync();

            return (parentId, childId);
        });
    }
}
