using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public sealed record Identifier
{    
    private static readonly Regex _identifierRegex = new("^[A-Za-z]+$", RegexOptions.Compiled);
    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }
    
    public static Result<Identifier, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < Constants.MIN_LENGTH_DEPARTMENT_IDENTIFIER ||
            value.Length > Constants.MAX_LENGTH_DEPARTMENT_IDENTIFIER ||
            !_identifierRegex.IsMatch(value))
        {
            return "Identifier is invalid";
        }
        return new Identifier(value);
    }
}