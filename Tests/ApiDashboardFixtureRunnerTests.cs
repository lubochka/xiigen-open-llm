// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using SmartSkillsDomain.Dashboard;

public sealed class ApiDashboardFixtureRunnerTests
{
    [Fact]
    public void ApiDashboardFixtureRunner_GuardFixture_LoadsKnownP36FixtureAndRefusesWithoutAuthorization()
    {
        var runner = new ApiDashboardFixtureRunner();

        var result = runner.GuardFixture("P36-SKILLSELECTION-01");

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var guard = result.Value!;
        Assert.True(guard.FixtureLoaded);
        Assert.Equal("P36-SKILLSELECTION-01", guard.FixtureId);
        Assert.Equal("authorization_refused", guard.AuthorizationStatus);
        Assert.False(guard.IsAuthorized);
        Assert.Contains("No eval authorization manifest object", guard.RefusalReason, StringComparison.Ordinal);
        Assert.False(guard.TrainingExecuted);
        Assert.False(guard.EvalExecuted);
        Assert.False(guard.CheckpointMutated);
        Assert.False(guard.SemanticQualityClaimed);
        Assert.False(guard.HostRuntimeProofClaimed);
        Assert.False(guard.ExternalAiUsed);
        Assert.Contains("P37", guard.Metadata["p37_runner_gap"], StringComparison.Ordinal);
    }

    [Fact]
    public void ApiDashboardFixtureRunner_GuardFixture_MissingFixtureReturnsReadableFailure()
    {
        var runner = new ApiDashboardFixtureRunner();

        var result = runner.GuardFixture("P36-NOT-A-REAL-FIXTURE");

        Assert.False(result.IsSuccess);
        Assert.Equal("API_DASHBOARD_FIXTURE_NOT_FOUND", result.ErrorCode);
        Assert.Contains("P36-NOT-A-REAL-FIXTURE", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void ApiDashboardFixtureRunner_GuardFixtureWithAuthorizationManifestPath_MalformedAuthorizationRefuses()
    {
        var runner = new ApiDashboardFixtureRunner();
        var manifestPath = WriteTemporaryManifest("this is not json");

        var result = runner.GuardFixtureWithAuthorizationManifestPath("P36-PROMPTTOTASK-01", manifestPath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var guard = result.Value!;
        Assert.False(guard.IsAuthorized);
        Assert.Equal("authorization_refused", guard.AuthorizationStatus);
        Assert.Contains("malformed", guard.RefusalReason, StringComparison.OrdinalIgnoreCase);
        AssertNoExecutionOrProofClaims(guard);
    }

    [Fact]
    public void ApiDashboardFixtureRunner_GuardFixture_NonAllowingAuthorizationRefuses()
    {
        var runner = new ApiDashboardFixtureRunner();
        var manifest = new ApiDashboardFixtureRunnerAuthorizationManifest(
            AuthorizationId: "auth-deny",
            AuthorizationScope: ApiDashboardFixtureRunner.AuthorizationScope,
            FixtureId: "P36-PROMPTTOTASK-01",
            EvalExecutionAllowed: false,
            TrainingAllowed: false,
            CheckpointMutationAllowed: false,
            ExternalAiAllowed: false);

        var result = runner.GuardFixture("P36-PROMPTTOTASK-01", manifest);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var guard = result.Value!;
        Assert.False(guard.IsAuthorized);
        Assert.Contains("eval_execution_allowed is not true", guard.RefusalReason, StringComparison.Ordinal);
        AssertNoExecutionOrProofClaims(guard);
    }

    [Fact]
    public void ApiDashboardFixtureRunner_GuardFixture_ExplicitAllowChangesOnlyAuthorizationMetadata()
    {
        var runner = new ApiDashboardFixtureRunner();
        var manifest = new ApiDashboardFixtureRunnerAuthorizationManifest(
            AuthorizationId: "auth-allow-fixture",
            AuthorizationScope: ApiDashboardFixtureRunner.AuthorizationScope,
            FixtureId: "P36-ORCHESTRATIONREASONING-01",
            EvalExecutionAllowed: true,
            TrainingAllowed: false,
            CheckpointMutationAllowed: false,
            ExternalAiAllowed: false);

        var result = runner.GuardFixture("P36-ORCHESTRATIONREASONING-01", manifest);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var guard = result.Value!;
        Assert.True(guard.IsAuthorized);
        Assert.Equal("authorization_metadata_accepted", guard.AuthorizationStatus);
        Assert.Empty(guard.RefusalReason);
        Assert.Contains("P40 still stops before eval/model execution", guard.NextAction, StringComparison.Ordinal);
        AssertNoExecutionOrProofClaims(guard);
    }

    [Fact]
    public void ApiDashboardFixtureRunner_GuardFixtureWithAuthorizationManifestPath_ExplicitAllowPathStillDoesNotRunEval()
    {
        var runner = new ApiDashboardFixtureRunner();
        var manifestPath = WriteTemporaryManifest(JsonSerializer.Serialize(new
        {
            authorization_id = "auth-file-allow",
            authorization_scope = ApiDashboardFixtureRunner.AuthorizationScope,
            fixture_id = "*",
            eval_execution_allowed = true,
            training_allowed = false,
            checkpoint_mutation_allowed = false,
            external_ai_allowed = false
        }));

        var result = runner.GuardFixtureWithAuthorizationManifestPath("P36-SKILLSELECTION-12", manifestPath);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var guard = result.Value!;
        Assert.True(guard.IsAuthorized);
        Assert.Equal("authorization_metadata_accepted", guard.AuthorizationStatus);
        AssertNoExecutionOrProofClaims(guard);
    }

    private static void AssertNoExecutionOrProofClaims(
        ApiDashboardFixtureRunnerResult guard)
    {
        Assert.False(guard.TrainingExecuted);
        Assert.False(guard.EvalExecuted);
        Assert.False(guard.CheckpointMutated);
        Assert.False(guard.SemanticQualityClaimed);
        Assert.False(guard.HostRuntimeProofClaimed);
        Assert.False(guard.ExternalAiUsed);
        Assert.Equal("false", guard.Metadata["training_executed"]);
        Assert.Equal("false", guard.Metadata["eval_executed"]);
        Assert.Equal("false", guard.Metadata["checkpoint_mutated"]);
        Assert.Equal("false", guard.Metadata["semantic_quality_claimed"]);
        Assert.Equal("false", guard.Metadata["host_runtime_claimed"]);
    }

    private static string WriteTemporaryManifest(
        string text)
    {
        var path = Path.Combine(Path.GetTempPath(), $"api-dashboard-fixture-runner-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, text);
        return path;
    }
}
