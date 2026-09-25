namespace Fix.Domain.Common.Interfaces;

/// <summary>Agregados que pertencem a um tenant (organização) e são isolados por ele.</summary>
public interface IOrganizationScoped
{
    Guid OrganizationId { get; }
}
