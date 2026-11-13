// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionRuntimeFlowTests
{
    [Fact]
    public async Task RoutingFlow_CallsCriticThenImproverAndPropagatesVerdict()
    {
        var order = new List<string>();
        var verdict = CreateVerdict("critic-verdict-custom", score: 0.37, passed: false);
        var critic = new RecordingCriticUnit(order, verdict);
        var improver = new RecordingImproverUnit(order);
        var flow = new DesignConversionRoutingFlow(critic, improver);

        var result = await flow.RunAsync(
            CreateInput(),
            runId: "run-order",
            flags: new LogicalExecutionFlags(),
            metadata: new Dictionary<string, object>());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(["critic", "improver"], order);
        Assert.Equal(["recording-critic", "recording-improver"], result.Value!.UnitExecutionOrder);
        Assert.Equal("critic-verdict-custom", result.Value.Candidate.Metadata["critic_verdict_id"]);
        Assert.Equal(result.Value.CriticOutput.Verdict, result.Value.ImproverOutput.Output.Verdict);
        Assert.False(result.Value.Output.Accepted);
        Assert.False(result.Value.DeterministicLogicClaimedAsLearning);
    }

    [Fact]
    public async Task RuntimeScaffoldUnits_CreateChangedCandidateFromDefectsAndMarkQaAsInputEvidenceOnly()
    {
        var input = CreateInput();
        var flow = new DesignConversionRoutingFlow();

        var result = await flow.RunAsync(
            input,
            runId: "run-scaffold",
            flags: new LogicalExecutionFlags(),
            metadata: new Dictionary<string, object>());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal($"{input.Request.RequestId}:run-scaffold:runtime-scaffold-candidate", result.Value!.Candidate.CandidateId);
        Assert.NotEqual(input.Request.Html, result.Value.Candidate.Html);
        Assert.NotEqual(input.Request.Css, result.Value.Candidate.Css);
        Assert.Contains("dcm-local-improved", result.Value.Candidate.Html);
        Assert.Contains("gap:12px", result.Value.Candidate.Css);
        Assert.Contains("padding:16px", result.Value.Candidate.Css);
        Assert.Contains("DESIGN_CONVERSION_IMPROVER_LOCAL_NON_COPY", result.Value.EvidenceRefs);
        Assert.Equal(bool.TrueString, result.Value.Candidate.Metadata["candidate_html_changed"]);
        Assert.Equal(bool.TrueString, result.Value.Candidate.Metadata["candidate_css_changed"]);
        Assert.Contains("render-qa-result:qa-result-001", result.Value.Candidate.EvidenceRefs);
        Assert.Contains("qa-comparison-artifact:comparison-001", result.Value.Candidate.EvidenceRefs);
        Assert.Contains("deterministic-input-evidence:fixture=render-qa-record", result.Value.Candidate.EvidenceRefs);
        Assert.Equal("input_evidence_only", result.Value.Candidate.Metadata["qa_data_role"]);
        Assert.False(result.Value.CriticOutput.DeterministicLogicClaimedAsLearning);
        Assert.False(result.Value.ImproverOutput.DeterministicLogicClaimedAsLearning);
    }

    [Fact]
    public async Task LogicalFlowResult_ExposesOrderMetricsAndEvidenceBoundary()
    {
        var flow = new DesignConversionRoutingFlow();

        var result = await flow.ExecuteAsync(new LogicalFlowRequest<DesignConversionRuntimeFlowInput>(
            RunId: "run-logical",
            FlowId: DesignConversionRoutingFlow.FlowId,
            ScenarioId: DesignConversionRoutingFlow.ScenarioId,
            Input: CreateInput(),
            Flags: new LogicalExecutionFlags(),
            Metadata: new Dictionary<string, object>()));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(DesignConversionRoutingFlow.FlowId, result.Value!.FlowId);
        Assert.Equal(
            $"{DesignConversionRuntimeScaffoldCriticUnit.UnitId}>{DesignConversionRuntimeScaffoldImproverUnit.UnitId}",
            result.Value.Metrics["unit_order"]);
        Assert.Equal(bool.FalseString, result.Value.Metrics["deterministic_logic_claimed_as_learning"]);
        Assert.Equal("input_evidence_only", result.Value.Metrics["qa_data_role"]);
        Assert.Contains("DESIGN_CONVERSION_ROUTING_FLOW_CRITIC_THEN_IMPROVER", result.Value.EvidenceIds);
        Assert.Contains(DesignConversionRuntimeScaffoldCriticUnit.InputEvidenceOnlyWarning, result.Value.Warnings);
    }

    private static DesignConversionRuntimeFlowInput CreateInput()
    {
        var request = CreateRequest();
        var expected = CreateImage("expected", 1440, 900);
        var actual = CreateImage("actual", 1440, 900);
        var qaVerdict = CreateVerdict("qa-verdict", score: 0.82, passed: true);
        var comparisonVerdict = CreateVerdict("comparison-verdict", score: 0.64, passed: false);
        var qaResult = new RenderQaResultRecord(
            ResultId: "qa-result-001",
            RequestId: request.RequestId,
            Expected: expected,
            Actual: actual,
            Verdict: qaVerdict,
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.18 },
            Warnings: []);
        var comparison = new DesignConversionQaComparisonArtifact(
            ArtifactId: "comparison-001",
            ComparisonRequestId: "comparison-request-001",
            Expected: expected,
            Actual: actual,
            Verdict: comparisonVerdict,
            DiffImageUri: "artifact://diff.png",
            DiffImageSha256: "sha256:diff",
            Metrics: new Dictionary<string, double> { ["pixel_delta"] = 0.36 },
            EvidenceRefs: ["comparison-source:001"]);

        return new DesignConversionRuntimeFlowInput(
            Request: request,
            QaResults: [qaResult],
            ComparisonArtifacts: [comparison],
            AiTeacherEvidenceRefs: ["teacher-review:redacted"],
            DeterministicEvidenceRefs: new Dictionary<string, string>
            {
                ["fixture"] = "render-qa-record"
            });
    }

    private static SubmittedDesignConversionRequest CreateRequest()
        => new(
            RequestId: "design-request-runtime-001",
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"card\">Hello</main>",
            Css: ".card{display:grid;}",
            DesignTreePayload: new DesignTreePayload(
                PayloadId: "tree-current",
                Format: "figma-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-current",
                Metadata: new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                PayloadId: "tree-original",
                Format: "original-json",
                Content: "{\"type\":\"FRAME\"}",
                Sha256: "sha256:tree-original",
                Metadata: new Dictionary<string, string>()),
            Screenshots: [new DesignRenderArtifact("shot-001", "screenshot", 1440, 900, "artifact://shot.png", "sha256:shot", new Dictionary<string, string>())],
            DesignRenders: [new DesignRenderArtifact("render-001", "design-render", 1440, 900, "artifact://render.png", "sha256:render", new Dictionary<string, string>())],
            Budget: new DesignConversionBudgetMetadata(
                UserId: "user-redacted",
                Tier: "pro",
                MaxImprovementCandidates: 3,
                MaxRenderQaRuns: 4,
                BudgetUsd: 1.50m,
                Metadata: new Dictionary<string, string>()),
            Provenance: new Dictionary<string, string> { ["source"] = "runtime-flow-test" });

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
                    Description: "Spacing evidence supplied by render QA input.",
                    X: 24,
                    Y: 40,
                    Width: 120,
                    Height: 32,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: [$"verdict-evidence:{verdictId}"],
            Passed: passed);

    private sealed class RecordingCriticUnit(
        List<string> order,
        VisualVerdict verdict) : IDesignConversionCriticUnit
    {
        public Task<OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionCriticUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            order.Add("critic");
            var output = new DesignConversionCriticUnitOutput(
                OutputId: "recording-critic-output",
                Verdict: verdict,
                QaEvidenceRefs: ["recording-qa-evidence"],
                AiTeacherEvidenceRefs: ["recording-teacher-evidence"],
                DeterministicLogicClaimedAsLearning: false);

            var result = new LogicalUnitResult<DesignConversionCriticUnitOutput>(
                UnitId: "recording-critic",
                UnitRole: "recording",
                DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                Output: output,
                Confidence: verdict.Score,
                PredicateIds: [],
                DomainTags: [DesignConversionRuntimeScaffoldCriticUnit.DomainTag],
                EvidenceIds: ["recording-critic-evidence"],
                Warnings: []);

            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionCriticUnitOutput>>.Ok(result));
        }
    }

    private sealed class RecordingImproverUnit(List<string> order) : IDesignConversionImproverUnit
    {
        public Task<OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>> ExecuteAsync(
            LogicalUnitRequest<DesignConversionImproverUnitInput> request,
            CancellationToken cancellationToken = default)
        {
            order.Add("improver");
            var output = new DesignConversionImprovementOutput(
                OutputId: "recording-improver-output",
                CandidateId: request.Input.Candidate.CandidateId,
                Html: request.Input.Candidate.Html,
                Css: request.Input.Candidate.Css,
                Verdict: request.Input.CriticVerdict,
                QaResults: [],
                EvidenceRefs: request.Input.Candidate.EvidenceRefs,
                Accepted: request.Input.CriticVerdict.Passed);
            var unitOutput = new DesignConversionImproverUnitOutput(
                OutputId: output.OutputId,
                Candidate: request.Input.Candidate,
                Output: output,
                QaEvidenceRefs: request.Input.QaEvidenceRefs,
                AiTeacherEvidenceRefs: request.Input.AiTeacherEvidenceRefs,
                DeterministicLogicClaimedAsLearning: false);

            var result = new LogicalUnitResult<DesignConversionImproverUnitOutput>(
                UnitId: "recording-improver",
                UnitRole: "recording",
                DomainTag: DesignConversionRuntimeScaffoldCriticUnit.DomainTag,
                Output: unitOutput,
                Confidence: request.Input.CriticVerdict.Score,
                PredicateIds: [],
                DomainTags: [DesignConversionRuntimeScaffoldCriticUnit.DomainTag],
                EvidenceIds: ["recording-improver-evidence"],
                Warnings: []);

            return Task.FromResult(OperationResult<LogicalUnitResult<DesignConversionImproverUnitOutput>>.Ok(result));
        }
    }
}
