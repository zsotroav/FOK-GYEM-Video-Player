using System.Collections;
using System.Drawing;
using FOK_GYEM.Serial;
using FOK_GYEM.PluginBase;

#pragma warning disable CA1416

namespace FOK_GYEM_VideoPlayer
{

    internal class Program
    {
        public static string Dir = "./";
        public static string Loop = "false";
        public static int Fps = 24;
        public static int Num = 4;

        public static int ModV = 7;
        public static int ModH = 24;
        public static int ModCnt = 1;

        public static int Baud = 57600;

        public static List<byte[]> TestImages = new()
        {
            new byte[] { 0x22, 0x01, 0x11, 0x22, 0x01, 0x11, 0x22, 0x01, 0x11, 0x22, 0xF9, 0x11, 0x22, 0x01, 0x11, 0x22, 0x01, 0x11, 0x22, 0x01, 0x11 },
            new byte[] { 0x44, 0x02, 0x22, 0x44, 0x02, 0x22, 0x45, 0xF2, 0x22, 0x44, 0x02, 0x22, 0x45, 0xF2, 0x22, 0x44, 0x02, 0x22, 0x44, 0x02, 0x22 },
            new byte[] { 0x88, 0x04, 0x44, 0x8B, 0xE4, 0x44, 0x88, 0x04, 0x44, 0x8B, 0xE4, 0x44, 0x88, 0x04, 0x44, 0x8B, 0xE4, 0x44, 0x88, 0x04, 0x44 },
            new byte[] { 0x10, 0x08, 0x88, 0x17, 0xC8, 0x88, 0x10, 0x08, 0x88, 0x13, 0xC8, 0x88, 0x14, 0x08, 0x88, 0x13, 0xC8, 0x88, 0x10, 0x08, 0x88 },
        };


        static void Main(string[] args)
        {
            Console.WriteLine("Hello, FOK-GYEM!");
            Dictionary<string, string> argsDictionary = new();

            List<Connection> connections = new();

            for (int i = 0; i < args.Length; i += 2) argsDictionary.Add(args[i], args[i + 1]);

            Console.Write("Directory of the images: ");
            Dir = (argsDictionary.TryGetValue("-dir", out var value) ? value : Console.ReadLine())!;
            Console.WriteLine(Dir);

            Console.Write("Frame rate of video: ");
            Fps = int.Parse((argsDictionary.TryGetValue("-fps", out value) ? value : Console.ReadLine()) ?? string.Empty);
            Console.WriteLine(Fps);

            Console.Write("Number of display controllers: ");
            Num = int.Parse((argsDictionary.TryGetValue("-num", out value) ? value : Console.ReadLine())!);
            Console.WriteLine(Num);

            for (int i = 0; i < Num; i++)
            {
                Console.Write($"COM Port {i}: ");
                var p = ((argsDictionary.TryGetValue($"-port{i}", out value) ? value : Console.ReadLine())!);
                Console.WriteLine(p);

                connections.Add(new Connection(i, p, Baud));
                if (!connections[^1].Init(ModCnt, ModH, ModV))
                {
                    Console.WriteLine("Connection failed, aborting...\nPress any key to exit.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Connected to display number {i + 1}");
                connections[^1].FullScreenWrite(TestImages[i], false);
            }

            Console.Write("Loop: ");
            Loop = (argsDictionary.TryGetValue("-loop", out value) ? value : Console.ReadLine())!;
            Console.WriteLine(Loop);

            //var conn = new Connection(Port, 57600);

            var loop = Loop.ToLower() == "true";

            Console.WriteLine("Connected all displays! Starting in a few seconds...\n\n");
            

            Thread.Sleep(2000);

            int n = Directory.GetFiles(Path.Combine(Dir, "0")).Length;
            int displayed = 0;
            var t = DateTime.Now.TimeOfDay;
            int frameTime = 10000000 / Fps;

            do
            {
                var done = false;
                while(!done)
                {
                    var frame = (int)((DateTime.Now.TimeOfDay - t).Ticks / frameTime);
                    if (frame >= n)
                    {
                        done = true;
                        continue;
                    }
                    var file = frame + ".png";

                    List<Task> tasks = new();

                    foreach (var connection in connections)
                    {
                        var screen = Utils.ToByteArray(LoadFromBmpFile(Path.Combine(Dir, connection.id.ToString(), file)));
                        tasks.Add(Task.Factory.StartNew(() => ThreadWrite(connection, screen)));
                    }

                    Task.WaitAll(tasks.ToArray());
                    Console.WriteLine(Path.GetFileName(file));
                    displayed++;
                }
            } while (loop);

            foreach (var conn in connections) conn.Destroy();

            Console.WriteLine(displayed);
            
        }

        public static void ThreadWrite(Connection conn, byte[] screen) => conn.FullScreenWrite(screen);

        public static BitArray LoadFromBmpFile(string loc, bool force = false)
        {
            var re = new BitArray(ModCnt * ModV * ModH);
            if (string.IsNullOrEmpty(loc) || !File.Exists(loc)) throw new InvalidDataException();

            var bmp = new Bitmap(Image.FromFile(loc));

            if (bmp.Width % ModH != 0 || bmp.Height != ModV || bmp.Width / ModH != ModCnt || force)
            {
                Console.WriteLine("Bitmap dimensions are incorrect");
                throw new InvalidDataException();
            }

            for (int x = 0; x < ModH * ModCnt; x++)
            {
                for (int y = 0; y < ModV; y++)
                {
                    var col = bmp.GetPixel(x, y);
                    var gs = (int)(col.R * 0.3 + col.G * 0.59 + col.B * 0.11);
                    re[x + y * ModH * ModCnt] = Color.FromArgb(gs, gs, gs) != Color.FromArgb(0, 0, 0);
                }
            }
            return re;
        }
    }
}