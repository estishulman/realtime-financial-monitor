using System.Threading.Channels;
using FinancialMonitor.Api.Application.Abstractions;
using FinancialMonitor.Api.Application.Transactions;
using FinancialMonitor.Api.Infrastructure.Persistence;
using FinancialMonitor.Api.Infrastructure.Realtime;
using FinancialMonitor.Api.Domain.Entities;
using FinancialMonitor.Api.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FinancialMonitor.Api.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "FrontendCors";

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
	options.AddPolicy(frontendCorsPolicy, policy =>
	{
		policy.WithOrigins(
			"http://localhost:5173",
			"http://localhost:8081")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});
var databaseProvider = builder.Configuration["Database:Provider"]?.ToLowerInvariant() ?? "sqlite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
	?? "Data Source=financial-monitor.db";
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddDbContextFactory<FinancialMonitorDbContext>(options =>
	_ = databaseProvider switch
	{
		"postgres" => options.UseNpgsql(connectionString),
		"sqlite" => options.UseSqlite(connectionString),
		_ => throw new InvalidOperationException("Database:Provider must be Sqlite or Postgres.")
	});
builder.Services.AddSingleton<EfTransactionRepository>();
builder.Services.AddMemoryCache();

var cacheProvider = builder.Configuration["Cache:Provider"]?.ToLowerInvariant() ?? "memory";
if (cacheProvider == "redis")
{
	builder.Services.AddStackExchangeRedisCache(options =>
	{
		options.Configuration = redisConnectionString;
		options.InstanceName = "FinancialMonitor:";
	});
	builder.Services.AddSingleton<ITransactionCache, RedisTransactionCache>();
}
else if (cacheProvider == "memory")
{
	builder.Services.AddSingleton<ITransactionCache, MemoryTransactionCache>();
}
else
{
	throw new InvalidOperationException("Cache:Provider must be Memory or Redis.");
}
builder.Services.AddSingleton<ITransactionRepository>(services =>
	new CachedTransactionRepository(
		services.GetRequiredService<EfTransactionRepository>(),
		services.GetRequiredService<ITransactionCache>()));
builder.Services.AddSingleton<ITransactionService, TransactionService>();
builder.Services.AddSingleton<ITransactionProcessor, TransactionProcessor>();
builder.Services.AddSingleton(Channel.CreateUnbounded<Transaction>(new UnboundedChannelOptions
{
	SingleReader = true,
	SingleWriter = false,
	AllowSynchronousContinuations = false
}));
var signalRBuilder = builder.Services.AddSignalR();
builder.Services.AddHostedService<TransactionBroadcasterService>();

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
	signalRBuilder.AddStackExchangeRedis(redisConnectionString);
}

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

app.UseCors(frontendCorsPolicy);
app.MapControllers();
app.MapHub<TransactionHub>("/hubs/transactions");

app.Run();

public partial class Program;
