#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Core.Configurations
#pragma warning restore IDE0130 // Namespace does not match folder structure
{

    /// <summary>
    /// Represents the configuration options for RabbitMQ.
    /// </summary>
    public class RabbitMQOptions
    {
        public string? HostName { get; set; } = "localhost";
        public int Port { get; set; } = -1;
        public string? UserName { get; set; } = "guest";
        public string? Password { get; set; } = "guest";
        public string? VirtualHost { get; set; } = "/";
        public string? ExchangeName { get; set; } = "cap.default.router";
        public bool PublishConfirms { get; set; } = false;

        public QueueArgumentsOptions? QueueArguments { get; set; } = new();
        public QueueRabbitOptions? QueueOptions { get; set; } = new();
        public BasicQosOptions? BasicQosOptions { get; set; }
    }

    /// <summary>
    /// Represents optional queue arguments ("x-arguments").
    /// </summary>
    public class QueueArgumentsOptions
    {
        public string? QueueMode { get; set; }
        public int MessageTTL { get; set; } = 864000000;
        public string? QueueType { get; set; }
    }

    /// <summary>
    /// Represents RabbitMQ native queue declaration options.
    /// </summary>
    public class QueueRabbitOptions
    {
        public bool Durable { get; set; } = true;
        public bool Exclusive { get; set; } = false;
        public bool AutoDelete { get; set; } = false;
    }

    /// <summary>
    /// Represents quality of service settings.
    /// </summary>
    public class BasicQosOptions
    {
        public ushort PrefetchCount { get; set; } = 0;
        public bool Global { get; set; } = false;
    }
}
