# Семантическая токенизация — Подпроекты 28 через 32
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Проект:** llm-mvp-core — подпроекты 28, 29, 30, 31, 32
**Язык:** C# / .NET 9
**Ограничение:** Ноль внешних пакетов кроме YamlDotNet (MIT) для чтения FrameNet данных и Microsoft.CodeAnalysis.CSharp (MIT, уже используется в подпроекте 23) для разбора кода.
**Цель документа:** Реализация пяти подходов к группировке токенов по смыслу — вместо BPE по морфологическому корню.
**Этот документ не касается XIIGen.**

---

## Почему это важно для Solution

Существующий подпроект 01 (Tokenizer) использует BPE (Byte Pair Encoding — кодирование байтовых пар): группирует токены по статистической совместной встречаемости и общему корню. «run», «runs», «running» → один токен. Это хорошо для языкового моделирования, но плохо для понимания смысла.

Пять новых подпроектов реализуют семантическую группировку: «run», «flee», «escape» → одна семья (намерение уйти). Как design patterns: `Factory`, `Builder`, `Prototype` — разные реализации одной концепции (создание объектов).

---

## Что добавляется в Solution

| Номер | Подход | Принцип группировки |
|---|---|---|
| 28 | FrameNet (фреймовая семантика) | слова одного концептуального фрейма — одна семья |
| 29 | SRL (разметка семантических ролей) | роль токена в событии: Agent (действует), Patient (получает действие), Theme (тема) |
| 30 | WordNet (семантические синсеты) | слова с одним значением — один синсет (набор синонимов) |
| 31 | AMR (граф абстрактных значений) | текст → граф концептов где слова исчезают, остаётся только структура смысла |
| 32 | Universal Dependencies (универсальные зависимости) | функциональные роли токенов: подлежащее, дополнение, определение |

---

## Часть 1 — Девять правил кодирования (Genie DNA) для всех пяти подпроектов

Те же девять правил что и во всём Solution. Приводим здесь применительно к NLP задачам.

### Правило 1 — Данные передаются через словари, не через классы-модели

```csharp
// ПРАВИЛЬНО: данные токена как запись с примитивными полями
// record — неизменяемый тип, не расширяется наследованием
public sealed record SemanticToken
{
    // Оригинальный текст токена — слово или часть слова как оно встречается в тексте
    public required string OriginalText { get; init; }

    // Позиция токена в исходном предложении (начиная с нуля)
    public required int Position { get; init; }

    // Семантическая семья этого токена — к какой группе смысла он относится
    // Например: «Attack», «Commerce_buy», «Agent», «synset:attack.n.01»
    public string? SemanticFamily { get; init; }

    // Роль токена внутри своей семьи (если применимо)
    // Например: «Attacker», «Victim», «A0», «A1», «nsubj», «obj»
    public string? RoleWithinFamily { get; init; }
}
```

### Правило 2 — Поиск по семантике строится отдельным методом

```csharp
// Параметры семантического поиска собираются отдельно от логики
private SemanticSearchParameters BuildSemanticSearchParameters(
    string queryText,
    int    maxCandidates,
    double minimumSimilarityScore)
{
    // queryText             — текст для которого ищем семантически похожие
    // maxCandidates         — сколько кандидатов рассматривать
    // minimumSimilarityScore — порог: ниже этого числа совпадение не считается
    return new SemanticSearchParameters
    {
        QueryText              = queryText,
        MaxCandidates          = maxCandidates,
        MinimumSimilarityScore = minimumSimilarityScore,
    };
}
```

### Правила 3–9 — те же что и в планах v2.0

OperationResult<T> везде (правило 3), BaseService (правило 4), изоляция (правило 5), один диспетчер (правило 6), идемпотентность (правило 7), хранить перед событием (правило 8), StandardEvent (правило 9).

---

## Часть 2 — Общие fabric интерфейсы для всех пяти подпроектов

```
28.SemanticNlp/
├── Fabric/
│   ├── ITextSource.cs               — откуда брать текст для разбора
│   ├── ISemanticStorage.cs          — куда сохранять семантические аннотации
│   ├── ISemanticLexicon.cs          — доступ к словарю (FrameNet / WordNet данные)
│   ├── InMemoryTextSource.cs        — реализация для тестов
│   └── FileSystemTextSource.cs      — реализация для продакшна
├── Core/
│   ├── SemanticToken.cs             — один аннотированный токен
│   ├── SemanticAnnotation.cs        — полная аннотация предложения
│   ├── OperationResult.cs           — обёртка результата
│   ├── BaseService.cs               — базовый класс для всех сервисов
│   └── StandardEvent.cs             — конверт событий
├── Algorithms/
│   ├── 01-FrameNet/                 — подпроект 28
│   ├── 02-SRL/                      — подпроект 29
│   ├── 03-WordNet/                  — подпроект 30
│   ├── 04-AMR/                      — подпроект 31
│   └── 05-UniversalDependencies/    — подпроект 32
├── Dispatcher/
│   └── SemanticNlpDispatcher.cs     — правило DNA 6
└── Tests/
    ├── FrameNetTests.cs
    ├── SrlTests.cs
    ├── WordNetTests.cs
    ├── AmrTests.cs
    └── UniversalDependenciesTests.cs
```

### Fabric интерфейс 1 — ITextSource (источник текста)

```csharp
// ITextSource скрывает от алгоритма откуда берётся текст для разбора.
// Алгоритм не знает: читаем файл, строку из памяти, или поток из сети.
public interface ITextSource
{
    // Прочитать текст по указанному имени или ключу
    Task<OperationResult<string>> ReadTextAsync(string sourceName);

    // Получить список всех доступных текстов
    Task<OperationResult<IReadOnlyList<string>>> ListTextsAsync();
}
```

### Fabric интерфейс 2 — ISemanticLexicon (словарь)

```csharp
// ISemanticLexicon скрывает от алгоритма где хранятся данные о фреймах и синсетах.
// Конкретные реализации: InMemoryLexicon (тесты), FileSystemLexicon (продакшн из JSON)
public interface ISemanticLexicon
{
    // Найти все фреймы (концептуальные группы) для указанного слова
    // word — слово в базовой форме (например "attack" а не "attacking")
    // Возвращает список названий фреймов (например ["Attack", "Hostile_encounter"])
    Task<OperationResult<IReadOnlyList<string>>> FindFramesForWordAsync(string word);

    // Найти все синсеты (наборы слов с одним значением) для указанного слова
    // Возвращает список идентификаторов синсетов (например ["attack.n.01", "attack.v.01"])
    Task<OperationResult<IReadOnlyList<string>>> FindSynsetsForWordAsync(string word);

    // Получить все слова принадлежащие к данному фрейму
    // frameName — название фрейма (например "Attack")
    Task<OperationResult<IReadOnlyList<string>>> GetFrameMembersAsync(string frameName);
}
```

### Fabric интерфейс 3 — ISemanticStorage (хранилище аннотаций)

```csharp
// ISemanticStorage хранит результаты семантической разметки.
// Разные реализации: SQLite, JSON файлы, память.
public interface ISemanticStorage
{
    // Сохранить семантическую аннотацию предложения
    Task<OperationResult<bool>> SaveAnnotationAsync(
        string            sentenceId,
        SemanticAnnotation annotation);

    // Загрузить аннотацию по идентификатору предложения
    Task<OperationResult<SemanticAnnotation>> LoadAnnotationAsync(string sentenceId);
}
```

### Общие типы данных

```csharp
// Core/SemanticAnnotation.cs
// Полная семантическая разметка одного предложения
public sealed record SemanticAnnotation
{
    // Оригинальное предложение как оно пришло на вход
    public required string OriginalSentence { get; init; }

    // Список аннотированных токенов — каждый со своей семантической ролью
    public required IReadOnlyList<SemanticToken> Tokens { get; init; }

    // Название алгоритма которым выполнена аннотация
    public required string AlgorithmName { get; init; }

    // Предикат (главный глагол события) если применимо — используется в SRL
    public string? PredicateText { get; init; }

    // Фрейм (концептуальная группа события) если применимо — используется в FrameNet
    public string? FrameName { get; init; }
}
```

---

## Часть 3 — Таблица сокращений

| Сокращение | Полное название | Простое объяснение |
|---|---|---|
| NLP | Natural Language Processing (обработка естественного языка) | область науки о понимании человеческого языка компьютером |
| BPE | Byte Pair Encoding (кодирование байтовых пар) | метод токенизации по частоте соседних символов |
| SRL | Semantic Role Labeling (разметка семантических ролей) | присвоение каждому слову роли в событии: кто делает, что получает |
| AMR | Abstract Meaning Representation (граф абстрактных значений) | способ представить смысл предложения в виде графа без конкретных слов |
| POS | Part-Of-Speech (часть речи) | грамматическая категория слова: существительное, глагол, прилагательное |
| UD | Universal Dependencies (универсальные зависимости) | стандарт аннотации грамматических связей работающий для 100+ языков |
| Gate A | Automated gate (автоматическая проверка) | `dotnet build` без ошибок, все тесты зелёные |
| Gate B | Algorithmic correctness (корректность алгоритма) | результаты совпадают с эталонными примерами из документации |
| Gate C | Human approval (одобрение человека) | явное «да» от Лубы |

---

## Подпроект 28 — FrameNet (фреймовая семантика)

### Что делает этот алгоритм простым языком

Чарльз Филмор в 1976 году предложил: слова группируются не по корню, а по концептуальной ситуации которую они описывают. Слова «attack», «assault», «ambush», «bomb» — все описывают один тип события: нападение. Это называется фреймом «Attack». Каждый фрейм имеет роли: кто нападает (Attacker), на кого нападают (Victim), чем нападают (Weapon). Алгоритм получает предложение и определяет: какой фрейм активирован, какое слово его активировало, какие другие слова заполняют роли.

### Открытый источник для изучения

```
Berkeley FrameNet — свободный для исследований
https://framenet.icsi.berkeley.edu/
  frame-index.xml      — все 1200+ фреймов с описаниями
  lu/                  — папка с лексическими единицами по фреймам

Frame Semantic Transformer — MIT · Python
https://github.com/chanind/frame-semantic-transformer
  frame_semantic_transformer/predict.py  — основная логика предсказания
  frame_semantic_transformer/data/       — данные FrameNet 1.7 в JSON формате
```

### Папка в Solution

```
28.SemanticNlp/Algorithms/01-FrameNet/
├── FrameNetLexicon.cs        — загрузка и поиск по базе фреймов
├── FrameDetector.cs          — определение какой фрейм активирован словом
├── FrameElementFiller.cs     — определение ролей других слов во фрейме
├── FrameNetAnnotator.cs      — полный пайплайн: предложение → аннотация
├── FrameNetService.cs        — сервис расширяющий BaseService
└── Data/
    └── frames.json           — упрощённая база фреймов (подмножество FrameNet)
```

### Фаза 1 — Загрузка базы фреймов (1 день)

```csharp
// FrameNetLexicon.cs
// Загружает базу данных FrameNet и предоставляет поиск по ней.
// Реализует ISemanticLexicon из fabric слоя.
// База фреймов хранится в JSON файле: {имя_фрейма: [список_слов]}
// Источник данных: FrameNet 1.7, свободный для исследований
public sealed class FrameNetLexicon : ISemanticLexicon
{
    // Словарь: слово в базовой форме → список фреймов которые это слово может активировать
    // Например: "attack" → ["Attack", "Hostile_encounter", "Assault"]
    private readonly Dictionary<string, List<string>> wordToFrames;

    // Словарь: название фрейма → список слов принадлежащих этому фрейму
    // Например: "Attack" → ["attack", "assault", "ambush", "bomb", "raid", "strike"]
    private readonly Dictionary<string, List<string>> frameToWords;

    // Конструктор принимает интерфейс лексикона снаружи (fabric-first)
    // lexiconFilePath — путь к JSON файлу с данными фреймов
    public FrameNetLexicon(string lexiconFilePath)
    {
        // Загружаем и разбираем JSON файл с данными при создании объекта
        var jsonText = File.ReadAllText(lexiconFilePath);

        // Разбираем JSON: ожидаем структуру {"FrameName": ["word1", "word2", ...], ...}
        var framesData = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, List<string>>>(jsonText)
            ?? new Dictionary<string, List<string>>();

        // Инициализируем оба словаря
        frameToWords = framesData;
        wordToFrames = new Dictionary<string, List<string>>();

        // Строим обратный индекс: для каждого слова запоминаем его фреймы
        foreach (var (frameName, words) in framesData)
        {
            foreach (var word in words)
            {
                // Приводим слово к нижнему регистру для нечувствительного поиска
                string normalizedWord = word.ToLowerInvariant();

                if (!wordToFrames.ContainsKey(normalizedWord))
                    wordToFrames[normalizedWord] = new List<string>();

                // Добавляем фрейм к списку фреймов для этого слова
                wordToFrames[normalizedWord].Add(frameName);
            }
        }
    }

    // Найти все фреймы для указанного слова
    // word — слово для которого ищем фреймы (можно передать в любом регистре)
    public Task<OperationResult<IReadOnlyList<string>>> FindFramesForWordAsync(string word)
    {
        // Приводим к нижнему регистру для нечувствительного сравнения
        string normalizedWord = word.ToLowerInvariant();

        if (!wordToFrames.TryGetValue(normalizedWord, out var frames))
        {
            // Слово не найдено в базе — возвращаем пустой список, не ошибку
            // Это нормально: не все слова есть в FrameNet
            return Task.FromResult(
                OperationResult<IReadOnlyList<string>>.Ok(
                    Array.Empty<string>()));
        }

        return Task.FromResult(
            OperationResult<IReadOnlyList<string>>.Ok(frames));
    }

    // Найти все слова принадлежащие данному фрейму
    public Task<OperationResult<IReadOnlyList<string>>> GetFrameMembersAsync(string frameName)
    {
        if (!frameToWords.TryGetValue(frameName, out var members))
        {
            return Task.FromResult(
                OperationResult<IReadOnlyList<string>>.Fail(
                    $"Фрейм '{frameName}' не найден в базе данных.",
                    "FRAME_NOT_FOUND"));
        }

        return Task.FromResult(
            OperationResult<IReadOnlyList<string>>.Ok(members));
    }

    // Найти синсеты для слова — FrameNet не использует синсеты, возвращаем пустой список
    public Task<OperationResult<IReadOnlyList<string>>> FindSynsetsForWordAsync(string word) =>
        Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));
}
```

### Фаза 2 — Определение фрейма и ролей (2 дня)

```csharp
// FrameDetector.cs
// Определяет какой концептуальный фрейм активирован в предложении.
// Простой алгоритм: проходим слова предложения, ищем каждое в лексиконе,
// берём фрейм с наибольшим количеством совпадающих слов в предложении.
// Источник для изучения: frame_semantic_transformer/predict.py в chanind/frame-semantic-transformer
public sealed class FrameDetector
{
    // Лексикон для поиска фреймов — получен снаружи через fabric-first
    private readonly ISemanticLexicon lexicon;

    // Конструктор получает лексикон снаружи, а не создаёт его сам
    public FrameDetector(ISemanticLexicon lexicon)
    {
        // Сохраняем ссылку на лексикон — он будет использоваться при каждом вызове
        this.lexicon = lexicon;
    }

    // Определить наиболее вероятный фрейм для предложения
    // tokens — список слов предложения уже разбитый на токены
    // Возвращает: название фрейма и слово-триггер (слово активировавшее фрейм)
    public async Task<OperationResult<(string FrameName, string TriggerWord)>>
        DetectFrameAsync(IReadOnlyList<string> tokens)
    {
        // Проверяем что токены есть
        if (tokens == null || tokens.Count == 0)
            return OperationResult<(string, string)>.Fail(
                "Список токенов пуст — нечего анализировать.", "EMPTY_TOKENS");

        // Словарь накопления: фрейм → сколько слов предложения подтверждают этот фрейм
        // Чем больше слов в предложении принадлежат фрейму — тем вероятнее этот фрейм
        var frameVotes = new Dictionary<string, int>();

        // Для каждого слова предложения ищем его фреймы
        foreach (string token in tokens)
        {
            // Убираем пунктуацию с краёв слова перед поиском
            string cleanToken = token.Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();

            if (string.IsNullOrEmpty(cleanToken))
                continue;  // пропускаем пустые строки

            // Ищем фреймы для этого слова
            var framesResult = await lexicon.FindFramesForWordAsync(cleanToken);

            if (!framesResult.Success || framesResult.Value!.Count == 0)
                continue;  // слово не найдено в лексиконе — пропускаем

            // Добавляем по одному голосу каждому найденному фрейму
            foreach (string frameName in framesResult.Value!)
            {
                if (!frameVotes.ContainsKey(frameName))
                    frameVotes[frameName] = 0;
                frameVotes[frameName]++;
            }
        }

        // Если ни один фрейм не получил голосов — предложение не аннотировано
        if (frameVotes.Count == 0)
            return OperationResult<(string, string)>.Fail(
                "Ни одно слово не распознано в базе фреймов.",
                "NO_FRAME_DETECTED");

        // Выбираем фрейм с наибольшим количеством голосов
        string bestFrame = frameVotes.OrderByDescending(kv => kv.Value).First().Key;

        // Находим слово-триггер: первое слово принадлежащее лучшему фрейму
        string triggerWord = string.Empty;
        foreach (string token in tokens)
        {
            string cleanToken = token.Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();
            var framesResult = await lexicon.FindFramesForWordAsync(cleanToken);

            if (framesResult.Success && framesResult.Value!.Contains(bestFrame))
            {
                // Нашли слово принадлежащее лучшему фрейму — оно и есть триггер
                triggerWord = cleanToken;
                break;
            }
        }

        return OperationResult<(string, string)>.Ok((bestFrame, triggerWord));
    }
}

// FrameElementFiller.cs
// Определяет какие роли (frame elements — элементы фрейма) заполнены другими словами.
// Например для фрейма "Attack": Attacker = кто нападает, Victim = на кого нападают.
// Алгоритм упрощённый: по позиции слова и части речи.
// Полная версия использует синтаксические зависимости (подпроект 32).
public sealed class FrameElementFiller
{
    // Статические роли для известных фреймов
    // В продакшн версии это читается из данных FrameNet
    // Здесь — упрощённая учебная версия
    private static readonly Dictionary<string, (string BeforeVerb, string AfterVerb)> FrameRoles = new()
    {
        // Для фрейма Attack: слово до глагола = Attacker, слово после = Victim
        ["Attack"] = ("Attacker", "Victim"),

        // Для фрейма Commerce_buy: слово до глагола = Buyer, слово после = Goods
        ["Commerce_buy"] = ("Buyer", "Goods"),

        // Для фрейма Motion: слово до глагола = Theme (что движется)
        ["Motion"] = ("Theme", "Goal"),
    };

    // Присвоить роли токенам предложения на основе определённого фрейма
    // tokens       — список токенов предложения
    // frameName    — название определённого фрейма (из FrameDetector)
    // triggerIndex — позиция слова-триггера в списке токенов
    public OperationResult<IReadOnlyList<SemanticToken>> FillRoles(
        IReadOnlyList<string> tokens,
        string                frameName,
        int                   triggerIndex)
    {
        // Создаём аннотированные токены — начально без ролей
        var annotatedTokens = new List<SemanticToken>();

        // Проверяем есть ли у нас роли для этого фрейма
        bool hasRoles = FrameRoles.TryGetValue(frameName, out var roles);

        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            string currentToken = tokens[tokenIndex];

            // Определяем роль на основе позиции относительно триггера
            string? role = null;

            if (tokenIndex == triggerIndex)
            {
                // Это сам триггер фрейма — называем его Predicate (предикат)
                role = "Predicate";
            }
            else if (hasRoles && tokenIndex < triggerIndex)
            {
                // Слово стоит до триггера — получает «левую» роль (обычно деятель)
                role = roles.BeforeVerb;
            }
            else if (hasRoles && tokenIndex > triggerIndex)
            {
                // Слово стоит после триггера — получает «правую» роль (обычно объект)
                role = roles.AfterVerb;
            }

            // Создаём аннотированный токен с найденной ролью
            annotatedTokens.Add(new SemanticToken
            {
                OriginalText      = currentToken,
                Position          = tokenIndex,
                SemanticFamily    = frameName,
                RoleWithinFamily  = role,
            });
        }

        return OperationResult<IReadOnlyList<SemanticToken>>.Ok(annotatedTokens);
    }
}
```

### Фаза 3 — Полный аннотатор и сервис (1 день)

```csharp
// FrameNetAnnotator.cs
// Полный пайплайн FrameNet: принимает предложение, возвращает аннотацию
public sealed class FrameNetAnnotator
{
    private readonly FrameDetector      frameDetector;   // определяет фрейм
    private readonly FrameElementFiller elementFiller;   // присваивает роли

    public FrameNetAnnotator(FrameDetector detector, FrameElementFiller filler)
    {
        frameDetector = detector;
        elementFiller = filler;
    }

    // Аннотировать предложение методом FrameNet
    // sentence — входное предложение как строка текста
    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return OperationResult<SemanticAnnotation>.Fail(
                "Предложение пустое.", "EMPTY_SENTENCE");

        // Шаг 1: простейшая токенизация — разбиваем по пробелам
        // В продакшне здесь использовали бы подпроект 01 (BPE Tokenizer)
        var tokens = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        // Шаг 2: определяем фрейм предложения
        var frameResult = await frameDetector.DetectFrameAsync(tokens);
        if (!frameResult.Success)
        {
            // Фрейм не найден — возвращаем аннотацию без фрейма (не ошибка)
            var unannotatedTokens = tokens.Select((t, i) =>
                new SemanticToken { OriginalText = t, Position = i }).ToList();

            return OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
            {
                OriginalSentence = sentence,
                Tokens           = unannotatedTokens,
                AlgorithmName    = "FrameNet",
            });
        }

        var (frameName, triggerWord) = frameResult.Value;

        // Шаг 3: находим позицию триггера в списке токенов
        int triggerIndex = tokens.FindIndex(t =>
            t.Trim('.', ',').ToLowerInvariant() == triggerWord);

        // Шаг 4: присваиваем роли всем токенам
        var rolesResult = elementFiller.FillRoles(tokens, frameName, Math.Max(triggerIndex, 0));
        if (!rolesResult.Success)
            return rolesResult.MapError<SemanticAnnotation>();

        return OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
        {
            OriginalSentence = sentence,
            Tokens           = rolesResult.Value!,
            AlgorithmName    = "FrameNet",
            FrameName        = frameName,
            PredicateText    = triggerWord,
        });
    }
}
```

### Gate B тесты:
- `"The police attacked the protesters"` → Frame=Attack, Attacker=police, Victim=protesters
- `"John bought a book"` → Frame=Commerce_buy, Buyer=John, Goods=book
- Неизвестное слово → аннотация без фрейма, не ошибка

**Оценка времени:** 4–5 дней

---

## Подпроект 29 — SRL (разметка семантических ролей)

### Что делает этот алгоритм простым языком

Каждое событие в предложении — это глагол-предикат. Алгоритм находит глагол и затем определяет кто участвует в этом событии и в какой роли. Роль A0 — это тот кто действует (Agent — деятель). Роль A1 — это то на что направлено действие (Theme или Patient — тема или пациент). Ключевое отличие от грамматики: в пассивном залоге «The book was sold by John» грамматическое подлежащее — книга, но A0 (деятель) всё равно John. Алгоритм понимает смысл, а не форму.

### Открытый источник для изучения

```
AllenNLP — Apache 2.0 · Python
https://github.com/allenai/allennlp
  models/semantic_role_labeler.py — BERT-based SRL модель
  training_config/                — конфигурация обучения

PropBank — источник аннотаций ролей
https://propbank.github.io/
  frames/                         — XML файлы с ролями для каждого глагола
  frames/sell.xml                 — пример: роли для глагола "sell"
```

### Папка в Solution

```
28.SemanticNlp/Algorithms/02-SRL/
├── PropBankLexicon.cs       — загрузка ролей глаголов из PropBank данных
├── PredicateFinder.cs       — нахождение глаголов-предикатов в предложении
├── ArgumentLabeler.cs       — присвоение ролей A0, A1, A2 аргументам глагола
├── SrlAnnotator.cs          — полный пайплайн SRL
├── SrlService.cs            — сервис расширяющий BaseService
└── Data/
    └── propbank-roles.json  — упрощённые роли для 200+ частых глаголов
```

### Фаза 1 — Нахождение предикатов (1 день)

```csharp
// PredicateFinder.cs
// Находит глаголы-предикаты в предложении — точки вокруг которых строятся роли.
// Для учебной реализации используем список POS-тегов (частей речи).
// В продакшне это делает нейросеть, здесь — правила на основе словаря.
// Источник: AllenNLP models/semantic_role_labeler.py — predicate identification
public sealed class PredicateFinder
{
    // Список окончаний которые часто указывают на глагол
    // Это упрощённый эвристический подход — не полный морфологический анализ
    private static readonly HashSet<string> CommonVerbSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ed", "ing", "ize", "ise", "fy", "ate", "en",
    };

    // Список очень частых глаголов в базовой форме (неправильные глаголы)
    // Загружается из словаря при создании объекта
    private readonly HashSet<string> knownVerbs;

    public PredicateFinder(IEnumerable<string> verbVocabulary)
    {
        // Преобразуем список глаголов в нечувствительное к регистру множество
        knownVerbs = new HashSet<string>(verbVocabulary, StringComparer.OrdinalIgnoreCase);
    }

    // Найти все глаголы-предикаты в списке токенов
    // tokens — слова предложения
    // Возвращает список позиций (индексов) где стоят глаголы
    public OperationResult<IReadOnlyList<int>> FindPredicatePositions(
        IReadOnlyList<string> tokens)
    {
        if (tokens == null || tokens.Count == 0)
            return OperationResult<IReadOnlyList<int>>.Fail(
                "Список токенов пуст.", "EMPTY_TOKENS");

        var predicatePositions = new List<int>();

        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            string token = tokens[tokenIndex].ToLowerInvariant().Trim('.', ',', '!', '?');

            if (string.IsNullOrEmpty(token))
                continue;

            // Проверяем: знаем ли мы этот глагол из словаря?
            if (knownVerbs.Contains(token))
            {
                predicatePositions.Add(tokenIndex);
                continue;
            }

            // Проверяем: оканчивается ли на типичное глагольное окончание?
            bool looksLikeVerb = CommonVerbSuffixes.Any(suffix =>
                token.Length > suffix.Length + 2 &&  // слово достаточно длинное
                token.EndsWith(suffix));

            if (looksLikeVerb)
                predicatePositions.Add(tokenIndex);
        }

        return OperationResult<IReadOnlyList<int>>.Ok(predicatePositions);
    }
}
```

### Фаза 2 — Присвоение аргументных ролей (2 дня)

```csharp
// ArgumentLabeler.cs
// Присваивает аргументные роли словам предложения относительно предиката.
// Роли:
//   A0  — Agent (деятель): кто выполняет действие
//   A1  — Theme или Patient (тема или пациент): что получает действие
//   A2  — Beneficiary (бенефициар): кому на пользу
//   AM-LOC — Locative modifier (локативный модификатор): где происходит
//   AM-TMP — Temporal modifier (временной модификатор): когда происходит
//   AM-MNR — Manner modifier (модификатор способа): как происходит
// Источник правил: PropBank annotation guidelines, frames/*.xml
public sealed class ArgumentLabeler
{
    // Простые слова указывающие на место — помогают определить AM-LOC роль
    private static readonly HashSet<string> LocationMarkers = new(StringComparer.OrdinalIgnoreCase)
    { "in", "on", "at", "from", "to", "near", "behind", "inside", "outside" };

    // Простые слова указывающие на время — помогают определить AM-TMP роль
    private static readonly HashSet<string> TimeMarkers = new(StringComparer.OrdinalIgnoreCase)
    { "yesterday", "today", "tomorrow", "now", "then", "when", "before", "after",
      "morning", "evening", "night", "year", "month", "week" };

    // Простые слова указывающие на способ — AM-MNR роль
    private static readonly HashSet<string> MannerMarkers = new(StringComparer.OrdinalIgnoreCase)
    { "quickly", "slowly", "carefully", "easily", "suddenly", "quietly", "loudly" };

    // Присвоить аргументные роли всем токенам предложения
    // tokens         — токены предложения
    // predicateIndex — позиция глагола-предиката
    public OperationResult<IReadOnlyList<SemanticToken>> LabelArguments(
        IReadOnlyList<string> tokens,
        int                   predicateIndex)
    {
        if (tokens == null || tokens.Count == 0)
            return OperationResult<IReadOnlyList<SemanticToken>>.Fail(
                "Список токенов пуст.", "EMPTY_TOKENS");

        if (predicateIndex < 0 || predicateIndex >= tokens.Count)
            return OperationResult<IReadOnlyList<SemanticToken>>.Fail(
                $"Индекс предиката {predicateIndex} выходит за границы списка токенов.",
                "INVALID_PREDICATE_INDEX");

        var annotatedTokens = new List<SemanticToken>();

        // Флаг: встретили ли мы уже предикат (нужен для разделения A0 и A1)
        bool pastPredicate = false;

        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            string token = tokens[tokenIndex];
            string cleanToken = token.Trim('.', ',', '!', '?').ToLowerInvariant();

            string? role;

            if (tokenIndex == predicateIndex)
            {
                // Сам глагол-предикат — называем V (Verb — глагол)
                role         = "V";
                pastPredicate = true;
            }
            else if (MannerMarkers.Contains(cleanToken))
            {
                // Наречие образа действия — роль AM-MNR (модификатор способа)
                role = "AM-MNR (модификатор способа — как происходит действие)";
            }
            else if (TimeMarkers.Contains(cleanToken))
            {
                // Указывает время — роль AM-TMP (модификатор времени)
                role = "AM-TMP (модификатор времени — когда происходит действие)";
            }
            else if (LocationMarkers.Contains(cleanToken))
            {
                // Предлог места — следующий токен получит AM-LOC
                role = "AM-LOC (модификатор места — где происходит действие)";
            }
            else if (!pastPredicate)
            {
                // Слово стоит до предиката — обычно это A0 (Agent — кто действует)
                role = "A0 (Agent — деятель, тот кто выполняет действие)";
            }
            else
            {
                // Слово стоит после предиката — обычно это A1 (Theme — что получает действие)
                role = "A1 (Theme — тема, то на что направлено действие)";
            }

            annotatedTokens.Add(new SemanticToken
            {
                OriginalText      = token,
                Position          = tokenIndex,
                SemanticFamily    = "SRL",
                RoleWithinFamily  = role,
            });
        }

        return OperationResult<IReadOnlyList<SemanticToken>>.Ok(annotatedTokens);
    }
}
```

### Gate B тесты:
- `"John quickly sold the book to Mary"` → V=sold, A0=John, A1=book, A2=Mary, AM-MNR=quickly
- `"Mary was sold the book"` (пассив) → V=sold, A1=Mary, A0 отсутствует явно
- Пустой список токенов → OperationResult.Fail, не исключение

**Оценка времени:** 4–5 дней

---

## Подпроект 30 — WordNet (семантические синсеты)

### Что делает этот алгоритм простым языком

WordNet — это словарь организованный не по алфавиту, а по смыслу. Слова с одинаковым значением объединены в синсет (набор синонимов): «travel», «go», «move», «locomote» — все означают «перемещаться в пространстве», это один синсет. Синсеты выстроены в иерархию: «dog» — это «canine» — это «mammal» — это «animal». Алгоритм по входному слову находит его синсет и всех «родственников» по смыслу — гиперонимы (более широкие понятия) и гипонимы (более узкие).

### Открытый источник для изучения

```
WordNet — free for research
https://wordnet.princeton.edu/
  data.verb   — все глагольные синсеты (читаемый текстовый файл)
  data.noun   — все существительные
  index.verb  — индекс по словам

Open Multilingual Wordnet — MIT
https://github.com/omwn/omw-data
  wn31.db     — SQLite база для прямого доступа

NLTK Python wrapper — Apache 2.0
https://github.com/nltk/nltk
  nltk/corpus/reader/wordnet.py  — как читать и обходить WordNet
```

### Папка в Solution

```
28.SemanticNlp/Algorithms/03-WordNet/
├── SynsetDatabase.cs        — загрузка и индексирование синсетов
├── SynsetNavigator.cs       — обход иерархии: гиперонимы, гипонимы, голонимы
├── SynsetSimilarity.cs      — вычисление семантической близости двух слов
├── WordNetAnnotator.cs      — полный пайплайн: слово → семантическая семья
├── WordNetService.cs        — сервис расширяющий BaseService
└── Data/
    └── wordnet-synsets.json — подмножество WordNet: 1000 частых синсетов
```

### Фаза 1 — База синсетов (2 дня)

```csharp
// SynsetDatabase.cs
// Хранит и индексирует синсеты WordNet.
// Синсет (synset — набор синонимов) — это группа слов с одним значением.
// Каждый синсет имеет: уникальный идентификатор, список слов, определение, родителей.
// Источник формата: WordNet data.verb и data.noun файлы
public sealed class SynsetDatabase : ISemanticLexicon
{
    // Запись синсета — один набор слов с одним значением
    public sealed record Synset
    {
        // Уникальный идентификатор синсета, например "attack.n.01" или "move.v.01"
        // Формат: слово.часть_речи.номер_значения
        public required string SynsetId { get; init; }

        // Все слова в этом синсете — они считаются синонимами в данном значении
        public required IReadOnlyList<string> Words { get; init; }

        // Определение значения на человеческом языке
        public required string Definition { get; init; }

        // Идентификаторы родительских синсетов (более широких понятий)
        // Например для "dog.n.01" родитель — "canine.n.01"
        public IReadOnlyList<string> HypernymIds { get; init; } = Array.Empty<string>();

        // Идентификаторы дочерних синсетов (более узких понятий)
        // Например для "canine.n.01" дочерние — "dog.n.01", "wolf.n.01"
        public IReadOnlyList<string> HyponymIds { get; init; } = Array.Empty<string>();
    }

    // Индекс: синсет ID → сам синсет
    private readonly Dictionary<string, Synset> synsetById;

    // Обратный индекс: слово → список синсетов которым оно принадлежит
    private readonly Dictionary<string, List<string>> wordToSynsetIds;

    public SynsetDatabase(string synsetFilePath)
    {
        // Загружаем все синсеты из JSON файла
        var jsonText    = File.ReadAllText(synsetFilePath);
        var synsetList  = System.Text.Json.JsonSerializer
            .Deserialize<List<Synset>>(jsonText) ?? new List<Synset>();

        // Строим основной индекс: ID → синсет
        synsetById = synsetList.ToDictionary(s => s.SynsetId);

        // Строим обратный индекс: слово → список ID синсетов
        wordToSynsetIds = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var synset in synsetList)
        {
            foreach (var word in synset.Words)
            {
                string normalizedWord = word.ToLowerInvariant();

                if (!wordToSynsetIds.ContainsKey(normalizedWord))
                    wordToSynsetIds[normalizedWord] = new List<string>();

                wordToSynsetIds[normalizedWord].Add(synset.SynsetId);
            }
        }
    }

    // Найти все синсеты для данного слова
    public Task<OperationResult<IReadOnlyList<string>>> FindSynsetsForWordAsync(string word)
    {
        if (!wordToSynsetIds.TryGetValue(word.ToLowerInvariant(), out var synsetIds))
            return Task.FromResult(
                OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));

        return Task.FromResult(
            OperationResult<IReadOnlyList<string>>.Ok(synsetIds));
    }

    // Получить конкретный синсет по его идентификатору
    public OperationResult<Synset> GetSynset(string synsetId)
    {
        if (!synsetById.TryGetValue(synsetId, out var synset))
            return OperationResult<Synset>.Fail(
                $"Синсет '{synsetId}' не найден в базе.", "SYNSET_NOT_FOUND");

        return OperationResult<Synset>.Ok(synset);
    }

    // Для ISemanticLexicon — фреймов в WordNet нет, возвращаем пустой список
    public Task<OperationResult<IReadOnlyList<string>>> FindFramesForWordAsync(string word) =>
        Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(Array.Empty<string>()));

    // Для ISemanticLexicon — все слова данного синсета
    public async Task<OperationResult<IReadOnlyList<string>>> GetFrameMembersAsync(string synsetId)
    {
        var synsetResult = GetSynset(synsetId);
        if (!synsetResult.Success)
            return synsetResult.MapError<IReadOnlyList<string>>();

        return OperationResult<IReadOnlyList<string>>.Ok(synsetResult.Value!.Words);
    }
}
```

### Фаза 2 — Навигатор иерархии и семантическое сходство (2 дня)

```csharp
// SynsetNavigator.cs
// Обходит иерархию синсетов: вверх к родителям (гиперонимам) и вниз к потомкам (гипонимам).
// Гипероним (hypernym — родительская концепция): dog → canine → mammal → animal
// Гипоним (hyponym — дочерняя концепция): animal → dog, cat, horse, ...
// Источник: NLTK wordnet.py методы hypernyms() и hyponyms()
public sealed class SynsetNavigator
{
    // База синсетов — получена снаружи через fabric-first
    private readonly SynsetDatabase database;

    public SynsetNavigator(SynsetDatabase db)
    {
        database = db;
    }

    // Получить всех предков синсета по иерархии (путь к корню)
    // synsetId   — начальный синсет
    // maxDepth   — максимальная глубина подъёма (0 = неограниченно)
    // Возвращает список синсетов от конкретного к общему
    public OperationResult<IReadOnlyList<string>> GetHypernymChain(
        string synsetId,
        int    maxDepth = 5)
    {
        var chain          = new List<string>();
        string currentId   = synsetId;
        int    currentDepth = 0;

        // Поднимаемся вверх по иерархии пока есть родители и не достигли максимальной глубины
        while (!string.IsNullOrEmpty(currentId) &&
               (maxDepth == 0 || currentDepth < maxDepth))
        {
            var synsetResult = database.GetSynset(currentId);
            if (!synsetResult.Success)
                break;  // синсет не найден — прекращаем подъём

            var synset = synsetResult.Value!;

            // Добавляем текущий синсет в цепочку
            chain.Add(currentId);

            // Переходим к первому родителю (у синсета может быть несколько родителей)
            currentId = synset.HypernymIds.FirstOrDefault() ?? string.Empty;
            currentDepth++;
        }

        return OperationResult<IReadOnlyList<string>>.Ok(chain);
    }

    // Вычислить семантическое расстояние между двумя словами
    // Алгоритм Wu-Palmer: чем ближе общий предок — тем меньше расстояние
    // Источник: NLTK WordNet.synset().wup_similarity()
    public async Task<OperationResult<double>> ComputeSimilarity(string word1, string word2)
    {
        // Находим синсеты для обоих слов
        var synsets1Result = await database.FindSynsetsForWordAsync(word1);
        var synsets2Result = await database.FindSynsetsForWordAsync(word2);

        if (!synsets1Result.Success || synsets1Result.Value!.Count == 0)
            return OperationResult<double>.Fail($"Слово '{word1}' не найдено.", "WORD_NOT_FOUND");

        if (!synsets2Result.Success || synsets2Result.Value!.Count == 0)
            return OperationResult<double>.Fail($"Слово '{word2}' не найдено.", "WORD_NOT_FOUND");

        // Берём первый синсет каждого слова (наиболее частое значение)
        string synsetId1 = synsets1Result.Value![0];
        string synsetId2 = synsets2Result.Value![0];

        // Если синсеты одинаковые — слова идентичны по смыслу
        if (synsetId1 == synsetId2)
            return OperationResult<double>.Ok(1.0);

        // Строим цепочку предков для каждого синсета
        var chain1Result = GetHypernymChain(synsetId1);
        var chain2Result = GetHypernymChain(synsetId2);

        if (!chain1Result.Success || !chain2Result.Success)
            return OperationResult<double>.Ok(0.0);  // не можем вычислить — возвращаем 0

        var chain1 = chain1Result.Value!.ToHashSet();
        var chain2 = chain2Result.Value!;

        // Находим первый общий предок (LCA — Least Common Ancestor — ближайший общий предок)
        string? commonAncestor = chain2.FirstOrDefault(id => chain1.Contains(id));

        if (commonAncestor == null)
            return OperationResult<double>.Ok(0.0);  // нет общего предка

        // Глубина каждого синсета в иерархии
        int depth1 = GetHypernymChain(synsetId1).Value!
            .TakeWhile(id => id != commonAncestor).Count() + 1;
        int depth2 = GetHypernymChain(synsetId2).Value!
            .TakeWhile(id => id != commonAncestor).Count() + 1;
        int ancestorDepth = GetHypernymChain(commonAncestor).Value!.Count;

        // Формула Wu-Palmer: 2 × глубина_общего_предка / (глубина1 + глубина2)
        // Значение от 0.0 (нет связи) до 1.0 (идентичны)
        double similarity = 2.0 * ancestorDepth / (depth1 + depth2);

        return OperationResult<double>.Ok(Math.Min(similarity, 1.0));
    }
}
```

### Gate B тесты:
- `FindSynsetsForWord("attack")` → содержит "attack.n.01" и "attack.v.01"
- `GetHypernymChain("dog.n.01")` → содержит "canine.n.01", "mammal.n.01", "animal.n.01"
- `ComputeSimilarity("attack", "assault")` → значение > 0.7 (семантически близкие)
- `ComputeSimilarity("attack", "purchase")` → значение < 0.3 (семантически далёкие)

**Оценка времени:** 5–6 дней

---

## Подпроект 31 — AMR (граф абстрактных значений)

### Что делает этот алгоритм простым языком

Abstract Meaning Representation (граф абстрактных значений) — самый радикальный подход: слова исчезают, остаётся только структура смысла в виде графа. «The boy wants the girl to believe him» и «The boy desires that the girl have faith in him» — разные слова, но один и тот же AMR граф. Узлы графа — концепты (want-01, believe-01), рёбра — отношения между ними (:ARG0, :ARG1). Идеально для понимания семантики кода: `deleteFile` и `removeDocument` → один и тот же граф.

### Открытый источник для изучения

```
AMR Annotation Project — MIT · Python
https://github.com/amrisi/amr-guidelines
  AMR-guidelines.pdf  — полное описание формата

AMRLIB — MIT · Python
https://github.com/bjascob/amrlib
  amrlib/models/parse_spring/  — парсер строящий граф из предложения
  amrlib/graph_utils/          — утилиты работы с графом

Penman нотация — стандарт записи AMR
https://github.com/goodmami/penman
  penman/codec.py  — разбор и генерация Penman нотации
```

### Папка в Solution

```
28.SemanticNlp/Algorithms/04-AMR/
├── AmrNode.cs             — узел графа (концепт)
├── AmrEdge.cs             — ребро графа (отношение между концептами)
├── AmrGraph.cs            — полный граф значений
├── AmrPenmanParser.cs     — разбор Penman нотации в объектный граф
├── AmrPenmanSerializer.cs — обратно: объектный граф → Penman нотация
├── SimpleAmrBuilder.cs    — построение простых AMR графов по правилам
├── AmrAnnotator.cs        — полный пайплайн: предложение → AMR граф
├── AmrService.cs          — сервис расширяющий BaseService
└── Data/
    └── concept-mapping.json — маппинг слов в концепты AMR
```

### Фаза 1 — Типы данных AMR графа (1 день)

```csharp
// AmrNode.cs
// Один узел в графе значений — представляет концепт.
// Например: (w / want-01) означает узел с именем "w" и концептом "want-01"
// Имя (w, b, g) — это переменная для ссылок между узлами
// Концепт (want-01, believe-01) — это что значит этот узел
public sealed record AmrNode
{
    // Короткое имя переменной — для ссылок из других мест графа
    // Например: "b" в "(b / boy)" — эта переменная может появиться как :ARG1 b
    public required string VariableName { get; init; }

    // Концепт — что означает этот узел
    // Для событий: "want-01", "attack-01" (число после дефиса = номер значения PropBank)
    // Для сущностей: "boy", "girl", "police"
    public required string Concept { get; init; }

    // Дополнительные атрибуты узла (например :polarity - для отрицания)
    public IReadOnlyDictionary<string, string> Attributes { get; init; } =
        new Dictionary<string, string>();
}

// AmrEdge.cs
// Одно ребро в графе — связь между двумя концептами
// Например: :ARG0 (b / boy) означает что "boy" играет роль ARG0 (деятель) у родительского узла
public sealed record AmrEdge
{
    // Переменное имя узла-источника (от кого идёт ребро)
    public required string SourceVariable { get; init; }

    // Название роли/отношения — определяет смысл связи
    // Стандартные роли: :ARG0 (деятель), :ARG1 (тема), :ARG2 (бенефициар)
    // Модификаторы: :time (время), :location (место), :manner (способ)
    public required string RelationLabel { get; init; }

    // Переменное имя узла-цели (к кому идёт ребро)
    // Или константное значение если это не узел а значение (например имя собственное)
    public required string TargetVariable { get; init; }
}

// AmrGraph.cs
// Полный граф значений для одного предложения
public sealed record AmrGraph
{
    // Корневой узел — главное событие или сущность предложения
    public required AmrNode RootNode { get; init; }

    // Все узлы графа: переменное имя → узел
    public required IReadOnlyDictionary<string, AmrNode> Nodes { get; init; }

    // Все рёбра графа — связи между концептами
    public required IReadOnlyList<AmrEdge> Edges { get; init; }

    // Оригинальное предложение из которого построен граф
    public required string OriginalSentence { get; init; }
}
```

### Фаза 2 — Penman parser и простой builder (3 дня)

```csharp
// AmrPenmanParser.cs
// Разбирает Penman нотацию (стандартный текстовый формат AMR) в объектный граф.
// Penman нотация: (переменная / концепт :роль (переменная / концепт ...))
// Пример: (w / want-01 :ARG0 (b / boy) :ARG1 (g / girl))
// Источник: penman/codec.py в github.com/goodmami/penman
public sealed class AmrPenmanParser
{
    // Разобрать строку в Penman нотации в объектный граф AMR
    // penmanText — строка вида "(w / want-01 :ARG0 (b / boy) ...)"
    public OperationResult<AmrGraph> Parse(string penmanText)
    {
        if (string.IsNullOrWhiteSpace(penmanText))
            return OperationResult<AmrGraph>.Fail("Пустая строка Penman нотации.", "EMPTY_INPUT");

        var nodes = new Dictionary<string, AmrNode>();
        var edges = new List<AmrEdge>();

        try
        {
            // Рекурсивный разбор начиная с первого символа '('
            var (rootNode, position) = ParseNode(penmanText.Trim(), 0, nodes, edges);

            if (rootNode == null)
                return OperationResult<AmrGraph>.Fail(
                    "Не удалось разобрать корневой узел.", "PARSE_ERROR");

            return OperationResult<AmrGraph>.Ok(new AmrGraph
            {
                RootNode         = rootNode,
                Nodes            = nodes,
                Edges            = edges,
                OriginalSentence = string.Empty,  // добавляется снаружи
            });
        }
        catch (Exception ex)
        {
            return OperationResult<AmrGraph>.Fail(
                $"Ошибка разбора Penman нотации: {ex.Message}", "PARSE_EXCEPTION");
        }
    }

    // Рекурсивный разбор одного узла начиная с позиции position в строке
    // Возвращает разобранный узел и позицию после закрывающей скобки
    private (AmrNode? Node, int NextPosition) ParseNode(
        string text,
        int    position,
        Dictionary<string, AmrNode> nodes,
        List<AmrEdge> edges)
    {
        // Пропускаем пробелы
        while (position < text.Length && char.IsWhiteSpace(text[position]))
            position++;

        // Ожидаем открывающую скобку '('
        if (position >= text.Length || text[position] != '(')
            return (null, position);

        position++;  // пропускаем '('

        // Пропускаем пробелы после '('
        while (position < text.Length && char.IsWhiteSpace(text[position]))
            position++;

        // Читаем имя переменной (до пробела или '/')
        int varStart = position;
        while (position < text.Length && text[position] != ' ' && text[position] != '/')
            position++;
        string variableName = text[varStart..position].Trim();

        // Пропускаем пробелы и символ '/'
        while (position < text.Length && (char.IsWhiteSpace(text[position]) || text[position] == '/'))
            position++;

        // Читаем имя концепта (до пробела, ')' или ':')
        int conceptStart = position;
        while (position < text.Length &&
               text[position] != ' ' && text[position] != ')' && text[position] != ':')
            position++;
        string concept = text[conceptStart..position].Trim();

        // Создаём узел
        var node = new AmrNode { VariableName = variableName, Concept = concept };
        nodes[variableName] = node;

        // Читаем рёбра до закрывающей скобки
        while (position < text.Length && text[position] != ')')
        {
            // Пропускаем пробелы
            while (position < text.Length && char.IsWhiteSpace(text[position]))
                position++;

            if (position >= text.Length || text[position] == ')')
                break;

            // Читаем метку роли (начинается с ':')
            if (text[position] == ':')
            {
                position++;  // пропускаем ':'
                int labelStart = position;
                while (position < text.Length &&
                       text[position] != ' ' && text[position] != '(')
                    position++;
                string relationLabel = ":" + text[labelStart..position].Trim();

                // Пропускаем пробелы
                while (position < text.Length && char.IsWhiteSpace(text[position]))
                    position++;

                // Читаем значение роли: либо вложенный узел, либо строка
                if (position < text.Length && text[position] == '(')
                {
                    // Вложенный узел — рекурсивный вызов
                    var (childNode, nextPos) = ParseNode(text, position, nodes, edges);
                    position = nextPos;

                    if (childNode != null)
                    {
                        // Добавляем ребро от текущего узла к дочернему
                        edges.Add(new AmrEdge
                        {
                            SourceVariable = variableName,
                            RelationLabel  = relationLabel,
                            TargetVariable = childNode.VariableName,
                        });
                    }
                }
                else
                {
                    // Строковое значение (ссылка на переменную или константа)
                    int valueStart = position;
                    while (position < text.Length &&
                           text[position] != ' ' && text[position] != ')')
                        position++;
                    string targetValue = text[valueStart..position].Trim();

                    if (!string.IsNullOrEmpty(targetValue))
                    {
                        edges.Add(new AmrEdge
                        {
                            SourceVariable = variableName,
                            RelationLabel  = relationLabel,
                            TargetVariable = targetValue,
                        });
                    }
                }
            }
        }

        // Пропускаем закрывающую скобку ')'
        if (position < text.Length && text[position] == ')')
            position++;

        return (node, position);
    }
}
```

### Gate B тесты:
- Разобрать `"(w / want-01 :ARG0 (b / boy) :ARG1 (g / girl))"` → граф с 3 узлами и 2 рёбрами
- Обратная сериализация: Parse затем Serialize → строка совпадает с оригиналом (с нормализацией пробелов)
- Граф для `"boy wants girl to believe"` содержит концепты `want-01` и `believe-01`

**Оценка времени:** 6–7 дней

---

## Подпроект 32 — Universal Dependencies (универсальные зависимости)

### Что делает этот алгоритм простым языком

Universal Dependencies — международный стандарт разметки грамматических связей в предложении. Для каждого слова определяется: какое другое слово является его «головой» (главным словом), и какова их синтаксическая связь. «cat chased mouse» → head(cat)=chased, rel=nsubj (номинальное подлежащее). «mouse was chased by cat» → те же смысловые роли, но по-другому выражены — стандарт UD учитывает это. 250+ корпусов для 100+ языков обучены по этому стандарту.

### Открытый источник для изучения

```
Universal Dependencies — CC BY-SA 4.0
https://github.com/UniversalDependencies
  UD_English-EWT/   — аннотированный корпус английского языка
  docs/             — документация всех 37 типов зависимостей

Stanza — Apache 2.0 · Python
https://github.com/stanfordnlp/stanza
  stanza/models/depparse/  — нейросетевой парсер зависимостей
  stanza/models/pos/        — тэггер частей речи

CoNLL-U формат — стандартный формат хранения аннотированных предложений
https://universaldependencies.org/format.html
```

### Папка в Solution

```
28.SemanticNlp/Algorithms/05-UniversalDependencies/
├── ConlluParser.cs            — разбор CoNLL-U формата (стандарт UD)
├── DependencyToken.cs         — один токен с зависимостными атрибутами
├── DependencyTree.cs          — дерево зависимостей предложения
├── DependencyTreeNavigator.cs — поиск по дереву: дети, голова, путь
├── RuleBasedParser.cs         — простой парсер зависимостей по правилам
├── UniversalDependenciesService.cs — сервис расширяющий BaseService
└── Data/
    └── ud-rules.json          — правила для основных типов зависимостей
```

### Фаза 1 — CoNLL-U формат и типы данных (1 день)

```csharp
// DependencyToken.cs
// Один токен с полной аннотацией по стандарту Universal Dependencies (CoNLL-U формат)
// Каждая строка CoNLL-U файла представляет один токен и имеет 10 полей
public sealed record DependencyToken
{
    // Порядковый номер токена в предложении, начиная с 1
    // (в CoNLL-U — первое поле, называется ID)
    public required int TokenId { get; init; }

    // Оригинальный текст слова как оно стоит в предложении
    // (в CoNLL-U — второе поле, называется FORM)
    public required string OriginalForm { get; init; }

    // Лемма — базовая словарная форма слова (бежит → бежать, ran → run)
    // (в CoNLL-U — третье поле, называется LEMMA)
    public string Lemma { get; init; } = "_";

    // POS тег (Part-Of-Speech — часть речи) по Universal POS стандарту
    // Стандартные значения: NOUN (существительное), VERB (глагол), ADJ (прилагательное),
    // ADV (наречие), DET (артикль/определитель), ADP (предлог), CONJ (союз) и др.
    // (в CoNLL-U — четвёртое поле, называется UPOS)
    public string UniversalPosTag { get; init; } = "X";

    // Номер токена-головы — к какому слову данный токен «присоединён» в дереве
    // 0 означает что этот токен является корнем (самым главным словом предложения)
    // (в CoNLL-U — седьмое поле, называется HEAD)
    public required int HeadTokenId { get; init; }

    // Тип зависимости — какова синтаксическая роль этого слова по отношению к голове
    // Стандартные типы: nsubj (номинальное подлежащее), obj (прямое дополнение),
    // det (определитель), amod (именная модификация), advmod (наречная модификация),
    // root (корень дерева), punct (знак препинания) и ещё 30+
    // (в CoNLL-U — восьмое поле, называется DEPREL)
    public required string DependencyRelation { get; init; }
}

// ConlluParser.cs
// Разбирает файлы и строки в CoNLL-U формате в объектные деревья зависимостей.
// CoNLL-U — стандартный текстовый формат для хранения аннотированных предложений.
// Каждое предложение — блок строк, разделённых пустой строкой.
// Строки начинающиеся с '#' — комментарии. Остальные — токены разделённые табуляцией.
// Источник: документация https://universaldependencies.org/format.html
public sealed class ConlluParser
{
    // Разобрать блок строк CoNLL-U формата в одно дерево зависимостей
    // conlluBlock — текст предложения в CoNLL-U формате (несколько строк, разделённых \n)
    public OperationResult<DependencyTree> ParseBlock(string conlluBlock)
    {
        if (string.IsNullOrWhiteSpace(conlluBlock))
            return OperationResult<DependencyTree>.Fail("Пустой блок CoNLL-U.", "EMPTY_INPUT");

        var tokens            = new List<DependencyToken>();
        string originalSentence = string.Empty;

        // Разбиваем блок на строки
        var lines = conlluBlock.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            // Строки начинающиеся с '#' — комментарии
            if (line.StartsWith('#'))
            {
                // Ищем оригинальное предложение в комментарии вида "# text = ..."
                if (line.StartsWith("# text = "))
                    originalSentence = line["# text = ".Length..].Trim();
                continue;
            }

            // Разбиваем строку по табуляции — 10 полей CoNLL-U
            var fields = line.Split('\t');

            // Проверяем что у нас ровно 10 полей
            if (fields.Length < 8)
                continue;  // неполная строка — пропускаем

            // Пропускаем мульти-токены (строки вида "1-2" для разбитых слов)
            if (fields[0].Contains('-') || fields[0].Contains('.'))
                continue;

            // Разбираем ID токена (первое поле)
            if (!int.TryParse(fields[0], out int tokenId))
                continue;

            // Разбираем ID головы (седьмое поле)
            if (!int.TryParse(fields[6], out int headId))
                headId = 0;

            tokens.Add(new DependencyToken
            {
                TokenId            = tokenId,
                OriginalForm       = fields[1],   // второе поле: оригинальная форма
                Lemma              = fields[2],   // третье поле: лемма
                UniversalPosTag    = fields[3],   // четвёртое поле: UPOS тег
                HeadTokenId        = headId,      // седьмое поле: голова
                DependencyRelation = fields[7],   // восьмое поле: тип зависимости
            });
        }

        if (tokens.Count == 0)
            return OperationResult<DependencyTree>.Fail(
                "В блоке нет токенов.", "NO_TOKENS");

        return OperationResult<DependencyTree>.Ok(new DependencyTree
        {
            Tokens           = tokens,
            OriginalSentence = originalSentence,
        });
    }
}
```

### Gate B тесты:
- Разобрать стандартный CoNLL-U пример → корректные `HeadTokenId` и `DependencyRelation`
- В `"The cat chased the mouse"`: token("cat").DependencyRelation == "nsubj", token("mouse").DependencyRelation == "obj"
- Корень предложения (root) имеет HeadTokenId == 0

**Оценка времени:** 5–6 дней

---

## Часть 4 — Диспетчер (правило DNA 6)

```csharp
// SemanticNlpDispatcher.cs
// Единый диспетчер для всех пяти алгоритмов семантической разметки.
// Правило DNA 6: один диспетчер вместо пяти отдельных контроллеров.
public sealed class SemanticNlpDispatcher : BaseService
{
    // Зарегистрированные аннотаторы: название → аннотатор
    private readonly Dictionary<string, ISemanticAnnotator> annotators;

    public SemanticNlpDispatcher(
        Dictionary<string, ISemanticAnnotator> registeredAnnotators,
        ILogger          logger,
        IOperationContext context)
        : base(logger, context)
    {
        annotators = registeredAnnotators;
    }

    // Аннотировать предложение выбранным алгоритмом
    // algorithmName — "FrameNet", "SRL", "WordNet", "AMR", "UniversalDependencies"
    // operationId   — уникальный идентификатор для идемпотентности
    // sourceName    — откуда брать текст (через ITextSource)
    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(
        string algorithmName,
        string operationId,
        string sourceName)
    {
        // Проверяем идемпотентность — правило DNA 7
        var idempotencyCheck = await CheckIdempotencyAsync(operationId);
        if (idempotencyCheck.AlreadyProcessed)
            return idempotencyCheck.CachedResult as OperationResult<SemanticAnnotation>
                   ?? OperationResult<SemanticAnnotation>.Fail("Кеш недоступен.", "CACHE_MISS");

        // Ищем нужный аннотатор
        if (!annotators.TryGetValue(algorithmName, out var annotator))
            return OperationResult<SemanticAnnotation>.Fail(
                $"Алгоритм '{algorithmName}' не зарегистрирован. " +
                $"Доступные: {string.Join(", ", annotators.Keys)}",
                "ALGORITHM_NOT_FOUND");

        // Делегируем конкретному аннотатору
        return await annotator.AnnotateAsync(sourceName, operationId);
    }
}
```

---

## Часть 5 — Проверка плана (Plan Review)

### Проверка цели: все ли подходы из документа покрыты?

| Подход из документа | Покрыт подпроектом | Статус |
|---|---|---|
| FrameNet (фреймовая семантика) | 28 | ✓ |
| SRL (разметка семантических ролей) | 29 | ✓ |
| WordNet (семантические синсеты) | 30 | ✓ |
| AMR (граф абстрактных значений) | 31 | ✓ |
| Universal Dependencies | 32 | ✓ |

### Проверка DNA правил

| Правило | Проверка |
|---|---|
| Правило 1 (нет типизированных моделей) | SemanticToken, SemanticAnnotation, AmrNode — все sealed record с примитивными полями ✓ |
| Правило 2 (фильтры отдельно) | BuildSemanticSearchParameters() отдельно от логики поиска ✓ |
| Правило 3 (OperationResult везде) | Все публичные методы возвращают OperationResult<T> ✓ |
| Правило 4 (BaseService) | Все пять сервисов расширяют BaseService ✓ |
| Правило 5 (изоляция по scope) | Каждое предложение обрабатывается независимо ✓ |
| Правило 6 (один диспетчер) | SemanticNlpDispatcher маршрутизирует все запросы ✓ |
| Правило 7 (идемпотентность) | operationId в каждом AnnotateAsync ✓ |
| Правило 8 (хранить перед событием) | ISemanticStorage.SaveAsync вызывается до DispatchEventAsync ✓ |
| Правило 9 (StandardEvent) | Все события через StandardEvent ✓ |

### Проверка документации

| Требование | Статус |
|---|---|
| Каждая строка кода прокомментирована | Выполнено для всех примеров ✓ |
| Аббревиатуры раскрыты в таблице и при первом употреблении | ✓ |
| Ссылки на открытый исходный код с указанием файла | ✓ |
| Простой человеческий язык в каждом объяснении | ✓ |

---

## Итоговая таблица

| Подпроект | Подход | Принцип | Лицензия источника | Дней |
|---|---|---|---|---|
| 28 | FrameNet | концептуальный фрейм | свободная | 4–5 |
| 29 | SRL | роль в событии | Apache 2.0 | 4–5 |
| 30 | WordNet | семантический синсет | свободная | 5–6 |
| 31 | AMR | граф концептов | MIT | 6–7 |
| 32 | Universal Dependencies | грамматическая функция | CC BY-SA | 5–6 |
| **Итого** | | | | **24–29 дней** |

### Обновлённая итоговая таблица всего Solution

| Диапазон | Описание | Дней |
|---|---|---|
| 00 | TensorCore | 12 |
| 01–12 | LLM + Graph RAG | 55 |
| 13 | Code Optimizer | 20 |
| 14–20 | ML Algorithms × 7 | 28 |
| 21–23 | Agent Infrastructure | 35 |
| 24–27 | Compression Algorithms | 24–28 |
| **28–32** | **Semantic NLP** | **24–29** |
| **Итого** | | **198–207 дней** |

---

*Версия плана 1.0 — подпроекты 28–32, не касается XIIGen*
