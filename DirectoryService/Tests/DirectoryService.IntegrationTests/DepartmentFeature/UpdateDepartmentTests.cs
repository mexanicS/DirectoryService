using DirectoryService.Application.DirectoryServiceManagement.Departments.Update;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class UpdateDepartmentTests : DirectoryBaseTests<UpdateDepartmentHandler>
{
    public UpdateDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartment_with_valid_data_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("OldName", "oldname");

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentCommand(departmentId.Value, "NewName", "newname");
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteContext(async context =>
        {
            var department = await context.Departments.FirstAsync(
                x => x.Id == departmentId, cancellationToken);

            Assert.Equal("NewName", department.Name.Value);
            Assert.Equal("newname", department.Identifier.Value);
        });
    }

    [Fact]
    public async Task UpdateDepartment_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act — передаём Id, которого нет в БД
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentCommand(Guid.NewGuid(), "NewName", "newname");
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
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
}
