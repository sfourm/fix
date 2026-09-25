using Fix.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Fix.Infrastructure.Persistence.Configurations;

internal static class ValueObjectConversions
{
    public static readonly ValueConverter<Name, string> Name = new(v => v.Value, v => Domain.Common.Name.Create(v));

    public static readonly ValueConverter<Title, string> Title = new(v => v.Value, v => Domain.Common.Title.Create(v));

    public static readonly ValueConverter<Slug, string> Slug = new(v => v.Value, v => Domain.Common.Slug.FromDatabase(v));

    public static readonly ValueConverter<Tenor, string> Tenor = new(v => v.Code, v => Domain.Common.Tenor.Create(v));

    // Para colunas anuláveis: o EF não chama o conversor com null, mas o tipo precisa aceitar a anotação.
    public static readonly ValueConverter<Tenor?, string> OptionalTenor = new(v => v!.Code, v => Domain.Common.Tenor.Create(v));

    public static readonly ValueConverter<CropYear, string> CropYear = new(v => v.Value, v => Domain.Common.CropYear.Create(v));
}
