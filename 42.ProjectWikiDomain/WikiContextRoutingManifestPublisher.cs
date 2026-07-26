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

public sealed class WikiContextRoutingManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public WikiContextRoutingManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<ProjectWikiContextRoutingPublicationResult>> PublishAsync(
        ProjectWikiContextRoutingTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiContextRoutingPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<ProjectWikiContextRoutingPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "WIKI_CONTEXT_ROUTING_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<ProjectWikiContextRoutingPublicationResult>.Fail("Context-routing gate must pass before publishing.", "CONTEXT_ROUTING_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiContextRoutingPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"wiki-context-routing-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, WikiContextRoutingUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        var policyState = new
        {
            unit_id = WikiContextRoutingUnit.UnitId,
            kind = "wiki_context_routing_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        };
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(policyState, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "context-routing-plan-fields",
            fields = new[] { "intent", "required_query_steps", "allow_semantic", "pre_checks", "route_kind" }
        }, JsonOptions), ct);

        var sourceDatasetSha256 = ComputeDatasetSha256(trainingResult);
        var manifest = new GeneratedCapabilityCheckpointManifest(
            WikiContextRoutingUnit.UnitId,
            "wiki-context-routing",
            "project-local-wiki",
            "v1",
            "Trusted",
            WikiContextRoutingUnit.UnitId,
            WikiContextRoutingUnit.ScenarioId,
            "wiki-stage-09",
            "wiki-context-routing",
            "Project wiki query plan to context retrieval route",
            BuildTrainingText(trainingResult),
            BuildOutputText(trainingResult),
            trainingRunId,
            ["DOCUMENT", "LEARNING", "ARCHITECTURE"],
            ["WIKI", "DOCUMENTATION", "CONTEXT_ROUTING"],
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
            UnitFamilyId: "wiki-context-routing",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train WikiContextRoutingUnit to choose BM25, GraphRAG, abstention, wrong-project rejection, or stale-document rejection.",
            TrainingBranch: "project-wiki/context-routing",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: sourceDatasetSha256);

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<ProjectWikiContextRoutingPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<ProjectWikiContextRoutingPublicationResult>();

        return OperationResult<ProjectWikiContextRoutingPublicationResult>.Ok(new ProjectWikiContextRoutingPublicationResult(
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

    private static string BuildTrainingText(ProjectWikiContextRoutingTrainingRunResult result)
        => $"50-row context-routing DPO gate. train_rows={result.TrainingRowCount}; heldout_rows={result.HeldOutRowCount}; unit={WikiContextRoutingUnit.UnitId}.";

    private static string BuildOutputText(ProjectWikiContextRoutingTrainingRunResult result)
        => $"route_accuracy={result.RouteAccuracy:0.######}; precheck_accuracy={result.PreCheckAccuracy:0.######}; confidence={result.AverageConfidence:0.######}.";

    private static string ComputeDatasetSha256(ProjectWikiContextRoutingTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
