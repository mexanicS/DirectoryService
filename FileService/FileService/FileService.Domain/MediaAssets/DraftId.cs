using CSharpFunctionalExtensions;
using SharedKernel;

namespace FileService.Domain.MediaAssets;

public record DraftId
{
    private DraftId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Result<DraftId, Error> Create(Guid value)
    {
        if (value == Guid.Empty)
        {
            return Error.Validation("file.draft_id.empty", "Идентификатор черновика не может быть пустым.");
        }

        return new DraftId(value);
    }

    public static implicit operator Guid(DraftId id) => id.Value;
}