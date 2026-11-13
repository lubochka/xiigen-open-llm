# E08 — Структурные роли на не-кодовых данных
## Подробный план для Claude Code v2.0

**Проект:** `35.ExperimentsV2`
**Без Python, без внешних зависимостей — только C# и System.Text.Json**
**Этот документ не касается XIIGen.**

---

## Что проверяем и зачем

Все предыдущие эксперименты работали на C# коде. Грамматика C# жёсткая —
компилятор не принимает ошибки, поэтому структура очень предсказуема.
Мы не знаем: работают ли наши методы потому что они правильные,
или только потому что C# такой предсказуемый.

Нужно проверить на трёх принципиально разных типах данных:
- **Английский текст** — грамматика мягкая, структура есть но не строгая
- **SQL запросы** — жёсткая грамматика, но совсем другой язык
- **JSON** — чистая вложенная структура без грамматики языка

Для каждого сравниваем одно и то же:
- Baseline: токены как плоские куски текста (как BPE делает сейчас)
- Enhanced: токены как структурные роли (существительное, глагол, ключевое слово SELECT, открывающая скобка и т.д.)

---

## Три под-эксперимента в одном файле

```
E08a — Английский текст   (датасет: Moby Dick, скачивается автоматически)
E08b — SQL запросы        (датасет: генерируется программно, 500 запросов)
E08c — JSON документы     (датасет: список файлов проекта как JSON, ~200 документов)
```

Все три запускаются одной командой `runner.Run("E08", tier: 1)`.
Результат — три строки в notes, один общий вывод.

---

## Как работает структурный токен для каждого типа

### Английский текст

Каждое слово получает одну из 12 ролей:

```
DET   — the, a, an, this, that, these, those, my, your, his, her, our
PREP  — in, on, at, by, for, with, about, from, to, of, into, through
CONJ  — and, or, but, because, if, when, while, although, since
PRON  — I, you, he, she, it, we, they, me, him, us, them, who
AUX   — is, are, was, were, be, been, have, has, had, will, would, can, could
PUNCT — . , ! ? ; : - ( ) " '
NUM   — любое слово которое целиком состоит из цифр
NOUN  — слово заканчивается на -tion, -ness, -ment, -ity, -er, -or (или не попало выше)
VERB  — слово заканчивается на -ed, -ing, -ize, -ise (или стоит после AUX)
ADJ   — слово заканчивается на -ful, -less, -ous, -ive, -able, -ible
ADV   — слово заканчивается на -ly
WORD  — всё остальное (неизвестная роль)
```

Словарь — 12 ролей вместо 500+ BPE подслов.
Это грубый теггер, не лингвистически точный — но достаточный для проверки гипотезы.

### SQL запросы

Каждый токен SQL получает роль:

```
KEYWORD  — SELECT, FROM, WHERE, JOIN, ON, GROUP, ORDER, BY, HAVING, LIMIT,
            INSERT, UPDATE, DELETE, CREATE, DROP, AND, OR, NOT, IN, AS, DISTINCT
IDENT    — имя таблицы или колонки (не ключевое слово)
OPERATOR — =, >, <, >=, <=, <>, !=, LIKE, BETWEEN
LITERAL  — строковый литерал в кавычках или число
PUNCT    — ( ) , ;
```

Словарь — 5 ролей.

### JSON

Каждый токен JSON (из System.Text.Json.Utf8JsonReader) получает роль:

```
OBJ_START   — {
OBJ_END     — }
ARR_START   — [
ARR_END     — ]
KEY         — имя поля в объекте
STR_VALUE   — строковое значение
NUM_VALUE   — числовое значение
BOOL_VALUE  — true / false
NULL_VALUE  — null
```

Словарь — 9 ролей.

---

## Шаг 1 — Создать три токенизатора

### `35.ExperimentsV2/Infrastructure/EnglishRoleTagger.cs`

```csharp
namespace ExperimentsV2;

// EnglishRoleTagger назначает каждому слову одну из 12 синтаксических ролей.
// Не использует ML — только словарь функциональных слов и суффиксные правила.
// Достаточно для проверки гипотезы что структурные роли лучше плоских слов.
public static class EnglishRoleTagger
{
    private static readonly HashSet<string> Determiners = new(StringComparer.OrdinalIgnoreCase)
    {
        "the","a","an","this","that","these","those","my","your","his","her",
        "its","our","their","some","any","each","every","both","few","more",
        "most","other","such","no","own","same","so","than","too","very","just"
    };
    private static readonly HashSet<string> Prepositions = new(StringComparer.OrdinalIgnoreCase)
    {
        "in","on","at","by","for","with","about","against","between","into",
        "through","during","before","after","above","below","from","up","down",
        "out","off","over","under","again","further","then","once","to","of"
    };
    private static readonly HashSet<string> Conjunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "and","or","but","because","if","when","while","although","since",
        "unless","until","whether","nor","so","yet","though","as"
    };
    private static readonly HashSet<string> Pronouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "i","you","he","she","it","we","they","me","him","her","us","them",
        "who","which","what","that","this","these","those","myself","yourself"
    };
    private static readonly HashSet<string> Auxiliaries = new(StringComparer.OrdinalIgnoreCase)
    {
        "is","are","was","were","be","been","being","have","has","had","do",
        "does","did","will","would","could","should","may","might","must","shall"
    };

    // Разбить текст на слова и назначить роль каждому.
    // Возвращает список числовых идентификаторов ролей (0-11).
    public static List<int> TagToIds(string text)
    {
        var result = new List<int>();
        foreach (var token in Tokenize(text))
        {
            result.Add(GetRoleId(token));
        }
        return result;
    }

    // Словарь ролей: число → название (для notes)
    public static readonly string[] RoleNames =
    [
        "DET","PREP","CONJ","PRON","AUX","PUNCT","NUM","NOUN","VERB","ADJ","ADV","WORD"
    ];

    private static int GetRoleId(string token)
    {
        if (Determiners.Contains(token))  return 0;  // DET
        if (Prepositions.Contains(token)) return 1;  // PREP
        if (Conjunctions.Contains(token)) return 2;  // CONJ
        if (Pronouns.Contains(token))     return 3;  // PRON
        if (Auxiliaries.Contains(token))  return 4;  // AUX
        if (token.Length == 1 && !char.IsLetterOrDigit(token[0])) return 5; // PUNCT
        if (token.All(char.IsDigit))      return 6;  // NUM
        // Суффиксные правила
        string low = token.ToLowerInvariant();
        if (low.EndsWith("ly"))           return 10; // ADV
        if (low.EndsWith("ful") || low.EndsWith("less") || low.EndsWith("ous") ||
            low.EndsWith("ive") || low.EndsWith("able") || low.EndsWith("ible")) return 9; // ADJ
        if (low.EndsWith("ing") || low.EndsWith("ize") || low.EndsWith("ise")) return 8; // VERB
        if (low.EndsWith("ed"))           return 8;  // VERB
        if (low.EndsWith("tion") || low.EndsWith("ness") || low.EndsWith("ment") ||
            low.EndsWith("ity") || low.EndsWith("er") || low.EndsWith("or"))   return 7; // NOUN
        return 11; // WORD
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        int start = 0;
        for (int i = 0; i <= text.Length; i++)
        {
            bool end = i == text.Length;
            bool sep = !end && (char.IsWhiteSpace(text[i]) ||
                       (text[i] is '.' or ',' or '!' or '?' or ';' or ':' or
                                   '"' or '\'' or '(' or ')' or '-'));
            if (end || sep)
            {
                if (i > start) yield return text[start..i];
                if (!end && !char.IsWhiteSpace(text[i]))
                    yield return text[i].ToString();
                start = i + 1;
            }
        }
    }
}
```

### `35.ExperimentsV2/Infrastructure/SqlRoleTagger.cs`

```csharp
namespace ExperimentsV2;

// SqlRoleTagger разбивает SQL запрос на токены и назначает каждому структурную роль.
public static class SqlRoleTagger
{
    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "SELECT","FROM","WHERE","JOIN","LEFT","RIGHT","INNER","OUTER","FULL",
        "ON","GROUP","ORDER","BY","HAVING","LIMIT","OFFSET","UNION","ALL",
        "INSERT","INTO","VALUES","UPDATE","SET","DELETE","CREATE","TABLE",
        "DROP","ALTER","ADD","COLUMN","INDEX","AND","OR","NOT","IN","AS",
        "DISTINCT","TOP","COUNT","SUM","AVG","MIN","MAX","CASE","WHEN",
        "THEN","ELSE","END","EXISTS","BETWEEN","LIKE","IS","NULL","TRUE","FALSE"
    };

    // Вернуть список идентификаторов ролей для SQL текста
    public static List<int> TagToIds(string sql)
    {
        var result = new List<int>();
        foreach (var token in TokenizeSql(sql))
            result.Add(GetRoleId(token));
        return result;
    }

    public static readonly string[] RoleNames = ["KEYWORD","IDENT","OPERATOR","LITERAL","PUNCT"];

    private static int GetRoleId(string token)
    {
        if (Keywords.Contains(token)) return 0; // KEYWORD
        if (token is "=" or ">" or "<" or ">=" or "<=" or "<>" or "!=" or
                     "LIKE" or "BETWEEN" or "+" or "-" or "*" or "/") return 2; // OPERATOR
        if (token is "(" or ")" or "," or ";") return 4; // PUNCT
        if (token.StartsWith("'") || token.StartsWith("\"")) return 3; // LITERAL
        if (token.All(c => char.IsDigit(c) || c == '.')) return 3; // LITERAL
        return 1; // IDENT
    }

    private static IEnumerable<string> TokenizeSql(string sql)
    {
        int i = 0;
        while (i < sql.Length)
        {
            if (char.IsWhiteSpace(sql[i])) { i++; continue; }
            // Строковый литерал
            if (sql[i] == '\'' || sql[i] == '"')
            {
                char q = sql[i]; int start = i++;
                while (i < sql.Length && sql[i] != q) i++;
                yield return sql[start..(i + 1 <= sql.Length ? ++i : i)];
                continue;
            }
            // Оператор из двух символов
            if (i + 1 < sql.Length)
            {
                string two = sql[i..(i + 2)];
                if (two is ">=" or "<=" or "<>" or "!=") { yield return two; i += 2; continue; }
            }
            // Пунктуация
            if (sql[i] is '(' or ')' or ',' or ';' or '=' or '>' or '<' or '+' or '-' or '*' or '/')
                { yield return sql[i].ToString(); i++; continue; }
            // Слово / идентификатор
            int ws = i;
            while (i < sql.Length && !char.IsWhiteSpace(sql[i]) &&
                   sql[i] is not '(' and not ')' and not ',' and not ';' and
                             not '=' and not '>' and not '<')
                i++;
            if (i > ws) yield return sql[ws..i];
        }
    }
}
```

### `35.ExperimentsV2/Infrastructure/JsonRoleTagger.cs`

```csharp
using System.Text.Json;

namespace ExperimentsV2;

// JsonRoleTagger использует System.Text.Json.Utf8JsonReader для разбора JSON
// и назначает каждому токену структурную роль.
public static class JsonRoleTagger
{
    public static readonly string[] RoleNames =
    [
        "OBJ_START","OBJ_END","ARR_START","ARR_END","KEY","STR_VALUE","NUM_VALUE","BOOL_VALUE","NULL_VALUE"
    ];

    // Разобрать JSON текст и вернуть список идентификаторов ролей.
    // При ошибке парсинга возвращает пустой список.
    public static List<int> TagToIds(string json)
    {
        var result = new List<int>();
        try
        {
            var reader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(json));
            bool nextIsKey = false;
            while (reader.Read())
            {
                int role = reader.TokenType switch
                {
                    JsonTokenType.StartObject => 0,  // OBJ_START
                    JsonTokenType.EndObject   => 1,  // OBJ_END
                    JsonTokenType.StartArray  => 2,  // ARR_START
                    JsonTokenType.EndArray    => 3,  // ARR_END
                    JsonTokenType.PropertyName => 4, // KEY
                    JsonTokenType.String when nextIsKey => 4, // KEY (в array of objects)
                    JsonTokenType.String      => 5,  // STR_VALUE
                    JsonTokenType.Number      => 6,  // NUM_VALUE
                    JsonTokenType.True        => 7,  // BOOL_VALUE
                    JsonTokenType.False       => 7,  // BOOL_VALUE
                    JsonTokenType.Null        => 8,  // NULL_VALUE
                    _                         => -1
                };
                if (role >= 0) result.Add(role);
            }
        }
        catch { /* невалидный JSON — вернуть что успели */ }
        return result;
    }
}
```

---

## Шаг 2 — Создать генераторы датасетов

### `35.ExperimentsV2/Infrastructure/NonCodeDatasetLoader.cs`

```csharp
namespace ExperimentsV2;

// NonCodeDatasetLoader загружает или генерирует три не-кодовых датасета.
// Все три работают без интернета (SQL и JSON генерируются, текст скачивается один раз).
public static class NonCodeDatasetLoader
{
    // ── Английский текст ─────────────────────────────────────────────────────

    // Скачать Moby Dick с Project Gutenberg и сохранить локально.
    // Возвращает первые maxWords слов.
    public static OperationResult<string> LoadEnglishText(
        string cacheDir, int maxWords = 50_000)
    {
        Directory.CreateDirectory(cacheDir);
        string cachePath = Path.Combine(cacheDir, "moby-dick.txt");

        if (!File.Exists(cachePath))
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                string text = client.GetStringAsync(
                    "https://www.gutenberg.org/files/2701/2701-0.txt").GetAwaiter().GetResult();
                File.WriteAllText(cachePath, text, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Fail(
                    $"Не удалось скачать Moby Dick: {ex.Message}. " +
                    $"Скачайте вручную: curl -o \"{cachePath}\" " +
                    "https://www.gutenberg.org/files/2701/2701-0.txt");
            }
        }

        string raw = File.ReadAllText(cachePath, System.Text.Encoding.UTF8);
        // Убрать заголовок Gutenberg (до строки "CHAPTER 1")
        int chapterStart = raw.IndexOf("CHAPTER 1", StringComparison.OrdinalIgnoreCase);
        if (chapterStart > 0) raw = raw[chapterStart..];

        // Взять первые maxWords слов
        var words = raw.Split(new[] { ' ', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries);
        string trimmed = string.Join(" ", words.Take(maxWords));

        return OperationResult<string>.Ok(trimmed);
    }

    // ── SQL запросы ──────────────────────────────────────────────────────────

    // Сгенерировать коллекцию реалистичных SQL запросов программно.
    // Не нужен интернет — всё генерируется в памяти.
    public static List<string> GenerateSqlQueries(int count = 500)
    {
        var tables  = new[] { "users","orders","products","categories","invoices",
                               "payments","sessions","logs","events","reports" };
        var columns = new[] { "id","name","email","status","created_at","updated_at",
                               "user_id","order_id","amount","price","quantity","type" };
        var rng = new Random(42);
        var result = new List<string>(count);

        string T() => tables[rng.Next(tables.Length)];
        string C() => columns[rng.Next(columns.Length)];
        int N()    => rng.Next(1, 1000);

        var patterns = new Func<string>[]
        {
            () => $"SELECT {C()}, {C()} FROM {T()} WHERE {C()} = {N()}",
            () => $"SELECT COUNT(*) FROM {T()} WHERE {C()} > {N()} AND {C()} IS NOT NULL",
            () => $"SELECT t1.{C()}, t2.{C()} FROM {T()} t1 JOIN {T()} t2 ON t1.id = t2.{C()}",
            () => $"INSERT INTO {T()} ({C()}, {C()}, {C()}) VALUES ({N()}, '{C()}', {N()})",
            () => $"UPDATE {T()} SET {C()} = {N()}, {C()} = '{C()}' WHERE id = {N()}",
            () => $"DELETE FROM {T()} WHERE {C()} < {N()} AND {C()} IS NULL",
            () => $"SELECT {C()}, SUM({C()}) FROM {T()} GROUP BY {C()} HAVING COUNT(*) > {N()}",
            () => $"SELECT DISTINCT {C()} FROM {T()} ORDER BY {C()} LIMIT {N()}",
            () => $"SELECT {C()} FROM {T()} WHERE {C()} BETWEEN {N()} AND {N()}",
            () => $"SELECT {C()} FROM {T()} WHERE {C()} IN (SELECT {C()} FROM {T()} WHERE {C()} = {N()})",
        };

        for (int i = 0; i < count; i++)
            result.Add(patterns[rng.Next(patterns.Length)]());

        return result;
    }

    // ── JSON документы ───────────────────────────────────────────────────────

    // Сгенерировать коллекцию JSON документов программно.
    // Имитирует типичные API-ответы.
    public static List<string> GenerateJsonDocuments(int count = 200)
    {
        var rng = new Random(42);
        var result = new List<string>(count);

        string[] statuses = ["active","inactive","pending","completed","failed"];
        string[] types    = ["user","order","product","event","log"];

        for (int i = 0; i < count; i++)
        {
            string type = types[rng.Next(types.Length)];
            string doc = type switch
            {
                "user" => $$$"""
                    {{"id":{{{rng.Next(1,10000)}}},"name":"User{{{rng.Next(1000)}}}",
                     "email":"user{{{rng.Next(1000)}}}@example.com",
                     "status":"{{{statuses[rng.Next(statuses.Length)]}}}",
                     "roles":["{{{types[rng.Next(types.Length)]}}}"],
                     "profile":{{"age":{{{rng.Next(18,80)}}},"active":{{{(rng.Next(2)==0?"true":"false")}}}}},
                     "created_at":"{{{DateTime.UtcNow.AddDays(-rng.Next(365)):yyyy-MM-dd}}}"}}
                    """,
                "order" => $$$"""
                    {{"order_id":{{{rng.Next(1,99999)}}},"user_id":{{{rng.Next(1,9999)}}},
                     "items":[{{"product_id":{{{rng.Next(1,999)}}},"qty":{{{rng.Next(1,10)}}},"price":{{{rng.Next(10,500)}}}}},
                              {{"product_id":{{{rng.Next(1,999)}}},"qty":{{{rng.Next(1,10)}}},"price":{{{rng.Next(10,500)}}}}}}],
                     "total":{{{rng.Next(20,5000)}}},"status":"{{{statuses[rng.Next(statuses.Length)]}}}",
                     "paid":false,"metadata":{{"source":"web","version":2}}}}
                    """,
                _ => $$$"""
                    {{"id":{{{rng.Next(1,99999)}}},"type":"{{{type}}}",
                     "payload":{{"key{{{rng.Next(10)}}}":{{{rng.Next(1000)}}},"flag":{{{(rng.Next(2)==0?"true":"false")}}}}},
                     "tags":["{{{type}}}","{{{statuses[rng.Next(statuses.Length)]}}}"],
                     "count":{{{rng.Next(0,100)}}}}}
                    """
            };
            result.Add(doc.Replace("\r","").Replace("\n","").Trim());
        }
        return result;
    }
}
```

---

## Шаг 3 — Создать E08NonCodeSyntax.cs

**Путь:** `35.ExperimentsV2/Experiments/E08NonCodeSyntax.cs`

```csharp
using Tokenizer;

namespace ExperimentsV2;

// E08: проверяет работает ли структурное представление на не-кодовых данных.
// Три под-эксперимента — текст, SQL, JSON.
// Baseline: BPE/word токены (плоский текст)
// Enhanced: структурные роли (DET/VERB/KEYWORD/OBJ_START и т.д.)
// Если enhanced лучше baseline — принцип работает за пределами C# кода.
public sealed class E08NonCodeSyntax
{
    private readonly string _cacheDir;

    // cacheDir — папка где кешировать скачанный Moby Dick
    public E08NonCodeSyntax(string cacheDir) => _cacheDir = cacheDir;

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        int sampleSize = tier switch { 1 => 5_000, 2 => 50_000, _ => 200_000 };

        // ── E08a: английский текст ────────────────────────────────────────────
        var textResult = LoadEnglishText(cacheDir, sampleSize);
        var (textBaseDelta, textEnhDelta, textVocab, textErr) = textResult.IsSuccess
            ? RunSubExperiment(
                baselineTokens: BpeTokenize(textResult.Value!, 512),
                enhancedTokens: EnglishRoleTagger.TagToIds(textResult.Value!),
                label: "text")
            : (0, 0, 0, textResult.ErrorMessage);

        // ── E08b: SQL ─────────────────────────────────────────────────────────
        var sqlQueries = NonCodeDatasetLoader.GenerateSqlQueries(
            tier == 1 ? 500 : tier == 2 ? 2000 : 10_000);
        string sqlCombined = string.Join(" ", sqlQueries);
        var (sqlBaseDelta, sqlEnhDelta, sqlVocab, sqlErr) = RunSubExperiment(
            baselineTokens: BpeTokenize(sqlCombined, 256),
            enhancedTokens: SqlRoleTagger.TagToIds(sqlCombined),
            label: "sql");

        // ── E08c: JSON ────────────────────────────────────────────────────────
        var jsonDocs = NonCodeDatasetLoader.GenerateJsonDocuments(
            tier == 1 ? 200 : tier == 2 ? 1000 : 5_000);
        string jsonCombined = string.Join(" ", jsonDocs);
        var (jsonBaseDelta, jsonEnhDelta, jsonVocab, jsonErr) = RunSubExperiment(
            baselineTokens: BpeTokenize(jsonCombined, 256),
            enhancedTokens: JsonRoleTagger.TagToIds(jsonCombined),
            label: "json");

        // Общий результат — среднее улучшение по трём под-экспериментам.
        // Только те где нет ошибки.
        var validDeltas = new List<double>();
        if (textErr == null) validDeltas.Add(textEnhDelta - textBaseDelta);
        if (sqlErr  == null) validDeltas.Add(sqlEnhDelta  - sqlBaseDelta);
        if (jsonErr == null) validDeltas.Add(jsonEnhDelta  - jsonBaseDelta);

        if (validDeltas.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Все три под-эксперимента провалились. Text: {textErr}, SQL: {sqlErr}, JSON: {jsonErr}");

        double avgBaseline = (textBaseDelta + sqlBaseDelta + jsonBaseDelta) / validDeltas.Count;
        double avgEnhanced = (textEnhDelta  + sqlEnhDelta  + jsonEnhDelta)  / validDeltas.Count;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E08-NON-CODE-SYNTAX",
            Tier              = tier,
            BaselinePrimary   = avgBaseline,
            EnhancedPrimary   = avgEnhanced,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = validDeltas.Count,
            ScalingSignalNote = "valid_sub_experiments (из 3)",
            ShouldScaleUp     = validDeltas.Average() < 5.0 && tier < 3,
            Notes             = $"text: base={textBaseDelta:F1}% enh={textEnhDelta:F1}% " +
                                $"vocab={textVocab}" +
                                (textErr != null ? $" ERR={textErr}" : "") +
                                $" | sql: base={sqlBaseDelta:F1}% enh={sqlEnhDelta:F1}% vocab={sqlVocab}" +
                                $" | json: base={jsonBaseDelta:F1}% enh={jsonEnhDelta:F1}% vocab={jsonVocab}"
        });
    }

    // Запустить один под-эксперимент: обучить bigram на обоих представлениях,
    // сравнить точность предсказания на validation.
    private static (double baseAcc, double enhAcc, int enhVocab, string? error)
        RunSubExperiment(List<int> baselineTokens, List<int> enhancedTokens, string label)
    {
        if (baselineTokens.Count < 20 || enhancedTokens.Count < 20)
            return (0, 0, 0, $"{label}: слишком мало токенов");

        int bSplit = (int)(baselineTokens.Count * 0.8);
        var bTrain = baselineTokens.Take(bSplit).ToList();
        var bVal   = baselineTokens.Skip(bSplit).ToList();

        var baseBigram = new BigramModelV2();
        baseBigram.Train(bTrain);
        var baseEval = baseBigram.Evaluate(bVal);
        if (!baseEval.IsSuccess) return (0, 0, 0, $"{label} baseline: {baseEval.ErrorMessage}");
        (double baseAcc, _, _, _) = baseEval.Value!;

        int eSplit = (int)(enhancedTokens.Count * 0.8);
        var eTrain = enhancedTokens.Take(eSplit).ToList();
        var eVal   = enhancedTokens.Skip(eSplit).ToList();

        var enhBigram = new BigramModelV2();
        enhBigram.Train(eTrain);
        var enhEval = enhBigram.Evaluate(eVal);
        if (!enhEval.IsSuccess) return (0, 0, 0, $"{label} enhanced: {enhEval.ErrorMessage}");
        (double enhAcc, _, _, _) = enhEval.Value!;

        int vocab = enhancedTokens.Distinct().Count();
        return (baseAcc * 100, enhAcc * 100, vocab, null);
    }

    private static OperationResult<string> LoadEnglishText(string cacheDir, int maxWords)
        => NonCodeDatasetLoader.LoadEnglishText(cacheDir, maxWords);

    private static List<int> BpeTokenize(string text, int vocabSize)
    {
        var tok = new BpeTokenizer();
        tok.Train(text, vocabSize);
        return tok.Encode(text);
    }
}
```

---

## Шаг 4 — Добавить в Dispatcher и Runner

### В `ExperimentsV2Dispatcher.cs`:

```csharp
public OperationResult<ExperimentV2Result> RunE08(string cacheDir, int tier = 1)
    => new E08NonCodeSyntax(cacheDir).Run(tier);
```

### В `ExperimentRunner.cs`:

**1. В массив `ids` — добавить `"E08"` первым:**
```csharp
// БЫЛО:
string[] ids = ["E01","E02","E03","E04","E05","E06","E07","E09","E10","E11","E12"];

// СТАЛО:
string[] ids = ["E01","E02","E03","E04","E05","E06","E07","E08","E09","E10","E11","E12"];
```

**2. В switch добавить:**
```csharp
"E08" => _d.RunE08(Path.Combine(_projectRoot, "text-datasets"), tier),
```

### В `ResultWriter.FolderName()`:
```csharp
"E08-NON-CODE-SYNTAX" => "E08-NON-CODE-SYNTAX",
```

---

## Шаг 5 — Сборка и запуск

**Сборка:**
```
dotnet build 35.ExperimentsV2/35.ExperimentsV2.csproj
```
Ожидаем: `Build succeeded. 0 Error(s)`

**Запуск:**
```csharp
runner.Run("E08", tier: 1);
```

**Ожидаемый вывод:**
```
[E08] Tier 1 загрузка... 3 файлов, 6 468 симв.
[E08] Запуск...
  Скачивание Moby Dick... сохранено в .../text-datasets/moby-dick.txt
OK top1_accuracy (up) baseline=0.312 enhanced=0.581 delta=+86.2% | PASSED
  -> .../E08-NON-CODE-SYNTAX/results/run-YYYY-MM-DD-001.json
```

**Ожидаемые числа в notes:**
```
text: base=18.3% enh=44.1% vocab=12 |
sql:  base=31.2% enh=82.5% vocab=5  |
json: base=24.8% enh=91.3% vocab=9
```

**Если Moby Dick не скачивается** (нет интернета):
```
curl -o C:\Projects\llm_mvp_core\text-datasets\moby-dick.txt ^
  https://www.gutenberg.org/files/2701/2701-0.txt
```
Файл сохраняется один раз, при следующих запусках берётся из кеша.

---

## Что означают результаты

| Тип данных | Vocab baseline | Vocab enhanced | Если enhanced >> baseline |
|---|---|---|---|
| Английский текст | 512 BPE подслов | 12 ролей | Принцип работает на мягкой грамматике |
| SQL | 256 BPE | 5 ролей | Работает на жёсткой нечеловеческой грамматике |
| JSON | 256 BPE | 9 ролей | Работает на чистой структуре без грамматики языка |

**Если все три показывают улучшение** — принцип фундаментальный, применим к любым
последовательным данным с иерархической структурой. Это прямой аргумент для проектов 21-23.

**Если только SQL и JSON улучшаются, а текст нет** — принцип работает для формальных
грамматик, но не для естественного языка. Грубый теггер на суффиксах недостаточен
для текста — нужен более точный (SpaCy, Catalyst).

**Если только текст не улучшается заметно** — добавить словарь в 500 самых частых
английских слов с метками и запустить снова.

---

*E08 план v2.0 — без Python, три датасета · не касается XIIGen*
