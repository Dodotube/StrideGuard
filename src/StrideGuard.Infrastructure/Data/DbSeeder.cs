using Microsoft.EntityFrameworkCore;
using StrideGuard.Application.Interfaces;
using StrideGuard.Domain.Entities;
using StrideGuard.Domain.Enums;

namespace StrideGuard.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        if (await db.Usuarios.AnyAsync())
            return;

        var arquiteto = new Usuario(
            nome: "Paulo Israel de Aguiar",
            email: "arquiteto@strideguard.com",
            passwordHash: hasher.Hash("Stride@123"),
            role: "SecurityArchitect");

        db.Usuarios.Add(arquiteto);

        var projeto = new Projeto(
            nome: "API Gateway de Pagamentos PIX",
            descricao: "Arquitetura de microsserviços financeiros exposta para aplicativos mobile e parceiros B2B.",
            usuarioId: arquiteto.Id);

        db.Projetos.Add(projeto);

        var apiGateway = new Componente("PIX Core API (.NET 8)", TipoComponente.ApiRest, TrustBoundary.InternetPublica, projeto.Id);
        var bancoSql = new Componente("Ledger Transacional (SQL Server)", TipoComponente.BancoDeDados, TrustBoundary.RedeInterna, projeto.Id);
        var idp = new Componente("OAuth2 / OpenID Provider", TipoComponente.AutenticacaoIdP, TrustBoundary.Dmz, projeto.Id);

        db.Componentes.AddRange(apiGateway, bancoSql, idp);

        var ameacaMitigada = new Ameaca(
            "Forja de Token JWT em Requisições de Transferência",
            "Tentativa de alteração de claims no token Bearer para personificar titular da conta.",
            CategoriaStride.Spoofing, 4, 5, apiGateway.Id);

        var mitigacao = new Mitigacao(
            "Assinatura assimétrica RS256, validação estrita de Issuer/Audience e expiração curta (15 min).",
            "Paulo Aguiar", true, ameacaMitigada.Id);

        ameacaMitigada.AdicionarMitigacao(mitigacao);
        ameacaMitigada.AlterarStatus(StatusAmeaca.Mitigada);

        var ameacaCriticaAberta = new Ameaca(
            "Exaustão de Recursos (DDoS / Sem Rate Limiting)",
            "Rajadas de requisições no endpoint /pix/transferir podem indisponibilizar o gateway.",
            CategoriaStride.DenialOfService, 5, 4, apiGateway.Id);

        var ameacaBancoAberta = new Ameaca(
            "Alteração de Saldo sem Trilha de Auditoria Imutável",
            "Modificações diretas na tabela de contas sem registro de log assinado (Repúdio).",
            CategoriaStride.Repudiation, 3, 5, bancoSql.Id);

        db.Ameacas.AddRange(ameacaMitigada, ameacaCriticaAberta, ameacaBancoAberta);
        await db.SaveChangesAsync();
    }
}
