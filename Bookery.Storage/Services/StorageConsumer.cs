using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Bookery.Storage.Services
{
    public class StorageConsumer : IHostedService, IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IModel? _channel;
        private EventingBasicConsumer? _consumer;
        private readonly IStorageService _storageService;

        public StorageConsumer(string hostname, int port, string username, string password, IStorageService storageService)
        {
            _storageService = storageService;
            _connectionFactory = new ConnectionFactory()
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password
            };
            
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel?.QueueDeclare(queue: "delete_file",
                durable: false, exclusive: false, autoDelete: false, arguments: null);

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += async (sender, args) =>
            {
                var id = Guid.Parse(Encoding.UTF8.GetString(args.Body.Span));
                await DeleteFile(id);
            };
            _channel?.BasicConsume(queue: "delete_file", autoAck: true, consumer: _consumer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }

        private async Task DeleteFile(Guid id)
        {
            await _storageService.Delete(id);
        }
    }
}
