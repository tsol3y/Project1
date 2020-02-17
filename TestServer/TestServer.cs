using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Buffers.Binary;
using System.IO;
using System.Collections.Generic;
//Add RayTracer

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var utf8 = new UTF8Encoding();//why is this encoding instead of decoding?
            UdpClient server = new UdpClient(3334);//UDP is receiving messages on port 3000

            IPEndPoint ip = null;
            
            byte[] initialLine = server.Receive(ref ip);
            String[] decodedLine = utf8.GetString(initialLine, 0, initialLine.Length).Split("~");            
            String[] sceneFile = new String[Int32.Parse(decodedLine[1])];
            sceneFile[Int32.Parse(decodedLine[0]) - 1] = decodedLine[2];
            //need to receive one line to know how big the byte array should be
            
            // we have already received the first packet so counter starts at 1
            int counter = 1;
            int upper = Int32.Parse(decodedLine[1]);

            while (counter < upper) {
                byte[] sceneLine = server.Receive(ref ip);
                decodedLine = utf8.GetString(sceneLine, 0, sceneLine.Length).Split("~");
                sceneFile[Int32.Parse(decodedLine[0]) - 1] = decodedLine[2];
                counter++;
            }

            foreach (String decoded in sceneFile) {
                Console.WriteLine(decoded);
            }

            // for(;;){
            //     IPEndPoint ip = null;
            //     Byte[] data = s.Receive(ref ip);//Receive receives and returns the message and puts the ip address and
            //                                     //port number of the sender 
            //     var m = utf8.GetString(data, 0, data.Length);//GetString turns the bytes into a string
            //     Console.WriteLine("{0}: {1}", ip, m);//prints to terminal
            // }
        }
    }
}
