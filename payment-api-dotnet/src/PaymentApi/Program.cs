using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PaymentApi.Errors;
using PaymentApi.Integration;
using PaymentApi.Options;
using PaymentApi.Persistence;
using PaymentApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Controladores + formato de error propio para 400 automáticos.
builder.Services
    .AddControllers(options => options.Filters.Add<ApiExceptionFilter>())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressMapClientErrors = true;
        options.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Create;
    });

// Swagger / OpenAPI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Requests API",
        Version = "v1",
        Description = "Registro y consulta de solicitudes de pago. La comisión se calcula en el servicio Java."
    });
});

// Configuración del servicio Java (URL y timeout externalizados), validada al iniciar.
builder.Services
    .AddOptions<FeeServiceOptions>()
    .Bind(builder.Configuration.GetSection(FeeServiceOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Cliente HTTP tipado hacia el servicio Java, con timeout finito.
builder.Services.AddHttpClient<IFeeCalculationClient, HttpFeeCalculationClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<FeeServiceOptions>>().Value;
    var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// Almacenamiento en memoria (Singleton: se comparte durante toda la ejecución) y lógica de negocio.
builder.Services.AddSingleton<IPaymentRequestRepository, InMemoryPaymentRequestRepository>();
builder.Services.AddScoped<IPaymentRequestService, PaymentRequestService>();

var app = builder.Build();

app.UseApiErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

// Permite referenciar Program desde pruebas de integración.
public partial class Program
{
}