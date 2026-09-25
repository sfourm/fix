using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Counterparties.Commands;
using Fix.Application.Counterparties.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Entities = Fix.Domain.AggregateRoots.Counterparties;

namespace Fix.Presentation.Services;

internal sealed class CounterpartyGrpcService(
    IValidationFactory validation,
    ICounterpartyService counterpartyService) : CounterpartyService.CounterpartyServiceBase
{
    public override async Task<Counterparty> CreateCounterparty(CreateCounterpartyRequest request, ServerCallContext context)
    {
        var data = request.Data ?? new CounterpartyInput();
        return (await validation.RunAsync(
            new CreateCounterpartyCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                data.Name,
                data.Type.ToDomain<Entities.CounterpartyType>("data.type"),
                data.Document.ToOptionalString(data.HasDocument),
                data.Address.ToOptionalString(data.HasAddress),
                data.Country.ToOptionalString(data.HasCountry),
                data.NotionalLimitUsd.ToOptionalDecimal(data.HasNotionalLimitUsd),
                data.MtmLimitUsd.ToOptionalDecimal(data.HasMtmLimitUsd)),
            counterpartyService.CreateCounterpartyAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<Counterparty> UpdateCounterparty(UpdateCounterpartyRequest request, ServerCallContext context)
    {
        var data = request.Data ?? new CounterpartyInput();
        return (await validation.RunAsync(
            new UpdateCounterpartyCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                data.Name,
                data.Type.ToDomain<Entities.CounterpartyType>("data.type"),
                data.Document.ToOptionalString(data.HasDocument),
                data.Address.ToOptionalString(data.HasAddress),
                data.Country.ToOptionalString(data.HasCountry),
                data.NotionalLimitUsd.ToOptionalDecimal(data.HasNotionalLimitUsd),
                data.MtmLimitUsd.ToOptionalDecimal(data.HasMtmLimitUsd)),
            counterpartyService.UpdateCounterpartyAsync,
            context.CancellationToken)).ToContract();
    }

    public override async Task<Counterparty> SetCounterpartyHomologation(
        SetCounterpartyHomologationRequest request,
        ServerCallContext context) =>
        (await validation.RunAsync(
            new SetCounterpartyHomologationCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Homologated),
            counterpartyService.SetCounterpartyHomologationAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeleteCounterparty(DeleteCounterpartyRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new DeleteCounterpartyCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            counterpartyService.DeleteCounterpartyAsync,
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Counterparty> GetCounterparty(GetCounterpartyRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetCounterpartyQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            counterpartyService.GetCounterpartyAsync,
            context.CancellationToken)).ToContract();

    public override async Task<ListCounterpartiesResponse> ListCounterparties(
        ListCounterpartiesRequest request,
        ServerCallContext context)
    {
        var counterparties = await validation.RunAsync(
            new ListCounterpartiesQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.OnlyHomologated),
            counterpartyService.ListCounterpartiesAsync,
            context.CancellationToken);

        return new ListCounterpartiesResponse { Counterparties = { counterparties.Select(c => c.ToContract()) } };
    }
}

