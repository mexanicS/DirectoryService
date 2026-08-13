using DirectoryService.Application.DirectoryServiceManagement.Departments.Move;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class MoveDepartmentTests : DirectoryBaseTests<MoveDepartmentHandler>
{
    public MoveDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Move_should_update_node_and_whole_subtree()
    {
        var tree = await CreateTree();

        var result = await Move(tree.Team.Id.Value, tree.Hq.Id.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(tree.Hq.Id.Value, result.Value.ParentId);
        Assert.Equal("hqroot.team", result.Value.Path);
        Assert.Equal(1, result.Value.Depth);

        await ExecuteContext(async context =>
        {
            var team = await context.Departments.AsNoTracking().SingleAsync(d => d.Id == tree.Team.Id);
            var engineer = await context.Departments.AsNoTracking().SingleAsync(d => d.Id == tree.Engineer.Id);

            Assert.Equal(tree.Hq.Id, team.ParentId);
            Assert.Equal("hqroot.team", team.Path.Value);
            Assert.Equal(1, team.Depth.Value);
            Assert.Equal("hqroot.team.engineer", engineer.Path.Value);
            Assert.Equal(2, engineer.Depth.Value);
        });
    }

    [Fact]
    public async Task Move_to_root_should_update_node_and_descendants()
    {
        var tree = await CreateTree();

        var result = await Move(tree.Team.Id.Value, null);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.ParentId);
        Assert.Equal("team", result.Value.Path);
        Assert.Equal(0, result.Value.Depth);

        await ExecuteContext(async context =>
        {
            var engineer = await context.Departments.AsNoTracking().SingleAsync(d => d.Id == tree.Engineer.Id);
            Assert.Equal("team.engineer", engineer.Path.Value);
            Assert.Equal(1, engineer.Depth.Value);
        });
    }

    [Fact]
    public async Task Move_under_descendant_should_return_cycle_error()
    {
        var tree = await CreateTree();

        var result = await Move(tree.It.Id.Value, tree.Engineer.Id.Value);

        AssertError(result, "department.move.cycle");
    }

    [Fact]
    public async Task Move_under_self_should_return_validation_error()
    {
        var tree = await CreateTree();

        var result = await Move(tree.Team.Id.Value, tree.Team.Id.Value);

        AssertError(result, "department.move.parent_is_self");
    }

    [Fact]
    public async Task Move_under_soft_deleted_parent_should_return_conflict()
    {
        var tree = await CreateTree();
        await ExecuteContext(async context =>
        {
            var parent = await context.Departments.SingleAsync(d => d.Id == tree.OtherRoot.Id);
            parent.SoftDelete();
            await context.SaveChangesAsync();
        });

        var result = await Move(tree.Team.Id.Value, tree.OtherRoot.Id.Value);

        AssertError(result, "department.move.parent_deleted");
    }

    [Fact]
    public async Task Move_with_missing_parent_should_return_not_found()
    {
        var tree = await CreateTree();

        var result = await Move(tree.Team.Id.Value, Guid.NewGuid());

        AssertError(result, "department.parent.not_found");
    }

    [Fact]
    public async Task Move_with_missing_department_should_return_not_found()
    {
        var result = await Move(Guid.NewGuid(), null);

        AssertError(result, "department.not_found");
    }

    [Fact]
    public async Task Move_to_current_parent_should_be_no_op_and_keep_updated_at()
    {
        var tree = await CreateTree();
        var before = await ExecuteContext(context => context.Departments
            .AsNoTracking()
            .Where(d => d.Id == tree.Team.Id)
            .Select(d => d.UpdatedAt)
            .SingleAsync());

        var result = await Move(tree.Team.Id.Value, tree.It.Id.Value);

        Assert.True(result.IsSuccess);
        Assert.Equal(before, result.Value.UpdatedAt);

        var after = await ExecuteContext(context => context.Departments
            .AsNoTracking()
            .Where(d => d.Id == tree.Team.Id)
            .Select(d => d.UpdatedAt)
            .SingleAsync());
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task Move_large_subtree_should_update_every_node()
    {
        var tree = await CreateTree(extraDescendants: 250);

        var result = await Move(tree.Team.Id.Value, tree.Hq.Id.Value);

        Assert.True(result.IsSuccess);
        await ExecuteContext(async context =>
        {
            var paths = await context.Departments
                .AsNoTracking()
                .Select(d => d.Path)
                .ToListAsync();
            var movedCount = paths.Count(path => path.Value.StartsWith("hqroot.team", StringComparison.Ordinal));
            Assert.Equal(252, movedCount);
        });
    }

    private Task<CSharpFunctionalExtensions.Result<MoveDepartmentResponse, SharedKernel.Errors>> Move(
        Guid departmentId,
        Guid? parentId) =>
        ExecuteHandler(handler => handler.Handle(
            new MoveDepartmentCommand(departmentId, parentId),
            CancellationToken.None));

    private static void AssertError(
        CSharpFunctionalExtensions.Result<MoveDepartmentResponse, SharedKernel.Errors> result,
        string code)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(code, result.Error.Single().Code);
    }

    private async Task<Tree> CreateTree(int extraDescendants = 0)
    {
        return await ExecuteContext(async context =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            context.Locations.Add(new Location(
                locationId,
                LocationName.Create("Tomsk").Value,
                Address.Create("Tomsk", "Lenina", "1", "634000").Value,
                Timezone.Create("Asia/Tomsk").Value));

            Department Root(string name, string identifier)
            {
                var id = new DepartmentId(Guid.NewGuid());
                return Department.CreateParent(
                    DepartmentName.Create(name).Value,
                    Identifier.Create(identifier).Value,
                    [DepartmentLocation.Create(id, locationId).Value],
                    id).Value;
            }

            Department Child(Department parent, string name, string identifier)
            {
                var id = new DepartmentId(Guid.NewGuid());
                return Department.CreateChild(
                    DepartmentName.Create(name).Value,
                    Identifier.Create(identifier).Value,
                    parent,
                    [DepartmentLocation.Create(id, locationId).Value],
                    id).Value;
            }

            var hq = Root("Headquarters", "hqroot");
            var otherRoot = Root("Branch", "branch");
            var it = Child(hq, "IT Department", "itdept");
            var team = Child(it, "Team", "team");
            var engineer = Child(team, "Engineer", "engineer");
            context.Departments.AddRange(hq, otherRoot, it, team, engineer);

            for (var i = 0; i < extraDescendants; i++)
            {
                context.Departments.Add(Child(team, $"Worker {i}", $"worker{ToLetters(i)}"));
            }

            await context.SaveChangesAsync();
            return new Tree(hq, otherRoot, it, team, engineer);
        });
    }

    private sealed record Tree(
        Department Hq,
        Department OtherRoot,
        Department It,
        Department Team,
        Department Engineer);

    private static string ToLetters(int value)
    {
        var result = string.Empty;
        do
        {
            result = (char)('a' + value % 26) + result;
            value = value / 26 - 1;
        } while (value >= 0);

        return result;
    }
}
