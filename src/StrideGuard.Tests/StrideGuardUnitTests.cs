using Moq;
using StrideGuard.Application.Interfaces;
using StrideGuard.Application.Services;
using StrideGuard.Domain.Entities;
using StrideGuard.Domain.Enums;
using StrideGuard.Domain.Exceptions;

namespace StrideGuard.Tests;

public class StrideGuardUnitTests
{
    [Fact]
    public void Teste01_Ameaca_DeveCalcularNivelRiscoCritico_QuandoScoreForMaiorOuIgualA20()
    {
        var ameaca = new Ameaca("SQL Injection", "Desc", CategoriaStride.InformationDisclosure, 5, 4, Guid.NewGuid());
        Assert.Equal(NivelRisco.Critico, ameaca.Risco);
    }

    [Fact]
    public void Teste02_Ameaca_DeveBloquearStatusMitigada_QuandoNaoHouverMitigacaoAprovada_RN03()
    {
        var ameaca = new Ameaca("Forja JWT", "Desc", CategoriaStride.Spoofing, 4, 5, Guid.NewGuid());
        var ex = Assert.Throws<DomainException>(() => ameaca.AlterarStatus(StatusAmeaca.Mitigada));
        Assert.Contains("RN03", ex.Message);
    }

    [Fact]
    public void Teste03_Ameaca_DevePermitirStatusMitigada_QuandoHouverMitigacaoAprovada()
    {
        var ameaca = new Ameaca("Forja JWT", "Desc", CategoriaStride.Spoofing, 4, 5, Guid.NewGuid());
        ameaca.AdicionarMitigacao(new Mitigacao("Validar assinatura RS256 e expiração", "Paulo", true, ameaca.Id));
        ameaca.AlterarStatus(StatusAmeaca.Mitigada);
        Assert.Equal(StatusAmeaca.Mitigada, ameaca.Status);
    }

    [Fact]
    public void Teste04_Ameaca_DeveRejeitarCategoriaStrideInvalida()
    {
        Assert.Throws<DomainException>(() =>
            new Ameaca("Teste", "Desc", (CategoriaStride)99, 3, 3, Guid.NewGuid()));
    }

    [Fact]
    public async Task Teste05_MotorStride_DeveGerarAmeacasAutomaticamente_ParaApiExpostaNaInternet()
    {
        var projeto = new Projeto("PIX Gateway", "Sistema financeiro", Guid.NewGuid());
        var comp = new Componente("API Pagamentos", TipoComponente.ApiRest, TrustBoundary.InternetPublica, projeto.Id);
        typeof(Projeto).GetField("_componentes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(projeto, new List<Componente> { comp });

        var projetoRepo = new Mock<IProjetoRepository>();
        projetoRepo.Setup(r => r.ObterCompletoPorIdAsync(projeto.Id)).ReturnsAsync(projeto);

        var ameacaRepo = new Mock<IAmeacaRepository>();
        var motor = new MotorGeradorStrideService(projetoRepo.Object, ameacaRepo.Object);

        await motor.AnalisarProjetoAsync(projeto.Id);

        ameacaRepo.Verify(r => r.AdicionarVariasAsync(It.Is<IEnumerable<Ameaca>>(list =>
            list.Any(a => a.Categoria == CategoriaStride.Spoofing) &&
            list.Any(a => a.Categoria == CategoriaStride.DenialOfService)
        )), Times.Once);
    }

    [Fact]
    public async Task Teste06_AuthService_DeveRejeitarLoginComSenhaIncorreta()
    {
        var usuario = new Usuario("Paulo", "paulo@stride.com", "hash_salvo");
        var repo = new Mock<IUsuarioRepository>();
        repo.Setup(r => r.ObterPorEmailAsync("paulo@stride.com")).ReturnsAsync(usuario);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("senha_errada", "hash_salvo")).Returns(false);

        var tokenService = new Mock<ITokenService>();
        var auth = new AuthService(repo.Object, hasher.Object, tokenService.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            auth.LoginAsync(new("paulo@stride.com", "senha_errada")));
    }
}
