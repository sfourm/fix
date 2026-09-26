namespace Fix.Storage.Application.Files.Dtos;

public sealed record FileContentDto(string FileName, string ContentType, byte[] Content);
