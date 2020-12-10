using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calculator
{
    static class СustomCalculator
    {
        private class Item
        {
            public double? Number { get; set; }
            public string Operator { get; set; }
        }

        private delegate double OperatorDelegate(double a, double b);
        private readonly static Dictionary<string, OperatorDelegate> operators;
        private readonly static Dictionary<string, byte> priority;
        static СustomCalculator()
        {
            operators = new Dictionary<string, OperatorDelegate>
            {
                {"+", (double a, double b) => { return a + b; } },
                {"-", (double a, double b) => { return a - b; } },
                {"*", (double a, double b) => { return a * b; } },
                {"/", (double a, double b) => { if (b == 0) throw new ArgumentException("Делитель не может быть равен 0"); return a / b; } }
            };
            priority = new Dictionary<string, byte>
            {
                {"+", 1 },
                {"-", 1 },
                {"*", 2 },
                {"/", 2 },
                {"(", 3 },
                {")", 3 }
            };
        }

        /// <summary>
        /// Произвести подсчет значения строкового выражения 
        /// </summary>
        /// <param name="line">Выражение</param>
        /// <returns></returns>
        public static double Calculate(string line)
        {
            //десятичные дроби подгоняем под один стандарт
            line = line.Replace(',', '.').Replace(" ", "");

            if (line == "")
                throw new ArgumentException("Пустая строка");
            //проверка строки на посторонние символы
            string pattern = @"([\d,.]+)|[+, \-, *,\/,(,)]";
            if (Regex.Replace(line, pattern, string.Empty) != "")
                throw new ArgumentException("Присутствуют посторонние символы: " + Regex.Replace(line, pattern, " "));

            double result = Cal(line);

            return result;
        }

        private static double Cal(string line)
        {
            //в цикле просчитываем от внутренних скобок к внешним до тех пор, пока sepOfPar не выдаст Null, т.е. закрывающиеся скобки не найдены
            List<string> sepOfPar = SeparationOfParentheses(line);
            while (sepOfPar != null && sepOfPar.Count != 0)
            {
                for (int i = 0; i < sepOfPar.Count(); i++)
                {
                    var par = CreateParentheses(sepOfPar[i]);
                    if (par == "")
                        throw new ArgumentException("Ошибка в введенном выражении");
                    List<Item> strParts2 = StrToParts(par);
                    double middleValue = LinearCounting(strParts2);

                    string strReplace = "(" + sepOfPar[i] + ")";
                    int position = line.IndexOf(strReplace);
                    line = line.Remove(position, strReplace.Length).Insert(position, middleValue.ToString().Replace(",", "."));
                }
                sepOfPar = SeparationOfParentheses(line);
            }

            List<Item> strParts3 = StrToParts(CreateParentheses(line));
            double result = LinearCounting(strParts3);

            return result;
        }

        /// <summary>
        /// Создание скобок для правильного порядка выполнения операций
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string CreateParentheses(string line)
        {
            List<Item> strParts = StrToParts(line);

            //не трогаем случаи, когда все операторы в строке имеют одинаковый вес
            bool lowPriority = false;
            bool highPriority = false;
            foreach (var part in strParts)
            {
                if (part.Operator != null && priority[part.Operator] == 1)
                    lowPriority = true;
                if (part.Operator != null && priority[part.Operator] == 2)
                    highPriority = true;
            }
            if (lowPriority != highPriority)
                return line;

            List<Item> strPartsTmp = new List<Item>();
            int? startPosition = null;
            int? finishPosition = null;
            int offset = 0;
            for (int i = 0; i < strParts.Count(); i++)
            {
                strPartsTmp.Add(strParts[i]);

                //если оператор меньшего веса, то добавляем его
                //если уже имеется позиция открытой скобки, то закрываем ее на позиции i + 1 (из-за добавления первой скобки, произойдет смещение на 1 вправо, поэтому текущий оператор не будет потерян)
                if (strParts[i].Operator == "+" || strParts[i].Operator == "-")
                {
                    if (startPosition != null && finishPosition == null)
                        finishPosition = i + 1 + offset;
                }

                if (strParts[i].Operator == "*" || strParts[i].Operator == "/")
                {
                    if (startPosition == null && finishPosition == null)
                        startPosition = i - 1 + offset;

                    //проверка, если оператор был последним
                    if (i + 2 == strParts.Count())
                    {
                        finishPosition = i + 3 + offset;
                        continue;
                    }
                }

                if (startPosition != null && finishPosition != null)
                {
                    strPartsTmp.Insert((int)startPosition, new Item { Operator = "(" });
                    strPartsTmp.Insert((int)finishPosition, new Item { Operator = ")" });
                    startPosition = null;
                    finishPosition = null;
                    offset = offset + 2;
                }
            }

            line = "";
            foreach (var a in strPartsTmp)
            {
                line += a.Number != null ? a.Number.ToString() : a.Operator;
            }

            if (line.Contains("(") && line.Contains(")"))
                line = Cal(line).ToString().Replace(",", ".");
            return line;
        }

        /// <summary>
        /// Поиск скобок без вложеных скобок
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static List<string> SeparationOfParentheses(string line)
        {
            if (!line.Contains("(") && !line.Contains(")"))
                return null;

            string pattern = @"\(([\d,\+, \-, \*,\/]*)\)";
            MatchCollection matches = Regex.Matches(line, pattern);
            List<string> strList = new List<string>();
            if (matches.Count > 0)
                foreach (Match match in matches)
                {
                    strList.Add(match.Groups[1].Value);
                }

            return strList;
        }

        private static List<Item> StrToParts(string line)
        {
            string pattern = @"([\d,.]+)|[+, \-, *,\/,(,)]";
            MatchCollection matches = Regex.Matches(line, pattern);
            List<Item> strParts = new List<Item>();
            if (matches.Count > 0)
                foreach (Match match in matches)
                {
                    double result;
                    if (double.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                    {
                        //NumberStyles.Any возвращает некорректное значение для бесконечных десяичных дробей
                        //result = double.Parse(match.Value, CultureInfo.InvariantCulture);
                        //отрабатываем отрицательные значения, чтобы минус попал в число, а не в операторы. Дополнительно решаем вопрос простановки скобок в подобных примерах: 2*-2
                        if (strParts.Count > 1 && strParts[strParts.Count - 2].Operator != null && strParts[strParts.Count - 1].Operator == "-")
                        {
                            strParts.RemoveAt(strParts.Count - 1);
                            strParts.Add(new Item { Number = result * (-1) });
                        }
                        else
                            strParts.Add(new Item { Number = result });
                    }
                    else
                        strParts.Add(new Item { Operator = match.Value });
                }

            return strParts;
        }

        private static double LinearCounting(List<Item> strParts)
        {
            //возвращаем значение, если на вход поступило одно число
            if (strParts.Count == 1 && strParts[0].Number != null)
                return (double)strParts[0].Number;

            for (int i = 0; i < strParts.Count(); i++)
            {
                if (strParts[i].Operator == null)
                    continue;
                //не должны проскакивать одиночные скобки
                if (strParts[i].Operator == "(" || strParts[i].Operator == ")")
                    throw new ArgumentException("Нет парных закрывающих скобок");
                //оператор не должен быть первым (кроме + и -) и последним в строке
                if (i - 1 < 0 && (strParts[i].Operator == "*" || strParts[i].Operator == "/"))
                    throw new ArgumentException("У оператора " + strParts[i].Operator + " отсутствует первый аргумент");
                if (i + 1 >= strParts.Count())
                    throw new ArgumentException("У оператора " + strParts[i].Operator + " отсутствует второй аргумент");
            }

            double first;
            double second;
            double? previous = null;
            for (int i = 0; i < strParts.Count(); i++)
            {
                //ищем оператор
                if (strParts[i].Operator == null)
                    continue;

                if (i - 1 >= 0 && strParts[i - 1].Number == null || i + 1 < strParts.Count() && strParts[i + 1].Number == null)
                    throw new ArgumentException("Операторы расположены подряд");

                //фиксируем предыдущее (first) и последующее (second) значения
                //перед операторами + и - может не стоять число, отрабатываем этот случай для first
                if ((strParts[i].Operator == "+" || strParts[i].Operator == "-") && (i == 0 || strParts[i].Operator == "("))
                    first = 0;
                else
                    first = (double)strParts[i - 1].Number;
                second = (double)strParts[i + 1].Number;

                //производим действие
                previous = operators[strParts[i].Operator](previous == null ? first : (double)previous, second);
            }

            return (double)previous;
        }
    }
}
