using Microsoft.EntityFrameworkCore;
using StrideGuard.Application.Interfaces;
using StrideGuard.Domain.Entities;
using StrideGuard.Infrastructure.Data;

namespace StrideGuard.Infrastructure.Repositories;

public class UsuarioRepository(AppDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorEmailAsync(string email) =>
        db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public async Task AdicionarAsync(Usuario usuario)
    {
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
    }
}

public class ProjetoRepository(AppDbContext db) : IProjetoRepository
{
    public async Task<IEnumerable<Projeto>> ListarPorUsuarioAsync(Guid usuarioId) =>
        await db.Projetos
            .Include(p => p.Componentes)
            .ThenInclude(c => c.Ameacas)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CriadoEm)
            .ToListAsync();

    public Task<Projeto?> ObterCompletoPorIdAsync(Guid id) =>
        db.Projetos
            .Include(p => p.Componentes)
            .ThenInclude(c => c.Ameacas)
            .ThenInclude(a => a.Mitigacoes)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task AdicionarAsync(Projeto projeto)
    {
        db.Projetos.Add(projeto);
        await db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Projeto projeto)
    {
        db.Projetos.Update(projeto);
        await db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Projeto projeto)
    {
        db.Projetos.Remove(projeto);
        await db.SaveChangesAsync();
    }
}

public class ComponenteRepository(AppDbContext db) : IComponenteRepository
{
    public Task<Componente?> ObterPorIdComAmeacasAsync(Guid id) =>
        db.Componentes.Include(c => c.Ameacas).FirstOrDefaultAsync(c => c.Id == id);

    public async Task AdicionarAsync(Componente componente)
    {
        db.Componentes.Add(componente);
        await db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Componente componente)
    {
        db.Componentes.Remove(componente);
        await db.SaveChangesAsync();
    }
}

public class AmeacaRepository(AppDbContext db) : IAmeacaRepository
{
    public Task<Ameaca?> ObterPorIdComMitigacoesAsync(Guid id) =>
        db.Ameacas.Include(a => a.Mitigacoes).FirstOrDefaultAsync(a => a.Id == id);

    public async Task AdicionarAsync(Ameaca ameaca)
    {
        db.Ameacas.Add(ameaca);
        await db.SaveChangesAsync();
    }

    public async Task AdicionarVariasAsync(IEnumerable<Ameaca> ameacas)
    {
        db.Ameacas.AddRange(ameacas);
        await db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Ameaca ameaca)
    {
        db.Ameacas.Update(ameaca);
        await db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Ameaca ameaca)
    {
        db.Ameacas.Remove(ameaca);
        await db.SaveChangesAsync();
    }
}
