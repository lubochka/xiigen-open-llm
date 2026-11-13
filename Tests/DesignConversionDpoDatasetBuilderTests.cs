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
    public void ToJsonlLines_SeparatesTrainOnlyOptimizerPayloadFromAuditMetadata()
    {
        const string chosenHtml = "<main data-optimizer-side=\"alpha\">alpha-render-content</main>";
        const string chosenCss = "[data-optimizer-side=alpha]{display:grid;gap:17px}";
        const string rejectedHtml = "<main data-optimizer-side=\"beta\">beta-render-content</main>";
        const string rejectedCss = "[data-optimizer-side=beta]{display:block;gap:3px}";
        var leakyExample = DesignConversionDpoGeneratorTests.CreateExample(
            "C:/Users/lubao/private-project/example-001",
            DesignConversionDpoUnits.ImproverUnit,
            usefulness: 0.88) with
        {
            SourceStructurePayload = "{\"tree\":\"normalized\",\"raw_path\":\"C:/Users/lubao/Documents/private.fig\"}",
            InternalUnitStatePayload = "{\"exact_prompt\":\"make the private button blue\"}",
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "request-from-user-luba-exact-prompt",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "file:///C:/Users/lubao/Documents/private.fig",
                ReviewerRef: "reviewer:user-luba",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["source_design_id"] = "C:/Users/lubao/Documents/private.fig"
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
                OutputId = "OUTCOME_ONLY_CHOSEN_OUTPUT_ID",
                CandidateId = "C:/Users/lubao/private-candidate-a.html",
                Html = chosenHtml,
                Css = chosenCss,
                EvidenceRefs = ["OUTCOME_ONLY_CHOSEN_EVIDENCE"]
            },
            Rejected = DesignConversionDpoGeneratorTests.CreateOutput(
                "rejected-private-output",
                accepted: false,
                score: 0.35,
                htmlSuffix: "rejected") with
            {
                OutputId = "OUTCOME_ONLY_REJECTED_OUTPUT_ID",
                CandidateId = "C:/Users/lubao/private-candidate-b.html",
                Html = rejectedHtml,
                Css = rejectedCss,
                EvidenceRefs = ["OUTCOME_ONLY_REJECTED_EVIDENCE"]
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
        var auditLine = Assert.Single(dataset.Value.ToAuditJsonlLines());

        Assert.DoesNotContain(Environment.NewLine, line, StringComparison.Ordinal);
        Assert.DoesNotContain(Environment.NewLine, auditLine, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        Assert.Equal(
            ["domainTag", "unitId", "prompt", "chosen", "rejected"],
            root.EnumerateObject().Select(property => property.Name).ToArray());
        using var promptDocument = JsonDocument.Parse(root.GetProperty("prompt").GetString()!);
        var prompt = promptDocument.RootElement;
        Assert.Equal(DesignConversionDpoUnits.ImproverUnit, prompt.GetProperty("target").GetProperty("unit_id").GetString());
        Assert.Equal("design_conversion_improver_unit", prompt.GetProperty("target").GetProperty("machine_part").GetString());
        Assert.Equal("figma", prompt.GetProperty("source").GetProperty("kind").GetString());
        Assert.Contains("normalized", prompt.GetProperty("source").GetProperty("structure_payload").GetString(), StringComparison.Ordinal);
        Assert.NotEmpty(prompt.GetProperty("internal_vector").EnumerateArray());
        Assert.False(prompt.TryGetProperty("result_features", out _));
        Assert.False(prompt.TryGetProperty("pair_family", out _));
        Assert.False(prompt.TryGetProperty("usefulness", out _));
        Assert.NotEmpty(prompt.GetProperty("source_encoder").GetProperty("feature_names").EnumerateArray());
        Assert.True(prompt.GetProperty("source_encoder").GetProperty("dimensions").GetInt32() > 0);
        Assert.False(prompt.GetProperty("source_encoder").TryGetProperty("vector_hash", out _));

        var promptText = root.GetProperty("prompt").GetString()!;
        using var chosenDocument = JsonDocument.Parse(root.GetProperty("chosen").GetString()!);
        using var rejectedDocument = JsonDocument.Parse(root.GetProperty("rejected").GetString()!);
        AssertExactOptimizerResponse(chosenDocument.RootElement, chosenHtml, chosenCss);
        AssertExactOptimizerResponse(rejectedDocument.RootElement, rejectedHtml, rejectedCss);
        Assert.DoesNotContain(rejectedHtml, root.GetProperty("chosen").GetString()!, StringComparison.Ordinal);
        Assert.DoesNotContain(chosenHtml, root.GetProperty("rejected").GetString()!, StringComparison.Ordinal);
        var optimizerText = string.Join('\n', promptText, root.GetProperty("chosen").GetString(), root.GetProperty("rejected").GetString());
        foreach (var forbidden in new[]
                 {
                     "OUTCOME_ONLY_CHOSEN_OUTPUT_ID", "OUTCOME_ONLY_REJECTED_OUTPUT_ID",
                     "private-candidate-a", "private-candidate-b",
                     "OUTCOME_ONLY_CHOSEN_EVIDENCE", "OUTCOME_ONLY_REJECTED_EVIDENCE",
                     DesignConversionNegativeReasonTaxonomy.Spacing,
                     "result_features", "visual_score", "accepted", "passed", "verdict",
                     "candidate_id", "output_id", "qa_result", "reviewer", "usefulness"
                 })
        {
            Assert.DoesNotContain(forbidden, optimizerText, StringComparison.OrdinalIgnoreCase);
        }

        using var auditDocument = JsonDocument.Parse(auditLine);
        var audit = auditDocument.RootElement;
        Assert.Equal(
            ["split", "identity", "custody", "result", "labels", "review", "validation", "trainingGrade"],
            audit.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal("train", audit.GetProperty("split").GetString());
        Assert.StartsWith("dcm-source-", audit.GetProperty("identity").GetProperty("sourceGroupId").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("dcm-result-", audit.GetProperty("labels").GetProperty("preferredResultRef").GetString(), StringComparison.Ordinal);
        Assert.Equal("chosen_over_rejected", audit.GetProperty("labels").GetProperty("preferenceLabel").GetString());
        Assert.True(audit.GetProperty("labels").GetProperty("scoreMargin").GetDouble() > 0);
        Assert.True(audit.GetProperty("validation").GetProperty("accepted_for_training").GetBoolean());
        Assert.False(audit.GetProperty("trainingGrade").GetBoolean());
        Assert.False(audit.GetProperty("review").GetProperty("rawReviewerReferenceIncluded").GetBoolean());

        foreach (var raw in new[]
                 {
                     chosenHtml, chosenCss, rejectedHtml, rejectedCss,
                     "request-from-user-luba-exact-prompt", "reviewer:user-luba",
                     "license-user-luba-private", "private-candidate-a", "private-candidate-b",
                     "C:/Users/lubao", "make the private button blue", "exact_prompt", "raw_path"
                 })
        {
            Assert.DoesNotContain(raw, auditLine, StringComparison.OrdinalIgnoreCase);
        }
        Assert.DoesNotContain("\"prompt\":", auditLine, StringComparison.Ordinal);
        Assert.DoesNotContain("\"chosen\":", auditLine, StringComparison.Ordinal);
        Assert.DoesNotContain("\"rejected\":", auditLine, StringComparison.Ordinal);
        Assert.Equal("training_candidate_only", dataset.Value.Manifest.Preflight.RouteClassifier);
        Assert.False(dataset.Value.Manifest.Preflight.ProductReady);
        Assert.Contains("approval_preflight", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("source_manifest", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("source_disjoint", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("training_candidate_only", dataset.Value.Manifest.EvidenceRefs);
        Assert.Contains("product_not_ready", dataset.Value.Manifest.EvidenceRefs);
    }

    private static void AssertExactOptimizerResponse(
        JsonElement response,
        string expectedHtml,
        string expectedCss)
    {
        Assert.Equal(["html", "css"], response.EnumerateObject().Select(property => property.Name).ToArray());
        Assert.Equal(expectedHtml, response.GetProperty("html").GetString());
        Assert.Equal(expectedCss, response.GetProperty("css").GetString());
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

    [Fact]
    public void BuildDataset_EvalOnlyLegalVerdictIsExcludedFromTrainAndHeldOut()
    {
        var build = BuildRecords(
        [
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "legal-verdict-dataset-accepted-train",
                    DesignConversionDpoUnits.CriticUnit),
                "legal-verdict-dataset-source-a"),
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "legal-verdict-dataset-accepted-heldout",
                    DesignConversionDpoUnits.ImproverUnit),
                "legal-verdict-dataset-source-b"),
            WithSource(
                DesignConversionDpoGeneratorTests.CreateExample(
                    "legal-verdict-dataset-eval-only",
                    DesignConversionDpoUnits.CriticUnit),
                "legal-verdict-dataset-source-c")
        ]);
        var records = build.AcceptedRecords
            .Select(record => record.RecordId == "legal-verdict-dataset-eval-only"
                ? WithTrainingRecordProvenanceMetadata(
                    record,
                    ("legal_verdict", "EVAL_ONLY"),
                    ("approval_status", "approved"),
                    ("approved_for_training", bool.TrueString))
                : record)
            .ToArray();
        var evalOnlyIdentityHash = DesignConversionTrainingMeasurementGatePolicy.ToRecordIdentityHash(
            records.Single(record => record.RecordId == "legal-verdict-dataset-eval-only"));
        var allowedIdentityHashes = records
            .Where(record => record.RecordId != "legal-verdict-dataset-eval-only")
            .Select(DesignConversionTrainingMeasurementGatePolicy.ToRecordIdentityHash)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var dataset = new DesignConversionDpoDatasetBuilder().BuildDataset(
            records,
            LocalFixtureOptions(
                DatasetId: "legal-verdict-dataset-eval-only",
                MinimumTrainRecords: 1,
                MinimumHeldOutRecords: 1));

        Assert.True(dataset.IsSuccess, dataset.ErrorMessage);
        Assert.Equal(2, dataset.Value!.Manifest.AcceptedRecordCount);
        Assert.Equal(1, dataset.Value.Manifest.ExcludedRecordCount);
        Assert.DoesNotContain(dataset.Value.TrainRows, row => row.RecordIdentityHash == evalOnlyIdentityHash);
        Assert.DoesNotContain(dataset.Value.HeldOutRows, row => row.RecordIdentityHash == evalOnlyIdentityHash);
        Assert.Contains(dataset.Value.Manifest.ExcludedRecords, excluded =>
            excluded.RecordIdentityHash == evalOnlyIdentityHash &&
            excluded.Reason == "not_trainable_for_dataset");
        Assert.Contains("eval_only_legal_verdict",
            DesignConversionTrainingMeasurementGatePolicy.GetRecordBlockReasons(
                records.Single(record => record.RecordId == "legal-verdict-dataset-eval-only")));
        Assert.Equal(
            allowedIdentityHashes,
            dataset.Value.Rows.Select(row => row.RecordIdentityHash).Order(StringComparer.Ordinal).ToArray());
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

    private static DesignConversionDpoTrainingRecord WithTrainingRecordProvenanceMetadata(
        DesignConversionDpoTrainingRecord record,
        params (string Key, string Value)[] values)
    {
        var metadata = new Dictionary<string, string>(
            record.Provenance.Metadata,
            StringComparer.Ordinal);
        foreach (var (key, value) in values)
            metadata[key] = value;

        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata }
        };
    }

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
