using RayTracer;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Buffers.Binary;
using System.IO;
using System.Collections.Generic;

namespace Client {
    class Program {
        static void Main(String[] args) {//needs to be able to take a scene file input  
            // We need to figure out where we are getting the IP addresses and port numbers
            // 
            byte[][] sceneArray = GenerateSceneArray(args[0]);   // send this to the server first
            UTF8Encoding utf8 = new UTF8Encoding();
            UdpClient client = new UdpClient(3333); //there were specific instructions about what port to use
            
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3334);
            foreach (byte [] encodedString in sceneArray) {
                client.Send(encodedString, encodedString.Length, ip);
            }
            // IPEndPoint[] serverArray = 

            
            //var startMsg = Console.ReadLine();//sceneArray[i]
            // foreach (byte [] encodedString in sceneArray) {
                // Console.WriteLine(utf8.GetString(encodedString, 0, encodedString.Length));
            // }
            // byte[] bytes = utf8.GetBytes(startMsg);

            // var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3334);
            // c.Send(bytes, startMsg.Length, ip);
            // var receivedBytes = c.Receive(ref ip);

            //var utf8 = new UTF8Encoding();//why is this encoding instead of decoding?
            // UdpClient s = new UdpClient(3000);//UDP is receiving messages on port 3000
                                   
            //var m = utf8.GetString(data, 0, data.Length);//GetString turns the bytes into a string

        }

        static void UpdateBitMap(byte[] update, byte[,,] bitmapToEdit) {
            var byteSpan = new Span<byte>(update);
            var X = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(3, 4));
            var Y = BinaryPrimitives.ReadInt32BigEndian(byteSpan.Slice(7, 4));
            bitmapToEdit[X,Y,0] = update[0];
            bitmapToEdit[X,Y,1] = update[1];
            bitmapToEdit[X,Y,2] = update[2];
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
                        // o.Write($"{ToByte(c.R)} {ToByte(c.G)} {ToByte(c.B)}  ");
                        o.Write($"{bitmap[x,y,0]} {bitmap[x,y,1]} {bitmap[x,y,2]}  ");
                    }
                    o.WriteLine();
                }
            }
        }
        
        public static byte[][] GenerateSceneArray(String filename) {
            int counter = 0;
            String line;
            List<List<String>> stringList = new List<List<String>>();
            StreamReader file = new StreamReader(@filename);

           while((line = file.ReadLine()) != null) {
                stringList.Add(new List<String>());
                stringList[counter].Add((counter + 1).ToString());
                stringList[counter].Add(line);
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
                encodedArray[i] = utf8.GetBytes((String.Format("{0}~{1}~{2}", stringList[i][1], stringList[i][0], stringList[i][2])));
            }

           // byte[] bytes = utf8.GetBytes(startMsg);

            return encodedArray;
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

 // RayTracer.RayTracerApp.WritePPM("test", bitmap);

            // foreach (List<string> arrayLine in stringArray) {
            //     System.Console.WriteLine("{0}~{1}~{2}", arrayLine[1], arrayLine[0], arrayLine[2]);
            // }

            // THESE LINES TEST RENDERING FROM A SCENE, THEY WILL
            // BE USED IN THE FINAL CLIENT
            // string inputFile = "default.scene";
            // string outputFile = "testOutput.ppm";           
            // var vs = RayTracer.RayTracer.ReadScene(inputFile);
            // var view = vs.Item1;
            // var width = view.Item1;
            // var height = view.Item2;
            
            // RayTracer.RayTracer rayTracer = new RayTracer.RayTracer(width, height);
            // byte[,,] bitmap = new byte[width, height, 3];
            
            // for(int x = 0; x < width; x++) {
            //     for(int y = 0; y < height; y++) {
            //         UpdateBitMap(rayTracer.Render(vs.Item2, x, y), bitmap);
            //     }
            // }
            // WritePPM(outputFile, bitmap);
        /////////////////////////////////////////////////////



            // byte[,,] bitmap = new byte[400, 400, 3];
            // RayTracer.RayTracer rayTracer = new RayTracer.RayTracer(400, 400,
            //     (int x, int y, Color color) => { bitmap[x, y] = color; });;
            
            // byte[] returnBytes = new byte[11];
            // returnBytes[0] = RayTracer.RayTracerApp.ToByte(200.0);
            // returnBytes[1] = RayTracer.RayTracerApp.ToByte(180.0);
            // returnBytes[2] = RayTracer.RayTracerApp.ToByte(160.0);
            // var byteSpan = new Span<byte>(returnBytes);
            // BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(3,4), 300);
            // BinaryPrimitives.WriteInt32BigEndian(byteSpan.Slice(7,4), 250);
            // UpdateBitMap(returnBytes, bitmap);
            // Console.WriteLine(bitmap[300,250].R);
            // Console.WriteLine(bitmap[300,250].G);
            // Console.WriteLine(bitmap[300,250].B);

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