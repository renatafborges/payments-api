using Kedu.Payments.Api.Domain.Enums;

namespace Kedu.Payments.Api.Contracts.Responses;

public class CobrancaDetalhadaResponse
{
    public int Id { get; set; }
    public int PlanoPagamentoId { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public MetodoPagamento MetodoPagamento { get; set; }
    public string CodigoPagamento { get; set; } = string.Empty;
    public StatusCobranca Status { get; set; }
    public bool Vencida { get; set; }
}