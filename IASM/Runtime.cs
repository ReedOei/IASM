using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IASM
{
    enum Commands
    {
        MOV,
        ADD,
        SUB,
        MULT,
        DIV,
        MOD,
        PRINT,
        GOTOEQ,
        GOTOGR,
        GOTO,
        CALL,
        DELAY,
        SPLIT,
        IF,
        LENGTH,
        EQ,
        GT,
        LT,
        GE,
        LE
    }

    public class Variable
    {
        public int Type;

        public string Value;

        public Variable(int Type, string Value)
        {
            this.Type = Type;

            this.Value = Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class Runtime
    {
        public delegate int Command(int i, ref List<Variable> Memory, List<long> Commands);

        public static Command External;

        public static List<Command> Commands;

        public List<Variable> Memory = new List<Variable>();

        public Runtime()
        {

        }

        public static void Init()
        {
            if (Commands == null)
            {
                Commands = new List<Command>();

                Commands.Add(MOV);
                Commands.Add(ADD);
                Commands.Add(SUB);
                Commands.Add(MULT);
                Commands.Add(DIV);
                Commands.Add(MOD);
                Commands.Add(PRINT);
                Commands.Add(GOTOEQ);
                Commands.Add(GOTOGR);
                Commands.Add(GOTO);
                Commands.Add(CALL);
                Commands.Add(DELAY);
                Commands.Add(SPLIT);
                Commands.Add(IF);
                Commands.Add(LENGTH);
                Commands.Add(EQ);
                Commands.Add(GT);
                Commands.Add(LT);
                Commands.Add(GE);
                Commands.Add(LE);
            }
        }

        public string Dump()
        {
            string Result = "";

            for (int i = 0; i < Memory.Count; i++)
            {
                Result += "\n" + i.ToString() + " = " + Memory[i].ToString();
            }

            return Result;
        }

        public string Run(List<long> Code)
        {
            if (Code.Count <= 0) return "";

            for (int i = 0; i < Code.Count; i++)
            {
                i = Commands[(int)Code[i]](i, ref Memory, Code) - 1;
            }

            return String.Join(" ", Code);
        }

        public static int PRINT(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];

            Console.WriteLine(Memory[(int)M1]);

            return i + 2;
        }

        public static int MOV(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long IsVar = Commands[i + 1];
            int Type = (int)Commands[i + 2];

            long Data = Commands[i + 3];

            string M1 = "";

            int Length = 0;
            if (IsVar == 1) //A constant
            {
                if (Type == (int)VarTypes.STRING)
                {
                    Length = (int)Commands[i + 3];

                    for (int x = i + 4; x < (i + 4 + Length); x++)
                    {
                        M1 += Convert.ToChar(Commands[x]);
                    }
                }
                else if (Type == (int)VarTypes.DECIMAL)
                {
                    Length = 1;

                    M1 = Convert.ToDouble(Commands[i + 3]) + Convert.ToDouble("0." + Commands[i + 4].ToString()).ToString();
                }
                else
                    M1 = Commands[i + 3].ToString();
            }
            else
                M1 = Memory[(int)Data].Value;

            long M2 = Commands[i + 4 + Length];

            if (M2 >= Memory.Count)
                Memory.Add(new Variable(Type, ""));

            Memory[(int)M2].Value = M1;

            return i + 5 + Length;
        }

        public static int ADD(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) + Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else if (Memory[(int)M1].Type == (int)VarTypes.DECIMAL)
                Memory[(int)M3].Value = (Convert.ToDouble(Memory[(int)M1].Value) + Convert.ToDouble(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Memory[(int)M1].Value + Memory[(int)M2].Value;

            return i + 4;
        }

        public static int SUB(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) - Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = (Convert.ToDouble(Memory[(int)M1].Value) - Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int MULT(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) * Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = (Convert.ToDouble(Memory[(int)M1].Value) * Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int DIV(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Convert.ToDouble(Memory[(int)M2].Value) == 0)
            {
                ShowError("Attempted to divide by 0! Memory: {0}:{1}, {2}:{3}. Type = {4}", M1.ToString(), Memory[(int)M1].Value, M2.ToString(), Memory[(int)M2].Value, Memory[(int)M1].Type.ToString());
            }

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) / Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = (Convert.ToDouble(Memory[(int)M1].Value) / Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int MOD(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) % Convert.ToInt32(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int GOTOEQ(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Value.Equals(Memory[(int)M2].Value))
                return (int)M3;

            return i + 4;
        }

        public static int GOTOGR(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
            {
                if (Convert.ToInt32(Memory[(int)M1].Value) > Convert.ToInt32(Memory[(int)M2].Value))
                    return (int)M3;
            }
            else if (Memory[(int)M1].Type == (int)VarTypes.DECIMAL)
            {
                if (Convert.ToDouble(Memory[(int)M1].Value) > Convert.ToDouble(Memory[(int)M2].Value))
                    return (int)M3;
            }
            else
            {
                ShowError("Cannot compare non-number types with greater than: Memory: {0}:{1}. Type={2}", M1.ToString(), Memory[(int)M1].ToString(), Memory[(int)M1].Type.ToString());
            }

            return i + 4;
        }

        public static int GOTO(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];

            return (int)M1;
        }

        public static int CALL(int i, ref List<Variable> Memory, List<long> Commands)
        {
            return External(i, ref Memory, Commands);
        }

        public static int DELAY(int i, ref List<Variable> Memory, List<long> Commands)
        {
            System.Threading.ManualResetEvent Waiter = new System.Threading.ManualResetEvent(false);

            Waiter.WaitOne((int)Commands[i + 1]);

            return i + 2;
        }

        public static int SPLIT(int i, ref List<Variable> Memory, List<long> Commands)
        {
            string Val = Memory[(int)Commands[i + 1]].Value;
            char SplitVal = Memory[(int)Commands[i + 2]].Value[0];
            
            List<string> Split = Val.Split(SplitVal).ToList();
            
            Memory[(int)Commands[i + 3]].Value = Split[0];
            if (Split.Count > 1)
                Memory[(int)Commands[i + 4]].Value = String.Join("", Split.Skip(1).ToArray());

            return i + 5;
        }
        
        public static int LENGTH(int i, ref List<Variable> Memory, List<long> Commands)
        {
            if ((Memory[(int)Commands[i + 1]].Type == (int)VarTypes.INTEGER) || (Memory[(int)Commands[i + 1]].Type == (int)VarTypes.DECIMAL))
                Memory[(int)Commands[i + 2]].Value = "1";
            else
                Memory[(int)Commands[i + 2]].Value = Memory[(int)Commands[i + 1]].Value.Count().ToString();
                
            return i + 3;
        }

        public static int IF(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Value.Equals("1"))
            {
                return (int)M2;
            }
            else
            {
                return (int)M3;
            }
        }

        public static int EQ(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            Memory[(int)M3].Value = Convert.ToInt32(Memory[(int)M1].Value.Equals(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int GT(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) > Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) > Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int LT(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) < Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) < Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int GE(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) >= Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) >= Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static int LE(int i, ref List<Variable> Memory, List<long> Commands)
        {
            long M1 = Commands[i + 1];
            long M2 = Commands[i + 2];
            long M3 = Commands[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) <= Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) <= Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public static void ShowError(string Message)
        {
            ShowError(Message, "");
        }

        public static void ShowError(string Message, params string[] args)
        {
            Console.WriteLine(Message, args);
            Console.ReadLine();

            System.Threading.Thread.CurrentThread.Abort();
        }
    }
}
