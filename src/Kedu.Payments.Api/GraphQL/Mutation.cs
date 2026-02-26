using HotChocolate;
using HotChocolate.Execution;
using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Entities;
using Kedu.Payments.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kedu.Payments.Api.GraphQL;

public class Mutation
{
    public async Task<RegistrarPagamentoPayload> RegistrarPagamento(
        int cobrancaId,
        decimal valor,
        [Service] AppDbContext db)
    {
        var cobranca = await db.Cobrancas
            .Include(c => c.Pagamentos)
            .FirstOrDefaultAsync(c => c.Id == cobrancaId);

        if (cobranca is null)
            throw new GraphQLException("Cobrança não encontrada.");

        if (cobranca.Status == StatusCobranca.CANCELADA)
            throw new GraphQLException("Não é permitido pagar cobrança cancelada.");

        if (valor <= 0) throw new GraphQLException("Valor inválido.");

        if (valor != cobranca.Valor)
            throw new GraphQLException("Pagamento parcial não é permitido. O valor deve ser igual ao valor da cobrança.");

        cobranca.Pagamentos.Add(new Pagamento
        {
            Valor = valor,
            DataPagamento = DateTime.Now
        });

        cobranca.Status = StatusCobranca.PAGA;
        await db.SaveChangesAsync();

        return new RegistrarPagamentoPayload
        {
            CobrancaId = cobranca.Id,
            Status = cobranca.Status.ToString()
        };
    }
}

public class RegistrarPagamentoPayload
{
    public int CobrancaId { get; set; }
    public string Status { get; set; } = "";
}