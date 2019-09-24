using System.Collections.Generic;
using System.Drawing;
using System;
using System.Windows.Forms;
using System.Linq;
using ImageRecognition;

namespace ImageCompare
{
    public class General
    {
        public static bool IsValid(string ImageFile, out Bitmap Result)
        {
            Image Check = Image.FromFile(ImageFile);
            Result = new Bitmap(Check);

            if (Check == null)
            {
                Console.WriteLine("Invalid image: " + ImageFile + " (probably does not exist).");
                return false;
            }
            else
                return true;
        }
    }

    public class Character
    {
        public string Text;
        public Point Position;

        public Character(string Text, Point Position)
        {
            this.Text = Text;
            this.Position = Position;
        }
    }

    public class ComparePoints : IComparer<Character>
    {
        public int Compare(Character P1, Character P2)
        {
            if (P1.Position.Y != P2.Position.Y)
                return P1.Position.Y.CompareTo(P2.Position.Y);
            else
                return P1.Position.X.CompareTo(P2.Position.X);
        }
    }

    public class Compare
    {
        public static double ComparePercentage(string FileOne, string FileTwo)
        {
            return ComparePercentage(Image.FromFile(FileOne), Image.FromFile(FileTwo));
        }

        public static double ComparePercentage(Image FileOne, Image FileTwo)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist)!");
                return 0.0;
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist)!");
                return 0.0;
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return ComparePercentage(MappedOne, MappedTwo, 1.0, false);
        }

        /// <summary>
        /// Compares the two bitmaps and returns the resulting percentage.
        /// </summary>
        /// <param name="MappedOne">The first image</param>
        /// <param name="MappedTwo">The second image</param>
        /// <param name="BreakPercentage">The bottom limit of the percentage. Method will quit when this is no longer attainable. Set to 0 to ignore.</param>
        /// <returns></returns>
        public static double ComparePercentage(Bitmap FileOne, Bitmap FileTwo, double BreakPercentage, bool RoughColorMatch)
        {
            int SmallestWidth = Utility.Utility.GetLowest(FileOne.Width, FileTwo.Width);
            int SmallestHeight = Utility.Utility.GetLowest(FileOne.Height, FileTwo.Height);

            LockBitmap MappedOne = new LockBitmap(FileOne);
            LockBitmap MappedTwo = new LockBitmap(FileTwo);

            MappedOne.LockBits();
            MappedTwo.LockBits();

            double Result = 0.0, Minimum = BreakPercentage * (double)SmallestHeight * (double)SmallestWidth;

            for (int x = 0; x < SmallestWidth; x++)
            {
                for (int y = 0; y < SmallestHeight; y++)
                {
                    if (ImageUtility.RoughColorMatch(MappedOne.GetPixel(x, y), MappedTwo.GetPixel(x, y), (RoughColorMatch) ? 25 : 0))
                        Result++;
                    else if (BreakPercentage != 0.0)
                    {
                        double Max = (Result + SmallestWidth - x + SmallestHeight - y) / (SmallestHeight * SmallestWidth);

                        if (Max < BreakPercentage)
                        {
                            MappedOne.UnlockBits();
                            MappedTwo.UnlockBits();

                            return Max;
                        }
                    }

                    if (Result >= Minimum)
                        break;
                }

                if (Result >= Minimum)
                    break;
            }

            MappedOne.UnlockBits();
            MappedTwo.UnlockBits();

            return Result / (SmallestHeight * SmallestWidth);
        }
        
        public static bool ExactMatch(LockBitmap MappedOne, int x, int y, int width, int height, LockBitmap MappedTwo)
        {
            for (int curX = 0; curX < width; curX++)
            {
                for (int curY = 0; curY < height; curY++)
                {
                    var p1 = MappedOne.GetPixel(x + curX, y + curY);
                    var p2 = MappedTwo.GetPixel(curX, curY);

                    if (!p1.Equals(p2))
                    {
                        return false;
                    }
                }
                
            }

            return true;
        }
    }

    public class Contain
    {
        public static IEnumerable<Point> Contains(string FileOne, string FileTwo)
        {
            return Contains(Image.FromFile(FileOne), Image.FromFile(FileTwo), 1.0);
        }

        public static IEnumerable<Point> Contains(string FileOne, string FileTwo, double Percentage)
        {
            return Contains(Image.FromFile(FileOne), Image.FromFile(FileTwo), Percentage);
        }

        public static IEnumerable<Point> Contains(Image FileOne, Image FileTwo, double Percentage)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist)!");
                return new List<Point>();
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist)!");
                return new List<Point>();
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return Contains(MappedOne, MappedTwo, Percentage);
        }

        public static IEnumerable<Point> Contains(Bitmap MappedOne, Bitmap MappedTwo, double Percentage)
        {
            LockBitmap lockedOne = new LockBitmap(MappedOne);
            LockBitmap lockedTwo = new LockBitmap(MappedTwo);

            lockedOne.LockBits();
            lockedTwo.LockBits();

            try
            {
                for (int x = 0; x < (MappedOne.Width - MappedTwo.Width + 1); x++)
                {
                    for (int y = 0; y < (MappedOne.Height - MappedTwo.Height + 1); y++)
                    {
                        //If they meet the specified percentage (or more of course) it works
                        //if (Compare.ComparePercentage(MappedOne.Clone(new Rectangle(x, y, MappedTwo.Width, MappedTwo.Height), MappedOne.PixelFormat), MappedTwo, Percentage, true) >= Percentage) {
                        if (Compare.ExactMatch(lockedOne, x, y, MappedTwo.Width, MappedTwo.Height, lockedTwo))
                        {
                            yield return new Point(x, y);

                            y += MappedTwo.Height; //Move so we don't get a bunch of matches here
                        }
                    }
                }
            }
            finally
            {
                lockedOne.UnlockBits();
                lockedTwo.UnlockBits();
            }
        }

        public static List<Point> DirectoryContains(string FileOne, string Directory)
        {
            List<Point> Result = new List<Point>();

            string[] Files = System.IO.Directory.GetFiles(Directory);
            foreach (string File in Files)
            {
                Result.AddRange(Contains(FileOne, File));
            }

            return Result;
        }
    }

    public class Output
    {
        public static Bitmap GetScreenshot()
        {
            int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
            
            Bitmap ScreenImage = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics Screenshot = Graphics.FromImage(ScreenImage);
            Screenshot.CopyFromScreen(new Point(0, 0), new Point(0, 0), new Size(ScreenWidth, ScreenHeight));
            
            return ScreenImage;
        }
    
        public static bool ClickOn(string FileOne)
        {
            return ClickOn(FileOne, true);
        }

        public static bool ClickOn(string FileOne, bool LeftClick)
        {
            return ClickOn(Image.FromFile(FileOne), LeftClick);
        }

        public static bool ClickOn(Image FileOne, bool LeftClick)
        {
            return ClickOn(new Bitmap(FileOne), LeftClick);
        }

        public static bool ClickOn(Bitmap MappedOne, bool LeftClick)
        {
            int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
            return ClickArea(MappedOne, 0, 0, ScreenWidth, ScreenHeight, LeftClick);
        }

        private static void ClickPoint(bool leftClick, Point point)
        {
            if (leftClick)
                MouseOutput.LeftClick(point.X, point.Y);
            else
                MouseOutput.RightClick(point.X, point.Y);
        }

        public static bool ClickArea(string FileOne, int X, int Y, int W, int H)
        {
            return ClickArea(FileOne, X, Y, W, H, true);
        }

        public static bool ClickArea(string FileOne, int X, int Y, int W, int H, bool LeftClick)
        {
            return ClickArea(Image.FromFile(FileOne), X, Y, W, H, LeftClick);
        }

        public static bool ClickArea(Image FileOne, int X, int Y, int W, int H, bool LeftClick)
        {
            return ClickArea(new Bitmap(FileOne), X, Y, W, H, LeftClick);
        }

        public static bool ClickArea(Bitmap MappedOne, int X, int Y, int W, int H, bool LeftClick)
        {
            return FindArea(MappedOne, X, Y, W, H).Find().IfPresent(p => ClickPoint(LeftClick, p));
        }

        public static IEnumerable<Point> Find(string MappedOne)
        {
            return Find(new Bitmap(Image.FromFile(MappedOne)));
        }

        public static IEnumerable<Point> Find(Bitmap MappedOne)
        {
            int ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
            int ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
            return FindArea(MappedOne, 0, 0, ScreenWidth, ScreenHeight);
        }

        public static IEnumerable<Point> FindArea(String file, int X, int Y, int W, int H)
        {
            return FindArea(new Bitmap(Image.FromFile(file)), X, Y, W, H);
        }

        public static IEnumerable<Point> FindArea(Bitmap MappedOne, int X, int Y, int W, int H)
        {
            Bitmap ScreenImage = new Bitmap(W, H);
            Graphics Screenshot = Graphics.FromImage(ScreenImage);
            Screenshot.CopyFromScreen(new Point(X, Y), new Point(0, 0), new Size(W, H));

            return Contain.Contains(ScreenImage, MappedOne, 0.9).Select(p => Adjust(X, Y, p, MappedOne));
        }

        private static Point Adjust(int x, int y, Point p, Bitmap mappedOne)
        {
            return new Point(x + p.X + mappedOne.Width / 2, y + p.Y + mappedOne.Height / 2);
        }
    }

    public class Dump
    {
        public static void DumpImage(string File)
        {
            DumpImage(Image.FromFile(File));
        }

        public static void DumpImage(Image File)
        {
            DumpImage(new Bitmap(File));
        }

        public static void DumpImage(Bitmap File)
        {
            for (int x = 0; x < File.Width; x++)
            {
                for (int y = 0; y < File.Height; y++)
                {
                    Color Pixel = File.GetPixel(x, y);
                    Console.Write(Pixel.R.ToString("X").PadLeft(2, '0') + Pixel.G.ToString("X").PadLeft(2, '0') + Pixel.B.ToString("X").PadLeft(2, '0') + " ");
                }

                Console.WriteLine("");
            }
        }
    }
}