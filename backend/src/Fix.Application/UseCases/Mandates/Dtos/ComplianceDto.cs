using Fix.Domain.Common;

namespace Fix.Application.Mandates.Dtos;

/// <summary>Enquadramento na política: dentro ou FORA, com o motivo.</summary>
public sealed record ComplianceDto(ComplianceStatus Status, string Reason);
