using Kedu.Payments.Api.Contracts.Responses;
using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kedu.Payments.Api.Controllers;

[ApiController]
[Route("responsaveis/{responsavelId:int}")]
public class ResponsaveisConsultasController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResponsaveisConsultasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("planos-de-pagamento")]
    public async Task<IActionResult> ListarPlanos(int responsavelId)
    {
        var existe = await _context.Responsaveis.AnyAsync(r => r.Id == responsavelId);
        if (!existe) return NotFound($"Responsável {responsavelId} não encontrado.");

        var planos = await _context.Planos
            .AsNoTracking()
            .Where(p => p.ResponsavelId == responsavelId)
            .Select(p => new
            {
                p.Id,
                p.ResponsavelId,
                p.CentroDeCustoId,
                p.Total
            })
            .ToListAsync();

        return Ok(planos);
    }

    [HttpGet("cobrancas/quantidade")]
    public async Task<IActionResult> QuantidadeCobrancas(int responsavelId)
    {
        var existe = await _context.Responsaveis.AnyAsync(r => r.Id == responsavelId);
        if (!existe) return NotFound($"Responsável {responsavelId} não encontrado.");

        var count = await _context.Cobrancas
            .AsNoTracking()
            .CountAsync(c => c.PlanoPagamento.ResponsavelId == responsavelId);

        return Ok(new { responsavelId, quantidade = count });
    }

    [HttpGet("cobrancas")]
    public async Task<IActionResult> ListarCobrancas(
    int responsavelId,
    string? status,
    string? metodoPagamento,
    bool? vencida)
    {
        var existe = await _context.Responsaveis.AnyAsync(r => r.Id == responsavelId);
        if (!existe) return NotFound($"Responsável {responsavelId} não encontrado.");

        var hoje = DateTime.Now.Date;

        var query = _context.Cobrancas
            .AsNoTracking()
            .Where(c => c.PlanoPagamento.ResponsavelId == responsavelId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<StatusCobranca>(status, ignoreCase: true, out var statusEnum))
                return BadRequest("Parâmetro 'status' inválido. Use: EMITIDA, PAGA ou CANCELADA.");

            query = query.Where(c => c.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(metodoPagamento))
        {
            if (!Enum.TryParse<MetodoPagamento>(metodoPagamento, ignoreCase: true, out var metodoEnum))
                return BadRequest("Parâmetro 'metodoPagamento' inválido. Use: PIX ou BOLETO.");

            query = query.Where(c => c.MetodoPagamento == metodoEnum);
        }

        var cobrancas = await query
            .Select(c => new CobrancaDetalhadaResponse
            {
                Id = c.Id,
                PlanoPagamentoId = c.PlanoPagamentoId,
                Valor = c.Valor,
                DataVencimento = c.DataVencimento,
                MetodoPagamento = c.MetodoPagamento,
                CodigoPagamento = c.CodigoPagamento,
                Status = c.Status,
                Vencida = (hoje > c.DataVencimento.Date) &&
                          (c.Status != StatusCobranca.PAGA) &&
                          (c.Status != StatusCobranca.CANCELADA)
            })
            .OrderBy(c => c.DataVencimento)
            .ToListAsync();

        if (vencida.HasValue)
            cobrancas = cobrancas.Where(c => c.Vencida == vencida.Value).ToList();

        return Ok(cobrancas);
    }
}