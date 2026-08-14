using DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class DeleteDepartmentTests : DirectoryBaseTests<DeleteDepartmentHandler>
{
    public DeleteDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteDepartment_with_existing_id_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("ToDelete", "todelete");

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new DeleteDepartmentCommand(departmentId.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteContext(async context =>
        {
            var exists = await context.Departments.AnyAsync(
                x => x.Id == departmentId, cancellationToken);

            Assert.False(exists);
        });
    }

    [Fact]
    public async Task DeleteDepartment_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act — пытаемся удалить отдел, которого нет в БД
        var result = await ExecuteHandler(sut =>
        {
            var command = new DeleteDepartmentCommand(Guid.NewGuid());
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DeleteDepartment_with_children_should_reparent_subtree()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (rootId, deletedDepartmentId, childId) = await CreateDepartmentHierarchyInDb();

        // act
        var result = await ExecuteHandler(sut => sut.Handle(
            new DeleteDepartmentCommand(deletedDepartmentId.Value),
            cancellationToken));

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteContext(async context =>
        {
            var deletedDepartmentExists = await context.Departments
                .AnyAsync(d => d.Id == deletedDepartmentId, cancellationToken);
            var child = await context.Departments
                .SingleAsync(d => d.Id == childId, cancellationToken);

            Assert.False(deletedDepartmentExists);
            Assert.Equal(rootId, child.ParentId);
            Assert.Equal("root.child", child.Path.Value);
            Assert.Equal(1, child.Depth.Value);
        });
    }

    private async Task<DepartmentId> CreateDepartmentInDb(string name, string identifier)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create("Tomsk").Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);

            var departmentId = new DepartmentId(Guid.NewGuid());
            var departmentLocation = DepartmentLocation.Create(departmentId, locationId).Value;
            var department = Department.CreateParent(
                DepartmentName.Create(name).Value,
                Identifier.Create(identifier).Value,
                [departmentLocation],
                departmentId).Value;
            context.Departments.Add(department);

            await context.SaveChangesAsync();
            return departmentId;
        });
    }

    private async Task<(DepartmentId rootId, DepartmentId deletedDepartmentId, DepartmentId childId)>
        CreateDepartmentHierarchyInDb()
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create("Tomsk").Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);

            var rootId = new DepartmentId(Guid.NewGuid());
            var root = Department.CreateParent(
                DepartmentName.Create("Root").Value,
                Identifier.Create("root").Value,
                [DepartmentLocation.Create(rootId, locationId).Value],
                rootId).Value;

            var deletedDepartmentId = new DepartmentId(Guid.NewGuid());
            var deletedDepartment = Department.CreateChild(
                DepartmentName.Create("Middle").Value,
                Identifier.Create("middle").Value,
                root,
                [DepartmentLocation.Create(deletedDepartmentId, locationId).Value],
                deletedDepartmentId).Value;

            var childId = new DepartmentId(Guid.NewGuid());
            var child = Department.CreateChild(
                DepartmentName.Create("Child").Value,
                Identifier.Create("child").Value,
                deletedDepartment,
                [DepartmentLocation.Create(childId, locationId).Value],
                childId).Value;

            context.Departments.AddRange(root, deletedDepartment, child);
            await context.SaveChangesAsync();

            return (rootId, deletedDepartmentId, childId);
        });
    }
}
