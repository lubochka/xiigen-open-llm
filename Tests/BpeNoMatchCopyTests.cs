// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tokenizer;

// FIX-BPE transfer, plan 12-bpe-no-match-copy.md, from BPE-NO-MATCH-COPY-01.
//
// While a vocabulary is learned, the pair chosen for a round occurs in only a
// small share of the text chunks. Every chunk was still given a fresh list and
// copied through it, including the great majority where nothing merged. The
// chunk is now returned untouched when its pair does not occur.
//
// What has to stay true: the vocabulary learned, the merges learned, and the
// tokens produced must be exactly what they were. Those are the assertions
// below; the saving itself is a by-product of not copying.
public sealed class BpeNoMatchCopyTests
{
    // Куда измерение печатается. Вывод теста попадает в файл результатов
    // прогона, поэтому измеренные числа сохраняются как улика, а не живут
    // только на экране.
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public BpeNoMatchCopyTests(Xunit.Abstractions.ITestOutputHelper output)
        => _output = output;

    private const string Corpus =
        "the quick brown fox jumps over the lazy dog the quick brown fox " +
        "a tokenizer learns merges from repeated pieces of ordinary text " +
        "the tokenizer learns the pieces the tokenizer repeats the pieces";

    // Repair R4 of the 27.10 transfer audit, BPE row (finding F5). Comparing two
    // runs of the NEW code cannot prove the old table survived: both could have
    // drifted together. The three values below were taken from the tokenizer as
    // it stood BEFORE this transfer — the file at commit 046e0ded876, the last
    // one before FIX-BPE — compiled and run on this very corpus outside the
    // project. They are a historical record, not an output of today's code.
    private static readonly (int Vocab, int TokenCount, string StateSha256)[] BeforeTheTransfer =
    [
        (260, 164, "a5e6c4f48f0b4f6b2dec33d2b07af4d574a96010f5aef3ff8dbd31a28bd53cd2"),
        (288, 88, "41448a7971f75b41df5e3e79b03d70864bb53a125f45116f520bf11e7b28601b"),
        (320, 46, "3278531fbd374e077f8c84aa4ee006d9dbf6303f7b79e2a79490d76302479296"),
    ];

    [Fact]
    public void TheTableIsExactlyTheOneTheOldTokenizerLearned()
    {
        // The whole point of the transfer was that a chunk without the pair is
        // left alone instead of being copied. If that had changed which merges
        // are learned, or in which order, these recorded fingerprints would not
        // come back.
        foreach (var (vocab, tokenCount, stateSha256) in BeforeTheTransfer)
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train(Corpus, vocab);

            Assert.Equal(vocab, tokenizer.VocabSize);
            Assert.Equal(stateSha256, tokenizer.ComputeStateSha256());
            Assert.Equal(tokenCount, tokenizer.Encode(Corpus).Count);
        }
    }

    [Theory]
    [InlineData(260)]
    [InlineData(288)]
    [InlineData(320)]
    public void TheLearnedVocabularyAndMergesAreUnchanged(int vocabSize)
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(Corpus, vocabSize);

        // Training must still reach the requested size and still be able to
        // reproduce the text it learned from.
        Assert.Equal(vocabSize, tokenizer.VocabSize);
        Assert.Equal(Corpus, tokenizer.Decode(tokenizer.Encode(Corpus)));

        // And the learned merges must still be applied. Round-tripping alone
        // would survive a tokenizer that learned merges but stopped using
        // them, because plain bytes decode back perfectly well. Compared with
        // a vocabulary of exactly 256 — which learns nothing — a larger one
        // has to encode this corpus in strictly fewer tokens.
        var untrained = new BpeTokenizer();
        untrained.Train(Corpus, 256);
        var baseline = untrained.Encode(Corpus).Count;
        var tokens = tokenizer.Encode(Corpus).Count;
        Assert.True(tokens < baseline,
            $"ожидалось меньше токенов, чем без слияний: {tokens} против {baseline}");
    }

    [Fact]
    public void TwoTokenizersTrainedOnTheSameTextAgreeExactly()
    {
        // Training is deterministic, so the state must be identical down to
        // the saved bytes — that is what "the learned state is unchanged"
        // means in a way that cannot be argued with.
        var first = new BpeTokenizer();
        var second = new BpeTokenizer();
        first.Train(Corpus, 300);
        second.Train(Corpus, 300);

        Assert.Equal(first.ComputeStateSha256(), second.ComputeStateSha256());
        Assert.Equal(Encode(first, Corpus), Encode(second, Corpus));
    }

    [Fact]
    public void ASavedAndReloadedTokenizerStillProducesTheSameTokens()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(Corpus, 300);
        var before = Encode(tokenizer, Corpus);
        var hash = tokenizer.ComputeStateSha256();

        var path = Path.Combine(Path.GetTempPath(), "bpe-no-match-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            tokenizer.Save(path);
            var reloaded = BpeTokenizer.Load(path);
            Assert.Equal(hash, reloaded.ComputeStateSha256());
            Assert.Equal(before, Encode(reloaded, Corpus));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void TextWithNoRepeatedPairsIsEncodedByteForByte()
    {
        // The extreme case of the change: nothing ever merges, so every chunk
        // takes the no-copy path on every round. The answer must still be the
        // plain bytes.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abcdefg", 260);

        const string Plain = "xyz";
        var encoded = tokenizer.Encode(Plain);
        Assert.Equal(new List<int> { (int)'x', (int)'y', (int)'z' }, encoded);
        Assert.Equal(Plain, tokenizer.Decode(encoded));
    }

    [Fact]
    public void EncodingIsStableWhenTheSameTextIsEncodedTwice()
    {
        // If a chunk were returned and then written into by someone else, the
        // second pass over the same text would differ. It must not.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(Corpus, 300);

        var once = Encode(tokenizer, Corpus);
        var twice = Encode(tokenizer, Corpus);
        Assert.Equal(once, twice);

        // And a longer text that contains the corpus still decodes back.
        var mixed = Corpus + " and some unseen trailing words";
        Assert.Equal(mixed, tokenizer.Decode(tokenizer.Encode(mixed)));
    }

    [Fact]
    public void MergesStillApplyWhereThePairDoesOccur()
    {
        // The other half: where the pair is present it must still be merged,
        // otherwise "no copy" would have turned into "no merge". A repeated
        // two-letter piece has to end up as one token, not two.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(string.Concat(Enumerable.Repeat("ab ", 40)), 258);

        var encoded = tokenizer.Encode("ab");
        Assert.Single(encoded);
        Assert.True(encoded[0] >= 256, $"ожидался выученный токен, получено {encoded[0]}");
        Assert.Equal("ab", tokenizer.Decode(encoded));
    }

    // ── Измерение расхода памяти за обучение ────────────────────────────────
    //
    // Зачем этот тест есть. Проверки выше доказывают, что выученная таблица от
    // правки «кусок без пары не копируется» не изменилась. Они не отвечают на
    // ресурсный вопрос: сколько памяти эта правка экономит ЗА ОБУЧЕНИЕ сейчас,
    // когда одинаковые куски текста сливаются по одному представителю, а не по
    // каждому повторению. Этот тест экономию не утверждает — он измеряет
    // расход и печатает его.
    //
    // Как получается ответ. Один и тот же тест запускается дважды: на обычном
    // продуктовом коде и на коде, где возврат того же списка при отсутствии
    // пары временно заменён копией тех же элементов. Ответ — разность двух
    // напечатанных чисел. Утверждения в конце охраняют не экономию, а
    // равенство работы обоих плеч: при разных слияниях, словаре или чтении
    // сравнивать расход нельзя.
    //
    // Источник данных. Профиль документации по коду из сохранённого входа
    // опыта разбивки слов. Корпус сверяется отпечатком до обучения, поэтому
    // подмена данных не пройдёт незамеченной.
    //
    // Объявленный бюджет вызовов: одно содержательное обучение, четыре чтения
    // экзаменационных строк и один повтор первого чтения как проба стоимости
    // первой компиляции кода на ходу.

    // Пять учебных строк сохранённого входа, профиль документации по коду.
    private static readonly string[] SavedCodeProfileTrainStrings =
    [
        "int Add(int a, int b) => a + b; // return the sum",
        "int Subtract(int a, int b) => a - b; // return the difference",
        "int Multiply(int a, int b) => a * b; // return the product",
        "string Echo(string value) => value; // return the same text",
        "// identifiers: alpha beta gamma delta aaaa aaaaa aaaaaa",
    ];

    // Четыре экзаменационные строки: они только читаются, в обучение не идут.
    // Не-ASCII знаки последней строки записаны кодами, чтобы результат не
    // зависел от кодировки этого файла: U+03A9 — греческая омега, U+0451 — ё.
    private static readonly string[] SavedCodeProfileExamStrings =
    [
        "int Add(int left, int right) => left + right; // return the sum",
        "int Subtract(int left, int right) => left - right; // return the difference",
        "aaaa aaaaa aaaaaa",
        "// value: Ω + ё",
    ];

    // Сколько раз повторяется набор учебных строк при сборке корпуса.
    private const int SavedCodeProfileCorpusRepeatCount = 8;

    // Размер словаря, который запрашивает обучение.
    private const int SavedCodeProfileTargetVocabulary = 320;

    // Сколько слияний обучение обязано выучить на этом корпусе.
    private const int SavedCodeProfileLearnedMergeCount = 64;

    // Длина корпуса в байтах кодировки UTF-8 по сохранённому входу.
    private const int SavedCodeProfileTrainUtf8Bytes = 2303;

    // Отпечаток корпуса по сохранённому входу: самопроверка состава данных.
    private const string SavedCodeProfileTrainSha256 =
        "4525d08f34536fa86c97f0123ad1140585838496afba4f4a483bede8a863bccb";

    [Fact]
    public void TrainingTheSavedCodeProfileReportsHowManyBytesItAllocates()
    {
        // Корпус собирается за пределами окна измерения: его сборка к обучению
        // не относится и в расход обучения попадать не должна.
        var corpus = string.Join(
            "\n",
            Enumerable
                .Repeat(SavedCodeProfileTrainStrings, SavedCodeProfileCorpusRepeatCount)
                .SelectMany(trainStrings => trainStrings));

        // Байты корпуса в кодировке UTF-8 — то, по чему считается отпечаток.
        var corpusBytes = Encoding.UTF8.GetBytes(corpus);

        // Длина обязана совпасть с записанной: иначе собран другой текст.
        Assert.Equal(SavedCodeProfileTrainUtf8Bytes, corpusBytes.Length);

        // Отпечаток обязан совпасть с записанным: это и есть доказательство,
        // что обучение идёт на корпусе сохранённого опыта, а не на похожем.
        var corpusSha256 = Convert.ToHexString(SHA256.HashData(corpusBytes)).ToLowerInvariant();
        Assert.Equal(SavedCodeProfileTrainSha256, corpusSha256);

        // Новый словарь: ещё ничего не выучено.
        var tokenizer = new BpeTokenizer();

        // Размер словаря до обучения записывается для отчёта как наблюдение.
        var vocabularyBeforeTraining = tokenizer.VocabSize;

        // Приборы создаются заранее, чтобы их собственные выделения памяти не
        // попали внутрь измеряемого расхода обучения.
        var stopwatch = new Stopwatch();

        // Процесс нужен для показаний процессорного времени.
        var hostProcess = Process.GetCurrentProcess();

        // Процессорное время до обучения снимается снаружи окна выделений.
        var processorTimeBefore = hostProcess.TotalProcessorTime;

        // ── окно измерения открыто: внутри только обучение ──────────────────
        stopwatch.Start();
        var allocatedBeforeTraining = GC.GetAllocatedBytesForCurrentThread();
        var outcome = tokenizer.Train(corpus, SavedCodeProfileTargetVocabulary);
        var allocatedAfterTraining = GC.GetAllocatedBytesForCurrentThread();
        stopwatch.Stop();
        // ── окно измерения закрыто ──────────────────────────────────────────

        // Процессорное время после обучения.
        var processorTimeAfter = hostProcess.TotalProcessorTime;

        // Главное измеряемое число: сколько байт выделено за обучение.
        var allocatedDuringTraining = allocatedAfterTraining - allocatedBeforeTraining;

        // Проба стоимости первой компиляции на ходу, часть первая: это первое
        // на данном потоке чтение экзаменационной строки, поэтому здесь впервые
        // компилируются пути кодирования и декодирования.
        var probeBeforeFirstReadout = GC.GetAllocatedBytesForCurrentThread();
        var firstExamTokens = tokenizer.Encode(SavedCodeProfileExamStrings[0]);
        var firstExamReadout = tokenizer.Decode(firstExamTokens);
        var probeAfterFirstReadout = GC.GetAllocatedBytesForCurrentThread();

        // Часть вторая: тот же лёгкий путь на той же строке, но пути уже
        // скомпилированы. Если счётчик видит расходы первой компиляции, первое
        // число будет заметно больше второго; если не видит — числа совпадут.
        var probeBeforeRepeatReadout = GC.GetAllocatedBytesForCurrentThread();
        var repeatExamTokens = tokenizer.Encode(SavedCodeProfileExamStrings[0]);
        var repeatExamReadout = tokenizer.Decode(repeatExamTokens);
        var probeAfterRepeatReadout = GC.GetAllocatedBytesForCurrentThread();

        // Выделения первого и повторного чтения одного и того же текста.
        var firstReadoutAllocatedBytes = probeAfterFirstReadout - probeBeforeFirstReadout;
        var repeatReadoutAllocatedBytes = probeAfterRepeatReadout - probeBeforeRepeatReadout;

        // Токены и прочитанный обратно текст всех четырёх строк. Первая строка
        // уже прочитана пробой выше и повторно не кодируется.
        var examTokens = new List<int>[SavedCodeProfileExamStrings.Length];
        var examReadouts = new string[SavedCodeProfileExamStrings.Length];
        examTokens[0] = firstExamTokens;
        examReadouts[0] = firstExamReadout;

        // Остальные три строки читаются по одному разу каждая.
        for (int index = 1; index < SavedCodeProfileExamStrings.Length; index++)
        {
            examTokens[index] = tokenizer.Encode(SavedCodeProfileExamStrings[index]);
            examReadouts[index] = tokenizer.Decode(examTokens[index]);
        }

        // Сколько строк вернулось точно такими, какими были.
        var exactlyRestoredExamStrings = 0;
        for (int index = 0; index < SavedCodeProfileExamStrings.Length; index++)
        {
            if (examReadouts[index] == SavedCodeProfileExamStrings[index])
                exactlyRestoredExamStrings++;
        }

        // Отпечаток обученного состояния: им сверяется равенство плеч целиком,
        // а не только по числу слияний и размеру словаря.
        var trainedStateSha256 = tokenizer.ComputeStateSha256();

        // ── печать измерения: эти строки и есть сохраняемый результат ────────
        _output.WriteLine("Профиль данных: документация по коду из сохранённого входа опыта разбивки слов.");
        _output.WriteLine($"Корпус: {corpusBytes.Length} байт в кодировке UTF-8, отпечаток {corpusSha256}.");
        _output.WriteLine($"Повторов учебного набора: {SavedCodeProfileCorpusRepeatCount}; запрошенный словарь: {SavedCodeProfileTargetVocabulary}.");
        _output.WriteLine($"Граница измерения, счётчик выделений до обучения: {allocatedBeforeTraining}");
        _output.WriteLine($"Граница измерения, счётчик выделений после обучения: {allocatedAfterTraining}");
        _output.WriteLine($"ВЫДЕЛЕНО БАЙТ ЗА ОБУЧЕНИЕ: {allocatedDuringTraining}");
        _output.WriteLine($"Время обучения по часам: {stopwatch.Elapsed.TotalMilliseconds:F4} мс.");
        _output.WriteLine($"Процессорное время процесса за обучение: {(processorTimeAfter - processorTimeBefore).TotalMilliseconds:F4} мс.");
        _output.WriteLine($"Словарь нового токенизатора до обучения: {vocabularyBeforeTraining}; после обучения: {tokenizer.VocabSize}.");
        _output.WriteLine($"Запрошено {outcome.RequestedVocabSize}, достигнуто {outcome.AchievedVocabSize}, выучено слияний {outcome.LearnedMergeCount}, причина выхода {outcome.ExitReason}.");
        _output.WriteLine($"Точно прочитано экзаменационных строк: {exactlyRestoredExamStrings} из {SavedCodeProfileExamStrings.Length}.");
        _output.WriteLine($"Отпечаток обученного состояния: {trainedStateSha256}");
        _output.WriteLine($"Проба первой компиляции на ходу: первое чтение {firstReadoutAllocatedBytes} байт, повтор того же чтения {repeatReadoutAllocatedBytes} байт.");
        _output.WriteLine($"Токены экзаменационных строк: {JsonSerializer.Serialize(examTokens)}");

        // Охрана нулевого счётчика: ноль означает, что измерение
        // недействительно, а не что экономия бесконечна.
        Assert.True(
            allocatedDuringTraining > 0,
            $"счётчик выделений показал {allocatedDuringTraining} байт за обучение: измерение недействительно");

        // Дальше — охрана равенства работы обоих плеч. Расход сравнивать можно
        // только если оба плеча выучили одно и то же.
        Assert.Equal(SavedCodeProfileTargetVocabulary, outcome.RequestedVocabSize);
        Assert.Equal(SavedCodeProfileTargetVocabulary, outcome.AchievedVocabSize);
        Assert.Equal(SavedCodeProfileTargetVocabulary, tokenizer.VocabSize);
        Assert.Equal(SavedCodeProfileLearnedMergeCount, outcome.LearnedMergeCount);
        Assert.Equal(SavedCodeProfileExamStrings.Length, exactlyRestoredExamStrings);

        // Повторное чтение той же строки обязано дать тот же текст: иначе
        // проба первой компиляции сравнивала бы разные работы.
        Assert.Equal(firstExamReadout, repeatExamReadout);
    }

    private static string Encode(BpeTokenizer tokenizer, string text)
        => JsonSerializer.Serialize(tokenizer.Encode(text));
}
