using Kedu.Payments.Api.Domain.Enums;

namespace Kedu.Payments.Api.Contracts.Responses;

public class PlanoPagamentoResponse
{
    public int Id { get; set; }
    public int ResponsavelId { get; set; }
    public int CentroDeCustoId { get; set; }
    public string CentroDeCusto { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<CobrancaResponse> Cobrancas { get; set; } = new();
}

public class CobrancaResponse
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public MetodoPagamento MetodoPagamento { get; set; }
    public StatusCobranca Status { get; set; }
    public string CodigoPagamento { get; set; } = string.Empty;
}