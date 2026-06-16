using CSharpFunctionalExtensions;
using DirectoryService.Application.DataBase;
using DirectoryService.Contract;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace DirectoryService.Application.DirectoryServiceManagement.Locations.GetTop;

public class GetTopLocationByCountDepartmentsHandler
{
    private readonly IReadDbContext _readDb;

    public GetTopLocationByCountDepartmentsHandler(IReadDbContext readDb)
    {
        _readDb = readDb;
    }

    public async Task<Result<TopLocationResponse, Error>> Handle(
        CancellationToken cancellationToken = default)
    {
        var topLocations = await (
            from location in _readDb.Locations
            let departmentCount = _readDb.DepartmentLocations
                .Count(dl => dl.LocationId == location.Id)
            orderby departmentCount descending
            select new LocationDto(
                location.Id.Value,
                location.Name.Value,
                location.Address.City,
                location.Address.Street,
                location.Address.HouseNumber,
                location.Address.ZipCode,
                departmentCount)
        ).Take(5).ToListAsync(cancellationToken);
        
        return new TopLocationResponse(topLocations);
    }
} 