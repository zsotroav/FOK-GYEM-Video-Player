using System.Collections;
using System.Drawing;
#pragma warning disable CA1416

namespace FOK_GYEM_Projector_preprocess
{
    internal class Program
    {
        public static string Dir = "./";

        public static int ModV = 7;
        public static int ModH = 24;
        public static int ModCnt = 4;


        static void Main(string[] args)
        {
            Console.WriteLine("Hello, FOK-GYEM!");
            Dictionary<string, string> argsDictionary = new();

            if (args.Length % 2 == 0)
            {
                for (int i = 0; i < args.Length; i += 2) argsDictionary.Add(args[i], args[i + 1]);

            }

            Console.Write("Directory of the images: ");
            Dir = (argsDictionary.TryGetValue("-dir", out var value) ? value : Console.ReadLine())!;
            Console.WriteLine(Dir);

            Console.Write("Mode of splicing: ");
            bool mode = (argsDictionary.TryGetValue("-mode", out value) ? value : Console.ReadLine()) == "module";
            Console.WriteLine(mode);

            foreach (var file in Directory.GetFiles(Dir))
            {
                //if (file[^4..] != ".bmp") continue;
                if (mode) Magic2(file);
                else Magic(file);
            }


        }

        public static void Magic(string loc)
        {
            var bmp = new Bitmap(Image.FromFile(loc));
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

            var re = new Bitmap(ModH * ModCnt, ModV);
            for (int i = 0; i < ModCnt; i++)
            {
                for (int x = 0; x < ModH; x++)
                {
                    for (int y = 0; y < ModV; y++)
                    {
                        var p = bmp.GetPixel(x, y + i * ModV);
                        var Y = 0.2126 * p.R + 0.7152 * p.G + 0.0722 * p.B;
                        re.SetPixel(x + i*ModH, y, Y < 128 ? Color.Black : Color.White);
                    }
                }
            }

            re.Save("C:\\_\\spliced\\"+Path.GetFileName(loc));

        }

        public static void Magic2(string loc)
        {
            var bmp = new Bitmap(Image.FromFile(loc));
            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);

            for (int i = 0; i < ModCnt; i++)
            {

                var re = new Bitmap(ModH, ModV);
                for (int x = 0; x < ModH; x++)
                {
                    for (int y = 0; y < ModV; y++)
                    {
                        var p = bmp.GetPixel(x, y + i * ModV);
                        var Y = 0.2126 * p.R + 0.7152 * p.G + 0.0722 * p.B;
                        re.SetPixel(x, y, Y < 128 ? Color.Black : Color.White);
                    }
                }


                re.Save($"C:\\_\\spliced\\{i}\\" + Path.GetFileName(loc));
            }


        }

    }
}