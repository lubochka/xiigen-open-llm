// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

public sealed record OrchestrationReasoningLearnedGuideStateDescriptor(
    string StateId,
    string GuideStateRef,
    string ManifestId,
    string ManifestSha256,
    string MachineType,
    string SchemaVersion,
    string SourceBaseCommit,
    string StateSha256,
    IReadOnlyList<OrchestrationReasoningLearnedPattern>? LearnedPatterns = null);

public sealed record OrchestrationReasoningLearnedPattern(
    string PatternId,
    string ScenarioRef,
    IReadOnlyList<string> RequiredTaskTypes,
    IReadOnlyList<string> RequiredConstraintSignals,
    IReadOnlyList<string> RequiredRequirementSignals,
    IReadOnlyList<string> RequiredDeliverableSignals,
    IReadOnlyList<string> RequiredEvidenceSignals,
    IReadOnlyList<string> PhaseFamilies,
    IReadOnlyList<string> GateFamilies,
    IReadOnlyList<string> AssetFamilies,
    IReadOnlyList<string> HostCheckSets,
    IReadOnlyList<string> RiskFlags,
    IReadOnlyList<string> NextGranularizationActions,
    double Confidence);

public interface IOrchestrationReasoningLearnedGuideStateLoader
{
    OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load();
}

public sealed record OrchestrationReasoningCheckpoint(
    int SchemaVersion,
    OrchestrationReasoningLearnedGuideStateDescriptor LearnedState,
    IReadOnlyList<string> AcceptedRefIds,
    string SourceManifestRef,
    bool SemanticQualityClaimed,
    bool ContinueTrainingClaimed,
    bool HostRuntimeClaimed,
    DateTimeOffset SavedAtUtc);

public sealed record OrchestrationReasoningCheckpointManifest(
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
    bool ContinueTrainingClaimed,
    bool HostRuntimeClaimed,
    DateTimeOffset ExportedAtUtc);

public sealed record OrchestrationReasoningLifecycleExport(
    string MachineId,
    string DomainTag,
    string Version,
    string CheckpointRef,
    string ManifestRef,
    string CheckpointSha256,
    DateTimeOffset ExportedAtUtc);

public sealed record OrchestrationReasoningMachineLoadResult(
    OrchestrationReasoningMachine Machine,
    OrchestrationReasoningCheckpoint Checkpoint,
    OrchestrationReasoningCheckpointManifest Manifest,
    string CheckpointRef);

public sealed record OrchestrationReasoningContinueTrainingResult(
    OrchestrationReasoningLearnedGuideStateDescriptor MergedLearnedState,
    IReadOnlyList<string> MergedAcceptedRefIds,
    IReadOnlyList<string> AddedPatternIds,
    bool ContinueTrainingRunClaimed,
    bool CheckpointRewritten,
    bool SemanticQualityClaimed,
    bool HostRuntimeClaimed);

public sealed class OrchestrationReasoningMachine
{
    public const string MachineId = "orchestration-reasoning-machine";

    public const string DomainTag = "SmartSkillsDomain.OrchestrationReasoning";

    public const string Version = "1.0.0";

    public const string NotTrainedStatus = "not_trained_no_learned_guide_state";

    public const string LoadedGuideStateReadyStatus = "loaded_guide_state_ready_no_learned_patterns";

    public const string NoMatchingPatternStatus = "loaded_guide_state_no_matching_learned_pattern";

    public const string BaselineInternalPlanProducedStatus = "baseline_internal_semantic_plan_produced";

    public const string LearnedPatternStatus = "loaded_guide_state_semantic_plan_produced";

    public const string PublicPlanProducedStatus = "orchestration_plan_produced";

    public const string NoInferenceScenarioId = "not-ready-no-orchestration-inference";

    private const int CheckpointSchemaVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IOrchestrationReasoningLearnedGuideStateLoader? _learnedGuideStateLoader;

    private OrchestrationReasoningLearnedGuideStateDescriptor? _importedLearnedGuideState;

    private IReadOnlyList<string> _importedAcceptedRefIds = [];

    public OrchestrationReasoningMachine()
    {
    }

    public OrchestrationReasoningMachine(IOrchestrationReasoningLearnedGuideStateLoader learnedGuideStateLoader)
        => _learnedGuideStateLoader = learnedGuideStateLoader;

    public OperationResult<OrchestrationReasoningResponse> Plan(OrchestrationReasoningRequest request)
    {
        var learnedGuideState = TryLoadLearnedGuideState();
        if (learnedGuideState is null)
            return OperationResult<OrchestrationReasoningResponse>.Ok(BuildNoSemanticPlanResponse(
                request,
                NotTrainedStatus,
                $"{request.RequestId}:not-trained",
                ["NO_LEARNED_GUIDE_STATE_LOADED", "NO_ORCHESTRATION_PLAN_GENERATED"],
                ["load learned OrchestrationReasoning guide state before semantic scenario planning"],
                BuildContractEvidence(null, null, 0)));

        var unit = new OrchestrationReasoningScenarioUnit(
            OrchestrationReasoningScenarioUnit.FromLearnedState(learnedGuideState));
        var unitDecision = unit.Decide(request);
        if (!unitDecision.IsSuccess)
            return unitDecision.MapError<OrchestrationReasoningResponse>();

        var chosenPattern = unitDecision.Value!.Pattern;
        if (!unitDecision.Value.IsReady || chosenPattern is null)
        {
            var baselineFallback = TryBuildBaselineInternalPlanResponse(
                request,
                learnedGuideState,
                unitDecision.Value);
            if (baselineFallback is not null)
                return OperationResult<OrchestrationReasoningResponse>.Ok(baselineFallback);

            var hasPatterns = learnedGuideState.LearnedPatterns?.Count > 0;
            return OperationResult<OrchestrationReasoningResponse>.Ok(BuildNoSemanticPlanResponse(
                request,
                hasPatterns ? NoMatchingPatternStatus : LoadedGuideStateReadyStatus,
                hasPatterns
                    ? $"{request.RequestId}:no-learned-guide-pattern-match"
                    : $"{request.RequestId}:loaded-guide-state-no-patterns",
                hasPatterns
                    ? ["LEARNED_GUIDE_STATE_DESCRIPTOR_LOADED", "ORCHESTRATION_REASONING_SCENARIO_UNIT_USED", "NO_MATCHING_LEARNED_GUIDE_PATTERN"]
                    : ["LEARNED_GUIDE_STATE_DESCRIPTOR_LOADED", "ORCHESTRATION_REASONING_SCENARIO_UNIT_USED", "NO_LEARNED_GUIDE_PATTERNS_AVAILABLE"],
                hasPatterns
                    ? ["structured task signals must match a learned OrchestrationReasoning pattern before semantic planning"]
                    : ["learned guide patterns must be present in loaded state before semantic scenario planning"],
                BuildContractEvidence(learnedGuideState, null, 0, unitDecision.Value)));
        }

        var matchedSignalCount = unitDecision.Value.MatchedSignalCount;
        var scenarioPlan = new OrchestrationScenarioPlan(
            ScenarioId: chosenPattern.ScenarioRef,
            Task: request.Task,
            SelectedLearnedPatterns: [chosenPattern.PatternId],
            PhaseFamilies: chosenPattern.PhaseFamilies,
            GateFamilies: chosenPattern.GateFamilies,
            AssetFamilies: chosenPattern.AssetFamilies,
            HostCheckSets: chosenPattern.HostCheckSets,
            RiskFlags: chosenPattern.RiskFlags,
            NextGranularizationActions: chosenPattern.NextGranularizationActions);

        var response = new OrchestrationReasoningResponse(
            ResponseId: $"{request.RequestId}:learned-guide-pattern:{chosenPattern.PatternId}",
            Request: request,
            ScenarioPlan: scenarioPlan,
            ContractEvidence: BuildContractEvidence(learnedGuideState, chosenPattern, matchedSignalCount, unitDecision.Value),
            ContractStatus: LearnedPatternStatus);

        return OperationResult<OrchestrationReasoningResponse>.Ok(response);
    }

    public OperationResult<OrchestrationReasoningCheckpoint> ExportLearnedState(
        string sourceManifestRef,
        IReadOnlyList<string> acceptedRefIds)
    {
        var learnedState = TryLoadLearnedGuideState();
        if (learnedState is null)
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "OrchestrationReasoning export requires a loaded learned guide state.",
                "ORCHESTRATION_REASONING_EXPORT_STATE_REQUIRED");
        if (learnedState.LearnedPatterns is null || learnedState.LearnedPatterns.Count == 0)
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "OrchestrationReasoning export requires at least one learned guide pattern.",
                "ORCHESTRATION_REASONING_EXPORT_PATTERNS_REQUIRED");
        if (string.IsNullOrWhiteSpace(sourceManifestRef))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "Source manifest ref is required.",
                "ORCHESTRATION_REASONING_SOURCE_MANIFEST_REQUIRED");

        var checkpoint = new OrchestrationReasoningCheckpoint(
            SchemaVersion: CheckpointSchemaVersion,
            LearnedState: learnedState,
            AcceptedRefIds: acceptedRefIds,
            SourceManifestRef: sourceManifestRef,
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        return OperationResult<OrchestrationReasoningCheckpoint>.Ok(checkpoint);
    }

    public OperationResult<bool> ImportLearnedState(OrchestrationReasoningCheckpoint checkpoint)
    {
        var validation = ValidateCheckpoint(checkpoint);
        if (!validation.IsSuccess)
            return validation;

        _importedLearnedGuideState = checkpoint.LearnedState;
        _importedAcceptedRefIds = checkpoint.AcceptedRefIds;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<OrchestrationReasoningContinueTrainingResult> ContinueTrainingFromLearnedState(
        OrchestrationReasoningLearnedGuideStateDescriptor deltaState,
        IReadOnlyList<string> deltaAcceptedRefIds)
    {
        var existingState = _importedLearnedGuideState;
        if (existingState is null)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires an imported checkpoint learned guide state with preserved accepted refs.",
                "ORCHESTRATION_REASONING_CONTINUE_IMPORTED_CHECKPOINT_STATE_REQUIRED");
        if (_importedAcceptedRefIds.Count == 0)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires preserved accepted refs from the imported checkpoint.",
                "ORCHESTRATION_REASONING_CONTINUE_IMPORTED_ACCEPTED_REFS_REQUIRED");
        if (deltaState is null)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires a delta learned guide state.",
                "ORCHESTRATION_REASONING_CONTINUE_DELTA_STATE_REQUIRED");
        if (deltaState.LearnedPatterns is null || deltaState.LearnedPatterns.Count == 0)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires delta learned patterns.",
                "ORCHESTRATION_REASONING_CONTINUE_DELTA_PATTERNS_REQUIRED");

        var existingPatterns = existingState.LearnedPatterns ?? [];
        if (existingPatterns.Count == 0)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires existing learned patterns.",
                "ORCHESTRATION_REASONING_CONTINUE_EXISTING_PATTERNS_REQUIRED");

        var existingPatternIds = existingPatterns
            .Select(pattern => pattern.PatternId)
            .ToHashSet(StringComparer.Ordinal);
        var deltaPatternIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var deltaPattern in deltaState.LearnedPatterns)
        {
            if (string.IsNullOrWhiteSpace(deltaPattern.PatternId))
                return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                    "Delta learned pattern id is required.",
                    "ORCHESTRATION_REASONING_CONTINUE_DELTA_PATTERN_ID_REQUIRED");
            if (existingPatternIds.Contains(deltaPattern.PatternId))
                return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                    "Delta learned pattern id collides with existing learned state.",
                    "ORCHESTRATION_REASONING_CONTINUE_DUPLICATE_PATTERN_ID");
            if (!deltaPatternIds.Add(deltaPattern.PatternId))
                return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                    "Delta learned pattern ids must be unique.",
                    "ORCHESTRATION_REASONING_CONTINUE_DUPLICATE_DELTA_PATTERN_ID");
        }

        var mergedPatterns = existingPatterns.Concat(deltaState.LearnedPatterns).ToArray();
        var stateMaterial = JsonSerializer.Serialize(mergedPatterns, JsonOptions);
        var mergedStateSha = HashBytes(Encoding.UTF8.GetBytes(stateMaterial));
        var mergedState = existingState with
        {
            StateId = $"{existingState.StateId}:continued:{deltaState.StateId}",
            GuideStateRef = $"{existingState.GuideStateRef}:continued",
            SourceBaseCommit = $"{existingState.SourceBaseCommit}+delta:{deltaState.SourceBaseCommit}",
            StateSha256 = mergedStateSha,
            LearnedPatterns = mergedPatterns
        };
        var mergedAcceptedRefs = _importedAcceptedRefIds
            .Concat(deltaAcceptedRefIds ?? [])
            .Where(refId => !string.IsNullOrWhiteSpace(refId))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        _importedLearnedGuideState = mergedState;
        _importedAcceptedRefIds = mergedAcceptedRefs;

        var result = new OrchestrationReasoningContinueTrainingResult(
            MergedLearnedState: mergedState,
            MergedAcceptedRefIds: mergedAcceptedRefs,
            AddedPatternIds: deltaState.LearnedPatterns.Select(pattern => pattern.PatternId).ToArray(),
            ContinueTrainingRunClaimed: false,
            CheckpointRewritten: false,
            SemanticQualityClaimed: false,
            HostRuntimeClaimed: false);

        return OperationResult<OrchestrationReasoningContinueTrainingResult>.Ok(result);
    }

    public async Task<OperationResult<OrchestrationReasoningLifecycleExport>> SaveCheckpointAsync(
        string checkpointPath,
        string manifestPath,
        string sourceManifestRef,
        IReadOnlyList<string> acceptedRefIds,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail("Save cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail("Checkpoint path is required.", "ORCHESTRATION_REASONING_CHECKPOINT_PATH_REQUIRED");
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail("Manifest path is required.", "ORCHESTRATION_REASONING_MANIFEST_PATH_REQUIRED");

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var fullManifestPath = Path.GetFullPath(manifestPath);
        if (string.Equals(fullCheckpointPath, fullManifestPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail("Manifest path must be separate from checkpoint path.", "ORCHESTRATION_REASONING_MANIFEST_PATH_MUST_BE_SEPARATE");

        var checkpointResult = ExportLearnedState(sourceManifestRef, acceptedRefIds);
        if (!checkpointResult.IsSuccess)
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        try
        {
            EnsureDirectoryFor(fullCheckpointPath);
            EnsureDirectoryFor(fullManifestPath);
            await WriteJsonAtomicallyAsync(fullCheckpointPath, checkpointResult.Value!, ct);
            var checkpointSha = await HashFileAsync(fullCheckpointPath, ct);
            var exportedAtUtc = DateTimeOffset.UtcNow;
            var manifestDirectory = Path.GetDirectoryName(fullManifestPath) ?? string.Empty;
            var manifest = new OrchestrationReasoningCheckpointManifest(
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
                ContinueTrainingClaimed: false,
                HostRuntimeClaimed: false,
                ExportedAtUtc: exportedAtUtc);
            await WriteJsonAtomicallyAsync(fullManifestPath, manifest, ct);

            var export = new OrchestrationReasoningLifecycleExport(
                MachineId,
                DomainTag,
                Version,
                fullCheckpointPath,
                fullManifestPath,
                checkpointSha,
                exportedAtUtc);
            return OperationResult<OrchestrationReasoningLifecycleExport>.Ok(export);
        }
        catch (Exception ex)
        {
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(ex.Message, "ORCHESTRATION_REASONING_CHECKPOINT_WRITE_FAILED");
        }
    }

    public async Task<OperationResult<bool>> ImportAsync(
        OrchestrationReasoningLifecycleExport export,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<bool>.Fail("Import cancelled.", "CANCELLED");
        if (export is null)
            return OperationResult<bool>.Fail("Export is required.", "ORCHESTRATION_REASONING_EXPORT_REQUIRED");
        if (!string.Equals(export.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Machine id mismatch.", "ORCHESTRATION_REASONING_MACHINE_ID_MISMATCH");
        if (!string.Equals(export.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Domain tag mismatch.", "ORCHESTRATION_REASONING_DOMAIN_TAG_MISMATCH");

        var loadResult = await LoadCheckpointAsync(export.CheckpointRef, export.ManifestRef, ct);
        if (!loadResult.IsSuccess)
            return OperationResult<bool>.Fail(loadResult.ErrorMessage, loadResult.ErrorCode);
        if (!string.Equals(loadResult.Value!.Manifest.CheckpointSha256, export.CheckpointSha256, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Checkpoint hash mismatch.", "ORCHESTRATION_REASONING_CHECKPOINT_HASH_MISMATCH");

        return ImportLearnedState(loadResult.Value.Checkpoint);
    }

    public static async Task<OperationResult<OrchestrationReasoningMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        string manifestPath,
        CancellationToken ct = default)
    {
        var checkpointResult = await ReadCheckpointAsync(checkpointPath, ct);
        if (!checkpointResult.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        var manifestResult = await ReadManifestAsync(manifestPath, ct);
        if (!manifestResult.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(manifestResult.ErrorMessage, manifestResult.ErrorCode);

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var fullManifestPath = Path.GetFullPath(manifestPath);
        var manifestValidation = await ValidateManifestAsync(
            checkpointResult.Value!,
            manifestResult.Value!,
            fullCheckpointPath,
            fullManifestPath,
            ct);
        if (!manifestValidation.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(manifestValidation.ErrorMessage, manifestValidation.ErrorCode);

        var machine = new OrchestrationReasoningMachine();
        var importResult = machine.ImportLearnedState(checkpointResult.Value!);
        if (!importResult.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(importResult.ErrorMessage, importResult.ErrorCode);

        return OperationResult<OrchestrationReasoningMachineLoadResult>.Ok(new OrchestrationReasoningMachineLoadResult(
            machine,
            checkpointResult.Value!,
            manifestResult.Value!,
            fullCheckpointPath));
    }

    private OrchestrationReasoningLearnedGuideStateDescriptor? TryLoadLearnedGuideState()
    {
        if (_importedLearnedGuideState is not null)
            return _importedLearnedGuideState;

        var result = _learnedGuideStateLoader?.Load();
        if (result is null || !result.IsSuccess)
            return null;

        return result.Value;
    }

    private static OrchestrationReasoningResponse BuildNoSemanticPlanResponse(
        OrchestrationReasoningRequest request,
        string status,
        string responseId,
        IReadOnlyList<string> riskFlags,
        IReadOnlyList<string> nextGranularizationActions,
        IReadOnlyList<string> contractEvidence)
    {
        var scenarioPlan = new OrchestrationScenarioPlan(
            ScenarioId: NoInferenceScenarioId,
            Task: request.Task,
            SelectedLearnedPatterns: [],
            PhaseFamilies: [],
            GateFamilies: [],
            AssetFamilies: [],
            HostCheckSets: [],
            RiskFlags: riskFlags,
            NextGranularizationActions: nextGranularizationActions);

        return new OrchestrationReasoningResponse(
            ResponseId: responseId,
            Request: request,
            ScenarioPlan: scenarioPlan,
            ContractEvidence: contractEvidence,
            ContractStatus: status);
    }

    private static OrchestrationReasoningResponse? TryBuildBaselineInternalPlanResponse(
        OrchestrationReasoningRequest request,
        OrchestrationReasoningLearnedGuideStateDescriptor learnedGuideState,
        OrchestrationReasoningScenarioDecision unitDecision)
    {
        var genericSignals = BuildGenericRuntimeSignals(request);
        if (genericSignals.Count < 2
            || !genericSignals.Contains("prompt_to_task_baseline_internal_task", StringComparer.OrdinalIgnoreCase))
            return null;

        var scenarioPlan = new OrchestrationScenarioPlan(
            ScenarioId: "baseline-internal-universal-plan",
            Task: request.Task,
            SelectedLearnedPatterns: [],
            PhaseFamilies: ["runtime-quality-gates"],
            GateFamilies:
            [
                "source-disjoint-eval",
                "ablation-control",
                "leakage-control",
                "fresh-load",
                "continue-training"
            ],
            AssetFamilies: ["common-baseline-internal"],
            HostCheckSets: ["host-boundary-from-request"],
            RiskFlags:
            [
                "baseline_internal_not_semantic_quality",
                "semantic_quality_claimed_false",
                "host_runtime_claimed_false",
                "no_checkpoint_mutation"
            ],
            NextGranularizationActions:
            [
                "score runtime rows before any readiness claim",
                "keep semantic and token claims closed until all proof gates pass"
            ]);

        return new OrchestrationReasoningResponse(
            ResponseId: $"{request.RequestId}:baseline-internal-plan",
            Request: request,
            ScenarioPlan: scenarioPlan,
            ContractEvidence: BuildContractEvidence(learnedGuideState, null, 0, unitDecision)
                .Where(item => !string.Equals(item, "NO_ORCHESTRATION_PLAN_GENERATED", StringComparison.Ordinal)
                               && !string.Equals(item, "ORCHESTRATION_REASONING_SCENARIO_UNIT_NOT_READY", StringComparison.Ordinal))
                .Concat(
                [
                    "BASELINE_INTERNAL_FALLBACK_USED",
                    "BASELINE_INTERNAL_PLAN_PRODUCED",
                    $"BASELINE_INTERNAL_SIGNAL_COUNT:{genericSignals.Count}",
                    "INFERENCE_SOURCE:BASELINE_INTERNAL_STRUCTURED_TASK_SIGNALS",
                    "HOST_RUNTIME_CLAIMED_FALSE",
                    "SEMANTIC_QUALITY_CLAIMED_FALSE",
                    "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
                ])
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            ContractStatus: BaselineInternalPlanProducedStatus);
    }

    private static IReadOnlyList<string> BuildGenericRuntimeSignals(OrchestrationReasoningRequest request)
    {
        var signals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(request.Task.TaskType)
            && !string.Equals(request.Task.TaskType, PromptToTaskMachine.NotReadyTaskType, StringComparison.Ordinal))
        {
            signals.Add("task_type_present");
        }
        if (request.Task.ExplicitUserRequirements.Count > 0)
            signals.Add("requirements_present");
        if (request.Task.Constraints.Count > 0)
            signals.Add("constraints_present");
        if (request.Task.Deliverables.Count > 0)
            signals.Add("deliverables_present");
        if (request.Task.EvidenceNeeds.Count > 0)
            signals.Add("evidence_needs_present");
        if (request.RuntimePolicyRefs.Count > 0)
            signals.Add("runtime_policy_refs_present");
        if (request.AdaptiveContextRefs.Count > 0)
            signals.Add("adaptive_context_refs_present");
        if (request.Task.SourceProvenance.TryGetValue("semantic_translation", out var semanticTranslation)
            && string.Equals(semanticTranslation, "baseline_internal_runtime_fallback", StringComparison.Ordinal))
        {
            signals.Add("prompt_to_task_baseline_internal_task");
        }

        return signals.Order(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> BuildContractEvidence(
        OrchestrationReasoningLearnedGuideStateDescriptor? learnedGuideState,
        OrchestrationReasoningScenarioUnitPattern? learnedPattern,
        int matchedSignalCount,
        OrchestrationReasoningScenarioDecision? unitDecision = null)
    {
        if (learnedGuideState is null)
        {
            return
            [
                "NO_LEARNED_GUIDE_STATE_LOADED",
                "NO_ORCHESTRATION_PLAN_GENERATED",
                "NO_RANKER_TEMPLATE_OR_FLOW_INDEX_USED",
                "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
            ];
        }

        var evidence = new[]
        {
            "GUIDE_STATE_DESCRIPTOR_LOADED",
            $"guide_state_id:{learnedGuideState.StateId}",
            $"guide_state_ref:{learnedGuideState.GuideStateRef}",
            $"manifest_id:{learnedGuideState.ManifestId}",
            $"manifest_sha256:{learnedGuideState.ManifestSha256}",
            $"machine_type:{learnedGuideState.MachineType}",
            $"schema_version:{learnedGuideState.SchemaVersion}",
            $"source_base_commit:{learnedGuideState.SourceBaseCommit}",
            $"state_sha256:{learnedGuideState.StateSha256}",
            "NO_RANKER_TEMPLATE_OR_FLOW_INDEX_USED",
            "ORCHESTRATION_REASONING_SCENARIO_UNIT_USED",
            "BASELINE_NOT_SEMANTIC_QUALITY",
            "HOST_RUNTIME_CLAIMED_FALSE",
            "SEMANTIC_QUALITY_CLAIMED_FALSE",
            "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM"
        };

        if (learnedPattern is null)
            return evidence
                .Concat(unitDecision?.EvidenceSignals ?? [])
                .Concat(["NO_ORCHESTRATION_PLAN_GENERATED"])
                .Distinct(StringComparer.Ordinal)
                .ToArray();

        return evidence.Concat(
        [
            "LEARNED_GUIDE_PATTERN_MATCHED",
            $"learned_pattern_id:{learnedPattern.PatternId}",
            $"scenario_ref:{learnedPattern.ScenarioRef}",
            $"matched_signal_count:{matchedSignalCount}",
            $"learned_pattern_confidence:{learnedPattern.Confidence:0.###}",
            $"orchestration_reasoning_scenario_unit_confidence:{unitDecision?.Confidence ?? 0.0:0.###}",
            $"orchestration_reasoning_scenario_unit_top_two_margin:{unitDecision?.TopTwoMargin ?? 0.0:0.###}",
            "INFERENCE_SOURCE:LOADED_LEARNED_GUIDE_STATE"
        ])
        .Concat(unitDecision?.EvidenceSignals ?? [])
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    }

    private static OperationResult<bool> ValidateCheckpoint(OrchestrationReasoningCheckpoint? checkpoint)
    {
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint deserialized null.", "ORCHESTRATION_REASONING_CHECKPOINT_DESERIALIZE_FAILED");
        if (checkpoint.SchemaVersion != CheckpointSchemaVersion)
            return OperationResult<bool>.Fail("Checkpoint schema is unsupported.", "ORCHESTRATION_REASONING_CHECKPOINT_SCHEMA_UNSUPPORTED");
        if (checkpoint.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Checkpoint must not claim semantic quality in OR-04A.", "ORCHESTRATION_REASONING_SEMANTIC_QUALITY_CLAIM_UNSUPPORTED");
        if (checkpoint.ContinueTrainingClaimed)
            return OperationResult<bool>.Fail("Checkpoint must not claim continue-training in OR-04A.", "ORCHESTRATION_REASONING_CONTINUE_TRAINING_CLAIM_UNSUPPORTED");
        if (checkpoint.HostRuntimeClaimed)
            return OperationResult<bool>.Fail("Checkpoint must not claim host runtime in OR-04A.", "ORCHESTRATION_REASONING_HOST_RUNTIME_CLAIM_UNSUPPORTED");
        if (checkpoint.LearnedState is null)
            return OperationResult<bool>.Fail("Checkpoint learned guide state is required.", "ORCHESTRATION_REASONING_CHECKPOINT_STATE_REQUIRED");
        if (!string.Equals(checkpoint.LearnedState.MachineType, nameof(OrchestrationReasoningMachine), StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Checkpoint machine type mismatch.", "ORCHESTRATION_REASONING_CHECKPOINT_MACHINE_TYPE_MISMATCH");
        if (checkpoint.LearnedState.LearnedPatterns is null || checkpoint.LearnedState.LearnedPatterns.Count == 0)
            return OperationResult<bool>.Fail("Checkpoint requires learned guide patterns.", "ORCHESTRATION_REASONING_CHECKPOINT_PATTERNS_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }

    private static async Task<OperationResult<OrchestrationReasoningCheckpoint>> ReadCheckpointAsync(
        string checkpointPath,
        CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail("Load cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail("Checkpoint path is required.", "ORCHESTRATION_REASONING_CHECKPOINT_PATH_REQUIRED");
        if (!File.Exists(checkpointPath))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail("Checkpoint file not found.", "ORCHESTRATION_REASONING_CHECKPOINT_NOT_FOUND");

        try
        {
            var json = await File.ReadAllTextAsync(checkpointPath, Encoding.UTF8, ct);
            var checkpoint = JsonSerializer.Deserialize<OrchestrationReasoningCheckpoint>(json, JsonOptions);
            var validation = ValidateCheckpoint(checkpoint);
            return validation.IsSuccess
                ? OperationResult<OrchestrationReasoningCheckpoint>.Ok(checkpoint!)
                : OperationResult<OrchestrationReasoningCheckpoint>.Fail(validation.ErrorMessage, validation.ErrorCode);
        }
        catch (Exception ex)
        {
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(ex.Message, "ORCHESTRATION_REASONING_CHECKPOINT_READ_FAILED");
        }
    }

    private static async Task<OperationResult<OrchestrationReasoningCheckpointManifest>> ReadManifestAsync(
        string manifestPath,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<OrchestrationReasoningCheckpointManifest>.Fail("Manifest path is required.", "ORCHESTRATION_REASONING_MANIFEST_PATH_REQUIRED");
        if (!File.Exists(manifestPath))
            return OperationResult<OrchestrationReasoningCheckpointManifest>.Fail("Manifest file not found.", "ORCHESTRATION_REASONING_MANIFEST_NOT_FOUND");

        try
        {
            var json = await File.ReadAllTextAsync(manifestPath, Encoding.UTF8, ct);
            var manifest = JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(json, JsonOptions);
            return manifest is null
                ? OperationResult<OrchestrationReasoningCheckpointManifest>.Fail("Manifest deserialized null.", "ORCHESTRATION_REASONING_MANIFEST_DESERIALIZE_FAILED")
                : OperationResult<OrchestrationReasoningCheckpointManifest>.Ok(manifest);
        }
        catch (Exception ex)
        {
            return OperationResult<OrchestrationReasoningCheckpointManifest>.Fail(ex.Message, "ORCHESTRATION_REASONING_MANIFEST_READ_FAILED");
        }
    }

    private static async Task<OperationResult<bool>> ValidateManifestAsync(
        OrchestrationReasoningCheckpoint checkpoint,
        OrchestrationReasoningCheckpointManifest manifest,
        string checkpointPath,
        string manifestPath,
        CancellationToken ct)
    {
        if (manifest.SchemaVersion != CheckpointSchemaVersion)
            return OperationResult<bool>.Fail("Manifest schema is unsupported.", "ORCHESTRATION_REASONING_MANIFEST_SCHEMA_UNSUPPORTED");
        if (!string.Equals(manifest.MachineId, MachineId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest machine id mismatch.", "ORCHESTRATION_REASONING_MANIFEST_MACHINE_ID_MISMATCH");
        if (!string.Equals(manifest.DomainTag, DomainTag, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest domain tag mismatch.", "ORCHESTRATION_REASONING_MANIFEST_DOMAIN_TAG_MISMATCH");
        var manifestDirectory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
        var resolvedCheckpointRef = ResolveManifestReference(manifestDirectory, manifest.CheckpointRef);
        var resolvedManifestRef = ResolveManifestReference(manifestDirectory, manifest.ManifestRef);
        if (!string.Equals(resolvedCheckpointRef, checkpointPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<bool>.Fail("Manifest checkpoint ref mismatch.", "ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_REF_MISMATCH");
        if (!string.Equals(resolvedManifestRef, manifestPath, StringComparison.OrdinalIgnoreCase))
            return OperationResult<bool>.Fail("Manifest ref mismatch.", "ORCHESTRATION_REASONING_MANIFEST_REF_MISMATCH");
        if (manifest.SemanticQualityClaimed)
            return OperationResult<bool>.Fail("Manifest must not claim semantic quality in OR-04A.", "ORCHESTRATION_REASONING_MANIFEST_SEMANTIC_QUALITY_CLAIM_UNSUPPORTED");
        if (manifest.ContinueTrainingClaimed)
            return OperationResult<bool>.Fail("Manifest must not claim continue-training in OR-04A.", "ORCHESTRATION_REASONING_MANIFEST_CONTINUE_TRAINING_CLAIM_UNSUPPORTED");
        if (manifest.HostRuntimeClaimed)
            return OperationResult<bool>.Fail("Manifest must not claim host runtime in OR-04A.", "ORCHESTRATION_REASONING_MANIFEST_HOST_RUNTIME_CLAIM_UNSUPPORTED");
        if (!string.Equals(manifest.SourceManifestRef, checkpoint.SourceManifestRef, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest source manifest ref must match checkpoint source manifest ref.", "ORCHESTRATION_REASONING_MANIFEST_SOURCE_REF_MISMATCH");
        var learnedPatternCount = checkpoint.LearnedState.LearnedPatterns?.Count ?? 0;
        if (manifest.LearnedPatternCount != learnedPatternCount)
            return OperationResult<bool>.Fail("Manifest learned pattern count must match checkpoint learned pattern count.", "ORCHESTRATION_REASONING_MANIFEST_PATTERN_COUNT_MISMATCH");

        var checkpointSha = await HashFileAsync(checkpointPath, ct);
        if (!string.Equals(manifest.CheckpointSha256, checkpointSha, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Manifest checkpoint hash mismatch.", "ORCHESTRATION_REASONING_MANIFEST_CHECKPOINT_HASH_MISMATCH");

        return OperationResult<bool>.Ok(true);
    }

    private static string ResolveManifestReference(string manifestDirectory, string reference)
        => Path.GetFullPath(Path.IsPathRooted(reference) ? reference : Path.Combine(manifestDirectory, reference));

    private static string CreatePortableManifestReference(string manifestDirectory, string targetPath)
    {
        var relativePath = Path.GetRelativePath(manifestDirectory, targetPath);
        return relativePath == "."
            ? Path.GetFileName(targetPath)
            : relativePath.Replace(Path.DirectorySeparatorChar, '/');
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
        var json = JsonSerializer.Serialize(value, JsonOptions).Replace("\r\n", "\n", StringComparison.Ordinal);
        var tmpPath = path + ".tmp";
        await File.WriteAllTextAsync(tmpPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), ct);
        File.Move(tmpPath, path, overwrite: true);
    }

    private static async Task<string> HashFileAsync(string path, CancellationToken ct)
    {
        var bytes = await File.ReadAllBytesAsync(path, ct);
        return HashBytes(bytes);
    }

    private static string HashBytes(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
}

public sealed class FileBackedOrchestrationReasoningLearnedGuideStateLoader(
    string checkpointPath,
    string manifestPath)
    : IOrchestrationReasoningLearnedGuideStateLoader
{
    public OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load()
    {
        var result = OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath)
            .GetAwaiter()
            .GetResult();
        return result.IsSuccess
            ? OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Ok(result.Value!.Checkpoint.LearnedState)
            : OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Fail(result.ErrorMessage, result.ErrorCode);
    }
}
