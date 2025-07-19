using APP.READ_MESSAGES.Libraries;
using APP_CHECKOUT.Interfaces;
using APP_CHECKOUT.Repositories;
using APP_CHECKOUT.Models.Models.Queue;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Configuration;
using MongoDB.Driver.Core.Events;



ServiceCollection service_collection = new ServiceCollection();
service_collection.AddSingleton<IMainServices, MainServices>();
service_collection.AddSingleton<ILoggingService, LoggingService>();
var service_provider = service_collection.BuildServiceProvider();
var main_service = service_provider.GetService<IMainServices>();
var log_service = service_provider.GetService<ILoggingService>();
try
{
    var host = ConfigurationManager.AppSettings["QUEUE_HOST"];
     host = ConfigurationManager.AppSettings["QUEUE_PORT"];
     host = ConfigurationManager.AppSettings["QUEUE_USERNAME"];
     host = ConfigurationManager.AppSettings["QUEUE_PASSWORD"];
     host = ConfigurationManager.AppSettings["QUEUE_V_HOST"];
    var factory = new ConnectionFactory()
    {
        HostName = ConfigurationManager.AppSettings["QUEUE_HOST"],
        Port = Convert.ToInt32(ConfigurationManager.AppSettings["QUEUE_PORT"]),
        VirtualHost = ConfigurationManager.AppSettings["QUEUE_V_HOST"],
        UserName = ConfigurationManager.AppSettings["QUEUE_USERNAME"],
        Password = ConfigurationManager.AppSettings["QUEUE_PASSWORD"],
    };
    using (var connection = factory.CreateConnection())
    using (var channel = connection.CreateModel())
    {
        channel.QueueDeclare(queue: ConfigurationManager.AppSettings["queue_name"],
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        Console.WriteLine("Service Waiting: " + DateTime.Now.ToString("dd/MM/yy HH:mm:ss"));
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                log_service.InsertLogTelegramDirect("Received: "+body);

                 var message = Encoding.UTF8.GetString(body);
                try
                {
                    var request = JsonConvert.DeserializeObject<CheckoutQueueModel>(message);
                    await main_service.Excute(request);
                }
                catch { }
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                string err = "Program: " + ex.ToString();
                Console.WriteLine(err);
                log_service.InsertLogTelegramDirect(err);

            }
        };

        channel.BasicConsume(queue: ConfigurationManager.AppSettings["queue_name"], autoAck: false, consumer: consumer);

        Console.ReadLine();

    }
}
catch (Exception ex)
{
    string err = "Main: " + ex.ToString();
    Console.WriteLine(err);
    log_service.InsertLogTelegramDirect(err);
}
