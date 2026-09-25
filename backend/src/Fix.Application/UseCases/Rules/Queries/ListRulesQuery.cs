using Fix.Application.Abstractions.Messaging;
using Fix.Application.Rules.Dtos;

namespace Fix.Application.Rules.Queries;

public sealed record ListRulesQuery : IQuery<IReadOnlyList<RuleDto>>;
