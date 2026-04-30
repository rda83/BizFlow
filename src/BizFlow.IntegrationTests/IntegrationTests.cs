using BizFlow.Core.Internal.Features.AddPipeline;
using BizFlow.Core.Services.DI;
using BizFlow.Storage.PostgreSQL;
using ClientBizFlow_attemp_1;
using ClientBizFlow_attemp_1.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using NUnit.Framework;
using Quartz;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;

[TestFixture]
public class IntegrationTests : IDisposable
{
    private WebApplicationFactory<Program> app;
    private PostgreSqlContainer _postgreSqlContainer;

    [OneTimeSetUp]
    public async Task OneTimeSetUp() { }

    [Test]
    public async Task GetUsers_ShouldReturnUsers()
    {

        // Создаём контейнер PostgreSQL
        _postgreSqlContainer = new PostgreSqlBuilder("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();

        // Запускаем контейнер
        await _postgreSqlContainer.StartAsync();

        var script = await File.ReadAllTextAsync("C:\\reps\\BizFlow\\src\\BizFlow.Storage.PostgreSQL\\v1_initial_schema.sql");
        await _postgreSqlContainer.ExecScriptAsync(script);

        var fakeTime = new FakeTimeProvider();
        fakeTime.SetUtcNow(new DateTimeOffset(2026, 4, 29, 1, 0, 0, TimeSpan.Zero));


        app = new WebApplicationFactory<Program>()
           .WithWebHostBuilder(builder =>
           {
               builder.ConfigureServices(services =>
               {
                   Quartz.SystemTime.UtcNow = () => fakeTime.GetUtcNow();
                   Quartz.SystemTime.Now = () => fakeTime.GetLocalNow();

                   services.RemoveAll<TimeProvider>();
                   services.AddSingleton<TimeProvider>(fakeTime);

                   var connectionString = _postgreSqlContainer.GetConnectionString();

                   services.AddDbContext<AppDbContext>(options =>
                       options.UseNpgsql(connectionString));

                   services.AddPostgreSQLBizFlowStorage(connectionString);
                   services.AddBizFlow(typeof(Program).Assembly);
               });
           });

      

        // Arrange
        var client = app.CreateClient();

        var optionsJson = "{\"param1\": \"value\", \"param2\": 42}";
        var optionsElement = JsonDocument.Parse(optionsJson).RootElement;

        var newItem = new AddPipelineCommand()
        { 
            Name = "TestPipeline",
            Blocked = false,
            CronExpression = "0 1 8 ? * * *",
            Description = string.Empty,
            PipelineItems = new List<AddPipelineItemCommand>() 
            {
                new AddPipelineItemCommand()
                {
                    SortOrder = 0,
                    Blocked = false,
                    Description = string.Empty,
                    Options = optionsElement,
                    TypeOperationId = "FirstOperation"
                }
            }
        
        };

        var json = JsonContent.Create(newItem);

        // Act

        try
        {
            var response = await client.PostAsync("/bizFlow/pipeline", json);
        }
        catch (Exception ex)
        {

            throw;
        }

        fakeTime.Advance(TimeSpan.FromHours(10));
        await Task.Delay(50000);

        // Assert
        //Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (app != null)
        {
            app.Dispose();
        }

        if (_postgreSqlContainer != null)
        {
            // Останавливаем и удаляем контейнер
            await _postgreSqlContainer.StopAsync();
        }
    }

    public void Dispose()
    {
        //await Container.DisposeAsync();
        //throw new NotImplementedException();
    }
}

