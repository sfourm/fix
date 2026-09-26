using System.Text;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Abstractions.Processing;
using Fix.Storage.Application.Abstractions.Storage;
using Fix.Storage.Application.Files.Commands;
using Fix.Storage.Application.Files.Events;
using Fix.Storage.Application.Files.Services;
using Fix.Storage.Domain.Abstractions;
using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Repositories;
using Fix.Storage.Infrastructure.Spreadsheets;
using Microsoft.Extensions.Logging.Abstractions;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;

namespace Fix.Storage.Tests;

public class ProcessingTests
{
    private readonly FakeFiles files = new();
    private readonly FakeLines lines = new();
    private readonly FakeStorage storage = new();
    private readonly FakeBus bus = new();
    private readonly FakeExecutor executor = new();

    private FileProcessingService Processing() =>
        new(files, lines, storage, new TableReader(), executor, bus, TimeProvider.System, NullLogger<FileProcessingService>.Instance);

    private async Task<File> UploadAsync(string csv)
    {
        storage.Objects["k"] = Encoding.UTF8.GetBytes(csv);
        var file = File.Receive(Guid.NewGuid(), Guid.NewGuid(), FileKind.Users, "membros.csv", "text/csv", 10, "k", "s3://b/k", DateTimeOffset.UtcNow);
        await files.AddAsync(file, default);
        return file;
    }

    private static ProcessFileLineCommand Command(FileLineReceived message) => new(message.FileId, message.LineId);

    [Fact]
    public async Task Each_line_is_published_separately_and_a_failing_line_does_not_stop_the_others()
    {
        var file = await UploadAsync("email;cargo\nana@x.com;\nquebra@x.com;\n\nbia@x.com;\n");
        var processing = Processing();

        await processing.IngestFileAsync(new IngestFileCommand(file.Id), default);

        var published = bus.Lines.ToList();
        Assert.Equal([1, 2, 3], published.Select(l => l.Number));
        Assert.Equal(FileStatus.Processing, files.Items[file.Id].Status);
        Assert.Equal(3, files.Items[file.Id].TotalLines);

        foreach (var message in published)
        {
            await processing.ProcessFileLineAsync(Command(message), default);
        }

        var result = files.Items[file.Id];
        var failed = lines.Items.Values.Single(l => l.Number == 2);
        Assert.Equal(FileStatus.CompletedWithErrors, result.Status);
        Assert.Equal((3, 2, 1), (result.ProcessedLines, result.SucceededLines, result.FailedLines));
        Assert.Equal(FileLineStatus.Failed, failed.Status);
        Assert.Equal("Conta não encontrada", failed.Message);
        Assert.Equal(100, bus.Progress.Last().Percent);
    }

    [Fact]
    public async Task Redelivered_line_is_not_executed_twice()
    {
        var file = await UploadAsync("email\nana@x.com\n");
        var processing = Processing();
        await processing.IngestFileAsync(new IngestFileCommand(file.Id), default);

        var message = bus.Lines.Single();
        await processing.ProcessFileLineAsync(Command(message), default);
        await processing.ProcessFileLineAsync(Command(message), default);

        Assert.Equal(1, executor.Calls);
        Assert.Equal(1, files.Items[file.Id].ProcessedLines);
    }

    [Fact]
    public async Task Core_unavailable_retries_then_fails_the_line()
    {
        var file = await UploadAsync("email\nana@x.com\n");
        executor.Unavailable = true;
        var processing = Processing();
        await processing.IngestFileAsync(new IngestFileCommand(file.Id), default);
        var message = bus.Lines.Single();

        for (var attempt = 1; attempt < FileProcessingService.MaxAttempts; attempt++)
        {
            await Assert.ThrowsAsync<TransientLineException>(() => processing.ProcessFileLineAsync(Command(message), default));
        }

        await processing.ProcessFileLineAsync(Command(message), default);

        Assert.Equal(FileLineStatus.Failed, lines.Items[message.LineId].Status);
        Assert.Equal(FileStatus.CompletedWithErrors, files.Items[file.Id].Status);
    }

    [Fact]
    public async Task Unreadable_file_fails_without_lines()
    {
        var file = await UploadAsync("telefone\n123\n");
        await Processing().IngestFileAsync(new IngestFileCommand(file.Id), default);

        Assert.Equal(FileStatus.Failed, files.Items[file.Id].Status);
        Assert.Contains("email", files.Items[file.Id].Error);
        Assert.Empty(bus.Lines);
    }

    [Fact]
    public async Task Stored_kinds_are_not_read()
    {
        var file = File.Receive(Guid.NewGuid(), Guid.NewGuid(), FileKind.Documents, "contrato.pdf", "application/pdf", 10, "k", "s3://b/k", DateTimeOffset.UtcNow);
        await files.AddAsync(file, default);

        await Processing().IngestFileAsync(new IngestFileCommand(file.Id), default);

        Assert.Equal(FileStatus.Stored, files.Items[file.Id].Status);
        Assert.Empty(bus.Lines);
    }

    /// <summary>Em memória, com as mesmas regras de <see cref="File"/> que o repositório Mongo aplica de forma atômica.</summary>
    private sealed class FakeFiles : IFileRepository
    {
        public Dictionary<Guid, File> Items { get; } = [];

        public Task<File?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.GetValueOrDefault(id));

        public Task<PagedList<File>> ListAsync(Guid organizationId, FileKind? kind, int page, int pageSize, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<FileKindSummary>> SummaryAsync(Guid organizationId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task AddAsync(File file, CancellationToken cancellationToken)
        {
            Items[file.Id] = file;
            return Task.CompletedTask;
        }

        public Task<bool> StartProcessingAsync(Guid id, int totalLines, CancellationToken cancellationToken) =>
            Task.FromResult(Items[id].StartProcessing(totalLines));

        public Task FailAsync(Guid id, string error, DateTimeOffset at, CancellationToken cancellationToken)
        {
            Items[id].MarkFailed(error, at);
            return Task.CompletedTask;
        }

        public Task<File> RegisterLineResultAsync(Guid id, bool succeeded, DateTimeOffset at, CancellationToken cancellationToken)
        {
            Items[id].RegisterLineResult(succeeded, at);
            return Task.FromResult(Items[id]);
        }
    }

    private sealed class FakeLines : IFileLineRepository
    {
        public Dictionary<Guid, FileLine> Items { get; } = [];

        public Task<FileLine?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Items.GetValueOrDefault(id));

        public Task<PagedList<FileLine>> ListAsync(Guid fileId, FileLineStatus? status, int page, int pageSize, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<FileLine>> AddManyAsync(IReadOnlyList<FileLine> lines, CancellationToken cancellationToken)
        {
            foreach (var line in lines) Items.TryAdd(line.Id, line);
            return Task.FromResult(lines);
        }

        public Task UpdateAsync(FileLine line, CancellationToken cancellationToken)
        {
            Items[line.Id] = line;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeStorage : IObjectStorage
    {
        public Dictionary<string, byte[]> Objects { get; } = [];

        public Task<string> PutAsync(string key, byte[] content, string contentType, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<byte[]> GetAsync(string key, CancellationToken cancellationToken) => Task.FromResult(Objects[key]);

        public (string Url, DateTimeOffset ExpiresAt) GetDownloadUrl(string key, string fileName, TimeSpan validity) =>
            throw new NotSupportedException();
    }

    private sealed class FakeBus : IEventBus
    {
        public List<FileLineReceived> Lines { get; } = [];

        public List<FileProgress> Progress { get; } = [];

        public Task PublishAsync(FileUpload message, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task PublishAsync(FileLineReceived message, CancellationToken cancellationToken)
        {
            Lines.Add(message);
            return Task.CompletedTask;
        }

        public Task PublishAsync(FileProgress message, CancellationToken cancellationToken)
        {
            Progress.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeExecutor : ILineExecutor
    {
        public bool Unavailable { get; set; }

        public int Calls { get; private set; }

        public Task<LineResult> ExecuteAsync(File file, FileLine line, CancellationToken cancellationToken)
        {
            Calls++;
            if (Unavailable) throw new TransientLineException("core fora do ar");
            return Task.FromResult(line.Values["email"].StartsWith("quebra", StringComparison.Ordinal)
                ? LineResult.Fail("Conta não encontrada")
                : LineResult.Ok("ok", line.Values["email"]));
        }
    }
}
