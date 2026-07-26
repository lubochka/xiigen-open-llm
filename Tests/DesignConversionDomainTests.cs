// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using AgentInfrastructure.Logical;
using DesignConversionDomain;

public sealed class DesignConversionDomainTests
{
    [Fact]
    public void SubmittedRequest_CarriesSourceMarkupTreesRendersAndBudget()
    {
        var request = CreateRequest();

        Assert.Equal(DesignConversionSourceKind.Figma, request.SourceKind);
        Assert.Contains("<main", request.Html);
        Assert.Equal(".card{display:grid;}", request.Css);
        Assert.Equal("figma-json", request.DesignTreePayload.Format);
        Assert.Equal("original-json", request.OriginalTreePayload.Format);
        Assert.Single(request.Screenshots);
        Assert.Single(request.DesignRenders);
        Assert.Equal("pro", request.Budget.Tier);
        Assert.Equal(3, request.Budget.MaxImprovementCandidates);
        Assert.Equal("conversation:current", request.Provenance["source"]);
    }

    [Fact]
    public void VisualVerdictAndQaRecords_AreSerializationFriendlyAcrossResolutions()
    {
        var expected = CreateImage("expected", 1440, 900);
        var actual = CreateImage("actual", 1440, 900);
        var verdict = CreateVerdict(score: 0.92, passed: true);
        var result = new RenderQaResultRecord(
            ResultId: "qa-result-001",
            RequestId: "design-request-001",
            Expected: expected,
            Actual: actual,
            Verdict: verdict,
            Metrics: new Dictionary<string, double>
            {
                ["pixel_delta"] = 0.08,
                ["layout_shift"] = 0.01
            },
            Warnings: []);

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var roundTrip = JsonSerializer.Deserialize<RenderQaResultRecord>(
            JsonSerializer.Serialize(result, options),
            options);

        Assert.NotNull(roundTrip);
        Assert.Equal("qa-result-001", roundTrip!.ResultId);
        Assert.Equal(1440, roundTrip.Expected.Resolution.Width);
        Assert.Equal(900, roundTrip.Actual.Resolution.Height);
        Assert.Equal(0.92, roundTrip.Verdict.Score);
        Assert.Single(roundTrip.Verdict.Defects);
        Assert.Equal(0.08, roundTrip.Metrics["pixel_delta"]);
    }

    [Fact]
    public void ImprovementOutput_IsImmutableAndCarriesQaEvidence()
    {
        var output = CreateImprovementOutput("output-accepted", accepted: true);
        var rejected = output with
        {
            OutputId = "output-rejected",
            Accepted = false
        };

        Assert.True(output.Accepted);
        Assert.False(rejected.Accepted);
        Assert.Equal("output-accepted", output.OutputId);
        Assert.Equal("output-rejected", rejected.OutputId);
        Assert.Single(output.QaResults);
        Assert.Contains("qa-artifact:001", output.EvidenceRefs);
    }

    [Fact]
    public void DpoTrainingRecord_CarriesUnitStateVectorStructureProvenanceLicenseAndQuarantine()
    {
        var chosen = CreateImprovementOutput("chosen-output", accepted: true);
        var rejected = CreateImprovementOutput("rejected-output", accepted: false);
        var record = new DesignConversionDpoTrainingRecord(
            RecordId: "dpo-design-001",
            UnitId: "DesignConversionCriticUnit",
            InternalUnitStatePayload: "{\"schema\":\"critic-state-v1\"}",
            InternalVectorPayload: [0.11, 0.42, 0.87],
            SourceStructurePayload: "{\"nodes\":[\"frame\",\"button\"]}",
            Chosen: chosen,
            Rejected: rejected,
            TieBreakerPayload: "{\"human\":\"chosen retains spacing\"}",
            UsefulnessScore: 0.81,
            NegativeQualityReason: "Rejected preserves markup but misses evidence-backed spacing.",
            NegativeReasonTaxonomy: DesignConversionNegativeReasonTaxonomy.Spacing,
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId: "design-request-001",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "training-data/design-conversion/manifest.json",
                ReviewerRef: "reviewer:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string> { ["split"] = "train" }),
            License: new DesignConversionLicenseScope(
                LicenseId: "license-project-local",
                RightsScope: "project-local-redacted",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: new Dictionary<string, string>()),
            Quarantine: new DesignConversionQuarantineState(
                IsQuarantined: false,
                Reason: "accepted",
                EvidenceRefs: ["license:checked"]));

        Assert.Equal("DesignConversionCriticUnit", record.UnitId);
        Assert.Equal(3, record.InternalVectorPayload.Count);
        Assert.Contains("frame", record.SourceStructurePayload);
        Assert.True(record.Chosen.Accepted);
        Assert.False(record.Rejected.Accepted);
        Assert.Equal(0.81, record.UsefulnessScore);
        Assert.Equal("Rejected preserves markup but misses evidence-backed spacing.", record.NegativeQualityReason);
        Assert.Equal(DesignConversionNegativeReasonTaxonomy.Spacing, record.NegativeReasonTaxonomy);
        Assert.True(record.License.AllowsTraining);
        Assert.False(record.License.ContainsUserPrivateData);
        Assert.False(record.Quarantine.IsQuarantined);
    }

    [Fact]
    public void UnitContracts_ExposeLogicalUnitsWithoutClaimingDeterministicLogicAsLearning()
    {
        Assert.True(typeof(ILogicalUnit<DesignConversionCriticUnitInput, DesignConversionCriticUnitOutput>)
            .IsAssignableFrom(typeof(IDesignConversionCriticUnit)));
        Assert.True(typeof(ILogicalUnit<DesignConversionImproverUnitInput, DesignConversionImproverUnitOutput>)
            .IsAssignableFrom(typeof(IDesignConversionImproverUnit)));

        var critic = new DesignConversionCriticUnitOutput(
            OutputId: "critic-output-001",
            Verdict: CreateVerdict(score: 0.74, passed: false),
            QaEvidenceRefs: ["qa-result:001"],
            AiTeacherEvidenceRefs: ["teacher-review:redacted"],
            DeterministicLogicClaimedAsLearning: false);
        var improver = new DesignConversionImproverUnitOutput(
            OutputId: "improver-output-001",
            Candidate: CreateCandidate(),
            Output: CreateImprovementOutput("improver-final", accepted: true),
            QaEvidenceRefs: ["qa-result:002"],
            AiTeacherEvidenceRefs: ["teacher-review:redacted"],
            DeterministicLogicClaimedAsLearning: false);

        Assert.False(critic.DeterministicLogicClaimedAsLearning);
        Assert.False(improver.DeterministicLogicClaimedAsLearning);
        Assert.Contains("teacher-review:redacted", critic.AiTeacherEvidenceRefs);
        Assert.Contains("qa-result:002", improver.QaEvidenceRefs);
    }

    [Fact]
    public void QaContracts_CarryRenderRequestsResultsAndComparisonArtifacts()
    {
        var image = CreateImage("actual-mobile", 390, 844);
        var renderRequest = new DesignConversionRenderRequest(
            RenderRequestId: "render-request-001",
            RequestId: "design-request-001",
            Html: "<main>mobile</main>",
            Css: "main{width:100%;}",
            Resolution: image.Resolution,
            Metadata: new Dictionary<string, string> { ["browser"] = "chromium" });
        var renderResult = new DesignConversionRenderResult(
            RenderResultId: "render-result-001",
            RenderRequestId: renderRequest.RenderRequestId,
            Image: image,
            ElapsedMs: 125,
            Warnings: []);
        var comparison = new DesignConversionQaComparisonArtifact(
            ArtifactId: "comparison-001",
            ComparisonRequestId: "comparison-request-001",
            Expected: CreateImage("expected-mobile", 390, 844),
            Actual: image,
            Verdict: CreateVerdict(score: 0.68, passed: false),
            DiffImageUri: "artifact://diff-mobile.png",
            DiffImageSha256: "sha256:diff-mobile",
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.32 },
            EvidenceRefs: ["render-result-001"]);

        Assert.Equal("chromium", renderRequest.Metadata["browser"]);
        Assert.Equal(125, renderResult.ElapsedMs);
        Assert.Equal(390, comparison.Actual.Resolution.Width);
        Assert.Equal("artifact://diff-mobile.png", comparison.DiffImageUri);
        Assert.False(comparison.Verdict.Passed);
    }

    private static SubmittedDesignConversionRequest CreateRequest()
        => new(
            RequestId: "design-request-001",
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
                Metadata: new Dictionary<string, string> { ["budget_policy"] = "bounded" }),
            Provenance: new Dictionary<string, string> { ["source"] = "conversation:current" });

    private static RenderQaImageRecord CreateImage(string id, int width, int height)
        => new(
            ImageId: id,
            Label: id,
            Resolution: new RenderQaResolution($"res-{width}x{height}", width, height, 1.0),
            Uri: $"artifact://{id}.png",
            Sha256: $"sha256:{id}",
            Metadata: new Dictionary<string, string>());

    private static VisualVerdict CreateVerdict(double score, bool passed)
        => new(
            VerdictId: $"verdict-{score:0.00}",
            Score: score,
            Defects:
            [
                new VisualDefect(
                    DefectId: "defect-spacing-001",
                    Category: "spacing",
                    Severity: "minor",
                    Description: "Button spacing differs from source render.",
                    X: 24,
                    Y: 40,
                    Width: 120,
                    Height: 32,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: ["qa-artifact:001"],
            Passed: passed);

    private static DesignConversionImprovementCandidate CreateCandidate()
        => new(
            CandidateId: "candidate-001",
            RequestId: "design-request-001",
            Html: "<main class=\"card improved\">Hello</main>",
            Css: ".card{display:grid;gap:12px;}",
            ChangeReasons: ["Align card spacing with source render."],
            EvidenceRefs: ["critic-output-001"],
            Metadata: new Dictionary<string, string>());

    private static DesignConversionImprovementOutput CreateImprovementOutput(string outputId, bool accepted)
        => new(
            OutputId: outputId,
            CandidateId: "candidate-001",
            Html: "<main class=\"card improved\">Hello</main>",
            Css: ".card{display:grid;gap:12px;}",
            Verdict: CreateVerdict(accepted ? 0.95 : 0.41, accepted),
            QaResults:
            [
                new RenderQaResultRecord(
                    ResultId: "qa-result-001",
                    RequestId: "design-request-001",
                    Expected: CreateImage("expected", 1440, 900),
                    Actual: CreateImage("actual", 1440, 900),
                    Verdict: CreateVerdict(accepted ? 0.95 : 0.41, accepted),
                    Metrics: new Dictionary<string, double> { ["visual_distance"] = accepted ? 0.05 : 0.59 },
                    Warnings: [])
            ],
            EvidenceRefs: ["qa-artifact:001"],
            Accepted: accepted);
}
