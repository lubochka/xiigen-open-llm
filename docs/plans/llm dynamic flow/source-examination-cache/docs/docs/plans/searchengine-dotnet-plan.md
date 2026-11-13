# Проект 12 — SearchEngine (поисковый движок на основе Lucene алгоритмов)

## Место в Solution

```
LlmFromScratch.sln
├── ...
└── 12.SearchEngine/     — инвертированный индекс, BM25, HNSW, анализаторы
```

Этот проект расширяет Graph RAG (проект 08) полноценным поисковым движком.
Использует проекты 06 (VectorSearch) и 07 (GraphEngine).

---

## Что это и зачем

OpenSearch и Elasticsearch — это распределённые серверы поверх одного ядра: Apache Lucene. Lucene — библиотека на Java, весь код открыт под Apache 2.0. Именно в ней живут все алгоритмы: инвертированный индекс, BM25, анализаторы текста, HNSW для векторного поиска.

Этот проект реализует то же самое на C# без Java, без JVM, без внешних серверов. Результат — встраиваемый поисковый движок который работает прямо внутри .NET процесса.

Полезно для:
- Поиска по кодовой базе в VS Code плагине (без внешнего OpenSearch сервера)
- Поиска по документации в Graph RAG
- Полнотекстового поиска по любым документам

---

## Источники для изучения

```
Алгоритмы:
https://github.com/apache/lucene                    Apache 2.0 · Java

Конкретные файлы:
lucene/core/src/java/org/apache/lucene/search/similarities/BM25Similarity.java
lucene/core/src/java/org/apache/lucene/index/IndexWriter.java
lucene/core/src/java/org/apache/lucene/util/hnsw/HnswGraphBuilder.java
lucene/core/src/java/org/apache/lucene/analysis/standard/StandardTokenizer.java

Оригинальная бумага BM25:
"Okapi at TREC-3" Robertson et al. 1994
```

---

## Структура подпроекта

```
12.SearchEngine/
├── Analysis/
│   ├── IAnalyzer.cs         — интерфейс анализатора
│   ├── StandardAnalyzer.cs  — разбивка текста на токены
│   ├── LowercaseFilter.cs   — приведение к нижнему регистру
│   ├── StopWordFilter.cs    — удаление стоп-слов (и, в, на...)
│   └── StemFilter.cs        — стемминг (playing → play)
├── Index/
│   ├── InvertedIndex.cs     — основная структура данных
│   ├── IndexWriter.cs       — запись документов
│   ├── IndexReader.cs       — чтение и поиск
│   ├── Segment.cs           — сегмент индекса
│   └── TermDictionary.cs    — словарь термов
├── Search/
│   ├── BM25Scorer.cs        — алгоритм релевантности
│   ├── QueryParser.cs       — разбор текстового запроса
│   ├── BooleanQuery.cs      — AND/OR/NOT запросы
│   └── SearchResult.cs      — результат поиска
├── Storage/
│   ├── IndexStorage.cs      — хранение на диске
│   └── SegmentMerger.cs     — слияние сегментов
└── Tests/
    └── SearchEngineTests.cs
```

---

## Фаза 1 — Анализаторы (2–3 дня)

Анализатор — это конвейер преобразований текста перед индексацией. Тот же конвейер применяется к поисковому запросу. Именно поэтому "Running" и "run" находят один документ — оба превращаются в токен "run".

```csharp
// Analysis/IAnalyzer.cs
public interface IAnalyzer
{
    // Текст → список токенов
    // "Hello World!" → ["hello", "world"]
    IEnumerable<AnalyzedToken> Analyze(string text);
}

public record AnalyzedToken
{
    public string Term     { get; init; } = ""; // нормализованный токен
    public int    Position { get; init; }       // позиция в тексте
    public int    Start    { get; init; }       // байтовое смещение начала
    public int    End      { get; init; }       // байтовое смещение конца
}

// Analysis/StandardAnalyzer.cs
// Стандартный анализатор: разбивка → нижний регистр → стоп-слова
// Читать: lucene/analysis/common/.../StandardAnalyzer.java
public class StandardAnalyzer : IAnalyzer
{
    private readonly HashSet<string> stopWords;

    public StandardAnalyzer(IEnumerable<string>? customStopWords = null)
    {
        // Стандартные стоп-слова английского языка
        stopWords = new HashSet<string>(
            new[] { "a", "an", "the", "is", "are", "was", "were",
                    "in", "on", "at", "to", "for", "of", "and", "or" }
            .Concat(customStopWords ?? [])
        );
    }

    public IEnumerable<AnalyzedToken> Analyze(string text)
    {
        int position = 0;

        // Шаг 1: разбиваем на токены по пробелам и знакам препинания
        foreach (var (token, start, end) in Tokenize(text))
        {
            // Шаг 2: нижний регистр
            var lower = token.ToLowerInvariant();

            // Шаг 3: убираем знаки препинания по краям
            var clean = lower.Trim('.', ',', '!', '?', ';', ':', '"', '\'');

            // Шаг 4: пропускаем стоп-слова и пустые строки
            if (string.IsNullOrEmpty(clean) || stopWords.Contains(clean))
                continue;

            // Шаг 5: стемминг — приводим к корневой форме
            var stem = Porter.Stem(clean);

            yield return new AnalyzedToken
            {
                Term     = stem,
                Position = position++,
                Start    = start,
                End      = end
            };
        }
    }

    // Разбивка на токены: находим границы слов
    private IEnumerable<(string token, int start, int end)> Tokenize(string text)
    {
        int start = -1;
        for (int i = 0; i <= text.Length; i++)
        {
            bool isLetter = i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_');

            if (isLetter && start < 0)
                start = i;
            else if (!isLetter && start >= 0)
            {
                yield return (text[start..i], start, i);
                start = -1;
            }
        }
    }
}

// Analysis/PorterStemmer.cs
// Алгоритм стемминга Портера 1980 года
// Читать: https://tartarus.org/martin/PorterStemmer/
// Правила суффиксов для английского языка
// "running" → "run", "happiness" → "happi", "studies" → "studi"
public static class Porter
{
    public static string Stem(string word)
    {
        if (word.Length <= 2) return word;

        word = Step1a(word); // мн. число: caresses → caress, flies → fli
        word = Step1b(word); // -ing, -ed: walking → walk, hoped → hope
        word = Step1c(word); // -y → -i: happy → happi
        word = Step2(word);  // суффиксы: ational → ate, izer → ize
        word = Step3(word);  // суффиксы: icate → ic, ful → пусто
        word = Step4(word);  // суффиксы: -ance, -ence, -er, -ic...
        word = Step5(word);  // финальная обрезка

        return word;
    }

    private static string Step1a(string w)
    {
        if (w.EndsWith("sses")) return w[..^2];
        if (w.EndsWith("ies"))  return w[..^2];
        if (w.EndsWith("ss"))   return w;
        if (w.EndsWith("s"))    return w[..^1];
        return w;
    }

    // ... остальные шаги по той же схеме
    // полная реализация: ~150 строк правил суффиксов
}
```

---

## Фаза 2 — Инвертированный индекс (3–4 дня)

Инвертированный индекс — это словарь где ключ это токен, а значение — список документов содержащих этот токен. Называется инвертированным потому что обычный индекс идёт от документа к словам, а этот наоборот.

```
Обычный:   документ_1 → [слово_A, слово_B, слово_C]
Инвертированный: слово_A → [документ_1, документ_3, документ_7]
                  слово_B → [документ_1, документ_2]
```

```csharp
// Index/PostingsList.cs
// Список вхождений токена в документах
// Читать: lucene/core/src/java/org/apache/lucene/index/PostingsEnum.java
public class PostingsList
{
    // Отсортированный список документов содержащих этот термин
    // Каждая запись: (docId, частота, позиции в документе)
    private readonly SortedList<int, Posting> postings = new();

    public void Add(int docId, int position)
    {
        if (!postings.TryGetValue(docId, out var posting))
        {
            posting = new Posting { DocId = docId };
            postings[docId] = posting;
        }
        posting.Frequency++;
        posting.Positions.Add(position);
    }

    // Document Frequency — в скольких документах встречается термин
    // Используется в BM25 для вычисления IDF
    public int DocumentFrequency => postings.Count;

    public IEnumerable<Posting> GetAll() => postings.Values;
}

public class Posting
{
    public int DocId     { get; init; }
    public int Frequency { get; set; }      // TF — частота в документе
    public List<int> Positions { get; } = new(); // для phrase queries
}

// Index/InvertedIndex.cs
// Главная структура данных
public class InvertedIndex
{
    // Термин → список документов
    private readonly Dictionary<string, PostingsList> terms = new();

    // Метаданные документов: id → (длина, путь к файлу)
    private readonly Dictionary<int, DocumentInfo> documents = new();

    // Среднее количество токенов в документе (нужно для BM25)
    private double averageDocLength;
    private int nextDocId = 0;

    // Добавить документ в индекс
    public int AddDocument(string text, string path, IAnalyzer analyzer)
    {
        int docId = nextDocId++;
        var tokens = analyzer.Analyze(text).ToList();

        documents[docId] = new DocumentInfo
        {
            Path      = path,
            Length    = tokens.Count, // длина в токенах
            RawText   = text
        };

        // Добавляем каждый токен в инвертированный список
        foreach (var token in tokens)
        {
            if (!terms.TryGetValue(token.Term, out var postings))
            {
                postings = new PostingsList();
                terms[token.Term] = postings;
            }
            postings.Add(docId, token.Position);
        }

        // Пересчитываем среднюю длину документа
        averageDocLength = documents.Values.Average(d => d.Length);

        return docId;
    }

    // Поиск документов содержащих термин
    public PostingsList? GetPostings(string term) =>
        terms.TryGetValue(term, out var p) ? p : null;

    public int TotalDocuments      => documents.Count;
    public double AvgDocLength     => averageDocLength;
    public DocumentInfo? GetDoc(int id) => documents.GetValueOrDefault(id);
}

// Index/SegmentMerger.cs
// Сегменты — это подиндексы. При каждом добавлении документов
// создаётся новый сегмент. Периодически сегменты сливаются в один.
// Это ключевое для производительности: поиск идёт параллельно
// по всем сегментам, затем результаты объединяются.
// Читать: lucene/core/src/java/org/apache/lucene/index/SegmentMerger.java
public class SegmentMerger
{
    // Слить несколько индексов в один
    // Алгоритм: сортировка словарей + слияние posting lists
    public InvertedIndex Merge(IEnumerable<InvertedIndex> segments) { ... }
}
```

---

## Фаза 3 — BM25 (2 дня)

BM25 (Best Match 25) — алгоритм ранжирования документов по релевантности запросу. Пришёл на смену TF-IDF и стал стандартом. Используется в Lucene, OpenSearch, Elasticsearch, Whoosh.

Формула:
```
Score(D, Q) = Σ IDF(qi) * (tf * (k1+1)) / (tf + k1 * (1 - b + b * dl/avgdl))

Где:
  qi   — токен из запроса
  IDF  — Inverse Document Frequency: log((N - df + 0.5) / (df + 0.5) + 1)
         редкие слова важнее частых
  tf   — частота токена в документе
  k1   — насыщение TF (обычно 1.2): уменьшает влияние очень частых слов
  b    — нормализация длины (обычно 0.75): штраф за длинные документы
  dl   — длина документа в токенах
  avgdl — средняя длина документа в коллекции
```

```csharp
// Search/BM25Scorer.cs
// Читать: lucene/.../BM25Similarity.java — главный класс для понимания
public class BM25Scorer
{
    private readonly float k1 = 1.2f; // насыщение TF
    private readonly float b  = 0.75f; // нормализация длины

    // Вычислить IDF для термина
    // N — всего документов, df — в скольких встречается термин
    // Формула из Lucene: log(1 + (N - df + 0.5) / (df + 0.5))
    private float ComputeIdf(int totalDocs, int docFrequency)
    {
        return MathF.Log(1 + (totalDocs - docFrequency + 0.5f) / (docFrequency + 0.5f));
    }

    // Вычислить вклад одного термина в счёт документа
    // tf — частота в документе, dl — длина документа, avgdl — средняя
    private float ComputeTermScore(float idf, int tf, int docLength, double avgDocLength)
    {
        float norm = 1 - b + b * (docLength / (float)avgDocLength);
        float tfNorm = (tf * (k1 + 1)) / (tf + k1 * norm);
        return idf * tfNorm;
    }

    // Оценить релевантность документа запросу
    // queryTerms — токены запроса после анализатора
    public Dictionary<int, float> Score(
        InvertedIndex index,
        IEnumerable<string> queryTerms)
    {
        var scores = new Dictionary<int, float>();

        foreach (var term in queryTerms)
        {
            var postings = index.GetPostings(term);
            if (postings is null) continue;

            float idf = ComputeIdf(index.TotalDocuments, postings.DocumentFrequency);

            foreach (var posting in postings.GetAll())
            {
                var docInfo = index.GetDoc(posting.DocId)!;
                float termScore = ComputeTermScore(
                    idf, posting.Frequency,
                    docInfo.Length, index.AvgDocLength
                );

                scores[posting.DocId] = scores.GetValueOrDefault(posting.DocId) + termScore;
            }
        }

        return scores;
    }
}

// Search/SearchEngine.cs
// Объединяет анализатор, индекс и BM25 в единый поисковый интерфейс
public class SearchEngine
{
    private readonly InvertedIndex index;
    private readonly IAnalyzer     analyzer;
    private readonly BM25Scorer    scorer;

    public SearchEngine(IAnalyzer? analyzer = null)
    {
        this.index    = new InvertedIndex();
        this.analyzer = analyzer ?? new StandardAnalyzer();
        this.scorer   = new BM25Scorer();
    }

    // Добавить документ в индекс
    public int Index(string text, string path) =>
        index.AddDocument(text, path, analyzer);

    // Найти топ-K документов по запросу
    public List<SearchResult> Search(string query, int topK = 10)
    {
        // 1. Анализируем запрос тем же анализатором что и документы
        var queryTerms = analyzer.Analyze(query).Select(t => t.Term);

        // 2. BM25 вычисляет счёт для каждого документа
        var scores = scorer.Score(index, queryTerms);

        // 3. Сортируем по убыванию счёта
        return scores
            .OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select(kv => new SearchResult
            {
                DocId    = kv.Key,
                Score    = kv.Value,
                Path     = index.GetDoc(kv.Key)!.Path,
                Snippet  = GenerateSnippet(index.GetDoc(kv.Key)!.RawText, query)
            })
            .ToList();
    }

    // Сгенерировать фрагмент текста вокруг найденных слов
    // Читать: lucene/highlighter/ — алгоритм подсветки
    private string GenerateSnippet(string text, string query, int snippetLen = 200)
    {
        var queryWords = query.ToLower().Split(' ');
        var lower      = text.ToLower();

        int bestPos = 0;
        foreach (var word in queryWords)
        {
            int pos = lower.IndexOf(word);
            if (pos >= 0) { bestPos = pos; break; }
        }

        int start = Math.Max(0, bestPos - snippetLen / 2);
        int end   = Math.Min(text.Length, start + snippetLen);
        return (start > 0 ? "..." : "") + text[start..end] + (end < text.Length ? "..." : "");
    }
}
```

---

## Фаза 4 — Гибридный поиск BM25 + векторный (2 дня)

Объединяет keyword поиск (BM25) и семантический поиск (HNSW из проекта 06) по схеме Reciprocal Rank Fusion — стандартный метод объединения ранжирований из разных систем.

```csharp
// Search/HybridSearch.cs
// Читать: OpenSearch hybrid search documentation + RRF paper 2009
public class HybridSearch
{
    private readonly SearchEngine  keywordSearch;
    private readonly HnswIndex     vectorIndex;
    private readonly EmbeddingModel embedder;

    // RRF — Reciprocal Rank Fusion
    // Объединяет два ранжирования без нормализации счётов
    // rrf_score = Σ 1 / (k + rank_i)  где k = 60 (константа сглаживания)
    // Документ в топ-1 keyword и топ-1 vector → наивысший совокупный счёт
    private const float RrfK = 60f;

    public async Task<List<SearchResult>> SearchAsync(string query, int topK = 10)
    {
        // 1. Keyword поиск через BM25
        var keywordResults = keywordSearch.Search(query, topK * 2);

        // 2. Векторный поиск через HNSW
        var queryEmbedding  = await embedder.EmbedAsync(query);
        var vectorResults   = vectorIndex.Search(queryEmbedding, topK * 2);

        // 3. RRF: объединяем ранжирования
        var rrf = new Dictionary<string, float>();

        for (int rank = 0; rank < keywordResults.Count; rank++)
        {
            var id = keywordResults[rank].Path;
            rrf[id] = rrf.GetValueOrDefault(id) + 1f / (RrfK + rank + 1);
        }

        for (int rank = 0; rank < vectorResults.Count; rank++)
        {
            var id = vectorResults[rank].id;
            rrf[id] = rrf.GetValueOrDefault(id) + 1f / (RrfK + rank + 1);
        }

        // 4. Сортируем по RRF счёту
        return rrf
            .OrderByDescending(kv => kv.Value)
            .Take(topK)
            .Select(kv => new SearchResult { Path = kv.Key, Score = kv.Value })
            .ToList();
    }
}
```

---

## Фаза 5 — Персистентность (1–2 дня)

```csharp
// Storage/IndexStorage.cs
// Сохраняем индекс на диск и загружаем обратно
// Lucene использует сложный бинарный формат с файлами .tim, .doc, .pos
// Мы используем простой JSON + бинарный для постинг листов
public class IndexStorage
{
    // Сохранить индекс
    // Файлы: terms.json (словарь), postings.bin (бинарные данные), docs.json
    public void Save(InvertedIndex index, string directory)
    {
        Directory.CreateDirectory(directory);

        // Словарь термов → смещения в бинарном файле
        var termOffsets = new Dictionary<string, long>();
        using var postingsFile = File.Create(Path.Combine(directory, "postings.bin"));

        // Для каждого термина пишем постинг лист
        // Формат: [docId: int32][frequency: int32][positionsCount: int32][positions: int32[]]
        foreach (var (term, postings) in index.GetAllTerms())
        {
            termOffsets[term] = postingsFile.Position;
            foreach (var posting in postings.GetAll())
            {
                Write(postingsFile, posting.DocId);
                Write(postingsFile, posting.Frequency);
                Write(postingsFile, posting.Positions.Count);
                foreach (var pos in posting.Positions) Write(postingsFile, pos);
            }
        }

        // Сохраняем словарь смещений
        File.WriteAllText(
            Path.Combine(directory, "terms.json"),
            JsonSerializer.Serialize(termOffsets)
        );

        // Сохраняем метаданные документов
        File.WriteAllText(
            Path.Combine(directory, "docs.json"),
            JsonSerializer.Serialize(index.GetAllDocs())
        );
    }

    private static void Write(Stream s, int value)
    {
        var bytes = BitConverter.GetBytes(value);
        s.Write(bytes);
    }

    // Загрузить индекс
    public InvertedIndex Load(string directory) { ... }
}
```

---

## Связь с остальными проектами Solution

```
12.SearchEngine
  ↑ использует
05.Embeddings    — для векторной части HybridSearch
06.VectorSearch  — HNSW индекс для векторного поиска
01.Tokenizer     — BPE токены как входные данные (опционально)

  ↓ используется в
08.GraphRag      — заменяет или дополняет LocalSearch
                   HybridSearch → более точный поиск по кодовой базе
```

---

## Итог по времени

| Фаза | Содержание | Дней |
|---|---|---|
| 1 | Анализаторы, стемминг | 2–3 |
| 2 | Инвертированный индекс | 3–4 |
| 3 | BM25 и поиск | 2 |
| 4 | Гибридный поиск BM25 + HNSW | 2 |
| 5 | Персистентность | 1–2 |
| **Итого** | | **10–13 дней** |

Полный Solution после добавления проекта 12: **47–63 дня**.
