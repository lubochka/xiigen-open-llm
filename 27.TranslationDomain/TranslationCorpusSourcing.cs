// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

// P1/P2 corpus sourcing helpers for TranslationDomain.
// These types do not download data and do not translate words. They only turn
// locally supplied, license-checked frequency rows into git-safe word-list JSONL.
public sealed record FrequencyWordSourceRow(
    string Word,
    string? Lemma,
    string Pos,
    int Rank,
    string SourceId,
    string License,
    bool CommercialAllowed);

public sealed record FrequencyWordEntry(
    string Word,
    string Lemma,
    string Pos,
    int Rank,
    string Language,
    string SourceId,
    string License);

public sealed record FrequencyWordListBuildOptions(
    string Language,
    int Limit,
    IReadOnlySet<string>? AllowedPos = null,
    IReadOnlySet<string>? AllowedLicenses = null);

public sealed record FrequencyWordListBuildResult(
    IReadOnlyList<FrequencyWordEntry> Entries,
    IReadOnlyList<FrequencyWordSourceRow> RejectedRows,
    IReadOnlyDictionary<string, int> RejectionCounts)
{
    public bool MeetsLimit => Entries.Count >= RequestedLimit;

    [JsonIgnore]
    public int RequestedLimit { get; init; }
}

public static class FrequencyWordListBuilder
{
    private static readonly HashSet<string> DefaultAllowedPos = new(StringComparer.OrdinalIgnoreCase)
    {
        "NOUN",
        "VERB",
        "ADJ",
        "ADV"
    };

    private static readonly HashSet<string> DefaultAllowedLicenses = new(StringComparer.OrdinalIgnoreCase)
    {
        "public_domain",
        "cc0",
        "cc_by",
        "cc_by_2fr",
        "cc_by_4",
        "cc_by_4_0",
        "cc_by_sa_verified",
        "project_open_license_verified"
    };

    public static FrequencyWordListBuildResult Build(
        IEnumerable<FrequencyWordSourceRow> rows,
        FrequencyWordListBuildOptions options)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Language))
            throw new ArgumentException("Language is required.", nameof(options));

        if (options.Limit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Limit must be positive.");

        var allowedPos = options.AllowedPos ?? DefaultAllowedPos;
        var allowedLicenses = options.AllowedLicenses ?? DefaultAllowedLicenses;
        var accepted = new List<FrequencyWordEntry>();
        var rejected = new List<FrequencyWordSourceRow>();
        var rejectionCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var seenLemmas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows.OrderBy(row => row.Rank))
        {
            var rejection = GetRejectionReason(row, allowedPos, allowedLicenses);
            if (rejection is not null)
            {
                rejected.Add(row);
                Increment(rejectionCounts, rejection);
                continue;
            }

            var lemma = NormalizeLemma(row);
            if (!seenLemmas.Add(lemma))
            {
                rejected.Add(row);
                Increment(rejectionCounts, "duplicate_lemma");
                continue;
            }

            accepted.Add(new FrequencyWordEntry(
                Word: row.Word.Trim(),
                Lemma: lemma,
                Pos: NormalizePos(row.Pos),
                Rank: row.Rank,
                Language: options.Language.Trim().ToLowerInvariant(),
                SourceId: row.SourceId.Trim(),
                License: NormalizeLicense(row.License)));

            if (accepted.Count >= options.Limit)
                break;
        }

        return new FrequencyWordListBuildResult(
            Entries: accepted,
            RejectedRows: rejected,
            RejectionCounts: rejectionCounts)
        {
            RequestedLimit = options.Limit
        };
    }

    public static IReadOnlyList<FrequencyWordSourceRow> ParseDelimited(
        string text,
        char delimiter = '\t')
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var lines = text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            return [];

        var headers = SplitLine(lines[0], delimiter)
            .Select((name, index) => (Name: NormalizeHeader(name), Index: index))
            .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);

        RequireHeader(headers, "word");
        RequireHeader(headers, "pos");
        RequireHeader(headers, "rank");
        RequireHeader(headers, "source_id");
        RequireHeader(headers, "license");
        RequireHeader(headers, "commercial_allowed");

        var rows = new List<FrequencyWordSourceRow>();
        for (var i = 1; i < lines.Length; i++)
        {
            var columns = SplitLine(lines[i], delimiter);
            if (columns.Length == 0)
                continue;

            rows.Add(new FrequencyWordSourceRow(
                Word: Get(columns, headers, "word"),
                Lemma: TryGet(columns, headers, "lemma"),
                Pos: Get(columns, headers, "pos"),
                Rank: int.Parse(Get(columns, headers, "rank")),
                SourceId: Get(columns, headers, "source_id"),
                License: Get(columns, headers, "license"),
                CommercialAllowed: ParseBool(Get(columns, headers, "commercial_allowed"))));
        }

        return rows;
    }

    public static string ToJsonl(IEnumerable<FrequencyWordEntry> entries)
    {
        var builder = new StringBuilder();
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new JsonStringEnumConverter() }
        };

        foreach (var entry in entries)
            builder.AppendLine(JsonSerializer.Serialize(entry, options));

        return builder.ToString();
    }

    public static async Task WriteJsonlAsync(
        IEnumerable<FrequencyWordEntry> entries,
        string outputPath,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await File.WriteAllTextAsync(outputPath, ToJsonl(entries), Encoding.UTF8, ct);
    }

    private static string? GetRejectionReason(
        FrequencyWordSourceRow row,
        IReadOnlySet<string> allowedPos,
        IReadOnlySet<string> allowedLicenses)
    {
        if (string.IsNullOrWhiteSpace(row.Word))
            return "missing_word";

        if (row.Rank <= 0)
            return "invalid_rank";

        if (!allowedPos.Contains(NormalizePos(row.Pos)))
            return "pos_not_allowed";

        if (!row.CommercialAllowed)
            return "commercial_not_allowed";

        if (!allowedLicenses.Contains(NormalizeLicense(row.License)))
            return "license_not_allowed";

        if (string.IsNullOrWhiteSpace(row.SourceId))
            return "missing_source_id";

        return null;
    }

    private static string NormalizeLemma(FrequencyWordSourceRow row)
        => string.IsNullOrWhiteSpace(row.Lemma)
            ? row.Word.Trim().ToLowerInvariant()
            : row.Lemma.Trim().ToLowerInvariant();

    private static string NormalizePos(string value)
        => value.Trim().ToUpperInvariant();

    private static string NormalizeLicense(string value)
        => value.Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");

    private static void Increment(Dictionary<string, int> counts, string key)
        => counts[key] = counts.TryGetValue(key, out var existing) ? existing + 1 : 1;

    private static string[] SplitLine(string line, char delimiter)
        => line.Split(delimiter).Select(value => value.Trim()).ToArray();

    private static string NormalizeHeader(string value)
        => value.Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");

    private static void RequireHeader(IReadOnlyDictionary<string, int> headers, string name)
    {
        if (!headers.ContainsKey(name))
            throw new FormatException($"Missing required column '{name}'.");
    }

    private static string Get(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headers,
        string name)
    {
        var index = headers[name];
        return index < columns.Count ? columns[index] : string.Empty;
    }

    private static string? TryGet(
        IReadOnlyList<string> columns,
        IReadOnlyDictionary<string, int> headers,
        string name)
        => headers.TryGetValue(name, out var index) && index < columns.Count
            ? columns[index]
            : null;

    private static bool ParseBool(string value)
        => value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) ||
           value.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase) ||
           value.Trim().Equals("1", StringComparison.OrdinalIgnoreCase);
}

public sealed record TatoebaPilotBuildOptions(
    string RussianSentencesPath,
    string EnglishSentencesPath,
    string RussianEnglishLinksPath,
    string OutputDirectory,
    string? PrivateOutputDirectory,
    int WordLimit = 100,
    int SentencesPerWord = 10,
    int MinSentenceWords = 4,
    int MaxSentenceWords = 18);

public sealed record TatoebaPilotBuildResult(
    int CandidatePairs,
    int EnglishWords,
    int RussianWords,
    int EnglishSentenceIndexRows,
    int RussianSentenceIndexRows,
    bool MeetsPilotGate,
    IReadOnlyDictionary<string, string> OutputPaths);

public sealed record TatoebaSentenceIndexEntry(
    string Word,
    string Lemma,
    string Language,
    string Direction,
    int Rank,
    string Pos,
    int SourceSentenceId,
    int TargetSentenceId,
    string SourcePattern,
    string TargetPattern,
    int SourceWordCount,
    int TargetWordCount,
    string SourceId,
    string License);

public sealed record TatoebaPrivateSentencePair(
    string Direction,
    string Word,
    string Language,
    int SourceSentenceId,
    int TargetSentenceId,
    string SourceText,
    string TargetText,
    string SourcePattern,
    string TargetPattern,
    string SourceId,
    string License);

public static partial class TatoebaPilotCorpusBuilder
{
    private const string SourceId = "tatoeba_2026_05_23";
    private const string License = "cc_by_2fr";

    private static readonly Regex WordRegex = new(@"[\p{L}]+(?:['’][\p{L}]+)?", RegexOptions.Compiled);

    private static readonly HashSet<string> EnglishStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "and", "for", "that", "with", "this", "from", "they", "you", "your", "are", "was", "were",
        "have", "has", "had", "not", "but", "his", "her", "she", "him", "our", "their", "there", "here",
        "what", "when", "where", "who", "why", "how", "into", "onto", "about", "than", "then", "them"
    };

    private static readonly HashSet<string> RussianStopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "что", "это", "как", "для", "или", "она", "они", "оно", "его", "её", "ему", "мне", "тебе",
        "меня", "тебя", "нас", "вас", "уже", "еще", "ещё", "быть", "был", "была", "были", "кто",
        "где", "когда", "почему", "потому", "чтобы", "если", "при", "над", "под", "без", "через"
    };

    public static async Task<TatoebaPilotBuildResult> BuildAsync(
        TatoebaPilotBuildOptions options,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        Validate(options);

        var links = await ReadLinksAsync(options.RussianEnglishLinksPath, ct);
        var requiredEnglishIds = links.SelectMany(pair => pair.Value).ToHashSet();
        var english = await ReadSentencesAsync(options.EnglishSentencesPath, requiredEnglishIds, ct);
        var pairs = await ReadCandidatePairsAsync(options, links, english, ct);

        var englishSelection = BuildSelection(
            pairs,
            language: "en",
            direction: "EN_RU",
            wordLimit: options.WordLimit,
            sentencesPerWord: options.SentencesPerWord);
        var russianSelection = BuildSelection(
            pairs,
            language: "ru",
            direction: "RU_EN",
            wordLimit: options.WordLimit,
            sentencesPerWord: options.SentencesPerWord);

        Directory.CreateDirectory(options.OutputDirectory);
        var outputPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["word_list_en"] = Path.Combine(options.OutputDirectory, "word-list-en-pilot-100.jsonl"),
            ["word_list_ru"] = Path.Combine(options.OutputDirectory, "word-list-ru-pilot-100.jsonl"),
            ["sentence_index_en_ru"] = Path.Combine(options.OutputDirectory, "sentence-index-en-ru-pilot-100.jsonl"),
            ["sentence_index_ru_en"] = Path.Combine(options.OutputDirectory, "sentence-index-ru-en-pilot-100.jsonl"),
            ["attribution_manifest"] = Path.Combine(options.OutputDirectory, "tatoeba-pilot-attribution-manifest.json")
        };

        await FrequencyWordListBuilder.WriteJsonlAsync(englishSelection.WordEntries, outputPaths["word_list_en"], ct);
        await FrequencyWordListBuilder.WriteJsonlAsync(russianSelection.WordEntries, outputPaths["word_list_ru"], ct);
        await WriteJsonlAsync(englishSelection.IndexEntries, outputPaths["sentence_index_en_ru"], ct);
        await WriteJsonlAsync(russianSelection.IndexEntries, outputPaths["sentence_index_ru_en"], ct);
        await WriteAttributionAsync(outputPaths["attribution_manifest"], ct);

        if (!string.IsNullOrWhiteSpace(options.PrivateOutputDirectory))
        {
            Directory.CreateDirectory(options.PrivateOutputDirectory);
            outputPaths["private_pairs_en_ru"] = Path.Combine(options.PrivateOutputDirectory, "sentence-pairs-en-ru-pilot-100.jsonl");
            outputPaths["private_pairs_ru_en"] = Path.Combine(options.PrivateOutputDirectory, "sentence-pairs-ru-en-pilot-100.jsonl");
            await WriteJsonlAsync(englishSelection.PrivatePairs, outputPaths["private_pairs_en_ru"], ct);
            await WriteJsonlAsync(russianSelection.PrivatePairs, outputPaths["private_pairs_ru_en"], ct);
        }

        var meetsPilotGate =
            englishSelection.WordEntries.Count >= options.WordLimit &&
            russianSelection.WordEntries.Count >= options.WordLimit &&
            englishSelection.IndexEntries.Count >= options.WordLimit * options.SentencesPerWord &&
            russianSelection.IndexEntries.Count >= options.WordLimit * options.SentencesPerWord;

        return new TatoebaPilotBuildResult(
            CandidatePairs: pairs.Count,
            EnglishWords: englishSelection.WordEntries.Count,
            RussianWords: russianSelection.WordEntries.Count,
            EnglishSentenceIndexRows: englishSelection.IndexEntries.Count,
            RussianSentenceIndexRows: russianSelection.IndexEntries.Count,
            MeetsPilotGate: meetsPilotGate,
            OutputPaths: outputPaths);
    }

    private static void Validate(TatoebaPilotBuildOptions options)
    {
        if (options.WordLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "WordLimit must be positive.");
        if (options.SentencesPerWord <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "SentencesPerWord must be positive.");
        if (!File.Exists(options.RussianSentencesPath))
            throw new FileNotFoundException("Russian Tatoeba sentences file not found.", options.RussianSentencesPath);
        if (!File.Exists(options.EnglishSentencesPath))
            throw new FileNotFoundException("English Tatoeba sentences file not found.", options.EnglishSentencesPath);
        if (!File.Exists(options.RussianEnglishLinksPath))
            throw new FileNotFoundException("Russian-English Tatoeba links file not found.", options.RussianEnglishLinksPath);
    }

    private static async Task<Dictionary<int, List<int>>> ReadLinksAsync(string path, CancellationToken ct)
    {
        var links = new Dictionary<int, List<int>>();
        await foreach (var line in File.ReadLinesAsync(path, ct))
        {
            var columns = line.Split('\t');
            if (columns.Length < 2 ||
                !int.TryParse(columns[0], out var russianId) ||
                !int.TryParse(columns[1], out var englishId))
            {
                continue;
            }

            if (!links.TryGetValue(russianId, out var englishIds))
            {
                englishIds = [];
                links[russianId] = englishIds;
            }

            englishIds.Add(englishId);
        }

        return links;
    }

    private static async Task<Dictionary<int, string>> ReadSentencesAsync(
        string path,
        IReadOnlySet<int> requiredIds,
        CancellationToken ct)
    {
        var sentences = new Dictionary<int, string>();
        await foreach (var line in File.ReadLinesAsync(path, ct))
        {
            var columns = line.Split('\t', 3);
            if (columns.Length < 3 || !int.TryParse(columns[0], out var id) || !requiredIds.Contains(id))
                continue;

            sentences[id] = columns[2];
        }

        return sentences;
    }

    private static async Task<List<ParallelSentencePair>> ReadCandidatePairsAsync(
        TatoebaPilotBuildOptions options,
        IReadOnlyDictionary<int, List<int>> links,
        IReadOnlyDictionary<int, string> english,
        CancellationToken ct)
    {
        var pairs = new List<ParallelSentencePair>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var line in File.ReadLinesAsync(options.RussianSentencesPath, ct))
        {
            var columns = line.Split('\t', 3);
            if (columns.Length < 3 || !int.TryParse(columns[0], out var russianId))
                continue;
            if (!links.TryGetValue(russianId, out var englishIds))
                continue;

            var russianText = columns[2];
            var russianWords = ExtractWords(russianText).Count;
            if (russianWords < options.MinSentenceWords || russianWords > options.MaxSentenceWords)
                continue;

            foreach (var englishId in englishIds)
            {
                if (!english.TryGetValue(englishId, out var englishText))
                    continue;

                var englishWords = ExtractWords(englishText).Count;
                if (englishWords < options.MinSentenceWords || englishWords > options.MaxSentenceWords)
                    continue;

                var key = $"{russianId}:{englishId}";
                if (!seen.Add(key))
                    continue;

                pairs.Add(new ParallelSentencePair(
                    RussianId: russianId,
                    EnglishId: englishId,
                    RussianText: russianText,
                    EnglishText: englishText,
                    RussianPattern: BuildPattern(russianText, "ru"),
                    EnglishPattern: BuildPattern(englishText, "en"),
                    RussianWordCount: russianWords,
                    EnglishWordCount: englishWords));
                break;
            }
        }

        return pairs;
    }

    private static TatoebaPilotSelection BuildSelection(
        IReadOnlyList<ParallelSentencePair> pairs,
        string language,
        string direction,
        int wordLimit,
        int sentencesPerWord)
    {
        var candidates = new Dictionary<string, List<ParallelSentencePair>>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in pairs)
        {
            var text = language == "en" ? pair.EnglishText : pair.RussianText;
            foreach (var word in ExtractWords(text).Select(word => word.ToLowerInvariant()).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!IsContentWord(word, language))
                    continue;

                if (!candidates.TryGetValue(word, out var rows))
                {
                    rows = [];
                    candidates[word] = rows;
                }

                rows.Add(pair);
            }
        }

        var selectedWords = candidates
            .Where(item => item.Value.Count >= sentencesPerWord)
            .OrderByDescending(item => item.Value.Count)
            .ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Take(wordLimit)
            .ToList();

        var wordEntries = new List<FrequencyWordEntry>();
        var indexEntries = new List<TatoebaSentenceIndexEntry>();
        var privatePairs = new List<TatoebaPrivateSentencePair>();
        var rank = 1;
        foreach (var (word, rows) in selectedWords.Select(item => (item.Key, item.Value)))
        {
            var selectedRows = SelectDiverseRows(rows, language, sentencesPerWord);
            var pos = GuessPos(word, language);
            wordEntries.Add(new FrequencyWordEntry(
                Word: word,
                Lemma: word,
                Pos: pos,
                Rank: rank,
                Language: language,
                SourceId: SourceId,
                License: License));

            foreach (var row in selectedRows)
            {
                var sourceText = language == "en" ? row.EnglishText : row.RussianText;
                var targetText = language == "en" ? row.RussianText : row.EnglishText;
                var sourcePattern = language == "en" ? row.EnglishPattern : row.RussianPattern;
                var targetPattern = language == "en" ? row.RussianPattern : row.EnglishPattern;
                var sourceId = language == "en" ? row.EnglishId : row.RussianId;
                var targetId = language == "en" ? row.RussianId : row.EnglishId;
                var sourceWordCount = language == "en" ? row.EnglishWordCount : row.RussianWordCount;
                var targetWordCount = language == "en" ? row.RussianWordCount : row.EnglishWordCount;

                indexEntries.Add(new TatoebaSentenceIndexEntry(
                    Word: word,
                    Lemma: word,
                    Language: language,
                    Direction: direction,
                    Rank: rank,
                    Pos: pos,
                    SourceSentenceId: sourceId,
                    TargetSentenceId: targetId,
                    SourcePattern: sourcePattern,
                    TargetPattern: targetPattern,
                    SourceWordCount: sourceWordCount,
                    TargetWordCount: targetWordCount,
                    SourceId: SourceId,
                    License: License));

                privatePairs.Add(new TatoebaPrivateSentencePair(
                    Direction: direction,
                    Word: word,
                    Language: language,
                    SourceSentenceId: sourceId,
                    TargetSentenceId: targetId,
                    SourceText: sourceText,
                    TargetText: targetText,
                    SourcePattern: sourcePattern,
                    TargetPattern: targetPattern,
                    SourceId: SourceId,
                    License: License));
            }

            rank++;
        }

        return new TatoebaPilotSelection(wordEntries, indexEntries, privatePairs);
    }

    private static IReadOnlyList<ParallelSentencePair> SelectDiverseRows(
        IReadOnlyList<ParallelSentencePair> rows,
        string language,
        int limit)
    {
        var selected = new List<ParallelSentencePair>();
        var seenPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            var pattern = language == "en" ? row.EnglishPattern : row.RussianPattern;
            if (seenPatterns.Add(pattern))
                selected.Add(row);
            if (selected.Count >= limit)
                return selected;
        }

        foreach (var row in rows)
        {
            if (selected.Contains(row))
                continue;
            selected.Add(row);
            if (selected.Count >= limit)
                break;
        }

        return selected;
    }

    private static IReadOnlyList<string> ExtractWords(string text)
        => WordRegex.Matches(text)
            .Select(match => match.Value)
            .Where(word => word.Length >= 2)
            .ToList();

    private static bool IsContentWord(string word, string language)
    {
        if (word.Length < 3)
            return false;
        if (word.Any(char.IsDigit))
            return false;
        return language == "en"
            ? !EnglishStopWords.Contains(word)
            : !RussianStopWords.Contains(word);
    }

    private static string BuildPattern(string text, string language)
        => string.Join("|", ExtractWords(text).Select(word => GuessPos(word, language)).Take(12));

    private static string GuessPos(string word, string language)
    {
        var lower = word.ToLowerInvariant();
        if (language == "en")
        {
            if (lower.EndsWith("ly", StringComparison.Ordinal))
                return "ADV";
            if (lower.EndsWith("ing", StringComparison.Ordinal) || lower.EndsWith("ed", StringComparison.Ordinal))
                return "VERB";
            if (lower.EndsWith("ous", StringComparison.Ordinal) ||
                lower.EndsWith("ful", StringComparison.Ordinal) ||
                lower.EndsWith("ive", StringComparison.Ordinal) ||
                lower.EndsWith("able", StringComparison.Ordinal) ||
                lower.EndsWith("ible", StringComparison.Ordinal) ||
                lower.EndsWith("al", StringComparison.Ordinal))
            {
                return "ADJ";
            }

            return "NOUN";
        }

        if (lower.EndsWith("ть", StringComparison.Ordinal) ||
            lower.EndsWith("ться", StringComparison.Ordinal) ||
            lower.EndsWith("ла", StringComparison.Ordinal) ||
            lower.EndsWith("ли", StringComparison.Ordinal) ||
            lower.EndsWith("ет", StringComparison.Ordinal) ||
            lower.EndsWith("ют", StringComparison.Ordinal) ||
            lower.EndsWith("ит", StringComparison.Ordinal) ||
            lower.EndsWith("ат", StringComparison.Ordinal))
        {
            return "VERB";
        }

        if (lower.EndsWith("но", StringComparison.Ordinal) || lower.EndsWith("ко", StringComparison.Ordinal))
            return "ADV";

        if (lower.EndsWith("ый", StringComparison.Ordinal) ||
            lower.EndsWith("ой", StringComparison.Ordinal) ||
            lower.EndsWith("ая", StringComparison.Ordinal) ||
            lower.EndsWith("ое", StringComparison.Ordinal) ||
            lower.EndsWith("ые", StringComparison.Ordinal))
        {
            return "ADJ";
        }

        return "NOUN";
    }

    private static async Task WriteJsonlAsync<T>(IEnumerable<T> rows, string path, CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var builder = new StringBuilder();
        foreach (var row in rows)
            builder.AppendLine(JsonSerializer.Serialize(row, options));

        await File.WriteAllTextAsync(path, builder.ToString(), Encoding.UTF8, ct);
    }

    private static async Task WriteAttributionAsync(string path, CancellationToken ct)
    {
        var payload = new
        {
            source_id = SourceId,
            provider = "Tatoeba",
            title = "Tatoeba parallel corpus",
            license = "CC BY 2.0 France",
            license_normalized = License,
            url = "https://tatoeba.org/en/downloads",
            export_date = "2026-05-23",
            commercial_allowed = true,
            attribution_required = true,
            raw_text_policy = "raw sentence text is private; git-safe artifacts store IDs, patterns, and attribution only"
        };
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine,
            Encoding.UTF8,
            ct);
    }

    private sealed record ParallelSentencePair(
        int RussianId,
        int EnglishId,
        string RussianText,
        string EnglishText,
        string RussianPattern,
        string EnglishPattern,
        int RussianWordCount,
        int EnglishWordCount);

    private sealed record TatoebaPilotSelection(
        IReadOnlyList<FrequencyWordEntry> WordEntries,
        IReadOnlyList<TatoebaSentenceIndexEntry> IndexEntries,
        IReadOnlyList<TatoebaPrivateSentencePair> PrivatePairs);
}
