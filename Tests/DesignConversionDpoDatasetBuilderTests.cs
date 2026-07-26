// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text.Json;
using DesignConversionDomain;

public sealed class DesignConversionDpoDatasetBuilderTests
{
    [Fact]
    public void BuildDataset_SourceDisjointSplitKeepsSameSourceOutOfBothSplits()
    {
        var build = BuildRecords(
        [
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("source-a-1", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95), "source-a"),
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("source-a-2", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.85), "source-a"),
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("source-b-1", DesignConversionDpoUnits.CriticUnit, usefulness: 0.75), "source-b"),
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("source-c-1", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.65), "source-c")
        ]);

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            build,
            LocalFixtureOptions(
                DatasetId: "dcm-dpo-dataset-source-disjoint-test",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.NotEmpty(dataset.Value!.TrainRows);
        Assert.NotEmpty(dataset.Value.HeldOutRows);
        Assert.True(dataset.Value.Manifest.CountGate.SourceDisjoint);
        Assert.True(dataset.Value.Manifest.CountGate.Passed);

        var trainSources = dataset.Value.TrainRows
            .Select(row => row.SourceIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(dataset.Value.HeldOutRows, row => Assert.DoesNotContain(row.SourceIdentityHash, trainSources));
        Assert.All(dataset.Value.Rows.GroupBy(row => row.SourceIdentityHash),
            group => Assert.Single(group.Select(row => row.Split).Distinct(StringComparer.Ordinal)));
        Assert.All(dataset.Value.Rows, row => Assert.StartsWith("dcm-source-", row.SourceGroupId, StringComparison.Ordinal));
        Assert.All(dataset.Value.Manifest.TrainSources.Concat(dataset.Value.Manifest.HeldOutSources),
            source => Assert.StartsWith("dcm-source-", source.SourceGroupId, StringComparison.Ordinal));
    }

    [Fact]
    public void BuildDataset_CanonicalizesSourceIdentityAcrossSourceKinds()
    {
        const string sharedDesignId = "shared-design-source-kind-collision";
        var build = BuildRecords(
        [
            WithSourceAndKind(
                DesignConversionDpoGeneratorTests.CreateExample("shared-figma", DesignConversionDpoUnits.CriticUnit, usefulness: 0.95),
                sharedDesignId,
                "figma"),
            WithSourceAndKind(
                DesignConversionDpoGeneratorTests.CreateExample("shared-html", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.9),
                sharedDesignId,
                "other"),
            WithSourceAndKind(
                DesignConversionDpoGeneratorTests.CreateExample("other-source-a", DesignConversionDpoUnits.CriticUnit, usefulness: 0.8),
                "other-source-a",
                "figma"),
            WithSourceAndKind(
                DesignConversionDpoGeneratorTests.CreateExample("other-source-b", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.7),
                "other-source-b",
                "other")
        ]);

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            build,
            LocalFixtureOptions(
                DatasetId: "dcm-dpo-dataset-source-kind-collision-test",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        var sharedRows = dataset.Value!.Rows
            .Where(row => row.SourceKind is "figma" or "other")
            .Where(row => row.UnitId is DesignConversionDpoUnits.CriticUnit or DesignConversionDpoUnits.ImproverUnit)
            .Where(row => row.UsefulnessScore is 0.95 or 0.9)
            .ToArray();

        Assert.Equal(2, sharedRows.Length);
        Assert.Single(sharedRows.Select(row => row.SourceIdentityHash).Distinct(StringComparer.Ordinal));
        Assert.Single(sharedRows.Select(row => row.SourceGroupId).Distinct(StringComparer.Ordinal));
        Assert.Single(sharedRows.Select(row => row.Split).Distinct(StringComparer.Ordinal));
        Assert.All(sharedRows, row => Assert.StartsWith("dcm-source-", row.SourceGroupId, StringComparison.Ordinal));

        var trainSources = dataset.Value.TrainRows
            .Select(row => row.SourceIdentityHash)
            .ToHashSet(StringComparer.Ordinal);
        Assert.All(dataset.Value.HeldOutRows, row => Assert.DoesNotContain(row.SourceIdentityHash, trainSources));
    }

    [Fact]
    public void BuildDataset_ExcludesQuarantinedInvalidNegativePairsFromDatasetRows()
    {
        var invalidNegative = DesignConversionDpoGeneratorTests.CreateExample(
            "invalid-negative",
            DesignConversionDpoUnits.CriticUnit,
            usefulness: 1.1) with
        {
            NegativeReasonTaxonomy = DesignConversionNegativeReasonTaxonomy.TrivialBroken
        };
        var build = BuildRecords(
        [
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("valid", DesignConversionDpoUnits.CriticUnit), "source-valid"),
            WithSource(invalidNegative, "source-invalid")
        ]);

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            build,
            LocalFixtureOptions(
                DatasetId: "dcm-dpo-dataset-quarantine-test",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Single(dataset.Value!.Rows);
        Assert.Equal(1, dataset.Value.Manifest.QuarantinedRecordCount);
        Assert.Equal(1, dataset.Value.Manifest.ExcludedRecordCount);
        var excluded = Assert.Single(dataset.Value.Manifest.ExcludedRecords);
        Assert.Contains("negative_reason_taxonomy_quarantine_only:trivial_broken", excluded.Reason, StringComparison.Ordinal);
        Assert.DoesNotContain(dataset.Value.Rows, row => row.RecordIdentityHash == excluded.RecordIdentityHash);
    }

    [Fact]
    public void BuildDataset_CountGateReportsInsufficientDataWithoutFakingFiftyPlusFifty()
    {
        var build = BuildRecords(
        [
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("small-1", DesignConversionDpoUnits.CriticUnit), "small-source-1"),
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("small-2", DesignConversionDpoUnits.CriticUnit), "small-source-2"),
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("small-3", DesignConversionDpoUnits.ImproverUnit), "small-source-3")
        ]);

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(build, LocalFixtureOptions());

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.False(dataset.Value!.Manifest.CountGate.Passed);
        Assert.Equal(50, dataset.Value.Manifest.CountGate.RequiredTrainRecords);
        Assert.Equal(50, dataset.Value.Manifest.CountGate.RequiredHeldOutRecords);
        Assert.Equal(3, dataset.Value.Manifest.CountGate.ActualTrainRecords + dataset.Value.Manifest.CountGate.ActualHeldOutRecords);
        Assert.Contains(dataset.Value.Manifest.CountGate.Reasons,
            reason => reason.StartsWith("TRAIN_RECORD_COUNT_BELOW_TARGET:", StringComparison.Ordinal));
        Assert.Contains(dataset.Value.Manifest.CountGate.Reasons,
            reason => reason.StartsWith("HELD_OUT_RECORD_COUNT_BELOW_TARGET:", StringComparison.Ordinal));
        Assert.Contains("DPO_DATASET_COUNT_GATE_NOT_PASSED", dataset.Value.Manifest.EvidenceRefs);
    }

    [Fact]
    public void ToJsonlLines_IncludesRequiredMetadataWithoutRawSourceOrCandidateLeakage()
    {
        var leakyExample = DesignConversionDpoGeneratorTests.CreateExample(
            "C:/Users/private-user/private-project/example-001",
            DesignConversionDpoUnits.ImproverUnit,
            usefulness: 0.88) with
        {
            SourceStructurePayload = "{\"tree\":\"normalized\",\"raw_path\":\"C:/Users/private-user/Documents/private.fig\"}",
            InternalUnitStatePayload = "{\"exact_prompt\":\"make the private button blue\"}",
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "request-from-user-luba-exact-prompt",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "file:///C:/Users/private-user/Documents/private.fig",
                ReviewerRef: "reviewer:user-luba",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["source_design_id"] = "C:/Users/private-user/Documents/private.fig"
                }),
            License = new DesignConversionLicenseScope(
                LicenseId: "license-user-luba-private",
                RightsScope: "project-local",
                AllowsTraining: true,
                ContainsUserPrivateData: false,
                Metadata: new Dictionary<string, string>()),
            Chosen = DesignConversionDpoGeneratorTests.CreateOutput(
                "chosen-private-output",
                accepted: true,
                score: 0.91,
                htmlSuffix: "chosen") with
            {
                CandidateId = "C:/Users/private-user/private-candidate-a.html"
            },
            Rejected = DesignConversionDpoGeneratorTests.CreateOutput(
                "rejected-private-output",
                accepted: false,
                score: 0.35,
                htmlSuffix: "rejected") with
            {
                CandidateId = "C:/Users/private-user/private-candidate-b.html"
            }
        };
        var build = BuildRecords([leakyExample]);

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            build,
            LocalFixtureOptions(
                DatasetId: "dcm-dpo-dataset-jsonl-test",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        var line = Assert.Single(dataset.Value!.ToJsonlLines());

        Assert.DoesNotContain(Environment.NewLine, line, StringComparison.Ordinal);
        Assert.Contains("\"unitId\"", line, StringComparison.Ordinal);
        Assert.Contains("\"domainTag\"", line, StringComparison.Ordinal);
        Assert.Contains("\"sourceIdentityHash\"", line, StringComparison.Ordinal);
        Assert.Contains("\"sourceGroupId\"", line, StringComparison.Ordinal);
        Assert.Contains("\"result\"", line, StringComparison.Ordinal);
        Assert.Contains("\"candidate_result\"", line, StringComparison.Ordinal);
        Assert.Contains("\"teacher_labels\"", line, StringComparison.Ordinal);
        Assert.Contains("\"teaching_point\"", line, StringComparison.Ordinal);
        Assert.Contains("\"curriculum_tier\"", line, StringComparison.Ordinal);
        Assert.Contains("\"validation_outcome\"", line, StringComparison.Ordinal);
        Assert.Contains("\"source_encoder\":\"DesignConversionDefaultSourceEncoder\"", line, StringComparison.Ordinal);
        Assert.Contains("\"anti_leakage\":\"forbidden_source_fields_redacted_before_train_ready_payload\"", line, StringComparison.Ordinal);
        Assert.Contains("\"trained_feature_claimed\":\"False\"", line, StringComparison.Ordinal);
        Assert.Contains("\"training_grade\":\"False\"", line, StringComparison.Ordinal);
        Assert.Contains("\"not_training_grade\":\"True\"", line, StringComparison.Ordinal);
        Assert.Contains("\"scaffold_negative\":\"True\"", line, StringComparison.Ordinal);
        Assert.Contains("\"product_ready_verified\":\"False\"", line, StringComparison.Ordinal);
        Assert.Contains("\"raw_source_request_id_included\":\"False\"", line, StringComparison.Ordinal);
        Assert.DoesNotContain("request-from-user-luba-exact-prompt", line, StringComparison.Ordinal);
        Assert.DoesNotContain("reviewer:user-luba", line, StringComparison.Ordinal);
        Assert.DoesNotContain("license-user-luba-private", line, StringComparison.Ordinal);
        Assert.DoesNotContain("private-candidate-a", line, StringComparison.Ordinal);
        Assert.DoesNotContain("private-candidate-b", line, StringComparison.Ordinal);
        Assert.DoesNotContain("C:/Users/", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("make the private button blue", line, StringComparison.Ordinal);
        Assert.DoesNotContain("exact_prompt", line, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_path", line, StringComparison.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        Assert.StartsWith("dcm-source-", root.GetProperty("sourceGroupId").GetString(), StringComparison.Ordinal);
        using var promptDocument = JsonDocument.Parse(root.GetProperty("prompt").GetString()!);
        var prompt = promptDocument.RootElement;
        Assert.Equal(DesignConversionDpoUnits.ImproverUnit, prompt.GetProperty("target").GetProperty("unit_id").GetString());
        Assert.Equal("design_conversion_improver_unit", prompt.GetProperty("target").GetProperty("machine_part").GetString());
        Assert.Equal("figma", prompt.GetProperty("source").GetProperty("kind").GetString());
        Assert.Contains("normalized", prompt.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.True(prompt.GetProperty("internal_vector_dimensions").GetInt32() > 0);
        Assert.StartsWith("dcm-result-", prompt.GetProperty("result_features").GetProperty("chosen_result_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("dcm-result-", prompt.GetProperty("result_features").GetProperty("rejected_result_ref").GetString(), StringComparison.Ordinal);
        Assert.Equal("result_features", prompt.GetProperty("result_features").GetProperty("namespace").GetString());
        Assert.False(prompt.GetProperty("result_features").GetProperty("source_authority").GetBoolean());
        Assert.Equal(
            "outcome_label_aggregate_not_source_authority",
            prompt.GetProperty("result_features").GetProperty("role").GetString());
        Assert.True(prompt.GetProperty("result_features").GetProperty("visual_score_margin").GetDouble() > 0);
        Assert.Equal(DesignConversionNegativeReasonTaxonomy.Spacing, prompt.GetProperty("result_features").GetProperty("pair_family").GetString());
        Assert.Equal("DesignConversionDefaultSourceEncoder", prompt.GetProperty("source_encoder").GetProperty("name").GetString());
        Assert.Equal("dcm-source-encoder-v1", prompt.GetProperty("source_encoder").GetProperty("schema").GetString());
        Assert.Equal(
            "target_source_structure_provenance_only",
            root.GetProperty("metadata").GetProperty("source_encoder_scope").GetString());
        Assert.Equal(
            "result_outcome_label_aggregate_not_source_encoder",
            root.GetProperty("metadata").GetProperty("result_features_scope").GetString());
        Assert.Equal(
            "result_feature_not_source_encoder_feature",
            root.GetProperty("metadata").GetProperty("visual_score_margin_role").GetString());
        Assert.Equal(bool.FalseString, root.GetProperty("metadata").GetProperty("source_encoder_result_features_in_vector").GetString());
        Assert.Equal(bool.FalseString, root.GetProperty("metadata").GetProperty("source_encoder_visual_score_margin_in_vector").GetString());
        Assert.Equal(bool.FalseString, root.GetProperty("metadata").GetProperty("source_encoder_usefulness_score_in_vector").GetString());
        Assert.Equal(bool.FalseString, root.GetProperty("metadata").GetProperty("source_encoder_answer_payload_in_vector").GetString());
        var candidateResult = root.GetProperty("candidate_result");
        Assert.StartsWith("dcm-result-", candidateResult.GetProperty("chosen").GetProperty("result_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("dcm-result-", candidateResult.GetProperty("rejected").GetProperty("result_ref").GetString(), StringComparison.Ordinal);
        Assert.True(candidateResult.GetProperty("chosen").GetProperty("accepted").GetBoolean());
        Assert.False(candidateResult.GetProperty("rejected").GetProperty("accepted").GetBoolean());

        var teacherLabels = root.GetProperty("teacher_labels");
        Assert.Equal("chosen_over_rejected", teacherLabels.GetProperty("preference_label").GetString());
        Assert.StartsWith("dcm-result-", teacherLabels.GetProperty("preferred_result_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("dcm-result-", teacherLabels.GetProperty("rejected_result_ref").GetString(), StringComparison.Ordinal);
        Assert.True(teacherLabels.GetProperty("score_margin").GetDouble() > 0);

        Assert.Equal("tier_1_high_confidence", root.GetProperty("curriculum_tier").GetString());
        Assert.Contains("prefer_accepted_design_over_rejected_design", root.GetProperty("teaching_point").GetString(), StringComparison.Ordinal);
        var validationOutcome = root.GetProperty("validation_outcome");
        Assert.True(validationOutcome.GetProperty("accepted_for_training").GetBoolean());
        Assert.False(validationOutcome.GetProperty("training_grade").GetBoolean());
        Assert.True(validationOutcome.GetProperty("not_training_grade").GetBoolean());
        Assert.Equal(
            "training_candidate_or_local_scaffold_not_training_grade",
            validationOutcome.GetProperty("training_grade_boundary").GetString());
        Assert.False(validationOutcome.GetProperty("quarantined").GetBoolean());
        Assert.True(validationOutcome.GetProperty("license_allows_training").GetBoolean());
        Assert.False(validationOutcome.GetProperty("contains_user_private_data").GetBoolean());
        Assert.True(validationOutcome.GetProperty("unit_id_present").GetBoolean());
        Assert.Equal("accepted_training_candidate_not_training_grade", validationOutcome.GetProperty("outcome").GetString());
        Assert.Equal("training_candidate_only", dataset.Value.Manifest.Preflight.RouteClassifier);
        Assert.False(dataset.Value.Manifest.Preflight.ProductReady);
        Assert.Contains("approval_preflight", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("source_manifest", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("source_disjoint", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("training_candidate_only", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("product_not_ready", dataset.Value.Manifest.EvidenceRefs);
        Assert.Equal("passed", root.GetProperty("metadata").GetProperty("approval_preflight").GetString());
        Assert.Equal("passed", root.GetProperty("metadata").GetProperty("source_manifest").GetString());
        Assert.Equal("declared", root.GetProperty("metadata").GetProperty("source_disjoint").GetString());
        Assert.Equal(bool.TrueString, root.GetProperty("metadata").GetProperty("training_candidate_only").GetString());
        Assert.Equal(bool.TrueString, root.GetProperty("metadata").GetProperty("product_not_ready").GetString());
    }

    [Fact]
    public void BuildDataset_MissingApprovalPreflightBlocksRows()
    {
        var build = BuildRecords(
        [
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("approval-missing", DesignConversionDpoUnits.CriticUnit), "approval-missing")
        ]);

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            build,
            new DesignConversionDpoDatasetOptions(
                DatasetId: "dcm-dpo-dataset-approval-missing-test",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.MissingApproval()));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Empty(dataset.Value!.TrainRows);
        Assert.Empty(dataset.Value.HeldOutRows);
        Assert.Equal("approval_preflight_missing", dataset.Value.Manifest.Preflight.RouteClassifier);
        Assert.False(dataset.Value.Manifest.Preflight.AllowsDatasetRowConstruction);
        Assert.Contains("approval_preflight_missing", dataset.Value.Manifest.CountGate.Reasons);
        Assert.Contains("DPO_DATASET_APPROVAL_PREFLIGHT_NOT_PASSED", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("product_not_ready", dataset.Value.Manifest.EvidenceRefs);
    }

    [Fact]
    public void BuildDataset_MissingSourceManifestPreflightBlocksRowsAfterApproval()
    {
        var build = BuildRecords(
        [
            WithSource(DesignConversionDpoGeneratorTests.CreateExample("source-manifest-missing", DesignConversionDpoUnits.CriticUnit), "source-manifest-missing")
        ]);

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            build,
            new DesignConversionDpoDatasetOptions(
                DatasetId: "dcm-dpo-dataset-source-manifest-missing-test",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1,
                Preflight: DesignConversionDpoDatasetPreflight.MissingSourceManifestAfterApproval()));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Empty(dataset.Value!.Rows);
        Assert.True(dataset.Value.Manifest.Preflight.ApprovalPreflightPassed);
        Assert.False(dataset.Value.Manifest.Preflight.SourceManifestPreflightPassed);
        Assert.Equal("source_manifest_preflight_missing", dataset.Value.Manifest.Preflight.RouteClassifier);
        Assert.Contains("source_manifest_preflight_missing", dataset.Value.Manifest.CountGate.Reasons);
        Assert.Contains("DPO_DATASET_SOURCE_MANIFEST_PREFLIGHT_NOT_PASSED", dataset.Value.Manifest.EvidenceRefs);
    }

    private static DesignConversionDpoBuildResult BuildRecords(IReadOnlyList<DesignConversionStoredDpoExample> examples)
    {
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords(examples);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return build.Value!;
    }

    private static DesignConversionStoredDpoExample WithSource(
        DesignConversionStoredDpoExample example,
        string sourceId)
        => WithSourceAndKind(example, sourceId, example.Provenance.SourceKind);

    private static DesignConversionStoredDpoExample WithSourceAndKind(
        DesignConversionStoredDpoExample example,
        string sourceId,
        string sourceKind)
        => example with
        {
            Provenance = example.Provenance with
            {
                SourceKind = sourceKind,
                SourceRequestId = $"request:{sourceId}",
                SourceManifestRef = $"manifest://dcm-source/{sourceId}",
                Metadata = new Dictionary<string, string>
                {
                    ["source_design_id"] = sourceId
                }
            }
        };

    private static DesignConversionDpoDatasetOptions LocalFixtureOptions(
        string DatasetId = "dcm-dpo-dataset",
        int MinimumTrainRecords = 50,
        int MinimumHeldOutRecords = 50)
        => new(
            DatasetId: DatasetId,
            MinimumTrainRecords: MinimumTrainRecords,
            MinimumHeldOutRecords: MinimumHeldOutRecords,
            Preflight: DesignConversionDpoDatasetPreflight.LocalFixtureTrainingCandidateOnly());
}
