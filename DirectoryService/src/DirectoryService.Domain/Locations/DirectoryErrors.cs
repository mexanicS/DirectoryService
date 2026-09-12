using SharedKernel;

namespace DirectoryService.Domain.Locations;

public static class DirectoryErrors
{
    public static Error AlreadyExistByAddress() =>
        Error.Validation("record.already.exist", "Локация с таким адрессом уже существует");
}
