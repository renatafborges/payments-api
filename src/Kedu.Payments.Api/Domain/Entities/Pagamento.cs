namespace Kedu.Payments.Api.Domain.Entities;

public class Pagamento
{
    public int Id { get; set; }

    public int CobrancaId { get; set; }
    public Cobranca Cobranca { get; set; } = null!;

    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; }
}