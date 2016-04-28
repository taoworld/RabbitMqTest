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
            var publishFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "tao",
                Password = "tao",
                VirtualHost = "myvirtualhost",
                Port = 5673
            };

            var consumerFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "tao",
                Password = "tao",
                VirtualHost = "anothervirtualhost",
                Port = 5673
            };

            //set specifique user and password
            //user can be create in Web Management pluggin 
            //rabbitmq-plugins enable rabbitmq_management
            //rabbitmq-plugins enable rabbitmq_shovel_management 
            //http://localhost:15672/

            //or add user by commend line : rabbitmqctl add_user {username} {password}
            using (var publishConnection = publishFactory.CreateConnection())
            using (var consumerConnection=consumerFactory.CreateConnection())
            {
                using (var publishChannel = publishConnection.CreateModel())
                using (var consumerChannel = consumerConnection.CreateModel())
                {
                //    channel.QueueBind("myqueue", "tao.Test.fanout", "");
                //    //QueueDeclare(queue: "hello",
                //    //                      durable: false,
                //    //                      exclusive: false,
                //    //                      autoDelete: false,
                //    //                      arguments: null);

                    var consumer = new EventingBasicConsumer(consumerChannel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);
                    };
                    //consumerChannel.BasicConsume(queue: "anotherqueue",
                    //                     noAck: true, 
                    //                     consumer: consumer);

                    var consumerThread = new Thread(() => ConsumerThread(consumerChannel, consumer));
                    consumerThread.Start();

                    var inputMessage = "Hello world";
                    while (!inputMessage.Equals("exit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var body = Encoding.UTF8.GetBytes(inputMessage);

                        publishChannel.BasicPublish(exchange: "myexchange.fanout",
                                             routingKey: "",
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
                channel.BasicConsume(queue: "anotherqueue",
                                         noAck: true,
                                         consumer: consumer);
                Thread.Sleep(1000);
            }
        }
    }
}
