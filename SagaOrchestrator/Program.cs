using Contracts.Constants;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using SagaOrchestrator.Database;
using SagaOrchestrator.Entities;
using SagaOrchestrator.StateMachines;
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
    var connectionString = builder.Configuration.GetConnectionString("SagaDbConnection");

    builder.Services.AddDbContext<SagaStateMachineDbContext>(config =>
    {
        config.EnableDetailedErrors();
        config.EnableSensitiveDataLogging();
        config.UseSqlServer(connectionString, options => options.MigrationsHistoryTable("SagaMigrations", "SAGAS"));
    });

    builder.Services.AddMassTransit(busConfigure =>
    {
        busConfigure.AddLogging(c => c.AddDebug());

        busConfigure.SetKebabCaseEndpointNameFormatter();

        busConfigure.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
        .EntityFrameworkRepository(options =>
        {
            options.ExistingDbContext<SagaStateMachineDbContext>();
            options.LockStatementProvider = new SqlServerLockStatementProvider();
        });



        busConfigure.UsingRabbitMq((context, configurator) =>
        {
            configurator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), host =>
            {
                host.Username(builder.Configuration["MessageBroker:Username"]);
                host.Password(builder.Configuration["MessageBroker:Password"]);
            });

            configurator.ReceiveEndpoint(QueuesConstants.CreateOrderMessageQueueName, e => e.ConfigureSaga<OrderStateInstance>(context));
        });

    });

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