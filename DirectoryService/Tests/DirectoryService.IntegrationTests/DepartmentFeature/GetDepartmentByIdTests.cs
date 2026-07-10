using DirectoryService.Application.DirectoryServiceManagement.Departments.GetById;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetDepartmentByIdTests : DirectoryBaseTests<GetDepartmentByIdHandler>
{
    public GetDepartmentByIdTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartmentById_with_existing_id_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var departmentId = await CreateDepartmentInDb("Finance", "finance");

        // act
        var result = await ExecuteHandler(sut =>
        {
            var query = new GetDepartmentByIdQuery(departmentId.Value);
            return sut.Handle(query, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(departmentId.Value, result.Value.Id);
        Assert.Equal("Finance", result.Value.Name);
    }

    [Fact]
    public async Task GetDepartmentById_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act — запрашиваем несуществующий Id
        var result = await ExecuteHandler(sut =>
        {
            var query = new GetDepartmentByIdQuery(Guid.NewGuid());
            return sut.Handle(query, cancellationToken);
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
