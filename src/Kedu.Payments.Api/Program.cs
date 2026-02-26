using Microsoft.EntityFrameworkCore;
using Kedu.Payments.Api.Data;
using System.Text.Json.Serialization;
using Kedu.Payments.Api.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

app.MapControllers();

app.MapGraphQL("/graphql");

app.Run();