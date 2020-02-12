using RayTracer;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Buffers.Binary;

namespace Client {
    class Program {
        static void Main(string[] args) {

            var testInt = 1024;
            byte[] returnBytes = new byte[11];
            var byteSpan = new Span<byte>(returnBytes);
            BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(3,4), testInt);
            
            var testResult = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(3, 4));
            Console.WriteLine(testResult);

            /*var utf8 = new UTF8Encoding();
            UdpClient c = new UdpClient(3333);

            var startMsg = Console.ReadLine();
            byte[] bytes = utf8.GetBytes(startMsg);

            var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3334);
            c.Send(bytes, startMsg.Length, ip);
            var receivedBytes = c.Receive(ref ip);
            
            RayTracer.RayTracerApp.WritePPM("test", bitmap);*/
        }
    }
}

/*namespace client{
    class Program{
        static void Main(string[] args){
            var utf8 = new UTF8Encoding();
            UdpClient s = new UdpClient();

            for(;;){
                var l = Console.Readline();
                if(l == null) return;
                var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1", 3000));
                var m = utf8.GetBytes(1);
                s.Send(m, m.Length, ip);
            }
        }
    }
}

namespace server{
    class Program{
        static void Main(string[] args){
            var utf8 = new UTF8Encoding();//why is this encoding instead of decoding?
            UdpClient s = new UdpClient(3000);//UDP is receiving messages on port 3000

            for(;;){
                IPEndPoint ip = null;
                Byte[] data = s.Receive(ref ip);//Receive receives and returns the message and puts the ip address and
                                                //port number of the sender 
                var m = utf8.GetString(data, 0, data.Length);//GetString turns the bytes into a string
                Console.WriteLine("{0}: {1}", ip, m);//prints to terminal
            }
        }
    }
}*/