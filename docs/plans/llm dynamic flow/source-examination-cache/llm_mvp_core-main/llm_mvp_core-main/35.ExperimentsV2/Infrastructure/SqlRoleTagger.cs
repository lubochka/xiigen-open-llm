namespace ExperimentsV2;

// Tags SQL tokens with one of 5 roles: KEYWORD=0, IDENT=1, OPERATOR=2, LITERAL=3, PUNCT=4.
// Hand-written tokeniser — no external SQL parser.  Used by E08b.
public static class SqlRoleTagger
{
    public const int RoleCount = 5;
    public const int KEYWORD   = 0;
    public const int IDENT     = 1;
    public const int OPERATOR  = 2;
    public const int LITERAL   = 3;
    public const int PUNCT     = 4;

    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT", "FROM", "WHERE", "JOIN", "ON", "INNER", "LEFT", "RIGHT", "OUTER",
        "FULL", "CROSS", "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE",
        "CREATE", "TABLE", "DROP", "ALTER", "ADD", "COLUMN", "INDEX",
        "ORDER", "BY", "GROUP", "HAVING", "LIMIT", "OFFSET", "UNION", "ALL",
        "AS", "DISTINCT", "NOT", "AND", "OR", "IS", "NULL", "IN", "BETWEEN",
        "LIKE", "EXISTS", "CASE", "WHEN", "THEN", "ELSE", "END",
        "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "CONSTRAINT", "DEFAULT"
    };

    // Tokenise SQL text and return a list of role ids.
    public static List<int> TagSql(string sql)
    {
        var result = new List<int>();
        int i = 0;

        while (i < sql.Length)
        {
            if (char.IsWhiteSpace(sql[i])) { i++; continue; }

            // String literal: '...'
            if (sql[i] == '\'')
            {
                i++;
                while (i < sql.Length && sql[i] != '\'') i++;
                if (i < sql.Length) i++;
                result.Add(LITERAL);
                continue;
            }

            // Number literal
            if (char.IsDigit(sql[i]))
            {
                while (i < sql.Length && (char.IsDigit(sql[i]) || sql[i] == '.')) i++;
                result.Add(LITERAL);
                continue;
            }

            // Quoted identifier: "..." or `...`
            if (sql[i] == '"' || sql[i] == '`')
            {
                char q = sql[i++];
                while (i < sql.Length && sql[i] != q) i++;
                if (i < sql.Length) i++;
                result.Add(IDENT);
                continue;
            }

            // Identifier or keyword
            if (char.IsLetter(sql[i]) || sql[i] == '_')
            {
                int start = i;
                while (i < sql.Length && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_')) i++;
                result.Add(Keywords.Contains(sql[start..i]) ? KEYWORD : IDENT);
                continue;
            }

            // Two-char operators: !=  <>  <=  >=
            if (i + 1 < sql.Length &&
                ((sql[i] == '!' && sql[i + 1] == '=') ||
                 (sql[i] == '<' && sql[i + 1] == '>') ||
                 (sql[i] == '<' && sql[i + 1] == '=') ||
                 (sql[i] == '>' && sql[i + 1] == '=')))
            {
                i += 2;
                result.Add(OPERATOR);
                continue;
            }

            // Single-char operators
            if (sql[i] == '=' || sql[i] == '<' || sql[i] == '>' ||
                sql[i] == '+' || sql[i] == '-' || sql[i] == '/' || sql[i] == '%')
            {
                i++;
                result.Add(OPERATOR);
                continue;
            }

            // Punctuation: , ; ( ) . * [ ]
            if (sql[i] == ',' || sql[i] == ';' || sql[i] == '(' || sql[i] == ')' ||
                sql[i] == '.' || sql[i] == '*' || sql[i] == '[' || sql[i] == ']')
            {
                i++;
                result.Add(PUNCT);
                continue;
            }

            i++; // skip unknown char
        }
        return result;
    }
}
