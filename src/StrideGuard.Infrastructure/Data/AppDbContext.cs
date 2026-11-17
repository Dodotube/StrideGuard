using Microsoft.EntityFrameworkCore;
using StrideGuard.Domain.Entities;

namespace StrideGuard.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<Componente> Componentes => Set<Componente>();
    public DbSet<Ameaca> Ameacas => Set<Ameaca>();
    public DbSet<Mitigacao> Mitigacoes => Set<Mitigacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Nome).HasMaxLength(150).IsRequired();
            e.Property(u => u.Email).HasMaxLength(180).IsRequired();
        });

        modelBuilder.Entity<Projeto>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Nome).HasMaxLength(150).IsRequired();
            e.HasMany(p => p.Componentes)
             .WithOne()
             .HasForeignKey(c => c.ProjetoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Componente>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).HasMaxLength(150).IsRequired();
            e.HasMany(c => c.Ameacas)
             .WithOne()
             .HasForeignKey(a => a.ComponenteId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Ameaca>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Titulo).HasMaxLength(200).IsRequired();
            e.HasMany(a => a.Mitigacoes)
             .WithOne()
             .HasForeignKey(m => m.AmeacaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Mitigacao>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.DescricaoControle).HasMaxLength(500).IsRequired();
        });
    }
}
