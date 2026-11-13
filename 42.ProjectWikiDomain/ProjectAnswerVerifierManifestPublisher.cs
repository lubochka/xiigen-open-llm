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

public sealed class ProjectAnswerVerifierManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public ProjectAnswerVerifierManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<ProjectAnswerVerifierPublicationResult>> PublishAsync(
        ProjectAnswerVerifierTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectAnswerVerifierPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<ProjectAnswerVerifierPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "PROJECT_ANSWER_VERIFIER_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<ProjectAnswerVerifierPublicationResult>.Fail("Answer-verifier gate must pass before publishing.", "ANSWER_VERIFIER_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<ProjectAnswerVerifierPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"project-answer-verifier-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, ProjectAnswerVerifier.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = ProjectAnswerVerifier.UnitId,
            kind = "project_answer_verifier_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "answer-verification-fields",
            fields = new[] { "answer", "evidence_index", "expected_project_id", "warnings", "citation_accuracy" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            ProjectAnswerVerifier.UnitId,
            "project-answer-verifier",
            "project-local-wiki",
            "v1",
            "Trusted",
            ProjectAnswerVerifier.UnitId,
            ProjectAnswerVerifier.ScenarioId,
            "wiki-stage-09-13",
            "project-answer-verification",
            "Wiki answer and evidence index to citation, boundary, and abstention verification",
            $"50-row answer-verifier DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"pass_fail_accuracy={trainingResult.PassFailAccuracy:0.######}; warning_set_accuracy={trainingResult.WarningSetAccuracy:0.######}; citation_accuracy={trainingResult.AverageCitationAccuracy:0.######}.",
            trainingRunId,
            ["DOCUMENT", "QA", "LEARNING"],
            ["WIKI", "DOCUMENTATION", "ANSWER_VERIFIER"],
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
            UnitFamilyId: "project-answer-verifier",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train ProjectAnswerVerifier to reject invalid citations, wrong-project answers, and undeclared missing evidence.",
            TrainingBranch: "project-wiki/project-answer-verifier",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingResult));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<ProjectAnswerVerifierPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<ProjectAnswerVerifierPublicationResult>();

        return OperationResult<ProjectAnswerVerifierPublicationResult>.Ok(new ProjectAnswerVerifierPublicationResult(
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

    private static string ComputeDatasetSha256(ProjectAnswerVerifierTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
