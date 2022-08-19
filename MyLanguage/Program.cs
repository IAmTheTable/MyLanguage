using System;
using MyLanguage.Parser;

namespace MyLanguage
{

    public class Program
    {
        public static void Main(string[] args)
        {
            string x = "5 + 5 + 69 + 2;";
            Parser.Parser p = new(x);

            p.Parse();
        }
    }
}