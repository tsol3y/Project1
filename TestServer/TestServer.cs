using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Buffers.Binary;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using RayTracer;

namespace TestServer
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var utf8 = new UTF8Encoding();//why is this encoding instead of decoding?
            UdpClient server = new UdpClient(3334);//UDP is receiving messages on port 3000
            bool notReceivedFirstPacket = true;
            // IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3333);
            var listener = server.ReceiveAsync();
            byte[] initialLine = new byte[0];
            IPEndPoint clientIP = null;
            while (notReceivedFirstPacket) {
                var action = Task.WaitAny(listener);
                switch(action) {
                    case 0:
                        var firstPacket = await listener;
                        initialLine = firstPacket.Buffer;
                        clientIP = firstPacket.RemoteEndPoint;
                        notReceivedFirstPacket = false;
                        break;
                    default:
                        break;
                }
            }
            // byte[] initialLine = server.Receive(ref ip);
            //IPEndPoint
            String[] decodedLine = utf8.GetString(initialLine, 0, initialLine.Length).Split("~");            
            String[] sceneFile = new String[Int32.Parse(decodedLine[1])];
            sceneFile[Int32.Parse(decodedLine[0]) - 1] = decodedLine[2];
            //need to receive one line to know how big the byte array should be
            
            // we have already received the first packet so counter starts at 1
            // int counter = 1;
            // int upper = Int32.Parse(decodedLine[1]);

            listener = server.ReceiveAsync();
            var timeout = Task.Delay(500);
            var sceneNotComplete = true;
////////////////////Receiving scene file/////////////////////////////////////
            while (sceneNotComplete) {
                var action = Task.WaitAny(listener, timeout);
                switch(action) {
                    case 0: //Receiving lines
                        var sceneLine = await listener;
                        // decodedLine = utf8.GetString(sceneLine, 0, sceneLine.Length).Split("~");
                        decodedLine = utf8.GetString(sceneLine.Buffer).Split("~");
               
                        sceneFile[Int32.Parse(decodedLine[0]) - 1] = decodedLine[2];
                        // if the scene file is complete, make sceneNotComplete false
                        // if (sceneFile.Where(l => String.IsNullOrEmpty(l)).Select()) {
                        // if (sceneFile.Count(l => String.IsNullOrEmpty(l)) == 0) {
                        if (sceneFile.Count(l => l == null) == 0) {
                            server.Send(new byte[0], 0, clientIP);
                            File.WriteAllLines(@args[0], sceneFile);
                            sceneNotComplete = false;
                        }
                        listener = server.ReceiveAsync();
                        break;
                    case 1: // timeout and we don't have every line
                        var missingIndices = sceneFile.Select((l, i) => new {l, i})
                                                    //   .Where(o => String.IsNullOrEmpty(o.l))
                                                      .Where(o => o.l == null)
                                                      .Select(o => o.i);
                        foreach (var index in missingIndices) {
                            // byte[] missingLine = new byte[4];
                            // var byteSpan = new Span <byte>(missingLine);
                            // BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(0, 4), index);
                            // server.Send(missingLine);
                            Console.WriteLine(index);
                            var missingLine = PackMissingLine(index);
                            server.Send(missingLine, missingLine.Length, clientIP);
                        }
                        timeout = Task.Delay(500);
                        break;
                    default: 
                        break;
                }
            }

            // while (counter < upper) {
            //     byte[] sceneLine = server.Receive(ref ip);
            //     decodedLine = utf8.GetString(sceneLine, 0, sceneLine.Length).Split("~");
            //     sceneFile[Int32.Parse(decodedLine[0]) - 1] = decodedLine[2];
            //     counter++;
            // }

            
            

            // foreach (String decoded in sceneFile) {
            //     Console.WriteLine(decoded);
            // }

            // for(;;){
            //     IPEndPoint ip = null;
            //     Byte[] data = s.Receive(ref ip);//Receive receives and returns the message and puts the ip address and
            //                                     //port number of the sender 
            //     var m = utf8.GetString(data, 0, data.Length);//GetString turns the bytes into a string
            //     Console.WriteLine("{0}: {1}", ip, m);//prints to terminal
            // }
        }

        public static byte[] PackMissingLine(int lineNumber) {
            byte[] missingLine = new byte[4];
            var byteSpan = new Span <byte>(missingLine);
            BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(0, 4), lineNumber);
            return missingLine;
        }
    }
}
