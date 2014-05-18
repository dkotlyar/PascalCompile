using System.Text.RegularExpressions;

public enum Regs { Program, Const, Type, Var, Function, Procedure, Record, CommentStart, Comment, CommentsStart, Comments, 
    Begin, End, EndProgram, Operation, EndElse, For, If, Else, Addr, Func, Proc, Expr, AssignVar};
public class Regexs
{
    private static string pattern_var_name = @"[a-z]+[\w_.^]*";
    private static string pattern_left_assign = @"(?:[\s]*)(?<var>" + pattern_var_name + @")(?:[\s]*)(?::=){1}";

    public static Regex[] Regexes = new Regex[] {
        new Regex(@"\bprogram\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\bconst\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\btype\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\bvar\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\bfunction\b(.*)\(([^\)]*)\)(.*):(.*)([^;]*);", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\bprocedure\b(.*)\(([^\)]*)\)([^;]*);", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"(.*)\brecord\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        new Regex(@"//([^\n]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"//([^\n]*)\n{1,}", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        new Regex(@"{+([^}]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"{[^}]*}", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        new Regex(@"\bbegin\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\bend\b([;]+)", RegexOptions.Multiline| RegexOptions.IgnoreCase),
        new Regex(@"\bend\b([.]+)", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"([^;]*);", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        new Regex(@"\bend\b(.*)\belse\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\bfor\b(?:[\s]*)(?<start>.*)(?:[\s]*)(to|downto)+(?:[\s]*)(?<exit_oper>.*)(?:[\s]*)\bdo\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"\bif\b(?:[\s]*)(?<expr>.*)(?:[\s]*)\bthen\b", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"((.*?)(?=else))", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        new Regex("^" + pattern_left_assign + @"(?:[\s]*)\@+(?<operand>" + pattern_var_name + @");{0,1}$", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex("^" + pattern_left_assign + @"(?:[\s]*)(?<func>" + pattern_var_name + @")\((?<param>.*)\)([\s]*);{0,1}$", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex(@"^(?:[\s]*)(?<proc>" + pattern_var_name + @")\((?<param>.*)\)([\s]*);{0,1}$", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex("^" + pattern_left_assign + @"(?<expr>[^;@]+);{0,1}$", RegexOptions.Multiline | RegexOptions.IgnoreCase),
        new Regex("^" + pattern_left_assign + @"(?:[\s]*)(?<operand>" + pattern_var_name + @")(?:[\s]*);{0,1}$", RegexOptions.IgnoreCase)
    };

    public static Match Match(string input, Regs regex)
    {
        return Regexes[(int)regex].Match(input);
    }
}