// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using Tokenizer;

// FIX-BW transfer, plan 31-bpe-weighted-chunks.md, from BW01.
//
// Ordinary text repeats the same word again and again. Every repetition used
// to be carried as its own list and merged on its own, round after round,
// although every copy of a word goes through exactly the same merges and
// contributes exactly the same pairs. One representative per distinct chunk is
// kept instead, carrying how many times it occurred.
//
// The claim that must hold is not "it is faster" but "it learns the same
// vocabulary". A weighting that changed which pair wins a round would change
// the tokenizer silently, and every model trained after it. That is what the
// tests below are for: the learned state has to be identical to the state
// learned from text where nothing repeats.
public sealed class BpeWeightedChunkTests
{
    // A block of five lines; the corpus repeats the whole block, which is the
    // shape the experiment measured.
    private static readonly string[] Block =
    [
        "the service validates the request before writing",
        "the service writes the record after validating",
        "a reader loads the record and returns the answer",
        "the writer persists the record and reports success",
        "a validator checks the request and reports failure"
    ];

    private static string Repeated(int times)
        => string.Join("\n", Enumerable.Range(0, times).SelectMany(_ => Block));

    // Repair R4 of the 27.10 transfer audit, BW row. The cases below compare two
    // runs of today's code against each other, which cannot tell whether the
    // merge order drifted: both arms would drift together. These three values
    // come from the tokenizer as it stood BEFORE this transfer — the file at
    // commit 046e0ded876, the last one before FIX-BPE and FIX-BW — compiled and
    // run on these very texts outside the project.
    private static readonly (int Times, int Vocab, int TokenCount, string StateSha256)[] BeforeTheTransfer =
    [
        (1, 300, 91, "1a6d29ae421f51d98f4889762b6e1c23dd6fa0dbad0620a6aedf0c58fe98a9e1"),
        (8, 300, 91, "1a6d29ae421f51d98f4889762b6e1c23dd6fa0dbad0620a6aedf0c58fe98a9e1"),
        (8, 320, 71, "5aaa0adde49387579f1eb1bc9efc3906f16e8833f22bc04b9836e6171d452a38"),
    ];

    [Fact]
    public void TheTableIsExactlyTheOneTheOldTokenizerLearned()
    {
        // Weighted representatives were supposed to change only how the count
        // is reached, never the count itself and never the order of merges.
        // These recorded fingerprints are what that promise means in numbers.
        foreach (var (times, vocab, tokenCount, stateSha256) in BeforeTheTransfer)
        {
            var tokenizer = new BpeTokenizer();
            tokenizer.Train(Repeated(times), vocab);

            Assert.Equal(vocab, tokenizer.VocabSize);
            Assert.Equal(stateSha256, tokenizer.ComputeStateSha256());
            Assert.Equal(tokenCount, tokenizer.Encode(Repeated(1)).Count);
        }

        // Recorded as it was found: the old tokenizer already produced the same
        // state for one block and for eight, so this is a property the transfer
        // preserved rather than one it created.
        Assert.Equal(BeforeTheTransfer[0].StateSha256, BeforeTheTransfer[1].StateSha256);
    }

    [Fact]
    public void RepeatingTheTextDoesNotChangeWhatIsLearned()
    {
        // Eight copies of the same block contain the same distinct words in the
        // same first-occurrence order as one copy. Weighted counting multiplies
        // every pair's count by eight, which cannot change which pair is
        // largest, so the learned state must be identical.
        var once = new BpeTokenizer();
        var eightTimes = new BpeTokenizer();
        once.Train(Repeated(1), 320);
        eightTimes.Train(Repeated(8), 320);

        Assert.Equal(once.ComputeStateSha256(), eightTimes.ComputeStateSha256());
        Assert.Equal(320, eightTimes.VocabSize);
    }

    [Theory]
    [InlineData(258)]
    [InlineData(280)]
    [InlineData(320)]
    public void TheSameTokensComeOutAtEveryVocabularySize(int vocabSize)
    {
        var once = new BpeTokenizer();
        var eightTimes = new BpeTokenizer();
        once.Train(Repeated(1), vocabSize);
        eightTimes.Train(Repeated(8), vocabSize);

        var probe = "the service writes the record and reports success";
        Assert.Equal(once.Encode(probe), eightTimes.Encode(probe));
        Assert.Equal(probe, eightTimes.Decode(eightTimes.Encode(probe)));
    }

    [Fact]
    public void AWordThatOccursMoreOftenStillWinsTheRound()
    {
        // The weight has to be carried, not dropped, and this is the case that
        // proves it. The rare word comes first, so its pair is the first one
        // recorded; the frequent word comes later but occurs five times. With
        // the weight, the frequent word's pair wins the only merge. Without it
        // every distinct chunk would count once, the two would tie, and the
        // tie would fall to the pair recorded first — the rare one.
        //
        // A corpus where the frequent word also came first would pass either
        // way, which is exactly the blind spot this replaces.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("ab cd cd cd cd cd", 257);

        Assert.Equal(257, tokenizer.VocabSize);

        // Exactly one merge exists and it did not go to the rare word.
        Assert.Equal(2, tokenizer.Encode("ab").Count);
        Assert.Equal("ab", tokenizer.Decode(tokenizer.Encode("ab")));
    }

    [Fact]
    public void EquallyFrequentPairsAreStillDecidedByWhichAppearedFirst()
    {
        // Two distinct words, one occurrence each: every pair has the same
        // count, so the round is decided purely by which pair was recorded
        // first. Representatives are therefore held in order of first
        // occurrence, and the first word must win the only merge.
        //
        // Without that ordering the same corpus would learn a different
        // vocabulary — silently, and differently on every run if the order
        // were arbitrary.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("ab cd", 257);

        Assert.Equal(257, tokenizer.VocabSize);
        Assert.Single(tokenizer.Encode("ab"));
        Assert.Equal("ab", tokenizer.Decode(tokenizer.Encode("ab")));
    }

    [Fact]
    public void OverlappingPairsInsideOneChunkAreStillCounted()
    {
        // A chunk like "aaaa" contains overlapping pairs. Counting per distinct
        // chunk instead of per adjacent position would lose them.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("aaaa aaaa bc", 257);

        Assert.Equal("aaaa", tokenizer.Decode(tokenizer.Encode("aaaa")));
        Assert.True(tokenizer.Encode("aaaa").Count < 4,
            "ожидалось слияние внутри повторяющегося фрагмента");
    }

    [Fact]
    public void TextWhereNothingRepeatsIsUnaffected()
    {
        // With every chunk distinct there is nothing to group, so this is the
        // case where the change must do exactly nothing.
        const string Unique = "alpha beta gamma delta epsilon zeta eta theta";
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(Unique, 300);

        // Training stops early when no pair is left to merge — long-standing
        // behaviour this change does not touch — so the vocabulary reaches 286
        // here rather than the requested 300. Pinned at the observed value so
        // a future change to that rule is noticed rather than absorbed.
        Assert.Equal(286, tokenizer.VocabSize);
        Assert.Equal(Unique, tokenizer.Decode(tokenizer.Encode(Unique)));

        // With every chunk distinct there is nothing to group, so grouping
        // must be a no-op: the same text trained twice gives the same state.
        var again = new BpeTokenizer();
        again.Train(Unique, 300);
        Assert.Equal(tokenizer.ComputeStateSha256(), again.ComputeStateSha256());
    }

    [Fact]
    public void CaseAndSpellingAreNotFoldedTogether()
    {
        // Only byte-identical strings may share a representative. If the
        // grouping normalised case, these two words would be merged into one
        // and the learned vocabulary would differ.
        var folded = new BpeTokenizer();
        folded.Train("Word word Word word Word word", 258);

        // Both spellings must still round-trip exactly as written.
        Assert.Equal("Word", folded.Decode(folded.Encode("Word")));
        Assert.Equal("word", folded.Decode(folded.Encode("word")));
        Assert.NotEqual(folded.Encode("Word"), folded.Encode("word"));
    }

    [Fact]
    public void SaveAndLoadStillReproduceTheSameTokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(Repeated(8), 320);
        var probe = Repeated(1);
        var before = tokenizer.Encode(probe);

        var path = Path.Combine(Path.GetTempPath(), "bpe-weighted-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            tokenizer.Save(path);
            var reloaded = BpeTokenizer.Load(path);
            Assert.Equal(tokenizer.ComputeStateSha256(), reloaded.ComputeStateSha256());
            Assert.Equal(before, reloaded.Encode(probe));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void TrainingTheRepeatedTextDoesNotCostEightTimesTheUniqueOne()
    {
        // Repair R4 of the 27.10 transfer audit, BW row: the resource profile
        // the plan asks for, measured here rather than quoted from the
        // experiment. Two profiles, the same block once and eight times.
        //
        // Weighted representatives are exactly the claim that repetition stops
        // costing: the eight-fold text carries the same distinct pieces, each
        // with a count, so training it must not cost eight times the single
        // block. The plan also says that if the gain is not there on the shared
        // base, that is to be recorded rather than chased — so what follows is
        // a measurement with a bound, not a demanded percentage.
        static long Cost(string text)
        {
            var warm = new BpeTokenizer();
            warm.Train(text, 300);
            var tokenizer = new BpeTokenizer();
            var before = GC.GetAllocatedBytesForCurrentThread();
            tokenizer.Train(text, 300);
            return GC.GetAllocatedBytesForCurrentThread() - before;
        }

        var once = Cost(Repeated(1));
        var eight = Cost(Repeated(8));

        Assert.True(once > 0 && eight > 0, $"один {once}, восемь {eight}");
        // Measured on this base: the eight-fold text costs about 1.30 of the
        // single block today, and about 1.83 when representatives are dropped
        // and every occurrence is paid for again. The bound sits between the
        // two measurements, with room on both sides, so it states the property
        // rather than a machine's exact byte count.
        Assert.True(eight < once * 1.5,
            $"обучение восьмикратного текста стоило {eight} байт против {once} за однократный, " +
            $"отношение {(double)eight / once:F3}: похоже, повторы снова оплачиваются каждый отдельно");
    }
}
