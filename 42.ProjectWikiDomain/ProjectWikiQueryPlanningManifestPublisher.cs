// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ModelServer.Contracts;
using ModelServer.Local;
using ModelServer.Remote;

public sealed class ProjectWikiQueryPlanningManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public ProjectWikiQueryPlanningManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<ProjectWikiQueryPlanningPublicationResult>> PublishAsync(
        ProjectWikiQueryPlanningTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiQueryPlanningPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<ProjectWikiQueryPlanningPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "PROJECT_WIKI_QUERY_PLANNING_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<ProjectWikiQueryPlanningPublicationResult>.Fail("Query-planning gate must pass before publishing.", "QUERY_PLANNING_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiQueryPlanningPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"project-wiki-query-planning-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, ProjectWikiQueryPlanningFlow.FlowId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        var policyState = new
        {
            unit_id = ProjectWikiQueryPlanningFlow.FlowId,
            kind = "query_planning_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        };
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(policyState, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "query-planning-text-fields",
            fields = new[] { "intent", "entities", "symbols", "required_query_steps", "sufficiency_checks" }
        }, JsonOptions), ct);

        var sourceDatasetSha256 = ComputeDatasetSha256(trainingResult);
        var manifest = new GeneratedCapabilityCheckpointManifest(
            ProjectWikiQueryPlanningFlow.FlowId,
            "project-wiki-query-planner",
            "project-local-wiki",
            "v1",
            "Trusted",
            ProjectWikiQueryPlanningFlow.FlowId,
            ProjectWikiQueryPlanningFlow.ScenarioId,
            "wiki-stage-09",
            "project-wiki-query-planning",
            "Developer question to wiki evidence-query plan",
            BuildTrainingText(trainingResult),
            BuildOutputText(trainingResult),
            trainingRunId,
            ["DOCUMENT", "LEARNING", "ARCHITECTURE"],
            ["WIKI", "DOCUMENTATION", "ORCHESTRATION"],
            policyPath,
            tokenizerPath,
            2048,
            96,
            64,
            2,
            4,
            4,
            128,
            2,
            4f,
            0.05f,
            ["q_proj", "v_proj"],
            DateTimeOffset.UtcNow,
            UnitFamilyId: "project-wiki-query-planning",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train ProjectWikiQueryPlanningFlow to turn developer questions into multi-step wiki evidence-query plans.",
            TrainingBranch: "project-wiki/query-planning",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: sourceDatasetSha256);

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<ProjectWikiQueryPlanningPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<ProjectWikiQueryPlanningPublicationResult>();

        return OperationResult<ProjectWikiQueryPlanningPublicationResult>.Ok(new ProjectWikiQueryPlanningPublicationResult(
            manifest.UnitId,
            trainingRunId,
            policyPath,
            tokenizerPath,
            manifestPath,
            uploaded.Value!.UnitEndpoint,
            uploaded.Value.ManifestSha256,
            registered.Value,
            []));
    }

    private static string BuildTrainingText(ProjectWikiQueryPlanningTrainingRunResult result)
        => $"50-row query-planning DPO gate. train_rows={result.TrainingRowCount}; heldout_rows={result.HeldOutRowCount}; unit={ProjectWikiQueryPlanningFlow.FlowId}.";

    private static string BuildOutputText(ProjectWikiQueryPlanningTrainingRunResult result)
        => $"query_plan_accuracy={result.QueryPlanAccuracy:0.######}; required_touchpoint_recall={result.RequiredTouchpointRecall:0.######}; missing_evidence_detection={result.MissingEvidenceDetection:0.######}; answer_readiness={result.AverageAnswerReadinessScore:0.######}.";

    private static string ComputeDatasetSha256(ProjectWikiQueryPlanningTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
