namespace Fix.Storage.Domain.AggregateRoots.Files;

/// <summary>Tipos de arquivo. Usuários, políticas, mandatos e boletas são processados linha a linha no core (só criação); documentos são só armazenados.</summary>
public enum FileKind
{
    Users = 1,
    Policies = 2,
    Mandates = 3,
    Orders = 4,
    Documents = 5,
}

public static class FileKindExtensions
{
    public static bool IsProcessed(this FileKind kind) => kind is not FileKind.Documents;

    public static string Label(this FileKind kind) => kind switch
    {
        FileKind.Users => "usuários",
        FileKind.Policies => "políticas",
        FileKind.Mandates => "mandatos",
        FileKind.Orders => "boletas",
        _ => "documentos",
    };
}
