using System;
using System.Configuration;

namespace MessageQueueTask.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new Client (
                ConfigurationManager.AppSettings["host"],
                ConfigurationManager.AppSettings["user"],
                ConfigurationManager.AppSettings["password"]
            );

            client.Callback += Print;

            Console.WriteLine("[x] Type any number to send, [enter] to exit.");
            while (true) {
                string msg = Console.ReadLine();
                if (msg == "") {
                    break;
                }
                client.Send(msg);
            }

            client.Close();
        }

        static void Print(string message)
        {
            Console.WriteLine($"[x] {message}");
        }
    }
}
