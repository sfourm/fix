using Fix.Domain.Abstractions;
using Fix.Domain.Common;

namespace Fix.Domain.AggregateRoots.Policies;

/// <summary>
/// Eixo da política-mãe (ex.: POL-PRE · Fixação de preço). Todo mandato se vincula a um eixo,
/// que declara o fator de risco, a frase-limite e as restrições.
/// </summary>
public sealed class PolicyAxis : Entity
{
    private List<string> _restrictions = [];

    private PolicyAxis()
    {
    }

    internal PolicyAxis(Guid policyId) => PolicyId = policyId;

    public Guid PolicyId { get; private set; }

    /// <summary>Código curto do eixo (ex.: POL-PRE).</summary>
    public string Code { get; private set; } = null!;

    public Title Title { get; private set; } = null!;

    public RiskFactor Factor { get; private set; }

    /// <summary>Frase-limite do eixo (ex.: "Fixar até 1.500 lotes NY11 · nunca abaixo de 16 c/lb").</summary>
    public string? Statement { get; private set; }

    /// <summary>Resumo do limite (ex.: "1.500 lotes · range 16–24 c/lb").</summary>
    public string? LimitDescription { get; private set; }

    /// <summary>Quem aprova o eixo (ex.: Conselho de Administração).</summary>
    public string? Approver { get; private set; }

    public IReadOnlyList<string> Restrictions => _restrictions.AsReadOnly();

    internal void Update(
        string code,
        Title title,
        RiskFactor factor,
        string? statement,
        string? limitDescription,
        string? approver,
        IEnumerable<string> restrictions)
    {
        Code = DomainGuard.OptionalText(code, 20, "código do eixo")?.ToUpperInvariant()
            ?? throw new DomainException("O código do eixo é obrigatório.");
        Title = title;
        Factor = factor;
        Statement = DomainGuard.OptionalText(statement, 500, "frase do eixo");
        LimitDescription = DomainGuard.OptionalText(limitDescription, 200, "limite do eixo");
        Approver = DomainGuard.OptionalText(approver, 150, "aprovador");
        _restrictions = restrictions
            .Select(r => DomainGuard.OptionalText(r, 300, "restrição"))
            .OfType<string>()
            .Distinct()
            .ToList();
    }
}

