namespace Fix.Domain.AggregateRoots.Orders;

public enum ApprovalStatus
{
    /// <summary>Emitida sem alçada: aguarda aprovação e ainda não consome o mandato.</summary>
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
}