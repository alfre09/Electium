using Electium.Domain.Entities;
using Electium.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Electium.Persistence.Contexts;

public class ElectiumDbContext : IdentityDbContext<AppUser>
{
    public ElectiumDbContext(DbContextOptions<ElectiumDbContext> options) : base(options)
    {
    }

    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<ElectivePosition> ElectivePositions => Set<ElectivePosition>();
    public DbSet<Election> Elections => Set<Election>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Citizen>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(80).IsRequired();
            e.Property(x => x.Email).HasMaxLength(120).IsRequired();
            e.Property(x => x.IdentityDocument).HasMaxLength(11).IsRequired();
            e.HasIndex(x => x.IdentityDocument).IsUnique();
        });

        builder.Entity<Party>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(300);
            e.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<ElectivePosition>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(300);
            e.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Election>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Description).HasMaxLength(400);
            e.Property(x => x.Status).HasConversion<int>();
        });

        builder.Entity<Candidate>(e =>
        {
            e.Property(x => x.PhotoPath).HasMaxLength(300);

            e.HasOne(x => x.Election).WithMany(x => x.Candidates)
                .HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Party).WithMany(x => x.Candidates)
                .HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ElectivePosition).WithMany(x => x.Candidates)
                .HasForeignKey(x => x.ElectivePositionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Citizen).WithMany(x => x.Candidacies)
                .HasForeignKey(x => x.CitizenId).OnDelete(DeleteBehavior.Restrict);

            // Un ciudadano solo puede ser candidato una vez por elección.
            e.HasIndex(x => new { x.ElectionId, x.CitizenId }).IsUnique();
            // Un partido solo presenta un candidato por puesto y elección.
            e.HasIndex(x => new { x.ElectionId, x.PartyId, x.ElectivePositionId }).IsUnique();
        });

        builder.Entity<Vote>(e =>
        {
            e.HasOne(x => x.Election).WithMany(x => x.Votes)
                .HasForeignKey(x => x.ElectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Citizen).WithMany(x => x.Votes)
                .HasForeignKey(x => x.CitizenId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ElectivePosition).WithMany(x => x.Votes)
                .HasForeignKey(x => x.ElectivePositionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Candidate).WithMany(x => x.Votes)
                .HasForeignKey(x => x.CandidateId).OnDelete(DeleteBehavior.Restrict);

            // Garantiza a nivel de base de datos un solo voto por puesto.
            e.HasIndex(x => new { x.ElectionId, x.CitizenId, x.ElectivePositionId }).IsUnique();
        });

        builder.Entity<AppUser>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(120);
            e.HasOne(x => x.Party).WithMany()
                .HasForeignKey(x => x.PartyId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
