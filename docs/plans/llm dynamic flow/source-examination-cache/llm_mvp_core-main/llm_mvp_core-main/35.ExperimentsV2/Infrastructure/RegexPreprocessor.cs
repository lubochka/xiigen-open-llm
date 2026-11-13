using System.Text.RegularExpressions;

namespace ExperimentsV2;

// RegexPreprocessor убирает шум из C# файлов до передачи в Roslyn.
// Цель: уменьшить объём текста который Roslyn должен парсить.
// Гарантия: нормализованный код синтаксически корректен (только чище).
public static class RegexPreprocessor
{
    // Порядок применения важен: сначала XML-docs, потом блочные, потом строки.
    private static readonly Regex XmlDocs = new(
        @"///[^\n]*", RegexOptions.Compiled);

    private static readonly Regex BlockComments = new(
        @"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex LineComments = new(
        @"//[^\n]*", RegexOptions.Compiled);

    private static readonly Regex StringLiterals = new(
        @"@""(?:[^""]|"""")*""|""(?:[^""\\]|\\.)*""",
        RegexOptions.Compiled);

    private static readonly Regex ExtraBlankLines = new(
        @"\n(\s*\n){2,}", RegexOptions.Compiled);

    // Нормализовать C# исходный код.
    // Возвращает текст без комментариев, с пустыми строковыми литералами,
    // без лишних пустых строк.
    public static string Normalize(string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return source;
        string s = XmlDocs.Replace(source, "");
        s = BlockComments.Replace(s, " ");
        s = LineComments.Replace(s, "");
        s = StringLiterals.Replace(s, "\"\"");
        s = ExtraBlankLines.Replace(s, "\n\n");
        return s;
    }
}
