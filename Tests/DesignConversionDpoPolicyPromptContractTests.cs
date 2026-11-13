// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;
using TransformerModel.Config;

public sealed class DesignConversionDpoPolicyPromptContractTests
{
    private static readonly string[] ExpectedKeys =
    [
        "vecp",
        "schema",
        "sig",
        "fmap",
        "vecn",
        "guard",
        "unit"
    ];

    [Fact]
    public void PolicyPrompt_ContainsOnlyOrderedPreOutcomeKeys()
    {
        var prompt = BuildPrompt(BuildRecord());
        var tokens = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var keys = tokens.Skip(1).Select(ReadKey).ToArray();

        Assert.Equal("srcenc", tokens[0]);
        Assert.Equal(ExpectedKeys, keys);
        Assert.DoesNotContain(keys, key => key is "vech" or "manifest" or "structure" or "tax" or "use");
    }

    [Fact]
    public void PolicyPrompt_OutcomeReviewAndAuditIdentityMutationsAreByteInvariant()
    {
        var baseline = BuildRecord();
        var mutated = MutateOutcomeReviewAndAuditIdentity(baseline);

        Assert.Equal(BuildPrompt(baseline), BuildPrompt(mutated));
    }

    [Fact]
    public void PolicyPrompt_LegitimateSourceVectorSummaryAndUnitRemainEffective()
    {
        var baseline = BuildRecord();
        var baselinePrompt = BuildPrompt(baseline);
        var summaryMetadata = new Dictionary<string, string>(baseline.Provenance.Metadata)
        {
            ["source_structure_signal_summary"] = "changed-legitimate-accepted-summary"
        };
        var summaryChanged = baseline with
        {
            Provenance = baseline.Provenance with { Metadata = summaryMetadata }
        };

        Assert.Contains("accepted", baselinePrompt, StringComparison.Ordinal);
        Assert.NotEqual(
            baselinePrompt,
            BuildPrompt(baseline with { InternalVectorPayload = [-0.75, 0.5, 0.125] }));
        Assert.NotEqual(baselinePrompt, BuildPrompt(summaryChanged));
        Assert.NotEqual(
            baselinePrompt,
            BuildPrompt(baseline with { UnitId = DesignConversionDpoUnits.ImproverUnit }));
    }

    [Fact]
    public void PolicyPrompt_OpaqueManifestAndStructureStayOutOfBandButRemainAuditCommitted()
    {
        var baseline = BuildRecord();
        var manifestChanged = baseline with
        {
            Provenance = baseline.Provenance with
            {
                SourceManifestRef = "artifact://opaque-source/changed-manifest.json"
            }
        };
        var structureChanged = baseline with
        {
            SourceStructurePayload = "{\"type\":\"OPAQUE_CHANGED_FRAME\",\"result_ref\":\"opaque-result\"}"
        };

        var baselineDescriptor = DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            [baseline],
            BuildConfig());
        var manifestDescriptor = DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            [manifestChanged],
            BuildConfig());
        var structureDescriptor = DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            [structureChanged],
            BuildConfig());

        var baselinePrompt = BuildPrompt(baseline);
        Assert.Equal(baselinePrompt, BuildPrompt(manifestChanged));
        Assert.Equal(baselinePrompt, BuildPrompt(structureChanged));
        Assert.DoesNotContain("vech=", baselinePrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("manifest=", baselinePrompt, StringComparison.Ordinal);
        Assert.DoesNotContain("structure=", baselinePrompt, StringComparison.Ordinal);
        Assert.True(baselineDescriptor.IsSuccess, baselineDescriptor.ErrorMessage);
        Assert.True(manifestDescriptor.IsSuccess, manifestDescriptor.ErrorMessage);
        Assert.True(structureDescriptor.IsSuccess, structureDescriptor.ErrorMessage);
        Assert.NotEqual(
            baselineDescriptor.Value!.SourceSignalSetSha256,
            manifestDescriptor.Value!.SourceSignalSetSha256);
        Assert.NotEqual(
            baselineDescriptor.Value.SourceSignalSetSha256,
            structureDescriptor.Value!.SourceSignalSetSha256);
    }

    [Fact]
    public void SourceAuditCommitment_UsesV3DomainAndLengthFramesManifestAndStructure()
    {
        Assert.Equal(
            "design-conversion-dpo-selected-source-signal-set-v3",
            DesignConversionDpoSourceEncoderContractBuilder.SourceSignalSetHashDomain);

        var baseline = BuildRecord();
        var manifestAAndStructureBc = baseline with
        {
            SourceStructurePayload = "bc",
            Provenance = baseline.Provenance with { SourceManifestRef = "a" }
        };
        var manifestAbAndStructureC = baseline with
        {
            SourceStructurePayload = "c",
            Provenance = baseline.Provenance with { SourceManifestRef = "ab" }
        };
        Assert.Equal(
            manifestAAndStructureBc.Provenance.SourceManifestRef + manifestAAndStructureBc.SourceStructurePayload,
            manifestAbAndStructureC.Provenance.SourceManifestRef + manifestAbAndStructureC.SourceStructurePayload);
        Assert.Equal(BuildPrompt(manifestAAndStructureBc), BuildPrompt(manifestAbAndStructureC));

        var first = DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            [manifestAAndStructureBc],
            BuildConfig());
        var second = DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            [manifestAbAndStructureC],
            BuildConfig());

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotEqual(
            first.Value!.SourceSignalSetSha256,
            second.Value!.SourceSignalSetSha256);
    }

    [Fact]
    public void SourceAuditSurfaces_RetainVectorIdentityAndRecordCommitment()
    {
        var baseline = BuildRecord();
        var sourceIds = DesignConversionDpoSourceEncoderContractBuilder.BuildSourceIds(baseline);
        var descriptor = DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            [baseline],
            BuildConfig());
        var auditIdentityChanged = DesignConversionDpoSourceEncoderContractBuilder.BuildDescriptor(
            [baseline with { RecordId = "audit-identity-changed" }],
            BuildConfig());

        Assert.Contains(
            "dpo_policy_bridge_source_encoder_vector_hash:vector-hash-authority",
            sourceIds);
        Assert.True(descriptor.IsSuccess, descriptor.ErrorMessage);
        Assert.True(auditIdentityChanged.IsSuccess, auditIdentityChanged.ErrorMessage);
        Assert.Equal("feature-map-authority", descriptor.Value!.FeatureMapHash);
        Assert.NotEqual(
            descriptor.Value.SourceSignalSetSha256,
            auditIdentityChanged.Value!.SourceSignalSetSha256);
    }

    private static string BuildPrompt(DesignConversionDpoTrainingRecord row)
        => DesignConversionDpoSourceEncoderContractBuilder.BuildPolicyPrompt(row);

    private static string ReadKey(string token)
    {
        var separator = token.IndexOf('=', StringComparison.Ordinal);
        Assert.True(separator > 0, $"Prompt token does not have a key/value boundary: {token}");
        return token[..separator];
    }

    private static DesignConversionDpoTrainingRecord BuildRecord()
    {
        var metadata = new Dictionary<string, string>
        {
            ["source_encoder"] = "source-encoder",
            ["source_encoder_schema"] = "source-schema-v1",
            ["source_encoder_role"] = "source-role",
            ["source_encoder_authority"] = "source-authority",
            ["source_encoder_feature_namespace"] = "source-features",
            ["source_encoder_feature_names"] = "layout,color,spacing",
            ["source_structure_signal_summary"] = "legitimate-accepted-source-summary",
            ["source_encoder_vector_hash"] = "vector-hash-authority",
            ["source_encoder_feature_map_hash"] = "feature-map-authority",
            ["source_encoder_shortcut_guard"] = "shortcut-guard-authority"
        };

        return new DesignConversionDpoTrainingRecord(
            RecordId: "policy-prompt-record",
            UnitId: DesignConversionDpoUnits.CriticUnit,
            InternalUnitStatePayload: "{\"state\":\"baseline\"}",
            InternalVectorPayload: [0.25, -0.5, 0.75],
            SourceStructurePayload: "{\"type\":\"FRAME\"}",
            Chosen: BuildOutput("chosen", score: 0.96, passed: true, accepted: true),
            Rejected: BuildOutput("rejected", score: 0.12, passed: false, accepted: false),
            TieBreakerPayload: "{\"winner\":\"chosen\"}",
            UsefulnessScore: 0.91,
            NegativeQualityReason: "baseline quality reason",
            NegativeReasonTaxonomy: "baseline-taxonomy",
            Provenance: new DesignConversionDpoProvenance(
                "baseline-request",
                "Figma",
                "artifact://stable-source/manifest.json",
                "baseline-reviewer",
                DateTimeOffset.UnixEpoch,
                metadata),
            License: new DesignConversionLicenseScope(
                "policy-prompt-license",
                "local-test",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                new Dictionary<string, string>()),
            Quarantine: new DesignConversionQuarantineState(
                IsQuarantined: false,
                Reason: string.Empty,
                EvidenceRefs: []));
    }

    private static DesignConversionDpoTrainingRecord MutateOutcomeReviewAndAuditIdentity(
        DesignConversionDpoTrainingRecord baseline)
        => baseline with
        {
            RecordId = "mutated-record-id",
            InternalUnitStatePayload = "{\"state\":\"mutated-outcome-state\"}",
            Chosen = MutateOutput(baseline.Chosen, "mutated-chosen", accepted: false),
            Rejected = MutateOutput(baseline.Rejected, "mutated-rejected", accepted: true),
            TieBreakerPayload = "{\"winner\":\"mutated\"}",
            UsefulnessScore = 0.01,
            NegativeQualityReason = "mutated negative quality reason",
            NegativeReasonTaxonomy = "mutated-taxonomy",
            Provenance = baseline.Provenance with
            {
                SourceRequestId = "mutated-source-request",
                ReviewerRef = "mutated-reviewer",
                CreatedAtUtc = DateTimeOffset.UnixEpoch.AddDays(7)
            },
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: true,
                Reason: "mutated outcome quarantine",
                EvidenceRefs: ["mutated-quarantine-evidence"])
        };

    private static DesignConversionImprovementOutput MutateOutput(
        DesignConversionImprovementOutput baseline,
        string prefix,
        bool accepted)
        => baseline with
        {
            OutputId = $"{prefix}-output-id",
            CandidateId = $"{prefix}-candidate-id",
            Html = $"<main>{prefix}-html</main>",
            Css = $"main {{ color: {prefix}; }}",
            Verdict = new VisualVerdict(
                $"{prefix}-verdict-id",
                0.42,
                [
                    new VisualDefect(
                        $"{prefix}-defect-id",
                        $"{prefix}-category",
                        "high",
                        $"{prefix}-description",
                        1,
                        2,
                        3,
                        4,
                        [$"{prefix}-defect-evidence"])
                ],
                [$"{prefix}-verdict-evidence"],
                Passed: !baseline.Verdict.Passed),
            QaResults = [BuildQaResult(prefix)],
            EvidenceRefs = [$"{prefix}-output-evidence"],
            Accepted = accepted
        };

    private static DesignConversionImprovementOutput BuildOutput(
        string prefix,
        double score,
        bool passed,
        bool accepted)
        => new(
            $"{prefix}-output",
            $"{prefix}-candidate",
            $"<main>{prefix}</main>",
            $"main {{ color: {prefix}; }}",
            new VisualVerdict(
                $"{prefix}-verdict",
                score,
                [],
                [$"{prefix}-verdict-evidence"],
                passed),
            QaResults: [],
            EvidenceRefs: [$"{prefix}-output-evidence"],
            Accepted: accepted);

    private static RenderQaResultRecord BuildQaResult(string prefix)
    {
        var resolution = new RenderQaResolution($"{prefix}-resolution", 1440, 900, 1.0);
        var expected = new RenderQaImageRecord(
            $"{prefix}-expected-image",
            "expected",
            resolution,
            $"artifact://{prefix}/expected.png",
            new string('a', 64),
            new Dictionary<string, string>());
        var actual = new RenderQaImageRecord(
            $"{prefix}-actual-image",
            "actual",
            resolution,
            $"artifact://{prefix}/actual.png",
            new string('b', 64),
            new Dictionary<string, string>());
        return new RenderQaResultRecord(
            $"{prefix}-qa-result",
            $"{prefix}-qa-request",
            expected,
            actual,
            new VisualVerdict(
                $"{prefix}-qa-verdict",
                0.33,
                [],
                [$"{prefix}-qa-evidence"],
                Passed: false),
            new Dictionary<string, double> { ["similarity"] = 0.33 },
            [$"{prefix}-qa-warning"]);
    }

    private static ModelConfig BuildConfig()
        => new()
        {
            VocabSize = 256,
            ContextLen = 128,
            HiddenDim = 16,
            NumLayers = 1,
            NumHeads = 4,
            NumKvHeads = 2,
            FfnHiddenDim = 32
        };
}
