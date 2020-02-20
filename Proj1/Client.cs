using RayTracer;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Buffers.Binary;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client {
    class Program {
        async static Task Main(String[] args) { // args contains 4 inputs, scene filename, output filename, width, height  
            // We need to figure out where we are getting the IP addresses and port numbers
            var width = Int32.Parse(args[2]);
            var height = Int32.Parse(args[3]);
            List <int> rowsCompleted = new List<int>();
            for (int y = 0; y < height; y++) {
                rowsCompleted.Add(y);
            }
            
            byte[,,] bitmap = new byte[width, height, 3];

            Queue<IPEndPoint> availableMachines = new Queue<IPEndPoint>();
            byte[][] sceneArray = GenerateSceneArray(args[0], width, height);
            UTF8Encoding utf8 = new UTF8Encoding();
            UdpClient client = new UdpClient(3333); //there were specific instructions about what port to use
            

            // NEED TO READ SERVER LIST HERE!!!!!(@#(%*^@#(*%)))
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3334);

            for (int i = 0; i < sceneArray.Length; i++) {
                client.Send(sceneArray[i], sceneArray[i].Length, ip);
            }

            var listener = client.ReceiveAsync();
            var timeout = Task.Delay(500);
            var notFinished = true;
            IPEndPoint queueIP = null;
            int currentRow = -1;
            byte[] traceRequest = new byte[8];

            while(notFinished) {
                var action = Task.WaitAny(listener, timeout);
                switch(action) {
                    case 0:
                        var serverResponse = await listener;
                        var bufferedResponse = serverResponse.Buffer;
                        var packetLength = bufferedResponse.Length;
                        var incomingIP = serverResponse.RemoteEndPoint;

                        if (packetLength == 4) {
                            var missingLine = UnpackMissingLine(bufferedResponse);
                            client.Send(sceneArray[missingLine], sceneArray[missingLine].Length, ip);
                        } else if (packetLength == 0) { // Confirmation
                            if (!availableMachines.Contains(incomingIP)) {
                                availableMachines.Enqueue(incomingIP);
                                Console.WriteLine("Confirmed");
                            }
                        } else if (packetLength == (4 + width * 3)) {   // Receiving update 
                            rowsCompleted.Remove(UpdateBitMap(bufferedResponse, bitmap, width));
                        }
                        listener = client.ReceiveAsync();
                        break;
                    case 1:
                        timeout = Task.Delay(500);
                        break;
                    default:
                        break;
                }

                if (currentRow < rowsCompleted.Count - 1) {
                    currentRow++;
                } else {
                    currentRow = 0;
                }
                if (availableMachines.Count > 0) {
                    if (rowsCompleted.Count > 0) {
                        traceRequest = PackLineRequest(rowsCompleted[currentRow]);
                        queueIP = availableMachines.Dequeue();
                        availableMachines.Enqueue(queueIP);
                        client.Send(traceRequest, traceRequest.Length, queueIP);
                    } else {
                        notFinished = false;
                    }
                }
            }
            WritePPM(args[1], bitmap);
        }
        public static byte[] PackLineRequest(int row) {
            byte[] returnBytes = new byte[4];
            var byteSpan = new Span <byte>(returnBytes);
            BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(0, 4), row);
            return returnBytes;
        }
        public static int UnpackMissingLine(byte[] lineNumber) {
            var byteSpan = new Span <byte>(lineNumber);
            return BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(0, 4));
        }
        static int UpdateBitMap(byte[] update, byte[,,] bitmapToEdit, int width) {
            var byteSpan = new Span<byte>(update);
            var Y = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(0, 4));
            for(int X = 0; X < width; X++){
                bitmapToEdit[X,Y,0] = update[4 + X * 3];
                bitmapToEdit[X,Y,1] = update[5 + X * 3];
                bitmapToEdit[X,Y,2] = update[6 + X * 3];
            }
            return Y;
        }
        public static void WritePPM(String fileName, byte[,,] bitmap)
        {
            var width = bitmap.GetLength(0);
            var height = bitmap.GetLength(1);
            var header = $"P3 {width} {height} 255\n";
            using (var o = new StreamWriter(fileName))
            {
                o.WriteLine(header);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        o.Write($"{bitmap[x,y,0]} {bitmap[x,y,1]} {bitmap[x,y,2]}  ");
                    }
                    o.WriteLine();
                }
            }
        }
        public static byte[][] GenerateSceneArray(String filename, int width, int height) {
            int counter = 0;
            String line;
            List<List<String>> stringList = new List<List<String>>();
            StreamReader file = new StreamReader(@filename);

           while((line = file.ReadLine()) != null) {
                stringList.Add(new List<String>());
                stringList[counter].Add((counter + 1).ToString());
                stringList[counter].Add(line);
                stringList[counter].Add(width.ToString());
                stringList[counter].Add(height.ToString());
                counter++;
            }

            file.Close();
            String lineCount = counter.ToString();

            for (int i = 0; i < counter; i++) {
                stringList[i].Insert(0, lineCount);
            }

            byte[][] encodedArray = new byte[counter][];
            var utf8 = new UTF8Encoding();
            
            for (int i = 0; i < counter; i++) {
                encodedArray[i] = utf8.GetBytes((String.Format("{0}~{1}~{2}~{3}~{4}", stringList[i][1], stringList[i][0], stringList[i][2], stringList[i][3], stringList[i][4])));
            }
            return encodedArray;
        }
    }
}