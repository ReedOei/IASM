using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

public class QuickImage
{
    private List<List<Color>> Pixels;
    public Bitmap Source;

    public QuickImage(Image Source)
    {
        Bitmap MappedSource = new Bitmap(Source);
        this.Source = MappedSource;

        Refresh();
    }
    
    public QuickImage(Bitmap Source)
    {
        this.Source = Source;
        
        Refresh();
    }

    public Bitmap Save(string Name)
    {
        Offload();
        Source.Save(Name);

        return Source;
    }

    public Bitmap Offload()
    {
        for (int x = 0; x < Pixels.Count; x++)
        {
            for (int y = 0; y < Pixels[x].Count; y++)
            {
                Source.SetPixel(x, y, Pixels[x][y]);
            }
        }

        return Source;
    }

    public void Refresh()
    {
        Pixels = new List<List<Color>>();

        for (int x = 0; x < Source.Width; x++)
        {
            Pixels.Add(new List<Color>());

            for (int y = 0; y < Source.Height; y++)
            {
                Pixels[x].Add(Source.GetPixel(x, y));
            }
        }
    }

    public Color GetPixel(int x, int y)
    {
        return Pixels[x][y];
    }

    public void SetPixel(int x, int y, Color Value)
    {
        Pixels[x][y] = Value;
    }

    public QuickImage Crop(Rectangle Area)
    {
        Source = Source.Clone(Area, Source.PixelFormat);

        Refresh();
        return this;
    }

    public IEnumerator<List<Color>> GetEnumerator()
    {
        return Pixels.GetEnumerator();
    }

    public Rectangle HeightBounds(Color Blank)
    {
        Point UpperBounds = GetColor(0, 0, 0, 1, Color.Black);
        Point LowerBounds = GetColor(Source.Width - 1, Source.Height - 1, 0, -1, Color.Black);

        return new Rectangle(UpperBounds.X, UpperBounds.Y, LowerBounds.X - UpperBounds.X + 1, LowerBounds.Y - UpperBounds.Y + 1);
    }

    public Rectangle HeightBoundLine(Color Blank)
    {
        Rectangle Result = new Rectangle(-1, -1, -1, -1);

        Point UpperBounds = GetColor(0, 0, 0, 1, Color.Black);

        //Find the upper bounds
        for (int y = 0; y < Source.Height; y++)
        {
            if ((Result.X == -1) || (Result.Y == -1))
            {
                for (int x = 0; x < Source.Width; x++)
                {
                    if (!ImageUtility.ColorMatch(GetPixel(x, y), Blank))
                    {
                        Result.X = x;
                        Result.Y = y;

                        break;
                    }
                }
            }
            else
                break;
        }

        Point LowerBounds = GetLine(Result.X, Result.Y, Source.Height, 0, 1, Color.White);

        //Find the lower bounds
        for (int y = Result.Y; y < Source.Height; y++)
        {
            if ((Result.Width == -1) || (Result.Height == -1))
            {
                bool Line = true;

                for (int x = Result.X; x < Source.Width; x++)
                {
                    if (!ImageUtility.ColorMatch(GetPixel(x, y), Blank))
                        Line = false;
                }

                if (Line)
                {
                    Result.Height = y - Result.Y + 1;
                    break;
                }
            }
            else
                break;
        }

        return Result;
    }

    public Rectangle WidthBounds(Color Blank)
    {
        Point LeftBounds = GetColor(0, 0, 1, 1, Color.Black);
        Point RightBounds = GetColor(Source.Width - 1, Source.Height - 1, 1, -1, Color.Black);

        return new Rectangle(LeftBounds.X, LeftBounds.Y, RightBounds.X - LeftBounds.X, RightBounds.Y - LeftBounds.Y);
    }

    public Rectangle WidthBoundLine(Color Blank)
    {
        Rectangle Result = new Rectangle(-1, -1, -1, -1);

        //Find the left bounds
        for (int x = 0; x < Source.Width; x++)
        {
            if ((Result.X == -1) || (Result.Y == -1))
            {
                for (int y = 0; y < Source.Height; y++)
                {
                    if (!ImageUtility.ColorMatch(GetPixel(x, y), Blank))
                    {
                        Result.X = x;
                        Result.Y = y;

                        break;
                    }
                }
            }
            else
                break;
        }

        //Find the right bounds
        for (int x = Result.X; x < Source.Width; x++)
        {
            if ((Result.Width == -1) || (Result.Height == -1))
            {
                bool Line = true;

                for (int y = Result.Y; y < Source.Height; y++)
                {
                    if (!ImageUtility.ColorMatch(GetPixel(x, y), Blank))
                        Line = false;
                }

                if (Line)
                {
                    Result.Width = x - Result.X + 1;
                    break;
                }
            }
            else
                break;
        }

        return Result;
    }

    delegate int Order(int Direction, int x, int y);

    public Point GetColor(int X, int Y, int Direction, int TravelDirection, Color Find)
    {
        return GetLine(X, Y, 1, Direction, TravelDirection, Find);
    }

    public Point GetLine(int X, int Y, int Length, int Direction, int TravelDirection, Color Find)
    {
        //0 is top down. This means we run through the x's at each height.
        Order GetX = (D, PX, PY) => (D == 0) ? PY : PX;
        Order GetY = (D, PX, PY) => (D == 0) ? PX : PY;

        int Width = (TravelDirection < 0) ? 0 : Source.Width;
        int Height = (TravelDirection < 0) ? 0 : Source.Height;

        for (int x = GetX(Direction, X, Y); x != GetX(Direction, Width, Height); x += TravelDirection)
        {
            for (int y = GetY(Direction, X, Y); y != GetY(Direction, Width, Height); y += TravelDirection)
            {
                bool Match = true;

                for (int i = y; i != (Length * TravelDirection + y); i += TravelDirection)
                {
                    if (!ImageUtility.ColorMatch(GetPixel(GetX(Direction, x, i), GetY(Direction, x, i)), Find))
                        Match = false;
                }

                if (Match)
                    return new Point(GetX(Direction, x, y), GetY(Direction, x, y));
            }
        }

        return Point.Empty;
    }

    public Bitmap ColorAllPixels(Color Replace, params Color[] Ignore)
    {
        for (int x = 0; x < Source.Width; x++)
        {
            for (int y = 0; y < Source.Height; y++)
            {
                bool Match = false;
                foreach (Color IgnoreColor in Ignore)
                {
                    if (ImageUtility.ColorMatch(GetPixel(x, y), IgnoreColor))
                    {
                        Match = true;
                        break;
                    }
                }

                if (!Match)
                    SetPixel(x, y, Replace);
            }
        }

        return Offload();
    }
}

class ImageUtility
{
    public static Image CloseCrop(Image Source, string Name)
    {
        QuickImage Check = new QuickImage(Source);

        Rectangle AreaHeight = Check.HeightBounds(Color.White);
        Rectangle AreaWidth = Check.WidthBounds(Color.White);

        Rectangle Area = new Rectangle(-1, -1, -1, -1);

        Area.X = AreaWidth.X;
        Area.Y = AreaHeight.Y;

        Area.Width = AreaWidth.Width;
        Area.Height = AreaHeight.Height;

        return (Image)Check.Crop(Area).Save(Name);
    }

    public static Bitmap Scale(Bitmap Source, double Percentage)
    {
        return Scale(Source, new Rectangle(0, 0, (int)(Source.Width * Percentage), (int)(Source.Height * Percentage)));
    }

    public static Bitmap Scale(Bitmap Source, Rectangle Size)
    {
        if (Source.Size != new Size(Size.Width, Size.Height))
        {
            Bitmap Result = new Bitmap(Source, new Size(Size.Width, Size.Height));
            Result.SetResolution(Source.HorizontalResolution, Source.VerticalResolution);

            Graphics Resizer = Graphics.FromImage(Result);
            //Resizer.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            Resizer.DrawImage(Source, Size);

            return Result;
        }
        else
            return Source;
    }

    public static Size ExtractTextSize(Bitmap Source)
    {
        Size Result = new Size();

        QuickImage Check = new QuickImage(Source);

        Rectangle AreaHeight = Check.HeightBoundLine(Color.White);
        Rectangle AreaWidth = Check.WidthBoundLine(Color.White);

        Result.Width = AreaWidth.Width - 1;
        Result.Height = AreaHeight.Height - 1;

        return Result;
    }

    public static bool IsWhite(Color Check)
    {
        return (Check.R == 255) && (Check.G == 255) && (Check.B == 255);
    }
    
    /*
    public static List<Bitmap> CreateFontLetters(string FontName, int Size)
    {
        List<Bitmap> Result = new List<Bitmap>();
    
        for (int i = 33: i < 127; i++)
        {
            Bitmap NewLetter = new Bitmap(100, 100);
            
            Point Location = new Point(0, 0);
            
            using (Graphics graphics = Graphics.FromImage(NewLetter))
            {
                using (Font newFont = new Font(FontName, Size))
                {
                    graphics.DrawString(Convert.ToChar(i), newFont, Brushes.White, Location);
                }
            }
        
            Result.Add(NewLetter);
        }
        
        return Result;
    }*/
    
    public static List<Bitmap> SaveFontLetters(string FontName, int Size, List<Bitmap> FontLetters)
    {
        for (int i = 0; i < FontLetters.Count(); i++)
        {
            FontLetters[i].Save(System.IO.Directory.GetCurrentDirectory() + "\\"  + FontName + "_" + Size.ToString() + "\\" + Convert.ToChar(33 + i) + ".bmp");
        }
        
        return FontLetters;
    }
    
    public static void CloseCropDirectory(string DirectoryName)
    {
        string Directory = System.IO.Directory.GetCurrentDirectory() + "\\" + DirectoryName + "\\";
        string[] Files = System.IO.Directory.GetFiles(Directory);

        foreach (string File in Files)
        {
            ImageUtility.CloseCrop(Image.FromFile(File), Directory + Utility.Utility.StripDirectoryName(File, Directory));
        }
    }
    
    /*
    public static void GenerateFontImages(string FontName, int Size)
    {
        SaveFontLetters(FontName, Size, CreateFontLetters(FontName, Size));
        CloseCropDirectory(FontName + "_" + Size.ToString());
    }*/

    public static Color AverageColor(QuickImage Source)
    {
        int R = 0;
        int G = 0;
        int B = 0;
        int Count = Source.Source.Width * Source.Source.Height;

        for (int x = 0; x < Source.Source.Width; x++)
        {
            for (int y = 0; y < Source.Source.Height; y++)
            {
                Color Pixel = Source.GetPixel(x, y);
                R += Pixel.R;
                G += Pixel.G;
                B += Pixel.B;
            }
        }

        return Color.FromArgb(R / Count, G / Count, B / Count);
    }

    public static Color AverageColor(Color A, Color B)
    {
        return Color.FromArgb((A.R + B.R) / 2, (A.G + B.G) / 2, (A.B + B.B) / 2);
    }

    public static Color ModeColor(QuickImage Source)
    {
        List<Color> Colors = new List<Color>();
        List<int> ColorCount = new List<int>();

        for (int x = 0; x < Source.Source.Width; x++)
        {
            for (int y = 0; y < Source.Source.Height; y++)
            {
                Color Pixel = Source.GetPixel(x, y);

                if (!Colors.Contains(Pixel))
                {
                    Colors.Add(Pixel);
                    ColorCount.Add(1);
                }
                else
                    ColorCount[Colors.IndexOf(Pixel)]++;
            }
        }

        return Colors[Utility.Utility.Greatest(ColorCount)];
    }

    public static bool ColorMatch(Color One, Color Two)
    {
        return RoughColorMatch(One, Two, 0);
    }

    public static bool RoughColorMatch(Color One, Color Two)
    {
        return RoughColorMatch(One, Two, 10);
    }

    public static bool RoughColorMatch(Color One, Color Two, int Precision)
    {
        return Utility.Utility.RoughMatch(One.R, Two.R, Precision) && Utility.Utility.RoughMatch(One.G, Two.G, Precision) && Utility.Utility.RoughMatch(One.B, Two.B, Precision);
    }

    public static bool ColorCheck(Color One, Color Two)
    {
        One = Color.FromArgb(Utility.Utility.LowerBound(One.R, 1), Utility.Utility.LowerBound(One.G, 1), Utility.Utility.LowerBound(One.B, 1));
        Two = Color.FromArgb(Utility.Utility.LowerBound(Two.R, 1), Utility.Utility.LowerBound(Two.G, 1), Utility.Utility.LowerBound(Two.B, 1));

        double ARg = (double)One.R / (double)One.G, ARb = (double)One.R / (double)One.B;
        double AGr = Math.Pow(ARg, -1), AGb = (double)One.G / (double)One.B;
        double ABr = Math.Pow(ARb, -1), ABg = Math.Pow(AGb, -1);

        double BRg = (double)Two.R / (double)Two.G, BRb = (double)Two.R / (double)Two.B;
        double BGr = Math.Pow(BRg, -1), BGb = (double)Two.G / (double)Two.B;
        double BBr = Math.Pow(BRb, -1), BBg = Math.Pow(BGb, -1);

        double Threshold = 0.2;

        int RgMatch = Convert.ToInt32(Utility.Utility.RoughMatch(ARg, BRg, Threshold));
        int RbMatch = Convert.ToInt32(Utility.Utility.RoughMatch(ARb, BRb, Threshold));
        int GrMatch = Convert.ToInt32(Utility.Utility.RoughMatch(AGr, BGr, Threshold));
        int GbMatch = Convert.ToInt32(Utility.Utility.RoughMatch(AGb, BGb, Threshold));
        int BrMatch = Convert.ToInt32(Utility.Utility.RoughMatch(ABr, BBr, Threshold));
        int BgMatch = Convert.ToInt32(Utility.Utility.RoughMatch(ABg, BBg, Threshold));

        List<int> AMax = new List<int> { One.R, One.G, One.B };
        List<int> BMax = new List<int> { Two.R, Two.G, Two.B };

        List<string> AColors = new List<string> { "r", "g", "b" };
        List<string> BColors = new List<string> { "r", "g", "b" };

        AColors = AColors.OrderBy(N => -AMax[AColors.IndexOf(N)]).ToList();
        BColors = BColors.OrderBy(N => -BMax[BColors.IndexOf(N)]).ToList();

        int RGBMatch = Convert.ToInt32(AColors.SequenceEqual(BColors));

        return (RgMatch + RbMatch + GrMatch + GbMatch + BrMatch + BgMatch + RGBMatch) > 2;
    }
}
