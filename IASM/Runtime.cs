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
        LE,
        ARG,
        GOTOVAR
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

        public int IntValue()
        {
            return Convert.ToInt32(Value);
        }

        public string StringValue()
        {
            return Value;
        }

        public double DoubleValue()
        {
            return Convert.ToDouble(Value);
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class Runtime
    {
        public delegate int Command(int i);

        public External External;
        public List<Command> Commands;

        public List<Variable> Memory = new List<Variable>();

        private List<string> Args;
        private List<long> Code;

        public Runtime(List<string> args, List<long> Code, External External)
        {
            this.Args = args;
            this.Code = Code;
            this.External = External;

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
            Commands.Add(ARG);
            Commands.Add(GOTOVAR);
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

        public string Run()
        {
            if (Code.Count <= 0)
            {
                return "";
            }

            for (int i = 0; i < Code.Count; i++)
            {
                i = Commands[(int)Code[i]](i) - 1;
            }

            return String.Join(" ", Code);
        }

        public Variable GetMemory(long i)
        {
            return Memory[(int)i];
        }

        public void SetMemory(long i, string val)
        {
            GetMemory(i).Value = val;
        }

        public int GOTOVAR(int i)
        {
            long M1 = Code[i + 1];
            return GetMemory(M1).IntValue();
        }

        public int ARG(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];

            int ArgIndex = GetMemory(M1).IntValue();

            if (ArgIndex < Args.Count())
            {
                SetMemory(M2, Args[GetMemory(M1).IntValue()]);
            }
            else
            {
                SetMemory(M2, "");
            }

            return i + 3;
        }

        public int PRINT(int i)
        {
            long M1 = Code[i + 1];

            Console.WriteLine(GetMemory(M1).StringValue());

            return i + 2;
        }

        public int MOV(int i)
        {
            long IsVar = Code[i + 1];
            int Type = (int)Code[i + 2];

            long Data = Code[i + 3];

            string M1 = "";

            int Length = 0;
            if (IsVar == 1) //A constant
            {
                if (Type == (int)VarTypes.STRING)
                {
                    Length = (int)Code[i + 3];

                    for (int x = i + 4; x < (i + 4 + Length); x++)
                    {
                        M1 += Convert.ToChar(Code[x]);
                    }
                }
                else if (Type == (int)VarTypes.DECIMAL)
                {
                    Length = 1;

                    M1 = Convert.ToDouble(Code[i + 3]) + Convert.ToDouble("0." + Code[i + 4].ToString()).ToString();
                }
                else
                    M1 = Code[i + 3].ToString();
            }
            else
                M1 = Memory[(int)Data].Value;

            long M2 = Code[i + 4 + Length];

            if (M2 >= Memory.Count)
                Memory.Add(new Variable(Type, ""));

            Memory[(int)M2].Value = M1;

            return i + 5 + Length;
        }

        public int ADD(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) + Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else if (Memory[(int)M1].Type == (int)VarTypes.DECIMAL)
                Memory[(int)M3].Value = (Convert.ToDouble(Memory[(int)M1].Value) + Convert.ToDouble(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Memory[(int)M1].Value + Memory[(int)M2].Value;

            return i + 4;
        }

        public int SUB(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) - Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = (Convert.ToDouble(Memory[(int)M1].Value) - Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public int MULT(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) * Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = (Convert.ToDouble(Memory[(int)M1].Value) * Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public int DIV(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

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

        public int MOD(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            Memory[(int)M3].Value = (Convert.ToInt32(Memory[(int)M1].Value) % Convert.ToInt32(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public int GOTOEQ(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Value.Equals(Memory[(int)M2].Value))
                return (int)M3;

            return i + 4;
        }

        public int GOTOGR(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

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

        public int GOTO(int i)
        {
            long M1 = Code[i + 1];

            return (int)M1;
        }

        public int CALL(int i)
        {
            return External.Run(i, ref Memory, Code);
        }

        public int DELAY(int i)
        {
            System.Threading.ManualResetEvent Waiter = new System.Threading.ManualResetEvent(false);

            Waiter.WaitOne((int)Code[i + 1]);

            return i + 2;
        }

        public int SPLIT(int i)
        {
            string Val = Memory[(int)Code[i + 1]].Value;
            char SplitVal = Memory[(int)Code[i + 2]].Value[0];

            List<string> Split = Val.Split(SplitVal).ToList();

            Memory[(int)Code[i + 3]].Value = Split[0];
            if (Split.Count > 1)
                Memory[(int)Code[i + 4]].Value = String.Join("", Split.Skip(1).ToArray());

            return i + 5;
        }

        public int LENGTH(int i)
        {
            Memory[(int)Code[i + 2]].Value = Memory[(int)Code[i + 1]].Value.Count().ToString();

            return i + 3;
        }

        public int IF(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Value.Equals("1"))
            {
                return (int)M2;
            }
            else
            {
                return (int)M3;
            }
        }

        public int EQ(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            Memory[(int)M3].Value = Convert.ToInt32(Memory[(int)M1].Value.Equals(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public int GT(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) > Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) > Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public int LT(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) < Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) < Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public int GE(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) >= Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) >= Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public int LE(int i)
        {
            long M1 = Code[i + 1];
            long M2 = Code[i + 2];
            long M3 = Code[i + 3];

            if (Memory[(int)M1].Type == (int)VarTypes.INTEGER)
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToInt32(Memory[(int)M1].Value) <= Convert.ToInt32(Memory[(int)M2].Value)).ToString();
            else
                Memory[(int)M3].Value = Convert.ToInt32(Convert.ToDouble(Memory[(int)M1].Value) <= Convert.ToDouble(Memory[(int)M2].Value)).ToString();

            return i + 4;
        }

        public void ShowError(string Message)
        {
            ShowError(Message, "");
        }

        public void ShowError(string Message, params string[] args)
        {
            Console.WriteLine(Message, args);
            Console.ReadLine();

            System.Threading.Thread.CurrentThread.Abort();
        }
    }
}

