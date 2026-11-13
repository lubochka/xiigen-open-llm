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
    IReadOnlyList<OrchestrationReasoningLearnedPattern>? LearnedPatterns = null,
    string? LearnedPatternsSha256 = null);

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
    OperationResult<OrchestrationReasoningAdmittedLearnedState> Load();
}

public interface IOrchestrationReasoningCheckpointStagingWriter
{
    Task WriteAsync(
        int ordinal,
        string stagingPath,
        byte[] bytes,
        CancellationToken ct);
}

public sealed record OrchestrationReasoningCheckpoint(
    int SchemaVersion,
    OrchestrationReasoningLearnedGuideStateDescriptor LearnedState,
    IReadOnlyList<string> AcceptedRefIds,
    string SourceManifestRef,
    bool SemanticQualityClaimed,
    bool ContinueTrainingClaimed,
    bool HostRuntimeClaimed,
    DateTimeOffset SavedAtUtc,
    OrchestrationReasoningSourceAdmissionReceipt? AdmissionReceipt = null,
    string? AdmissionProofRef = null,
    string? AdmissionProofSha256 = null);

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
    DateTimeOffset ExportedAtUtc,
    string? AdmissionProofRef = null,
    string? AdmissionProofSha256 = null);

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
    bool HostRuntimeClaimed,
    OrchestrationReasoningAdmittedLearnedState AdmittedState);

public sealed class OrchestrationReasoningMachine
{
    public const string MachineId = "orchestration-reasoning-machine";

    public const string DomainTag = "SmartSkillsDomain.OrchestrationReasoning";

    public const string Version = "2.0.0";

    public const string NotTrainedStatus = "not_trained_no_learned_guide_state";

    public const string LoadedGuideStateReadyStatus = "loaded_guide_state_ready_no_learned_patterns";

    public const string NoMatchingPatternStatus = "loaded_guide_state_no_matching_learned_pattern";

    public const string BaselineInternalPlanProducedStatus = "baseline_internal_semantic_plan_produced";

    public const string LearnedPatternStatus = "loaded_guide_state_semantic_plan_produced";

    public const string PublicPlanProducedStatus = "orchestration_plan_produced";

    public const string NoInferenceScenarioId = "not-ready-no-orchestration-inference";

    private const int CheckpointSchemaVersion = 2;

    private const string SourceMaterialOutsideBundleMessage =
        "Admission proof source material refs must be portable relative paths inside the checkpoint bundle directory.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IOrchestrationReasoningLearnedGuideStateLoader? _learnedGuideStateLoader;
    private readonly IOrchestrationReasoningCheckpointStagingWriter _stagingWriter;

    private OrchestrationReasoningAdmittedLearnedState? _importedAdmittedState;
    private OrchestrationReasoningAdmittedLearnedState? _loadedAdmittedState;

    private IReadOnlyList<string> _importedAcceptedRefIds = [];

    private string? _lastAdmissionError;

    public OrchestrationReasoningMachine()
        : this(null, null)
    {
    }

    public OrchestrationReasoningMachine(IOrchestrationReasoningLearnedGuideStateLoader learnedGuideStateLoader)
        : this(learnedGuideStateLoader, null)
    {
    }

    public OrchestrationReasoningMachine(IOrchestrationReasoningCheckpointStagingWriter stagingWriter)
        : this(null, stagingWriter)
    {
    }

    public OrchestrationReasoningMachine(
        IOrchestrationReasoningLearnedGuideStateLoader? learnedGuideStateLoader,
        IOrchestrationReasoningCheckpointStagingWriter? stagingWriter)
    {
        _learnedGuideStateLoader = learnedGuideStateLoader;
        _stagingWriter = stagingWriter ?? new FileCheckpointStagingWriter();
    }

    public OperationResult<OrchestrationReasoningResponse> Plan(OrchestrationReasoningRequest request)
    {
        var admittedState = TryLoadLearnedGuideState();
        var learnedGuideState = admittedState?.State;
        if (learnedGuideState is null)
            return OperationResult<OrchestrationReasoningResponse>.Ok(BuildNoSemanticPlanResponse(
                request,
                NotTrainedStatus,
                $"{request.RequestId}:not-trained",
                [
                    "NO_LEARNED_GUIDE_STATE_LOADED",
                    "NO_ORCHESTRATION_PLAN_GENERATED",
                    _lastAdmissionError is null
                        ? "ORCHESTRATION_REASONING_SOURCE_ADMISSION_REQUIRED"
                        : $"ORCHESTRATION_REASONING_SOURCE_ADMISSION_REJECTED:{_lastAdmissionError}"
                ],
                ["load learned OrchestrationReasoning guide state before semantic scenario planning"],
                BuildContractEvidence(null, null, 0)));

        var unit = new OrchestrationReasoningScenarioUnit(
            OrchestrationReasoningScenarioUnit.FromLearnedState(admittedState!));
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
        IReadOnlyList<string> acceptedRefIds,
        DateTimeOffset? savedAtUtc = null)
    {
        var admittedState = TryLoadLearnedGuideState();
        var learnedState = admittedState?.State;
        if (learnedState is null || admittedState is null)
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "OrchestrationReasoning export requires a verified admitted learned guide state.",
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode);
        if (learnedState.LearnedPatterns is null || learnedState.LearnedPatterns.Count == 0)
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "OrchestrationReasoning export requires at least one learned guide pattern.",
                "ORCHESTRATION_REASONING_EXPORT_PATTERNS_REQUIRED");
        if (string.IsNullOrWhiteSpace(sourceManifestRef))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "Source manifest ref is required.",
                "ORCHESTRATION_REASONING_SOURCE_MANIFEST_REQUIRED");
        if (!string.Equals(sourceManifestRef, learnedState.ManifestId, StringComparison.Ordinal))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "Source manifest ref must exactly match the verified admitted state.",
                "ORCHESTRATION_REASONING_SOURCE_MANIFEST_MISMATCH");
        if (!SetsEqual(acceptedRefIds, admittedState.Receipt.PairIds))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "Accepted refs must exactly match the verified admission receipt pair ids.",
                "ORCHESTRATION_REASONING_ACCEPTED_REFS_MISMATCH");

        return CreateCheckpointFromAdmitted(
            admittedState,
            sourceManifestRef,
            acceptedRefIds,
            savedAtUtc);
    }

    private static OperationResult<OrchestrationReasoningCheckpoint> CreateCheckpointFromAdmitted(
        OrchestrationReasoningAdmittedLearnedState admittedState,
        string sourceManifestRef,
        IReadOnlyList<string> acceptedRefIds,
        DateTimeOffset? savedAtUtc)
    {
        var learnedState = admittedState.State;
        if (string.IsNullOrWhiteSpace(sourceManifestRef))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "Source manifest ref is required.",
                "ORCHESTRATION_REASONING_SOURCE_MANIFEST_REQUIRED");
        if (!string.Equals(sourceManifestRef, learnedState.ManifestId, StringComparison.Ordinal))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "Source manifest ref must exactly match the verified admitted state.",
                "ORCHESTRATION_REASONING_SOURCE_MANIFEST_MISMATCH");
        if (!SetsEqual(acceptedRefIds, admittedState.Receipt.PairIds))
            return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                "Accepted refs must exactly match the verified admission receipt pair ids.",
                "ORCHESTRATION_REASONING_ACCEPTED_REFS_MISMATCH");

        var checkpoint = new OrchestrationReasoningCheckpoint(
            SchemaVersion: CheckpointSchemaVersion,
            LearnedState: learnedState,
            AcceptedRefIds: acceptedRefIds,
            SourceManifestRef: sourceManifestRef,
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            // Боевой checkpoint фиксирует реальное время. Тестовая генерация
            // committed evidence передаёт постоянный момент, чтобы повторный
            // прогон доказывал смысл, а не переписывал файл из-за часов.
            SavedAtUtc: savedAtUtc ?? DateTimeOffset.UtcNow,
            AdmissionReceipt: admittedState.Receipt);

        return OperationResult<OrchestrationReasoningCheckpoint>.Ok(checkpoint);
    }

    public OperationResult<bool> ImportLearnedState(OrchestrationReasoningCheckpoint checkpoint)
    {
        _ = checkpoint;
        return OperationResult<bool>.Fail(
            "A persistence DTO cannot grant runtime admission authority; use verified checkpoint loading.",
            OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode);
    }

    public OperationResult<bool> ImportAdmittedState(
        OrchestrationReasoningAdmittedLearnedState admittedState)
    {
        var validation = OrchestrationReasoningSourceAdmission.Validate(admittedState);
        if (!validation.IsSuccess)
            return validation;
        _importedAdmittedState = admittedState;
        _importedAcceptedRefIds = admittedState.Receipt.PairIds;
        return OperationResult<bool>.Ok(true);
    }

    public OperationResult<OrchestrationReasoningContinueTrainingResult> ContinueTrainingFromLearnedState(
        OrchestrationReasoningAdmittedLearnedState deltaAdmittedState,
        IReadOnlyList<string> deltaAcceptedRefIds)
    {
        var existingAdmittedState = _importedAdmittedState;
        var existingState = existingAdmittedState?.State;
        if (existingState is null || existingAdmittedState is null)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires an imported checkpoint learned guide state with preserved accepted refs.",
                "ORCHESTRATION_REASONING_CONTINUE_IMPORTED_CHECKPOINT_STATE_REQUIRED");
        if (_importedAcceptedRefIds.Count == 0)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires preserved accepted refs from the imported checkpoint.",
                "ORCHESTRATION_REASONING_CONTINUE_IMPORTED_ACCEPTED_REFS_REQUIRED");
        if (deltaAdmittedState is null)
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Continue-training requires an admitted delta learned guide state.",
                "ORCHESTRATION_REASONING_CONTINUE_DELTA_STATE_REQUIRED");
        var deltaValidation = OrchestrationReasoningSourceAdmission.Validate(deltaAdmittedState);
        if (!deltaValidation.IsSuccess)
            return deltaValidation.MapError<OrchestrationReasoningContinueTrainingResult>();
        if (!SetsEqual(deltaAcceptedRefIds, deltaAdmittedState.Receipt.PairIds))
            return OperationResult<OrchestrationReasoningContinueTrainingResult>.Fail(
                "Delta accepted refs must exactly match the verified delta receipt pair ids.",
                "ORCHESTRATION_REASONING_CONTINUE_DELTA_ACCEPTED_REFS_MISMATCH");
        var deltaState = deltaAdmittedState.State;
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

        var mergedAdmittedResult = OrchestrationReasoningSourceAdmission.Merge(
            existingAdmittedState,
            deltaAdmittedState,
            $"{existingState.StateId}:continued:{deltaState.StateId}",
            $"{existingState.GuideStateRef}:continued",
            $"{existingState.SourceBaseCommit}+delta:{deltaState.SourceBaseCommit}");
        if (!mergedAdmittedResult.IsSuccess)
            return mergedAdmittedResult.MapError<OrchestrationReasoningContinueTrainingResult>();
        var mergedAdmittedState = mergedAdmittedResult.Value!;
        var mergedState = mergedAdmittedState.State;
        var mergedAcceptedRefs = mergedAdmittedState.Receipt.PairIds;

        _importedAdmittedState = mergedAdmittedState;
        _importedAcceptedRefIds = mergedAcceptedRefs;

        var result = new OrchestrationReasoningContinueTrainingResult(
            MergedLearnedState: mergedState,
            MergedAcceptedRefIds: mergedAcceptedRefs,
            AddedPatternIds: deltaState.LearnedPatterns.Select(pattern => pattern.PatternId).ToArray(),
            ContinueTrainingRunClaimed: false,
            CheckpointRewritten: false,
            SemanticQualityClaimed: false,
            HostRuntimeClaimed: false,
            AdmittedState: mergedAdmittedState);

        return OperationResult<OrchestrationReasoningContinueTrainingResult>.Ok(result);
    }

    public async Task<OperationResult<OrchestrationReasoningLifecycleExport>> SaveCheckpointAsync(
        string checkpointPath,
        string manifestPath,
        CancellationToken ct = default,
        DateTimeOffset? savedAtUtc = null)
    {
        var admitted = TryLoadLearnedGuideState();
        return admitted is null
            ? OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                "Checkpoint save requires verified runtime admission authority.",
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode)
            : await SaveCheckpointAsync(
                checkpointPath,
                manifestPath,
                admitted.State.ManifestId,
                admitted.Receipt.PairIds,
                ct,
                savedAtUtc);
    }

    public async Task<OperationResult<OrchestrationReasoningLifecycleExport>> SaveCheckpointAsync(
        string checkpointPath,
        string manifestPath,
        string sourceManifestRef,
        IReadOnlyList<string> acceptedRefIds,
        CancellationToken ct = default,
        DateTimeOffset? savedAtUtc = null)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail("Save cancelled.", "CANCELLED");
        if (string.IsNullOrWhiteSpace(checkpointPath))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail("Checkpoint path is required.", "ORCHESTRATION_REASONING_CHECKPOINT_PATH_REQUIRED");
        if (string.IsNullOrWhiteSpace(manifestPath))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail("Manifest path is required.", "ORCHESTRATION_REASONING_MANIFEST_PATH_REQUIRED");

        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var fullManifestPath = Path.GetFullPath(manifestPath);
        var proofPath = OrchestrationReasoningSourceAdmission.GetProofPath(fullCheckpointPath);
        if (PathsEqual(fullCheckpointPath, fullManifestPath)
            || PathsEqual(fullCheckpointPath, proofPath)
            || PathsEqual(fullManifestPath, proofPath))
        {
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                "Checkpoint, manifest, and proof paths must be three distinct normalized paths.",
                "ORCHESTRATION_REASONING_CHECKPOINT_TRIPLE_PATH_ALIAS");
        }
        var checkpointDirectory = Path.GetDirectoryName(fullCheckpointPath) ?? string.Empty;
        var manifestDirectory = Path.GetDirectoryName(fullManifestPath) ?? string.Empty;
        if (!PathsEqual(checkpointDirectory, manifestDirectory))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                "Checkpoint and manifest must be co-located.",
                "ORCHESTRATION_REASONING_CHECKPOINT_MANIFEST_DIRECTORY_MISMATCH");

        var admitted = TryLoadLearnedGuideState();
        if (admitted is null)
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                "Checkpoint save requires verified runtime admission authority.",
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode);

        if (!string.Equals(sourceManifestRef, admitted.State.ManifestId, StringComparison.Ordinal))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                "Source manifest ref must exactly match the verified admitted state before bundle packaging.",
                "ORCHESTRATION_REASONING_SOURCE_MANIFEST_MISMATCH");
        if (!SetsEqual(acceptedRefIds, admitted.Receipt.PairIds))
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                "Accepted refs must exactly match the verified admission receipt pair ids before bundle packaging.",
                "ORCHESTRATION_REASONING_ACCEPTED_REFS_MISMATCH");

        var bundlePlanResult = CreatePortableSourceMaterialBundlePlan(checkpointDirectory, admitted);
        if (!bundlePlanResult.IsSuccess)
            return bundlePlanResult.MapError<OrchestrationReasoningLifecycleExport>();
        var bundlePlan = bundlePlanResult.Value!;

        var checkpointResult = CreateCheckpointFromAdmitted(
            bundlePlan.AdmittedState,
            bundlePlan.AdmittedState.State.ManifestId,
            // После упаковки admission пересчитан на bundle-local пути.
            // Pair ids остаются той же проверенной совокупностью, но берём их
            // из нового receipt, чтобы checkpoint/proof/manifest описывали
            // один и тот же переносимый пакет, а не смесь старого corpus refs и
            // уже скопированного bundle.
            bundlePlan.AdmittedState.Receipt.PairIds,
            savedAtUtc);
        if (!checkpointResult.IsSuccess)
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        // Source material refs сохраняются только внутри каталога checkpoint.
        // Так checkpoint/proof/source bundle остаётся переносимым и не может
        // направить admission на случайные файлы окружающей файловой системы.
        var sourceMaterials = bundlePlan.SourceMaterials;
        var proof = new OrchestrationReasoningSourceAdmissionProof(
            SchemaVersion: 2,
            Receipt: checkpointResult.Value!.AdmissionReceipt!,
            SourceMaterials: sourceMaterials);
        var proofBytes = OrchestrationReasoningCanonicalState.SerializeAdmissionProof(proof);
        var proofSha = OrchestrationReasoningCanonicalState.HashBytes(proofBytes);
        var proofExists = File.Exists(proofPath);
        var replaceExistingProof = false;
        if (proofExists)
        {
            var existingProofBytes = await File.ReadAllBytesAsync(proofPath, ct);
            if (!existingProofBytes.AsSpan().SequenceEqual(proofBytes))
            {
                replaceExistingProof = IsEquivalentPortableSourceMaterialProofUpgrade(
                    existingProofBytes,
                    proof,
                    checkpointDirectory);
                if (!replaceExistingProof)
                {
                    return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                        "An admission proof already exists with different immutable content.",
                        OrchestrationReasoningSourceAdmission.ProofImmutableConflictCode);
                }
            }
        }

        if (proofExists && replaceExistingProof)
        {
            var existingProofBytes = await File.ReadAllBytesAsync(proofPath, ct);
            if (!IsEquivalentPortableSourceMaterialProofUpgrade(existingProofBytes, proof, checkpointDirectory))
            {
                return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                    "An admission proof already exists with different immutable content.",
                    OrchestrationReasoningSourceAdmission.ProofImmutableConflictCode);
            }
        }

        var proofRef = Path.GetFileName(proofPath);
        var checkpoint = checkpointResult.Value! with
        {
            AdmissionProofRef = proofRef,
            AdmissionProofSha256 = proofSha
        };
        var checkpointBytes = SerializeJson(checkpoint);
        var checkpointSha = OrchestrationReasoningCanonicalState.HashBytes(checkpointBytes);
        var exportedAtUtc = savedAtUtc ?? DateTimeOffset.UtcNow;
        var manifest = new OrchestrationReasoningCheckpointManifest(
            SchemaVersion: CheckpointSchemaVersion,
            MachineId: MachineId,
            DomainTag: DomainTag,
            Version: Version,
            CheckpointRef: CreatePortableManifestReference(manifestDirectory, fullCheckpointPath),
            ManifestRef: CreatePortableManifestReference(manifestDirectory, fullManifestPath),
            CheckpointSha256: checkpointSha,
            SourceManifestRef: checkpoint.SourceManifestRef,
            LearnedPatternCount: checkpoint.LearnedState.LearnedPatterns?.Count ?? 0,
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            ExportedAtUtc: exportedAtUtc,
            AdmissionProofRef: proofRef,
            AdmissionProofSha256: proofSha);
        var manifestBytes = SerializeJson(manifest);

        var transactionId = Guid.NewGuid().ToString("N");
        var proofStage = proofPath + $".tmp.{transactionId}.proof";
        var checkpointStage = fullCheckpointPath + $".tmp.{transactionId}.checkpoint";
        var manifestStage = fullManifestPath + $".tmp.{transactionId}.manifest";
        var sourceStages = bundlePlan.Copies
            .Select(copy => new PortableSourceMaterialStage(
                copy,
                copy.TargetPath + $".tmp.{transactionId}.source"))
            .ToArray();
        var snapshots = new[]
        {
            CaptureFile(proofPath),
            CaptureFile(fullCheckpointPath),
            CaptureFile(fullManifestPath)
        }.Concat(bundlePlan.Copies.Select(copy => CaptureFile(copy.TargetPath))).ToArray();
        var directorySnapshots = CaptureTransactionDirectories(checkpointDirectory, sourceStages);
        var transactionSucceeded = false;

        try
        {
            Directory.CreateDirectory(checkpointDirectory);
            await _stagingWriter.WriteAsync(1, proofStage, proofBytes, ct);
            await _stagingWriter.WriteAsync(2, checkpointStage, checkpointBytes, ct);
            await _stagingWriter.WriteAsync(3, manifestStage, manifestBytes, ct);
            foreach (var sourceStage in sourceStages)
            {
                EnsureDirectoryFor(sourceStage.StagePath);
                await File.WriteAllBytesAsync(sourceStage.StagePath, sourceStage.Copy.Bytes, ct);
            }
            var stagedSourcesValidation = ValidateStagedSourceMaterials(sourceStages);
            if (!stagedSourcesValidation.IsSuccess)
                return stagedSourcesValidation.MapError<OrchestrationReasoningLifecycleExport>();

            var stagedValidation = ValidateStagedTriple(
                proofStage,
                checkpointStage,
                manifestStage,
                proofSha,
                checkpointSha);
            if (!stagedValidation.IsSuccess)
                return stagedValidation.MapError<OrchestrationReasoningLifecycleExport>();

            ct.ThrowIfCancellationRequested();
            foreach (var sourceStage in sourceStages)
            {
                EnsureDirectoryFor(sourceStage.Copy.TargetPath);
                File.Move(sourceStage.StagePath, sourceStage.Copy.TargetPath, overwrite: true);
            }

            ct.ThrowIfCancellationRequested();
            if (!proofExists || replaceExistingProof)
                File.Move(proofStage, proofPath, overwrite: replaceExistingProof);
            else
                File.Delete(proofStage);
            File.Move(checkpointStage, fullCheckpointPath, overwrite: true);
            File.Move(manifestStage, fullManifestPath, overwrite: true);

            transactionSucceeded = true;
            return OperationResult<OrchestrationReasoningLifecycleExport>.Ok(
                new OrchestrationReasoningLifecycleExport(
                    MachineId,
                    DomainTag,
                    Version,
                    fullCheckpointPath,
                    fullManifestPath,
                    checkpointSha,
                    exportedAtUtc));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            RestoreFiles(snapshots);
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                "Save cancelled.",
                "CANCELLED");
        }
        catch (Exception ex)
        {
            RestoreFiles(snapshots);
            return OperationResult<OrchestrationReasoningLifecycleExport>.Fail(
                ex.Message,
                "ORCHESTRATION_REASONING_CHECKPOINT_WRITE_FAILED");
        }
        finally
        {
            DeleteIfExists(proofStage);
            DeleteIfExists(checkpointStage);
            DeleteIfExists(manifestStage);
            foreach (var sourceStage in sourceStages)
                DeleteIfExists(sourceStage.StagePath);
            if (!transactionSucceeded)
                RestoreDirectories(directorySnapshots);
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

        var admitted = loadResult.Value.Machine.TryLoadLearnedGuideState();
        return admitted is null
            ? OperationResult<bool>.Fail(
                "Loaded checkpoint did not produce runtime admission authority.",
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode)
            : ImportAdmittedState(admitted);
    }

    public static async Task<OperationResult<OrchestrationReasoningMachineLoadResult>> LoadCheckpointAsync(
        string checkpointPath,
        string manifestPath,
        CancellationToken ct = default)
    {
        var fullCheckpointPath = Path.GetFullPath(checkpointPath);
        var fullManifestPath = Path.GetFullPath(manifestPath);
        var expectedProofPath = OrchestrationReasoningSourceAdmission.GetProofPath(fullCheckpointPath);
        if (PathsEqual(fullCheckpointPath, fullManifestPath)
            || PathsEqual(fullCheckpointPath, expectedProofPath)
            || PathsEqual(fullManifestPath, expectedProofPath))
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(
                "Checkpoint, manifest, and proof paths must be three distinct normalized paths.",
                "ORCHESTRATION_REASONING_CHECKPOINT_TRIPLE_PATH_ALIAS");
        if (!PathsEqual(
                Path.GetDirectoryName(fullCheckpointPath) ?? string.Empty,
                Path.GetDirectoryName(fullManifestPath) ?? string.Empty))
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(
                "Checkpoint and manifest must be co-located.",
                "ORCHESTRATION_REASONING_CHECKPOINT_MANIFEST_DIRECTORY_MISMATCH");

        var checkpointResult = await ReadCheckpointAsync(checkpointPath, ct);
        if (!checkpointResult.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(checkpointResult.ErrorMessage, checkpointResult.ErrorCode);

        var manifestResult = await ReadManifestAsync(manifestPath, ct);
        if (!manifestResult.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(manifestResult.ErrorMessage, manifestResult.ErrorCode);

        var manifestValidation = await ValidateManifestAsync(
            checkpointResult.Value!,
            manifestResult.Value!,
            fullCheckpointPath,
            fullManifestPath,
            ct);
        if (!manifestValidation.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(manifestValidation.ErrorMessage, manifestValidation.ErrorCode);

        var proofValidation = await ValidateProofAsync(
            checkpointResult.Value!,
            manifestResult.Value!,
            fullCheckpointPath,
            fullManifestPath,
            ct);
        if (!proofValidation.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(
                proofValidation.ErrorMessage,
                proofValidation.ErrorCode);

        var machine = new OrchestrationReasoningMachine();
        var importResult = machine.ImportAdmittedState(proofValidation.Value!);
        if (!importResult.IsSuccess)
            return OperationResult<OrchestrationReasoningMachineLoadResult>.Fail(importResult.ErrorMessage, importResult.ErrorCode);

        return OperationResult<OrchestrationReasoningMachineLoadResult>.Ok(new OrchestrationReasoningMachineLoadResult(
            machine,
            checkpointResult.Value!,
            manifestResult.Value!,
            fullCheckpointPath));
    }

    private OrchestrationReasoningAdmittedLearnedState? TryLoadLearnedGuideState()
    {
        if (_importedAdmittedState is not null)
        {
            var importedValidation = OrchestrationReasoningSourceAdmission.Validate(_importedAdmittedState);
            if (importedValidation.IsSuccess)
                return _importedAdmittedState;
            _lastAdmissionError = importedValidation.ErrorCode;
            return null;
        }

        if (_loadedAdmittedState is not null)
            return _loadedAdmittedState;

        var result = _learnedGuideStateLoader?.Load();
        if (result is null || !result.IsSuccess || result.Value is null)
        {
            _lastAdmissionError = result?.ErrorCode;
            return null;
        }

        var validation = OrchestrationReasoningSourceAdmission.Validate(result.Value);
        if (!validation.IsSuccess)
        {
            _lastAdmissionError = validation.ErrorCode;
            return null;
        }

        _loadedAdmittedState = result.Value;
        return _loadedAdmittedState;
    }

    internal OrchestrationReasoningAdmittedLearnedState? GetVerifiedAdmittedState()
        => TryLoadLearnedGuideState();

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
        if (checkpoint.SchemaVersion == 1)
            return OperationResult<bool>.Fail(
                "Common schema-v1 checkpoints require a provenance rebuild.",
                OrchestrationReasoningSourceAdmission.ProvenanceRebuildRequiredCode);
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
        var admissionValidation = OrchestrationReasoningSourceAdmission.ValidateReceiptAgainstState(
            checkpoint.LearnedState,
            checkpoint.AdmissionReceipt);
        if (!admissionValidation.IsSuccess)
            return admissionValidation;
        if (!string.Equals(checkpoint.SourceManifestRef, checkpoint.LearnedState.ManifestId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail(
                "Checkpoint source manifest ref does not match verified state provenance.",
                "ORCHESTRATION_REASONING_SOURCE_MANIFEST_MISMATCH");
        if (!SetsEqual(checkpoint.AcceptedRefIds, checkpoint.AdmissionReceipt!.PairIds))
            return OperationResult<bool>.Fail(
                "Checkpoint accepted refs do not match verified receipt provenance.",
                "ORCHESTRATION_REASONING_ACCEPTED_REFS_MISMATCH");
        if (string.IsNullOrWhiteSpace(checkpoint.AdmissionProofRef)
            || string.IsNullOrWhiteSpace(checkpoint.AdmissionProofSha256))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint admission proof binding is required.",
                OrchestrationReasoningSourceAdmission.ProofNotFoundCode);
        }

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
            using var document = JsonDocument.Parse(json);
            var schemaVersion = ReadSchemaVersion(document.RootElement);
            if (schemaVersion == 1
                && OrchestrationReasoningSourceAdmission.ContainsCommonCheckpointMarkers(document.RootElement))
            {
                return OperationResult<OrchestrationReasoningCheckpoint>.Fail(
                    "Common schema-v1 checkpoints require a provenance rebuild.",
                    OrchestrationReasoningSourceAdmission.ProvenanceRebuildRequiredCode);
            }
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
            using var document = JsonDocument.Parse(json);
            if (ReadSchemaVersion(document.RootElement) == 1)
            {
                return OperationResult<OrchestrationReasoningCheckpointManifest>.Fail(
                    "Common schema-v1 manifests require a provenance rebuild.",
                    OrchestrationReasoningSourceAdmission.ProvenanceRebuildRequiredCode);
            }
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
        if (string.IsNullOrWhiteSpace(manifest.AdmissionProofRef)
            || string.IsNullOrWhiteSpace(manifest.AdmissionProofSha256)
            || !string.Equals(manifest.AdmissionProofRef, checkpoint.AdmissionProofRef, StringComparison.Ordinal)
            || !string.Equals(manifest.AdmissionProofSha256, checkpoint.AdmissionProofSha256, StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "Checkpoint and manifest admission-proof bindings must match.",
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
        }

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

    private static OperationResult<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>> CreatePortableSourceMaterialReferences(
        string checkpointDirectory,
        OrchestrationReasoningAdmittedLearnedState admitted)
    {
        var portableSources = new List<OrchestrationReasoningSourceMaterialReference>();
        foreach (var source in OrchestrationReasoningSourceAdmission.GetSourceReferences(admitted))
        {
            var payloadRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.PayloadRef);
            if (!payloadRef.IsSuccess)
                return payloadRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();
            var manifestRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.ManifestRef);
            if (!manifestRef.IsSuccess)
                return manifestRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();
            var ledgerRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.LedgerRef);
            if (!ledgerRef.IsSuccess)
                return ledgerRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();
            var evidenceRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.EvidenceRef);
            if (!evidenceRef.IsSuccess)
                return evidenceRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();

            portableSources.Add(new OrchestrationReasoningSourceMaterialReference(
                payloadRef.Value!,
                manifestRef.Value!,
                ledgerRef.Value!,
                evidenceRef.Value!));
        }

        return OperationResult<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>.Ok(portableSources);
    }

    private static OperationResult<PortableSourceMaterialBundlePlan> CreatePortableSourceMaterialBundlePlan(
        string checkpointDirectory,
        OrchestrationReasoningAdmittedLearnedState admitted)
    {
        var sourceStates = new List<OrchestrationReasoningAdmittedLearnedState>();
        var copies = new List<PortableSourceMaterialCopy>();
        var sourceMaterials = new List<OrchestrationReasoningSourceMaterialReference>();
        var verifiedSources = admitted.VerifiedSources;
        for (var index = 0; index < verifiedSources.Count; index++)
        {
            var source = verifiedSources[index];
            var snapshotValidation = ValidateVerifiedSourceMaterialSnapshot(source);
            if (!snapshotValidation.IsSuccess)
                return snapshotValidation.MapError<PortableSourceMaterialBundlePlan>();
            var layoutResult = CreatePortableSourceMaterialLayout(checkpointDirectory, source, index);
            if (!layoutResult.IsSuccess)
                return layoutResult.MapError<PortableSourceMaterialBundlePlan>();
            var layout = layoutResult.Value!;

            var patternResult = OrchestrationReasoningTrainer.MaterializePatternsFromPayload(source.PayloadBytes);
            if (!patternResult.IsSuccess)
                return patternResult.MapError<PortableSourceMaterialBundlePlan>();
            if (!TryReadSourcePairsSha256(source.ManifestBytes, out var sourcePairsSha, out var errorMessage))
            {
                return OperationResult<PortableSourceMaterialBundlePlan>.Fail(
                    errorMessage,
                    OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
            }

            // Checkpoint packages are self-contained: the saved state points to
            // the bundle-local manifest copy that the proof will reopen later,
            // not to whichever temporary corpus path originally trained it.
            var sourceState = OrchestrationReasoningTrainer.BuildLearnedState(
                patternResult.Value!,
                source.Decision.ManifestSha256,
                sourcePairsSha,
                layout.ManifestRef);
            var bundledSource = new OrchestrationReasoningVerifiedSourceMaterial(
                layout.PayloadPath,
                layout.ManifestPath,
                layout.LedgerPath,
                layout.EvidencePath,
                source.PayloadBytes,
                source.ManifestBytes,
                source.LedgerBytes,
                source.EvidenceBytes,
                source.Decision);
            var sourceAdmitted = OrchestrationReasoningSourceAdmission.CreateAdmitted(
                sourceState,
                [bundledSource]);
            sourceStates.Add(sourceAdmitted);
            sourceMaterials.Add(new OrchestrationReasoningSourceMaterialReference(
                layout.PayloadRef,
                layout.ManifestRef,
                layout.LedgerRef,
                layout.EvidenceRef));
            copies.AddRange(
            [
                new PortableSourceMaterialCopy(layout.PayloadPath, source.PayloadBytes, source.Decision.PayloadSha256),
                new PortableSourceMaterialCopy(layout.ManifestPath, source.ManifestBytes, source.Decision.ManifestSha256),
                new PortableSourceMaterialCopy(layout.LedgerPath, source.LedgerBytes, source.Decision.LedgerSha256),
                new PortableSourceMaterialCopy(layout.EvidencePath, source.EvidenceBytes, source.Decision.EvidenceSha256)
            ]);
        }

        if (sourceStates.Count == 0)
        {
            return OperationResult<PortableSourceMaterialBundlePlan>.Fail(
                "At least one verified source material is required for a portable checkpoint bundle.",
                OrchestrationReasoningSourceAdmission.ProofNotFoundCode);
        }

        var packaged = sourceStates[0];
        for (var index = 1; index < sourceStates.Count; index++)
        {
            var delta = sourceStates[index];
            var merge = OrchestrationReasoningSourceAdmission.Merge(
                packaged,
                delta,
                $"{packaged.State.StateId}:continued:{delta.State.StateId}",
                $"{packaged.State.GuideStateRef}:continued",
                $"{packaged.State.SourceBaseCommit}+delta:{delta.State.SourceBaseCommit}");
            if (!merge.IsSuccess)
                return merge.MapError<PortableSourceMaterialBundlePlan>();
            packaged = merge.Value!;
        }

        return OperationResult<PortableSourceMaterialBundlePlan>.Ok(
            new PortableSourceMaterialBundlePlan(
                packaged,
                copies,
            sourceMaterials));
    }

    private static OperationResult<bool> ValidateVerifiedSourceMaterialSnapshot(
        OrchestrationReasoningVerifiedSourceMaterial source)
    {
        // Save не перечитывает временный corpus: admission уже сделал
        // проверенный снимок payload/manifest/ledger/evidence bytes. Здесь мы
        // сверяем снимок с decision hashes, чтобы не опубликовать bundle из
        // внутренне противоречивого admitted state.
        if (!string.Equals(
                OrchestrationReasoningCanonicalState.HashBytes(source.PayloadBytes),
                source.Decision.PayloadSha256,
                StringComparison.Ordinal)
            || !string.Equals(
                OrchestrationReasoningCanonicalState.HashBytes(source.ManifestBytes),
                source.Decision.ManifestSha256,
                StringComparison.Ordinal)
            || !string.Equals(
                OrchestrationReasoningCanonicalState.HashBytes(source.LedgerBytes),
                source.Decision.LedgerSha256,
                StringComparison.Ordinal)
            || !string.Equals(
                OrchestrationReasoningCanonicalState.HashBytes(source.EvidenceBytes),
                source.Decision.EvidenceSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<bool>.Fail(
                "Verified source material snapshot does not match its admission decision hashes.",
                OrchestrationReasoningSourceAdmission.StateAdmissionInvalidCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<PortableSourceMaterialLayout> CreatePortableSourceMaterialLayout(
        string checkpointDirectory,
        OrchestrationReasoningVerifiedSourceMaterial source,
        int index)
    {
        var existingLayout = TryCreateExistingBundleLocalSourceMaterialLayout(checkpointDirectory, source);
        if (existingLayout.IsSuccess)
            return existingLayout;

        // Внешний corpus превращается в переносимый bundle-local snapshot.
        // Детерминированный индекс даёт стабильные proof refs и не раскрывает
        // исходный временный путь пользователя/теста.
        var sourceDirectoryRef = $"source-materials/{index + 1:D2}";
        return OperationResult<PortableSourceMaterialLayout>.Ok(new PortableSourceMaterialLayout(
            PayloadRef: $"{sourceDirectoryRef}/payload.jsonl",
            ManifestRef: $"{sourceDirectoryRef}/manifest.json",
            LedgerRef: $"{sourceDirectoryRef}/source-admission-ledger.json",
            EvidenceRef: $"{sourceDirectoryRef}/source-admission-evidence.json",
            PayloadPath: Path.Combine(checkpointDirectory, "source-materials", $"{index + 1:D2}", "payload.jsonl"),
            ManifestPath: Path.Combine(checkpointDirectory, "source-materials", $"{index + 1:D2}", "manifest.json"),
            LedgerPath: Path.Combine(checkpointDirectory, "source-materials", $"{index + 1:D2}", "source-admission-ledger.json"),
            EvidencePath: Path.Combine(checkpointDirectory, "source-materials", $"{index + 1:D2}", "source-admission-evidence.json")));
    }

    private static OperationResult<PortableSourceMaterialLayout> TryCreateExistingBundleLocalSourceMaterialLayout(
        string checkpointDirectory,
        OrchestrationReasoningVerifiedSourceMaterial source)
    {
        var payloadRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.PayloadPath);
        var manifestRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.ManifestPath);
        var ledgerRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.LedgerPath);
        var evidenceRef = CreatePortableSourceMaterialReference(checkpointDirectory, source.EvidencePath);
        if (!payloadRef.IsSuccess || !manifestRef.IsSuccess || !ledgerRef.IsSuccess || !evidenceRef.IsSuccess)
            return SourceMaterialOutsideBundle<PortableSourceMaterialLayout>();

        return OperationResult<PortableSourceMaterialLayout>.Ok(new PortableSourceMaterialLayout(
            payloadRef.Value!,
            manifestRef.Value!,
            ledgerRef.Value!,
            evidenceRef.Value!,
            Path.GetFullPath(source.PayloadPath),
            Path.GetFullPath(source.ManifestPath),
            Path.GetFullPath(source.LedgerPath),
            Path.GetFullPath(source.EvidencePath)));
    }

    private static OperationResult<string> CreatePortableSourceMaterialReference(
        string checkpointDirectory,
        string targetPath)
    {
        try
        {
            var fullTargetPath = Path.GetFullPath(targetPath);
            if (!IsPathInsideDirectoryTree(checkpointDirectory, fullTargetPath))
                return SourceMaterialOutsideBundle<string>();

            var relativePath = Path.GetRelativePath(Path.GetFullPath(checkpointDirectory), fullTargetPath);
            var normalized = NormalizePortableSourceMaterialReference(relativePath);
            return normalized.IsSuccess
                ? normalized
                : SourceMaterialOutsideBundle<string>();
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return SourceMaterialOutsideBundle<string>();
        }
    }

    private static OperationResult<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>> ResolveSourceMaterialReferences(
        string checkpointDirectory,
        IReadOnlyList<OrchestrationReasoningSourceMaterialReference> sourceMaterials)
    {
        var resolvedSources = new List<OrchestrationReasoningSourceMaterialReference>();
        foreach (var source in sourceMaterials)
        {
            var payloadRef = ResolveSourceMaterialReference(checkpointDirectory, source.PayloadRef);
            if (!payloadRef.IsSuccess)
                return payloadRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();
            var manifestRef = ResolveSourceMaterialReference(checkpointDirectory, source.ManifestRef);
            if (!manifestRef.IsSuccess)
                return manifestRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();
            var ledgerRef = ResolveSourceMaterialReference(checkpointDirectory, source.LedgerRef);
            if (!ledgerRef.IsSuccess)
                return ledgerRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();
            var evidenceRef = ResolveSourceMaterialReference(checkpointDirectory, source.EvidenceRef);
            if (!evidenceRef.IsSuccess)
                return evidenceRef.MapError<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>();

            resolvedSources.Add(new OrchestrationReasoningSourceMaterialReference(
                payloadRef.Value!,
                manifestRef.Value!,
                ledgerRef.Value!,
                evidenceRef.Value!));
        }

        return OperationResult<IReadOnlyList<OrchestrationReasoningSourceMaterialReference>>.Ok(resolvedSources);
    }

    private static OperationResult<string> ResolveSourceMaterialReference(
        string checkpointDirectory,
        string reference)
    {
        var portable = NormalizePortableSourceMaterialReference(reference);
        if (!portable.IsSuccess)
            return portable;

        try
        {
            var nativeReference = portable.Value!.Replace('/', Path.DirectorySeparatorChar);
            var resolvedPath = Path.GetFullPath(Path.Combine(Path.GetFullPath(checkpointDirectory), nativeReference));
            return IsPathInsideDirectoryTree(checkpointDirectory, resolvedPath)
                ? OperationResult<string>.Ok(resolvedPath)
                : SourceMaterialOutsideBundle<string>();
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return SourceMaterialOutsideBundle<string>();
        }
    }

    private static OperationResult<string> NormalizePortableSourceMaterialReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return SourceMaterialOutsideBundle<string>();

        var normalized = reference.Trim().Replace('\\', '/');
        if (normalized.StartsWith("/", StringComparison.Ordinal)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || IsWindowsDriveQualifiedReference(normalized))
        {
            return SourceMaterialOutsideBundle<string>();
        }

        var segments = new List<string>();
        foreach (var segment in normalized.Split('/'))
        {
            if (segment.Length == 0 || string.Equals(segment, ".", StringComparison.Ordinal))
                continue;
            if (string.Equals(segment, "..", StringComparison.Ordinal))
                return SourceMaterialOutsideBundle<string>();
            segments.Add(segment);
        }

        return segments.Count == 0
            ? SourceMaterialOutsideBundle<string>()
            : OperationResult<string>.Ok(string.Join("/", segments));
    }

    private static bool IsWindowsDriveQualifiedReference(string reference)
        => reference.Length >= 2
           && reference[1] == ':'
           && ((reference[0] >= 'A' && reference[0] <= 'Z')
               || (reference[0] >= 'a' && reference[0] <= 'z'));

    private static bool IsPathInsideDirectoryTree(string directory, string path)
    {
        var fullDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        var directoryPrefix = fullDirectory + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static OperationResult<T> SourceMaterialOutsideBundle<T>()
        => OperationResult<T>.Fail(
            SourceMaterialOutsideBundleMessage,
            OrchestrationReasoningSourceAdmission.ProofSourceMaterialOutsideBundleCode);

    private static bool IsEquivalentPortableSourceMaterialProofUpgrade(
        byte[] existingProofBytes,
        OrchestrationReasoningSourceAdmissionProof portableProof,
        string checkpointDirectory)
    {
        try
        {
            var existingProof = ParseAdmissionProof(existingProofBytes);
            if (existingProof.SchemaVersion != portableProof.SchemaVersion
                || existingProof.SourceMaterials.Count != portableProof.SourceMaterials.Count
                || !OrchestrationReasoningCanonicalState.SerializeReceiptCore(existingProof.Receipt)
                    .AsSpan()
                    .SequenceEqual(OrchestrationReasoningCanonicalState.SerializeReceiptCore(portableProof.Receipt)))
            {
                return false;
            }

            var existingResolved = ResolveSourceMaterialReferences(checkpointDirectory, existingProof.SourceMaterials);
            var portableResolved = ResolveSourceMaterialReferences(checkpointDirectory, portableProof.SourceMaterials);
            if (!existingResolved.IsSuccess || !portableResolved.IsSuccess)
                return false;

            return existingResolved.Value!
                .Zip(portableResolved.Value!)
                .All(pair =>
                    PathsEqual(pair.First.PayloadRef, pair.Second.PayloadRef)
                    && PathsEqual(pair.First.ManifestRef, pair.Second.ManifestRef)
                    && PathsEqual(pair.First.LedgerRef, pair.Second.LedgerRef)
                    && PathsEqual(pair.First.EvidenceRef, pair.Second.EvidenceRef));
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or ArgumentException)
        {
            return false;
        }
    }

    private static bool PathsEqual(string left, string right)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            StringComparison.OrdinalIgnoreCase);

    private static bool SetsEqual(IEnumerable<string>? left, IEnumerable<string>? right)
        => (left ?? []).ToHashSet(StringComparer.Ordinal)
            .SetEquals(right ?? []);

    private static byte[] SerializeJson<T>(T value)
        => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(
            JsonSerializer.Serialize(value, JsonOptions)
                .Replace("\r\n", "\n", StringComparison.Ordinal));

    private static OperationResult<bool> ValidateStagedTriple(
        string proofStage,
        string checkpointStage,
        string manifestStage,
        string expectedProofSha,
        string expectedCheckpointSha)
    {
        try
        {
            var proofBytes = File.ReadAllBytes(proofStage);
            var checkpointBytes = File.ReadAllBytes(checkpointStage);
            var manifestBytes = File.ReadAllBytes(manifestStage);
            var proof = ParseAdmissionProof(proofBytes);
            var checkpoint = JsonSerializer.Deserialize<OrchestrationReasoningCheckpoint>(
                checkpointBytes,
                JsonOptions);
            var manifest = JsonSerializer.Deserialize<OrchestrationReasoningCheckpointManifest>(
                manifestBytes,
                JsonOptions);
            if (proof.SchemaVersion != 2
                || proof.SourceMaterials.Count == 0
                || checkpoint is null
                || manifest is null
                || !string.Equals(
                    OrchestrationReasoningCanonicalState.HashBytes(proofBytes),
                    expectedProofSha,
                    StringComparison.Ordinal)
                || !string.Equals(
                    OrchestrationReasoningCanonicalState.HashBytes(checkpointBytes),
                    expectedCheckpointSha,
                    StringComparison.Ordinal)
                || !string.Equals(checkpoint.AdmissionProofSha256, expectedProofSha, StringComparison.Ordinal)
                || !string.Equals(manifest.AdmissionProofSha256, expectedProofSha, StringComparison.Ordinal)
                || !string.Equals(manifest.CheckpointSha256, expectedCheckpointSha, StringComparison.Ordinal)
                || !string.Equals(
                    proof.Receipt.ReceiptSha256,
                    checkpoint.AdmissionReceipt?.ReceiptSha256,
                    StringComparison.Ordinal))
            {
                return OperationResult<bool>.Fail(
                    "Staged checkpoint triple failed internal validation.",
                    "ORCHESTRATION_REASONING_CHECKPOINT_STAGED_TRIPLE_INVALID");
            }
            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex) when (ex is IOException or JsonException or InvalidDataException)
        {
            return OperationResult<bool>.Fail(
                ex.Message,
                "ORCHESTRATION_REASONING_CHECKPOINT_STAGED_TRIPLE_INVALID");
        }
    }

    private static OperationResult<bool> ValidateStagedSourceMaterials(
        IReadOnlyList<PortableSourceMaterialStage> sourceStages)
    {
        try
        {
            foreach (var sourceStage in sourceStages)
            {
                var bytes = File.ReadAllBytes(sourceStage.StagePath);
                if (!string.Equals(
                        OrchestrationReasoningCanonicalState.HashBytes(bytes),
                        sourceStage.Copy.ExpectedSha256,
                        StringComparison.Ordinal))
                {
                    return OperationResult<bool>.Fail(
                        "Staged source material failed admission hash validation.",
                        OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
                }
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return OperationResult<bool>.Fail(
                ex.Message,
                "ORCHESTRATION_REASONING_CHECKPOINT_STAGED_TRIPLE_INVALID");
        }
    }

    private static FileSnapshot CaptureFile(string path)
        => File.Exists(path)
            ? new FileSnapshot(path, true, File.ReadAllBytes(path))
            : new FileSnapshot(path, false, null);

    private static void RestoreFiles(IEnumerable<FileSnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            try
            {
                if (snapshot.Existed)
                    File.WriteAllBytes(snapshot.Path, snapshot.Bytes!);
                else
                    DeleteIfExists(snapshot.Path);
            }
            catch
            {
                // Preserve the original transaction error. A subsequent load
                // still fails closed if rollback itself was externally blocked.
            }
        }
    }

    private static IReadOnlyList<DirectorySnapshot> CaptureTransactionDirectories(
        string checkpointDirectory,
        IReadOnlyList<PortableSourceMaterialStage> sourceStages)
    {
        var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddDirectoryAndParentsWithinCheckpoint(checkpointDirectory, checkpointDirectory, directories);
        foreach (var sourceStage in sourceStages)
        {
            AddDirectoryAndParentsWithinCheckpoint(
                checkpointDirectory,
                Path.GetDirectoryName(sourceStage.Copy.TargetPath),
                directories);
            AddDirectoryAndParentsWithinCheckpoint(
                checkpointDirectory,
                Path.GetDirectoryName(sourceStage.StagePath),
                directories);
        }

        return directories
            .Select(directory => new DirectorySnapshot(directory, Directory.Exists(directory)))
            .OrderByDescending(snapshot => snapshot.Path.Length)
            .ToArray();
    }

    private static void AddDirectoryAndParentsWithinCheckpoint(
        string checkpointDirectory,
        string? directory,
        ISet<string> directories)
    {
        if (string.IsNullOrWhiteSpace(directory))
            return;

        var checkpointRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(checkpointDirectory));
        var current = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        while (PathsEqual(current, checkpointRoot) || IsPathInsideDirectoryTree(checkpointRoot, current))
        {
            directories.Add(current);
            if (PathsEqual(current, checkpointRoot))
                break;

            var parent = Path.GetDirectoryName(current);
            if (string.IsNullOrWhiteSpace(parent))
                break;
            current = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent));
        }
    }

    private static void RestoreDirectories(IEnumerable<DirectorySnapshot> snapshots)
    {
        foreach (var snapshot in snapshots)
        {
            if (snapshot.Existed || !Directory.Exists(snapshot.Path))
                continue;

            try
            {
                if (!Directory.EnumerateFileSystemEntries(snapshot.Path).Any())
                    Directory.Delete(snapshot.Path);
            }
            catch
            {
                // A failed save is never accepted. Directory cleanup is best effort
                // because an external process can create files during rollback.
            }
        }
    }

    private static void DeleteIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best-effort cleanup; callers never accept a failed transaction.
        }
    }

    private sealed record FileSnapshot(string Path, bool Existed, byte[]? Bytes);

    private sealed record DirectorySnapshot(string Path, bool Existed);

    private sealed record PortableSourceMaterialBundlePlan(
        OrchestrationReasoningAdmittedLearnedState AdmittedState,
        IReadOnlyList<PortableSourceMaterialCopy> Copies,
        IReadOnlyList<OrchestrationReasoningSourceMaterialReference> SourceMaterials);

    private sealed record PortableSourceMaterialLayout(
        string PayloadRef,
        string ManifestRef,
        string LedgerRef,
        string EvidenceRef,
        string PayloadPath,
        string ManifestPath,
        string LedgerPath,
        string EvidencePath);

    private sealed record PortableSourceMaterialCopy(
        string TargetPath,
        byte[] Bytes,
        string ExpectedSha256);

    private sealed record PortableSourceMaterialStage(
        PortableSourceMaterialCopy Copy,
        string StagePath);

    private sealed class FileCheckpointStagingWriter
        : IOrchestrationReasoningCheckpointStagingWriter
    {
        public Task WriteAsync(
            int ordinal,
            string stagingPath,
            byte[] bytes,
            CancellationToken ct)
        {
            _ = ordinal;
            return File.WriteAllBytesAsync(stagingPath, bytes, ct);
        }
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

    private static async Task WriteBytesAtomicallyAsync(
        string path,
        byte[] bytes,
        bool overwrite,
        CancellationToken ct)
    {
        var tmpPath = path + ".tmp";
        await File.WriteAllBytesAsync(tmpPath, bytes, ct);
        File.Move(tmpPath, path, overwrite);
    }

    private static async Task<OperationResult<OrchestrationReasoningAdmittedLearnedState>> ValidateProofAsync(
        OrchestrationReasoningCheckpoint checkpoint,
        OrchestrationReasoningCheckpointManifest manifest,
        string checkpointPath,
        string manifestPath,
        CancellationToken ct)
    {
        var checkpointDirectory = Path.GetDirectoryName(checkpointPath) ?? string.Empty;
        var manifestDirectory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
        var checkpointProofPath = ResolveManifestReference(
            checkpointDirectory,
            checkpoint.AdmissionProofRef!);
        var manifestProofPath = ResolveManifestReference(
            manifestDirectory,
            manifest.AdmissionProofRef!);
        if (!string.Equals(checkpointProofPath, manifestProofPath, StringComparison.OrdinalIgnoreCase)
            || !PathsEqual(
                checkpointProofPath,
                OrchestrationReasoningSourceAdmission.GetProofPath(checkpointPath))
            || PathsEqual(checkpointProofPath, checkpointPath)
            || PathsEqual(checkpointProofPath, manifestPath)
            || !File.Exists(checkpointProofPath))
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "The checkpoint source-admission proof was not found.",
                OrchestrationReasoningSourceAdmission.ProofNotFoundCode);
        }

        var proofBytes = await File.ReadAllBytesAsync(checkpointProofPath, ct);
        var proofSha = OrchestrationReasoningCanonicalState.HashBytes(proofBytes);
        if (!string.Equals(proofSha, checkpoint.AdmissionProofSha256, StringComparison.Ordinal)
            || !string.Equals(proofSha, manifest.AdmissionProofSha256, StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "The checkpoint source-admission proof hash does not match.",
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
        }

        try
        {
            var proof = ParseAdmissionProof(proofBytes);
            if (proof.SchemaVersion != 2
                || proof.SourceMaterials.Count == 0
                || !string.Equals(
                    proof.Receipt.ReceiptSha256,
                    checkpoint.AdmissionReceipt!.ReceiptSha256,
                    StringComparison.Ordinal))
            {
                return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                    "The source-admission proof receipt does not bind the checkpoint state.",
                    OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
            }
            var persistedReceiptValidation = OrchestrationReasoningSourceAdmission.ValidateReceiptAgainstState(
                checkpoint.LearnedState,
                proof.Receipt);
            if (!persistedReceiptValidation.IsSuccess)
            {
                return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                    persistedReceiptValidation.ErrorMessage,
                    OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
            }

            // Proof source refs are intentionally portable bundle refs only. A
            // rooted path or traversal would let the proof read ambient files, so
            // containment is checked before source admission performs any file read.
            var resolvedSourceMaterialsResult = ResolveSourceMaterialReferences(
                checkpointDirectory,
                proof.SourceMaterials);
            if (!resolvedSourceMaterialsResult.IsSuccess)
                return resolvedSourceMaterialsResult.MapError<OrchestrationReasoningAdmittedLearnedState>();

            // Proof rehydration must not call the ordinary source-admission
            // file reader: source-material refs are portable bundle refs, so a
            // lexical inside-bundle path can still escape through a
            // symlink/junction/reparse component. The proof reader admits only
            // bytes read from files opened inside the checkpoint bundle handle
            // boundary.
            var rehydrated = await RehydrateCheckpointStateFromProofSourceMaterialsAsync(
                checkpointDirectory,
                checkpoint.LearnedState,
                proof.Receipt,
                proof.SourceMaterials,
                ct);
            if (!rehydrated.IsSuccess)
                return rehydrated;
            return rehydrated;
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException)
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                ex.Message,
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
        }
    }

    private static OrchestrationReasoningSourceAdmissionProof ParseAdmissionProof(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        var root = document.RootElement;
        var receiptElement = root.GetProperty("receipt");
        var receipt = new OrchestrationReasoningSourceAdmissionReceipt(
            SchemaVersion: receiptElement.GetProperty("schema_version").GetInt32(),
            PayloadSha256: receiptElement.GetProperty("payload_sha256").GetString()!,
            ManifestSha256: receiptElement.GetProperty("manifest_sha256").GetString()!,
            LedgerSha256: receiptElement.GetProperty("ledger_sha256").GetString()!,
            EvidenceSha256: receiptElement.GetProperty("evidence_sha256").GetString()!,
            AllowedSourceKind: receiptElement.GetProperty("allowed_source_kind").GetString()!,
            RowIds: ReadStringArray(receiptElement, "row_ids"),
            SourceIds: ReadStringArray(receiptElement, "source_ids"),
            PairIds: ReadStringArray(receiptElement, "pair_ids"),
            SourceRefs: ReadStringArray(receiptElement, "source_refs"),
            SourceSha256s: ReadStringArray(receiptElement, "source_sha256s"),
            LearnedPatternsSha256: receiptElement.GetProperty("learned_patterns_sha256").GetString()!,
            StateSha256: receiptElement.GetProperty("state_sha256").GetString()!,
            ParentReceiptSha256s: ReadStringArray(receiptElement, "parent_receipt_sha256s"),
            ReceiptSha256: receiptElement.GetProperty("receipt_sha256").GetString()!);
        var materials = root.GetProperty("source_materials")
            .EnumerateArray()
            .Select(item => new OrchestrationReasoningSourceMaterialReference(
                item.GetProperty("payload_ref").GetString()!,
                item.GetProperty("manifest_ref").GetString()!,
                item.GetProperty("ledger_ref").GetString()!,
                item.GetProperty("evidence_ref").GetString()!))
            .ToArray();
        return new OrchestrationReasoningSourceAdmissionProof(
            root.GetProperty("schema_version").GetInt32(),
            receipt,
            materials);
    }

    private static async Task<OperationResult<OrchestrationReasoningAdmittedLearnedState>>
        RehydrateCheckpointStateFromProofSourceMaterialsAsync(
            string checkpointDirectory,
            OrchestrationReasoningLearnedGuideStateDescriptor checkpointState,
            OrchestrationReasoningSourceAdmissionReceipt receipt,
            IReadOnlyList<OrchestrationReasoningSourceMaterialReference> sourceMaterials,
            CancellationToken ct)
    {
        var verifiedSources = new List<OrchestrationReasoningVerifiedSourceMaterial>();
        var logicalManifestRefs = ValidateLogicalManifestRefsAgainstProof(
            checkpointState.ManifestId,
            sourceMaterials);
        if (!logicalManifestRefs.IsSuccess)
            return logicalManifestRefs.MapError<OrchestrationReasoningAdmittedLearnedState>();

        OrchestrationReasoningAdmittedLearnedState? reconstructed = null;
        for (var index = 0; index < sourceMaterials.Count; index++)
        {
            var source = sourceMaterials[index];
            var admission = await OrchestrationReasoningProofBundleSourceMaterialReader.AdmitAsync(
                checkpointDirectory,
                source,
                ct);
            if (!admission.IsSuccess)
                return admission.MapError<OrchestrationReasoningAdmittedLearnedState>();

            var patternResult = OrchestrationReasoningTrainer.MaterializePatternsFromPayload(
                admission.Value!.PayloadBytes);
            if (!patternResult.IsSuccess)
                return patternResult.MapError<OrchestrationReasoningAdmittedLearnedState>();

            if (!TryReadSourcePairsSha256(admission.Value.ManifestBytes, out var sourcePairsSha, out var errorMessage))
            {
                return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                    errorMessage,
                    OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
            }

            verifiedSources.Add(admission.Value);
            var sourceState = OrchestrationReasoningTrainer.BuildLearnedState(
                patternResult.Value!,
                admission.Value.Decision.ManifestSha256,
                sourcePairsSha,
                logicalManifestRefs.Value![index]);
            var sourceAdmitted = OrchestrationReasoningSourceAdmission.CreateAdmitted(
                sourceState,
                [admission.Value]);
            if (reconstructed is null)
            {
                reconstructed = sourceAdmitted;
                continue;
            }

            var merge = OrchestrationReasoningSourceAdmission.Merge(
                reconstructed,
                sourceAdmitted,
                $"{reconstructed.State.StateId}:continued:{sourceAdmitted.State.StateId}",
                $"{reconstructed.State.GuideStateRef}:continued",
                $"{reconstructed.State.SourceBaseCommit}+delta:{sourceAdmitted.State.SourceBaseCommit}");
            if (!merge.IsSuccess)
                return merge;
            reconstructed = merge.Value!;
        }

        if (reconstructed is null
            || !OrchestrationReasoningCanonicalState.SerializeStateCore(reconstructed.State)
                .AsSpan()
                .SequenceEqual(OrchestrationReasoningCanonicalState.SerializeStateCore(checkpointState))
            || !string.Equals(reconstructed.State.StateSha256, checkpointState.StateSha256, StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "Reverified source materials do not reproduce the persisted learned state.",
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
        }

        if (!OrchestrationReasoningCanonicalState.SerializeReceiptCore(reconstructed.Receipt)
                .AsSpan()
                .SequenceEqual(OrchestrationReasoningCanonicalState.SerializeReceiptCore(receipt))
            || !string.Equals(
                reconstructed.Receipt.ReceiptSha256,
                receipt.ReceiptSha256,
                StringComparison.Ordinal))
        {
            return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(
                "Reverified source materials do not reproduce the persisted admission receipt.",
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
        }

        return OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(reconstructed);
    }

    private static OperationResult<IReadOnlyList<string>> ValidateLogicalManifestRefsAgainstProof(
        string manifestId,
        IReadOnlyList<OrchestrationReasoningSourceMaterialReference> sourceMaterials)
    {
        var logicalManifestRefs = SplitLogicalManifestRefs(
            manifestId,
            sourceMaterials.Count);
        if (logicalManifestRefs.Count != sourceMaterials.Count)
        {
            return OperationResult<IReadOnlyList<string>>.Fail(
                "Persisted learned-state manifest refs do not match proof source materials.",
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
        }

        var verifiedRefs = new List<string>();
        for (var index = 0; index < logicalManifestRefs.Count; index++)
        {
            var persistedRef = NormalizeStandaloneLogicalManifestRef(logicalManifestRefs[index]);
            var proofRef = NormalizeStandaloneLogicalManifestRef(sourceMaterials[index].ManifestRef);
            if (!persistedRef.IsSuccess
                || !proofRef.IsSuccess
                || !string.Equals(persistedRef.Value, proofRef.Value, StringComparison.Ordinal))
            {
                return OperationResult<IReadOnlyList<string>>.Fail(
                    "Persisted learned-state manifest ref must match the proof-bound source material manifest ref.",
                    OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
            }

            verifiedRefs.Add(persistedRef.Value!);
        }

        return OperationResult<IReadOnlyList<string>>.Ok(verifiedRefs);
    }

    private static OperationResult<string> NormalizeStandaloneLogicalManifestRef(string manifestRef)
    {
        var normalized = NormalizePortableSourceMaterialReference(manifestRef);
        if (!normalized.IsSuccess)
            return normalized;
        if (normalized.Value!.Contains(':', StringComparison.Ordinal)
            || normalized.Value.Contains(';', StringComparison.Ordinal))
        {
            return OperationResult<string>.Fail(
                "Persisted learned-state manifest ref must be one plain bundle-local manifest path.",
                OrchestrationReasoningSourceAdmission.ProofHashMismatchCode);
        }

        return normalized; // The single ref is now safe to compare; only the proof reader is allowed to open it.
    }

    private static IReadOnlyList<string> SplitLogicalManifestRefs(string manifestId, int expectedCount)
    {
        if (expectedCount == 1)
            return [manifestId];

        var parts = manifestId.Split(";delta:", StringSplitOptions.None);
        return parts.Length == expectedCount
            ? parts
            : [];
    }

    private static bool TryReadSourcePairsSha256(
        byte[] manifestBytes,
        out string sourcePairsSha,
        out string errorMessage)
    {
        sourcePairsSha = string.Empty;
        errorMessage = string.Empty;
        try
        {
            using var manifestDocument = JsonDocument.Parse(manifestBytes);
            if (!manifestDocument.RootElement.TryGetProperty("source_pairs_sha256", out var sourcePairsElement)
                || sourcePairsElement.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(sourcePairsElement.GetString()))
            {
                errorMessage = "Source manifest does not contain a bound source_pairs_sha256.";
                return false;
            }

            sourcePairsSha = sourcePairsElement.GetString()!;
            return true;
        }
        catch (JsonException ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string name)
        => root.GetProperty(name)
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();

    private static int ReadSchemaVersion(JsonElement root)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, "schemaVersion", StringComparison.OrdinalIgnoreCase)
                || string.Equals(property.Name, "schema_version", StringComparison.OrdinalIgnoreCase))
            {
                return property.Value.ValueKind == JsonValueKind.Number
                    && property.Value.TryGetInt32(out var version)
                    ? version
                    : 0;
            }
        }
        return 0;
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
    public OperationResult<OrchestrationReasoningAdmittedLearnedState> Load()
    {
        var result = OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath)
            .GetAwaiter()
            .GetResult();
        return result.IsSuccess
            && result.Value!.Machine.GetVerifiedAdmittedState() is { } admitted
            ? OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(admitted)
            : OperationResult<OrchestrationReasoningAdmittedLearnedState>.Fail(result.ErrorMessage, result.ErrorCode);
    }
}
