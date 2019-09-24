using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IASM
{
    public enum VarTypes
    {
        INTEGER,
        DECIMAL,
        STRING
    }

    public struct CompileVariable
    {
        public int Type;
        public string Name, Value;

        public CompileVariable(int Type, string Name, string Value)
        {
            this.Type = Type;

            this.Name = Name;
            this.Value = Value;
        }
    }

    public class Compiler
    {
        public static List<long> Compile(string Input) //Run a file
        {
            return Compile(System.IO.File.ReadAllLines(Input).ToList());
        }

        public static bool MemoryContains(List<CompileVariable> Memory, string VarName)
        {       
            foreach (CompileVariable Check in Memory)
            {
                if (Check.Name.Equals(VarName))
                    return true;
            }

            return false;
        }

        public static string CheckForIndex(List<CompileVariable> Memory, string Name, int Line, ref bool Error, bool IgnoreError=false)
        {
            int Index = Memory.FindIndex(N => N.Name.Equals(Name));

            if (Index == -1)
            {
                Error = true;

                if (!IgnoreError)
                    Console.WriteLine("Variable \"{0}\" not declared.", Name, Line);
            }

            return Index.ToString();
        }

        public static List<long> Compile(List<string> Input)
        {
            List<CompileVariable> Memory = new List<CompileVariable>(); //The list of differently named memory locations. These will be converted to indices
            List<string> Points = new List<string>(); //A list of goto locations.
            List<int> PointLocation = new List<int>(); //The position of a given location in the Result.

            List<string> Result = new List<string>();

            bool Error = false;

            for (int x = 0; x < Input.Count(); x++)
            {
                string Line = Input[x];
                if (String.IsNullOrWhiteSpace(Line))
                    continue;

                string[] Params = Line.Split(' ');

                if ((Params.Length > 0) && (Params[0].Length > 0))
                {
                    if (Params[0][0] == ':')
                    {
                        Points.Add(Params[0].Substring(1));
                        PointLocation.Add(Result.Count);
                    }
                    else if (Params[0].ToLower().Equals("if"))
                    {
                        Result.Add(((int)Commands.IF).ToString());

                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                        Result.Add(Params[2]);
                        Result.Add(Params[3]);
                    }
                    else if (Params[0].ToLower().Equals("length"))
                    {
                        Result.Add(((int)Commands.LENGTH).ToString());

                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                    }
                    else if (Params[0].ToLower().Equals("split"))
                    {
                        Result.Add(((int)Commands.SPLIT).ToString());
                        
                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                        Result.Add(CheckForIndex(Memory, Params[2], x, ref Error));
                        Result.Add(CheckForIndex(Memory, Params[3], x, ref Error));
                        Result.Add(CheckForIndex(Memory, Params[4], x, ref Error));
                    }
                    else if (Params[0].ToLower().Equals("mov"))
                    {
                        int Type = 0;

                        Result.Add(((int)Commands.MOV).ToString());

                        if (MemoryContains(Memory, Params[1]))
                        {
                            Result.Add("0"); //A variable

                            Type = Memory.Find(N => N.Name.Equals(Params[1])).Type;

                            Result.Add(Type.ToString());
                            Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                        }
                        else
                        {
                            Result.Add("1"); //A constant

                            //Determine variable type
                            if ((Params[1][0] >= '0') && (Params[1][0] <= '9')) //A number of some type
                            {
                                if (Params[1].Contains('.')) //A decimal
                                {
                                    Type = (int)VarTypes.DECIMAL;
                                    Result.Add(((int)VarTypes.DECIMAL).ToString());

                                    string[] Halves = Params[1].Split('.');

                                    Result.Add(Halves[0]);
                                    Result.Add(Halves[1]);
                                }
                                else
                                {
                                    Type = (int)VarTypes.INTEGER;
                                    Result.Add(((int)VarTypes.INTEGER).ToString());

                                    Result.Add(Params[1]);
                                }
                            }
                            else //A string
                            {
                                Type = (int)VarTypes.STRING;
                                Result.Add(((int)VarTypes.STRING).ToString());
                                Result.Add(Params[1].Length.ToString());

                                foreach (char Add in Params[1])
                                {
                                    Result.Add(Convert.ToInt32(Add).ToString());
                                }
                            }
                        }

                        if (!MemoryContains(Memory, Params[2]))
                            Memory.Add(new CompileVariable(Type, Params[2], ""));

                        Result.Add(CheckForIndex(Memory, Params[2], x, ref Error));
                    }
                    else if (Params[0].ToLower().Equals("add") || Params[0].ToLower().Equals("sub") ||
                             Params[0].ToLower().Equals("mult") || Params[0].ToLower().Equals("div") ||
                             Params[0].ToLower().Equals("mod") || Params[0].ToLower().Equals("eq") ||
                             Params[0].ToLower().Equals("gt") || Params[0].ToLower().Equals("lt") ||
                             Params[0].ToLower().Equals("ge") || Params[0].ToLower().Equals("le"))
                    {
                        if (Params[0].ToLower().Equals("add"))
                            Result.Add(((int)Commands.ADD).ToString());
                        else if (Params[0].ToLower().Equals("sub"))
                            Result.Add(((int)Commands.SUB).ToString());
                        else if (Params[0].ToLower().Equals("mult"))
                            Result.Add(((int)Commands.MULT).ToString());
                        else if (Params[0].ToLower().Equals("div"))
                            Result.Add(((int)Commands.DIV).ToString());
                        else if (Params[0].ToLower().Equals("mod"))
                            Result.Add(((int)Commands.MOD).ToString());
                        else if (Params[0].ToLower().Equals("eq"))
                            Result.Add(((int)Commands.EQ).ToString());
                        else if (Params[0].ToLower().Equals("gt"))
                            Result.Add(((int)Commands.GT).ToString());
                        else if (Params[0].ToLower().Equals("lt"))
                            Result.Add(((int)Commands.LT).ToString());
                        else if (Params[0].ToLower().Equals("ge"))
                            Result.Add(((int)Commands.GE).ToString());
                        else if (Params[0].ToLower().Equals("le"))
                            Result.Add(((int)Commands.LE).ToString());

                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                        Result.Add(CheckForIndex(Memory, Params[2], x, ref Error));
                        Result.Add(CheckForIndex(Memory, Params[3], x, ref Error));
                    }
                    else if (Params[0].ToLower().Equals("print"))
                    {
                        Result.Add(((int)Commands.PRINT).ToString());

                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                    }
                    else if (Params[0].ToLower().Equals("gotoeq"))
                    {
                        Result.Add(((int)Commands.GOTOEQ).ToString());

                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                        Result.Add(CheckForIndex(Memory, Params[2], x, ref Error));
                        Result.Add(Params[3]);
                    }
                    else if (Params[0].ToLower().Equals("gotogr"))
                    {
                        Result.Add(((int)Commands.GOTOGR).ToString());

                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));
                        Result.Add(CheckForIndex(Memory, Params[2], x, ref Error));
                        Result.Add(Params[3]);
                    }
                    else if (Params[0].ToLower().Equals("goto"))
                    {
                        Result.Add(((int)Commands.GOTO).ToString());

                        Result.Add(Params[1]);
                    }
                    else if (Params[0].ToLower().Equals("call"))
                    {
                        Result.Add(((int)Commands.CALL).ToString());

                        Result.Add(CheckForIndex(Memory, Params[1], x, ref Error));

                        int LengthSpot = Result.Count();
                        int Len = 0;

                        for (int i = 2; i < Params.Count(); i++)
                        {
                            string Index = CheckForIndex(Memory, Params[i], x, ref Error, IgnoreError: true);
                            
                            if (Error)
                            {
                                Error = false; //These errors aren't actual errors, just testing and for diagnosis for the code writer.

                                foreach (char Add in Params[i])
                                {
                                    Result.Add(Convert.ToInt64(Add).ToString());
                                    Len++;
                                }

                                Result.Add(Convert.ToInt64(' ').ToString());
                                Len++;
                            }
                            else
                            {
                                Result.Add("-1");
                                Len++;
                                Result.Add(Index);
                                Len++;
                            }
                        }

                        Result.Insert(LengthSpot, Len.ToString());
                    }
                    else if (Params[0].ToLower().Equals("delay"))
                    {
                        Result.Add(((int)Commands.DELAY).ToString());

                        Result.Add(Params[1]);
                    }
                }
            }

            for (int i = 0; i < Result.Count; i++)
            {
                int Test = 0;

                if (Result[i].Length < 10)
                {
                    if (!int.TryParse(Result[i], out Test)) //It must be a goto location
                        Result[i] = PointLocation[Points.FindIndex(N => N.Equals(Result[i]))].ToString();
                }
                else if (Result[i].Length >= 10)
                {
                    Result[i] = PointLocation[Points.FindIndex(N => N.Equals(Result[i]))].ToString();
                }
            }

            Console.WriteLine(String.Join(" ", Result));

            if (Error)
            {
                Console.ReadLine();

                System.Threading.Thread.CurrentThread.Abort();
            }

            return Result.Select(N => Convert.ToInt64(N)).ToList();
        }
    }
}
