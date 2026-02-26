using System.Net;
using System.Net.Http.Json;
using Kedu.Payments.Api.Data;
using Kedu.Payments.Api.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Kedu.Payments.Api.Tests;

public class FluxoDePagamentoTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public FluxoDePagamentoTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<int> EnsureCentroDeCustoAsync(string nome)
    {
        var createResp = await _client.PostAsJsonAsync("/centros-de-custo", new { nome });

        if (createResp.StatusCode != HttpStatusCode.Created &&
            createResp.StatusCode != HttpStatusCode.Conflict)
        {
            var body = await createResp.Content.ReadAsStringAsync();
            throw new Exception($"Falha ao garantir centro de custo '{nome}'. Status={createResp.StatusCode}. Body={body}");
        }

        var listResp = await _client.GetAsync("/centros-de-custo");
        listResp.EnsureSuccessStatusCode();

        var centros = await listResp.Content.ReadFromJsonAsync<List<CentroDeCustoItem>>();
        var centro = centros!.FirstOrDefault(c => c.Nome == nome);

        if (centro is null)
            throw new Exception($"Centro de custo '{nome}' não encontrado após criação/listagem.");

        return centro.Id;
    }

    private sealed class CentroDeCustoItem
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
    }

    private static async Task<ResponsavelResponse> CreateResponsavelAsync(HttpClient client, string nome)
    {
        var resp = await client.PostAsJsonAsync("/responsaveis", new { nome });
        resp.EnsureSuccessStatusCode();

        var responsavel = await resp.Content.ReadFromJsonAsync<ResponsavelResponse>();
        Assert.NotNull(responsavel);
        Assert.True(responsavel!.Id > 0);

        return responsavel!;
    }

    [Fact]
    public async Task CriarPlano_CalculaTotal_GeraCodigos_StatusEmitida()
    {
        var centroId = await EnsureCentroDeCustoAsync("MATRICULA");
        var responsavel = await CreateResponsavelAsync(_client, "Maria Silva");

        var payload = new
        {
            responsavelId = responsavel.Id,
            centroDeCustoId = centroId,
            cobrancas = new[]
            {
                new { valor = 350.00m, dataVencimento = "2026-03-10", metodoPagamento = "BOLETO" },
                new { valor = 350.00m, dataVencimento = "2026-04-10", metodoPagamento = "PIX" }
            }
        };

        var respPlano = await _client.PostAsJsonAsync("/planos-de-pagamento", payload);

        Assert.Equal(HttpStatusCode.Created, respPlano.StatusCode);

        var plano = await respPlano.Content.ReadFromJsonAsync<PlanoResponse>();
        Assert.NotNull(plano);

        Assert.Equal(700.00m, plano!.Total);
        Assert.Equal(2, plano.Cobrancas.Count);

        Assert.All(plano.Cobrancas, c => Assert.Equal("EMITIDA", c.Status));

        Assert.StartsWith("BOL-", plano.Cobrancas[0].CodigoPagamento);
        Assert.StartsWith("PIX-", plano.Cobrancas[1].CodigoPagamento);
    }

    [Fact]
    public async Task RegistrarPagamento_AlteraStatusParaPaga()
    {
        var centroId = await EnsureCentroDeCustoAsync("MENSALIDADE");
        var responsavel = await CreateResponsavelAsync(_client, "João");

        var respPlano = await _client.PostAsJsonAsync("/planos-de-pagamento", new
        {
            responsavelId = responsavel.Id,
            centroDeCustoId = centroId,
            cobrancas = new[] { new { valor = 100.00m, dataVencimento = "2026-03-10", metodoPagamento = "BOLETO" } }
        });
        respPlano.EnsureSuccessStatusCode();

        var plano = await respPlano.Content.ReadFromJsonAsync<PlanoResponse>();
        Assert.NotNull(plano);
        var cobrancaId = plano!.Cobrancas.Single().Id;

        var respPagar = await _client.PostAsJsonAsync($"/cobrancas/{cobrancaId}/pagamentos", new
        {
            valor = 100.00m,
            dataPagamento = "2026-02-25T20:00:00"
        });
        respPagar.EnsureSuccessStatusCode();

        var respCobrancas = await _client.GetAsync($"/responsaveis/{responsavel.Id}/cobrancas");
        respCobrancas.EnsureSuccessStatusCode();

        var cobrancas = await respCobrancas.Content.ReadFromJsonAsync<List<CobrancaResponse>>();
        Assert.NotNull(cobrancas);

        var c1 = cobrancas!.Single(x => x.Id == cobrancaId);
        Assert.Equal("PAGA", c1.Status);
        Assert.False(c1.Vencida);
    }

    [Fact]
    public async Task RegistrarPagamento_EmCobrancaCancelada_Retorna422()
    {
        var centroId = await EnsureCentroDeCustoAsync("MATERIAL");
        var responsavel = await CreateResponsavelAsync(_client, "Ana");

        var respPlano = await _client.PostAsJsonAsync("/planos-de-pagamento", new
        {
            responsavelId = responsavel.Id,
            centroDeCustoId = centroId,
            cobrancas = new[] { new { valor = 50.00m, dataVencimento = "2026-03-10", metodoPagamento = "PIX" } }
        });
        respPlano.EnsureSuccessStatusCode();

        var plano = await respPlano.Content.ReadFromJsonAsync<PlanoResponse>();
        Assert.NotNull(plano);
        var cobrancaId = plano!.Cobrancas.Single().Id;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cobranca = await db.Cobrancas.FindAsync(cobrancaId);
            Assert.NotNull(cobranca);

            cobranca!.Status = StatusCobranca.CANCELADA;
            await db.SaveChangesAsync();
        }

        var respPagar = await _client.PostAsJsonAsync($"/cobrancas/{cobrancaId}/pagamentos", new
        {
            valor = 50.00m,
            dataPagamento = "2026-02-25T20:00:00"
        });

        Assert.Equal((HttpStatusCode)422, respPagar.StatusCode);
    }

    [Fact]
    public async Task ListarCobrancas_CalculaVencida_QuandoDataPassouEStatusNaoPagaNemCancelada()
    {
        var centroId = await EnsureCentroDeCustoAsync("MENSALIDADE");
        var responsavel = await CreateResponsavelAsync(_client, "Paulo");

        var dataPassada = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");

        var respPlano = await _client.PostAsJsonAsync("/planos-de-pagamento", new
        {
            responsavelId = responsavel.Id,
            centroDeCustoId = centroId,
            cobrancas = new[] { new { valor = 80.00m, dataVencimento = dataPassada, metodoPagamento = "BOLETO" } }
        });
        respPlano.EnsureSuccessStatusCode();

        var plano = await respPlano.Content.ReadFromJsonAsync<PlanoResponse>();
        Assert.NotNull(plano);
        var cobrancaId = plano!.Cobrancas.Single().Id;

        var respCobrancas = await _client.GetAsync($"/responsaveis/{responsavel.Id}/cobrancas");
        respCobrancas.EnsureSuccessStatusCode();

        var cobrancas = await respCobrancas.Content.ReadFromJsonAsync<List<CobrancaResponse>>();
        Assert.NotNull(cobrancas);

        var c1 = cobrancas!.Single(x => x.Id == cobrancaId);

        Assert.Equal("EMITIDA", c1.Status);
        Assert.True(c1.Vencida);
    }

    [Fact]
    public async Task CentrosDeCusto_Criar_Listar_E_DuplicadoRetornaConflict()
    {
        var criarCentroDeCusto = await _client.PostAsJsonAsync("/centros-de-custo", new { nome = "TAXA_DE_PROVA" });
        Assert.True(criarCentroDeCusto.StatusCode == HttpStatusCode.Created || criarCentroDeCusto.StatusCode == HttpStatusCode.Conflict);

        var listaCentrosDeCusto = await _client.GetAsync("/centros-de-custo");
        listaCentrosDeCusto.EnsureSuccessStatusCode();

        var centros = await listaCentrosDeCusto.Content.ReadFromJsonAsync<List<CentroDeCustoItem>>();
        Assert.NotNull(centros);
        Assert.Contains(centros!, c => c.Nome == "TAXA_DE_PROVA");

        var criarCentroDeCustoDuplicado = await _client.PostAsJsonAsync("/centros-de-custo", new { nome = "TAXA_DE_PROVA" });
        Assert.Equal(HttpStatusCode.Conflict, criarCentroDeCustoDuplicado.StatusCode);
    }
}