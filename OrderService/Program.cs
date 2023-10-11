using Contracts.Constants;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Consumers;
using OrderService.Database;
using OrderService.Services;
using Serilog;

var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateBootstrapLogger();
try
{
    Log.Information("starting web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<OrderDbContext>(config => config.UseSqlServer(builder.Configuration.GetConnectionString("OrderDbConnection"),
         options => options.MigrationsHistoryTable("OrderMigrations", "ORDERS")));

    builder.Services.AddMassTransit(busConfigure =>
    {
        busConfigure.SetKebabCaseEndpointNameFormatter();

        busConfigure.AddConsumer<OrderCompletedEventConsumer>();
        busConfigure.AddConsumer<OrderFailedEventConsumer>();

        busConfigure.UsingRabbitMq((context, configurator) =>
        {
            configurator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), host =>
            {
                host.Username(builder.Configuration["MessageBroker:Username"]);
                host.Password(builder.Configuration["MessageBroker:Password"]);
            });


            configurator.ReceiveEndpoint(QueuesConstants.OrderCompletedEventQueueName, x =>
            {
                x.ConfigureConsumer<OrderCompletedEventConsumer>(context);
            });

            configurator.ReceiveEndpoint(QueuesConstants.OrderFailedEventQueueName, x =>
            {
                x.ConfigureConsumer<OrderFailedEventConsumer>(context);
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