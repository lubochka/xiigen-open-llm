# 24.Compression - документация реализации

`24.Compression` - это набор детерминированных алгоритмов сжатия байтов: LZ-семейство, entropy coding, BWT-пайплайн и специализированные кодеки. На вход обычно приходит `byte[] input` и `CompressionOptions`, на выходе - `OperationResult<CompressionResult>` с compressed bytes, исходной длиной и именем алгоритма. Это инфраструктура для хранения/передачи данных, не `ILearningMachine`, не обученное semantic reasoning и не доказательство качества модели. `24.Compression/state.json` полезен как исторический статус подпроекта, но не является свежим runtime proof.

## 1. Простая UML-схема для draw.io

```mermaid
classDiagram
    class ICompressor {
        <<interface>>
        +CompressAsync(byte[] input, CompressionOptions options)
        +DecompressAsync(byte[] compressed, int expectedOriginalLength)
    }
    class CompressionOptions
    class CompressionResult
    class OperationResult~T~
    class LzMatchFinder
    class SnappyCompressor
    class LzavCompressor
    class Lz4Compressor
    class ZstandardCompressor
    class SymbolStatistics
    class HuffmanTree
    class HuffmanCompressor
    class DeflateCompressor
    class RansCompressor
    class BurrowsWheelerTransform
    class MoveToFrontTransform
    class RunLengthEncoder
    class Bzip2StyleCompressor
    class BrotliContextModel
    class BrotliStaticDictionary
    class BrotliCompressor
    class LzmaRangeCoder
    class LzmaMarkovModel
    class LzmaCompressor
    class OpenZlStructureAnalyzer
    class OpenZlCompressor

    ICompressor <|.. SnappyCompressor
    ICompressor <|.. LzavCompressor
    ICompressor <|.. Lz4Compressor
    ICompressor <|.. ZstandardCompressor
    ICompressor <|.. HuffmanCompressor
    ICompressor <|.. DeflateCompressor
    ICompressor <|.. RansCompressor
    ICompressor <|.. Bzip2StyleCompressor
    ICompressor <|.. BrotliCompressor
    ICompressor <|.. LzmaCompressor
    ICompressor <|.. OpenZlCompressor
    LzmaCompressor --> LzMatchFinder
    ZstandardCompressor --> HuffmanCompressor
    HuffmanCompressor --> SymbolStatistics
    HuffmanCompressor --> HuffmanTree
    RansCompressor --> SymbolStatistics
    Bzip2StyleCompressor --> BurrowsWheelerTransform
    Bzip2StyleCompressor --> MoveToFrontTransform
    Bzip2StyleCompressor --> RunLengthEncoder
    Bzip2StyleCompressor --> HuffmanCompressor
    BrotliCompressor --> BrotliContextModel
    BrotliCompressor --> BrotliStaticDictionary
    LzmaCompressor --> LzmaRangeCoder
    LzmaCompressor --> LzmaMarkovModel
    OpenZlCompressor --> OpenZlStructureAnalyzer
    OpenZlCompressor --> ICompressor

    note for ICompressor "ПОЛНАЯ\nЕдиный контракт compress/decompress\nдля всех codec classes."
    note for SnappyCompressor "ПОЛНАЯ\nHash-table LZ77, literal/copy stream,\nfast compression path."
    note for LzavCompressor "ПОЛНАЯ\nChained hash LZ search,\ncopy marker + literal blocks."
    note for Lz4Compressor "ПОЛНАЯ\nLZ4 token format:\nliteral nibble + match nibble."
    note for ZstandardCompressor "ЧАСТИЧНАЯ/упрощенная\nZstd-style LZ77 + Huffman literals,\nне полный FSE Zstandard."
    note for HuffmanCompressor "ПОЛНАЯ\nFrequency table -> Huffman tree ->\nLSB-first bit-packed bytes."
    note for DeflateCompressor "ПОЛНАЯ wrapper\nOperationResult wrapper над .NET DeflateStream."
    note for RansCompressor "ПОЛНАЯ\nrANS normalized frequencies,\nreverse encode / forward decode."
    note for Bzip2StyleCompressor "ПОЛНАЯ/упрощенная\nBWT -> MTF -> RLE -> Huffman\nblock pipeline."
    note for BrotliCompressor "ПОЛНАЯ wrapper + analytical helpers\nBrotliStream для bytes,\ncontext/dictionary helpers рядом."
    note for LzmaCompressor "ПОЛНАЯ/упрощенная\nRange coder + Markov + LZ match finder."
    note for OpenZlCompressor "ПОЛНАЯ/упрощенная\nCSV/JSON columnar rearrangement,\nthen inner compressor."
```

| Узел | Что делает | Статус | Где живет |
|---|---|---|---|
| `ICompressor` | Общий async-контракт `CompressAsync`/`DecompressAsync`. | Полная | `24.Compression/Fabric/Interfaces.cs` |
| `CompressionOptions`, `CompressionResult`, `OperationResult<T>` | DTO/options/result envelope для всех кодеков. | Полная | `24.Compression/Core/*.cs` |
| LZ family | Ищет повторяющиеся байтовые участки и заменяет их ссылками назад. | Полная/упрощенная по формату | `24.Compression/Families/01-LzFamily/*` |
| Entropy family | Считает частоты и кодирует частые символы более коротко. | Полная, `Deflate` wrapper | `24.Compression/Families/02-EntropyCoding/*` |
| BWT family | Переставляет блоки, делает MTF/RLE, затем Huffman. | Полная/упрощенная | `24.Compression/Families/03-BwtFamily/*` |
| Specialized codecs | Brotli/LZMA/OpenZL variants and helpers. | Полная/частично wrapper/упрощенная | `24.Compression/Families/04-Specialized/*` |

### Симуляции по каждой важной связи UML

| Связь | Пример входа | Что происходит | Ожидаемый результат | Что НЕ доказывает |
|---|---|---|---|---|
| `Caller -> ICompressor` | `input = UTF8("aaaa...")`, `CompressionOptions{CompressionLevel=6}` | Caller вызывает единый контракт, не зная конкретный алгоритм. | `OperationResult<CompressionResult>.Ok(...)` или `Fail(code)`. | Не доказывает, что алгоритм лучший для всех данных. |
| `Snappy/LZAV/LZ4 -> LZ match logic` | Повтор `abcdefghabcdefgh` | Hash/window ищет совпадение и пишет literal/copy sequence. | Decompress возвращает исходные bytes. | Не доказывает semantic понимание данных. |
| `ZstandardCompressor -> HuffmanCompressor` | Текст с повторяющимися literals | Zstd-style сначала делает LZ77 sequences, затем Huffman-compress literals. | Frame с original length, literal section и sequences. | Это не полный production Zstandard с FSE (Finite State Entropy). |
| `HuffmanCompressor -> SymbolStatistics/HuffmanTree` | `aaaaab` | Частоты строят дерево; частый `a` получает короткий code. | Bit-packed stream + 256-byte code length table. | Не доказывает качество на случайных данных. |
| `RansCompressor -> SymbolStatistics` | Repetitive bytes | Frequencies normalize до `Scale=65536`, encoder идет backwards. | Header с original length + 256 frequency entries + encoded state. | Не является обучением вероятностей; таблица строится из текущего input. |
| `Bzip2Style -> BWT -> MTF -> RLE -> Huffman` | `"banana banana"` | BWT группирует похожие байты, MTF дает малые числа, RLE сжимает runs, Huffman кодирует. | Decompress reverse pipeline восстанавливает bytes. | Это bzip2-style, а не полная совместимость с `.bz2` форматом. |
| `BrotliCompressor -> BrotliStream` | HTML/CSS text | Wrapper выбирает quality и делегирует byte-level compression .NET `BrotliStream`. | Round-trip bytes через Brotli. | Context model/static dictionary не доказывают full custom Brotli encoder. |
| `LzmaCompressor -> RangeCoder/Markov/LzMatchFinder` | Текст с повторами | Literal/match flag, match length/distance или literal byte кодируются range coder. | `AlgorithmName="LZMA"`, decompress восстанавливает bytes в тестах. | Это simplified single-pass LZMA, не полный LZMA SDK feature set. |
| `OpenZlCompressor -> StructureAnalyzer -> inner ICompressor` | CSV с 5 строками и 3 колонками | Analyzer видит columnar-friendly CSV, rearranges by columns, затем inner compressor. | Header mode `1`, decompress делает decolumnize. | Structured rearrangement не равно semantic data understanding. |

## 2. Алгоритмы без кода на русском

### Алгоритм A: общий контракт сжатия

| Поле | Значение |
|---|---|
| Классы | `OperationResult<T>`, `CompressionOptions`, `CompressionResult`, `ICompressor`, `ICompressionStorage`, `ICompressionBenchmark` |
| Методы | `CompressAsync(byte[] input, CompressionOptions options)`, `DecompressAsync(byte[] compressed, int expectedOriginalLength)` |
| Файлы | `24.Compression/Core/Core.cs:5`, `24.Compression/Core/Types.cs:4`, `24.Compression/Fabric/Interfaces.cs:5` |
| Вход | `byte[] input`, `CompressionOptions{CompressionLevel, DictionaryPath, MaxMemoryMegabytes}` |
| Выход | `OperationResult<CompressionResult>` или `OperationResult<byte[]>` |
| Статус | Полная инфраструктура contracts/DTO |

Шаги:

1. Caller выбирает compressor, например `new Lz4Compressor()`.
2. Передает bytes и options.
3. Compressor возвращает `OperationResult.Ok(new CompressionResult { CompressedBytes, OriginalLength, AlgorithmName })` либо `Fail`.
4. Decompress принимает compressed bytes и expected original length, восстанавливает bytes или возвращает `Fail`.

Что не делает: не обучает модель, не хранит semantic state, не доказывает качество конкретного codec без runtime QA.

### Алгоритм B: LZ family - `SnappyCompressor`, `LzavCompressor`, `Lz4Compressor`, `ZstandardCompressor`

| Поле | Значение |
|---|---|
| Классы | `LzMatchFinder`, `SnappyCompressor`, `LzavCompressor`, `Lz4Compressor`, `ZstandardCompressor` |
| Методы | `FindBestMatch(ReadOnlySpan<byte>, int, int, int)`, `CompressAsync`, `DecompressAsync` |
| Файлы | `24.Compression/Families/01-LzFamily/*.cs` |
| Вход | bytes вроде `"patternpatternpattern"` |
| Выход | literal/copy encoded bytes в `CompressionResult` |
| Статус | Реализовано; `ZstandardCompressor` честно simplified Zstd-style |

Шаги:

1. Пустой input отклоняется как `EMPTY_INPUT`.
2. Алгоритм сканирует input слева направо.
3. Hash table или match finder ищет предыдущий участок с такими же байтами.
4. Если match найден, compressor пишет ссылку назад: distance/offset и length.
5. Если match не найден, compressor пишет literal bytes.
6. Decompress читает header/sequence format и копирует literals или bytes из уже восстановленного output.

Ветви:

| Ветка | Пример | Результат | QA |
|---|---|---|---|
| Empty input | `[]` | `Fail("EMPTY_INPUT")` | `Tests/CompressionTests.cs:31`, `73`, `117`, `159` |
| Repetitive data | `"xxxx"` * 500 | compressed меньше input для LZ4/Snappy tests | `Tests/CompressionTests.cs:20`, `106` |
| Binary round-trip | bytes `0..255` | output после decompress равен input | `Tests/CompressionTests.cs:81` |

Что не делает: LZ family не понимает смысл данных; это byte-level redundancy search.

### Алгоритм C: entropy coding - `SymbolStatistics`, `HuffmanTree`, `HuffmanCompressor`, `DeflateCompressor`, `RansCompressor`

| Поле | Значение |
|---|---|
| Классы | `SymbolStatistics`, `HuffmanTree`, `HuffmanCompressor`, `DeflateCompressor`, `RansCompressor` |
| Методы | `CountFrequencies`, `ComputeShannonEntropy`, `Build`, `CompressAsync`, `DecompressAsync` |
| Файлы | `24.Compression/Families/02-EntropyCoding/*.cs` |
| Вход | bytes с частотами символов |
| Выход | Huffman/rANS/Deflate compressed bytes |
| Статус | Реализовано; `DeflateCompressor` является wrapper над `.NET DeflateStream` |

Шаги:

1. `SymbolStatistics.CountFrequencies` строит массив из 256 счетчиков.
2. Huffman path строит дерево: каждый present symbol становится leaf, две наименьшие частоты сливаются до root.
3. `HuffmanCompressor` пишет 256-byte code length table, original length и bit-packed payload.
4. `RansCompressor` нормализует частоты до `Scale=65536`, кодирует input backwards и пишет frequency header.
5. `DeflateCompressor` выбирает `.NET CompressionLevel` и делегирует `DeflateStream`.

Ветви:

| Ветка | Пример | Результат | QA |
|---|---|---|---|
| Single symbol Huffman | `AAAAA` | code length >= 1 | `Tests/CompressionTests.cs:231` |
| Empty compressor input | `[]` | `Fail("EMPTY_INPUT")` | `Tests/CompressionTests.cs:276`, `299`, `339` |
| Corrupt/too short decode | too short buffer | `Fail("CORRUPT")` в code branches | Source evidence, no fresh test run in D2E |

Что не делает: entropy coding использует статистику текущего блока, не trainable semantic model.

### Алгоритм D: BWT family - `BurrowsWheelerTransform`, `MoveToFrontTransform`, `RunLengthEncoder`, `Bzip2StyleCompressor`

| Поле | Значение |
|---|---|
| Классы | `BurrowsWheelerTransform`, `MoveToFrontTransform`, `RunLengthEncoder`, `Bzip2StyleCompressor` |
| Методы | `Forward`, `Inverse`, `CompressAsync`, `DecompressAsync` |
| Файлы | `24.Compression/Families/03-BwtFamily/*.cs` |
| Вход | byte block, например `"banana"` или long text |
| Выход | transformed bytes / restored bytes / bzip2-style frame |
| Статус | Реализовано как bzip2-style pipeline, не full `.bz2` compatibility claim |

Шаги:

1. BWT строит все rotation start indices, сортирует rotations и берет last column.
2. BWT сохраняет `OriginalRow`, без него inverse невозможен.
3. MTF держит список символов `0..255`; каждый byte заменяется позицией и переносится вперед.
4. RLE превращает runs одинаковых MTF values в `(value,count)`.
5. Bzip2-style compressor по блокам делает `BWT -> MTF -> RLE -> Huffman`.
6. Decompress идет обратно: Huffman -> RLE inverse -> MTF inverse -> BWT inverse.

Ветви:

| Ветка | Пример | Результат | QA |
|---|---|---|---|
| Single byte BWT | `[0x42]` | round-trip | `Tests/CompressionTests.cs:382` |
| Long RLE run | 300 zeros | split into count <= 255 and inverse restores | `Tests/CompressionTests.cs:444` |
| Empty bzip2-style input | `[]` | `Fail` | `Tests/CompressionTests.cs:479` |

Что не делает: BWT переставляет bytes для лучшего сжатия, но не распознает смысл текста.

### Алгоритм E: specialized codecs - Brotli, LZMA, OpenZL

| Поле | Значение |
|---|---|
| Классы | `BrotliContextModel`, `BrotliStaticDictionary`, `BrotliCompressor`, `LzmaRangeCoder`, `LzmaMarkovModel`, `LzmaCompressor`, `OpenZlStructureAnalyzer`, `OpenZlCompressor` |
| Методы | `Train`, `FindMatch`, `CompressAsync`, `EncodeBit`, `EncodeLiteral`, `Analyze`, columnarize/decolumnize через compressor methods |
| Файлы | `24.Compression/Families/04-Specialized/*.cs` |
| Вход | web text, LZMA bytes, CSV/JSON text |
| Выход | Brotli/LZMA/OpenZL compressed bytes или structure report |
| Статус | Реализовано; Brotli/Deflate delegate to .NET streams; OpenZL/LZMA are simplified project implementations |

Шаги:

1. Brotli path: `BrotliCompressor` rejects empty input, maps quality, compresses/decompresses через `BrotliStream`; context model/dictionary are helpers with direct tests.
2. LZMA path: `LzmaCompressor` writes original length, for each position selects literal or match using `LzMatchFinder`, encodes via `LzmaRangeCoder` and `LzmaMarkovModel`.
3. OpenZL path: analyzer detects CSV/JSON/Unknown; if columnar-friendly, compressor rearranges values by columns and delegates to inner `ICompressor`; otherwise mode `0` passes bytes through to inner compressor.

Ветви:

| Ветка | Пример | Результат | QA |
|---|---|---|---|
| Brotli context train | `"aaab"` | distribution count changes | `Tests/CompressionTests.cs:509` |
| Brotli dictionary hit | `"the quick brown fox"` | returns `"the "` entry | `Tests/CompressionTests.cs:531` |
| LZMA literal/match | short text | round-trip | `Tests/CompressionTests.cs:711` |
| OpenZL CSV/JSON | 4+ rows, 2+ columns | mode CSV/JSON columnar path | `Tests/CompressionTests.cs:751`, `809`, `823` |
| OpenZL passthrough | plain text | mode `0`, inner compressor only | `Tests/CompressionTests.cs:799` |

Что не делает: context counters and Markov probabilities are compression statistics, not trained domain reasoning.

## 3. Где логика сидит в коде

| Логика | Файл | Почему важно | Что доказывает | Чего не доказывает |
|---|---|---|---|---|
| Result/options/contracts | `24.Compression/Core/Core.cs:5`, `24.Compression/Core/Types.cs:4`, `24.Compression/Fabric/Interfaces.cs:5` | Все compressors возвращают одинаковые DTO/envelope. | Public API shape. | Fresh runtime success. |
| LZ match search | `24.Compression/Families/01-LzFamily/LzPrimitives.cs:20` | Общий longest back-reference helper. | Byte match logic. | Semantic compression. |
| Snappy/LZAV/LZ4 | `SnappyCompressor.cs:18`, `LzavCompressor.cs:18`, `Lz4Compressor.cs:14` | Основные LZ variants. | Literal/copy formats implemented. | Full upstream codec compatibility. |
| Zstandard-style | `ZstandardCompressor.cs:14` | LZ77 + Huffman literals frame. | Project Zstd-style behavior. | Full Zstandard/FSE implementation. |
| Huffman/rANS/Deflate | `HuffmanCompressor.cs:10`, `RansCompressor.cs:18`, `DeflateCompressor.cs:11` | Entropy coding paths. | Round-trip algorithms/wrappers exist. | Product-level compression benchmark. |
| BWT pipeline | `Bzip2StyleCompressor.cs:16` | Multi-stage block transform. | BWT/MTF/RLE/Huffman chaining. | `.bz2` file compatibility. |
| Brotli/LZMA/OpenZL | `BrotliCompressor.cs:18`, `LzmaCompressor.cs:13`, `OpenZlCompressor.cs:18` | Specialized codecs and structured-data path. | Compression infrastructure. | Learned semantic data model. |

## 4. Короткие важные цитаты кода

### Единый контракт compressor

> Почему важно: все алгоритмы обязаны принимать одинаковые input/options и возвращать `OperationResult`, поэтому caller может заменить codec без знания внутреннего формата. Это доказывает контракт, но не доказывает fresh runtime pass.

`24.Compression/Fabric/Interfaces.cs:5`

```csharp
public interface ICompressor
{
    Task<OperationResult<CompressionResult>> CompressAsync(
        byte[] input,
        CompressionOptions options);
```

### LZ match finder ищет ссылку назад, а не смысл

> Почему важно: LZ-сжатие основано на повторе байтов в окне. Ветка `bestLength >= minimumMatchLength` решает, есть ли copy token; это deterministic byte matching.

`24.Compression/Families/01-LzFamily/LzPrimitives.cs:20`

```csharp
public static LzBackReference? FindBestMatch(
    ReadOnlySpan<byte> input,
    int currentPosition,
    int windowSize,
    int minimumMatchLength)
```

`24.Compression/Families/01-LzFamily/LzPrimitives.cs:52`

```csharp
return bestLength >= minimumMatchLength
    ? new LzBackReference(bestDistance, bestLength)
    : null;
```

### Huffman строит коды из частот текущего input

> Почему важно: частые байты получают более короткие коды через дерево, построенное из текущих частот. Это не trained state: таблица рождается заново для блока.

`24.Compression/Families/02-EntropyCoding/HuffmanCompressor.cs:17`

```csharp
var freq = SymbolStatistics.CountFrequencies(input);
var tree = new HuffmanTree();
tree.Build(freq);
```

### Bzip2-style pipeline явно обратим

> Почему важно: compressor делает BWT, MTF, RLE и Huffman; decompressor идет обратно. Это показывает actual algorithm steps, а не только название класса.

`24.Compression/Families/03-BwtFamily/Bzip2StyleCompressor.cs:39`

```csharp
var (bwtData, originalRow) = BurrowsWheelerTransform.Forward(block);
```

`24.Compression/Families/03-BwtFamily/Bzip2StyleCompressor.cs:112`

```csharp
int[]  mtfData = RunLengthEncoder.Inverse(huffResult.Value!);
byte[] bwtData = MoveToFrontTransform.Inverse(mtfData);
byte[] block   = BurrowsWheelerTransform.Inverse(bwtData, originalRow);
```

### Brotli и Deflate честно являются wrappers над .NET streams

> Почему важно: статус должен быть честным. Здесь byte-level correctness делегируется стандартной библиотеке, а не self-written full Brotli/Deflate implementation.

`24.Compression/Families/04-Specialized/BrotliCompressor.cs:28`

```csharp
using var output = new MemoryStream();
await using var brotli = new BrotliStream(output, (CompressionLevel)Math.Min(quality, 3));
await brotli.WriteAsync(input);
```

`24.Compression/Families/02-EntropyCoding/DeflateCompressor.cs:25`

```csharp
using var outputStream     = new MemoryStream();
await using var deflateStream = new DeflateStream(outputStream, dotNetLevel);
await deflateStream.WriteAsync(input);
```

### OpenZL выбирает CSV/JSON/passthrough branch

> Почему важно: structured compression branch не всегда включается. Только `IsColumnarFriendly` CSV/JSON получает rearrangement; прочее идет passthrough к inner compressor.

`24.Compression/Families/04-Specialized/OpenZlCompressor.cs:30`

```csharp
if (report.IsColumnarFriendly && report.Format == OpenZlStructureAnalyzer.DataFormat.Csv)
{
    payload = ColumnarizeCsv(text, report);
    mode    = 1;
}
```

`24.Compression/Families/04-Specialized/OpenZlCompressor.cs:42`

```csharp
else
{
    payload = input;
    mode    = 0;
}
```

## 5. Test, e2e и QA mapping

Тесты в `Tests/CompressionTests.cs` были прочитаны, но в D2E не запускались по ограничению work order.

| Поведение | Evidence | Статус | Честный gap |
|---|---|---|---|
| Snappy round-trip, repetitive, empty, single byte | `Tests/CompressionTests.cs:10`-`48` | Unit covered in source | Нет fresh run в D2E |
| LZAV text/name/empty/binary | `Tests/CompressionTests.cs:55`-`88` | Unit covered in source | Нет corrupt input fuzzing |
| LZ4 text/repetitive/empty/ratio | `Tests/CompressionTests.cs:96`-`131` | Unit covered in source | Нет benchmark throughput gate |
| Zstandard-style text/repetitive/empty | `Tests/CompressionTests.cs:139`-`163` | Unit covered in source | Нет compatibility claim with real `.zst` |
| Symbol/Huffman/Deflate/rANS | `Tests/CompressionTests.cs:169`-`353` | Unit + round-trip source coverage | Нет broad randomized property tests |
| BWT/MTF/RLE/bzip2-style | `Tests/CompressionTests.cs:359`-`491` | Transform and round-trip source coverage | Нет `.bz2` compatibility claim |
| Brotli helpers/wrapper | `Tests/CompressionTests.cs:497`-`602` | Context/dictionary/round-trip coverage | Wrapper uses .NET stream; no custom Brotli encoder proof |
| LZMA range/Markov/compressor | `Tests/CompressionTests.cs:606`-`743` | Bit/probability/round-trip coverage | Simplified implementation; no official LZMA corpus parity |
| OpenZL analyzer/compressor | `Tests/CompressionTests.cs:747`-`848` | CSV/JSON/passthrough source coverage | Tests assert round-trip/contains, not compression ratio superiority |

## 6. Проверки против ложной готовности

- `24.Compression` не является `ILearningMachine`; здесь нет training lifecycle, model checkpoint, manifest, locator, fresh-load или continue-training evidence.
- `BrotliContextModel.Train` и `LzmaMarkovModel` используют статистические/probability tables для сжатия bytes; это не semantic training.
- `state.json` - historical/status context only. Его нельзя принимать как свежую runtime QA.
- JSON/status/build artifacts не заменяют реальную реализацию алгоритма или runtime round-trip.
- `DeflateCompressor` и `BrotliCompressor` честно помечены как wrappers над `.NET` streams, а не как полностью self-written codec internals.
- Zstandard/Bzip2/LZMA/OpenZL docs говорят "style/simplified", без claims полной форматной совместимости.

## 7. Таблица статуса реализации

| Компонент | Файл | Статус | Что реально работает сейчас |
|---|---|---|---|
| `OperationResult<T>` | `24.Compression/Core/Core.cs` | ✅ Полная | `Ok(value)` / `Fail(message, code)` envelope. |
| `CompressionOptions`, `CompressionResult`, `BenchmarkResult` | `24.Compression/Core/Types.cs` | ✅ Полная | Options, compressed bytes metadata, ratio, benchmark DTO. |
| `ICompressor`, `ICompressionStorage`, `ICompressionBenchmark` | `24.Compression/Fabric/Interfaces.cs` | ✅ Полная | Fabric-first contracts; storage/benchmark contracts present. |
| `LzMatchFinder` | `24.Compression/Families/01-LzFamily/LzPrimitives.cs` | ✅ Полная | Sliding-window longest match helper. |
| `SnappyCompressor` | `24.Compression/Families/01-LzFamily/SnappyCompressor.cs` | ✅ Полная | Varint length + literal/copy entries. |
| `LzavCompressor` | `24.Compression/Families/01-LzFamily/LzavCompressor.cs` | ✅ Полная | Chained hash, max chain 16, literal/copy format. |
| `Lz4Compressor` | `24.Compression/Families/01-LzFamily/Lz4Compressor.cs` | ✅ Полная | LZ4-style token/literal/match format. |
| `ZstandardCompressor` | `24.Compression/Families/01-LzFamily/ZstandardCompressor.cs` | ⚠️ Частичная | Zstd-style LZ77 + Huffman literals; no full FSE/full `.zst` claim. |
| `SymbolStatistics` | `24.Compression/Families/02-EntropyCoding/SymbolStatistics.cs` | ✅ Полная | Counts, Shannon entropy, estimated compressed bytes. |
| `HuffmanTree`, `HuffmanCompressor` | `24.Compression/Families/02-EntropyCoding/*.cs` | ✅ Полная | Tree build, canonical LSB-first codes, compress/decompress. |
| `DeflateCompressor` | `24.Compression/Families/02-EntropyCoding/DeflateCompressor.cs` | ✅ Полная wrapper | OperationResult wrapper over `.NET DeflateStream`. |
| `RansCompressor` | `24.Compression/Families/02-EntropyCoding/RansCompressor.cs` | ✅ Полная | rANS encode/decode with normalized frequencies. |
| `BurrowsWheelerTransform`, `MoveToFrontTransform`, `RunLengthEncoder` | `24.Compression/Families/03-BwtFamily/*.cs` | ✅ Полная | Forward/inverse transforms and RLE split at 255. |
| `Bzip2StyleCompressor` | `24.Compression/Families/03-BwtFamily/Bzip2StyleCompressor.cs` | ⚠️ Частичная | BWT/MTF/RLE/Huffman block pipeline; no full `.bz2` format claim. |
| `BrotliContextModel`, `BrotliStaticDictionary` | `24.Compression/Families/04-Specialized/*.cs` | ✅ Полная helpers | Second-order counts and small static dictionary subset. |
| `BrotliCompressor` | `24.Compression/Families/04-Specialized/BrotliCompressor.cs` | ✅ Полная wrapper | `BrotliStream` compress/decompress plus helper fields. |
| `LzmaRangeCoder`, `LzmaMarkovModel`, `LzmaCompressor` | `24.Compression/Families/04-Specialized/*.cs` | ⚠️ Частичная | Simplified single-pass LZMA-style range/Markov/LZ match path. |
| `OpenZlStructureAnalyzer`, `OpenZlCompressor` | `24.Compression/Families/04-Specialized/*.cs` | ⚠️ Частичная | CSV/JSON detection, columnar rearrangement, inner compressor; no semantic data model claim. |

Documentation status: draft/pass for DOC-WO-D2E sub-agent evidence.
Last verified against source: 2026-06-08.
Known gaps: no fresh test run in D2E; no benchmark/property/fuzz evidence added.
