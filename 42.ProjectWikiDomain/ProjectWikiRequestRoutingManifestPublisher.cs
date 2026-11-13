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

public sealed class ProjectWikiRequestRoutingManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public ProjectWikiRequestRoutingManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<ProjectWikiRequestRoutingPublicationResult>> PublishAsync(
        ProjectWikiRequestRoutingTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "PROJECT_WIKI_REQUEST_ROUTING_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Request-routing gate must pass before publishing.", "REQUEST_ROUTING_GATE_NOT_PASSED");

        if (trainingResult.ModelSnapshot is null)
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Request-routing model snapshot is required before publishing.", "REQUEST_ROUTING_SNAPSHOT_REQUIRED");

        if (!trainingResult.ModelSnapshot.GatePassed)
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Request-routing snapshot held-out gate must pass before publishing.", "REQUEST_ROUTING_SNAPSHOT_GATE_NOT_PASSED");

        if (!trainingResult.ModelSnapshot.SemanticQualityClaimed)
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Request-routing snapshot has semantic_quality_claimed=false and cannot be published.", "REQUEST_ROUTING_SEMANTIC_QUALITY_NOT_CLAIMED");

        if (!trainingResult.ModelSnapshot.PublishAllowed)
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Request-routing snapshot is not marked publishable.", "REQUEST_ROUTING_SNAPSHOT_NOT_PUBLISHABLE");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"project-wiki-request-routing-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, ProjectWikiRequestRouterUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = ProjectWikiRequestRouterUnit.UnitId,
            kind = "project_wiki_request_routing_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "request-routing-prompt-fields",
            fields = new[] { "prompt", "route_kind", "target_unit", "required_inputs" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            ProjectWikiRequestRouterUnit.UnitId,
            "project-wiki-request-router",
            "project-local-wiki",
            "v1",
            "Trusted",
            ProjectWikiRequestRouterUnit.UnitId,
            ProjectWikiRequestRouterUnit.ScenarioId,
            "wiki-stage-00",
            "project-wiki-request-routing",
            "Natural user prompt to project wiki trainable unit route",
            $"50-row request-routing DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"route_accuracy={trainingResult.RouteAccuracy:0.######}; confidence={trainingResult.AverageConfidence:0.######}; evaluation_id={trainingResult.ModelSnapshot.EvaluationId}.",
            trainingRunId,
            ["DOCUMENT", "LEARNING", "ARCHITECTURE"],
            ["WIKI", "DOCUMENTATION", "REQUEST_ROUTING"],
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
            UnitFamilyId: "project-wiki-request-routing",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train ProjectWikiRequestRouterUnit to route project-local wiki prompts to the correct trainable unit.",
            TrainingBranch: "project-wiki/request-routing",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingResult));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<ProjectWikiRequestRoutingPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<ProjectWikiRequestRoutingPublicationResult>();

        return OperationResult<ProjectWikiRequestRoutingPublicationResult>.Ok(new ProjectWikiRequestRoutingPublicationResult(
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

    private static string ComputeDatasetSha256(ProjectWikiRequestRoutingTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
