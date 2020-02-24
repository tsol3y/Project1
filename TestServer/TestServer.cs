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
        async static Task Main(string[] args) // args contains one input, scene filename
        {
            var utf8 = new UTF8Encoding();
            UdpClient server = new UdpClient(3334);
            bool notReceivedFirstPacket = true;
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

            String[] decodedLine = utf8.GetString(initialLine, 0, initialLine.Length).Split("~");            
            String[] sceneFile = new String[Int32.Parse(decodedLine[1])];
            sceneFile[Int32.Parse(decodedLine[0]) - 1] = decodedLine[2];
            int width = Int32.Parse(decodedLine[3]);
            int height = Int32.Parse(decodedLine[4]);

            listener = server.ReceiveAsync();
            var timeout = Task.Delay(500);
            var sceneNotComplete = true;

            while (sceneNotComplete) {
                var action = Task.WaitAny(listener, timeout);

                switch(action) {
                    case 0:
                        var sceneLine = await listener;
                        decodedLine = utf8.GetString(sceneLine.Buffer).Split("~");
               
                        sceneFile[Int32.Parse(decodedLine[0]) - 1] = decodedLine[2];

                        if (sceneFile.Count(l => l == null) == 0) {
                            File.WriteAllLines(@args[0], sceneFile);
                            sceneNotComplete = false;
                        }
                        listener = server.ReceiveAsync();
                        break;
                    case 1: // timeout and we don't have every line
                        var missingIndices = sceneFile.Select((l, i) => new {l, i})
                                                      .Where(o => o.l == null)
                                                      .Select(o => o.i);
                        foreach (var index in missingIndices) {
                            var missingLine = PackMissingLine(index);
                            server.Send(missingLine, missingLine.Length, clientIP);
        
                        }
                        timeout = Task.Delay(500);
                        break;
                    default: 
                        break;
                }
            }
            
            listener = server.ReceiveAsync();
            var tracerScene = RayTracer.RayTracer.ReadScene(args[0]).Item2;
            RayTracer.RayTracer rayTracer = new RayTracer.RayTracer(width, height);
            
            server.Send(new byte[0], 0, clientIP); // Confirmation
            
            while (true) {
                var action = Task.WaitAny(listener);
                switch(action) {
                    case 0:
                        var clientRequest = await listener;
                        var bufferedRequest = clientRequest.Buffer;
                        var packetLength = bufferedRequest.Length;
                        if (packetLength == 4) { // ensure this is a request
                            var y = UnpackLineRequest(bufferedRequest);
                            var returnTrace = rayTracer.Render(tracerScene, y);
                            server.Send(returnTrace, returnTrace.Length, clientIP);
                        }
                        listener = server.ReceiveAsync();
                        break;
                    default:
                        break;
                }
            }
        }

        public static byte[] PackMissingLine(int lineNumber) {
            byte[] missingLine = new byte[4];
            var byteSpan = new Span <byte>(missingLine);
            BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(0, 4), lineNumber);
            return missingLine;
        }

        public static int UnpackLineRequest(byte[] lineNumber) {
            var byteSpan = new Span <byte>(lineNumber);
            var test =  BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(0, 4));
            return test;
        }
    }
}
