using Kedu.Payments.Api.Domain.Enums;

namespace Kedu.Payments.Api.Domain.Entities;

public class Cobranca
{
    public int Id { get; set; }

    public int PlanoPagamentoId { get; set; }
    public PlanoPagamento PlanoPagamento { get; set; } = null!;

    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }

    public MetodoPagamento MetodoPagamento { get; set; }
    public StatusCobranca Status { get; set; }

    public string CodigoPagamento { get; set; } = string.Empty;

    public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
}