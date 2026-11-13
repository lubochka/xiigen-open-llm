# E08 — Структура предложения на обычном языке
## Подробный план для Claude Code v1.0

**Проект:** `35.ExperimentsV2` (добавляем новый эксперимент)  
**Датасет:** Moby Dick (Project Gutenberg, публичное достояние) — 1 файл .txt  
**Внешние зависимости:** Python + SpaCy (устанавливаем один раз)  
**Этот документ не касается XIIGen.**

---

## Что делаем

Проверяем: работает ли принцип AST-токенов (структурные типы вместо
плоских подслов) на человеческом тексте, а не на коде.

Вместо Roslyn → SpaCy (Python). Вместо SyntaxKind → Universal Dependencies pos-tags.
Пример: "The cat sat" → `["DET", "NOUN", "VERB"]`

---

## Шаг 0 — Подготовить датасет (один раз)

### 0а. Скачать текстовый файл

```
# В терминале (PowerShell или bash):
curl -o moby-dick.txt https://www.gutenberg.org/files/2701/2701-0.txt
```

Или в C# коде при первом запуске:
```csharp
using var client = new HttpClient();
await File.WriteAllTextAsync("moby-dick.txt",
    await client.GetStringAsync("https://www.gutenberg.org/files/2701/2701-0.txt"));
```

Файл весит ~1.2 МБ. Содержит ~220 000 слов.

Сохранить файл в: `C:\Projects\llm_mvp_core\text-datasets\moby-dick.txt`
(или любой путь — он будет передаваться в `DatasetLoader.LoadText()`)

### 0б. Установить Python и SpaCy

```
pip install spacy
python -m spacy download en_core_web_sm
```

Проверить что работает:
```python
# В Python:
import spacy
nlp = spacy.load("en_core_web_sm")
doc = nlp("The cat sat on the mat")
print([token.pos_ for token in doc])
# Ожидаем: ['DET', 'NOUN', 'VERB', 'ADP', 'DET', 'NOUN']
```

### 0в. Создать Python-скрипт-сервис

**Путь:** `35.ExperimentsV2/Infrastructure/spacy_pos_tagger.py`

```python
#!/usr/bin/env python3
"""
Читает предложения из stdin (одно предложение на строку),
возвращает POS-теги в JSON (массив строк) в stdout.
Используется из C# через Process.
"""
import sys
import json
import spacy

nlp = spacy.load("en_core_web_sm")

for line in sys.stdin:
    line = line.strip()
    if not line:
        print("[]")
        sys.stdout.flush()
        continue
    doc = nlp(line)
    tags = [token.pos_ for token in doc if not token.is_space]
    print(json.dumps(tags))
    sys.stdout.flush()
```

---

## Шаг 1 — Создать TextDatasetLoader.cs

**Путь:** `35.ExperimentsV2/Infrastructure/TextDatasetLoader.cs`

```csharp
namespace ExperimentsV2;

// TextDatasetLoader загружает текстовые файлы для E08.
// Разбивает на предложения по точкам/восклицательным/вопросительным знакам.
public static class TextDatasetLoader
{
    // Загрузить первые maxSentences предложений из текстового файла.
    // textFilePath — абсолютный путь к .txt файлу.
    public static OperationResult<TextDatasetContents> Load(
        string textFilePath, int maxSentences)
    {
        if (!File.Exists(textFilePath))
            return OperationResult<TextDatasetContents>.Fail(
                $"Файл не найден: {textFilePath}");

        string raw = File.ReadAllText(textFilePath, System.Text.Encoding.UTF8);

        // Разбить на предложения по . ! ?
        // Убрать очень короткие (< 5 слов) и очень длинные (> 50 слов)
        var sentences = raw
            .Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s =>
            {
                int words = s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                return words >= 5 && words <= 50;
            })
            .Take(maxSentences)
            .ToList()
            .AsReadOnly();

        if (sentences.Count == 0)
            return OperationResult<TextDatasetContents>.Fail(
                "Нет подходящих предложений в файле");

        return OperationResult<TextDatasetContents>.Ok(new TextDatasetContents
        {
            FilePath  = textFilePath,
            Sentences = sentences,
            CombinedText = string.Join(" ", sentences)
        });
    }
}

public sealed record TextDatasetContents
{
    public required string FilePath  { get; init; }
    public required IReadOnlyList<string> Sentences { get; init; }
    public required string CombinedText { get; init; }
    public int SentenceCount => Sentences.Count;
}
```

---

## Шаг 2 — Создать SpacyPosTagger.cs

**Путь:** `35.ExperimentsV2/Infrastructure/SpacyPosTagger.cs`

```csharp
using System.Diagnostics;
using System.Text.Json;

namespace ExperimentsV2;

// SpacyPosTagger вызывает Python-скрипт spacy_pos_tagger.py через subprocess.
// Передаёт предложения в stdin, получает POS-теги в stdout как JSON.
public sealed class SpacyPosTagger : IDisposable
{
    private readonly Process _process;
    private bool _disposed;

    // scriptPath — абсолютный путь к spacy_pos_tagger.py
    public SpacyPosTagger(string scriptPath)
    {
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Скрипт не найден: {scriptPath}");

        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = "python",
                Arguments              = $"\"{scriptPath}\"",
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            }
        };
        _process.Start();
    }

    // Получить POS-теги для одного предложения.
    // Возвращает список строк вроде ["DET", "NOUN", "VERB", "ADP", "DET", "NOUN"]
    public OperationResult<IReadOnlyList<string>> GetPosTags(string sentence)
    {
        try
        {
            // Убрать переносы строк из предложения (скрипт читает по строкам)
            string sanitized = sentence.Replace('\n', ' ').Replace('\r', ' ');
            _process.StandardInput.WriteLine(sanitized);
            _process.StandardInput.Flush();

            string? line = _process.StandardOutput.ReadLine();
            if (line == null)
                return OperationResult<IReadOnlyList<string>>.Fail("SpaCy не ответил");

            var tags = JsonSerializer.Deserialize<List<string>>(line);
            if (tags == null || tags.Count == 0)
                return OperationResult<IReadOnlyList<string>>.Fail("Пустой ответ SpaCy");

            return OperationResult<IReadOnlyList<string>>.Ok(tags.AsReadOnly());
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<string>>.Fail($"SpaCy ошибка: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _process.StandardInput.Close(); } catch { }
        try { _process.WaitForExit(2000); } catch { }
        try { _process.Kill(); } catch { }
        _process.Dispose();
    }
}
```

---

## Шаг 3 — Создать E08TextSyntax.cs

**Путь:** `35.ExperimentsV2/Experiments/E08TextSyntax.cs`

```csharp
namespace ExperimentsV2;

// E08: сравнение POS-тегов (структурные типы слов) vs BPE-подслов
// на обычном английском тексте.
// Аналог E06 (AST vs BPE), но для человеческого языка.
// Baseline: bigram на BPE-токенах текста (vocab 512)
// Enhanced: bigram на POS-тегах (vocab ~17 тегов Universal Dependencies)
public sealed class E08TextSyntax
{
    // scriptPath — путь к spacy_pos_tagger.py
    private readonly string _scriptPath;

    public E08TextSyntax(string scriptPath)
    {
        _scriptPath = scriptPath;
    }

    public OperationResult<ExperimentV2Result> Run(
        TextDatasetContents dataset,
        int tier = 1)
    {
        if (dataset.SentenceCount < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало предложений");

        // ── Baseline: BPE на всём тексте ─────────────────────────────────────
        var t0  = Stopwatch();
        var bpeTok = new Tokenizer.BpeTokenizer();
        bpeTok.Train(dataset.CombinedText, vocabSize: 512);
        var bpeTokens = bpeTok.Encode(dataset.CombinedText);

        int bpeSplit   = (int)(bpeTokens.Count * 0.8);
        var bpeTrain   = bpeTokens.Take(bpeSplit).ToList();
        var bpeVal     = bpeTokens.Skip(bpeSplit).ToList();

        var baseBigram = new BigramModelV2();
        baseBigram.Train(bpeTrain);
        var baseEval   = baseBigram.Evaluate(bpeVal);
        if (!baseEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Baseline: " + baseEval.ErrorMessage);

        (double bpePerp, double bpeTop1, _, _) = baseEval.Value!;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: POS-теги через SpaCy ───────────────────────────────────
        var t1 = Stopwatch();
        var posTokens = new List<int>();
        var posVocab  = new Dictionary<string, int>(); // тег → числовой id

        using var tagger = new SpacyPosTagger(_scriptPath);

        foreach (var sentence in dataset.Sentences)
        {
            var tagsResult = tagger.GetPosTags(sentence);
            if (!tagsResult.IsSuccess) continue;

            foreach (var tag in tagsResult.Value!)
            {
                if (!posVocab.TryGetValue(tag, out int tagId))
                    posVocab[tag] = tagId = posVocab.Count;
                posTokens.Add(tagId);
            }
        }

        if (posTokens.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                "SpaCy вернул слишком мало тегов. Проверить что Python и SpaCy установлены.");

        int posSplit   = (int)(posTokens.Count * 0.8);
        var posTrain   = posTokens.Take(posSplit).ToList();
        var posVal     = posTokens.Skip(posSplit).ToList();

        var enhBigram  = new BigramModelV2();
        enhBigram.Train(posTrain);
        var enhEval    = enhBigram.Evaluate(posVal);
        if (!enhEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Enhanced: " + enhEval.ErrorMessage);

        (double posPerp, double posTop1, _, _) = enhEval.Value!;
        double enhancedMs = ElapsedMs(t1);

        // Scaling signal: насколько POS словарь меньше BPE словаря
        double vocabRatio = (double)posVocab.Count / bpeTok.VocabSize;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E08-TEXT-SYNTAX",
            Tier              = tier,
            BaselinePrimary   = bpePerp,
            EnhancedPrimary   = posPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = vocabRatio,
            ScalingSignalNote = "pos_vocab_ratio (POS типов / BPE типов — меньше = компактнее)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"sentences={dataset.SentenceCount}, " +
                                $"bpeTokens={bpeTokens.Count}, bpeVocab={bpeTok.VocabSize}, " +
                                $"posTokens={posTokens.Count}, posVocab={posVocab.Count} " +
                                $"({string.Join(",", posVocab.Keys.Take(10))}), " +
                                $"bpeTop1={bpeTop1:P1}, posTop1={posTop1:P1}"
        });
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
```

---

## Шаг 4 — Добавить в Runner

E08 требует отдельного запуска с текстовым датасетом — не через `RunAll()`
(там только C# файлы). Добавить отдельный метод:

### В `ExperimentRunner.cs` добавить публичный метод:

```csharp
// Запустить E08 с текстовым датасетом.
// textFilePath — путь к .txt файлу (например moby-dick.txt)
// scriptPath  — путь к spacy_pos_tagger.py
public OperationResult<ExperimentV2Result> RunE08Text(
    string textFilePath,
    string scriptPath,
    int maxSentences = 500,
    int tier = 1)
{
    Console.Write($"[E08] Загрузка текста ({maxSentences} предложений)... ");
    var loadResult = TextDatasetLoader.Load(textFilePath, maxSentences);
    if (!loadResult.IsSuccess)
    {
        Console.WriteLine($"ERROR: {loadResult.ErrorMessage}");
        return OperationResult<ExperimentV2Result>.Fail(loadResult.ErrorMessage);
    }
    var dataset = loadResult.Value!;
    Console.WriteLine($"{dataset.SentenceCount} предложений загружено.");

    Console.Write($"[E08] Запуск (POS-теги через SpaCy)... ");
    var experiment = new E08TextSyntax(scriptPath);
    var result = experiment.Run(dataset, tier);

    if (!result.IsSuccess)
    {
        Console.WriteLine($"ERROR: {result.ErrorMessage}");
        return result;
    }

    var writeOp = ResultWriter.Write(result.Value!, _outputRoot);
    string fileLine = writeOp.IsSuccess ? $"-> {writeOp.Value}" : $"WRITE FAILED";

    var v = result.Value!;
    string status = v.Passed ? "PASSED" : (v.ShouldScaleUp ? "SCALE_UP" : "FAILED");
    Console.WriteLine(
        $"perplexity down baseline={v.BaselinePrimary:F2} " +
        $"enhanced={v.EnhancedPrimary:F2} delta={v.ImprovementPct:+0.0;-0.0}% | {status}");
    Console.WriteLine($"  {fileLine}");

    return result;
}
```

### В `ResultWriter.FolderName()`:
```csharp
"E08-TEXT-SYNTAX" => "E08-TEXT-SYNTAX",
```

---

## Шаг 5 — Запустить

```csharp
var runner = new ExperimentRunner(
    projectRoot: @"C:\Projects\llm_mvp_core",
    outputRoot:  @"C:\Projects\llm_mvp_core\experiments-v2");

runner.RunE08Text(
    textFilePath: @"C:\Projects\llm_mvp_core\text-datasets\moby-dick.txt",
    scriptPath:   @"C:\Projects\llm_mvp_core\35.ExperimentsV2\Infrastructure\spacy_pos_tagger.py",
    maxSentences: 500,
    tier: 1);
```

**Ожидаемый вывод:**
```
[E08] Загрузка текста (500 предложений)... 500 предложений загружено.
[E08] Запуск (POS-теги через SpaCy)...
perplexity down baseline=312.45 enhanced=8.23 delta=+97.4% | PASSED
  -> .../E08-TEXT-SYNTAX/results/run-YYYY-MM-DD-001.json
```

**Ожидаемые числа в notes:**
```
sentences=500, bpeTokens=8420, bpeVocab=512,
posTokens=12340, posVocab=17 (NOUN,VERB,DET,ADP,ADJ,ADV,PUNCT,PRON,...),
bpeTop1=8.2%, posTop1=41.5%
```

**Интерпретация:**
- `posVocab=17` — только 17 уникальных POS-тегов (против 512 BPE) — словарь крошечный
- `posTop1=41%` vs `bpeTop1=8%` — POS значительно лучше предсказывает
- Сравнить с E06 на коде: там AST top1=50% vs BPE top1=21%

---

## Если Python/SpaCy не установлен

Ошибка при запуске: `Скрипт не найден` или `SpaCy вернул слишком мало тегов`

Проверить:
```
python --version         # должен быть 3.8+
python -m spacy info     # должен показать установленные модели
python -m spacy validate # проверить что en_core_web_sm загружена
```

Если `python` не найден — попробовать `python3` и изменить
в `SpacyPosTagger.cs` строку `FileName = "python"` на `FileName = "python3"`.

---

*E08 план v1.0 · не касается XIIGen*
