# Compression Algorithms — Sub-Projects 24 through 27
## Source document: compression algorithm families reference
## Solution: LlmFromScratch.sln — Implementation Plan v1.0

**Project:** llm-mvp-core  
**Language:** C# / .NET 8  
**External packages:** none — all algorithms implemented from scratch following reference C sources  
**Document purpose:** Step-by-step implementation plan for four compression algorithm families as independent sub-projects within the same solution  
**This document does not touch XIIGen work.**

---

## What this document adds to the solution

Four sub-projects covering the complete landscape of lossless data compression:

| Number | Family name | Algorithms inside |
|---|---|---|
| 24 | Lempel-Ziv dictionary family | Snappy, LZAV, LZ4, Zstandard |
| 25 | Entropy coding family | Huffman, DEFLATE, rANS |
| 26 | Block sorting family (Burrows-Wheeler) | BWT pipeline, bzip2-style compressor |
| 27 | Specialized compressors | Brotli, LZMA, OpenZL |

**Why compression matters to this solution:** sub-projects 02, 04, 07, 09 all store large binary files (token shards, model checkpoints, graph snapshots, knowledge indexes). Sub-project 24 alone can reduce checkpoint file sizes by 2–4× with negligible CPU cost. Sub-projects 25–27 add progressively better ratios at the cost of speed.

**Recommended reading order before writing any code** (from reference document):
1. Snappy first — the simplest LZ implementation (~500 reference lines)
2. `ryg_rans` second — teaches entropy coding in 100 lines
3. DEFLATE (zlib `trees.c`) third — combines both families

---

## Part 1 — Coding Rules (Genie DNA) — All Nine Apply

All nine coding rules from the main implementation plan (v2.0) apply to all four sub-projects without exception. The reminders specific to compression:

- Coding rule 3: every compression and decompression method returns `OperationResult<T>` — a failed compression (input too short, corrupted header) returns a failure result, never throws
- Coding rule 7: any compression operation that writes to persistent storage accepts an `operationId` — if the same file is submitted twice, the compressed output is returned from cache without recompressing
- Coding rule 8: compressed bytes are saved to `ICompressionStorage` before any "compression complete" event is dispatched
- Coding rule 4: every compressor service extends `BaseService`

---

## Part 2 — Global Abbreviation Table for Compression Domain

All abbreviations used across sub-projects 24–27:

| Short form | Full name | Plain-language explanation |
|---|---|---|
| LZ | Lempel-Ziv algorithm (алгоритм Лемпеля-Зива) | named after its inventors Abraham Lempel and Jacob Ziv; finds repeated sequences in data and replaces them with shorter references |
| LZ77 | Lempel-Ziv 1977 (алгоритм Лемпеля-Зива 1977 года) | the original 1977 paper algorithm: slide a window over data, when you see a sequence you saw before, write a (distance, length) pair instead |
| LZ4 | Lempel-Ziv 4 (четвёртая реализация алгоритма) | a very fast variant of LZ77 — trades compression ratio for extreme speed |
| LZAV | Lempel-Ziv AVANEEV variant (вариант Аванеева) | a 2024 single-file C implementation that achieves better ratio than LZ4 in less code |
| BWT | Burrows-Wheeler Transform (преобразование Берроуза-Уилера) | rearranges the bytes of a block so that identical bytes cluster together, making them easier to compress |
| MTF | Move-To-Front transform (преобразование "переместить в начало") | after BWT, frequently seen bytes are assigned small numbers; common bytes produce runs of small numbers |
| RLE | Run-Length Encoding (кодирование длин серий) | replaces sequences of identical bytes (e.g. AAAA) with a count and one example (4, A) |
| DEFLATE | DEFLATE compression format (формат сжатия DEFLATE) | the algorithm used inside ZIP files, PNG images, and HTTP gzip: combines LZ77 for pattern matching with Huffman coding for entropy compression |
| ANS | Asymmetric Numeral Systems (асимметричные числовые системы) | a family of entropy coders invented by Jarek Duda that can reach near-theoretical compression limits |
| rANS | range Asymmetric Numeral Systems (диапазонный вариант ANS) | the most practical ANS variant: processes symbols one at a time using arithmetic on a single integer state |
| tANS | table Asymmetric Numeral Systems (табличный вариант ANS) | another ANS variant: uses a pre-built lookup table for fast encoding and decoding |
| FSE | Finite State Entropy (энтропия конечных состояний) | Facebook's name for their tANS implementation, used inside Zstandard |
| LZMA | Lempel-Ziv-Markov chain Algorithm (алгоритм Лемпеля-Зива с цепями Маркова) | the algorithm inside 7-Zip: combines LZ pattern matching with Markov chain probability models for the highest compression ratios |
| Markov chain | Markov chain probability model (цепи Маркова) | a model where the probability of the next symbol depends on the previous few symbols; LZMA uses this to predict what byte comes next |
| Range coding | Range coding entropy coder (диапазонный кодировщик) | an entropy coder that represents data as a narrowing interval of real numbers; LZMA uses it instead of Huffman |
| Huffman coding | Huffman entropy coding (коды Хаффмана) | assigns short bit sequences to frequent symbols and long ones to rare symbols; builds a tree where each leaf is a symbol |
| Entropy coding | Entropy coding (энтропийное кодирование) | any method that assigns variable-length bit sequences to symbols based on their frequency; the theoretical limit is Shannon entropy |
| Shannon entropy | Shannon entropy (энтропия Шеннона) | the theoretical minimum number of bits needed to represent a data source; no compressor can do better without additional structure |
| Sliding window | Sliding window (скользящее окно) | a fixed-size buffer of previously seen bytes that the compressor searches for matches; in LZ4 the window is 64 KB |
| Match length | Match length (длина совпадения) | when the compressor finds a previous occurrence of the current sequence, this is how many bytes match |
| Back-reference | Back-reference (обратная ссылка) | a (distance, length) pair that says "copy N bytes from M positions back in the output buffer" |
| Literal byte | Literal byte (буквальный байт) | a byte that could not be matched to anything previous and is stored as-is |
| Context model | Context model (контекстная модель) | a data structure that predicts the probability of the next byte based on the bytes already seen |
| Zstandard | Zstandard compression algorithm (алгоритм сжатия Zstandard, сокращённо zstd) | Facebook's 2016 compressor: combines LZ77 matching with FSE entropy coding; the best balance of speed and compression ratio |
| Dictionary training | Dictionary training (обучение словаря) | for small files, Zstandard can learn a "dictionary" from many similar files and use it to compress each one better |
| Brotli | Brotli compression algorithm (алгоритм сжатия Brotli) | Google's web compressor: LZ77 + Huffman + second-order context modeling; better than gzip for HTML, CSS, JavaScript |
| Second-order context | Second-order context model (контекстная модель второго порядка) | predicts the next byte based on the two preceding bytes; Brotli uses this for better text compression |
| OpenZL | OpenZL structured data compressor (компрессор для структурированных данных) | Meta's 2025 research compressor: learns the format of structured data and applies a chain of transformations before entropy coding |
| Compression ratio | Compression ratio (степень сжатия) | how many times smaller the compressed output is vs the original; ratio 3.0 means 1 MB compressed to ~333 KB |
| Throughput | Compression throughput (пропускная способность при сжатии) | how many megabytes per second the compressor processes |

---

## Part 3 — Shared Fabric Interfaces (defined once, used by all four sub-projects)

All four sub-projects live under one project root:

```
25.Compression/
├── Fabric/
│   ├── ICompressor.cs           — compress bytes → smaller bytes
│   ├── IDecompressor.cs         — restore original bytes from compressed
│   ├── ICompressionStorage.cs   — save and load compressed files
│   └── ICompressionBenchmark.cs — measure speed and ratio
├── Core/
│   ├── CompressionOptions.cs    — configurable parameters per algorithm
│   ├── CompressionResult.cs     — compressed bytes + metadata
│   └── BenchmarkResult.cs       — timing, ratio, throughput
├── Families/
│   ├── 01-LzFamily/             — sub-project 24
│   ├── 02-EntropyCoding/        — sub-project 25
│   ├── 03-BwtFamily/            — sub-project 26
│   └── 04-Specialized/          — sub-project 27
└── Tests/
```

```csharp
// ICompressor is the single interface that every compression algorithm implements.
// The caller does not need to know whether it is talking to Snappy, LZ4, or LZMA —
// they all offer the same two-method contract.
public interface ICompressor
{
    // Compress the input bytes and return the smaller compressed representation
    // input — the raw bytes to compress (any content: code, text, binary data)
    // options — optional parameters such as compression level or dictionary path
    // Returns the compressed bytes on success, or a failure result if compression is impossible
    //   (e.g. input is already compressed and cannot be made smaller)
    Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input,
        CompressionOptions options);

    // Decompress previously compressed bytes back to their original form
    // compressed — the bytes produced by a previous call to CompressAsync
    // expectedOriginalLength — the number of bytes to expect in the output
    //   (stored in the compressed stream header; used to pre-allocate output buffer)
    Task<OperationResult<byte[]>> DecompressAsync(
        byte[] compressed,
        int expectedOriginalLength);
}

// ICompressionStorage hides where compressed files are saved.
// A compressor service never writes files directly — it uses this interface.
// Concrete implementations: InMemoryCompressionStorage (tests), DiskCompressionStorage (production)
public interface ICompressionStorage
{
    // Save a compressed block under a given name for later retrieval
    Task<OperationResult<bool>> SaveAsync(string name, CompressionResult compressed);

    // Load a previously saved compressed block by name
    Task<OperationResult<CompressionResult>> LoadAsync(string name);
}

// ICompressionBenchmark runs a timed comparison of multiple compressors on the same input
// and returns a structured result showing speed and compression ratio for each.
public interface ICompressionBenchmark
{
    // Run all registered compressors on the test data and return metrics
    // testData — the bytes to compress; should be representative of production data
    Task<OperationResult<List<BenchmarkResult>>> RunAllAsync(byte[] testData);
}

// CompressionOptions carries algorithm-specific tuning parameters.
// Most callers leave all options at their defaults.
public record CompressionOptions
{
    // The compression level — higher means better ratio but slower speed
    // The exact meaning of each level varies per algorithm; 1 is always fastest
    public int CompressionLevel { get; init; } = 1;

    // Path to a pre-trained dictionary file (used only by Zstandard and OpenZL)
    // When null, the algorithm compresses without a dictionary
    public string? DictionaryPath { get; init; }

    // Maximum memory the compressor may use (in megabytes)
    // Useful when running in a constrained environment like a mobile device
    public int MaxMemoryMegabytes { get; init; } = 64;
}

// CompressionResult wraps the compressed bytes with the metadata needed to decompress them.
public record CompressionResult
{
    // The compressed bytes — smaller than the original (usually)
    public byte[] CompressedBytes { get; init; } = Array.Empty<byte>();

    // The length of the original uncompressed data in bytes
    // Required for decompression to know how large the output buffer should be
    public int OriginalLength { get; init; }

    // The name of the algorithm that produced this result (e.g. "Snappy", "LZ4")
    public string AlgorithmName { get; init; } = string.Empty;

    // The compression ratio: original size divided by compressed size
    // A ratio of 3.5 means the original was 3.5 times larger than the compressed output
    public double CompressionRatio => OriginalLength / (double)Math.Max(1, CompressedBytes.Length);
}
```

---

## Sub-Project 24 — Lempel-Ziv Dictionary Family

**What this family does in plain language:** all four algorithms in this family work the same way. They slide a window over the input data. When they find a sequence of bytes that appeared somewhere in the window before, they write a back-reference (how far back, how many bytes to copy) instead of the bytes themselves. When no previous match exists, they write the literal byte unchanged. The difference between the algorithms is how large the window is, how they find matches, and what format they use to encode the back-references.

**Algorithms in this sub-project:**
1. Snappy — the simplest implementation (~500 lines in C); read first
2. LZAV — single C header file; better ratio than LZ4 with less code
3. LZ4 — fastest general-purpose compressor (500+ MB/s compression, 2+ GB/s decompression)
4. Zstandard (зопращённо zstd — стандарт сжатия от Facebook) — best speed/ratio balance, plus dictionary training for small files

**Reference sources:**
- Snappy: `https://github.com/google/snappy` — `snappy.cc` (~500 lines)
- LZAV: `https://github.com/avaneev/lzav` — `lzav.h` (single file, MIT)
- LZ4: `https://github.com/lz4/lz4` — `lib/lz4.c`
- Zstandard: `https://github.com/facebook/zstd` — `lib/compress/zstd_compress.c`

**Dependencies within the solution:** none. Sub-project 24 is self-contained.

**Folder:** `25.Compression/Families/01-LzFamily/`

---

### Phase 0 — Shared LZ primitives (1 day)

Before any specific algorithm, define the data structures used by all four algorithms.

What to write: `LzMatchFinder.cs`, `LzBackReference.cs`

```csharp
// LzBackReference describes one match: "copy N bytes starting M positions back in the output"
// Every Lempel-Ziv algorithm produces a stream of these records mixed with literal bytes.
public record LzBackReference
{
    // How many positions back in the already-written output to start the copy
    // For example, distance=5 means "start copying from 5 bytes before the current position"
    public int Distance { get; init; }

    // How many bytes to copy from the back-reference position
    // Length can be longer than the distance — this encodes a repeating sequence
    // Example: distance=3, length=9 repeats the last 3 bytes three times
    public int Length { get; init; }
}

// LzToken is one entry in the compressed stream: either a back-reference or a literal byte
// Every Lempel-Ziv compressor produces a sequence of these tokens
public readonly struct LzToken
{
    // Whether this token is a literal byte (true) or a back-reference (false)
    public bool IsLiteral { get; init; }

    // The raw byte value — valid only when IsLiteral is true
    public byte LiteralByte { get; init; }

    // The back-reference location and length — valid only when IsLiteral is false
    public LzBackReference? BackReference { get; init; }
}
```

`LzMatchFinder` scans a byte array for the longest match at a given position:

```csharp
// LzMatchFinder searches backwards through a sliding window to find the longest
// sequence of bytes that matches the bytes starting at the current position.
// This is the innermost loop of every Lempel-Ziv compressor —
// the faster it runs, the faster the whole compressor runs.
public static class LzMatchFinder
{
    // Find the best back-reference starting at the given position in the input
    // input — the full input byte array
    // currentPosition — the position we are trying to compress
    // windowSize — how far back we are allowed to search for matches (the sliding window)
    // minimumMatchLength — matches shorter than this are not worth recording (too many bits)
    // Returns null if no useful match was found (the current byte should be a literal)
    public static LzBackReference? FindBestMatch(
        ReadOnlySpan<byte> input,
        int currentPosition,
        int windowSize,
        int minimumMatchLength)
    {
        // The furthest back we are allowed to look is limited by the window size
        // and by not going before the start of the array
        int searchStart = Math.Max(0, currentPosition - windowSize);

        // Track the best match found so far
        int bestDistance = 0;
        int bestLength = 0;

        // Search backwards from the current position through the window
        for (int candidate = currentPosition - 1; candidate >= searchStart; candidate--)
        {
            // Count how many consecutive bytes match starting at candidate vs currentPosition
            int matchLength = 0;
            int maxMatchLength = input.Length - currentPosition;

            while (matchLength < maxMatchLength
                   && input[candidate + matchLength] == input[currentPosition + matchLength])
            {
                // This byte matches — extend the match by one more byte
                matchLength++;
            }

            // If this match is longer than the best we have seen, record it
            if (matchLength > bestLength)
            {
                bestLength = matchLength;
                bestDistance = currentPosition - candidate;

                // If we found a match equal to the maximum possible, stop early
                if (bestLength == maxMatchLength) break;
            }
        }

        // Only return a match if it is long enough to be worth the back-reference overhead
        if (bestLength >= minimumMatchLength)
        {
            return new LzBackReference { Distance = bestDistance, Length = bestLength };
        }

        // No useful match found — the caller should emit a literal byte
        return null;
    }
}
```

Tests: find a match in a string with a known repetition; verify distance and length are correct; verify null is returned when no repetition exists.

Phase exit: `dotnet build` exits 0.

---

### Phase 1 — Snappy (simplest Lempel-Ziv — 2 days)

**What Snappy does:** uses a 16-entry hash table per 64 KB block to find 4-byte matches quickly. No compression levels — always uses the same fast path. The format is: a varint (variable-length integer encoding — кодирование целых чисел переменной длиной для экономии байт) holding the original length, followed by a sequence of tagged entries (literal run or copy).

**Reference:** `https://github.com/google/snappy` — `snappy.cc`, ~500 lines. Read the entire file before writing any C# code.

What to write: `SnappyCompressor.cs`

```csharp
// SnappyCompressor implements Google's Snappy compression algorithm.
// Snappy trades compression ratio for extreme speed — it is not the smallest
// but it is one of the fastest LZ-family compressors.
// Reference: https://github.com/google/snappy (BSD 3-Clause, C++)
public class SnappyCompressor : BaseService, ICompressor
{
    // The size of the hash table used to find matches quickly
    // A larger table finds more matches but uses more memory
    // Snappy uses a fixed 16 KB hash table (16,384 entries)
    private const int HashTableSize = 1 << 14; // 16,384 entries

    // The minimum number of bytes that must match to be worth a back-reference
    // A match of 1 or 2 bytes costs more to encode as a back-reference than as literals
    private const int MinimumMatchLength = 4;

    // The maximum distance to search backwards for a match
    // Snappy limits this to fit the distance in a small number of bits
    private const int MaximumMatchDistance = 32768; // 32 KB

    public async Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input,
        CompressionOptions options)
    {
        // Guard: Snappy requires at least 1 byte of input
        if (input.Length == 0)
        {
            return OperationResult<CompressionResult>.Fail(
                "Cannot compress empty input — Snappy requires at least one byte.",
                "EMPTY_INPUT");
        }

        // The output buffer — in the worst case (incompressible data), output is
        // slightly larger than input due to the format overhead
        var output = new byte[input.Length + (input.Length / 6) + 32];
        int outputPosition = 0;

        // Write the original (uncompressed) length as a varint at the start of the stream
        // The decompressor uses this to know how large the output buffer must be
        outputPosition += WriteVarint(output, outputPosition, input.Length);

        // Walk through the input, building the hash table and finding matches
        var hashTable = new int[HashTableSize];

        int inputPosition = 0;
        int literalStart = 0; // where the current run of literal bytes started

        while (inputPosition < input.Length)
        {
            // Compute a hash of the 4 bytes starting at the current position
            // The hash maps those 4 bytes to a slot in the hash table
            int hash = Hash4Bytes(input, inputPosition);

            // Look up whether we have seen these 4 bytes before
            int candidatePosition = hashTable[hash];

            // Update the hash table to point to the current position
            // (overwrite whatever was there before — Snappy uses a simple non-chaining table)
            hashTable[hash] = inputPosition;

            // Check if the candidate position actually has the same 4 bytes
            // (a hash collision means the bytes might not actually match)
            bool matchFound = candidatePosition >= 0
                && inputPosition - candidatePosition < MaximumMatchDistance
                && input[candidatePosition] == input[inputPosition]
                && input[candidatePosition + 1] == input[inputPosition + 1]
                && input[candidatePosition + 2] == input[inputPosition + 2]
                && input[candidatePosition + 3] == input[inputPosition + 3];

            if (!matchFound)
            {
                // No match — advance by one byte and continue accumulating literals
                inputPosition++;
                continue;
            }

            // Found a match! First, flush any accumulated literal bytes
            if (inputPosition > literalStart)
            {
                outputPosition += EmitLiterals(
                    output, outputPosition,
                    input, literalStart, inputPosition - literalStart);
            }

            // Extend the match as far as possible beyond the initial 4-byte match
            int matchLength = 4;
            while (inputPosition + matchLength < input.Length
                   && input[candidatePosition + matchLength] == input[inputPosition + matchLength])
            {
                matchLength++;
            }

            // Emit the back-reference: (distance back, number of bytes to copy)
            int distance = inputPosition - candidatePosition;
            outputPosition += EmitBackReference(output, outputPosition, distance, matchLength);

            // Advance past the matched bytes
            inputPosition += matchLength;
            literalStart = inputPosition;
        }

        // Flush any remaining literal bytes at the end of the input
        if (literalStart < input.Length)
        {
            outputPosition += EmitLiterals(
                output, outputPosition,
                input, literalStart, input.Length - literalStart);
        }

        // Return only the bytes actually written — not the full pre-allocated buffer
        var result = new CompressionResult
        {
            CompressedBytes = output[..outputPosition],
            OriginalLength = input.Length,
            AlgorithmName = "Snappy"
        };

        return OperationResult<CompressionResult>.Ok(result);
    }

    // Hash 4 bytes starting at the given position to a table index
    // This is a simple multiplicative hash — fast and good enough for Snappy
    private static int Hash4Bytes(byte[] data, int position)
    {
        // Combine 4 bytes into one 32-bit integer using bit shifts
        uint fourBytes = (uint)(data[position]
                               | (data[position + 1] << 8)
                               | (data[position + 2] << 16)
                               | (data[position + 3] << 24));

        // Multiply by a large prime and shift right to get a table index
        // This spreads values evenly across the HashTableSize entries
        return (int)((fourBytes * 0x1e35a7bd) >> 18) & (HashTableSize - 1);
    }

    // ... EmitLiterals, EmitBackReference, WriteVarint, DecompressAsync ...
    public Task<OperationResult<byte[]>> DecompressAsync(byte[] compressed, int expectedOriginalLength)
        => throw new NotImplementedException("Implement in Phase 1 completion");
}
```

Gate B check: compress the string `"abcdefabcdefabcdef"` (repeats the 6-char sequence 3 times). Verify the compressed output is smaller than the input. Verify that decompressing produces the original string exactly.

Tests:
- Round-trip: compress any string, decompress, verify exact match
- Verify incompressible data (random bytes) does not crash — output may be slightly larger
- Verify empty input returns failure result, not exception

**Estimated: 2 days**

---

### Phase 2 — LZAV single-file compressor (1.5 days)

**What LZAV does:** a 2024 C header-only compressor that achieves better compression ratio than LZ4 while being faster to compress. The entire algorithm is in one `.h` file (~800 lines). Uses a 4-byte hash with chaining (chains multiple positions with the same hash, unlike Snappy's single entry) to find longer matches.

**Reference:** `https://github.com/avaneev/lzav` — `lzav.h` — MIT license, read the full file.

What to write: `LzavCompressor.cs`

Key differences from Snappy:
- Hash table uses chaining (each slot holds a linked list of positions) — finds longer matches
- Separate "fast" and "high compression" modes (different chain lengths)
- Header format is slightly different (uses separate length and offset byte sequences)

Tests:
- Same round-trip test as Snappy
- Compare compression ratio on a 100 KB block of C# source code: LZAV should produce smaller output than Snappy
- Verify the exact compressed size matches the reference C implementation (within 1% due to acceptable algorithmic variation)

**Estimated: 1.5 days**

---

### Phase 3 — LZ4 compressor (2 days)

**What LZ4 does:** designed around one principle — the decompressor must be able to run at memory bandwidth speed (the bottleneck is RAM, not CPU). The format is aggressively simple: each sequence is a token byte followed by optional extra length bytes, followed by literal bytes, followed by a 2-byte offset and optional match length. The decompressor is ~150 lines of straight-line code.

**Reference:** `https://github.com/lz4/lz4` — `lib/lz4.c` and `lib/lz4.h` — BSD 2-Clause.

What to write: `Lz4Compressor.cs`, `Lz4HcCompressor.cs`

`Lz4HcCompressor` (High Compression variant — вариант с лучшим сжатием, но меньшей скоростью) uses a deeper match search (2-level hash chain) for better ratio at the cost of compression speed.

Token format (one token byte encodes both the literal length and the match length):
```csharp
// In LZ4 format, each compressed sequence starts with one token byte.
// The high 4 bits encode the number of literal bytes that follow.
// The low 4 bits encode the match length minus the minimum match length.
// If either field is 15 (the maximum for 4 bits), extra bytes follow.
private static byte MakeTokenByte(int literalCount, int matchLength)
{
    // Clamp each field to 15 — anything larger requires extra bytes after the token
    int literalNibble = Math.Min(15, literalCount);
    int matchNibble = Math.Min(15, matchLength - MinimumMatchLength);

    // Pack both nibbles into one byte (high 4 bits = literals, low 4 bits = match)
    return (byte)((literalNibble << 4) | matchNibble);
}
```

Tests:
- Round-trip correctness
- Throughput benchmark: compressing 10 MB of C# source code should complete in under 100ms
- Compare ratio vs Snappy and LZAV on the same test corpus

**Estimated: 2 days**

---

### Phase 4 — Zstandard with dictionary training (2.5 days)

**What Zstandard does:** combines LZ77 match finding with Finite State Entropy (FSE) coding (a tANS variant) for entropy compression of both the literal bytes and the back-reference offsets. The result beats gzip in ratio while being faster to decompress. Additionally, Zstandard can train a shared dictionary from a collection of similar small files — this dramatically improves compression of small files (under 1 KB) that normally cannot benefit from LZ matching because there is no prior data to reference.

**Reference:** `https://github.com/facebook/zstd` — `lib/compress/zstd_compress.c` — BSD license.

This is the most complex algorithm in this sub-project. Implement it in two parts:

**Part A — basic Zstandard (without dictionary):** implements the frame format, the block format, the sequence format (literal + match pairs), and the Huffman compression of literal bytes.

**Part B — dictionary training:** reads a collection of sample files, extracts the most frequently occurring byte sequences, and writes them into a dictionary file. Future compressions load this dictionary and use it as a synthetic "prior window" even when compressing the first byte of a file.

```csharp
// ZstandardDictionaryTrainer learns a shared dictionary from a collection of sample files.
// After training, any file compressed with this dictionary can reference patterns from it
// as if those patterns appeared at the start of the file — dramatically improving
// compression of small files that are too short to benefit from LZ matching alone.
public class ZstandardDictionaryTrainer : BaseService
{
    // Train a dictionary from a set of sample files and save it to disk
    // sampleFiles — paths to representative files of the type to be compressed
    //   (e.g. many small JSON files, or many short C# source files)
    // outputDictionaryPath — where to write the trained dictionary file
    // targetDictionarySize — how many bytes the dictionary should occupy
    //   (larger dictionaries cover more patterns but use more memory at compression time)
    public async Task<OperationResult<bool>> TrainAsync(
        List<string> sampleFiles,
        string outputDictionaryPath,
        int targetDictionarySize = 102400) // 100 KB default
    {
        // Load all sample file contents into memory
        var samples = new List<byte[]>();
        foreach (var path in sampleFiles)
        {
            var content = await File.ReadAllBytesAsync(path);
            samples.Add(content);
        }

        // Collect all byte n-grams (sequences of N consecutive bytes) from all samples
        // Count how often each n-gram appears across the samples
        var ngramFrequencies = CountNgramFrequencies(samples, ngramLength: 8);

        // Select the most frequent n-grams up to the target dictionary size
        var dictionaryBytes = SelectTopNgrams(ngramFrequencies, targetDictionarySize);

        // Write the dictionary to disk with a standard header identifying it as a Zstandard dictionary
        await WriteDictionaryFileAsync(outputDictionaryPath, dictionaryBytes);

        return OperationResult<bool>.Ok(true);
    }
}
```

Tests:
- Round-trip without dictionary
- Train a dictionary on 50 JSON files; compress a new JSON file with and without dictionary; verify the dictionary version is smaller
- Verify the dictionary file format can be loaded back and produces the same compression output

**Estimated: 2.5 days**

---

### Phase 5 — Acceptance criteria and summary

**Gate B for all four algorithms:** compress the same 1 MB block of C# source code with each algorithm and compare the output size to the reference C implementation (via a pre-computed table). All sizes must be within 2% of the reference.

| Algorithm | Expected compression ratio on source code | Expected compression throughput |
|---|---|---|
| Snappy | 2.0–2.5× | > 200 MB/s |
| LZAV | 2.3–2.8× | > 300 MB/s |
| LZ4 | 2.2–2.7× | > 400 MB/s |
| Zstandard (level 1) | 3.0–3.8× | > 150 MB/s |

**Total estimated days for sub-project 24: 10**

---

## Sub-Project 25 — Entropy Coding Family

**What entropy coding does in plain language:** after a dictionary compressor (like LZ4) has replaced repeated sequences with back-references, the remaining bytes are not evenly distributed — some patterns are much more common than others. Entropy coding assigns shorter bit sequences to common patterns and longer ones to rare patterns. This second pass can add another 10–40% reduction on top of the dictionary compression. DEFLATE (used in ZIP and PNG) combines LZ77 and Huffman in one format. ANS (Asymmetric Numeral Systems — асимметричные числовые системы) is the modern replacement for Huffman that achieves compression closer to the theoretical Shannon entropy limit.

**Reference sources:**
- Huffman + DEFLATE: `https://github.com/madler/zlib` — `trees.c`, `deflate.c`, `inflate.c`
- rANS: `https://github.com/rygorous/ryg_rans` — `rans_byte.h` (100 lines, public domain) — **read this first**

**Folder:** `25.Compression/Families/02-EntropyCoding/`

---

### Phase 0 — Frequency counting and symbol statistics (0.5 day)

All entropy coders start by counting how often each byte value (0–255) appears in the data. This produces the symbol frequency table that drives code assignment.

```csharp
// SymbolStatistics counts how often each possible byte value appears in a data block.
// Every entropy coder uses this table to assign shorter codes to frequent symbols.
public static class SymbolStatistics
{
    // Count how many times each byte value (0 to 255) appears in the input
    // Returns an array of 256 counts, where counts[b] = number of occurrences of byte b
    public static long[] CountFrequencies(ReadOnlySpan<byte> input)
    {
        // Allocate a counter for each possible byte value (0 through 255)
        var counts = new long[256];

        // Walk through every byte in the input and increment its counter
        foreach (byte b in input)
        {
            counts[b]++;
        }

        return counts;
    }

    // Compute the Shannon entropy (theoretical minimum bits per symbol) of a frequency distribution
    // This tells us the best possible compression ratio for this data
    // entropy close to 8.0 bits/byte = nearly random data, hard to compress
    // entropy close to 0.0 bits/byte = highly repetitive data, easy to compress
    public static double ComputeShannonEntropy(long[] frequencies)
    {
        // Total number of symbols in the data
        long totalSymbols = frequencies.Sum();
        if (totalSymbols == 0) return 0;

        double entropy = 0;
        foreach (long count in frequencies)
        {
            if (count == 0) continue;

            // Probability of this symbol
            double probability = (double)count / totalSymbols;

            // Shannon's formula: each symbol contributes -p × log2(p) bits to entropy
            entropy -= probability * Math.Log2(probability);
        }

        return entropy;
    }
}
```

Phase exit: `dotnet build` exits 0.

---

### Phase 1 — Huffman tree construction and coding (2 days)

**What Huffman coding does:** builds a binary tree where the most frequent symbol is closest to the root (fewest steps = shortest code). To decode, walk from root to leaf following the bits.

What to write: `HuffmanTree.cs`, `HuffmanCompressor.cs`

```csharp
// HuffmanTree builds and stores the Huffman code tree for a set of symbol frequencies.
// After building, each symbol has an assigned bit code (a short sequence of 0s and 1s).
// Frequent symbols get short codes; rare symbols get long codes.
// Reference: zlib trees.c — the canonical implementation
public class HuffmanTree
{
    // The bit code assigned to each symbol (indexed by byte value 0-255)
    // codes[65] = the bit pattern for the ASCII letter 'A'
    public uint[] Codes { get; private set; } = new uint[256];

    // The number of bits in each symbol's code
    // lengths[65] = how many bits are in the code for 'A'
    public int[] CodeLengths { get; private set; } = new int[256];

    // Build the optimal Huffman tree from symbol frequencies
    // frequencies — counts[b] = number of times byte value b appeared in the data
    public void Build(long[] frequencies)
    {
        // Create one leaf node per symbol that appeared at least once
        // (Symbols with count 0 are not included — they never appear in the data)
        var heap = new List<HuffmanNode>();
        for (int symbol = 0; symbol < 256; symbol++)
        {
            if (frequencies[symbol] > 0)
            {
                heap.Add(new HuffmanNode
                {
                    Symbol = symbol,
                    Frequency = frequencies[symbol],
                    IsLeaf = true
                });
            }
        }

        // Edge case: all bytes are the same value (single-symbol data)
        if (heap.Count == 1)
        {
            // Assign a single-bit code to the one symbol
            Codes[heap[0].Symbol] = 0;
            CodeLengths[heap[0].Symbol] = 1;
            return;
        }

        // Build the tree bottom-up using a min-heap (priority queue).
        // Invariant: the two least-frequent nodes are at the top of the heap.
        while (heap.Count > 1)
        {
            // Remove the two least-frequent nodes
            // These become the left and right children of a new internal node
            SortByFrequency(heap);
            var leftChild = heap[0];
            var rightChild = heap[1];
            heap.RemoveRange(0, 2);

            // Create a new internal node whose frequency is the sum of both children
            // This internal node has no symbol — it is just a structural node in the tree
            var parent = new HuffmanNode
            {
                Frequency = leftChild.Frequency + rightChild.Frequency,
                IsLeaf = false,
                Left = leftChild,
                Right = rightChild
            };

            heap.Add(parent);
        }

        // The tree is complete — assign codes by walking from root to every leaf
        // Walking left appends a 0 bit; walking right appends a 1 bit
        AssignCodes(heap[0], currentCode: 0, currentLength: 0);
    }

    // Walk the tree recursively and assign the bit code to each leaf (symbol)
    private void AssignCodes(HuffmanNode node, uint currentCode, int currentLength)
    {
        if (node.IsLeaf)
        {
            // This is a leaf — the symbol gets the bit pattern accumulated so far
            Codes[node.Symbol] = currentCode;
            CodeLengths[node.Symbol] = currentLength;
            return;
        }

        // Walk left: append a 0 bit to the current code
        if (node.Left != null)
            AssignCodes(node.Left, currentCode << 1, currentLength + 1);

        // Walk right: append a 1 bit to the current code
        if (node.Right != null)
            AssignCodes(node.Right, (currentCode << 1) | 1, currentLength + 1);
    }
}

// HuffmanNode is one node in the Huffman tree — either a leaf (one symbol) or an internal node
public class HuffmanNode
{
    // Which byte value this leaf represents (only meaningful for leaf nodes)
    public int Symbol { get; init; }

    // How many times this symbol (or the union of symbols below) appeared in the input
    public long Frequency { get; init; }

    // Whether this node is a leaf (has a symbol) or an internal junction
    public bool IsLeaf { get; init; }

    // Left and right children (null for leaf nodes)
    public HuffmanNode? Left { get; init; }
    public HuffmanNode? Right { get; init; }
}
```

Gate B check: encode the string `"aabbbbcccccccc"` and verify: symbol 'c' (most frequent) gets the shortest code, 'a' (least frequent) gets the longest. Decode the encoded bitstring and verify exact match.

Tests:
- Round-trip correctness for ASCII text
- Verify single-symbol input (all same byte) is handled without crash
- Verify compressed size is smaller than input for English text but may be larger for random bytes

**Estimated: 2 days**

---

### Phase 2 — DEFLATE (LZ77 combined with Huffman coding) (2.5 days)

**What DEFLATE is:** the format used inside ZIP files, PNG images, and HTTP gzip responses. It is LZ77 match-finding followed by Huffman coding of the resulting token stream. The Huffman trees themselves are also stored in compressed form (using another Huffman code on top).

**Reference:** zlib `deflate.c` + `trees.c` + `inflate.c`. These files together show the complete algorithm.

What to write: `DeflateCompressor.cs`

The DEFLATE format has three block types:
- Stored blocks (no compression — used when compression would make data larger)
- Fixed Huffman blocks (use a pre-defined code table from the standard)
- Dynamic Huffman blocks (include a custom Huffman table at the start of each block)

Implement Phase 2a (stored + fixed blocks) and Phase 2b (dynamic blocks) separately.

**Phase 2a — stored and fixed Huffman (1.5 days):**

```csharp
// DeflateCompressor implements the DEFLATE compression format.
// DEFLATE is used inside ZIP files (.zip), PNG images, and HTTP gzip responses.
// It combines LZ77 pattern matching with Huffman entropy coding.
// Reference: zlib deflate.c + trees.c (zlib license)
public class DeflateCompressor : BaseService, ICompressor
{
    // The pre-defined "fixed" Huffman code table from the DEFLATE standard (RFC 1951)
    // This table does not need to be transmitted — both compressor and decompressor know it.
    // Literal bytes 0–143 get 8-bit codes, 144–255 get 9-bit codes,
    // match lengths 256–279 get 7-bit codes, 280–287 get 8-bit codes.
    private static readonly int[] FixedLiteralLengths = BuildFixedLiteralLengths();

    private static int[] BuildFixedLiteralLengths()
    {
        // The DEFLATE standard specifies these code lengths by range
        var lengths = new int[288];
        for (int i = 0; i <= 143; i++) lengths[i] = 8;   // bytes 0-143: 8 bits
        for (int i = 144; i <= 255; i++) lengths[i] = 9; // bytes 144-255: 9 bits
        for (int i = 256; i <= 279; i++) lengths[i] = 7; // lengths: 7 bits
        for (int i = 280; i <= 287; i++) lengths[i] = 8; // lengths: 8 bits
        return lengths;
    }

    public async Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input, CompressionOptions options)
    {
        // ... LZ77 pass to find matches and literals ...
        // ... Huffman coding pass over the token stream ...
        // ... write the DEFLATE block format ...
        throw new NotImplementedException("Implement across Phase 2a and 2b");
    }
}
```

**Phase 2b — dynamic Huffman (1 day):** builds a custom Huffman table from the actual data and transmits it in the block header. Better compression than fixed tables.

Gate B check: compress `"Hello, World!"` and verify the output matches the DEFLATE output produced by .NET's built-in `System.IO.Compression.DeflateStream` for the same input.

Tests:
- Round-trip: compress and decompress, exact match
- Verify PNG-like data (RGBA pixels) compresses well
- Verify DEFLATE output is byte-for-byte compatible with .NET's `DeflateStream`

**Estimated: 2.5 days**

---

### Phase 3 — rANS (range Asymmetric Numeral Systems — диапазонный вариант асимметричных числовых систем) (2 days)

**What rANS does in plain language:** Huffman codes must have whole-number bit lengths (you cannot assign 2.3 bits to a symbol). rANS can assign fractional bit lengths, getting arbitrarily close to the Shannon entropy limit. The algorithm keeps one large integer state. To encode a symbol, it multiplies the state by a scaling factor derived from the symbol's frequency and adds an offset. The state grows; when it becomes too large, the encoder flushes the low bits to the output. Decoding runs in reverse.

**Reference:** `https://github.com/rygorous/ryg_rans` — `rans_byte.h` — **100 lines of C, public domain.** Read this before writing any C# code. The entire algorithm fits in one screen.

What to write: `RansCompressor.cs`

```csharp
// RansCompressor implements the rANS (range Asymmetric Numeral Systems) entropy coder.
// rANS achieves compression closer to the theoretical Shannon limit than Huffman coding
// because it is not limited to whole-number bit lengths for each symbol.
// Reference: https://github.com/rygorous/ryg_rans (rans_byte.h, public domain, ~100 lines)
public class RansCompressor : BaseService, ICompressor
{
    // The size of the ANS state in bits.
    // The state must stay within the range [LowerBound, 4 × LowerBound).
    // When encoding, we flush bits to bring the state back into range.
    private const uint LowerBound = 1u << 23; // 8,388,608 — chosen so one byte is flushed at a time

    // Build the cumulative frequency table needed for rANS
    // The table maps each symbol to its frequency and cumulative frequency sum
    // cumFreq[s] = sum of all frequencies for symbols 0 through s-1
    private static (uint[] freq, uint[] cumFreq, uint total) BuildFrequencyTable(long[] rawFrequencies)
    {
        // Normalize the raw counts to a power of 2 total (required by rANS arithmetic)
        // This is the one conceptually hard part of rANS — see ryg_rans for details
        const uint targetTotal = 1u << 16; // 65,536 — the ANS scaling factor

        var freq = new uint[256];
        var cumFreq = new uint[257]; // one extra entry to hold the total

        // ... normalization code: scale counts so they sum to exactly targetTotal ...

        return (freq, cumFreq, targetTotal);
    }

    // Encode one symbol into the ANS state
    // state — the current ANS state (a large integer that encodes all symbols seen so far)
    // symbol — the byte to encode (0-255)
    // freq — this symbol's normalized frequency (from the frequency table)
    // cumFreq — the cumulative frequency sum of all symbols before this one
    // total — the sum of all frequencies (must equal targetTotal)
    // Returns the updated state after encoding the symbol
    private static uint EncodeSymbol(uint state, byte symbol, uint freq, uint cumFreq, uint total)
    {
        // The rANS update formula: new_state = (state / freq) * total + cumFreq + (state % freq)
        // This compresses one symbol into the state without writing any bits to the output
        return (state / freq) * total + cumFreq + (state % freq);
    }
}
```

Gate B check: encode the byte string `[1, 1, 1, 1, 2, 2, 3]` using rANS and compare the number of output bits to Huffman coding of the same string. rANS must use fewer bits (closer to Shannon entropy of approximately 1.38 bits/symbol for this distribution).

Tests:
- Round-trip correctness
- Verify output size is within 1% of Shannon entropy for a text file
- Verify output size is smaller than Huffman for skewed distributions (one symbol appearing 90% of the time)

**Estimated: 2 days**

**Total estimated days for sub-project 25: 7**

---

## Sub-Project 26 — Block Sorting (Burrows-Wheeler) Family

**What this family does in plain language:** instead of finding matches as you scan forward (like LZ77), the Burrows-Wheeler Transform rearranges the entire block of data at once so that identical characters cluster together. After the transform, a simple Move-To-Front transform converts the clusters into runs of small numbers, and then Run-Length Encoding compresses the runs. Finally, Huffman coding compresses the output. This pipeline achieves better compression ratio than LZ family on most data, but is much slower because it processes the entire block at once.

**Reference sources:**
- bzip2: `https://sourceware.org/bzip2/` — `compress.c` + `blocksort.c` — BSD-like license
- The `blocksort.c` file is the most interesting — it implements an optimized suffix array sort

**Folder:** `25.Compression/Families/03-BwtFamily/`

---

### Phase 1 — Burrows-Wheeler Transform (2 days)

**What BWT does in plain language:** take a string `abracadabra` and consider all its rotations (shifting the string by 1 each time, wrapping around). Sort all rotations alphabetically. The last column of the sorted table is the BWT output. This last column has a magical property: identical characters cluster together because all rotations starting with the same prefix are adjacent after sorting. The original string can be reconstructed from the BWT output plus a single integer (which row of the sorted table was the original).

What to write: `BurrowsWheelerTransform.cs`

```csharp
// BurrowsWheelerTransform rearranges the bytes of a block so that identical bytes cluster.
// After this transformation, the data becomes much easier to compress with entropy coders.
// Reference: bzip2 blocksort.c (BSD license)
public static class BurrowsWheelerTransform
{
    // Apply the Burrows-Wheeler Transform to a block of bytes
    // input — the block to transform (should be a fixed-size block, typically 100–900 KB)
    // Returns the transformed bytes and the row index of the original string in the sorted table
    //   (the row index is needed to reverse the transform during decompression)
    public static (byte[] transformed, int originalRow) Forward(byte[] input)
    {
        int blockLength = input.Length;

        // Create the sorted order of all rotations without materializing them
        // A rotation i is the string: input[i], input[i+1], ..., input[n-1], input[0], ..., input[i-1]
        // We sort by creating an array of indices and sorting the indices by their rotation

        // Step 1: build an array of rotation starting positions (0 to blockLength-1)
        var sortedRotations = Enumerable.Range(0, blockLength).ToArray();

        // Step 2: sort the rotation indices by lexicographic order of their rotation
        // This is the expensive step — O(N log N) with a fast suffix sorter
        Array.Sort(sortedRotations, (a, b) => CompareRotations(input, a, b, blockLength));

        // Step 3: the transformed output is the last character of each sorted rotation
        // The last character of rotation starting at index i is input[(i + blockLength - 1) % blockLength]
        var transformed = new byte[blockLength];
        int originalRow = -1;

        for (int rank = 0; rank < blockLength; rank++)
        {
            // The last character of the rotation starting at sortedRotations[rank]
            int lastPosition = (sortedRotations[rank] + blockLength - 1) % blockLength;
            transformed[rank] = input[lastPosition];

            // If this rotation is the original string (starts at position 0),
            // record its rank so the decompressor can find the starting point
            if (sortedRotations[rank] == 0)
            {
                originalRow = rank;
            }
        }

        return (transformed, originalRow);
    }

    // Compare two rotations lexicographically, used when sorting
    private static int CompareRotations(byte[] input, int startA, int startB, int length)
    {
        for (int offset = 0; offset < length; offset++)
        {
            byte charA = input[(startA + offset) % length];
            byte charB = input[(startB + offset) % length];
            if (charA != charB) return charA.CompareTo(charB);
        }
        return 0; // rotations are identical (should not happen for a valid block)
    }

    // Reverse the Burrows-Wheeler Transform to recover the original block
    // bwtOutput — the transformed bytes from Forward()
    // originalRow — the row index returned by Forward()
    public static byte[] Inverse(byte[] bwtOutput, int originalRow)
    {
        int blockLength = bwtOutput.Length;

        // Step 1: sort the BWT output to get the first column of the sorted rotation table
        // The first column contains the same characters as the BWT output but in sorted order
        var firstColumn = (byte[])bwtOutput.Clone();
        Array.Sort(firstColumn);

        // Step 2: build the "next" array that links each row in the sorted table
        // to the row of its right-cyclic rotation
        // ... (standard BWT inverse algorithm) ...

        // Step 3: walk the next array starting from originalRow to reconstruct the original string
        var original = new byte[blockLength];
        int currentRow = originalRow;
        for (int i = blockLength - 1; i >= 0; i--)
        {
            original[i] = bwtOutput[currentRow];
            // currentRow = next[currentRow] — follow the inverse mapping
        }

        return original;
    }
}
```

Gate B: transform `"abracadabra"`, verify the output is `"rdarcaaaabb"` (with original row index 3 — the standard textbook example). Inverse transform must return `"abracadabra"`.

Tests:
- Round-trip: forward then inverse = original
- Verify the BWT output has more repeated characters than the input (measure run count)
- Verify performance: transforming 100 KB should complete in under 2 seconds

**Estimated: 2 days**

---

### Phase 2 — Move-To-Front transform and Run-Length Encoding (1.5 days)

**What Move-To-Front (MTF) does in plain language:** maintains a list of all 256 byte values. When it sees a byte, it outputs the current position of that byte in the list and moves it to position 0. Because the BWT output has runs of identical characters, the most recently seen character is always near position 0 — so the MTF output is mostly small numbers (zeros and ones). Small numbers are easier to compress.

**What Run-Length Encoding does in plain language:** scans the MTF output for consecutive identical values. Replaces each run with a (value, count) pair. Particularly effective on the zeros produced by MTF on BWT output.

What to write: `MoveToFrontTransform.cs`, `RunLengthEncoder.cs`

```csharp
// MoveToFrontTransform converts BWT output (repeated characters) into
// a stream of small numbers (mostly zeros) that is easy to compress further.
// After BWT, identical characters appear in runs; MTF outputs 0 for any repeated character.
public static class MoveToFrontTransform
{
    // Apply Move-To-Front encoding to a byte sequence
    // input — typically the BWT-transformed bytes
    // Returns a sequence of integers (0-255) where small numbers indicate recently seen bytes
    public static int[] Forward(byte[] input)
    {
        // Initialize the symbol list in canonical order: [0, 1, 2, ..., 255]
        // This list tracks which symbol is at which position
        var symbolList = Enumerable.Range(0, 256).ToArray();

        var output = new int[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            // Find the current position of the input byte in the symbol list
            // Position 0 = most recently seen, 255 = never seen or seen long ago
            int position = Array.IndexOf(symbolList, (int)input[i]);

            // Output the position — small positions indicate the byte appeared recently
            output[i] = position;

            // Move this symbol to the front of the list (position 0)
            // Shift everything before it one position to the right
            for (int j = position; j > 0; j--)
            {
                symbolList[j] = symbolList[j - 1];
            }
            symbolList[0] = input[i];
        }

        return output;
    }
}
```

Tests:
- Apply MTF to the BWT output of `"abracadabra"`, verify the output contains mostly zeros
- Round-trip: MTF forward then inverse = original
- Apply RLE after MTF, verify the output is smaller than MTF output alone

**Estimated: 1.5 days**

---

### Phase 3 — Full bzip2-style pipeline (1.5 days)

Wire all four stages into a single compressor that matches bzip2's behavior:

```
Input block
  → Burrows-Wheeler Transform (block sorting)
  → Move-To-Front transform
  → Run-Length Encoding
  → Huffman entropy coding
  → Compressed block
```

What to write: `Bzip2StyleCompressor.cs`

This class orchestrates the four stages. It does not implement them — it calls the classes from Phases 1 and 2, plus the Huffman coder from sub-project 25.

Gate B check: compress the same 10 KB text file with the pipeline and compare the output size to actual bzip2 (run via a shell command in the test, compare results). Sizes must be within 5% (the bzip2 implementation uses additional optimizations not worth replicating at this stage).

Tests:
- Round-trip the complete pipeline
- Verify compression ratio is better than LZ4 on English text (bzip2 typically achieves 3.5–4× on text vs LZ4's 2.5–3×)
- Verify processing time for 1 MB: must complete under 10 seconds (BWT is slow by design)

**Total estimated days for sub-project 26: 5**

---

## Sub-Project 27 — Specialized Algorithms

**Folder:** `25.Compression/Families/04-Specialized/`

This sub-project implements three algorithms that push compression quality further for specific use cases.

---

### Phase 1 — Brotli (for web content and text) (3 days)

**What Brotli does:** Google's compressor for web content. Adds two innovations on top of LZ77 + Huffman: (1) a built-in static dictionary of 13,000 common words and phrases from the web (so even a single compressed file can reference common patterns without prior data), and (2) second-order context modeling — predicts the next byte using the previous two bytes, not just one. This makes it significantly better than gzip for HTML, CSS, and JavaScript.

**Reference:** `https://github.com/google/brotli` — `c/enc/encode.cc` — MIT license.

What to write: `BrotliCompressor.cs`, `BrotliStaticDictionary.cs`

The static dictionary is a fixed binary file. `BrotliStaticDictionary.cs` loads it and exposes it as a lookup table: given a string, returns the position in the dictionary and a transform index (Brotli supports 121 transforms that modify dictionary words, e.g. uppercase first letter, add suffix).

Second-order context modeling:
```csharp
// BrotliContextModel maintains statistics for predicting the next byte based on
// the two most recently seen bytes — this is "second-order context modeling"
// (контекстная модель второго порядка — предсказание следующего байта по двум предыдущим).
// A regular compressor uses only the most recent byte (first-order context).
// Using two bytes gives better predictions for text because many letter pairs
// strongly predict what comes next (e.g. "qu" almost always precedes a vowel in English).
public class BrotliContextModel
{
    // A table indexed by [previous_byte][byte_before_that] → frequency distribution
    // context[65][32] contains frequency counts for all bytes that followed "A" preceded by space
    private readonly long[,,] contextFrequencies = new long[256, 256, 256];

    // Record that byte `current` was seen after the two-byte context (prev1, prev2)
    public void RecordObservation(byte prev2, byte prev1, byte current)
    {
        contextFrequencies[prev2, prev1, current]++;
    }

    // Get the predicted probability distribution for the next byte given the context
    public long[] GetDistribution(byte prev2, byte prev1)
    {
        var dist = new long[256];
        for (int next = 0; next < 256; next++)
        {
            dist[next] = contextFrequencies[prev2, prev1, next];
        }
        return dist;
    }
}
```

Gate B: compress a 10 KB HTML page. Brotli must produce a smaller file than LZ4 and comparable to gzip level 9. Verify exact round-trip.

Tests:
- Round-trip HTML, CSS, JavaScript files
- Verify Brotli size < gzip size for a typical HTML page
- Verify static dictionary references are correctly decoded

**Estimated: 3 days**

---

### Phase 2 — LZMA (highest compression ratio) (3 days)

**What LZMA does:** uses LZ77 for pattern matching (similar to LZ4) but adds Markov chain probability models (цепи Маркова — статистические модели где вероятность следующего символа зависит от нескольких предыдущих) and range coding (диапазонное кодирование — кодировщик энтропии который работает с вещественным интервалом а не с битами). Each back-reference and each literal byte is encoded using a model trained on the bytes already seen. This adaptive modeling is why LZMA achieves the best compression ratios but is slow to compress (the models update after every byte).

**Reference:** `https://github.com/jljusten/LZMA-SDK` — `SDK/C/LzmaEnc.c` + `SDK/C/LzmaDec.c` — public domain.

What to write: `LzmaCompressor.cs`, `LzmaRangeCoder.cs`, `LzmaMarkovModel.cs`

The `LzmaRangeCoder` implements the core arithmetic: maintains a "range" interval that narrows with each symbol encoded, flushing bytes when the range becomes too small. This is the `range coding` technique described in the reference.

```csharp
// LzmaRangeCoder implements range coding (диапазонное кодирование).
// Range coding represents the entire compressed message as a single real number
// in the interval [0, 1). Each symbol narrows the interval based on its probability.
// When enough bits of the interval are fixed, they are flushed to the output.
// This achieves compression efficiency very close to Shannon entropy.
// Reference: Lempel-Ziv-Markov chain Algorithm SDK (LzmaDec.c, public domain)
public class LzmaRangeCoder
{
    // The current lower bound of the range interval
    // Range coding maintains an interval [low, low + range) that narrows as symbols are encoded
    private ulong low = 0;

    // The width of the current interval
    // When range < MinimumRange, we flush the top byte to the output and rescale
    private uint range = 0xFFFFFFFF;

    // The minimum range before flushing (must be a power of 2 for efficient rescaling)
    private const uint MinimumRange = 1u << 24;

    // Encode one binary decision (0 or 1) with a given probability for the 0 outcome
    // probability — the predicted probability of the 0 outcome, on a scale of 0 to 2047
    //   (LZMA uses 11-bit fixed-point probabilities for speed)
    // actualBit — the actual bit being encoded (0 or 1)
    // outputBuffer — the growing output byte buffer
    public void EncodeBit(ref ushort probability, int actualBit, List<byte> outputBuffer)
    {
        // Split the current interval into two parts proportional to the probabilities
        // The 0-outcome gets the lower portion, the 1-outcome gets the upper portion
        uint midPoint = (range >> 11) * probability;

        if (actualBit == 0)
        {
            // The 0 outcome was observed — narrow the range to the lower portion
            // and update the probability estimate toward "0 is more likely"
            range = midPoint;
            probability += (ushort)((2048 - probability) >> 5); // move toward 2048 (certain 0)
        }
        else
        {
            // The 1 outcome was observed — shift the interval up to the upper portion
            low += midPoint;
            range -= midPoint;
            probability -= (ushort)(probability >> 5); // move toward 0 (certain 1)
        }

        // When the range shrinks below the minimum, flush the top byte to output
        // and rescale both low and range to maintain precision
        while (range < MinimumRange)
        {
            outputBuffer.Add((byte)(low >> 24));
            low <<= 8;
            range <<= 8;
        }
    }
}
```

Gate B check: compress a 1 MB binary file. LZMA must produce a smaller file than Zstandard level 1 (its design goal is best ratio, not best speed). Verify decompression produces exact original.

Tests:
- Round-trip a binary file and a text file
- Verify LZMA ratio > Zstandard ratio > LZ4 ratio on the same test corpus
- Verify processing time: LZMA is expected to be 10–50× slower than LZ4 for compression (this is normal and should be documented in the test)

**Estimated: 3 days**

---

### Phase 3 — OpenZL (structured data, 2025) (2.5 days)

**What OpenZL does:** a 2025 research compressor from Meta. Unlike general compressors, it analyzes the structure of the input data first (e.g. JSON fields, column values in CSV, binary record fields). Then it applies a chain of transformations to reorder the data into a more compressible form before applying entropy coding. For structured data (JSON, CSV, columnar binary formats), it achieves compression ratios comparable to specialized column-store compressors.

**Reference:** `https://github.com/facebookresearch/OpenZL` — Apache 2.0 license.

What to write: `OpenZlStructureAnalyzer.cs`, `OpenZlCompressor.cs`

```csharp
// OpenZlStructureAnalyzer detects the structure of the input data and selects
// an appropriate transformation chain before compression.
// For JSON: identifies field names and groups all values for the same field together
//   (columnar rearrangement — перестановка столбцов).
// For CSV: separates columns and compresses each column with the best algorithm for that type.
// For unstructured binary: falls back to Zstandard.
public class OpenZlStructureAnalyzer : BaseService
{
    // Analyze the input bytes and return a description of the detected structure
    // Returns: the structure type and suggested transformation chain
    public OperationResult<DataStructureDescription> Analyze(byte[] input)
    {
        // Attempt to detect JSON structure (starts with '{' or '[')
        if (input.Length > 0 && (input[0] == '{' || input[0] == '['))
        {
            return OperationResult<DataStructureDescription>.Ok(
                DetectJsonStructure(input));
        }

        // Attempt to detect CSV structure (contains lines with consistent comma counts)
        if (LooksLikeCsv(input))
        {
            return OperationResult<DataStructureDescription>.Ok(
                DetectCsvStructure(input));
        }

        // No recognized structure — use general-purpose compression
        return OperationResult<DataStructureDescription>.Ok(
            new DataStructureDescription { StructureType = "unstructured" });
    }
}
```

Gate B check: compress a 100 KB JSON file with OpenZL and compare to Zstandard. OpenZL must achieve at least 5% better compression ratio on the JSON file. Unstructured binary data should fall back to Zstandard-equivalent quality.

Tests:
- JSON file: OpenZL > Zstandard in ratio
- CSV file: OpenZL > Zstandard in ratio
- Random binary: falls back gracefully (ratio within 3% of Zstandard)
- Round-trip all three input types

**Estimated: 2.5 days**

**Total estimated days for sub-project 27: 8.5**

---

## Plan Review Checks

**Goal delivery check:** all 10 algorithms from the source document are covered — LZ4, Zstandard, Snappy, LZAV (sub-project 24); DEFLATE, Huffman, rANS (sub-project 25); bzip2-style BWT pipeline (sub-project 26); Brotli, LZMA, OpenZL (sub-project 27). All eleven goals map to implementation phases with Gate B verification steps. ✓

**Design artifact completeness check:** each algorithm names the specific source file from the open-source reference used as the specification. ✓

**Authentication check:** no HTTP controllers produced. No auth declaration needed. ✓

**NDJSON type coverage check:** no arbiter fixture files produced. Not applicable. ✓

**Definition-of-done check:** each sub-project defines acceptance criteria with verifiable metrics (compression ratio, throughput, round-trip correctness). Gate A (build + tests), Gate B (output matches or exceeds reference implementations), Gate C (explicit approval from Luba) apply to each sub-project. ✓

**Count accuracy check:** 4 sub-project families → sub-projects 24 through 27. No number drift. ✓

---

## Summary Table

| Sub-project | Family name | Algorithms | Reference sources | Days |
|---|---|---|---|---|
| 24 | Lempel-Ziv dictionary | Snappy, LZAV, LZ4, Zstandard | snappy.cc, lzav.h, lz4.c, zstd | 10 |
| 25 | Entropy coding | Huffman, DEFLATE, rANS | zlib trees.c, rans_byte.h | 7 |
| 26 | Burrows-Wheeler block sorting | BWT, MTF, RLE, bzip2 pipeline | bzip2 blocksort.c | 5 |
| 27 | Specialized | Brotli, LZMA, OpenZL | google/brotli, LZMA-SDK, OpenZL | 8.5 |
| **Total** | | **11 algorithms** | | **30.5 days** |

---

## Updated Solution Structure

```
LlmFromScratch.sln
├── 00.TensorCore/
├── 01.Tokenizer/ … 13.CodeOptimizer/
├── 14.MachineLearning/  (sub-projects 14–23 inside)
└── 25.Compression/                   ← this plan
    ├── Fabric/
    │   ├── ICompressor.cs
    │   ├── IDecompressor.cs
    │   ├── ICompressionStorage.cs
    │   └── ICompressionBenchmark.cs
    ├── Core/
    │   ├── CompressionOptions.cs
    │   ├── CompressionResult.cs
    │   └── BenchmarkResult.cs
    ├── Families/
    │   ├── 01-LzFamily/              sub-project 24
    │   │   ├── LzMatchFinder.cs
    │   │   ├── SnappyCompressor.cs
    │   │   ├── LzavCompressor.cs
    │   │   ├── Lz4Compressor.cs
    │   │   └── ZstandardCompressor.cs
    │   ├── 02-EntropyCoding/         sub-project 25
    │   │   ├── SymbolStatistics.cs
    │   │   ├── HuffmanTree.cs
    │   │   ├── HuffmanCompressor.cs
    │   │   ├── DeflateCompressor.cs
    │   │   └── RansCompressor.cs
    │   ├── 03-BwtFamily/             sub-project 26
    │   │   ├── BurrowsWheelerTransform.cs
    │   │   ├── MoveToFrontTransform.cs
    │   │   ├── RunLengthEncoder.cs
    │   │   └── Bzip2StyleCompressor.cs
    │   └── 04-Specialized/           sub-project 27
    │       ├── BrotliStaticDictionary.cs
    │       ├── BrotliContextModel.cs
    │       ├── BrotliCompressor.cs
    │       ├── LzmaRangeCoder.cs
    │       ├── LzmaMarkovModel.cs
    │       ├── LzmaCompressor.cs
    │       ├── OpenZlStructureAnalyzer.cs
    │       └── OpenZlCompressor.cs
    └── Tests/
        ├── SnappyTests.cs
        ├── LzavTests.cs
        ├── Lz4Tests.cs
        ├── ZstandardTests.cs
        ├── HuffmanTests.cs
        ├── DeflateTests.cs
        ├── RansTests.cs
        ├── BwtTests.cs
        ├── Bzip2StyleTests.cs
        ├── BrotliTests.cs
        ├── LzmaTests.cs
        └── OpenZlTests.cs
```

---

## Pre-Commit Checklist (same as all other sub-projects)

```
□ Every public method returns OperationResult<T>                          (coding rule 3)
□ Every service class extends BaseService                                   (coding rule 4)
□ No scopeId or sessionId parameter in any method signature                (coding rule 5)
□ No new() call on a fabric implementation inside a service                (fabric-first)
□ Every line of code carries a plain-language comment
□ No abbreviation without the full form on the same line
□ Variable names fully spelled out (no lz, bwt, mtf, rle, rans, ans)
□ dotnet build exits 0, no warnings
□ All unit tests pass
□ Gate B: output size within stated tolerance of reference C implementation
```

---

## Dependency Map

Sub-projects 24–27 are independent of each other and of all other sub-projects 01–23.

One optional dependency: `ZstandardCompressor` and `BrotliCompressor` produce smaller output when used on the token shards from sub-project 02 (DatasetPipeline). This is an optional integration — both algorithms work without it.

The Huffman coder from sub-project 25 can optionally be reused by sub-project 26 (instead of re-implementing it inside the BWT pipeline). This reuse is encouraged — import via the shared `ICompressor` interface.

---

*Plan version 1.0 — covers sub-projects 24 through 27 (four compression algorithm families, eleven algorithms total). Does not touch sub-projects 01–23 or XIIGen work.*
