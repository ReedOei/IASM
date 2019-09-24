using System;
using System.Threading;
using Utility;

public class Script
{
    public static string HandleMethods(string Name)
    {
        Store Params = new Store(Store.SeperateByWord(Name));

        Console.WriteLine("Calling external function");

        if (Params[0] == "delay")
        {
            Thread.Sleep(Convert.ToInt32(Params[1]));
        }
        else if (Params[0] == "clickpos")
        {
            
        }
        else
            return ImageRecognition.Library.ProcessCommand(Name);

        return "";
    }
}