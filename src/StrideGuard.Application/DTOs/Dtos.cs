using StrideGuard.Domain.Enums;

namespace StrideGuard.Application.DTOs;

public record RegistrarUsuarioRequest(string Nome, string Email, string Senha);
public record LoginRequest(string Email, string Senha);
public record AuthResponse(string Token, string Nome, string Email, Guid UsuarioId);

public record ProjetoRequest(string Nome, string Descricao);
public record ProjetoResponse(Guid Id, string Nome, string Descricao, DateTime CriadoEm, int TotalComponentes, int TotalAmeacas);

public record ComponenteRequest(string Nome, TipoComponente Tipo, TrustBoundary FronteiraConfianca, Guid ProjetoId);
public record ComponenteResponse(Guid Id, string Nome, string Tipo, string FronteiraConfianca, int QtdAmeacas);

public record AmeacaRequest(string Titulo, string Descricao, CategoriaStride Categoria, int Probabilidade, int Impacto, Guid ComponenteId);
public record AtualizarAmeacaRequest(string Titulo, string Descricao, int Probabilidade, int Impacto, StatusAmeaca Status);
public record MitigacaoRequest(string DescricaoControle, string Responsavel, bool Aprovada);
public record MitigacaoResponse(Guid Id, string DescricaoControle, string Responsavel, bool Aprovada);

public record AmeacaResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    string Categoria,
    int Probabilidade,
    int Impacto,
    string Risco,
    string Status,
    Guid ComponenteId,
    string NomeComponente,
    List<MitigacaoResponse> Mitigacoes);

public record RelatorioStrideResponse(
    Guid ProjetoId,
    string NomeProjeto,
    int TotalAmeacas,
    int AmeacasMitigadas,
    double PercentualMitigado,
    Dictionary<string, int> ContagemPorCategoriaStride,
    Dictionary<string, int> ContagemPorNivelRisco);
