using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

namespace Server
{
    class Program
    {
        /*async static Task Main(string[] args)
        {
            var sock = new UdpClient(9001);
            var t = sock.ReceiveAsync();
            var d = Task.Delay(5000);
            var k = Task.Run(() => Console.ReadLine());

            for (;;) {
                var i = Task.WaitAny(t, d, k);
                switch (i) {
                    case 0:
                        var r = await t;
                        Console.WriteLine($"{r.RemoteEndPoint}: {Encoding.UTF8.GetString(r.Buffer)}");
                        t = sock.ReceiveAsync();
                        break;
                    case 1:
                        Console.WriteLine("5 seconds have passed.");
                        d = Task.Delay(5000);
                        break;
                    case 2:
                        var s = await k;
                        Console.WriteLine($"You typed: {s}");
                        k = Task.Run(() => Console.ReadLine());
                        break;
                    default:
                        break;
                }
            }
        }*/
    }
}