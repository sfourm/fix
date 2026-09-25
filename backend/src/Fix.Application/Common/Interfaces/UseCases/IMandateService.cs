using Fix.Application.Mandates.Commands;
using Fix.Application.Mandates.Dtos;
using Fix.Application.Mandates.Queries;
using Fix.Domain.Abstractions;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Mandatos: emissão com enquadramento na política, aprovação e saldo. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface IMandateService
{
    Task<MandateDto> IssueMandateAsync(IssueMandateCommand command, CancellationToken cancellationToken);

    Task<MandateDto> UpdateMandateAsync(UpdateMandateCommand command, CancellationToken cancellationToken);

    Task<MandateDto> ApproveMandateAsync(ApproveMandateCommand command, CancellationToken cancellationToken);

    Task<MandateDto> RejectMandateAsync(RejectMandateCommand command, CancellationToken cancellationToken);

    Task<MandateDto> CloseMandateAsync(CloseMandateCommand command, CancellationToken cancellationToken);

    Task DeleteMandateAsync(DeleteMandateCommand command, CancellationToken cancellationToken);

    Task<MandateDto> GetMandateAsync(GetMandateQuery query, CancellationToken cancellationToken);

    Task<PagedList<MandateDto>> ListMandatesAsync(ListMandatesQuery query, CancellationToken cancellationToken);

    Task<ComplianceDto> PreviewMandateComplianceAsync(PreviewMandateComplianceQuery query, CancellationToken cancellationToken);
}
