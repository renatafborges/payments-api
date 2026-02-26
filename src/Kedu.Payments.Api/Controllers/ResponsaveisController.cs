using Microsoft.AspNetCore.Mvc;
using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Entities;

namespace Kedu.Payments.Api.Controllers;

[ApiController]
[Route("responsaveis")]
public class ResponsaveisController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResponsaveisController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CriarResponsavel([FromBody] ResponsavelFinanceiro request)
    {
        _context.Responsaveis.Add(request);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = request.Id }, request);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var responsavel = await _context.Responsaveis.FindAsync(id);

        if (responsavel == null)
            return NotFound();

        return Ok(responsavel);
    }
}