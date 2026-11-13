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

/// <summary>
/// Byte-Pair Encoding tokeniser.
/// Train: reads raw text, iteratively merges the most frequent adjacent byte-pair
/// until the vocabulary reaches the target size.
/// Encode/Decode: apply learned merges to map text ↔ integer tokens.
/// </summary>
public class BpeTokenizer
{
    // EXPERIMENT ONLY. A uses the untouched native body below the branch.
    public static bool ExperimentElideUnchangedChunkCopies { get; set; }
    public static long ExperimentNoMatchReuses { get; private set; }
    public static long ExperimentChangedChunkCopies { get; private set; }

    private static List<int> MergeChunkWithoutNoMatchCopy(List<int> ids, (int, int) pair, int newToken)
    {
        List<int>? output = null;
        int i = 0;
        while (i < ids.Count)
        {
            if (i < ids.Count - 1 && ids[i] == pair.Item1 && ids[i + 1] == pair.Item2)
            {
                if (output is null)
                {
                    output = new List<int>(ids.Count);
                    for (int prefix = 0; prefix < i; prefix++) output.Add(ids[prefix]);
                }
                output.Add(newToken);
                i += 2;
            }
            else
            {
                output?.Add(ids[i]);
                i++;
            }
        }
        if (output is null)
        {
            ExperimentNoMatchReuses++;
            return ids;
        }
        ExperimentChangedChunkCopies++;
        return output;
    }

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
    public int EosToken  => _vocab.Count - 1;   // last token in vocab is treated as EOS

    // ── Training ──────────────────────────────────────────────────────────────

    public void Train(string text, int vocabSize)
    {
        if (vocabSize < 256)
            throw new ArgumentOutOfRangeException(nameof(vocabSize), "Must be ≥ 256");

        // Initialise vocab with all 256 single bytes (tokens 0–255)
        _vocab = Enumerable.Range(0, 256)
            .ToDictionary(i => i, i => new byte[] { (byte)i });
        _merges = [];

        // Split text into word chunks, then convert each chunk to byte-token ids
        var chunks = RegexSplitter.Split(text)
            .Select(w => Encoding.UTF8.GetBytes(w).Select(b => (int)b).ToList())
            .ToList();

        int nextToken = 256;
        while (_vocab.Count < vocabSize)
        {
            var pairCounts = CountPairs(chunks);
            if (pairCounts.Count == 0) break;

            var best = pairCounts.MaxBy(kv => kv.Value).Key;
            _merges[best] = nextToken;
            _vocab[nextToken] = [.. _vocab[best.Item1], .. _vocab[best.Item2]];
            chunks = MergeAll(chunks, best, nextToken);
            nextToken++;
        }
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
    {
        var bytes = tokens.SelectMany(t =>
            _vocab.TryGetValue(t, out var b) ? b : Array.Empty<byte>()).ToArray();
        return Encoding.UTF8.GetString(bytes);
    }

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

    private static Dictionary<(int, int), int> CountPairs(List<List<int>> chunks)
    {
        var counts = new Dictionary<(int, int), int>();
        foreach (var chunk in chunks)
        {
            for (int i = 0; i < chunk.Count - 1; i++)
            {
                var pair = (chunk[i], chunk[i + 1]);
                counts[pair] = counts.GetValueOrDefault(pair) + 1;
            }
        }
        return counts;
    }

    private static List<List<int>> MergeAll(List<List<int>> chunks, (int, int) pair, int newToken)
    {
        var result = new List<List<int>>(chunks.Count);
        foreach (var chunk in chunks)
            result.Add(MergeChunk(chunk, pair, newToken));
        return result;
    }

    private static List<int> MergeChunk(List<int> ids, (int, int) pair, int newToken)
    {
        if (ExperimentElideUnchangedChunkCopies)
            return MergeChunkWithoutNoMatchCopy(ids, pair, newToken);
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
