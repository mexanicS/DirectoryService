using DirectoryService.Application.DirectoryServiceManagement.Locations.GetTop;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.LocationFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetTopLocationByCountDepartmentsTests : DirectoryBaseTests<GetTopLocationByCountDepartmentsHandler>
{
    public GetTopLocationByCountDepartmentsTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTopLocationByCountDepartments_should_return_ordered_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var (locationA, _) = await CreateLocationWithDepartments("Location A", 2);
        var (locationB, _) = await CreateLocationWithDepartments("Location B", 1);
        var (locationC, _) = await CreateLocationWithDepartments("Location C", 0);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value.Locations);

        var topLocations = result.Value.Locations.ToList();
        
        var posA = topLocations.FindIndex(l => l.Id == locationA.Value);
        var posB = topLocations.FindIndex(l => l.Id == locationB.Value);
        var posC = topLocations.FindIndex(l => l.Id == locationC.Value);

        Assert.True(posA >= 0);
        Assert.True(posB >= 0);
        Assert.True(posC >= 0);

        Assert.True(posA < posB, "Location A (2 departments) should be ranked higher than Location B (1 department)");
        Assert.True(posB < posC, "Location B (1 department) should be ranked higher than Location C (0 departments)");

        Assert.Equal(2, topLocations[posA].DepartmentCount);
        Assert.Equal(1, topLocations[posB].DepartmentCount);
        Assert.Equal(0, topLocations[posC].DepartmentCount);
    }

    [Fact]
    public async Task GetTopLocationByCountDepartments_with_cancelled_token_should_cancel()
    {
        // arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // act & assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await ExecuteHandler(sut => sut.Handle(cts.Token));
        });
    }

    private async Task<(LocationId LocationId, List<DepartmentId> DepartmentIds)> CreateLocationWithDepartments(string locationName, int departmentCount)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            var location = new Location(
                locationId,
                LocationName.Create(locationName).Value,
                Address.Create("City", "Street", Guid.NewGuid().ToString().Substring(0, 4), "634000").Value,
                Timezone.Create("normis").Value);
            context.Locations.Add(location);

            var departmentIds = new List<DepartmentId>();
            for (int i = 0; i < departmentCount; i++)
            {
                var departmentId = new DepartmentId(Guid.NewGuid());
                var departmentLocation = DepartmentLocation.Create(departmentId, locationId).Value;
                var uniqueLetters = new string(Guid.NewGuid().ToString().Where(char.IsLetter).ToArray());
                var department = Department.CreateParent(
                    DepartmentName.Create($"Dep{uniqueLetters}").Value,
                    Identifier.Create($"dep{uniqueLetters}").Value,
                    [departmentLocation],
                    departmentId).Value;

                context.Departments.Add(department);
                departmentIds.Add(departmentId);
            }

            await context.SaveChangesAsync();

            return (locationId, departmentIds);
        });
    }
}
