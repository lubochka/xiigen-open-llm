// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningStateLoadingTests
{
    [Fact]
    public void OrchestrationReasoningStateLoading_UnloadedStateReturnsNotTrainedResult()
    {
        var machine = new OrchestrationReasoningMachine();

        var result = machine.Plan(CreateRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, result.Value!.ContractStatus);
        Assert.Contains("NO_LEARNED_GUIDE_STATE_LOADED", result.Value.ContractEvidence);
        Assert.Contains("NO_LEARNED_GUIDE_STATE_LOADED", result.Value.ScenarioPlan.RiskFlags);
        Assert.Empty(result.Value.ScenarioPlan.SelectedLearnedPatterns);
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_LoadedDescriptorIsDetected()
    {
        var descriptor = CreateDescriptor();
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(descriptor));

        var result = machine.Plan(CreateRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal(OrchestrationReasoningMachine.LoadedGuideStateReadyStatus, result.Value!.ContractStatus);
        Assert.Contains("GUIDE_STATE_DESCRIPTOR_LOADED", result.Value.ContractEvidence);
        Assert.Contains("LEARNED_GUIDE_STATE_DESCRIPTOR_LOADED", result.Value.ScenarioPlan.RiskFlags);
        Assert.Contains("NO_LEARNED_GUIDE_PATTERNS_AVAILABLE", result.Value.ScenarioPlan.RiskFlags);
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_PreservesManifestAndStateIdentity()
    {
        var descriptor = CreateDescriptor();
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(descriptor));

        var response = machine.Plan(CreateNonMatchingRequest()).Value!;

        Assert.Contains($"guide_state_id:{descriptor.StateId}", response.ContractEvidence);
        Assert.Contains($"guide_state_ref:{descriptor.GuideStateRef}", response.ContractEvidence);
        Assert.Contains($"manifest_id:{descriptor.ManifestId}", response.ContractEvidence);
        Assert.Contains($"manifest_sha256:{descriptor.ManifestSha256}", response.ContractEvidence);
        Assert.Contains($"machine_type:{descriptor.MachineType}", response.ContractEvidence);
        Assert.Contains($"schema_version:{descriptor.SchemaVersion}", response.ContractEvidence);
        Assert.Contains($"source_base_commit:{descriptor.SourceBaseCommit}", response.ContractEvidence);
        Assert.Contains($"state_sha256:{descriptor.StateSha256}", response.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_LoadedDescriptorStillProducesNoScenarioSemantics()
    {
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor()));

        var response = machine.Plan(CreateRequest()).Value!;

        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, response.ScenarioPlan.ScenarioId);
        Assert.Empty(response.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Empty(response.ScenarioPlan.PhaseFamilies);
        Assert.Empty(response.ScenarioPlan.GateFamilies);
        Assert.Empty(response.ScenarioPlan.AssetFamilies);
        Assert.Empty(response.ScenarioPlan.HostCheckSets);
        Assert.Contains("NO_ORCHESTRATION_PLAN_GENERATED", response.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_LoadedLearnedPatternProducesScenarioPlan()
    {
        var pattern = CreatePattern();
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([pattern])));

        var response = machine.Plan(CreateLearnedRequest("sha256:private-one")).Value!;

        Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, response.ContractStatus);
        Assert.Equal("scenario:bounded-implementation-review-loop", response.ScenarioPlan.ScenarioId);
        Assert.Equal([pattern.PatternId], response.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Contains("implementation", response.ScenarioPlan.PhaseFamilies);
        Assert.Contains("parent-review", response.ScenarioPlan.GateFamilies);
        Assert.Contains("llm_mvp_core", response.ScenarioPlan.AssetFamilies);
        Assert.Contains("codex", response.ScenarioPlan.HostCheckSets);
        Assert.Contains("no_hidden_determinism_review_required", response.ScenarioPlan.RiskFlags);
        Assert.Contains("write completion matrix before parent review", response.ScenarioPlan.NextGranularizationActions);
        Assert.Contains("LEARNED_GUIDE_PATTERN_MATCHED", response.ContractEvidence);
        Assert.Contains($"learned_pattern_id:{pattern.PatternId}", response.ContractEvidence);
        Assert.Contains("INFERENCE_SOURCE:LOADED_LEARNED_GUIDE_STATE", response.ContractEvidence);
        Assert.Contains("ORCHESTRATION_REASONING_SCENARIO_UNIT_USED", response.ContractEvidence);
        Assert.Contains("BASELINE_NOT_SEMANTIC_QUALITY", response.ContractEvidence);
        Assert.Contains("HOST_RUNTIME_CLAIMED_FALSE", response.ContractEvidence);
        Assert.Contains("SEMANTIC_QUALITY_CLAIMED_FALSE", response.ContractEvidence);
    }

    [Fact]
    public async Task OrchestrationReasoningStateLoading_CommonCheckpointProducesUniversalTaskFamilyPlans()
    {
        var checkpointPath = RepoPath(
            "models",
            "orchestration-reasoning",
            "orchestration-reasoning-v1",
            "orchestration-reasoning-v1-checkpoint.json");
        var manifestPath = RepoPath(
            "models",
            "orchestration-reasoning",
            "orchestration-reasoning-v1",
            "manifest.json");

        var load = await OrchestrationReasoningMachine.LoadCheckpointAsync(checkpointPath, manifestPath);

        Assert.True(load.IsSuccess, load.ErrorMessage);
        Assert.Equal(15, load.Value!.Checkpoint.LearnedState.LearnedPatterns!.Count);
        Assert.Equal(15, load.Value.Manifest.LearnedPatternCount);
        foreach (var taskFamily in CreateUniversalTaskFamilyCases())
        {
            var response = load.Value.Machine.Plan(CreateUniversalTaskFamilyRequest(taskFamily)).Value!;
            var plan = response.ScenarioPlan;
            var planText = string.Join(
                " ",
                plan.PhaseFamilies
                    .Concat(plan.GateFamilies)
                    .Concat(plan.AssetFamilies)
                    .Concat(plan.RiskFlags)
                    .Concat(plan.NextGranularizationActions));

            Assert.Equal(OrchestrationReasoningMachine.LearnedPatternStatus, response.ContractStatus);
            Assert.Equal(taskFamily.ExpectedPatternId, plan.SelectedLearnedPatterns.Single());
            Assert.Contains("universal-orchestration-plan", plan.ScenarioId, StringComparison.Ordinal);
            Assert.Contains("review", planText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("rollback", planText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("evidence", planText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("stop", planText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("LEARNED_GUIDE_PATTERN_MATCHED", response.ContractEvidence);
            Assert.Contains("INFERENCE_SOURCE:LOADED_LEARNED_GUIDE_STATE", response.ContractEvidence);
            Assert.DoesNotContain("llm_mvp_core", planText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("phase12", planText, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_TiedTopTwoMarginProducesNoScenario()
    {
        var firstPattern = CreatePattern();
        var secondPattern = CreatePattern() with
        {
            PatternId = "or-pattern-bounded-implementation-review-tie",
            ScenarioRef = "scenario:competing-bounded-implementation-review-loop"
        };
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([firstPattern, secondPattern])));

        var response = machine.Plan(CreateLearnedRequest("sha256:private-tied")).Value!;

        Assert.Equal(OrchestrationReasoningMachine.NoMatchingPatternStatus, response.ContractStatus);
        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, response.ScenarioPlan.ScenarioId);
        Assert.Empty(response.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Contains("ORCHESTRATION_REASONING_SCENARIO_UNIT_USED", response.ContractEvidence);
        Assert.Contains("ORCHESTRATION_REASONING_SCENARIO_UNIT_NOT_READY", response.ContractEvidence);
        Assert.Contains("BASELINE_NOT_SEMANTIC_QUALITY", response.ContractEvidence);
        Assert.DoesNotContain("LEARNED_GUIDE_PATTERN_MATCHED", response.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_LearnedStateWithoutSignalMatchProducesNoScenario()
    {
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));

        var response = machine.Plan(CreateRequest()).Value!;

        Assert.Equal(OrchestrationReasoningMachine.NoMatchingPatternStatus, response.ContractStatus);
        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, response.ScenarioPlan.ScenarioId);
        Assert.Empty(response.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Empty(response.ScenarioPlan.PhaseFamilies);
        Assert.Contains("NO_MATCHING_LEARNED_GUIDE_PATTERN", response.ScenarioPlan.RiskFlags);
        Assert.Contains("NO_ORCHESTRATION_PLAN_GENERATED", response.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_DifferentRawPromptHashesWithSameSignalsUseSameLearnedPattern()
    {
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));
        var first = machine.Plan(CreateLearnedRequest("sha256:private-one")).Value!;
        var second = machine.Plan(CreateLearnedRequest("sha256:private-two")).Value!;

        Assert.NotEqual(first.Request.Task.RawPromptSha256, second.Request.Task.RawPromptSha256);
        Assert.Equal(first.ScenarioPlan.ScenarioId, second.ScenarioPlan.ScenarioId);
        Assert.Equal(first.ScenarioPlan.SelectedLearnedPatterns, second.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Equal(first.ScenarioPlan.NextGranularizationActions, second.ScenarioPlan.NextGranularizationActions);
        Assert.Contains("ORCHESTRATION_REASONING_SCENARIO_UNIT_USED", first.ContractEvidence);
        Assert.Contains("ORCHESTRATION_REASONING_SCENARIO_UNIT_USED", second.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningStateLoading_AddsNoDecisionPathTrainerCheckpointOrMaterializedState()
    {
        var inspectedTypes = new[]
        {
            typeof(OrchestrationReasoningMachine),
            typeof(OrchestrationReasoningLearnedGuideStateDescriptor),
            typeof(IOrchestrationReasoningLearnedGuideStateLoader)
        };
        var memberNames = inspectedTypes
            .SelectMany(type => type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .Select(member => member.Name)
            .ToArray();
        var fieldTypes = typeof(OrchestrationReasoningMachine)
            .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Select(field => field.FieldType)
            .ToArray();

        Assert.DoesNotContain(typeof(Regex), fieldTypes);
        Assert.DoesNotContain(fieldTypes, type => type.Name.Contains("Dictionary", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Candidate", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Ranker", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Selector", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("ResultGap", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Completion", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("FlowIndex", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("PromptFlowIndex", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Template", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Lookup", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Trainer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Asset", StringComparison.OrdinalIgnoreCase));
    }

    private static OrchestrationReasoningLearnedGuideStateDescriptor CreateDescriptor(
        IReadOnlyList<OrchestrationReasoningLearnedPattern>? learnedPatterns = null)
        => new(
            StateId: "orchestration-reasoning-state:v1:test-descriptor",
            GuideStateRef: "guide-state:orchestration-reasoning:common:v1",
            ManifestId: "models/orchestration-reasoning/orchestration-reasoning-v1/manifest",
            ManifestSha256: "sha256:orch-manifest-test",
            MachineType: "OrchestrationReasoningMachine",
            SchemaVersion: "orchestration-reasoning-state-v1",
            SourceBaseCommit: "commit:orch-test",
            StateSha256: "sha256:orch-state-test",
            LearnedPatterns: learnedPatterns);

    private static OrchestrationReasoningLearnedPattern CreatePattern()
        => new(
            PatternId: "or-pattern-bounded-implementation-review",
            ScenarioRef: "scenario:bounded-implementation-review-loop",
            RequiredTaskTypes: ["bounded_implementation_slice"],
            RequiredConstraintSignals: ["no host repos", "no external AI"],
            RequiredRequirementSignals: ["parent review required"],
            RequiredDeliverableSignals: ["focused test evidence"],
            RequiredEvidenceSignals: ["ablation evidence"],
            PhaseFamilies: ["implementation", "review"],
            GateFamilies: ["parent-review", "completion-matrix", "no-hidden-determinism"],
            AssetFamilies: ["llm_mvp_core"],
            HostCheckSets: ["codex"],
            RiskFlags: ["no_hidden_determinism_review_required", "no_training_claim"],
            NextGranularizationActions: ["write completion matrix before parent review", "prepare commit boundary only after acceptance"],
            Confidence: 0.93);

    private static OrchestrationReasoningRequest CreateRequest()
        => new(
            RequestId: "orch-state-request-001",
            Task: CreateStructuredTask(),
            GuideDerivedStateRefs: ["guide-state:orchestration-reasoning:common:v1"],
            CorpusBundleIds: ["corpus:orchestration-reasoning:draft"],
            AdaptiveContextRefs: new Dictionary<string, string>
            {
                ["project_local_state"] = "adaptive:not-loaded"
            },
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["external_ai"] = "approval-required",
                ["deterministic_shortcut"] = "forbidden"
            });

    private static StructuredTaskContract CreateStructuredTask()
        => new(
            PromptId: "prompt-orch-state-001",
            RawPromptSha256: "sha256:orch-state-001",
            UserLanguage: "en",
            TaskType: "bounded_work_order",
            ExplicitUserRequirements: ["add guide-state boundary", "do not produce scenario semantics"],
            Constraints: ["no broad tests", "no host repos", "no training"],
            Deliverables: ["state loading test"],
            ForbiddenActions: ["FlowIndex", "PromptFlowIndex", "template plan generation"],
            RequiredHosts: ["claude", "cursor", "codex"],
            EvidenceNeeds: ["filtered OrchestrationReasoningStateLoading tests"],
            SourceProvenance: new Dictionary<string, string>
            {
                ["source_root_id"] = "sonnet-local-analyzes",
                ["work_order"] = "WO-ORCH-IMPL-02"
            },
            SplitId: "split-orch-impl-02");

    private static OrchestrationReasoningRequest CreateLearnedRequest(string rawPromptSha256)
        => new(
            RequestId: $"orch-state-request-{rawPromptSha256}",
            Task: CreateStructuredTask() with
            {
                RawPromptSha256 = rawPromptSha256,
                TaskType = "bounded_implementation_slice",
                ExplicitUserRequirements = ["parent review required"],
                Constraints = ["no host repos", "no external AI"],
                Deliverables = ["focused test evidence"],
                EvidenceNeeds = ["ablation evidence"]
            },
            GuideDerivedStateRefs: ["guide-state:orchestration-reasoning:common:v1"],
            CorpusBundleIds: ["corpus:orchestration-reasoning:common"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["deterministic_shortcut"] = "forbidden"
            });

    private static OrchestrationReasoningRequest CreateNonMatchingRequest()
        => new(
            RequestId: "orch-state-nomatch",
            Task: CreateStructuredTask() with
            {
                TaskType = "documentation_question",
                ExplicitUserRequirements = ["explain existing docs"],
                Constraints = ["read only"],
                Deliverables = ["short answer"],
                EvidenceNeeds = ["source citation"],
                RequiredHosts = []
            },
            GuideDerivedStateRefs: ["guide-state:orchestration-reasoning:common:v1"],
            CorpusBundleIds: ["corpus:orchestration-reasoning:common"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["mode"] = "read_only"
            });

    private static IReadOnlyList<UniversalTaskFamilyCase> CreateUniversalTaskFamilyCases()
        =>
        [
            new(
                Name: "approval-gated",
                TaskType: "plan_only_or_approval_gated_task",
                Requirements: ["preserve planning and approval boundaries before implementation"],
                Constraints:
                [
                    "common model only",
                    "no external AI",
                    "no raw private prompt text",
                    "semantic_quality_claimed=false"
                ],
                Deliverables: ["plan task contract", "approval gate"],
                EvidenceNeeds: ["held-out metrics", "ablation controls", "fresh-load probe", "continue-training probe"],
                ExpectedPatternId: "or-task-family-pattern:approval-gated-universal-plan"),
            new(
                Name: "structured-audit",
                TaskType: "structured_audit_task",
                Requirements: ["turn broad audit or roadmap language into concrete audit outputs and gates"],
                Constraints:
                [
                    "common model only",
                    "no external AI",
                    "no raw private prompt text",
                    "semantic_quality_claimed=false"
                ],
                Deliverables: ["audit outputs", "gap table", "verification gates"],
                EvidenceNeeds: ["held-out metrics", "ablation controls", "fresh-load probe", "continue-training probe"],
                ExpectedPatternId: "or-task-family-pattern:structured-audit-universal-plan"),
            new(
                Name: "documentation-validation",
                TaskType: "deliverable_task",
                Requirements: ["Validation and documentation clauses in the prompt are first-class task requirements."],
                Constraints:
                [
                    "common model only",
                    "no external AI",
                    "no raw private prompt text",
                    "semantic_quality_claimed=false"
                ],
                Deliverables: ["structured task contract"],
                EvidenceNeeds: ["held-out metrics", "ablation controls", "fresh-load probe", "continue-training probe"],
                ExpectedPatternId: "or-task-family-pattern:documentation-validation-universal-plan")
        ];

    private static OrchestrationReasoningRequest CreateUniversalTaskFamilyRequest(
        UniversalTaskFamilyCase taskFamily)
        => new(
            RequestId: $"universal-task-family-{taskFamily.Name}",
            Task: new StructuredTaskContract(
                PromptId: $"universal-task-family-{taskFamily.Name}",
                RawPromptSha256: $"sha256:universal-task-family-{taskFamily.Name}",
                UserLanguage: "en",
                TaskType: taskFamily.TaskType,
                ExplicitUserRequirements: taskFamily.Requirements,
                Constraints: taskFamily.Constraints,
                Deliverables: taskFamily.Deliverables,
                ForbiddenActions:
                [
                    "exact prompt lookup as final authority",
                    "raw private text copy",
                    "semantic quality claim"
                ],
                RequiredHosts: ["core-only"],
                EvidenceNeeds: taskFamily.EvidenceNeeds,
                SourceProvenance: new Dictionary<string, string>
                {
                    ["task_family_probe"] = taskFamily.Name,
                    ["raw_prompt_text"] = "not_stored"
                },
                SplitId: $"universal-task-family-{taskFamily.Name}"),
            GuideDerivedStateRefs: ["common:universal-task-family-plan-evidence"],
            CorpusBundleIds: ["common:orchestration-reasoning"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>
            {
                ["external_ai"] = "forbidden_without_explicit_approval",
                ["deterministic_shortcut"] = "forbidden",
                ["semantic_quality_claimed"] = "false",
                ["host_runtime_claimed"] = "false"
            });

    private sealed record UniversalTaskFamilyCase(
        string Name,
        string TaskType,
        IReadOnlyList<string> Requirements,
        IReadOnlyList<string> Constraints,
        IReadOnlyList<string> Deliverables,
        IReadOnlyList<string> EvidenceNeeds,
        string ExpectedPatternId);

    private sealed class StaticOrchestrationReasoningGuideStateLoader(
        OrchestrationReasoningLearnedGuideStateDescriptor descriptor)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load()
            => OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor>.Ok(descriptor);
    }

    private static string RepoPath(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
            directory = directory.Parent;

        Assert.NotNull(directory);
        return Path.Combine([directory!.FullName, .. parts]);
    }
}
