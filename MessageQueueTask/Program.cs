using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Configuration;
using MessageQueueTask.Actions;

namespace MessageQueueTask.Server
{
    class Programm
    {
        static void Main(string[] args)
        {
            var newApp = CheckExistance();
            if (!newApp) {
                Console.WriteLine("[x] Aplication has already started");
                Console.ReadLine();
                return;
            }

            var server = new Server(
                ConfigurationManager.AppSettings["host"],
                ConfigurationManager.AppSettings["user"],
                ConfigurationManager.AppSettings["password"]
            );

            server.Start(new CalcAction());

            Console.WriteLine("[x] Press [enter] to exit.");
            Console.ReadLine();

            server.Stop();
        }

        static bool CheckExistance()
        {
            var guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();
            var mutex = new Mutex(true, guid, out var newApp);
            return newApp;
        }
    }
}
