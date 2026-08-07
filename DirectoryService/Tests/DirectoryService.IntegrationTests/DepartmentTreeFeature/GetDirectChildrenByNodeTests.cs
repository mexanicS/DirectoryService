using DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetDirectChildrenByNode;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.DepartmentTreeFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetDirectChildrenByNodeTests : DirectoryBaseTests<GetDirectChildrenByNodeHandler>
{
    public GetDirectChildrenByNodeTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDirectChildrenByNode_on_empty_tree_should_return_empty_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act — при пустой базе
        var result = await ExecuteHandler(sut => sut.Handle("head", cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetDirectChildrenByNode_on_leaf_node_without_children_should_return_empty_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (_, _, grandChild) = await CreateDepartmentHierarchy();

        // act — вызываем для листового узла без детей (backend)
        var result = await ExecuteHandler(sut => sut.Handle(grandChild.Path.Value, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetDirectChildrenByNode_on_nonexistent_node_should_return_empty_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        await CreateDepartmentHierarchy();

        // act — передаём путь несуществующего узла
        var result = await ExecuteHandler(sut => sut.Handle("nonexistent.path", cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetDirectChildrenByNode_on_parent_node_should_return_direct_children()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (root, child, grandChild) = await CreateDepartmentHierarchy();

        // act — вызываем для корневого узла (head)
        var result = await ExecuteHandler(sut => sut.Handle(root.Path.Value, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        
        var directChild = result.Value[0];
        Assert.Equal(child.Id.Value, directChild.Id);
        Assert.Equal("Development", directChild.Name);
        Assert.True(directChild.HasChildren);
        Assert.Equal(1, directChild.ChildrenCount);
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
