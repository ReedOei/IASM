using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Drawing;

namespace Utility
{
    public static class Utility
    {
        public static Random Generator;

        public static double StandardDeviation(this List<double> Input)
        {
            double Mean = Input.Average();

            return Math.Sqrt(Input.Select(N => Math.Pow(N - Mean, 2)).Average());
        }

        public static double FirstQuartile(List<double> Values)
        {
            Values.Sort();

            int Index = Values.Take(Values.Count() / 2).Count();

            double Result = Values[Index / 2 - 1];

            if (Index % 2 == 0) //It is even
            {
                Result += Values[Index / 2];

                Result /= 2;
            }

            return Result;
        }

        public static double ThirdQuartile(List<double> Values)
        {
            Values.Sort();

            Values.RemoveRange(0, Values.Count() / 2);

            int Index = Values.Count();

            double Result = Values[Index / 2 - 1];

            if (Index % 2 == 0) //It is even
            {
                Result += Values[Index / 2];

                Result /= 2;
            }

            return Result;
        }

        public static double InterQuartileRange(List<double> Values)
        {
            Values.Sort();

            return ThirdQuartile(new List<double>(Values)) - FirstQuartile(Values);
        }

        public static double Distance(Point X, Point Y)
        {
            return Math.Sqrt(Math.Pow(X.X - Y.Y, 2) + Math.Pow(X.Y - Y.Y, 2));
        }

        public static int Matches(string Input, params string[] Checks)
        {
            for (int i = 0; i < Checks.Length; i++)
            {
                if (Input == Checks[i])
                    return i;
            }

            return -1;
        }

        public static List<N> SubList<N>(List<N> Sub, int StartIndex)
        {
            List<N> Result = new List<N>();

            for (int i = StartIndex; i < Sub.Count; i++)
                Result.Add(Sub[i]);

            return Result;
        }

        public static int GetHighest(int One, int Two)
        {
            return (One > Two) ? One : Two;
        }

        public static int Greatest(List<int> Check)
        {
            int Highest = 0;

            for (int i = 0; i < Check.Count; i++)
            {
                if (Check[i] > Check[Highest])
                    Highest = i;
            }

            return Highest;
        }

        public static int LowerBound(int One, int Limit)
        {
            if (One < Limit)
                return Limit;
            else
                return One;
        }

        public static int Clamp(int A, int Limit)
        {
            return (A > Limit) ? Limit : A;
        }

        public static int GetLowest(int One, int Two)
        {
            return (int)GetLowest((double)One, (double)Two);
        }

        public static double GetLowest(double One, double Two)
        {
            return (One < Two) ? One : Two;
        }

        public static int GetGreatest(int One, int Two)
        {
            return (int)GetGreatest((double)One, (double)Two);
        }

        public static double GetGreatest(double One, double Two)
        {
            return (One > Two) ? One : Two;
        }

        public static bool RoughMatch(double ValueOne, double ValueTwo, double Precision)
        {
            return ((ValueOne + Precision) >= ValueTwo) && (ValueOne <= (ValueTwo + Precision));
        }

        public static string StripExtension(string FileName)
        {
            return FileName.Substring(0, FileName.IndexOf('.'));
        }

        public static string StripDirectoryName(string FileName, string DirectoryName)
        {
            return FileName.Replace(DirectoryName, "");
        }

        public static XmlElement GetFirstNodeTag(string Name, XmlDocument Data)
        {
            XmlNodeList Nodes = Data.GetElementsByTagName(Name);

            if (Nodes.Count > 0)
                return (XmlElement)Nodes[0];
            else
            {
                XmlElement Invalid = new XmlDocument().CreateElement("i");
                Invalid.InnerText = "0";
                return Invalid;
            }
        }

        public static List<XmlNode> GetTopLevelNodes(string Name, XmlDocument Data)
        {
            List<XmlNode> Matches = new List<XmlNode>();

            foreach (XmlNode Check in Data.DocumentElement.ChildNodes)
            {
                if (Check.Name == Name)
                    Matches.Add(Check);
            }

            return Matches;
        }

        public static bool HasNodeTag(string Name, XmlDocument Data)
        {
            return Data.GetElementsByTagName(Name).Count > 0;
        }

        public static XmlDocument ToXmlDocument(XmlNode Node)
        {
            XmlDocument Result = new XmlDocument();
            Result.LoadXml(Node.OuterXml);

            return Result;
        }

        public static XmlDocument ToXmlDocument(XmlNodeList Data)
        {
            XmlDocument Result = new XmlDocument();

            string XML = "";

            foreach (XmlNode Add in Data)
            {
                XML += Add.OuterXml;
            }

            Result.LoadXml("<root>" + XML + "</root>");

            return Result;
        }
    }

    public class Store
    {
        public Store()
        {
            Information = new List<string>();
        }

        public Store(List<string> InitValue)
        {
            Information = InitValue;
        }

        public Store(string RawValue)
        {
            Information = SeperateByWord(RawValue);
        }

        public Store(params string[] RawValue)
        {
            Information = new List<string>();

            foreach (string Value in RawValue)
            {
                Information.Add(Value);
            }
        }

        public static string RemovePunctuation(string Input)
        {
            Input = Input.Trim('.', '?', '!', ',', '\\', '/');

            return Input;
        }

        public bool AddInformation(string NewInformation)
        {
            Information.Add(NewInformation);

            return true;
        }

        public static List<string> Lower(List<string> Input)
        {
            for (int i = 0; i < Input.Count; i++)
            {
                Input[i] = Input[i].ToLower();
            }

            return Input;
        }

        public Store Lower()
        {
            for (int i = 0; i < Information.Count; i++)
            {
                Information[i] = Information[i].ToLower();
            }

            return this;
        }

        public string ConcatAllInformation()
        {
            string Result = "";

            for (int i = 0; i < Information.Count; i++)
            {
                Result += Information[i] + " ";
            }

            return Result;
        }

        public string SafeConcat()
        {
            string Result = "";

            for (int i = 0; i < Information.Count; i++)
            {
                if (Information[i].IndexOf(' ') != -1)
                {
                    Result += "\"" + Information[i] + "\" ";
                }
                else
                    Result += Information[i] + " ";
            }

            return Result;
        }

        public void Clear()
        {
            Information.Clear();
        }

        public void RemoveEmpty()
        {
            Information.RemoveAll(String.IsNullOrWhiteSpace);
        }

        public void CullString(string Cull)
        {
            for (int i = 0; i < Information.Count; i++)
            {
                Information[i] = Information[i].Replace(Cull, "");
            }
        }

        public void SeperateByWord(Store ToBeSeperated)
        {
            string Seperated = "";
            bool Quotes = false;

            for (int i = 0; i < ToBeSeperated.Information.Count; i++)
            {
                for (int l = 0; l < ToBeSeperated.Information[i].Length; l++)
                {
                    if (Quotes)
                    {
                        if (ToBeSeperated.Information[i].ElementAt(l) == '\"')
                        {
                            Information.Add(Seperated);

                            Seperated = "";

                            Quotes = false;
                        }
                        else
                        {
                            Seperated += ToBeSeperated.Information[i].ElementAt(l);
                        }
                    }
                    else
                    {
                        if (ToBeSeperated.Information[i].ElementAt(l) == '\"')
                        {
                            Quotes = true;
                        }
                        else if (ToBeSeperated.Information[i].ElementAt(l) == ' ')
                        {
                            Information.Add(Seperated);

                            Seperated = "";
                        }
                        else
                        {
                            Seperated += ToBeSeperated.Information[i].ElementAt(l);
                        }
                    }
                }
            }

            if (!System.String.IsNullOrWhiteSpace(Seperated))
            {
                Information.Add(Seperated);
            }
        }

        public static int GetNumber(string Check)
        {
            string ResultString = "0";

            foreach (Char e in Check)
            {
                if ((e >= '0') && (e <= '9'))
                    ResultString += e;
            }

            ResultString = ResultString.Remove(8, ResultString.Length - 8);

            return Convert.ToInt32(ResultString);
        }

        public static List<string> SeperateByWord(string ToBeSeperated)
        {
            string Seperated = "";
            bool Quotes = false;

            List<string> ResultSeperated = new List<string>();

            for (int l = 0; l < ToBeSeperated.Length; l++)
            {
                if (Quotes)
                {
                    if (ToBeSeperated[l] == '\"')
                    {
                        ResultSeperated.Add(Seperated);

                        Seperated = "";

                        Quotes = false;
                    }
                    else
                    {
                        Seperated += ToBeSeperated[l];
                    }
                }
                else
                {
                    if (ToBeSeperated[l] == '\"')
                    {
                        Quotes = true;
                    }
                    else if (ToBeSeperated[l] == ' ')
                    {
                        ResultSeperated.Add(Seperated);

                        Seperated = "";
                    }
                    else
                    {
                        Seperated += ToBeSeperated[l];
                    }
                }
            }

            if (!System.String.IsNullOrWhiteSpace(Seperated))
            {
                ResultSeperated.Add(Seperated);
            }

            return ResultSeperated;
        }

        /// <summary>
        /// Seperates the input into words and phrases. If an entry is a phrase it will be left as one word even if it is longer than that.
        /// </summary>
        /// <param name="ToBeSeperated">The input to be seperated into a list.</param>
        /// <param name="Phrases">The list of phrases to recognize.</param>
        /// <returns></returns>
        public static List<string> SeperateByPhrase(string ToBeSeperated, List<string> Phrases)
        {
            List<string> Result = new List<string>();
            foreach (string Phrase in Phrases)
            {
                int Start = ToBeSeperated.IndexOf(Phrase);

                while (Start != -1)
                {
                    Result.Add(ToBeSeperated.Substring(Start, Phrase.Length));
                    ToBeSeperated = ToBeSeperated.Remove(Start, Phrase.Length);
                    Start = ToBeSeperated.IndexOf(Phrase);
                }
            }

            Result.AddRange(SeperateByWord(ToBeSeperated));

            return Result;
        }

        public string GetInformation(int index)
        {
            if (index >= Information.Count)
            {
                return "";
            }
            else
            {
                return Information[index];
            }
        }

        public int Count()
        {
            return Information.Count;
        }

        public List<string> Information { get; set; }

        public string this[int Index]
        {
            get
            {
                return GetInformation(Index);
            }

            set
            {
                if (Index < Information.Count)
                    Information[Index] = value;
                else
                    Information.Add(value);
            }
        }
    }
}