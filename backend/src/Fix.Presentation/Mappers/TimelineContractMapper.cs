using Fix.Application.Abstractions.Timeline;
using Fix.Contracts.V1;
using Google.Protobuf.WellKnownTypes;

namespace Fix.Presentation.Mappers;

internal static class TimelineContractMapper
{
    public static TimelineEntry ToContract(this TimelineEntryDto entry)
    {
        var contract = new TimelineEntry
        {
            Id = entry.Id.ToString(),
            EntityType = entry.EntityType,
            EntityId = entry.EntityId.ToString(),
            Action = entry.Action,
            ChangesJson = entry.Snapshot,
            OccurredAt = Timestamp.FromDateTimeOffset(entry.OccurredAt),
        };

        if (entry.AuthorId is { } authorId)
        {
            contract.AuthorId = authorId.ToString();
        }

        return contract;
    }
}
