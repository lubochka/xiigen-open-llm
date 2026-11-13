// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using TrainableRouter;
using TrainableRouter.Training;

// FIX-KR transfer, plan 49-router-learned-key-normalization.md, from KN-ROUTER-LEARNED-KEY-01.
//
// Two lessons taught the router that "C#" and ".NET" belong to the code route. The
// writer stores those two words normalised, as "csharp" and "dotnet". The reader only
// lowercased the request, so a person who typed the technologies the ordinary way
// reached none of the keys the lessons had written, and the router refused. The
// transfer applies the writer's own two replacements, from the writer's own constants,
// in the writer's own order, before the same lowercasing.
//
// Nothing is trained here. The profiles below are the exact learned memory published
// by the experiment, experiments-v2/RP01-MICRO/KR01/results/A.json (sha256
// 03dc8315697a71ea55ad33488ad613c063cd52a81cfaff49987d80c5244aaa78), field "profiles";
// the B arm published byte-identical profiles, which is the point - the memory never
// changed, only the ability to read it.
public sealed class RouterLearnedKeyReadingTests
{
    private static readonly string[] Candidates = ["code", "docs"];

    [Fact]
    public async Task OrdinaryTechnologySpelling_ReachesTheKeysTheLessonWrote()
    {
        var scorer = TrainedScorer();

        // Q1/Q2 of the experiment panel: the spellings a person actually types. Before
        // the transfer these scored zero and the router refused; the learned keys were
        // present the whole time and simply unreachable.
        foreach (var prompt in new[] { "C# .NET", ".NET C#" })
        {
            var ranked = await Rank(scorer, prompt);
            Assert.Equal("code", ranked[0].UnitId);
            Assert.Equal(2.0, ranked[0].Score);
            Assert.Equal(0.44, ranked[0].Confidence, 10);
            Assert.Equal(0.0, ranked[1].Score);
        }

        // Q3: the canonical spelling. It already worked before the transfer, and it must
        // keep working - this is the control proving the change added reach rather than
        // moving it from one spelling to another.
        var canonical = await Rank(scorer, "csharp dotnet");
        Assert.Equal("code", canonical[0].UnitId);
        Assert.Equal(2.0, canonical[0].Score);

        // Q4: the other route's lesson, untouched by either replacement.
        var other = await Rank(scorer, "manual guide");
        Assert.Equal("docs", other[0].UnitId);
        Assert.Equal(2.0, other[0].Score);
    }

    [Fact]
    public async Task WholePanelIsAnsweredOnlyWhenTheMemoryIsThere()
    {
        // With no learned memory every row refuses: that is the ablation, not a result.
        var untrained = new BaselineKeywordCandidateGenerator(
            new Dictionary<string, UnitScoringProfile>(StringComparer.Ordinal));
        var trained = TrainedScorer();

        string[] prompts = ["C# .NET", ".NET C#", "csharp dotnet", "manual guide"];
        string[] gold = ["code", "code", "code", "docs"];

        var untrainedCorrect = 0;
        var trainedCorrect = 0;
        for (var index = 0; index < prompts.Length; index++)
        {
            if (await Decide(untrained, prompts[index]) == gold[index])
                untrainedCorrect++;
            if (await Decide(trained, prompts[index]) == gold[index])
                trainedCorrect++;
        }

        Assert.Equal(0, untrainedCorrect);
        Assert.Equal(4, trainedCorrect);
    }

    [Fact]
    public async Task ReplacementAppliesToEveryTextFieldTheReaderAlreadyUsed()
    {
        var scorer = TrainedScorer();

        // C1-C4: the reader builds its text from four fields. The transfer changed the
        // text, not which fields go into it, so each field behaves the same way.
        Assert.Equal("code", (await Rank(scorer, "c# .net"))[0].UnitId);
        Assert.Equal("code", (await Rank(scorer, string.Empty, taskSlice: "C# .NET"))[0].UnitId);
        Assert.Equal("code", (await Rank(scorer, string.Empty, direction: "C# .NET"))[0].UnitId);
        Assert.Equal("code", (await Rank(scorer, string.Empty, domainHint: "C# .NET"))[0].UnitId);

        // The whole direction string is not one learned key, so no direction boost is
        // added on top of the two hits; the hint string is not the domain tag either.
        Assert.Equal(2.0, (await Rank(scorer, string.Empty, direction: "C# .NET"))[0].Score);
        Assert.Equal(2.0, (await Rank(scorer, string.Empty, domainHint: "C# .NET"))[0].Score);
    }

    [Fact]
    public async Task OneAliasAloneOrNoEvidenceStillRefuses()
    {
        var scorer = TrainedScorer();

        // C5/C6: a single alias is one hit, confidence 0.32, below the unchanged 0.40
        // floor. The transfer restores reach to the keys; it does not lower the bar.
        foreach (var prompt in new[] { "C#", ".NET" })
        {
            var ranked = await Rank(scorer, prompt);
            Assert.Equal(1.0, ranked[0].Score);
            Assert.Equal(0.32, ranked[0].Confidence, 10);
            Assert.Null(await Decide(scorer, prompt));
        }

        // C7: no evidence at all must refuse, never fall through to the first candidate.
        Assert.Null(await Decide(scorer, "weather forecast"));

        // C8: both routes hit twice, so the gap is zero and the tie stays unresolved.
        var tied = await Rank(scorer, "C# .NET manual guide");
        Assert.Equal(tied[0].Score, tied[1].Score);
        Assert.Null(await Decide(scorer, "C# .NET manual guide"));
    }

    [Fact]
    public async Task TwoKnownWeaknessesAreRecordedAsTheyAre_NotAsSuccesses()
    {
        var scorer = TrainedScorer();

        // C9: a domain hint adds 20 to its profile, which outweighs two learned hits.
        // That is the existing deterministic policy, not something this transfer fixes,
        // and not the right answer to the prompt alone. Pinned so it cannot drift
        // unnoticed and cannot be quietly called a win.
        Assert.Equal("docs", await Decide(scorer, "C# .NET", domainHint: "documentation"));

        // C10: the match is a substring test with no word boundaries, so an invented
        // string becomes "abcsharpxyz foodotnetwork" and collects both keys. The honest
        // answer here would be a refusal. The current code routes it to code; the plan
        // says to record that, not to hide it or relabel it as correct.
        var collision = await Decide(scorer, "abc#xyz foo.network");
        Assert.Equal("code", collision);
    }

    [Fact]
    public async Task ProfilesWrittenByTheRealTrainerAreReadBackWithTheSameSpellingRules()
    {
        // Repair R4 of the 27.10 transfer audit, KR row (finding F5). The helper
        // below is called "trained" but builds its profiles by hand, so it can
        // never expose a mismatch between what the writer stores and what the
        // reader looks for. Here the profiles come from the real trainer.
        //
        // The whole point of this transfer is that the reader normalises the
        // two language spellings exactly as the writer does. The lessons below
        // are written with "C#" and ".NET" on purpose: the writer stores the
        // normalised form, so a reader that skipped the replacement would find
        // nothing to match.
        var registry = new RoutingUnitMetadataRegistry(
        [
            new UnitRoutingMetadata("code", "executor", "programming", ["text"], 0.20, "KR-REAL-TRAINER"),
            new UnitRoutingMetadata("docs", "executor", "documentation", ["text"], 0.20, "KR-REAL-TRAINER"),
        ]);

        var trained = new RoutingProfileTrainer(registry).Train(
        [
            // Every lesson is the same sentence except for one word, so that
            // word is the only thing that can decide the answer. For the code
            // unit that word is a language name written the ordinary way.
            new RoutingTrainingExample("l1", "the C#", "", null, null,
                ["code", "docs"], "code", "docs", "it is source code", "a guide would be written instead"),
            new RoutingTrainingExample("l2", "the .NET", "", null, null,
                ["code", "docs"], "code", "docs", "it is source code", "a guide would be written instead"),
            new RoutingTrainingExample("l3", "the guide", "", null, null,
                ["code", "docs"], "docs", "code", "it is prose", "code would be edited instead"),
            new RoutingTrainingExample("l4", "the manual", "", null, null,
                ["code", "docs"], "docs", "code", "it is prose", "code would be edited instead"),
        ]);

        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        // What the writer actually stored, pinned: the two language names are
        // kept in their normalised spelling, and nothing shared leaks in, so
        // the language name is the only thing that can decide the answer.
        Assert.Equal(
            "code=csharp,dotnet | docs=guide,manual",
            string.Join(" | ", trained.Value!.Select(entry =>
                entry.Key + "=" + string.Join(",", entry.Value.Keywords))));
        var scorer = new BaselineKeywordCandidateGenerator(trained.Value!);

        // Asked in the same spelling the lessons used, the question must reach
        // the unit those lessons taught.
        Assert.Equal("code", await Best(scorer, "the C#"));
        Assert.Equal("code", await Best(scorer, "the .NET"));

        // And the other side still wins its own question, so the answer above
        // is not simply the only unit that ever scores.
        Assert.Equal("docs", await Best(scorer, "the guide"));
    }

    // The unit the scorer puts first for one question.
    private static async Task<string> Best(BaselineKeywordCandidateGenerator scorer, string prompt)
    {
        var scored = await scorer.ScoreAsync(Request(prompt));
        Assert.True(scored.IsSuccess, scored.ErrorMessage);
        return scored.Value!.OrderByDescending(entry => entry.Score).First().UnitId;
    }

    private static BaselineKeywordCandidateGenerator TrainedScorer()
        => new(new Dictionary<string, UnitScoringProfile>(StringComparer.Ordinal)
        {
            ["code"] = new UnitScoringProfile(
                UnitId: "code",
                UnitRole: "executor",
                DomainTag: "programming",
                Keywords: ["csharp", "dotnet"],
                AllowedInputs: ["text"],
                ConfidenceFloor: 0.20,
                MetadataSourceRef: "KN-ROUTER-LEARNED-KEY-01",
                BaseScore: 0.0),
            ["docs"] = new UnitScoringProfile(
                UnitId: "docs",
                UnitRole: "executor",
                DomainTag: "documentation",
                Keywords: ["guide", "manual"],
                AllowedInputs: ["text"],
                ConfidenceFloor: 0.20,
                MetadataSourceRef: "KN-ROUTER-LEARNED-KEY-01",
                BaseScore: 0.0)
        });

    private static RoutingRequest Request(
        string prompt,
        string taskSlice = "",
        string? direction = null,
        string? domainHint = null)
        => new(
            RequestId: "kr-" + prompt.GetHashCode(StringComparison.Ordinal).ToString("x8"),
            OriginalPrompt: prompt,
            TaskSlice: taskSlice,
            RequiredDirection: direction,
            DomainHint: domainHint,
            CandidateUnitIds: Candidates,
            Context: new Dictionary<string, string>(StringComparer.Ordinal));

    private static async Task<UnitScoringResult[]> Rank(
        BaselineKeywordCandidateGenerator scorer,
        string prompt,
        string taskSlice = "",
        string? direction = null,
        string? domainHint = null)
    {
        var scored = await scorer.ScoreAsync(Request(prompt, taskSlice, direction, domainHint));
        Assert.True(scored.IsSuccess);
        return scored.Value!;
    }

    // Runs the real flow, so the unchanged confidence floor and score gap decide, not
    // the test's own reading of the numbers.
    private static async Task<string?> Decide(
        BaselineKeywordCandidateGenerator scorer,
        string prompt,
        string? domainHint = null)
    {
        var flow = new TrainableRoutingFlow(scorer, new InMemoryRoutingDecisionLog());
        var decision = await flow.RouteAsync(Request(prompt, domainHint: domainHint));
        Assert.True(decision.IsSuccess);
        return decision.Value!.ChosenUnitId;
    }
}
