using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MessageQueueTask.Actions;

namespace MessageQueueTask.Server
{
    class Server
    {
        private const string QueueName = "server_queue";

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public Server(string host, string user, string password)
        {
            var factory = new ConnectionFactory {
                HostName = host,
                UserName = user,
                Password = password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Start(IAction action)
        {
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false, 
                arguments: null
            );
            _channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                string response = null;

                var replyProps = _channel.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties.CorrelationId;

                try {
                    var message = Encoding.UTF8.GetString(ea.Body);
                    int number = int.Parse(message);
                    response = $"{number} --> {action.Proceed(number)}";
                }
                catch (Exception) {
                    response = "Error occured";
                }
                finally {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    _channel.BasicPublish (
                        exchange: "",
                        routingKey: ea.BasicProperties.ReplyTo,
                        basicProperties: replyProps,
                        body: responseBytes
                    );
                    _channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                }
            };

            _channel.BasicConsume(
                queue: QueueName,
                autoAck: false,
                consumer: consumer
            );
        }

        public void Stop()
        {
            _connection.Close();
        }
    }
}
