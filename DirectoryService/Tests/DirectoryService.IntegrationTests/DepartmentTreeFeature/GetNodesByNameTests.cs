using DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetNodesByName;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.DepartmentTreeFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetNodesByNameTests : DirectoryBaseTests<GetNodesByNameHandler>
{
    public GetNodesByNameTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetNodesByName_on_empty_tree_should_return_empty_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act — при пустой базе
        var result = await ExecuteHandler(sut =>
        {
            var command = new GetNodesByNameHandlerCommand("Backend");
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetNodesByName_when_no_matching_name_should_return_empty_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        await CreateDepartmentHierarchy();

        // act — поиск по названию, которого нет в базе
        var result = await ExecuteHandler(sut =>
        {
            var command = new GetNodesByNameHandlerCommand("NonExistentDepartmentName");
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetNodesByName_with_too_short_query_should_return_validation_error(string invalidQuery)
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act — слишком короткий или пустой запрос
        var result = await ExecuteHandler(sut =>
        {
            var command = new GetNodesByNameHandlerCommand(invalidQuery);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetNodesByName_with_matching_name_should_return_nodes_and_ancestors()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (root, child, grandChild) = await CreateDepartmentHierarchy();

        // act — ищем по названию "acke" (частичное совпадение с "Backend")
        var result = await ExecuteHandler(sut =>
        {
            var command = new GetNodesByNameHandlerCommand("acke");
            return sut.Handle(command, cancellationToken);
        });

        // assert — должен вернуть найденный узел и всех его предков (root, child, grandChild)
        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        Assert.Contains(result.Value, x => x.Id == grandChild.Id.Value);
        Assert.Contains(result.Value, x => x.Id == child.Id.Value);
        Assert.Contains(result.Value, x => x.Id == root.Id.Value);
    }

    private async Task<(Department root, Department child, Department grandChild)> CreateDepartmentHierarchy()
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
            var rootLoc = DepartmentLocation.Create(rootId, locationId).Value;
            var root = Department.CreateParent(
                DepartmentName.Create("Head").Value,
                Identifier.Create("head").Value,
                [rootLoc],
                rootId).Value;
            context.Departments.Add(root);

            var childId = new DepartmentId(Guid.NewGuid());
            var childLoc = DepartmentLocation.Create(childId, locationId).Value;
            var child = Department.CreateChild(
                DepartmentName.Create("Development").Value,
                Identifier.Create("dev").Value,
                root,
                [childLoc],
                childId).Value;
            context.Departments.Add(child);

            var grandChildId = new DepartmentId(Guid.NewGuid());
            var grandChildLoc = DepartmentLocation.Create(grandChildId, locationId).Value;
            var grandChild = Department.CreateChild(
                DepartmentName.Create("Backend").Value,
                Identifier.Create("backend").Value,
                child,
                [grandChildLoc],
                grandChildId).Value;
            context.Departments.Add(grandChild);

            await context.SaveChangesAsync();

            return (root, child, grandChild);
        });
    }
}
