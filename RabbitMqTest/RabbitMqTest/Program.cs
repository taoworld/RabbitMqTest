using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace RabbitMqTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Input message!");
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            //set specifique user and password
            //user can be create in Web Management pluggin 
            //rabbitmq-plugins enable rabbitmq_management
            //http://localhost:15672/

            //or add user by commend line : rabbitmqctl add_user {username} {password}

            factory.UserName = "tao";
            factory.Password = "futao";
            
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);
                    };
                    channel.BasicConsume(queue: "hello",
                                         noAck: true,
                                         consumer: consumer);

                    var consumerThread = new Thread(() => ConsumerThread(channel, consumer));
                    consumerThread.Start();

                    var inputMessage = "Hello world";
                    while (!inputMessage.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var body = Encoding.UTF8.GetBytes(inputMessage);

                        channel.BasicPublish(exchange: "",
                                             routingKey: "hello",
                                             basicProperties: null,
                                             body: body);

                        inputMessage = Console.ReadLine();
                    }

                    consumerThread.Abort();
                }

                Console.Write("Finished!");
                Console.ReadLine();
            }
        }

        private static void ConsumerThread(IModel channel, EventingBasicConsumer consumer)
        {
            while (true)
            {
                channel.BasicConsume(queue: "hello",
                                         noAck: true,
                                         consumer: consumer);
                Thread.Sleep(1000);
            }
        }
    }
}
