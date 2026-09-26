namespace Fix.Storage.Application.Files.Dtos;

public sealed record FileDownloadUrlDto(string Url, DateTimeOffset ExpiresAt);
