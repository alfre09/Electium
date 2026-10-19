using System.Security.Claims;
using Electium.Application.Constants;
using Electium.Domain.Entities;
using Electium.Persistence.Contexts;
using Electium.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Electium.Persistence.Seed;

/// <summary>
/// Datos iniciales para poder probar el sistema desde el primer arranque.
/// Los documentos y correos son ficticios.
/// </summary>
public static class DbSeeder
{
    public const string AdminEmail = "admin@electium.local";
    public const string AdminPassword = "Admin123*";
    public const string LeaderEmail = "dirigente@electium.local";
    public const string LeaderPassword = "Lider123*";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ElectiumDbContext>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<AppUser>>();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.Leader })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }

        await SeedBusinessDataAsync(db);
        await SeedUsersAsync(db, users);
    }

    private static async Task SeedBusinessDataAsync(ElectiumDbContext db)
    {
        if (await db.Parties.AnyAsync() || await db.Citizens.AnyAsync())
            return;

        var parties = new[]
        {
            new Party { Name = "Partido Horizonte", Description = "Desarrollo económico y empleo." },
            new Party { Name = "Alianza Ciudadana", Description = "Transparencia y participación ciudadana." },
            new Party { Name = "Movimiento Renovación", Description = "Educación, ciencia y tecnología." }
        };
        db.Parties.AddRange(parties);

        var positions = new[]
        {
            new ElectivePosition { Name = "Presidente", Description = "Presidente de la República." },
            new ElectivePosition { Name = "Alcalde", Description = "Alcalde del municipio." }
        };
        db.ElectivePositions.AddRange(positions);

        var names = new (string Name, string Last)[]
        {
            ("Carlos", "Martínez"), ("Laura", "Peña"), ("Miguel", "Santos"),
            ("Ana", "Rodríguez"), ("Luis", "Fernández"), ("Rosa", "Jiménez"),
            ("Pedro", "Almonte"), ("María", "Castillo"), ("José", "Núñez"),
            ("Carmen", "Reyes"), ("Juan", "Batista"), ("Sofía", "Medina")
        };

        var citizens = names
            .Select((n, i) => new Citizen
            {
                Name = n.Name,
                LastName = n.Last,
                Email = $"ciudadano{i + 1}@example.com",
                // Documentos ficticios: 00100000011, 00100000022, ...
                IdentityDocument = $"001{(i + 1) * 11:D8}",
                IsActive = true
            })
            .ToList();
        db.Citizens.AddRange(citizens);

        var election = new Election
        {
            Name = "Elecciones Generales",
            Description = "Elección de prueba creada automáticamente.",
        };
        db.Elections.Add(election);

        await db.SaveChangesAsync();

        // 3 partidos x 2 puestos = 6 candidatos (los 6 primeros ciudadanos).
        var index = 0;
        foreach (var position in positions)
        {
            foreach (var party in parties)
            {
                db.Candidates.Add(new Candidate
                {
                    ElectionId = election.Id,
                    PartyId = party.Id,
                    ElectivePositionId = position.Id,
                    CitizenId = citizens[index++].Id
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(ElectiumDbContext db, UserManager<AppUser> users)
    {
        if (await users.FindByEmailAsync(AdminEmail) is null)
        {
            var admin = new AppUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = "Administrador Electium",
                IsActive = true
            };

            if ((await users.CreateAsync(admin, AdminPassword)).Succeeded)
                await users.AddToRoleAsync(admin, AppRoles.Admin);
        }

        if (await users.FindByEmailAsync(LeaderEmail) is null)
        {
            var party = await db.Parties.OrderBy(p => p.Id).FirstOrDefaultAsync();
            if (party is null)
                return;

            var leader = new AppUser
            {
                UserName = LeaderEmail,
                Email = LeaderEmail,
                EmailConfirmed = true,
                FullName = "Dirigente Político",
                PartyId = party.Id,
                IsActive = true
            };

            if ((await users.CreateAsync(leader, LeaderPassword)).Succeeded)
            {
                await users.AddToRoleAsync(leader, AppRoles.Leader);
                await users.AddClaimAsync(leader, new Claim(AppClaims.PartyId, party.Id.ToString()));
            }
        }
    }
}
