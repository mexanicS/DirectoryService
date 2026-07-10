using DirectoryService.Application.DirectoryServiceManagement.Departments.RemovePositionFromDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class RemovePositionFromDepartmentTests : DirectoryBaseTests<RemovePositionFromDepartmentHandler>
{
    public RemovePositionFromDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RemovePositionFromDepartment_when_position_assigned_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (departmentId, positionId) = await CreateDepartmentWithPosition();

        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new RemovePositionFromDepartmentCommand(departmentId.Value, positionId.Value);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId.Value, result.Value);
    }

    [Fact]
    public async Task RemovePositionFromDepartment_when_position_not_assigned_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("Legal", "legal");

        // act — позиция не привязана к этому отделу
        var result = await ExecuteHandler(sut =>
        {
            var command = new RemovePositionFromDepartmentCommand(departmentId.Value, Guid.NewGuid());
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    /// <summary>
    /// Создаёт отдел с уже привязанной позицией напрямую через DbContext.
    /// </summary>
    private async Task<(DepartmentId departmentId, PositionId positionId)> CreateDepartmentWithPosition()
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

            var positionId = new PositionId(Guid.NewGuid());
            var position = new Position(
                positionId,
                PositionName.Create("Manager").Value,
                Description.Create(string.Empty).Value);
            context.Positions.Add(position);

            var departmentId = new DepartmentId(Guid.NewGuid());
            var departmentLocation = DepartmentLocation.Create(departmentId, locationId).Value;
            var department = Department.CreateParent(
                DepartmentName.Create("Support").Value,
                Identifier.Create("support").Value,
                [departmentLocation],
                departmentId).Value;

            // привязываем позицию к отделу через доменный метод
            department.AddPosition(positionId.Value);

            context.Departments.Add(department);
            await context.SaveChangesAsync();
            return (departmentId, positionId);
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
}
