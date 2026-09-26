using Fix.Contracts.V1;
using Fix.Storage.Application.Abstractions.Processing;
using Fix.Storage.Domain.AggregateRoots.Files;
using Fix.Storage.Domain.AggregateRoots.Files.Templates;
using Fix.Storage.Infrastructure.Options;
using Grpc.Core;
using Microsoft.Extensions.Options;
using File = Fix.Storage.Domain.AggregateRoots.Files.File;
using FileKind = Fix.Storage.Domain.AggregateRoots.Files.FileKind;
using FileLine = Fix.Storage.Domain.AggregateRoots.Files.FileLine;

namespace Fix.Storage.Infrastructure.CoreService;

/// <summary>
/// Monta o request do proto a partir da linha e executa no core-service em nome de quem enviou o arquivo — as regras,
/// alçadas e validações do core valem como numa tela. Erro de negócio ou de preenchimento vira falha da linha; core
/// fora do ar vira <see cref="TransientLineException"/> (a linha volta para a fila).
/// </summary>
internal sealed class CoreLineExecutor(
    OrganizationService.OrganizationServiceClient organizations,
    PolicyService.PolicyServiceClient policies,
    MandateService.MandateServiceClient mandates,
    OrderService.OrderServiceClient orders,
    CoreLookups lookups,
    IOptions<CoreOptions> options) : ILineExecutor
{
    public async Task<LineResult> ExecuteAsync(File file, FileLine line, CancellationToken cancellationToken)
    {
        var context = new RequestContext { UserId = file.UploadedBy.ToString(), OrganizationId = file.OrganizationId.ToString() };
        var values = new LineValues(line.Values);
        var call = new CallOptions(
            deadline: DateTime.UtcNow.AddSeconds(options.Value.TimeoutSeconds),
            cancellationToken: cancellationToken);
        try
        {
            return file.Kind switch
            {
                FileKind.Users => await AddMemberAsync(context, values, call, cancellationToken),
                FileKind.Policies => await CreatePolicyAsync(context, values, call),
                FileKind.Mandates => await IssueMandateAsync(context, values, call, cancellationToken),
                FileKind.Orders => await RegisterOrderAsync(context, values, call, cancellationToken),
                _ => LineResult.Fail($"Arquivos de {file.Kind.Label()} não são processados."),
            };
        }
        catch (LineValueException exception)
        {
            return LineResult.Fail(exception.Message);
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded or StatusCode.ResourceExhausted)
        {
            throw new TransientLineException($"core-service indisponível ({exception.StatusCode})", exception);
        }
        catch (RpcException exception)
        {
            return LineResult.Fail(string.IsNullOrWhiteSpace(exception.Status.Detail) ? exception.StatusCode.ToString() : exception.Status.Detail);
        }
    }

    /// <summary>Usuários: adiciona à organização uma conta já cadastrada, com cargo, mesa e grupo opcionais.</summary>
    private async Task<LineResult> AddMemberAsync(RequestContext context, LineValues values, CallOptions call, CancellationToken cancellationToken)
    {
        var email = values.Required("email");
        var rule = values.Text("cargo") is { } cargo ? await lookups.RuleAsync(context, cargo, cancellationToken) : null;
        var desk = values.Option("mesa", LineValues.Desks) ?? Desk.Unspecified;
        // O grupo é conferido antes: grupo inexistente não deixa um membro criado pela metade.
        var group = values.Text("grupo") is { } groupName ? await lookups.GroupAsync(context, groupName, cancellationToken) : null;

        var member = await organizations.AddMemberAsync(
            new AddMemberRequest { Context = context, Email = email, RuleCode = rule?.Code ?? string.Empty, Desk = desk },
            call);

        if (group is null)
        {
            return LineResult.Ok($"{email} adicionado{(rule is null ? string.Empty : $" como {rule.Name}")}.", email);
        }

        try
        {
            await organizations.AddGroupMemberAsync(
                new AddGroupMemberRequest { Context = context, GroupId = group.Id, MemberId = member.MemberId },
                call);
        }
        catch (RpcException exception) when (exception.StatusCode is not (StatusCode.Unavailable or StatusCode.DeadlineExceeded))
        {
            return LineResult.Fail($"{email} foi adicionado, mas não entrou no grupo {group.Name}: {exception.Status.Detail}");
        }

        return LineResult.Ok($"{email} adicionado ao grupo {group.Name}{(rule is null ? string.Empty : $" como {rule.Name}")}.", email);
    }

    /// <summary>Políticas: só criação — cada linha cria uma política nova em rascunho (alterações seguem pela tela).</summary>
    private async Task<LineResult> CreatePolicyAsync(RequestContext context, LineValues values, CallOptions call)
    {
        var request = new CreatePolicyRequest
        {
            Context = context,
            Code = values.Required("codigo"),
            Title = values.Required("titulo"),
            Version = values.Required("versao"),
            ValidFrom = values.RequiredDate("vigencia_inicio"),
            UseTemplate = values.Flag("usar_modelo"),
        };
        if (values.Text("descricao") is { } description) request.Description = description;
        if (values.Date("vigencia_fim") is { } validTo) request.ValidTo = validTo;

        var policy = await policies.CreatePolicyAsync(request, call);
        return LineResult.Ok($"Política {policy.Code} {policy.Version} criada em rascunho{(request.UseTemplate ? " com o modelo FIX" : string.Empty)}.", policy.Code);
    }

    /// <summary>Mandatos: emite o mandato no eixo da política (entra pendente de aprovação, como pela tela).</summary>
    private async Task<LineResult> IssueMandateAsync(RequestContext context, LineValues values, CallOptions call, CancellationToken cancellationToken)
    {
        var policy = await lookups.PolicyAsync(context, values.Required("politica"), cancellationToken);
        var axisCode = values.Required("eixo");
        var axis = policy.Axes.FirstOrDefault(a => FileTemplates.Normalize(a.Code) == FileTemplates.Normalize(axisCode))
            ?? throw new LineValueException($"O eixo \"{axisCode}\" não existe na política {policy.Code}.");

        var terms = new MandateTerms
        {
            Title = values.Required("titulo"),
            Commodity = values.Option("commodity", LineValues.Commodities) ?? Commodity.Unspecified,
            QuantityUnit = values.Option("unidade", LineValues.Units) ?? MeasurementUnit.Unspecified,
            Price = new PriceCriteria { AtMarket = values.Flag("a_mercado") },
        };
        if (values.Text("criterio") is { } criteria) terms.Criteria = criteria;
        if (values.Text("tela") is { } tenor) terms.Tenor = tenor;
        if (values.Number("quantidade") is { } quantity) terms.Quantity = quantity;
        if (values.Number("preco_alvo") is { } target) terms.Price.Target = target;
        if (values.Number("preco_min") is { } min) terms.Price.Min = min;
        if (values.Number("preco_max") is { } max) terms.Price.Max = max;
        if (values.Text("unidade_preco") is { } priceUnit) terms.Price.Unit = priceUnit;
        if (values.Date("janela_inicio") is { } start) terms.WindowStart = start;
        if (values.Date("janela_fim") is { } end) terms.WindowEnd = end;

        var mandate = await mandates.IssueMandateAsync(
            new IssueMandateRequest
            {
                Context = context,
                PolicyId = policy.Id,
                AxisId = axis.Id,
                Type = values.RequiredOption("tipo", LineValues.MandateTypes),
                Terms = terms,
            },
            call);
        return LineResult.Ok($"Mandato {mandate.Code} emitido no eixo {axis.Code} da política {policy.Code}.", mandate.Code);
    }

    /// <summary>Boletas: registra a operação (com ou sem mandato); o enquadramento e a aprovação seguem no core.</summary>
    private async Task<LineResult> RegisterOrderAsync(RequestContext context, LineValues values, CallOptions call, CancellationToken cancellationToken)
    {
        var mandate = values.Text("mandato") is { } mandateCode ? await lookups.MandateAsync(context, mandateCode, cancellationToken) : null;
        var counterparty = await lookups.CounterpartyAsync(context, values.Required("contraparte"), cancellationToken);
        var type = values.RequiredOption("instrumento", LineValues.OrderTypes);

        var terms = new OrderTerms
        {
            Type = type,
            Direction = values.RequiredOption("operacao", LineValues.Directions),
            Tenor = values.Required("tela"),
            Price = values.RequiredNumber("preco"),
            TradeDate = values.RequiredDate("data_operacao"),
            OptionKind = values.Option("tipo_opcao", LineValues.OptionKinds) ?? OptionKind.Unspecified,
            Commodity = values.Option("commodity", LineValues.Commodities) ?? Commodity.Unspecified,
            CoveredSale = values.Flag("venda_coberta"),
        };
        if (values.Number("lotes") is { } lots) terms.Lots = lots;
        if (values.Number("nocional_usd") is { } notional) terms.NotionalUsd = notional;
        if (values.Text("unidade_preco") is { } priceUnit) terms.PriceUnit = priceUnit;
        if (values.Number("premio") is { } premium) terms.Premium = premium;
        if (values.Text("justificativa") is { } justification) terms.Justification = justification;
        if (values.Text("observacoes") is { } notes) terms.Notes = notes;

        var request = new RegisterOrderRequest { Context = context, CounterpartyId = counterparty.Id, Terms = terms };
        if (mandate is not null) request.MandateId = mandate.Id;

        var order = await orders.RegisterOrderAsync(request, call);
        return LineResult.Ok(
            $"Boleta {order.Code} registrada com {counterparty.Name}{(mandate is null ? " (sem mandato)" : $" no mandato {mandate.Code}")}.",
            order.Code);
    }
}
