namespace Fix.Domain.AggregateRoots.Orders;

/// <summary>Confirmation: confirmação escrita da contraparte, que precisa bater com a boleta.</summary>
public enum ConfirmationStatus
{
    Pending = 1,

    /// <summary>Conforme.</summary>
    Confirmed = 2,

    /// <summary>Divergente: em reconciliação.</summary>
    Divergent = 3,

    /// <summary>Recusado: irregular.</summary>
    Refused = 4,
}