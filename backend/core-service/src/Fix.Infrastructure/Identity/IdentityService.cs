using Fix.Application.Abstractions.Authentication;
using Fix.Application.Abstractions.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fix.Infrastructure.Identity;

internal sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<Guid> CreateUserAsync(
        string email,
        string password,
        string fullName,
        CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            Email = email,
            UserName = email,
            FullName = fullName,
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            return user.Id;
        }

        if (result.Errors.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail)
                or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            throw new ConflictException("Já existe um usuário com este e-mail.");
        }

        throw new ValidationException(result.Errors.Select(e => new ValidationFailure(e.Code, e.Description)));
    }

    public async Task<UserInfo?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            return null;
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return ToUserInfo(user);
    }

    public async Task<UserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToUserInfo(user);
    }

    public async Task<UserInfo?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : ToUserInfo(user);
    }

    public async Task<IReadOnlyList<UserInfo>> GetUsersAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken)
    {
        // Roles de plataforma não são necessárias em listagens.
        return await userManager.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserInfo(u.Id, u.Email!, u.FullName, Array.Empty<string>()))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Papéis de plataforma não vêm mais do Identity: são resolvidos pela membership na organização FIX.</summary>
    private static UserInfo ToUserInfo(ApplicationUser user) => new(user.Id, user.Email!, user.FullName, []);
}

