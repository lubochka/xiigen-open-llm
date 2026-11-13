# Тесты для редактирования кода: нечёткое совпадение и мультипатч
## Plan v1.0

**Проект:** `LlmFromScratch.sln` — `Tests/` и `21.AgentInfrastructure/Editing/`
**Этот документ не касается XIIGen.**

---

## Что тестируем и зачем

Из исследования Aider: **без нечёткого совпадения (fuzzy match) число ошибок
применения патча вырастает в 9 раз.** LLM часто возвращает search-текст с
незначительными различиями — лишний пробел, другой перенос строки, сдвиг
отступов. Текущий `FileChangeApplier` использует `StringComparison.Ordinal`
и в таких случаях возвращает `PATCH_NOT_FOUND` вместо успешного применения.

**Два улучшения — два набора тестов:**

| Улучшение | Тесты | Статус до |
|---|---|---|
| Нечёткое совпадение (whitespace normalization) | 8 тестов | FAIL — не реализовано |
| Множественные патчи (ApplyManyPatchesAsync) | 6 тестов | FAIL — метода нет |
| False-CONJ в ComplexTaskDecomposer | 4 теста | FAIL — не защищено |

TDD-порядок: написать тесты → увидеть FAIL → реализовать → PASS.

---

## Часть 1: Нечёткое совпадение в FileChangeApplier

### Новый интерфейс

```csharp
// Добавить в FileChangeApplier.cs рядом с ApplyPatchAsync:
public async Task<OperationResult<bool>> ApplyPatchFuzzyAsync(
    string filePath,
    string searchText,
    string replacementText)
```

**Алгоритм (два прохода):**
```
Проход 1: точное совпадение (Ordinal) — без изменений
Проход 2 (если Проход 1 → PATCH_NOT_FOUND):
  1. Нормализовать searchText: убрать trailing spaces, унифицировать CRLF→LF
  2. Нормализовать content аналогично
  3. Искать нормализованный searchText в нормализованном content
  4. Если нашли → применить replacement к ОРИГИНАЛЬНОМУ content
     (замена находит правильный диапазон через mapping нормализованных позиций)
  5. Если не нашли → PATCH_NOT_FOUND (без изменений)
```

Важно: применение идёт к **оригинальному** content — не нормализованному.
Нормализация только для поиска, не для результата.

### Тесты (добавить в AgentInfrastructureTests.cs)

```csharp
// ── FileChangeApplier — Fuzzy matching ───────────────────────────────────────

[Fact]
public async Task FileChangeApplier_TrailingSpaceInSearch_MatchesFuzzy()
{
    // LLM вернул search-текст с лишним пробелом в конце
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "int x = 1;\nint y = 2;"
    });
    var writer = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    // Точное совпадение провалится — есть лишний пробел
    var exactResult = await applier.ApplyPatchAsync("src/foo.cs", "int x = 1; ", "int x = 42;");
    Assert.Equal("PATCH_NOT_FOUND", exactResult.ErrorCode);

    // Нечёткое совпадение должно сработать
    var fuzzyResult = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "int x = 1; ", "int x = 42;");
    Assert.True(fuzzyResult.IsSuccess);
    Assert.Equal("int x = 42;\nint y = 2;", writer.Written["src/foo.cs"]);
}

[Fact]
public async Task FileChangeApplier_CrlfVsLf_MatchesFuzzy()
{
    // Файл с LF, LLM вернул CRLF в search-тексте
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "void M()\n{\n    return;\n}"
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    string searchWithCrlf  = "void M()\r\n{\r\n    return;\r\n}";
    string replacement     = "void M()\n{\n    return 42;\n}";

    var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", searchWithCrlf, replacement);
    Assert.True(result.IsSuccess);
    Assert.Contains("return 42;", writer.Written["src/foo.cs"]);
}

[Fact]
public async Task FileChangeApplier_IndentationMismatch_MatchesFuzzy()
{
    // Файл с 4 пробелами, LLM вернул 2 пробела в search-тексте
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "class C\n{\n    void M()\n    {\n        return;\n    }\n}"
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    // LLM использовал 2 пробела
    string searchWith2 = "    void M()\n    {\n        return;\n    }";
    string replWith2   = "    void M()\n    {\n        return 42;\n    }";

    // Точное совпадение — ОК (они одинаковые в данном случае)
    // Тест специфичен: search содержит правильный контекст но с лишними пробелами в конце строк
    string searchTrail = "    void M()  \n    {  \n        return;  \n    }  ";
    var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", searchTrail, replWith2);
    Assert.True(result.IsSuccess);
    Assert.Contains("return 42;", writer.Written["src/foo.cs"]);
}

[Fact]
public async Task FileChangeApplier_FuzzyAmbiguous_ReturnsAmbiguous()
{
    // Нечёткое совпадение должно также проверять на множественность
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "int x = 1;\nint x = 1;"
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    // Лишний пробел + два совпадения → всё равно AMBIGUOUS
    var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "int x = 1; ", "int x = 42;");
    Assert.Equal("PATCH_AMBIGUOUS", result.ErrorCode);
}

[Fact]
public async Task FileChangeApplier_FuzzyExactFirst_ExactWins()
{
    // Если точное совпадение работает — fuzzy не должен ничего менять
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "int x = 1;"
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    var result = await applier.ApplyPatchFuzzyAsync("src/foo.cs", "int x = 1;", "int x = 42;");
    Assert.True(result.IsSuccess);
    Assert.Equal("int x = 42;", writer.Written["src/foo.cs"]);
}
```

### Реализация ApplyPatchFuzzyAsync (добавить в FileChangeApplier.cs)

```csharp
// Нечёткое применение патча: сначала точное совпадение,
// потом поиск с нормализацией пробелов если точное не сработало.
public async Task<OperationResult<bool>> ApplyPatchFuzzyAsync(
    string filePath,
    string searchText,
    string replacementText)
{
    // Шаг 1: попробовать точное совпадение
    var exactResult = await ApplyPatchAsync(filePath, searchText, replacementText);
    if (exactResult.IsSuccess || exactResult.ErrorCode != "PATCH_NOT_FOUND")
        return exactResult;

    // Шаг 2: нормализовать и попробовать снова
    var readResult = await _reader.ReadFileAsync(filePath);
    if (!readResult.IsSuccess) return exactResult; // вернуть исходную ошибку

    string content       = readResult.Value!;
    string normalContent = NormalizeWhitespace(content);
    string normalSearch  = NormalizeWhitespace(searchText);

    int occurrences = CountOccurrences(normalContent, normalSearch);

    if (occurrences == 0)
        return OperationResult<bool>.Fail(
            $"Search text not found in '{filePath}' (fuzzy match also failed).",
            "PATCH_NOT_FOUND");

    if (occurrences > 1)
        return OperationResult<bool>.Fail(
            $"Search text appears {occurrences} times (fuzzy). Make the patch more specific.",
            "PATCH_AMBIGUOUS");

    // Найти позицию в нормализованном тексте и применить к оригинальному
    // Используем mapping нормализованных позиций к оригинальным
    int normIdx  = normalContent.IndexOf(normalSearch, StringComparison.Ordinal);
    int origStart = MapNormalizedToOriginal(content, normalContent, normIdx);
    int origEnd   = MapNormalizedToOriginal(content, normalContent,
                                             normIdx + normalSearch.Length);

    if (origStart < 0 || origEnd < 0 || origEnd > content.Length)
        return OperationResult<bool>.Fail("Fuzzy position mapping failed.", "PATCH_NOT_FOUND");

    string patched = content[..origStart] + replacementText + content[origEnd..];
    return await _writer.WriteFileAsync(filePath, patched);
}

// Нормализация: убрать trailing whitespace с каждой строки, CRLF→LF
private static string NormalizeWhitespace(string text)
{
    var lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
    return string.Join("\n", lines.Select(l => l.TrimEnd()));
}

// Маппинг позиции в нормализованной строке обратно в оригинальную
private static int MapNormalizedToOriginal(
    string original, string normalized, int normalizedPos)
{
    if (normalizedPos <= 0) return 0;
    if (normalizedPos >= normalized.Length) return original.Length;

    // Идти по обеим строкам одновременно, пропуская нормализованные символы
    int o = 0, n = 0;
    while (n < normalizedPos && o < original.Length)
    {
        char oc = original[o];
        char nc = n < normalized.Length ? normalized[n] : '\0';
        if (oc == nc) { o++; n++; }
        else if (oc == '\r' && o + 1 < original.Length && original[o + 1] == '\n')
            o++; // пропустить \r из CRLF
        else if (oc == ' ' || oc == '\t')
        {
            // Trailing whitespace пропускается в нормализации
            // Если следующий символ в normalized — \n, то этот пробел был trailing
            if (nc == '\n' || nc == '\0') o++;
            else { o++; n++; }
        }
        else { o++; n++; }
    }
    return o;
}
```

---

## Часть 2: Множественные патчи (ApplyManyPatchesAsync)

### Новый интерфейс

```csharp
// Применить несколько патчей за одну операцию.
// Если один патч провалится — остановиться и вернуть ошибку
// с индексом первого неудачного патча.
public record MultiPatchResult(
    int    AppliedCount,
    string? FirstFailedErrorCode,
    int     FirstFailedIndex);    // -1 если все прошли

public async Task<OperationResult<MultiPatchResult>> ApplyManyPatchesAsync(
    string filePath,
    IReadOnlyList<(string Search, string Replacement)> patches,
    bool useFuzzy = true)
```

**Семантика:**
- Патчи применяются по порядку к промежуточному состоянию файла
- Первый провал останавливает всё (атомарность: либо все, либо никто)
- `useFuzzy=true` включает нечёткое совпадение для каждого патча

### Тесты

```csharp
// ── FileChangeApplier — Multi-patch ──────────────────────────────────────────

[Fact]
public async Task FileChangeApplier_TwoPatches_BothApplied()
{
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "int x = 1;\nint y = 2;"
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    var patches = new (string, string)[]
    {
        ("int x = 1;", "int x = 42;"),
        ("int y = 2;", "int y = 99;"),
    };
    var result = await applier.ApplyManyPatchesAsync("src/foo.cs", patches);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value!.AppliedCount);
    Assert.Equal(-1, result.Value!.FirstFailedIndex);
    Assert.Equal("int x = 42;\nint y = 99;", writer.Written["src/foo.cs"]);
}

[Fact]
public async Task FileChangeApplier_ThreePatches_SecondFails_StopsAndReverts()
{
    // Если второй патч провалился — файл не должен быть изменён (атомарность)
    var original = "void A() {}\nvoid B() {}\nvoid C() {}";
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = original
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    var patches = new (string, string)[]
    {
        ("void A() {}", "void A() { return; }"),
        ("void NONEXISTENT() {}", "void X() {}"),  // провал
        ("void C() {}", "void C() { return; }"),
    };
    var result = await applier.ApplyManyPatchesAsync("src/foo.cs", patches);

    Assert.False(result.IsSuccess);
    Assert.Equal("PATCH_NOT_FOUND", result.Value!.FirstFailedErrorCode);
    Assert.Equal(1, result.Value!.FirstFailedIndex);
    // Файл не должен быть изменён — атомарность
    Assert.False(writer.Written.ContainsKey("src/foo.cs"));
}

[Fact]
public async Task FileChangeApplier_SequentialPatches_SecondUsesResultOfFirst()
{
    // Второй патч ищет текст который появился в результате первого патча
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "var x = GetValue();"
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    var patches = new (string, string)[]
    {
        ("var x = GetValue();", "var x = GetValue();\nif (x == null) return;"),
        ("if (x == null) return;", "if (x == null) throw new ArgumentNullException();"),
    };
    var result = await applier.ApplyManyPatchesAsync("src/foo.cs", patches);

    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value!.AppliedCount);
    Assert.Contains("ArgumentNullException", writer.Written["src/foo.cs"]);
}

[Fact]
public async Task FileChangeApplier_EmptyPatchList_SucceedsWithZero()
{
    var reader = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/foo.cs"] = "int x = 1;"
    });
    var writer  = new InMemoryCodeWriter();
    var applier = new FileChangeApplier(reader, writer);

    var result = await applier.ApplyManyPatchesAsync("src/foo.cs",
        Array.Empty<(string, string)>());

    Assert.True(result.IsSuccess);
    Assert.Equal(0, result.Value!.AppliedCount);
    // Файл не перезаписывался — нет изменений
    Assert.False(writer.Written.ContainsKey("src/foo.cs"));
}
```

### Реализация ApplyManyPatchesAsync (добавить в FileChangeApplier.cs)

```csharp
public async Task<OperationResult<MultiPatchResult>> ApplyManyPatchesAsync(
    string filePath,
    IReadOnlyList<(string Search, string Replacement)> patches,
    bool useFuzzy = true)
{
    if (patches.Count == 0)
        return OperationResult<MultiPatchResult>.Ok(
            new MultiPatchResult(0, null, -1));

    // Прочитать файл один раз
    var readResult = await _reader.ReadFileAsync(filePath);
    if (!readResult.IsSuccess)
        return OperationResult<MultiPatchResult>.Fail(
            readResult.ErrorMessage, readResult.ErrorCode);

    string current = readResult.Value!;

    for (int i = 0; i < patches.Count; i++)
    {
        var (search, replacement) = patches[i];

        // Применить патч к текущему состоянию в памяти
        string normal    = useFuzzy ? NormalizeWhitespace(current)  : current;
        string normSearch = useFuzzy ? NormalizeWhitespace(search)   : search;

        int occurrences = CountOccurrences(
            useFuzzy ? normal : current,
            useFuzzy ? normSearch : search);

        if (occurrences == 0)
            return OperationResult<MultiPatchResult>.Ok(
                new MultiPatchResult(i, "PATCH_NOT_FOUND", i));

        if (occurrences > 1)
            return OperationResult<MultiPatchResult>.Ok(
                new MultiPatchResult(i, "PATCH_AMBIGUOUS", i));

        // Применить
        if (useFuzzy)
        {
            int normIdx   = normal.IndexOf(normSearch, StringComparison.Ordinal);
            int origStart = MapNormalizedToOriginal(current, normal, normIdx);
            int origEnd   = MapNormalizedToOriginal(current, normal,
                                                     normIdx + normSearch.Length);
            if (origStart < 0 || origEnd < 0)
                return OperationResult<MultiPatchResult>.Ok(
                    new MultiPatchResult(i, "PATCH_NOT_FOUND", i));
            current = current[..origStart] + replacement + current[origEnd..];
        }
        else
        {
            current = current.Replace(search, replacement, StringComparison.Ordinal);
        }
    }

    // Все патчи прошли — записать файл один раз
    var writeResult = await _writer.WriteFileAsync(filePath, current);
    if (!writeResult.IsSuccess)
        return OperationResult<MultiPatchResult>.Fail(
            writeResult.ErrorMessage, writeResult.ErrorCode);

    return OperationResult<MultiPatchResult>.Ok(
        new MultiPatchResult(patches.Count, null, -1));
}
```

---

## Часть 3: False-CONJ в ComplexTaskDecomposer

Урок из E21: союз "and" внутри одной задачи не должен разбивать её.
"Find null and log them" — одна задача, не две.

### Тесты (добавить в AgentAlgorithmsTests.cs)

```csharp
// ── ComplexTaskDecomposer — False conjunction guard ───────────────────────────

[Fact]
public void ComplexTaskDecomposer_FalseConjunction_StaysAsSingleTask()
{
    // "and" внутри одной операции — НЕ разбивать
    var decomposer = new ComplexTaskDecomposer();

    var result = decomposer.Decompose("Find methods that return null and log them");
    Assert.True(result.IsSuccess);

    // Должен остаться ОДИН под-эпизод, не два
    Assert.Single(result.Value!);
}

[Fact]
public void ComplexTaskDecomposer_TrueConjunction_SplitsIntoTwo()
{
    // "and" между двумя независимыми операциями — разбивать
    var decomposer = new ComplexTaskDecomposer();

    var result = decomposer.Decompose("Add null check to ValidateToken and write unit tests for it");
    Assert.True(result.IsSuccess);

    // Должно быть ДВА под-эпизода
    Assert.Equal(2, result.Value!.Count);
}

[Fact]
public void ComplexTaskDecomposer_FalseConjunctionWithCheck_Preserved()
{
    // "and" в "check null and empty" — одна операция
    var decomposer = new ComplexTaskDecomposer();

    var result = decomposer.Decompose("Check null and empty before accessing the property");
    Assert.True(result.IsSuccess);
    Assert.Single(result.Value!);
}

[Fact]
public void ComplexTaskDecomposer_TrueConjunctionThenSplit_SplitsCorrectly()
{
    // "then" между независимыми операциями — разбивать
    var decomposer = new ComplexTaskDecomposer();

    var result = decomposer.Decompose("Fix the null reference exception then add a regression test");
    Assert.True(result.IsSuccess);
    Assert.Equal(2, result.Value!.Count);
    Assert.Equal("Fix the null reference exception", result.Value![0].Description);
}
```

### Изменение в ComplexTaskDecomposer.cs

Добавить проверку что обе части после split имеют самостоятельный глагол:

```csharp
// В SplitAtConjunctions: проверить что разбиение даёт независимые операции
private static bool IsIndependentOperation(string part)
{
    var lower = part.ToLowerInvariant().Trim();
    // Операция независима если начинается с глагола (imperative)
    var imperativeStarters = new[]
    {
        "add", "create", "write", "build", "implement", "fix", "refactor",
        "update", "remove", "delete", "check", "test", "verify", "review"
    };
    return imperativeStarters.Any(v => lower.StartsWith(v + " ") || lower == v);
}

// В Decompose: разбивать только если обе части независимы
private static List<string> SplitAtConjunctions(string text)
{
    // ... существующий код ...
    // После разбивки: убрать случаи где правая часть не независима
    var filtered = new List<string>();
    for (int i = 0; i < result.Count; i++)
    {
        if (i == 0 || IsIndependentOperation(result[i]))
            filtered.Add(result[i]);
        else
            // Присоединить к предыдущему (это была false conjunction)
            filtered[^1] += /* original conjunction */ " " + result[i];
    }
    return filtered;
}
```

---

## Порядок выполнения в Claude Code

```
1. Запустить существующие тесты — убедиться что 0 failures
   dotnet test Tests/LlmFromScratch.Tests.csproj

2. Добавить новые тесты в AgentInfrastructureTests.cs (Fuzzy + MultiPatch)
   Добавить новые тесты в AgentAlgorithmsTests.cs (FalseConj)
   → Ожидать FAIL на новых тестах (TDD: red phase)

3. Реализовать ApplyPatchFuzzyAsync в FileChangeApplier.cs
   Реализовать MultiPatchResult record + ApplyManyPatchesAsync
   → Запустить тесты → PASS на fuzzy + multi-patch

4. Обновить ComplexTaskDecomposer.cs — добавить IsIndependentOperation
   → Запустить тесты → PASS на false-CONJ тестах

5. Проверить что ВСЕ старые тесты всё ещё PASS
   dotnet test Tests/LlmFromScratch.Tests.csproj → 0 failures
```

---

## Итоговый счёт тестов

| Файл | До | После | Новые |
|---|---|---|---|
| AgentInfrastructureTests.cs | 4 FileChangeApplier | 13 | +5 fuzzy +4 multi-patch |
| AgentAlgorithmsTests.cs | 4 ComplexTaskDecomposer | 8 | +4 false-CONJ |
| **Всего новых** | | | **13** |

---

*Тест-план v1.0 — FileChangeApplier + ComplexTaskDecomposer · не касается XIIGen*
