namespace Kedu.Payments.Api.Domain.Entities;

public class ResponsavelFinanceiro
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public ICollection<PlanoPagamento> PlanosDePagamento { get; set; } = new List<PlanoPagamento>();
}