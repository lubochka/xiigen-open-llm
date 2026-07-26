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

public sealed class WikiTraceAggregatorManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public WikiTraceAggregatorManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<WikiTraceAggregatorPublicationResult>> PublishAsync(
        WikiTraceAggregatorTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<WikiTraceAggregatorPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<WikiTraceAggregatorPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "WIKI_TRACE_AGGREGATOR_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<WikiTraceAggregatorPublicationResult>.Fail("Trace-aggregator gate must pass before publishing.", "TRACE_AGGREGATOR_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<WikiTraceAggregatorPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"wiki-trace-aggregator-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, WikiTraceAggregatorUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = WikiTraceAggregatorUnit.UnitId,
            kind = "wiki_trace_aggregator_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "wiki-trace-aggregation-fields",
            fields = new[] { "wiki_query_trace", "wiki_unit_trace", "failed_units", "remediation", "stop_blind_training" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            WikiTraceAggregatorUnit.UnitId,
            "wiki-trace-aggregator",
            "project-local-wiki",
            "v1",
            "Trusted",
            WikiTraceAggregatorUnit.UnitId,
            WikiTraceAggregatorUnit.ScenarioId,
            "wiki-stage-13",
            "wiki-trace-aggregation",
            "WikiQueryTrace and WikiUnitTrace to failed-unit/remediation/stop decision",
            $"50-row trace-aggregator DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"failed_unit_accuracy={trainingResult.FailedUnitAccuracy:0.######}; remediation_accuracy={trainingResult.RemediationAccuracy:0.######}; stop_accuracy={trainingResult.StopDecisionAccuracy:0.######}.",
            trainingRunId,
            ["DOCUMENT", "QA", "LEARNING"],
            ["WIKI", "DOCUMENTATION", "TRACE_AGGREGATION"],
            policyPath,
            tokenizerPath,
            4096,
            128,
            96,
            2,
            4,
            4,
            128,
            2,
            4f,
            0.05f,
            ["q_proj", "v_proj"],
            DateTimeOffset.UtcNow,
            UnitFamilyId: "wiki-trace-aggregator",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train WikiTraceAggregator to choose next remediation batches from query/unit traces.",
            TrainingBranch: "project-wiki/wiki-trace-aggregator",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingResult));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<WikiTraceAggregatorPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<WikiTraceAggregatorPublicationResult>();

        return OperationResult<WikiTraceAggregatorPublicationResult>.Ok(new WikiTraceAggregatorPublicationResult(
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

    private static string ComputeDatasetSha256(WikiTraceAggregatorTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
