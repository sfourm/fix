namespace Fix.Application.Organizations.Dtos;

/// <param name="IsInternal">Organização nativa da FIX (equipe interna).</param>
/// <param name="InternalAccess">O usuário vê a organização pelo acesso interno da equipe FIX, sem ser membro.</param>
public sealed record OrganizationDto(Guid Id, string Name, string Slug, bool IsInternal = false, bool InternalAccess = false);
