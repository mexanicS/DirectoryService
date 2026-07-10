using DirectoryService.Application.DirectoryServiceManagement.Positions.Delete;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.PositionFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class DeletePositionTests : DirectoryBaseTests<DeletePositionHandler>
{
    public DeletePositionTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeletePosition_with_existing_id_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var positionId = await CreatePositionInDb();

        var command = new DeletePositionCommand(positionId.Value);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(positionId.Value, result.Value);

        await ExecuteContext(async context =>
        {
            var exists = await context.Positions.AnyAsync(p => p.Id == positionId, cancellationToken);
            Assert.False(exists);
        });
    }

    [Fact]
    public async Task DeletePosition_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var nonexistentId = Guid.NewGuid();

        var command = new DeletePositionCommand(nonexistentId);

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<PositionId> CreatePositionInDb()
    {
        return await ExecuteContext(async context =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create("ToDelete").Value,
                Description.Create("Description").Value).Value;
            context.Positions.Add(position);
            await context.SaveChangesAsync();
            return positionId;
        });
    }
}
