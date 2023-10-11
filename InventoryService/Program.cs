using Contracts.Constants;
using InventoryService.Consumers.Events;
using InventoryService.Consumers.Messages;
using InventoryService.Database;
using InventoryService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<InventoryDbContext>(config => config.UseSqlServer(builder.Configuration.GetConnectionString("InventoryDbConnection"),
         options => options.MigrationsHistoryTable("InventoryMigrations", "INV")));

    builder.Services.AddMassTransit(busConfigure =>
    {
        busConfigure.SetKebabCaseEndpointNameFormatter();
        busConfigure.AddConsumer<OrderCreatedEventConsumer>();
        busConfigure.AddConsumer<StockRollBackMessageConsumer>();

        busConfigure.UsingRabbitMq((context, configurator) =>
        {
            configurator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), host =>
            {
                host.Username(builder.Configuration["MessageBroker:Username"]);
                host.Password(builder.Configuration["MessageBroker:Password"]);
            });

            configurator.ReceiveEndpoint(QueuesConstants.OrderCreatedEventQueueName, e =>
            {
                e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
            });

            configurator.ReceiveEndpoint(QueuesConstants.StockRollBackMessageQueueName, e =>
            {
                e.ConfigureConsumer<StockRollBackMessageConsumer>(context);
            });
        });

    });

    builder.Services.AddScoped<IMassTransitService, MassTransitService>();

    var app = builder.Build();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    await app.PopulateDatabasePreparation();
    app.Run();
    return 0;

}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}