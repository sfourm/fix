using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Dtos;

public sealed record FileTemplateColumnDto(string Name, bool Required, string Description, string Example);

public sealed record FileTemplateDto(
    FileKind Kind,
    // Falso = só armazena.
    bool Processed,
    IReadOnlyList<string> AcceptedExtensions,
    IReadOnlyList<FileTemplateColumnDto> Columns,
    // CSV de exemplo em UTF-8 com BOM (vazio nos tipos só armazenados).
    byte[] ExampleCsv);
