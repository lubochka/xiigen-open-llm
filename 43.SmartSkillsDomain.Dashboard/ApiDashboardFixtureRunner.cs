// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain.Dashboard;

using System.IO;
using System.Text.Json;
using AgentInfrastructure;

public sealed record ApiDashboardFixtureRunnerAuthorizationManifest(
    string AuthorizationId,
    string AuthorizationScope,
    string FixtureId,
    bool EvalExecutionAllowed,
    bool TrainingAllowed,
    bool CheckpointMutationAllowed,
    bool ExternalAiAllowed);

public sealed record ApiDashboardFixtureRunnerResult(
    string FixtureId,
    string? ScenarioId,
    string? Machine,
    string? MethodId,
    string AuthorizationStatus,
    string RefusalReason,
    bool IsAuthorized,
    bool FixtureLoaded,
    bool TrainingExecuted,
    bool EvalExecuted,
    bool CheckpointMutated,
    bool SemanticQualityClaimed,
    bool HostRuntimeProofClaimed,
    bool ExternalAiUsed,
    string NextAction,
    string EvidenceScope,
    IReadOnlyDictionary<string, string> Metadata);

public sealed class ApiDashboardFixtureRunner
{
    public const string AuthorizationScope = "semantic-quality-fixture-eval";
    public const string GuardEvidenceScope = "dashboard fixture runner guard only; no model eval, no training, no checkpoint mutation, no semantic-quality proof, no host runtime proof";
    private const string P37ReadinessGapLedgerPath = "artifacts/smart-skills-semantic-quality-completion/p37-runner-dashboard-wiring-design/p37-readiness-gap-ledger.jsonl";
    private const string RunnerAuthorizationGapId = "P37-GAP-02";

    public OperationResult<ApiDashboardFixtureRunnerResult> GuardFixture(
        string fixtureId,
        string? workspaceRoot = null)
        => GuardFixture(
            fixtureId,
            authorizationManifest: null,
            workspaceRoot);

    public OperationResult<ApiDashboardFixtureRunnerResult> GuardFixture(
        string fixtureId,
        ApiDashboardFixtureRunnerAuthorizationManifest? authorizationManifest,
        string? workspaceRoot = null)
    {
        var fixtureLookup = LoadFixture(fixtureId, workspaceRoot);
        if (!fixtureLookup.IsSuccess || fixtureLookup.Value is null)
        {
            return fixtureLookup.MapError<ApiDashboardFixtureRunnerResult>();
        }

        var gapResult = LoadRunnerGapStatus(workspaceRoot);
        var decision = EvaluateAuthorization(fixtureLookup.Value.FixtureId, authorizationManifest, manifestProblem: null);
        return OperationResult<ApiDashboardFixtureRunnerResult>.Ok(
            BuildResult(
                fixtureLookup.Value,
                decision,
                gapResult.IsSuccess ? gapResult.Value ?? "P37 runner authorization gap not found." : gapResult.ErrorMessage ?? "P37 runner authorization gap could not be read."));
    }

    public OperationResult<ApiDashboardFixtureRunnerResult> GuardFixtureWithAuthorizationManifestPath(
        string fixtureId,
        string? authorizationManifestPath,
        string? workspaceRoot = null)
    {
        var fixtureLookup = LoadFixture(fixtureId, workspaceRoot);
        if (!fixtureLookup.IsSuccess || fixtureLookup.Value is null)
        {
            return fixtureLookup.MapError<ApiDashboardFixtureRunnerResult>();
        }

        var manifestResult = LoadAuthorizationManifest(authorizationManifestPath, workspaceRoot);
        var gapResult = LoadRunnerGapStatus(workspaceRoot);
        var decision = EvaluateAuthorization(
            fixtureLookup.Value.FixtureId,
            manifestResult.IsSuccess ? manifestResult.Value : null,
            manifestResult.IsSuccess ? null : manifestResult.ErrorMessage);

        return OperationResult<ApiDashboardFixtureRunnerResult>.Ok(
            BuildResult(
                fixtureLookup.Value,
                decision,
                gapResult.IsSuccess ? gapResult.Value ?? "P37 runner authorization gap not found." : gapResult.ErrorMessage ?? "P37 runner authorization gap could not be read."));
    }

    private static OperationResult<ApiDashboardFixtureExampleRow> LoadFixture(
        string fixtureId,
        string? workspaceRoot)
    {
        var requestedFixtureId = fixtureId?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(requestedFixtureId))
        {
            return OperationResult<ApiDashboardFixtureExampleRow>.Fail(
                "Dashboard fixture runner guard requires a fixture id.",
                "API_DASHBOARD_FIXTURE_ID_REQUIRED");
        }

        var examples = SemanticQualityFixtureCatalogProvider.BuildFixtureExamples(workspaceRoot);
        if (!examples.IsSuccess || examples.Value is null)
        {
            return examples.MapError<ApiDashboardFixtureExampleRow>();
        }

        var fixture = examples.Value.SingleOrDefault(row => string.Equals(row.FixtureId, requestedFixtureId, StringComparison.Ordinal));
        if (fixture is null)
        {
            return OperationResult<ApiDashboardFixtureExampleRow>.Fail(
                $"Dashboard fixture runner guard could not find P36 fixture '{requestedFixtureId}'.",
                "API_DASHBOARD_FIXTURE_NOT_FOUND");
        }

        return OperationResult<ApiDashboardFixtureExampleRow>.Ok(fixture);
    }

    private static OperationResult<ApiDashboardFixtureRunnerAuthorizationManifest> LoadAuthorizationManifest(
        string? authorizationManifestPath,
        string? workspaceRoot)
    {
        if (string.IsNullOrWhiteSpace(authorizationManifestPath))
        {
            return OperationResult<ApiDashboardFixtureRunnerAuthorizationManifest>.Fail(
                "No eval authorization manifest was provided.",
                "API_DASHBOARD_EVAL_AUTHORIZATION_MANIFEST_MISSING");
        }

        var path = Path.IsPathRooted(authorizationManifestPath)
            ? authorizationManifestPath
            : Path.Combine(ResolveWorkspaceRoot(workspaceRoot), authorizationManifestPath);
        if (!File.Exists(path))
        {
            return OperationResult<ApiDashboardFixtureRunnerAuthorizationManifest>.Fail(
                $"Eval authorization manifest was not found at '{path}'.",
                "API_DASHBOARD_EVAL_AUTHORIZATION_MANIFEST_NOT_FOUND");
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement;
            return OperationResult<ApiDashboardFixtureRunnerAuthorizationManifest>.Ok(
                new ApiDashboardFixtureRunnerAuthorizationManifest(
                    AuthorizationId: ReadString(root, "authorization_id"),
                    AuthorizationScope: ReadString(root, "authorization_scope"),
                    FixtureId: ReadString(root, "fixture_id"),
                    EvalExecutionAllowed: ReadBool(root, "eval_execution_allowed"),
                    TrainingAllowed: ReadBool(root, "training_allowed"),
                    CheckpointMutationAllowed: ReadBool(root, "checkpoint_mutation_allowed"),
                    ExternalAiAllowed: ReadBool(root, "external_ai_allowed")));
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException)
        {
            return OperationResult<ApiDashboardFixtureRunnerAuthorizationManifest>.Fail(
                $"Eval authorization manifest is malformed: {ex.Message}",
                "API_DASHBOARD_EVAL_AUTHORIZATION_MANIFEST_MALFORMED");
        }
    }

    private static AuthorizationDecision EvaluateAuthorization(
        string fixtureId,
        ApiDashboardFixtureRunnerAuthorizationManifest? authorizationManifest,
        string? manifestProblem)
    {
        if (!string.IsNullOrWhiteSpace(manifestProblem))
        {
            return new AuthorizationDecision(
                IsAuthorized: false,
                Status: "authorization_refused",
                RefusalReason: manifestProblem,
                NextAction: "Provide a separate explicit eval authorization manifest before any eval runner is requested.");
        }

        if (authorizationManifest is null)
        {
            return new AuthorizationDecision(
                IsAuthorized: false,
                Status: "authorization_refused",
                RefusalReason: "No eval authorization manifest object was provided.",
                NextAction: "Provide a separate explicit eval authorization manifest before any eval runner is requested.");
        }

        var reasons = new List<string>();
        if (string.IsNullOrWhiteSpace(authorizationManifest.AuthorizationId))
            reasons.Add("authorization_id is missing");
        if (!string.Equals(authorizationManifest.AuthorizationScope, AuthorizationScope, StringComparison.Ordinal))
            reasons.Add($"authorization_scope must be '{AuthorizationScope}'");
        if (!authorizationManifest.EvalExecutionAllowed)
            reasons.Add("eval_execution_allowed is not true");
        if (!string.Equals(authorizationManifest.FixtureId, fixtureId, StringComparison.Ordinal) && !string.Equals(authorizationManifest.FixtureId, "*", StringComparison.Ordinal))
            reasons.Add("fixture_id does not match the requested fixture");
        if (authorizationManifest.TrainingAllowed)
            reasons.Add("training_allowed must remain false for this dashboard guard");
        if (authorizationManifest.CheckpointMutationAllowed)
            reasons.Add("checkpoint_mutation_allowed must remain false for this dashboard guard");
        if (authorizationManifest.ExternalAiAllowed)
            reasons.Add("external_ai_allowed must remain false for this dashboard guard");

        if (reasons.Count > 0)
        {
            return new AuthorizationDecision(
                IsAuthorized: false,
                Status: "authorization_refused",
                RefusalReason: string.Join("; ", reasons),
                NextAction: "Fix the authorization manifest or keep the fixture in parse-only dashboard guard mode.");
        }

        return new AuthorizationDecision(
            IsAuthorized: true,
            Status: "authorization_metadata_accepted",
            RefusalReason: string.Empty,
            NextAction: "Authorization metadata is accepted, but P40 still stops before eval/model execution; a separate authorized eval slice must run any eval.");
    }

    private static ApiDashboardFixtureRunnerResult BuildResult(
        ApiDashboardFixtureExampleRow fixture,
        AuthorizationDecision decision,
        string p37RunnerGapStatus)
        => new(
            FixtureId: fixture.FixtureId,
            ScenarioId: fixture.ScenarioId,
            Machine: fixture.Machine,
            MethodId: fixture.MethodId,
            AuthorizationStatus: decision.Status,
            RefusalReason: decision.RefusalReason,
            IsAuthorized: decision.IsAuthorized,
            FixtureLoaded: true,
            TrainingExecuted: false,
            EvalExecuted: false,
            CheckpointMutated: false,
            SemanticQualityClaimed: false,
            HostRuntimeProofClaimed: false,
            ExternalAiUsed: false,
            NextAction: decision.NextAction,
            EvidenceScope: GuardEvidenceScope,
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["fixture_id"] = fixture.FixtureId,
                ["scenario_id"] = fixture.ScenarioId,
                ["machine"] = fixture.Machine,
                ["heldout_source_family"] = fixture.HeldoutSourceFamily,
                ["p37_runner_gap"] = p37RunnerGapStatus,
                ["training_executed"] = "false",
                ["eval_executed"] = "false",
                ["checkpoint_mutated"] = "false",
                ["semantic_quality_claimed"] = "false",
                ["host_runtime_claimed"] = "false"
            });

    private static OperationResult<string> LoadRunnerGapStatus(
        string? workspaceRoot)
    {
        var path = Path.Combine(ResolveWorkspaceRoot(workspaceRoot), P37ReadinessGapLedgerPath);
        if (!File.Exists(path))
        {
            return OperationResult<string>.Fail(
                "P37 runner readiness gap ledger is missing.",
                "API_DASHBOARD_P37_RUNNER_GAP_LEDGER_MISSING");
        }

        try
        {
            foreach (var line in File.ReadLines(path).Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                using var document = JsonDocument.Parse(line);
                var root = document.RootElement;
                if (string.Equals(ReadString(root, "gap_id"), RunnerAuthorizationGapId, StringComparison.Ordinal))
                {
                    return OperationResult<string>.Ok($"{RunnerAuthorizationGapId}: {ReadString(root, "next_design_or_execution_step")}");
                }
            }

            return OperationResult<string>.Fail(
                "P37 runner authorization gap row was not found.",
                "API_DASHBOARD_P37_RUNNER_GAP_NOT_FOUND");
        }
        catch (Exception ex) when (ex is JsonException or IOException or InvalidOperationException)
        {
            return OperationResult<string>.Fail(
                $"P37 runner readiness gap ledger could not be read: {ex.Message}",
                "API_DASHBOARD_P37_RUNNER_GAP_LEDGER_READ_FAILED");
        }
    }

    private static string ReadString(
        JsonElement row,
        string propertyName)
        => row.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static bool ReadBool(
        JsonElement row,
        string propertyName)
        => row.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.True;

    private static string ResolveWorkspaceRoot(string? workspaceRoot)
    {
        if (!string.IsNullOrWhiteSpace(workspaceRoot))
            return workspaceRoot;

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "43.SmartSkillsDomain.Dashboard", "ApiDashboardCatalog.cs")))
                return current.FullName;

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    private sealed record AuthorizationDecision(
        bool IsAuthorized,
        string Status,
        string RefusalReason,
        string NextAction);
}
