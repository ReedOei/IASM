using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using WindowsInput.Native;
using WindowsInput;
using System.Threading;
using System.Runtime.InteropServices;

namespace IASM
{
    public class External
    {
        public static int Run(int i, ref List<Variable> Memory, List<long> Commands)
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

    }

    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                if (args[0] == "compile")
                {
                    var CompiledCode = new Compiler(File.ReadAllLines(args[1]).ToList()).Compile();
                    if (CompiledCode != null)
                    {
                        if (args.Length > 2) // Save to a file
                        {
                            File.WriteAllText(args[2], string.Join(" ", CompiledCode.Select(X => X.ToString())));
                        }
                        else // Print to screen
                        {
                            Console.WriteLine(string.Join(" ", CompiledCode.Select(X => X.ToString())));
                        }
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (args[0] == "run")
                {
                    var CompiledCode = File.ReadAllText(args[1]).Split(' ').Select(X => Convert.ToInt64(X)).ToList();
                    new Runtime(args.ToList().Skip(1).ToList(), CompiledCode, new External()).Run();
                }
                else
                {
                    ShowUsage();
                    return 1;
                }
            }
            else
            {
                ShowUsage();
                return 1;
            }

            return 0;
        }

        public static void ShowUsage()
        {
            Console.WriteLine("Usage: IASM command FILE");
            Console.WriteLine("  command - May be 'compile' or 'run'");
        }
    }
}

