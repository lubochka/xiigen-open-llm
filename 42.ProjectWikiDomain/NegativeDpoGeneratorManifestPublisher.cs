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

public sealed class NegativeDpoGeneratorManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public NegativeDpoGeneratorManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<NegativeDpoGeneratorPublicationResult>> PublishAsync(
        NegativeDpoGeneratorTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<NegativeDpoGeneratorPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<NegativeDpoGeneratorPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "NEGATIVE_DPO_GENERATOR_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<NegativeDpoGeneratorPublicationResult>.Fail("Negative DPO generator gate must pass before publishing.", "NEGATIVE_DPO_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<NegativeDpoGeneratorPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"negative-dpo-generator-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, NegativeDpoGeneratorTrainingRunner.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        var policyState = new
        {
            unit_id = NegativeDpoGeneratorTrainingRunner.UnitId,
            kind = "negative_dpo_generator_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        };
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(policyState, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "negative-dpo-documentation-fields",
            fields = new[] { "chosen_documentation", "rejected_documentation", "negative_type", "wrong_annotation", "evidence_ids" }
        }, JsonOptions), ct);

        var sourceDatasetSha256 = ComputeDatasetSha256(trainingResult);
        var manifest = new GeneratedCapabilityCheckpointManifest(
            NegativeDpoGeneratorTrainingRunner.UnitId,
            "negative-dpo-generator",
            "project-local-wiki",
            "v1",
            "Trusted",
            NegativeDpoGeneratorTrainingRunner.UnitId,
            NegativeDpoGeneratorTrainingRunner.ScenarioId,
            "wiki-stage-06",
            "negative-dpo-generator",
            "Accepted documentation unit to specific rejected DPO contrast",
            BuildTrainingText(trainingResult),
            BuildOutputText(trainingResult),
            trainingRunId,
            ["DOCUMENT", "LEARNING", "QA"],
            ["WIKI", "DOCUMENTATION", "NEGATIVE_DPO"],
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
            UnitFamilyId: "negative-dpo-generator",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train NegativeDpoGenerator to create specific rejected documentation contrasts with [WRONG:] consequences.",
            TrainingBranch: "project-wiki/negative-dpo-generator",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: sourceDatasetSha256);

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<NegativeDpoGeneratorPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<NegativeDpoGeneratorPublicationResult>();

        return OperationResult<NegativeDpoGeneratorPublicationResult>.Ok(new NegativeDpoGeneratorPublicationResult(
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

    private static string BuildTrainingText(NegativeDpoGeneratorTrainingRunResult result)
        => $"50-row negative-DPO generator gate. train_rows={result.TrainingRowCount}; heldout_rows={result.HeldOutRowCount}; unit={NegativeDpoGeneratorTrainingRunner.UnitId}.";

    private static string BuildOutputText(NegativeDpoGeneratorTrainingRunResult result)
        => $"wrong_annotation_rate={result.WrongAnnotationRate:0.######}; rejected_defect_accuracy={result.RejectedDefectAccuracy:0.######}; type_coverage={result.NegativeTypeCoverageRate:0.######}.";

    private static string ComputeDatasetSha256(NegativeDpoGeneratorTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
