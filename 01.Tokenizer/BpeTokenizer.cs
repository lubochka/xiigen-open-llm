// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LlmFromScratch.Tests")]

namespace Tokenizer;

// FIX-VOCAB-REACH-01: причина, по которой обучение словаря закончилось.
// Прежде обучение заканчивалось молча в обоих случаях, и вызывающий не мог их
// различить, не прочитав фактический размер словаря по своей догадке.
public enum BpeTrainingExitReason
{
    // Запрошенный размер словаря достигнут: цикл остановился по условию счёта.
    RequestedSizeReached = 0,

    // Соседние пары исчерпаны: каждый кусок учебного текста сжат в один токен,
    // склеивать больше нечего, и запрошенный размер недостижим на этом корпусе.
    // Это НЕ ошибка данных: маленький корпус законен, и обучение на нём верно.
    AdjacentPairsExhausted = 1
}

// FIX-VOCAB-REACH-01: итог одного обучения словаря кусочков слов.
//
// Зачем он нужен. Ёмкость словаря ограничена составом учебного текста, а не
// запросом: потолок равен 256 плюс разность между числом байт в различных
// кусках и числом самих кусков. Девяносто восьмой опыт измерил это прямо —
// запросы 629 и 2048 на корпусе из 21 различного куска (158 байт) дали
// побайтово одно и то же состояние: 336 токенов, 80 слияний. Вызывающий,
// рассчитавший размер матрицы представлений по запрошенным 2048, получал 336 и
// узнавал об этом только если сам догадывался прочитать фактический размер.
//
// Почему итог возвращается, а не хранится свойством. У загруженного словаря
// такого свойства не было бы правдивого значения: он обучен в другом месте и,
// возможно, с другим запросом. Возвращаемый итог существует ровно там, где у
// вопроса есть ответ, и не добавляет экземпляру ни одного поля, поэтому
// сохранённый артефакт словаря не меняется ни на байт.
public sealed record BpeTrainingOutcome(
    // Размер, который запросил вызывающий.
    int RequestedVocabSize,
    // Размер, который фактически получился. Для недостижимого запроса он меньше.
    int AchievedVocabSize,
    // Сколько слияний выучено сверх 256 однобайтовых токенов.
    int LearnedMergeCount,
    // Причина выхода из цикла обучения.
    BpeTrainingExitReason ExitReason)
{
    // Достигнут ли запрошенный размер. Признак выводится из причины выхода, а
    // не хранится отдельно, чтобы два поля не могли разойтись между собой.
    public bool RequestedVocabSizeReached =>
        ExitReason == BpeTrainingExitReason.RequestedSizeReached;

    // Насколько запрос не добран. Ноль, когда запрос достигнут.
    public int VocabSizeShortfall =>
        Math.Max(0, RequestedVocabSize - AchievedVocabSize);
}

/// <summary>
/// Byte-Pair Encoding tokeniser.
/// Train: reads raw text, iteratively merges the most frequent adjacent byte-pair
/// until the vocabulary reaches the target size.
/// Encode/Decode: apply learned merges to map text ↔ integer tokens.
/// </summary>
public class BpeTokenizer
{
    private Dictionary<int, byte[]> _vocab = [];
    private Dictionary<(int, int), int> _merges = [];

    // This is deliberately far below the former 512 MiB ModelServer admission
    // ceiling.  A JSON BPE vocabulary used by this implementation (including
    // Qwen-style byte tokens plus merges) is normally measured in MiB, not
    // hundreds of MiB.  The contract bounds the JSON DOM itself at 64 MiB,
    // a single token at 1 MiB encoded / 768 KiB decoded, and all token bytes
    // retained by the loaded vocabulary at 48 MiB.
    public const long MaximumCanonicalArtifactBytes = 64L * 1024 * 1024;
    public const int MaximumCanonicalEncodedTokenCharacters = 1024 * 1024;
    public const int MaximumCanonicalDecodedTokenBytes = 768 * 1024;
    public const long MaximumCanonicalDecodedVocabularyBytes = 48L * 1024 * 1024;

    public int VocabSize => _vocab.Count;
    // FIX-EOS-MARKER-01: словарь кусочков слов НЕ объявляет признака конца
    // ответа, и теперь говорит это прямо вместо того, чтобы выдумывать его.
    //
    // Прежде здесь возвращался размер словаря минус один, то есть последняя
    // запись обученного словаря, с пояснением «последний токен словаря
    // считается концом ответа». Последняя запись — обычный кусок слова: на
    // измеренном корпусе ею оказался ` close` с номером 335. Потребитель,
    // принявший это число за служебный признак, обрывал ответ на обычном слове,
    // и какой именно кусок станет признаком, менялось при каждом переобучении.
    //
    // Почему возвращается отсутствие, а не выделяется служебный токен. Чтобы
    // признак был настоящим, артефакт словаря должен сказать, КАКОЙ номер
    // служебный. Формат сохранённого состояния этого не позволяет: строгий
    // читатель принимает ровно два свойства верхнего уровня и отвергает любое
    // неизвестное, признака версии в формате нет, а номера обязаны быть
    // непрерывными. Объявить признак явно значило бы сделать три свойства
    // вместо двух и тем сломать совместимость 3210 сохранённых словарей. Это
    // число измерено по ФОРМЕ артефакта, а не по имени файла: файлов с именем
    // `tokenizer.json` в дереве 3383, но 173 из них вовсе не словарь, а
    // описатель полей обходчика кода на 147 байт. Прежняя запись исполнителя
    // называла 3383, и это было завышением. Состав цены: из 3210 словарей 3201
    // лежит в замороженных хозяевах опытов, 8 под `.kiro`, и ровно один в
    // `config`. Такая цена — решение владельца проекта, а не исполнителя, и
    // оно предъявлено отдельно.
    //
    // Служебный токен следующим непрерывным номером формат бы не сломал, но
    // артефакт по-прежнему не говорил бы, какой номер служебный, — сегодняшняя
    // двусмысленность переносилась бы, а не снималась. Поэтому до решения
    // владельца честный ответ один: признака нет.
    //
    // Это не входит в сохранённое состояние: сериализуются только таблица
    // слияний и словарь, поэтому ни один из 3210 артефактов этой правкой не
    // задет.
    public int? EosToken => null;

    // ── Training ──────────────────────────────────────────────────────────────

    // FIX-VOCAB-REACH-01: обучение сообщает свой итог, а не заканчивается молча.
    // Возвращаемое значение можно отбросить, поэтому ни один существующий
    // вызывающий не ломается; но теперь ответ на вопрос «запрос достигнут?»
    // лежит прямо на месте вызова, а не ждёт догадки прочитать размер словаря.
    // Проверка аргумента ниже оставлена исключением НАМЕРЕННО: это ошибка
    // программиста, а не свойство данных, и её замена — отдельный фактор.
    public BpeTrainingOutcome Train(string text, int vocabSize)
    {
        if (vocabSize < 256)
            throw new ArgumentOutOfRangeException(nameof(vocabSize), "Must be ≥ 256");

        // Initialise vocab with all 256 single bytes (tokens 0–255)
        _vocab = Enumerable.Range(0, 256)
            .ToDictionary(i => i, i => new byte[] { (byte)i });
        _merges = [];

        // FIX-BW transfer, plan 31-bpe-weighted-chunks.md, from BW01.
        //
        // Ordinary text repeats the same word over and over. Each repetition
        // used to be carried as its own list and merged on its own, round after
        // round, although every copy of a word goes through exactly the same
        // merges and contributes exactly the same pairs. One representative per
        // distinct chunk is kept instead, carrying how many times it occurred.
        //
        // The learned vocabulary is unchanged: a pair's count is the same sum
        // as before, now reached by adding the weight once instead of adding
        // one many times, and representatives are held in order of first
        // occurrence so ties between equally frequent pairs resolve as before.
        // A repeated identical chunk could never introduce a pair that its
        // first occurrence had not already introduced.
        var chunks = WeightedChunks(text);

        int nextToken = 256;
        // FIX-VOCAB-REACH-01: причина выхода из цикла. По умолчанию — достижение
        // запрошенного размера, потому что именно по этому условию цикл
        // заканчивается в обычном случае; исчерпание пар выставляется ниже на
        // той самой ветви, где прежде стоял молчаливый выход.
        var exitReason = BpeTrainingExitReason.RequestedSizeReached;
        while (_vocab.Count < vocabSize)
        {
            var pairCounts = CountPairs(chunks);
            if (pairCounts.Count == 0)
            {
                // Склеивать больше нечего: каждый кусок сжат в один токен.
                // Запрошенный размер на этом корпусе недостижим, и это
                // сообщается наружу вместо молчаливого выхода. Поведение самого
                // обучения не меняется ни на шаг: словарь и таблица слияний
                // получаются ровно те же, что прежде.
                exitReason = BpeTrainingExitReason.AdjacentPairsExhausted;
                break;
            }

            var best = pairCounts.MaxBy(kv => kv.Value).Key;
            _merges[best] = nextToken;
            _vocab[nextToken] = [.. _vocab[best.Item1], .. _vocab[best.Item2]];
            MergeAll(chunks, best, nextToken);
            nextToken++;
        }

        // Итог собирается из фактического состояния, а не из ожиданий: размер
        // словаря и число слияний читаются с самих таблиц.
        return new BpeTrainingOutcome(
            RequestedVocabSize: vocabSize,
            AchievedVocabSize: _vocab.Count,
            LearnedMergeCount: _merges.Count,
            ExitReason: exitReason);
    }

    // One representative per distinct source string, in order of first
    // occurrence. Nothing is normalised, lower-cased or sorted: only strings
    // that are identical byte for byte share a representative.
    private sealed class WeightedChunk
    {
        internal WeightedChunk(List<int> tokenIds, int firstOccurrenceOrdinal)
        {
            TokenIds = tokenIds;
            Multiplicity = 1;
            FirstOccurrenceOrdinal = firstOccurrenceOrdinal;
        }

        internal List<int> TokenIds { get; set; }

        internal int Multiplicity { get; set; }

        internal int FirstOccurrenceOrdinal { get; }
    }

    private static List<WeightedChunk> WeightedChunks(string text)
    {
        var byText = new Dictionary<string, WeightedChunk>(StringComparer.Ordinal);
        var representatives = new List<WeightedChunk>();
        foreach (var word in RegexSplitter.Split(text))
        {
            if (byText.TryGetValue(word, out var existing))
            {
                existing.Multiplicity++;
                continue;
            }

            var created = new WeightedChunk(
                Encoding.UTF8.GetBytes(word).Select(b => (int)b).ToList(),
                representatives.Count);
            byText[word] = created;
            representatives.Add(created);
        }

        return representatives;
    }

    // ── Encoding ──────────────────────────────────────────────────────────────

    public List<int> Encode(string text)
    {
        var result = new List<int>();
        foreach (var chunk in RegexSplitter.Split(text))
        {
            var ids = Encoding.UTF8.GetBytes(chunk).Select(b => (int)b).ToList();
            ids = ApplyMerges(ids);
            result.AddRange(ids);
        }
        return result;
    }

    // ── Decoding ──────────────────────────────────────────────────────────────

    public string Decode(List<int> tokens)
        // Текст — это строка из тех же байтов: отдельного способа сборки больше нет, поэтому
        // байты, которые видит вызывающий, и байты, из которых получается текст, совпадают.
        => Encoding.UTF8.GetString(DecodeToBytes(tokens));

    // DecodeToBytes возвращает байты ответа ДО превращения в строку.
    //
    // Зачем это нужно. Словарь этого токенизатора байтовый: один токен — один байт. Значит одна
    // русская буква занимает два токена, а составной символ — четыре. Если выдача ответа
    // остановилась по числу токенов, последний символ может оказаться разорванным, и превращение
    // таких байтов в строку даёт символ замены вместо оборванного символа — тихо, без отказа.
    // Решать, где граница ответа, обязан владелец состояния ответа, то есть генератор; для этого
    // ему нужны именно байты, а не уже испорченная строка.
    //
    // О виде возврата. В этом проекте нет конверта результата операции: соседние открытые методы
    // здесь тоже возвращают обычные значения, и добавлять сюда зависимость от ядра ради одного
    // метода — отдельная работа, а не часть этой починки. Поэтому вид возврата сознательно
    // такой же, как у соседей, и это названо здесь, а не скрыто.
    public byte[] DecodeToBytes(List<int> tokens)
        // Неизвестный номер токена даёт ноль байтов — прежнее поведение сборки сохранено дословно.
        => tokens.SelectMany(t =>
            _vocab.TryGetValue(t, out var b) ? b : Array.Empty<byte>()).ToArray();

    // ── Persistence ───────────────────────────────────────────────────────────

    public void Save(string path)
    {
        using var stream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);
        Save(stream);
        stream.Flush(flushToDisk: true);
    }

    // Writes the same canonical bytes as Save(path) to a caller-owned stream.
    // The stream is truncated, positioned at byte 0 before writing, left open,
    // and left positioned at the exact end of the canonical artifact.
    public void Save(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanWrite || !stream.CanSeek)
            throw new ArgumentException("Tokenizer stream must be writable and seekable.", nameof(stream));

        var payload = Encoding.UTF8.GetBytes(SerializeCanonicalState());
        stream.Position = 0;
        stream.SetLength(0);
        stream.Write(payload);
        stream.Flush();
    }

    // Hash the complete canonical tokenizer state used by Save.
    public string ComputeStateSha256()
        => Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(SerializeCanonicalState())))
            .ToLowerInvariant();

    // Hash the exact durable tokenizer artifact bytes.
    public static string ComputeArtifactSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return ComputeArtifactSha256(stream);
    }

    // Hash the complete tokenizer artifact from the caller-owned stream without
    // closing it or changing the caller's current position.
    public static string ComputeArtifactSha256(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("Tokenizer stream must be readable and seekable.", nameof(stream));

        var originalPosition = stream.Position;
        try
        {
            stream.Position = 0;
            return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    public static BpeTokenizer Load(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Load(stream);
    }

    // Load from the caller-owned readable, seekable stream without closing it.
    public static BpeTokenizer Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("Tokenizer stream must be readable and seekable.", nameof(stream));

        var model = JsonSerializer.Deserialize<SavedModel>(stream)
            ?? throw new InvalidOperationException("Failed to deserialize tokenizer");
        if (stream.Position != stream.Length)
            throw new InvalidOperationException("Tokenizer file contains trailing data.");

        var t = new BpeTokenizer
        {
            _vocab = model.Vocab.ToDictionary(
                kv => kv.Key,
                kv => Convert.FromBase64String(kv.Value)),
            _merges = model.Merges.ToDictionary(
                m => (m.A, m.B),
                m => m.Result)
        };
        return t;
    }

    /// <summary>
    /// Parses the durable BPE format with the invariants that make its merge
    /// order meaningful.  This is intentionally stricter than <see cref="Load(Stream)"/>:
    /// ModelServer uses it at an untrusted startup boundary, before it may
    /// report its retained artifacts as ready.
    /// </summary>
    public static BpeTokenizer ValidateAndLoadCanonicalArtifact(
        Stream stream,
        int maximumVocabularySize,
        CancellationToken cancellationToken = default)
        => ValidateAndLoadCanonicalArtifact(
            stream,
            maximumVocabularySize,
            CanonicalTokenizerArtifactLimits.Production,
            cancellationToken);

    /// <summary>
    /// Internal budget overload used to prove that each pre-allocation guard is
    /// enforced by the parser itself.  Production callers always use the fixed
    /// canonical contract above; callers cannot relax it at the host boundary.
    /// </summary>
    internal static BpeTokenizer ValidateAndLoadCanonicalArtifact(
        Stream stream,
        int maximumVocabularySize,
        CanonicalTokenizerArtifactLimits limits,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead || !stream.CanSeek)
            throw new ArgumentException("Tokenizer stream must be readable and seekable.", nameof(stream));
        if (maximumVocabularySize < 256)
            throw new ArgumentOutOfRangeException(nameof(maximumVocabularySize));
        limits.Validate();

        cancellationToken.ThrowIfCancellationRequested();
        if (stream.Length <= 0 || stream.Length > limits.MaximumArtifactBytes)
            throw new InvalidDataException("Tokenizer artifact exceeds the canonical byte budget.");
        stream.Position = 0;
        using var cancellationAwareStream = new CancellationAwareReadStream(
            stream,
            cancellationToken);
        using var document = JsonDocument.Parse(cancellationAwareStream, new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 128,
        });
        cancellationToken.ThrowIfCancellationRequested();

        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Tokenizer root must be an object.");

        JsonElement vocabElement = default;
        JsonElement mergesElement = default;
        var topLevelNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!topLevelNames.Add(property.Name))
                throw new InvalidDataException("Tokenizer contains a duplicate top-level property.");
            switch (property.Name)
            {
                case "Vocab":
                    vocabElement = property.Value;
                    break;
                case "Merges":
                    mergesElement = property.Value;
                    break;
                default:
                    throw new InvalidDataException("Tokenizer contains an unknown top-level property.");
            }
        }

        if (topLevelNames.Count != 2
            || vocabElement.ValueKind != JsonValueKind.Object
            || mergesElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Tokenizer must contain canonical Vocab and Merges values.");
        }

        var vocab = new Dictionary<int, byte[]>();
        var decodedVocabularyBytes = 0L;
        foreach (var property in vocabElement.EnumerateObject())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!int.TryParse(
                    property.Name,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var tokenId)
                || tokenId < 0
                || !StringComparer.Ordinal.Equals(
                    property.Name,
                    tokenId.ToString(CultureInfo.InvariantCulture))
                || property.Value.ValueKind != JsonValueKind.String
                || vocab.ContainsKey(tokenId)
                || vocab.Count >= maximumVocabularySize)
            {
                throw new InvalidDataException("Tokenizer vocabulary entry is malformed.");
            }

            var encodedToken = property.Value.GetString();
            var decodedTokenBytes = GetCanonicalBase64DecodedLength(encodedToken, limits);
            decodedVocabularyBytes = AddDecodedVocabularyBytes(
                decodedVocabularyBytes,
                decodedTokenBytes,
                limits.MaximumDecodedVocabularyBytes);
            if (!vocab.TryAdd(
                    tokenId,
                    DecodeCanonicalBase64(encodedToken, decodedTokenBytes)))
            {
                throw new InvalidDataException("Tokenizer vocabulary entry is malformed.");
            }
        }

        if (vocab.Count < 256)
            throw new InvalidDataException("Tokenizer vocabulary must contain all base byte tokens.");
        for (var tokenId = 0; tokenId < vocab.Count; tokenId++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!vocab.TryGetValue(tokenId, out var tokenBytes) || tokenBytes.Length == 0)
                throw new InvalidDataException("Tokenizer vocabulary IDs must be contiguous and non-empty.");
            if (tokenId < 256 && (tokenBytes.Length != 1 || tokenBytes[0] != (byte)tokenId))
                throw new InvalidDataException("Tokenizer base byte vocabulary is not canonical.");
        }

        var merges = new Dictionary<(int, int), int>();
        var mergeIndex = 0;
        foreach (var mergeElement in mergesElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (a, b, result) = ReadCanonicalMerge(mergeElement);
            if (result != 256 + mergeIndex
                || a < 0
                || b < 0
                || a >= result
                || b >= result
                || !vocab.TryGetValue(a, out var aBytes)
                || !vocab.TryGetValue(b, out var bBytes)
                || !vocab.TryGetValue(result, out var resultBytes)
                || !ConcatenatesTo(aBytes, bBytes, resultBytes)
                || !merges.TryAdd((a, b), result))
            {
                throw new InvalidDataException("Tokenizer merge table is not a canonical BPE history.");
            }
            mergeIndex++;
        }

        if (mergeIndex != vocab.Count - 256)
            throw new InvalidDataException("Tokenizer merge count does not match its vocabulary.");

        return new BpeTokenizer
        {
            _vocab = vocab,
            _merges = merges,
        };
    }

    private static int GetCanonicalBase64DecodedLength(
        string? encoded,
        CanonicalTokenizerArtifactLimits limits)
    {
        if (string.IsNullOrEmpty(encoded))
            throw new InvalidDataException("Tokenizer vocabulary value is empty.");
        if (encoded.Length > limits.MaximumEncodedTokenCharacters
            || encoded.Length % 4 != 0)
        {
            throw new InvalidDataException("Tokenizer vocabulary value exceeds the canonical token byte budget.");
        }

        var padding = encoded[^1] == '='
            ? encoded[^2] == '=' ? 2 : 1
            : 0;
        var decodedLength = checked((encoded.Length / 4) * 3 - padding);
        if (decodedLength <= 0 || decodedLength > limits.MaximumDecodedTokenBytes)
        {
            throw new InvalidDataException("Tokenizer vocabulary value exceeds the canonical token byte budget.");
        }
        return decodedLength;
    }

    private static long AddDecodedVocabularyBytes(
        long currentBytes,
        int nextTokenBytes,
        long maximumBytes)
    {
        try
        {
            var total = checked(currentBytes + nextTokenBytes);
            if (total > maximumBytes)
                throw new InvalidDataException("Tokenizer vocabulary exceeds the canonical decoded byte budget.");
            return total;
        }
        catch (OverflowException exception)
        {
            throw new InvalidDataException(
                "Tokenizer vocabulary exceeds the canonical decoded byte budget.",
                exception);
        }
    }

    private static byte[] DecodeCanonicalBase64(string? encoded, int expectedLength)
    {
        try
        {
            var bytes = Convert.FromBase64String(encoded!);
            if (bytes.Length != expectedLength
                || !StringComparer.Ordinal.Equals(Convert.ToBase64String(bytes), encoded))
                throw new InvalidDataException("Tokenizer vocabulary value is not canonical base64.");
            return bytes;
        }
        catch (FormatException exception)
        {
            throw new InvalidDataException("Tokenizer vocabulary value is invalid base64.", exception);
        }
    }

    private static (int A, int B, int Result) ReadCanonicalMerge(JsonElement mergeElement)
    {
        if (mergeElement.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("Tokenizer merge entry must be an object.");

        var names = new HashSet<string>(StringComparer.Ordinal);
        int? a = null;
        int? b = null;
        int? result = null;
        foreach (var property in mergeElement.EnumerateObject())
        {
            if (!names.Add(property.Name)
                || property.Value.ValueKind != JsonValueKind.Number
                || !property.Value.TryGetInt32(out var value))
            {
                throw new InvalidDataException("Tokenizer merge entry is malformed.");
            }

            switch (property.Name)
            {
                case "A": a = value; break;
                case "B": b = value; break;
                case "Result": result = value; break;
                default: throw new InvalidDataException("Tokenizer merge entry contains an unknown property.");
            }
        }

        if (names.Count != 3 || a is null || b is null || result is null)
            throw new InvalidDataException("Tokenizer merge entry is incomplete.");
        return (a.Value, b.Value, result.Value);
    }

    private static bool ConcatenatesTo(byte[] first, byte[] second, byte[] result)
    {
        if (result.Length != checked(first.Length + second.Length))
            return false;
        return result.AsSpan(0, first.Length).SequenceEqual(first)
            && result.AsSpan(first.Length).SequenceEqual(second);
    }

    /// <summary>
    /// JsonDocument has no cancellation-token overload. Wrapping its bounded
    /// source keeps cancellation observable between parser reads without taking
    /// ownership of the caller's retained stream.
    /// </summary>
    private sealed class CancellationAwareReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly CancellationToken _cancellationToken;

        internal CancellationAwareReadStream(Stream inner, CancellationToken cancellationToken)
        {
            _inner = inner;
            _cancellationToken = cancellationToken;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            return _inner.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            return _inner.Read(buffer);
        }

        public override int ReadByte()
        {
            _cancellationToken.ThrowIfCancellationRequested();
            return _inner.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
            => _inner.Seek(offset, origin);

        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            // The caller owns the retained stream and closes it separately.
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private string SerializeCanonicalState()
    {
        var model = new SavedModel
        {
            Merges = _merges
                .Select(kv => new MergeEntry
                {
                    A = kv.Key.Item1,
                    B = kv.Key.Item2,
                    Result = kv.Value
                })
                .OrderBy(entry => entry.Result)
                .ThenBy(entry => entry.A)
                .ThenBy(entry => entry.B)
                .ToList(),
            Vocab = _vocab
                .OrderBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => Convert.ToBase64String(kv.Value))
        };
        return JsonSerializer.Serialize(model);
    }

    // FIX-BW: every adjacent position of a representative is still visited,
    // overlapping pairs included; the representative's weight is added instead
    // of one, which is the same total the repeated chunks used to produce.
    private static Dictionary<(int, int), int> CountPairs(List<WeightedChunk> chunks)
    {
        var counts = new Dictionary<(int, int), int>();
        foreach (var chunk in chunks)
        {
            var ids = chunk.TokenIds;
            for (int i = 0; i < ids.Count - 1; i++)
            {
                var pair = (ids[i], ids[i + 1]);
                counts[pair] = counts.GetValueOrDefault(pair) + chunk.Multiplicity;
            }
        }
        return counts;
    }

    // FIX-BW: the unchanged merge runs once per representative instead of once
    // per repetition. Weight and order are kept; representatives are never
    // re-joined by their merged contents.
    private static void MergeAll(List<WeightedChunk> chunks, (int, int) pair, int newToken)
    {
        foreach (var chunk in chunks)
            chunk.TokenIds = MergeChunk(chunk.TokenIds, pair, newToken);
    }

    // FIX-BPE transfer, plan 12-bpe-no-match-copy.md, from BPE-NO-MATCH-COPY-01.
    //
    // While the vocabulary is being learned, the pair chosen for this round
    // occurs in only a small share of the chunks. Every chunk was nevertheless
    // given a fresh list and copied through it element by element, including
    // the great majority where nothing was merged at all.
    //
    // Ownership boundary, stated because it is the one thing this changes for
    // callers: when the pair does not occur, the SAME list is returned, not a
    // copy. A caller must therefore treat the result as read-only, or take
    // ownership of a list it built itself. Both current callers do: training
    // reassigns the chunk list each round and only counts pairs afterwards,
    // and encoding reassigns and only reads the result.
    private static List<int> MergeChunk(List<int> ids, (int, int) pair, int newToken)
    {
        // Find the first place the pair occurs. Until one is found nothing has
        // to be built, and in most chunks none ever is.
        int first = -1;
        for (int scan = 0; scan < ids.Count - 1; scan++)
        {
            if (ids[scan] == pair.Item1 && ids[scan + 1] == pair.Item2)
            {
                first = scan;
                break;
            }
        }

        if (first < 0)
            return ids;

        var output = new List<int>(ids.Count);
        int i = 0;
        while (i < ids.Count)
        {
            if (i < ids.Count - 1 && ids[i] == pair.Item1 && ids[i + 1] == pair.Item2)
            {
                output.Add(newToken);
                i += 2;
            }
            else
            {
                output.Add(ids[i]);
                i++;
            }
        }
        return output;
    }

    private List<int> ApplyMerges(List<int> ids)
    {
        // Apply merges in the order they were learned (insertion order of _merges)
        foreach (var (pair, token) in _merges)
            ids = MergeChunk(ids, pair, token);
        return ids;
    }

    // ── Serialisation helpers ─────────────────────────────────────────────────

    private class SavedModel
    {
        public List<MergeEntry> Merges { get; set; } = [];
        public Dictionary<int, string> Vocab { get; set; } = [];
    }

    private class MergeEntry
    {
        public int A { get; set; }
        public int B { get; set; }
        public int Result { get; set; }
    }
}

/// <summary>
/// Fixed parser allocation contract.  The internal form lets tests exercise
/// the exact pre-allocation guards with tiny inputs while production always
/// uses <see cref="Production"/>.
/// </summary>
internal readonly record struct CanonicalTokenizerArtifactLimits(
    long MaximumArtifactBytes,
    int MaximumEncodedTokenCharacters,
    int MaximumDecodedTokenBytes,
    long MaximumDecodedVocabularyBytes)
{
    internal static CanonicalTokenizerArtifactLimits Production { get; } = new(
        BpeTokenizer.MaximumCanonicalArtifactBytes,
        BpeTokenizer.MaximumCanonicalEncodedTokenCharacters,
        BpeTokenizer.MaximumCanonicalDecodedTokenBytes,
        BpeTokenizer.MaximumCanonicalDecodedVocabularyBytes);

    internal void Validate()
    {
        if (MaximumArtifactBytes <= 0
            || MaximumEncodedTokenCharacters <= 0
            || MaximumDecodedTokenBytes <= 0
            || MaximumDecodedVocabularyBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumArtifactBytes));
        }

        var encodedUpperBound = checked((MaximumEncodedTokenCharacters / 4) * 3);
        if (MaximumDecodedTokenBytes > encodedUpperBound
            || MaximumDecodedVocabularyBytes < MaximumDecodedTokenBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumDecodedTokenBytes));
        }
    }
}
