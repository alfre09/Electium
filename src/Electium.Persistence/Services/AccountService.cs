using System.Security.Claims;
using Electium.Application.Constants;
using Electium.Application.Dtos;
using Electium.Application.Exceptions;
using Electium.Application.Interfaces;
using Electium.Application.ViewModels;
using Electium.Domain.Entities;
using Electium.Persistence.Identity;
using Microsoft.AspNetCore.Identity;

namespace Electium.Persistence.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _users;
    private readonly SignInManager<AppUser> _signIn;
    private readonly IGenericRepository<Party> _parties;

    public AccountService(
        UserManager<AppUser> users,
        SignInManager<AppUser> signIn,
        IGenericRepository<Party> parties)
    {
        _users = users;
        _signIn = signIn;
        _parties = parties;
    }

    public async Task<string?> LoginAsync(LoginViewModel model)
    {
        const string invalid = "Correo o contraseña incorrectos.";

        var user = await _users.FindByEmailAsync(model.Email.Trim());
        if (user is null)
            return invalid;

        if (!user.IsActive)
            return "Esta cuenta está desactivada. Contacte al administrador.";

        var result = await _signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
            return null;

        return result.IsLockedOut
            ? "Cuenta bloqueada temporalmente por demasiados intentos. Intente más tarde."
            : invalid;
    }

    public Task LogoutAsync() => _signIn.SignOutAsync();

    public async Task<List<LeaderDto>> GetLeadersAsync()
    {
        var leaders = await _users.GetUsersInRoleAsync(AppRoles.Leader);
        var parties = (await _parties.GetAllAsync()).ToDictionary(p => p.Id, p => p.Name);

        return leaders
            .OrderBy(u => u.FullName)
            .Select(u => new LeaderDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                PartyId = u.PartyId,
                PartyName = u.PartyId is int pid && parties.TryGetValue(pid, out var name) ? name : "—",
                IsActive = u.IsActive
            })
            .ToList();
    }

    public async Task CreateLeaderAsync(LeaderViewModel model)
    {
        var party = await _parties.GetByIdAsync(model.PartyId)
            ?? throw new BusinessException("El partido seleccionado no existe.");

        var email = model.Email.Trim().ToLowerInvariant();
        if (await _users.FindByEmailAsync(email) is not null)
            throw new BusinessException("Ya existe un usuario con ese correo.");

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = model.FullName.Trim(),
            PartyId = party.Id,
            IsActive = true
        };

        var created = await _users.CreateAsync(user, model.Password);
        Ensure(created);

        Ensure(await _users.AddToRoleAsync(user, AppRoles.Leader));
        Ensure(await _users.AddClaimAsync(user, new Claim(AppClaims.PartyId, party.Id.ToString())));
    }

    public async Task SetLeaderActiveAsync(string userId, bool isActive)
    {
        var user = await FindLeaderAsync(userId);
        user.IsActive = isActive;
        Ensure(await _users.UpdateAsync(user));
    }

    public async Task DeleteLeaderAsync(string userId)
    {
        var user = await FindLeaderAsync(userId);
        Ensure(await _users.DeleteAsync(user));
    }

    private async Task<AppUser> FindLeaderAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId)
            ?? throw new BusinessException("El usuario no existe.");

        if (!await _users.IsInRoleAsync(user, AppRoles.Leader))
            throw new BusinessException("Solo se pueden administrar cuentas de dirigentes.");

        return user;
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
