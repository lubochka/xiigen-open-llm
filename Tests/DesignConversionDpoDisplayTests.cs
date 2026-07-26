// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DesignConversionDomain;
using System.Text.Json;

public sealed class DesignConversionDpoDisplayTests
{
    [Fact]
    public void BuildDisplay_SurfacesSanitizedHighUsefulnessCardWithTrainingBoundary()
    {
        var records = BuildRecords([
            DesignConversionDpoGeneratorTests.CreateExample("useful-private-source", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.92) with
            {
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
                    })
            }
        ]);

        var result = new DesignConversionDpoDisplayBuilder().BuildDisplay(records);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(result.Value!.Rows);
        Assert.Equal(1, result.Value.TrainingCandidateCount);
        Assert.Equal(0, result.Value.BlockedCount);
        Assert.Contains("dpo_usefulness_display_is_review_priority_only", result.Value.Warnings);
        Assert.Contains("training_candidate_only", result.Value.Warnings);
        Assert.Contains("product_not_ready", result.Value.Warnings);

        var row = result.Value.Rows[0];
        Assert.StartsWith("dcm-dpo-display-", row.DisplayId, StringComparison.Ordinal);
        Assert.Equal(DesignConversionDpoUnits.ImproverUnit, row.UnitId);
        Assert.Equal(DesignConversionSourceKind.Figma.ToString(), row.SourceKind);
        Assert.StartsWith("dcm-display-source-", row.SourceRef, StringComparison.Ordinal);
        Assert.Contains("result_ref=dcm-display-chosen-result-", row.ChosenSummary, StringComparison.Ordinal);
        Assert.Contains("status=accepted", row.ChosenSummary, StringComparison.Ordinal);
        Assert.Contains("result_ref=dcm-display-rejected-result-", row.RejectedSummary, StringComparison.Ordinal);
        Assert.Contains("status=rejected", row.RejectedSummary, StringComparison.Ordinal);
        Assert.Equal(0.92, row.UsefulnessScore);
        Assert.Equal("high_usefulness", row.UsefulnessLabel);
        Assert.Contains("usefulness_score_role=review_priority_only", row.UsefulnessFactors);
        Assert.Contains("training_candidate_only", row.UsefulnessFactors);
        Assert.Contains("spacing:major", row.Defects);
        Assert.Contains("design-conversion-dpo-display:training_candidate_only", row.EvidenceRefs);
        Assert.False(row.IsBlocked);
        Assert.Equal("not_blocked", row.BlockedReason);
        Assert.True(row.TrainingCandidateOnly);
        Assert.True(row.ProductNotReady);
        Assert.Equal("training_candidate_only", row.ReadinessBoundary);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["training_candidate_only"]);
        Assert.Equal(bool.TrueString, row.BoundaryMetadata["product_not_ready"]);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["raw_source_request_id_included"]);

        var serialized = JsonSerializer.Serialize(row);
        Assert.DoesNotContain("SourceRequestId", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("SourceManifestRef", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("ReviewerRef", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("CandidateId", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("request-from-user-luba", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("candidate_order", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDisplay_LabelsLowUsefulnessAsReviewPriorityNotModelReadiness()
    {
        var records = BuildRecords([
            DesignConversionDpoGeneratorTests.CreateExample("low", DesignConversionDpoUnits.CriticUnit, usefulness: 0.31)
        ]);

        var result = new DesignConversionDpoDisplayBuilder().BuildDisplay(records);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var row = Assert.Single(result.Value!.Rows);
        Assert.Equal("low_usefulness", row.UsefulnessLabel);
        Assert.True(row.TrainingCandidateOnly);
        Assert.True(row.ProductNotReady);
        Assert.Contains("heldout_quality=not_proven", row.UsefulnessFactors);

        var serialized = JsonSerializer.Serialize(row);
        Assert.DoesNotContain("accuracy", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("confidence", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("model_ready", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildDisplay_MarksQuarantinePrivateProviderAndMissingEvidenceRowsBlocked()
    {
        var records = BuildRecords([
            DesignConversionDpoGeneratorTests.CreateExample("private", DesignConversionDpoUnits.ImproverUnit, usefulness: 0.93) with
            {
                License = DesignConversionDpoGeneratorTests.CreateLicense(allowsTraining: true, containsPrivateData: true)
            },
            DesignConversionDpoGeneratorTests.CreateExample("missing-evidence", DesignConversionDpoUnits.CriticUnit, usefulness: 0.72) with
            {
                Rejected = DesignConversionDpoGeneratorTests.CreateOutput(
                    "missing-evidence-rejected",
                    accepted: false,
                    score: 0.31,
                    htmlSuffix: "missing-evidence") with
                {
                    EvidenceRefs = [],
                    QaResults = []
                }
            }
        ]);
        var providerMissing = BuildRecords([
            DesignConversionDpoGeneratorTests.CreateExample("provider-missing", DesignConversionDpoUnits.CriticUnit, usefulness: 0.88)
        ]).Single() with
        {
            Quarantine = new DesignConversionQuarantineState(
                IsQuarantined: false,
                Reason: "visual_judge_provider_missing",
                EvidenceRefs: ["provider:missing"])
        };

        var result = new DesignConversionDpoDisplayBuilder().BuildDisplay(records.Concat([providerMissing]).ToArray());

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(3, result.Value!.Rows.Count);
        Assert.Equal(0, result.Value.TrainingCandidateCount);
        Assert.Equal(3, result.Value.BlockedCount);
        Assert.Equal(2, result.Value.QuarantinedCount);
        Assert.Equal(1, result.Value.PrivateDataCount);

        var privateRow = Assert.Single(result.Value.Rows, row => row.BlockedReason.Contains("private_data_requires_quarantine", StringComparison.Ordinal));
        Assert.Equal("blocked", privateRow.UsefulnessLabel);
        Assert.False(privateRow.TrainingCandidateOnly);
        Assert.Equal("blocked_private_data", privateRow.RightsBoundary);

        var missingEvidenceRow = Assert.Single(result.Value.Rows, row => row.BlockedReason.Contains("rejected_not_realistic_negative", StringComparison.Ordinal));
        Assert.True(missingEvidenceRow.IsBlocked);
        Assert.Contains("design-conversion-dpo-validation:rejected_not_realistic_negative", missingEvidenceRow.EvidenceRefs);
        Assert.Equal("blocked_not_training_candidate", missingEvidenceRow.ReadinessBoundary);

        var providerRow = Assert.Single(result.Value.Rows, row => row.BlockedReason.Contains("provider_missing_or_blocked", StringComparison.Ordinal));
        Assert.True(providerRow.IsBlocked);
        Assert.False(providerRow.TrainingCandidateOnly);
    }

    [Fact]
    public async Task DisplayService_ReadsStorageAndCanHideBlockedRowsForUserPayload()
    {
        var storage = new InMemoryDesignConversionDpoStorage();
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("high", DesignConversionDpoUnits.CriticUnit, usefulness: 0.91));
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("private", DesignConversionDpoUnits.CriticUnit, usefulness: 0.99) with
        {
            License = DesignConversionDpoGeneratorTests.CreateLicense(allowsTraining: true, containsPrivateData: true)
        });
        await Save(storage, DesignConversionDpoGeneratorTests.CreateExample("low", DesignConversionDpoUnits.CriticUnit, usefulness: 0.31));

        var result = await new DesignConversionDpoDisplayService(storage).GetDisplayAsync(
            new DesignConversionDpoDisplayQuery(
                UnitId: DesignConversionDpoUnits.CriticUnit,
                IncludeBlocked: false,
                MinimumUsefulnessScore: 0.5,
                Limit: 5));

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var row = Assert.Single(result.Value!.Rows);
        Assert.Equal("high_usefulness", row.UsefulnessLabel);
        Assert.False(row.IsBlocked);
        Assert.True(row.TrainingCandidateOnly);
        Assert.Equal(1, result.Value.TrainingCandidateCount);
        Assert.Equal(0, result.Value.BlockedCount);
    }

    private static IReadOnlyList<DesignConversionDpoTrainingRecord> BuildRecords(
        IReadOnlyList<DesignConversionStoredDpoExample> examples)
    {
        var result = new DesignConversionDpoGenerator().BuildTrainReadyRecords(examples);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.AllRecords;
    }

    private static async Task Save(IDesignConversionDpoStorage storage, DesignConversionStoredDpoExample example)
    {
        var result = await storage.SaveExampleAsync(example);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }
}
