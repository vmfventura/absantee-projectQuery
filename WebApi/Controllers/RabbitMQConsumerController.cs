using System.Text;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Application.DTO;
using Application.Services;

namespace WebApi.Controllers;

public class RabbitMQConsumerController : IRabbitMQConsumerController
{
    private readonly IConnectionFactory _connectionFactory;
    private IConnection _connection;
    private IModel _channel;
    private string _queueName;
    private string nameProject;
    private readonly ProjectService _projectService;
    List<string> _errorMessages = new List<string>();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RabbitMQConsumerController(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _connectionFactory = connectionFactory;
        nameProject = "project_create";
    }

    public void ConfigQueue(string queueName)
    {
        _queueName = queueName;
        _connection = _connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: nameProject, type: ExchangeType.Fanout);

        _channel.QueueDeclare(queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(queue: _queueName,
            exchange: nameProject,
            routingKey: string.Empty);
    }

    public void StartConsuming()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            ProjectDTO deserializedObject = ProjectGatewayDTO.ToDTO(message);
            Console.WriteLine($" [x] Received {deserializedObject}");
            Console.WriteLine($" [x] Start checking if exists.");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var projectService = scope.ServiceProvider.GetRequiredService<ProjectService>();

                ProjectDTO projectResultDTO = projectService.AddFromAMQP(deserializedObject, _errorMessages).Result;
            }
        };
        _channel.BasicConsume(queue: _queueName,
            autoAck: true,
            consumer: consumer);
    }

    public void StopConsuming()
    {
        _channel.Close();
        _connection.Close();
    }
}
