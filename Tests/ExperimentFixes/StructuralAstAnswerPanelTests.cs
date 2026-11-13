// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using SemanticCompiler.Prediction;

// Repair R4 of the 27.10 transfer audit, row "memory of the facade across
// several files" (experiment AST-SIB-CACHE-01).
//
// The transfer added a per-file cache of a node's siblings. Its own tests show
// that one training keeps every file in one memory, and two measured resource
// profiles show what the cache saves. The part the audit still called open is
// the plain one: the answers themselves, before and after training, against a
// gold the test did not invent.
//
// That gold exists. The experiment wrote down, for each of its sixteen exam
// questions, the right answer and what the model replied before and after the
// lessons. Both the teaching material and those answer sheets live in the
// repository, so the whole exam can be sat again here with the product's own
// code and compared question by question.
public sealed class StructuralAstAnswerPanelTests
{
    // The teaching material and the exam, pinned by their bytes exactly as the
    // experiment's own manifest pins them.
    private const string NarrowTrainSha256 =
        "db4db01555a16f7b9957fe8bf5b6be10ad1cb0462c1e50c00f5695a1286295f3";
    private const string WideTrainSha256 =
        "c97163b805035ec8dcbe7630e19faa37e6147cd3612130cfa8f8d8145af1e7ad";
    private const string HeldOutSha256 =
        "4618aeb927eae84b5e5eed24f602ee5de9e92b329c53dc95cbac4aeafd4334d8";

    // The recorded answer sheets. The experiment ran two arms: the original
    // code and the code with the cache. The product carries the cache, so its
    // sheet is the one to match — and the experiment recorded that both arms
    // answered alike, which is checked here too rather than taken on trust.
    private const string NarrowOriginalSheetSha256 =
        "9d92573e26cd3e5e8e365d21c6b7e74979cb237fe674c5a953d2d427ca082b49";
    private const string NarrowCachedSheetSha256 =
        "5e063ab28c85dbfee1cb59082eeebcdd61cd8fe8e1ffd739bb445586128285d1";
    private const string WideOriginalSheetSha256 =
        "5b81345c663bcf48dcca139ceb6335a9dd5f8077ffd05a9c59641bf7144784cd";
    private const string WideCachedSheetSha256 =
        "3fbe95f95440af4c162c412dfd8b13c0635fe645145d9e992f403d0ce8dab8c1";

    private sealed class RecordedQuery
    {
        [JsonPropertyName("index")] public int Index { get; init; }
        [JsonPropertyName("queryKind")] public string QueryKind { get; init; } = "";
        [JsonPropertyName("goldCompactId")] public int GoldCompactId { get; init; }
        [JsonPropertyName("goldSyntaxKind")] public string GoldSyntaxKind { get; init; } = "";
        [JsonPropertyName("predictedBefore")] public int PredictedBefore { get; init; }
        [JsonPropertyName("predictedAfter")] public int PredictedAfter { get; init; }
    }

    private sealed class RecordedSheet
    {
        [JsonPropertyName("arm")] public string Arm { get; init; } = "";
        [JsonPropertyName("profile")] public string Profile { get; init; } = "";
        [JsonPropertyName("queries")] public RecordedQuery[] Queries { get; init; } = [];
    }

    private static string ExperimentDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine(directory!.FullName, "experiments-v2", "RP01-MICRO", "AST01");
    }

    private static byte[] Pinned(string relativePath, string expectedSha256)
    {
        var path = Path.Combine(ExperimentDirectory(), relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path), $"Закреплённый файл опыта не найден: {path}");

        var bytes = File.ReadAllBytes(path);
        Assert.Equal(expectedSha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        return bytes;
    }

    private static string PinnedText(string relativePath, string expectedSha256)
        => System.Text.Encoding.UTF8.GetString(Pinned(relativePath, expectedSha256)).TrimStart('﻿');

    private static RecordedSheet Sheet(string relativePath, string expectedSha256)
    {
        var sheet = JsonSerializer.Deserialize<RecordedSheet>(Pinned(relativePath, expectedSha256),
            new JsonSerializerOptions());
        Assert.NotNull(sheet);
        Assert.Equal(16, sheet!.Queries.Length);
        return sheet;
    }

    // The exam, built exactly as the experiment built it: sixteen nodes of the
    // held-out file, each asked "what kind of node comes next", with the real
    // next node as the right answer.
    private static (Microsoft.CodeAnalysis.SyntaxNode Node, int Gold, string GoldKind)[] Exam()
    {
        var examNodes = CSharpSyntaxTree.ParseText(PinnedText("data/heldout.cs", HeldOutSha256))
            .GetRoot().DescendantNodes().ToArray();
        Assert.True(examNodes.Length >= 17, $"экзамен слишком мал: {examNodes.Length}");

        return [.. Enumerable.Range(0, 16).Select(i =>
            (examNodes[i],
             (int)StructuralVectorExtractor.NodeTypeId(examNodes[i + 1]),
             examNodes[i + 1].Kind().ToString()))];
    }

    public static TheoryData<string, string, string, string, string, double> Profiles() => new()
    {
        { "narrow", "data/train-narrow.cs", NarrowTrainSha256,
          "results/attempt-0001/nB/predictions.json", NarrowCachedSheetSha256, 0.75 },
        { "wide", "data/train-wide.cs", WideTrainSha256,
          "results/attempt-0001/wB/predictions.json", WideCachedSheetSha256, 0.4375 },
    };

    [Theory]
    [MemberData(nameof(Profiles))]
    public void TheSixteenAnswersBeforeAndAfterTrainingMatchTheRecordedSheet(
        string profile, string trainPath, string trainSha256,
        string sheetPath, string sheetSha256, double recordedAccuracyAfter)
    {
        var exam = Exam();
        var sheet = Sheet(sheetPath, sheetSha256);
        Assert.Equal(profile, sheet.Profile);

        // The exam the sheet was written for must be the exam being sat now:
        // same question, same right answer, in the same order.
        for (var question = 0; question < exam.Length; question++)
        {
            Assert.Equal(question, sheet.Queries[question].Index);
            Assert.Equal(exam[question].GoldKind, sheet.Queries[question].GoldSyntaxKind);
            Assert.Equal(exam[question].Gold, sheet.Queries[question].GoldCompactId);
        }

        var predictor = new StructuralKnnPredictor();
        Assert.Equal(0, predictor.TrainingSize);

        // Before the lessons: every answer, not a total.
        for (var question = 0; question < exam.Length; question++)
            Assert.Equal(sheet.Queries[question].PredictedBefore,
                predictor.PredictFromNode(exam[question].Node, 0));

        predictor.TrainFromSource(PinnedText(trainPath, trainSha256), 0);
        Assert.True(predictor.TrainingSize > 0, "после обучения память осталась пустой");

        // After the lessons: again every answer. A matching score with two
        // answers swapped would pass a total and fail here.
        for (var question = 0; question < exam.Length; question++)
            Assert.Equal(sheet.Queries[question].PredictedAfter,
                predictor.PredictFromNode(exam[question].Node, 0));

        // And the two numbers the experiment reported, recomputed from the
        // product's own answers rather than copied from the report: nothing
        // right before the lessons, and the recorded share right after them.
        var rightBefore = Enumerable.Range(0, exam.Length)
            .Count(question => sheet.Queries[question].PredictedBefore == exam[question].Gold);
        var rightAfter = Enumerable.Range(0, exam.Length)
            .Count(question => predictor.PredictFromNode(exam[question].Node, 0) == exam[question].Gold);

        Assert.Equal(0, rightBefore);
        Assert.Equal(recordedAccuracyAfter, rightAfter / 16.0);
    }

    [Theory]
    [InlineData("narrow", "results/attempt-0001/nA/predictions.json", NarrowOriginalSheetSha256,
                "results/attempt-0001/nB/predictions.json", NarrowCachedSheetSha256)]
    [InlineData("wide", "results/attempt-0001/wA/predictions.json", WideOriginalSheetSha256,
                "results/attempt-0001/wB/predictions.json", WideCachedSheetSha256)]
    public void TheCacheChangedNoAnswer_AsBothRecordedSheetsAgree(
        string profile, string originalPath, string originalSha256, string cachedPath, string cachedSha256)
    {
        // The claim the whole transfer rests on: the cache is a saving, not a
        // different model. The experiment recorded both arms' answers; they
        // must agree with each other and with the product as it stands.
        var original = Sheet(originalPath, originalSha256);
        var cached = Sheet(cachedPath, cachedSha256);
        Assert.Equal("A", original.Arm);
        Assert.Equal("B", cached.Arm);
        Assert.Equal(profile, original.Profile);

        for (var question = 0; question < original.Queries.Length; question++)
        {
            Assert.Equal(original.Queries[question].GoldCompactId, cached.Queries[question].GoldCompactId);
            Assert.Equal(original.Queries[question].PredictedBefore, cached.Queries[question].PredictedBefore);
            Assert.Equal(original.Queries[question].PredictedAfter, cached.Queries[question].PredictedAfter);
        }
    }
}
