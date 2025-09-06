using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Departments;

public sealed record Identifier
{    
    private const int MIN_LENGTH_TEXT = 3;
    
    private const int MAX_LENGTH_TEXT = 150;
    
    private static readonly Regex _identifierRegex = new("^[A-Za-z]+$", RegexOptions.Compiled);
    public string Value { get; }

    private Identifier(string value)
    {
        Value = value;
    }
    
    public static Result<Identifier, string> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length < MIN_LENGTH_TEXT ||
            value.Length > MAX_LENGTH_TEXT ||
            !_identifierRegex.IsMatch(value))
        {
            return "Identifier is invalid";
        }
        return new Identifier(value);
    }
}