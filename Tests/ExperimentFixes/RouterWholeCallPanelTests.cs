// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using TrainableRouter;
using TrainableRouter.Contracts;
using TrainableRouter.Training;

// Repair R4 of the 27.10 transfer audit, row "reading the router's learned
// keys" (experiment KN-ROUTER-LEARNED-KEY-01).
//
// What the transfer's own tests already prove: the ordinary spellings of the
// technologies reach the keys the lessons wrote, and the profiles are written
// by the real writer rather than by hand.
//
// What the audit still called open, and what is closed here:
//   - the fingerprint and the provenance of the pinned learned memory, rather
//     than a payload quoted in a comment;
//   - a call of the whole router, not only of the part that scores candidates.
//
// The router is therefore driven end to end exactly as the experiment drove
// it, and every decision is compared with the decision the experiment
// recorded at the time: the chosen route, whether the router refused, the
// confidence, and each ranked candidate including the authority that produced
// it.
public sealed class RouterWholeCallPanelTests
{
    // The recorded run of the arm that carries the transfer. Pinning it by its
    // bytes is itself the fingerprint the audit asked for: a replaced file is
    // a different run and the test stops instead of agreeing with it.
    private const string RecordedRunSha256 =
        "5f9c16b06b5a27b1a43aed8710b371465e41d0b7cb89688fbe4d2de061ca6a5f";

    // The provenance every learned profile must carry: the card the lessons
    // came from. A profile without it is memory of unknown origin.
    private const string ExpectedProvenance = "KN-ROUTER-LEARNED-KEY-01";

    private static readonly string[] Candidates = ["code", "docs"];

    private sealed class RecordedCandidate
    {
        [JsonPropertyName("UnitId")] public string UnitId { get; init; } = "";
        [JsonPropertyName("UnitRole")] public string UnitRole { get; init; } = "";
        [JsonPropertyName("DomainTag")] public string DomainTag { get; init; } = "";
        [JsonPropertyName("Score")] public double Score { get; init; }
        [JsonPropertyName("Confidence")] public double Confidence { get; init; }
        [JsonPropertyName("AuthorityRole")] public string AuthorityRole { get; init; } = "";
    }

    private sealed class RecordedDecision
    {
        [JsonPropertyName("query")] public string Query { get; init; } = "";
        [JsonPropertyName("gold")] public string Gold { get; init; } = "";
        [JsonPropertyName("actual")] public string? Actual { get; init; }
        [JsonPropertyName("KnowledgeEdge")] public bool KnowledgeEdge { get; init; }
        [JsonPropertyName("Confidence")] public double Confidence { get; init; }
        [JsonPropertyName("correct")] public bool Correct { get; init; }
        [JsonPropertyName("RankedCandidates")] public RecordedCandidate[] RankedCandidates { get; init; } = [];
    }

    private sealed class RecordedProfile
    {
        [JsonPropertyName("UnitId")] public string UnitId { get; init; } = "";
        [JsonPropertyName("UnitRole")] public string UnitRole { get; init; } = "";
        [JsonPropertyName("DomainTag")] public string DomainTag { get; init; } = "";
        [JsonPropertyName("Keywords")] public string[] Keywords { get; init; } = [];
        [JsonPropertyName("AllowedInputs")] public string[] AllowedInputs { get; init; } = [];
        [JsonPropertyName("ConfidenceFloor")] public double ConfidenceFloor { get; init; }
        [JsonPropertyName("MetadataSourceRef")] public string MetadataSourceRef { get; init; } = "";
        [JsonPropertyName("BaseScore")] public double BaseScore { get; init; }
    }

    private sealed class RecordedRun
    {
        [JsonPropertyName("arm")] public string Arm { get; init; } = "";
        [JsonPropertyName("before")] public RecordedDecision[] Before { get; init; } = [];
        [JsonPropertyName("after")] public RecordedDecision[] After { get; init; } = [];
        [JsonPropertyName("profiles")] public Dictionary<string, RecordedProfile> Profiles { get; init; } = [];
        [JsonPropertyName("profileUtf8Bytes")] public int ProfileUtf8Bytes { get; init; }
        [JsonPropertyName("trainRows")] public int TrainRows { get; init; }
        [JsonPropertyName("routeCalls")] public int RouteCalls { get; init; }
    }

    // The log is outside this experiment, exactly as the experiment declared:
    // if the router ever tried to write a decision here, that would be a new
    // behaviour and the test must say so loudly rather than swallow it.
    private sealed class RefusingLog : IRoutingDecisionLog
    {
        public Task<OperationResult<bool>> AppendAsync(RoutingDecisionRecord decision, CancellationToken ct = default)
            => throw new InvalidOperationException("запись решения не входит в этот опыт");

        public Task<OperationResult<IReadOnlyList<RoutingDecisionRecord>>> ListAsync(CancellationToken ct = default)
            => throw new InvalidOperationException("перечисление решений не входит в этот опыт");
    }

    private static RecordedRun Recorded()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        var path = Path.Combine(directory!.FullName,
            "experiments-v2", "RP01-MICRO", "KR01", "results", "B.json");
        Assert.True(File.Exists(path), $"Закреплённый прогон опыта не найден: {path}");

        var bytes = File.ReadAllBytes(path);
        Assert.Equal(RecordedRunSha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());

        var run = JsonSerializer.Deserialize<RecordedRun>(bytes, new JsonSerializerOptions());
        Assert.NotNull(run);
        Assert.Equal("B", run!.Arm);
        return run;
    }

    // The two lessons and the two unit descriptions, exactly as the experiment
    // declared them.
    private static RoutingUnitMetadataRegistry Registry() => new(
    [
        new("code", "executor", "programming", ["text"], 0.20, ExpectedProvenance),
        new("docs", "executor", "documentation", ["text"], 0.20, ExpectedProvenance),
    ]);

    private static RoutingTrainingExample[] Lessons() =>
    [
        new("T1", "C# .NET", "", null, null, Candidates, "code", "docs", "code lesson", "wrong unit"),
        new("T2", "manual guide", "", null, null, Candidates, "docs", "code", "docs lesson", "wrong unit"),
    ];

    private static IReadOnlyDictionary<string, UnitScoringProfile> TrainedMemory()
    {
        var registry = Registry();
        var trained = new RoutingProfileTrainer(registry).Train(Lessons(), registry);
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        return trained.Value!;
    }

    // One pass over the exam through the WHOLE router: the flow itself, not the
    // scorer underneath it.
    private static async Task<RoutingDecision[]> RouteWholePanel(
        IReadOnlyDictionary<string, UnitScoringProfile> memory, RecordedDecision[] sheet)
    {
        var flow = new TrainableRoutingFlow(new BaselineKeywordCandidateGenerator(memory), new RefusingLog());
        var decisions = new List<RoutingDecision>();
        for (var question = 0; question < sheet.Length; question++)
        {
            var result = await flow.RouteAsync(new RoutingRequest(
                "Q" + question, sheet[question].Query, "", null, null,
                Candidates, new Dictionary<string, string>()));
            Assert.True(result.IsSuccess, result.ErrorMessage);
            decisions.Add(result.Value!);
        }

        return [.. decisions];
    }

    private static void AssertMatchesSheet(RecordedDecision expected, RoutingDecision actual)
    {
        Assert.Equal(expected.Actual, actual.ChosenUnitId);
        Assert.Equal(expected.KnowledgeEdge, actual.KnowledgeEdge);
        Assert.Equal(expected.Confidence, actual.Confidence, 10);
        Assert.Equal(expected.Correct, !actual.KnowledgeEdge && actual.ChosenUnitId == expected.Gold);

        // Every ranked candidate, with the authority that produced it. The
        // provenance of a decision is part of the decision, not decoration.
        Assert.Equal(expected.RankedCandidates.Length, actual.RankedCandidates.Length);
        for (var place = 0; place < expected.RankedCandidates.Length; place++)
        {
            var want = expected.RankedCandidates[place];
            var got = actual.RankedCandidates[place];
            Assert.Equal(want.UnitId, got.UnitId);
            Assert.Equal(want.UnitRole, got.UnitRole);
            Assert.Equal(want.DomainTag, got.DomainTag);
            Assert.Equal(want.Score, got.Score, 10);
            Assert.Equal(want.Confidence, got.Confidence, 10);
            Assert.Equal(want.AuthorityRole, got.AuthorityRole);
        }
    }

    [Fact]
    public async Task TheWholeRouterAnswersTheRecordedPanelBeforeAndAfterTheLessons()
    {
        var recorded = Recorded();
        Assert.Equal(2, recorded.TrainRows);
        Assert.Equal(8, recorded.RouteCalls);
        Assert.Equal(4, recorded.Before.Length);
        Assert.Equal(4, recorded.After.Length);

        // With no memory at all the router must refuse every question: it has
        // nothing to choose on. The recorded sheet says exactly that, and the
        // refusal itself is checked rather than assumed.
        var cold = await RouteWholePanel(new Dictionary<string, UnitScoringProfile>(), recorded.Before);
        for (var question = 0; question < cold.Length; question++)
        {
            AssertMatchesSheet(recorded.Before[question], cold[question]);
            Assert.True(cold[question].KnowledgeEdge, $"вопрос {question}: маршрутизатор не отказался без памяти");
            Assert.Null(cold[question].ChosenUnitId);
        }

        // With the memory the real writer produced, every question must be
        // answered, and answered exactly as recorded.
        var warm = await RouteWholePanel(TrainedMemory(), recorded.After);
        for (var question = 0; question < warm.Length; question++)
        {
            AssertMatchesSheet(recorded.After[question], warm[question]);
            Assert.False(warm[question].KnowledgeEdge, $"вопрос {question}: маршрутизатор отказался, имея память");
            Assert.Equal(recorded.After[question].Gold, warm[question].ChosenUnitId);
        }

        // Four of four right after the lessons, none before them.
        Assert.Equal(0, recorded.Before.Count(row => row.Correct));
        Assert.Equal(4, recorded.After.Count(row => row.Correct));
    }

    [Fact]
    public void TheLearnedMemoryHasTheRecordedShapeItsProvenanceAndItsSize()
    {
        var recorded = Recorded();
        var memory = TrainedMemory();

        // Shape: the same two routes, field by field, as the run recorded.
        Assert.Equal(recorded.Profiles.Count, memory.Count);
        foreach (var (unitId, want) in recorded.Profiles)
        {
            Assert.True(memory.TryGetValue(unitId, out var got), $"в памяти нет маршрута {unitId}");
            Assert.Equal(want.UnitId, got!.UnitId);
            Assert.Equal(want.UnitRole, got.UnitRole);
            Assert.Equal(want.DomainTag, got.DomainTag);
            Assert.Equal(want.Keywords, got.Keywords);
            Assert.Equal(want.AllowedInputs, got.AllowedInputs);
            Assert.Equal(want.ConfidenceFloor, got.ConfidenceFloor, 10);
            Assert.Equal(want.BaseScore, got.BaseScore, 10);

            // Provenance: every learned profile names the card it came from.
            Assert.Equal(ExpectedProvenance, want.MetadataSourceRef);
            Assert.Equal(ExpectedProvenance, got.MetadataSourceRef);
        }

        // Size: the run recorded how many bytes the whole memory takes when
        // written out. Recomputed here the same way, from the product's own
        // type, so a silently fatter or thinner memory is noticed even when
        // every field above still looks right.
        Assert.Equal(426, recorded.ProfileUtf8Bytes);
        Assert.Equal(recorded.ProfileUtf8Bytes, JsonSerializer.SerializeToUtf8Bytes(memory).Length);
    }

    [Fact]
    public void TheWriterIsTheOnlySourceOfTheMemory_AnEmptyLessonSetTeachesNothing()
    {
        // The row this repair belongs to exists because a hand-built profile
        // once stood in for a written one. So: with no lessons there must be
        // no learned memory at all, and nothing may appear from the registry
        // alone.
        var registry = Registry();
        var trained = new RoutingProfileTrainer(registry).Train([], registry);

        if (trained.IsSuccess)
            Assert.Empty(trained.Value!);
        else
            Assert.False(string.IsNullOrWhiteSpace(trained.ErrorMessage), "отказ без причины");
    }
}
