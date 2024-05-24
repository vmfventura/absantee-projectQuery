using Microsoft.EntityFrameworkCore;
using Application.Services;
using DataModel.Repository;
using DataModel.Mapper;
using Domain.Factory;
using Domain.IRepository;
using Gateway;
using RabbitMQ.Client;
using WebApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
string replicaNameArg = Array.Find(args, arg => arg.Contains("replicaName"));
string replicaName;

if (replicaNameArg != null)
{
    replicaName = replicaNameArg.Split('=')[1];
}
else
{
    replicaName = config.GetConnectionString("replicaName") ?? "Repl1";
}

var connectionStringName = "AbsanteeDatabase" + replicaName;
var connectionString = config.GetConnectionString(connectionStringName);

var projectQueueNameConfig = "ProjectQueues:" + replicaName;
var projectQueueName = config[projectQueueNameConfig] ?? "DefaultProjectQueue";

var projectUpdateQueueNameConfig = "ProjectUpdateQueues:" + replicaName;
var projectUpdateQueueName = config[projectUpdateQueueNameConfig] ?? "DefaultProjectUpdateQueue";

builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") });

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddDbContext<AbsanteeContext>(opt =>
    opt.UseSqlite(connectionString)
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IProjectRepository, ProjectRepository>();
builder.Services.AddTransient<IProjectFactory, ProjectFactory>();
builder.Services.AddTransient<ProjectMapper>();
builder.Services.AddTransient<ProjectService>();
builder.Services.AddTransient<IMessagePublisher, ProjectGateway>();
builder.Services.AddTransient<IMessageUpdatePublisher, ProjectGatewayUpdate>();

builder.Services.AddScoped<ProjectService>();

builder.Services.AddSingleton<IRabbitMQConsumerController, RabbitMQConsumerController>();
builder.Services.AddSingleton<IRabbitMQConsumerUpdateController, RabbitMQConsumerUpdateController>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();


var rabbitMQConsumerService = app.Services.GetRequiredService<IRabbitMQConsumerController>();
rabbitMQConsumerService.ConfigQueue(projectQueueName);
rabbitMQConsumerService.StartConsuming();

var rabbitMQConsumerUpdateService = app.Services.GetRequiredService<IRabbitMQConsumerUpdateController>();
rabbitMQConsumerUpdateService.ConfigQueue(projectUpdateQueueName);
rabbitMQConsumerUpdateService.StartConsuming();

app.MapControllers();
app.Run();

// Partial class for testing purposes
public partial class Program { }
