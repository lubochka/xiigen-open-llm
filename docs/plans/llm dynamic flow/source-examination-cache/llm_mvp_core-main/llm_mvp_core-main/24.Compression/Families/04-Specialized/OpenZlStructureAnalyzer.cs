namespace Compression;

// Analyzes structured data (JSON, CSV) to identify columns that can be
// rearranged for better compression. Columnar layouts group similar values
// together, which improves LZ77 match distances and entropy coding.
public sealed class OpenZlStructureAnalyzer
{
    public enum DataFormat { Unknown, Csv, Json }

    public sealed record ColumnInfo(string Name, int Index, DataFormat Format, double EstimatedEntropy);

    public sealed record StructureReport(
        DataFormat Format,
        int RowCount,
        int ColumnCount,
        IReadOnlyList<ColumnInfo> Columns,
        bool IsColumnarFriendly);

    // Detect format and analyze columns.
    public StructureReport Analyze(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new StructureReport(DataFormat.Unknown, 0, 0, [], false);

        var trimmed = text.TrimStart();

        if (trimmed.StartsWith('[') || trimmed.StartsWith('{'))
            return AnalyzeJson(text);

        return AnalyzeCsv(text);
    }

    private StructureReport AnalyzeCsv(string text)
    {
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
            return new StructureReport(DataFormat.Csv, lines.Length, 0, [], false);

        var headers = SplitCsvLine(lines[0]);
        int colCount = headers.Length;
        var rows     = new List<string[]>();

        for (int i = 1; i < lines.Length; i++)
        {
            var parts = SplitCsvLine(lines[i]);
            if (parts.Length == colCount)
                rows.Add(parts);
        }

        var columns = new List<ColumnInfo>();
        for (int c = 0; c < colCount; c++)
        {
            var values  = rows.Select(r => r[c]).ToArray();
            double ent  = EstimateStringEntropy(values);
            columns.Add(new ColumnInfo(headers[c].Trim(), c, DataFormat.Csv, ent));
        }

        bool friendly = rows.Count >= 4 && colCount >= 2;
        return new StructureReport(DataFormat.Csv, rows.Count, colCount, columns, friendly);
    }

    private StructureReport AnalyzeJson(string text)
    {
        // Parse a JSON array of objects using lightweight scanning.
        var records = ParseJsonArray(text);
        if (records.Count == 0)
            return new StructureReport(DataFormat.Json, 0, 0, [], false);

        var allKeys = records.SelectMany(r => r.Keys).Distinct().ToList();
        var columns = new List<ColumnInfo>();

        for (int i = 0; i < allKeys.Count; i++)
        {
            string key  = allKeys[i];
            var vals    = records.Where(r => r.ContainsKey(key)).Select(r => r[key]).ToArray();
            double ent  = EstimateStringEntropy(vals);
            columns.Add(new ColumnInfo(key, i, DataFormat.Json, ent));
        }

        bool friendly = records.Count >= 4 && allKeys.Count >= 2;
        return new StructureReport(DataFormat.Json, records.Count, allKeys.Count, columns, friendly);
    }

    // Estimate byte-level Shannon entropy from a set of string values.
    private static double EstimateStringEntropy(string[] values)
    {
        if (values.Length == 0) return 0;

        var freq = new Dictionary<char, int>();
        int total = 0;

        foreach (var v in values)
        {
            foreach (char c in v)
            {
                freq[c] = freq.GetValueOrDefault(c) + 1;
                total++;
            }
        }

        if (total == 0) return 0;

        double entropy = 0;
        foreach (var count in freq.Values)
        {
            double p = (double)count / total;
            entropy -= p * Math.Log2(p);
        }

        return entropy;
    }

    private static string[] SplitCsvLine(string line)
    {
        var parts  = new List<string>();
        bool inQ   = false;
        var  cur   = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')        { inQ = !inQ; }
            else if (c == ',' && !inQ) { parts.Add(cur.ToString()); cur.Clear(); }
            else                 { cur.Append(c); }
        }

        parts.Add(cur.ToString());
        return parts.ToArray();
    }

    // Minimal JSON array-of-objects parser — handles flat string/number values.
    private static List<Dictionary<string, string>> ParseJsonArray(string json)
    {
        var result = new List<Dictionary<string, string>>();
        int i      = 0;

        SkipWhitespace(json, ref i);
        if (i >= json.Length || json[i] != '[') return result;
        i++;

        while (i < json.Length && json[i] != ']')
        {
            SkipWhitespace(json, ref i);
            if (i >= json.Length || json[i] != '{') { i++; continue; }
            i++;

            var record = new Dictionary<string, string>();

            while (i < json.Length && json[i] != '}')
            {
                SkipWhitespace(json, ref i);
                string key = ReadJsonString(json, ref i);
                SkipWhitespace(json, ref i);
                if (i < json.Length && json[i] == ':') i++;
                SkipWhitespace(json, ref i);
                string val = ReadJsonValue(json, ref i);
                SkipWhitespace(json, ref i);
                if (i < json.Length && json[i] == ',') i++;
                if (!string.IsNullOrEmpty(key))
                    record[key] = val;
            }

            if (i < json.Length) i++; // skip '}'
            result.Add(record);
            SkipWhitespace(json, ref i);
            if (i < json.Length && json[i] == ',') i++;
        }

        return result;
    }

    private static void SkipWhitespace(string s, ref int i)
    {
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
    }

    private static string ReadJsonString(string s, ref int i)
    {
        if (i >= s.Length || s[i] != '"') return "";
        i++;
        var sb = new System.Text.StringBuilder();
        while (i < s.Length && s[i] != '"')
        {
            if (s[i] == '\\') i++;
            if (i < s.Length) sb.Append(s[i++]);
        }
        if (i < s.Length) i++; // closing quote
        return sb.ToString();
    }

    private static string ReadJsonValue(string s, ref int i)
    {
        if (i >= s.Length) return "";
        if (s[i] == '"') return ReadJsonString(s, ref i);

        var sb = new System.Text.StringBuilder();
        while (i < s.Length && s[i] != ',' && s[i] != '}' && s[i] != ']')
            sb.Append(s[i++]);
        return sb.ToString().Trim();
    }
}
