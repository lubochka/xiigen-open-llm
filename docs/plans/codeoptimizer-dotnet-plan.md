# Проект 13 — Code Optimizer (оптимизатор кода)
## Solution: LlmFromScratch.sln — Отдельный план подпроекта v1.0

**Проект:** llm-mvp-core — подпроект 13  
**Язык:** C# / .NET 8  
**Ограничение:** Никаких внешних NuGet пакетов, только Microsoft.CodeAnalysis.CSharp (Roslyn) для разбора синтаксиса C#  
**Цель документа:** Пошаговый план реализации набора алгоритмов оптимизации исходного кода на C#

**Этот документ относится только к проекту llm-mvp-core и не касается XIIGen.**

---

## Принципы написания кода в этом проекте

### Принцип 1 — Fabric-first (интерфейс прежде всего)

Каждый компонент который что-то делает с внешним миром (читает файлы, вызывает анализаторы, пишет результаты) должен скрываться за интерфейсом. Код который использует этот компонент знает только об интерфейсе — не о конкретной реализации.

Зачем: если завтра мы захотим заменить способ чтения файлов или добавить кэширование — мы меняем только реализацию за интерфейсом, а всё что использует интерфейс продолжает работать без изменений.

```
Правильно:  ICodeReader → FileSystemCodeReader (конкретная реализация)
Неправильно: Service напрямую открывает файл через File.ReadAllText()
```

### Принцип 2 — OperationResult<T> вместо исключений

Каждый метод который может завершиться с ошибкой возвращает `OperationResult<T>` — контейнер который содержит либо успешный результат, либо описание ошибки. Исключения используются только для ситуаций вроде "кончилась память" — не для бизнес-логики.

Зачем: вызывающий код всегда знает что нужно проверить результат. Ошибки не теряются молча.

```csharp
// Правильно:
public OperationResult<OptimizedCode> Optimize(string sourceCode) { ... }

// Неправильно:
public string Optimize(string sourceCode) { throw new Exception("..."); }
```

### Принцип 3 — Только словари и записи (record) как выходные данные

Все данные которые передаются между компонентами — это простые записи (record) или словари. Никаких сложных иерархий классов с наследованием.

Зачем: просто читать, просто сериализовать, просто тестировать.

### Принцип 4 — Каждая строка кода документирована на человеческом языке

Каждое поле, каждый метод, каждый параметр имеет комментарий на русском языке объясняющий зачем это нужно и что это делает. Никаких аббревиатур без расшифровки.

### Принцип 5 — Идемпотентность (повторный запуск даёт тот же результат)

Запустить оптимизатор дважды на одном файле должно давать тот же результат что и запустить один раз. Никаких побочных эффектов зависящих от порядка выполнения.

---

## Зависимости от других подпроектов

```
01 Tokenizer     — нет прямой зависимости, но токенизатор используется для
                   подсчёта токенов при оценке сложности
12 Search Engine — нет прямой зависимости, но поисковый движок можно использовать
                   для поиска похожих паттернов оптимизации в базе знаний

13 Code Optimizer — полностью независим, можно начинать сразу
```

---

## Структура подпроекта

```
13.CodeOptimizer/
├── Fabric/
│   ├── ICodeReader.cs              — интерфейс чтения исходного кода
│   ├── ICodeWriter.cs              — интерфейс записи оптимизированного кода
│   ├── IOptimizationReporter.cs    — интерфейс отчётности о найденных улучшениях
│   └── FileSystemCodeAccess.cs     — конкретная реализация чтения/записи файлов
├── Core/
│   ├── OperationResult.cs          — контейнер результата (успех или ошибка)
│   ├── OptimizationFinding.cs      — запись об одном найденном улучшении
│   ├── OptimizationReport.cs       — полный отчёт по файлу или проекту
│   └── OptimizationContext.cs      — контекст запуска (какие проверки включены)
├── Analysis/
│   ├── DataFlowAnalyzer.cs         — анализ потока данных (какие переменные живые)
│   └── ControlFlowAnalyzer.cs      — анализ потока управления (какой код достижим)
├── Passes/
│   ├── IOptimizationPass.cs        — интерфейс одного прохода оптимизации
│   ├── DeadCodeEliminator.cs       — удаление мёртвого кода (код который никогда не выполняется)
│   ├── ConstantFolder.cs           — свёртка констант (вычисление 2+2 во время компиляции)
│   ├── StrengthReducer.cs          — замена дорогих операций дешёвыми (x*2 → x<<1)
│   ├── CommonSubexpressionEliminator.cs — устранение повторных вычислений
│   └── LoopOptimizer.cs            — оптимизации внутри циклов
├── Pipeline/
│   └── OptimizationPipeline.cs     — конвейер запуска нескольких проходов по порядку
└── Tests/
    ├── DeadCodeEliminatorTests.cs
    ├── ConstantFolderTests.cs
    ├── StrengthReducerTests.cs
    ├── CommonSubexpressionEliminatorTests.cs
    └── LoopOptimizerTests.cs
```

---

## Источники алгоритмов для изучения

```
LLVM — Apache 2.0 — C++ — весь код открыт
https://github.com/llvm/llvm-project

Конкретные файлы для чтения перед каждой фазой:
Фаза 2 (мёртвый код):   llvm/lib/Transforms/Scalar/DCE.cpp
Фаза 3 (константы):     llvm/lib/Analysis/ConstantFolding.cpp
Фаза 4 (замена):        llvm/lib/Transforms/Scalar/StrengthReductionPass.cpp
Фаза 5 (повторы):       llvm/lib/Transforms/Scalar/GVN.cpp
Фаза 6 (циклы):         llvm/lib/Transforms/Scalar/LICM.cpp

Roslyn (для разбора C#) — Apache 2.0 — C#
https://github.com/dotnet/roslyn

Roslynator (500+ готовых анализаторов C#) — Apache 2.0 — C#
https://github.com/dotnet/roslynator
src/Analyzers/CSharp/Analysis/ — каждый анализатор отдельный класс, читать как примеры
```

---

## Фаза 0 — Интерфейсный слой (fabric-first) (1 день)

Прежде чем писать любую логику — определяем все интерфейсы. Это fabric-first принцип: сначала контракт, потом реализация.

```csharp
// Fabric/ICodeReader.cs
// Интерфейс чтения исходного кода — скрывает от логики оптимизации
// способ получения кода (файловая система, база данных, память, сеть)
public interface ICodeReader
{
    // Прочитать исходный код по указанному пути
    // Возвращает OperationResult — либо текст кода, либо описание ошибки
    // Никогда не выбрасывает исключение — ошибки идут через OperationResult
    OperationResult<string> ReadSource(string path);

    // Получить список всех файлов C# в указанной папке и её подпапках
    // Возвращает пути к файлам отфильтрованные по расширению .cs
    OperationResult<IReadOnlyList<string>> ListSourceFiles(string directoryPath);
}

// Fabric/ICodeWriter.cs
// Интерфейс записи результата оптимизации — скрывает способ сохранения
public interface ICodeWriter
{
    // Записать оптимизированный код по указанному пути
    // overwrite — разрешить ли перезаписать существующий файл
    OperationResult<bool> WriteOptimized(string path, string optimizedCode, bool overwrite);
}

// Fabric/IOptimizationReporter.cs
// Интерфейс отчётности — скрывает куда идут отчёты (консоль, файл, база)
public interface IOptimizationReporter
{
    // Записать одно найденное улучшение
    void ReportFinding(OptimizationFinding finding);

    // Записать итоговый отчёт по файлу
    void ReportFileSummary(OptimizationReport report);
}

// Fabric/FileSystemCodeAccess.cs
// Конкретная реализация ICodeReader и ICodeWriter через файловую систему
// Это единственное место в подпроекте где используется File.ReadAllText и подобные
public class FileSystemCodeAccess : ICodeReader, ICodeWriter
{
    public OperationResult<string> ReadSource(string path)
    {
        // Проверяем что путь не пустой — пустой путь это ошибка конфигурации
        if (string.IsNullOrWhiteSpace(path))
            return OperationResult<string>.Failure("Путь к файлу не указан");

        // Проверяем что файл существует — отсутствующий файл не должно быть исключением
        if (!File.Exists(path))
            return OperationResult<string>.Failure($"Файл не найден: {path}");

        // Читаем содержимое файла в кодировке UTF-8
        // Оборачиваем в try/catch только для системных ошибок (нет прав, диск занят)
        try
        {
            var content = File.ReadAllText(path, Encoding.UTF8);
            return OperationResult<string>.Success(content);
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Failure($"Ошибка чтения файла {path}: {ex.Message}");
        }
    }

    public OperationResult<IReadOnlyList<string>> ListSourceFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return OperationResult<IReadOnlyList<string>>.Failure($"Папка не найдена: {directoryPath}");

        // Ищем все .cs файлы рекурсивно, исключаем папки bin и obj
        // которые содержат скомпилированные артефакты а не исходный код
        var files = Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .ToList();

        return OperationResult<IReadOnlyList<string>>.Success(files);
    }

    public OperationResult<bool> WriteOptimized(string path, string optimizedCode, bool overwrite)
    {
        if (!overwrite && File.Exists(path))
            return OperationResult<bool>.Failure($"Файл уже существует и перезапись запрещена: {path}");

        try
        {
            File.WriteAllText(path, optimizedCode, Encoding.UTF8);
            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure($"Ошибка записи файла {path}: {ex.Message}");
        }
    }
}
```

### Фаза 0 — что написать для тестов

- Создать фиктивный (mock) ICodeReader который возвращает заранее заданный код из словаря
- Убедиться что FileSystemCodeAccess правильно возвращает ошибку для несуществующего файла
- Убедиться что список файлов не включает пути из папок bin и obj

**Выход из фазы:** все тесты зелёные, `dotnet build` без предупреждений.

---

## Фаза 1 — Базовые типы данных (1 день)

```csharp
// Core/OperationResult.cs
// Контейнер результата операции — заменяет исключения для бизнес-ошибок
// T — тип успешного результата (например string, int, OptimizationReport)
public sealed class OperationResult<T>
{
    // Значение при успехе — null если операция завершилась с ошибкой
    public T? Value { get; }

    // Сообщение об ошибке — null если операция прошла успешно
    public string? ErrorMessage { get; }

    // Флаг успеха — true значит Value содержит данные, false значит ErrorMessage содержит причину
    public bool IsSuccess { get; }

    // Закрытый конструктор — создание только через фабричные методы Success и Failure
    private OperationResult(T? value, string? errorMessage, bool isSuccess)
    {
        Value        = value;
        ErrorMessage = errorMessage;
        IsSuccess    = isSuccess;
    }

    // Фабричный метод для успешного результата
    // Использовать когда операция прошла как надо
    public static OperationResult<T> Success(T value) =>
        new(value, null, true);

    // Фабричный метод для неуспешного результата
    // Использовать когда что-то пошло не так — вместо throw
    public static OperationResult<T> Failure(string errorMessage) =>
        new(default, errorMessage, false);

    // Преобразовать результат одного типа в результат другого типа
    // Нужно чтобы прокидывать ошибку вверх по цепочке вызовов без потери сообщения
    public OperationResult<TNew> MapError<TNew>() =>
        OperationResult<TNew>.Failure(ErrorMessage ?? "Неизвестная ошибка");
}

// Core/OptimizationFinding.cs
// Запись об одном конкретном найденном улучшении в коде
public sealed record OptimizationFinding
{
    // Тип улучшения — что именно было найдено (например "Мёртвый код", "Свёртка констант")
    public required string OptimizationType { get; init; }

    // Путь к файлу где найдено улучшение
    public required string FilePath { get; init; }

    // Номер строки в файле (начиная с 1) где начинается проблема
    public required int LineNumber { get; init; }

    // Оригинальный фрагмент кода который можно улучшить
    public required string OriginalCode { get; init; }

    // Предлагаемый улучшенный вариант — или null если просто указываем на проблему
    public string? SuggestedReplacement { get; init; }

    // Объяснение на человеческом языке почему это улучшение полезно
    public required string HumanReadableExplanation { get; init; }

    // Насколько уверены в предложении (0.0 — не уверены, 1.0 — очень уверены)
    public required double ConfidenceScore { get; init; }
}

// Core/OptimizationReport.cs
// Полный отчёт об оптимизации одного файла
public sealed record OptimizationReport
{
    // Путь к проанализированному файлу
    public required string FilePath { get; init; }

    // Все найденные улучшения в этом файле
    public required IReadOnlyList<OptimizationFinding> Findings { get; init; }

    // Сколько времени в миллисекундах заняла проверка
    public required long AnalysisDurationMilliseconds { get; init; }

    // Удалось ли разобрать файл без ошибок синтаксиса
    // false если файл не компилируется — тогда Findings может быть неполным
    public required bool ParsedSuccessfully { get; init; }

    // Ошибки разбора если файл содержит синтаксические ошибки
    public IReadOnlyList<string> ParseErrors { get; init; } = Array.Empty<string>();
}

// Core/OptimizationContext.cs
// Контекст одного запуска оптимизатора — настройки что искать
public sealed record OptimizationContext
{
    // Включить ли поиск мёртвого кода (код который никогда не выполняется)
    public bool EnableDeadCodeDetection { get; init; } = true;

    // Включить ли свёртку констант (вычисление выражений из констант во время анализа)
    public bool EnableConstantFolding { get; init; } = true;

    // Включить ли замену дорогих операций дешёвыми (умножение на 2 → сдвиг)
    public bool EnableStrengthReduction { get; init; } = true;

    // Включить ли устранение одинаковых подвыражений (вычислять x+y один раз)
    public bool EnableCommonSubexpressionElimination { get; init; } = true;

    // Включить ли оптимизации цикла (вынос инвариантов, размотка)
    public bool EnableLoopOptimization { get; init; } = true;

    // Минимальная уверенность (от 0.0 до 1.0) для включения улучшения в отчёт
    // Улучшения с уверенностью ниже порога не попадут в отчёт
    public double MinimumConfidenceThreshold { get; init; } = 0.7;
}
```

**Выход из фазы:** все типы компилируются, базовые unit тесты для OperationResult проходят.

---

## Фаза 2 — Анализ потоков данных и управления (3–4 дня)

Перед тем как оптимизировать код нужно понять два вещи: где какие переменные "живые" (анализ потока данных) и какой код вообще достижим (анализ потока управления). Все последующие оптимизации опираются на эти два анализатора.

**Источник для чтения перед написанием:**
```
LLVM: llvm/lib/Analysis/LiveVariables.cpp  — анализ живых переменных
LLVM: llvm/lib/Analysis/CFG.cpp            — граф потока управления
```

```csharp
// Analysis/ControlFlowAnalyzer.cs
// Анализатор потока управления — строит граф блоков кода и определяет
// какой код может быть выполнен а какой никогда не достигается
public class ControlFlowAnalyzer
{
    // Разобрать метод и вернуть множество строк которые достижимы
    // "Достижимая строка" — это строка которая при каком-то входном значении может выполниться
    // "Недостижимая строка" — это строка после return, или в ветке if (false) и т.п.
    //
    // syntaxTree — разобранное Roslyn-ом синтаксическое дерево C# кода
    // semanticModel — семантическая модель Roslyn знающая типы всех выражений
    public OperationResult<IReadOnlySet<int>> GetReachableLines(
        SyntaxTree syntaxTree,          // дерево синтаксиса (структура кода)
        SemanticModel semanticModel)    // семантика (типы, значения, символы)
    {
        var reachableLines = new HashSet<int>();
        var root = syntaxTree.GetRoot();

        // Обходим все методы в файле и для каждого строим граф потока управления
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            AnalyzeMethodReachability(method, semanticModel, reachableLines);
        }

        return OperationResult<IReadOnlySet<int>>.Success(reachableLines);
    }

    // Анализ достижимости одного метода
    // Алгоритм: обход в ширину (BFS) по базовым блокам начиная с первого блока метода
    // Базовый блок — это последовательность инструкций без ветвлений
    private void AnalyzeMethodReachability(
        MethodDeclarationSyntax method,         // узел синтаксического дерева — метод
        SemanticModel semanticModel,             // семантическая модель для вычисления констант
        HashSet<int> reachableLines)             // множество куда добавляем достижимые строки
    {
        // Получаем граф потока управления через Roslyn flow analysis
        // Roslyn сам строит граф для нас — нам остаётся его обойти
        var controlFlow = semanticModel.AnalyzeControlFlow(
            method.Body?.Statements.First(),    // от первого оператора метода
            method.Body?.Statements.Last()      // до последнего оператора метода
        );

        if (controlFlow == null) return;

        // Все операторы которые Roslyn считает достижимыми добавляем в результат
        // Используем номер строки как идентификатор
        foreach (var statement in controlFlow.EntryPointIsReachable
            ? method.Body?.Statements ?? []
            : [])
        {
            var lineSpan = statement.GetLocation().GetLineSpan();
            for (int line = lineSpan.StartLinePosition.Line + 1;
                      line <= lineSpan.EndLinePosition.Line + 1;
                      line++)
            {
                reachableLines.Add(line);
            }
        }
    }
}

// Analysis/DataFlowAnalyzer.cs
// Анализатор потока данных — определяет в каждой точке программы
// какие переменные содержат "живые" значения (значения которые ещё будут использованы)
//
// "Живая переменная" в точке P — переменная чьё значение будет прочитано
// на каком-то пути выполнения начиная с точки P
// "Мёртвая переменная" — переменная чьё значение никогда не будет прочитано
// (можно не хранить и не вычислять)
public class DataFlowAnalyzer
{
    // Найти все присваивания переменным чьи значения никогда не используются
    // Алгоритм backward liveness analysis: идём от конца метода к началу,
    // отмечаем переменные как живые когда их читают,
    // убираем из живых когда в них пишут без последующего чтения
    //
    // Источник: LLVM/lib/Transforms/Scalar/DCE.cpp — алгоритм dead store elimination
    public OperationResult<IReadOnlyList<OptimizationFinding>> FindDeadAssignments(
        SyntaxTree syntaxTree,          // синтаксическое дерево разобранного C# файла
        SemanticModel semanticModel,    // семантика — нужна для разрешения символов
        string filePath)                // путь к файлу — нужен для отчётности
    {
        var findings = new List<OptimizationFinding>();
        var root = syntaxTree.GetRoot();

        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            // Roslyn предоставляет готовый анализ потока данных для метода
            // Мы используем его вместо того чтобы реализовывать алгоритм с нуля
            var dataFlow = semanticModel.AnalyzeDataFlow(
                method.Body ?? (SyntaxNode)method.ExpressionBody!.Expression
            );

            if (dataFlow == null) continue;

            // AlwaysAssigned — переменные которым всегда присваивается значение
            // ReadInside — переменные которые читаются внутри анализируемого блока
            // Разница между ними — переменные которым присваивают но никогда не читают
            var assignedButNeverRead = dataFlow.AlwaysAssigned
                .Except(dataFlow.ReadInside)
                .Except(dataFlow.ReadOutside);  // исключаем переменные которые читают снаружи

            foreach (var symbol in assignedButNeverRead)
            {
                // Находим строку где это присваивание происходит
                var assignmentLine = FindAssignmentLine(method, symbol, semanticModel);

                findings.Add(new OptimizationFinding
                {
                    OptimizationType = "Мёртвое присваивание (переменной присваивается значение которое никогда не читается)",
                    FilePath         = filePath,
                    LineNumber       = assignmentLine,
                    OriginalCode     = symbol.Name,
                    SuggestedReplacement = null,  // нельзя автоматически удалить — нужна проверка
                    HumanReadableExplanation =
                        $"Переменная '{symbol.Name}' получает значение но это значение " +
                        $"никогда не используется. Можно рассмотреть удаление присваивания " +
                        $"или всей переменной если она больше нигде не нужна.",
                    ConfidenceScore = 0.85  // высокая уверенность но не 1.0 — могут быть edge cases
                });
            }
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(findings);
    }

    // Найти номер строки где происходит присваивание символу в методе
    // Обходим все узлы AssignmentExpression в методе и ищем тот что относится к символу
    private int FindAssignmentLine(
        MethodDeclarationSyntax method,     // метод в котором ищем
        ISymbol targetSymbol,               // символ (переменная) которую ищем
        SemanticModel model)                // семантика для разрешения символов
    {
        foreach (var assignment in method.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            var leftSymbol = model.GetSymbolInfo(assignment.Left).Symbol;
            if (SymbolEqualityComparer.Default.Equals(leftSymbol, targetSymbol))
                return assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        }
        return 0;  // 0 означает "строка не найдена" — не должно происходить
    }
}
```

**Тесты для фазы 2:**
- Код с `return` в середине метода — строки после return должны быть недостижимы
- Код с `if (true) { ... } else { ... }` — ветка else должна быть недостижима
- Метод где переменная `x` получает значение но никогда не используется — должен быть найден в мёртвых присваиваниях

**Выход из фазы:** тесты проходят, анализаторы корректно работают на 5 тестовых примерах.

---

## Фаза 3 — Устранение мёртвого кода (2 дня)

Мёртвый код — это код который никогда не выполняется. Источник алгоритма: `llvm/lib/Transforms/Scalar/DCE.cpp`

```csharp
// Passes/IOptimizationPass.cs
// Интерфейс одного прохода оптимизации — fabric-first для всех проходов
// Каждый конкретный алгоритм оптимизации реализует этот интерфейс
public interface IOptimizationPass
{
    // Человекочитаемое название этого прохода оптимизации
    // Используется в отчётах и логах
    string PassName { get; }

    // Выполнить проход оптимизации на разобранном синтаксическом дереве
    // syntaxTree — дерево синтаксиса C# файла разобранное Roslyn-ом
    // semanticModel — семантическая информация (типы переменных, значения констант)
    // filePath — путь к файлу для указания в отчёте
    // context — настройки запуска (включены ли отдельные виды оптимизаций)
    // Возвращает список найденных улучшений
    OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        string filePath,
        OptimizationContext context);
}

// Passes/DeadCodeEliminator.cs
// Проход устранения мёртвого кода
// Мёртвый код бывает трёх видов:
// 1. Код после безусловного return/throw — никогда не выполнится
// 2. Код в ветке if с заведомо ложным условием if (false) — никогда не выполнится
// 3. Присваивания переменным значения которых никогда не читаются
//
// Алгоритм основан на: LLVM DCE.cpp + Roslyn ControlFlow/DataFlow analysis
public class DeadCodeEliminator : IOptimizationPass
{
    // Название прохода для отчётов
    public string PassName => "Устранение мёртвого кода (Dead Code Elimination — поиск кода который никогда не выполняется)";

    private readonly ControlFlowAnalyzer controlFlow;  // анализатор достижимости строк
    private readonly DataFlowAnalyzer dataFlow;          // анализатор живости переменных

    // Конструктор принимает анализаторы через параметры — fabric-first принцип
    // Не создаём анализаторы сами — получаем снаружи чтобы можно было заменить на тесты
    public DeadCodeEliminator(ControlFlowAnalyzer controlFlow, DataFlowAnalyzer dataFlow)
    {
        this.controlFlow = controlFlow;
        this.dataFlow    = dataFlow;
    }

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        string filePath,
        OptimizationContext context)
    {
        // Если поиск мёртвого кода отключён в настройках — возвращаем пустой список
        if (!context.EnableDeadCodeDetection)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();

        // Шаг 1: найти недостижимые строки через анализ потока управления
        var reachableResult = controlFlow.GetReachableLines(syntaxTree, semanticModel);
        if (!reachableResult.IsSuccess)
            return reachableResult.MapError<IReadOnlyList<OptimizationFinding>>();

        var reachableLines = reachableResult.Value!;

        // Шаг 2: все строки с кодом которые не в множестве достижимых — мёртвые
        var root = syntaxTree.GetRoot();
        foreach (var statement in root.DescendantNodes().OfType<StatementSyntax>())
        {
            var lineNumber = statement.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            // Пропускаем строки которые достижимы — они живые
            if (reachableLines.Contains(lineNumber)) continue;

            // Пропускаем пустые операторы и фигурные скобки — они не несут логики
            if (statement is BlockSyntax || statement is EmptyStatementSyntax) continue;

            var originalCode = statement.ToString().Trim();

            // Проверяем что уверенность выше порога из настроек
            const double deadCodeConfidence = 0.9;  // высокая уверенность для явно недостижимого кода
            if (deadCodeConfidence < context.MinimumConfidenceThreshold) continue;

            findings.Add(new OptimizationFinding
            {
                OptimizationType = "Мёртвый код (код который никогда не выполняется)",
                FilePath         = filePath,
                LineNumber       = lineNumber,
                OriginalCode     = originalCode.Length > 100
                    ? originalCode[..100] + "..."  // обрезаем длинные строки для читаемости
                    : originalCode,
                SuggestedReplacement     = null,   // удалить — но только после проверки человеком
                HumanReadableExplanation =
                    $"Строка {lineNumber} никогда не будет выполнена. " +
                    $"Скорее всего она стоит после return, break, throw, " +
                    $"или находится в заведомо ложной ветке условия.",
                ConfidenceScore = deadCodeConfidence
            });
        }

        // Шаг 3: найти мёртвые присваивания через анализ потока данных
        var deadAssignmentsResult = dataFlow.FindDeadAssignments(syntaxTree, semanticModel, filePath);
        if (!deadAssignmentsResult.IsSuccess)
            return deadAssignmentsResult;

        findings.AddRange(deadAssignmentsResult.Value!
            .Where(f => f.ConfidenceScore >= context.MinimumConfidenceThreshold));

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(findings);
    }
}
```

**Тесты для фазы 3:**
```csharp
// Тест 1: код после безусловного return должен быть найден как мёртвый
const string testCode1 = @"
    public void Method() {
        return;
        Console.WriteLine(""Никогда не выполнится"");  // должно быть найдено
    }";

// Тест 2: код в else после if (true) должен быть найден как мёртвый
const string testCode2 = @"
    public void Method() {
        if (true) { Console.WriteLine(""Всегда""); }
        else { Console.WriteLine(""Никогда""); }  // должно быть найдено
    }";

// Тест 3: переменная которой присваивают но никогда не читают
const string testCode3 = @"
    public void Method() {
        int x = 42;  // должно быть найдено — x никогда не используется
        Console.WriteLine(""hello"");
    }";
```

**Выход из фазы:** все три теста проходят, Gate A выполнен.

---

## Фаза 4 — Свёртка констант (2 дня)

Свёртка констант (Constant Folding — вычисление выражений из постоянных значений во время анализа а не во время выполнения) — если в коде написано `2 + 2` или `"hello" + " " + "world"` — можно заменить на `4` и `"hello world"` сразу при компиляции.

**Источник для чтения:** `llvm/lib/Analysis/ConstantFolding.cpp`

```csharp
// Passes/ConstantFolder.cs
// Проход свёртки констант
// Ищет арифметические и строковые выражения состоящие только из константных значений
// и предлагает заменить их на результат вычисления
public class ConstantFolder : IOptimizationPass
{
    public string PassName =>
        "Свёртка констант (Constant Folding — замена вычислений из констант на готовый результат)";

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        string filePath,
        OptimizationContext context)
    {
        if (!context.EnableConstantFolding)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root = syntaxTree.GetRoot();

        // Обходим все бинарные выражения (a OP b) в коде
        foreach (var binaryExpr in root.DescendantNodes().OfType<BinaryExpressionSyntax>())
        {
            // Спрашиваем Roslyn: является ли всё это выражение константным?
            // Roslyn умеет вычислять константные выражения сам
            var constantValue = semanticModel.GetConstantValue(binaryExpr);

            // Если выражение не константное — пропускаем
            if (!constantValue.HasValue) continue;

            var lineNumber  = binaryExpr.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var originalCode = binaryExpr.ToString().Trim();
            var resultValue  = constantValue.Value?.ToString() ?? "null";

            // Добавляем кавычки для строковых результатов чтобы замена была синтаксически корректна
            var suggestion = constantValue.Value is string
                ? $"\"{resultValue}\""
                : resultValue;

            // Не сообщаем об уже простых литералах — только о вычисляемых выражениях
            // "hello" уже свёрнуто, "hello" + " " нет
            if (binaryExpr.Left is LiteralExpressionSyntax && binaryExpr.Right is LiteralExpressionSyntax)
            {
                findings.Add(new OptimizationFinding
                {
                    OptimizationType = "Свёртка констант (вычисление постоянного выражения)",
                    FilePath         = filePath,
                    LineNumber       = lineNumber,
                    OriginalCode     = originalCode,
                    SuggestedReplacement     = suggestion,
                    HumanReadableExplanation =
                        $"Выражение '{originalCode}' всегда равно {suggestion}. " +
                        $"Можно заменить на константу — это уберёт лишнее вычисление при каждом вызове.",
                    ConfidenceScore = 0.95  // очень высокая — Roslyn это вычислил точно
                });
            }
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(findings);
    }
}
```

**Тесты для фазы 4:**
- `2 + 2` → должно быть найдено и предложена замена на `4`
- `"hello" + " " + "world"` → должно быть найдено, предложена замена на `"hello world"`
- `Math.PI * 2` → не должно быть найдено (Math.PI — не литерал)
- `x + 2` где x — переменная → не должно быть найдено

**Выход из фазы:** тесты проходят, Gate A выполнен.

---

## Фаза 5 — Замена дорогих операций дешёвыми (2 дня)

Замена дорогих операций дешёвыми (Strength Reduction — уменьшение "веса" вычислений) — умножение на степень двойки заменяется битовым сдвигом, деление на константу заменяется умножением на обратное значение.

**Источник для чтения:** `llvm/lib/Transforms/Scalar/StrengthReductionPass.cpp`

```csharp
// Passes/StrengthReducer.cs
// Проход замены дорогих операций дешёвыми
// Умножение — дорогая операция (несколько тактов процессора)
// Битовый сдвиг — дешёвая операция (один такт)
// x * 2 == x << 1 (сдвиг влево на 1 бит == умножение на 2)
// x * 4 == x << 2, x * 8 == x << 3 и т.д.
public class StrengthReducer : IOptimizationPass
{
    public string PassName =>
        "Замена дорогих операций дешёвыми (Strength Reduction — замена умножения на битовый сдвиг там где это применимо)";

    // Таблица замен: умножитель → количество бит для сдвига
    // Умножение на 2^n заменяется сдвигом влево на n позиций
    private static readonly Dictionary<long, int> PowerOfTwoShifts = new()
    {
        { 2,   1 },   // x * 2   == x << 1
        { 4,   2 },   // x * 4   == x << 2
        { 8,   3 },   // x * 8   == x << 3
        { 16,  4 },   // x * 16  == x << 4
        { 32,  5 },   // x * 32  == x << 5
        { 64,  6 },   // x * 64  == x << 6
        { 128, 7 },   // x * 128 == x << 7
        { 256, 8 },   // x * 256 == x << 8
    };

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        string filePath,
        OptimizationContext context)
    {
        if (!context.EnableStrengthReduction)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root = syntaxTree.GetRoot();

        // Ищем все операции умножения (*)
        foreach (var multiplication in root.DescendantNodes()
            .OfType<BinaryExpressionSyntax>()
            .Where(e => e.IsKind(SyntaxKind.MultiplyExpression)))
        {
            var lineNumber = multiplication.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            // Проверяем правую часть: является ли множитель степенью двойки?
            var rightConstant = semanticModel.GetConstantValue(multiplication.Right);
            if (!rightConstant.HasValue) continue;

            // Приводим значение к long для единообразной проверки
            if (rightConstant.Value is not (int or long or short)) continue;
            var multiplierValue = Convert.ToInt64(rightConstant.Value);

            if (!PowerOfTwoShifts.TryGetValue(multiplierValue, out int shiftAmount)) continue;

            var leftOperand  = multiplication.Left.ToString().Trim();
            var originalCode = multiplication.ToString().Trim();
            var suggestion   = $"({leftOperand} << {shiftAmount})";

            findings.Add(new OptimizationFinding
            {
                OptimizationType = "Замена умножения на битовый сдвиг (Strength Reduction)",
                FilePath         = filePath,
                LineNumber       = lineNumber,
                OriginalCode     = originalCode,
                SuggestedReplacement     = suggestion,
                HumanReadableExplanation =
                    $"'{originalCode}' можно заменить на '{suggestion}'. " +
                    $"Битовый сдвиг влево на {shiftAmount} позиций делает то же самое " +
                    $"что умножение на {multiplierValue}, но выполняется быстрее. " +
                    $"Примечание: применимо только к целым числам, проверьте что " +
                    $"'{leftOperand}' не может быть отрицательным если это важно.",
                ConfidenceScore = 0.80  // хорошая уверенность, но нужна проверка знака
            });
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(findings);
    }
}
```

**Тесты для фазы 5:**
- `x * 2` → предложена замена на `(x << 1)`
- `count * 8` → предложена замена на `(count << 3)`
- `x * 3` → не должно быть найдено (3 не является степенью двойки)
- `2 * x` → не должно быть найдено (константа слева, а не справа)

**Выход из фазы:** тесты проходят, Gate A выполнен.

---

## Фаза 6 — Устранение повторных вычислений (3 дня)

Устранение повторных вычислений (Common Subexpression Elimination — устранение общих подвыражений) — если одно и то же выражение вычисляется несколько раз и между вычислениями ничего не изменилось — достаточно вычислить один раз и запомнить результат.

**Источник для чтения:** `llvm/lib/Transforms/Scalar/GVN.cpp` (Global Value Numbering — глобальная нумерация значений, алгоритм основанный на уникальном номере для каждого уникального вычисленного значения)

```csharp
// Passes/CommonSubexpressionEliminator.cs
// Проход устранения повторных вычислений
// Ищет одинаковые выражения которые встречаются несколько раз в одном методе
// и предлагает вычислить их один раз и сохранить в переменную
public class CommonSubexpressionEliminator : IOptimizationPass
{
    public string PassName =>
        "Устранение повторных вычислений (Common Subexpression Elimination — вычислить одинаковое выражение один раз)";

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        string filePath,
        OptimizationContext context)
    {
        if (!context.EnableCommonSubexpressionElimination)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root = syntaxTree.GetRoot();

        // Анализируем каждый метод отдельно — за пределы метода не выходим
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            // Словарь: текст выражения → список мест где оно встречается
            var expressionOccurrences = new Dictionary<string, List<int>>();

            // Собираем все вызовы методов и бинарные выражения
            // Это самые распространённые кандидаты на устранение повторений
            var candidateExpressions = method.DescendantNodes()
                .Where(n => n is InvocationExpressionSyntax   // вызовы методов: Math.Sqrt(x)
                         || n is BinaryExpressionSyntax        // бинарные: a + b, a * b
                         || n is MemberAccessExpressionSyntax) // обращения к полям: obj.Length
                .OfType<ExpressionSyntax>();

            foreach (var expr in candidateExpressions)
            {
                // Нормализуем текст выражения — убираем пробелы для сравнения
                var normalizedText = NormalizeExpression(expr.ToString());

                // Пропускаем простые идентификаторы (x, y, count) — они уже "дешёвые"
                if (IsSimpleIdentifier(expr)) continue;

                // Пропускаем литералы (42, "hello") — они тоже дешёвые
                if (expr is LiteralExpressionSyntax) continue;

                var lineNumber = expr.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                if (!expressionOccurrences.ContainsKey(normalizedText))
                    expressionOccurrences[normalizedText] = new List<int>();

                expressionOccurrences[normalizedText].Add(lineNumber);
            }

            // Отчитываемся только о тех выражениях что встречаются больше одного раза
            foreach (var (expressionText, lines) in expressionOccurrences)
            {
                if (lines.Count < 2) continue;  // встречается только раз — не повтор

                // Генерируем имя переменной для кэширования результата
                var suggestedVariableName = GenerateVariableName(expressionText);
                var linesList = string.Join(", ", lines);

                findings.Add(new OptimizationFinding
                {
                    OptimizationType = "Повторное вычисление одного выражения (Common Subexpression)",
                    FilePath         = filePath,
                    LineNumber       = lines[0],  // указываем на первое вхождение
                    OriginalCode     = expressionText,
                    SuggestedReplacement =
                        $"var {suggestedVariableName} = {expressionText}; // вычислить один раз",
                    HumanReadableExplanation =
                        $"Выражение '{expressionText}' вычисляется {lines.Count} раз " +
                        $"на строках {linesList}. " +
                        $"Если значение между этими вызовами не меняется — " +
                        $"можно сохранить результат в переменную и использовать её.",
                    ConfidenceScore = 0.65  // средняя уверенность — нужна проверка что значение не изменилось
                });
            }
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(findings);
    }

    // Нормализовать текст выражения для сравнения
    // Убираем лишние пробелы чтобы "a + b" и "a+b" считались одним выражением
    private static string NormalizeExpression(string text) =>
        string.Join(" ", text.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    // Проверить является ли узел простым идентификатором (x, count, name)
    private static bool IsSimpleIdentifier(ExpressionSyntax expr) =>
        expr is IdentifierNameSyntax;

    // Сгенерировать читаемое имя переменной для кэшированного результата
    // Math.Sqrt(x) → sqrtResult, items.Count → countResult
    private static string GenerateVariableName(string expressionText)
    {
        // Берём последнее слово из выражения и добавляем "Result"
        var lastWord = expressionText
            .Split('.', '(', ')', ' ', '+', '-', '*', '/')
            .LastOrDefault(w => !string.IsNullOrEmpty(w) && char.IsLetter(w[0]))
            ?? "cached";

        return char.ToLower(lastWord[0]) + lastWord[1..] + "Result";
    }
}
```

**Тесты для фазы 6:**
- Метод где `Math.Sqrt(x)` вызывается дважды → должно быть найдено
- Метод где `items.Count` используется трижды → должно быть найдено
- Метод где `x + 1` встречается только один раз → не должно быть найдено

**Выход из фазы:** тесты проходят, Gate A выполнен.

---

## Фаза 7 — Оптимизации циклов (3 дня)

Оптимизации циклов — самое важное место для производительности. Двум алгоритмам мы уделяем особое внимание: вынос инвариантов цикла (перенос вычислений которые не меняются в теле цикла — за пределы цикла) и обнаружение лишних операций в условии завершения цикла.

**Источник для чтения:** `llvm/lib/Transforms/Scalar/LICM.cpp` (Loop Invariant Code Motion — перемещение инвариантного кода цикла)

```csharp
// Passes/LoopOptimizer.cs
// Проход оптимизации циклов
// Два вида оптимизаций:
// 1. Вынос инвариантов — вычисления которые дают одинаковый результат на каждой итерации
//    должны быть вынесены за пределы цикла
// 2. Кэширование свойства Count/Length — частая ошибка: вычислять Count коллекции
//    на каждой итерации цикла вместо один раз до цикла
public class LoopOptimizer : IOptimizationPass
{
    public string PassName =>
        "Оптимизация циклов (Loop Optimization — вынос постоянных вычислений за пределы цикла)";

    public OperationResult<IReadOnlyList<OptimizationFinding>> Execute(
        SyntaxTree syntaxTree,
        SemanticModel semanticModel,
        string filePath,
        OptimizationContext context)
    {
        if (!context.EnableLoopOptimization)
            return OperationResult<IReadOnlyList<OptimizationFinding>>.Success([]);

        var findings = new List<OptimizationFinding>();
        var root = syntaxTree.GetRoot();

        // Обрабатываем все виды циклов: for, while, foreach, do-while
        foreach (var loop in root.DescendantNodes().Where(n =>
            n is ForStatementSyntax      ||  // for (int i = 0; ...)
            n is WhileStatementSyntax    ||  // while (condition)
            n is ForEachStatementSyntax  ||  // foreach (var item in collection)
            n is DoStatementSyntax))         // do { } while (condition)
        {
            // Ищем Count/Length в условии for-цикла — типичная проблема
            if (loop is ForStatementSyntax forLoop)
            {
                FindCountInForCondition(forLoop, filePath, findings);
            }

            // Ищем вызовы методов в теле цикла которые не зависят от переменной итерации
            FindLoopInvariantCalls(loop, semanticModel, filePath, findings);
        }

        return OperationResult<IReadOnlyList<OptimizationFinding>>.Success(findings);
    }

    // Найти вычисление Count или Length в условии for-цикла
    // for (int i = 0; i < items.Count; i++) — Count вычисляется на каждой итерации
    // for (int i = 0; i < count; i++)       — count вычислен один раз до цикла
    private void FindCountInForCondition(
        ForStatementSyntax forLoop,         // синтаксический узел for-цикла
        string filePath,                    // путь к файлу для отчёта
        List<OptimizationFinding> findings) // список куда добавляем находки
    {
        var condition = forLoop.Condition;
        if (condition == null) return;

        // Ищем обращения к .Count или .Length в условии цикла
        foreach (var memberAccess in condition.DescendantNodesAndSelf()
            .OfType<MemberAccessExpressionSyntax>()
            .Where(m => m.Name.Identifier.Text is "Count" or "Length"))
        {
            var lineNumber   = forLoop.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var originalCode = memberAccess.ToString().Trim();

            findings.Add(new OptimizationFinding
            {
                OptimizationType = "Повторное вычисление Count/Length в условии цикла",
                FilePath         = filePath,
                LineNumber       = lineNumber,
                OriginalCode     = $"for (...; i < {originalCode}; ...)",
                SuggestedReplacement =
                    $"int count = {originalCode};\nfor (...; i < count; ...)",
                HumanReadableExplanation =
                    $"'{originalCode}' вычисляется заново на каждой итерации цикла. " +
                    $"Вынесите это значение в переменную до цикла: " +
                    $"'int count = {originalCode};' — и используйте 'count' в условии. " +
                    $"Для неизменяемых коллекций это безопасно и ускоряет цикл.",
                ConfidenceScore = 0.85
            });
        }
    }

    // Найти вызовы методов в теле цикла которые не зависят от переменной итерации
    // Это упрощённая версия LICM — полная реализация требует анализа зависимостей
    private void FindLoopInvariantCalls(
        SyntaxNode loop,                    // синтаксический узел любого цикла
        SemanticModel semanticModel,         // семантика для определения чистоты метода
        string filePath,                    // путь к файлу для отчёта
        List<OptimizationFinding> findings) // список куда добавляем находки
    {
        // Получаем имена переменных объявленных в заголовке for-цикла (обычно i, j)
        var loopVariables = GetLoopIterationVariables(loop);

        // Ищем вызовы методов в теле цикла
        var body = GetLoopBody(loop);
        if (body == null) return;

        foreach (var call in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            // Проверяем что вызов не использует переменные цикла
            var usedIdentifiers = call.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(id => id.Identifier.Text)
                .ToHashSet();

            bool usesLoopVariables = usedIdentifiers.Any(id => loopVariables.Contains(id));
            if (usesLoopVariables) continue;  // зависит от итерации — нельзя вынести

            var lineNumber   = call.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            var originalCode = call.ToString().Trim();

            // Добавляем только уверенные случаи — вызовы без аргументов как Math.GetHashCode()
            // или с только константными аргументами
            if (call.ArgumentList.Arguments.Count == 0)
            {
                findings.Add(new OptimizationFinding
                {
                    OptimizationType = "Инвариант цикла (Loop Invariant — вычисление которое не меняется в цикле)",
                    FilePath         = filePath,
                    LineNumber       = lineNumber,
                    OriginalCode     = originalCode,
                    SuggestedReplacement = $"// Вынесите '{originalCode}' за пределы цикла",
                    HumanReadableExplanation =
                        $"Вызов '{originalCode}' в строке {lineNumber} не зависит от " +
                        $"переменной цикла. Если результат не меняется от итерации к итерации — " +
                        $"вынесите вызов за пределы цикла чтобы выполнять его один раз.",
                    ConfidenceScore = 0.70  // умеренная уверенность — нужна ручная проверка
                });
            }
        }
    }

    // Получить имена переменных объявленных в заголовке for-цикла (i, j, index)
    private static HashSet<string> GetLoopIterationVariables(SyntaxNode loop)
    {
        if (loop is ForStatementSyntax forLoop)
        {
            return forLoop.Declaration?.Variables
                .Select(v => v.Identifier.Text)
                .ToHashSet() ?? new HashSet<string>();
        }
        if (loop is ForEachStatementSyntax foreachLoop)
            return new HashSet<string> { foreachLoop.Identifier.Text };

        return new HashSet<string>();
    }

    // Получить тело цикла как синтаксический узел
    private static SyntaxNode? GetLoopBody(SyntaxNode loop) => loop switch
    {
        ForStatementSyntax f       => f.Statement,
        WhileStatementSyntax w     => w.Statement,
        ForEachStatementSyntax fe  => fe.Statement,
        DoStatementSyntax d        => d.Statement,
        _                          => null
    };
}
```

**Тесты для фазы 7:**
- `for (int i = 0; i < items.Count; i++)` → должно быть найдено, предложено вынести Count
- `for (int i = 0; i < n; i++) { Math.PI; }` → Math.PI вынести за цикл
- `for (int i = 0; i < n; i++) { DoWork(i); }` → DoWork(i) не трогать, зависит от i

**Выход из фазы:** тесты проходят, Gate A выполнен.

---

## Фаза 8 — Конвейер оптимизаций и интеграция (2 дня)

```csharp
// Pipeline/OptimizationPipeline.cs
// Конвейер оптимизаций — запускает все проходы по порядку на одном файле
// или на всём проекте и собирает общий отчёт
public class OptimizationPipeline
{
    // Все зарегистрированные проходы оптимизации — fabric-first, получаем снаружи
    private readonly IReadOnlyList<IOptimizationPass> passes;

    // Интерфейс чтения кода — fabric-first, не знаем откуда берётся код
    private readonly ICodeReader codeReader;

    // Интерфейс отчётности — fabric-first, не знаем куда идут отчёты
    private readonly IOptimizationReporter reporter;

    // Конструктор — все зависимости передаются снаружи, ни одна не создаётся внутри
    public OptimizationPipeline(
        IReadOnlyList<IOptimizationPass> passes,    // список проходов которые нужно применить
        ICodeReader codeReader,                      // как читать файлы
        IOptimizationReporter reporter)              // куда отправлять результаты
    {
        this.passes     = passes;
        this.codeReader = codeReader;
        this.reporter   = reporter;
    }

    // Запустить анализ одного файла
    // Возвращает полный отчёт со всеми найденными улучшениями
    public OperationResult<OptimizationReport> AnalyzeFile(
        string filePath,            // путь к C# файлу который нужно проанализировать
        OptimizationContext context) // настройки — какие проверки включены
    {
        var startTime = Stopwatch.GetTimestamp();

        // Шаг 1: читаем исходный код через интерфейс
        var readResult = codeReader.ReadSource(filePath);
        if (!readResult.IsSuccess)
            return readResult.MapError<OptimizationReport>();

        // Шаг 2: разбираем C# код через Roslyn в синтаксическое дерево
        // SyntaxTree — дерево структуры кода (блоки, методы, выражения)
        var syntaxTree = CSharpSyntaxTree.ParseText(readResult.Value!);

        // Шаг 3: создаём компиляцию для получения семантической информации
        // Без компиляции Roslyn не знает типы переменных и значения констант
        var compilation = CSharpCompilation.Create(
            assemblyName: "Analysis",           // имя сборки — только для анализа
            syntaxTrees: [syntaxTree],           // анализируем только этот файл
            references:                          // ссылки на стандартную библиотеку .NET
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            ]
        );

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Шаг 4: проверяем были ли ошибки синтаксиса
        var parseErrors = syntaxTree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.GetMessage())
            .ToList();

        // Шаг 5: запускаем каждый проход по очереди и собираем все находки
        var allFindings = new List<OptimizationFinding>();

        foreach (var pass in passes)
        {
            var passResult = pass.Execute(syntaxTree, semanticModel, filePath, context);

            if (!passResult.IsSuccess)
            {
                // Один проход упал — логируем и идём дальше, не останавливаем всё
                reporter.ReportFinding(new OptimizationFinding
                {
                    OptimizationType = "Ошибка прохода оптимизации",
                    FilePath         = filePath,
                    LineNumber       = 0,
                    OriginalCode     = pass.PassName,
                    HumanReadableExplanation =
                        $"Проход '{pass.PassName}' завершился с ошибкой: {passResult.ErrorMessage}",
                    ConfidenceScore = 1.0
                });
                continue;
            }

            allFindings.AddRange(passResult.Value!);
        }

        // Шаг 6: сортируем находки по номеру строки для удобства чтения
        var sortedFindings = allFindings
            .OrderBy(f => f.LineNumber)
            .ToList();

        var elapsedMs = Stopwatch.GetElapsedTime(startTime).Milliseconds;

        var report = new OptimizationReport
        {
            FilePath                    = filePath,
            Findings                    = sortedFindings,
            AnalysisDurationMilliseconds = elapsedMs,
            ParsedSuccessfully          = parseErrors.Count == 0,
            ParseErrors                 = parseErrors
        };

        reporter.ReportFileSummary(report);
        return OperationResult<OptimizationReport>.Success(report);
    }

    // Запустить анализ всех C# файлов в папке
    public OperationResult<IReadOnlyList<OptimizationReport>> AnalyzeDirectory(
        string directoryPath,       // путь к папке с проектом
        OptimizationContext context) // настройки что проверять
    {
        // Получаем список файлов через интерфейс
        var filesResult = codeReader.ListSourceFiles(directoryPath);
        if (!filesResult.IsSuccess)
            return filesResult.MapError<IReadOnlyList<OptimizationReport>>();

        var reports = new List<OptimizationReport>();

        foreach (var filePath in filesResult.Value!)
        {
            var fileReport = AnalyzeFile(filePath, context);

            // Если один файл не удалось обработать — продолжаем с остальными
            if (fileReport.IsSuccess)
                reports.Add(fileReport.Value!);
        }

        return OperationResult<IReadOnlyList<OptimizationReport>>.Success(reports);
    }
}
```

---

## Критерии приёмки (Acceptance Criteria — условия при которых подпроект считается завершённым)

1. **Gate A (автоматический):** `dotnet build` завершается с кодом 0, все unit тесты зелёные, никаких предупреждений компилятора
2. **Gate B (алгоритмическая корректность):** результаты C# реализации совпадают с ожидаемыми по LLVM алгоритму для каждого из 5 типов оптимизаций
3. **Gate C (одобрение):** явное "да" от Лубы перед переходом к следующему подпроекту

---

## Оценка времени

| Фаза | Содержание | Дней |
|---|---|---|
| 0 | Интерфейсный слой (fabric-first) | 1 |
| 1 | Базовые типы данных | 1 |
| 2 | Анализ потока данных и управления | 3–4 |
| 3 | Устранение мёртвого кода | 2 |
| 4 | Свёртка констант | 2 |
| 5 | Замена дорогих операций дешёвыми | 2 |
| 6 | Устранение повторных вычислений | 3 |
| 7 | Оптимизации циклов | 3 |
| 8 | Конвейер и интеграция | 2 |
| **Итого** | | **19–20 дней** |

---

## Место в общей таблице подпроектов

| Подпроект | Название | Зависимости | Дней |
|---|---|---|---|
| 01–12 | (существующие) | (см. основной план) | 55 |
| **13** | **Code Optimizer (оптимизатор кода)** | **независим** | **20** |
| **Итого** | | | **75 дней** |

---

*Версия плана 1.0 — подпроект 13, не касается XIIGen*
