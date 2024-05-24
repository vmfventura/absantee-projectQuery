using DotNet.Testcontainers.Builders;
using Testcontainers.RabbitMq;

namespace WebApi.IntegrationTests.Fixtures;

public class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _rabbitMqContainer;

    public RabbitMqFixture()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.11-management")
            .WithPortBinding(5672, 5672)
            .WithPortBinding(15672, 15672)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
            .Build();
    }

    public string GetConnectionString()
    {
        return $"amqp://guest:guest@localhost:{_rabbitMqContainer.GetMappedPublicPort(5672)}";
    }

    public async Task InitializeAsync()
    {
        await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.StopAsync();
    }
}
