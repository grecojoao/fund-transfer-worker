using FundTransfer.Domain.Bus.Consumers;
using FundTransfer.Domain.Commands;
using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Handlers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace FundTransfer.Infra.RabbitMq.Consumers
{
    public class TransferConsumer : IBusConsumer
    {
        private readonly FundTransferHandler _handler;
        private readonly IConfiguration _configuration;

        public TransferConsumer(FundTransferHandler handler, IConfiguration configuration)
        {
            _handler = handler;
            _configuration = configuration;
        }

        public Task ReceiveAsync()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = GetHostName() };
                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();
                channel.QueueDeclare(queue: GetTransferQueueName(), durable: true, exclusive: false, autoDelete: false, arguments: null);
                RunWorker(channel);
                RunWorker(channel);
                Log.Information($"[Transfer Consumer] - Initiated");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "");
            }
            return Task.CompletedTask;
        }

        private void RunWorker(IModel channel)
        {
            channel.BasicQos(0, 10, false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Guid transactionId = default;
                try
                {
                    var transferCommand = JsonConvert.DeserializeObject<TransferCommand>(message);
                    transactionId = transferCommand.TransactionId;
                    var commandResult = await _handler.Handle(transferCommand, default);
                    var transfer = (Transfer)commandResult.Data;
                    if (!commandResult.Sucess)
                    {
                        channel.BasicNack(ea.DeliveryTag, false, true);
                        Log.Debug($"[{transactionId}] - {JsonConvert.SerializeObject(transfer)}");
                        Log.Information($"[{transactionId}] - Nack: {message}");
                    }
                    else
                    {
                        channel.BasicAck(ea.DeliveryTag, false);
                        Log.Debug($"[{transactionId}] - {JsonConvert.SerializeObject(transfer)}");
                        Log.Information($"[{transactionId}] - Ack: {message}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "");
                    Log.Warning($"[{transactionId}] - Nack: {message}");
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            channel.BasicConsume(queue: GetTransferQueueName(), autoAck: false, consumer: consumer);
        }

        private string GetTransferQueueName() =>
            _configuration["RabbitMq:TransferQueue"];

        private string GetHostName() =>
            _configuration["RabbitMq:Url"];
    }
}