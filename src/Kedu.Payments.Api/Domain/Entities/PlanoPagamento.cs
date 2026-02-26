using Kedu.Payments.Api.Domain.Enums;

namespace Kedu.Payments.Api.Domain.Entities;

public class PlanoPagamento
{
    public int Id { get; set; }

    public int ResponsavelId { get; set; }
    public ResponsavelFinanceiro Responsavel { get; set; } = null!;

    public int CentroDeCustoId { get; set; }
    public CentroDeCusto CentroDeCusto { get; set; } = null!;

    public decimal Total { get; set; }

    public ICollection<Cobranca> Cobrancas { get; set; } = new List<Cobranca>();
}