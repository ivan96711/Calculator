using System;
using System.Data;
using System.Diagnostics;

namespace Calculator
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Введите пример:");
                string line = Console.ReadLine();

                try
                {
                    Console.WriteLine("Сustom сalculator: " + СustomCalculator.Calculate(line));
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("Сustom сalculator: " + e.Message);
                }

                try
                {
                    Console.WriteLine("DataTable: " + new DataTable().Compute(line, null).ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                Console.WriteLine();
            }
        }
    }
}
