// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;

public sealed class DesignConversionDpoGeneratorTests
{
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
    public void BuildTrainReadyRecords_SourceEncoderRedactsForbiddenLeakageBeforeTrainReadyRecord()
    {
        var generator = new DesignConversionDpoGenerator();
        var rawScreenshotBytes = new string('A', 180);
        var leaky = CreateExample(
            "C:/Users/private-user/private-project/example-001",
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
                  "raw_path":"C:/Users/private-user/Documents/private.fig",
                  "candidate_index":2,
                  "raw_screenshot_bytes":"{{rawScreenshotBytes}}",
                  "breakpoints":[390,1440]
                }
                """,
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "request-from-user-luba-exact-prompt",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "file:///C:/Users/private-user/Documents/private.fig",
                ReviewerRef: "reviewer:user-luba",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["source_design_id"] = "C:/Users/private-user/Documents/private.fig",
                    ["candidate_order"] = "chosen-first"
                }),
            Chosen = CreateOutput(
                "chosen-private-output",
                accepted: true,
                score: 0.91,
                htmlSuffix: "chosen") with
            {
                CandidateId = "C:/Users/private-user/private-candidate-a.html",
                Html = "<img src=\"C:/Users/private-user/private.png\"><main class=\"card chosen\">Hello</main>"
            },
            Rejected = CreateOutput(
                "rejected-private-output",
                accepted: false,
                score: 0.35,
                htmlSuffix: "rejected") with
            {
                CandidateId = "C:/Users/private-user/private-candidate-b.html"
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
        Assert.DoesNotContain("C:/Users/", trainReadySurface, StringComparison.OrdinalIgnoreCase);
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
                  "file_path":"C:/Users/private-user/private/source.fig",
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
        Assert.DoesNotContain("C:/Users/", serialized, StringComparison.OrdinalIgnoreCase);
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
    public void BuildRuntimeCreatedTrainingCandidates_KeepsSyntheticRuntimeNegativeInReviewQueue()
    {
        var generator = new DesignConversionDpoGenerator();

        var result = generator.BuildRuntimeCreatedTrainingCandidates(CreateRuntimeEvaluation());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var record = Assert.Single(result.Value!.AcceptedRecords);
        Assert.Empty(result.Value.QuarantinedRecords);
        Assert.False(record.Quarantine.IsQuarantined);
        Assert.Equal("train_ready", record.Quarantine.Reason);
        Assert.Equal(DesignConversionNegativeReasonTaxonomy.SyntheticRuntimeNotTrainReady, record.NegativeReasonTaxonomy);
        Assert.Contains("synthetic_runtime_not_train_ready", record.NegativeQualityReason, StringComparison.Ordinal);
        Assert.Contains("not_training_grade", record.NegativeQualityReason, StringComparison.Ordinal);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["synthetic_negative"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["scaffold_negative"]);
        Assert.Equal(bool.FalseString, record.Provenance.Metadata["training_grade"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["not_training_grade"]);
        Assert.Equal("synthetic_runtime_scaffold_not_training_grade", record.Provenance.Metadata["training_grade_boundary"]);
        Assert.Equal(bool.FalseString, record.Provenance.Metadata["approved_for_training"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["training_candidate_only"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["product_not_ready"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["source_disjoint_not_proven"]);
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
            Metadata: new Dictionary<string, string> { ["source_quality"] = "teacher-reviewed" });

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

    private static DesignConversionInternalEvaluationResult CreateRuntimeEvaluation()
    {
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
            Sha256 = "actual-sha"
        };
        var request = new SubmittedDesignConversionRequest(
            RequestId: "runtime-request",
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
            Provenance: new Dictionary<string, string>
            {
                ["source_manifest_ref"] = "manifest://runtime-source"
            });
        var verdict = CreateVerdict("runtime-output-verdict", 0.82, passed: true);
        var output = new DesignConversionImprovementOutput(
            "runtime-output",
            "runtime-candidate",
            "<main class=\"improved\">Source</main>",
            ".improved{display:grid;gap:16px;}",
            verdict,
            QaResults: [],
            EvidenceRefs: ["runtime-output:evidence"],
            Accepted: true);
        var candidate = new DesignConversionImprovementCandidate(
            "runtime-candidate",
            request.RequestId,
            output.Html,
            output.Css,
            ChangeReasons: ["improved_spacing"],
            EvidenceRefs: ["runtime-candidate:evidence"],
            Metadata: new Dictionary<string, string>());
        var runtimeFlow = new DesignConversionRuntimeFlowResult(
            request,
            new DesignConversionCriticUnitOutput(
                "critic-output",
                verdict,
                QaEvidenceRefs: ["qa:evidence"],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            new DesignConversionImproverUnitOutput(
                "improver-output",
                candidate,
                output,
                QaEvidenceRefs: ["qa:evidence"],
                AiTeacherEvidenceRefs: [],
                DeterministicLogicClaimedAsLearning: false),
            candidate,
            output,
            UnitExecutionOrder: [DesignConversionDpoUnits.CriticUnit, DesignConversionDpoUnits.ImproverUnit],
            EvidenceRefs: ["runtime-flow:evidence"],
            DeterministicLogicClaimedAsLearning: false);
        var render = new DesignConversionRenderResult(
            "render-result",
            "render-request",
            actual,
            ElapsedMs: 12,
            Warnings: []);
        var comparison = new DesignConversionQaComparisonArtifact(
            "comparison-artifact",
            "comparison-request",
            expected,
            actual,
            verdict,
            "artifact://diff",
            "diff-sha",
            new Dictionary<string, double> { ["pixel_delta"] = 0.08 },
            EvidenceRefs: ["comparison:evidence"]);
        var judge = new DesignConversionVisualJudgeResult(
            "judge-result",
            "judge-request",
            "scaffold",
            false,
            verdict,
            new DesignConversionVisualJudgeProviderMetadata(
                "local-scaffold",
                "localhost",
                "none",
                false,
                new Dictionary<string, string>()),
            0m,
            new Dictionary<string, double>(),
            ["judge:evidence"],
            new Dictionary<string, string>());
        var boundary = new DesignConversionVisualQaBoundaryTrace(
            new DesignConversionViewportCoverage(
                IsMultiResolution: false,
                IsComplete: true,
                RequiredViewportIds: ["desktop"],
                ObservedViewportIds: ["desktop"],
                MissingViewportIds: [],
                EvidenceRefs: ["viewport:evidence"]),
            VisualJudgeStatus: "local-scaffold",
            VisualJudgeProviderBoundary: "external_provider_not_configured",
            ExternalAiVisualJudgeExecuted: false,
            TierPolicy: new DesignConversionTierPolicyDecision(
                "free",
                RequiredScore: 0.8,
                BestScore: 0.82,
                EstimatedCostUsd: 0m,
                BudgetUsd: 0m,
                RemainingBudgetUsd: 0m,
                AttemptCount: 1,
                MaxImprovementCandidates: 1,
                StopReason: "quality_met",
                ShouldContinue: false,
                EvidenceRefs: ["tier:evidence"],
                Metadata: new Dictionary<string, string>()),
            VisualDecision: new DesignConversionVisualComparisonDecision(
                "accept",
                QualityScore: 0.82,
                RequiredThreshold: 0.8,
                EstimatedCostUsd: 0m,
                BudgetUsd: 0m,
                RemainingBudgetUsd: 0m,
                CostBudgetMet: true,
                QualityThresholdMet: true,
                CanRetry: false,
                CanPersistTrainingCandidate: true,
                ProviderBoundaryStatus: "external_provider_not_configured",
                ExternalAiVisualJudgeExecuted: false,
                CorrectionFindings: [],
                EvidenceRefs: ["visual-decision:evidence"],
                Metadata: new Dictionary<string, string>()),
            ProductReady: false,
            TrainedFeatureClaimed: false,
            EvidenceRefs: ["boundary:evidence"],
            Warnings: ["product_not_ready"]);

        return new DesignConversionInternalEvaluationResult(
            "runtime-job",
            request,
            RenderResults: [render],
            ComparisonArtifacts: [comparison],
            VisualJudgeResults: [judge],
            QaResults: [],
            RuntimeFlowResult: runtimeFlow,
            Verdict: verdict,
            ImprovedHtml: output.Html,
            ImprovedCss: output.Css,
            EvidenceRefs: ["runtime:evidence"],
            Warnings: ["product_not_ready"],
            ScaffoldMarkers: new Dictionary<string, string>(),
            DeterministicLogicClaimedAsLearning: false,
            boundary);
    }

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
                    Description: "Teacher evidence compares visual spacing.",
                    X: 20,
                    Y: 40,
                    Width: 100,
                    Height: 30,
                    EvidenceImageIds: ["expected", "actual"])
            ],
            EvidenceRefs: [$"visual-verdict:{verdictId}"],
            Passed: passed);
}
