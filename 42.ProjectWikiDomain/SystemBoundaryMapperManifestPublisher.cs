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

public sealed class SystemBoundaryMapperManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public SystemBoundaryMapperManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<SystemBoundaryMapperPublicationResult>> PublishAsync(
        SystemBoundaryMapperTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<SystemBoundaryMapperPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<SystemBoundaryMapperPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "SYSTEM_BOUNDARY_MAPPER_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<SystemBoundaryMapperPublicationResult>.Fail("System-boundary gate must pass before publishing.", "SYSTEM_BOUNDARY_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<SystemBoundaryMapperPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"system-boundary-mapper-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, SystemBoundaryMapperUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = SystemBoundaryMapperUnit.UnitId,
            kind = "system_boundary_mapper_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "system-boundary-project-evidence-fields",
            fields = new[] { "project_id", "project_role", "stack", "version", "route_or_call_markers", "data_markers", "evidence_ids" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            SystemBoundaryMapperUnit.UnitId,
            "system-boundary-mapper",
            "project-local-wiki",
            "v1",
            "Trusted",
            SystemBoundaryMapperUnit.UnitId,
            SystemBoundaryMapperUnit.ScenarioId,
            "wiki-stage-cross-stack-boundary",
            "system-boundary-mapping",
            "Project evidence to Angular/.NET/SQL system boundary map",
            $"50-row system-boundary DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"boundary_edge_accuracy={trainingResult.BoundaryEdgeAccuracy:0.######}; unknown_boundary_accuracy={trainingResult.UnknownBoundaryAccuracy:0.######}; evidence_grounding_rate={trainingResult.EvidenceGroundingRate:0.######}.",
            trainingRunId,
            ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"],
            ["WIKI", "DOCUMENTATION", "SYSTEM_BOUNDARY", "CROSS_STACK"],
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
            UnitFamilyId: "system-boundary-mapper",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train SystemBoundaryMapperUnit to identify Angular, .NET, and SQL stored procedure project boundaries as one business system.",
            TrainingBranch: "project-wiki/system-boundary-mapper",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingResult));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<SystemBoundaryMapperPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<SystemBoundaryMapperPublicationResult>();

        return OperationResult<SystemBoundaryMapperPublicationResult>.Ok(new SystemBoundaryMapperPublicationResult(
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

    private static string ComputeDatasetSha256(SystemBoundaryMapperTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
