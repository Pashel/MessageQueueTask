using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueueTask.Client
{
    public class Client
    {
        private const string QueueName = "server_queue";

        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly IBasicProperties _properties;

        public Action<string> Callback { get; set; }

        public Client(string host, string user, string password)
        {
            var factory = new ConnectionFactory {
                HostName = host,
                UserName = user,
                Password = password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);

            _properties = _channel.CreateBasicProperties();
            _properties.ReplyTo = _replyQueueName;
        }

        public void Send(string argument)
        {
            var correlationId = Guid.NewGuid().ToString();
            _properties.CorrelationId = correlationId;
            
            _consumer.Received += (model, ea) => {
                var response = Encoding.UTF8.GetString(ea.Body);
                if (ea.BasicProperties.CorrelationId == correlationId) {
                    Callback?.Invoke(response);
                }
            };

            var messageBytes = Encoding.UTF8.GetBytes(argument);
            _channel.BasicPublish (
                exchange: "",
                routingKey: QueueName,
                basicProperties: _properties,
                body: messageBytes
            );
            _channel.BasicConsume (
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true
            );
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}

