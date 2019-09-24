using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

public class RelativeRect
{
    public float X, Y, Width, Height;

    public RelativeRect(float X, float Y, float Width, float Height)
    {
        this.X = X;
        this.Y = Y;

        this.Width = Width;
        this.Height = Height;
    }

    public bool Contains(float CX, float CY)
    {
        return ((CX > X) && (CX < (X + Width))) && ((CY > Y) && (CY < (Y + Height)));
    }
}

public class Shape
{
    public float X, Y;
    public float EndX, EndY;

    public bool Dot, Line, Rectangle;

    public List<Shape> Neighbors = new List<Shape>();

    public Shape(float X, float Y)
    {
        this.X = X;
        this.Y = Y;

        Dot = true;
    }

    public Shape(bool Line, float X, float Y, float EndX, float EndY)
    {
        this.X = X;
        this.Y = Y;

        this.EndX = EndX;
        this.EndY = EndY;

        this.Line = Line;
        Rectangle = !Line;
    }

    public override bool Equals(object obj)
    {
        Shape Other = (Shape)obj;

        if (Dot && Other.Dot)
            return (X == Other.X) && (Y == Other.Y);
        else if ((Line && Other.Line) || (Rectangle && Other.Rectangle))
            return (X == Other.X) && (Y == Other.Y) && (EndX == Other.EndX) && (EndY == Other.EndY);

        return false;
    }

    public bool Equals(object obj, double Tolerance)
    {
        Shape Other = (Shape)obj;

        if (Dot && Other.Dot)
            return Utility.Utility.RoughMatch(X, Other.X, Tolerance) && Utility.Utility.RoughMatch(Y, Other.Y, Tolerance);
        else if ((Line && Other.Line) || (Rectangle && Other.Rectangle))
            return Utility.Utility.RoughMatch(X, Other.X, Tolerance) && Utility.Utility.RoughMatch(Y, Other.Y, Tolerance) && Utility.Utility.RoughMatch(EndX, Other.EndX, Tolerance) && Utility.Utility.RoughMatch(EndY, Other.EndY, Tolerance);

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public Vector FollowLine(Vector Position)
    {
        if (Neighbors.Count > 1)
        {
            foreach (Shape Check in Neighbors)
            {
                if ((Check.X != Position.StartX) || (Check.Y != Position.StartY))
                {
                    float SC = (Position.StartY - Check.Y) / (Position.StartX - Check.X);

                    bool XSign = Math.Sign(X - Check.X) == Math.Sign(Position.StartX - Position.EndX);
                    bool YSign = Math.Sign(Y - Check.Y) == Math.Sign(Position.StartY - Position.EndY);

                    if ((SC == Position.Slope) && XSign && YSign) 
                    {
                        Position = Check.FollowLine(new Vector(Position.StartX, Position.StartY, Check.X, Check.Y));

                        break;
                    }
                }
            }
        }

        return Position;
    }

    public float Percentage()
    {
        if (Line)
            return 4;
        else if (Rectangle)
            return 5;
        else
            return 2;
    }

    public float GetSlope()
    {
        if (Line)
        {
            return (EndY - Y) / (EndX - X);
        }

        return 0.0f;
    }

    public bool IsInside(RelativeRect Size)
    {
        if (Dot)
            return Size.Contains(X, Y);
        else
            return Size.Contains(X, Y) && Size.Contains(EndX, EndY);
    }

    public override string ToString()
    {
        string Display = "";

        if (Dot)
            Display += "Dot: ";
        else if (Line)
            Display += "Line: ";
        else if (Rectangle)
            Display += "Rectangle: ";

        Display += "(" + X + ", "+ Y + ")";

        if (Line || Rectangle)
            Display += ", (" + EndX + ", "+ EndY + ")";

        return Display;
    }
}

public class Vector
{
    public float StartX, StartY, EndX, EndY, Slope;

    public Vector(float StartX, float StartY, float EndX, float EndY)
    {
        this.StartX = StartX;
        this.StartY = StartY;

        this.EndX = EndX;
        this.EndY = EndY;

        this.Slope = (StartY - EndY) / (StartX - EndX);
    }
}

public class Template
{
    public int Width, Height;
    List<Shape> Components = new List<Shape>();

    public Template(Image Source)
    {
        Create(new QuickImage(Source));
    }

    public Template(Bitmap Source)
    {
        Create(new QuickImage(Source));
    }

    public Template(QuickImage Source)
    {
        Create(Source);
    }

    public Template(int Width, int Height, List<Shape> Components)
    {
        this.Width = Width;
        this.Height = Height;

        Components = new List<Shape>(Components);
    }

    public static Template Combine(Template Base, Template Mod)
    {
        return Combine(Base, Mod, 0);
    }

    public static Template Combine(Template Base, Template Mod, double Tolerance)
    {
        int i = 0;
        while (i < Base.Components.Count)
        {
            if (Mod.Components.Where(N => N.Equals(Base.Components[i], Tolerance)).Count() > 0)
                i++;
            else
                Base.Components.RemoveAt(i);
        }

        return Base;
    }

    public static bool Compare(Template Base, Template Check)
    {
        return Compare(Base, Check, 1);
    }

    public static bool Compare(Template Base, Template Check, double Percentage)
    {
        //float BaseCount = Base.Components.Count;
        //float CheckCount = Check.Components.Count;

        //Template Combined = Combine(Base, Check);

        //return ((float)Combined.Components.Count / CheckCount >= Percentage) && ((float)Combined.Components.Count / BaseCount >= Percentage);

        return MatchAmount(Base, Check) >= Percentage;
    }

    public static double MatchAmount(Template Base, Template Check)
    {
        double BaseCount = Base.Components.Count;
        double CheckCount = Check.Components.Count;

        Template Combined = Combine(Base, Check);

        return Utility.Utility.GetLowest((double)Combined.Components.Count / CheckCount, (double)Combined.Components.Count / BaseCount);
    }

    public void Create(QuickImage Source)
    {
        Console.WriteLine("Template processing starting.");

        Console.WriteLine("Getting image size.");
        Width = Source.Source.Width;
        Height = Source.Source.Height;
        Console.WriteLine("Done. Image is {0}x{1}.", Width, Height);

        Color Background = new Color();
        Console.WriteLine("Finding the mode color.");
        Background = ImageUtility.ModeColor(Source);
        Console.WriteLine("Done. Mode color is " + Background.ToString() + ".");

        Console.WriteLine("Creating list of all dots.");
        List<Shape> Dots = FindAllDots(Source, Background);

        Console.WriteLine("Finding all dots that are on their own.");
        Dots = FilterDots(Dots);
        Console.WriteLine("Done. {0} found.", Components.Count);

        List<Shape> LineCandidates = new List<Shape>();

        Console.WriteLine("Finding all possible lines");
        //Find everything that could be a line
        int i = 0;
        while (i < Dots.Count)
        {
            if (Dots[i].Neighbors.Count == 2)
            {
                LineCandidates.Add(Dots[i]);
                Dots.RemoveAt(i);
            }
            else
                i++;
        }

        Console.WriteLine("Done. {0} found.", LineCandidates.Count);

        Console.WriteLine("Filtering line candidates");
        //Filter the lines
        LineCandidates = FilterLineCandidates(LineCandidates);
        Console.WriteLine("Done. {0} left.", LineCandidates.Count);

        int OldCount = Components.Count;

        //Create the actual lines
        Console.WriteLine("Creating lines.");
        Components.AddRange(MakeLines(LineCandidates));

        int Created = Components.Count - OldCount;
        Console.WriteLine("Done. {0} made.", Created);

        Console.WriteLine("Adding {0} dots to the component list.", Dots.Count);
        //Add the rest of the leftover dots as plain dots.
        Components.AddRange(Dots);

        Console.WriteLine("Done.");

        Console.WriteLine("Template created. {0} components.", Components.Count);
    }

    public List<Shape> MakeLines(List<Shape> LineCandidates)
    {
        List<Shape> Result = new List<Shape>();
        
        foreach (Shape MakeLine in LineCandidates)
        {
            //Find the first half
            Vector Half1 = MakeLine.Neighbors[0].FollowLine(new Vector(MakeLine.X, MakeLine.Y, MakeLine.Neighbors[0].X, MakeLine.Neighbors[0].Y));
            
            //Find the second half
            Vector Half2 = MakeLine.Neighbors[1].FollowLine(new Vector(MakeLine.X, MakeLine.Y, MakeLine.Neighbors[1].X, MakeLine.Neighbors[1].Y));
            
            //Combine both halves together, starting at the second half's end and going to the first half's end (which are the two endpoints). Use slope of first half because its the opposite direction of the second half, which is the direction the line is actually going it
            Result.Add(new Shape(true, Half1.EndX, Half1.EndY, Half2.EndX, Half2.EndY));
        }

        Console.WriteLine("{0} lines generated", Result.Count);

        Console.WriteLine("Removing duplicates.");
        int n = 0;
        while (n < Result.Count)
        {
            int PreviousSize = Result.Count;

            Shape Save = Result[n];
            Result.RemoveAll(N => N.Equals(Result[n]));
            Result.Add(Save);
            
            if (PreviousSize == Result.Count)
                n++;
        }
        Console.WriteLine("{0} lines left.", Result.Count);

        Console.WriteLine("Combining lines.");
        //Combine lines
        n = 0;
        while (n < Result.Count)
        {
            float Slope = Result[n].GetSlope();
            List<Shape> Candidates = Result.Where(N => (N.X == Result[n].EndX) && (N.Y == Result[n].EndY) && (N.GetSlope() == Slope)).ToList();

            if (Candidates.Count() > 0)
            {
                Result[n].EndX = Candidates.First().EndX;
                Result[n].EndY = Candidates.First().EndY;

                Result.Remove(Candidates.First());
            }
            else
                n++;
        }
        Console.WriteLine("{0} lines left.", Result.Count);

        return Result;
    }

    public List<Shape> FilterDots(List<Shape> Dots)
    {
        List<Shape> Result = new List<Shape>();

        //Remove all of the single dots
        int i = 0;
        while (i < Dots.Count)
        {
            if (Dots[i].Neighbors.Count <= 1)
            {
                Components.Add(Dots[i]);
                Dots.RemoveAt(i);
            }
            else
            {
                Result.Add(Dots[i]);
                i++;
            }
        }

        return Result;
    }

    public List<Shape> FilterLineCandidates(List<Shape> LineCandidates)
    {
        //Remove all of the single dots, or groups that couldn't be lines
        int i = 0;
        while (i < LineCandidates.Count)
        {
            //Find slopes
            float SAC = (LineCandidates[i].Neighbors[1].Y - LineCandidates[i].Neighbors[0].Y) / (LineCandidates[i].Neighbors[1].X - LineCandidates[i].Neighbors[0].X);
            float SAB = (LineCandidates[i].Y - LineCandidates[i].Neighbors[0].Y) / (LineCandidates[i].X - LineCandidates[i].Neighbors[0].X);
            float SBC = (LineCandidates[i].Neighbors[1].Y - LineCandidates[i].Y) / (LineCandidates[i].Neighbors[1].X - LineCandidates[i].X);

            //Make sure they match
            if ((SAC == SAB) && (SAB == SBC))
                i++;
            else
                LineCandidates.RemoveAt(i);
        }

        return LineCandidates;
    }

    public List<Shape> FindAllDots(QuickImage Source, Color Background)
    {
        List<Shape> Dots = new List<Shape>();

        float W1 = 1.0f / Source.Source.Width;
        float H1 = 1.0f / Source.Source.Height;

        for (float x = 0.0f; x < Source.Source.Width; x++)
        {
            for (float y = 0.0f; y < Source.Source.Height; y++)
            {
                if (!ImageUtility.RoughColorMatch(Background, Source.GetPixel((int)x, (int)y), 2))
                {
                    Dots.Add(new Shape(x / Source.Source.Width, y / Source.Source.Height));
                }
            }
        }

        Console.WriteLine("Done");

        Console.WriteLine("Finding neighbors.");

        //Calculate all neighbors
        for (int i = 0; i < Dots.Count; i++)
        {
            if (i % Utility.Utility.LowerBound((Dots.Count / 40), 1) == 0)
                Console.WriteLine("Processing: {0}/{1}", i, Dots.Count);

            Shape Find = Dots[i];
            bool LX = false, GX = false;
            bool LY = false, GY = false;

            if (Find.X > 0)
                LX = true;
            if (Find.X < (Source.Source.Width - 1))
                GX = true;
            if (Find.Y > 0)
                LY = true;
            if (Find.Y < (Source.Source.Height - 1))
                GY = true;

            List<Shape> GXC = Dots.Where(N => N.Equals(new Shape(Find.X + W1, Find.Y))).ToList();

            if (LX && Dots.Where(N => N.Equals(new Shape(Find.X - W1, Find.Y))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X - W1, Find.Y))));
            if (GX && Dots.Where(N => N.Equals(new Shape(Find.X + W1, Find.Y))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X + W1, Find.Y))));
            if (LY && Dots.Where(N => N.Equals(new Shape(Find.X, Find.Y - H1))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X, Find.Y - H1))));
            if (GY && Dots.Where(N => N.Equals(new Shape(Find.X, Find.Y + H1))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X, Find.Y + H1))));
            if (LX && LY && Dots.Where(N => N.Equals(new Shape(Find.X - W1, Find.Y - H1))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X - W1, (Find.Y - H1)))));
            if (LX && GY && Dots.Where(N => N.Equals(new Shape(Find.X - W1, Find.Y + H1))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X - W1, (Find.Y + H1)))));
            if (GX && LY && Dots.Where(N => N.Equals(new Shape(Find.X + W1, Find.Y - H1))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X + W1, (Find.Y - H1)))));
            if (GX && GY && Dots.Where(N => N.Equals(new Shape(Find.X + W1, Find.Y + H1))).Count() > 0)
                Find.Neighbors.Add(Dots.First(D => D.Equals(new Shape(Find.X + W1, (Find.Y + H1)))));
        }

        Console.WriteLine("Done");

        return Dots;
    }

    public Template Crop(RelativeRect Size)
    {
        return new Template((int)(Size.Width * Width), (int)(Size.Height * Height), Components.Where(N => N.IsInside(Size)).ToList());
    }

    public void Dump()
    {
        foreach (Shape Display in Components)
        {
            Console.WriteLine(Display.ToString());
        }
    }

    public void Offload(string FileName)
    {
        Bitmap Offloaded = new Bitmap(Width, Height);

        foreach (Shape Draw in Components)
        {
            if (Draw.Dot)
                Offloaded.SetPixel((int)(Draw.X * Width), (int)(Draw.Y * Height), Color.Black);
            else if (Draw.Line)
            {
                using (Graphics g = Graphics.FromImage(Offloaded))
                {
                    g.DrawLine(new Pen(Color.Black), Draw.X * Width, Draw.Y * Width, Draw.EndX * Width, Draw.EndY * Height);
                }
            }
        }

        Offloaded.Save(FileName);
    }
}

public class OCR
{
    public static List<Point> Contains(Template MappedOne, Template Search, double Percentage, int Max)
    {
        List<Point> Result = new List<Point>();

        for (int x = 0; x < (MappedOne.Width - Search.Width + 1); x++)
        {
            for (int y = 0; y < (MappedOne.Height - Search.Height + 1); y++)
            {
                Template SearchField = MappedOne.Crop(new RelativeRect((float)x / (float)MappedOne.Width, (float)y / (float)MappedOne.Height, Search.Width, Search.Height));

                //If they meet the specified percentage (or more of course) it works
                if (Template.Compare(SearchField, Search, Percentage))
                {
                    Result.Add(new Point(x, y));

                    if ((Result.Count >= Max) && (Max > 0))
                        return Result;

                    y += Search.Height; //Move so we don't get a bunch of matches here
                }
            }
        }

        return Result;
    }

    public static string ReadImage(string FileOne, string CharList, double Percentage)
    {
        List<ImageCompare.Character> ResultText = new List<ImageCompare.Character>();

        Template ReadingImage = new Template(new Bitmap(Image.FromFile(FileOne))); //The image to read.

        string[] Files = System.IO.Directory.GetFiles(CharList);
        foreach (string File in Files)
        {
            Template ReadImage = new Template(new Bitmap(Image.FromFile(File))); //The character to look for in the image.

            List<Point> Points = Contains(ReadingImage, ReadImage, Percentage, 0);

            foreach (Point Add in Points)
            {
                ResultText.Add(new ImageCompare.Character(Utility.Utility.StripExtension(Utility.Utility.StripDirectoryName(File, CharList + "\\")), Add));
            }
        }

        //ResultText.Sort((P1, P2) => (P1.Position.X.CompareTo(P2.Position.X)));
        ResultText.Sort(new ImageCompare.ComparePoints());

        string ReadString = "";

        foreach (ImageCompare.Character Append in ResultText)
        {
            ReadString += Append.Text;
        }

        return ReadString;
    }
}