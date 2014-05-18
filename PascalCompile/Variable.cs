using System;
using System.Collections.Generic;

class Variable : ICloneable
{
    public string name { get; set; }
    public object value { get; set; }
    public string type { get; set; }
    public bool pointer { get; set; }

    public Variable() { }

    public Variable(string _name)
    {
        name = _name;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }

    public Variable _Clone()
    {
        return (Variable)Clone();
    }

    public void Dump()
    {
        Console.WriteLine("{0}({1},{4})[{2}] = {3};",
            name,
            GetType().Name,
            pointer,
            value,
            type);
    }
}

class NullVariable : Variable
{
    public NullVariable()
    {
        name = "undefined";
        type = "undefined";
        value = "undefined";
    }
}

class Constant : Variable
{
    public object Value
    {
        get
        {
            return value;
        }
    }

    public Constant(string _name, object _value)
    {
        name = _name;
        value = _value;
        pointer = false;
    }
}

class Boolean : Variable
{
    public bool Value
    {
        get { return (bool)value; }
        set { this.value = (bool)value; }
    }

    public Boolean() {
        type = "boolean";
        value = false;
        pointer = false;
    }

    public Boolean(string _name)
    {
        name = _name;
        type = "boolean";
        value = false;
        pointer = false;
    }
}

class Real : Variable
{
    public double Value
    {
        get { return (double)value; }
        set { this.value = (double)value; }
    }

    public Real() {
        type = "real";
        value = 0;
        pointer = false;
    }

    public Real(string _name)
    {
        name = _name;
        type = "real";
        value = 0;
        pointer = false;
    }

    public void SetValue(string expression)
    {
        if (pointer)
            throw new Exception("Нельзя присвоить ссылке числовое значение");
        else
            value = (double)Calculate(expression);
    }

    public static double Calculate(string expression)
    {
        int open_bkt = expression.Split('(').Length - 1;
        int close_bkt = expression.Split(')').Length - 1;
        if (open_bkt != close_bkt)
            throw new Exception("Количество скобок в выражении \"" + expression + "\" не совпадает");

        expression = expression.Replace(".", ",");
        expression = expression.Replace("-", "+-");
        expression = expression.Replace(" ", "");

        while (expression.IndexOf("--") > -1)
            expression = expression.Replace("--", "+");
        while (expression.IndexOf("++") > -1)
            expression = expression.Replace("++", "+");

        if (expression.IndexOf("*/") > -1 || expression.IndexOf("/*") > -1 ||
            expression.IndexOf("**") > -1 || expression.IndexOf("//") > -1 ||
            expression.IndexOf("+*") > -1 || expression.IndexOf("*+") > -1 ||
            expression.IndexOf("+/") > -1 || expression.IndexOf("/+") > -1)
            throw new Exception("Встретилась неизвестная команда");

        while (expression.IndexOf("(") > -1)
        {
            int start_pos = expression.IndexOf("(") +1;
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

        string[] sum = expression.Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);

        double sum_result = 0;
        for (int j = 0; j < sum.Length; j++)
        {
            string operation = "";
            for (int i = 0; i < sum[j].Length; i++)
                if (sum[j][i] == '*' || sum[j][i] == '/')
                    operation += sum[j][i];
            string[] operands = sum[j].Split(new char[] { '*', '/' });
            if (operands.Length == 0)
                continue;

            double result;
            double.TryParse(operands[0], out result);
            for (int i = 1; i < operands.Length; i++)
            {
                double oper = double.Parse(operands[i]);
                switch (operation[i - 1])
                {
                    case '*':
                        result *= oper;
                        break;
                    case '/':
                        result /= oper;
                        break;
                    default:
                        throw new Exception("Неизвестная операция: " + operation[i - 1]);
                }
            }
            sum_result += result;
        }

        return sum_result;
    }
}

class Integer : Real
{
    new public int Value
    {
        get { return (int)value; }
        set { this.value = (int)value; }
    }

    public Integer()
    {
        type = "integer";
        value = 0;
        pointer = false;
    }

    public Integer(string _name)
    {
        name = _name;
        type = "integer";
        value = 0;
        pointer = false;
    }

    new public void SetValue(string expression)
    {
        value = (int)Calculate(expression);
    }
}

class Record : Variable
{
    private Environs.EnvironsStuct[] Value;

    /// <summary>
    /// Создаёт пустую переменную типа Record
    /// </summary>
    public Record()
    {
        type = "record";
        pointer = false;
    }

    /// <summary>
    /// Создаёт переменную типа Record
    /// </summary>
    /// <param name="_name">Имя переменной</param>
    public Record(string _name)
    {
        name = _name;
        type = "record";
        pointer = false;
    }

    /// <summary>
    /// Создаёт переменную типа Record
    /// </summary>
    /// <param name="_name">Имя переменной</param>
    /// <param name="_pointer">Указатель на ссылку</param>
    /// <param name="length">Количество полей в переменной</param>
    public Record(string _name, int length) 
    {
        name = _name;
        type = "record";
        pointer = false;
        SetLength(length);
    }

    /// <summary>
    /// Устанавливает число полей 
    /// </summary>
    /// <param name="length">Количество полей в переменной</param>
    /// <returns>True в случае успешного присвоения, False в случае возникновения ошибки</returns>
    public bool SetLength(int length)
    {
        if (Value == null)
        {
            Value = new Environs.EnvironsStuct[length];
            value = Value;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Инициализирует поля
    /// </summary>
    /// <param name="names">Массив имён полей</param>
    public void SetFields(string[] names)
    {
        if (Value == null)
            SetLength(names.Length);

        for (int i = 0; i < Value.Length && i < names.Length; i++)
        {
            Value[i].name = names[i];
            Value[i].value = new Variable();
        }
        value = Value;
    }

    /// <summary>
    /// Инициализирует поле значением переменной
    /// </summary>
    /// <param name="var">Переменная, которая будет присвоена полю</param>
    /// <param name="name">Имя переменной</param>
    /// <returns>True в случае успешного присвоения, False в случае возникновения ошибки</returns>
    public bool SetVariable(Variable var, string name)
    {
        if (Value == null)
            return false;
        for (int i = 0; i < Value.Length; i++)
        {
            if (Value[i].name == name)
            {
                Value[i].value = var;
                value = Value;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Записывает значение передаваемой переменной Record в текущий экземпляр класса
    /// </summary>
    /// <param name="var">Переменная типа Record, значение которой должно быть скопировано</param>
    /// <returns>true в случае успешного комирования, false в случае возникновния ошибки</returns>
    public bool SetValue(Record var)
    {
        if (Value.Length != var.Value.Length)
            return false;
        bool is_equals = true;
        for (int i = 0; i < Value.Length; i++)
            is_equals = is_equals &&
                Value[i].name == var.Value[i].name &&
                Value[i].value.GetType().Name == var.Value[i].value.GetType().Name;
        if (!is_equals)
            return false;
        for (int i = 0; i < Value.Length; i++)
        {
            Value[i].value = var.Value[i].value._Clone();
        }
        value = Value;
        return true;
    }

    /// <summary>
    /// Возвращает переменную запрашиваемого поля
    /// </summary>
    /// <param name="name">Имя поля, значение которого нужно вернуть</param>
    /// <returns>Переменная типа Variable</returns>
    public Variable GetVariable(string name)
    {
        if (Value == null)
            return null;

        for (int i = 0; i < Value.Length; i++)
            if (Value[i].name == name)
                return Value[i].value;
        return null;
    }

    /// <summary>
    /// Получает количество полей в типе
    /// </summary>
    /// <returns>Целое число, количество потомков</returns>
    public int GetFieldCount()
    {
        if (Value == null)
            return 0;
        return Value.Length;
    }

    /// <summary>
    /// Возвращает структуру полей
    /// </summary>
    /// <returns>Массив полей типа Rec</returns>
    public Environs.EnvironsStuct[] GetFieldsArray()
    {
        return Value;
    }
}

class Massiv : Record
{
    private Environs.EnvironsStuct[] Value;

    /// <summary>
    /// Создаёт пустую переменную типа Array
    /// </summary>
    public Massiv()
    {
        type = "array";
        pointer = false;
    }

    /// <summary>
    /// Создаёт переменную типа Array
    /// </summary>
    /// <param name="_name">Имя переменной</param>
    public Massiv(string _name)
    {
        name = _name;
        type = "array";
        pointer = false;
    }
}

class Pointer : Variable
{
    public Variable Value
    {
        get
        {
            return (Variable)value;
        }
        set
        {
            this.value = value;
        }
    }

    public Pointer() 
    {
        pointer = true;
        type = "pointer";
        value = new NullVariable();
    }

    public Pointer(string _name)
    {
        name = _name;
        pointer = true;
        type = "pointer";
        value = new NullVariable();
    }

    public Pointer(string _name, string _pointer_type)
    {
        name = _name;
        pointer = true;
        type = _pointer_type;
        value = new NullVariable();
    }
}