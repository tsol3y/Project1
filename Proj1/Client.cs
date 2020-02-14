using RayTracer;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Buffers.Binary;

namespace Client {
    class Program {
        static void Main(string[] args) {
            RayTracer.Color[,] bitmap = new Color[400, 400];
            RayTracer.RayTracer rayTracer = new RayTracer.RayTracer(400, 400,
                (int x, int y, Color color) => { bitmap[x, y] = color; });;
            
            byte[] returnBytes = new byte[11];
            returnBytes[0] = RayTracer.RayTracerApp.ToByte(200.0);
            returnBytes[1] = RayTracer.RayTracerApp.ToByte(180.0);
            returnBytes[2] = RayTracer.RayTracerApp.ToByte(160.0);
            var byteSpan = new Span<byte>(returnBytes);
            BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(3,4), 300);
            BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(7,4), 250);
            updateBitMap(returnBytes, bitmap);
            Console.WriteLine(bitmap[300,250].R);
            Console.WriteLine(bitmap[300,250].G);
            Console.WriteLine(bitmap[300,250].B);






            // var testResult = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(3, 4));
            // Console.WriteLine(testResult);
            // double testDouble = 156;
            // var testByte = RayTracerApp.ToByte(testDouble);
            // var testByte = Convert.ToByte(testDouble);
            // byte[] testByteArray = new byte[8];
            // testByteArray[7] = testByte;
            // var returnDouble = Convert.ToDouble(testByte);
            // if we wanted to, we could put the testByte into a byte[] and then convert it to a Double
            // Console.WriteLine(returnDouble);


            /*var utf8 = new UTF8Encoding();
            UdpClient c = new UdpClient(3333);

            var startMsg = Console.ReadLine();
            byte[] bytes = utf8.GetBytes(startMsg);

            var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3334);
            c.Send(bytes, startMsg.Length, ip);
            var receivedBytes = c.Receive(ref ip);
            
            RayTracer.RayTracerApp.WritePPM("test", bitmap);*/
        }

        static void updateBitMap(byte[] update, Color[,] bitmapToEdit) {
            var byteSpan = new Span<byte>(update);
            var R = Convert.ToDouble(update[0]);
            var G = Convert.ToDouble(update[1]);
            var B = Convert.ToDouble(update[2]);
            var X = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(3, 4));
            var Y = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(7, 4));
            bitmapToEdit[X,Y] = new Color(R, G, B);
            // This doesn't do anything yet. We need to convert R G B bytes into doubles to make a Color object
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