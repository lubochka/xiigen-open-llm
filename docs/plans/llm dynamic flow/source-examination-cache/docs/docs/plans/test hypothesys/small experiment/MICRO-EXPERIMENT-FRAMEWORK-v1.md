# MICRO-EXPERIMENT-FRAMEWORK-v1
## Шесть микро-экспериментов на основе llm_mvp_core как датасета

**Проект:** llm-mvp-core — Micro-Experiment Framework  
**Датасет уровень 1 (micro):** 01.Tokenizer + 02.DatasetPipeline + 05.Embeddings + 07.GraphEngine + 09.Inference + 10.LoRA — шесть наименьших подпроектов, ~9 100 токенов, 195 файлов  
**Цель:** каждый эксперимент запускается за < 30 секунд, использует < 200 МБ RAM, без обучения нейросети  
**Этот документ не касается XIIGen.**

---

## Почему сам проект как датасет

Проект llm_mvp_core содержит 195 файлов .cs, ~78K слов, ~199K BPE токенов. Это реальный C# код с известной структурой — мы знаем правильные ответы (компилятор подтверждает синтаксис), можем измерить точно, можем запускать на любом ноутбуке.

```
Датасетные уровни — постепенно расширяем:

Tier 1 — micro    ~9 100 токенов    6 подпроектов    < 5 секунд на эксперимент
Tier 2 — small    ~61 000 токенов  18 подпроектов    < 30 секунд
Tier 3 — medium  ~141 000 токенов  все подпроекты    < 2 минуты
Tier 4 — project  любой внешний C# репозиторий       < 10 минут
Tier 5 — dataset  CodeSearchNet C# (~500K функций)   часы (одноразово)

Правило перехода: эксперимент прошёл Tier N (все метрики +5%) → автоматически
запускается на Tier N+1. Если не прошёл — подбираем параметры и повторяем.
```

---

## Формат вывода: JSON + MD

Каждый прогон производит два файла в `experiments/results/`:

```
experiments/
  results/
    E01-AST-tokenizer/
      run-2026-04-30-001.json    ← машиночитаемый результат
      run-2026-04-30-001.md      ← человекочитаемый отчёт
    E02-frequency-ranking/
      run-2026-04-30-001.json
      run-2026-04-30-001.md
    ...
  schema/
    experiment-result-v1.json    ← JSON Schema (уже создана)
  summaries/
    progress.json                ← сводка по всем экспериментам
    progress.md                  ← сводная таблица для человека
```

### Шаблон JSON-результата (заполняется автоматически при каждом прогоне)

```json
{
  "meta": {
    "experiment_id": "E01-AST-TOKENIZER-v1",
    "experiment_name": "AST узлы вместо BPE токенов",
    "hypothesis": "Токенизация через AST Roslyn уменьшит словарь в 5 раз и повысит точность предсказания следующего токена",
    "run_timestamp_utc": "2026-04-30T12:00:00Z",
    "machine": { "os": "Windows 11", "cpu": "...", "ram_total_mb": 16384, "dotnet_version": "9.0" }
  },
  "dataset": {
    "tier": 1,
    "tier_name": "micro — 01.Tokenizer + 02.DatasetPipeline",
    "files_count": 19,
    "tokens_approx": 3099,
    "size_bytes": 12516
  },
  "baseline": {
    "name": "BPE-flat",
    "description": "Стандартная BPE токенизация, словарь 256-1000",
    "metrics": { "...": "..." }
  },
  "improved": {
    "name": "AST-roslyn",
    "description": "Токены = типы узлов AST, словарь ~200 типов",
    "change_applied": "CSharpSyntaxTree.ParseText → узлы дерева вместо строк",
    "metrics": { "...": "..." }
  },
  "delta": {
    "prediction_error_pct": -18.4,
    "time_ms_pct": 12.3,
    "memory_peak_mb_pct": -22.1,
    "cpu_avg_pct_pct": -8.5,
    "disk_write_bytes_pct": -67.2,
    "goal_met": true
  },
  "verdict": {
    "passed": true,
    "summary": "AST токенизация уменьшила ошибку предсказания на 18% и сократила словарь в 4.7 раза. Время немного выросло из-за Roslyn разбора (+12%), но все остальные метрики улучшились.",
    "metrics_that_improved": ["prediction_error", "memory_peak_mb", "cpu_avg_pct", "disk_write_bytes"],
    "metrics_that_regressed": ["time_ms"],
    "next_action": "PROMOTE_TO_NEXT_TIER",
    "notes": "Рассмотреть кеширование AST чтобы сократить время в следующем прогоне"
  }
}
```

### Шаблон MD-отчёта (один файл = один прогон)

```markdown
# E01 — AST узлы вместо BPE токенов
**Запущено:** 30 апреля 2026, 12:00 UTC  
**Датасет:** Tier 1 — micro (~3 099 токенов, 19 файлов)  
**Гипотеза:** Токенизация через AST уменьшит словарь в 5 раз и повысит точность

## Результат: ✅ ПРОШЁЛ → повышаем до Tier 2

| Метрика | Baseline (BPE) | Improved (AST) | Изменение |
|---|---|---|---|
| Ошибка предсказания | 31.2% | 25.5% | **-18.4% ✅** |
| Время (мс) | 45 мс | 50 мс | +12.3% ⚠️ |
| Память (МБ) | 87 МБ | 68 МБ | **-22.1% ✅** |
| CPU (среднее) | 23% | 21% | **-8.5% ✅** |
| Диск (байт) | 48 КБ | 16 КБ | **-67.2% ✅** |

## Что произошло
AST Roslyn разбирает C# файл за ~8 мс. Словарь сократился с 347 BPE токенов
до 73 AST типов. Модель видит StructuredType вместо "class" + " " + имя.

## Что делать дальше
Запустить E01 на Tier 2. Добавить кеш AST чтобы убрать +12% по времени.
```

---

## Шесть экспериментов

### E01 — AST токенизация vs BPE (уровень 33)

**Гипотеза:** Представление кода как типов AST-узлов (200 типов) вместо BPE токенов (1000+) уменьшает ошибку предсказания и размер хранения.

**Что измеряем:**
- Baseline: BPE токенизатор из подпроекта 01 — размер словаря, точность предсказания следующего токена по N-граммам
- Improved: Roslyn CSharpSyntaxTree → типы узлов как токены, те же N-граммы

**Как измеряем точность (без обучения нейросети):**
```
1. Взять 100 случайных позиций в коде
2. Для каждой позиции: посмотреть 3 предыдущих токена
3. Найти в остальном коде все вхождения этой же 3-граммы
4. Предсказать следующий токен как самый частый после этой 3-граммы
5. Абсолютная ошибка = доля позиций где предсказание не совпало

Baseline: 3-граммы из BPE токенов (строки вроде "public", " bool", " Validate")
Improved: 3-граммы из AST типов (узлы вроде "MethodDecl", "ReturnType", "Identifier")
```

**Реализация (C#, < 50 строк рабочего кода):**
```csharp
// Experiment01Runner.cs
// Запускает E01: BPE vs AST, измеряет 5 метрик, записывает JSON + MD

public sealed class Experiment01Runner
{
    private readonly string projectRoot;    // корень llm_mvp_core
    private readonly ILogger logger;

    public async Task<ExperimentResult> RunAsync(DatasetTier tier)
    {
        var files     = LoadFilesForTier(tier);
        var stopwatch = Stopwatch.StartNew();
        var memBefore = GC.GetTotalMemory(true);

        // Baseline: BPE токенизация
        var bpeTokens  = TokenizeAllFilesBpe(files);
        var bpeMetrics = MeasureNGramAccuracy(bpeTokens, ngram: 3, samples: 100);

        // Improved: AST узлы
        var astTokens  = TokenizeAllFilesAst(files);
        var astMetrics = MeasureNGramAccuracy(astTokens, ngram: 3, samples: 100);

        return BuildResult("E01", bpeMetrics, astMetrics, stopwatch, memBefore);
    }

    private IReadOnlyList<string> TokenizeAllFilesAst(IReadOnlyList<string> files)
    {
        var tokens = new List<string>();
        foreach (var file in files)
        {
            // Roslyn разбирает файл в синтаксическое дерево
            var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));

            // Обходим все узлы дерева и берём их тип как токен
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                // SyntaxKind.MethodDeclaration → "MethodDecl" (короткое имя)
                tokens.Add(node.Kind().ToString().Replace("Syntax", ""));
            }
        }
        return tokens;
    }
}
```

**Ожидаемый результат на Tier 1:**
- Ошибка предсказания: -15 до -25% (словарь в 4–5 раз меньше = меньше вариантов)
- Время: +5 до +15% (Roslyn разбор добавляет ~8 мс на файл)
- Память: -20 до -35% (токены как enum значения, не строки)
- CPU: -5 до -10%
- Диск: -50 до -70% (AST сериализации компактнее)

**Минимальные ресурсы:**
- Время прогона: ~3–8 секунд на Tier 1
- RAM: ~40–60 МБ
- CPU: короткий пик при Roslyn разборе, потом тихо
- Диск: ~2–5 КБ JSON + ~1 КБ MD

---

### E02 — Ранжирование по частоте (уровень 34)

**Гипотеза:** Если предлагать наиболее часто встречающийся следующий токен для данного типа, точность предсказания методов повысится по сравнению с алфавитным списком.

**Что измеряем:**
- Baseline: для выражения `myList.` — случайный или алфавитный порядок методов
- Improved: порядок по частоте вызовов в проекте (из статистики UsageFrequencyCollector)

**Как измеряем без нейросети:**
```
1. Найти в коде все выражения вида `тип.метод()`
2. Для 50 таких выражений спрятать часть после точки
3. Baseline: предложить первый метод алфавитно
4. Improved: предложить метод с наибольшей частотой для данного типа
5. Сравнить точность top-1 и top-5
```

**Ожидаемый результат:**
- Top-1 точность: +20 до +35% (частый порядок vs алфавитный)
- Время: аналогично baseline (поиск в словаре O(1))
- Память: +5% (хранить таблицу частот)

---

### E03 — N-граммная модель (уровень 35)

**Гипотеза:** N-граммная модель обученная на самом проекте предсказывает следующий токен точнее чем случайное угадывание.

**Что измеряем:**
- Baseline: случайное предсказание из словаря (нижняя граница)
- Improved: биграммная модель P(B|A) обученная на данном проекте
- Improved+: триграммная модель P(C|A,B)

**Прогрессивное измерение:**
```
Прогон 1: биграмма на Tier 1 (6 файлов) → ожидаем +15 до +30%
Прогон 2: триграмма на Tier 1 → ожидаем ещё +10 до +15%
Прогон 3: биграмма на Tier 2 (18 файлов) → ожидаем +5 до +10% от Tier 1
```

**Ключевая метрика — перплексия (сложность предсказания):**
```
Перплексия = 2^(-среднее_log2_вероятности)
Ниже = модель более уверена = лучше предсказывает

Baseline (случайно, словарь 500 токенов): перплексия ~500
Биграмма: ожидаем перплексию ~50-100
Триграмма: ожидаем ~20-50
Нейросеть (для сравнения в будущем): ~5-15
```

---

### E04 — BM25 поиск по локальному контексту (уровень 36)

**Гипотеза:** BM25 поиск по другим файлам проекта находит релевантный контекст который улучшает предсказание при автодополнении.

**Что измеряем:**
- Baseline: предсказание только по локальному контексту (текущий файл)
- Improved: BM25 поиск по всем файлам проекта + добавление найденного в контекст

**Как измеряем:**
```
1. Взять 30 выражений где метод определён в другом файле
   (например: jwtService.Validate — определён не там где вызывается)
2. Baseline: предсказать только из локального файла → часто промах
3. Improved: BM25 по ключевому слову "jwtService" находит файл с определением
             → первые 3 метода из найденного файла в предложения
4. Recall@3: сколько раз правильный метод попал в топ-3
```

**Использует:** подпроект 12 (SearchEngine — InvertedIndex + BM25Scorer) уже реализован!

**Ожидаемый результат:**
- Recall@3 для cross-file методов: +30 до +50%
- Время: +50 до +200% (BM25 поиск по файлам), но в абсолютных числах ~20-50 мс
- Память: +20 до +40% (индекс всех файлов)

---

### E05 — Фильтр синтаксически допустимых токенов (уровень 37)

**Гипотеза:** Ограничение предложений только синтаксически допустимыми токенами снижает ошибку предсказания и гарантирует корректность.

**Что измеряем:**
- Baseline: N-граммная модель предлагает любой токен из словаря
- Improved: Roslyn анализирует контекст → маска допустимых → N-граммная модель только по допустимым

**Специальная метрика — синтаксическая корректность:**
```
Для 100 предсказаний:
  Baseline: сколько предложений синтаксически корректны
  Improved: сколько предложений синтаксически корректны

Ожидание:
  Baseline: ~72% синтаксически корректны (ошибки: "public public", "return if")
  Improved: ~99% (фильтр убирает недопустимые)
```

---

### E06 — PageRank ранжирование (уровень 38)

**Гипотеза:** Методы с высоким PageRank в графе вызовов должны предлагаться первыми и встречаться в датасете чаще.

**Что измеряем:**
- Baseline: методы ранжированы по частоте упоминания в тексте
- Improved: методы ранжированы по PageRank (кто их вызывает × важность вызывающего)

**Как измеряем:**
```
1. Построить граф вызовов из подпроекта 23 (OssEquivalents/CallGraphBuilder)
2. Запустить PageRank из подпроекта 07 (GraphEngine/PageRankCalculator)
3. Сравнить топ-10 методов по частоте vs топ-10 по PageRank
4. Для каждого случайного запроса: какой подход чаще предлагает правильный метод

Метрика: MRR (Mean Reciprocal Rank — среднеобратный ранг)
  MRR = среднее(1/позиция_правильного_ответа)
  MRR=1.0 — правильный всегда первый
  MRR=0.5 — правильный в среднем на второй позиции
```

---

## Общий бегунок экспериментов (ExperimentRunner)

```csharp
// ExperimentRunner.cs
// Запускает все шесть экспериментов последовательно на заданном тире.
// Сохраняет JSON + MD для каждого прогона.
// Обновляет progress.json и progress.md.

public sealed class ExperimentRunner : BaseService
{
    private readonly string projectRoot;     // путь к llm_mvp_core
    private readonly string outputDir;       // куда писать результаты

    // Запустить все эксперименты на указанном тире
    // tier — уровень датасета (1=micro, 2=small, 3=medium, ...)
    public async Task<ProgressSummary> RunAllAsync(int tier)
    {
        var runners = new IExperimentRunner[]
        {
            new Experiment01Runner(projectRoot),  // AST vs BPE
            new Experiment02Runner(projectRoot),  // Частота
            new Experiment03Runner(projectRoot),  // N-граммы
            new Experiment04Runner(projectRoot),  // BM25
            new Experiment05Runner(projectRoot),  // Фильтр
            new Experiment06Runner(projectRoot),  // PageRank
        };

        var results = new List<ExperimentResult>();

        foreach (var runner in runners)
        {
            Logger.LogInformation("Запуск {Name} на Tier {Tier}...", runner.Name, tier);

            var result = await runner.RunAsync(tier);

            // Сохраняем JSON
            await SaveJsonAsync(result, outputDir);

            // Сохраняем MD
            await SaveMarkdownAsync(result, outputDir);

            results.Add(result);

            // Если эксперимент прошёл — предлагаем перейти на следующий тир
            if (result.Verdict.Passed && result.Verdict.NextAction == "PROMOTE_TO_NEXT_TIER")
            {
                Logger.LogInformation("✅ {Name} прошёл! Готов к Tier {Next}",
                    runner.Name, tier + 1);
            }
        }

        // Обновляем сводку прогресса
        var summary = BuildProgressSummary(results, tier);
        await SaveProgressAsync(summary, outputDir);

        return summary;
    }
}
```

---

## Измерение ресурсов (ResourceMonitor)

```csharp
// ResourceMonitor.cs
// Измеряет время, память, CPU и диск во время выполнения эксперимента.
// Не требует внешних инструментов — только System.Diagnostics.
public sealed class ResourceMonitor : IDisposable
{
    private readonly Stopwatch   stopwatch;
    private readonly Process     currentProcess;
    private readonly List<double> cpuSamples;
    private readonly string      tempCounterFile;
    private Timer?               sampleTimer;

    private long diskBytesWrittenAtStart;
    private long memoryAtStart;

    public ResourceMonitor()
    {
        stopwatch        = new Stopwatch();
        currentProcess   = Process.GetCurrentProcess();
        cpuSamples       = new List<double>();
        tempCounterFile  = Path.GetTempFileName();
    }

    // Начать измерение — вызвать перед экспериментом
    public void Start()
    {
        // Запоминаем стартовые значения
        memoryAtStart           = GC.GetTotalMemory(forceFullCollection: true);
        diskBytesWrittenAtStart = GetDiskWrittenBytes();

        // Запускаем таймер для периодического сбора CPU (каждые 100 мс)
        sampleTimer = new Timer(SampleCpu, null, 0, 100);

        stopwatch.Start();
    }

    // Остановить измерение и вернуть результат
    public ResourceMeasurement Stop()
    {
        stopwatch.Stop();
        sampleTimer?.Dispose();

        return new ResourceMeasurement
        {
            // Время в миллисекундах
            TotalTimeMs        = stopwatch.ElapsedMilliseconds,

            // Память: пик минус старт = сколько взял алгоритм
            MemoryPeakMb       = GC.GetTotalMemory(false) / 1_048_576.0,
            MemoryBaselineMb   = memoryAtStart / 1_048_576.0,
            MemoryAlgorithmMb  = (GC.GetTotalMemory(false) - memoryAtStart) / 1_048_576.0,

            // CPU: среднее по образцам
            CpuAveragePct      = cpuSamples.Count > 0 ? cpuSamples.Average() : 0,
            CpuPeakPct         = cpuSamples.Count > 0 ? cpuSamples.Max() : 0,

            // Диск: что было записано во время измерения
            DiskWrittenBytes   = GetDiskWrittenBytes() - diskBytesWrittenAtStart,
        };
    }

    // Периодически собираем загрузку CPU
    private void SampleCpu(object? state)
    {
        try
        {
            currentProcess.Refresh();
            // Примерная загрузка CPU через счётчик TotalProcessorTime
            cpuSamples.Add(currentProcess.TotalProcessorTime.TotalMilliseconds);
        }
        catch { /* тихо игнорируем если процесс завершился */ }
    }

    // Получить байты записанные на диск через счётчик IO
    private static long GetDiskWrittenBytes()
    {
        try
        {
            // На Linux: /proc/self/io даёт write_bytes
            if (File.Exists("/proc/self/io"))
            {
                var lines = File.ReadAllLines("/proc/self/io");
                var writeLine = lines.FirstOrDefault(l => l.StartsWith("write_bytes:"));
                if (writeLine != null && long.TryParse(writeLine.Split(':')[1].Trim(), out long bytes))
                    return bytes;
            }
        }
        catch { /* на Windows это недоступно */ }
        return 0;
    }

    public void Dispose() => sampleTimer?.Dispose();
}
```

---

## Progress tracking: progress.json + progress.md

### progress.json (обновляется после каждого прогона)

```json
{
  "last_updated": "2026-04-30T12:00:00Z",
  "current_tier": 1,
  "experiments": {
    "E01": {
      "name": "AST токенизация",
      "status": "PASSED",
      "current_tier": 1,
      "best_delta_pct": -18.4,
      "runs_count": 1,
      "last_run": "2026-04-30T12:00:00Z"
    },
    "E02": { "status": "NOT_STARTED" },
    "E03": { "status": "NOT_STARTED" },
    "E04": { "status": "NOT_STARTED" },
    "E05": { "status": "NOT_STARTED" },
    "E06": { "status": "NOT_STARTED" }
  },
  "overall_status": "IN_PROGRESS",
  "promoted_to_tier2": [],
  "notes": "Первый прогон E01 — успех. Запустить E02."
}
```

### progress.md (для человека)

```markdown
# Прогресс микро-экспериментов
**Обновлено:** 30 апреля 2026, 12:00  
**Текущий тир:** Tier 1 (micro, ~9 100 токенов)

## Статус экспериментов

| Эксперимент | Подпроект | Статус | Лучший результат | Тир |
|---|---|---|---|---|
| E01 AST токенизация | 33 | ✅ PASSED | ошибка -18.4% | Tier 1 |
| E02 Частота вызовов | 34 | ⏳ Не запущен | — | — |
| E03 N-граммная модель | 35 | ⏳ Не запущен | — | — |
| E04 BM25 контекст | 36 | ⏳ Не запущен | — | — |
| E05 Фильтр допустимых | 37 | ⏳ Не запущен | — | — |
| E06 PageRank ранжирование | 38 | ⏳ Не запущен | — | — |

## Что сделать дальше
1. Запустить E02 на Tier 1
2. После прохождения E01–E06 на Tier 1 — повысить до Tier 2
```

---

## Структура файлов в Solution

```
llm_mvp_core/
└── experiments/                              ← новая папка
    ├── schema/
    │   └── experiment-result-v1.json         ← JSON Schema (уже создана)
    ├── framework/
    │   ├── ExperimentResult.cs               ← C# типы для JSON
    │   ├── ResourceMonitor.cs                ← замер 5 метрик
    │   ├── ExperimentRunner.cs               ← общий бегунок
    │   ├── DatasetLoader.cs                  ← загрузка по тирам
    │   └── ReportWriter.cs                   ← запись JSON + MD
    ├── runners/
    │   ├── Experiment01_AstTokenizer.cs
    │   ├── Experiment02_FrequencyRanking.cs
    │   ├── Experiment03_NGramModel.cs
    │   ├── Experiment04_Bm25Context.cs
    │   ├── Experiment05_SyntaxFilter.cs
    │   └── Experiment06_PageRankRanking.cs
    └── results/                              ← автоматически создаётся
        ├── E01-AST-tokenizer/
        │   └── run-YYYY-MM-DD-NNN.json / .md
        ├── ...
        ├── progress.json
        └── progress.md
```

---

## Правила экономии ресурсов

Каждый эксперимент должен соблюдать:

```
Tier 1 ограничения:
  Время:   < 30 секунд общего времени прогона
  RAM:     < 200 МБ (измеряется ResourceMonitor)
  CPU:     никаких параллельных потоков на Tier 1 (один поток)
  Диск:    < 1 МБ на все файлы результатов

Tier 2 ограничения:
  Время:   < 5 минут
  RAM:     < 500 МБ
  CPU:     можно 2 потока
  Диск:    < 10 МБ

Общие правила:
  НЕЛЬЗЯ: обучать нейросеть (даже 1 шаг)
  НЕЛЬЗЯ: скачивать внешние данные
  НЕЛЬЗЯ: держать открытые файлы дольше 1 секунды
  МОЖНО:  использовать уже реализованные подпроекты 01–38 напрямую
  МОЖНО:  кешировать AST разбор между экспериментами (один прогон Roslyn)
  ДОЛЖЕН: каждый прогон производить два файла: .json + .md
```

---

## Проверка плана (Plan Review)

### Проверка цели: все ли уровни покрыты?

| Уровень автодополнения | Эксперимент | Что тестируем | Benchmark метрика |
|---|---|---|---|
| 33 Статический анализ | E01 | AST vocab vs BPE vocab | N-граммная точность |
| 34 Частота | E02 | Частотный порядок vs алфавит | Top-1/Top-5 точность |
| 35 N-граммы | E03 | Биграмма + триграмма | Перплексия |
| 36 BM25 | E04 | BM25 cross-file vs локальный | Recall@3 |
| 37 Фильтр | E05 | Roslyn маска vs без маски | Синтаксическая корректность |
| 38 PageRank | E06 | PageRank ранжирование vs частота | MRR |

### Проверка ресурсов: не нарушаем ли ограничения?

Tier 1 — micro (~9 100 токенов):
- E01 AST: Roslyn разбор 6 файлов × ~8 мс = ~50 мс. RAM: ~30 МБ. ✅
- E02 Частота: подсчёт словаря O(n). < 5 мс. RAM: < 5 МБ. ✅
- E03 N-граммы: построение биграмм из 9K токенов. < 10 мс. RAM: < 10 МБ. ✅
- E04 BM25: InvertedIndex из подпроекта 12 на 6 файлах. < 20 мс. RAM: < 20 МБ. ✅
- E05 Фильтр: Roslyn для каждой позиции, кешировать AST. < 50 мс. RAM: < 30 МБ. ✅
- E06 PageRank: граф 6 файлов × ~8 мс + PageRank 20 итераций. < 30 мс. ✅

Итого E01–E06 на Tier 1: < 3 секунд, < 150 МБ RAM, < 100 КБ на диске. ✅

### Проверка DNA правил

| Правило | Проверка |
|---|---|
| Правило 3 (OperationResult везде) | ExperimentResult.Verdict.Passed + ErrorMessage ✓ |
| Правило 7 (идемпотентность) | run_timestamp в имени файла → повторный запуск создаёт новый файл ✓ |
| Правило 8 (хранить перед событием) | JSON сохраняется до отображения прогресса ✓ |

---

*Версия 1.0 — MICRO-EXPERIMENT-FRAMEWORK · не касается XIIGen*
