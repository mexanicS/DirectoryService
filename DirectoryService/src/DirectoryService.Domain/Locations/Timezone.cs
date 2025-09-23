using System.Runtime.InteropServices.JavaScript;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.Locations;

public record Timezone
{
    public string Value { get; }

    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsInvalid(nameof(Timezone));
        }

        return new Timezone(value);
    }
}