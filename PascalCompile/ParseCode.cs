using System;
using System.Linq;
using System.Text.RegularExpressions;

public class ParseCode
{
    /// <summary>
    /// Структура переменных и типов
    /// </summary>
    private struct TypeStruct
    {
        public string name;
        public string type;
    };

    /// <summary>
    /// Структура программы
    /// </summary>
    private struct ProgramStruct
    {
        public Tree program_tree;
        public TypeStruct[] consts;
        public TypeStruct[] types;
        public TypeStruct[] vars;
    }

    ProgramStruct program;
    Tree cursor;
    private Environs env;

    public ParseCode() { }

    public ParseCode(string _code)
    {
        SetCode(_code);
    }

    /// <summary>
    /// Устанавливает новый код и автоматически распознаёт его содержимое
    /// </summary>
    /// <param name="_code"></param>
    public void SetCode(string _code)
    {
        env = new Environs();
        program = new ProgramStruct();
        program.program_tree = GetProgramTree(_code.ToLower());
        program.consts = GetTypesStruct(program.program_tree, "const");
        program.types = GetTypesStruct(program.program_tree, "type");
        program.vars = GetTypesStruct(program.program_tree, "var");

        CheckConflictNames();

        program.program_tree.Dump();

        cursor = GetElementCursor(program.program_tree, "begin");
        if (cursor.HasChild())
            cursor = cursor.GetFirstChild();
        else
            cursor = null;

        InitVariable();
    }

    /// <summary>
    /// Инициализация переменных
    /// </summary>
    private void InitVariable()
    {
        foreach (TypeStruct variable in program.vars)
        {
            string[] variables = variable.name.Split(',');
            foreach (string var_name in variables)
                InitVariable(var_name, variable.type);
        }
    }

    /// <summary>
    /// Инициализирует динамическую переменную
    /// </summary>
    /// <param name="type">Тип динамической переменной</param>
    /// <returns>Созданная переменная</returns>
    private Variable InitVariable(string type)
    {
        return InitVariable(null, type);
    }

    /// <summary>
    /// Инициализирует переменную
    /// </summary>
    /// <param name="name">Имя переменной</param>
    /// <param name="type">Тип переменной</param>
    /// <param name="add_to_enviroment">Флаг, указывающий на то, стоит ли добавлять переменную в окружение</param>
    /// <returns>Созданную переменную</returns>
    private Variable InitVariable(string name, string type)
    {
        if (name == null)
        {
            name = "DYNAMIC";
        }

        type = GetRealType(FindType(type).Trim());
        bool pointer = (type.StartsWith("^")) ? true : false;
        if (pointer)
        {
            type = type.Remove(0, 1);
            Pointer p = new Pointer(name, type);
            if (name != "")
                env.Add(p);
            return p;
        }
        else
        {
            if (type.StartsWith("record") && type.EndsWith("end"))
            {
                string var_type = type.Remove(0, 6)
                    .Remove(type.Length - 9);
                var_type = var_type.Trim();
                TypeStruct[] tmp = GetTypeStructFromField(var_type);
                string[] names = new string[tmp.Length];
                Record r = new Record(name);
                r.type = type;
                for (int i = 0; i < names.Length; i++)
                    names[i] = tmp[i].name;
                r.SetFields(names);
                for (int i = 0; i < names.Length; i++)
                {
                    Variable v = InitVariable("", tmp[i].type);
                    r.SetVariable(v, names[i]);
                }
                if (name != "")
                    env.Add(r);
                return r;
            }
            else if (type == "real")
            {
                Real r = new Real(name);
                if (name != "")
                    env.Add(r);
                return r;
            }
            else if (type == "integer")
            {
                Integer i = new Integer(name);
                if (name != "")
                    env.Add(i);
                return i;
            }
            else
            {
                Variable v = new Variable(name);
                if (name != "")
                    env.Add(v);
                return v;
            }
        }
    }

    /// <summary>
    /// Создаёт дерево программы
    /// </summary>
    /// <param name="code">Исходный код</param>
    /// <returns>Дерево программы</returns>
    static Tree GetProgramTree(string code)
    {
        string[] replace = new string[] { " ", "\r", "\n", "\t" };
        for (int i = 0; i < replace.Length; i++)
            code = code.Replace(replace[i], "☺" + replace[i]);
        code = code.Replace(";", ";☺");
        string[] words = code.Split('☺');
        string str_stack = "";

        Tree root_program = new Tree();
        Tree cursor = root_program;
        cursor.is_root = true;

        bool is_wait = true;
        bool is_func = false;

        int words_count = 0, line_count = 1;

        foreach (string word in words)
        {
            if (!string.IsNullOrWhiteSpace(str_stack) && !is_wait)
                throw new Exception("Ошибка, ожидалась ; после " + str_stack);
            is_wait = false;

            words_count += word.Length;

            str_stack = str_stack.Trim();
            str_stack += word;

            Regex new_line = new Regex(@"(\n){1}", RegexOptions.Multiline);
            Match nl_match = new_line.Match(str_stack);
            if (nl_match.Success)
                line_count++;

            while (!cursor.wait && (cursor.type == "if" || cursor.type == "for"))
                cursor = cursor.parent;

            #region Regex
            //Regex regex_program = new Regex(@"\bprogram\b", RegexOptions.Multiline);
            //Regex regex_const = new Regex(@"\bconst\b", RegexOptions.Multiline);
            //Regex regex_type = new Regex(@"\btype\b", RegexOptions.Multiline);
            //Regex regex_var = new Regex(@"\bvar\b", RegexOptions.Multiline);
            //Regex regex_function = new Regex(@"\bfunction\b(.*)\(([^\)]*)\)(.*):(.*)([^;]*);", RegexOptions.Multiline);
            //Regex regex_procedure = new Regex(@"\bprocedure\b(.*)\(([^\)]*)\)([^;]*);", RegexOptions.Multiline);
            //Regex regex_record = new Regex(@"(.*)\brecord\b", RegexOptions.Multiline);

            //Regex regex_comment_start = new Regex(@"//([^\n]*)", RegexOptions.Multiline);
            //Regex regex_comment = new Regex(@"//([^\n]*)\n{1,}", RegexOptions.Multiline);

            //Regex regex_comments_start = new Regex(@"{+([^}]*)", RegexOptions.Multiline);
            //Regex regex_comments = new Regex(@"{[^}]*}", RegexOptions.Multiline);

            //Regex regex_begin = new Regex(@"\bbegin\b", RegexOptions.Multiline);
            //Regex regex_end = new Regex(@"\bend\b([;]+)", RegexOptions.Multiline);
            //Regex regex_end_program = new Regex(@"\bend\b([.]+)", RegexOptions.Multiline);
            //Regex regex_expr = new Regex(@"([^;]*);", RegexOptions.Multiline);

            //Regex regex_end_else = new Regex(@"\bend\b(.*)\belse\b", RegexOptions.Multiline);
            //Regex regex_for = new Regex(@"\bfor\b(.*)(to|downto)+(.*)\bdo\b", RegexOptions.Multiline);
            //Regex regex_if = new Regex(@"\bif\b(.*)\bthen\b", RegexOptions.Multiline);
            //Regex regex_else = new Regex(@"((.*?)(?=else))", RegexOptions.Multiline);
            #endregion

            Match match;

            #region Вырезаем комментарии
            //match = regex_comments.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Comments)).Success)
            {
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                continue;
            }
            //match = regex_comments_start.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.CommentsStart)).Success)
            {
                is_wait = true;
                continue;
            }
            //match = regex_comment.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Comment)).Success)
            {
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                continue;
            }
            //match = regex_comment_start.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.CommentStart)).Success)
            {
                is_wait = true;
                continue;
            }
            #endregion

            #region Корень программы, блоки функций и операторов не обрабатываются
            #region PROGRAM
            //match = regex_program.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.CommentStart)).Success)
            {
                while (!cursor.is_root)
                    cursor = cursor.parent;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "program";
                cursor.wait = true;
                continue;
            }
            #endregion

            #region CONST
            //match = regex_const.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Const)).Success)
            {
                while (!cursor.is_root)
                    cursor = cursor.parent;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "const";
                continue;
            }
            #endregion

            #region TYPE
            //match = regex_type.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Type)).Success)
            {
                while (!cursor.is_root)
                    cursor = cursor.parent;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "type";
                continue;
            }
            #endregion

            #region VAR
            //match = regex_var.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Var)).Success)
            {
                while (!cursor.is_root)
                    cursor = cursor.parent;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "var";
                continue;
            }
            #endregion

            #region FUNCTION
            //match = regex_function.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Function)).Success)
            {
                while (!cursor.is_root)
                    cursor = cursor.parent;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "function";
                cursor.wait = true;
                cursor.is_root = true;
                continue;
            }
            #endregion

            #region PROCEDURE
            if ((match = Regexs.Match(str_stack, Regs.Procedure)).Success)
            {
                while (!cursor.is_root)
                    cursor = cursor.parent;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "procedure";
                cursor.wait = true;
                cursor.is_root = true;
                continue;
            }
            #endregion

            #region RECORD
            if ((match = Regexs.Match(str_stack, Regs.Record)).Success)
            {
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "record";
                continue;
            }
            #endregion
            #endregion

            #region Код программы, обработка функций и операторов
            #region IF
            //match = regex_if.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.If)).Success)
            {
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "if";
                cursor.ignore_sibling = true;
                cursor.ignore_exit = true;
                continue;
            }
            #endregion

            #region END + ELSE
            //match = regex_end_else.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.EndElse)).Success)
            {
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor = cursor.parent;
                cursor.wait = true;
                continue;
            }
            #endregion

            #region ELSE
            //match = regex_else.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Else)).Success)
            {
                if (cursor.type != "if")
                    throw new Exception("Потерян блок if для конструкции else");
                if (!string.IsNullOrWhiteSpace(match.Value))
                    cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor.wait = true;
                Regex r_temp = new Regex(@"(.*)\belse\b", RegexOptions.Multiline);
                match = r_temp.Match(str_stack);
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                continue;
            }
            #endregion

            #region FOR
            //match = regex_for.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.For)).Success)
            {
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild(); // встал на дерево, вершиной которого является for
                cursor.type = "for";
                continue;
            }
            #endregion
            #endregion

            #region BEGIN
            //match = regex_begin.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Begin)).Success)
            {
                while (!cursor.is_root && !is_func)
                    cursor = cursor.parent;
                if (cursor.type == "function" || cursor.type == "procedure")
                    cursor.is_root = false;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor = cursor.GetLastChild();
                cursor.type = "begin";
                cursor.ignore_enter = true;
                cursor.ignore_exit = true;
                if (!is_func)
                {
                    is_func = true;
                    cursor.is_root = true;
                }
                continue;
            }
            #endregion

            #region END
            //match = regex_end_program.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.EndProgram)).Success)
            {
                if (cursor.is_root)
                    is_func = false;
                else
                    throw new Exception("Конец программы встретился раньше, чем закрылись все операторные скобки");
                str_stack = "";
                cursor = root_program;
                break;
            }
            //match = regex_end.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.End)).Success)
            if (match.Success)
            {
                if (cursor.is_root)
                    is_func = false;
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor = cursor.parent;
                continue;
            }
            #endregion

            #region Выражения
            //match = regex_expr.Match(str_stack);
            if ((match = Regexs.Match(str_stack, Regs.Operation)).Success)
            {
                str_stack = str_stack.Remove(str_stack.IndexOf(match.Value), match.Value.Length);
                cursor.AppendChild(new Tree(match.Value, words_count - match.Value.TrimStart().Length, words_count, line_count));
                cursor.GetLastChild().type = "operation";
                continue;
            }
            #endregion

            is_wait = true;
        }

        return root_program;
    }

    /// <summary>
    /// Получает курсор на указанный дочерний элемент
    /// </summary>
    /// <param name="root">Родитель</param>
    /// <param name="command">Команда курсора</param>
    /// <returns>Курсор</returns>
    static Tree GetElementCursor(Tree root, string command)
    {
        if (!root.HasChild())
            return null;
        Tree cursor = root.GetChildById(0);
        while (cursor.command != command && cursor.next != null)
            cursor = cursor.next;
        if (cursor.command != command)
            return null;
        return cursor;
    }

    /// <summary>
    /// Приводит дерево к типу TypeStruct
    /// </summary>
    /// <param name="root">Корневой элемент</param>
    /// <param name="root_name">Имя родителя</param>
    /// <returns></returns>
    static TypeStruct[] GetTypesStruct(Tree root, string root_name)
    {
        if (root_name != "type" && root_name != "var" && root_name != "const")
            return null;

        if (root == null)
            return null;

        Tree start_cursor = GetElementCursor(root, root_name);

        if (start_cursor == null)
            return null;

        if (!start_cursor.HasChild())
            return null;

        TypeStruct[] types = new TypeStruct[start_cursor.GetChildCount()];

        for (int i = 0; i < types.Length; i++)
        {
            Tree cursor = start_cursor.GetChildById(i);
            string type_str = cursor.command;
            if (cursor.HasChild())
                cursor = cursor.GetChildById(0);
            while (cursor.parent.command != root_name)
            {
                cursor.enters_count++;
                if (cursor.enters_count == 1)
                    type_str += " " + cursor.command;
                else if (cursor.type == "record")
                    type_str += " end;";
                if (cursor.HasChild() && cursor.enters_count == 1)
                    cursor = cursor.GetChildById(0);
                else if (cursor.next == null)
                    cursor = cursor.parent;
                else
                    cursor = cursor.next;
            }

            if (cursor.type == "record")
                type_str += " end;";

            int j = 0;
            if (root_name == "type" || root_name == "const")
                while (type_str[j] != '=')
                    j++;
            else if (root_name == "var")
                while (type_str[j] != ':')
                    j++;
            while (type_str.EndsWith(";"))
                type_str = type_str.Remove(type_str.Length - 1);

            string[] replace = new string[] { "\t", "\n", "\r" };
            for (int k = 0; k < replace.Length; k++)
                type_str = type_str.Replace(replace[k], " ");
            while (type_str.IndexOf("  ") > -1)
                type_str = type_str.Replace("  ", " ");

            types[i] = new TypeStruct();
            types[i].name = type_str.Remove(j).Trim();
            types[i].type = type_str.Remove(0, j + 1).Trim();
        }

        return types;
    }

    /// <summary>
    /// Возвращает строку запрошенного типа
    /// </summary>
    /// <param name="_name">Имя типа</param>
    /// <returns>Тип в формате строки</returns>
    private string FindType(string _name)
    {
        foreach (TypeStruct ts in program.types)
            if (ts.name == _name)
                return ts.type;
        return _name;
    }

    /// <summary>
    /// Возвращает настоящий тип переменной
    /// </summary>
    /// <param name="type">Исходный тип</param>
    /// <returns>Настоящий тип</returns>
    private string GetRealType(string type)
    {
        string[] replace = new string[] { " ", ":", ",", "^" };
        for (int i = 0; i < replace.Length; i++)
            type = type.Replace(replace[i], "☺" + replace[i] + "☺");
        type = type.Replace(";", "☺;☺");
        string[] words = type.Split('☺');
        type = "";
        bool replaced = true;
        foreach (string word in words)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                type += " ";
                continue;
            }

            if (word == "record" || word == ";")
                replaced = false;
            else if (word == ":")
                replaced = true;

            if (replaced)
                type += FindType(word.Trim());
            else
                type += word.Trim();
        }

        for (int i = 1; i < replace.Length; i++)
        {
            while (type.IndexOf(replace[i] + " ") > -1)
                type = type.Replace(replace[i] + " ", replace[i]);
            while (type.IndexOf(" " + replace[i]) > -1)
                type = type.Replace(" " + replace[i], replace[i]);
        }
        while (type.IndexOf("  ") > -1)
            type = type.Replace("  ", " ");

        return type.Trim();
    }

    /// <summary>
    /// Обработка следующей команды
    /// </summary>
    /// <returns>Указатель на выполненную команду</returns>
    public Tree DoNextCommand()
    {
        Tree current = cursor;
        if (cursor == null)
            return null;
        if (cursor.is_root)
        {
            cursor = null;
            return null;
        }

        if (current.type == "for")
        {
            if (!current.HasChild())
                throw new Exception("Оператор FOR не имеет потомков");

            InitFor(current.command, current.enters_count);

            bool result = GetForResult(current.command);
            if (result)
                cursor = FindChildCursor(current);
            else
                cursor = FindNextCursor(current);
        }
        else if (current.type == "if"/* && current.enters_count == 0*/)
        {
            if (!current.HasChild())
                throw new Exception("Оператор IF не имеет потомков");
            bool result = GetIfResult(current.command);
            if (result)
                cursor = FindChildCursor(current);
            else if (current.GetChildCount() >= 2)
                cursor = FindChildCursor(current, 1);
            else
                cursor = FindNextCursor(current);
        }
        else
        {
            WorkCommand(current.command);
            cursor = FindNextCursor(current);
        }

        current.enters_count++;
        return current;
    }

    /// <summary>
    /// Обработка команды
    /// </summary>
    /// <param name="command_line">Команда языка Pascal</param>
    public void WorkCommand(string command_line)
    {
        Match m;
        if ((m = Regexs.Match(command_line, Regs.Addr)).Success)
        {
            env.AssignmentAddr(m.Groups["var"].Value, m.Groups["operand"].Value);
            //Console.WriteLine("{0} := @{1}; // Взятие адреса",
            //    m.Groups["var"],
            //    m.Groups["operand"]);
        }
        else if ((m = Regexs.Match(command_line, Regs.AssignVar)).Success)
        {
            env.AssignmentVar(m.Groups["var"].Value, m.Groups["operand"].Value);
            //Console.WriteLine("{0} := @{1}; // Взятие адреса",
            //    m.Groups["var"],
            //    m.Groups["operand"]);
        }
        else if ((m = Regexs.Match(command_line, Regs.Expr)).Success)
        {
            env.AssignmentExpr(m.Groups["var"].Value, m.Groups["expr"].Value);
            //Console.WriteLine("{0} := {1}; // Присвоить значение выражения",
            //    m.Groups["var"],
            //    m.Groups["expr"]);
        }
        else if ((m = Regexs.Match(command_line, Regs.Func)).Success)
        {
            //Console.WriteLine("{0} := {1}({2}); // Присвоить значение функции",
            //    m.Groups["var"],
            //    m.Groups["func"],
            //    m.Groups["param"]);
        }
        else if ((m = Regexs.Match(command_line, Regs.Proc)).Success)
        {
            //Console.WriteLine("{0}({1}); // Выполнить процедуру",
            //    m.Groups["proc"],
            //    m.Groups["param"]);
        }
        else
        {
            Console.WriteLine(command_line + " // Нераспознанная команда");
        }

        return;


        //Console.WriteLine(command_line);
        //if (command_line.IndexOf(";") == command_line.Length - 1)
        //    command_line = command_line.Remove(command_line.Length - 1);
        //else if (command_line.IndexOf(";") > -1)
        //    throw new Exception("Встретился операнд конца строки внутри операции");

        //if (command_line.Contains(":=@"))
        //{
        //    int r = command_line.IndexOf(":=@");
        //    string operand = "";
        //    for (int i = 0; i < r; i++)
        //        operand += command_line[i];
        //    string second_operand = "";
        //    for (int i = r + 3; i < command_line.Length; i++)
        //        second_operand += command_line[i];
        //    env.AssignmentAddr(operand, second_operand);
        //}
        //else if (command_line.Contains(":="))
        //{
        //    int r = command_line.IndexOf(":=");
        //    string operand = "";
        //    for (int i = 0; i < r; i++)
        //        operand += command_line[i];
        //    string expression = "";
        //    for (int i = r + 2; i < command_line.Length; i++)
        //        expression += command_line[i];
        //    env.Assignment(operand, expression);
        //}
        //else if (command_line.StartsWith("new(") && command_line.EndsWith(")"))
        //{
        //    string operand = command_line.Remove(0, 4).Remove(command_line.Length - 5);
        //    Variable var = env.GetElementByName(operand);
        //    if (var == null)
        //        throw new Exception("Используется необъявленная переменная: " + operand);
        //    if (!var.pointer)
        //        throw new Exception("Операция new не применима к статическим типам");
        //    ((Pointer)var).Value = InitVariable(var.type);
        //}
        //else
        //{
        //    env.GetElementByName(command_line).Dump();
        //}
    }

    /// <summary>
    /// Ищет ребёнка у курсора
    /// </summary>
    /// <param name="cursor">Курсор</param>
    /// <param name="child_id">Номер, с которого нужно начинать поиск</param>
    /// <returns>Курсор</returns>
    private Tree FindChildCursor(Tree cursor, int child_id = 0)
    {
        if (cursor.GetChildCount() > child_id)
            cursor = cursor.GetChildById(child_id);

        if (cursor.ignore_enter && cursor.HasChild())
            cursor = FindChildCursor(cursor);

        return cursor;
    }

    /// <summary>
    /// Ищет брата у курсора
    /// </summary>
    /// <param name="cursor">Курсор</param>
    /// <returns>Курсор</returns>
    private Tree FindSibling(Tree cursor)
    {
        if (cursor.parent.ignore_sibling)
            cursor = FindSibling(cursor.parent);
        else if (cursor.next != null)
            cursor = cursor.next;
        else if (cursor.parent.ignore_exit)
            cursor = FindSibling(cursor.parent);
        else
            cursor = cursor.parent;

        return cursor;
    }

    /// <summary>
    /// Ищет следующий для выполнения курсор
    /// </summary>
    /// <param name="cursor">Курсор</param>
    /// <returns>Курсор</returns>
    private Tree FindNextCursor(Tree cursor)
    {
        if (cursor.enters_count == 0 && cursor.HasChild())
            cursor = FindChildCursor(cursor);
        else
            cursor = FindSibling(cursor);


        return cursor;
    }

    /// <summary>
    /// Инициализация или следующий шаг цикла FOR
    /// </summary>
    /// <param name="command_line">Строка цикла FOR</param>
    /// <param name="enters_count">Количество произведённых итераций</param>
    private void InitFor(string command_line, int enters_count)
    {
        Match m;
        string start;
        if ((m = Regexs.Match(command_line, Regs.For)).Success)
            start = m.Groups["start"].Value;
        else
            throw new Exception("Была получена ошибка при иниализации блока FOR");

        if (enters_count == 0)
            WorkCommand(start);
        else
        {
            Match var;
            if ((var = Regexs.Match(start, Regs.Expr)).Success)
            {
                string var_name = var.Groups["var"].Value;
                WorkCommand(var_name + ":=" + var_name + "+1");
            }
        }
    }

    /// <summary>
    /// Получает результат выполнения выражения, переданного в условный оператор
    /// </summary>
    /// <param name="command_line">Строка условного оператора</param>
    /// <returns>Логическое значение</returns>
    public bool GetIfResult(string command_line)
    {
        Match m;
        if ((m = Regexs.Match(command_line, Regs.If)).Success)
        {
            object result = env.Calculate(m.Groups["expr"].Value);
            if (result.GetType().Name == "Boolean")
                return (bool)result;
            return false;
        }
        else
            return false;
    }

    /// <summary>
    /// Получает результат для работы цикла FOR
    /// </summary>
    /// <param name="command_line">Строка цикла FOR</param>
    /// <returns>Логическое значение</returns>
    public bool GetForResult(string command_line)
    {
        Match m;
        if ((m = Regexs.Match(command_line, Regs.For)).Success)
        {
            Match var;
            if ((var = Regexs.Match(m.Groups["start"].Value, Regs.Expr)).Success)
            {
                object result = env.Calculate(var.Groups["var"].Value + "<=" + m.Groups["exit_oper"].Value);
                if (result.GetType().Name == "Boolean")
                    return (bool)result;
                else
                    return false;
            }
            else
                return false;
        }
        else
            return false;
    }

    /// <summary>
    /// Возвращает текущее окружение
    /// </summary>
    /// <returns>Окружение программы</returns>
    public Environs GetEnviroment()
    {
        return env;
    }

    /// <summary>
    /// Разделяет строку на массив переменных, определённых структорой TypeStruct
    /// </summary>
    /// <param name="type">Строка, содержащая код блока VAR</param>
    /// <returns>Массив переменных</returns>
    private TypeStruct[] GetTypeStructFromField(string var)
    {
        TypeStruct[] vars = new TypeStruct[0];

        int dp = 0, i = 1, var_declarate = 0;
        bool is_name = true,
            is_type = false;
        string name = var[0].ToString(), type = "";

        while (i < var.Length)
        {
            if (var[i] == ':')
                dp++;
            else if (var[i] == ';')
                dp--;

            if (dp == 0 && char.IsLetter(var[i]) && var[i - 1] == ' ')
                is_name = true;
            else if (dp == 1 && var[i - 1] == ':')
                is_type = true;
            else if (dp == 1 && var[i] == ':')
            {
                is_name = false;
                string[] names = name.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var_declarate = names.Length;
                Array.Resize(ref vars, vars.Length + var_declarate);
                for (int j = 0; j < var_declarate; j++)
                {
                    vars[vars.Length - var_declarate + j] = new TypeStruct();
                    vars[vars.Length - var_declarate + j].name = names[j];
                }
                name = "";
            }
            else if (dp == 0 && var[i] == ';')
            {
                is_type = false;
                for (int j = 0; j < var_declarate; j++)
                    vars[vars.Length - var_declarate + j].type = type;
                type = "";
            }

            if (is_name)
                name += var[i];
            if (is_type)
                type += var[i];

            i++;
        }

        return vars;
    }

    /// <summary>
    /// Проверяет, чтобы имена переменных, типов и констант не повторялись
    /// </summary>
    private void CheckConflictNames()
    {
        string[] names;
        int c_len = 0, t_len = 0, v_len = 0;

        if (program.consts != null)
            c_len = program.consts.Length;
        if (program.types != null)
            t_len = program.types.Length;
        if (program.vars != null)
            v_len = program.vars.Length;

        names = new string[c_len + t_len + v_len];

        for (int i = 0; i < c_len; i++)
        {
            for (int j = 0; j < names.Length; j++)
                if (program.consts[i].name == names[j])
                    throw new Exception("Конфликт имён. Нельзя переопределить имя константы/типа/переменной.");
            names[i] = program.consts[i].name;
        }

        int delta = c_len;

        for (int i = 0; i < t_len; i++)
        {
            for (int j = 0; j < names.Length; j++)
                if (program.types[i].name == names[j])
                    throw new Exception("Конфликт имён. Нельзя переопределить имя константы/типа/переменной.");
            names[i + delta] = program.types[i].name;
        }

        delta += t_len;

        for (int i = 0; i < v_len; i++)
        {
            string[] nms = program.vars[i].name.Split(',');
            foreach (string n in nms)
            {
                for (int j = 0; j < names.Length; j++)
                    if (n.Trim() == names[j])
                        throw new Exception("Конфликт имён. Нельзя переопределить имя константы/типа/переменной.");
                names[i + delta] = n.Trim();
            }
        }
    }
}
