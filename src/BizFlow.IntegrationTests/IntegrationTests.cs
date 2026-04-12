using ClientBizFlow_attemp_1;
using ClientBizFlow_attemp_1.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.DependencyInjection;


[TestFixture]
public class IntegrationTests : IDisposable
{
    private WebApplicationFactory<Program> _factory;
    private PostgreSqlContainer _postgreSqlContainer;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Создаём контейнер PostgreSQL
        _postgreSqlContainer = new PostgreSqlBuilder("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        // Запускаем контейнер
        await _postgreSqlContainer.StartAsync();

        // Создаём WebApplicationFactory с переопределённой строкой подключения
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Переопределяем строку подключения к БД
                    var connectionString = _postgreSqlContainer.GetConnectionString();
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(connectionString));
                });
            });
    }

    [Test]
    public async Task GetUsers_ShouldReturnUsers()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_factory != null)
        {
            _factory.Dispose();
        }

        if (_postgreSqlContainer != null)
        {
            // Останавливаем и удаляем контейнер
            await _postgreSqlContainer.StopAsync();
        }
    }

    public void Dispose()
    {
        //throw new NotImplementedException();
    }
}

