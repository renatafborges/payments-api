using Kedu.Payments.Api.Contracts.Requests;
using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Entities;
using Kedu.Payments.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kedu.Payments.Api.Controllers;

[ApiController]
[Route("cobrancas/{cobrancaId:int}/pagamentos")]
public class PagamentosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PagamentosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar(int cobrancaId, [FromBody] RegistrarPagamentoRequest request)
    {
        if (request.Valor <= 0)
            return BadRequest("Valor do pagamento deve ser maior que zero.");

        var cobranca = await _context.Cobrancas
            .Include(c => c.Pagamentos)
            .FirstOrDefaultAsync(c => c.Id == cobrancaId);

        if (cobranca == null)
            return NotFound($"Cobrança {cobrancaId} não encontrada.");

        if (cobranca.Status == StatusCobranca.CANCELADA)
            return UnprocessableEntity("Não é permitido pagar uma cobrança CANCELADA.");

        var pagamento = new Pagamento
        {
            CobrancaId = cobrancaId,
            Valor = request.Valor,
            DataPagamento = request.DataPagamento == default ? DateTime.Now : request.DataPagamento
        };

        cobranca.Pagamentos.Add(pagamento);
        cobranca.Status = StatusCobranca.PAGA;

        await _context.SaveChangesAsync();

        return Ok(new { cobrancaId, status = cobranca.Status, pagamentoId = pagamento.Id });
    }
}