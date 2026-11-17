using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideGuard.Application.DTOs;
using StrideGuard.Application.Interfaces;
using StrideGuard.Application.Services;
using StrideGuard.Domain.Entities;
using StrideGuard.Domain.Exceptions;

namespace StrideGuard.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AmeacasController(
    IAmeacaRepository ameacaRepo,
    IComponenteRepository componenteRepo) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AmeacaResponse>> Criar([FromBody] AmeacaRequest req)
    {
        var comp = await componenteRepo.ObterPorIdComAmeacasAsync(req.ComponenteId)
            ?? throw new NotFoundException("Componente não encontrado.");

        var ameaca = new Ameaca(req.Titulo, req.Descricao, req.Categoria, req.Probabilidade, req.Impacto, req.ComponenteId);
        await ameacaRepo.AdicionarAsync(ameaca);

        return Created("", MotorGeradorStrideService.MapToResponse(ameaca, comp.Nome));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AmeacaResponse>> Atualizar(Guid id, [FromBody] AtualizarAmeacaRequest req)
    {
        var ameaca = await ameacaRepo.ObterPorIdComMitigacoesAsync(id)
            ?? throw new NotFoundException("Ameaça não encontrada.");

        ameaca.DefinirRisco(req.Probabilidade, req.Impacto);
        ameaca.AlterarStatus(req.Status);
        await ameacaRepo.AtualizarAsync(ameaca);

        return Ok(MotorGeradorStrideService.MapToResponse(ameaca, string.Empty));
    }

    [HttpPost("{id:guid}/mitigacoes")]
    public async Task<ActionResult<AmeacaResponse>> AdicionarMitigacao(Guid id, [FromBody] MitigacaoRequest req)
    {
        var ameaca = await ameacaRepo.ObterPorIdComMitigacoesAsync(id)
            ?? throw new NotFoundException("Ameaça não encontrada.");

        var mitigacao = new Mitigacao(req.DescricaoControle, req.Responsavel, req.Aprovada, ameaca.Id);
        ameaca.AdicionarMitigacao(mitigacao);
        await ameacaRepo.AtualizarAsync(ameaca);

        return Ok(MotorGeradorStrideService.MapToResponse(ameaca, string.Empty));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var ameaca = await ameacaRepo.ObterPorIdComMitigacoesAsync(id)
            ?? throw new NotFoundException("Ameaça não encontrada.");

        await ameacaRepo.RemoverAsync(ameaca);
        return NoContent();
    }
}
