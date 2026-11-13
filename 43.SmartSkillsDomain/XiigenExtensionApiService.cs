// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class XiigenExtensionApiService
{
    public const string PartialBoundaryStatus = "core_boundary_partial_no_semantic_quality_claim";
    public const string InvalidRequestStatus = "invalid_request";
    public const string ErrorStatus = "core_boundary_error";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string SkillSelectionRouteMode = "candidate_position_scaffold_not_final_semantic_authority";
    private const string PositiveSemanticQualityEvidence = "SEMANTIC_QUALITY_CLAIMED_TRUE";
    private const string HostRequestedSkillSurfaceFallbackNoSemanticAuthorityEvidence =
        "HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_SEMANTIC_AUTHORITY";
    private const string HostRequestedSkillSurfaceFallbackNoModelMutationEvidence =
        "HOST_REQUESTED_SKILL_SURFACE_FALLBACK_NO_MODEL_MUTATION";
    private const string UserAdaptiveSkillSelectionEmptyCommonHostSurfaceFallbackEvidence =
        "USER_ADAPTIVE_SKILL_SELECTION_EMPTY_COMMON_HOST_SURFACE_FALLBACK";
    private const string UserAdaptiveSkillSelectionEmptyCommonHostSurfaceFallbackUnavailableEvidence =
        "USER_ADAPTIVE_SKILL_SELECTION_EMPTY_COMMON_HOST_SURFACE_FALLBACK_UNAVAILABLE";
    private const string UserAdaptiveSkillSelectionFallbackNoModelMutationEvidence =
        "USER_ADAPTIVE_SKILL_SELECTION_FALLBACK_NO_MODEL_MUTATION";

    private static readonly string[] SemanticNoClaimEvidence =
    [
        "BASELINE_NOT_SEMANTIC_QUALITY",
        "SEMANTIC_QUALITY_CLAIMED_FALSE",
        "NO_TRAINED_STATE_LOADED",
        "NO_LEARNED_GUIDE_STATE_LOADED",
        "NO_SEMANTIC_CLASSIFICATION_PERFORMED",
        "NO_ORCHESTRATION_PLAN_GENERATED",
        "NO_TRAINING_CHECKPOINT_OR_ASSET_CLAIM",
        "NO_LEARNED_PATTERNS_AVAILABLE",
        "NO_LEARNED_GUIDE_PATTERNS_AVAILABLE",
        "NO_MATCHING_LEARNED_PATTERN",
        "NO_MATCHING_LEARNED_GUIDE_PATTERN",
        "PROMPT_TO_TASK_TASK_PATTERN_UNIT_NOT_READY",
        "ORCHESTRATION_REASONING_SCENARIO_UNIT_NOT_READY"
    ];

    private readonly PromptToTaskMachine _promptToTaskMachine;
    private readonly OrchestrationReasoningMachine _orchestrationReasoningMachine;
    private readonly SkillSelectionMachine _skillSelectionMachine;
    private readonly CommonAssetLoadState _commonAssetLoadState;

    public XiigenExtensionApiService()
        : this(LoadCommonAssets(FindRepoRoot()))
    {
    }

    public XiigenExtensionApiService(string commonAssetRoot)
        : this(LoadCommonAssets(commonAssetRoot))
    {
    }

    public XiigenExtensionApiService(string commonAssetRoot, string userAdaptiveAssetRoot)
        : this(LoadCommonThenUserAdaptiveAssets(commonAssetRoot, userAdaptiveAssetRoot))
    {
    }

    public XiigenExtensionApiService(
        PromptToTaskMachine? promptToTaskMachine,
        OrchestrationReasoningMachine? orchestrationReasoningMachine,
        SkillSelectionMachine? skillSelectionMachine)
    {
        _promptToTaskMachine = promptToTaskMachine ?? new PromptToTaskMachine();
        _orchestrationReasoningMachine = orchestrationReasoningMachine ?? new OrchestrationReasoningMachine();
        _skillSelectionMachine = skillSelectionMachine ?? new SkillSelectionMachine();
        _commonAssetLoadState = CommonAssetLoadState.Injected(
            _promptToTaskMachine,
            _orchestrationReasoningMachine,
            _skillSelectionMachine);
    }

    private XiigenExtensionApiService(CommonAssetLoadState commonAssetLoadState)
    {
        _promptToTaskMachine = commonAssetLoadState.PromptToTaskMachine;
        _orchestrationReasoningMachine = commonAssetLoadState.OrchestrationReasoningMachine;
        _skillSelectionMachine = commonAssetLoadState.SkillSelectionMachine;
        _commonAssetLoadState = commonAssetLoadState;
    }

    public OperationResult<XiigenExtensionResponse> ProcessHostExtensionRequest(
        XiigenExtensionRequest request)
    {
        if (request is null)
        {
            var nullRequestResponse = BuildValidationResponse(
                request: null,
                errors: ["request_required"]);
            return OperationResult<XiigenExtensionResponse>.Ok(nullRequestResponse);
        }

        var validation = XiigenExtensionApiContract.ValidateRequest(request);
        if (!validation.IsValid)
        {
            var invalidResponse = BuildValidationResponse(request, validation.Errors);
            return OperationResult<XiigenExtensionResponse>.Ok(invalidResponse);
        }

        var promptSha256 = ComputeSha256(request.UserPromptOrTaskText ?? string.Empty);
        var activeLoadStateResult = ResolveRequestLoadState(request);
        if (!activeLoadStateResult.IsSuccess || activeLoadStateResult.Value is null)
        {
            var invalidResponse = BuildValidationResponse(
                request,
                [activeLoadStateResult.ErrorCode]);
            return OperationResult<XiigenExtensionResponse>.Ok(invalidResponse);
        }

        var activeLoadState = activeLoadStateResult.Value;
        var promptToTaskRequest = BuildPromptToTaskRequest(request, promptSha256);
        var promptToTaskResult = activeLoadState.PromptToTaskMachine.Translate(promptToTaskRequest);
        if (!promptToTaskResult.IsSuccess)
        {
            var promptToTaskErrorResponse = BuildErrorResponse(
                request,
                promptSha256,
                "prompt_to_task_failed",
                promptToTaskResult.ErrorCode);
            return OperationResult<XiigenExtensionResponse>.Ok(promptToTaskErrorResponse);
        }

        var promptToTask = promptToTaskResult.Value!;
        var orchestrationRequest = BuildOrchestrationRequest(request, promptToTask.StructuredTask);
        var orchestrationResult = activeLoadState.OrchestrationReasoningMachine.Plan(orchestrationRequest);
        if (!orchestrationResult.IsSuccess)
        {
            var orchestrationErrorResponse = BuildErrorResponse(
                request,
                promptSha256,
                "orchestration_reasoning_failed",
                orchestrationResult.ErrorCode);
            return OperationResult<XiigenExtensionResponse>.Ok(orchestrationErrorResponse);
        }

        var orchestration = orchestrationResult.Value!;
        var skillSelection = DecideSkills(request, promptSha256, activeLoadState);
        if (!skillSelection.IsSuccess)
        {
            var skillSelectionErrorResponse = BuildErrorResponse(
                request,
                promptSha256,
                "skill_selection_failed",
                skillSelection.ErrorCode);
            return OperationResult<XiigenExtensionResponse>.Ok(skillSelectionErrorResponse);
        }

        var skillDecision = skillSelection.Value!;
        var semanticQualityClaimed = ShouldClaimSemanticQuality(promptToTask, orchestration, skillDecision);
        var warnings = BuildWarnings(promptToTask, orchestration, skillDecision, semanticQualityClaimed, activeLoadState);
        var selectedSkills = BuildHostSelectedSkills(skillDecision, activeLoadState);
        var checkSetResults = BuildCheckSetResults(
            request,
            promptToTask,
            orchestration,
            skillDecision,
            selectedSkills,
            semanticQualityClaimed,
            activeLoadState);
        var structuredTaskRefs = BuildStructuredTaskRefs(promptToTask.StructuredTask);
        var scenarioRefs = BuildScenarioRefs(orchestration.ScenarioPlan);
        var chatDisplayText = BuildHostChatDisplayText(
            request,
            promptToTask,
            orchestration,
            skillDecision,
            selectedSkills,
            semanticQualityClaimed,
            activeLoadState);
        var responseWithoutTrace = new XiigenExtensionResponse(
            Success: true,
            Status: PartialBoundaryStatus,
            ChatDisplayText: chatDisplayText,
            SelectedSkills: selectedSkills,
            CheckSetResults: checkSetResults,
            StructuredTaskRefs: structuredTaskRefs,
            ScenarioRefs: scenarioRefs,
            Warnings: warnings,
            Errors: [],
            DebugTrace: null);
        var debugTrace = BuildDebugTrace(
            request,
            promptSha256,
            responseWithoutTrace);
        var response = responseWithoutTrace with
        {
            ChatDisplayText = XiigenExtensionApiContract.BuildChatDisplayText(
                chatDisplayText,
                request.DebugEnabled,
                debugTrace),
            DebugTrace = request.DebugEnabled ? debugTrace : null
        };

        return OperationResult<XiigenExtensionResponse>.Ok(response);
    }

    private static XiigenExtensionResponse BuildValidationResponse(
        XiigenExtensionRequest? request,
        IReadOnlyList<string> errors)
    {
        var responseWithoutTrace = new XiigenExtensionResponse(
            Success: false,
            Status: InvalidRequestStatus,
            ChatDisplayText: "Xiigen request validation failed.",
            SelectedSkills: [],
            CheckSetResults: new Dictionary<string, string>
            {
                ["core_boundary_executed"] = "false",
                ["semantic_quality_claimed"] = "false",
                ["runtime_extension_claimed"] = "false"
            },
            StructuredTaskRefs: [],
            ScenarioRefs: [],
            Warnings:
            [
                "semantic_quality_claimed=false",
                "core_boundary_only_no_host_runtime_claim"
            ],
            Errors: errors,
            DebugTrace: null);
        if (request is null)
            return responseWithoutTrace;

        var promptSha256 = ComputeSha256(request.UserPromptOrTaskText ?? string.Empty);
        var debugTrace = BuildDebugTrace(request, promptSha256, responseWithoutTrace);
        return responseWithoutTrace with
        {
            ChatDisplayText = XiigenExtensionApiContract.BuildChatDisplayText(
                responseWithoutTrace.ChatDisplayText,
                request.DebugEnabled,
                debugTrace),
            DebugTrace = request.DebugEnabled ? debugTrace : null
        };
    }

    private static XiigenExtensionResponse BuildErrorResponse(
        XiigenExtensionRequest request,
        string promptSha256,
        string error,
        string errorCode)
    {
        var responseWithoutTrace = new XiigenExtensionResponse(
            Success: false,
            Status: ErrorStatus,
            ChatDisplayText: "Xiigen core boundary failed before semantic execution.",
            SelectedSkills: [],
            CheckSetResults: new Dictionary<string, string>
            {
                ["core_boundary_executed"] = "false",
                ["semantic_quality_claimed"] = "false",
                ["runtime_extension_claimed"] = "false"
            },
            StructuredTaskRefs: [],
            ScenarioRefs: [],
            Warnings:
            [
                "semantic_quality_claimed=false",
                "core_boundary_only_no_host_runtime_claim"
            ],
            Errors: [error, errorCode],
            DebugTrace: null);
        var debugTrace = BuildDebugTrace(request, promptSha256, responseWithoutTrace);
        return responseWithoutTrace with
        {
            ChatDisplayText = XiigenExtensionApiContract.BuildChatDisplayText(
                responseWithoutTrace.ChatDisplayText,
                request.DebugEnabled,
                debugTrace),
            DebugTrace = request.DebugEnabled ? debugTrace : null
        };
    }

    private OperationResult<CommonAssetLoadState> ResolveRequestLoadState(
        XiigenExtensionRequest request)
    {
        if (request.TargetLocator is null)
            return OperationResult<CommonAssetLoadState>.Ok(_commonAssetLoadState);

        var locator = request.TargetLocator;
        var mode = NormalizeTargetLocatorMode(locator);
        var requestedRoot = string.Equals(mode, "rollback", StringComparison.Ordinal)
            ? locator.RollbackAssetRoot
            : locator.UserAdaptiveAssetRoot;
        var missingRootError = string.Equals(mode, "rollback", StringComparison.Ordinal)
            ? "rollback_locator_asset_root_required"
            : "target_locator_user_adaptive_asset_root_required";

        if (string.IsNullOrWhiteSpace(requestedRoot))
        {
            return OperationResult<CommonAssetLoadState>.Fail(
                "Target locator asset root was blank.",
                missingRootError);
        }

        var resolvedRoot = ResolveTargetLocatorRoot(requestedRoot);
        if (!Directory.Exists(resolvedRoot))
        {
            var errorCode = string.Equals(mode, "rollback", StringComparison.Ordinal)
                ? "rollback_locator_asset_root_not_found"
                : "target_locator_user_adaptive_asset_root_not_found";
            return OperationResult<CommonAssetLoadState>.Fail(
                $"Target locator asset root not found: {RedactPath(resolvedRoot)}",
                errorCode);
        }

        var loaded = LoadCommonThenUserAdaptiveAssets(FindRepoRoot(), resolvedRoot);
        return OperationResult<CommonAssetLoadState>.Ok(WithTargetLocatorRefs(
            loaded,
            locator,
            mode,
            resolvedRoot));
    }

    private static CommonAssetLoadState WithTargetLocatorRefs(
        CommonAssetLoadState state,
        XiigenTargetLocator locator,
        string mode,
        string resolvedRoot)
    {
        var refs = new Dictionary<string, string>(state.CheckSetRefs, StringComparer.Ordinal)
        {
            ["target_locator_bound"] = "true",
            ["target_locator_source"] = "host_request",
            ["target_locator_mode"] = mode,
            ["target_locator_candidate_id"] = locator.LocatorCandidateId,
            ["target_candidate_id"] = locator.TargetCandidateId,
            ["rollback_candidate_id"] = locator.RollbackCandidateId,
            ["target_locator_user_adaptive_asset_root"] = RedactPath(resolvedRoot),
            ["target_locator_rollback_requested"] = string.Equals(mode, "rollback", StringComparison.Ordinal).ToString().ToLowerInvariant()
        };

        foreach (var entry in locator.LocatorRefs ?? new Dictionary<string, string>())
        {
            refs[$"target_locator_ref.{entry.Key}"] = entry.Value ?? string.Empty;
        }

        return state with { CheckSetRefs = refs };
    }

    private static string NormalizeTargetLocatorMode(XiigenTargetLocator locator)
        => string.Equals(locator.LocatorMode, "rollback", StringComparison.OrdinalIgnoreCase)
            ? "rollback"
            : "target";

    private static string ResolveTargetLocatorRoot(string root)
        => Path.IsPathRooted(root)
            ? Path.GetFullPath(root)
            : Path.GetFullPath(Path.Combine(FindRepoRoot(), root));

    private OperationResult<SkillSelectionDecision> DecideSkills(
        XiigenExtensionRequest request,
        string promptSha256,
        CommonAssetLoadState activeLoadState)
    {
        var availableSkills = SafeList(request.AvailableSkills);
        if (availableSkills.Count == 0)
        {
            var noSkillsDecision = new SkillSelectionDecision(
                SelectedSkillIds: [],
                RejectedSkillIds: [],
                EvidenceFeatures: ["NO_AVAILABLE_SKILLS_PROVIDED"],
                Confidence: 0.0,
                RequiresSecondPass: true,
                DecisionText: "No available skills were provided by the host.");
            return OperationResult<SkillSelectionDecision>.Ok(noSkillsDecision);
        }

        var skillRequest = new SkillSelectionRequest(
            Prompt: request.UserPromptOrTaskText ?? string.Empty,
            ProjectRoot: ReadFirstValue(
                request.HostFacts,
                "workspace",
                "project_root",
                "projectRoot"),
            AvailableSkillIds: availableSkills,
            HostFacts: SafeDictionary(request.HostFacts),
            UserPromptSourceEntryId: ReadFirstValueOrDefault(
                request.Provenance,
                request.CorrelationId,
                "source_prompt_entry_id",
                "sourcePromptEntryId",
                "source",
                "ledger_id",
                "ledgerId"));

        var activeDecision = activeLoadState.SkillSelectionMachine.DecideFromTrainedState(skillRequest);
        if (!activeDecision.IsSuccess
            || activeDecision.Value is null
            || activeDecision.Value.SelectedSkillIds.Count > 0
            || SafeList(request.RequestedSkills).Count == 0)
        {
            return activeDecision;
        }

        if (!IsUserAdaptiveLoaded(activeLoadState))
        {
            if (activeDecision.Value.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal))
            {
                var fallbackDecision = BuildRequestedSkillSurfaceFallbackDecision(request, activeDecision.Value);
                if (fallbackDecision.IsSuccess
                    && fallbackDecision.Value is not null
                    && fallbackDecision.Value.SelectedSkillIds.Count > 0)
                {
                    return fallbackDecision;
                }

                return activeDecision;
            }

            return BuildRequestedSkillSurfaceFallbackDecision(request, activeDecision.Value);
        }

        var commonDecision = activeLoadState.CommonSkillSelectionMachine.DecideFromTrainedState(skillRequest);
        if (!commonDecision.IsSuccess
            || commonDecision.Value is null
            || commonDecision.Value.SelectedSkillIds.Count == 0)
        {
            return BuildRequestedSkillSurfaceFallbackDecision(
                request,
                activeDecision.Value,
                UserAdaptiveSkillSelectionEmptyCommonHostSurfaceFallbackUnavailableEvidence);
        }

        var activeEvidence = SafeList(activeDecision.Value.EvidenceFeatures);
        var commonEvidence = SafeList(commonDecision.Value.EvidenceFeatures);
        var mergedEvidence = activeEvidence
            .Concat(commonEvidence)
            .Append(UserAdaptiveSkillSelectionEmptyCommonHostSurfaceFallbackEvidence)
            .Append(UserAdaptiveSkillSelectionFallbackNoModelMutationEvidence)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var mergedDecisionText =
            "User-adaptive skill-selection checkpoint loaded but returned no selected skills; " +
            "common host-requested skill-surface fallback preserved host skill surface with no model mutation and no semantic authority. " +
            commonDecision.Value.DecisionText;

        return OperationResult<SkillSelectionDecision>.Ok(new SkillSelectionDecision(
            SelectedSkillIds: commonDecision.Value.SelectedSkillIds,
            RejectedSkillIds: commonDecision.Value.RejectedSkillIds,
            EvidenceFeatures: mergedEvidence,
            Confidence: commonDecision.Value.Confidence,
            RequiresSecondPass: commonDecision.Value.RequiresSecondPass,
            DecisionText: mergedDecisionText));
    }

    private static OperationResult<SkillSelectionDecision> BuildRequestedSkillSurfaceFallbackDecision(
        XiigenExtensionRequest request,
        SkillSelectionDecision priorDecision,
        string? extraEvidence = null)
    {
        var available = new HashSet<string>(SafeList(request.AvailableSkills), StringComparer.Ordinal);
        var filteredShortcutDecoyCount = SafeList(request.RequestedSkills)
            .Count(skillId => !string.IsNullOrWhiteSpace(skillId)
                && available.Contains(skillId)
                && IsShortcutDecoySkillId(skillId));
        var selected = SafeList(request.RequestedSkills)
            .Where(skillId => !string.IsNullOrWhiteSpace(skillId)
                && available.Contains(skillId)
                && !IsShortcutDecoySkillId(skillId))
            .Distinct(StringComparer.Ordinal)
            .Take(5)
            .ToArray();
        if (selected.Length == 0)
            return OperationResult<SkillSelectionDecision>.Ok(priorDecision);

        var rejected = SafeList(request.AvailableSkills)
            .Except(selected, StringComparer.Ordinal)
            .Take(10)
            .ToArray();
        var evidence = SafeList(priorDecision.EvidenceFeatures)
            .Concat(new[]
            {
                SkillSelectionMachine.SemanticPipelineBoundaryEvidence,
                HostRequestedSkillSurfaceFallbackNoSemanticAuthorityEvidence,
                "HOST_REQUESTED_SKILL_SURFACE_AVAILABLE_INTERSECTION",
                HostRequestedSkillSurfaceFallbackNoModelMutationEvidence,
                $"method={NormalizeMethod(request.Method)}",
                "semantic_quality_claimed=false"
            })
            .Concat(filteredShortcutDecoyCount > 0
                ? [$"HOST_REQUESTED_SKILL_SURFACE_SHORTCUT_DECOYS_FILTERED={filteredShortcutDecoyCount}"]
                : [])
            .Concat(string.IsNullOrWhiteSpace(extraEvidence) ? [] : [extraEvidence])
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var decisionText =
            "Host-requested available skills surfaced from the host skill surface; " +
            "fallback evidence only, no semantic authority, no semantic quality claim, no model mutation.";

        return OperationResult<SkillSelectionDecision>.Ok(new SkillSelectionDecision(
            SelectedSkillIds: selected,
            RejectedSkillIds: rejected,
            EvidenceFeatures: evidence,
            Confidence: Math.Max(priorDecision.Confidence, 0.55),
            RequiresSecondPass: false,
            DecisionText: decisionText));
    }

    private static PromptToTaskRequest BuildPromptToTaskRequest(
        XiigenExtensionRequest request,
        string promptSha256)
        => new(
            PromptId: request.CorrelationId,
            RawPrompt: request.UserPromptOrTaskText ?? string.Empty,
            RawPromptSha256: promptSha256,
            PromptProvenance: BuildPromptProvenance(request, promptSha256),
            UserLanguage: ReadFirstValueOrDefault(
                request.HostFacts,
                "unknown",
                "user_language",
                "language",
                "locale"),
            ExplicitConstraints: BuildExplicitConstraints(request.CheckSetInputs),
            ContextFields: BuildPromptContextFields(request, promptSha256));

    private static OrchestrationReasoningRequest BuildOrchestrationRequest(
        XiigenExtensionRequest request,
        StructuredTaskContract structuredTask)
        => new(
            RequestId: $"{request.CorrelationId}:orchestration",
            Task: structuredTask,
            GuideDerivedStateRefs: BuildNamedRefs(
                request.CheckSetInputs,
                request.HostFacts,
                "guide_state_ref",
                "guideStateRef"),
            CorpusBundleIds: BuildNamedRefs(
                request.CheckSetInputs,
                request.Provenance,
                "corpus_bundle_id",
                "corpusBundleId"),
            AdaptiveContextRefs: BuildAdaptiveContextRefs(request),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["external_ai"] = "forbidden_without_explicit_approval",
                ["deterministic_shortcut"] = "forbidden",
                ["semantic_quality_claimed"] = "false",
                ["host_runtime_claimed"] = "false"
            });

    private IReadOnlyDictionary<string, string> BuildCheckSetResults(
        XiigenExtensionRequest request,
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision,
        IReadOnlyList<string> selectedSkills,
        bool semanticQualityClaimed,
        CommonAssetLoadState activeLoadState)
    {
        var results = new Dictionary<string, string>
        {
            ["core_boundary_executed"] = "true",
            ["semantic_quality_claimed"] = semanticQualityClaimed.ToString().ToLowerInvariant(),
            ["runtime_extension_claimed"] = "false",
            ["prompt_to_task_status"] = promptToTask.ContractStatus,
            ["orchestration_reasoning_status"] = BuildPublicOrchestrationReasoningStatus(orchestration),
            ["orchestration_reasoning_internal_status"] = orchestration.ContractStatus,
            ["orchestration_reasoning_selected_pattern_count"] = orchestration.ScenarioPlan.SelectedLearnedPatterns.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["orchestration_reasoning_phase_family_count"] = orchestration.ScenarioPlan.PhaseFamilies.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["orchestration_reasoning_gate_family_count"] = orchestration.ScenarioPlan.GateFamilies.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["skill_selection_requires_second_pass"] = skillDecision.RequiresSecondPass.ToString().ToLowerInvariant(),
            ["skill_selection_confidence"] = skillDecision.Confidence.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["skill_selection_evidence"] = BuildHostSkillSelectionEvidence(skillDecision),
            ["skill_selection_selected_skill_count"] = selectedSkills.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["skill_selection_source_skill_count"] = skillDecision.SelectedSkillIds.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["skill_selection_requested_skill_count"] = SafeList(request.RequestedSkills).Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["skill_selection_available_skill_count"] = SafeList(request.AvailableSkills).Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["requested_skills_count"] = SafeList(request.RequestedSkills).Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["selected_skills_count"] = selectedSkills.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["available_skills_count"] = SafeList(request.AvailableSkills).Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["skill_selection_method_semantic_surface_required"] = RequiresSkillSurface(request).ToString().ToLowerInvariant(),
            ["required_skill_selection_absent"] = (RequiresSkillSurface(request) && selectedSkills.Count == 0).ToString().ToLowerInvariant(),
            ["skill_selection_method_rationale"] = BuildMethodSpecificSkillRationale(request, skillDecision)
        };

        foreach (var entry in BuildHostQualityProjectionResults(
            request,
            promptToTask,
            orchestration,
            skillDecision,
            selectedSkills,
            semanticQualityClaimed,
            activeLoadState))
        {
            results[entry.Key] = entry.Value;
        }

        if (IsUserAdaptiveLoaded(activeLoadState) && skillDecision.SelectedSkillIds.Count > selectedSkills.Count)
            results["skill_selection_host_display_cap"] = "adaptive_top_3";

        foreach (var entry in activeLoadState.CheckSetRefs)
            results[entry.Key] = entry.Value;

        return results;
    }

    private static IReadOnlyDictionary<string, string> BuildHostQualityProjectionResults(
        XiigenExtensionRequest request,
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision,
        IReadOnlyList<string> selectedSkills,
        bool semanticQualityClaimed,
        CommonAssetLoadState activeLoadState)
    {
        var references = BuildQualityProjectionReferences(request, promptToTask, orchestration);
        var promptToTaskProjection = BuildPromptToTaskProjection(request, promptToTask);
        var taskToPlanProjection = BuildTaskToPlanProjection(orchestration);
        var taskToSkillsProjection = BuildTaskToSkillsProjection(request, skillDecision, selectedSkills);
        var nonClaims = BuildNonClaimsProjection(semanticQualityClaimed);
        var disclosures = BuildDeterministicBaselineDisclosures(activeLoadState);
        var fullProjection = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["task"] = promptToTaskProjection,
            ["prompt_to_task"] = promptToTaskProjection,
            ["plan"] = taskToPlanProjection,
            ["task_to_plan"] = taskToPlanProjection,
            ["skills"] = taskToSkillsProjection,
            ["task_to_skills"] = taskToSkillsProjection,
            ["references"] = references,
            ["non_claims"] = nonClaims,
            ["shortcut_disclosures"] = disclosures,
            ["deterministic_baseline_disclosures"] = disclosures,
            ["external_ai_used"] = false,
            ["training_started"] = false,
            ["token_savings_claimed"] = false,
            ["semantic_quality_claimed"] = semanticQualityClaimed,
            ["readiness_claimed"] = false
        };

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["prompt_to_task"] = SerializeProjection(promptToTaskProjection),
            ["task_to_plan"] = SerializeProjection(taskToPlanProjection),
            ["task_to_skills"] = SerializeProjection(taskToSkillsProjection),
            ["references"] = SerializeProjection(references),
            ["non_claims"] = SerializeProjection(nonClaims),
            ["deterministic_baseline_disclosures"] = SerializeProjection(disclosures),
            ["row12_core_api_quality_projection"] = SerializeProjection(fullProjection)
        };
    }

    private static IReadOnlyDictionary<string, object> BuildPromptToTaskProjection(
        XiigenExtensionRequest request,
        PromptToTaskResponse promptToTask)
    {
        var task = promptToTask.StructuredTask;
        var constraints = task.Constraints
            .Concat(task.ForbiddenActions.Select(action => $"forbidden:{action}"))
            .Concat(BuildNoClaimConstraintRefs(request))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var acceptanceGates = new[]
        {
            "prompt_to_task response includes task_id, task_title, constraints, and acceptance_gates",
            "task_to_plan response includes exactly 3 ordered steps and one verification gate per step",
            "task_to_skills response selects required skills only from the host available skill surface",
            "non_claims keep token savings, semantic quality, readiness, training, checkpoint promotion, and locator update claims closed"
        };

        return new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["task_id"] = task.PromptId,
            ["task_title"] = BuildTaskTitle(request, task),
            ["task_type"] = task.TaskType,
            ["requirements"] = task.ExplicitUserRequirements,
            ["constraints"] = constraints,
            ["deliverables"] = task.Deliverables,
            ["acceptance_gates"] = acceptanceGates,
            ["evidence_needs"] = task.EvidenceNeeds,
            ["required_hosts"] = task.RequiredHosts,
            ["raw_prompt_redacted"] = true,
            ["raw_prompt_sha256"] = task.RawPromptSha256,
            ["contract_status"] = promptToTask.ContractStatus,
            ["contract_evidence"] = promptToTask.ContractEvidence
        };
    }

    private static IReadOnlyList<string> BuildNoClaimConstraintRefs(
        XiigenExtensionRequest request)
    {
        var refs = new List<string>
        {
            "external_ai_send=false",
            "token_savings_claimed=false",
            "semantic_quality_claimed=false",
            "readiness_claimed=false",
            "training_started=false",
            "checkpoint_mutation=false",
            "locator_update=false"
        };

        foreach (var entry in SafeDictionary(request.CheckSetInputs))
        {
            if (entry.Key.StartsWith("constraint.", StringComparison.OrdinalIgnoreCase)
                || entry.Key.StartsWith("constraint:", StringComparison.OrdinalIgnoreCase))
            {
                refs.Add($"{entry.Key}=present");
            }
        }

        return refs.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyDictionary<string, object> BuildTaskToPlanProjection(
        OrchestrationReasoningResponse orchestration)
        => new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["scenario_id"] = orchestration.ScenarioPlan.ScenarioId,
            ["steps"] = "3",
            ["ordered_steps"] = new object[]
            {
                new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["step"] = "step 1",
                    ["title"] = "extract bounded task contract",
                    ["uses"] = "prompt_to_task",
                    ["verification_gate"] = "verification gate: task_id, task_title, constraints, and acceptance_gates are present"
                },
                new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["step"] = "step 2",
                    ["title"] = "derive universal execution plan",
                    ["uses"] = "task_to_plan",
                    ["verification_gate"] = "verification gate: exactly three ordered steps are present"
                },
                new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["step"] = "step 3",
                    ["title"] = "select required skills and evidence boundaries",
                    ["uses"] = "task_to_skills",
                    ["verification_gate"] = "verification gate: selected skills and non_claims are present"
                }
            },
            ["phase_families"] = orchestration.ScenarioPlan.PhaseFamilies,
            ["gate_families"] = orchestration.ScenarioPlan.GateFamilies,
            ["asset_families"] = orchestration.ScenarioPlan.AssetFamilies,
            ["risk_flags"] = orchestration.ScenarioPlan.RiskFlags,
            ["next_actions"] = orchestration.ScenarioPlan.NextGranularizationActions,
            ["contract_status"] = orchestration.ContractStatus,
            ["contract_evidence"] = orchestration.ContractEvidence
        };

    private static IReadOnlyDictionary<string, object> BuildTaskToSkillsProjection(
        XiigenExtensionRequest request,
        SkillSelectionDecision skillDecision,
        IReadOnlyList<string> selectedSkills)
        => new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["selected_required_skills"] = selectedSkills,
            ["requested_skills"] = SafeList(request.RequestedSkills),
            ["available_skill_count"] = SafeList(request.AvailableSkills).Count,
            ["references"] = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["selection_source"] = "host_available_skill_surface_and_core_skill_selection",
                ["decision_text"] = skillDecision.DecisionText,
                ["confidence"] = skillDecision.Confidence.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
                ["requires_second_pass"] = skillDecision.RequiresSecondPass
            },
            ["evidence"] = skillDecision.EvidenceFeatures,
            ["semantic_quality_claimed"] = false,
            ["training_started"] = false
        };

    private static IReadOnlyDictionary<string, object> BuildQualityProjectionReferences(
        XiigenExtensionRequest request,
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration)
        => new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["marker"] = ReadFirstValueOrDefault(request.CheckSetInputs, "not-provided", "marker"),
            ["scenario_id"] = ReadFirstValueOrDefault(request.CheckSetInputs, "not-provided", "scenario_id", "scenarioId"),
            ["pair_id"] = ReadFirstValueOrDefault(request.CheckSetInputs, "not-provided", "pair_id", "pairId"),
            ["source"] = ReadFirstValueOrDefault(request.Provenance, "host-request", "source"),
            ["structured_task_refs"] = BuildStructuredTaskRefs(promptToTask.StructuredTask),
            ["scenario_refs"] = BuildScenarioRefs(orchestration.ScenarioPlan),
            ["raw_prompt_sha256"] = promptToTask.StructuredTask.RawPromptSha256,
            ["raw_prompt_redacted"] = true
        };

    private static IReadOnlyList<string> BuildNonClaimsProjection(
        bool semanticQualityClaimed)
    {
        var claims = new List<string>
        {
            "no token savings claim",
            "no semantic quality claim",
            "no readiness claim",
            "no training",
            "no checkpoint promotion",
            "no locator update",
            "no external AI",
            "no external AI send"
        };

        if (semanticQualityClaimed)
            claims.Add("semantic quality remains gated");

        return claims;
    }

    private static IReadOnlyList<string> BuildDeterministicBaselineDisclosures(
        CommonAssetLoadState activeLoadState)
    {
        var disclosures = new List<string>
        {
            "synthetic no-send workload",
            "heuristic tokenizer/accounting",
            "no external AI send",
            "token delta alone not quality proof",
            "deterministic baseline projection disclosed; not semantic inference",
            "no training, checkpoint promotion, or locator update performed"
        };

        if (IsUserAdaptiveLoaded(activeLoadState))
            disclosures.Add("user adaptive asset path may be loaded, but this response makes no learnability claim");

        return disclosures;
    }

    private static string BuildTaskTitle(
        XiigenExtensionRequest request,
        StructuredTaskContract task)
    {
        var scenarioId = ReadFirstValueOrDefault(request.CheckSetInputs, string.Empty, "scenario_id", "scenarioId");
        return string.IsNullOrWhiteSpace(scenarioId)
            ? $"Bounded Smart Skills task for {task.TaskType}"
            : $"Bounded Smart Skills task for {scenarioId}";
    }

    private static string SerializeProjection(object value)
        => JsonSerializer.Serialize(value, JsonOptions);

    private static string BuildPublicOrchestrationReasoningStatus(
        OrchestrationReasoningResponse orchestration)
        => IsStructuredLearnedOrchestrationPlan(orchestration)
            ? OrchestrationReasoningMachine.PublicPlanProducedStatus
            : orchestration.ContractStatus;

    private static bool IsStructuredLearnedOrchestrationPlan(
        OrchestrationReasoningResponse orchestration)
    {
        if (!string.Equals(orchestration.ContractStatus, OrchestrationReasoningMachine.LearnedPatternStatus, StringComparison.Ordinal))
            return false;
        if (string.Equals(orchestration.ScenarioPlan.ScenarioId, OrchestrationReasoningMachine.NoInferenceScenarioId, StringComparison.Ordinal))
            return false;
        if (orchestration.ScenarioPlan.SelectedLearnedPatterns.Count == 0
            || orchestration.ScenarioPlan.PhaseFamilies.Count == 0
            || orchestration.ScenarioPlan.GateFamilies.Count == 0
            || orchestration.ScenarioPlan.NextGranularizationActions.Count == 0)
        {
            return false;
        }

        var planText = string.Join(
            " ",
            orchestration.ScenarioPlan.PhaseFamilies
                .Concat(orchestration.ScenarioPlan.GateFamilies)
                .Concat(orchestration.ScenarioPlan.AssetFamilies)
                .Concat(orchestration.ScenarioPlan.RiskFlags)
                .Concat(orchestration.ScenarioPlan.NextGranularizationActions));
        return ContainsAny(planText, "review")
            && ContainsAny(planText, "rollback")
            && ContainsAny(planText, "evidence")
            && (ContainsAny(planText, "stop") || ContainsAny(planText, "halt"));
    }

    private static bool ContainsAny(string text, string value)
        => text.Contains(value, StringComparison.OrdinalIgnoreCase);

    private IReadOnlyList<string> BuildWarnings(
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision,
        bool semanticQualityClaimed,
        CommonAssetLoadState activeLoadState)
    {
        if (IsUserAdaptiveLoaded(activeLoadState)
            && !skillDecision.EvidenceFeatures.Contains("NO_TRAINED_STATE_AVAILABLE", StringComparer.Ordinal)
            && !skillDecision.EvidenceFeatures.Contains("NO_AVAILABLE_SKILLS_PROVIDED", StringComparer.Ordinal))
        {
            return
            [
                $"semantic_quality_claimed={semanticQualityClaimed.ToString().ToLowerInvariant()}",
                "core_boundary_only_no_host_runtime_claim",
                "user_adaptive_learned_state_loaded",
                "skill_selection_semantic_quality_unclaimed"
            ];
        }

        var warnings = new List<string>
        {
            $"semantic_quality_claimed={semanticQualityClaimed.ToString().ToLowerInvariant()}",
            "core_boundary_only_no_host_runtime_claim",
            $"PromptToTaskMachine status: {promptToTask.ContractStatus}.",
            $"OrchestrationReasoningMachine status: {orchestration.ContractStatus}."
        };

        warnings.AddRange(activeLoadState.Warnings);

        if (skillDecision.EvidenceFeatures.Contains("NO_TRAINED_STATE_AVAILABLE", StringComparer.Ordinal)
            || skillDecision.EvidenceFeatures.Contains("NO_AVAILABLE_SKILLS_PROVIDED", StringComparer.Ordinal))
        {
            warnings.Add($"SkillSelectionMachine status: {skillDecision.DecisionText}");
        }
        else
        {
            warnings.Add("SkillSelectionMachine used loaded baseline/typed trained state; semantic quality remains unclaimed.");
        }

        return warnings;
    }

    private IReadOnlyList<string> BuildHostSelectedSkills(
        SkillSelectionDecision skillDecision,
        CommonAssetLoadState activeLoadState)
    {
        return SafeList(skillDecision.SelectedSkillIds);
    }

    private string BuildHostChatDisplayText(
        XiigenExtensionRequest request,
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision,
        IReadOnlyList<string> selectedSkills,
        bool semanticQualityClaimed,
        CommonAssetLoadState activeLoadState)
    {
        var prefix = IsUserAdaptiveLoaded(activeLoadState)
            ? "Adaptive task+plan+skills ready."
            : "Core task+plan+skills ready.";
        var skillText = BuildSelectedSkillDisplayText(selectedSkills, skillDecision.SelectedSkillIds.Count);
        var statusText = semanticQualityClaimed
            ? "Semantic quality remains gated."
            : "Semantic quality is not claimed.";
        var learnedStateText = IsUserAdaptiveLoaded(activeLoadState)
            && promptToTask.ContractStatus.Contains("learned_state", StringComparison.Ordinal)
            && orchestration.ContractStatus.Contains("loaded_guide_state", StringComparison.Ordinal)
                ? " Learned state used."
                : string.Empty;
        var methodText = BuildMethodSpecificDisplayText(request, promptToTask, orchestration, skillDecision);

        return string.IsNullOrWhiteSpace(skillText)
            ? $"{prefix}{learnedStateText} {methodText} {statusText}"
            : $"{prefix}{learnedStateText} {methodText} Skills: {skillText}. {statusText}";
    }

    private static string BuildMethodSpecificDisplayText(
        XiigenExtensionRequest request,
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision)
    {
        var method = NormalizeMethod(request.Method);
        return method switch
        {
            XiigenExtensionApiContract.PromptToTaskMethod =>
                $"Prompt-to-task produced task type {promptToTask.StructuredTask.TaskType} with {promptToTask.StructuredTask.Deliverables.Count} deliverable refs.",
            XiigenExtensionApiContract.OrchestrationPlanMethod =>
                $"Orchestration-plan produced a bounded scenario plan with {orchestration.ScenarioPlan.PhaseFamilies.Count} phase refs.",
            XiigenExtensionApiContract.SkillSelectionMethod =>
                IsHostRequestedSkillSurfaceFallback(skillDecision)
                    ? $"Skill-selection chose {skillDecision.SelectedSkillIds.Count} host-requested available skills; no semantic authority or quality claim."
                    : $"Skill-selection chose {skillDecision.SelectedSkillIds.Count} semantic skills with confidence {skillDecision.Confidence:0.###}.",
            _ =>
                $"Method {method} produced task, plan, and skill evidence."
        };
    }

    private static string BuildMethodSpecificSkillRationale(
        XiigenExtensionRequest request,
        SkillSelectionDecision skillDecision)
    {
        var method = NormalizeMethod(request.Method);
        var selected = skillDecision.SelectedSkillIds.Count == 0
            ? "none"
            : string.Join("|", skillDecision.SelectedSkillIds.Take(5));
        var evidence = BuildHostSkillSelectionEvidence(skillDecision);
        var reason = IsHostRequestedSkillSurfaceFallback(skillDecision)
            ? "host_requested_available_skill_surface_fallback_no_semantic_authority"
            : "semantic_task_and_host_skill_surface";
        return $"method={method};selected={selected};reason={reason};evidence={evidence}";
    }

    private static bool IsHostRequestedSkillSurfaceFallback(SkillSelectionDecision skillDecision)
        => skillDecision.EvidenceFeatures.Contains(HostRequestedSkillSurfaceFallbackNoModelMutationEvidence, StringComparer.Ordinal);

    private static bool RequiresSkillSurface(XiigenExtensionRequest request)
        => string.Equals(NormalizeMethod(request.Method), XiigenExtensionApiContract.SkillSelectionMethod, StringComparison.Ordinal)
            || SafeList(request.RequestedSkills).Count > 0
            || SafeList(request.AvailableSkills).Count > 0;

    private static string NormalizeMethod(string? method)
        => string.IsNullOrWhiteSpace(method)
            ? "unknown"
            : method.Trim();

    private static string BuildSelectedSkillDisplayText(
        IReadOnlyList<string> selectedSkills,
        int sourceSkillCount)
    {
        if (selectedSkills.Count == 0)
            return string.Empty;

        var suffix = sourceSkillCount > selectedSkills.Count
            ? $" +{sourceSkillCount - selectedSkills.Count}"
            : string.Empty;
        return $"{string.Join(", ", selectedSkills)}{suffix}";
    }

    private static string BuildHostSkillSelectionEvidence(SkillSelectionDecision skillDecision)
    {
        if (skillDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal))
            return string.Join(",", skillDecision.EvidenceFeatures);

        var compactEvidence = new List<string>();
        AddEvidenceIfPresent(compactEvidence, skillDecision.EvidenceFeatures, SkillSelectionMachine.SemanticPipelineBoundaryEvidence);
        AddFirstEvidenceEndingWith(compactEvidence, skillDecision.EvidenceFeatures, "_UNIT_EXECUTED");
        AddFirstEvidenceEndingWith(compactEvidence, skillDecision.EvidenceFeatures, "_CHECKPOINT_SECTION");
        AddEvidenceIfPresent(compactEvidence, skillDecision.EvidenceFeatures, HostRequestedSkillSurfaceFallbackNoSemanticAuthorityEvidence);
        AddFirstEvidenceContaining(compactEvidence, skillDecision.EvidenceFeatures, "HOST_REQUESTED_SKILL_SURFACE");
        AddEvidenceIfPresent(compactEvidence, skillDecision.EvidenceFeatures, HostRequestedSkillSurfaceFallbackNoModelMutationEvidence);
        AddFirstEvidenceContaining(compactEvidence, skillDecision.EvidenceFeatures, "HOST_REQUESTED_SKILL_SURFACE_SHORTCUT_DECOYS_FILTERED");
        AddEvidenceIfPresent(compactEvidence, skillDecision.EvidenceFeatures, UserAdaptiveSkillSelectionEmptyCommonHostSurfaceFallbackEvidence);
        AddEvidenceIfPresent(compactEvidence, skillDecision.EvidenceFeatures, UserAdaptiveSkillSelectionEmptyCommonHostSurfaceFallbackUnavailableEvidence);
        AddEvidenceIfPresent(compactEvidence, skillDecision.EvidenceFeatures, UserAdaptiveSkillSelectionFallbackNoModelMutationEvidence);
        AddFirstEvidenceContaining(compactEvidence, skillDecision.EvidenceFeatures, "semantic_quality_claimed=false");

        if (compactEvidence.Count == 0)
            compactEvidence.AddRange(skillDecision.EvidenceFeatures.Take(3));

        compactEvidence.Add("api_evidence_compacted=true");
        return string.Join(",", compactEvidence.Distinct(StringComparer.Ordinal));
    }

    private static bool IsUserAdaptiveLoaded(CommonAssetLoadState activeLoadState)
        => activeLoadState.CheckSetRefs.TryGetValue("user_adaptive_any_asset_loaded", out var loaded)
            && string.Equals(loaded, "true", StringComparison.Ordinal);

    private static void AddEvidenceIfPresent(
        ICollection<string> target,
        IReadOnlyList<string> evidence,
        string value)
    {
        if (evidence.Contains(value, StringComparer.Ordinal))
            target.Add(value);
    }

    private static void AddFirstEvidenceEndingWith(
        ICollection<string> target,
        IReadOnlyList<string> evidence,
        string suffix)
    {
        var value = evidence.FirstOrDefault(item => item.EndsWith(suffix, StringComparison.Ordinal));
        if (!string.IsNullOrWhiteSpace(value))
            target.Add(value);
    }

    private static void AddFirstEvidenceContaining(
        ICollection<string> target,
        IReadOnlyList<string> evidence,
        string value)
    {
        var match = evidence.FirstOrDefault(item => item.Contains(value, StringComparison.Ordinal));
        if (!string.IsNullOrWhiteSpace(match))
            target.Add(match);
    }

    private static IReadOnlyList<string> BuildStructuredTaskRefs(
        StructuredTaskContract structuredTask)
        =>
        [
            $"structured-task:{structuredTask.PromptId}:{structuredTask.SplitId}:{structuredTask.RawPromptSha256}"
        ];

    private static IReadOnlyList<string> BuildScenarioRefs(
        OrchestrationScenarioPlan scenarioPlan)
        => [$"scenario:{scenarioPlan.ScenarioId}"];

    private static IReadOnlyDictionary<string, string> BuildPromptContextFields(
        XiigenExtensionRequest request,
        string promptSha256)
    {
        var fields = new Dictionary<string, string>
        {
            ["host_name"] = request.HostName,
            ["host_type"] = request.HostType,
            ["xiigen_method"] = request.Method,
            ["correlation_id"] = request.CorrelationId,
            ["raw_prompt_sha256"] = promptSha256,
            ["raw_prompt_redacted"] = "true"
        };

        foreach (var entry in SafeDictionary(request.HostFacts))
            fields[$"host_fact.{entry.Key}"] = entry.Value;

        foreach (var entry in SafeDictionary(request.Provenance))
            fields[$"provenance_key.{entry.Key}"] = "present";

        return fields;
    }

    private static IReadOnlyList<string> BuildExplicitConstraints(
        IReadOnlyDictionary<string, string>? checkSetInputs)
    {
        var constraints = new List<string>();
        foreach (var entry in SafeDictionary(checkSetInputs))
        {
            if (entry.Key.StartsWith("constraint.", StringComparison.OrdinalIgnoreCase)
                || entry.Key.StartsWith("constraint:", StringComparison.OrdinalIgnoreCase))
            {
                constraints.Add($"{entry.Key}=present");
            }
        }

        return constraints;
    }

    private static string BuildPromptProvenance(
        XiigenExtensionRequest request,
        string promptSha256)
    {
        var source = ReadFirstValueOrDefault(
            request.Provenance,
            "host-request",
            "source_prompt_entry_id",
            "sourcePromptEntryId",
            "source",
            "ledger_id",
            "ledgerId");
        return $"source:{source};correlation:{request.CorrelationId};prompt_sha256:{promptSha256}";
    }

    private static IReadOnlyDictionary<string, string> BuildAdaptiveContextRefs(
        XiigenExtensionRequest request)
    {
        var refs = new Dictionary<string, string>
        {
            ["user_adaptive_state"] = ReadFirstValueOrDefault(
                request.Provenance,
                "not-loaded",
                "user_adaptive_ref",
                "userAdaptiveRef",
                "project_local_state_ref",
                "projectLocalStateRef")
        };

        if (request.TargetLocator is not null)
        {
            refs["target_locator_candidate_id"] = request.TargetLocator.LocatorCandidateId;
            refs["target_candidate_id"] = request.TargetLocator.TargetCandidateId;
            refs["rollback_candidate_id"] = request.TargetLocator.RollbackCandidateId;
            refs["target_locator_mode"] = NormalizeTargetLocatorMode(request.TargetLocator);
        }

        return refs;
    }

    private static IReadOnlyList<string> BuildNamedRefs(
        IReadOnlyDictionary<string, string>? primary,
        IReadOnlyDictionary<string, string>? secondary,
        params string[] keys)
    {
        var refs = new List<string>();
        foreach (var key in keys)
        {
            var value = ReadFirstValue(primary, key);
            if (!string.IsNullOrWhiteSpace(value))
                refs.Add(value);
        }

        foreach (var key in keys)
        {
            var value = ReadFirstValue(secondary, key);
            if (!string.IsNullOrWhiteSpace(value)
                && !refs.Contains(value, StringComparer.Ordinal))
            {
                refs.Add(value);
            }
        }

        return refs;
    }

    private static XiigenDebugTrace BuildDebugTrace(
        XiigenExtensionRequest request,
        string promptSha256,
        XiigenExtensionResponse response)
    {
        var inputPayload = JsonSerializer.Serialize(
            new
            {
                request.Method,
                request.HostName,
                request.HostType,
                request.CorrelationId,
                UserPromptSha256 = promptSha256,
                UserPromptOrTaskText = "[redacted]",
                TargetLocatorPresent = request.TargetLocator is not null,
                TargetLocator = BuildTargetLocatorDebugPayload(request.TargetLocator),
                RequestedSkills = SafeList(request.RequestedSkills),
                AvailableSkillCount = SafeList(request.AvailableSkills).Count,
                HostFactKeys = SafeDictionary(request.HostFacts).Keys.Order(StringComparer.Ordinal).ToArray(),
                CheckSetInputKeys = SafeDictionary(request.CheckSetInputs).Keys.Order(StringComparer.Ordinal).ToArray(),
                ProvenanceKeys = SafeDictionary(request.Provenance).Keys.Order(StringComparer.Ordinal).ToArray()
            },
            JsonOptions);
        var outputPayload = JsonSerializer.Serialize(
            new
            {
                response.Success,
                response.Status,
                response.SelectedSkills,
                response.StructuredTaskRefs,
                response.ScenarioRefs,
                response.Warnings,
                response.Errors,
                QualityProjection = BuildDebugQualityProjection(response.CheckSetResults),
                SemanticQualityClaimed = response.CheckSetResults.TryGetValue("semantic_quality_claimed", out var semanticQualityClaimed)
                    ? semanticQualityClaimed
                    : "false",
                RuntimeExtensionClaimed = false
            },
            JsonOptions);

        return new XiigenDebugTrace(
            Method: string.IsNullOrWhiteSpace(request.Method) ? "invalid" : request.Method,
            InputPayloadText: inputPayload,
            OutputPayloadText: outputPayload,
            ContainsRawPrivateContent: false,
            ContainsSecrets: false);
    }

    private static IReadOnlyDictionary<string, string> BuildDebugQualityProjection(
        IReadOnlyDictionary<string, string> checkSetResults)
    {
        var projection = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in new[]
        {
            "prompt_to_task",
            "task_to_plan",
            "task_to_skills",
            "references",
            "non_claims",
            "deterministic_baseline_disclosures",
            "row12_core_api_quality_projection"
        })
        {
            if (checkSetResults.TryGetValue(key, out var value))
                projection[key] = value;
        }

        return projection;
    }

    private static object? BuildTargetLocatorDebugPayload(XiigenTargetLocator? locator)
        => locator is null
            ? null
            : new
            {
                locator.LocatorCandidateId,
                locator.TargetCandidateId,
                locator.RollbackCandidateId,
                LocatorMode = NormalizeTargetLocatorMode(locator),
                UserAdaptiveAssetRoot = RedactPathForDebug(locator.UserAdaptiveAssetRoot),
                RollbackAssetRoot = RedactPathForDebug(locator.RollbackAssetRoot),
                LocatorRefKeys = SafeDictionary(locator.LocatorRefs).Keys.Order(StringComparer.Ordinal).ToArray()
            };

    private static string RedactPathForDebug(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        try
        {
            return RedactPath(ResolveTargetLocatorRoot(path));
        }
        catch
        {
            return Path.GetFileName(path);
        }
    }

    private static string ComputeSha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static IReadOnlyDictionary<string, string> SafeDictionary(
        IReadOnlyDictionary<string, string>? source)
        => source ?? new Dictionary<string, string>();

    private static IReadOnlyList<string> SafeList(IReadOnlyList<string>? source)
        => source ?? [];

    private static bool IsShortcutDecoySkillId(string? skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
            return true;

        var normalized = skillId.Trim().ToLowerInvariant();
        return IsSha256ShapedId(normalized)
            || normalized.Contains("row-id", StringComparison.Ordinal)
            || normalized.Contains("row_id", StringComparison.Ordinal)
            || normalized.Contains("scenario-id", StringComparison.Ordinal)
            || normalized.Contains("scenario_id", StringComparison.Ordinal)
            || normalized.Contains("pair-id", StringComparison.Ordinal)
            || normalized.Contains("pair_id", StringComparison.Ordinal)
            || normalized.Contains("exact-prompt", StringComparison.Ordinal)
            || normalized.Contains("exact_prompt", StringComparison.Ordinal)
            || normalized.Contains("output-hash", StringComparison.Ordinal)
            || normalized.Contains("output_hash", StringComparison.Ordinal)
            || normalized.Contains("prompt-hash", StringComparison.Ordinal)
            || normalized.Contains("prompt_hash", StringComparison.Ordinal)
            || normalized.Contains("prompt.hash", StringComparison.Ordinal)
            || normalized.Contains("prompthash", StringComparison.Ordinal)
            || normalized.Contains("prompt/hash", StringComparison.Ordinal)
            || normalized.StartsWith("sha256-", StringComparison.Ordinal)
            || normalized.StartsWith("sha256_", StringComparison.Ordinal)
            || normalized.StartsWith("sha256:", StringComparison.Ordinal);
    }

    private static bool IsSha256ShapedId(string value)
    {
        if (value.Length != 64)
            return false;

        return value.All(IsHexDigit);
    }

    private static bool IsHexDigit(char value)
        => value is >= '0' and <= '9'
            || value is >= 'a' and <= 'f';

    private static string ReadFirstValue(
        IReadOnlyDictionary<string, string>? source,
        params string[] keys)
        => ReadFirstValueOrDefault(source, string.Empty, keys);

    private static string ReadFirstValueOrDefault(
        IReadOnlyDictionary<string, string>? source,
        string fallback,
        params string[] keys)
    {
        foreach (var key in keys)
        {
            if (source is not null
                && source.TryGetValue(key, out var value)
                && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return fallback;
    }

    private bool ShouldClaimSemanticQuality(
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision)
        // Learned status names are runtime state labels only. Semantic quality requires
        // explicit future positive evidence from every component and no no-claim evidence.
        => _commonAssetLoadState.AllLoaded
            && !skillDecision.RequiresSecondPass
            && !skillDecision.EvidenceFeatures.Contains("NO_TRAINED_STATE_AVAILABLE", StringComparer.Ordinal)
            && !skillDecision.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal)
            && HasPositiveSemanticQualityEvidence(promptToTask.ContractEvidence)
            && HasPositiveSemanticQualityEvidence(orchestration.ContractEvidence)
            && HasPositiveSemanticQualityEvidence(skillDecision.EvidenceFeatures)
            && !HasSemanticNoClaimEvidence(promptToTask.ContractEvidence)
            && !HasSemanticNoClaimEvidence(orchestration.ContractEvidence)
            && !HasSemanticNoClaimEvidence(skillDecision.EvidenceFeatures);

    private static bool HasPositiveSemanticQualityEvidence(IReadOnlyList<string> evidence)
        => evidence.Contains(PositiveSemanticQualityEvidence, StringComparer.Ordinal);

    private static bool HasSemanticNoClaimEvidence(IReadOnlyList<string> evidence)
        => evidence.Any(item => SemanticNoClaimEvidence.Any(noClaim =>
            string.Equals(item, noClaim, StringComparison.Ordinal)
            || item.Contains(noClaim, StringComparison.Ordinal)));

    private static CommonAssetLoadState LoadCommonAssets(string repoRoot)
    {
        var root = string.IsNullOrWhiteSpace(repoRoot) ? FindRepoRoot() : Path.GetFullPath(repoRoot);
        var warnings = new List<string>();
        var refs = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["trained_model_gate_ref"] = "API-01",
            ["common_asset_root"] = RedactPath(root)
        };

        var promptToTask = new PromptToTaskMachine();
        var orchestration = new OrchestrationReasoningMachine();
        var skillSelection = new SkillSelectionMachine();

        var pttManifest = Path.Combine(root, "models", "prompt-to-task", "prompt-to-task-v1", "manifest.json");
        var pttCheckpoint = Path.Combine(root, "models", "prompt-to-task", "prompt-to-task-v1", "prompt-to-task-v1-checkpoint.json");
        var pttLoad = PromptToTaskMachine.LoadCheckpointAsync(pttCheckpoint, pttManifest).GetAwaiter().GetResult();
        var pttLoaded = pttLoad.IsSuccess;
        if (pttLoaded)
        {
            promptToTask = pttLoad.Value!.Machine;
            refs["prompt_to_task_asset_loaded"] = "true";
            refs["prompt_to_task_manifest_ref"] = RedactPath(pttManifest);
            refs["prompt_to_task_checkpoint_ref"] = RedactPath(pttCheckpoint);
            refs["prompt_to_task_checkpoint_sha256"] = pttLoad.Value.Manifest.CheckpointSha256;
        }
        else
        {
            refs["prompt_to_task_asset_loaded"] = "false";
            refs["prompt_to_task_load_error"] = pttLoad.ErrorCode;
            warnings.Add($"PromptToTask common asset load skipped/failed: {pttLoad.ErrorCode}");
        }

        var orManifest = Path.Combine(root, "models", "orchestration-reasoning", "orchestration-reasoning-v1", "manifest.json");
        var orCheckpoint = ResolveManifestCheckpointRef(orManifest)
            ?? Path.Combine(root, "models", "orchestration-reasoning", "orchestration-reasoning-v1", "checkpoint.json");
        var orLoad = OrchestrationReasoningMachine.LoadCheckpointAsync(orCheckpoint, orManifest).GetAwaiter().GetResult();
        var orLoaded = orLoad.IsSuccess;
        if (orLoaded)
        {
            orchestration = orLoad.Value!.Machine;
            refs["orchestration_reasoning_asset_loaded"] = "true";
            refs["orchestration_reasoning_manifest_ref"] = RedactPath(orManifest);
            refs["orchestration_reasoning_checkpoint_ref"] = RedactPath(orCheckpoint);
            refs["orchestration_reasoning_checkpoint_sha256"] = orLoad.Value.Manifest.CheckpointSha256;
        }
        else
        {
            refs["orchestration_reasoning_asset_loaded"] = "false";
            refs["orchestration_reasoning_load_error"] = orLoad.ErrorCode;
            warnings.Add($"OrchestrationReasoning common asset load skipped/failed: {orLoad.ErrorCode}");
        }

        var skillManifest = Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "manifest.json");
        var skillCheckpoint = ResolveManifestCheckpointRef(skillManifest)
            ?? Path.Combine(root, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var skillLoad = LoadSkillSelectionCommonCheckpoint(skillCheckpoint);
        var skillLoaded = skillLoad.IsSuccess;
        if (skillLoaded)
        {
            skillSelection = skillLoad.Value!.Machine;
            refs["skill_selection_asset_loaded"] = "true";
            refs["skill_selection_manifest_ref"] = RedactPath(skillManifest);
            refs["skill_selection_checkpoint_ref"] = RedactPath(skillCheckpoint);
            refs["skill_selection_checkpoint_sha256"] = TryReadSkillSelectionCheckpointSha(skillManifest);
        }
        else
        {
            refs["skill_selection_asset_loaded"] = "false";
            refs["skill_selection_load_error"] = skillLoad.ErrorCode;
            warnings.Add($"SkillSelection common asset load skipped/failed: {skillLoad.ErrorCode}");
        }

        refs["all_common_assets_loaded"] = (pttLoaded && orLoaded && skillLoaded).ToString().ToLowerInvariant();

        return new CommonAssetLoadState(
            promptToTask,
            orchestration,
            skillSelection,
            skillSelection,
            refs,
            warnings,
            pttLoaded && orLoaded && skillLoaded);
    }

    private static CommonAssetLoadState LoadCommonThenUserAdaptiveAssets(
        string commonAssetRoot,
        string userAdaptiveAssetRoot)
    {
        var common = LoadCommonAssets(commonAssetRoot);
        var userRoot = Path.GetFullPath(userAdaptiveAssetRoot);
        var refs = new Dictionary<string, string>(common.CheckSetRefs, StringComparer.Ordinal)
        {
            ["adaptive_asset_root"] = RedactPath(userRoot)
        };
        var warnings = new List<string>(common.Warnings);

        var promptToTask = common.PromptToTaskMachine;
        var orchestration = common.OrchestrationReasoningMachine;
        var skillSelection = common.SkillSelectionMachine;

        var pttManifest = Path.Combine(userRoot, "prompt-to-task", "prompt-to-task-v1", "manifest.json");
        var pttCheckpoint = Path.Combine(userRoot, "prompt-to-task", "prompt-to-task-v1", "prompt-to-task-v1-checkpoint.json");
        var pttLoad = PromptToTaskMachine.LoadCheckpointAsync(pttCheckpoint, pttManifest).GetAwaiter().GetResult();
        if (pttLoad.IsSuccess)
        {
            promptToTask = pttLoad.Value!.Machine;
            refs["prompt_to_task_asset_source"] = "user_adaptive_cache";
            refs["prompt_to_task_user_asset_loaded_first"] = "true";
            refs["prompt_to_task_user_manifest_ref"] = RedactPath(pttManifest);
            refs["prompt_to_task_user_checkpoint_ref"] = RedactPath(pttCheckpoint);
        }
        else
        {
            refs["prompt_to_task_user_asset_loaded_first"] = "false";
            refs["prompt_to_task_user_load_error"] = pttLoad.ErrorCode;
            refs["prompt_to_task_asset_source"] = refs.TryGetValue("prompt_to_task_asset_loaded", out var loaded) && loaded == "true"
                ? "repo_common_fallback"
                : "none";
        }

        var orManifest = Path.Combine(userRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "manifest.json");
        var orCheckpoint = ResolveManifestCheckpointRef(orManifest)
            ?? Path.Combine(userRoot, "orchestration-reasoning", "orchestration-reasoning-v1", "checkpoint.json");
        var orLoad = OrchestrationReasoningMachine.LoadCheckpointAsync(orCheckpoint, orManifest).GetAwaiter().GetResult();
        if (orLoad.IsSuccess)
        {
            orchestration = orLoad.Value!.Machine;
            refs["orchestration_reasoning_asset_source"] = "user_adaptive_cache";
            refs["orchestration_reasoning_user_asset_loaded_first"] = "true";
            refs["orchestration_reasoning_user_manifest_ref"] = RedactPath(orManifest);
            refs["orchestration_reasoning_user_checkpoint_ref"] = RedactPath(orCheckpoint);
        }
        else
        {
            refs["orchestration_reasoning_user_asset_loaded_first"] = "false";
            refs["orchestration_reasoning_user_load_error"] = orLoad.ErrorCode;
            refs["orchestration_reasoning_asset_source"] = refs.TryGetValue("orchestration_reasoning_asset_loaded", out var loaded) && loaded == "true"
                ? "repo_common_fallback"
                : "none";
        }

        var skillManifest = Path.Combine(userRoot, "skill-selection", "skill-selection-v1", "manifest.json");
        var skillCheckpoint = Path.Combine(userRoot, "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var skillLoad = LoadSkillSelectionUserCheckpoint(skillCheckpoint, skillManifest);
        if (skillLoad.IsSuccess)
        {
            skillSelection = skillLoad.Value!.Machine;
            refs["skill_selection_asset_source"] = "user_adaptive_cache";
            refs["skill_selection_user_asset_loaded_first"] = "true";
            refs["skill_selection_user_manifest_ref"] = RedactPath(skillManifest);
            refs["skill_selection_user_checkpoint_ref"] = RedactPath(skillCheckpoint);
        }
        else
        {
            refs["skill_selection_user_asset_loaded_first"] = "false";
            refs["skill_selection_user_load_error"] = skillLoad.ErrorCode;
            refs["skill_selection_asset_source"] = refs.TryGetValue("skill_selection_asset_loaded", out var loaded) && loaded == "true"
                ? "repo_common_fallback"
                : "none";
        }

        refs["user_adaptive_any_asset_loaded"] = (
            refs["prompt_to_task_user_asset_loaded_first"] == "true"
            || refs["orchestration_reasoning_user_asset_loaded_first"] == "true"
            || refs["skill_selection_user_asset_loaded_first"] == "true").ToString().ToLowerInvariant();
        refs["user_adaptive_load_order"] = "user_cache_first_repo_common_fallback_second";
        refs["all_common_assets_loaded"] = common.AllLoaded.ToString().ToLowerInvariant();

        return new CommonAssetLoadState(
            promptToTask,
            orchestration,
            skillSelection,
            common.CommonSkillSelectionMachine,
            refs,
            warnings,
            common.AllLoaded);
    }

    private static string FindRepoRoot()
    {
        var candidates = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };
        foreach (var candidate in candidates)
        {
            var current = new DirectoryInfo(candidate);
            while (current is not null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "43.SmartSkillsDomain"))
                    && Directory.Exists(Path.Combine(current.FullName, "models")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }

    private static string RedactPath(string path)
    {
        var full = Path.GetFullPath(path);
        var root = FindRepoRoot();
        var relative = Path.GetRelativePath(root, full);
        return relative.StartsWith("..", StringComparison.Ordinal)
            ? Path.GetFileName(full)
            : relative.Replace(Path.DirectorySeparatorChar, '/');
    }

    private static string TryReadSkillSelectionCheckpointSha(string manifestPath)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath, Encoding.UTF8));
            return document.RootElement.TryGetProperty("checkpoint_sha256", out var hash)
                ? hash.GetString() ?? string.Empty
                : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string? ResolveManifestCheckpointRef(string manifestPath)
    {
        try
        {
            if (!File.Exists(manifestPath))
                return null;

            using var document = JsonDocument.Parse(File.ReadAllText(manifestPath, Encoding.UTF8));
            if (!document.RootElement.TryGetProperty("checkpointRef", out var checkpointRef)
                && !document.RootElement.TryGetProperty("checkpoint_filename", out checkpointRef)
                && !document.RootElement.TryGetProperty("checkpointFilename", out checkpointRef))
            {
                return null;
            }

            var reference = checkpointRef.GetString();
            if (string.IsNullOrWhiteSpace(reference))
                return null;

            if (Path.IsPathRooted(reference))
                return Path.GetFullPath(reference);

            var manifestDirectory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
            return Path.GetFullPath(Path.Combine(manifestDirectory, reference));
        }
        catch
        {
            return null;
        }
    }

    private static OperationResult<SkillSelectionMachineLoadResult> LoadSkillSelectionCommonCheckpoint(string checkpointPath)
    {
        try
        {
            if (!File.Exists(checkpointPath))
            {
                return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                    "Checkpoint file not found.",
                    "CHECKPOINT_NOT_FOUND");
            }

            var load = SkillSelectionMachine.LoadCheckpointAsync(checkpointPath).GetAwaiter().GetResult();
            return load.IsSuccess || !string.Equals(load.ErrorCode, "CHECKPOINT_SCHEMA_UNSUPPORTED", StringComparison.Ordinal)
                ? load
                : LoadSnakeCaseSkillSelectionCheckpoint(checkpointPath);
        }
        catch (Exception ex)
        {
            return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                ex.Message,
                "SKILL_SELECTION_COMMON_CHECKPOINT_READ_FAILED");
        }
    }

    private static OperationResult<SkillSelectionMachineLoadResult> LoadSnakeCaseSkillSelectionCheckpoint(
        string checkpointPath)
    {
        try
        {
            var json = File.ReadAllText(checkpointPath, Encoding.UTF8);
            var compatibleJson = ConvertSnakeCaseCheckpointJsonToCamelCase(json);
            var checkpoint = JsonSerializer.Deserialize<SkillSelectionCheckpoint>(compatibleJson, JsonOptions);
            var validation = ValidateSkillSelectionCheckpointForApi(checkpoint);
            if (!validation.IsSuccess)
            {
                return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                    validation.ErrorMessage,
                    validation.ErrorCode);
            }

            var machine = new SkillSelectionMachine();
            machine.LoadCheckpoint(checkpoint!);
            return OperationResult<SkillSelectionMachineLoadResult>.Ok(new SkillSelectionMachineLoadResult(
                machine,
                checkpoint!,
                Path.GetFullPath(checkpointPath)));
        }
        catch (Exception ex)
        {
            return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                ex.Message,
                "SKILL_SELECTION_SNAKE_CASE_CHECKPOINT_READ_FAILED");
        }
    }

    private static string ConvertSnakeCaseCheckpointJsonToCamelCase(string json)
    {
        using var document = JsonDocument.Parse(json);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteCamelCaseCheckpointJson(document.RootElement, writer, preserveObjectPropertyNames: false);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteCamelCaseCheckpointJson(
        JsonElement element,
        Utf8JsonWriter writer,
        bool preserveObjectPropertyNames)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(preserveObjectPropertyNames
                        ? property.Name
                        : ToCamelCaseJsonPropertyName(property.Name));
                    WriteCamelCaseCheckpointJson(
                        property.Value,
                        writer,
                        preserveObjectPropertyNames: IsSkillSelectionCheckpointDictionaryProperty(property.Name));
                }

                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCamelCaseCheckpointJson(item, writer, preserveObjectPropertyNames: false);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }

    private static bool IsSkillSelectionCheckpointDictionaryProperty(string propertyName)
    {
        var normalized = propertyName.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        return normalized == "learnedsignals"
            || normalized == "candidatepositionweights"
            || normalized.EndsWith("weights", StringComparison.Ordinal);
    }

    private static string ToCamelCaseJsonPropertyName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return propertyName;

        var parts = propertyName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return char.ToLowerInvariant(propertyName[0]) + propertyName[1..];

        var builder = new StringBuilder(parts[0].ToLowerInvariant());
        foreach (var part in parts.Skip(1))
        {
            builder.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
                builder.Append(part[1..].ToLowerInvariant());
        }

        return builder.ToString();
    }

    private static OperationResult<bool> ValidateSkillSelectionCheckpointForApi(
        SkillSelectionCheckpoint? checkpoint)
    {
        if (checkpoint is null)
            return OperationResult<bool>.Fail("Checkpoint deserialized null.", "CHECKPOINT_DESERIALIZE_FAILED");
        if (checkpoint.SchemaVersion != 1)
            return OperationResult<bool>.Fail("Checkpoint schema is unsupported.", "CHECKPOINT_SCHEMA_UNSUPPORTED");
        if (!string.Equals(checkpoint.RouteMode, SkillSelectionRouteMode, StringComparison.Ordinal))
            return OperationResult<bool>.Fail("Checkpoint route mode is unsupported.", "CHECKPOINT_ROUTE_MODE_UNSUPPORTED");
        if (checkpoint.CandidatePositionWeights is null
            || checkpoint.TrainedPairIds is null
            || checkpoint.TrainedPromptKeys is null)
        {
            return OperationResult<bool>.Fail("Checkpoint is missing trained state sections.", "CHECKPOINT_STATE_SECTIONS_REQUIRED");
        }

        var unitValidations = new[]
        {
            ValidateSkillSelectionApiUnit(checkpoint.PromptUnderstanding, SkillSelectionMachine.PromptUnderstandingUnitId, "Prompt understanding"),
            ValidateSkillSelectionApiUnit(checkpoint.TaskUnderstanding, SkillSelectionMachine.TaskUnderstandingUnitId, "Task understanding"),
            ValidateSkillSelectionApiUnit(checkpoint.TaskType, SkillSelectionMachine.TaskTypeUnitId, "Task type"),
            ValidateSkillSelectionApiUnit(checkpoint.Routing, SkillSelectionMachine.RoutingUnitId, "Routing"),
            ValidateSkillSelectionApiUnit(checkpoint.SkillCandidates, SkillSelectionMachine.SkillCandidatesUnitId, "Skill candidates"),
            ValidateSkillSelectionApiUnit(checkpoint.SkillSelection, SkillSelectionMachine.SkillSelectionUnitId, "Skill selection"),
            ValidateSkillSelectionApiUnit(checkpoint.PromptContext, SkillSelectionMachine.PromptContextUnitId, "Prompt context"),
            ValidateSkillSelectionApiUnit(checkpoint.ResultGap, SkillSelectionMachine.ResultGapUnitId, "Result gap"),
            ValidateSkillSelectionApiUnit(checkpoint.OutputValidation, SkillSelectionMachine.OutputValidationUnitId, "Output validation"),
            ValidateSkillSelectionApiUnit(checkpoint.TaskCompletion, SkillSelectionMachine.TaskCompletionUnitId, "Task completion"),
            ValidateSkillSelectionApiUnit(checkpoint.IntermediateChecks, SkillSelectionMachine.IntermediateChecksUnitId, "Intermediate checks")
        };

        return unitValidations.FirstOrDefault(validation => !validation.IsSuccess) ?? OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateSkillSelectionApiUnit<T>(
        T? state,
        string expectedUnitId,
        string displayName)
        where T : class
    {
        if (state is null)
            return OperationResult<bool>.Fail($"{displayName} unit is missing.", "CHECKPOINT_STATE_SECTIONS_REQUIRED");

        var unitIdProperty = typeof(T).GetProperty("UnitId");
        var schemaVersionProperty = typeof(T).GetProperty("SchemaVersion");
        var skillProfilesProperty = typeof(T).GetProperty("SkillProfiles");
        var trainingPairIdsProperty = typeof(T).GetProperty("TrainingPairIds");
        var semanticQualityClaimedProperty = typeof(T).GetProperty("SemanticQualityClaimed");
        if (unitIdProperty is null
            || schemaVersionProperty is null
            || skillProfilesProperty is null
            || trainingPairIdsProperty is null
            || semanticQualityClaimedProperty is null)
        {
            return OperationResult<bool>.Fail($"{displayName} unit shape is unsupported.", "CHECKPOINT_STATE_SECTIONS_REQUIRED");
        }

        if (!string.Equals(unitIdProperty.GetValue(state) as string, expectedUnitId, StringComparison.Ordinal))
            return OperationResult<bool>.Fail($"{displayName} unit id is unsupported.", $"{ToErrorPrefix(displayName)}_UNIT_UNSUPPORTED");
        if ((int)(schemaVersionProperty.GetValue(state) ?? 0) != 1)
            return OperationResult<bool>.Fail($"{displayName} unit schema is unsupported.", $"{ToErrorPrefix(displayName)}_SCHEMA_UNSUPPORTED");
        if ((bool)(semanticQualityClaimedProperty.GetValue(state) ?? false))
            return OperationResult<bool>.Fail($"{displayName} unit must not claim full semantic quality.", $"{ToErrorPrefix(displayName)}_QUALITY_CLAIM_UNSUPPORTED");
        if (skillProfilesProperty.GetValue(state) is null || trainingPairIdsProperty.GetValue(state) is null)
            return OperationResult<bool>.Fail($"{displayName} unit is missing trained state sections.", "CHECKPOINT_STATE_SECTIONS_REQUIRED");

        return OperationResult<bool>.Ok(true);
    }

    private static string ToErrorPrefix(string displayName)
        => displayName.Replace(" ", "_", StringComparison.Ordinal).ToUpperInvariant();

    private static OperationResult<SkillSelectionMachineLoadResult> LoadSkillSelectionUserCheckpoint(
        string checkpointPath,
        string manifestPath)
    {
        try
        {
            if (!File.Exists(checkpointPath))
            {
                return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                    "Checkpoint file not found.",
                    "CHECKPOINT_NOT_FOUND");
            }

            if (!File.Exists(manifestPath))
            {
                return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                    "Manifest file not found.",
                    "MANIFEST_NOT_FOUND");
            }

            var machine = new SkillSelectionMachine();
            var import = machine.ImportAsync(new LearningMachineExport(
                SkillSelectionMachine.MachineId,
                SkillSelectionMachine.DomainTag,
                SkillSelectionMachine.Version,
                Path.GetFullPath(checkpointPath),
                Path.GetFullPath(manifestPath),
                0.0,
                DateTimeOffset.UtcNow)).GetAwaiter().GetResult();
            if (!import.IsSuccess)
            {
                return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                    import.ErrorMessage,
                    import.ErrorCode);
            }

            var checkpointLoad = SkillSelectionMachine.LoadCheckpointAsync(checkpointPath).GetAwaiter().GetResult();
            if (!checkpointLoad.IsSuccess)
            {
                return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                    checkpointLoad.ErrorMessage,
                    checkpointLoad.ErrorCode);
            }

            return OperationResult<SkillSelectionMachineLoadResult>.Ok(new SkillSelectionMachineLoadResult(
                machine,
                checkpointLoad.Value!.Checkpoint,
                Path.GetFullPath(checkpointPath)));
        }
        catch (Exception ex)
        {
            return OperationResult<SkillSelectionMachineLoadResult>.Fail(
                ex.Message,
                "SKILL_SELECTION_USER_CHECKPOINT_READ_FAILED");
        }
    }

    private sealed record CommonAssetLoadState(
        PromptToTaskMachine PromptToTaskMachine,
        OrchestrationReasoningMachine OrchestrationReasoningMachine,
        SkillSelectionMachine SkillSelectionMachine,
        SkillSelectionMachine CommonSkillSelectionMachine,
        IReadOnlyDictionary<string, string> CheckSetRefs,
        IReadOnlyList<string> Warnings,
        bool AllLoaded)
    {
        public CommonAssetLoadState(
            PromptToTaskMachine promptToTaskMachine,
            OrchestrationReasoningMachine orchestrationReasoningMachine,
            SkillSelectionMachine skillSelectionMachine,
            IReadOnlyDictionary<string, string> checkSetRefs,
            IReadOnlyList<string> warnings,
            bool allLoaded)
            : this(
                promptToTaskMachine,
                orchestrationReasoningMachine,
                skillSelectionMachine,
                skillSelectionMachine,
                checkSetRefs,
                warnings,
                allLoaded)
        {
        }

        public static CommonAssetLoadState Injected(
            PromptToTaskMachine? promptToTaskMachine = null,
            OrchestrationReasoningMachine? orchestrationReasoningMachine = null,
            SkillSelectionMachine? skillSelectionMachine = null)
        {
            var injectedPromptToTask = promptToTaskMachine ?? new PromptToTaskMachine();
            var injectedOrchestration = orchestrationReasoningMachine ?? new OrchestrationReasoningMachine();
            var injectedSkillSelection = skillSelectionMachine ?? new SkillSelectionMachine();
            return new CommonAssetLoadState(
                injectedPromptToTask,
                injectedOrchestration,
                injectedSkillSelection,
                injectedSkillSelection,
                new Dictionary<string, string>
                {
                    ["trained_model_gate_ref"] = "injected_machines_no_default_common_asset_loader",
                    ["all_common_assets_loaded"] = "false"
                },
                [],
                false);
        }
    }
}
