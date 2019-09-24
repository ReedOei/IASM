using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using WindowsInput.Native;
using WindowsInput;
using System.Threading;
using System.Runtime.InteropServices

namespace IASM
{
    public class Program
    {
        public static int External(int i, ref List<Variable> Memory, List<long> Commands)
        {
            List<long> Applied = Commands.Skip(i + 3).ToList();
            Applied = Applied.Take((int)Commands[i + 2]).ToList();
            
            for (int k = 0; k < Applied.Count(); k++)
            {
                if (Applied[k] == -1)
                {
                    Applied.RemoveAt(k);
                    int VariableIndex = (int)Applied[k];
                    Applied.RemoveAt(k);
                    string VariableValue = Memory[VariableIndex].Value;

                    for (int v = 0; v < VariableValue.Count(); v++ )
                    {
                        Applied.Insert(k + v, VariableValue[v]);
                    }

                    if ((k + VariableValue.Count()) != (Applied.Count() - 1))
                        Applied.Insert(k + VariableValue.Count(), 32L);
                }
            }

            string Params = String.Join("", Applied.Select(N => Convert.ToChar(N)).ToArray());

            string Result = ProcessCommand(Params);

            Memory[(int)Commands[i + 1]].Value = Result;

            //Console.WriteLine(Result);

            return i + (int)Commands[i + 2] + 4 - 1;
        }

        public static string ProcessCommand(string Params)
        {
            var CommandArgs = Params.Split(' ').Skip(1).ToList();
            var CommandName = Params.Split(' ')[0];

            if (CommandName.Equals("keypress"))
            {
                if (PressKey(CommandArgs[0].ToUpper()))
                {
                    return CommandArgs[0].ToUpper();
                }

                return "NO KEY FOUND";
            }
            else if (CommandName.Equals("type"))
            {
                for (int i = 1; i < CommandArgs.Count(); i++)
                {
                    Type(Convert.ToInt32(CommandArgs[0]), CommandArgs[i]);
                }

                return Params;
            }
            else
            {
                return ImageRecognition.Library.ProcessCommand(Params);
            }
        }

        private static Dictionary<string, VirtualKeyCode> Keycodes()
        {
            var Result = new Dictionary<string, VirtualKeyCode>();

            foreach (VirtualKeyCode kc in Enum.GetValues(typeof(VirtualKeyCode)))
            {
                if (kc.ToString().StartsWith("VK_"))
                {
                    Result[kc.ToString().Substring(3)] = kc;
                }
                else
                {
                    Result[kc.ToString()] = kc;
                }
            }

            return Result;
        }

        public static string Type(int Delay, string Str)
        {
            List<string> Keys = new List<string>();

            Str = Str.ToUpper();

            while (Str.Length > 0)
            {
                var KeyCode = Keycodes().First(kc => Str.StartsWith(kc.Key));
                PressKey(KeyCode.Value.ToString());

                Str = Str.Substring(KeyCode.Key.Length);

                Thread.Sleep(Delay);
            }

            return Str;
        }

        public static bool PressKey(VirtualKeyCode KeyCode)
        {
            new InputSimulator().Keyboard.KeyPress(KeyCode);

            return true;
        }

        public static bool PressKey(string Key)
        {
            foreach (VirtualKeyCode KeyCode in Enum.GetValues(typeof(VirtualKeyCode)))
            {
                if (KeyCode.ToString().Equals(Key))
                {
                    return PressKey(KeyCode);
                }
            }

            return false;
        }

        static void Main(string[] args)
        {
            Runtime.Init();

            Runtime Run = new Runtime();

            Runtime.External = External;

            if (args.Length > 0)
            {
                if (args[0].Contains('.'))
                    args = System.IO.File.ReadAllLines(args[0]);

                Run.Run(Compiler.Compile(args.ToList()));
            }
            else
            {
                string FileName = Console.ReadLine();
                
                args = System.IO.File.ReadAllLines(FileName);

                Run.Run(Compiler.Compile(args.ToList()));
            }

            Console.WriteLine(Run.Dump());

            Console.ReadLine();
        }
    }
}
