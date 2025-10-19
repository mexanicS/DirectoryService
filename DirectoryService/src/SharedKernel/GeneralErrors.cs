namespace SharedKernel;

public static class GeneralErrors
{
    public static Error ValueIsInvalid(string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.invalid", $"{label} не корректно");
    }
    
    public static Error ValueIsMustBeLess(int length, string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.must.be.less",
            $"{label} должно быть меньше {length} символов.");
    }
    
    public static Error ValueIsMustBeBetween(int lengthMin, int lengthMax, string? name = null)
    {
        string label = name ?? "значение";
        return Error.Validation("value.is.must.be.less",
            $"{label} должно быть от {lengthMin} до {lengthMax} символов.");
    }

    public static Error NotFound(Guid? id = null, string? name = null)
    {
        string forId = id == null ? string.Empty : $" по Id '{id}'";
        return Error.NotFound("record.not.found", $"{name ?? "запись"} не найдена{forId}");
    }

    public static Error ValueIsRequired(string? name = null)
    {
        string label = name == null ? string.Empty : " " + name + " ";
        return Error.Validation("length.is.invalid", $"Поле{label}обязательно");
    }

    public static Error AlreadyExist()
    {
        return Error.Validation("record.already.exist", "Запись уже существует");
    }

    public static Error Failure(string? message = null)
    {
        return Error.Failure("server.failure", message ?? "Серверная ошибка");
    }
}