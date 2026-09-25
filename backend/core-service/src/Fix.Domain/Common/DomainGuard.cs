using Fix.Domain.Abstractions;

namespace Fix.Domain.Common;

public static class DomainGuard
{
    public const int DescriptionMaxLength = 2000;

    public static string? OptionalText(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"O campo '{field}' deve ter no máximo {maxLength} caracteres.");
        }

        return trimmed;
    }

    public static string? Description(string? value) => OptionalText(value, DescriptionMaxLength, "descrição");
}
