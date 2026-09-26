namespace Fix.Storage.Application.Abstractions.Authorization;

/// <summary>Exige que o usuário seja membro da organização do contrato (regras efetivas consultadas no core).</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class RequireMembershipAttribute : Attribute;
