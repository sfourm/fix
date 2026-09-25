using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

public sealed class Title : ValueObject
{
    public const int MaxLength = 200;

    private Title(string value) => Value = value;

    public string Value { get; }

    public static Title Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("O título é obrigatório.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"O título deve ter no máximo {MaxLength} caracteres.");
        }

        return new Title(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
