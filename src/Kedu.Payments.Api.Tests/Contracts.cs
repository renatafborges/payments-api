using System.Text.Json.Serialization;

namespace Kedu.Payments.Api.Tests;

public class ResponsavelResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
}

public class PlanoResponse
{
    public int Id { get; set; }
    public int ResponsavelId { get; set; }
    public int CentroDeCustoId { get; set; }
    public decimal Total { get; set; }
    public List<CobrancaResponse> Cobrancas { get; set; } = new();
}

public class CobrancaResponse
{
    public int Id { get; set; }
    public int PlanoPagamentoId { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public string Status { get; set; } = "";
    public string MetodoPagamento { get; set; } = "";
    public string CodigoPagamento { get; set; } = "";
    public bool Vencida { get; set; }
}