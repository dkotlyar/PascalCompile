using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Calculation
{
    Environs environment;

    public Calculation() { }

    public Calculation(Environs _environment)
    {
        environment = _environment;
    }

    /// <summary>
    /// Производит рассчёт выражения, результат выдается типа double или boolean
    /// </summary>
    /// <param name="expression">Исходное выражение</param>
    /// <returns>Ответ (число или логическое значеие)</returns>
    public object Calculate(string expression)
    {
        return CalcPolsc(ToPolsc(expression));
    }

    /// <summary>
    /// Преобразует выражение в обратную Польскую нотацию
    /// </summary>
    /// <param name="expression">Исходное выражение</param>
    /// <returns>Выражение в формате ОПН</returns>
    private string ToPolsc(string expression)
    {
        int open_bkt = expression.Split('(').Length - 1;
        int close_bkt = expression.Split(')').Length - 1;
        if (open_bkt != close_bkt)
            throw new Exception("Количество скобок в выражении \"" + expression + "\" не совпадает");

        #region func
        //Regex func_start = new Regex(@"([A-Za-z0-9_]+)\(");
        //while (func_start.IsMatch(expression))
        //{
        //    string func_name = "";
        //    int start_pos = func_start.Match(expression).Index;
        //    while (expression[start_pos] != '(')
        //        func_name += expression[start_pos++];

        //    start_pos++;
        //    int bkt_count = 1;
        //    int end_pos;
        //    for (end_pos = start_pos; end_pos < expression.Length && bkt_count > 0; end_pos++)
        //        if (expression[end_pos] == '(')
        //            bkt_count++;
        //        else if (expression[end_pos] == ')')
        //            bkt_count--;
        //    string sub_expression = expression.Remove(0, start_pos)
        //        .Remove(end_pos - start_pos - 1);
        //    Console.WriteLine(func_name);
        //    Console.WriteLine(sub_expression);
        //    Console.ReadKey(true);
        //    //expression = expression.Replace("(" + sub_expression + ")", Calculate(sub_expression).ToString());
        //}
        #endregion

        while (expression.IndexOf("(") > -1)
        {
            int start_pos = expression.IndexOf("(") + 1;
            int bkt_count = 1;
            int end_pos;
            for (end_pos = start_pos; end_pos < expression.Length && bkt_count > 0; end_pos++)
                if (expression[end_pos] == '(')
                    bkt_count++;
                else if (expression[end_pos] == ')')
                    bkt_count--;
            string sub_expression = expression.Remove(0, start_pos)
                .Remove(end_pos - start_pos - 1);
            expression = expression.Replace("(" + sub_expression + ")", Calculate(sub_expression).ToString());
        }

        return SplitRegexFourth(expression);
    }

    /// <summary>
    /// Разделяет выражение на операции с самым низким приоритетом
    /// </summary>
    /// <param name="expression">Исходное выражение</param>
    /// <returns>Выражение в формате ОПН</returns>
    private string SplitRegexFourth(string expression)
    {
        string result = "";
        string[] words = new Regex(@"(?:[\s]*(<>|<=|>=|=|<|>)[\s]*)").Split(expression);
        if (words.Length == 0)
            throw new Exception("Пустое выражение");

        for (int i = 0; i < words.Length; i += 2)
        {
            string sum_result = SplitRegexThird(words[i]);
            result += sum_result + " ";
            if (i > 1)
                result += words[i - 1] + " ";
        }

        return result;
    }

    /// <summary>
    /// Разделяет выражение на операции с приоритетом №3
    /// </summary>
    /// <param name="expression">Исходное выражение</param>
    /// <returns>Выражение в формате ОПН</returns>
    private string SplitRegexThird(string expression)
    {
        string result = "";
        string[] words = new Regex(@"(?:[\s]*(\+|or|-)[\s]*)").Split(expression);
        if (words.Length == 0)
            throw new Exception("Пустое выражение");

        for (int i = 0; i < words.Length; i += 2)
        {
            string sum_result = SplitRegexSecond(words[i]);
            result += sum_result + " ";
            if (i > 1)
                result += words[i - 1] + " ";
        }

        return result;
    }

    /// <summary>
    /// Разделяет выражение на операции с приоритетом №2
    /// </summary>
    /// <param name="expression">Исходное выражение</param>
    /// <returns>Выражение в формате ОПН</returns>
    private string SplitRegexSecond(string expression)
    {
        string result = "";
        string[] words = new Regex(@"(?:[\s]*(\*|/|div|mod|and)[\s]*)").Split(expression);
        if (words.Length == 0)
            throw new Exception("Пустое выражение");

        for (int i = 0; i < words.Length; i += 2)
        {
            string sum_result = SplitRegexFirst(words[i]);
            result += sum_result + " ";
            if (i > 1)
                result += words[i - 1] + " ";
        }

        return result;
    }

    /// <summary>
    /// Разделяет выражение на операции с приоритетом №1
    /// </summary>
    /// <param name="expression">Исходное выражение</param>
    /// <returns>Выражение в формате ОПН</returns>
    private string SplitRegexFirst(string expression)
    {
        string result = "";
        string[] words = new Regex(@"(?:[\s]*(not)[\s]*)").Split(expression);
        if (words.Length == 0)
            throw new Exception("Пустое выражение");

        if (words.Length == 1)
            return OperandToNormal(expression);

        for (int i = 2; i < words.Length; i += 2)
            result += OperandToNormal(words[i]) + " " + words[i - 1];

        return result;
    }

    /// <summary>
    /// Вычисляет значение вырожения из ОПН
    /// </summary>
    /// <param name="expression">Выражение в формате ОПН</param>
    /// <returns>Результат вычисления, число или логическое значение</returns>
    private object CalcPolsc(string expression)
    {
        string[] operands = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string[] operations = new string[] { "<>", "<=", "<", ">=", ">", "=", "+", "-",
                "or", "and", "*", "/", "div", "mod" }; // "not"

        Stack stack = new Stack();

        foreach (string word in operands)
        {
            if (string.IsNullOrWhiteSpace(word))
                continue;
            if (word == "not")
            {
                object operand = stack.Pop();
                if (operand.GetType().Name != "Boolean")
                    throw new Exception("Операция NOT не применима к типу " + operand.GetType().Name);
                stack.Push(!(bool)operand);
            }
            else if (operations.Contains(word))
            {
                object result = SimpleCalc(stack.Pop(), stack.Pop(), word);
                stack.Push(result);
            }
            else
            {
                double d;
                bool b;
                if (bool.TryParse(word, out b))
                    stack.Push(b);
                else if (double.TryParse(word, out d))
                    stack.Push(d);
                else
                    throw new Exception("Неизвестный тип операнда в выражении");
            }
        }

        if (stack.Count == 1)
            return stack.Pop();
        else
            throw new Exception("Не все операции были выполнены");
    }

    /// <summary>
    /// Производит операцию над двумя операндами
    /// </summary>
    /// <param name="x">Второй операнд в выражении</param>
    /// <param name="y">Первый операнд в выражении</param>
    /// <param name="operation">Знак операции</param>
    /// <returns>Результат вычисления, число или логическое значение</returns>
    private object SimpleCalc(object x, object y, string operation)
    {
        if (x == null || y == null)
            throw new Exception("Операция не применима к типу null");
        if (x.GetType().Name == "Double" && y.GetType().Name == "Double")
            return DoubleCalc((double)y, (double)x, operation);
        else if (x.GetType().Name == "Boolean" && y.GetType().Name == "Boolean")
            return BoolCalc((bool)y, (bool)x, operation);
        else
            throw new Exception(string.Format("Операция не применима к типам {0} и {1}",
                x.GetType().Name, y.GetType().Name));
    }

    /// <summary>
    /// Производит алгербоическую операцию над двумя операндами
    /// </summary>
    /// <param name="x">Первый операнд в выражении</param>
    /// <param name="y">Второй операнд в выражении</param>
    /// <param name="operation">Знак операции</param>
    /// <returns>Результат вычисления, число</returns>
    private object DoubleCalc(double x, double y, string operation)
    {
        switch (operation)
        {
            case "+":
                return x + y;
            case "-":
                return x - y;
            case "*":
                return x * y;
            case "/":
                return x / y;
            case "div":
                return (int)(x / y);
            case "mod":
                return (int)x % (int)y;
            case "<>":
                return x != y;
            case "<=":
                return x <= y;
            case "<":
                return x < y;
            case ">=":
                return x >= y;
            case ">":
                return x > y;
            case "=":
                return x == y;
            default:
                throw new Exception(string.Format("Операция {0} не применима к типу Double", operation));
        }
    }

    /// <summary>
    /// Производит логическую операцию над двумя операндами
    /// </summary>
    /// <param name="x">Первый операнд в выражении</param>
    /// <param name="y">Второй операнд в выражении</param>
    /// <param name="operation">Знак операции</param>
    /// <returns>Результат вычисления, логическое значение</returns>
    private object BoolCalc(bool x, bool y, string operation)
    {
        switch (operation)
        {
            case "<>":
                return x != y;
            case "=":
                return x == y;
            case "or":
                return x || y;
            case "and":
                return x && y;
            default:
                throw new Exception(string.Format("Операция {0} не применима к типу Boolean", operation));
        }
    }

    /// <summary>
    /// Приводит операнд к нормальному виду
    /// </summary>
    /// <param name="operand">Исходный операнд</param>
    /// <returns></returns>
    private string OperandToNormal(string operand)
    {
        double d;
        if (double.TryParse(operand.Replace(".", ","), out d))
            return d.ToString();
        
        Variable v = environment.GetElementByName(operand);

        //Console.WriteLine(operand);
        //v.Dump();

        if (v.GetType().Name != "NullVariable")
            return v.value.ToString();

        return operand;
    }
}