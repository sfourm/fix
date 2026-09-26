namespace Fix.Storage.Application.Abstractions.Context;

/// <summary>
/// Requisição executada em nome de um usuário. O id vem no contrato: a autenticação é responsabilidade do BFF, o
/// storage apenas confia no id recebido e valida as regras no core.
/// </summary>
public interface IUserRequest
{
    Guid UserId { get; }
}

/// <summary>Requisição executada dentro de uma organização (tenant).</summary>
public interface IOrganizationRequest : IUserRequest
{
    Guid OrganizationId { get; }
}
