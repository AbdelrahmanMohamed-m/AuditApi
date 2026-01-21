/*
using System.Text;
using System.Text.Json;
using AuditApi.Data;
using AuditApi.DTOs;
using AuditApi.mappers;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuditApi.Services;

public class RabbitMQConsumer : BackgroundService
{
    private readonly RabbitMQSettings _settings;
    private readonly IActivityRepository _repository;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQConsumer(IOptions<RabbitMQSettings> settings, IActivityRepository repository, ILogger<RabbitMQConsumer> logger)
    {
        _settings = settings.Value;
        _repository = repository;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _settings.QueueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var dto = JsonSerializer.Deserialize<ActivityDto>(message);
                    if (dto != null)
                    {
                        var activity = ActvityMapper.ToModel(dto);
                        activity.Timestamp = DateTime.UtcNow;
                        await _repository.AddActivity(activity);
                        _logger.LogInformation("Processed activity from RabbitMQ: {TaskId}", activity.TaskId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from RabbitMQ");
                }

                _channel?.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: _settings.QueueName,
                                  autoAck: false,
                                  consumer: consumer);

            _logger.LogInformation("RabbitMQ consumer started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ consumer. RabbitMQ may not be available.");
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
*/