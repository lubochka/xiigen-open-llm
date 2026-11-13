// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentInfrastructure;
using SmartSkillsDomain;

public sealed class OrchestrationReasoningMachineSkeletonTests
{
    [Fact]
    public void OrchestrationReasoningMachineSkeleton_ConstructAndCallReturnsNotTrainedResult()
    {
        var machine = new OrchestrationReasoningMachine();
        var request = CreateRequest();

        var result = machine.Plan(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(OrchestrationReasoningMachine.NotTrainedStatus, result.Value!.ContractStatus);
        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, result.Value.ScenarioPlan.ScenarioId);
        Assert.Contains("NO_LEARNED_GUIDE_STATE_LOADED", result.Value.ContractEvidence);
        Assert.Contains("NO_ORCHESTRATION_PLAN_GENERATED", result.Value.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_PreservesStructuredTaskIdentityAndProvenance()
    {
        var machine = new OrchestrationReasoningMachine();
        var request = CreateRequest();

        var response = machine.Plan(request).Value!;

        Assert.Equal(request, response.Request);
        Assert.Equal(request.Task, response.ScenarioPlan.Task);
        Assert.Equal(request.Task.PromptId, response.ScenarioPlan.Task.PromptId);
        Assert.Equal(request.Task.RawPromptSha256, response.ScenarioPlan.Task.RawPromptSha256);
        Assert.Equal(request.Task.SplitId, response.ScenarioPlan.Task.SplitId);
        Assert.Equal("sonnet-local-analyzes", response.ScenarioPlan.Task.SourceProvenance["source_root_id"]);
        Assert.Equal("WO-ORCH-IMPL-01", response.ScenarioPlan.Task.SourceProvenance["work_order"]);
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_DoesNotGenerateScenarioSemantics()
    {
        var machine = new OrchestrationReasoningMachine();
        var response = machine.Plan(CreateNonMatchingRequest()).Value!;

        Assert.Empty(response.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Empty(response.ScenarioPlan.PhaseFamilies);
        Assert.Empty(response.ScenarioPlan.GateFamilies);
        Assert.Empty(response.ScenarioPlan.AssetFamilies);
        Assert.Empty(response.ScenarioPlan.HostCheckSets);
        Assert.Contains("NO_ORCHESTRATION_PLAN_GENERATED", response.ScenarioPlan.RiskFlags);
        Assert.Single(response.ScenarioPlan.NextGranularizationActions);
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_DoesNotReintroduceRawPromptText()
    {
        const string rawPrompt = "Secret raw prompt text must stay out of orchestration reasoning.";
        var response = new OrchestrationReasoningMachine()
            .Plan(CreateRequest(rawPrompt))
            .Value!;
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var requestPropertyNames = typeof(OrchestrationReasoningRequest)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("RawPrompt", requestPropertyNames);
        Assert.DoesNotContain("PromptText", requestPropertyNames);
        Assert.DoesNotContain(rawPrompt, json);
        Assert.Contains("sha256:orch-impl-001", json);
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_UsesOperationResultEnvelope()
    {
        var method = typeof(OrchestrationReasoningMachine).GetMethod(nameof(OrchestrationReasoningMachine.Plan));

        Assert.NotNull(method);
        Assert.Equal(typeof(OperationResult<OrchestrationReasoningResponse>), method!.ReturnType);
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_HasNoDeterministicSelectorRankerTemplateOrMaterializedState()
    {
        var machineType = typeof(OrchestrationReasoningMachine);
        var fieldTypes = machineType
            .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            .Select(field => field.FieldType)
            .ToArray();
        var memberNames = machineType
            .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(member => member.Name)
            .ToArray();

        Assert.DoesNotContain(typeof(Regex), fieldTypes);
        Assert.DoesNotContain(fieldTypes, type => type.Name.Contains("Dictionary", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Selector", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Ranker", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Template", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("FlowIndex", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("PromptFlowIndex", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Trainer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(memberNames, name => name.Contains("Asset", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_LoadedLearnedPatternProducesScenarioPlan()
    {
        var pattern = CreatePattern();
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([pattern])));

        var response = machine.Plan(CreateLearnedRequest("sha256:machine-private-one")).Value!;

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
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_LearnedStateWithoutSignalMatchProducesNoScenario()
    {
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));

        var response = machine.Plan(CreateNonMatchingRequest()).Value!;

        Assert.Equal(OrchestrationReasoningMachine.NoMatchingPatternStatus, response.ContractStatus);
        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, response.ScenarioPlan.ScenarioId);
        Assert.Empty(response.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Empty(response.ScenarioPlan.PhaseFamilies);
        Assert.Contains("NO_MATCHING_LEARNED_GUIDE_PATTERN", response.ScenarioPlan.RiskFlags);
        Assert.Contains("NO_ORCHESTRATION_PLAN_GENERATED", response.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_PartialSignalOverlapDoesNotSelectLearnedPattern()
    {
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));

        var response = machine.Plan(CreatePartialOverlapRequest()).Value!;

        Assert.Equal(OrchestrationReasoningMachine.NoMatchingPatternStatus, response.ContractStatus);
        Assert.Equal(OrchestrationReasoningMachine.NoInferenceScenarioId, response.ScenarioPlan.ScenarioId);
        Assert.Empty(response.ScenarioPlan.SelectedLearnedPatterns);
        Assert.DoesNotContain("LEARNED_GUIDE_PATTERN_MATCHED", response.ContractEvidence);
        Assert.Contains("NO_ORCHESTRATION_PLAN_GENERATED", response.ContractEvidence);
    }

    [Fact]
    public void OrchestrationReasoningMachineSkeleton_DifferentRawPromptHashesWithSameSignalsUseSameLearnedPattern()
    {
        var machine = new OrchestrationReasoningMachine(new StaticOrchestrationReasoningGuideStateLoader(CreateDescriptor([CreatePattern()])));
        var first = machine.Plan(CreateLearnedRequest("sha256:machine-private-one")).Value!;
        var second = machine.Plan(CreateLearnedRequest("sha256:machine-private-two")).Value!;

        Assert.NotEqual(first.Request.Task.RawPromptSha256, second.Request.Task.RawPromptSha256);
        Assert.Equal(first.ScenarioPlan.ScenarioId, second.ScenarioPlan.ScenarioId);
        Assert.Equal(first.ScenarioPlan.SelectedLearnedPatterns, second.ScenarioPlan.SelectedLearnedPatterns);
        Assert.Equal(first.ScenarioPlan.NextGranularizationActions, second.ScenarioPlan.NextGranularizationActions);
    }

    private static OrchestrationReasoningRequest CreateRequest(string rawPrompt = "not passed to orchestration")
        => new(
            RequestId: "orch-request-impl-001",
            Task: CreateStructuredTask(rawPrompt),
            GuideDerivedStateRefs: [],
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

    private static StructuredTaskContract CreateStructuredTask(string rawPrompt)
        => new(
            PromptId: "prompt-orch-impl-001",
            RawPromptSha256: "sha256:orch-impl-001",
            UserLanguage: "en",
            TaskType: "bounded_work_order",
            ExplicitUserRequirements: ["add orchestration skeleton", "do not add selector logic"],
            Constraints: ["no broad tests", "no host repos", "no training"],
            Deliverables: ["callable skeleton test"],
            ForbiddenActions: ["FlowIndex", "PromptFlowIndex", "template plan generation"],
            RequiredHosts: ["claude", "cursor", "codex"],
            EvidenceNeeds: ["filtered OrchestrationReasoningMachineSkeleton tests"],
            SourceProvenance: new Dictionary<string, string>
            {
                ["source_root_id"] = "sonnet-local-analyzes",
                ["work_order"] = "WO-ORCH-IMPL-01",
                ["raw_prompt_local_fixture_sha256"] = $"sha256:{rawPrompt.Length}"
            },
            SplitId: "split-orch-impl-01");

    private static OrchestrationReasoningRequest CreateLearnedRequest(string rawPromptSha256)
        => new(
            RequestId: $"orch-machine-request-{rawPromptSha256}",
            Task: CreateStructuredTask("not passed to orchestration") with
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
            RequestId: "orch-machine-nomatch",
            Task: CreateStructuredTask("not passed to orchestration") with
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

    private static OrchestrationReasoningRequest CreatePartialOverlapRequest()
        => new(
            RequestId: "orch-machine-partial-overlap",
            Task: CreateStructuredTask("not passed to orchestration") with
            {
                TaskType = "bounded_implementation_slice",
                ExplicitUserRequirements = ["parent review required"],
                Constraints = ["no host repos"],
                Deliverables = ["short answer"],
                EvidenceNeeds = ["source citation"],
                RequiredHosts = []
            },
            GuideDerivedStateRefs: ["guide-state:orchestration-reasoning:common:v1"],
            CorpusBundleIds: ["corpus:orchestration-reasoning:common"],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>());

    private static OrchestrationReasoningLearnedGuideStateDescriptor CreateDescriptor(
        IReadOnlyList<OrchestrationReasoningLearnedPattern>? learnedPatterns = null)
        => new(
            StateId: "orchestration-reasoning-state:v1:machine-test",
            GuideStateRef: "guide-state:orchestration-reasoning:common:v1",
            ManifestId: "models/orchestration-reasoning/orchestration-reasoning-v1/manifest",
            ManifestSha256: "sha256:orch-machine-manifest-test",
            MachineType: "OrchestrationReasoningMachine",
            SchemaVersion: "orchestration-reasoning-state-v1",
            SourceBaseCommit: "commit:orch-machine-test",
            StateSha256: "sha256:orch-machine-state-test",
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

    private sealed class StaticOrchestrationReasoningGuideStateLoader(
        OrchestrationReasoningLearnedGuideStateDescriptor descriptor)
        : IOrchestrationReasoningLearnedGuideStateLoader
    {
        public OperationResult<OrchestrationReasoningAdmittedLearnedState> Load()
            => OperationResult<OrchestrationReasoningAdmittedLearnedState>.Ok(
                OrchestrationReasoningSourceAdmissionTestFixture.Admit(descriptor, descriptor.StateId));
    }
}
