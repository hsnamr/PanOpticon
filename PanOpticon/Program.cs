using PanOpticon;
using PanOpticon.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PanOpticonOptions>(builder.Configuration.GetSection("PanOpticon"));
builder.Services.AddSingleton<RedisJobStore>();
builder.Services.AddSingleton<KafkaPublisher>();
builder.Services.AddSingleton<MinioStorage>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ReferenceDataService>();
builder.Services.AddScoped<JobOrchestrationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PanOpticon", Version = "1.0.0" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PanOpticon v1"));

app.MapControllers();

app.MapGet("/", () => new { service = "PanOpticon", version = "1.0.0", docs = "/swagger" });
app.MapGet("/health", () => new { status = "ok" });

var redis = app.Services.GetRequiredService<RedisJobStore>();
await redis.ConnectAsync();

app.Lifetime.ApplicationStopping.Register(() =>
{
    redis.Close();
    app.Services.GetRequiredService<KafkaPublisher>().Close();
});

app.Run();
