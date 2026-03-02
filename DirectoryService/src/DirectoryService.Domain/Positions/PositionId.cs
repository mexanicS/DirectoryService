namespace DirectoryService.Domain.Positions;

public record PositionId(Guid Value)
{
    public static implicit operator Guid(PositionId positionId)
    {
        return positionId.Value;
    }
}