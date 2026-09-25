using System.Globalization;
using System.Text;
using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

public sealed class Slug : ValueObject
{
    public const int MaxLength = 160;

    private Slug(string value) => Value = value;

    public string Value { get; }

    public static Slug From(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var character in text.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length == 0)
        {
            throw new DomainException("Não foi possível gerar um slug válido a partir do nome.");
        }

        return new Slug(slug.Length > MaxLength ? slug[..MaxLength].TrimEnd('-') : slug);
    }

    public static Slug FromDatabase(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
