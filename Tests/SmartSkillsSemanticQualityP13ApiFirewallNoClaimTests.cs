// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.Json.Nodes;
using SmartSkillsDomain;

public sealed class SmartSkillsSemanticQualityP13ApiFirewallNoClaimTests
{
    private const string PositiveSemanticQualityEvidence = "SEMANTIC_QUALITY_CLAIMED_TRUE";
    private const string BaselineNoClaimEvidence = "BASELINE_NOT_SEMANTIC_QUALITY";
    private const string ExplicitNoClaimEvidence = "SEMANTIC_QUALITY_CLAIMED_FALSE";

    [Fact]
    public void P13_FirewallRequiresAllThreePositiveComponentsAndArtifactsRecordNoClaimBoundary()
    {
        var service = CreateLoadedApiService(
            new PromptToTaskMachine(),
            new OrchestrationReasoningMachine(),
            new SkillSelectionMachine());

        var promptBaseline = CreatePromptToTaskResponse(
            "ptt-p11-p12-lifecycle",
            [BaselineNoClaimEvidence, ExplicitNoClaimEvidence]);
        var orchestrationBaseline = CreateOrchestrationResponse(
            "or-p11-p12-lifecycle",
            promptBaseline.StructuredTask,
            [BaselineNoClaimEvidence, ExplicitNoClaimEvidence]);
        var missingSkillSelection = CreateSkillDecision([]);

        Assert.False(InvokeShouldClaimSemanticQuality(service, promptBaseline, orchestrationBaseline, missingSkillSelection));

        var promptPositive = CreatePromptToTaskResponse("ptt-positive-control", [PositiveSemanticQualityEvidence]);
        var orchestrationPositive = CreateOrchestrationResponse(
            "or-positive-control",
            promptPositive.StructuredTask,
            [PositiveSemanticQualityEvidence]);
        var skillPositive = CreateSkillDecision([PositiveSemanticQualityEvidence]);

        Assert.True(InvokeShouldClaimSemanticQuality(service, promptPositive, orchestrationPositive, skillPositive));
        Assert.False(InvokeShouldClaimSemanticQuality(service, promptPositive, orchestrationPositive, CreateSkillDecision([])));
        Assert.False(InvokeShouldClaimSemanticQuality(
            service,
            promptPositive,
            orchestrationPositive,
            CreateSkillDecision([PositiveSemanticQualityEvidence, SkillSelectionMachine.CandidatePositionScaffoldEvidence])));
        Assert.False(InvokeShouldClaimSemanticQuality(
            service,
            promptPositive,
            orchestrationPositive,
            CreateSkillDecision([PositiveSemanticQualityEvidence, ExplicitNoClaimEvidence])));
        Assert.False(InvokeShouldClaimSemanticQuality(
            service,
            CreatePromptToTaskResponse("ptt-no-claim-control", [PositiveSemanticQualityEvidence, ExplicitNoClaimEvidence]),
            orchestrationPositive,
            skillPositive));

        var report = ReadJson("artifacts", "smart-skills-semantic-quality-completion", "p13-api-firewall-no-claim", "smart-skills-p13-api-firewall-no-claim-report.json");
        Assert.Equal("PASS_CANDIDATE_READY_FOR_PARENT_REVIEW", (string?)report["verdict"]);
        Assert.Equal(false, (bool?)report["full_api_semantic_quality_claimed"]);
        Assert.Equal(false, (bool?)report["semantic_quality_claimed"]);
        Assert.Equal(false, (bool?)report["dashboard_qa_required"]);
        Assert.Contains("no API/result-display surface changed", (string?)report["dashboard_qa_reason"] ?? string.Empty, StringComparison.Ordinal);
        Assert.Contains("SkillSelection", (string?)report["blocked_reason"] ?? string.Empty, StringComparison.Ordinal);
        var focusedResult = (string?)report["focused_test"]?["result"] ?? string.Empty;
        Assert.DoesNotContain("pending", focusedResult, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Failed=0", focusedResult, StringComparison.Ordinal);
        Assert.Contains("Passed=1", focusedResult, StringComparison.Ordinal);

        var selfCheck = ReadJson("artifacts", "smart-skills-semantic-quality-completion", "p13-api-firewall-no-claim", "p13-self-check.json");
        Assert.Equal(false, (bool?)selfCheck["semantic_quality_claimed"]);
        Assert.Equal(false, (bool?)selfCheck["full_api_semantic_quality_claimed"]);
        Assert.Equal(false, (bool?)selfCheck["skill_selection_readiness_claimed"]);
        Assert.Equal(false, (bool?)selfCheck["extension_runtime_readiness_claimed"]);
        Assert.Equal(false, (bool?)selfCheck["final_model_ready_claimed"]);
    }

    private static PromptToTaskResponse CreatePromptToTaskResponse(string id, IReadOnlyList<string> evidence)
    {
        var request = new PromptToTaskRequest(
            PromptId: id,
            RawPrompt: string.Empty,
            RawPromptSha256: $"sha256:{id}",
            PromptProvenance: "p13-test-artificial-firewall-fixture",
            UserLanguage: "en",
            ExplicitConstraints: [],
            ContextFields: new Dictionary<string, string>());
        var task = new StructuredTaskContract(
            PromptId: id,
            RawPromptSha256: $"sha256:{id}",
            UserLanguage: "en",
            TaskType: "firewall_contract_probe",
            ExplicitUserRequirements: ["exercise api firewall only"],
            Constraints: ["no project readiness claim"],
            Deliverables: ["boolean firewall decision"],
            ForbiddenActions: ["claim full semantic quality"],
            RequiredHosts: ["test"],
            EvidenceNeeds: ["all component evidence checked"],
            SourceProvenance: new Dictionary<string, string> { ["scope"] = "p13-artificial-firewall-test" },
            SplitId: "p13-firewall-fixture");

        return new PromptToTaskResponse(
            ResponseId: id,
            Request: request,
            StructuredTask: task,
            ContractEvidence: evidence,
            ContractStatus: "p13_firewall_fixture");
    }

    private static OrchestrationReasoningResponse CreateOrchestrationResponse(
        string id,
        StructuredTaskContract task,
        IReadOnlyList<string> evidence)
    {
        var request = new OrchestrationReasoningRequest(
            RequestId: id,
            Task: task,
            GuideDerivedStateRefs: [],
            CorpusBundleIds: [],
            AdaptiveContextRefs: new Dictionary<string, string>(),
            RuntimePolicyRefs: new Dictionary<string, string>());
        var plan = new OrchestrationScenarioPlan(
            ScenarioId: id,
            Task: task,
            SelectedLearnedPatterns: [],
            PhaseFamilies: [],
            GateFamilies: [],
            AssetFamilies: [],
            HostCheckSets: [],
            RiskFlags: [],
            NextGranularizationActions: []);

        return new OrchestrationReasoningResponse(
            ResponseId: id,
            Request: request,
            ScenarioPlan: plan,
            ContractEvidence: evidence,
            ContractStatus: "p13_firewall_fixture");
    }

    private static SkillSelectionDecision CreateSkillDecision(IReadOnlyList<string> evidence)
        => new(
            SelectedSkillIds: ["api-firewall"],
            RejectedSkillIds: [],
            EvidenceFeatures: evidence,
            Confidence: 1.0,
            RequiresSecondPass: false,
            DecisionText: "P13 artificial firewall fixture.");

    private static XiigenExtensionApiService CreateLoadedApiService(
        PromptToTaskMachine promptToTaskMachine,
        OrchestrationReasoningMachine orchestrationReasoningMachine,
        SkillSelectionMachine skillSelectionMachine)
    {
        var serviceType = typeof(XiigenExtensionApiService);
        var stateType = serviceType.GetNestedType("CommonAssetLoadState", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CommonAssetLoadState type was not found.");
        var state = Activator.CreateInstance(
            stateType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args:
            [
                promptToTaskMachine,
                orchestrationReasoningMachine,
                skillSelectionMachine,
                new Dictionary<string, string>
                {
                    ["trained_model_gate_ref"] = "p13-test-loaded-common-assets",
                    ["all_common_assets_loaded"] = "true",
                    ["prompt_to_task_asset_loaded"] = "true",
                    ["orchestration_reasoning_asset_loaded"] = "true",
                    ["skill_selection_asset_loaded"] = "true"
                },
                Array.Empty<string>(),
                true
            ],
            culture: null)
            ?? throw new InvalidOperationException("CommonAssetLoadState could not be created.");
        var constructor = serviceType.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: [stateType],
            modifiers: null)
            ?? throw new InvalidOperationException("XiigenExtensionApiService state constructor was not found.");

        return (XiigenExtensionApiService)constructor.Invoke([state]);
    }

    private static bool InvokeShouldClaimSemanticQuality(
        XiigenExtensionApiService service,
        PromptToTaskResponse promptToTask,
        OrchestrationReasoningResponse orchestration,
        SkillSelectionDecision skillDecision)
    {
        var method = typeof(XiigenExtensionApiService).GetMethod(
            "ShouldClaimSemanticQuality",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("ShouldClaimSemanticQuality was not found.");

        return (bool)method.Invoke(service, [promptToTask, orchestration, skillDecision])!;
    }

    private static JsonNode ReadJson(params string[] parts)
    {
        var path = RepoPath(parts);
        var text = File.ReadAllText(path);
        return JsonNode.Parse(text) ?? throw new InvalidOperationException($"Could not parse {path}.");
    }

    private static string RepoPath(params string[] parts)
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "AGENTS.md"))
                && Directory.Exists(Path.Combine(current, "artifacts")))
            {
                return Path.Combine([current, .. parts]);
            }

            current = Directory.GetParent(current)?.FullName;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
