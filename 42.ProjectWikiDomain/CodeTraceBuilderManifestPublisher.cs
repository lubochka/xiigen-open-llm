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

public sealed class CodeTraceBuilderManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;

    public CodeTraceBuilderManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public async Task<OperationResult<CodeTraceBuilderPublicationResult>> PublishAsync(
        CodeTraceBuilderTrainingRunResult trainingResult,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<CodeTraceBuilderPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");

        if (trainingResult is null)
            return OperationResult<CodeTraceBuilderPublicationResult>.Fail("Training result is required.", "TRAINING_RESULT_REQUIRED");

        if (!string.Equals(trainingResult.Verdict, "CODE_TRACE_BUILDER_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<CodeTraceBuilderPublicationResult>.Fail("Code-trace gate must pass before publishing.", "CODE_TRACE_GATE_NOT_PASSED");

        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<CodeTraceBuilderPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"code-trace-builder-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, CodeTraceBuilderUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = CodeTraceBuilderUnit.UnitId,
            kind = "code_trace_builder_policy_state",
            training_result = trainingResult,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "code-trace-source-fields",
            fields = new[] { "source_text", "methods", "call_edges", "value_flows" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            CodeTraceBuilderUnit.UnitId,
            "code-trace-builder",
            "project-local-wiki",
            "v1",
            "Trusted",
            CodeTraceBuilderUnit.UnitId,
            CodeTraceBuilderUnit.ScenarioId,
            "wiki-stage-02-03",
            "code-trace-building",
            "Source code to method, caller-callee, and value-flow trace",
            $"50-row code-trace DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"method_recall={trainingResult.MethodRecall:0.######}; call_edge_recall={trainingResult.CallEdgeRecall:0.######}; value_flow_recall={trainingResult.ValueFlowRecall:0.######}.",
            trainingRunId,
            ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"],
            ["WIKI", "DOCUMENTATION", "CODE_TRACE"],
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
            UnitFamilyId: "code-trace-builder",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train CodeTraceBuilder to recover method, callgraph, and value-flow evidence for bottom-up documentation.",
            TrainingBranch: "project-wiki/code-trace-builder",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingResult));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<CodeTraceBuilderPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<CodeTraceBuilderPublicationResult>();

        return OperationResult<CodeTraceBuilderPublicationResult>.Ok(new CodeTraceBuilderPublicationResult(
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

    private static string ComputeDatasetSha256(CodeTraceBuilderTrainingRunResult result)
    {
        var text = JsonSerializer.Serialize(result.Cases, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }
}
