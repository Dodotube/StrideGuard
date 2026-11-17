using StrideGuard.Application.DTOs;
using StrideGuard.Application.Interfaces;
using StrideGuard.Domain.Entities;
using StrideGuard.Domain.Enums;
using StrideGuard.Domain.Exceptions;

namespace StrideGuard.Application.Services;

public class MotorGeradorStrideService
{
    private readonly IProjetoRepository _projetoRepo;
    private readonly IAmeacaRepository _ameacaRepo;

    public MotorGeradorStrideService(IProjetoRepository projetoRepo, IAmeacaRepository ameacaRepo)
    {
        _projetoRepo = projetoRepo;
        _ameacaRepo = ameacaRepo;
    }

    public async Task<IEnumerable<AmeacaResponse>> AnalisarProjetoAsync(Guid projetoId)
    {
        var projeto = await _projetoRepo.ObterCompletoPorIdAsync(projetoId)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (!projeto.Componentes.Any())
            throw new DomainException("Adicione ao menos um componente antes de executar a análise STRIDE.");

        var novasAmeacas = new List<Ameaca>();

        foreach (var comp in projeto.Componentes)
        {
            var regras = GerarRegrasPorComponente(comp);
            foreach (var r in regras)
            {
                if (!comp.Ameacas.Any(a => a.Titulo.Equals(r.Titulo, StringComparison.OrdinalIgnoreCase)))
                {
                    novasAmeacas.Add(r);
                }
            }
        }

        if (novasAmeacas.Any())
            await _ameacaRepo.AdicionarVariasAsync(novasAmeacas);

        var atualizado = await _projetoRepo.ObterCompletoPorIdAsync(projetoId);
        return atualizado!.Componentes
            .SelectMany(c => c.Ameacas.Select(a => MapToResponse(a, c.Nome)))
            .ToList();
    }

    private static List<Ameaca> GerarRegrasPorComponente(Componente comp)
    {
        var lista = new List<Ameaca>();
        bool expostoInternet = comp.FronteiraConfianca == TrustBoundary.InternetPublica;

        switch (comp.Tipo)
        {
            case TipoComponente.ApiRest:
                lista.Add(new Ameaca(
                    "Forja ou Roubo de Token JWT",
                    "Atacante intercepta ou forja assinatura de token Bearer para personificar usuário.",
                    CategoriaStride.Spoofing, expostoInternet ? 5 : 3, 5, comp.Id));
                lista.Add(new Ameaca(
                    "Manipulação de Payload / Mass Assignment",
                    "Alteração indevida de parâmetros JSON na requisição para modificar campos protegidos.",
                    CategoriaStride.Tampering, 4, 4, comp.Id));
                if (expostoInternet)
                {
                    lista.Add(new Ameaca(
                        "Exaustão de Recursos (DDoS / Sem Rate Limit)",
                        "Rajadas de requisições HTTP derrubam a disponibilidade da API pública.",
                        CategoriaStride.DenialOfService, 5, 4, comp.Id));
                }
                break;

            case TipoComponente.BancoDeDados:
                lista.Add(new Ameaca(
                    "SQL Injection / Acesso Não Autorizado a Tabelas",
                    "Extração massiva de registros sensíveis via consultas não parametrizadas.",
                    CategoriaStride.InformationDisclosure, 3, 5, comp.Id));
                lista.Add(new Ameaca(
                    "Ausência de Trilha de Auditoria em Alterações Críticas",
                    "Usuário privilegiado altera saldos ou registros sem log imutável (Repúdio).",
                    CategoriaStride.Repudiation, 3, 4, comp.Id));
                break;

            case TipoComponente.AutenticacaoIdP:
                lista.Add(new Ameaca(
                    "Quebra de Controle de Acesso (BOLA / Privilege Escalation)",
                    "Usuário comum acessa endpoints administrativos por falha na validação de Claims/Roles.",
                    CategoriaStride.ElevationOfPrivilege, 4, 5, comp.Id));
                lista.Add(new Ameaca(
                    "Ataque de Força Bruta / Credential Stuffing",
                    "Tentativas automatizadas de login sem bloqueio progressivo ou MFA.",
                    CategoriaStride.Spoofing, 5, 4, comp.Id));
                break;

            case TipoComponente.FrontendSpa:
                lista.Add(new Ameaca(
                    "Cross-Site Scripting (XSS) Armazenado",
                    "Injeção de scripts maliciosos no navegador da vítima para roubo de sessão.",
                    CategoriaStride.InformationDisclosure, 4, 4, comp.Id));
                break;

            case TipoComponente.MensageriaFila:
                lista.Add(new Ameaca(
                    "Envenenamento de Mensagens no Broker (Tampering)",
                    "Publicação de eventos falsificados na fila sem assinatura digital.",
                    CategoriaStride.Tampering, 3, 4, comp.Id));
                break;
        }

        return lista;
    }

    public static AmeacaResponse MapToResponse(Ameaca a, string nomeComponente) => new(
        a.Id,
        a.Titulo,
        a.Descricao,
        a.Categoria.ToString(),
        a.Probabilidade,
        a.Impacto,
        a.Risco.ToString(),
        a.Status.ToString(),
        a.ComponenteId,
        nomeComponente,
        a.Mitigacoes.Select(m => new MitigacaoResponse(m.Id, m.DescricaoControle, m.Responsavel, m.Aprovada)).ToList()
    );
}
