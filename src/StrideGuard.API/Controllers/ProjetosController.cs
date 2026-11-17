using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideGuard.Application.DTOs;
using StrideGuard.Application.Interfaces;
using StrideGuard.Application.Services;
using StrideGuard.Domain.Entities;
using StrideGuard.Domain.Enums;
using StrideGuard.Domain.Exceptions;

namespace StrideGuard.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjetosController(
    IProjetoRepository projetoRepo,
    IComponenteRepository componenteRepo,
    MotorGeradorStrideService motorStride) : ControllerBase
{
    private Guid ObterUsuarioId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjetoResponse>>> Listar()
    {
        var projetos = await projetoRepo.ListarPorUsuarioAsync(ObterUsuarioId());
        return Ok(projetos.Select(p => new ProjetoResponse(
            p.Id, p.Nome, p.Descricao, p.CriadoEm,
            p.Componentes.Count,
            p.Componentes.Sum(c => c.Ameacas.Count))));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<object>> ObterDetalhes(Guid id)
    {
        var p = await projetoRepo.ObterCompletoPorIdAsync(id)
            ?? throw new NotFoundException("Projeto não encontrado.");

        return Ok(new
        {
            p.Id,
            p.Nome,
            p.Descricao,
            Componentes = p.Componentes.Select(c => new ComponenteResponse(
                c.Id, c.Nome, c.Tipo.ToString(), c.FronteiraConfianca.ToString(), c.Ameacas.Count)),
            Ameacas = p.Componentes.SelectMany(c =>
                c.Ameacas.Select(a => MotorGeradorStrideService.MapToResponse(a, c.Nome)))
        });
    }

    [HttpPost]
    public async Task<ActionResult<ProjetoResponse>> Criar([FromBody] ProjetoRequest req)
    {
        var projeto = new Projeto(req.Nome, req.Descricao, ObterUsuarioId());
        await projetoRepo.AdicionarAsync(projeto);
        return CreatedAtAction(nameof(ObterDetalhes), new { id = projeto.Id },
            new ProjetoResponse(projeto.Id, projeto.Nome, projeto.Descricao, projeto.CriadoEm, 0, 0));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] ProjetoRequest req)
    {
        var projeto = await projetoRepo.ObterCompletoPorIdAsync(id)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (projeto.UsuarioId != ObterUsuarioId())
            throw new UnauthorizedAccessException("Apenas o proprietário pode editar este projeto.");

        projeto.Atualizar(req.Nome, req.Descricao);
        await projetoRepo.AtualizarAsync(projeto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var projeto = await projetoRepo.ObterCompletoPorIdAsync(id)
            ?? throw new NotFoundException("Projeto não encontrado.");

        if (projeto.UsuarioId != ObterUsuarioId())
            throw new UnauthorizedAccessException("Regra RN04: Apenas o proprietário pode excluir este projeto.");

        await projetoRepo.RemoverAsync(projeto);
        return NoContent();
    }

    [HttpPost("{id:guid}/componentes")]
    public async Task<ActionResult<ComponenteResponse>> AdicionarComponente(Guid id, [FromBody] ComponenteRequest req)
    {
        _ = await projetoRepo.ObterCompletoPorIdAsync(id)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var comp = new Componente(req.Nome, req.Tipo, req.FronteiraConfianca, id);
        await componenteRepo.AdicionarAsync(comp);

        return Created("", new ComponenteResponse(comp.Id, comp.Nome, comp.Tipo.ToString(), comp.FronteiraConfianca.ToString(), 0));
    }

    [HttpPost("{id:guid}/analisar-stride")]
    public async Task<ActionResult<IEnumerable<AmeacaResponse>>> AnalisarStride(Guid id)
    {
        var ameacas = await motorStride.AnalisarProjetoAsync(id);
        return Ok(ameacas);
    }

    [HttpGet("{id:guid}/relatorio")]
    public async Task<ActionResult<RelatorioStrideResponse>> ObterRelatorio(Guid id)
    {
        var projeto = await projetoRepo.ObterCompletoPorIdAsync(id)
            ?? throw new NotFoundException("Projeto não encontrado.");

        var todas = projeto.Componentes.SelectMany(c => c.Ameacas).ToList();
        int total = todas.Count;
        int mitigadas = todas.Count(a => a.Status == StatusAmeaca.Mitigada);
        double pct = total == 0 ? 100 : Math.Round((double)mitigadas / total * 100, 1);

        var porCategoria = Enum.GetValues<CategoriaStride>()
            .ToDictionary(cat => cat.ToString(), cat => todas.Count(a => a.Categoria == cat));

        var porRisco = Enum.GetValues<NivelRisco>()
            .ToDictionary(r => r.ToString(), r => todas.Count(a => a.Risco == r));

        return Ok(new RelatorioStrideResponse(projeto.Id, projeto.Nome, total, mitigadas, pct, porCategoria, porRisco));
    }
}
