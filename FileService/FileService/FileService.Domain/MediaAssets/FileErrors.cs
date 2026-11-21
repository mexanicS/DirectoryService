using SharedKernel;

namespace FileService.Domain.MediaAssets;

public static class FileErrors
{
    public static Error InvalidTransition(AssetStatus status, string operation) =>
        Error.Validation("file.invalid_transition", $"Операция {operation} недоступна в статусе {status}.");

    public static Error InvalidTimestamp() =>
        Error.Validation("file.timestamp.invalid", "Ожидается время UTC не раньше последнего изменения.");

    public static Error InvalidBinding() =>
        Error.Validation("file.binding.invalid", "Нужно указать либо черновик, либо целевую сущность.");

    public static Error AlreadyAttached() =>
        Error.Validation("file.already_attached", "Файл уже прикреплён к другой сущности.");

    public static Error Required(string field) =>
        GeneralErrors.ValueIsRequired(field);

    public static Error InvalidKind() =>
        Error.Validation("file.kind.invalid", "Неизвестная категория файла.");

    public static Error InvalidUsage() =>
        Error.Validation("file.usage.invalid", "Неизвестное назначение файла.");

    public static Error KindMismatch() =>
        Error.Validation("file.kind_mismatch", "Категория файла не соответствует назначению.");

    public static Error MimeNotAllowed() =>
        Error.Validation("file.mime_not_allowed", "MIME-тип не разрешён для этого назначения.");

    public static Error SizeExceeded(long maximum) =>
        Error.Validation("file.size_exceeded", $"Размер файла не должен превышать {maximum} байт.");

    public static Error StorageKeyMismatch() =>
        Error.Validation("file.storage_key_mismatch", "Ключ не совпадает с зарезервированным ключом.");

    public static Error SizeMismatch() =>
        Error.Validation("file.size_mismatch", "Фактический размер не совпадает с заявленным.");

    public static Error MimeMismatch() =>
        Error.Validation("file.mime_mismatch", "Определённый MIME-тип не совпадает с заявленным.");

    public static Error StorageNotConfirmed() =>
        Error.Validation("file.storage_not_confirmed", "Объект хранения ещё не подтверждён.");
}