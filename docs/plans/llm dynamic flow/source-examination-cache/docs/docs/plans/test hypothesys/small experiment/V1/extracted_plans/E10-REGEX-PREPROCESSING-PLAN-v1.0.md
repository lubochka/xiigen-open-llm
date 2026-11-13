# E10 — Регекс как быстрый фильтр перед Roslyn
## Experiment Plan v1.0 · LlmFromScratch.sln

**Вопрос:** насколько регекс-нормализация перед Roslyn ускоряет
подготовку данных без потери качества токенизации?

**Этот документ не касается XIIGen.**

---

## Гипотеза

Roslyn разбирает каждый файл полностью — включая комментарии,
строковые литералы, XML-документацию, пустые строки.
Значительная часть этого — шум который не несёт информации
о структуре программы.

Если убрать шум регексом до Roslyn:
- Roslyn получает меньше символов → работает быстрее
- AST становится чище (меньше StringLiteralExpression узлов)
- Словарь AST типов насыщается быстрее (меньше мусорных узлов)

**Ограничение:** регекс не понимает вложенность. Нельзя безопасно
убирать блоки через регекс — только строки, комментарии, whitespace.

---

## Что убираем регексом (безопасно)

```csharp
// RegexPreprocessor.cs — быстрый предфильтр перед Roslyn.
// Применяется к исходному тексту ДО передачи в CSharpSyntaxTree.ParseText().
public static class RegexPreprocessor
{
    // Шаг 1: удалить однострочные комментарии // ...
    // Безопасно: // внутри строк не будет после шага 2
    private static readonly Regex LineComments = new(@"//[^\n]*", RegexOptions.Compiled);

    // Шаг 2: заменить строковые литералы на заглушку ""
    // "hello world" → "" (содержимое строки не несёт структурной информации)
    // Важно: verbatim strings (@"...") тоже
    private static readonly Regex StringLiterals = new(
        @"""(?:[^""\\]|\\.)*""|@""(?:[^""]|"""")*""",
        RegexOptions.Compiled);

    // Шаг 3: заменить блочные комментарии /* ... */ на один пробел
    private static readonly Regex BlockComments = new(
        @"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    // Шаг 4: убрать XML-документацию /// ...
    private static readonly Regex XmlDocs = new(@"///[^\n]*", RegexOptions.Compiled);

    // Шаг 5: нормализовать пустые строки (> 2 подряд → 1)
    private static readonly Regex BlankLines = new(@"\n\s*\n\s*\n", RegexOptions.Compiled);

    public static string Normalize(string source)
    {
        string s = XmlDocs.Replace(source, "");
        s = BlockComments.Replace(s, " ");
        s = LineComments.Replace(s, "");
        s = StringLiterals.Replace(s, "\"\"");
        s = BlankLines.Replace(s, "\n\n");
        return s;
    }
}
```

**Что НЕ трогаем регексом:**
- Фигурные скобки `{}` — вложенность нельзя парсить регексом
- Круглые скобки `()` — то же
- Содержимое методов — Roslyn справляется
- `#if` директивы — слишком сложная логика

---

## Метрика эксперимента

Не точность предсказания (она не должна измениться) — а **время обработки**.

```
primary_metric = "preparation_time_ms"
lower_is_better = true

Baseline: Roslyn.ParseText(rawSource)
Enhanced: Roslyn.ParseText(RegexPreprocessor.Normalize(rawSource))

Ожидаем:
  Время Roslyn: -20 до -40% (меньше символов для парсинга)
  Размер AST: -10 до -25% (меньше StringLiteral и Comment узлов)
  Точность предсказания: ±0% (структура кода не изменилась)
```

Вторичная метрика: размер AST (количество узлов) и
`ast_saturation_ratio` — должен насыщаться быстрее (меньше шумовых типов).

---

## Тест на корректность (обязательно)

После нормализации код должен компилироваться:

```csharp
// CorrectnessTest: нормализованный код должен успешно парситься Roslyn
// (компиляция не нужна, только синтаксический разбор без ошибок)
public static bool IsValid(string normalizedSource)
{
    var tree = CSharpSyntaxTree.ParseText(normalizedSource);
    return !tree.GetDiagnostics()
                .Any(d => d.Severity == DiagnosticSeverity.Error);
}
```

Запустить на всех файлах датасета. Если ошибки — найти проблемный паттерн
и добавить исключение в `RegexPreprocessor`.

---

## Ожидаемое влияние на другие эксперименты

| Эксперимент | Влияние |
|---|---|
| E06 AST токены | AST словарь чище, возможно чуть быстрее насыщается |
| E09 структурный вектор | Меньше шумовых узлов в пути через дерево |
| E08 текст (обычный язык) | Не применимо — это C# препроцессор |
| Нейросеть (следующий этап) | Меньше токенов для обучения = быстрее каждый шаг |

---

## Порядок выполнения

```
1. Написать RegexPreprocessor.cs с методом Normalize()
2. Написать тест на корректность на Scale 1 датасете
   → убедиться что 0 синтаксических ошибок после нормализации
3. Написать E10RegexPrep.cs — замеряет время Roslyn с нормализацией и без
4. runner.Run("E10", scale: 1)  ← Scale 1 достаточен, нет смысла в Scale 0
5. Если время сократилось ≥ 10%:
   → Интегрировать Normalize() в DatasetLoader перед передачей в E06/E09
   → Запустить E06 и E09 снова, сравнить результаты
```

**Критерий "прошёл":** `preparation_time_ms` снизилось ≥ 10%
при нулевых синтаксических ошибках в нормализованных файлах.

---

*E10 plan v1.0 — regex preprocessing experiment · не касается XIIGen*
