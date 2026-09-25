namespace Fix.Application.Abstractions.Context;

/// <summary>
/// Requisição executada em nome de um usuário. O id vem no contrato: a autenticação é
/// responsabilidade do BFF, o core apenas confia no id recebido e valida as permissões na base.
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
