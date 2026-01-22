using System.Text;
using System.Text.Json;
using AuditApi.Data;
using AuditApi.DTOs;
using AuditApi.mappers;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuditApi.Services;

public class RabbitMqConsumer(
    IOptions<RabbitMQSettings> settings,
    IServiceProvider serviceProvider,
    ILogger<RabbitMqConsumer> logger)
    : BackgroundService
{
    private readonly RabbitMQSettings _settings = settings.Value;
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken); // Give RabbitMQ time to start

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await _channel.BasicQosAsync(
                prefetchSize: 0, 
                prefetchCount: 1, 
                global: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var dto = JsonSerializer.Deserialize<ActivityDto>(message);
                    if (dto != null)
                    {
                        using var scope = serviceProvider.CreateScope();
                        var repository = scope.ServiceProvider.GetRequiredService<IActivityRepository>();
                        
                        var activity = ActvityMapper.ToModel(dto);
                        activity.Timestamp = DateTime.UtcNow;
                        await repository.AddActivity(activity);
                        
                        logger.LogInformation("Processed activity from RabbitMQ: {TaskId}", activity.TaskId);
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message from RabbitMQ");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _settings.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            logger.LogInformation("RabbitMQ consumer started and listening on queue: {QueueName}", _settings.QueueName);

            // Keep running until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("RabbitMQ consumer is shutting down");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start RabbitMQ consumer");
            throw;
        }
    }

    public override async void Dispose()
    {
        try
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }
            
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing RabbitMQ consumer");
        }
        
        base.Dispose();
    }
}