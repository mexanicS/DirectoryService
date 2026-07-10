using DirectoryService.Application.DirectoryServiceManagement.Positions.Update;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.PositionFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class UpdatePositionTests : DirectoryBaseTests<UpdatePositionHandler>
{
    public UpdatePositionTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePosition_with_valid_data_should_succeed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var positionId = await CreatePositionInDb("Old Name", "Old description");

        var command = new UpdatePositionCommand(positionId.Value, "New Name", "New description");

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(positionId.Value, result.Value);

        await ExecuteContext(async context =>
        {
            var position = await context.Positions.FirstOrDefaultAsync(p => p.Id == positionId, cancellationToken);
            Assert.NotNull(position);
            Assert.Equal("New Name", position.Name.Value);
            Assert.Equal("New description", position.Description.Value);
        });
    }

    [Fact]
    public async Task UpdatePosition_with_nonexistent_id_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var nonexistentId = Guid.NewGuid();

        var command = new UpdatePositionCommand(nonexistentId, "New Name", "New description");

        // act
        var result = await ExecuteHandler(sut => sut.Handle(command, cancellationToken));

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<PositionId> CreatePositionInDb(string name, string description)
    {
        return await ExecuteContext(async context =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create(name).Value,
                Description.Create(description).Value).Value;
            context.Positions.Add(position);
            await context.SaveChangesAsync();
            return positionId;
        });
    }
}
