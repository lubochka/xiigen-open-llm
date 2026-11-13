# Алгоритмы автодополнения кода — Подпроекты 33 через 38
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Проект:** llm-mvp-core — подпроекты 33, 34, 35, 36, 37, 38
**Язык:** C# / .NET 9
**Внешние пакеты:** Microsoft.CodeAnalysis.CSharp (Roslyn — MIT, уже используется в подпроектах 13 и 23) · никаких дополнительных пакетов
**Цель документа:** Пошаговый план реализации шести уровней алгоритмов предложения следующего токена, как в расширениях VS Code
**Этот документ не касается XIIGen.**

---

## Что добавляется в Solution

| Номер | Уровень | Название | Ключевая идея |
|---|---|---|---|
| 33 | Уровень 1 | Статический анализ (Language Server Protocol стиль) | Перечислить все синтаксически допустимые токены в данной позиции через Roslyn |
| 34 | Уровень 2А | Ранжирование по частоте вызовов | Какие методы чаще всего вызывают у данного типа — по статистике GitHub |
| 35 | Уровень 2Б | N-граммная модель токенов | Какой токен чаще всего следует за данной парой токенов |
| 36 | Уровень 3 | BM25 поиск по локальному контексту | Найти похожие фрагменты в том же проекте и предложить оттуда |
| 37 | Уровень 4 | Трансформер-модель с фильтром допустимых | Нейросеть предсказывает вероятность каждого допустимого токена |
| 38 | Уровень 5 | PageRank на графе зависимостей AST | Важность символа определяется тем сколько мест его вызывает |

**Зачем это в Solution:** подпроекты 33–38 реализуют механизм автодополнения для агентов подпроектов 21–23. Когда агент генерирует код — он использует эти алгоритмы для выбора следующего токена. Каждый уровень можно использовать отдельно или в комбинации — как VS Code сначала получает кандидатов от LSP (33), потом ранжирует их нейросетью (37).

---

## Часть 1 — Девять правил кодирования (Genie DNA) для всех шести подпроектов

### Правило 1 — Данные передаются через записи с примитивными полями

```csharp
// ПРАВИЛЬНО: запись с примитивными полями
// CompletionRequest — запрос на автодополнение в данной позиции
public sealed record CompletionRequest
{
    // Весь исходный код файла как строка текста
    public required string SourceCode { get; init; }

    // Позиция курсора в файле: строка (начиная с 0)
    public required int CursorLine { get; init; }

    // Позиция курсора в файле: столбец (начиная с 0)
    public required int CursorColumn { get; init; }

    // Путь к файлу — нужен для BM25 поиска по соседним файлам
    public required string FilePath { get; init; }

    // Максимальное количество предложений которые нужно вернуть
    public int MaxSuggestions { get; init; } = 10;
}

// CompletionSuggestion — одно предложение автодополнения
public sealed record CompletionSuggestion
{
    // Текст который нужно вставить в позицию курсора
    public required string InsertText { get; init; }

    // Краткое описание для подсказки (тип метода, документация)
    public required string Description { get; init; }

    // Оценка релевантности от 0.0 до 1.0 (выше = лучше)
    public required double Score { get; init; }

    // Название алгоритма который предложил эту подсказку
    public required string SourceAlgorithm { get; init; }
}
```

### Правило 2 — Параметры поиска собираются отдельным методом

```csharp
// Параметры поиска по индексу собираются отдельно от самого поиска
private TokenSearchParameters BuildTokenSearchParameters(
    string typeName,
    int    maxResults,
    double minimumScore)
{
    // typeName      — имя типа для которого ищем методы
    // maxResults    — сколько кандидатов вернуть максимум
    // minimumScore  — порог ниже которого кандидаты отбрасываются
    return new TokenSearchParameters
    {
        TypeName     = typeName,
        MaxResults   = maxResults,
        MinimumScore = minimumScore,
    };
}
```

### Правила 3–9 — те же что и в плане v2.0

OperationResult<T> вместо исключений (правило 3), BaseService (правило 4), изоляция по области действия (правило 5), один диспетчер (правило 6), идемпотентность (правило 7), сохранение перед событием (правило 8), StandardEvent конверт (правило 9).

---

## Часть 2 — Общие fabric интерфейсы для всех шести подпроектов

```
33.CodeCompletion/
├── Fabric/
│   ├── ICompletionProvider.cs      — один метод: GetSuggestionsAsync(request)
│   ├── ITokenIndex.cs              — хранение и поиск по индексу токенов
│   ├── IUsageStatistics.cs         — частоты вызовов методов по типам
│   ├── ISourceCodeReader.cs        — чтение файлов проекта для BM25
│   ├── InMemoryTokenIndex.cs       — реализация для тестов
│   └── FileSystemSourceReader.cs  — реализация для продакшна
├── Core/
│   ├── CompletionRequest.cs        — запрос автодополнения
│   ├── CompletionSuggestion.cs     — одно предложение
│   ├── CompletionContext.cs        — разобранный контекст курсора
│   ├── OperationResult.cs
│   ├── BaseService.cs
│   └── StandardEvent.cs
├── Algorithms/
│   ├── 01-StaticAnalysis/          — подпроект 33
│   ├── 02-FrequencyRanking/        — подпроект 34
│   ├── 03-NGramModel/              — подпроект 35
│   ├── 04-BM25LocalContext/        — подпроект 36
│   ├── 05-TransformerFilter/       — подпроект 37
│   └── 06-PageRankAst/             — подпроект 38
├── Dispatcher/
│   └── CompletionDispatcher.cs     — правило DNA 6
└── Tests/
```

### Fabric интерфейс 1 — ICompletionProvider (поставщик подсказок)

```csharp
// ICompletionProvider — единый интерфейс для всех шести алгоритмов.
// Вызывающий код не знает какой алгоритм работает внутри.
// Конкретные реализации: StaticAnalysisProvider, FrequencyRankingProvider и т.д.
public interface ICompletionProvider
{
    // Получить список предложений для данной позиции в коде
    // request — запрос с исходным кодом и позицией курсора
    // Возвращает список предложений отсортированных по убыванию оценки
    Task<OperationResult<IReadOnlyList<CompletionSuggestion>>> GetSuggestionsAsync(
        CompletionRequest request);

    // Название этого алгоритма — используется в логах и метриках
    string AlgorithmName { get; }
}
```

### Fabric интерфейс 2 — IUsageStatistics (статистика использования)

```csharp
// IUsageStatistics хранит статистику: сколько раз каждый метод вызывался
// у данного типа по данным из GitHub репозиториев.
// Конкретные реализации: JsonFileStatistics (продакшн), InMemoryStatistics (тесты)
public interface IUsageStatistics
{
    // Получить частоту вызова метода у данного типа
    // typeName   — имя типа, например "List<string>"
    // methodName — имя метода, например "Add"
    // Возвращает число от 0.0 до 1.0: 0.38 = метод вызывается в 38% случаев
    Task<OperationResult<double>> GetMethodFrequencyAsync(string typeName, string methodName);

    // Получить топ-K самых частых методов для данного типа
    Task<OperationResult<IReadOnlyList<(string MethodName, double Frequency)>>>
        GetTopMethodsAsync(string typeName, int topK);
}
```

### Fabric интерфейс 3 — ISourceCodeReader (чтение исходников проекта)

```csharp
// ISourceCodeReader предоставляет доступ к файлам проекта для BM25 поиска.
// Алгоритм не знает откуда берутся файлы: файловая система, память, или архив.
public interface ISourceCodeReader
{
    // Прочитать содержимое файла по пути
    Task<OperationResult<string>> ReadFileAsync(string filePath);

    // Получить список всех файлов .cs в проекте
    Task<OperationResult<IReadOnlyList<string>>> ListSourceFilesAsync(string projectRoot);
}
```

---

## Часть 3 — Таблица сокращений

| Сокращение | Полное название | Простое объяснение |
|---|---|---|
| LSP | Language Server Protocol (протокол языкового сервера) | стандарт общения редактора кода с программой которая понимает синтаксис языка |
| AST | Abstract Syntax Tree (абстрактное синтаксическое дерево) | структура представляющая код как дерево: у метода есть имя, тип возврата, параметры — каждое в своём узле |
| BM25 | Best Match 25 (алгоритм ранжирования документов версии 25) | формула для оценки насколько документ релевантен поисковому запросу, используется в поисковиках |
| TF-IDF | Term Frequency — Inverse Document Frequency (частота термина — обратная частота документа) | слово важно если встречается часто в одном документе но редко во всех остальных |
| N-граммная модель | N-gram language model (модель языка на основе N-грамм) | считает как часто встречается каждая последовательность из N токенов |
| PageRank | PageRank алгоритм ранжирования (алгоритм Ларри Пейджа) | страница важна если на неё ссылаются важные страницы; метод → вызывается из важных мест |
| Gate A | Automated gate check (автоматическая проверка) | `dotnet build` без ошибок, все тесты зелёные |
| Gate B | Algorithmic correctness (проверка корректности) | результаты соответствуют эталонным примерам |
| Gate C | Human approval (одобрение человека) | явное «да» от Лубы |

---

## Подпроект 33 — Статический анализ (Language Server Protocol стиль)

### Что делает этот алгоритм простым языком

Компилятор (Roslyn) знает: переменная `myList` имеет тип `List<string>`. Значит после точки допустимы только методы `List<string>`: `Add`, `Remove`, `Count`, `Where` и т.д. Всё остальное — синтаксическая ошибка. Алгоритм перечисляет только допустимые варианты. Никакого предсказания, только знание синтаксиса.

Аналогия: кроссворд. В клетке 3 буквы, начинается на «К», заканчивается на «Т» — допустимы только «КОТ», «КИТ», «КАТ». Алгоритм перечисляет подходящие варианты, но не выбирает лучший.

### Открытый источник для изучения

```
Roslyn (Microsoft) — Apache 2.0 — C#
https://github.com/dotnet/roslyn

Ключевые API которые мы используем:
  CSharpSyntaxTree.ParseText(sourceCode)
      → разбирает строку кода в синтаксическое дерево

  semanticModel.GetSymbolInfo(expressionNode)
      → возвращает тип выражения слева от точки

  semanticModel.GetMemberGroup(expressionNode)
      → возвращает список всех допустимых членов типа

  semanticModel.LookupSymbols(position)
      → все переменные и методы видимые в данной позиции
```

### Папка в Solution

```
33.CodeCompletion/Algorithms/01-StaticAnalysis/
├── RoslynContextAnalyzer.cs   — разбор кода и определение позиции курсора
├── TypeMemberEnumerator.cs    — перечисление членов типа через Roslyn
├── ScopeSymbolCollector.cs    — сбор символов из области видимости
├── StaticAnalysisProvider.cs — реализует ICompletionProvider
└── Tests/StaticAnalysisTests.cs
```

### Фаза 1 — Разбор контекста курсора (2 дня)

```csharp
// RoslynContextAnalyzer.cs
// Определяет что именно стоит перед курсором и какого типа выражение.
// Это фундаментальный компонент — остальные алгоритмы 34–38 используют его.
// Источник: Roslyn Language Server протокол, OmniSharp реализация
public sealed class RoslynContextAnalyzer
{
    // Разобрать контекст: что написано перед курсором и какой это тип
    // sourceCode   — полный текст файла C#
    // cursorLine   — строка курсора (начиная с 0)
    // cursorColumn — столбец курсора (начиная с 0)
    // Возвращает CompletionContext — всё что нужно остальным алгоритмам
    public OperationResult<CompletionContext> AnalyzeContext(
        string sourceCode,
        int    cursorLine,
        int    cursorColumn)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            return OperationResult<CompletionContext>.Fail(
                "Исходный код пустой — нечего анализировать.", "EMPTY_SOURCE");

        // Шаг 1: разбираем весь файл в синтаксическое дерево
        // ParseText создаёт полное AST дерево C# кода
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

        // Шаг 2: создаём семантическую модель через мини-компиляцию
        // Компиляция нужна чтобы Roslyn знал типы всех переменных
        var compilation = CSharpCompilation.Create("CompletionAnalysis")
            .AddSyntaxTrees(syntaxTree)
            .AddReferences(MetadataReference.CreateFromFile(
                typeof(object).Assembly.Location))  // добавляем базовые типы .NET
            .AddReferences(MetadataReference.CreateFromFile(
                typeof(Enumerable).Assembly.Location));  // добавляем LINQ типы

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Шаг 3: переводим (строка, столбец) в абсолютную позицию в тексте
        // Roslyn работает с абсолютными позициями а не строка/столбец
        var lineSpan       = syntaxTree.GetText().Lines[cursorLine];
        int absolutePosition = lineSpan.Start + cursorColumn;

        // Шаг 4: находим узел AST дерева в позиции курсора
        var root       = syntaxTree.GetRoot();
        var nodeAtCursor = root.FindToken(absolutePosition - 1).Parent;

        if (nodeAtCursor == null)
            return OperationResult<CompletionContext>.Fail(
                "Не удалось найти узел в позиции курсора.", "NODE_NOT_FOUND");

        // Шаг 5: определяем тип завершения по типу узла
        // После точки → Member Access (доступ к члену типа)
        // В начале строки → Identifier (имя переменной или метода)
        var completionKind = DetermineCompletionKind(nodeAtCursor);

        // Шаг 6: если это доступ к члену типа — определяем тип объекта слева от точки
        string? expressionTypeName = null;
        if (completionKind == CompletionKind.MemberAccess)
        {
            var memberAccess = nodeAtCursor.Ancestors()
                .OfType<MemberAccessExpressionSyntax>()
                .FirstOrDefault();

            if (memberAccess != null)
            {
                var typeInfo = semanticModel.GetTypeInfo(memberAccess.Expression);
                expressionTypeName = typeInfo.Type?.ToDisplayString();
            }
        }

        // Собираем контекст из всего что нашли
        return OperationResult<CompletionContext>.Ok(new CompletionContext
        {
            SyntaxTree        = syntaxTree,
            SemanticModel     = semanticModel,
            AbsolutePosition  = absolutePosition,
            NodeAtCursor      = nodeAtCursor,
            CompletionKind    = completionKind,
            ExpressionTypeName = expressionTypeName,
            CurrentPrefix      = ExtractCurrentPrefix(sourceCode, absolutePosition),
        });
    }

    // Определить что именно хочет завершить разработчик
    // по типу узла AST в позиции курсора
    private static CompletionKind DetermineCompletionKind(SyntaxNode node)
    {
        // Если узел — это часть выражения с точкой (myList.___)
        // то нужно предлагать члены типа
        if (node.Ancestors().OfType<MemberAccessExpressionSyntax>().Any())
            return CompletionKind.MemberAccess;

        // Если узел — имя в объявлении переменной (var ___ = ...)
        // то нужно предлагать типы и переменные из области видимости
        if (node.Ancestors().OfType<VariableDeclarationSyntax>().Any())
            return CompletionKind.TypeName;

        // По умолчанию — предлагать всё видимое в данной позиции
        return CompletionKind.IdentifierInScope;
    }

    // Извлечь текст уже введённый после точки (префикс фильтрации)
    // Например если написано "myList.Ad" — префикс "Ad"
    private static string ExtractCurrentPrefix(string sourceCode, int position)
    {
        int start = position - 1;

        // Идём назад пока символы составляют допустимый идентификатор C#
        while (start > 0 &&
               (char.IsLetterOrDigit(sourceCode[start]) || sourceCode[start] == '_'))
        {
            start--;
        }

        return sourceCode.Substring(start + 1, position - start - 1);
    }
}

// Тип завершения — что именно пишет разработчик
public enum CompletionKind
{
    // После точки: myList.___ → предлагаем члены типа List<string>
    MemberAccess,

    // Имя типа: var x = new ___ → предлагаем конструкторы
    TypeName,

    // Идентификатор: ___ → предлагаем всё из области видимости
    IdentifierInScope,
}

// Результат анализа контекста — всё что нужно алгоритмам 33–38
public sealed record CompletionContext
{
    // Разобранное синтаксическое дерево всего файла
    public required SyntaxTree SyntaxTree { get; init; }

    // Семантическая модель — знает типы всех выражений
    public required SemanticModel SemanticModel { get; init; }

    // Абсолютная позиция курсора в тексте файла
    public required int AbsolutePosition { get; init; }

    // Узел AST в котором находится курсор
    public required SyntaxNode NodeAtCursor { get; init; }

    // Что именно дополняет разработчик
    public required CompletionKind CompletionKind { get; init; }

    // Имя типа слева от точки (null если не MemberAccess)
    // Например: "System.Collections.Generic.List<string>"
    public string? ExpressionTypeName { get; init; }

    // Префикс уже введённый после точки — для фильтрации
    // Например: если написано "myList.Ad" → "Ad"
    public required string CurrentPrefix { get; init; }
}
```

### Фаза 2 — Перечисление допустимых токенов (2 дня)

```csharp
// TypeMemberEnumerator.cs
// Перечисляет все члены типа (методы, свойства, поля) через Roslyn.
// Это то что Language Server Protocol делает для IntelliSense.
// Источник: Roslyn Language Server, OmniSharp: CompletionHandler.cs
public sealed class TypeMemberEnumerator
{
    // Получить все методы и свойства доступные в данной позиции
    // context — разобранный контекст из RoslynContextAnalyzer
    // Возвращает список CompletionSuggestion без оценок (score=0.5 для всех)
    // Оценки добавляются алгоритмами ранжирования 34–38
    public OperationResult<IReadOnlyList<CompletionSuggestion>> Enumerate(
        CompletionContext context)
    {
        var suggestions = new List<CompletionSuggestion>();

        if (context.CompletionKind == CompletionKind.MemberAccess
            && context.ExpressionTypeName != null)
        {
            // Получаем все символы видимые через точку в данной позиции
            // LookupSymbols возвращает все члены типа которые можно вызвать
            var symbols = context.SemanticModel.LookupSymbols(
                context.AbsolutePosition,
                container: FindTypeSymbol(context));

            foreach (var symbol in symbols)
            {
                // Пропускаем символы которые не начинаются на введённый префикс
                if (!symbol.Name.StartsWith(context.CurrentPrefix,
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                // Создаём подсказку с нейтральной оценкой 0.5
                // Настоящие оценки добавят алгоритмы ранжирования 34–38
                suggestions.Add(new CompletionSuggestion
                {
                    InsertText       = symbol.Name,
                    Description      = BuildDescription(symbol),
                    Score            = 0.5,  // нейтральная оценка — будет заменена
                    SourceAlgorithm  = "StaticAnalysis",
                });
            }
        }
        else if (context.CompletionKind == CompletionKind.IdentifierInScope)
        {
            // Получаем все идентификаторы видимые в данной позиции
            var symbols = context.SemanticModel.LookupSymbols(context.AbsolutePosition);

            foreach (var symbol in symbols)
            {
                if (!symbol.Name.StartsWith(context.CurrentPrefix,
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                suggestions.Add(new CompletionSuggestion
                {
                    InsertText       = symbol.Name,
                    Description      = BuildDescription(symbol),
                    Score            = 0.5,
                    SourceAlgorithm  = "StaticAnalysis",
                });
            }
        }

        // Сортируем алфавитно — алгоритмы ранжирования изменят порядок
        var sorted = suggestions.OrderBy(s => s.InsertText).ToList();
        return OperationResult<IReadOnlyList<CompletionSuggestion>>.Ok(sorted);
    }

    // Построить описание для подсказки из символа Roslyn
    // Описание показывается разработчику в popup подсказке
    private static string BuildDescription(ISymbol symbol)
    {
        return symbol switch
        {
            IMethodSymbol method =>
                // Для метода: показываем сигнатуру "(параметры) → тип_возврата"
                $"({string.Join(", ", method.Parameters.Select(p => $"{p.Type.Name} {p.Name}"))}) → {method.ReturnType.Name}",

            IPropertySymbol property =>
                // Для свойства: показываем тип
                $"Свойство типа {property.Type.Name}",

            IFieldSymbol field =>
                // Для поля: показываем тип
                $"Поле типа {field.Type.Name}",

            ILocalSymbol local =>
                // Для локальной переменной: показываем тип
                $"Переменная типа {local.Type.Name}",

            _ => symbol.Kind.ToString()
        };
    }

    // Найти символ типа по его имени в семантической модели
    private static INamespaceOrTypeSymbol? FindTypeSymbol(CompletionContext context)
    {
        // Ищем символ типа который стоит слева от точки
        // через узел выражения в AST дереве
        var memberAccess = context.NodeAtCursor.Ancestors()
            .OfType<MemberAccessExpressionSyntax>()
            .FirstOrDefault();

        if (memberAccess == null) return null;

        var typeInfo = context.SemanticModel.GetTypeInfo(memberAccess.Expression);
        return typeInfo.Type;
    }
}
```

**Gate B тесты подпроекта 33:**
- `List<string> myList = ...; myList.` → возвращает Add, Remove, Count, Where и другие методы List
- `string text = ...; text.` → возвращает Length, Contains, StartsWith и другие методы string
- `int x = 5; string.` → возвращает статические методы string: Format, IsNullOrEmpty, Join
- Неполный код с синтаксической ошибкой → OperationResult.Fail, не исключение

**Оценка времени:** 4–5 дней

---

## Подпроект 34 — Ранжирование по частоте вызовов

### Что делает этот алгоритм простым языком

Статический анализ (33) вернул 40 методов для `List<string>`. Все они допустимы, но какой вывести первым? Алгоритм 34 знает по статистике 500K GitHub репозиториев: метод `Add()` вызывается у List в 38% случаев, `Count` — в 27%, `Where()` — в 13%. Первым выводим `Add`. Это и есть «цитируемость кода».

Аналогия: библиотека. Все книги одинаково допустимы к прочтению. Но библиотекарь знает: эту книгу берут 100 раз в месяц, ту — 2 раза. Если студент просит «что-нибудь по физике» — библиотекарь предлагает популярное первым.

### Открытый источник для изучения

```
Pythia (JetBrains Research) — MIT — Python
https://arxiv.org/abs/1912.00742

Метод baseline (чистая частота):
  frequency_dataset/method_frequencies.json
  Формат: {"List<string>.Add": 0.38, "List<string>.Count": 0.27, ...}
  Данные собраны из 2700 Python репозиториев GitHub

IntelliCode (Microsoft):
  devblogs.microsoft.com/visualstudio/type-less-code-more-with-intellicode-completions
  Тот же принцип, но обучен на 500K C# репозиториев
```

### Папка в Solution

```
33.CodeCompletion/Algorithms/02-FrequencyRanking/
├── UsageFrequencyCollector.cs  — сбор статистики из репозиториев
├── FrequencyDatabase.cs        — хранение и поиск частот
├── FrequencyRankingProvider.cs — реализует ICompletionProvider
├── Data/
│   └── method_frequencies.json — предсобранные данные для C#
└── Tests/FrequencyRankingTests.cs
```

### Фаза 1 — База данных частот (2 дня)

```csharp
// FrequencyDatabase.cs
// Хранит предсобранную статистику: какой метод у какого типа
// как часто вызывается в реальном C# коде.
// Данные собираются один раз из открытых GitHub репозиториев
// и сохраняются в JSON файл для быстрого доступа.
public sealed class FrequencyDatabase
{
    // Словарь: "ИмяТипа.ИмяМетода" → частота от 0.0 до 1.0
    // Пример: "System.Collections.Generic.List.Add" → 0.38
    private readonly Dictionary<string, double> frequencyMap;

    // Создать базу данных из JSON файла
    // jsonFilePath — путь к файлу method_frequencies.json
    public FrequencyDatabase(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException(
                $"Файл с частотами методов не найден: {jsonFilePath}. " +
                "Запустите UsageFrequencyCollector для сбора данных.");

        // Читаем JSON файл и десериализуем в словарь
        var jsonText      = File.ReadAllText(jsonFilePath);
        frequencyMap = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, double>>(jsonText)
            ?? new Dictionary<string, double>();
    }

    // Получить частоту вызова метода у данного типа
    // Возвращает 0.0 если статистики нет (новый или редкий метод)
    public double GetFrequency(string typeName, string methodName)
    {
        // Нормализуем имя типа: убираем пространства имён для более широкого сопоставления
        // "System.Collections.Generic.List<string>" → "List"
        string shortTypeName = ExtractShortTypeName(typeName);

        // Строим ключ поиска
        string lookupKey = $"{shortTypeName}.{methodName}";

        // Ищем сначала с полным именем типа, потом с коротким
        if (frequencyMap.TryGetValue($"{typeName}.{methodName}", out double exactFreq))
            return exactFreq;

        if (frequencyMap.TryGetValue(lookupKey, out double shortFreq))
            return shortFreq;

        // Статистики нет — возвращаем маленькое ненулевое значение
        // (не 0.0 чтобы новые методы не исчезали из списка)
        return 0.01;
    }

    // Получить топ-K методов отсортированных по частоте
    public IReadOnlyList<(string MethodName, double Frequency)> GetTopMethods(
        string typeName,
        int    topK)
    {
        string shortTypeName = ExtractShortTypeName(typeName);
        string prefix        = shortTypeName + ".";

        // Находим все записи для данного типа
        return frequencyMap
            .Where(kv => kv.Key.StartsWith(prefix) ||
                         kv.Key.StartsWith(typeName + "."))
            .Select(kv => (
                MethodName: kv.Key.Split('.').Last(),  // берём только имя метода
                Frequency:  kv.Value))
            .OrderByDescending(x => x.Frequency)
            .Take(topK)
            .ToList();
    }

    // Извлечь короткое имя типа без пространства имён и обобщённых параметров
    // "System.Collections.Generic.List<string>" → "List"
    private static string ExtractShortTypeName(string fullTypeName)
    {
        // Убираем обобщённые параметры: "List<string>" → "List"
        string withoutGenerics = fullTypeName.Split('<')[0].Trim();

        // Убираем пространство имён: "System.Collections.Generic.List" → "List"
        return withoutGenerics.Split('.').Last();
    }
}
```

### Фаза 2 — Сборщик статистики из репозиториев (2 дня)

```csharp
// UsageFrequencyCollector.cs
// Обходит C# файлы и считает как часто каждый метод вызывается у каждого типа.
// Запускается один раз на обучающем наборе репозиториев.
// Результат сохраняется в method_frequencies.json.
public sealed class UsageFrequencyCollector : BaseService
{
    // Источник файлов C# для сбора статистики
    private readonly ISourceCodeReader sourceReader;

    public UsageFrequencyCollector(
        ISourceCodeReader sourceReader,
        ILogger           logger,
        IOperationContext context)
        : base(logger, context)
    {
        this.sourceReader = sourceReader;
    }

    // Собрать статистику из всех C# файлов проекта
    // operationId  — для идемпотентности (правило DNA 7)
    // projectRoot  — корневая папка репозитория
    // outputPath   — куда сохранить JSON с результатами
    public async Task<OperationResult<Dictionary<string, double>>> CollectAsync(
        string operationId,
        string projectRoot,
        string outputPath)
    {
        // Проверяем идемпотентность (правило DNA 7)
        var cached = await CheckIdempotencyAsync(operationId);
        if (cached.AlreadyProcessed)
            return cached.CachedResult as OperationResult<Dictionary<string, double>>
                   ?? OperationResult<Dictionary<string, double>>.Fail("Кеш недоступен.", "CACHE_MISS");

        // Получаем список всех C# файлов
        var filesResult = await sourceReader.ListSourceFilesAsync(projectRoot);
        if (!filesResult.Success)
            return filesResult.MapError<Dictionary<string, double>>();

        // Счётчики: сколько раз метод вызывался и сколько раз встречался тип
        var methodCallCounts = new Dictionary<string, int>();  // "List.Add" → 847
        var typeUsageCounts  = new Dictionary<string, int>();  // "List" → 2230

        // Обрабатываем каждый файл
        foreach (string filePath in filesResult.Value!)
        {
            var fileResult = await sourceReader.ReadFileAsync(filePath);
            if (!fileResult.Success)
                continue;  // пропускаем нечитаемые файлы

            // Разбираем файл и считаем вызовы методов
            CollectFromFile(fileResult.Value!, methodCallCounts, typeUsageCounts);
        }

        // Вычисляем относительные частоты: count / total
        var frequencies = new Dictionary<string, double>();
        foreach (var (key, count) in methodCallCounts)
        {
            // Ключ имеет вид "List.Add", типName = "List"
            string typeName = key.Split('.')[0];

            if (typeUsageCounts.TryGetValue(typeName, out int total) && total > 0)
            {
                // Относительная частота = сколько раз вызван / сколько раз встречался тип
                frequencies[key] = (double)count / total;
            }
        }

        // Правило DNA 8: сохраняем перед отправкой события
        var json = System.Text.Json.JsonSerializer.Serialize(frequencies,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(outputPath, json);

        // Правило DNA 9: отправляем событие о завершении сбора
        await DispatchEventAsync(new StandardEvent
        {
            EventType = "frequency_collection.completed",
            Payload   = new Dictionary<string, object>
            {
                ["filesProcessed"] = filesResult.Value!.Count,
                ["methodsFound"]   = frequencies.Count,
                ["outputPath"]     = outputPath,
            }
        });

        return OperationResult<Dictionary<string, double>>.Ok(frequencies);
    }

    // Разобрать один C# файл и добавить вызовы методов в счётчики
    private static void CollectFromFile(
        string                      sourceCode,
        Dictionary<string, int>     methodCallCounts,
        Dictionary<string, int>     typeUsageCounts)
    {
        // Разбираем файл Roslyn-ом
        var syntaxTree   = CSharpSyntaxTree.ParseText(sourceCode);
        var root         = syntaxTree.GetRoot();

        // Обходим все выражения доступа к члену типа: myList.Add(...)
        foreach (var memberAccess in root.DescendantNodes()
                                         .OfType<MemberAccessExpressionSyntax>())
        {
            // Берём только имя типа слева от точки без подробностей
            string expressionText = memberAccess.Expression.ToString();
            string methodName     = memberAccess.Name.Identifier.Text;

            // Получаем простое имя типа (убираем пространства имён)
            string shortTypeName = expressionText.Split('.').Last().Split('<')[0];

            if (string.IsNullOrEmpty(shortTypeName) || string.IsNullOrEmpty(methodName))
                continue;

            string key = $"{shortTypeName}.{methodName}";

            // Увеличиваем счётчик вызова метода
            methodCallCounts.TryAdd(key, 0);
            methodCallCounts[key]++;

            // Увеличиваем счётчик использования типа
            typeUsageCounts.TryAdd(shortTypeName, 0);
            typeUsageCounts[shortTypeName]++;
        }
    }
}
```

### Фаза 3 — Поставщик с ранжированием по частоте (1 день)

```csharp
// FrequencyRankingProvider.cs
// Получает кандидатов от StaticAnalysisProvider (33)
// и переоценивает их по частоте из FrequencyDatabase.
public sealed class FrequencyRankingProvider : BaseService, ICompletionProvider
{
    public string AlgorithmName => "FrequencyRanking";

    private readonly StaticAnalysisProvider  staticProvider;  // базовые кандидаты
    private readonly FrequencyDatabase        frequencyDb;     // статистика

    public FrequencyRankingProvider(
        StaticAnalysisProvider staticProvider,
        FrequencyDatabase      frequencyDb,
        ILogger logger, IOperationContext context)
        : base(logger, context)
    {
        this.staticProvider = staticProvider;
        this.frequencyDb    = frequencyDb;
    }

    public async Task<OperationResult<IReadOnlyList<CompletionSuggestion>>> GetSuggestionsAsync(
        CompletionRequest request)
    {
        // Шаг 1: получаем кандидатов от статического анализа
        var staticResult = await staticProvider.GetSuggestionsAsync(request);
        if (!staticResult.Success)
            return staticResult;

        // Шаг 2: для каждого кандидата ищем его частоту в базе
        var contextResult = new RoslynContextAnalyzer().AnalyzeContext(
            request.SourceCode, request.CursorLine, request.CursorColumn);

        if (!contextResult.Success)
            return staticResult;  // если анализ контекста не удался — возвращаем неранжированный список

        string typeName = contextResult.Value!.ExpressionTypeName ?? string.Empty;

        // Шаг 3: пересчитываем оценки по частотам
        var reranked = staticResult.Value!
            .Select(suggestion =>
            {
                // Получаем частоту из базы данных
                double frequency = frequencyDb.GetFrequency(typeName, suggestion.InsertText);

                // Новая оценка = частота (0.0–1.0)
                // Методы с высокой частотой уйдут вверх списка
                return suggestion with
                {
                    Score           = frequency,
                    SourceAlgorithm = AlgorithmName,
                };
            })
            .OrderByDescending(s => s.Score)  // сортируем по убыванию
            .Take(request.MaxSuggestions)
            .ToList();

        return OperationResult<IReadOnlyList<CompletionSuggestion>>.Ok(reranked);
    }
}
```

**Gate B тесты подпроекта 34:**
- Для `List<string>.` → `Add` должен иметь score > `Clear` (Add используется чаще)
- Для `string.` → `IsNullOrEmpty` должен иметь score > `IsInterned` (IsNullOrEmpty популярнее)
- Неизвестный тип → возвращает кандидатов с score=0.01, не ошибку

**Оценка времени:** 5–6 дней

---

## Подпроект 35 — N-граммная модель токенов

### Что делает этот алгоритм простым языком

Статистика вызовов (34) знает что у `List` чаще всего вызывают `Add`. Но N-граммная модель знает больше: если разработчик написал `return new` — следующим вероятно будет тип объекта. Если написал `if (` — вероятно будет `string.IsNullOrEmpty` или `!`. Контекст два–три токена назад сильно предсказывает следующий.

Аналогия: T9 на старых телефонах. После «прив» чаще всего «ет», «ет,» или «ет!». Модель не понимает смысл — просто считает соседние слова.

### Открытый источник для изучения

```
Работа "Sequence Model Design for Code Completion in the Modern IDE"
https://arxiv.org/abs/2004.05249
Авторы: Gareth Ari Aye (Google)
Раздел 3: Character-level input with token output для OOV токенов
Раздел 4: Смешанная модель статического анализа + языковая модель
```

### Папка в Solution

```
33.CodeCompletion/Algorithms/03-NGramModel/
├── NGramIndexBuilder.cs    — построение индекса N-грамм из корпуса
├── NGramIndex.cs           — хранение и поиск N-грамм
├── NGramTokenizer.cs       — разбивка C# кода на токены для N-грамм
├── NGramProvider.cs        — реализует ICompletionProvider
└── Tests/NGramModelTests.cs
```

### Фаза 1 — Токенизация кода для N-граммной модели (1 день)

```csharp
// NGramTokenizer.cs
// Разбивает C# код на токены для N-граммной модели.
// Для N-грамм важны только значимые токены: ключевые слова, идентификаторы,
// операторы. Пробелы и комментарии пропускаются.
// Источник: CSharpSyntaxTree токены Roslyn
public sealed class NGramTokenizer
{
    // Разбить исходный код на список значимых токенов
    // sourceCode — C# исходный код
    // Возвращает список строк: ключевые слова и идентификаторы
    public OperationResult<IReadOnlyList<string>> Tokenize(string sourceCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            return OperationResult<IReadOnlyList<string>>.Fail(
                "Исходный код пустой.", "EMPTY_SOURCE");

        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var tokens     = new List<string>();

        // Обходим все токены синтаксического дерева
        foreach (var token in syntaxTree.GetRoot().DescendantTokens())
        {
            // Пропускаем пробельные токены — они не несут смысла для N-грамм
            if (token.IsKind(SyntaxKind.WhitespaceTrivia)  ||
                token.IsKind(SyntaxKind.EndOfLineTrivia)   ||
                token.IsKind(SyntaxKind.SingleLineCommentTrivia))
                continue;

            string tokenText = token.Text.Trim();
            if (string.IsNullOrEmpty(tokenText))
                continue;

            // Нормализуем строковые литералы — конкретный текст строки не важен
            // "hello world" и "goodbye" → одинаковый токен STRING_LITERAL
            if (token.IsKind(SyntaxKind.StringLiteralToken))
            {
                tokens.Add("STRING_LITERAL");
                continue;
            }

            // Нормализуем числовые литералы
            // 42 и 100 → NUMBER_LITERAL
            if (token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                tokens.Add("NUMBER_LITERAL");
                continue;
            }

            tokens.Add(tokenText);
        }

        return OperationResult<IReadOnlyList<string>>.Ok(tokens);
    }
}
```

### Фаза 2 — Индекс N-грамм (2 дня)

```csharp
// NGramIndex.cs
// Хранит статистику: сколько раз за данными N токенами следовал конкретный токен.
// Поддерживает биграммы (N=2) и триграммы (N=3).
// Источник: стандартная реализация N-граммной языковой модели
public sealed class NGramIndex
{
    // Размер N для N-грамм (2 = биграмма, 3 = триграмма)
    private readonly int ngramSize;

    // Основная структура хранения:
    // Ключ: строка "(токен1, токен2)" — N-1 предыдущих токенов
    // Значение: словарь {следующий_токен → количество_вхождений}
    private readonly Dictionary<string, Dictionary<string, int>> ngramCounts;

    // Суммарные счётчики для каждого контекста (нужны для нормализации)
    private readonly Dictionary<string, int> contextTotals;

    public NGramIndex(int ngramSize = 2)
    {
        if (ngramSize < 2 || ngramSize > 4)
            throw new ArgumentOutOfRangeException(nameof(ngramSize),
                "Размер N-граммы должен быть от 2 до 4. " +
                "Меньше 2 — слишком мало контекста. Больше 4 — слишком мало данных.");

        this.ngramSize      = ngramSize;
        this.ngramCounts    = new Dictionary<string, Dictionary<string, int>>();
        this.contextTotals  = new Dictionary<string, int>();
    }

    // Добавить последовательность токенов в индекс
    // Извлекает все N-граммы из последовательности и обновляет счётчики
    public void AddTokenSequence(IReadOnlyList<string> tokens)
    {
        // Для каждой позиции i начиная с позиции (N-1)
        for (int i = ngramSize - 1; i < tokens.Count; i++)
        {
            // Берём N-1 предыдущих токенов как контекст
            var contextTokens = tokens.Skip(i - (ngramSize - 1)).Take(ngramSize - 1);
            string contextKey = string.Join("\t", contextTokens);  // разделитель — табуляция

            // Токен который следует за этим контекстом
            string nextToken = tokens[i];

            // Добавляем в словарь счётчиков
            if (!ngramCounts.ContainsKey(contextKey))
                ngramCounts[contextKey] = new Dictionary<string, int>();

            ngramCounts[contextKey].TryAdd(nextToken, 0);
            ngramCounts[contextKey][nextToken]++;

            // Обновляем суммарный счётчик для этого контекста
            contextTotals.TryAdd(contextKey, 0);
            contextTotals[contextKey]++;
        }
    }

    // Предсказать вероятность следующего токена по контексту
    // contextTokens — последние N-1 токенов (то что уже написано)
    // nextToken     — токен для которого вычисляем вероятность
    // Возвращает вероятность от 0.0 до 1.0
    public double GetProbability(IReadOnlyList<string> contextTokens, string nextToken)
    {
        string contextKey = string.Join("\t", contextTokens.TakeLast(ngramSize - 1));

        if (!ngramCounts.TryGetValue(contextKey, out var nextTokenCounts))
        {
            // Контекст не встречался в обучении → очень маленькая вероятность
            // Сглаживание Лапласа (Laplace smoothing): добавляем 1 к каждому счётчику
            return 1.0 / (1 + 50000);  // 50000 ≈ размер словаря C#
        }

        int nextCount    = nextTokenCounts.GetValueOrDefault(nextToken, 0);
        int contextTotal = contextTotals[contextKey];

        // Сглаживание Лапласа: (count + 1) / (total + vocabulary_size)
        // Предотвращает нулевые вероятности для невиданных токенов
        return (nextCount + 1.0) / (contextTotal + 50000);
    }

    // Получить топ-K наиболее вероятных следующих токенов по контексту
    public IReadOnlyList<(string Token, double Probability)> GetTopKNextTokens(
        IReadOnlyList<string> contextTokens,
        int topK)
    {
        string contextKey = string.Join("\t", contextTokens.TakeLast(ngramSize - 1));

        if (!ngramCounts.TryGetValue(contextKey, out var nextTokenCounts))
            return Array.Empty<(string, double)>();

        int contextTotal = contextTotals[contextKey];

        return nextTokenCounts
            .OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select(kv => (kv.Key, Probability: (kv.Value + 1.0) / (contextTotal + 50000)))
            .ToList();
    }
}
```

**Gate B тесты подпроекта 35:**
- После токенов `["return", "new"]` → вероятность типов (List, Dictionary, StringBuilder) выше чем ключевых слов
- После `["if", "("]` → вероятность `"string.IsNullOrEmpty"`, `"!"` выше чем `"42"`
- Триграммная модель точнее биграммной на 10K строк C# кода (HeldOut перплексия)

**Оценка времени:** 5–7 дней

---

## Подпроект 36 — BM25 поиск по локальному контексту

### Что делает этот алгоритм простым языком

Когда разработчик пишет `jwtService.` — мы ищем в других файлах проекта все места где `jwtService` встречается рядом с вызовом методов. Находим: в `AuthController.cs` написано `jwtService.GetClaims(token)`, в `Middleware.cs` — `jwtService.Validate(header)`. BM25 ранжирует эти находки и предлагает `GetClaims` первым — именно этот метод чаще всего используется вместе с `jwtService`.

Аналогия: Google по корпоративному wiki. Ищешь «jwtService» — находишь страницы где это слово встречается часто и важно.

### Открытый источник для изучения

```
BM25 оригинальная статья:
  Robertson et al. "Okapi at TREC-3", 1994
  Формула: score = Σ IDF(t) × tf × (k1+1) / (tf + k1 × (1-b+b×dl/avgdl))

Continue.dev — Apache 2.0 — TypeScript
https://github.com/continuedev/continue
  core/indexing/CodebaseIndexer.ts   — индексация файлов проекта
  core/retrieval/BM25Retriever.ts    — BM25 поиск по кодовой базе

Наш подпроект 12 (SearchEngine) — уже реализован!
  Используем BM25Scorer и InvertedIndex из подпроекта 12
  Это главный пример повторного использования кода в Solution
```

### Папка в Solution

```
33.CodeCompletion/Algorithms/04-BM25LocalContext/
├── ProjectIndexer.cs         — индексирует файлы проекта при старте
├── LocalContextSearcher.cs   — ищет похожие фрагменты кода через BM25
├── CodeChunkExtractor.cs     — разбивает файлы на чанки по методам
├── BM25CompletionProvider.cs — реализует ICompletionProvider
└── Tests/BM25LocalContextTests.cs
```

### Фаза 1 — Индексация проекта (2 дня)

```csharp
// ProjectIndexer.cs
// Индексирует все C# файлы проекта для быстрого BM25 поиска.
// Переиспользует InvertedIndex из подпроекта 12 (SearchEngine).
// Каждый метод в проекте становится отдельным документом индекса.
public sealed class ProjectIndexer : BaseService
{
    // Инвертированный индекс из подпроекта 12 (SearchEngine) — переиспользуем
    private readonly InvertedIndex invertedIndex;

    // Читатель исходников — fabric интерфейс
    private readonly ISourceCodeReader sourceReader;

    // Отображение: ID документа → исходный код чанка + информация о местоположении
    private readonly Dictionary<int, CodeChunk> chunkRegistry;

    public ProjectIndexer(
        InvertedIndex     invertedIndex,
        ISourceCodeReader sourceReader,
        ILogger logger, IOperationContext context)
        : base(logger, context)
    {
        this.invertedIndex = invertedIndex;
        this.sourceReader  = sourceReader;
        this.chunkRegistry = new Dictionary<int, CodeChunk>();
    }

    // Проиндексировать все файлы проекта
    // operationId  — для идемпотентности
    // projectRoot  — корень проекта
    public async Task<OperationResult<int>> IndexProjectAsync(
        string operationId,
        string projectRoot)
    {
        var cached = await CheckIdempotencyAsync(operationId);
        if (cached.AlreadyProcessed)
            return OperationResult<int>.Ok(chunkRegistry.Count);

        var filesResult = await sourceReader.ListSourceFilesAsync(projectRoot);
        if (!filesResult.Success)
            return filesResult.MapError<int>();

        int totalChunks = 0;
        int chunkId     = 0;

        foreach (string filePath in filesResult.Value!)
        {
            var fileResult = await sourceReader.ReadFileAsync(filePath);
            if (!fileResult.Success)
                continue;

            // Разбиваем файл на чанки (каждый метод = отдельный документ)
            var chunks = ExtractMethodChunks(fileResult.Value!, filePath);

            foreach (var chunk in chunks)
            {
                // Добавляем чанк в инвертированный индекс (из подпроекта 12)
                // Передаём ID чанка и его текст
                var addResult = invertedIndex.AddDocument(chunkId, chunk.SourceText);
                if (addResult.Success)
                {
                    chunkRegistry[chunkId] = chunk;
                    chunkId++;
                    totalChunks++;
                }
            }
        }

        Logger.LogInformation("Индексация завершена: {Count} методов из {Files} файлов",
            totalChunks, filesResult.Value!.Count);

        return OperationResult<int>.Ok(totalChunks);
    }

    // Найти чанки кода релевантные запросу
    // query   — поисковый запрос (например "jwtService")
    // topK    — сколько результатов вернуть
    public OperationResult<IReadOnlyList<CodeChunk>> Search(string query, int topK)
    {
        // Используем BM25Scorer из подпроекта 12
        var searchResult = invertedIndex.Search(query, topK);
        if (!searchResult.Success)
            return searchResult.MapError<IReadOnlyList<CodeChunk>>();

        var chunks = searchResult.Value!
            .Select(docId => chunkRegistry.GetValueOrDefault(docId))
            .Where(chunk => chunk != null)
            .Select(chunk => chunk!)
            .ToList();

        return OperationResult<IReadOnlyList<CodeChunk>>.Ok(chunks);
    }

    // Разбить C# файл на чанки — по одному на каждый метод
    private static IReadOnlyList<CodeChunk> ExtractMethodChunks(
        string sourceCode,
        string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root       = syntaxTree.GetRoot();
        var chunks     = new List<CodeChunk>();

        // Находим все объявления методов в файле
        foreach (var method in root.DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>())
        {
            chunks.Add(new CodeChunk
            {
                SourceText   = method.ToString(),
                FilePath     = filePath,
                MethodName   = method.Identifier.Text,
                StartLine    = method.GetLocation().GetLineSpan().StartLinePosition.Line,
            });
        }

        return chunks;
    }
}

// Один фрагмент кода (метод или функция) из проекта
public sealed record CodeChunk
{
    // Исходный текст метода как строка
    public required string SourceText { get; init; }

    // Путь к файлу где находится этот метод
    public required string FilePath { get; init; }

    // Имя метода (для отображения в подсказке)
    public required string MethodName { get; init; }

    // Строка начала метода в файле (для перехода к определению)
    public required int StartLine { get; init; }
}
```

**Gate B тесты подпроекта 36:**
- Запрос `"jwtService"` при наличии `jwtService.Validate()` в другом файле → `Validate` в топ-3
- Запрос `"await _repository"` → методы репозитория из соседних файлов в результатах
- Пустой индекс → OperationResult.Fail с понятным сообщением, не пустой список

**Оценка времени:** 4–5 дней

---

## Подпроект 37 — Трансформер-модель с фильтром допустимых токенов

### Что делает этот алгоритм простым языком

Нейросеть предсказывает вероятность каждого из 50 000 токенов как следующего. Но большинство из них синтаксически недопустимы: нельзя написать `public public`, нельзя написать `return if`. Фильтр от Roslyn (подпроект 33) оставляет только допустимые — обычно 10–200 из 50 000. Нейросеть переоценивает только эти. Это обеспечивает и высокое качество предсказания и гарантированную синтаксическую корректность.

Аналогия: олимпиадный кроссворд + словарь. Словарь предлагает 100 слов на «К», три буквы. Олимпийский чемпион выбирает лучшее — но только из того что предложил словарь.

### Открытый источник для изучения

```
StarCoder2 — BigCode · Apache 2.0
https://github.com/bigcode-project/starcoder2
  Поддерживает Fill-in-the-Middle (заполнение середины):
  tokens: <PRE> код_до_курсора <SUF> код_после_курсора <MID> [предсказание]

DeepSeek-Coder — MIT
https://github.com/deepseek-ai/DeepSeek-Coder
  deepseek-coder-1.3b — маленькая модель для локальной работы

Работа Google (Aye, 2020):
  "combining static analysis' ability to enumerate all valid keywords
   with the ability of a language model to place a probability distribution over them"
```

### Папка в Solution

```
33.CodeCompletion/Algorithms/05-TransformerFilter/
├── LocalModelInference.cs      — запуск маленькой локальной LM
├── AllowedTokensFilter.cs      — фильтр допустимых токенов от Roslyn
├── FillInTheMiddleBuilder.cs   — сборка FIM-запроса для модели
├── TransformerProvider.cs      — реализует ICompletionProvider
└── Tests/TransformerFilterTests.cs
```

### Фаза 1 — Сборка Fill-in-the-Middle запроса (1 день)

```csharp
// FillInTheMiddleBuilder.cs
// Строит запрос для языковой модели в формате Fill-in-the-Middle (заполнение середины).
// Вместо «продолжи этот код» модель получает: «здесь PREFIX, здесь SUFFIX, заполни MIDDLE».
// Это даёт модели двусторонний контекст — она видит и что до курсора и что после.
public sealed class FillInTheMiddleBuilder
{
    // Специальные токены обозначающие части FIM запроса
    // Эти токены есть в словаре StarCoder2 и DeepSeek-Coder
    private const string PrefixToken = "<fim_prefix>";   // начало: «код до курсора»
    private const string SuffixToken = "<fim_suffix>";   // конец: «код после курсора»
    private const string MiddleToken = "<fim_middle>";   // место где нужно предсказать

    // Максимальный размер контекста в символах (чтобы не превысить лимит модели)
    private const int MaxContextCharacters = 2000;

    // Построить FIM запрос для модели
    // sourceCode    — полный код файла
    // cursorPosition — абсолютная позиция курсора
    // Возвращает строку запроса которую подаём в языковую модель
    public OperationResult<string> BuildRequest(string sourceCode, int cursorPosition)
    {
        if (cursorPosition < 0 || cursorPosition > sourceCode.Length)
            return OperationResult<string>.Fail(
                $"Позиция курсора {cursorPosition} выходит за пределы кода длиной {sourceCode.Length}.",
                "INVALID_CURSOR_POSITION");

        // Код до курсора — берём последние MaxContextCharacters/2 символов
        string prefix = sourceCode.Substring(
            Math.Max(0, cursorPosition - MaxContextCharacters / 2),
            Math.Min(cursorPosition, MaxContextCharacters / 2));

        // Код после курсора — берём следующие MaxContextCharacters/2 символов
        string suffix = sourceCode.Substring(
            cursorPosition,
            Math.Min(sourceCode.Length - cursorPosition, MaxContextCharacters / 2));

        // Собираем FIM запрос в формате StarCoder2/DeepSeek-Coder
        string fimRequest = $"{PrefixToken}{prefix}{SuffixToken}{suffix}{MiddleToken}";

        return OperationResult<string>.Ok(fimRequest);
    }
}

// AllowedTokensFilter.cs
// Фильтрует предсказания нейросети: оставляет только синтаксически допустимые токены.
// Использует Roslyn (подпроект 33) для определения допустимых токенов.
public sealed class AllowedTokensFilter
{
    private readonly TypeMemberEnumerator memberEnumerator;    // из подпроекта 33
    private readonly RoslynContextAnalyzer contextAnalyzer;   // из подпроекта 33

    public AllowedTokensFilter(
        TypeMemberEnumerator  memberEnumerator,
        RoslynContextAnalyzer contextAnalyzer)
    {
        this.memberEnumerator = memberEnumerator;
        this.contextAnalyzer  = contextAnalyzer;
    }

    // Отфильтровать предсказания модели: оставить только допустимые токены
    // predictions      — словарь {токен → вероятность} от языковой модели
    // completionContext — контекст курсора для проверки допустимости
    // Возвращает отфильтрованный и перенормированный словарь
    public OperationResult<IReadOnlyDictionary<string, double>> Filter(
        IReadOnlyDictionary<string, double> predictions,
        CompletionContext                    completionContext)
    {
        // Получаем список всех допустимых токенов через Roslyn
        var allowedResult = memberEnumerator.Enumerate(completionContext);
        if (!allowedResult.Success)
            return allowedResult.MapError<IReadOnlyDictionary<string, double>>();

        // Строим множество допустимых имён для быстрой проверки
        var allowedNames = allowedResult.Value!
            .Select(s => s.InsertText)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Оставляем только предсказания из допустимого множества
        var filtered = predictions
            .Where(kv => allowedNames.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        if (filtered.Count == 0)
        {
            // Если фильтр убрал всё — возвращаем все допустимые с равными вероятностями
            // Это происходит когда модель не знает этот контекст
            double equalProbability = 1.0 / allowedNames.Count;
            filtered = allowedNames.ToDictionary(name => name, _ => equalProbability);
        }

        // Перенормируем: сумма вероятностей должна быть 1.0
        double total = filtered.Values.Sum();
        var normalized = filtered.ToDictionary(
            kv => kv.Key,
            kv => kv.Value / total);

        return OperationResult<IReadOnlyDictionary<string, double>>.Ok(normalized);
    }
}
```

**Gate B тесты подпроекта 37:**
- Выход всегда синтаксически корректен: предложения проходят Roslyn компиляцию без ошибок
- После `List<string> x; x.` → в предложениях только методы List, никаких ключевых слов
- Если модель предлагает `public` после точки — фильтр убирает его

**Оценка времени:** 6–8 дней

---

## Подпроект 38 — PageRank на графе зависимостей AST

### Что делает этот алгоритм простым языком

Одни методы вызываются из 50 мест в проекте, другие — только из одного. Метод `Authenticate()` в AuthService, вероятно, вызывается из множества контроллеров и middleware — это важный метод. Алгоритм PageRank применённый к графу вызовов даёт каждому методу «важность» пропорциональную тому сколько важных мест его вызывает. Когда разработчик пишет код в AuthController — мы поднимаем методы с высоким PageRank наверх списка.

Аналогия: PageRank для веб-страниц. Страница важна если на неё ссылаются важные страницы. Метод важен если его вызывают важные методы.

### Открытый источник для изучения

```
Aider — Apache 2.0 — Python
https://github.com/Aider-AI/aider
  aider/repomap.py — 600 строк, Tree-sitter + NetworkX PageRank
  Алгоритм:
    1. Tree-sitter разбирает все файлы
    2. Для каждого определения символа: узел в графе
    3. Для каждого использования символа: ребро от использующего к определяющему
    4. NetworkX pagerank(graph) → ранг каждого символа
    5. Топ символы → показываем в контексте

Наш подпроект 07 (GraphEngine) — уже реализован!
  PageRankCalculator из подпроекта 07 используем напрямую
```

### Папка в Solution

```
33.CodeCompletion/Algorithms/06-PageRankAst/
├── CallGraphBuilder.cs      — строит граф вызовов из Roslyn AST
├── SymbolPageRanker.cs      — вычисляет PageRank символов (переиспользует 07)
├── PageRankCache.cs         — кеш рангов (пересчитывается при изменении файлов)
├── PageRankProvider.cs      — реализует ICompletionProvider
└── Tests/PageRankAstTests.cs
```

### Фаза 1 — Построение графа вызовов (3 дня)

```csharp
// CallGraphBuilder.cs
// Строит граф вызовов для всего проекта: кто что вызывает.
// Переиспользует KnowledgeGraph из подпроекта 07 (GraphEngine).
// Источник: aider/repomap.py — основная идея алгоритма
public sealed class CallGraphBuilder : BaseService
{
    private readonly ISourceCodeReader sourceReader;

    // KnowledgeGraph из подпроекта 07 — переиспользуем напрямую
    private KnowledgeGraph? callGraph;

    // Отображение: имя символа → числовой ID в графе
    private readonly Dictionary<string, int> symbolToNodeId;

    // Отображение: числовой ID → имя символа (для обратного поиска)
    private readonly Dictionary<int, string> nodeIdToSymbol;

    private int nextNodeId;

    public CallGraphBuilder(
        ISourceCodeReader sourceReader,
        ILogger logger, IOperationContext context)
        : base(logger, context)
    {
        this.sourceReader  = sourceReader;
        this.symbolToNodeId = new Dictionary<string, int>();
        this.nodeIdToSymbol = new Dictionary<int, string>();
        this.nextNodeId    = 0;
    }

    // Построить граф вызовов для всего проекта
    // operationId  — для идемпотентности
    // projectRoot  — корень проекта
    public async Task<OperationResult<KnowledgeGraph>> BuildGraphAsync(
        string operationId,
        string projectRoot)
    {
        var cached = await CheckIdempotencyAsync(operationId);
        if (cached.AlreadyProcessed && callGraph != null)
            return OperationResult<KnowledgeGraph>.Ok(callGraph);

        callGraph = new KnowledgeGraph();

        var filesResult = await sourceReader.ListSourceFilesAsync(projectRoot);
        if (!filesResult.Success)
            return filesResult.MapError<KnowledgeGraph>();

        // Проход 1: собираем все определения методов и классов
        // Каждое определение = узел в графе
        foreach (string filePath in filesResult.Value!)
        {
            var fileResult = await sourceReader.ReadFileAsync(filePath);
            if (!fileResult.Success) continue;

            ExtractDefinitions(fileResult.Value!, filePath);
        }

        // Проход 2: собираем все использования (вызовы методов)
        // Каждый вызов = ребро от вызывающего к вызываемому
        foreach (string filePath in filesResult.Value!)
        {
            var fileResult = await sourceReader.ReadFileAsync(filePath);
            if (!fileResult.Success) continue;

            ExtractUsages(fileResult.Value!, filePath);
        }

        Logger.LogInformation("Граф вызовов построен: {Nodes} узлов, {Edges} рёбер",
            symbolToNodeId.Count,
            callGraph.EdgeCount);

        return OperationResult<KnowledgeGraph>.Ok(callGraph);
    }

    // Извлечь все определения символов из файла (методы, классы, свойства)
    private void ExtractDefinitions(string sourceCode, string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root       = syntaxTree.GetRoot();

        // Обходим все объявления методов — каждый становится узлом графа
        foreach (var method in root.DescendantNodes()
                                    .OfType<MethodDeclarationSyntax>())
        {
            // Полное имя метода: "FileName.MethodName" для уникальности
            string symbolName = $"{Path.GetFileNameWithoutExtension(filePath)}.{method.Identifier.Text}";

            if (!symbolToNodeId.ContainsKey(symbolName))
            {
                // Добавляем новый узел в граф с числовым ID
                int nodeId = nextNodeId++;
                symbolToNodeId[symbolName] = nodeId;
                nodeIdToSymbol[nodeId]     = symbolName;
                callGraph!.AddNode(nodeId, symbolName);
            }
        }
    }

    // Извлечь все вызовы методов из файла (рёбра графа)
    private void ExtractUsages(string sourceCode, string filePath)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var root       = syntaxTree.GetRoot();
        string fileBaseName = Path.GetFileNameWithoutExtension(filePath);

        // Обходим все объявления методов — они могут вызывать другие методы
        foreach (var callerMethod in root.DescendantNodes()
                                          .OfType<MethodDeclarationSyntax>())
        {
            string callerName = $"{fileBaseName}.{callerMethod.Identifier.Text}";
            if (!symbolToNodeId.TryGetValue(callerName, out int callerNodeId))
                continue;

            // Внутри метода ищем все вызовы других методов
            foreach (var invocation in callerMethod.DescendantNodes()
                                                    .OfType<InvocationExpressionSyntax>())
            {
                // Пытаемся определить имя вызываемого метода
                string calleeName = ExtractCalleeName(invocation);
                if (string.IsNullOrEmpty(calleeName)) continue;

                // Ищем узел для вызываемого метода
                var calleeEntry = symbolToNodeId
                    .FirstOrDefault(kv => kv.Key.EndsWith("." + calleeName));

                if (calleeEntry.Key == null) continue;

                int calleeNodeId = calleeEntry.Value;

                // Добавляем ребро: вызывающий → вызываемый
                // Вес ребра = 1.0 (можно усилить если метод вызывается несколько раз)
                callGraph!.AddEdge(callerNodeId, calleeNodeId, weight: 1.0);
            }
        }
    }

    // Извлечь имя вызываемого метода из выражения вызова
    private static string ExtractCalleeName(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            // Доступ к члену: obj.Method() → "Method"
            MemberAccessExpressionSyntax memberAccess =>
                memberAccess.Name.Identifier.Text,

            // Простой вызов: Method() → "Method"
            IdentifierNameSyntax identifier =>
                identifier.Identifier.Text,

            _ => string.Empty
        };
    }

    // Получить ID узла по имени символа (для внешнего использования)
    public int? GetNodeId(string symbolName) =>
        symbolToNodeId.TryGetValue(symbolName, out int id) ? id : null;

    // Получить имя символа по ID узла
    public string? GetSymbolName(int nodeId) =>
        nodeIdToSymbol.TryGetValue(nodeId, out string? name) ? name : null;
}

// SymbolPageRanker.cs
// Вычисляет PageRank для каждого символа в графе вызовов.
// Переиспользует PageRankCalculator из подпроекта 07 (GraphEngine).
public sealed class SymbolPageRanker
{
    // PageRankCalculator из подпроекта 07 — переиспользуем напрямую
    private readonly PageRankCalculator pageRankCalculator;

    // Кеш рангов — пересчитываем только когда граф изменился
    private Dictionary<string, double>? cachedRanks;

    public SymbolPageRanker(PageRankCalculator pageRankCalculator)
    {
        this.pageRankCalculator = pageRankCalculator;
    }

    // Вычислить PageRank для всех символов в графе
    // callGraph      — граф вызовов из CallGraphBuilder
    // callGraphBuilder — для преобразования ID → имена символов
    public OperationResult<Dictionary<string, double>> ComputeRanks(
        KnowledgeGraph    callGraph,
        CallGraphBuilder  callGraphBuilder)
    {
        // Вычисляем PageRank через алгоритм из подпроекта 07
        // Алгоритм: power iteration (итерационный метод степеней)
        // Сходится за ~50 итераций для типичных графов
        var nodeRanks = pageRankCalculator.Compute(callGraph, iterations: 50);

        // Преобразуем числовые ID обратно в имена символов
        var symbolRanks = new Dictionary<string, double>();
        foreach (var (nodeId, rank) in nodeRanks)
        {
            string? symbolName = callGraphBuilder.GetSymbolName(nodeId);
            if (symbolName != null)
                symbolRanks[symbolName] = rank;
        }

        cachedRanks = symbolRanks;
        return OperationResult<Dictionary<string, double>>.Ok(symbolRanks);
    }

    // Получить ранг конкретного символа (по имени)
    // Возвращает 0.0 если символ не найден в кеше
    public double GetRank(string symbolName)
    {
        if (cachedRanks == null) return 0.0;
        return cachedRanks.GetValueOrDefault(symbolName, 0.0);
    }
}
```

**Gate B тесты подпроекта 38:**
- Метод вызываемый из 10 мест получает PageRank выше метода вызываемого из 1 места
- PageRankProvider предлагает методы с высоким рангом выше методов с низким
- Граф из 3 файлов (A→B→C, A→C) → метод C получает наивысший ранг (вызывается дважды)

**Оценка времени:** 6–7 дней

---

## Часть 4 — Диспетчер для всех шести алгоритмов (правило DNA 6)

```csharp
// CompletionDispatcher.cs
// Единый диспетчер для шести алгоритмов автодополнения.
// Правило DNA 6: один диспетчер вместо шести отдельных контроллеров.
// Может использовать один алгоритм или объединять результаты нескольких.
public sealed class CompletionDispatcher : BaseService
{
    // Зарегистрированные поставщики: имя → реализация ICompletionProvider
    private readonly Dictionary<string, ICompletionProvider> providers;

    public CompletionDispatcher(
        Dictionary<string, ICompletionProvider> providers,
        ILogger logger, IOperationContext context)
        : base(logger, context)
    {
        this.providers = providers;
    }

    // Получить подсказки от конкретного алгоритма
    // algorithmName — "StaticAnalysis", "FrequencyRanking", "NGram",
    //                "BM25LocalContext", "TransformerFilter", "PageRankAst"
    public async Task<OperationResult<IReadOnlyList<CompletionSuggestion>>> GetSuggestionsAsync(
        string            algorithmName,
        string            operationId,
        CompletionRequest request)
    {
        var idempotencyCheck = await CheckIdempotencyAsync(operationId);
        if (idempotencyCheck.AlreadyProcessed)
            return idempotencyCheck.CachedResult as OperationResult<IReadOnlyList<CompletionSuggestion>>
                   ?? OperationResult<IReadOnlyList<CompletionSuggestion>>.Fail("Кеш недоступен.", "CACHE_MISS");

        if (!providers.TryGetValue(algorithmName, out var provider))
            return OperationResult<IReadOnlyList<CompletionSuggestion>>.Fail(
                $"Алгоритм '{algorithmName}' не зарегистрирован. " +
                $"Доступные: {string.Join(", ", providers.Keys)}",
                "ALGORITHM_NOT_FOUND");

        return await provider.GetSuggestionsAsync(request);
    }

    // Объединить результаты нескольких алгоритмов (ансамблирование)
    // Каждый алгоритм вносит свой вклад, итоговый score = взвешенная сумма
    public async Task<OperationResult<IReadOnlyList<CompletionSuggestion>>> GetEnsembleSuggestionsAsync(
        string            operationId,
        CompletionRequest request,
        Dictionary<string, double>? algorithmWeights = null)
    {
        // Веса по умолчанию (подобраны экспериментально)
        var weights = algorithmWeights ?? new Dictionary<string, double>
        {
            ["StaticAnalysis"]  = 0.0,   // базовый фильтр, не влияет на ранг
            ["FrequencyRanking"] = 0.35,  // частота использования важна
            ["NGram"]           = 0.15,  // контекст двух предыдущих токенов
            ["BM25LocalContext"] = 0.25,  // локальный контекст проекта важен
            ["TransformerFilter"] = 0.15, // нейросеть умеренно влияет
            ["PageRankAst"]     = 0.10,  // граф зависимостей
        };

        // Получаем кандидатов от статического анализа (список допустимых)
        var staticResult = await providers["StaticAnalysis"].GetSuggestionsAsync(request);
        if (!staticResult.Success)
            return staticResult;

        // Для каждого кандидата вычисляем взвешенную сумму от всех алгоритмов
        var scoreAccumulator = staticResult.Value!
            .ToDictionary(s => s.InsertText, s => 0.0);

        foreach (var (algorithmName, weight) in weights)
        {
            if (weight == 0.0 || !providers.ContainsKey(algorithmName))
                continue;

            var result = await providers[algorithmName].GetSuggestionsAsync(request);
            if (!result.Success)
                continue;

            foreach (var suggestion in result.Value!)
            {
                if (!scoreAccumulator.ContainsKey(suggestion.InsertText))
                    continue;

                // Добавляем взвешенный вклад этого алгоритма
                scoreAccumulator[suggestion.InsertText] += suggestion.Score * weight;
            }
        }

        // Строим финальный список отсортированный по итоговому score
        var finalSuggestions = staticResult.Value!
            .Select(s => s with
            {
                Score           = scoreAccumulator.GetValueOrDefault(s.InsertText, 0.0),
                SourceAlgorithm = "Ensemble",
            })
            .OrderByDescending(s => s.Score)
            .Take(request.MaxSuggestions)
            .ToList();

        return OperationResult<IReadOnlyList<CompletionSuggestion>>.Ok(finalSuggestions);
    }
}
```

---

## Часть 5 — Проверка плана (Plan Review)

### Проверка цели: все ли уровни покрыты?

| Уровень | Описание | Подпроект | Статус |
|---|---|---|---|
| Уровень 1 | LSP-стиль статический анализ | 33 | ✓ |
| Уровень 2А | Ранжирование по частоте вызовов | 34 | ✓ |
| Уровень 2Б | N-граммная модель | 35 | ✓ |
| Уровень 3 | BM25 поиск по локальному контексту | 36 | ✓ |
| Уровень 4 | Трансформер-модель с фильтром | 37 | ✓ |
| Уровень 5 | PageRank на графе зависимостей | 38 | ✓ |

### Переиспользование существующих подпроектов

| Что переиспользуем | Откуда | В каком подпроекте |
|---|---|---|
| Roslyn разбор кода | Подпроект 13 (CodeOptimizer) | 33, 34, 35, 38 |
| InvertedIndex + BM25Scorer | Подпроект 12 (SearchEngine) | 36 |
| KnowledgeGraph + PageRankCalculator | Подпроект 07 (GraphEngine) | 38 |
| CallGraphBuilder | Подпроект 23 (OSSEquivalents) | 38 |
| HnswIndex | Подпроект 06 (VectorSearch) | 36 (опционально) |
| LZ4 сжатие | Подпроект 24 (Compression) | кеш индексов |

### Проверка DNA правил

| Правило | Проверка |
|---|---|
| Правило 1 (нет типизированных моделей) | CompletionRequest, CompletionSuggestion, CodeChunk — sealed record с примитивами ✓ |
| Правило 2 (фильтры отдельным методом) | BuildTokenSearchParameters() отдельно от логики поиска ✓ |
| Правило 3 (OperationResult везде) | Все публичные методы возвращают OperationResult<T> ✓ |
| Правило 4 (BaseService) | FrequencyRankingProvider, ProjectIndexer, CallGraphBuilder extends BaseService ✓ |
| Правило 6 (один диспетчер) | CompletionDispatcher маршрутизирует все запросы ✓ |
| Правило 7 (идемпотентность) | operationId в CollectAsync, BuildGraphAsync, IndexProjectAsync ✓ |
| Правило 8 (хранить перед событием) | SaveAsync до DispatchEventAsync везде ✓ |
| Правило 9 (StandardEvent) | Все события через StandardEvent ✓ |

---

## Итоговая таблица подпроектов

| Подпроект | Уровень | Ключевые компоненты | Переиспользует | Дней |
|---|---|---|---|---|
| 33 | Статический анализ | RoslynContextAnalyzer, TypeMemberEnumerator | Подпроект 13 | 4–5 |
| 34 | Частота вызовов | UsageFrequencyCollector, FrequencyDatabase | Подпроект 13 | 5–6 |
| 35 | N-граммная модель | NGramIndex, NGramTokenizer | Подпроект 13 | 5–7 |
| 36 | BM25 локальный | ProjectIndexer, LocalContextSearcher | Подпроекты 12, 13 | 4–5 |
| 37 | Трансформер + фильтр | FillInTheMiddleBuilder, AllowedTokensFilter | Подпроект 33 | 6–8 |
| 38 | PageRank AST | CallGraphBuilder, SymbolPageRanker | Подпроекты 07, 23 | 6–7 |
| **Итого** | | | | **30–38 дней** |

### Обновлённая итоговая таблица всего Solution

| Диапазон | Описание | Дней |
|---|---|---|
| 00–12 | TensorCore + LLM + Graph RAG | 67 |
| 13 | Code Optimizer | 20 |
| 14–20 | ML Algorithms | 28 |
| 21–23 | Agent Infrastructure | 35 |
| 24–27 | Compression | 24–28 |
| 28–32 | Semantic NLP | 24–29 |
| **33–38** | **Code Completion Algorithms** | **30–38** |
| **Итого** | | **228–245 дней** |

---

*Версия плана 1.0 — подпроекты 33–38, не касается XIIGen*
