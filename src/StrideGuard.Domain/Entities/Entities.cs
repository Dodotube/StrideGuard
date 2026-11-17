using StrideGuard.Domain.Enums;
using StrideGuard.Domain.Exceptions;

namespace StrideGuard.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }

    public Usuario(string nome, string email, string passwordHash, string role = "SecurityArchitect")
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome é obrigatório.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("E-mail é obrigatório.");

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
    }
}

public class Projeto
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; }
    public string Descricao { get; private set; }
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;
    public Guid UsuarioId { get; private set; }

    private readonly List<Componente> _componentes = new();
    public IReadOnlyCollection<Componente> Componentes => _componentes.AsReadOnly();

    protected Projeto() { Nome = null!; Descricao = null!; }

    public Projeto(string nome, string descricao, Guid usuarioId)
    {
        Atualizar(nome, descricao);
        UsuarioId = usuarioId;
    }

    public void Atualizar(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do projeto é obrigatório.");

        Nome = nome.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
    }
}

public class Componente
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; }
    public TipoComponente Tipo { get; private set; }
    public TrustBoundary FronteiraConfianca { get; private set; }
    public Guid ProjetoId { get; private set; }

    private readonly List<Ameaca> _ameacas = new();
    public IReadOnlyCollection<Ameaca> Ameacas => _ameacas.AsReadOnly();

    protected Componente() { Nome = null!; }

    public Componente(string nome, TipoComponente tipo, TrustBoundary fronteira, Guid projetoId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do componente é obrigatório.");

        Nome = nome.Trim();
        Tipo = tipo;
        FronteiraConfianca = fronteira;
        ProjetoId = projetoId;
    }
}

public class Ameaca
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Titulo { get; private set; }
    public string Descricao { get; private set; }
    public CategoriaStride Categoria { get; private set; }
    public int Probabilidade { get; private set; }
    public int Impacto { get; private set; }
    public NivelRisco Risco { get; private set; }
    public StatusAmeaca Status { get; private set; } = StatusAmeaca.Aberta;
    public Guid ComponenteId { get; private set; }

    private readonly List<Mitigacao> _mitigacoes = new();
    public IReadOnlyCollection<Mitigacao> Mitigacoes => _mitigacoes.AsReadOnly();

    protected Ameaca() { Titulo = null!; Descricao = null!; }

    public Ameaca(string titulo, string descricao, CategoriaStride categoria, int probabilidade, int impacto, Guid componenteId)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("Título da ameaça é obrigatório.");
        if (!Enum.IsDefined(typeof(CategoriaStride), categoria))
            throw new DomainException("Categoria STRIDE inválida.");

        Titulo = titulo.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        Categoria = categoria;
        ComponenteId = componenteId;
        DefinirRisco(probabilidade, impacto);
    }

    public void DefinirRisco(int probabilidade, int impacto)
    {
        if (probabilidade is < 1 or > 5 || impacto is < 1 or > 5)
            throw new DomainException("Probabilidade e Impacto devem estar entre 1 e 5.");

        Probabilidade = probabilidade;
        Impacto = impacto;
        int score = probabilidade * impacto;

        Risco = score switch
        {
            >= 20 => NivelRisco.Critico,
            >= 15 => NivelRisco.Alto,
            >= 7 => NivelRisco.Medio,
            _ => NivelRisco.Baixo
        };
    }

    public void AdicionarMitigacao(Mitigacao mitigacao)
    {
        _mitigacoes.Add(mitigacao);
        if (Status == StatusAmeaca.Aberta)
            Status = StatusAmeaca.EmAnalise;
    }

    public void AlterarStatus(StatusAmeaca novoStatus)
    {
        if (novoStatus == StatusAmeaca.Mitigada && !_mitigacoes.Any(m => m.Aprovada))
            throw new DomainException("Regra RN03: Uma ameaça só pode ser marcada como 'Mitigada' se possuir pelo menos uma contramedida aprovada.");

        Status = novoStatus;
    }
}

public class Mitigacao
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string DescricaoControle { get; private set; }
    public string Responsavel { get; private set; }
    public bool Aprovada { get; private set; }
    public Guid AmeacaId { get; private set; }

    protected Mitigacao() { DescricaoControle = null!; Responsavel = null!; }

    public Mitigacao(string descricaoControle, string responsavel, bool aprovada, Guid ameacaId)
    {
        if (string.IsNullOrWhiteSpace(descricaoControle))
            throw new DomainException("Descrição do controle de mitigação é obrigatória.");

        DescricaoControle = descricaoControle.Trim();
        Responsavel = string.IsNullOrWhiteSpace(responsavel) ? "Time de Engenharia" : responsavel.Trim();
        Aprovada = aprovada;
        AmeacaId = ameacaId;
    }

    public void Aprovar() => Aprovada = true;
}
