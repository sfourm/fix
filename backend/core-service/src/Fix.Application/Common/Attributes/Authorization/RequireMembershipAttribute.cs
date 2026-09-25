namespace Fix.Application.Abstractions.Authorization;

/// <summary>Exige que o usuário seja membro da organização do contrato.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class RequireMembershipAttribute : Attribute;

/// <summary>Exige que o usuário possua a role na organização (via rules diretas ou dos grupos).</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class RequireRoleAttribute(string role) : RequireMembershipAttribute
{
    public string Role { get; } = role;
}
