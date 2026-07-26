// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningMachineContinueTrainingTests
{
    [Fact]
    public void OrchestrationReasoningContinueTraining_MergePreservesExistingPatternAndAppendsDeltaPattern()
    {
        var existingPattern = Pattern("existing-pattern", "existing-scenario", "existing_constraint");
        var deltaPattern = Pattern("delta-pattern", "delta-scenario", "delta_constraint");
        var machine = MachineWithState(State("existing-state", existingPattern), ["accepted:existing"]);

        var result = machine.ContinueTrainingFromLearnedState(State("delta-state", deltaPattern), ["accepted:delta"]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.MergedLearnedState.LearnedPatterns!.Count);
        Assert.Contains(result.Value.MergedLearnedState.LearnedPatterns, pattern => pattern.PatternId == existingPattern.PatternId);
        Assert.Contains(result.Value.MergedLearnedState.LearnedPatterns, pattern => pattern.PatternId == deltaPattern.PatternId);
        Assert.Equal([deltaPattern.PatternId], result.Value.AddedPatternIds);
        Assert.Contains("accepted:existing", result.Value.MergedAcceptedRefIds);
        Assert.Contains("accepted:delta", result.Value.MergedAcceptedRefIds);
        Assert.NotEqual(State("existing-state", existingPattern).StateSha256, result.Value.MergedLearnedState.StateSha256);
        Assert.False(result.Value.ContinueTrainingRunClaimed);
        Assert.False(result.Value.CheckpointRewritten);
        Assert.False(result.Value.SemanticQualityClaimed);
        Assert.False(result.Value.HostRuntimeClaimed);
    }

    [Fact]
    public void OrchestrationReasoningContinueTraining_DuplicatePatternIdIsRejected()
    {
        var existingPattern = Pattern("same-pattern", "existing-scenario", "existing_constraint");
        var deltaPattern = Pattern("same-pattern", "delta-scenario", "delta_constraint");
        var machine = MachineWithState(State("existing-state", existingPattern), ["accepted:existing"]);

        var result = machine.ContinueTrainingFromLearnedState(State("delta-state", deltaPattern), ["accepted:delta"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_CONTINUE_DUPLICATE_PATTERN_ID", result.ErrorCode);
    }

    [Fact]
    public void OrchestrationReasoningContinueTraining_NoExistingLoadedStateIsRejected()
    {
        var machine = new OrchestrationReasoningMachine();

        var result = machine.ContinueTrainingFromLearnedState(State("delta-state", Pattern("delta-pattern", "delta-scenario", "delta_constraint")), ["accepted:delta"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_CONTINUE_IMPORTED_CHECKPOINT_STATE_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public void OrchestrationReasoningContinueTraining_LoaderOnlyStateIsRejectedToPreserveAcceptedRefs()
    {
        var existingPattern = Pattern("existing-pattern", "existing-scenario", "existing_constraint");
        var deltaPattern = Pattern("delta-pattern", "delta-scenario", "delta_constraint");
        var machine = new OrchestrationReasoningMachine(new StaticLoader(State("loader-state", existingPattern)));

        var result = machine.ContinueTrainingFromLearnedState(State("delta-state", deltaPattern), ["accepted:delta"]);

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATION_REASONING_CONTINUE_IMPORTED_CHECKPOINT_STATE_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public void OrchestrationReasoningContinueTraining_MergedMachinePlansForOldAndNewPatternsWithoutRawPromptMemory()
    {
        var existingPattern = Pattern("existing-pattern", "existing-scenario", "existing_constraint");
        var deltaPattern = Pattern("delta-pattern", "delta-scenario", "delta_constraint");
        var machine = MachineWithState(State("existing-state", existingPattern), ["accepted:existing"]);
        var merge = machine.ContinueTrainingFromLearnedState(State("delta-state", deltaPattern), ["accepted:delta"]);
        Assert.True(merge.IsSuccess, merge.ErrorMessage);

        var oldPlan = machine.Plan(RequestFor(existingPattern, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
        var newPlan = machine.Plan(RequestFor(deltaPattern, "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"));

        Assert.True(oldPlan.IsSuccess, oldPlan.ErrorMessage);
        Assert.True(newPlan.IsSuccess, newPlan.ErrorMessage);
        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, oldPlan.Value!.ContractStatus);
        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, newPlan.Value!.ContractStatus);
        Assert.Equal(existingPattern.ScenarioRef, oldPlan.Value.ScenarioPlan.ScenarioId);
        Assert.Equal(deltaPattern.ScenarioRef, newPlan.Value.ScenarioPlan.ScenarioId);
        Assert.Equal([existingPattern.PatternId], oldPlan.Value.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Equal([deltaPattern.PatternId], newPlan.Value.ScenarioPlan.SelectedLearnedPatterns);
    }

    private static OrchestrationReasoningMachine MachineWithState(
        OrchestrationReasoningLearnedGuideStateDescriptor state,
        IReadOnlyList<string> acceptedRefs)
    {
        var machine = new OrchestrationReasoningMachine();
        var checkpoint = new OrchestrationReasoningCheckpoint(
            SchemaVersion: 1,
            LearnedState: state,
            AcceptedRefIds: acceptedRefs,
            SourceManifestRef: "training-data/orchestration-reasoning/orchestration-reasoning-dpo-trainer-payloads-manifest.json",
            SemanticQualityClaimed: false,
            ContinueTrainingClaimed: false,
            HostRuntimeClaimed: false,
            SavedAtUtc: DateTimeOffset.UnixEpoch);
        var import = machine.ImportLearnedState(checkpoint);
        Assert.True(import.IsSuccess, import.ErrorMessage);
        return machine;
    }

    private static OrchestrationReasoningLearnedGuideStateDescriptor State(
        string stateId,
        OrchestrationReasoningLearnedPattern pattern)
        => new(
            StateId: stateId,
            GuideStateRef: $"guide-state:{stateId}",
            ManifestId: $"manifest:{stateId}",
            ManifestSha256: $"sha256:{stateId}",
            MachineType: nameof(OrchestrationReasoningMachine),
            SchemaVersion: "orchestration-reasoning-state-v1",
            SourceBaseCommit: $"source:{stateId}",
            StateSha256: $"state-sha:{stateId}",
            LearnedPatterns: [pattern]);

    private static OrchestrationReasoningLearnedPattern Pattern(
        string patternId,
        string scenarioRef,
        string constraintSignal)
        => new(
            PatternId: patternId,
            ScenarioRef: scenarioRef,
            RequiredTaskTypes: ["universal_reasoning"],
            RequiredConstraintSignals: [constraintSignal],
            RequiredRequirementSignals: [$"{constraintSignal}_requirement"],
            RequiredDeliverableSignals: [$"{constraintSignal}_deliverable"],
            RequiredEvidenceSignals: [$"{constraintSignal}_evidence"],
            PhaseFamilies: ["orchestration-reasoning"],
            GateFamilies: ["continue-training"],
            AssetFamilies: ["llm_mvp_core"],
            HostCheckSets: ["core-only"],
            RiskFlags: ["no_checkpoint_rewrite"],
            NextGranularizationActions: ["parent review before command slice"],
            Confidence: 0.8);

    private static OrchestrationReasoningRequest RequestFor(
        OrchestrationReasoningLearnedPattern pattern,
        string rawPromptSha256)
        => new(
            RequestId: $"or-continue-{pattern.PatternId}",
            Task: new StructuredTaskContract(
                PromptId: "or-continue-fixture",
                RawPromptSha256: rawPromptSha256,
                UserLanguage: "en",
                TaskType: pattern.RequiredTaskTypes[0],
                ExplicitUserRequirements: pattern.RequiredRequirementSignals,
                Constraints: pattern.RequiredConstraintSignals,
                Deliverables: pattern.RequiredDeliverableSignals,
                ForbiddenActions: ["raw_prompt_memory", "deterministic_route_lookup"],
                RequiredHosts: ["core-only"],
                EvidenceNeeds: pattern.RequiredEvidenceSignals,
                SourceProvenance: new Dictionary<string, string>
                {
                    ["source_ref"] = $"continue-fixture:{pattern.PatternId}",
                    ["raw_prompt_memory"] = "forbidden"
                },
                SplitId: "or-continue-split"),
            GuideDerivedStateRefs: [pattern.PatternId],
            CorpusBundleIds: ["corpus:orchestration-reasoning:continue-fixture"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private sealed class StaticLoader(OrchestrationReasoningLearnedGuideStateDescriptor descriptor)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load()
            => OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Ok(descriptor);
    }
}
