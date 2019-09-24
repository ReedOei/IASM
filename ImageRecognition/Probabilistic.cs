using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml;
using System.IO;

using Utility;

namespace ImageRecognition
{
    struct ColorCoordinate
    {
        public Point Position;

        public Color Pixel;

        public ColorCoordinate(Point Position, Color Pixel)
        {
            this.Position = Position;

            this.Pixel = Pixel;
        }
    }

    public class Classifier
    {
        List<ImageClass> Classes;

        public Classifier(string PathName)
        {
            List<string> ClassNames = Directory.GetFiles(PathName).ToList();

            Classes = new List<ImageClass>();

            foreach (string CreateClass in ClassNames)
            {
                Classes.Add(new ImageClass(CreateClass));
            }
        }

        public Classifier(string ImageClass, bool IsSingleImageClass)
        {
            Classes = new List<ImageClass>();

            Classes.Add(new ImageClass(ImageClass));
        }

        public List<string> Classify(string Subject, bool UseProbabilisticCompare)
        {
            return Classify(new Bitmap(Subject), UseProbabilisticCompare);
        }

        public List<string> Classify(Bitmap Subject, bool UseProbabilisticCompare)
        {
            return Classify(new ProbabilisticImage(Subject), UseProbabilisticCompare);
        }

        public List<string> Classify(ProbabilisticImage Subject, bool UseProbabilisticCompare)
        {
            List<string> Results = new List<string>();

            foreach (ImageClass Check in Classes)
            {
                double Aggregate = 0.0;

                if (Check.TestClass(Subject, out Aggregate, UseProbabilisticCompare))
                    Results.Add(Check.Name + ": " + Aggregate);
            }

            return Results;
        }
    }

    public class ImageClass
    {
        public string Name;

        List<ProbabilisticImage> Samples;
        ProbabilisticImage Master;

        public ImageClass(string Path)
        {
            XmlDocument Data = new XmlDocument();
            Data.Load(Path);

            Samples = new List<ProbabilisticImage>();

            Name = Data.DocumentElement.GetElementsByTagName("Name")[0].InnerText;

            XmlNodeList SampleList = Data.DocumentElement.GetElementsByTagName("Sample");
            foreach (XmlNode CreateSample in SampleList)
            {
                Console.WriteLine(CreateSample.InnerText);

                Samples.Add(new ProbabilisticImage(new Bitmap(CreateSample.InnerText)));
            }

            //Master = Probabilistic.CreateMaster(Samples);
        }

        public ImageClass(string Name, List<string> Samples)
        {
            this.Name = Name;

            this.Samples = new List<ProbabilisticImage>();

            foreach (string CreateSample in Samples)
            {
                this.Samples.Add(new ProbabilisticImage(new Bitmap(CreateSample)));
            }

            //Master = ProbabilisticImage.CreateMaster(this.Samples);
        }

        public ImageClass(string Name, List<ProbabilisticImage> Samples, ProbabilisticImage Master)
        {
            this.Name = Name;

            this.Samples = Samples;

            this.Master = Master;
        }

        public bool TestClass(string TestImage, out double Aggregate, bool UseProbabilisticCompare)
        {
            return TestClass(new Bitmap(TestImage), out Aggregate, UseProbabilisticCompare);
        }

        public bool TestClass(Bitmap TestImage, out double Aggregate, bool UseProbabilisticCompare)
        {
            return TestClass(new ProbabilisticImage(TestImage), out Aggregate, UseProbabilisticCompare);
        }

        public bool TestClass(ProbabilisticImage TestImage, out double Aggregate, bool UseProbabilisticCompare)
        {
            double Threshold = 0.0;

            Aggregate = 0.0;

            foreach (ProbabilisticImage TestSample in Samples)
            {
                double Percentage = (UseProbabilisticCompare) ? Probabilistic.ComparePercentage(TestSample, TestImage) : Normal.ComparePercentage(TestSample, TestImage);

                //Console.WriteLine(Percentage);

                if (Percentage > Aggregate)
                    Aggregate = Percentage;
            }

            return Aggregate > Threshold;
        }
    }

    public class ProbabilisticImage
    {
        public Point Center;
        public int Width, Height, R;
        public Color BackgroundColor;

        List<int> Lengths;

        LockBitmap Source;

        public bool IsCircle = false;

        public int SampleCount = 0;

        public ProbabilisticImage(Bitmap Data)
        {
            Width = Data.Width;
            Height = Data.Height;

            Source = new LockBitmap(Data);

            Source.LockBits();

            BackgroundColor = GetBackgroundColor();
            Center = GetCenter();
            Lengths = GetLengths();
            IsCircle = GetShape();

            R = GetRadius();

            SampleCount = GetSampleCount();
        }

        Point GetCenter()
        {
            int X1 = GetOuterHorizontalLineLeft(new Point(Width - 1, Height / 2)).X;
            int X2 = GetOuterHorizontalLineRight(new Point(0, Height / 2)).X;

            int Y1 = GetOuterVerticalLineUp(new Point(Width / 2, Height - 1)).Y;
            int Y2 = GetOuterVerticalLineDown(new Point(Width / 2, 0)).Y;

            Point Result = new Point(
                (X1 + X2) / 2,
                (Y1 + Y2) / 2
                );

            return Result;
        }

        Point GetOuterVerticalLineDown(Point Center)
        {
            int Length = 0;

            for (Length = Center.Y; Length < Height; Length++)
            {
                if (!ImageUtility.RoughColorMatch(Source.GetPixel(Center.X, Length), BackgroundColor, 10))
                    break;
            }

            return new Point(Center.X, Length);
        }

        Point GetOuterVerticalLineUp(Point Center)
        {
            int Length = 0;

            for (Length = Center.Y; Length >= 0; Length--)
            {
                if (!ImageUtility.RoughColorMatch(Source.GetPixel(Center.X, Length), BackgroundColor, 10))
                    break;
            }

            return new Point(Center.X, Length);
        }

        Point GetOuterHorizontalLineRight(Point Center)
        {
            int Length = 0;

            for (Length = Center.X; Length < Width; Length++)
            {
                if (!ImageUtility.RoughColorMatch(Source.GetPixel(Length, Center.Y), BackgroundColor, 10))
                    break;
            }

            return new Point(Length, Center.Y);
        }

        Point GetOuterHorizontalLineLeft(Point Center)
        {
            int Length = 0;

            for (Length = Center.X; Length >= 0; Length--)
            {
                if (!ImageUtility.RoughColorMatch(Source.GetPixel(Length, Center.Y), BackgroundColor, 10))
                    break;
            }

            return new Point(Length, Center.Y);
        }

        Point GetOuterNegativeLineUp(Point Center)
        {
            int Length = 0;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length >= 0; Length--)
            {
                if (Source.GetPixel(Length, Height - Length - 1) != BackgroundColor)
                    break;
            }

            return new Point(Length, Height - Length - 1);
        }

        Point GetOuterNegativeLineDown(Point Center)
        {
            int Length = 0;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length < Utility.Utility.GetLowest(Width, Height); Length++)
            {
                if (Source.GetPixel(Length, Height - Length - 1) != BackgroundColor)
                    break;
            }

            return new Point(Length, Height - Length - 1);
        }

        Point GetOuterPositiveLineUp(Point Center)
        {
            int Length = 0;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length >= 0; Length--)
            {
                if (Source.GetPixel(Width - Length - 1, Length) != BackgroundColor)
                    break;
            }

            return new Point(Length, Length);
        }

        Point GetOuterPositiveLineDown(Point Center)
        {
            int Length = 0;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length >= 0; Length--)
            {
                if (Source.GetPixel(Length, Height - Length - 1) != BackgroundColor)
                    break;
            }

            return new Point(Length, Length);
        }

        Point GetVerticalLineDown(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Center.Y; Length < Height; Length ++)
            {
                if (Source.GetPixel(Center.X, Length) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Center.X, Length);
        }

        Point GetVerticalLineUp(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Center.Y; Length >= 0; Length--)
            {
                if (Source.GetPixel(Center.X, Length) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Center.X, Length);
        }

        Point GetHorizontalLineRight(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Center.X; Length < Width; Length++)
            {
                if (Source.GetPixel(Length, Center.Y) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Length, Center.Y);
        }

        Point GetHorizontalLineLeft(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Center.X; Length >= 0; Length--)
            {
                if (Source.GetPixel(Length, Center.Y) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Length, Center.Y);
        }

        Point GetNegativeLineUp(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length >= 0; Length--)
            {
                if (Source.GetPixel(Length, Height - Length - 1) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Length, Height - Length - 1);
        }

        Point GetNegativeLineDown(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length < Utility.Utility.GetLowest(Width, Height); Length++)
            {
                if (Source.GetPixel(Length, Height - Length - 1) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Length, Height - Length - 1);
        }

        Point GetPositiveLineUp(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length >= 0; Length--)
            {
                if (Source.GetPixel(Length, Length) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Length, Length);
        }

        Point GetPositiveLineDown(Point Center)
        {
            int Length = 0;
            bool First = false;

            for (Length = Utility.Utility.GetLowest(Center.X, Center.Y); Length < Utility.Utility.GetLowest(Width, Height); Length++)
            {
                if (Source.GetPixel(Length, Length) == BackgroundColor)
                {
                    if (First)
                        break;
                }
                else
                    First = true;
            }

            return new Point(Length, Length);
        }

        int GetVerticalLength(bool Up, Point Center)
        {
            if (Up)
                return Math.Abs(GetOuterVerticalLineUp(new Point(Center.X, Height - 1)).Y - Center.Y);
            else
                return Math.Abs(GetOuterVerticalLineDown(new Point(Center.X, 0)).Y - Center.Y);
        }

        int GetHorizontalLength(bool Left, Point Center)
        {
            if (Left)
                return Math.Abs(GetOuterHorizontalLineLeft(new Point(Width - 1, Center.Y)).X - Center.X);
            else
                return Math.Abs(GetOuterHorizontalLineRight(new Point(0, Center.Y)).X - Center.X);
        }
        
        int GetNegativeLength(bool Up, Point Center)
        {
            if (Up)
                return (int)Utility.Utility.Distance(GetOuterNegativeLineUp(new Point(Width - 1, Height - 1)), Center);
            else
                return (int)Utility.Utility.Distance(GetOuterNegativeLineDown(new Point(0, 0)), Center);
        }

        int GetPositiveLength(bool Up, Point Center)
        {
            if (Up)
                return (int)Utility.Utility.Distance(GetOuterPositiveLineUp(new Point(Width - 1, Height - 1)), Center);
            else
                return (int)Utility.Utility.Distance(GetOuterPositiveLineDown(new Point(Width - 1, Height - 1)), Center);
        }

        List<int> GetLengths()
        {
            List<int> Result = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };

            Result[0] = GetVerticalLength(true, Center);
            Result[1] = GetVerticalLength(false, Center);

            Result[2] = GetHorizontalLength(true, Center);
            Result[3] = GetHorizontalLength(false, Center);

            Result[4] = GetNegativeLength(true, Center);
            Result[5] = GetNegativeLength(false, Center);

            Result[6] = GetPositiveLength(true, Center);
            Result[7] = GetPositiveLength(false, Center);

            return Result;
        }

        bool GetShape()
        {
            double MeanStraightLines = new List<double> { Lengths[0], Lengths[1], Lengths[2], Lengths[3] }.Average();
            double MeanDiagonalLines = new List<double> { Lengths[4], Lengths[5], Lengths[6], Lengths[7] }.Average();

            double Result = MeanStraightLines * Math.Sqrt(2) / 1.05;

            //Console.WriteLine("MeanStraightLines: " + MeanStraightLines);
            //Console.WriteLine("MeanDiagonalLines: " + MeanDiagonalLines);
            //Console.WriteLine("Result: " + Result);

            return Result > MeanDiagonalLines;
        }

        int GetRadius()
        {
            return (int)Lengths.Min();
        }

        int GetSampleCount()
        {
            int Result = 0;

            List<double> ColorWeights = new List<double>();
            List<Color> Colors = GetPrimaryColors(out ColorWeights);

            double PixelCount = ColorWeights.Sum();

            //Console.WriteLine("Primary Color: " + Colors[ColorWeights.IndexOf(ColorWeights.Max())]);

            ColorWeights = ColorWeights.Where(N => N != 1).ToList();

            foreach (double Add in ColorWeights)
            {
                if ((Add / PixelCount) > 0.004)
                {
                    double Trials = Math.Pow(Add / PixelCount, -1);

                    Result += (int)Trials;
                }
            }

            return Result;
        }

        List<ColorCoordinate> GetColorCoodinates()
        {
            //Find all colors and their associated coordinates
            List<ColorCoordinate> Colors = new List<ColorCoordinate>();

            if (IsCircle)
            {
                for (double t = 0.0; t < 2 * Math.PI; t += 0.1)
                {
                    for (double r = 0; r < R; r++)
                    {
                        int X = (int)(Math.Cos(t) * r) + Center.X;
                        int Y = (int)(Math.Sin(t) * r) + Center.Y;

                        X = Utility.Utility.Clamp(Utility.Utility.LowerBound(X, 0), Width - 1);
                        Y = Utility.Utility.Clamp(Utility.Utility.LowerBound(Y, 0), Height - 1);

                        Color CurrentPixel = Source.GetPixel(X, Y);

                        Colors.Add(new ColorCoordinate(new Point(X, Y), CurrentPixel));
                    }
                }
            }
            else
            {
                for (int x = -R; x < R; x += 2)
                {
                    for (int y = -R; y < R; y += 2)
                    {
                        int X = x + Center.X;
                        int Y = y + Center.Y;

                        X = Utility.Utility.Clamp(Utility.Utility.LowerBound(X, 0), Width - 1);
                        Y = Utility.Utility.Clamp(Utility.Utility.LowerBound(Y, 0), Height - 1);

                        Color CurrentPixel = Source.GetPixel(X, Y);

                        Colors.Add(new ColorCoordinate(new Point(X, Y), CurrentPixel));
                    }
                }
            }

            return Colors;
        }

        List<Color> GetPrimaryColors(out List<double> ColorWeights)
        {
            List<Color> Colors = GetColorCoodinates().Select(N => N.Pixel).ToList();

            ColorWeights = Colors.GroupBy(N => new { N.R, N.G, N.B }).Select(N => Convert.ToDouble(N.Count())).ToList();

            return Colors;
        }

        Color GetBackgroundColor()
        {
            List<Color> Result = new List<Color>();

            //Top and bottom
            for (int x = 0; x < Width; x++)
            {
                Result.Add(Source.GetPixel(x, 0));
                Result.Add(Source.GetPixel(x, Height - 1));
            }

            //Left and right
            for (int y = 0; y < Height; y++)
            {
                Result.Add(Source.GetPixel(0, y));
                Result.Add(Source.GetPixel(Width - 1, y));
            }

            //Result = Result.OrderBy(N => Counts[Result.IndexOf(N)]).ToList();

            //Get the total weighted number of colors
            int AverageR = 0, AverageG = 0, AverageB = 0;
            for (int i = 0; i < Result.Count; i++)
            {
                AverageR += Result[i].R;
                AverageG += Result[i].G;
                AverageB += Result[i].B;
            }

            //Get the mean
            AverageR /= Result.Count();
            AverageG /= Result.Count();
            AverageB /= Result.Count();

            List<Color> Results = Result.GroupBy(N => new { N.R, N.G, N.B }).Select(N => N.First()).ToList();

            //Color AverageResult = Color.FromArgb(AverageR, AverageG, AverageB);

            return Results.First();
        }

        /// <summary>
        /// Gets the relative point (X, Y) in this image. Takes into account offsets.
        /// </summary>
        /// <param name="X">The relative x coordinate. Should be between -1 and 1.</param>
        /// <param name="Y">The relative y coordinate. Should be between -1 and 1.</param>
        /// <returns>The point x percent across the x axis and y percent across the y axis of the picture.</returns>
        public Point GetCircularPoint(double X, double Y)
        {
            Point Result = new Point();

            X = (X + 1) / 2;
            Y = (Y + 1) / 2;

            Result.X = Utility.Utility.Clamp(Utility.Utility.LowerBound((int)(Math.Cos(X * 2 * Math.PI) * R + Center.X), 0), Width - 1);
            Result.Y = Utility.Utility.Clamp(Utility.Utility.LowerBound((int)(Math.Sin(Y * 2 * Math.PI) * R + Center.Y), 0), Height - 1);

            return Result;
        }

        public Point GetRectangularPoint(double X, double Y)
        {
            Point Result = new Point();

            Result.X = Utility.Utility.Clamp(Utility.Utility.LowerBound((int)(X * R + Center.X), 0), Width - 1);
            Result.Y = Utility.Utility.Clamp(Utility.Utility.LowerBound((int)(Y * R + Center.Y), 0), Height - 1);

            return Result;
        }

        public Color GetPixel(Point Position)
        {
            return GetPixel(Position.X, Position.Y);
        }

        public Color GetPixel(int X, int Y)
        {
            return Source.GetPixel(X, Y);
        }

        public static ProbabilisticImage Combine(string A, string B)
        {
            return Combine(new Bitmap(A), new Bitmap(B));
        }

        public static ProbabilisticImage Combine(Bitmap A, Bitmap B)
        {
            return Combine(new ProbabilisticImage(A), new ProbabilisticImage(B));
        }

        public static ProbabilisticImage Combine(ProbabilisticImage A, ProbabilisticImage B)
        {
            int SampleCount = Utility.Utility.GetGreatest(A.SampleCount, B.SampleCount);

            Bitmap Result = new Bitmap(A.Source.Width, A.Source.Height);

            for (int i = 0; i < SampleCount; i++ )
            {
                double RelativeX = Utility.Utility.Generator.NextDouble() * 2.0 - 1.0;
                double RelativeY = Utility.Utility.Generator.NextDouble() * 2.0 - 1.0;

                Point ACompare = new Point();
                Point BCompare = new Point();

                if (A.IsCircle && B.IsCircle)
                {
                    ACompare = A.GetCircularPoint(RelativeX, RelativeY);
                    BCompare = B.GetCircularPoint(RelativeX, RelativeY);
                }
                else
                {
                    ACompare = A.GetRectangularPoint(RelativeX, RelativeY);
                    BCompare = B.GetRectangularPoint(RelativeX, RelativeY);
                }

                Color APixel = A.GetPixel(ACompare);
                Color BPixel = B.GetPixel(BCompare);

                if (ImageUtility.ColorCheck(A.BackgroundColor, APixel) ^ ImageUtility.ColorCheck(B.BackgroundColor, BPixel))
                    continue;
                else if (ImageUtility.ColorCheck(A.BackgroundColor, APixel) && ImageUtility.ColorCheck(B.BackgroundColor, BPixel))
                {
                    Result.SetPixel(ACompare.X, ACompare.Y, APixel);
                    continue;
                }
                else if (ImageUtility.ColorCheck(APixel, BPixel))
                {
                    Result.SetPixel(ACompare.X, ACompare.Y, APixel);
                }
            }

            return new ProbabilisticImage(Result);
        }

        public static ProbabilisticImage CreateMaster(List<ProbabilisticImage> Samples)
        {
            ProbabilisticImage A = Samples.First();

            Samples.RemoveAt(0);

            while (Samples.Count() > 0)
            {
                A = Combine(A, Samples.First());

                Samples.RemoveAt(0);
            }

            return A;
        }

        public void Save(string FileName)
        {
            Source.UnlockBits();

            Source.source.Save(FileName);

            Source.LockBits();
        }

        public override string ToString()
        {
            string Result = "Width: " + Width + " Height: " + Height + "\n"
                + "Shape: " + (IsCircle ? "Circular" : "Squarical") + "\n"
                + "Lengths: {" + String.Join(", ", Lengths) + "}\n"
                + "Background Color: " + BackgroundColor + "\n"
                + "Center: " + Center + "\n"
                + "Sample Count: " + SampleCount + "\n"
                + "R: " + R;

            return Result;
        }
    }

    public class Normal
    {
        public static double CompareColorPercentage(string FileOne, string FileTwo)
        {
            return CompareColorPercentage(Image.FromFile(FileOne), Image.FromFile(FileTwo));
        }

        public static double CompareColorPercentage(Image FileOne, Image FileTwo)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist).");
                return 0.0;
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist).");
                return 0.0;
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return CompareColorPercentage(MappedOne, MappedTwo, true);
        }

        public static double CompareColorPercentage(Bitmap A, Bitmap B, bool RoughColorMatch)
        {
            return CompareColorPercentage(new ProbabilisticImage(A), new ProbabilisticImage(B), RoughColorMatch);
        }

        public static double CompareColorPercentage(ProbabilisticImage A, ProbabilisticImage B, bool RoughColorMatch)
        {
            double PointCount = Math.Pow(A.R, 2);
            double Amount = 0.0, IncreaseAmount = 1.0 / PointCount;

            for (int x = -A.R; x < A.R; x++)
            {
                for (int y = -A.R; y < A.R; y++)
                {
                    double RelativeX = (double)x / (double)A.R;
                    double RelativeY = (double)y / (double)A.R;

                    Point ACompare = new Point();
                    Point BCompare = new Point();

                    if (A.IsCircle && B.IsCircle)
                    {
                        ACompare = A.GetCircularPoint(RelativeX, RelativeY);
                        BCompare = B.GetCircularPoint(RelativeX, RelativeY);
                    }
                    else
                    {
                        ACompare = A.GetRectangularPoint(RelativeX, RelativeY);
                        BCompare = B.GetRectangularPoint(RelativeX, RelativeY);
                    }

                    Color APixel = A.GetPixel(ACompare);
                    Color BPixel = B.GetPixel(BCompare);

                    //Console.WriteLine("{0} == {1} = {2}", APixel.ToString(), B.GetPixel(BCompare).ToString(), ImageUtility.ColorCheck(APixel, B.GetPixel(BCompare)));

                    if (ImageUtility.ColorCheck(A.BackgroundColor, APixel) ^ ImageUtility.ColorCheck(B.BackgroundColor, BPixel))
                        continue;
                    else if (ImageUtility.ColorCheck(A.BackgroundColor, APixel) && ImageUtility.ColorCheck(B.BackgroundColor, BPixel))
                    {
                        Amount += IncreaseAmount;
                        continue;
                    }
                    else if (ImageUtility.ColorCheck(APixel, BPixel))
                    {
                        Amount += IncreaseAmount;
                    }
                }
            }

            return Amount;
        }

        public static double ComparePercentage(string FileOne, string FileTwo)
        {
            return ComparePercentage(Image.FromFile(FileOne), Image.FromFile(FileTwo));
        }

        public static double ComparePercentage(Image FileOne, Image FileTwo)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist).");
                return 0.0;
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist).");
                return 0.0;
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return ComparePercentage(MappedOne, MappedTwo);
        }

        public static double ComparePercentage(Bitmap FileOne, Bitmap FileTwo)
        {
            return ComparePercentage(new ProbabilisticImage(FileOne), new ProbabilisticImage(FileTwo));
        }

        public static double ComparePercentage(ProbabilisticImage FileOne, ProbabilisticImage FileTwo)
        {
            double ColorPercentage = CompareColorPercentage(FileOne, FileTwo, true);
            double ShapePercentage = CompareShapePercentage(FileOne, FileTwo);

            //Console.WriteLine("{0} percentage of color samples match.", ColorPercentage);
            //Console.WriteLine("{0} percentage of shape samples match.", ShapePercentage);

            return ColorPercentage / 2 + ShapePercentage / 2;
        }

        public static double CompareShapePercentage(string FileOne, string FileTwo)
        {
            return CompareShapePercentage(Image.FromFile(FileOne), Image.FromFile(FileTwo));
        }

        public static double CompareShapePercentage(Image FileOne, Image FileTwo)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist).");
                return 0.0;
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist).");
                return 0.0;
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return CompareShapePercentage(MappedOne, MappedTwo);
        }

        public static double CompareShapePercentage(Bitmap A, Bitmap B)
        {
            return CompareShapePercentage(new ProbabilisticImage(A), new ProbabilisticImage(B));
        }

        public static double CompareShapePercentage(ProbabilisticImage A, ProbabilisticImage B)
        {
            double PointCount = Math.Pow(A.R, 2);
            double Amount = 0.0, IncreaseAmount = 1.0 / PointCount;

            for (int x = -A.R; x < A.R; x++)
            {
                for (int y = -A.R; y < A.R; y++)
                {
                    double RelativeX = (double)x / (double)A.R;
                    double RelativeY = (double)y / (double)A.R;

                    Point ACompare = new Point();
                    Point BCompare = new Point();

                    if (A.IsCircle && B.IsCircle)
                    {
                        ACompare = A.GetCircularPoint(RelativeX, RelativeY);
                        BCompare = B.GetCircularPoint(RelativeX, RelativeY);
                    }
                    else
                    {
                        ACompare = A.GetRectangularPoint(RelativeX, RelativeY);
                        BCompare = B.GetRectangularPoint(RelativeX, RelativeY);
                    }

                    Color APixel = A.GetPixel(ACompare);
                    Color BPixel = B.GetPixel(BCompare);

                    //Console.WriteLine("{0} == {1} = {2}", APixel.ToString(), B.GetPixel(BCompare).ToString(), ImageUtility.ColorCheck(APixel, B.GetPixel(BCompare)));

                    //If neither is a background color, then it must be part of the main picture, and therefore the shape is the same here.
                    if ((!ImageUtility.ColorCheck(A.BackgroundColor, APixel)) == (!ImageUtility.ColorCheck(B.BackgroundColor, BPixel)))
                        Amount += IncreaseAmount;
                }
            }

            return Amount;
        }
    }

    public class Probabilistic
    {
        public static double CompareColorPercentage(string FileOne, string FileTwo)
        {
            return CompareColorPercentage(Image.FromFile(FileOne), Image.FromFile(FileTwo));
        }

        public static double CompareColorPercentage(Image FileOne, Image FileTwo)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist).");
                return 0.0;
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist).");
                return 0.0;
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return CompareColorPercentage(MappedOne, MappedTwo, true);
        }

        public static double CompareColorPercentage(Bitmap A, Bitmap B, bool RoughColorMatch)
        {
            return CompareColorPercentage(new ProbabilisticImage(A), new ProbabilisticImage(B), RoughColorMatch);
        }

        public static double CompareColorPercentage(ProbabilisticImage A, ProbabilisticImage B, bool RoughColorMatch)
        {
            double PointCount = Utility.Utility.GetGreatest(A.SampleCount, B.SampleCount);
            double Amount = 0.0, IncreaseAmount = 1.0 / PointCount;

            Console.WriteLine($"Sampling {PointCount} pixels.");

            for (int i = 0; i < PointCount; i++)
            {
                double RelativeX = Utility.Utility.Generator.NextDouble() * 2.0 - 1.0;
                double RelativeY = Utility.Utility.Generator.NextDouble() * 2.0 - 1.0;

                Point ACompare = new Point();
                Point BCompare = new Point();

                if (A.IsCircle && B.IsCircle)
                {
                    ACompare = A.GetCircularPoint(RelativeX, RelativeY);
                    BCompare = B.GetCircularPoint(RelativeX, RelativeY);
                }
                else
                {
                    ACompare = A.GetRectangularPoint(RelativeX, RelativeY);
                    BCompare = B.GetRectangularPoint(RelativeX, RelativeY);
                }

                Color APixel = A.GetPixel(ACompare);
                Color BPixel = B.GetPixel(BCompare);

                //Console.WriteLine("{0} == {1} = {2}", APixel.ToString(), B.GetPixel(BCompare).ToString(), ImageUtility.ColorCheck(APixel, B.GetPixel(BCompare)));

                if (ImageUtility.ColorCheck(A.BackgroundColor, APixel) ^ ImageUtility.ColorCheck(B.BackgroundColor, BPixel))
                    continue;
                else if (ImageUtility.ColorCheck(A.BackgroundColor, APixel) && ImageUtility.ColorCheck(B.BackgroundColor, BPixel))
                {
                    Amount += IncreaseAmount;
                    continue;
                }
                else if (ImageUtility.ColorCheck(APixel, BPixel))
                {
                    Amount += IncreaseAmount;
                }
            }

            return Amount;
        }

        public static double ComparePercentage(string FileOne, string FileTwo)
        {
            return ComparePercentage(Image.FromFile(FileOne), Image.FromFile(FileTwo));
        }

        public static double ComparePercentage(Image FileOne, Image FileTwo)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist).");
                return 0.0;
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist).");
                return 0.0;
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return ComparePercentage(MappedOne, MappedTwo);
        }

        public static double ComparePercentage(Bitmap FileOne, Bitmap FileTwo)
        {
            return ComparePercentage(new ProbabilisticImage(FileOne), new ProbabilisticImage(FileTwo));
        }

        public static double ComparePercentage(ProbabilisticImage FileOne, ProbabilisticImage FileTwo)
        {
            double ColorPercentage = CompareColorPercentage(FileOne, FileTwo, true);
            double ShapePercentage = CompareShapePercentage(FileOne, FileTwo);

            //Console.WriteLine("{0} percentage of color samples match.", ColorPercentage);
            //Console.WriteLine("{0} percentage of shape samples match.", ShapePercentage);

            return ColorPercentage / 2 + ShapePercentage / 2;
        }

        public static double CompareShapePercentage(string FileOne, string FileTwo)
        {
            return CompareShapePercentage(Image.FromFile(FileOne), Image.FromFile(FileTwo));
        }

        public static double CompareShapePercentage(Image FileOne, Image FileTwo)
        {
            if (FileOne == null)
            {
                Console.WriteLine("File one is invalid (probably does not exist).");
                return 0.0;
            }
            else if (FileTwo == null)
            {
                Console.WriteLine("File two is invalid (probably does not exist).");
                return 0.0;
            }

            Bitmap MappedOne = new Bitmap(FileOne);
            Bitmap MappedTwo = new Bitmap(FileTwo);

            return CompareShapePercentage(MappedOne, MappedTwo);
        }

        public static double CompareShapePercentage(Bitmap A, Bitmap B)
        {
            return CompareShapePercentage(new ProbabilisticImage(A), new ProbabilisticImage(B));
        }

        public static double CompareShapePercentage(ProbabilisticImage A, ProbabilisticImage B)
        {
            double PointCount = Utility.Utility.GetGreatest(A.SampleCount, B.SampleCount);
            double Amount = 0.0, IncreaseAmount = 1.0 / PointCount;

            for (int i = 0; i < PointCount; i++)
            {
                double RelativeX = Utility.Utility.Generator.NextDouble() * 2.0 - 1.0;
                double RelativeY = Utility.Utility.Generator.NextDouble() * 2.0 - 1.0;

                Point ACompare = new Point();
                Point BCompare = new Point();

                if (A.IsCircle && B.IsCircle)
                {
                    ACompare = A.GetCircularPoint(RelativeX, RelativeY);
                    BCompare = B.GetCircularPoint(RelativeX, RelativeY);
                }
                else
                {
                    ACompare = A.GetRectangularPoint(RelativeX, RelativeY);
                    BCompare = B.GetRectangularPoint(RelativeX, RelativeY);
                }

                //If neither is a background color, then it must be part of the main picture, and therefore the shape is the same here.
                if ((!ImageUtility.ColorCheck(A.BackgroundColor, A.GetPixel(ACompare))) == (!ImageUtility.ColorCheck(B.BackgroundColor, B.GetPixel(BCompare))))
                    Amount += IncreaseAmount;
            }

            return Amount;
        }

        public static List<Point> Contains(string FileOne, string FileTwo)
        {
            return Contains(Image.FromFile(FileOne), Image.FromFile(FileTwo), 1.0);
        }

        public static List<Point> Contains(string FileOne, string FileTwo, double Percentage)
        {
            return Contains(Image.FromFile(FileOne), Image.FromFile(FileTwo), Percentage);
        }

        public static List<Point> Contains(Image FileOne, Image FileTwo, double Percentage)
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

        public static List<Point> Contains(Bitmap MappedOne, Bitmap MappedTwo, double Percentage)
        {
            List<Point> Result = new List<Point>();

            for (int x = 0; x < (MappedOne.Width - MappedTwo.Width + 1); x++)
            {
                for (int y = 0; y < (MappedOne.Height - MappedTwo.Height + 1); y++)
                {
                    //If they meet the specified percentage (or more of course) it works
                    if (ComparePercentage(MappedOne.Clone(new Rectangle(x, y, MappedTwo.Width, MappedTwo.Height), MappedOne.PixelFormat), MappedTwo) >= Percentage)
                    {
                        Result.Add(new Point(x, y));

                        y += MappedTwo.Height; //Move so we don't get a bunch of matches here
                    }
                }
            }

            return Result;
        }
    }
}
