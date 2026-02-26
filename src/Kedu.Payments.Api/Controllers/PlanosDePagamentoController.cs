using Kedu.Payments.Api.Contracts.Requests;
using Kedu.Payments.Api.Contracts.Responses;
using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Entities;
using Kedu.Payments.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kedu.Payments.Api.Controllers;

[ApiController]
[Route("planos-de-pagamento")]
public class PlanosDePagamentoController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlanosDePagamentoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPlanoPagamentoRequest request)
    {
        if (request.ResponsavelId <= 0)
            return BadRequest("responsavelId inválido.");

        if (request.Cobrancas == null || request.Cobrancas.Count == 0)
            return BadRequest("Informe ao menos uma cobrança.");

        if (request.Cobrancas.Any(c => c.Valor <= 0))
            return BadRequest("Valor da cobrança deve ser maior que zero.");

        var responsavelExiste = await _context.Responsaveis
            .AnyAsync(r => r.Id == request.ResponsavelId);

        if (!responsavelExiste)
            return NotFound($"Responsável {request.ResponsavelId} não encontrado.");

        var centroExiste = await _context.CentrosDeCusto
        .AnyAsync(c => c.Id == request.CentroDeCustoId);
        
        if (!centroExiste) 
            return NotFound($"Centro de custo {request.CentroDeCustoId} não encontrado.");

        var plano = new PlanoPagamento
        {
            ResponsavelId = request.ResponsavelId,
            CentroDeCustoId = request.CentroDeCustoId,
        };

        foreach (var c in request.Cobrancas.OrderBy(x => x.DataVencimento))
        {
            var cobranca = new Cobranca
            {
                Valor = c.Valor,
                DataVencimento = c.DataVencimento.Date,
                MetodoPagamento = c.MetodoPagamento,
                Status = StatusCobranca.EMITIDA,
                CodigoPagamento = GerarCodigoPagamento(c.MetodoPagamento)
            };

            plano.Cobrancas.Add(cobranca);
        }

        plano.Total = plano.Cobrancas.Sum(x => x.Valor);
        

        _context.Planos.Add(plano);
        await _context.SaveChangesAsync();

        await _context.Entry(plano)
        .Reference(p => p.CentroDeCusto)
        .LoadAsync();

        var response = MapPlano(plano);

        return CreatedAtAction(nameof(ObterPorId), new { id = plano.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var plano = await _context.Planos
            .Include(p => p.Cobrancas)
            .Include(p => p.CentroDeCusto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plano == null)
            return NotFound();

        return Ok(MapPlano(plano));
    }

    [HttpGet("{id:int}/total")]
    public async Task<IActionResult> ObterTotal(int id)
    {
        var plano = await _context.Planos
            .AsNoTracking()
            .Select(p => new { p.Id, p.Total })
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plano == null)
            return NotFound();

        return Ok(new { planoId = plano.Id, total = plano.Total });
    }

    private static PlanoPagamentoResponse MapPlano(PlanoPagamento plano)
    {
        return new PlanoPagamentoResponse
        {
            Id = plano.Id,
            ResponsavelId = plano.ResponsavelId,
            CentroDeCustoId = plano.CentroDeCustoId,
            CentroDeCusto = plano.CentroDeCusto?.Nome ?? string.Empty,
            Total = plano.Total,
            Cobrancas = plano.Cobrancas
                .OrderBy(c => c.DataVencimento)
                .Select(c => new CobrancaResponse
                {
                    Id = c.Id,
                    Valor = c.Valor,
                    DataVencimento = c.DataVencimento,
                    MetodoPagamento = c.MetodoPagamento,
                    Status = c.Status,
                    CodigoPagamento = c.CodigoPagamento
                })
                .ToList()
        };
    }

    private static string GerarCodigoPagamento(MetodoPagamento metodo)
    {
        return metodo switch
        {
            MetodoPagamento.BOLETO => $"BOL-{Guid.NewGuid():N}".ToUpperInvariant(),
            MetodoPagamento.PIX => $"PIX-{Guid.NewGuid():N}".ToUpperInvariant(),
            _ => $"PG-{Guid.NewGuid():N}".ToUpperInvariant()
        };
    }
}