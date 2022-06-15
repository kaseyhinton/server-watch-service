using System.Net;
using System.Net.Sockets;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        Dictionary<IPAddress, IpAddressCacheResult> IpAddressCache =
            new Dictionary<IPAddress, IpAddressCacheResult>();

        app.MapGet("/server/{ipaddress}", (string ipaddress) =>
        {
            try
            {
                var addresses = Dns.GetHostAddresses(ipaddress);
                if (!addresses.Any())
                {
                    return "Error";
                }

                if (IpAddressCache.ContainsKey(addresses[0]) && DateTime.Now - IpAddressCache[addresses[0]].LastPingDate < TimeSpan.FromSeconds(60))
                {
                    Console.WriteLine("Serving result from cache");
                    return IpAddressCache[addresses[0]].Status;
                }
                else if (IpAddressCache.ContainsKey(addresses[0]))
                {
                    Console.WriteLine("Removing stale cache entry");
                    IpAddressCache.Remove(addresses[0]);
                }
                IPEndPoint ipep = new IPEndPoint(addresses[0], 80);
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Connect(ipep);

                Console.WriteLine($"{addresses[0]}");

                string status = server.Connected ? "Online" : "Offline";

                IpAddressCache.Add(addresses[0], new IpAddressCacheResult
                {
                    LastPingDate = DateTime.Now,
                    Status = status
                });

                return status;
            }
            catch (Exception)
            {
                return "Error";
            }
        });

        app.Run($"http://localhost:{3000}");
    }
}

class IpAddressCacheResult
{
    public string? Status { get; set; }
    public DateTime LastPingDate { get; set; }
}