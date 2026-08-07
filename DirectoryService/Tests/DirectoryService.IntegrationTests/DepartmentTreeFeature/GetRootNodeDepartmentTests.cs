using DirectoryService.Application.DirectoryServiceManagement.DepartmentTree.GetRootNodeDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;

namespace DirectoryService.IntegrationTests.DepartmentTreeFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class GetRootNodeDepartmentTests : DirectoryBaseTests<GetRootNodeDepartmentHandler>
{
    public GetRootNodeDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetRootNodeDepartment_on_empty_tree_should_return_empty_list()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act — при пустой базе
        var result = await ExecuteHandler(sut => sut.Handle(cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetRootNodeDepartment_with_root_nodes_should_return_root_departments()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var (root1, root2) = await CreateMultipleDepartmentRootsWithChildren();

        // act — запрос корневых отделов
        var result = await ExecuteHandler(sut => sut.Handle(cancellationToken));

        // assert — должны веруться только узлы верхнего уровня (nlevel(path) == 1)
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);

        var rootDep1 = result.Value.FirstOrDefault(x => x.Id == root1.Id.Value);
        Assert.NotNull(rootDep1);
        Assert.Equal("Headquarters", rootDep1.Name);
        Assert.True(rootDep1.HasChildren);
        Assert.Equal(1, rootDep1.ChildrenCount);

        var rootDep2 = result.Value.FirstOrDefault(x => x.Id == root2.Id.Value);
        Assert.NotNull(rootDep2);
        Assert.Equal("Branch", rootDep2.Name);
        Assert.False(rootDep2.HasChildren);
        Assert.Equal(0, rootDep2.ChildrenCount);
    }

    private async Task<(Department root1, Department root2)> CreateMultipleDepartmentRootsWithChildren()
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

            // First root department with a child
            var root1Id = new DepartmentId(Guid.NewGuid());
            var root1Loc = DepartmentLocation.Create(root1Id, locationId).Value;
            var root1 = Department.CreateParent(
                DepartmentName.Create("Headquarters").Value,
                Identifier.Create("headq").Value,
                [root1Loc],
                root1Id).Value;
            context.Departments.Add(root1);

            var child1Id = new DepartmentId(Guid.NewGuid());
            var child1Loc = DepartmentLocation.Create(child1Id, locationId).Value;
            var child1 = Department.CreateChild(
                DepartmentName.Create("IT Department").Value,
                Identifier.Create("itdept").Value,
                root1,
                [child1Loc],
                child1Id).Value;
            context.Departments.Add(child1);

            // Second root department without children
            var root2Id = new DepartmentId(Guid.NewGuid());
            var root2Loc = DepartmentLocation.Create(root2Id, locationId).Value;
            var root2 = Department.CreateParent(
                DepartmentName.Create("Branch").Value,
                Identifier.Create("branch").Value,
                [root2Loc],
                root2Id).Value;
            context.Departments.Add(root2);

            await context.SaveChangesAsync();

            return (root1, root2);
        });
    }
}
