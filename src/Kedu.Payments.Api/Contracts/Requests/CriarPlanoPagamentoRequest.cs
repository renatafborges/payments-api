using Kedu.Payments.Api.Domain.Enums;

namespace Kedu.Payments.Api.Contracts.Requests;

public class CriarPlanoPagamentoRequest
{
    public int ResponsavelId { get; set; }
    public int CentroDeCustoId { get; set; }
    public List<CriarCobrancaRequest> Cobrancas { get; set; } = new();
}

public class CriarCobrancaRequest
{
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public MetodoPagamento MetodoPagamento { get; set; }
}