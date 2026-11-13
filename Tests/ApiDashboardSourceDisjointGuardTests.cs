// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardSourceDisjointGuardTests
{
    [Fact]
    public void ApiDashboardSourceDisjointGuard_VerifyP36FixtureRows_PassesCurrentP36ButDoesNotAuthorizeEval()
    {
        var guard = new ApiDashboardSourceDisjointGuard();

        var result = guard.VerifyP36FixtureRows();

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.True(report.SourceDisjointPrecheckPassed);
        Assert.Equal(36, report.FixtureCount);
        Assert.Equal(12, report.CountsByMachine["PromptToTask"]);
        Assert.Equal(12, report.CountsByMachine["OrchestrationReasoning"]);
        Assert.Equal(12, report.CountsByMachine["SkillSelection"]);
        Assert.Empty(report.DuplicateFixtureIds);
        Assert.Empty(report.MissingManifestFamilies);
        Assert.Empty(report.UnexpectedManifestFamilies);
        Assert.Empty(report.FamiliesBothTrainAndEval);
        Assert.True(report.UniversalOnly);
        AssertNoProofFlags(report);
        Assert.False(report.CanProceedToEvalRunner);
        Assert.Contains("parse-only", report.EvidenceScope, StringComparison.Ordinal);
    }

    [Fact]
    public void ApiDashboardSourceDisjointGuard_VerifySourceDisjointRows_DetectsDuplicateFixtureIds()
    {
        var paths = WriteTempFixtureSet(
            ManifestJson("PromptToTask", ("family_duplicate", 2)),
            FixtureJson("DUPLICATE-ID", "PromptToTask", "eval_holdout", "family_duplicate"),
            FixtureJson("DUPLICATE-ID", "PromptToTask", "eval_holdout", "family_duplicate"));
        var guard = new ApiDashboardSourceDisjointGuard();

        var result = guard.VerifySourceDisjointRows(paths.ManifestPath, paths.FixturePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Contains("DUPLICATE-ID", report.DuplicateFixtureIds);
        Assert.False(report.SourceDisjointPrecheckPassed);
        AssertNoProofFlags(report);
    }

    [Fact]
    public void ApiDashboardSourceDisjointGuard_VerifySourceDisjointRows_DetectsMissingAndUnexpectedFamilyMismatch()
    {
        var paths = WriteTempFixtureSet(
            ManifestJson("PromptToTask", ("family_expected", 1), ("family_extra", 1)),
            FixtureJson("FIXTURE-01", "PromptToTask", "eval_holdout", "family_expected"),
            FixtureJson("FIXTURE-02", "PromptToTask", "eval_holdout", "family_missing"));
        var guard = new ApiDashboardSourceDisjointGuard();

        var result = guard.VerifySourceDisjointRows(paths.ManifestPath, paths.FixturePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Contains("family_missing", report.MissingManifestFamilies);
        Assert.Contains("family_extra", report.UnexpectedManifestFamilies);
        Assert.False(report.SourceDisjointPrecheckPassed);
        AssertNoProofFlags(report);
    }

    [Fact]
    public void ApiDashboardSourceDisjointGuard_VerifySourceDisjointRows_DetectsTrainEvalOverlap()
    {
        var paths = WriteTempFixtureSet(
            ManifestJson("PromptToTask", ("family_overlap", 2)),
            FixtureJson("TRAIN-01", "PromptToTask", "train_seed", "family_overlap"),
            FixtureJson("EVAL-01", "PromptToTask", "eval_holdout", "family_overlap"));
        var guard = new ApiDashboardSourceDisjointGuard();

        var result = guard.VerifySourceDisjointRows(paths.ManifestPath, paths.FixturePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;
        Assert.Contains("family_overlap", report.FamiliesBothTrainAndEval);
        Assert.False(report.SourceDisjointPrecheckPassed);
        AssertNoProofFlags(report);
    }

    [Fact]
    public void ApiDashboardSourceDisjointGuard_VerifySourceDisjointRows_DoesNotTurnProofFlagsTrue()
    {
        var paths = WriteTempFixtureSet(
            ManifestJson("SkillSelection", ("family_safe", 1)),
            FixtureJson("SAFE-01", "SkillSelection", "eval_holdout", "family_safe"));
        var guard = new ApiDashboardSourceDisjointGuard();

        var result = guard.VerifySourceDisjointRows(paths.ManifestPath, paths.FixturePath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        AssertNoProofFlags(result.Value!);
        Assert.False(result.Value!.CanProceedToEvalRunner);
    }

    private static void AssertNoProofFlags(
        ApiDashboardSourceDisjointGuardReport report)
    {
        Assert.False(report.TrainingRun);
        Assert.False(report.EvalRun);
        Assert.False(report.CheckpointMutated);
        Assert.False(report.SemanticQualityClaimed);
        Assert.False(report.RawPrivateTextCopied);
    }

    private static TempFixtureSet WriteTempFixtureSet(
        string manifestJson,
        params string[] fixtureRows)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"api-dashboard-source-disjoint-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var manifestPath = Path.Combine(directory, "manifest.json");
        var fixturePath = Path.Combine(directory, "fixtures.jsonl");
        File.WriteAllText(manifestPath, manifestJson);
        File.WriteAllText(fixturePath, string.Join(Environment.NewLine, fixtureRows));
        return new TempFixtureSet(manifestPath, fixturePath);
    }

    private static string ManifestJson(
        string machine,
        params (string Family, int Count)[] families)
    {
        var familyJson = string.Join(",", families.Select(family => $"\"{family.Family}\":{family.Count}"));
        var total = families.Sum(family => family.Count);
        return $$"""
        {
          "schemaVersion": "p36.source-disjoint-manifest.v1",
          "countsByMachine": { "{{machine}}": {{total}} },
          "countsByMachineAndFamily": { "{{machine}}": { {{familyJson}} } },
          "universalOnly": true,
          "rawPrivateTextCopied": false,
          "trainingRun": false,
          "evalRun": false,
          "checkpointMutated": false,
          "semanticQualityClaimed": false
        }
        """;
    }

    private static string FixtureJson(
        string fixtureId,
        string machine,
        string splitRole,
        string sourceFamily)
        => $$"""{"fixture_id":"{{fixtureId}}","machine":"{{machine}}","split_role":"{{splitRole}}","heldout_source_family":"{{sourceFamily}}","redacted_input":"example","expected_output_slots":{},"universal_only":true,"raw_private_text_copied":false,"training_allowed":false,"eval_execution_allowed":false,"not_executed_yet":true}""";

    private sealed record TempFixtureSet(
        string ManifestPath,
        string FixturePath);
}
