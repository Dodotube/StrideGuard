using StrideGuard.Domain.Entities;

namespace StrideGuard.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task AdicionarAsync(Usuario usuario);
}

public interface IProjetoRepository
{
    Task<IEnumerable<Projeto>> ListarPorUsuarioAsync(Guid usuarioId);
    Task<Projeto?> ObterCompletoPorIdAsync(Guid id);
    Task AdicionarAsync(Projeto projeto);
    Task AtualizarAsync(Projeto projeto);
    Task RemoverAsync(Projeto projeto);
}

public interface IComponenteRepository
{
    Task<Componente?> ObterPorIdComAmeacasAsync(Guid id);
    Task AdicionarAsync(Componente componente);
    Task RemoverAsync(Componente componente);
}

public interface IAmeacaRepository
{
    Task<Ameaca?> ObterPorIdComMitigacoesAsync(Guid id);
    Task AdicionarAsync(Ameaca ameaca);
    Task AdicionarVariasAsync(IEnumerable<Ameaca> ameacas);
    Task AdicionarMitigacaoAsync(Mitigacao mitigacao);
    Task AtualizarAsync(Ameaca ameaca);
    Task RemoverAsync(Ameaca ameaca);
}

public interface ITokenService
{
    string GerarToken(Usuario usuario);
}

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verify(string senha, string hash);
}
