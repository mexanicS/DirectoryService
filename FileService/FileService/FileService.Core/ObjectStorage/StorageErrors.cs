using FileService.Domain.StorageReferences;
using SharedKernel;

namespace FileService.Core.ObjectStorage;

public static class StorageErrors
{
    public static Error Unavailable() =>
        Error.Failure("storage.unavailable", "Хранилище файлов недоступно.");

    public static Error ObjectNotFound(StorageKey key) =>
        Error.NotFound("storage.object.not_found", $"Объект {key.Value} не найден в хранилище.");

    public static Error AccessDenied() =>
        Error.Failure("storage.access_denied", "Отказано в доступе к хранилищу файлов.");
}