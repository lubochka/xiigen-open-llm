// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;
using ContinuousLearning;
using System.Text.Json;

public sealed class DesignConversionDpoGeneratorTests
{
    private const string ExpectedTeacherModelId = "qwen-test:9b";
    private static readonly string ExpectedTeacherManifestDigest =
        new('c', 64);
    private static readonly string ExpectedTeacherModelLayerDigest =
        new('d', 64);

    [Fact]
    public void BuildTrainReadyRecords_CreatesCriticAndImproverPairs()
    {
        var generator = new DesignConversionDpoGenerator();

        var result = generator.BuildTrainReadyRecords(
        [
            CreateExample("critic-pair", DesignConversionDpoUnits.CriticUnit, usefulness: 0.72),
            CreateExample("improver-pair", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.91)
        ]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(2, result.Value!.AcceptedRecords.Count);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Equal([DesignConversionDpoUnits.ImproverUnit, DesignConversionDpoUnits.CriticUnit],
            result.Value.AcceptedRecords.Select(record => record.UnitId).ToArray());
        Assert.All(result.Value.AcceptedRecords, record =>
        {
            Assert.False(record.Quarantine.IsQuarantined);
            Assert.Equal("train_ready", record.Quarantine.Reason);
            Assert.True(record.Chosen.Accepted);
            Assert.False(record.Rejected.Accepted);
            Assert.True(record.Chosen.Verdict.Score > record.Rejected.Verdict.Score);
            Assert.Equal("Rejected is a realistic near-miss with evidence-backed spacing defects.", record.NegativeQualityReason);
            Assert.Equal(DesignConversionNegativeReasonTaxonomy.Spacing, record.NegativeReasonTaxonomy);
            Assert.Equal(bool.FalseString, record.Provenance.Metadata["deterministic_logic_claimed_as_learning"]);
            Assert.Equal("evidence_only", record.Provenance.Metadata["qa_teacher_evidence_role"]);
            Assert.Equal("evidence_explanation_only", record.Provenance.Metadata["negative_quality_reason_role"]);
            Assert.Equal("review_priority_only", record.Provenance.Metadata["usefulness_score_role"]);
        });
    }

    [Fact]
    public void BuildTrainReadyRecords_QuarantinesInvalidNegativeExamples()
    {
        var generator = new DesignConversionDpoGenerator();
        var sameBase = CreateExample("same", DesignConversionDpoUnits.CriticUnit);
        var sameAsChosen = sameBase with
        {
            Rejected = sameBase.Chosen with
            {
                OutputId = "same-rejected",
                CandidateId = "candidate-same-rejected",
                Accepted = false
            }
        };
        var emptyRejected = CreateExample("empty", DesignConversionDpoUnits.ImproverUnit) with
        {
            Rejected = CreateOutput("empty-rejected", accepted: false, score: 0.12, htmlSuffix: "", includeMarkup: false)
        };
        var licenseBlocked = CreateExample("license", DesignConversionDpoUnits.CriticUnit) with
        {
            License = CreateLicense(allowsTraining: false, containsPrivateData: false)
        };

        var result = generator.BuildTrainReadyRecords([sameAsChosen, emptyRejected, licenseBlocked]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Equal(3, result.Value.QuarantinedRecords.Count);
        Assert.Contains(result.Value.QuarantinedRecords, record => record.Quarantine.Reason.Contains("chosen_rejected_same", StringComparison.Ordinal));
        Assert.Contains(result.Value.QuarantinedRecords, record => record.Quarantine.Reason.Contains("rejected_markup_required", StringComparison.Ordinal));
        Assert.Contains(result.Value.QuarantinedRecords, record => record.Quarantine.Reason.Contains("license_blocks_training", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildTrainReadyRecords_QuarantinesMissingTeacherProvenanceBeforeTrainReady()
    {
        var generator = new DesignConversionDpoGenerator();
        var missingTeacherProvenance = CreateExample("missing-teacher-provenance", DesignConversionDpoUnits.CriticUnit) with
        {
            TeacherProvenance = null
        };

        var result = generator.BuildTrainReadyRecords([missingTeacherProvenance]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        var record = Assert.Single(result.Value.QuarantinedRecords);
        Assert.True(record.Quarantine.IsQuarantined);
        Assert.Contains("teacher_provenance_required", record.Quarantine.Reason, StringComparison.Ordinal);
        Assert.Null(record.TeacherProvenance);
    }

    [Fact]
    public void BuildTrainReadyRecords_SourceEncoderRedactsForbiddenLeakageBeforeTrainReadyRecord()
    {
        var generator = new DesignConversionDpoGenerator();
        var rawScreenshotBytes = new string('A', 180);
        var leaky = CreateExample(
            "C:/Users/lubao/private-project/example-001",
            DesignConversionDpoUnits.ImproverUnit,
            usefulness: 0.89) with
        {
            InternalUnitStatePayload = """
                {
                  "schema":"design-conversion-unit-state-v1",
                  "exact_prompt":"make the private button blue",
                  "user_id":"user-luba",
                  "transient_run_id":"run-123-private",
                  "focus":"layout"
                }
                """,
            SourceStructurePayload = $$"""
                {
                  "tree":["frame","button"],
                  "raw_path":"C:/Users/lubao/Documents/private.fig",
                  "candidate_index":2,
                  "raw_screenshot_bytes":"{{rawScreenshotBytes}}",
                  "breakpoints":[390,1440]
                }
                """,
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "request-from-user-luba-exact-prompt",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "file:///C:/Users/lubao/Documents/private.fig",
                ReviewerRef: "reviewer:user-luba",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["source_design_id"] = "C:/Users/lubao/Documents/private.fig",
                    ["candidate_order"] = "chosen-first"
                }),
            Chosen = CreateOutput(
                "chosen-private-output",
                accepted: true,
                score: 0.91,
                htmlSuffix: "chosen") with
            {
                CandidateId = "C:/Users/lubao/private-candidate-a.html",
                Html = "<img src=\"C:/Users/lubao/private.png\"><main class=\"card chosen\">Hello</main>"
            },
            Rejected = CreateOutput(
                "rejected-private-output",
                accepted: false,
                score: 0.35,
                htmlSuffix: "rejected") with
            {
                CandidateId = "C:/Users/lubao/private-candidate-b.html"
            }
        };

        var result = generator.BuildTrainReadyRecords([leaky]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.AcceptedRecords);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.Equal(DesignConversionDpoUnits.ImproverUnit, record.UnitId);
        Assert.Equal("DesignConversionDefaultSourceEncoder", record.Provenance.Metadata["source_encoder"]);
        Assert.Equal("dcm-source-encoder-v1", record.Provenance.Metadata["source_encoder_schema"]);
        Assert.Equal("design_conversion_improver_unit", record.Provenance.Metadata["machine_part"]);
        Assert.Contains("anti_leakage", record.Provenance.Metadata.Keys);
        Assert.Contains("source_encoder_redaction_codes", record.Provenance.Metadata.Keys);

        var trainReadySurface = string.Join(
            "\n",
            record.RecordId,
            record.InternalUnitStatePayload,
            record.SourceStructurePayload,
            record.TieBreakerPayload,
            record.Provenance.SourceRequestId,
            record.Provenance.SourceManifestRef,
            record.Provenance.ReviewerRef,
            record.Chosen.OutputId,
            record.Chosen.CandidateId,
            record.Chosen.Html,
            record.Rejected.CandidateId,
            string.Join("|", record.Quarantine.EvidenceRefs),
            string.Join("|", record.Provenance.Metadata.Select(pair => $"{pair.Key}={pair.Value}")));

        Assert.Contains("\"tree\"", record.SourceStructurePayload, StringComparison.Ordinal);
        Assert.Contains("\"breakpoints\"", record.SourceStructurePayload, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/lubao", trainReadySurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", trainReadySurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("make the private button blue", trainReadySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("exact_prompt", trainReadySurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("candidate_index", trainReadySurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(rawScreenshotBytes, trainReadySurface, StringComparison.Ordinal);
        Assert.DoesNotContain("user-luba", trainReadySurface, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("run-123-private", trainReadySurface, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildTrainReadyRecords_SourceEncoderDerivesVectorFromStructureWithoutResultSignalShortcut()
    {
        var generator = new DesignConversionDpoGenerator();
        var complex = CreateExample("figma-complex", DesignConversionDpoUnits.ImproverUnit, usefulness: 1.05) with
        {
            InternalVectorPayload = [],
            SourceStructurePayload = """
                {
                  "root":{"type":"FRAME","layoutMode":"VERTICAL","width":1440,"height":900},
                  "children":[
                    {"type":"TEXT","name":"headline"},
                    {"type":"INSTANCE","component":"primary_button","constraints":{"horizontal":"CENTER"}},
                    {"type":"IMAGE","name":"hero_asset"}
                  ],
                  "breakpoints":["desktop","mobile"]
                }
                """,
            Chosen = CreateOutput("figma-complex-chosen", accepted: true, score: 0.94, htmlSuffix: "complex-chosen") with
            {
                Html = "<main class=\"hero grid\"><h1>Title</h1><button class=\"cta primary\">Go</button></main>",
                Css = ".hero{display:grid;gap:24px;padding:48px}.cta.primary{display:flex;border-radius:8px}"
            },
            Rejected = CreateOutput("figma-complex-rejected", accepted: false, score: 0.43, htmlSuffix: "complex-rejected") with
            {
                Html = "<main class=\"hero\"><h1>Title</h1><button>Go</button></main>",
                Css = ".hero{display:block}.cta{gap:0}"
            }
        };
        var sparse = CreateExample("figma-sparse", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.82) with
        {
            InternalVectorPayload = [],
            SourceStructurePayload = """
                {
                  "root":{"type":"FRAME","width":390,"height":844},
                  "children":[{"type":"TEXT","name":"caption"}],
                  "breakpoints":["mobile"]
                }
                """,
            Chosen = CreateOutput("figma-sparse-chosen", accepted: true, score: 0.78, htmlSuffix: "sparse-chosen") with
            {
                Html = "<main class=\"panel\"><p>Caption</p></main>",
                Css = ".panel{display:flex;padding:16px}"
            },
            Rejected = CreateOutput("figma-sparse-rejected", accepted: false, score: 0.36, htmlSuffix: "sparse-rejected") with
            {
                Html = "<main><p>Caption</p></main>",
                Css = "main{display:block}"
            }
        };

        var result = generator.BuildTrainReadyRecords([complex, sparse]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        var complexRecord = result.Value.AcceptedRecords.Single(record => record.RecordId == "figma-complex");
        var sparseRecord = result.Value.AcceptedRecords.Single(record => record.RecordId == "figma-sparse");
        Assert.NotEmpty(complexRecord.InternalVectorPayload);
        Assert.NotEmpty(sparseRecord.InternalVectorPayload);
        Assert.False(complexRecord.InternalVectorPayload.SequenceEqual(sparseRecord.InternalVectorPayload));
        Assert.NotEqual(
            complexRecord.Provenance.Metadata["source_encoder_vector_hash"],
            sparseRecord.Provenance.Metadata["source_encoder_vector_hash"]);
        Assert.Equal(bool.FalseString, complexRecord.Provenance.Metadata["source_kind_in_vector"]);
        Assert.Equal(bool.FalseString, complexRecord.Provenance.Metadata["source_id_in_vector"]);
        Assert.Equal(bool.FalseString, complexRecord.Provenance.Metadata["result_features_in_source_vector"]);
        Assert.Equal(bool.FalseString, complexRecord.Provenance.Metadata["visual_score_margin_in_source_vector"]);
        Assert.Equal(bool.FalseString, complexRecord.Provenance.Metadata["usefulness_score_in_source_vector"]);
        Assert.Equal(bool.FalseString, complexRecord.Provenance.Metadata["answer_payload_in_source_vector"]);
        Assert.Equal(bool.FalseString, complexRecord.Provenance.Metadata["provided_internal_vector_in_source_vector"]);
        Assert.Equal("source_structure_provenance_only", complexRecord.Provenance.Metadata["source_encoder_feature_namespace"]);
        Assert.Equal("result_features", complexRecord.Provenance.Metadata["result_feature_namespace"]);
        Assert.Equal("source_kind_source_id_not_vector_features", complexRecord.Provenance.Metadata["source_encoder_shortcut_guard"]);
        Assert.Contains("visual_score_margin=", complexRecord.Provenance.Metadata["result_signal_summary"]);
        var complexFeatureNames = complexRecord.Provenance.Metadata["source_encoder_feature_names"]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Equal(complexRecord.InternalVectorPayload.Count, complexFeatureNames.Length);
        Assert.Contains("source_json_node_count", complexFeatureNames);
        Assert.Contains("source_layout_hint_count", complexFeatureNames);
        Assert.DoesNotContain("chosen_css_rule_count", complexFeatureNames);
        Assert.DoesNotContain("rejected_css_rule_count", complexFeatureNames);
        Assert.DoesNotContain("accepted_flag_delta", complexFeatureNames);
        Assert.DoesNotContain("visual_score_margin", complexFeatureNames);
        Assert.DoesNotContain("chosen_visual_score", complexFeatureNames);
        Assert.DoesNotContain("rejected_visual_score", complexFeatureNames);
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("source_kind", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("source_id", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("chosen", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("rejected", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("result", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("visual", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("answer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("provided_internal_vector", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(complexFeatureNames, feature => feature.Contains("usefulness", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildTrainReadyRecords_SourceEncoderRepresentsFigmaAndCanvaStructureWithoutRawPayloadLeakage()
    {
        var generator = new DesignConversionDpoGenerator();
        var figma = CreateExample("figma-structure", DesignConversionDpoUnits.CriticUnit) with
        {
            InternalVectorPayload = [],
            SourceStructurePayload = """
                {
                  "source":"figma",
                  "file_path":"C:/Users/lubao/private/source.fig",
                  "nodes":[{"type":"FRAME"},{"type":"COMPONENT"},{"type":"TEXT"}],
                  "layout":{"mode":"AUTO_LAYOUT","gap":12,"width":1024,"height":768}
                }
                """,
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "figma-request-redacted",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/figma-redacted",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string> { ["split"] = "train" })
        };
        var canva = CreateExample("canva-structure", DesignConversionDpoUnits.CriticUnit) with
        {
            InternalVectorPayload = [],
            SourceStructurePayload = """
                {
                  "source":"canva",
                  "nodes":[{"kind":"section"},{"kind":"image"},{"kind":"button"}],
                  "viewport":{"name":"desktop","width":1366,"height":768},
                  "constraints":["grid","padding"]
                }
                """,
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "canva-request-redacted",
                SourceKind: DesignConversionSourceKind.Canva.ToString(),
                SourceManifestRef: "manifest://design-conversion/canva-redacted",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string> { ["split"] = "train" })
        };

        var result = generator.BuildTrainReadyRecords([figma, canva]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        Assert.All(result.Value.AcceptedRecords, record =>
        {
            Assert.True(record.InternalVectorPayload.Count >= 10);
            Assert.Contains("nodes=", record.Provenance.Metadata["source_structure_signal_summary"]);
            Assert.Contains("layout_hints=", record.Provenance.Metadata["source_structure_signal_summary"]);
            Assert.Contains("visual_score_margin=", record.Provenance.Metadata["result_signal_summary"]);
            Assert.Contains("source_frame_like_count", record.Provenance.Metadata["source_encoder_feature_names"]);
            Assert.Contains("source_layout_hint_count", record.Provenance.Metadata["source_encoder_feature_names"]);
            Assert.DoesNotContain("visual_score_margin", record.Provenance.Metadata["source_encoder_feature_names"]);
            Assert.Equal(bool.FalseString, record.Provenance.Metadata["visual_score_margin_in_source_vector"]);
        });
        var serialized = string.Join(
            "\n",
            result.Value.AcceptedRecords.Select(record =>
                string.Join("|", record.SourceStructurePayload, string.Join("|", record.Provenance.Metadata.Values))));
        Assert.DoesNotContain("C:/Users/lubao", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private/source.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("source_kind_in_vector=True", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildTrainReadyRecords_QuarantinesUnknownOrDiagnosticOnlyNegativeTaxonomy()
    {
        var generator = new DesignConversionDpoGenerator();
        var unknown = CreateExample("unknown-taxonomy", DesignConversionDpoUnits.CriticUnit) with
        {
            NegativeReasonTaxonomy = "unmapped_visual_issue"
        };
        var diagnosticOnly = CreateExample("diagnostic-only", DesignConversionDpoUnits.ImproverUnit) with
        {
            NegativeReasonTaxonomy = DesignConversionNegativeReasonTaxonomy.TrivialBroken
        };
        var noReason = CreateExample("no-reason", DesignConversionDpoUnits.CriticUnit) with
        {
            NegativeQualityReason = ""
        };

        var result = generator.BuildTrainReadyRecords([unknown, diagnosticOnly, noReason]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Equal(3, result.Value.QuarantinedRecords.Count);
        Assert.Contains(result.Value.QuarantinedRecords, record => record.Quarantine.Reason.Contains("negative_reason_taxonomy_unknown", StringComparison.Ordinal));
        Assert.Contains(result.Value.QuarantinedRecords, record => record.Quarantine.Reason.Contains("negative_reason_taxonomy_quarantine_only:trivial_broken", StringComparison.Ordinal));
        Assert.Contains(result.Value.QuarantinedRecords, record => record.Quarantine.Reason.Contains("negative_quality_reason_required", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildTrainReadyRecords_QuarantinesForbiddenLearningLeakageTokens()
    {
        var generator = new DesignConversionDpoGenerator();
        var leaky = CreateExample("source-request-leakage", DesignConversionDpoUnits.CriticUnit) with
        {
            SourceStructurePayload = "{\"source_request_id_as_feature\":\"request-123\",\"tree\":[\"frame\"]}",
            NegativeReasonTaxonomy = DesignConversionNegativeReasonTaxonomy.LeakageShortcut,
            NegativeQualityReason = "source_request_id_as_feature would teach a leakage shortcut instead of visual quality."
        };

        var result = generator.BuildTrainReadyRecords([leaky]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        var record = Assert.Single(result.Value.QuarantinedRecords);
        Assert.True(record.Quarantine.IsQuarantined);
        Assert.Contains("hidden_deterministic_or_qa_leakage_field", record.Quarantine.Reason, StringComparison.Ordinal);
        Assert.Contains("negative_reason_taxonomy_quarantine_only:leakage_shortcut", record.Quarantine.Reason, StringComparison.Ordinal);
    }


    [Theory]
    [InlineData(DesignConversionNegativeReasonTaxonomy.Overflow,
        "Rejected is a realistic near-miss where content overflows the source frame while evidence still maps the layout.")]
    [InlineData(DesignConversionNegativeReasonTaxonomy.Responsive,
        "Rejected is a realistic near-miss where the layout fails the narrow viewport breakpoint while preserving the main structure.")]
    [InlineData(DesignConversionNegativeReasonTaxonomy.ZOrder,
        "Rejected is a realistic near-miss where the overlay order hides the primary action while the rest of the frame renders.")]
    public void BuildTrainReadyRecords_AcceptsGuideAlignedCanonicalNegativeTaxonomy(
        string taxonomy,
        string negativeQualityReason)
    {
        var generator = new DesignConversionDpoGenerator();
        var example = CreateExample($"canonical-{taxonomy}", DesignConversionDpoUnits.CriticUnit, usefulness: 1.12) with
        {
            NegativeReasonTaxonomy = taxonomy,
            NegativeQualityReason = negativeQualityReason,
            Chosen = CreateOutput($"canonical-{taxonomy}-chosen", accepted: true, score: 0.86, htmlSuffix: $"{taxonomy}-chosen"),
            Rejected = CreateOutput($"canonical-{taxonomy}-rejected", accepted: false, score: 0.34, htmlSuffix: $"{taxonomy}-near-miss")
        };

        var result = generator.BuildTrainReadyRecords([example]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.QuarantinedRecords);
        var record = Assert.Single(result.Value.AcceptedRecords);
        Assert.Equal(taxonomy, record.NegativeReasonTaxonomy);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.Equal("train_ready", record.Quarantine.Reason);
    }

    [Fact]
    public void BuildTrainReadyRecords_AcceptsCriticNearMissWhenRejectedSelfScoreIsHigher()
    {
        var generator = new DesignConversionDpoGenerator();
        var example = CreateExample("critic-score-inversion", DesignConversionDpoUnits.CriticUnit, usefulness: 1.30) with
        {
            Chosen = CreateOutput("critic-score-inversion-chosen", accepted: true, score: 0.72, htmlSuffix: "chosen"),
            Rejected = CreateOutput("critic-score-inversion-rejected", accepted: false, score: 0.91, htmlSuffix: "rejected"),
            NegativeQualityReason = "Rejected reports a higher self-score but misses the evidence-backed spacing defect."
        };

        var result = generator.BuildTrainReadyRecords([example]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.AcceptedRecords);
        Assert.Equal("critic-score-inversion", record.RecordId);
        Assert.Equal(1.30, record.UsefulnessScore);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.Equal(0.72, record.Chosen.Verdict.Score);
        Assert.Equal(0.91, record.Rejected.Verdict.Score);
    }

    [Fact]
    public void BuildTrainReadyRecords_QuarantinesImproverWhenRejectedVisualScoreIsNotLower()
    {
        var generator = new DesignConversionDpoGenerator();
        var example = CreateExample("improver-score-inversion", DesignConversionDpoUnits.ImproverUnit) with
        {
            Chosen = CreateOutput("improver-score-inversion-chosen", accepted: true, score: 0.72, htmlSuffix: "chosen"),
            Rejected = CreateOutput("improver-score-inversion-rejected", accepted: false, score: 0.91, htmlSuffix: "rejected")
        };

        var result = generator.BuildTrainReadyRecords([example]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.QuarantinedRecords);
        Assert.Contains("improver_rejected_quality_not_lower_than_chosen", record.Quarantine.Reason);
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_ArbitraryMetadataAndSingleEvaluationDoNotCreateTrainReadyRow()
    {
        var generator = new DesignConversionDpoGenerator();
        var evaluation = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true,
            visualDecisionMetadataOverride: CreateArbitraryRejectedCandidateMetadata());

        Assert.True(evaluation.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);

        var result = generator.BuildRuntimeCreatedTrainingCandidates(evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=server_owned_route_admission_not_bound",
            result.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
        Assert.DoesNotContain("runtime_rejected_independently_evaluated=true", result.Value.Warnings);
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_ServerOwnedSingleEvaluationCreatesReviewPendingCandidateOnly()
    {
        var generator = new DesignConversionDpoGenerator();
        var evaluation = CreateServerOwnedSingleEvaluation();

        var result = generator.BuildRuntimeCreatedTrainingCandidates(evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.Equal("runtime-candidate-review-pending", record.Quarantine.Reason);
        Assert.True(record.Chosen.Accepted);
        Assert.False(record.Rejected.Accepted);
        Assert.Null(record.TeacherProvenance);
        Assert.Equal(DesignConversionDpoUnits.ImproverUnit, record.UnitId);
        var metadata = record.Provenance.Metadata;
        Assert.Equal(bool.TrueString, metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.TrueString, metadata["server_owned_route_admission_tenant_bound"]);
        Assert.Equal(bool.TrueString, metadata["review_required"]);
        Assert.Equal("pending", metadata["approval_status"]);
        Assert.Equal(bool.FalseString, metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, metadata["training_candidate_only"]);
        Assert.Equal(bool.FalseString, metadata["training_grade"]);
        Assert.Equal(bool.TrueString, metadata["not_training_grade"]);
        Assert.Equal(bool.FalseString, metadata["production_dpo"]);
        Assert.Equal(bool.FalseString, metadata["model_readiness_claimed"]);
        Assert.Equal(bool.FalseString, metadata["independent_preference_proven"]);
        Assert.Equal(bool.TrueString, metadata["independent_preference_not_proven"]);
        Assert.Equal(bool.FalseString, metadata["runtime_rejected_independently_evaluated"]);
        Assert.Equal(bool.FalseString, metadata["chosen_rejected_pair_proven"]);
        Assert.Equal(bool.TrueString, metadata["chosen_rejected_pair_not_proven"]);
        Assert.Equal(bool.FalseString, metadata["high_quality_negative_pair_proven"]);
        Assert.Equal(bool.TrueString, metadata["high_quality_negative_pair_not_proven"]);
        Assert.Contains("runtime_created_dpo_candidate_review_pending", result.Value.Warnings);
        Assert.Contains("chosen_rejected_pair_proven=false", result.Value.Warnings);
        Assert.Contains("chosen_rejected_pair_not_proven", result.Value.Warnings);
        Assert.Contains("high_quality_negative_pair_proven=false", result.Value.Warnings);
        Assert.Contains("high_quality_negative_pair_not_proven", result.Value.Warnings);
        Assert.DoesNotContain("chosen_rejected_pair_proven", result.Value.Warnings);
        Assert.DoesNotContain("high_quality_negative_pair_proven", result.Value.Warnings);
        var blockReasons =
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(record);
        Assert.Contains("approval_missing", blockReasons);
        Assert.Contains("high_quality_negative_pair_not_proven", blockReasons);
        Assert.False(
            DesignConversionTrainingMeasurementGatePolicy
                .IsRecordAllowedForTrainingCandidate(record));
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_ServerOwnedSingleEvaluationAllowsVerifiedActiveModelRuntimeMarkers()
    {
        var generator = new DesignConversionDpoGenerator();
        var evaluation = CreateServerOwnedSingleEvaluation();
        var scaffoldMarkers = new Dictionary<string, string>(
            evaluation.ScaffoldMarkers,
            StringComparer.Ordinal);
        foreach (var pair in CreateVerifiedActiveModelRuntimeMarkers())
            scaffoldMarkers[pair.Key] = pair.Value;

        var result = generator.BuildRuntimeCreatedTrainingCandidates(
            evaluation with { ScaffoldMarkers = scaffoldMarkers });

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.AcceptedRecords);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.Equal(
            "runtime-candidate-review-pending",
            record.Quarantine.Reason);
        Assert.Equal(
            bool.TrueString,
            record.Provenance.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(
            bool.TrueString,
            record.Provenance.Metadata["review_required"]);
        Assert.Equal(
            bool.FalseString,
            record.Provenance.Metadata["approved_for_training"]);
        Assert.Contains(
            "runtime_created_dpo_candidate_review_pending",
            result.Value.Warnings);

        var unknownMarkers = new Dictionary<string, string>(
            scaffoldMarkers,
            StringComparer.Ordinal)
        {
            ["active_model_runtime_unreviewed_marker"] = "trusted"
        };
        var unknownMarkerResult = generator.BuildRuntimeCreatedTrainingCandidates(
            evaluation with { ScaffoldMarkers = unknownMarkers });
        Assert.True(unknownMarkerResult.IsSuccess, unknownMarkerResult.ErrorMessage);
        Assert.Empty(unknownMarkerResult.Value!.AcceptedRecords);
        Assert.Contains(
            unknownMarkerResult.Value.Warnings,
            warning => warning.Contains(
                "synthetic_or_scaffold_single_evaluation_not_allowed",
                StringComparison.Ordinal));

        var malformedMarkers = new Dictionary<string, string>(
            scaffoldMarkers,
            StringComparer.Ordinal)
        {
            ["active_model_checkpoint_sha256"] = "not-a-sha256"
        };
        var malformedMarkerResult = generator.BuildRuntimeCreatedTrainingCandidates(
            evaluation with { ScaffoldMarkers = malformedMarkers });
        Assert.True(malformedMarkerResult.IsSuccess, malformedMarkerResult.ErrorMessage);
        Assert.Empty(malformedMarkerResult.Value!.AcceptedRecords);
        Assert.Contains(
            malformedMarkerResult.Value.Warnings,
            warning => warning.Contains(
                "synthetic_or_scaffold_single_evaluation_not_allowed",
                StringComparison.Ordinal));
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_RejectedEvidenceUnavailableFallsBackToReviewPendingCandidateOnly()
    {
        var generator = new DesignConversionDpoGenerator();
        var chosenEvaluation = CreateServerOwnedSingleEvaluation();
        var rejectedEvaluation = CreateRuntimeEvaluation(
            outputId: "runtime-rejected-output",
            candidateId: "runtime-rejected-candidate",
            verdictId: "runtime-rejected-verdict",
            score: 0.64,
            passed: false,
            htmlSuffix: "rejected",
            includeRealEvidenceRefs: false);

        var result = generator.BuildRuntimeCreatedTrainingCandidates(
            chosenEvaluation,
            rejectedEvaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Null(record.TeacherProvenance);
        Assert.Equal(
            "runtime-candidate-review-pending",
            record.Quarantine.Reason);
        Assert.Equal(
            bool.TrueString,
            record.Provenance.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(
            bool.TrueString,
            record.Provenance.Metadata["review_required"]);
        Assert.Equal(
            bool.FalseString,
            record.Provenance.Metadata["approved_for_training"]);
        Assert.Equal(
            bool.FalseString,
            record.Provenance.Metadata["training_grade"]);
        var blockReasons =
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(record);
        Assert.Contains("approval_missing", blockReasons);
        Assert.Contains("high_quality_negative_pair_not_proven", blockReasons);
        Assert.False(
            DesignConversionTrainingMeasurementGatePolicy
                .IsRecordAllowedForTrainingCandidate(record));
        Assert.Contains(
            "runtime_rejected_candidate_pair_unavailable",
            result.Value.Warnings);
        Assert.Contains(
            "runtime_rejected_candidate_pair_unavailable_reason=runtime_rejected_candidate_evidence_refs_required",
            result.Value.Warnings);
        Assert.Contains(
            "runtime_created_dpo_candidate_review_pending",
            result.Value.Warnings);
        Assert.DoesNotContain("chosen_rejected_pair_proven", result.Value.Warnings);
        Assert.DoesNotContain("high_quality_negative_pair_proven", result.Value.Warnings);
    }

    [Theory]
    [InlineData(ServerOwnedSingleEvaluationMutation.MissingRouteBinding, "server_owned_route_admission_not_bound")]
    [InlineData(ServerOwnedSingleEvaluationMutation.MalformedRouteBinding, "route_admission_binding_malformed")]
    [InlineData(ServerOwnedSingleEvaluationMutation.MissingTrustedBrowserRender, "server_owned_browser_render_not_proven")]
    [InlineData(ServerOwnedSingleEvaluationMutation.MissingTrustedPngComparison, "server_owned_local_png_comparison_not_proven")]
    [InlineData(ServerOwnedSingleEvaluationMutation.MissingLocalVisualLabeler, "local_visual_labeler_boundary_not_proven")]
    [InlineData(ServerOwnedSingleEvaluationMutation.ArbitrarySingleEvaluation, "server_owned_browser_render_not_proven")]
    public void BuildRuntimeCreatedTrainingCandidates_ServerOwnedSingleEvaluationRequiresAllServerOwnedGates(
        ServerOwnedSingleEvaluationMutation mutation,
        string expectedReason)
    {
        var generator = new DesignConversionDpoGenerator();
        var evaluation = CreateServerOwnedSingleEvaluation(mutation);

        var result = generator.BuildRuntimeCreatedTrainingCandidates(evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        var reasonWarning = Assert.Single(
            result.Value.Warnings,
            warning => warning.StartsWith(
                "runtime_created_dpo_candidate_quarantine_reason=",
                StringComparison.Ordinal));
        Assert.Contains(expectedReason, reasonWarning, StringComparison.Ordinal);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
        Assert.DoesNotContain("runtime_created_dpo_candidate_review_pending", result.Value.Warnings);
        Assert.DoesNotContain("chosen_rejected_pair_proven", result.Value.Warnings);
        Assert.DoesNotContain("high_quality_negative_pair_proven", result.Value.Warnings);
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_SameEvaluationDoesNotCreateTrainReadyRow()
    {
        var generator = new DesignConversionDpoGenerator();
        var evaluation = CreateRuntimeEvaluation(canPersistTrainingCandidate: true);

        var result = generator.BuildRuntimeCreatedTrainingCandidates(evaluation, evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=runtime_rejected_candidate_identity_must_be_distinct",
            result.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_ScaffoldMarkedRejectedEvaluationDoesNotCreateTrainReadyRow()
    {
        var generator = new DesignConversionDpoGenerator();
        var chosen = CreateRuntimeEvaluation(canPersistTrainingCandidate: true);
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-loser-job",
            requestId: "runtime-loser-request",
            outputId: "runtime-loser-output",
            candidateId: "runtime-loser-candidate",
            verdictId: "runtime-loser-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "loser",
            scaffoldMarkers: new Dictionary<string, string> { ["scaffold_negative"] = bool.TrueString });

        var result = generator.BuildRuntimeCreatedTrainingCandidates(chosen, rejected);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=runtime_rejected_candidate_synthetic_or_scaffold",
            result.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_OperationalServiceMarkersRemainPairEligibleButFailClosedWithoutTeacher()
    {
        var generator = new DesignConversionDpoGenerator();
        var chosen = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true,
            scaffoldMarkers: CreateOperationalServiceMarkers());
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-operational-marker-loser-job",
            requestId: "runtime-operational-marker-loser-request",
            outputId: "runtime-operational-marker-loser-output",
            candidateId: "runtime-operational-marker-loser-candidate",
            verdictId: "runtime-operational-marker-loser-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "operational-marker-loser",
            scaffoldMarkers: CreateOperationalServiceMarkers());

        var result = generator.BuildRuntimeCreatedTrainingCandidates(chosen, rejected);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(generator.CanBuildRuntimeCreatedTrainingCandidatePair(chosen, rejected).Value);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=runtime_teacher_attestation_required",
            result.Value.Warnings);
        Assert.Contains("chosen_rejected_pair_proven", result.Value.Warnings);
        Assert.Contains("high_quality_negative_pair_proven", result.Value.Warnings);
        Assert.Contains("runtime_teacher_attestation_verified=false", result.Value.Warnings);
        Assert.Contains("training_candidate_only=false", result.Value.Warnings);
        Assert.DoesNotContain("training_candidate_only", result.Value.Warnings);
        Assert.DoesNotContain("training_candidate_only=true", result.Value.Warnings);
        Assert.DoesNotContain(
            "runtime_created_dpo_candidate_not_created_insufficient_evidence",
            result.Value.Warnings);
        Assert.DoesNotContain(
            "chosen_rejected_pair_or_negative_quality_not_proven",
            result.Value.Warnings);
        Assert.DoesNotContain(
            "high_quality_negative_pair_not_proven",
            result.Value.Warnings);
    }

    [Theory]
    [InlineData("unrecognized_provider_marker", "true")]
    [InlineData("render_provider_injected", "fake")]
    [InlineData("render_provider_injected", "placeholder")]
    [InlineData("render_provider_injected", "stand_in")]
    [InlineData("browser_renderer_injected_provider_kind", "UnreviewedRenderProvider")]
    [InlineData("synthetic_negative", "true")]
    [InlineData("scaffold_negative", "true")]
    public void BuildRuntimeCreatedTrainingCandidates_NonOperationalRejectedMarkerIsRejected(
        string markerKey,
        string markerValue)
    {
        var generator = new DesignConversionDpoGenerator();
        var chosen = CreateRuntimeEvaluation(canPersistTrainingCandidate: true);
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-non-operational-marker-loser-job",
            requestId: "runtime-non-operational-marker-loser-request",
            outputId: "runtime-non-operational-marker-loser-output",
            candidateId: "runtime-non-operational-marker-loser-candidate",
            verdictId: "runtime-non-operational-marker-loser-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "non-operational-marker-loser",
            scaffoldMarkers: new Dictionary<string, string> { [markerKey] = markerValue });

        var result = generator.BuildRuntimeCreatedTrainingCandidates(chosen, rejected);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=runtime_rejected_candidate_synthetic_or_scaffold",
            result.Value.Warnings);
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_RejectedEvaluationWithoutRealEvidenceRefsDoesNotCreateTrainReadyRow()
    {
        var generator = new DesignConversionDpoGenerator();
        var chosen = CreateRuntimeEvaluation(canPersistTrainingCandidate: true);
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-loser-job",
            requestId: "runtime-loser-request",
            outputId: "runtime-loser-output",
            candidateId: "runtime-loser-candidate",
            verdictId: "runtime-loser-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "loser",
            includeRealEvidenceRefs: false);

        var result = generator.BuildRuntimeCreatedTrainingCandidates(chosen, rejected);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=runtime_rejected_candidate_evidence_refs_required",
            result.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
    }

    [Fact]
    public async Task BuildRuntimeCreatedTrainingCandidates_AttestationOnlyOverloadRemainsNonAuthoritative()
    {
        var generator = new DesignConversionDpoGenerator();
        var chosen = CreateRuntimeEvaluation(canPersistTrainingCandidate: true);
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-loser-job",
            requestId: "runtime-loser-request",
            outputId: "runtime-loser-output",
            candidateId: "runtime-loser-candidate",
            verdictId: "runtime-loser-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "loser");

        using var teacher = new FakeTeacherInferenceClient(BuildValidTeacherOutput);
        using var runtime = CreateTeacherRuntime(teacher);
        var attestation = await runtime.AttestAsync(
            chosen,
            rejected,
            CreateTeacherCallReservation());
        Assert.True(attestation.IsSuccess, attestation.ErrorMessage);
        var result = generator.BuildRuntimeCreatedTrainingCandidates(
            chosen,
            rejected,
            attestation.Value!);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Equal(1, teacher.InvocationCount);
        Assert.Contains(
            "runtime_teacher_attestation_only_is_non_authoritative",
            result.Value.Warnings);
    }

    [Fact]
    public async Task BuildRuntimeCreatedTrainingCandidates_AttestationFallbackCannotCreateRecord()
    {
        const string privateBoundaryPath = "C:/Users/lubao/private-qa/raw-capture.png";
        var boundaryWarnings = new[]
        {
            "DETERMINISTIC_RENDER_SCAFFOLD_DOES_NOT_CLAIM_VISUAL_FIDELITY",
            $"BROWSER_RENDER_PROOF_RECEIPT_REQUIRED raw_probe={privateBoundaryPath}",
            "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT",
            "SERVER_OWNED_SCAFFOLD_RENDER_PROVENANCE_CANONICALIZED",
            "SERVER_OWNED_BROWSER_RENDER_PROVENANCE_CANONICALIZED",
            "BROWSER_RENDER_PROOF_RECEIPT_REQUIRED",
            $"PRIVATE_CHROMIUM_BROWSER_CAPTURE raw_probe={privateBoundaryPath}",
            $"BROWSER_CAPTURE_RESOLUTION_REQUIRED_PRIVATE_SUFFIX raw_probe={privateBoundaryPath}",
            $"SERVER_OWNED_BROWSER_RENDER_PROVENANCE_CANONICALIZED_EXTRA raw_probe={privateBoundaryPath}",
            "browser_render_proof_receipt_required",
            "unrelated_private_warning C:/Users/lubao/private-qa/not-boundary.txt",
            "DETERMINISTIC_RENDER_SCAFFOLD_NOT_BROWSER_SCREENSHOT"
        };
        var generator = new DesignConversionDpoGenerator();
        var chosen = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: true,
            warnings: boundaryWarnings);
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-warning-loser-job",
            requestId: "runtime-warning-loser-request",
            outputId: "runtime-warning-loser-output",
            candidateId: "runtime-warning-loser-candidate",
            verdictId: "runtime-warning-loser-verdict",
            score: 0.46,
            passed: false,
            htmlSuffix: "warning-loser",
            warnings: boundaryWarnings);

        Assert.Empty(chosen.QaResults);
        Assert.Single(chosen.ComparisonArtifacts);
        Assert.Empty(rejected.QaResults);
        Assert.Single(rejected.ComparisonArtifacts);

        using var teacher = new FakeTeacherInferenceClient(BuildValidTeacherOutput);
        using var runtime = CreateTeacherRuntime(teacher);
        var attestation = await runtime.AttestAsync(
            chosen,
            rejected,
            CreateTeacherCallReservation());
        Assert.True(attestation.IsSuccess, attestation.ErrorMessage);

        var result = generator.BuildRuntimeCreatedTrainingCandidates(
            chosen,
            rejected,
            attestation.Value!);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains(
            "runtime_teacher_attestation_only_is_non_authoritative",
            result.Value.Warnings);
    }

    [Fact]
    public async Task TeacherAttestation_UsesCanonicalRefsOnlyPromptAndStrictlyRejectsMalformedOutputs()
    {
        var chosen = CreateRuntimeEvaluation(canPersistTrainingCandidate: true);
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-strict-loser-job",
            requestId: "runtime-strict-loser-request",
            outputId: "runtime-strict-loser-output",
            candidateId: "runtime-strict-loser-candidate",
            verdictId: "runtime-strict-loser-verdict",
            score: 0.41,
            passed: false,
            htmlSuffix: "strict-loser");
        var prompt = DesignConversionDpoTeacherAttestationRuntime
            .BuildCanonicalPromptForTesting(chosen, rejected);

        Assert.True(prompt.IsSuccess, prompt.ErrorMessage);
        Assert.DoesNotContain(chosen.ImprovedHtml, prompt.Value!, StringComparison.Ordinal);
        Assert.DoesNotContain(chosen.ImprovedCss, prompt.Value!, StringComparison.Ordinal);
        Assert.DoesNotContain(rejected.ImprovedHtml, prompt.Value!, StringComparison.Ordinal);
        Assert.DoesNotContain(rejected.ImprovedCss, prompt.Value!, StringComparison.Ordinal);
        Assert.Contains("attest_measured_preference_only", prompt.Value!, StringComparison.Ordinal);

        using var teacher = new FakeTeacherInferenceClient(BuildValidTeacherOutput);
        using var runtime = CreateTeacherRuntime(teacher);
        var accepted = await runtime.AttestAsync(
            chosen,
            rejected,
            CreateTeacherCallReservation());
        Assert.True(accepted.IsSuccess, accepted.ErrorMessage);
        Assert.StartsWith("sha256:", accepted.Value!.OutputHash, StringComparison.Ordinal);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
            accepted.Value.InferenceProfileId);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Authoring.ConfigurationSha256,
            accepted.Value.InferenceProfileConfigurationSha256);
        Assert.Equal(
            LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId,
            accepted.Value.Metadata["teacher_inference_profile_id"]);
        Assert.Contains(
            $"teacher-inference-profile:id={LocalQwenOllamaInferenceProfileAuthority.Authoring.ProfileId}",
            accepted.Value.EvidenceRefs);
        Assert.Contains(
            $"teacher-inference-profile:configuration_sha256={LocalQwenOllamaInferenceProfileAuthority.Authoring.ConfigurationSha256}",
            accepted.Value.EvidenceRefs);
        Assert.Equal(1, teacher.InvocationCount);

        var valid = teacher.LastOutput!;
        var chosenRef = DesignConversionDpoTeacherAttestationRuntime.BuildOutputRef(
            chosen.RuntimeFlowResult.Output);
        var rejectedRef = DesignConversionDpoTeacherAttestationRuntime.BuildOutputRef(
            rejected.RuntimeFlowResult.Output);
        var invalid = new[]
        {
            "```json\n" + valid + "\n```",
            valid.Replace("{", "{\"unknown\":true,", StringComparison.Ordinal),
            valid.Replace("\"schema\":", "\"schema\":\"duplicate\",\"schema\":", StringComparison.Ordinal),
            valid.Replace($",\"chosen_ref\":\"{chosenRef}\"", string.Empty, StringComparison.Ordinal),
            valid.Replace(chosenRef, rejectedRef, StringComparison.Ordinal),
            valid.Replace(
                $"\"preferred_ref\":\"{chosenRef}\"",
                $"\"preferred_ref\":\"{rejectedRef}\"",
                StringComparison.Ordinal),
            valid.Replace("\"supports_preference\":true", "\"supports_preference\":false", StringComparison.Ordinal),
            new string('x', DesignConversionDpoTeacherAttestationRuntime.MaximumOutputUtf8Bytes + 1),
            "{\"schema\":{\"a\":{\"b\":{\"c\":{\"d\":{\"e\":{\"f\":{\"g\":{\"h\":1}}}}}}}}}"
        };
        Assert.All(invalid, value =>
        {
            var parsed = DesignConversionDpoTeacherAttestationRuntime
                .ParseOutputForTesting(value, chosen, rejected);
            Assert.False(parsed.IsSuccess);
        });
    }

    [Fact]
    public async Task TeacherAttestation_CancellationAndDisposalHaveNoInferenceSideEffects()
    {
        var chosen = CreateRuntimeEvaluation(canPersistTrainingCandidate: true);
        var rejected = CreateRuntimeEvaluation(
            canPersistTrainingCandidate: false,
            jobId: "runtime-lifetime-loser-job",
            requestId: "runtime-lifetime-loser-request",
            outputId: "runtime-lifetime-loser-output",
            candidateId: "runtime-lifetime-loser-candidate",
            verdictId: "runtime-lifetime-loser-verdict",
            score: 0.4,
            passed: false,
            htmlSuffix: "lifetime-loser");
        var teacher = new FakeTeacherInferenceClient(BuildValidTeacherOutput);
        var runtime = CreateTeacherRuntime(teacher);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => runtime.AttestAsync(
                chosen,
                rejected,
                CreateTeacherCallReservation(),
                cancelled.Token));
        Assert.Equal(0, teacher.InvocationCount);

        runtime.Dispose();
        var disposed = await runtime.AttestAsync(
            chosen,
            rejected,
            CreateTeacherCallReservation());
        Assert.False(disposed.IsSuccess);
        Assert.Equal(
            DesignConversionDpoTeacherAttestationRuntime.RuntimeUnavailableErrorCode,
            disposed.ErrorCode);
        Assert.Equal(0, teacher.InvocationCount);
        Assert.Equal(1, teacher.DisposeCount);
    }

    [Fact]
    public void BuildRuntimeCreatedTrainingCandidates_CanPersistFalseDoesNotCreateCandidate()
    {
        var generator = new DesignConversionDpoGenerator();
        var evaluation = CreateRuntimeEvaluation(canPersistTrainingCandidate: false);

        Assert.False(evaluation.VisualQaBoundaryTrace.VisualDecision.CanPersistTrainingCandidate);

        var result = generator.BuildRuntimeCreatedTrainingCandidates(evaluation);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Empty(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.Contains(
            "runtime_created_dpo_candidate_not_created_visual_decision_can_persist_training_candidate_false",
            result.Value.Warnings);
        Assert.Contains(
            "runtime_created_dpo_candidate_quarantine_reason=visual_decision_can_persist_training_candidate_false",
            result.Value.Warnings);
        Assert.Contains("runtime_created_dpo_candidate=false", result.Value.Warnings);
        Assert.Contains("training_candidate_only=false", result.Value.Warnings);
        Assert.DoesNotContain("training_candidate_only", result.Value.Warnings);
        Assert.DoesNotContain("training_candidate_only=true", result.Value.Warnings);
    }

    [Fact]
    public void BuildTrainReadyRecords_OrdersAcceptedPairsByUsefulness()
    {
        var generator = new DesignConversionDpoGenerator();

        var result = generator.BuildTrainReadyRecords(
        [
            CreateExample("medium", DesignConversionDpoUnits.CriticUnit, usefulness: 0.60),
            CreateExample("high", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
            CreateExample("low", DesignConversionDpoUnits.CriticUnit, usefulness: 0.30)
        ]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(["high", "medium", "low"], result.Value!.AcceptedRecords.Select(record => record.RecordId).ToArray());
    }

    public static DesignConversionStoredDpoExample CreateExample(
        string exampleId,
        string unitId,
        double usefulness = 0.8)
        => new(
            ExampleId: exampleId,
            UnitId: unitId,
            InternalUnitStatePayload: "{\"schema\":\"design-conversion-unit-state-v1\",\"focus\":\"layout\"}",
            InternalVectorPayload: [0.12, 0.34, 0.56],
            SourceStructurePayload: "{\"tree\":[\"frame\",\"button\"],\"redacted\":true}",
            Chosen: CreateOutput($"{exampleId}-chosen", accepted: true, score: 0.88, htmlSuffix: "chosen"),
            Rejected: CreateOutput($"{exampleId}-rejected", accepted: false, score: 0.31, htmlSuffix: "rejected"),
            TieBreakerPayload: "{\"reason\":\"chosen preserves spacing and hierarchy\"}",
            UsefulnessScore: usefulness,
            NegativeQualityReason: "Rejected is a realistic near-miss with evidence-backed spacing defects.",
            NegativeReasonTaxonomy: DesignConversionNegativeReasonTaxonomy.Spacing,
            Provenance: new DesignConversionDpoProvenance(
                SourceRequestId: "design-request-redacted",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/source-redacted",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string> { ["split"] = "train" }),
            License: CreateLicense(allowsTraining: true, containsPrivateData: false),
            TeacherEvidenceRefs: ["teacher-evidence:redacted"],
            Metadata: new Dictionary<string, string> { ["source_quality"] = "teacher-reviewed" },
            TeacherProvenance: TrainingTeacherProvenance.Deterministic($"design-conversion-test:{exampleId}"));

    public static DesignConversionImprovementOutput CreateOutput(
        string id,
        bool accepted,
        double score,
        string htmlSuffix,
        bool includeMarkup = true)
        => new(
            OutputId: id,
            CandidateId: $"candidate-{id}",
            Html: includeMarkup ? $"<main class=\"card {htmlSuffix}\">Hello</main>" : "",
            Css: includeMarkup ? $".card.{htmlSuffix}{{display:grid;gap:{(accepted ? 12 : 2)}px;}}" : "",
            Verdict: CreateVerdict($"{id}-verdict", score, accepted),
            QaResults: [],
            EvidenceRefs: [$"qa-teacher:{id}"],
            Accepted: accepted);

    public static DesignConversionLicenseScope CreateLicense(bool allowsTraining, bool containsPrivateData)
        => new(
            LicenseId: "project-local-redacted",
            RightsScope: "project-local",
            AllowsTraining: allowsTraining,
            ContainsUserPrivateData: containsPrivateData,
            Metadata: new Dictionary<string, string>());

    internal static DesignConversionInternalEvaluationResult CreateRuntimeEvaluation(
        bool canPersistTrainingCandidate = true,
        IReadOnlyDictionary<string, string>? visualDecisionMetadataOverride = null,
        string jobId = "runtime-job",
        string requestId = "runtime-request",
        string outputId = "runtime-output",
        string candidateId = "runtime-candidate",
        string verdictId = "runtime-output-verdict",
        double score = 0.82,
        bool passed = true,
        string htmlSuffix = "improved",
        IReadOnlyDictionary<string, string>? scaffoldMarkers = null,
        IReadOnlyList<string>? warnings = null,
        bool includeRealEvidenceRefs = true,
        IReadOnlyDictionary<string, string>? requestProvenanceOverride = null,
        bool trustedServerOwnedBrowserRender = false,
        bool trustedServerOwnedLocalPngComparison = false,
        bool localVisualLabeler = false)
    {
        var visualDecisionMetadata = visualDecisionMetadataOverride is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(visualDecisionMetadataOverride);
        IReadOnlyList<string> evaluationWarnings = warnings ?? ["product_not_ready"];
        var renderImageMetadata = trustedServerOwnedBrowserRender
            ? TrustedBrowserRenderMetadata()
            : new Dictionary<string, string>();
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
        var expected = new RenderQaImageRecord(
            "expected-image",
            "expected",
            resolution,
            "artifact://expected",
            "expected-sha",
            new Dictionary<string, string>());
        var actual = expected with
        {
            ImageId = "actual-image",
            Label = "actual",
            Uri = "artifact://actual",
            Sha256 = "actual-sha",
            Metadata = renderImageMetadata
        };
        var requestProvenance = requestProvenanceOverride is null
            ? new Dictionary<string, string> { ["source_manifest_ref"] = "manifest://runtime-source" }
            : new Dictionary<string, string>(requestProvenanceOverride);
        var request = new SubmittedDesignConversionRequest(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            Html: "<main class=\"source\">Source</main>",
            Css: ".source{display:grid;gap:4px;}",
            DesignTreePayload: new DesignTreePayload(
                "design-tree",
                "json",
                "{\"node\":\"frame\"}",
                "design-tree-sha",
                new Dictionary<string, string>()),
            OriginalTreePayload: new DesignTreePayload(
                "original-tree",
                "json",
                "{\"node\":\"frame\"}",
                "original-tree-sha",
                new Dictionary<string, string>()),
            Screenshots:
            [
                new DesignRenderArtifact(
                    "source-screenshot",
                    "screenshot",
                    1440,
                    900,
                    "artifact://source-screenshot",
                    "source-screenshot-sha",
                    new Dictionary<string, string>())
            ],
            DesignRenders: [],
            Budget: new DesignConversionBudgetMetadata(
                "user-redacted",
                "free",
                1,
                1,
                0m,
                new Dictionary<string, string>()),
            Provenance: requestProvenance);
        var trustedLocalPngEvidence = trustedServerOwnedLocalPngComparison
            ? new[]
            {
                ServerOwnedLocalPngComparisonProvider.ProviderKind,
                ServerOwnedLocalPngComparisonProvider.Boundary
            }
            : Array.Empty<string>();
        var createdVerdict = CreateVerdict(verdictId, score, passed, includeRealEvidenceRefs);
        var verdict = createdVerdict with
        {
            EvidenceRefs = includeRealEvidenceRefs
                ? createdVerdict.EvidenceRefs
                    .Concat(trustedLocalPngEvidence)
                    .ToArray()
                : trustedLocalPngEvidence
        };
        var output = new DesignConversionImprovementOutput(
            outputId,
            candidateId,
            $"<main class=\"{htmlSuffix}\">Source</main>",
            $".{htmlSuffix}{{display:grid;gap:{(passed ? 16 : 1)}px;}}",
            verdict,
            QaResults: [],
            EvidenceRefs: includeRealEvidenceRefs ? [$"{outputId}:evidence"] : [],
            Accepted: passed);
        var candidate = new DesignConversionImprovementCandidate(
            candidateId,
            request.RequestId,
            output.Html,
            output.Css,
            ChangeReasons: ["improved_spacing"],
            EvidenceRefs: includeRealEvidenceRefs ? ["runtime-candidate:evidence"] : [],
            Metadata: new Dictionary<string, string>());
        var runtimeFlow = new DesignConversionRuntimeFlowResult(
            request,
            new DesignConversionCriticUnitOutput(
                "critic-output",
                verdict,
                QaEvidenceRefs: includeRealEvidenceRefs ? ["qa:evidence"] : [],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            new DesignConversionImproverUnitOutput(
                "improver-output",
                candidate,
                output,
                QaEvidenceRefs: includeRealEvidenceRefs ? ["qa:evidence"] : [],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            candidate,
            output,
            UnitExecutionOrder: [DesignConversionDpoUnits.CriticUnit, DesignConversionDpoUnits.ImproverUnit],
            EvidenceRefs: includeRealEvidenceRefs ? ["runtime-flow:evidence"] : [],
            DeterministicLogicClaimedAsLearning: false);
        var render = new DesignConversionRenderResult(
            $"{outputId}-render-result",
            $"{outputId}-render-request",
            actual,
            ElapsedMs: 12,
            Warnings: []);
        var comparison = new DesignConversionQaComparisonArtifact(
            $"{outputId}-comparison-artifact",
            $"{outputId}-comparison-request",
            expected,
            actual,
            verdict,
            "artifact://diff",
            "diff-sha",
            new Dictionary<string, double>
            {
                ["pixel_delta"] = 0.08,
                ["visual_comparison_proven"] =
                    trustedServerOwnedLocalPngComparison ? 1.0 : 0.0
            },
            EvidenceRefs: includeRealEvidenceRefs
                ? ["comparison:evidence", .. trustedLocalPngEvidence]
                : trustedLocalPngEvidence)
        {
            Metadata = trustedServerOwnedLocalPngComparison
                ? TrustedServerOwnedLocalPngComparisonMetadata()
                : new Dictionary<string, string>()
        };
        var visualJudgeMetadata = localVisualLabeler
            ? new Dictionary<string, string>
            {
                [DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker] =
                    bool.TrueString
            }
            : new Dictionary<string, string>();
        var visualJudgeEvidenceRefs = includeRealEvidenceRefs
            ? new List<string> { "judge:evidence" }
            : [];
        if (localVisualLabeler)
            visualJudgeEvidenceRefs.Add(
                DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker);
        var judge = new DesignConversionVisualJudgeResult(
            $"{outputId}-judge-result",
            $"{outputId}-judge-request",
            localVisualLabeler
                ? DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker
                : "scaffold",
            false,
            verdict,
            new DesignConversionVisualJudgeProviderMetadata(
                localVisualLabeler
                    ? DesignConversionVisualQaBoundaryPolicy.LocalVisualLabelerMarker
                    : "local-scaffold",
                "localhost",
                "none",
                false,
                visualJudgeMetadata),
            0m,
            new Dictionary<string, double>(),
            visualJudgeEvidenceRefs,
            visualJudgeMetadata);
        var boundary = new DesignConversionVisualQaBoundaryTrace(
            new DesignConversionViewportCoverage(
                IsMultiResolution: false,
                IsComplete: true,
                RequiredViewportIds: ["desktop"],
                ObservedViewportIds: ["desktop"],
                MissingViewportIds: [],
                EvidenceRefs: includeRealEvidenceRefs ? ["viewport:evidence"] : []),
            VisualJudgeStatus: "local-scaffold",
            VisualJudgeProviderBoundary: "external_provider_not_configured",
            ExternalAiVisualJudgeExecuted: false,
            TierPolicy: new DesignConversionTierPolicyDecision(
                "free",
                RequiredScore: 0.8,
                BestScore: score,
                EstimatedCostUsd: 0m,
                BudgetUsd: 0m,
                RemainingBudgetUsd: 0m,
                AttemptCount: 1,
                MaxImprovementCandidates: 1,
                StopReason: "quality_met",
                ShouldContinue: false,
                EvidenceRefs: includeRealEvidenceRefs ? ["tier:evidence"] : [],
                Metadata: new Dictionary<string, string>()),
            VisualDecision: new DesignConversionVisualComparisonDecision(
                "accept",
                QualityScore: score,
                RequiredThreshold: 0.8,
                EstimatedCostUsd: 0m,
                BudgetUsd: 0m,
                RemainingBudgetUsd: 0m,
                CostBudgetMet: true,
                QualityThresholdMet: true,
                CanRetry: false,
                CanPersistTrainingCandidate: canPersistTrainingCandidate,
                ProviderBoundaryStatus: "external_provider_not_configured",
                ExternalAiVisualJudgeExecuted: false,
                CorrectionFindings: [],
                EvidenceRefs: includeRealEvidenceRefs ? ["visual-decision:evidence"] : [],
                Metadata: visualDecisionMetadata),
            ProductReady: false,
            TrainedFeatureClaimed: false,
            EvidenceRefs: includeRealEvidenceRefs ? ["boundary:evidence"] : [],
            Warnings: evaluationWarnings);

        return new DesignConversionInternalEvaluationResult(
            jobId,
            request,
            RenderResults: includeRealEvidenceRefs ? [render] : [],
            ComparisonArtifacts: [comparison],
            VisualJudgeResults: [judge],
            QaResults: [],
            RuntimeFlowResult: runtimeFlow,
            Verdict: verdict,
            ImprovedHtml: output.Html,
            ImprovedCss: output.Css,
            EvidenceRefs: includeRealEvidenceRefs ? ["runtime:evidence"] : [],
            Warnings: evaluationWarnings,
            ScaffoldMarkers: scaffoldMarkers ?? new Dictionary<string, string>(),
            DeterministicLogicClaimedAsLearning: false,
            boundary);
    }

    internal static DesignConversionInternalEvaluationResult
        CreateServerOwnedSingleEvaluation(
            ServerOwnedSingleEvaluationMutation mutation =
                ServerOwnedSingleEvaluationMutation.None)
        => mutation == ServerOwnedSingleEvaluationMutation.ArbitrarySingleEvaluation
            ? CreateRuntimeEvaluation(
                canPersistTrainingCandidate: true,
                visualDecisionMetadataOverride:
                    CreateArbitraryRejectedCandidateMetadata(),
                requestProvenanceOverride:
                    CreateServerOwnedRouteAdmissionProvenance())
            : CreateRuntimeEvaluation(
                canPersistTrainingCandidate: true,
                scaffoldMarkers: CreateOperationalServiceMarkers(),
                requestProvenanceOverride:
                    mutation == ServerOwnedSingleEvaluationMutation.MissingRouteBinding
                        ? null
                        : mutation ==
                            ServerOwnedSingleEvaluationMutation.MalformedRouteBinding
                            ? CreateMalformedRouteAdmissionProvenance()
                            : CreateServerOwnedRouteAdmissionProvenance(),
                trustedServerOwnedBrowserRender:
                    mutation !=
                    ServerOwnedSingleEvaluationMutation.MissingTrustedBrowserRender,
                trustedServerOwnedLocalPngComparison:
                    mutation !=
                    ServerOwnedSingleEvaluationMutation.MissingTrustedPngComparison,
                localVisualLabeler:
                    mutation !=
                    ServerOwnedSingleEvaluationMutation.MissingLocalVisualLabeler);

    private static IReadOnlyDictionary<string, string>
        CreateServerOwnedRouteAdmissionProvenance()
    {
        var tenantIdRef =
            DesignConversionModelActivationRegistry.BuildTenantIdRef(
                "runtime-server-owned-candidate-tenant");
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["source_manifest_ref"] = "manifest://runtime-source",
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                bool.TrueString,
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionTenantIdRefMetadataKey] =
                tenantIdRef,
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                BuildRouteAdmissionScopeRef(
                    "design-conversion-live-route\n" + tenantIdRef)
        };
    }

    private static IReadOnlyDictionary<string, string>
        CreateMalformedRouteAdmissionProvenance()
    {
        var provenance = new Dictionary<string, string>(
            CreateServerOwnedRouteAdmissionProvenance(),
            StringComparer.Ordinal)
        {
            [DesignConversionThinClientJobRuntime
                .ServerOwnedRouteAdmissionScopeRefMetadataKey] =
                "sha256:design-conversion-route-admission-scope:not64"
        };
        return provenance;
    }

    private static string BuildRouteAdmissionScopeRef(string source)
        => "sha256:design-conversion-route-admission-scope:" +
           Convert.ToHexString(
                   System.Security.Cryptography.SHA256.HashData(
                       System.Text.Encoding.UTF8.GetBytes(source)))
               .ToLowerInvariant();

    private static string BuildScopedSha256Ref(string scope, string source)
        => "sha256:" + scope + ":" +
           Convert.ToHexString(
                   System.Security.Cryptography.SHA256.HashData(
                       System.Text.Encoding.UTF8.GetBytes(source)))
               .ToLowerInvariant();

    private static Dictionary<string, string> TrustedBrowserRenderMetadata()
        => new(StringComparer.Ordinal)
        {
            [DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey] =
                bool.TrueString,
            [DesignConversionRenderEvidenceTrustBoundary
                .ApprovalEvidenceAllowedKey] =
                bool.TrueString,
            [DesignConversionRenderEvidenceTrustBoundary.TrustClassificationKey] =
                DesignConversionRenderEvidenceTrustBoundary
                    .BrowserBackedClassification,
            ["provider_kind"] =
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
            ["browser_or_playwright_used"] = bool.TrueString
        };

    private static Dictionary<string, string>
        TrustedServerOwnedLocalPngComparisonMetadata()
        => new(StringComparer.Ordinal)
        {
            [DesignConversionRenderEvidenceTrustBoundary.ProductionTrustedKey] =
                bool.TrueString,
            [DesignConversionRenderEvidenceTrustBoundary
                .ApprovalEvidenceAllowedKey] =
                bool.TrueString,
            [DesignConversionComparisonEvidenceTrustBoundary
                .TrustClassificationKey] =
                DesignConversionComparisonEvidenceTrustBoundary
                    .ServerOwnedLocalPngClassification,
            [DesignConversionComparisonEvidenceTrustBoundary
                .ServerCanonicalizedKey] =
                bool.TrueString,
            ["provider_kind"] = ServerOwnedLocalPngComparisonProvider.ProviderKind,
            ["comparison_provider_kind"] =
                ServerOwnedLocalPngComparisonProvider.ProviderKind,
            ["comparison_boundary"] = ServerOwnedLocalPngComparisonProvider.Boundary,
            ["hidden_determinism_boundary"] =
                ServerOwnedLocalPngComparisonProvider.Boundary,
            ["server_owned_local_png_comparison_trust_verified"] =
                bool.TrueString,
            ["local_png_pixel_comparison"] = bool.TrueString,
            ["visual_comparison_proven"] = bool.TrueString,
            ["expected_sha256_verified"] = bool.TrueString,
            ["rendered_sha256_verified"] = bool.TrueString,
            ["diff_sha256_verified"] = bool.TrueString,
            ["diff_png_signature_verified"] = bool.TrueString,
            ["diff_png_nonblank_pixels_verified"] = bool.TrueString,
            ["comparison_diff_readback_sha256_verified"] = bool.TrueString
        };

    public enum ServerOwnedSingleEvaluationMutation
    {
        None,
        MissingRouteBinding,
        MalformedRouteBinding,
        MissingTrustedBrowserRender,
        MissingTrustedPngComparison,
        MissingLocalVisualLabeler,
        ArbitrarySingleEvaluation
    }

    private static Dictionary<string, string> CreateOperationalServiceMarkers()
        => new()
        {
            ["service_first_no_asp_net_host"] = bool.TrueString,
            ["render_provider_injected"] = bool.TrueString,
            ["comparison_provider_injected"] = bool.TrueString,
            ["visual_judge_provider_injected"] = bool.TrueString,
            ["browser_renderer_provider_available"] = bool.TrueString,
            ["browser_renderer_explicit_local_injection_proven"] = bool.TrueString,
            ["browser_renderer_default_production_wired"] = bool.FalseString,
            ["browser_renderer_default_scope"] =
                "explicit_local_injection_only_not_production_default",
            ["browser_renderer_injected_provider_kind"] =
                BrowserBackedDesignConversionRenderProvider.ProviderKind,
            ["external_ai_visual_judge_provider_available"] = bool.TrueString,
            ["external_ai_visual_judge_configured"] = bool.TrueString,
            ["local_visual_judge_default_wired"] = bool.FalseString,
            ["local_visual_judge_explicitly_injected"] = bool.FalseString,
            ["visual_judge_not_configured_fallback"] = bool.FalseString,
            ["persistent_storage_implemented"] = bool.FalseString,
            ["deterministic_logic_claimed_as_learning"] = bool.FalseString
        };

    private static IReadOnlyDictionary<string, string>
        CreateVerifiedActiveModelRuntimeMarkers()
    {
        var tenantIdRef = DesignConversionModelActivationRegistry
            .BuildTenantIdRef("runtime-server-owned-candidate-tenant");
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["active_model_runtime_selection"] = "durable_registry",
            ["active_model_runtime_selection_boundary"] =
                "design_conversion_modelserver_runtime_selection_reads_durable_active_model_and_fresh_loads_checkpoint",
            ["active_model_runtime_tenant_context_source"] =
                "server_owned_route_admission",
            ["active_model_runtime_trusted_tenant_bound"] = bool.TrueString,
            ["active_model_runtime_caller_tenant_metadata_authoritative"] =
                bool.FalseString,
            ["active_model_runtime_fresh_load_attempted"] = bool.TrueString,
            ["active_model_runtime_fresh_load_succeeded"] = bool.TrueString,
            ["active_model_runtime_previous_preserved"] = bool.FalseString,
            ["active_model_generation"] = "1",
            ["active_model_tenant_id_ref"] = tenantIdRef,
            ["active_model_admitted_tenant_id_ref"] = tenantIdRef,
            ["active_model_admitted_scope_ref"] =
                BuildRouteAdmissionScopeRef(
                    "design-conversion-live-route\n" + tenantIdRef),
            ["active_model_id"] = "ready-json-loopback-active-manifest:0123456789ab",
            ["active_model_manifest_id"] =
                "ready-json-loopback-active-manifest",
            ["active_model_version"] = "design-conversion-v1",
            ["active_model_checkpoint_sha256"] = new string('a', 64),
            ["active_model_checkpoint_path_ref"] = BuildScopedSha256Ref(
                "design-conversion-model-activation-checkpoint-path",
                "verified-active-model-checkpoint-path"),
            ["active_model_checkpoint_weights_source"] =
                "local_checkpoint_feature_weights",
            ["active_model_checkpoint_fresh_load_verified"] = bool.TrueString,
            ["active_model_checkpoint_real_weights"] = bool.TrueString,
            ["active_model_local_only"] = bool.TrueString,
            ["product_ready"] = bool.FalseString
        };
    }

    private static Dictionary<string, string> CreateArbitraryRejectedCandidateMetadata()
        => new()
        {
            ["runtime_rejected_independently_evaluated"] = bool.TrueString,
            ["runtime_rejected_synthetic"] = bool.FalseString,
            ["runtime_rejected_output_id"] = "runtime-loser-output",
            ["runtime_rejected_candidate_id"] = "runtime-loser-candidate",
            ["runtime_rejected_html_ref"] = "dcm-runtime-rejected-html-measured-ref",
            ["runtime_rejected_css_ref"] = "dcm-runtime-rejected-css-measured-ref",
            ["runtime_rejected_verdict_id"] = "runtime-loser-verdict",
            ["runtime_rejected_visual_score"] = "0.46",
            ["runtime_rejected_negative_reason_taxonomy"] = DesignConversionNegativeReasonTaxonomy.Spacing,
            ["runtime_rejected_negative_quality_reason"] =
                "Measured rejected candidate has lower judge score and evidence-backed spacing defects.",
            ["runtime_rejected_defect_category"] = "spacing",
            ["runtime_rejected_defect_severity"] = "major",
            ["runtime_rejected_evidence_refs"] = "runtime-loser-render;runtime-loser-judge",
            ["runtime_rejected_evidence_source"] = "runtime_visual_judge_metadata"
        };

    private static VisualVerdict CreateVerdict(
        string verdictId,
        double score,
        bool passed,
        bool includeEvidenceRefs = true)
        => new(
            VerdictId: verdictId,
            Score: score,
            Defects:
            [
                new VisualDefect(
                    DefectId: $"{verdictId}:spacing",
                    Category: "spacing",
                    Severity: passed ? "info" : "major",
                    Description: "Teacher evidence compares visual spacing.",
                    X: 20,
                    Y: 40,
                    Width: 100,
                    Height: 30,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: includeEvidenceRefs ? [$"visual-verdict:{verdictId}"] : [],
            Passed: passed);

    private static DesignConversionDpoTeacherCallReservation
        CreateTeacherCallReservation()
    {
        var reservation = DesignConversionDpoTeacherCallReservation.Create(
            configuredMaximumCalls: 1,
            observedCallCount: 0);
        Assert.True(reservation.IsSuccess, reservation.ErrorMessage);
        return reservation.Value!;
    }

    private static DesignConversionDpoTeacherAttestationRuntime
        CreateTeacherRuntime(ITrainingTeacherInferenceClient teacher)
        => new(
            teacher,
            ExpectedTeacherModelId,
            ExpectedTeacherManifestDigest,
            ExpectedTeacherModelLayerDigest);

    private static string BuildValidTeacherOutput(string prompt)
    {
        using var document = System.Text.Json.JsonDocument.Parse(prompt);
        var root = document.RootElement;
        var chosenRef = root.GetProperty("chosen").GetProperty("ref").GetString()!;
        var rejectedRef = root.GetProperty("rejected").GetProperty("ref").GetString()!;
        var evidenceRef = root.GetProperty("evidence_refs")[0].GetString()!;
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            schema = DesignConversionDpoTeacherAttestationRuntime.Schema,
            chosen_ref = chosenRef,
            rejected_ref = rejectedRef,
            preferred_ref = chosenRef,
            supports_preference = true,
            evidence_refs = new[] { evidenceRef }
        });
    }

    private sealed class FakeTeacherInferenceClient(
        Func<string, string> outputFactory) : ITrainingTeacherInferenceClient
    {
        public int InvocationCount { get; private set; }
        public int DisposeCount { get; private set; }
        public string? LastPrompt { get; private set; }
        public string? LastOutput { get; private set; }

        public Task<AgentInfrastructure.OperationResult<TrainingTeacherInferenceResult>>
            InferAsync(
                string prompt,
                ITrainingTeacherCallPermit callPermit,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var issued = callPermit.TryIssue();
            if (!issued.IsSuccess)
            {
                return Task.FromResult(
                    issued.MapError<TrainingTeacherInferenceResult>());
            }
            InvocationCount++;
            LastPrompt = prompt;
            LastOutput = outputFactory(prompt);
            return Task.FromResult(
                AgentInfrastructure.OperationResult<TrainingTeacherInferenceResult>.Ok(
                    new TrainingTeacherInferenceResult(
                        LastOutput,
                        new TrainingTeacherProvenance(
                            TrainingTeacherSourceKind.ExternalTeacherOutput,
                            OpenWeightTeacherFamily.Qwen,
                            TrainingTeacherAccessMode.LocalHostedOpenWeight,
                            TrainingTeacherHostingProvider.Ollama,
                            ExpectedTeacherModelId,
                            ExpectedTeacherManifestDigest,
                            TrainingTeacherModelLicense.Apache20,
                            ProviderTermsChecked: false,
                            ProviderTermsAllowTraining: false,
                            ClosedProprietaryModel: false,
                            TeacherOutputUsedForTraining: true,
                            RequestId: "teacher-request-test",
                            InvocationCount: 1)
                        {
                            ModelArtifactDigestKind =
                                LegalTrainingTeacherAdmissionAuthority.OllamaModelLayerSha256DigestKind,
                            ModelArtifactDigest = ExpectedTeacherModelLayerDigest
                        })
                    {
                        InferenceProfile =
                            LocalQwenOllamaInferenceProfileAuthority.Authoring
                    }));
        }

        public void Dispose() => DisposeCount++;
    }
}
