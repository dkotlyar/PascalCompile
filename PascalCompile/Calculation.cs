using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;

public class Calculation
{
    Environs environment;

    public struct MathStruct
    {
        public int type;
        public object value;
    }

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
        //Console.WriteLine("'{0}'", expression);
        //Queue opn = ToPolscQueue(expression);
        //foreach (object obj in opn)
        //    Console.WriteLine("{0} :: {1}", ((MathStruct)obj).value, ((MathStruct)obj).type);
        ////Console.ReadKey(true);
        //object result = CalcPolsc(opn);
        //Console.WriteLine(result);
        //Console.ReadKey(true);
        //return CalcPolsc(opn);
        return CalcPolsc(ToPolscQueue(expression));
        //return CalcPolsc(ToPolsc(expression));
    }

    /// <summary>
    /// Вычисляет обратную польскую нотацию из переданного выражения
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public Queue ToPolscQueue(string expression)
    {
        //Console.WriteLine("'{0}'", expression);
        Queue operands = new Queue();

        string[] fourh = new string[] { "<>", "<=", ">=", "<", ">", "=" };
        string[] third = new string[] { "+", "-", "or" };
        string[] second = new string[] { "*", "/", "and" };
        string[] first = new string[] { "not" };

        string[] operations = fourh.Concat(third).ToArray()
                .Concat(second).ToArray()
                .Concat(first).ToArray();

        //string[] operations = new string[] { " ", "+", "-", "*", "/", "<>", "<=", ">=", "<", ">", "=",
        //        "and", "or", "not"};
        char[] splitters = new char[] { ' ', '+', '-', '*', '/', '<', '>', '=', '(', ')' };

        #region разделяем строку на сущности
        string str_stack = "";
        bool is_string = false, is_ignore_s = false, is_ignore_r = false, is_func = false;
        int opn_bkt_s = 0, opn_bkt_r = 0;
        for (int i = 0; i < expression.Length; i++)
        {
            if (!is_string && expression[i] != '\'')
            {
                #region скобки массива
                if (expression[i] == '[' && opn_bkt_s == 0)
                {
                    is_ignore_s = true;
                    opn_bkt_s = 1;
                    str_stack += expression[i];
                }
                else if (expression[i] == '[' && is_ignore_s)
                {
                    opn_bkt_s++;
                    str_stack += expression[i];
                }
                else if (expression[i] == ']' && opn_bkt_s == 1)
                {
                    is_ignore_s = false;
                    opn_bkt_s = 0;
                    str_stack += expression[i];
                    continue;
                }
                else if (expression[i] == ']' && is_ignore_s)
                {
                    opn_bkt_s--;
                    str_stack += expression[i];
                }
                else if (is_ignore_s)
                {
                    str_stack += expression[i];
                }
                #endregion
                #region скобки функции
                else if (expression[i] == '(' && opn_bkt_r == 0 && str_stack.Length > 0 && char.IsLetter(str_stack[0]))
                {
                    is_func = true;
                    is_ignore_r = true;
                    opn_bkt_r = 1;
                    str_stack += expression[i];
                }
                else if (expression[i] == '(' && is_ignore_r)
                {
                    opn_bkt_r++;
                    str_stack += expression[i];
                }
                else if (expression[i] == ')' && opn_bkt_r == 1)
                {
                    is_ignore_r = false;
                    opn_bkt_r = 0;
                    str_stack += expression[i];
                    continue;
                }
                else if (expression[i] == ')' && is_ignore_r)
                {
                    opn_bkt_r--;
                    str_stack += expression[i];
                }
                else if (is_ignore_r)
                {
                    str_stack += expression[i];
                }
                #endregion
            }

            #region строковое выражение
            if (expression[i] == '\'')
            {
                if (!is_ignore_r && !is_ignore_s)
                {
                    MathStruct ms = new MathStruct();
                    if (is_string)
                    {
                        ms.type = 0;
                        ms.value = str_stack;
                        operands.Enqueue(ms);
                    }
                    else if (str_stack.Trim().Length > 0)
                    {
                        str_stack = str_stack.Trim();
                        ms.value = str_stack;
                        if (operations.Contains(str_stack))
                            ms.type = 2;
                        else
                            ms.type = 1;
                        operands.Enqueue(ms);
                    }
                    str_stack = "";
                }
                else
                    str_stack += expression[i];
                is_string = !is_string;
            }
            else if (is_string)
                str_stack += expression[i];
            #endregion
            else if (!is_ignore_r && !is_ignore_s)
            {
                #region
                str_stack = str_stack.Trim();
                if (str_stack.Length == 0)
                    str_stack += expression[i];
                else if (operations.Contains(str_stack + expression[i]))
                {
                    MathStruct ms = new MathStruct();
                    ms.type = 2;
                    ms.value = str_stack + expression[i];
                    operands.Enqueue(ms);
                    str_stack = "";
                }
                else if (!splitters.Contains(expression[i]) && !splitters.Contains(str_stack[0]))
                    str_stack += expression[i];
                else
                {
                    MathStruct ms = new MathStruct();
                    ms.value = str_stack;
                    if (splitters.Contains(str_stack[0]))
                        ms.type = 2;
                    else if (is_func)
                        ms.type = 3;
                    else
                        ms.type = 1;
                    operands.Enqueue(ms);
                    str_stack = expression[i].ToString();
                    is_func = false;
                }
                #endregion
            }
        }
        if (str_stack.Trim().Length > 0)
        {
            MathStruct ms = new MathStruct();
            ms.value = str_stack.Trim();
            if (str_stack.Length == 1 && splitters.Contains(str_stack[0]))
                ms.type = 2;
            else if (is_func)
                ms.type = 3;
            else
                ms.type = 1;
            operands.Enqueue(ms);
        }
        #endregion

        //foreach (Object obj in operands)
        //    Console.WriteLine("{0} :: {1}", ((Calculation.MathStruct)obj).value, ((Calculation.MathStruct)obj).type);

        #region Записываем ОПН в очередь
        Queue opn = new Queue();
        Stack st = new Stack();
        Stack level_stack = new Stack();
        foreach (Object obj in operands)
        {
            int type = ((MathStruct)obj).type;
            object value = ((MathStruct)obj).value;
            if (type == 2)
            {
                if (value.ToString() == "(")
                {
                    st.Push(value);
                    level_stack.Push(5);
                }
                else if (value.ToString() == ")")
                {

                    while (st.Count > 0 && st.Peek().ToString() != "(")
                    {
                        MathStruct ms = new MathStruct() { type = 2, value = st.Pop() };
                        opn.Enqueue(ms);
                        level_stack.Pop();
                    }
                    if (st.Count > 0 && st.Peek().ToString() == "(")
                    {
                        st.Pop();
                        level_stack.Pop();
                    }
                }
                else
                {
                    int level = 0;

                    if (fourh.Contains(value.ToString()))
                        level = 4;
                    else if (third.Contains(value.ToString()))
                        level = 3;
                    else if (second.Contains(value.ToString()))
                        level = 2;
                    else if (first.Contains(value.ToString()))
                        level = 1;

                    if (level == 0)
                        throw new Exception("Не удалось определить уровень операции " + value.ToString());

                    while (level_stack.Count > 0 && (int)level_stack.Peek() <= level && st.Count > 0)
                    {
                        MathStruct ms = new MathStruct() { type = 2, value = st.Pop() };
                        opn.Enqueue(ms);
                        level_stack.Pop();
                    }

                    st.Push(value);
                    level_stack.Push(level);
                }
            }
            else if (type == 3)
            {
                MathStruct ms = new MathStruct() { type = 1 };
                ms.value = CalcFunc(value.ToString());
                opn.Enqueue(ms);
            }
            else
            {
                opn.Enqueue(obj);
            }
        }

        while (st.Count > 0)
        {
            MathStruct ms = new MathStruct() { type = 2, value = st.Pop() };
            opn.Enqueue(ms);
        }
        #endregion

        return opn;
    }

    /// <summary>
    /// Преобразует выражение в обратную Польскую нотацию
    /// </summary>
    /// <param name="expression">Исходное выражение</param>
    /// <returns>Выражение в формате ОПН</returns>
    private string ToPolsc(string expression)
    {
        expression = expression.Replace(" ", "");
        int open_bkt = expression.Split('(').Length - 1;
        int close_bkt = expression.Split(')').Length - 1;
        if (open_bkt != close_bkt)
            throw new Exception("Количество скобок в выражении \"" + expression + "\" не совпадает");

        #region func
        Regex func_start = new Regex(@"([A-Za-z0-9_]+)\(");
        while (func_start.IsMatch(expression))
        {
            string func_name = "";
            int start_pos, func_start_pos = func_start.Match(expression).Index;
            start_pos = func_start_pos;
            while (expression[start_pos] != '(')
                func_name += expression[start_pos++];

            start_pos++;
            int bkt_count = 1;
            int end_pos;
            for (end_pos = start_pos; end_pos < expression.Length && bkt_count > 0; end_pos++)
                if (expression[end_pos] == '(')
                    bkt_count++;
                else if (expression[end_pos] == ')')
                    bkt_count--;
            string sub_expression = expression.Remove(0, start_pos)
                .Remove(end_pos - start_pos - 1);
            expression = expression.Remove(func_start_pos, end_pos - func_start_pos)
                .Insert(func_start_pos, CalcFunc(func_name, sub_expression).ToString());
        }
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
        if (expression == "")
            return "0";

        string result = "";
        string[] words = new Regex(@"(?:[\s]*(not)[\s]*)").Split(expression);

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
                "or", "and", "*", "/", "div", "mod" };

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
    /// Вычисляет значение вырожения из ОПН
    /// </summary>
    /// <param name="expression">Выражение в формате ОПН</param>
    /// <returns>Результат вычисления, число или логическое значение</returns>
    public object CalcPolsc(Queue expression)
    {
        string[] operations = new string[] { "<>", "<=", "<", ">=", ">", "=", "+", "-",
                "or", "and", "*", "/", "div", "mod" };

        Stack stack = new Stack();

        foreach (object ms in expression)
        {
            MathStruct MS = (MathStruct)ms;
            if (MS.type == 2 && MS.value.ToString() == "not")
            {
                object operand = stack.Pop();
                if (operand.GetType().Name != "Boolean")
                    throw new Exception("Операция NOT не применима к типу " + operand.GetType().Name);
                stack.Push(!(bool)operand);
            }
            else if (MS.type == 2)
            {
                if (MS.value.ToString() == "-" && stack.Count == 1)
                {
                    object temp = stack.Pop();
                    stack.Push(0.0);
                    stack.Push(temp);
                }
                object result = SimpleCalc(stack.Pop(), stack.Pop(), MS.value.ToString());
                stack.Push(result);
            }
            else if (MS.type == 1)
            {

                double d;
                bool b;
                if (double.TryParse(MS.value.ToString().Replace(".", ","), out d))
                {
                    stack.Push(d);
                }
                else if (bool.TryParse(MS.value.ToString(), out b))
                {
                    stack.Push(b);
                }
                else
                {
                    Variable v = environment.GetElementByName(MS.value.ToString());

                    if (v.GetType().Name != "NullVariable")
                        MS.value = v.value;
                    if (double.TryParse(MS.value.ToString().Replace(".", ","), out d))
                    {
                        stack.Push(d);
                    }
                    else if (bool.TryParse(MS.value.ToString(), out b))
                    {
                        stack.Push(b);
                    }
                    else
                        throw new Exception("Неизвестная переменная " + MS.value.ToString());
                }
            }
            else
            {
                stack.Push(MS.value.ToString());
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
        else if (x.GetType().Name == "String" || y.GetType().Name == "String")
            return StringCalc(y.ToString(), x.ToString(), operation);
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
    /// Сравнивает строки между собой
    /// </summary>
    /// <param name="x">Первая строка</param>
    /// <param name="y">Вторая строка</param>
    /// <param name="operation">Знак операции</param>
    /// <returns>Результат вычисления, логическое значение</returns>
    private object StringCalc(string x, string y, string operation)
    {
        switch (operation)
        {
            case "+":
                return x + y;
            case "<>":
                return x != y;
            case "=":
                return x == y;
            case "<=":
                return String.CompareOrdinal(x, y) <= 0;
            case ">=":
                return String.CompareOrdinal(x, y) >= 0;
            case "<":
                return String.CompareOrdinal(x, y) < 0;
            case ">":
                return String.CompareOrdinal(x, y) > 0;
            default:
                throw new Exception(string.Format("Операция {0} не применима к типу String", operation));
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

        if (v.GetType().Name != "NullVariable")
            return v.value.ToString();

        return operand;
    }

    /// <summary>
    /// Вычисляет значение функции с параметрами
    /// </summary>
    /// <param name="func">Имя функции и параметры в формате Имя_Функции(Параметр[, Параметр])</param>
    /// <returns>Значение функции</returns>
    private object CalcFunc(string func)
    {
        func = func.Trim();
        string func_name = func.Remove(func.IndexOf("("));
        string _params = func.Remove(0, func.IndexOf("(")+1).Remove(func.Length - func.IndexOf("(") - 2);
        int opn_s_bkt = 0, opn_r_bkt = 0;
        bool is_string = false;
        for (int i = 0; i < _params.Length; i++)
        {
            if (_params[i] == '\'')
                is_string = !is_string;
            else if (_params[i] == '(')
                opn_r_bkt++;
            else if (_params[i] == ')')
                opn_r_bkt--;
            else if (_params[i] == '[')
                opn_s_bkt++;
            else if (_params[i] == ']')
                opn_s_bkt--;
            else if (_params[i] == ',' && opn_r_bkt == 0 && opn_s_bkt == 0 && !is_string)
                _params = _params.Insert(i++, "☺").Remove(i, 1);
        }
        string[] param_arr = _params.Split('☺');

        if (param_arr.Length == 1)
        {
            object param = Calculate(param_arr[0]);

            if (param.GetType().Name != "Double")
                throw new Exception("Функция " + func_name + " не принимает параметр типа " + param.GetType().Name);

            switch (func_name)
            {
                case "sign":
                    return Math.Sign((double)param).ToString();
                case "abs":
                    return Math.Abs((double)param).ToString();
                case "sin":
                    return Math.Sin((double)param).ToString();
                case "sinh":
                    return Math.Sinh((double)param).ToString();
                case "cos":
                    return Math.Cos((double)param).ToString();
                case "cosh":
                    return Math.Cosh((double)param).ToString();
                case "tan":
                    return Math.Tan((double)param).ToString();
                case "tanh":
                    return Math.Tanh((double)param).ToString();
                case "arcsin":
                    return Math.Acos((double)param).ToString();
                case "arccos":
                    return Math.Asin((double)param).ToString();
                case "arctan":
                    return Math.Atan((double)param).ToString();
                case "exp":
                    return Math.Exp((double)param).ToString();
                case "ln":
                    return Math.Log((double)param, Math.E).ToString();
                case "log2":
                    return Math.Log((double)param, 2).ToString();
                case "log10":
                    return Math.Log10((double)param).ToString();
                case "sqrt":
                    return Math.Sqrt((double)param).ToString();
                case "sqr":
                    return ((double)param * (double)param).ToString();
                case "round":
                    return Math.Round((double)param).ToString();
                case "trunc":
                case "int":
                    return Math.Truncate((double)param).ToString();
                case "frac":
                    return ((double)param - Math.Truncate((double)param)).ToString();
                case "floor":
                    return Math.Floor((double)param).ToString();
                case "ceil":
                    return Math.Ceiling((double)param).ToString();
                case "radtodeg":
                    return ((double)param / Math.PI * 180).ToString();
                case "degtorad":
                    return ((double)param / 180 * Math.PI).ToString();
                case "random":
                    return new Random().Next((int)(double)param).ToString();
                case "logn":
                case "power":
                case "max":
                case "min":
                    throw new Exception("Функци " + func_name + " не принимает 1 параметр");
                default:
                    throw new Exception("Неизвестная функция " + func_name);
            }
        }
        else if (param_arr.Length == 2)
        {
            object first_param = Calculate(param_arr[0]);
            object second_param = Calculate(param_arr[1]);

            if (first_param.GetType().Name != "Double")
                throw new Exception("Функция " + func_name + " не принимает параметр типа " + first_param.GetType().Name);
            if (second_param.GetType().Name != "Double")
                throw new Exception("Функция " + func_name + " не принимает параметр типа " + second_param.GetType().Name);

            switch (func_name)
            {
                case "logn":
                    return Math.Log((double)first_param, (double)second_param);
                case "power":
                    return Math.Pow((double)first_param, (double)second_param);
                case "random":
                    return new Random().Next((int)(double)first_param, (int)(double)second_param);
                case "max":
                    return Math.Max((double)first_param, (double)second_param);
                case "min":
                    return Math.Min((double)first_param, (double)second_param);
                case "sign":
                case "abs":
                case "sin":
                case "sinh":
                case "cos":
                case "cosh":
                case "tan":
                case "tanh":
                case "arcsin":
                case "arccos":
                case "arctan":
                case "exp":
                case "ln":
                case "log2":
                case "log10":
                case "sqrt":
                case "sqr":
                case "round":
                case "trunc":
                case "int":
                case "frac":
                case "floor":
                case "ceil":
                case "radtodeg":
                case "degtorad":
                    throw new Exception("Функция " + func_name + " не принимает 2 параметра");
                default:
                    throw new Exception("Неизвестная функция " + func_name);
            }
        }

        return 0;
    }

    private string CalcFunc(string func_name, string _param)
    {
        object param = Calculate(_param);
        if (param.GetType().Name != "Double")
            throw new Exception("Функция " + func_name + " не принимает параметр типа " + param.GetType().Name);

        switch (func_name)
        {
            case "sign":
                return Math.Sign((double)param).ToString();
            case "abs":
                return Math.Abs((double)param).ToString();
            case "sin":
                return Math.Sin((double)param).ToString();
            case "sinh":
                return Math.Sinh((double)param).ToString();
            case "cos":
                return Math.Cos((double)param).ToString();
            case "cosh":
                return Math.Cosh((double)param).ToString();
            case "tan":
                return Math.Tan((double)param).ToString();
            case "tanh":
                return Math.Tanh((double)param).ToString();
            case "arcsin":
                return Math.Acos((double)param).ToString();
            case "arccos":
                return Math.Asin((double)param).ToString();
            case "arctan":
                return Math.Atan((double)param).ToString();
            case "exp":
                return Math.Exp((double)param).ToString();
            case "ln":
                return Math.Log((double)param, Math.E).ToString();
            case "log2":
                return Math.Log((double)param, 2).ToString();
            case "log10":
                return Math.Log10((double)param).ToString();
            case "sqrt":
                return Math.Sqrt((double)param).ToString();
            case "sqr":
                return ((double)param * (double)param).ToString();
            case "round":
                return Math.Round((double)param).ToString();
            case "trunc":
            case "int":
                return Math.Truncate((double)param).ToString();
            case "frac":
                return ((double)param - Math.Truncate((double)param)).ToString();
            case "floor":
                return Math.Floor((double)param).ToString();
            case "ceil":
                return Math.Ceiling((double)param).ToString();
            case "radtodeg":
                return ((double)param / Math.PI * 180).ToString();
            case "degtorad":
                return ((double)param / 180 * Math.PI).ToString();
            case "random":
                return new Random().Next((int)(double)param).ToString();
            default:
                throw new Exception("Неизвестная функция " + func_name);
        }
    }
}