// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

// Repair R4 of the 27.10 transfer audit, row "reuse of the query's tag pairs"
// (plan 51-pos-query-bigram-reuse.md, experiment PQ01).
//
// The transfer's own tests prove the arithmetic and prove that the comparison
// can no longer be handed the raw query. What the audit found missing is the
// panel the experiment actually measured: the fixed 128/32 split, the answer
// for every single exam row, and the order of the learned memory.
//
// The oracle here is not this test's own opinion. It is the experiment's
// recorded answer sheet, written by the experiment host at the time of the
// run and pinned by hash below. If the product's answers and that sheet ever
// disagree, one of the two moved, and the test says which row.
public sealed class PosQueryBigramPanelTests
{
    // The two artefacts of attempt 0001, pinned by their bytes. A replaced
    // file is a different panel, and the test stops rather than quietly
    // learning from other rows.
    private const string FixtureSha256 =
        "a62864516bc053f1d5a99f46dc9f913b3c13b776d9fe1fec02e70538b2a4027c";
    private const string AnswerSheetSha256 =
        "44290c173e4861b2fcfa58d4926c08bc3850542fef0c253e0156fe7d029338ed";

    private sealed record Row(int[] PosSeq, int Pattern);
    private sealed record Fixture(int Seed, int PerPattern, int Split, Row[] Rows);

    // One line of the recorded answer sheet. "before" is the untrained
    // reading, "after" is the reading once the 128 lessons are learned; A and
    // B are the two arms the experiment compared.
    private sealed class RecordedRow
    {
        [JsonPropertyName("index")] public int Index { get; init; }
        [JsonPropertyName("pos_sequence")] public int[] PosSequence { get; init; } = [];
        [JsonPropertyName("gold")] public int Gold { get; init; }
        [JsonPropertyName("a_before")] public int ABefore { get; init; }
        [JsonPropertyName("a_after")] public int AAfter { get; init; }
        [JsonPropertyName("b_before")] public int BBefore { get; init; }
        [JsonPropertyName("b_after")] public int BAfter { get; init; }
        [JsonPropertyName("answers_match")] public bool AnswersMatch { get; init; }
    }

    private static string AttemptDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine(directory!.FullName,
            "experiments-v2", "RP01-MICRO", "PQ01", "results", "attempt-0001");
    }

    private static byte[] Pinned(string fileName, string expectedSha256)
    {
        var path = Path.Combine(AttemptDirectory(), fileName);
        Assert.True(File.Exists(path), $"Закреплённый планом файл не найден: {path}");

        var bytes = File.ReadAllBytes(path);
        Assert.Equal(expectedSha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        return bytes;
    }

    private static Fixture LoadFixture()
    {
        var fixture = JsonSerializer.Deserialize<Fixture>(Pinned("fixture.json", FixtureSha256),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(fixture);
        Assert.Equal(160, fixture!.Rows.Length);
        Assert.Equal(128, fixture.Split);
        return fixture;
    }

    private static RecordedRow[] LoadAnswerSheet()
    {
        var sheet = JsonSerializer.Deserialize<RecordedRow[]>(
            Pinned("per-row.json", AnswerSheetSha256), new JsonSerializerOptions());
        Assert.NotNull(sheet);
        Assert.Equal(32, sheet!.Length);
        return sheet;
    }

    [Fact]
    public void TheRecordedSheetDescribesTheExamHalfOfThePinnedPanel()
    {
        // Before the sheet can judge anything, it must be the sheet for this
        // panel: its 32 lines must be the 32 rows after the split, in order,
        // with the same tags and the same right answer.
        var fixture = LoadFixture();
        var sheet = LoadAnswerSheet();
        var exam = fixture.Rows.Skip(fixture.Split).ToArray();
        Assert.Equal(32, exam.Length);

        for (var line = 0; line < sheet.Length; line++)
        {
            Assert.Equal(line, sheet[line].Index);
            Assert.Equal(exam[line].PosSeq, sheet[line].PosSequence);
            Assert.Equal(exam[line].Pattern, sheet[line].Gold);
        }
    }

    [Fact]
    public void ThePanelIsAnsweredRowByRowExactlyAsTheExperimentRecordedIt()
    {
        var fixture = LoadFixture();
        var sheet = LoadAnswerSheet();
        var train = fixture.Rows.Take(fixture.Split).ToArray();
        var exam = fixture.Rows.Skip(fixture.Split).ToArray();

        // Untrained: an empty memory falls back to the rule reading. This is
        // the "before" column of the sheet.
        var classifier = new SemanticCompiler.PatternClassifier(k: 3);
        for (var line = 0; line < exam.Length; line++)
            Assert.Equal(sheet[line].ABefore, classifier.Predict(exam[line].PosSeq));

        classifier.Train([.. train.Select(row => (row.PosSeq, row.Pattern))]);

        // Trained: every row, not a total. A matching total can hide two rows
        // that swapped places, which is exactly the drift a reused query set
        // could cause.
        for (var line = 0; line < exam.Length; line++)
            Assert.Equal(sheet[line].AAfter, classifier.Predict(exam[line].PosSeq));

        // The experiment ran two arms and recorded that they answered alike.
        // That recorded agreement is the claim the transfer rests on, so the
        // other arm's column is checked against the same product reading.
        Assert.All(sheet, line => Assert.True(line.AnswersMatch, $"строка {line.Index}: ответы вариантов разошлись"));
        for (var line = 0; line < exam.Length; line++)
            Assert.Equal(sheet[line].BAfter, classifier.Predict(exam[line].PosSeq));

        // The two headline numbers of the panel, recomputed from the product's
        // own answers rather than copied from the report: 11 of 32 right
        // before the lessons, 28 of 32 after them.
        var rightBefore = sheet.Count(line => line.ABefore == line.Gold);
        var rightAfter = Enumerable.Range(0, exam.Length)
            .Count(line => classifier.Predict(exam[line].PosSeq) == exam[line].Pattern);
        Assert.Equal(11, rightBefore);
        Assert.Equal(28, rightAfter);

        // And the four rows that stay wrong stay named, rather than rounded
        // away into a pass. They are the panel's own remaining weakness.
        var stillWrong = Enumerable.Range(0, exam.Length)
            .Where(line => classifier.Predict(exam[line].PosSeq) != exam[line].Pattern)
            .ToArray();
        Assert.Equal<int[]>([11, 21, 24, 30], stillWrong);
    }

    [Fact]
    public void TheLearnedMemoryHoldsTheTrainingHalfInTheFixtureOrder()
    {
        // The order of the remembered rows is part of the state: the nearest-k
        // vote settles ties by it. So the memory must be the first 128 rows of
        // the panel, in the panel's own order, and nothing else.
        var fixture = LoadFixture();
        var train = fixture.Rows.Take(fixture.Split).ToArray();

        var classifier = new SemanticCompiler.PatternClassifier(k: 3);
        classifier.Train([.. train.Select(row => (row.PosSeq, row.Pattern))]);

        var field = typeof(SemanticCompiler.PatternClassifier).GetField("_train",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.True(field is not null, "не найдено поле памяти _train");
        var remembered = (List<(int[] PosSeq, int Pattern)>)field!.GetValue(classifier)!;

        Assert.Equal(128, remembered.Count);
        for (var index = 0; index < remembered.Count; index++)
        {
            Assert.Equal(train[index].Pattern, remembered[index].Pattern);
            Assert.Equal(train[index].PosSeq, remembered[index].PosSeq);
        }

        // Learning the panel a second time must not stack a second copy on top
        // of the first: a memory that grew would change every later vote.
        classifier.Train([.. train.Select(row => (row.PosSeq, row.Pattern))]);
        Assert.Equal(128, ((List<(int[] PosSeq, int Pattern)>)field.GetValue(classifier)!).Count);
    }
}
