using Microsoft.EntityFrameworkCore;
using Kedu.Payments.Api.Domain.Entities;

namespace Kedu.Payments.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<ResponsavelFinanceiro> Responsaveis => Set<ResponsavelFinanceiro>();
    public DbSet<PlanoPagamento> Planos => Set<PlanoPagamento>();
    public DbSet<Cobranca> Cobrancas => Set<Cobranca>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();
    public DbSet<CentroDeCusto> CentrosDeCusto => Set<CentroDeCusto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ResponsavelFinanceiro>()
            .HasMany(r => r.PlanosDePagamento)
            .WithOne(p => p.Responsavel)
            .HasForeignKey(p => p.ResponsavelId);

        modelBuilder.Entity<PlanoPagamento>()
            .HasMany(p => p.Cobrancas)
            .WithOne(c => c.PlanoPagamento)
            .HasForeignKey(c => c.PlanoPagamentoId);

        modelBuilder.Entity<Cobranca>()
            .HasMany(c => c.Pagamentos)
            .WithOne(p => p.Cobranca)
            .HasForeignKey(p => p.CobrancaId);

        modelBuilder.Entity<PlanoPagamento>()
            .HasOne(p => p.CentroDeCusto)
            .WithMany()
            .HasForeignKey(p => p.CentroDeCustoId);

        modelBuilder.Entity<Cobranca>()
            .Property(c => c.DataVencimento)
            .HasColumnType("timestamp without time zone");

        modelBuilder.Entity<Pagamento>()
            .Property(p => p.DataPagamento)
            .HasColumnType("timestamp without time zone");
    }
}