using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Entities;
using Kedu.Payments.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kedu.Payments.Api.GraphQL;

public class Query
{
    public IQueryable<CentroDeCusto> CentrosDeCusto([Service] AppDbContext db)
        => db.CentrosDeCusto.AsNoTracking();

    public Task<ResponsavelFinanceiro?> Responsavel(int id, [Service] AppDbContext db)
        => db.Responsaveis
            .AsNoTracking()
            .Include(r => r.PlanosDePagamento)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<PlanoPagamento?> PlanoPagamento(int id, [Service] AppDbContext db)
        => db.Planos
            .AsNoTracking()
            .Include(p => p.CentroDeCusto)
            .Include(p => p.Cobrancas)
            .FirstOrDefaultAsync(p => p.Id == id);

    public IQueryable<Cobranca> Cobrancas([Service] AppDbContext db)
    => db.Cobrancas;

    public IQueryable<Cobranca> CobrancasVencidas([Service] AppDbContext db)
    {
        var hoje = DateTime.Now.Date;

        return db.Cobrancas
            .Where(c =>
                hoje > c.DataVencimento.Date &&
                c.Status != StatusCobranca.PAGA &&
                c.Status != StatusCobranca.CANCELADA);
    }
}