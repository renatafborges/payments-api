using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kedu.Payments.Api.Controllers;

[ApiController]
[Route("centros-de-custo")]
public class CentrosDeCustoController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CentroDeCusto request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
            return BadRequest("Nome é obrigatório.");

        var nome = request.Nome.Trim().ToUpperInvariant();
        var existe = await _context.CentrosDeCusto.AnyAsync(x => x.Nome == nome);
        if (existe) return Conflict("Centro de custo já existe.");

        var centro = new CentroDeCusto { Nome = nome };
        _context.CentrosDeCusto.Add(centro);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = centro.Id }, centro);
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
        => Ok(await _context.CentrosDeCusto.AsNoTracking().OrderBy(x => x.Id).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var centro = await _context.CentrosDeCusto.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return centro is null ? NotFound() : Ok(centro);
    }
}