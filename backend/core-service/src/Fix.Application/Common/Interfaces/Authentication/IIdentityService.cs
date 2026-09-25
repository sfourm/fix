namespace Fix.Application.Abstractions.Authentication;

public sealed record UserInfo(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles);

/// <summary>Abstração sobre o ASP.NET Core Identity.</summary>
public interface IIdentityService
{
    Task<Guid> CreateUserAsync(string email, string password, string fullName, CancellationToken cancellationToken);

    Task<UserInfo?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);

    Task<UserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<UserInfo?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserInfo>> GetUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);
}
