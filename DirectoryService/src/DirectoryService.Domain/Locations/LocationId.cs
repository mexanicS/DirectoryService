namespace DirectoryService.Domain.Locations;

public record LocationId(Guid Value)
{
    public static implicit operator Guid(LocationId volunteerId)
    {
        return volunteerId.Value;
    }
    
    public static LocationId Create(Guid id) => new LocationId(id);
}