using DirectoryService.Application.DirectoryServiceManagement.Departments.Get;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetDepartmentsTests : DirectoryBaseTests<GetDepartmentsHandler>
{
    public GetDepartmentsTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDepartments_with_valid_pagination_should_return_paged_result()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        
        var location = await CreateLocationInDb();

        await CreateDepartmentInDb("Engineering", "engineering", location);
        await CreateDepartmentInDb("Marketing", "marketing", location);

        // act
        var result = await ExecuteHandler(sut =>
        {
            var query = new GetDepartmentsQuery(null, null, null, new PaginationRequest(1, 20));
            return sut.Handle(query, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.TotalCount >= 2);
        Assert.NotEmpty(result.Value.Items);
    }

    private async Task<Location> CreateLocationInDb()
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

            await context.SaveChangesAsync();
            
            return location;
        });
    }

    [Fact]
    public async Task GetDepartments_with_zero_page_size_should_return_empty_items()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var location = await CreateLocationInDb();
        
        await CreateDepartmentInDb("Engineering", "engineering", location);

        // act — PageSize = 0 означает что Take(0) вернёт пустую коллекцию
        var result = await ExecuteHandler(sut =>
        {
            var query = new GetDepartmentsQuery(null, null, null, new PaginationRequest(1, 0));
            return sut.Handle(query, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
    }

    private async Task CreateDepartmentInDb(string name, string identifier, Location location)
    {
        await ExecuteContext(async context =>
        {
            var departmentId = new DepartmentId(Guid.NewGuid());
            var departmentLocation = DepartmentLocation.Create(departmentId, location.Id).Value;
            var department = Department.CreateParent(
                DepartmentName.Create(name).Value,
                Identifier.Create(identifier).Value,
                [departmentLocation],
                departmentId).Value;
            context.Departments.Add(department);

            await context.SaveChangesAsync();
        });
    }
}
