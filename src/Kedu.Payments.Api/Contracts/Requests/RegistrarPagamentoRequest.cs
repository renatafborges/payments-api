namespace Kedu.Payments.Api.Contracts.Requests;

public class RegistrarPagamentoRequest
{
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; }
}