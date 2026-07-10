using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.DTOs;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.IntegrationTests.DepartmentFeature;

[Trait("Category", "Integration")]
[Trait("Service", "DirectoryService")]
public class CreateDepartmentTests : DirectoryBaseTests<CreateDepartmentHandler>

{
    public CreateDepartmentTests(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateDepartment_with_Valid_data_should_succeed()
    {
        // arrange
        LocationId locationId = await CreateLocationId();
        
        var cancellationToken = CancellationToken.None;
        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentDto("SAB", "sab", null, [locationId.Value]);
            return sut.Handle(command, cancellationToken);
        });
        
        //assert

        await ExecuteContext(async context =>
        {
            var department = await context.Departments.FirstAsync(x => x.Id == new DepartmentId(result.Value) , cancellationToken);
            
            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);
        
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_empty_locations_should_fail()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentDto("SAB", "sab", null, []);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_identifier_should_fail()
    {
        // arrange
        LocationId locationId = await CreateLocationId();
        var cancellationToken = CancellationToken.None;

        // act — identifier содержит цифры, нарушает regex ^[A-Za-z]+$
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentDto("SAB", "sab123", null, [locationId.Value]);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_with_nonexistent_parent_should_fail()
    {
        // arrange
        LocationId locationId = await CreateLocationId();
        var nonExistentParentId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // act — передаём ParentId, которого нет в БД
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentDto("SAB", "sab", nonExistentParentId, [locationId.Value]);
            return sut.Handle(command, cancellationToken);
        });

        // assert
        Assert.True(result.IsFailure);
    }

    private async Task<LocationId> CreateLocationId()
    {
        return await ExecuteContext(async context =>
        {
            var location = new Location(new LocationId(Guid.NewGuid()),
                LocationName.Create("Tomsk").Value,
                Address.Create("Tomsk", "Istochnaya", "42", "634000").Value,
                Timezone.Create("normis").Value);

            context.Locations.Add(location);
            await context.SaveChangesAsync();
            LocationId locationId = location.Id;

            return locationId;
        });
    }


}