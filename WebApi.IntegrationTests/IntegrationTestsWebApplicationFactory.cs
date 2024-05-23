using System.Data.Common;
using DataModel.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApi.Controllers;

namespace WebApi.IntegrationTests;

public class IntegrationTestsWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, confBuilder) =>
        {
            // Set up the configuration to read from the command line or environment variable
            var environmentVariableArg = Environment.GetEnvironmentVariable("Arg");
            if (string.IsNullOrEmpty(environmentVariableArg))
            {
                throw new ArgumentNullException(nameof(environmentVariableArg), "Environment variable 'Arg' cannot be null or empty.");
            }

            var args = new string[] { environmentVariableArg };
            confBuilder.AddCommandLine(args);
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove the AppDbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AbsanteeContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remove the database connection registration
            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            // Add SQLite in-memory database
            services.AddSingleton<DbConnection>(container =>
            {
                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();
                return connection;
            });

            services.AddDbContext<AbsanteeContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });

            // Register required RabbitMQ services
            // services.AddSingleton<IRabbitMQConsumerController, RabbitMQConsumerController>();
            // services.AddSingleton<IRabbitMQConsumerUpdateController, RabbitMQConsumerUpdateController>();
        });

        builder.UseSetting("launchProfile", "https1");
        builder.UseEnvironment("Development");
    }
}
