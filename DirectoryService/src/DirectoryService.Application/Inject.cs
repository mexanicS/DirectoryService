using DirectoryService.Application.DirectoryServiceManagement.Departments.AddPositionToDepartment;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Create;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Departments.GetById;
using DirectoryService.Application.DirectoryServiceManagement.Departments.LinkDepartmentAndLocation;
using DirectoryService.Application.DirectoryServiceManagement.Departments.RemovePositionFromDepartment;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UnLinkDepartmentAndLocationHandler;
using DirectoryService.Application.DirectoryServiceManagement.Departments.Update;
using DirectoryService.Application.DirectoryServiceManagement.Departments.UpdateLocations;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Create;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Locations.GetById;
using DirectoryService.Application.DirectoryServiceManagement.Locations.Update;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Create;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Delete;
using DirectoryService.Application.DirectoryServiceManagement.Positions.Update;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class Inject
{
    public static IServiceCollection AddSpeciesApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateLocationHandler>();
        services.AddScoped<UpdateLocationHandler>();
        services.AddScoped<DeleteLocationHandler>();
        
        services.AddScoped<LinkDepartmentAndLocationHandler>();
        
        services.AddScoped<UnLinkDepartmentAndLocationHandler>();
        
        services.AddScoped<CreateDepartmentHandler>();
        services.AddScoped<UpdateDepartmentHandler>();
        services.AddScoped<DeleteDepartmentHandler>();
        
        services.AddScoped<CreatePositionHandler>();
        services.AddScoped<UpdatePositionHandler>();
        services.AddScoped<DeletePositionHandler>();
        
        services.AddScoped<UpdateLocationsByDepartmentHandler>();
        
        services.AddScoped<GetLocationByIdHandler>();
        services.AddScoped<GetDepartmentByIdHandler>();
        
        services.AddScoped<AddPositionToDepartmentHandler>();
        services.AddScoped<RemovePositionFromDepartmentHandler>();
        
        services.AddValidatorsFromAssembly(typeof(Inject).Assembly);
        
        return services;
    }
}