using Fix.Application.Organizations.Commands;
using Fix.Application.Organizations.Dtos;
using Fix.Application.Organizations.Queries;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Organização: setup da companhia, membros, grupos e organograma. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface IOrganizationService
{
    Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationCommand command, CancellationToken cancellationToken);

    Task<OrganizationDto> UpdateOrganizationAsync(UpdateOrganizationCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> UpdateCompanyProfileAsync(UpdateCompanyProfileCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> UpdateIndustrialProfileAsync(UpdateIndustrialProfileCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> UpdateBudgetAsync(UpdateBudgetCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> UpdateFinancialsAsync(UpdateFinancialsCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> AddCommodityAsync(AddCommodityCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> UpdateCommodityAsync(UpdateCommodityCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> RemoveCommodityAsync(RemoveCommodityCommand command, CancellationToken cancellationToken);

    Task<Guid> AddMemberAsync(AddMemberCommand command, CancellationToken cancellationToken);

    Task ChangeMemberDeskAsync(ChangeMemberDeskCommand command, CancellationToken cancellationToken);

    Task RemoveMemberAsync(RemoveMemberCommand command, CancellationToken cancellationToken);

    Task<Guid> CreateGroupAsync(CreateGroupCommand command, CancellationToken cancellationToken);

    Task SetMemberAlcadasAsync(SetMemberAlcadasCommand command, CancellationToken cancellationToken);

    Task SetGroupAlcadasAsync(SetGroupAlcadasCommand command, CancellationToken cancellationToken);

    Task TransferOwnershipAsync(TransferOwnershipCommand command, CancellationToken cancellationToken);

    Task MoveGroupAsync(MoveGroupCommand command, CancellationToken cancellationToken);

    Task AddGroupMemberAsync(AddGroupMemberCommand command, CancellationToken cancellationToken);

    Task<OrganizationSetupDto> GetOrganizationAsync(GetOrganizationQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizationDto>> ListUserOrganizationsAsync(ListUserOrganizationsQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetUserRolesAsync(GetUserRolesQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<MemberDto>> ListMembersAsync(ListMembersQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<GroupDto>> ListGroupsAsync(ListGroupsQuery query, CancellationToken cancellationToken);
}
