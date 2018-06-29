using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace IpWatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load data
            string fileName = "ips.txt";
            var slackUrl = args[0];
            if (slackUrl is null)
            {
                throw new ArgumentNullException("Webhook url must be provided!");
            }
            var webhookUrl = new Uri(args[0]);
            var slackClient = new SlackClient(webhookUrl);


            string knownIps = "";
            if (File.Exists(fileName))
            {
                 knownIps = File.ReadAllLines("ips.txt") [0];
            }


            var host = Dns.GetHostEntry(Dns.GetHostName());
            var currentIps = $"{DateTime.UtcNow.ToString("s").Replace('T', ' ')}: ";

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                { 
                    currentIps += $"{ip.ToString()}, ";
                }
            }

            if (knownIps.Length == 0 || knownIps.Split(':').Last() != currentIps.Split(':').Last())
            {
                Console.WriteLine($"New ips found: {currentIps}" );
                File.WriteAllText(fileName, currentIps);

                try
                {
                    var httpResponseMessage = slackClient.SendMessageAsync(currentIps).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
