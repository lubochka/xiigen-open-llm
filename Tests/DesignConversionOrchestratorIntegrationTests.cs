// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;
using OrchestratorModel.Logical;
using System.Text.Json;

namespace LlmFromScratch.Tests;

public sealed class DesignConversionOrchestratorIntegrationTests
{
    private const string PromptSentinelHtml = "<article id=\"prompt-sentinel-html\">do-not-trust-prompt-html</article>";
    private const string PromptSentinelCss = ".prompt-sentinel{color:hotpink;}";
    private const string PromptSentinelTree = "{\"private\":\"prompt-tree-sentinel\"}";

    [Fact]
    public void AssistantResolvesDesignConversionRuntimeFlow()
    {
        var assistantResult = LogicalOrchestratorAssistant.CreateDefault(new DesignConversionAssistantNullSessionStorage());

        Assert.True(assistantResult.IsSuccess, assistantResult.ErrorMessage);
        var assistant = assistantResult.Value!;

        var typedFlow = assistant.ResolveRegisteredFlowByScenario<DesignConversionRuntimeFlowInput>(
            DesignConversionRoutingFlow.ScenarioId);
        var stringFlow = assistant.ResolveRegisteredFlowByScenario<string>(
            DesignConversionRoutingFlow.ScenarioId);

        Assert.True(stringFlow.IsSuccess, stringFlow.ErrorMessage);
        Assert.IsType<DesignConversionOrchestrationFlowAdapter>(stringFlow.Value);
        Assert.True(
            typedFlow.IsSuccess,
            $"Typed resolution failed while string flow success={stringFlow.IsSuccess}: {typedFlow.ErrorMessage}");
        Assert.IsType<DesignConversionRoutingFlow>(typedFlow.Value);

        var manuallyBuiltAssistant = new LogicalOrchestratorAssistant(
            new DesignConversionAssistantNullSessionStorage(),
            orchestrator: null);
        var unconfiguredFlow = manuallyBuiltAssistant.ResolveRegisteredFlowByScenario<DesignConversionRuntimeFlowInput>(
            DesignConversionRoutingFlow.ScenarioId);

        Assert.False(unconfiguredFlow.IsSuccess);
        Assert.Equal("FLOW_REGISTRY_NOT_CONFIGURED", unconfiguredFlow.ErrorCode);
    }

    [Fact]
    public async Task AssistantExplicitDesignConversionScenario_UsesTypedMetadataAndPreservesRuntimeEvidence()
    {
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new DesignConversionAssistantNullSessionStorage()).Value!;
        var typedInput = CreateInput("typed-positive");
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey] = typedInput
        };

        var result = await assistant.RunAsync(
            BuildPromptSentinel(),
            DesignConversionRoutingFlow.ScenarioId,
            new LogicalExecutionFlags(),
            metadata);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionRoutingFlow.ScenarioId, result.Value!.ScenarioId);
        Assert.Equal(DesignConversionRoutingFlow.FlowId, result.Value.SelectedFlowId);
        Assert.Equal(DesignConversionRuntimeScaffoldCriticUnit.UnitId, result.Value.Metrics["critic_unit_id"]);
        Assert.Equal(DesignConversionRuntimeScaffoldImproverUnit.UnitId, result.Value.Metrics["improver_unit_id"]);
        Assert.Equal(
            $"{DesignConversionRuntimeScaffoldCriticUnit.UnitId}>{DesignConversionRuntimeScaffoldImproverUnit.UnitId}",
            result.Value.Metrics["unit_order"]);
        Assert.Equal(bool.FalseString, result.Value.Metrics["deterministic_logic_claimed_as_learning"]);
        Assert.Equal("input_evidence_only", result.Value.Metrics["qa_data_role"]);
        Assert.Contains("DESIGN_CONVERSION_ROUTING_FLOW_CRITIC_THEN_IMPROVER", result.Value.EvidenceIds);
        Assert.Contains("DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE", result.Value.EvidenceIds);
        Assert.Contains("deterministic-input-evidence:fixture=orchestrator-integration", result.Value.EvidenceIds);
        Assert.StartsWith($"{DesignConversionRuntimeScaffoldImproverUnit.UnitId}:", result.Value.FinalHumanResult, StringComparison.Ordinal);
        AssertNoRawDesignPayloadInUserVisibleSurfaces(result.Value, typedInput);
    }

    [Fact]
    public async Task AssistantExplicitDesignConversionScenario_MissingTypedInputFailsWithoutDefaults()
    {
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new DesignConversionAssistantNullSessionStorage()).Value!;

        var result = await assistant.RunAsync(
            BuildPromptSentinel(),
            DesignConversionRoutingFlow.ScenarioId,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>(StringComparer.Ordinal));

        Assert.False(result.IsSuccess);
        Assert.Equal("ORCHESTRATOR_FAILED", result.ErrorCode);
        Assert.Contains(DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey, result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("DesignConversionRuntimeFlowInput", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DirectAdapter_WrongMetadataTypeReturnsStableErrorCode()
    {
        var adapter = new DesignConversionOrchestrationFlowAdapter();
        var request = new LogicalFlowRequest<string>(
            RunId: "adapter-wrong-type",
            FlowId: DesignConversionRoutingFlow.FlowId,
            ScenarioId: DesignConversionRoutingFlow.ScenarioId,
            Input: BuildPromptSentinel(),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey] = PromptSentinelHtml
            });

        var result = await adapter.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionOrchestrationFlowAdapter.InputWrongTypeErrorCode, result.ErrorCode);
        Assert.Contains(DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DirectAdapter_StructuredJsonInputReturnsStableWrongTypeErrorCode()
    {
        var adapter = new DesignConversionOrchestrationFlowAdapter();
        var structuredJson = JsonSerializer.SerializeToElement(CreateInput("json-object-wrong-type"));
        var request = new LogicalFlowRequest<string>(
            RunId: "adapter-json-wrong-type",
            FlowId: DesignConversionRoutingFlow.FlowId,
            ScenarioId: DesignConversionRoutingFlow.ScenarioId,
            Input: BuildPromptSentinel(),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey] = structuredJson
            });

        var result = await adapter.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionOrchestrationFlowAdapter.InputWrongTypeErrorCode, result.ErrorCode);
        Assert.Contains(DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DirectAdapter_ValidTypedAliasSucceeds()
    {
        var adapter = new DesignConversionOrchestrationFlowAdapter();
        var typedInput = CreateInput("typed-alias");
        var request = new LogicalFlowRequest<string>(
            RunId: "adapter-valid-alias",
            FlowId: DesignConversionRoutingFlow.FlowId,
            ScenarioId: DesignConversionRoutingFlow.ScenarioId,
            Input: BuildPromptSentinel(),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [DesignConversionOrchestrationFlowAdapter.DesignConversionInputMetadataAlias] = typedInput
            });

        var result = await adapter.ExecuteAsync(request);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionRoutingFlow.ScenarioId, result.Value!.ScenarioId);
        Assert.Equal(DesignConversionRoutingFlow.FlowId, result.Value.FlowId);
        Assert.StartsWith($"{typedInput.Request.RequestId}:", result.Value.Metrics["candidate_id"], StringComparison.Ordinal);
    }

    [Fact]
    public async Task DirectAdapter_CanonicalWrongTypeTakesPrecedenceOverValidTypedAlias()
    {
        var adapter = new DesignConversionOrchestrationFlowAdapter();
        var typedInput = CreateInput("typed-alias-behind-invalid-canonical");
        var request = new LogicalFlowRequest<string>(
            RunId: "adapter-canonical-precedence",
            FlowId: DesignConversionRoutingFlow.FlowId,
            ScenarioId: DesignConversionRoutingFlow.ScenarioId,
            Input: BuildPromptSentinel(),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey] = PromptSentinelHtml,
                [DesignConversionOrchestrationFlowAdapter.DesignConversionInputMetadataAlias] = typedInput
            });

        var result = await adapter.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal(DesignConversionOrchestrationFlowAdapter.InputWrongTypeErrorCode, result.ErrorCode);
        Assert.Contains(DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey, result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AssistantPromptSentinelDifferentFromTypedMetadata_IsNotDomainPayload()
    {
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new DesignConversionAssistantNullSessionStorage()).Value!;
        var typedInput = CreateInput("typed-sentinel-boundary");
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [DesignConversionOrchestrationFlowAdapter.RuntimeFlowInputMetadataKey] = typedInput
        };

        var result = await assistant.RunAsync(
            BuildPromptSentinel(),
            DesignConversionRoutingFlow.ScenarioId,
            new LogicalExecutionFlags(),
            metadata);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal($"{typedInput.Request.RequestId}:critic:input-evidence-aggregate", result.Value!.Metrics["critic_verdict_id"]);
        Assert.StartsWith(
            $"{typedInput.Request.RequestId}:",
            result.Value.Metrics["candidate_id"],
            StringComparison.Ordinal);
        Assert.DoesNotContain("prompt-sentinel", result.Value.Metrics["candidate_id"], StringComparison.Ordinal);
        Assert.DoesNotContain("prompt-sentinel", result.Value.Metrics["critic_verdict_id"], StringComparison.Ordinal);
        Assert.DoesNotContain(PromptSentinelTree, result.Value.FinalHumanResult, StringComparison.Ordinal);
    }

    private static DesignConversionRuntimeFlowInput CreateInput(string requestId)
    {
        var request = CreateRequest(requestId);
        var expected = CreateImage($"{requestId}-expected", 1440, 900);
        var actual = CreateImage($"{requestId}-actual", 1440, 900);
        var qaVerdict = CreateVerdict($"{requestId}:qa-verdict", score: 0.82, passed: true);
        var comparisonVerdict = CreateVerdict($"{requestId}:comparison-verdict", score: 0.64, passed: false);
        var qaResult = new RenderQaResultRecord(
            ResultId: $"{requestId}-qa-result",
            RequestId: request.RequestId,
            Expected: expected,
            Actual: actual,
            Verdict: qaVerdict,
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.18 },
            Warnings: []);
        var comparison = new DesignConversionQaComparisonArtifact(
            ArtifactId: $"{requestId}-comparison",
            ComparisonRequestId: $"{requestId}-comparison-request",
            Expected: expected,
            Actual: actual,
            Verdict: comparisonVerdict,
            DiffImageUri: $"artifact://{requestId}/diff.png",
            DiffImageSha256: $"sha256:{requestId}-diff",
            Metrics: new Dictionary<string, double> { ["pixel_delta"] = 0.36 },
            EvidenceRefs: [$"comparison-source:{requestId}"]);

        return new DesignConversionRuntimeFlowInput(
            Request: request,
            QaResults: [qaResult],
            ComparisonArtifacts: [comparison],
            AiTeacherEvidenceRefs: [$"teacher-review:{requestId}:redacted"],
            DeterministicEvidenceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["fixture"] = "orchestrator-integration"
            });
    }

    private static SubmittedDesignConversionRequest CreateRequest(string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            Html: $"<main class=\"typed-card\" data-request=\"{requestId}\">typed metadata only</main>",
            Css: $".typed-card-{requestId}{{display:grid;gap:4px;}}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: $"{requestId}-tree-current",
                Format: "figma-json",
                Content: $"{{\"typedTree\":\"{requestId}\"}}",
                Sha256: $"sha256:{requestId}-tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: $"{requestId}-tree-original",
                Format: "original-json",
                Content: $"{{\"typedOriginalTree\":\"{requestId}\"}}",
                Sha256: $"sha256:{requestId}-tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots: [new DesignRenderArtifact($"{requestId}-shot", "screenshot", 1440, 900, $"artifact://{requestId}/shot.png", $"sha256:{requestId}-shot", new Dictionary<string, string>())],
            DesignRenders: [new DesignRenderArtifact($"{requestId}-render", "design-render", 1440, 900, $"artifact://{requestId}/render.png", $"sha256:{requestId}-render", new Dictionary<string, string>())],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 4,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["source"] = "typed-metadata-fixture"
            });

    private static RenderQaImageRecord CreateImage(string id, int width, int height)
        => new(
            ImageId: id,
            Label: id,
            Resolution: new RenderQaResolution($"res-{width}x{height}", width, height, 1.0),
            Uri: $"artifact://{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static VisualVerdict CreateVerdict(string verdictId, double score, bool passed)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{verdictId}:spacing",
                    Category: "spacing",
                    Severity: passed ? "info" : "major",
                    Description: "Spacing evidence supplied by typed render QA input.",
                    X: 24,
                    Y: 40,
                    Width: 120,
                    Height: 32,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: [$"verdict-evidence:{verdictId}"],
            Passed: passed);

    private static void AssertNoRawDesignPayloadInUserVisibleSurfaces(
        LogicalAssistantRunResult result,
        DesignConversionRuntimeFlowInput typedInput)
    {
        string[] forbiddenPayloads =
        [
            PromptSentinelHtml,
            PromptSentinelCss,
            PromptSentinelTree,
            typedInput.Request.Html,
            typedInput.Request.Css,
            typedInput.Request.DesignTreePayload.Content,
            typedInput.Request.OriginalTreePayload.Content
        ];
        var userVisibleSurfaces = new List<(string Name, string Content)>
        {
            ("FinalHumanResult", result.FinalHumanResult)
        };

        foreach (var metric in result.Metrics)
        {
            userVisibleSurfaces.Add(($"Metrics key '{metric.Key}'", metric.Key));
            userVisibleSurfaces.Add(($"Metrics value for '{metric.Key}'", metric.Value));
        }

        foreach (var evidenceId in result.EvidenceIds)
            userVisibleSurfaces.Add(("EvidenceId", evidenceId));

        foreach (var surface in userVisibleSurfaces)
        {
            for (var payloadIndex = 0; payloadIndex < forbiddenPayloads.Length; payloadIndex++)
            {
                Assert.False(
                    surface.Content.Contains(forbiddenPayloads[payloadIndex], StringComparison.Ordinal),
                    $"{surface.Name} contained forbidden raw payload #{payloadIndex + 1}.");
            }
        }
    }

    private static string BuildPromptSentinel()
        => $"""
            Convert this prompt-only payload, but it must not become the DesignConversion domain input:
            HTML={PromptSentinelHtml}
            CSS={PromptSentinelCss}
            TREE={PromptSentinelTree}
            """;
}

file sealed class DesignConversionAssistantNullSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok((IReadOnlyList<string>)Array.Empty<string>()));
}
