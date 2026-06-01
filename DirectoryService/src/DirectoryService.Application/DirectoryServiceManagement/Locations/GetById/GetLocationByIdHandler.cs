using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Contract;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.GetById;

public class GetLocationByIdHandler
{
    private readonly IReadDbContext _readDb;

    public GetLocationByIdHandler(IReadDbContext readDb)
    {
        _readDb = readDb;
    }

    public async Task<Result<LocationResponse, Error>> Handle(
        GetLocationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var response = await _readDb.Locations
            .Where(l => l.Id == query.Id && l.IsActive)
            .Select(l => new LocationResponse(
                l.Id.Value,
                l.Name.Value,
                l.Address.City,
                l.Address.Street,
                l.Address.HouseNumber,
                l.Address.ZipCode,
                l.Timezone.Value,
                l.IsActive,
                l.CreatedAt,
                l.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (response is null)
            return GeneralErrors.NotFound(query.Id, nameof(Location));

        return response;
    }
}
