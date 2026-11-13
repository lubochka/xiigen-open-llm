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

public sealed class CrossStackBusinessFlowGraphManifestPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IModelRepository _repository;
    private readonly IRemoteUnitLocator _locator;
    private readonly CrossStackFlowPairBatchValidator _validator = new();

    public CrossStackBusinessFlowGraphManifestPublisher(
        IModelRepository? repository = null,
        IRemoteUnitLocator? locator = null)
    {
        _repository = repository ?? new LocalModelRepository();
        _locator = locator ?? new InMemoryRemoteUnitLocator();
    }

    public Task<OperationResult<CrossStackBusinessFlowGraphPublicationResult>> PublishAsync(
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics,
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphPublicationResult>.Fail(
                "Publication cancelled.",
                "PUBLISH_CANCELLED"));
        }

        // Публичный путь может только описать candidate; opaque authority
        // рождается внутри validator-owned pipeline, не из caller-provided bool.
        return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphPublicationResult>.Fail(
            "Opaque CrossStack publication authority is required.",
            "CROSS_STACK_GRAPH_PUBLICATION_AUTHORITY_REQUIRED"));
    }

    internal async Task<OperationResult<CrossStackBusinessFlowGraphPublicationResult>> PublishWithAuthorityAsync(
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics,
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackBusinessFlowGraphPublicationAuthority publicationAuthority,
        string artifactRoot,
        string serverBaseUrl = "https://models.local",
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<CrossStackBusinessFlowGraphPublicationResult>.Fail("Publication cancelled.", "PUBLISH_CANCELLED");
        // Защита на последней двери: publisher повторно сверяет exact rows,
        // provenance, split role, matrix и identities до первого файла/upload/register.
        var authority = _validator.ValidatePublicationCandidate(
            trainingBatch,
            heldOutBatch,
            trainingResult,
            dpoMetrics,
            publicationAuthority,
            requireSemanticSource: true);
        if (!authority.IsSuccess)
            return authority.MapError<CrossStackBusinessFlowGraphPublicationResult>();
        if (!string.Equals(trainingResult.Verdict, "CROSS_STACK_BUSINESS_FLOW_GRAPH_GATE_PASSED", StringComparison.Ordinal))
            return OperationResult<CrossStackBusinessFlowGraphPublicationResult>.Fail("Cross-stack graph gate must pass before publishing.", "CROSS_STACK_GRAPH_GATE_NOT_PASSED");
        // Дверь публикации по КОНЕЧНЫМ метрикам обучения предпочтений. Пороги
        // не изменились, но правило теперь записано один раз и одинаково для
        // публикатора и конвейера. Прежнее выражение здесь отказывало по
        // условию «меньше порога» и поэтому молча пропускало незамеренное
        // значение: сравнение с не-числом всегда ложно, и дальше шли запись
        // весов, манифеста, выгрузка и регистрация.
        var dpoGate = ProjectWikiDpoPublicationMetricGate.Evaluate(dpoMetrics);
        if (!dpoGate.IsSuccess)
            return dpoGate.MapError<CrossStackBusinessFlowGraphPublicationResult>();
        if (string.IsNullOrWhiteSpace(artifactRoot))
            return OperationResult<CrossStackBusinessFlowGraphPublicationResult>.Fail("Artifact root is required.", "ARTIFACT_ROOT_REQUIRED");

        var trainingRunId = $"cross-stack-business-flow-graph-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        var runRoot = Path.Combine(artifactRoot, CrossStackBusinessFlowGraphUnit.UnitId, trainingRunId);
        Directory.CreateDirectory(runRoot);
        var policyPath = Path.Combine(runRoot, "policy.bin");
        var tokenizerPath = Path.Combine(runRoot, "tokenizer.json");
        var manifestPath = Path.Combine(runRoot, "generated_capability_manifest.json");
        await File.WriteAllTextAsync(policyPath, JsonSerializer.Serialize(new
        {
            unit_id = CrossStackBusinessFlowGraphUnit.UnitId,
            kind = "cross_stack_business_flow_graph_policy_state",
            training_result = trainingResult,
            dpo_metrics = dpoMetrics,
            dpo_policy_checkpoint = dpoMetrics.PolicyCheckpointPath,
            created_at_utc = DateTimeOffset.UtcNow
        }, JsonOptions), ct);
        await File.WriteAllTextAsync(tokenizerPath, JsonSerializer.Serialize(new
        {
            tokenizer_kind = "cross-stack-business-flow-graph-fields",
            fields = new[] { "angular_route", "angular_service", "api_endpoint", "controller", "dto", "repository", "stored_procedure", "sql_table", "result_shape", "ui_state_consumer" }
        }, JsonOptions), ct);

        var manifest = new GeneratedCapabilityCheckpointManifest(
            CrossStackBusinessFlowGraphUnit.UnitId,
            "cross-stack-business-flow-graph",
            "project-local-wiki",
            "v1",
            "Trusted",
            CrossStackBusinessFlowGraphUnit.UnitId,
            CrossStackBusinessFlowGraphUnit.ScenarioId,
            "wiki-stage-cross-stack-business-flow",
            "cross-stack-business-flow-graph",
            "Angular client + .NET server + SQL stored procedure evidence to one business flow graph",
            $"50-row cross-stack business-flow DPO gate. train_rows={trainingResult.TrainingRowCount}; heldout_rows={trainingResult.HeldOutRowCount}.",
            $"path_recall={trainingResult.ClientToStoredProcedurePathRecall:0.######}; dpo_accuracy={dpoMetrics.FinalAccuracy:0.######}; dpo_reward_margin={dpoMetrics.FinalRewardMargin:0.######}.",
            trainingRunId,
            ["DOCUMENT", "ARCHITECTURE", "IMPLEMENT"],
            ["WIKI", "DOCUMENTATION", "CROSS_STACK", "ANGULAR", "DOTNET", "SQL_STORED_PROCEDURE"],
            policyPath,
            tokenizerPath,
            4096,
            192,
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
            UnitFamilyId: "cross-stack-business-flow-graph",
            TrainingStepCount: trainingResult.TrainingRowCount,
            OriginalPrompt: "Train CrossStackBusinessFlowGraphUnit to connect Angular client, .NET server, and SQL stored procedures as one business system.",
            TrainingBranch: "project-wiki/cross-stack-business-flow-graph",
            TrainingRunId: trainingRunId,
            SourceDatasetSha256: ComputeDatasetSha256(trainingBatch, heldOutBatch));

        await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, JsonOptions), ct);
        var uploaded = await _repository.UploadAsync(manifest, serverBaseUrl, ct);
        if (!uploaded.IsSuccess)
            return uploaded.MapError<CrossStackBusinessFlowGraphPublicationResult>();

        var registered = await _locator.RegisterAsync(uploaded.Value!, ct);
        if (!registered.IsSuccess)
            return registered.MapError<CrossStackBusinessFlowGraphPublicationResult>();

        return OperationResult<CrossStackBusinessFlowGraphPublicationResult>.Ok(new CrossStackBusinessFlowGraphPublicationResult(
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

    private static string ComputeDatasetSha256(
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch)
    {
        var text = string.Join(
            "|",
            CrossStackBusinessFlowGraphBatchIntegrity.RowFingerprintVersion,
            CrossStackBusinessFlowGraphBatchIntegrity.ComputeRowsSha256(trainingBatch),
            CrossStackBusinessFlowGraphBatchIntegrity.ComputeRowsSha256(heldOutBatch));
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }

}
