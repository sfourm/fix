using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

public sealed class Name : ValueObject
{
    public const int MaxLength = 150;

    private Name(string value) => Value = value;

    public string Value { get; }

    public static Name Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("O nome é obrigatório.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"O nome deve ter no máximo {MaxLength} caracteres.");
        }

        return new Name(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}
