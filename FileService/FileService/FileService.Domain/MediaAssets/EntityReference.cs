using CSharpFunctionalExtensions;
using FileService.Domain.Shared;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record EntityReference
{
    private EntityReference(string type, Guid id)
    {
        Type = type;
        Id = id;
    }

    public string Type { get; }

    public Guid Id { get; }

    public static Result<EntityReference, Error> Create(string? type, Guid id)
    {
        if (string.IsNullOrWhiteSpace(type) || id == Guid.Empty)
        {
            return Error.Validation("file.entity_reference.invalid", "Нужны тип и непустой идентификатор сущности.");
        }

        type = type.Trim().ToLowerInvariant();
        if (type.Length > Constants.MAX_LENGTH_ENTITY_TYPE ||
            type[0] is < 'a' or > 'z' ||
            !type.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '_' or '-'))
        {
            return Error.Validation("file.entity_reference.invalid", "Тип сущности имеет неверный формат.");
        }

        return new EntityReference(type, id);
    }
}