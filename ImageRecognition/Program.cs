using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using Scripting;

namespace ImageRecognition
{
    public static class Library
    {
        public static bool HasCommand(string Input)
        {
            var Commands = new List<string> { "compare", "togglewriteresult", "normalcompare", "probshape", "probmaster", "probcombine", "probimage", "probcompare", "create", "probclassify",
                "classify", "probcontains", "clickon", "clickpos", "cursorpos", "clickarea", "contains", "percentage", "directory", "read", "edit", "dump", "quit", "template" };

            return false;
        }

        public static string ProcessCommand(string Input)
        {
            try
            {
                string Result = "";

                Store Params = new Store(Store.SeperateByWord(Input));
                Stopwatch Ticks = new Stopwatch();
                Ticks.Start();

                if (Params[0] == "compare")
                {
                    if (Params[1] == "percentage")
                    {
                        Result = ImageCompare.Compare.ComparePercentage(Params[2], Params[3]).ToString();
                    }
                }
                else if (Params[0] == "togglewriteresult")
                {
                    Program.WriteResult = !Program.WriteResult;
                }
                else if (Params[0] == "normalcompare")
                {
                    Result = Normal.ComparePercentage(Params[1], Params[2]).ToString();
                }
                else if (Params[0] == "probshape")
                {
                    Result = Probabilistic.CompareShapePercentage(Params[1], Params[2]).ToString();
                }
                else if (Params[0] == "probmaster")
                {
                    string[] Files = System.IO.Directory.GetFiles(Params[1], "*.png", System.IO.SearchOption.AllDirectories);

                    Console.WriteLine("Creating master from {0} files.", Files.Length);

                    List<ProbabilisticImage> Sources = new List<ProbabilisticImage>();

                    foreach (string File in Files)
                    {
                        Sources.Add(new ProbabilisticImage(new Bitmap(File)));
                    }

                    ProbabilisticImage Master = ProbabilisticImage.CreateMaster(Sources);
                    Master.Save(Params[2]);

                    Console.WriteLine("Finished.");
                }
                else if (Params[0] == "probcombine")
                {
                    string FileA = Params[1];
                    string FileB = Params[2];
                    string OutputFile = Params[3];

                    ProbabilisticImage.Combine(FileA, FileB).Save(OutputFile);
                }
                else if (Params[0] == "probimage")
                {
                    ProbabilisticImage CreatedImage = new ProbabilisticImage(new Bitmap(Params[1]));

                    Result = CreatedImage.ToString();
                }
                else if (Params[0] == "probcompare")
                {
                    Result = Probabilistic.ComparePercentage(Params[1], Params[2]).ToString();
                }
                else if (Params[0] == "create")
                {
                    if (Params[1] == "classifier")
                    {
                        Program.Classifier = new Classifier(Params[2]);
                    }
                    else if (Params[1] == "single_classifier")
                    {
                        Program.Classifier = new Classifier(Params[2], true);
                    }
                }
                else if (Params[0] == "probclassify")
                {
                    if (Params[1] == "directory")
                    {
                        string[] Files = System.IO.Directory.GetFiles(Params[2], "*.png", System.IO.SearchOption.AllDirectories);

                        Console.WriteLine("Checking {0} files.", Files.Length);

                        foreach (string File in Files)
                        {
                            var ClassifyResult = Program.Classifier.Classify(File, true);

                            string Classes = "";

                            if (ClassifyResult.Count() > 0)
                            {
                                var Temp = ClassifyResult
                                    .Select(N => N.Substring(N.IndexOf(":") + 2).Trim())
                                    .ToList();

                                var Weights = Temp
                                    .Select(N => Convert.ToDouble(N))
                                    .ToList();
                                var Names = ClassifyResult.Select(N => N.Substring(0, N.IndexOf(":"))).ToList();

                                Classes = File + ": " + String.Join(", ", ClassifyResult);

                                Console.WriteLine(Classes);
                                Console.WriteLine("Image is probably: " + Names[Weights.IndexOf(Weights.Max())] + ".\n");

                                Result += Classes + "\n" + "Image is probably: " + Names[Weights.IndexOf(Weights.Max())] + ".\n\n";
                            }
                            else
                            {
                                Console.WriteLine("No matches!");

                                Classes = File + ": " + "None";

                                Result += Classes + "\n";
                            }
                        }

                        Console.WriteLine("Finished.");
                    }
                    else
                        Result = String.Join(", ", Program.Classifier.Classify(Params[1], true));
                }
                else if (Params[0] == "classify")
                {
                    if (Params[1] == "directory")
                    {
                        string[] Files = System.IO.Directory.GetFiles(Params[2], "*.png", System.IO.SearchOption.AllDirectories);

                        Console.WriteLine("Checking {0} files.", Files.Length);

                        foreach (string File in Files)
                        {
                            var ClassifyResult = Program.Classifier.Classify(File, false);

                            string Classes = "";

                            if (ClassifyResult.Count() > 0)
                            {
                                var Temp = ClassifyResult
                                    .Select(N => N.Substring(N.IndexOf(":") + 2).Trim())
                                    .ToList();

                                var Weights = Temp
                                    .Select(N => Convert.ToDouble(N))
                                    .ToList();
                                var Names = ClassifyResult.Select(N => N.Substring(0, N.IndexOf(":"))).ToList();

                                Classes = File + ": " + String.Join(", ", ClassifyResult);

                                Console.WriteLine(Classes);
                                Console.WriteLine("Image is probably: " + Names[Weights.IndexOf(Weights.Max())] + ".\n");

                                Result += Classes + "\n" + "Image is probably: " + Names[Weights.IndexOf(Weights.Max())] + ".\n\n";
                            }
                            else
                            {
                                Console.WriteLine("No matches!");

                                Classes = File + ": " + "None";

                                Result += Classes + "\n";
                            }
                        }

                        Console.WriteLine("Finished.");
                    }
                    else
                        Result = String.Join(", ", Program.Classifier.Classify(Params[1], false));
                }
                else if (Params[0] == "probcontains")
                {
                    List<Point> Points = Probabilistic.Contains(Params[1], Params[2], Convert.ToDouble(Params[3]));
                    string Stuff = "";

                    foreach (Point Place in Points)
                    {
                        Stuff += Place.ToString() + "\n";
                    }

                    Console.WriteLine(Stuff);

                    Result = Points.Count().ToString();
                }
                else if (Params[0] == "clickon")
                {
                    if (Params.Count() > 2) // Specifying the click type
                        Result = Convert.ToInt32(ImageCompare.Output.ClickOn(Params[1], Convert.ToBoolean(Params[2]))).ToString();
                    else
                        Result = Convert.ToInt32(ImageCompare.Output.ClickOn(Params[1])).ToString();
                }
                else if (Params[0] == "clickpos")
                {
                    if (Params.Count() > 3)
                    {
                        if (Params[3].Equals("true"))
                            ImageCompare.MouseOutput.LeftClick(Convert.ToInt32(Params[1]), Convert.ToInt32(Params[2]));
                        else
                            ImageCompare.MouseOutput.RightClick(Convert.ToInt32(Params[1]), Convert.ToInt32(Params[2]));
                    }
                    else
                    {
                        ImageCompare.MouseOutput.LeftClick(Convert.ToInt32(Params[1]), Convert.ToInt32(Params[2]));
                    }
                }
                else if (Params[0] == "cursorpos")
                {
                    Result = Cursor.Position.X + ":" + Cursor.Position.Y;
                }
                else if (Params[0] == "clickarea")
                {
                    Console.WriteLine("Clicking {0}", Params[1]);

                    if (Params.Count() > 6)
                        Result = Convert.ToInt32(ImageCompare.Output.ClickArea(Params[1], Convert.ToInt32(Params[2]), Convert.ToInt32(Params[3]), Convert.ToInt32(Params[4]), Convert.ToInt32(Params[5]), Convert.ToBoolean(Params[6]))).ToString();
                    else
                        Result = Convert.ToInt32(ImageCompare.Output.ClickArea(Params[1], Convert.ToInt32(Params[2]), Convert.ToInt32(Params[3]), Convert.ToInt32(Params[4]), Convert.ToInt32(Params[5]))).ToString();
                }
                else if (Params[0] == "contains")
                {
                    if (Params[1] == "percentage")
                    {
                        Result = ImageCompare.Contain.Contains(Params[2], Params[3], Convert.ToDouble(Params[4])).Count().ToString();
                    }
                    else if (Params[1] == "directory")
                    {
                        List<Point> Points = ImageCompare.Contain.DirectoryContains(Params[3], System.IO.Directory.GetCurrentDirectory() + "\\" + Params[2]);

                        Result = Points.Count().ToString();
                    }
                    else if (Params[1] == "read")
                    {
                        Result = "";// ImageCompare.Contain.ReadImage(Params[3], Params[2], Convert.ToDouble(Params[4]));
                    }
                    else if (Params[1] == "max")
                    {
                        Result = ImageCompare.Contain.Contains(Params[2], Params[3], Convert.ToInt32(Params[4])).Count().ToString();
                    }
                    else
                    {
                        Result = ImageCompare.Contain.Contains(Params[1], Params[2]).Count().ToString();
                    }
                }
                else if (Params[0] == "edit")
                {
                    if (Params[1] == "closecrop")
                    {
                        if (Params[2] == "directory")
                        {
                            string Directory = System.IO.Directory.GetCurrentDirectory() + "\\" + Params[3] + "\\";
                            string[] Files = System.IO.Directory.GetFiles(Directory);

                            System.IO.Directory.CreateDirectory(Directory + "edited\\");

                            foreach (string File in Files)
                            {
                                ImageUtility.CloseCrop(Image.FromFile(File), Directory + "edited\\" + Utility.Utility.StripDirectoryName(File, Directory));
                            }
                        }
                        else
                        {
                            ImageUtility.CloseCrop(Image.FromFile(Params[2]), Params[3]);
                        }
                    }
                    else if (Params[1] == "scale")
                    {
                        if (Params.Information.Count > 5)
                            ImageUtility.Scale(new Bitmap(Image.FromFile(Params[2])), new Rectangle(0, 0, Convert.ToInt32(Params[3]), Convert.ToInt32(Params[4]))).Save(Params[5]);
                        else if (Params.Information.Count == 5)
                            ImageUtility.Scale(new Bitmap(Image.FromFile(Params[2])), Convert.ToDouble(Params[3])).Save(Params[4]);
                    }
                }
                else if (Params[0] == "dump")
                {
                    ImageCompare.Dump.DumpImage(Params[1]);
                }
                else if (Params[0] == "quit")
                {
                    return "quit";
                }
                else if (Params[0] == "template")
                {
                    if (Params[1] == "combine")
                    {
                        Template Base = new Template(Image.FromFile(Params[2]));
                        Template Mod = new Template(Image.FromFile(Params[3]));

                        Template Combined = Template.Combine(Base, Mod);

                        Program.Created = Combined;
                    }
                    else if (Params[1] == "compare")
                    {
                        Template Base = new Template(Image.FromFile(Params[2]));
                        Template Mod = new Template(Image.FromFile(Params[3]));

                        bool DoMatch = Template.Compare(Base, Mod);

                        Console.WriteLine("Templates {0} match.", (DoMatch) ? "do" : "do not");

                        Result = Convert.ToInt32(DoMatch).ToString();
                    }
                    else if (Params[1] == "check")
                    {
                        Template Base = new Template(Image.FromFile(Params[2]));
                        Template Mod = new Template(Image.FromFile(Params[3]));

                        double Amount = Convert.ToDouble(Params[4]);

                        bool DoMatch = Template.Compare(Base, Mod, Amount);

                        Console.WriteLine("Templates {0} match.", (DoMatch) ? "do" : "do not");

                        Result = Convert.ToInt32(DoMatch).ToString();
                    }
                    else if (Params[1] == "matchamount")
                    {
                        Template Base = new Template(Image.FromFile(Params[2]));
                        Template Mod = new Template(Image.FromFile(Params[3]));

                        double Amount = Template.MatchAmount(Base, Mod);

                        Console.WriteLine("Templates match {0}", Amount);

                        Result = Amount.ToString();
                    }
                    else if (Params[1] == "contains")
                    {
                        Template Container = new Template(Image.FromFile(Params[2]));
                        Template Search = new Template(Image.FromFile(Params[3]));

                        List<Point> Points = OCR.Contains(Container, Search, Convert.ToDouble(Params[4]), 0);

                        Result = String.Join(", ", Points.Select(N => N.ToString()));
                    }
                    else if (Params[1] == "offload")
                    {
                        Program.Created.Offload(Params[2]);
                    }
                    else if (Params[1] == "dump")
                    {
                        Program.Created.Dump();
                    }
                    else if (Params[1] == "crop")
                    {
                        Program.Created = Program.Created.Crop(new RelativeRect((float)Convert.ToDouble(Params[2]), (float)Convert.ToDouble(Params[3]),
                            (float)Convert.ToDouble(Params[4]), (float)Convert.ToDouble(Params[5])));
                    }
                    else if (Params.Information.Count > 2)
                    {
                        List<string> Files = new List<string>(Params.Information);

                        Files.RemoveAt(0);

                        foreach (string File in Files)
                        {
                            Program.Created = new Template(Image.FromFile(File));

                            Program.Created.Offload(Utility.Utility.StripExtension(File) + "_visual.png");
                        }
                    }
                    else
                    {
                        Program.Created = new Template(Image.FromFile(Params[1]));
                    }
                }
                else if (Params[0] == "help")
                {
                    Console.WriteLine("Standard Algorithms:");
                    Console.WriteLine("compare percentage file1 file2 - Returns the amount the two images match in percentages.");
                    Console.WriteLine("clickon file - Searches for the specified image on screen, and clicks on it.");
                    Console.WriteLine("dump file - Dumps the RGB of each pixels of the image to the screen.");
                    Console.WriteLine("contains");
                    Console.WriteLine("\tfile1 file2 - Returns a list of all the points at which a copy of file2 exists in file1.");
                    Console.WriteLine("\tall file1 file2 percentage max - Returns a list up to size max where file2 matched percentage amount in file1");
                    Console.WriteLine("\tdirectory directory file - Returns a list of points where any file in the directory exists in file1");
                    Console.WriteLine("\tmax file1 file2 amount - Returns up to amount results where file2 exists in file1.");
                    Console.WriteLine("\tpercentage file1 file2 amount - Returns a list of points that file2 matches by at least the amount (should be [0, 1]) in file1");
                    Console.WriteLine("\tread charlist file percentage - Attempts to read the file with the list of characters charlist (should be a directory)");
                    Console.WriteLine("\t\tthat match by at least percentage amount.");
                    Console.WriteLine("edit");
                    Console.WriteLine("\tclosecrop");
                    Console.WriteLine("\t\tfile savename - Crops the file as much possible without losing any background, then saves it as savename.");
                    Console.WriteLine("\t\tdirectory name - Crops all files in directory as much as possible, then puts them into a folder called edited inside the target directory.");
                    Console.WriteLine("\tscale file width height savename - Scales the file to be width and height then saves it as savename.");
                    Console.WriteLine("\tscale file percentage savename - Scales the file to be the specified percentage of its previous size.");
                    Console.WriteLine();
                    Console.WriteLine("Template-Based Algorithms:");
                    Console.WriteLine("template");
                    Console.WriteLine("\tcombine file1 file2 - Generates templates from file1 and file2, then combines them. The result is stored for later access.");
                    Console.WriteLine("\tcompare file1 file2 - Tells whether the two templates match completely, returning 1 if they do and 0 if not.");
                    Console.WriteLine("\tcheck file1 file2 percentage - Tells whether the two files match by at least percentage, returning 1 if they do and 0 if not.");
                    Console.WriteLine("\tcrop x y width height - Crops the current template to the specified size.");
                    Console.WriteLine("\tcontains file1 file2 - Returns all the positions in which file2 occurs in file1.");
                    Console.WriteLine("\tdump - Dumps the most recently created template to the screen.");
                    Console.WriteLine("\tmatchamount file1 file2 - Returns the amount that file1 and file2 match.");
                    Console.WriteLine("\toffload filename - Offloads the most recent template to filename.");
                    Console.WriteLine("\tfile - Generates a template from the file.");
                    Console.WriteLine();
                    Console.WriteLine("Probablistic:");
                    Console.WriteLine("probimage filename - Creates a probablistic image from the file specified.");
                    Console.WriteLine("probcompare file1 file2 - Compares the two images and returns the amount they match (percentage).");
                    Console.WriteLine("probcontains file1 file2 - Checks for all instances of file2 inside file1. Returns a list of points.");
                    Console.WriteLine();
                    Console.WriteLine("Other:");
                    Console.WriteLine("help - Shows this help menu.");
                    Console.WriteLine("quit - Exits the program.");
                    Console.WriteLine("togglewriteresult - Toggles displaying the result of commands.");
                }
                else
                {
                    Console.WriteLine("Unknown Command: {0}", Params[0]);
                }

                Ticks.Stop();

                TimeSpan Length = Ticks.Elapsed;

                return Result;
            }
            catch (Exception E)
            {
                Console.WriteLine(E.ToString());
            }

            return "";
        }
    }

    static class Program
    {
        public static Interpreter Commands = new Interpreter();

        public static Template Created;

        public static bool WriteResult = true;

        public static Classifier Classifier;

        static void Main(string[] args)
        {
            bool Running = true;
            string Input = "";

            Utility.Utility.Generator = new Random((int)DateTime.Now.Ticks);

            Commands.HandleMethods = Script.HandleMethods;

            while (Running)
            {
                Input = Console.ReadLine();

                if (Library.ProcessCommand(Input) == "quit")
                    break;
            }
        }
    }
}