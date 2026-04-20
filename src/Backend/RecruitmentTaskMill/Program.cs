using Backend.Interfaces;
using Backend.Services;
using Backend.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Options
builder.Services.Configure<DataGenerationOptions>(builder.Configuration.GetSection("DataGeneration"));

builder.Services.AddMemoryCache();

// Services
builder.Services.AddSingleton<IClientIdProvider, ClientIdProvider>();
builder.Services.AddSingleton<IRequestQueue, RequestQueue>();
builder.Services.AddSingleton<IDataRequestService, DataRequestService>();
builder.Services.AddSingleton<IDataGenerationService, DataGenerationService>();

builder.Services.AddHostedService<RequestWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();