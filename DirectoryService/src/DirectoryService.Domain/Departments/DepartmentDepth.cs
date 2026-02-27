using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Departments;

public record DepartmentDepth
{
    private DepartmentDepth(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<DepartmentDepth, Error> Create(int value)
    {
        if (value < 0)
        {
            return GeneralErrors.ValueIsInvalid(nameof(DepartmentDepth));
        }

        return new DepartmentDepth(value);
    }
}