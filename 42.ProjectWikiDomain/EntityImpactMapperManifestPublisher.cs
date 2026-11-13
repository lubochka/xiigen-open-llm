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

public sealed class EntityImpactMapperManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public EntityImpactMapperManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<EntityImpactMapperPublicationResult>> PublishAsync(
        EntityImpactMapperTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<EntityImpactMapperPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<EntityImpactMapperPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "ENTITY_IMPACT_MAPPER_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<EntityImpactMapperPublicationResult>.Fail("Entity-impact gate must pass before publishing.", "ENTITY_IMPACT_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<EntityImpactMapperPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"entity-impact-mapper-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, EntityImpactMapperUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = EntityImpactMapperUnit.UnitId,
            kind = "entity_impact_mapper_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "entity-impact-evidence-fields",
            fields = new[] { "entity_name", "evidence_records", "touchpoint_map", "evidence_ids" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            EntityImpactMapperUnit.UnitId,
            "entity-impact-mapper",
            "project-local-wiki",
            "v1",
            "Trusted",
            EntityImpactMapperUnit.UnitId,
            EntityImpactMapperUnit.ScenarioId,
            "wiki-stage-11",
            "entity-impact-mapping",
            "Entity and evidence records to developer change-impact touchpoint map",
            $"50-row entity-impact DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"evidence_recall={trainingResult.EvidenceRecall:0.######}; touchpoint_recall={trainingResult.TouchpointRecall:0.######}.",
            trainingRunId,
            ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"],
            ["WIKI", "DOCUMENTATION", "ENTITY_IMPACT"],
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
            UnitFamilyId: "entity-impact-mapper",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train EntityImpactMapper to answer developer field/entity change impact questions from project evidence.",
            TrainingBranch: "project-wiki/entity-impact-mapper",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingResult));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<EntityImpactMapperPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<EntityImpactMapperPublicationResult>();

        return OperationResult<EntityImpactMapperPublicationResult>.Ok(new EntityImpactMapperPublicationResult(
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

    private static string ComputeDatasetSha256(EntityImpactMapperTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
