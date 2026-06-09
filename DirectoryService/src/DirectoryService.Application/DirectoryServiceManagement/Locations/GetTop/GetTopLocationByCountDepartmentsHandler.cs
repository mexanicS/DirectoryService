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

    public async Task<Result<IReadOnlyList<TopLocationResponse>, Error>> Handle(
        CancellationToken cancellationToken = default)
    {
        var topLocations = await (
            from location in _readDb.Locations
            let departmentCount = _readDb.DepartmentLocations
                .Count(dl => dl.LocationId == location.Id)
            orderby departmentCount descending
            select new TopLocationResponse(
                location.Id.Value,
                location.Name.Value,
                location.Address.City,
                location.Address.Street,
                location.Address.HouseNumber,
                location.Address.ZipCode,
                departmentCount)
        ).Take(5).ToListAsync(cancellationToken);
        
        return topLocations;
    }
} 