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

public sealed class ProjectWikiAnswerComposerManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public ProjectWikiAnswerComposerManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<ProjectWikiAnswerComposerPublicationResult>> PublishAsync(
        ProjectWikiAnswerComposerTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectWikiAnswerComposerPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<ProjectWikiAnswerComposerPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "PROJECT_WIKI_ANSWER_COMPOSER_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<ProjectWikiAnswerComposerPublicationResult>.Fail("Answer-composer gate must pass before publishing.", "ANSWER_COMPOSER_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectWikiAnswerComposerPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"project-wiki-answer-composer-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, ProjectWikiAnswerComposerTrainingRunner.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        var policyState = new
        {
            unit_id = ProjectWikiAnswerComposerTrainingRunner.UnitId,
            kind = "wiki_answer_composer_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        };
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(policyState, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "answer-composer-evidence-fields",
            fields = new[] { "question", "meaning", "location", "parameters", "returns", "callers", "callees", "evidence_ids", "missing_evidence" }
        }, JsonOptions), ct);

        var sourceDatasetSha256 = ComputeDatasetSha256(trainingResult);
        var manifest = new GeneratedCapabilityCheckpointManifest(
            ProjectWikiAnswerComposerTrainingRunner.UnitId,
            "project-wiki-answer-composer",
            "project-local-wiki",
            "v1",
            "Trusted",
            ProjectWikiAnswerComposerTrainingRunner.UnitId,
            ProjectWikiAnswerComposerTrainingRunner.ScenarioId,
            "wiki-stage-09",
            "project-wiki-answer-composer",
            "Project wiki evidence context to grounded developer answer",
            BuildTrainingText(trainingResult),
            BuildOutputText(trainingResult),
            trainingRunId,
            ["DOCUMENT", "LEARNING", "ARCHITECTURE"],
            ["WIKI", "DOCUMENTATION", "ANSWER_COMPOSITION"],
            policyPath,
            tokenizerPath,
            2048,
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
            UnitFamilyId: "project-wiki-answer-composer",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train ProjectWikiAnswerComposer to answer developer wiki questions only from accepted local evidence.",
            TrainingBranch: "project-wiki/answer-composer",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: sourceDatasetSha256);

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<ProjectWikiAnswerComposerPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<ProjectWikiAnswerComposerPublicationResult>();

        return OperationResult<ProjectWikiAnswerComposerPublicationResult>.Ok(new ProjectWikiAnswerComposerPublicationResult(
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

    private static string BuildTrainingText(ProjectWikiAnswerComposerTrainingRunResult result)
        => $"50-row answer-composer DPO gate. train_rows={result.TrainingRowCount}; heldout_rows={result.HeldOutRowCount}; unit={ProjectWikiAnswerComposerTrainingRunner.UnitId}.";

    private static string BuildOutputText(ProjectWikiAnswerComposerTrainingRunResult result)
        => $"exact_evidence_accuracy={result.ExactEvidenceAccuracy:0.######}; citation_accuracy={result.CitationAccuracy:0.######}; answer_shape_accuracy={result.AnswerShapeAccuracy:0.######}; missing_evidence_abstention={result.MissingEvidenceAbstentionAccuracy:0.######}.";

    private static string ComputeDatasetSha256(ProjectWikiAnswerComposerTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
