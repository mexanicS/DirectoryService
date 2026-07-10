using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Create;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.PositionFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class CreatePositionTests : DirectoryBaseTests<CreatePositionHandler>
{
    public CreatePositionTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreatePosition_with_valid_data_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("HRDept", "hrdept");

        var command = new CreatePositionDto("HR Specialist", "Manages human resources", [departmentId.Value]);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteContext(async context =>
        {
            var position = await context.Positions.FirstOrDefaultAsync(p => p.Id == new PositionId(result.Value), cancellationToken);
            Assert.NotNull(position);
            Assert.Equal("HR Specialist", position.Name.Value);
            Assert.Equal("Manages human resources", position.Description.Value);
            Assert.True(position.IsActive);

            var depPositionExists = await context.DepartmentPositions.AnyAsync(
                dp => dp.DepartmentId == departmentId && dp.PositionId == position.Id, cancellationToken);
            Assert.True(depPositionExists);
        });
    }

    [Fact]
    public async Task CreatePosition_with_existing_name_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("HRDept", "hrdept");
        
        await ExecuteContext(async context =>
        {
            var existingPosition = Position.Create(
                new PositionId(Guid.NewGuid()),
                PositionName.Create("HR Specialist").Value,
                Description.Create("Existing description").Value).Value;
            context.Positions.Add(existingPosition);
            await context.SaveChangesAsync(cancellationToken);
        });

        var command = new CreatePositionDto("HR Specialist", "Manages human resources", [departmentId.Value]);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

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
