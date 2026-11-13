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

public sealed class DocumentationGeneratorManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public DocumentationGeneratorManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<DocumentationGeneratorPublicationResult>> PublishAsync(
        DocumentationGeneratorTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<DocumentationGeneratorPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<DocumentationGeneratorPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "DOCUMENTATION_GENERATOR_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<DocumentationGeneratorPublicationResult>.Fail("Documentation-generator gate must pass before publishing.", "DOCUMENTATION_GENERATOR_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<DocumentationGeneratorPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"documentation-generator-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, DocumentationGeneratorUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = DocumentationGeneratorUnit.UnitId,
            kind = "documentation_generator_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "documentation-unit-fields",
            fields = new[] { "meaning", "location", "parameters", "returns", "callers", "callees", "stack_version", "evidence_ids" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            DocumentationGeneratorUnit.UnitId,
            "documentation-generator",
            "project-local-wiki",
            "v1",
            "Trusted",
            DocumentationGeneratorUnit.UnitId,
            DocumentationGeneratorUnit.ScenarioId,
            "wiki-stage-04",
            "bottom-up-documentation-generation",
            "Accepted code trace unit to complete bottom-up documentation text",
            $"50-row documentation-generator DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"required_field_coverage={trainingResult.RequiredFieldCoverage:0.######}.",
            trainingRunId,
            ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"],
            ["WIKI", "DOCUMENTATION", "DOC_GENERATION"],
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
            UnitFamilyId: "documentation-generator",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train DocumentationGenerator to produce complete code documentation from accepted trace evidence.",
            TrainingBranch: "project-wiki/documentation-generator",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingResult));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<DocumentationGeneratorPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<DocumentationGeneratorPublicationResult>();

        return OperationResult<DocumentationGeneratorPublicationResult>.Ok(new DocumentationGeneratorPublicationResult(
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

    private static string ComputeDatasetSha256(DocumentationGeneratorTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
