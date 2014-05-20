using System;
using System.Collections.Generic;

public class Environs
{
    /// <summary>
    /// Структура переменной, хранит переменную и её реальное имя
    /// </summary>
    public struct EnvironsStuct
    {
        /// <summary>
        /// Хранимая переменная
        /// </summary>
        public Variable value;

        /// <summary>
        /// Имя хранимой переменной
        /// </summary>
        public string name;
    };

    /// <summary>
    /// Список окружения, хранятся переменные типа EnvironsStruct
    /// </summary>
    public List<EnvironsStuct> enviroment;
    private Calculation calc;

    public Environs()
    {
        enviroment = new List<EnvironsStuct>();
        calc = new Calculation(this);
    }

    ///// <summary>
    ///// Добавляет переменную в окружение
    ///// </summary>
    ///// <param name="var">Переменная</param>
    //public void Add(Variable var)
    //{
    //    Add(var, var.name);
    //}

    /// <summary>
    /// Добавляет переменную в окружение
    /// </summary>
    /// <param name="var">Переменная</param>
    /// <param name="name">Имя переменной</param>
    public void Add(Variable var, string name)
    {
        EnvironsStuct es = new EnvironsStuct();
        es.name = name;
        es.value = var;
        enviroment.Add(es);
    }

    /// <summary>
    /// Возвращает переменную по её имени
    /// </summary>
    /// <param name="name">Имя переменной</param>
    /// <returns>Переменная</returns>
    public Variable GetElementByName(string name)
    {
        string[] names = name.Split('.');

        if (names.Length == 0)
            return new NullVariable();

        Variable var = new NullVariable();
        for (int i = 0; i < names.Length; i++)
        {
            bool pointer = names[i].EndsWith("^");
            if (pointer) names[i] = names[i].Remove(names[i].Length - 1);

            if (i == 0)
            {
                foreach (EnvironsStuct item in enviroment)
                {
                    if (item.name == names[i])
                        var = item.value;
                }
            }
            else
            {
                if (var.GetType().Name == "NullVariable")
                    break;
                else if (var.GetType().Name == "Record")
                    var = ((Record)var).GetVariable(names[i]);
            }

            if (pointer)
                var = ((Pointer)var).Value;
        }

        return var;
    }

    ///// <summary>
    ///// Присваивает переменной с именем name значение expression
    ///// </summary>
    ///// <param name="name">Имя переменной</param>
    ///// <param name="expression">Выражение</param>
    //public void Assignment(string name, string expression)
    //{
    //    Variable var = GetElementByName(name);
    //    if (var == null)
    //        throw new Exception("Используется необъявленная переменная: " + name);

    //    if (var.pointer)
    //    {
    //        Variable try_ = GetElementByName(expression);
    //        if (try_ != null)
    //            if (try_.pointer)
    //            {
    //                AssignmentLink(name, try_);
    //                return;
    //            }
    //        throw new Exception("Нельзя присвоить указателю значение выражения");
    //    }

    //    switch (var.GetType().Name)
    //    {
    //        case "Integer":
    //            expression = ExpressionToNumberLine(expression, "Integer");
    //            ((Integer)var).SetValue(expression);
    //            break;
    //        case "Real":
    //            expression = ExpressionToNumberLine(expression, "Real");
    //            ((Real)var).SetValue(expression);
    //            break;
    //        case "Record":
    //            Variable right_operand = GetElementByName(expression);
    //            if (right_operand == null)
    //                throw new Exception("Нельзя присвоить типу Запись значение выражения");
    //            ((Record)var).SetValue((Record)right_operand);
    //            break;
    //        default:
    //            throw new Exception("Неизвестный тип переменной " + name);
    //    }
    //}

    /// <summary>
    /// Присваивает переменной значение операнда
    /// </summary>
    /// <param name="var_name">Переменная</param>
    /// <param name="operand_name">Операнд</param>
    public void AssignmentVar(string var_name, string operand_name)
    {
        Variable var = GetElementByName(var_name);
        Variable operand = GetElementByName(operand_name);

        if (var.GetType().Name != operand.GetType().Name)
            throw new Exception("Нельзя присвоить переменной с типом " + var.GetType().Name 
                + " значение переменной с типом " + operand.GetType().Name);
        if (var.pointer)
            if (operand.pointer)
                AssignmentLink(var_name, operand);
            else
                throw new Exception("Нельзя присвоить значение переменной указателю");
        else
            if (var.GetType().Name == "Record")
                ((Record)var).SetValue((Record)operand);
            else
                var.value = operand.value;
    }

    /// <summary>
    /// Присваивает переменной значение выражения
    /// </summary>
    /// <param name="var_name">Имя переменной</param>
    /// <param name="expression">Выражение</param>
    public void AssignmentExpr(string var_name, string expression)
    {
        object result = Calculate(expression);
        Variable var = GetElementByName(var_name);

        if (result.GetType().Name == "Boolean" && var.GetType().Name == "Boolean")
            var.value = result;
        else if (result.GetType().Name == "Double")
        {
            if (var.GetType().Name == "Integer")
            {
                if ((int)(double)result == (double)result)
                    var.value = (int)(double)result;
                else
                    throw new Exception("Нельзя присвоить целочисленной переменной вещественное значение");
            }
            else if (var.GetType().Name == "Real")
                var.value = result;
            else
                throw new Exception("Нельзя присвоить переменной типа " + var.type + " значение типа " + result.GetType().Name);
        }
        else
            throw new Exception("Нельзя присвоить переменной типа " + var.type + " значение типа " + result.GetType().Name);
    }

    /// <summary>
    /// Присвоить переменной с имененим pointer_name адрес переменной variable_name
    /// </summary>
    /// <param name="pointer_name">Имя ссылки на переменну.</param>
    /// <param name="variable_name">Имя переменной, адрес которой нужно присвоить</param>
    public void AssignmentAddr(string pointer_name, string variable_name)
    {
        Variable pointer = GetElementByName(pointer_name);
        if (pointer == null)
            throw new Exception("Используется необъявленная переменная: " + pointer_name);
        Variable variable = GetElementByName(variable_name);
        if (variable == null)
            throw new Exception("Используется необъявленная переменная: " + variable_name);
        if (!pointer.pointer)
            throw new Exception("Операция new не применима к статическим типам");
        if (pointer.type != "pointer" && pointer.type != variable.type)
            throw new Exception("Тип указателя и переменной не совпадают");
        ((Pointer)pointer).Value = variable;
    }

    /// <summary>
    /// Связывает переменную указатель с новым адресом, на которую она должна ссылаться
    /// </summary>
    /// <param name="var_name">Имя указателя</param>
    /// <param name="operand">Новы адрес</param>
    public void AssignmentLink(string var_name, Variable operand)
    {
        string[] names = var_name.Split('.');

        if (names.Length == 0)
            return;
        else if (names.Length == 1)
        {
            if (names[0].EndsWith("^"))
                throw new Exception("Нельзя к разыменованной ссылке присвоить значение адреса");
            else
                for (int i = 0; i < enviroment.Count; i++)
                    if (enviroment[i].name == names[0])
                    {
                        enviroment.RemoveAt(i);
                        Add(operand, var_name);
                    }
        }
        else
        {
            Variable var = new NullVariable();
            for (int i = 0; i < names.Length - 1; i++)
            {
                bool pointer = names[i].EndsWith("^");
                if (pointer) names[i] = names[i].Remove(names[i].Length - 1);

                if (i == 0)
                {
                    foreach (EnvironsStuct item in enviroment)
                        if (item.name == names[i])
                            var = item.value;
                }
                else
                {
                    if (var.GetType().Name == "NullVariable")
                        break;
                    else if (var.GetType().Name == "Record")
                        var = ((Record)var).GetVariable(names[i]);
                }

                if (pointer)
                    var = ((Pointer)var).Value;
            }

            if (names[names.Length - 1].EndsWith("^"))
                throw new Exception("Нельзя к разыменованной ссылке присвоить значение адреса");
            else
                if (var.GetType().Name == "Record")
                    ((Record)var).SetVariable(operand, names[names.Length - 1]);
        }
    }

    ///// <summary>
    ///// Преобразует выражение к числовой строке
    ///// </summary>
    ///// <param name="expression">Исходное выражение</param>
    ///// <param name="class_name">Формат строки (Integer - целочисленное, Real - Вещественное)</param>
    ///// <returns>Числовая строка</returns>
    //private string ExpressionToNumberLine(string expression, string class_name)
    //{
    //    string[] operands = expression.Split(new char[] { '+', '-', '*', '/', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
    //    for (int i = 0; i < operands.Length; i++)
    //        for (int j = 0; j < operands.Length - i; j++)
    //            if (String.Compare(operands[i], operands[j]) > 0)
    //            {
    //                string tmp = operands[i];
    //                operands[i] = operands[j];
    //                operands[j] = tmp;
    //            }
    //    foreach (string operand in operands)
    //    {
    //        Variable var = GetElementByName(operand);
    //        bool digits = true;
    //        foreach (char c in operand)
    //            digits = digits && (char.IsDigit(c) || c == ',' || c == '.' || c == '-');

    //        string value = "";
    //        if (digits)
    //            continue;
    //        if (class_name == "Integer")
    //            value = GetIntegerValue(operand);
    //        else if (class_name == "Real")
    //            value = GetRealValue(operand);
    //        expression = expression.Replace(operand, value);
    //    }
    //    return expression;
    //}

    ///// <summary>
    ///// Возвращает целочисленное значение переменной в формате строки
    ///// </summary>
    ///// <param name="name">Имя переменной</param>
    ///// <returns>Целочисленное значение в формате строки</returns>
    //private string GetIntegerValue(string name)
    //{
    //    Variable var = GetElementByName(name);
    //    if (var == null || var.GetType().Name == "NullVariable")
    //        throw new Exception("Используется необъявленная переменная: '" + name + "'");
    //    return ((Integer)var).value.ToString();
    //}

    ///// <summary>
    ///// Возвращает вещественное значение переменной в формате строки
    ///// </summary>
    ///// <param name="name">Имя переменной</param>
    ///// <returns>Вещественное значение в формате строки</returns>
    //private string GetRealValue(string name)
    //{
    //    Variable var = GetElementByName(name);
    //    if (var == null || var.GetType().Name == "NullVariable")
    //        throw new Exception("Используется необъявленная переменная: '" + name + "'");
    //    return ((Real)var).value.ToString();
    //}

    /// <summary>
    /// Производит математические вычисления над переданной строкой
    /// </summary>
    /// <param name="expr">Выражение</param>
    /// <returns>double или boolean</returns>
    public object Calculate(string expr)
    {
        return calc.Calculate(expr);
    }

    /// <summary>
    /// Пытается посчитать выражение и записывает результат в переменную output. Возвращает true при успешном подсчёте и false в случае ошибки
    /// </summary>
    /// <param name="expr">Выражение</param>
    /// <param name="output">Переменная для вывода результата</param>
    /// <returns>boolean</returns>
    public bool TryCalculate(string expr, out object output)
    {
        output = null;
        try
        {
            output = Calculate(expr);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dump()
    {
        foreach (EnvironsStuct item in enviroment)
            Console.WriteLine("{0}({1}) = {2};",
                item.name,
                item.value.GetType().Name,
                item.value.value);
    }
}
