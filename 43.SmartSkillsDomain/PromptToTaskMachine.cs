// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record PromptToTaskLearnedStateDescriptor(
    string StateId,
    string ManifestId,
    string ManifestSha256,
    string MachineType,
    string SchemaVersion,
    string SourceBaseCommit,
    string StateSha256,
    IReadOnlyList<PromptToTaskLearnedPattern>? LearnedPatterns = null);

public sealed record PromptToTaskLearnedPattern(
    string PatternId,
    string TaskType,
    IReadOnlyList<string> LearnedSignals,
    IReadOnlyList<string> ExplicitUserRequirements,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> Deliverables,
    IReadOnlyList<string> ForbiddenActions,
    IReadOnlyList<string> RequiredHosts,
    IReadOnlyList<string> EvidenceNeeds,
    string SplitId,
    double Confidence);

public interface IPromptToTaskLearnedStateLoader
{
    OperationResult<PromptToTaskLearnedStateDescriptor> Load();
}

public sealed record PromptToTaskCheckpoint(
    int SchemaVersion,
    PromptToTaskLearnedStateDescriptor LearnedState,
    IReadOnlyList<string> AcceptedRefIds,
    string SourceManifestRef,
    bool SemanticQualityClaimed,
    DateTimeOffset SavedAtUtc);

public sealed record PromptToTaskCheckpointManifest(
    int SchemaVersion,
    string MachineId,
    string DomainTag,
    string Version,
    string CheckpointRef,
    string ManifestRef,
    string CheckpointSha256,
    string SourceManifestRef,
    int LearnedPatternCount,
    bool SemanticQualityClaimed,
    DateTimeOffset ExportedAtUtc);

public sealed record PromptToTaskLifecycleExport(
    string MachineId,
    string DomainTag,
    string Version,
    string CheckpointRef,
    string ManifestRef,
    string CheckpointSha256,
    DateTimeOffset ExportedAtUtc);

public sealed record PromptToTaskMachineLoadResult(
    PromptToTaskMachine Machine,
    PromptToTaskCheckpoint Checkpoint,
    PromptToTaskCheckpointManifest Manifest,
    string CheckpointRef);

public sealed class PromptToTaskMachine
{
    public const string MachineId = "prompt-to-task-machine";

    public const string DomainTag = "SmartSkillsDomain.PromptToTask";

    public const string Version = "1.0.0";

    public const string NotTrainedStatus = "not_trained_no_loaded_state";

    public const string LoadedStateNoPatternsStatus = "loaded_state_ready_no_learned_patterns";

    public const string NoMatchingPatternStatus = "loaded_state_no_matching_learned_pattern";

    public const string BaselineInternalTaskProducedStatus = "baseline_internal_semantic_task_produced";

    public const string LearnedPatternStatus = "learned_state_semantic_task_produced";

    public const string NotReadyTaskType = "not_ready_no_semantic_task";

    private const int CheckpointSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly IPromptToTaskLearnedStateLoader? _learnedStateLoader;

    private PromptToTaskLearnedStateDescriptor? _importedLearnedState;

    public PromptToTaskMachine()
    {
    }

    public PromptToTaskMachine(IPromptToTaskLearnedStateLoader learnedStateLoader)
        => _learnedStateLoader = learnedStateLoader;

    public OperationResult<PromptToTaskResponse> Translate(PromptToTaskRequest request)
    {
        var learnedState = TryLoadLearnedState();
        if (learnedState is null)
            return OperationResult<PromptToTaskResponse>.Ok(BuildNotReadyResponse(
                request,
                null,
                NotTrainedStatus,
                $"{request.PromptId}:not-trained",
                ["trained PromptToTask state must be loaded before prompt-to-task inference can be claimed"],
                BuildNotTrainedContractEvidence()));

        var unit = new PromptToTaskTaskPatternUnit(
            PromptToTaskTaskPatternUnit.FromLearnedState(learnedState));
        var unitDecision = unit.Decide(request);
        if (!unitDecision.IsSuccess)
            return unitDecision.MapError<PromptToTaskResponse>();

        var selectedPattern = unitDecision.Value!.Pattern;
        if (!unitDecision.Value.IsReady || selectedPattern is null)
        {
            var baselineFallback = TryBuildBaselineInternalTaskResponse(
                request,
                learnedState,
                unitDecision.Value);
            if (baselineFallback is not null)
                return OperationResult<PromptToTaskResponse>.Ok(baselineFallback);

            var hasPatterns = learnedState.LearnedPatterns?.Count > 0;
            var status = hasPatterns ? NoMatchingPatternStatus : LoadedStateNoPatternsStatus;
            var responseId = hasPatterns
                ? $"{request.PromptId}:no-learned-pattern-match"
                : $"{request.PromptId}:loaded-state-no-patterns";
            IReadOnlyList<string> evidenceNeeds = hasPatterns
                ? new[] { "request redacted signals must match a learned PromptToTask pattern before prompt-to-task inference can be claimed" }
                : new[] { "learned PromptToTask patterns must be present in loaded state before prompt-to-task inference can be claimed" };
            IReadOnlyList<string> evidence = hasPatterns
                ? new[]
                {
                    "LEARNED_STATE_DESCRIPTOR_LOADED",
                    "PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED",
                    "NO_MATCHING_LEARNED_PATTERN",
                    "BASELINE_NOT_SEMANTIC_QUALITY",
                    "SEMANTIC_QUALITY_CLAIMED_FALSE",
                    "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
                }
                : new[]
                {
                    "LEARNED_STATE_DESCRIPTOR_LOADED",
                    "PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED",
                    "NO_LEARNED_PATTERNS_AVAILABLE",
                    "NO_SEMANTIC_CLASSIFICATION_PERFORMED",
                    "BASELINE_NOT_SEMANTIC_QUALITY",
                    "SEMANTIC_QUALITY_CLAIMED_FALSE",
                    "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
                };

            return OperationResult<PromptToTaskResponse>.Ok(BuildNotReadyResponse(
                request,
                learnedState,
                status,
                responseId,
                evidenceNeeds,
                evidence.Concat(unitDecision.Value.EvidenceSignals).Distinct(StringComparer.Ordinal).ToArray()));
        }

        var matchedSignalCount = unitDecision.Value.MatchedSignalCount;
        var task = new StructuredTaskContract(
            PromptId: request.PromptId,
            RawPromptSha256: request.RawPromptSha256,
            UserLanguage: request.UserLanguage,
            TaskType: selectedPattern.TaskType,
            ExplicitUserRequirements: selectedPattern.ExplicitUserRequirements,
            Constraints: MergeLists(request.ExplicitConstraints, selectedPattern.Constraints),
            Deliverables: selectedPattern.Deliverables,
            ForbiddenActions: selectedPattern.ForbiddenActions,
            RequiredHosts: selectedPattern.RequiredHosts,
            EvidenceNeeds: selectedPattern.EvidenceNeeds,
            SourceProvenance: BuildSourceProvenance(request, learnedState, selectedPattern, matchedSignalCount),
            SplitId: selectedPattern.SplitId);

        var response = new PromptToTaskResponse(
            ResponseId: $"{request.PromptId}:learned-pattern:{selectedPattern.PatternId}",
            Request: request,
            StructuredTask: task,
            ContractEvidence: new[]
            {
                "LEARNED_STATE_DESCRIPTOR_LOADED",
                "PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED",
                "LEARNED_PATTERN_MATCHED",
                $"LEARNED_PATTERN_ID:{selectedPattern.PatternId}",
                $"MATCHED_SIGNAL_COUNT:{matchedSignalCount}",
                $"PROMPT_TO_TASK_TASK_PATTERN_UNIT_CONFIDENCE:{unitDecision.Value.Confidence:0.###}",
                $"PROMPT_TO_TASK_TASK_PATTERN_UNIT_TOP_TWO_MARGIN:{unitDecision.Value.TopTwoMargin:0.###}",
                "BASELINE_NOT_SEMANTIC_QUALITY",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
            }.Concat(unitDecision.Value.EvidenceSignals).Distinct(StringComparer.Ordinal).ToArray(),
            ContractStatus: LearnedPatternStatus);

        return OperationResult<PromptToTaskResponse>.Ok(response);
    }

    public OperationResult<PromptToTaskCheckpoint> ExportLearnedState(
        string sourceManifestRef,
        IReadOnlyList<string> acceptedRefIds)
    {
        var learnedState = TryLoadLearnedState();
        if (learnedState is null)
            return OperationResult<PromptToTaskCheckpoint>.Fail(
                "PromptToTask export requires a loaded learned state.",
                "PROMPT_TO_TASK_EXPORT_STATE_REQUIRED");
        if (learnedState.LearnedPatterns is null || learnedState.LearnedPatterns.Count == 0)
            return OperationResult<PromptToTaskCheckpoint>.Fail(
                "PromptToTask export requires at least one learned pattern.",
                "PROMPT_TO_TASK_EXPORT_PATTERNS_REQUIRED");
        if (string.IsNullOrWhiteSpace(sourceManifestRef))
            return OperationResult<PromptToTaskCheckpoint>.Fail(
                "Source manifest ref is required.",
                "PROMPT_TO_TASK_SOURCE_MANIFEST_REQUIRED");

        var checkpoint = new PromptToTaskCheckpoint(
            SchemaVersion: CheckpointSchemaVersion,
            LearnedState: learnedState,
            AcceptedRefIds: acceptedRefIds,
            SourceManifestRef: sourceManifestRef,
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        return OperationResult<PromptToTaskCheckpoint>.Ok(checkpoint);
    }

    public OperationResult<bool> ImportLearnedState(PromptToTaskCheckpoint checkpoint)
    {
        var validation = ValidateCheckpoint(checkpoint);
        if (!validation.IsSuccess)
            return validation;

        _importedLearnedState = checkpoint.LearnedState;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<PromptToTaskCheckpoint> ContinueWithAcceptedPattern(
        PromptToTaskLearnedPattern learnedPattern,
        string acceptedRefId)
    {
        if (learnedPattern is null)
            return OperationResult<PromptToTaskCheckpoint>.Fail(
                "Learned pattern is required for PromptToTask continue.",
                "PROMPT_TO_TASK_CONTINUE_PATTERN_REQUIRED");
        if (string.IsNullOrWhiteSpace(acceptedRefId))
            return OperationResult<PromptToTaskCheckpoint>.Fail(
                "Accepted ref id is required for PromptToTask continue.",
                "PROMPT_TO_TASK_CONTINUE_ACCEPTED_REF_REQUIRED");

        var current = TryLoadLearnedState();
        if (current is null)
            return OperationResult<PromptToTaskCheckpoint>.Fail(
                "PromptToTask continue requires an imported or loaded learned state.",
                "PROMPT_TO_TASK_CONTINUE_STATE_REQUIRED");

        var patterns = (current.LearnedPatterns ?? []).Concat([learnedPattern]).ToArray();
        var continued = current with
        {
            StateId = $"{current.StateId}:continue:{acceptedRefId}",
            StateSha256 = HashText(string.Join("|", patterns.Select(pattern => pattern.PatternId))),
            LearnedPatterns = patterns
        };
        var checkpoint = new PromptToTaskCheckpoint(
            SchemaVersion: CheckpointSchemaVersion,
            LearnedState: continued,
            AcceptedRefIds: [acceptedRefId],
            SourceManifestRef: current.ManifestId,
            SemanticQualityClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        _importedLearnedState = continued;
        return OperationResult<PromptToTaskCheckpoint>.Ok(checkpoint);
    }

    public async Task<OperationResult<PromptToTaskLifecycleExport>> SaveCheckpointAsync(
        string checkpointPath,
        string manifestPath,
        string sourceManifestRef,
        IReadOnlyList<string> acceptedRefIds,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<PromptToTaskLifecycleExport>.Fail("Save cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<PromptToTaskLifecycleExport>.Fail("Checkpoint path is required.", "PROMPT_TO_TASK_CHECKPOINT_PATH_REQUIRED");
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<PromptToTaskLifecycleExport>.Fail("Manifest path is required.", "PROMPT_TO_TASK_MANIFEST_PATH_REQUIRED");

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var fullManifestPath = Path.GetFullPath(manifestPath);
        if (string.Equals(fullCheckpointPath, fullManifestPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<PromptToTaskLifecycleExport>.Fail("Manifest path must be separate from checkpoint path.", "PROMPT_TO_TASK_MANIFEST_PATH_MUST_BE_SEPARATE");

        var checkpointResult = ExportLearnedState(sourceManifestRef, acceptedRefIds);
        if (!checkpointResult.IsSuccess)
            return OperationResult<PromptToTaskLifecycleExport>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        try
        {
            EnsureDirectoryFor(fullCheckpointPath);
            EnsureDirectoryFor(fullManifestPath);
            await WriteJsonAtomicallyAsync(fullCheckpointPath, checkpointResult.Value!, ct);
            var checkpointSha = await HashFileAsync(fullCheckpointPath, ct);
            var exportedAtUtc = DateTimeOffset.UtcNow;
            var manifestDirectory = Path.GetDirectoryName(fullManifestPath) ?? string.Empty;
            var manifest = new PromptToTaskCheckpointManifest(
                SchemaVersion: CheckpointSchemaVersion,
                MachineId: MachineId,
                DomainTag: DomainTag,
                Version: Version,
                CheckpointRef: CreatePortableManifestReference(manifestDirectory, fullCheckpointPath),
                ManifestRef: CreatePortableManifestReference(manifestDirectory, fullManifestPath),
                CheckpointSha256: checkpointSha,
                SourceManifestRef: sourceManifestRef,
                LearnedPatternCount: checkpointResult.Value!.LearnedState.LearnedPatterns?.Count ?? 0,
                SemanticQualityClaimed: false,
                ExportedAtUtc: exportedAtUtc);
            await WriteJsonAtomicallyAsync(fullManifestPath, manifest, ct);

            var export = new PromptToTaskLifecycleExport(
                MachineId,
                DomainTag,
                Version,
                fullCheckpointPath,
                fullManifestPath,
                checkpointSha,
                exportedAtUtc);
            return OperationResult<PromptToTaskLifecycleExport>.Ok(export);
        }
        catch (Exception ex)
        {
            return OperationResult<PromptToTaskLifecycleExport>.Fail(ex.Message, "PROMPT_TO_TASK_CHECKPOINT_WRITE_FAILED");
        }
    }

    public async Task<OperationResult<bool>> ImportAsync(
        PromptToTaskLifecycleExport export,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("Import cancelled.", "CANCELLED");
        if (export is null)
            return OperationResult<bool>.Fail("Export is required.", "PROMPT_TO_TASK_EXPORT_REQUIRED");
        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Machine id mismatch.", "PROMPT_TO_TASK_MACHINE_ID_MISMATCH");
        if (!string.Equals(export.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Domain tag mismatch.", "PROMPT_TO_TASK_DOMAIN_TAG_MISMATCH");

        var loadResult = await LoadCheckpointAsync(export.CheckpointRef, export.ManifestRef, ct);
        if (!loadResult.IsSuccess)
            return OperationResult<bool>.Fail(loadResult.ErrorMessage, loadResult.ErrorCode);
        if (!string.Equals(loadResult.Value!.Manifest.CheckpointSha256, export.CheckpointSha256, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Checkpoint hash mismatch.", "PROMPT_TO_TASK_CHECKPOINT_HASH_MISMATCH");

        return ImportLearnedState(loadResult.Value.Checkpoint);
    }

    public static async Task<OperationResult<PromptToTaskMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        string manifestPath,
        CancellationToken ct = default)
    {
        var checkpointResult = await ReadCheckpointAsync(checkpointPath, ct);
        if (!checkpointResult.IsSuccess)
            return OperationResult<PromptToTaskMachineLoadResult>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        var manifestResult = await ReadManifestAsync(manifestPath, ct);
        if (!manifestResult.IsSuccess)
            return OperationResult<PromptToTaskMachineLoadResult>.Fail(manifestResult.ErrorMessage, manifestResult.ErrorCode);

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var fullManifestPath = Path.GetFullPath(manifestPath);
        var manifestValidation = await ValidateManifestAsync(manifestResult.Value!, fullCheckpointPath, fullManifestPath, ct);
        if (!manifestValidation.IsSuccess)
            return OperationResult<PromptToTaskMachineLoadResult>.Fail(manifestValidation.ErrorMessage, manifestValidation.ErrorCode);

        var machine = new PromptToTaskMachine();
        var importResult = machine.ImportLearnedState(checkpointResult.Value!);
        if (!importResult.IsSuccess)
            return OperationResult<PromptToTaskMachineLoadResult>.Fail(importResult.ErrorMessage, importResult.ErrorCode);

        return OperationResult<PromptToTaskMachineLoadResult>.Ok(new PromptToTaskMachineLoadResult(
            machine,
            checkpointResult.Value!,
            manifestResult.Value!,
            fullCheckpointPath));
    }

    private PromptToTaskLearnedStateDescriptor? TryLoadLearnedState()
    {
        if (_importedLearnedState is not null)
            return _importedLearnedState;

        var result = _learnedStateLoader?.Load();
        if (result is null || !result.IsSuccess)
            return null;

        return result.Value;
    }

    private static PromptToTaskResponse BuildNotReadyResponse(
        PromptToTaskRequest request,
        PromptToTaskLearnedStateDescriptor? learnedState,
        string status,
        string responseId,
        IReadOnlyList<string> evidenceNeeds,
        IReadOnlyList<string> contractEvidence)
    {
        var task = new StructuredTaskContract(
            PromptId: request.PromptId,
            RawPromptSha256: request.RawPromptSha256,
            UserLanguage: request.UserLanguage,
            TaskType: NotReadyTaskType,
            ExplicitUserRequirements: [],
            Constraints: request.ExplicitConstraints,
            Deliverables: [],
            ForbiddenActions: [],
            RequiredHosts: [],
            EvidenceNeeds: evidenceNeeds,
            SourceProvenance: BuildSourceProvenance(request, learnedState, null, 0),
            SplitId: learnedState is null ? "not-ready" : "loaded-state-no-semantic-task");

        return new PromptToTaskResponse(
            ResponseId: responseId,
            Request: request,
            StructuredTask: task,
            ContractEvidence: contractEvidence,
            ContractStatus: status);
    }

    private static PromptToTaskResponse? TryBuildBaselineInternalTaskResponse(
        PromptToTaskRequest request,
        PromptToTaskLearnedStateDescriptor learnedState,
        PromptToTaskTaskPatternUnitDecision unitDecision)
    {
        var genericSignals = BuildGenericRuntimeSignals(request);
        if (genericSignals.Count < 2
            || !genericSignals.Contains("extension_api_boundary_present", StringComparer.OrdinalIgnoreCase))
            return null;

        var task = new StructuredTaskContract(
            PromptId: request.PromptId,
            RawPromptSha256: request.RawPromptSha256,
            UserLanguage: request.UserLanguage,
            TaskType: "baseline_internal_universal_task",
            ExplicitUserRequirements:
            [
                "derive a universal task contract from the redacted request signals",
                "preserve user constraints without stack-specific shortcut authority"
            ],
            Constraints: MergeLists(
                request.ExplicitConstraints,
                [
                    "baseline_internal_only",
                    "semantic_quality_claimed_false",
                    "deterministic_shortcut_forbidden"
                ]),
            Deliverables:
            [
                "structured task contract",
                "runtime evidence requirements",
                "claim-boundary notes"
            ],
            ForbiddenActions:
            [
                "claim semantic quality from baseline/internal fallback",
                "use exact source, pair id, or scenario id as final authority",
                "mutate common checkpoints"
            ],
            RequiredHosts: ["host_boundary_from_request"],
            EvidenceNeeds:
            [
                "redacted input/output trace",
                "source-disjoint evaluation gate",
                "ablation and leakage controls",
                "fresh-load and continue-training proof"
            ],
            SourceProvenance: BuildBaselineInternalSourceProvenance(request, learnedState, genericSignals.Count),
            SplitId: "baseline-internal-universal-task");

        return new PromptToTaskResponse(
            ResponseId: $"{request.PromptId}:baseline-internal-task",
            Request: request,
            StructuredTask: task,
            ContractEvidence:
            [
                "LEARNED_STATE_DESCRIPTOR_LOADED",
                "PROMPT_TO_TASK_TASK_PATTERN_UNIT_USED",
                "NO_MATCHING_LEARNED_PATTERN",
                "BASELINE_INTERNAL_FALLBACK_USED",
                "BASELINE_INTERNAL_TASK_PRODUCED",
                $"BASELINE_INTERNAL_SIGNAL_COUNT:{genericSignals.Count}",
                "BASELINE_NOT_SEMANTIC_QUALITY",
                "SEMANTIC_QUALITY_CLAIMED_FALSE",
                "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
            ],
            ContractStatus: BaselineInternalTaskProducedStatus);
    }

    private static IReadOnlyList<string> BuildGenericRuntimeSignals(PromptToTaskRequest request)
    {
        var signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(request.RawPrompt))
            signals.Add("raw_prompt_present");
        if (!string.IsNullOrWhiteSpace(request.RawPromptSha256))
            signals.Add("raw_prompt_hash_present");
        if (request.ContextFields.Count > 0)
            signals.Add("context_fields_present");
        if (request.ContextFields.ContainsKey("raw_prompt_redacted"))
            signals.Add("raw_prompt_redacted");
        if (request.ContextFields.ContainsKey("xiigen_method")
            && request.ContextFields.ContainsKey("raw_prompt_redacted"))
        {
            signals.Add("extension_api_boundary_present");
        }
        if (request.ExplicitConstraints.Count > 0)
            signals.Add("explicit_constraints_present");

        foreach (var field in request.ContextFields)
        {
            if (field.Key.StartsWith("host_fact.", StringComparison.Ordinal)
                || field.Key.StartsWith("provenance_key.", StringComparison.Ordinal))
            {
                signals.Add("host_or_provenance_boundary_present");
                break;
            }
        }

        return signals.Order(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyDictionary<string, string> BuildBaselineInternalSourceProvenance(
        PromptToTaskRequest request,
        PromptToTaskLearnedStateDescriptor learnedState,
        int genericSignalCount)
        => new Dictionary<string, string>
        {
            ["prompt_provenance"] = request.PromptProvenance,
            ["semantic_translation"] = "baseline_internal_runtime_fallback",
            ["machine_state"] = "descriptor_loaded",
            ["state_id"] = learnedState.StateId,
            ["manifest_id"] = learnedState.ManifestId,
            ["manifest_sha256"] = learnedState.ManifestSha256,
            ["machine_type"] = learnedState.MachineType,
            ["schema_version"] = learnedState.SchemaVersion,
            ["source_base_commit"] = learnedState.SourceBaseCommit,
            ["state_sha256"] = learnedState.StateSha256,
            ["generic_runtime_signal_count"] = genericSignalCount.ToString(),
            ["semantic_quality_claimed"] = "false"
        };

    private static IReadOnlyList<string> MergeLists(
        IReadOnlyList<string> requestConstraints,
        IReadOnlyList<string> patternConstraints)
        => requestConstraints
            .Concat(patternConstraints)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyDictionary<string, string> BuildSourceProvenance(
        PromptToTaskRequest request,
        PromptToTaskLearnedStateDescriptor? learnedState,
        PromptToTaskTaskPatternUnitPattern? learnedPattern,
        int matchedSignalCount)
    {
        var provenance = new Dictionary<string, string>
        {
            ["prompt_provenance"] = request.PromptProvenance,
            ["semantic_translation"] = "not_performed"
        };

        if (learnedState is null)
        {
            provenance["machine_state"] = "none_loaded";
            return provenance;
        }

        provenance["machine_state"] = "descriptor_loaded";
        provenance["state_id"] = learnedState.StateId;
        provenance["manifest_id"] = learnedState.ManifestId;
        provenance["manifest_sha256"] = learnedState.ManifestSha256;
        provenance["machine_type"] = learnedState.MachineType;
        provenance["schema_version"] = learnedState.SchemaVersion;
        provenance["source_base_commit"] = learnedState.SourceBaseCommit;
        provenance["state_sha256"] = learnedState.StateSha256;

        if (learnedPattern is null)
            return provenance;

        provenance["semantic_translation"] = "learned_pattern_state_evaluation";
        provenance["learned_pattern_id"] = learnedPattern.PatternId;
        provenance["matched_signal_count"] = matchedSignalCount.ToString();
        provenance["learned_pattern_confidence"] = learnedPattern.Confidence.ToString("0.###");
        return provenance;
    }

    private static IReadOnlyList<string> BuildNotTrainedContractEvidence()
        =>
        [
            "NO_TRAINED_STATE_LOADED",
            "NO_SEMANTIC_CLASSIFICATION_PERFORMED",
            "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
        ];

    private static OperationResult<bool> ValidateCheckpoint(PromptToTaskCheckpoint? checkpoint)
    {
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint deserialized null.", "PROMPT_TO_TASK_CHECKPOINT_DESERIALIZE_FAILED");
        if (checkpoint.SchemaVersion != CheckpointSchemaVersion)
            return OperationResult<bool>.Fail("Checkpoint schema is unsupported.", "PROMPT_TO_TASK_CHECKPOINT_SCHEMA_UNSUPPORTED");
        if (checkpoint.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Checkpoint must not claim semantic quality in PTT-04.", "PROMPT_TO_TASK_SEMANTIC_QUALITY_CLAIM_UNSUPPORTED");
        if (checkpoint.LearnedState is null)
            return OperationResult<bool>.Fail("Checkpoint learned state is required.", "PROMPT_TO_TASK_CHECKPOINT_STATE_REQUIRED");
        if (!string.Equals(checkpoint.LearnedState.MachineType, nameof(PromptToTaskMachine), StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Checkpoint machine type mismatch.", "PROMPT_TO_TASK_CHECKPOINT_MACHINE_TYPE_MISMATCH");
        if (checkpoint.LearnedState.LearnedPatterns is null || checkpoint.LearnedState.LearnedPatterns.Count == 0)
            return OperationResult<bool>.Fail("Checkpoint requires learned patterns.", "PROMPT_TO_TASK_CHECKPOINT_PATTERNS_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }

    private static async Task<OperationResult<PromptToTaskCheckpoint>> ReadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<PromptToTaskCheckpoint>.Fail("Load cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<PromptToTaskCheckpoint>.Fail("Checkpoint path is required.", "PROMPT_TO_TASK_CHECKPOINT_PATH_REQUIRED");
        if (!File.Exists(checkpointPath))
            return OperationResult<PromptToTaskCheckpoint>.Fail("Checkpoint file not found.", "PROMPT_TO_TASK_CHECKPOINT_NOT_FOUND");

        try
        {
            var json = await File.ReadAllTextAsync(checkpointPath, Encoding.UTF8, ct);
            var checkpoint = JsonSerializer.Deserialize<PromptToTaskCheckpoint>(json, JsonOptions);
            var validation = ValidateCheckpoint(checkpoint);
            return validation.IsSuccess
                ? OperationResult<PromptToTaskCheckpoint>.Ok(checkpoint!)
                : OperationResult<PromptToTaskCheckpoint>.Fail(validation.ErrorMessage, validation.ErrorCode);
        }
        catch (Exception ex)
        {
            return OperationResult<PromptToTaskCheckpoint>.Fail(ex.Message, "PROMPT_TO_TASK_CHECKPOINT_READ_FAILED");
        }
    }

    private static async Task<OperationResult<PromptToTaskCheckpointManifest>> ReadManifestAsync(
        string manifestPath,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<PromptToTaskCheckpointManifest>.Fail("Manifest path is required.", "PROMPT_TO_TASK_MANIFEST_PATH_REQUIRED");
        if (!File.Exists(manifestPath))
            return OperationResult<PromptToTaskCheckpointManifest>.Fail("Manifest file not found.", "PROMPT_TO_TASK_MANIFEST_NOT_FOUND");

        try
        {
            var json = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8, ct);
            var manifest = JsonSerializer.Deserialize<PromptToTaskCheckpointManifest>(json, JsonOptions);
            return manifest is null
                ? OperationResult<PromptToTaskCheckpointManifest>.Fail("Manifest deserialized null.", "PROMPT_TO_TASK_MANIFEST_DESERIALIZE_FAILED")
                : OperationResult<PromptToTaskCheckpointManifest>.Ok(manifest);
        }
        catch (Exception ex)
        {
            return OperationResult<PromptToTaskCheckpointManifest>.Fail(ex.Message, "PROMPT_TO_TASK_MANIFEST_READ_FAILED");
        }
    }

    private static async Task<OperationResult<bool>> ValidateManifestAsync(
        PromptToTaskCheckpointManifest manifest,
        string checkpointPath,
        string manifestPath,
        CancellationToken ct)
    {
        if (manifest.SchemaVersion != CheckpointSchemaVersion)
            return OperationResult<bool>.Fail("Manifest schema is unsupported.", "PROMPT_TO_TASK_MANIFEST_SCHEMA_UNSUPPORTED");
        if (!string.Equals(manifest.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest machine id mismatch.", "PROMPT_TO_TASK_MANIFEST_MACHINE_ID_MISMATCH");
        if (!string.Equals(manifest.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest domain tag mismatch.", "PROMPT_TO_TASK_MANIFEST_DOMAIN_TAG_MISMATCH");
        var manifestDirectory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
        var resolvedCheckpointRef = ResolveManifestReference(manifestDirectory, manifest.CheckpointRef);
        var resolvedManifestRef = ResolveManifestReference(manifestDirectory, manifest.ManifestRef);
        if (!string.Equals(resolvedCheckpointRef, checkpointPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<bool>.Fail("Manifest checkpoint ref mismatch.", "PROMPT_TO_TASK_MANIFEST_CHECKPOINT_REF_MISMATCH");
        if (!string.Equals(resolvedManifestRef, manifestPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<bool>.Fail("Manifest ref mismatch.", "PROMPT_TO_TASK_MANIFEST_REF_MISMATCH");
        if (manifest.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Manifest must not claim semantic quality in PTT-04.", "PROMPT_TO_TASK_MANIFEST_SEMANTIC_QUALITY_CLAIM_UNSUPPORTED");

        var checkpointSha = await HashFileAsync(checkpointPath, ct);
        if (!string.Equals(manifest.CheckpointSha256, checkpointSha, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest checkpoint hash mismatch.", "PROMPT_TO_TASK_MANIFEST_CHECKPOINT_HASH_MISMATCH");

        return OperationResult<bool>.Ok(true);
    }

    private static string ResolveManifestReference(string manifestDirectory, string reference)
        => Path.GetFullPath(Path.IsPathRooted(reference) ? reference : Path.Combine(manifestDirectory, reference));

    private static string CreatePortableManifestReference(string manifestDirectory, string targetPath)
    {
        var relative = Path.GetRelativePath(manifestDirectory, targetPath);
        return relative.StartsWith("..", StringComparison.Ordinal)
            ? targetPath
            : relative.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static void EnsureDirectoryFor(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }

    private static async Task WriteJsonAtomicallyAsync<T>(
        string path,
        T value,
        CancellationToken ct)
    {
        var json = NormalizeTextArtifact(JsonSerializer.Serialize(value, JsonOptions));
        var tmpPath = path + ".tmp";
        await File.WriteAllTextAsync(tmpPath, json, Utf8NoBom, ct);
        File.Move(tmpPath, path, overwrite: true);
    }

    private static string NormalizeTextArtifact(string text)
    {
        var normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        var lines = normalized
            .Split('\n')
            .Select(line => line.TrimEnd(' ', '\t'));
        return string.Join("\n", lines);
    }

    private static async Task<string> HashFileAsync(string path, CancellationToken ct)
    {
        var bytes = await File.ReadAllBytesAsync(path, ct);
        return HashBytes(bytes);
    }

    private static string HashText(string value)
        => HashBytes(Encoding.UTF8.GetBytes(value));

    private static string HashBytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed class FileBackedPromptToTaskLearnedStateLoader(
    string checkpointPath,
    string manifestPath)
    : IPromptToTaskLearnedStateLoader
{
    public OperationResult<PromptToTaskLearnedStateDescriptor> Load()
    {
        var result = PromptToTaskMachine.LoadCheckpointAsync(checkpointPath, manifestPath)
            .GetAwaiter()
            .GetResult();
        return result.IsSuccess
            ? OperationResult<PromptToTaskLearnedStateDescriptor>.Ok(result.Value!.Checkpoint.LearnedState)
            : OperationResult<PromptToTaskLearnedStateDescriptor>.Fail(result.ErrorMessage, result.ErrorCode);
    }
}
