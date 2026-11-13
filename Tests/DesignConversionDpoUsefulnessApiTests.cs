// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed class DesignConversionDpoUsefulnessApiTests
{
    private const string PreflightRequester = "server:operator-a4";
    private const string PreflightReason = "post-approval preflight only";
    private const string ExactApprovedRowError =
        "DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_EXACT_APPROVED_ROW_REQUIRED";

    [Fact]
    public async Task QueryAsync_ReturnsUserVisibleUsefulnessRowsWithStatusBucketsAndNoRawPayloads()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-usefulness-api");
        var db = new JsonlDesignConversionLocalArtifactDb(root);

        await Save(db, BuildArtifactRecord(
            "record-train-ready",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("train-ready", DesignConversionDpoUnits.CriticUnit, 0.91)]));
        await Save(db, BuildArtifactRecord(
            "record-heldout",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("heldout-row", DesignConversionDpoUnits.CriticUnit, 0.89, split: "held_out")]));
        await Save(db, BuildArtifactRecord(
            "record-provider",
            DesignConversionVisualQaBoundaryPolicy.ProviderRequired,
            [BuildTrainingRecord("provider-missing", DesignConversionDpoUnits.CriticUnit, 0.97)]));
        await Save(db, BuildArtifactRecord(
            "record-quarantine",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("quarantined", DesignConversionDpoUnits.ImproverUnit, 0.72) with
            {
                Quarantine = new DesignConversionQuarantineState(
                    IsQuarantined: true,
                    Reason: "manual_quarantine",
                    EvidenceRefs: ["quarantine:evidence"])
            }]));
        await Save(db, BuildArtifactRecord(
            "record-negative-quality",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("negative-quality", DesignConversionDpoUnits.CriticUnit, 0.66) with
            {
                NegativeQualityReason = "",
                NegativeReasonTaxonomy = DesignConversionNegativeReasonTaxonomy.TrivialBroken,
                Quarantine = new DesignConversionQuarantineState(
                    IsQuarantined: true,
                    Reason: "negative_quality_reason_required;negative_reason_taxonomy_quarantine_only:trivial_broken",
                    EvidenceRefs: ["design-conversion-dpo-validation:negative_quality_reason_required"])
            }]));

        var response = await new DesignConversionDpoUsefulnessApi(db).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApiShape, response.Value!.ApiShape);
        Assert.False(response.Value.ProductReady);
        Assert.Equal(5, response.Value.Rows.Count);
        Assert.Equal(5, response.Value.RowCount);
        Assert.Equal(1, response.Value.TrainReadyCount);
        Assert.Equal(2, response.Value.QuarantinedCount);
        Assert.Equal(1, response.Value.ProviderMissingCount);
        Assert.Equal(1, response.Value.RejectedNegativeQualityCount);
        Assert.Equal(1, response.Value.HeldOutCount);
        Assert.Equal(3, response.Value.BlockedCount);
        Assert.Equal(0.83, response.Value.AverageUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.66, response.Value.MinimumUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.97, response.Value.MaximumUsefulnessScore.GetValueOrDefault());
        Assert.Contains("raw_html_css_tree_private_provider_payloads_not_emitted", response.Value.Warnings);
        Assert.Equal("5", response.Value.BoundaryMetadata["row_count"]);
        Assert.Equal("0.83", response.Value.BoundaryMetadata["average_usefulness_score"]);
        Assert.Equal("0.66", response.Value.BoundaryMetadata["minimum_usefulness_score"]);
        Assert.Equal("0.97", response.Value.BoundaryMetadata["maximum_usefulness_score"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["provider_payload_included"]);
        Assert.Contains("retrain_decision_surface_refs_only", response.Value.Warnings);
        Assert.Equal("row_status_plus_refs_only", response.Value.BoundaryMetadata["retrain_decision_surface"]);

        Assert.All(response.Value.Rows, row =>
        {
            Assert.False(string.IsNullOrWhiteSpace(row.RetrainDecisionStatus));
            Assert.NotEmpty(row.RetrainDecisionEvidenceRefs);
            Assert.Equal(row.RetrainDecisionStatus, row.BoundaryMetadata["api_retrain_decision_status"]);
            Assert.Equal("row_status_plus_refs_only", row.BoundaryMetadata["retrain_decision_surface"]);
            Assert.Contains(
                $"design-conversion-dpo-retrain-decision:review_status:{row.ReviewStatus}",
                row.RetrainDecisionEvidenceRefs);
            Assert.Contains(
                $"design-conversion-dpo-retrain-decision:usefulness_status:{row.UsefulnessStatus}",
                row.RetrainDecisionEvidenceRefs);
        });

        var trainReadyRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "train-ready");
        Assert.True(trainReadyRow.TrainReady);
        Assert.Equal("high_usefulness", trainReadyRow.UsefulnessStatus);
        Assert.False(trainReadyRow.ProviderMissing);
        Assert.Equal("ready-for-retrain-intake", trainReadyRow.RetrainDecisionStatus);
        Assert.Contains("design-conversion-dpo-retrain-decision:status:ready-for-retrain-intake", trainReadyRow.RetrainDecisionEvidenceRefs);
        Assert.Contains("design-conversion-dpo-retrain-decision:train_ready:true", trainReadyRow.RetrainDecisionEvidenceRefs);
        Assert.Contains("design-conversion-dpo-display:training_candidate_only", trainReadyRow.RetrainDecisionEvidenceRefs);

        var heldOutRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "heldout");
        Assert.True(heldOutRow.HeldOut);
        Assert.Equal("held_out", heldOutRow.BoundaryMetadata["split"]);
        Assert.Equal("holdout-evidence-not-training-input", heldOutRow.RetrainDecisionStatus);

        var providerMissingRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "provider-missing");
        Assert.True(providerMissingRow.ProviderMissing);
        Assert.Contains("provider_missing_or_blocked", providerMissingRow.BlockReasons);
        Assert.Equal("blocked-provider-evidence-required", providerMissingRow.RetrainDecisionStatus);

        var quarantinedRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "quarantined");
        Assert.True(quarantinedRow.Quarantined);
        Assert.Contains("manual_quarantine", quarantinedRow.BlockReasons);
        Assert.Equal("blocked-quarantined", quarantinedRow.RetrainDecisionStatus);

        var rejectedNegativeQualityRow = Assert.Single(response.Value.Rows, row => row.ReviewStatus == "rejected-negative-quality");
        Assert.True(rejectedNegativeQualityRow.RejectedNegativeQuality);
        Assert.Contains(rejectedNegativeQualityRow.BlockReasons, reason => reason.Contains("negative_quality_reason_required", StringComparison.Ordinal));
        Assert.Equal("blocked-negative-quality", rejectedNegativeQualityRow.RetrainDecisionStatus);

        var serialized = JsonSerializer.Serialize(response.Value);
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".private", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("raw_provider_payload", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("candidate_order", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QueryForAdmittedTenantAsync_ReturnsOnlyExactCanonicalTenantRowsAndRejectsMissingMalformedOrSiblingBindings()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-usefulness-tenant-scope");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var admittedTenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef("w053-tenant-alpha");
        var siblingTenantIdRef = DesignConversionModelActivationRegistry.BuildTenantIdRef("w053-tenant-sibling");
        var admittedScopeRef = BuildRouteAdmissionScopeRef("w053-scope-alpha");
        var siblingScopeRef = BuildRouteAdmissionScopeRef("w053-scope-sibling");

        await Save(db, BuildArtifactRecord(
            "tenant-scope-admitted",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [WithRouteAdmissionBinding(BuildTrainingRecord("tenant-scope-admitted", DesignConversionDpoUnits.CriticUnit, 0.71), bool.TrueString, admittedTenantIdRef, admittedScopeRef)]));
        await Save(db, BuildArtifactRecord(
            "tenant-scope-sibling",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [WithRouteAdmissionBinding(BuildTrainingRecord("tenant-scope-sibling", DesignConversionDpoUnits.CriticUnit, 0.99), bool.TrueString, siblingTenantIdRef, siblingScopeRef)]));
        await Save(db, BuildArtifactRecord(
            "tenant-scope-unbound",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("tenant-scope-unbound", DesignConversionDpoUnits.CriticUnit, 0.98)]));
        await Save(db, BuildArtifactRecord(
            "tenant-scope-missing-bound",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [WithRouteAdmissionBinding(BuildTrainingRecord("tenant-scope-missing-bound", DesignConversionDpoUnits.CriticUnit, 0.975), null, admittedTenantIdRef, admittedScopeRef)]));
        await Save(db, BuildArtifactRecord(
            "tenant-scope-malformed-bound",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [WithRouteAdmissionBinding(BuildTrainingRecord("tenant-scope-malformed-bound", DesignConversionDpoUnits.CriticUnit, 0.97), "true", admittedTenantIdRef, admittedScopeRef)]));
        await Save(db, BuildArtifactRecord(
            "tenant-scope-malformed-tenant",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [WithRouteAdmissionBinding(BuildTrainingRecord("tenant-scope-malformed-tenant", DesignConversionDpoUnits.CriticUnit, 0.96), bool.TrueString, "sha256:design-conversion-model-activation-tenant:ABC", admittedScopeRef)]));
        await Save(db, BuildArtifactRecord(
            "tenant-scope-malformed-scope",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [WithRouteAdmissionBinding(BuildTrainingRecord("tenant-scope-malformed-scope", DesignConversionDpoUnits.CriticUnit, 0.95), bool.TrueString, admittedTenantIdRef, "sha256:design-conversion-route-admission-scope:ABC")]));

        var spy = LocalArtifactDbSpy.Wrap(db);
        var api = new DesignConversionDpoUsefulnessApi(spy.Db);
        var scoped = await api.QueryForAdmittedTenantAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true, Limit: 1),
            admittedTenantIdRef);

        Assert.True(scoped.IsSuccess, scoped.ErrorMessage);
        var row = Assert.Single(scoped.Value!.Rows);
        Assert.Equal(0.71, row.UsefulnessScore);
        Assert.Equal(1, scoped.Value.RowCount);
        Assert.Equal(bool.TrueString, scoped.Value.BoundaryMetadata["tenant_scope_applied"]);
        Assert.Equal("server_admission_context", scoped.Value.BoundaryMetadata["tenant_scope_source"]);
        Assert.Equal(1, spy.QueryDisplayRowsCallCount);
        Assert.DoesNotContain(
            row.BoundaryMetadata.Keys,
            key => key.StartsWith("server_owned_route_admission_", StringComparison.Ordinal));

        foreach (var invalidAuthority in new[]
                 {
                     string.Empty,
                     "sha256:design-conversion-model-activation-tenant:ABC",
                     siblingTenantIdRef + " "
                 })
        {
            var rejected = await api.QueryForAdmittedTenantAsync(
                new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true),
                invalidAuthority);
            Assert.False(rejected.IsSuccess);
            Assert.Equal(
                "DESIGN_CONVERSION_DPO_USEFULNESS_ADMITTED_TENANT_ID_REF_INVALID",
                rejected.ErrorCode);
        }

        Assert.Equal(1, spy.QueryDisplayRowsCallCount);
        var legacy = await api.QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        Assert.True(legacy.IsSuccess, legacy.ErrorMessage);
        Assert.Equal(7, legacy.Value!.RowCount);
        Assert.Equal(bool.FalseString, legacy.Value.BoundaryMetadata["tenant_scope_applied"]);
        Assert.Equal("legacy_unscoped_query", legacy.Value.BoundaryMetadata["tenant_scope_source"]);

        var serialized = JsonSerializer.Serialize(scoped.Value);
        Assert.DoesNotContain(admittedTenantIdRef, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(siblingTenantIdRef, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("server_owned_route_admission_tenant_id_ref", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task QueryAsyncAndLocalReadbackExposeSourceEncoderIndicatorsWithoutRawPayloads()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-source-encoder-readback-api");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await Save(db, BuildArtifactRecord(
            "record-source-encoder",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("source-encoder-readback", DesignConversionDpoUnits.ImproverUnit, 0.94)]));
        var api = new DesignConversionDpoUsefulnessApi(db);

        var usefulness = await api.QueryAsync(new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
        var readback = await api.QueryLocalArtifactReadbackAsync(
            new DesignConversionLocalArtifactReadbackApiRequest(JobId: "record-source-encoder:job"));

        Assert.True(usefulness.IsSuccess, usefulness.ErrorMessage);
        var usefulnessRow = Assert.Single(usefulness.Value!.Rows);
        Assert.StartsWith("dcm-source-encoder-vector-", usefulnessRow.BoundaryMetadata["source_encoder_vector_hash"], StringComparison.Ordinal);
        Assert.True(int.Parse(usefulnessRow.BoundaryMetadata["source_encoder_dimensions"]) > 0);
        Assert.Contains("source_json_node_count", usefulnessRow.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("visual_score_margin", usefulnessRow.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.DoesNotContain("chosen_css_rule_count", usefulnessRow.BoundaryMetadata["source_encoder_feature_names"]);
        Assert.Equal("source_kind_source_id_not_vector_features", usefulnessRow.BoundaryMetadata["source_encoder_shortcut_guard"]);
        Assert.Equal(bool.FalseString, usefulnessRow.BoundaryMetadata["source_kind_in_vector"]);
        Assert.Equal(bool.FalseString, usefulnessRow.BoundaryMetadata["source_id_in_vector"]);

        Assert.True(readback.IsSuccess, readback.ErrorMessage);
        var readbackRow = Assert.Single(readback.Value!.Rows);
        Assert.NotEqual("missing", readbackRow.BoundaryMetadata["dpo_source_encoder_vector_hashes"]);
        Assert.NotEqual("missing", readbackRow.BoundaryMetadata["dpo_internal_vector_dimension_count"]);
        Assert.Contains("source_json_node_count", readbackRow.BoundaryMetadata["dpo_source_encoder_feature_names"]);
        Assert.DoesNotContain("visual_score_margin", readbackRow.BoundaryMetadata["dpo_source_encoder_feature_names"]);
        Assert.DoesNotContain("chosen_css_rule_count", readbackRow.BoundaryMetadata["dpo_source_encoder_feature_names"]);
        Assert.Contains("visual_score_margin=", readbackRow.BoundaryMetadata["dpo_result_signal_summary"]);
        Assert.Equal("source_kind_source_id_not_vector_features", readbackRow.BoundaryMetadata["dpo_source_encoder_shortcut_guard"]);
        Assert.Equal(bool.FalseString, readbackRow.BoundaryMetadata["dpo_source_kind_in_vector"]);
        Assert.Equal(bool.FalseString, readbackRow.BoundaryMetadata["dpo_source_id_in_vector"]);

        var serialized = JsonSerializer.Serialize(new { Usefulness = usefulness.Value, Readback = readback.Value });
        Assert.DoesNotContain("<main", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("C:/Users/lubao", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private.fig", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exact_prompt private", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LocalReadback_DoesNotClaimTrainingApprovalWhenReviewIsApprovedButCentralEligibilityDenies()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString("N"),
            "dpo-review-approved-central-ineligible");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        var trainingRecord = BuildTrainingRecord(
            "review-approved-central-ineligible",
            DesignConversionDpoUnits.CriticUnit,
            0.91);
        trainingRecord = trainingRecord with
        {
            Provenance = trainingRecord.Provenance with
            {
                Metadata = new Dictionary<string, string>(
                    trainingRecord.Provenance.Metadata,
                    StringComparer.Ordinal)
                {
                    ["runtime_created_dpo_candidate"] = bool.TrueString,
                    ["split"] = "runtime_training_candidate_review",
                    ["review_required"] = bool.TrueString,
                    ["approval_status"] = "approved",
                    ["review_status"] =
                        JsonlDesignConversionLocalArtifactDb
                            .RuntimeCandidateReviewApprovedStatus,
                    ["approved_for_training"] = bool.TrueString
                }
            }
        };
        var record = BuildArtifactRecord(
            "review-approved-central-ineligible",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [trainingRecord]);
        var append = await db.AppendRecordAsync(record);
        Assert.True(append.IsSuccess, append.ErrorMessage);

        var response =
            await new DesignConversionDpoUsefulnessApi(db)
                .QueryLocalArtifactReadbackAsync(
                    new DesignConversionLocalArtifactReadbackApiRequest(
                        JobId: record.JobId));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        var row = Assert.Single(response.Value!.Rows);
        Assert.Equal(
            "review-approved-training-ineligible",
            row.DpoApprovalStatus);
        Assert.Equal(
            bool.FalseString,
            row.BoundaryMetadata["approved_for_training"]);
        Assert.Equal(
            bool.FalseString,
            row.BoundaryMetadata[
                "post_evaluation_training_eligible"]);
        Assert.Contains(
            "runtime_created_dpo_candidate_review_approved_but_training_ineligible",
            row.Warnings);
        Assert.Contains(
            "approved_for_training=false",
            row.Warnings);
        Assert.DoesNotContain(
            "runtime_created_dpo_candidate_approved_for_training",
            row.Warnings);
        Assert.DoesNotContain(
            "approved_for_training=true",
            row.Warnings);
        var serialized = JsonSerializer.Serialize(response.Value);
        Assert.DoesNotContain(
            "approved_for_training=true",
            serialized,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync_ApprovedExactCandidate_CreatesOneIntakeAndReplaysPreflight()
    {
        var fixture = await BuildApprovedRuntimeCandidateFixture("approved-exact");
        var requestedAt = DateTimeOffset.Parse("2026-08-04T14:15:16+03:00", CultureInfo.InvariantCulture);
        var replayRequestedAt = DateTimeOffset.Parse("2026-08-09T20:30:00+03:00", CultureInfo.InvariantCulture);
        var expectedRequestedAt = requestedAt.ToUniversalTime();

        var result = await RunApprovedPreflightAsync(fixture, requestedAt);
        AssertSuccessfulPreflightCore(fixture.Approval, result, expectedRequestedAt, replayHit: false);
        AssertDbSideEffects(fixture.Db, exports: 1, creates: 1, preflights: 1);

        var handoffRow = await LoadApprovedHandoffRow(fixture.Inner, fixture.Approval);
        AssertIndependentMeasurementGateContract(result, handoffRow);
        var intake = await AssertSingleIntakeAsync(fixture.Inner);
        AssertApprovedIntakeContract(fixture.Approval, result, intake, expectedRequestedAt);

        var replay = await RunApprovedPreflightAsync(
            fixture,
            replayRequestedAt,
            candidateOrTrainingRecordId: fixture.Approval.TrainingRecordId);

        AssertSuccessfulPreflightCore(fixture.Approval, replay, expectedRequestedAt, replayHit: true);
        Assert.Equal((result.IntakeId, result.PreflightId, result.IdempotencyKeyRef, result.StableIntakeIdRef),
            (replay.IntakeId, replay.PreflightId, replay.IdempotencyKeyRef, replay.StableIntakeIdRef));
        Assert.Equal(expectedRequestedAt, (await AssertSingleIntakeAsync(fixture.Inner)).RequestedAtUtc);
        AssertDbSideEffects(fixture.Db, exports: 2, creates: 1, preflights: 2);
    }

    [Fact]
    public async Task RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync_UnapprovedCrossedAndNonCanonicalInputs_FailClosed()
    {
        var inner = NewDb("approved-runtime-preflight-fail-closed");
        var pending = BuildRuntimePendingTrainingRecord(
            "pending-preflight",
            DesignConversionDpoUnits.CriticUnit,
            0.92);
        var record = BuildArtifactRecord(
            "record-pending-preflight",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [pending]);
        await Save(inner, record);

        var counting = LocalArtifactDbSpy.Wrap(inner);
        var response = await new DesignConversionDpoUsefulnessApi(counting.Db)
            .RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync(
                record.RecordId,
                $"{pending.RecordId}:dpo_candidate",
                PreflightRequester,
                PreflightReason,
                DateTimeOffset.UnixEpoch.AddDays(6));

        Assert.False(response.IsSuccess);
        Assert.Equal(ExactApprovedRowError, response.ErrorCode);
        AssertDbSideEffects(counting, exports: 1, creates: 0, preflights: 0);

        var firstApproval = await AppendAndApproveRuntimeCandidate(inner, "crossed-first");
        var secondApproval = await AppendAndApproveRuntimeCandidate(inner, "crossed-second");
        counting = LocalArtifactDbSpy.Wrap(inner);

        var crossed = await new DesignConversionDpoUsefulnessApi(counting.Db)
            .RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync(
                firstApproval.LocalArtifactRecordId,
                secondApproval.CandidateId,
                PreflightRequester,
                PreflightReason,
                DateTimeOffset.UnixEpoch.AddDays(7));

        Assert.False(crossed.IsSuccess);
        Assert.Equal(ExactApprovedRowError, crossed.ErrorCode);
        AssertDbSideEffects(counting, exports: 1, creates: 0, preflights: 0);
        Assert.NotNull(counting.LastExportQuery);
        Assert.True(counting.LastExportQuery!.ApprovedTrainingOnly);
        Assert.Equal(firstApproval.LocalArtifactRecordId, counting.LastExportQuery.LocalArtifactRecordId);
        Assert.Equal(secondApproval.CandidateId, counting.LastExportQuery.CandidateOrTrainingRecordId);

        foreach (var command in BuildNonCanonicalCommands(firstApproval))
        {
            var whitespaceCounting = LocalArtifactDbSpy.Wrap(inner);
            var nonCanonicalResponse = await new DesignConversionDpoUsefulnessApi(whitespaceCounting.Db)
                .RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync(
                    command.LocalArtifactRecordId,
                    command.CandidateOrTrainingRecordId,
                    command.RequestedByRef,
                    command.Reason,
                    DateTimeOffset.UnixEpoch.AddDays(8));

            Assert.False(nonCanonicalResponse.IsSuccess);
            Assert.Equal("DESIGN_CONVERSION_APPROVED_RUNTIME_DPO_PREFLIGHT_INPUT_NOT_CANONICAL", nonCanonicalResponse.ErrorCode);
            AssertDbSideEffects(whitespaceCounting, exports: 0, creates: 0, preflights: 0);
        }

        var intakes = await inner.ListTrainingIntakeRequestsAsync();
        Assert.True(intakes.IsSuccess, intakes.ErrorMessage);
        Assert.Empty(intakes.Value!);
    }

    [Fact]
    public async Task QueryAsync_FiltersRowsThroughExistingDisplayQuery()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-usefulness-api-filter");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await Save(db, BuildArtifactRecord(
            "record-high",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("high", DesignConversionDpoUnits.CriticUnit, 0.93)]));
        await Save(db, BuildArtifactRecord(
            "record-low",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("low", DesignConversionDpoUnits.CriticUnit, 0.31)]));

        var response = await new DesignConversionDpoUsefulnessApi(db).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(
                UnitId: DesignConversionDpoUnits.CriticUnit,
                IncludeBlocked: false,
                MinimumUsefulnessScore: 0.5,
                Limit: 1));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.Equal(1, response.Value!.RowCount);
        Assert.Equal(0.93, response.Value.AverageUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.93, response.Value.MinimumUsefulnessScore.GetValueOrDefault());
        Assert.Equal(0.93, response.Value.MaximumUsefulnessScore.GetValueOrDefault());
        var row = Assert.Single(response.Value!.Rows);
        Assert.Equal("train-ready", row.ReviewStatus);
        Assert.Equal("high_usefulness", row.UsefulnessStatus);
        Assert.True(row.TrainReady);
    }

    [Fact]
    public async Task QueryAsync_ExposesUsefulnessMetricsForTrainingRecordWithProductReadyFalseBoundary()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "dpo-usefulness-api-metrics");
        var db = new JsonlDesignConversionLocalArtifactDb(root);
        await Save(db, BuildArtifactRecord(
            "record-metrics",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [BuildTrainingRecord("metrics-row", DesignConversionDpoUnits.CriticUnit, 0.88)]));

        var response = await new DesignConversionDpoUsefulnessApi(db).QueryAsync(
            new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));

        Assert.True(response.IsSuccess, response.ErrorMessage);
        Assert.False(response.Value!.ProductReady);
        var row = Assert.Single(response.Value.Rows);
        Assert.Equal(0.88, row.UsefulnessScore);
        Assert.Equal(0.88, row.UsefulnessMetrics.Score);
        Assert.Equal(0.57, row.UsefulnessMetrics.QualityGain);
        Assert.Equal(DesignConversionNegativeReasonTaxonomy.Spacing, row.UsefulnessMetrics.NegativePairUsefulness);
        Assert.Contains("source_kind=Figma", row.UsefulnessMetrics.SourceCoverage);
        Assert.Contains("source_ref=present", row.UsefulnessMetrics.SourceCoverage);
        Assert.Contains("heldout_quality=not_proven", row.UsefulnessMetrics.SourceCoverage);
        Assert.Equal("not_proven", row.UsefulnessMetrics.SourceDisjointStatus);
        Assert.Equal("not_proven", row.UsefulnessMetrics.HeldOutEvaluationStatus);
        Assert.Equal(
            "retrain_intake_eligible;local_promotion_requires_training_run_measurement_gate_and_policy_checkpoint",
            row.UsefulnessMetrics.PromotionEligibility);
        Assert.Equal(new[] { "policy_checkpoint_evidence:not_present" }, row.UsefulnessMetrics.PolicyCheckpointEvidenceRefs);
        Assert.False(row.UsefulnessMetrics.ProductReady);
        Assert.True(row.ProductNotReady);
        Assert.Equal("product_ready=false", row.UsefulnessMetrics.ProductReadyBoundary);
        Assert.Equal(bool.FalseString, row.BoundaryMetadata["api_product_ready"]);
    }

    private static void AssertNoTrainingMutation(
        DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult result)
        => Assert.False(result.TrainingRunnerInvoked || result.TrainingStarted ||
                        result.CheckpointMutated || result.WeightsProduced || result.ModelUploadReady);

    private static void AssertFalseMetadata(
        IReadOnlyDictionary<string, string> metadata,
        params string[] keys)
    {
        foreach (var key in keys)
            Assert.Equal(bool.FalseString, metadata[key]);
    }

    private static async Task<DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult>
        RunApprovedPreflightAsync(
            ApprovedRuntimeCandidateFixture fixture,
            DateTimeOffset requestedAt,
            string? candidateOrTrainingRecordId = null)
    {
        var response = await fixture.Api.RunApprovedRuntimeDpoCandidateTrainingIntakePreflightAsync(
            fixture.Approval.LocalArtifactRecordId,
            candidateOrTrainingRecordId ?? fixture.Approval.CandidateId,
            PreflightRequester,
            PreflightReason,
            requestedAt);

        Assert.True(response.IsSuccess, response.ErrorMessage);
        return response.Value!;
    }

    private static void AssertSuccessfulPreflightCore(
        DesignConversionDpoCandidateApprovalResult approval,
        DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult result,
        DateTimeOffset expectedRequestedAtUtc,
        bool replayHit)
    {
        Assert.Equal(
            (approval.LocalArtifactRecordId, approval.CandidateId, approval.TrainingRecordId, approval.UnitId),
            (result.LocalArtifactRecordId, result.CandidateId, result.TrainingRecordId, result.UnitId));
        Assert.Equal((1, 1, "blocked", false),
            (result.AcceptedCandidateCount, result.DpoStepCount, result.PreflightStatus, result.MayStartTraining));
        AssertNoTrainingMutation(result);
        Assert.Equal(expectedRequestedAtUtc, result.PersistedRequestedAtUtc);
        Assert.StartsWith("memory://design-conversion/dpo-handoff/", result.SourceHandoffArtifactRef, StringComparison.Ordinal);
        Assert.StartsWith("training-intake-", result.IntakeId, StringComparison.Ordinal);
        Assert.StartsWith("memory://design-conversion/training-intake/", result.IntakeRef, StringComparison.Ordinal);
        Assert.StartsWith("training-intake-preflight-", result.PreflightId, StringComparison.Ordinal);
        Assert.Contains("approved_runtime_dpo_candidate_training_intake_preflight_only", result.EvidenceRefs);
        Assert.Contains("source_disjoint:not_durably_proven", result.EvidenceRefs);
        Assert.Contains("training_runner_invoked=false", result.Warnings);
        var serialized = JsonSerializer.Serialize(result);
        Assert.DoesNotContain(PreflightRequester, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain(PreflightReason, serialized, StringComparison.Ordinal);
        Assert.Equal((replayHit, !replayHit), (result.IdempotencyReplayHit, result.IdempotencyCreatedNewSideEffect));
    }

    private static void AssertDbSideEffects(
        LocalArtifactDbSpy db,
        int exports,
        int creates,
        int preflights)
    {
        Assert.Equal(exports, db.ExportApprovedRuntimeDpoCandidatesCallCount);
        Assert.Equal(0, db.BroadExportApprovedRuntimeDpoCandidatesCallCount);
        Assert.Equal(creates, db.CreateTrainingIntakeCallCount);
        Assert.Equal(preflights, db.RunTrainingIntakePreflightCallCount);
        Assert.Equal((0, 0), (db.RunTrainingIntakeCallCount, db.RunTrainingIntakeAndPromoteLocalCallCount));
    }

    private static async Task<DesignConversionTrainingIntakeRecord> AssertSingleIntakeAsync(
        JsonlDesignConversionLocalArtifactDb db)
    {
        var intakes = await db.ListTrainingIntakeRequestsAsync();
        Assert.True(intakes.IsSuccess, intakes.ErrorMessage);
        return Assert.Single(intakes.Value!);
    }

    private static void AssertApprovedIntakeContract(
        DesignConversionDpoCandidateApprovalResult approval,
        DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult result,
        DesignConversionTrainingIntakeRecord intake,
        DateTimeOffset expectedRequestedAtUtc)
    {
        Assert.Equal((result.IntakeId, result.IntakeRef, 1, expectedRequestedAtUtc),
            (intake.IntakeId, intake.IntakeRef, intake.AcceptedCandidateCount, intake.RequestedAtUtc));
        var candidate = Assert.Single(intake.CandidateRefs);
        Assert.Equal(
            (approval.LocalArtifactRecordId, approval.CandidateId, approval.TrainingRecordId),
            (candidate.LocalArtifactRecordId, candidate.CandidateId, candidate.TrainingRecordId));
        Assert.Equal("1", intake.BoundaryMetadata["coordinator_dpo_step_count"]);
        Assert.Equal(result.MeasurementGateId, intake.BoundaryMetadata["coordinator_measurement_gate_id"]);
        Assert.Equal(result.MeasurementGateUnitId, intake.BoundaryMetadata["coordinator_measurement_gate_unit_id"]);
        Assert.Equal(result.MeasurementGateContractRef, intake.BoundaryMetadata["coordinator_measurement_gate_contract_ref"]);
        Assert.Equal(result.IdempotencyKeyRef, intake.BoundaryMetadata["training_intake_idempotency_key_ref"]);
        Assert.Equal(result.StableIntakeIdRef, intake.BoundaryMetadata["coordinator_stable_intake_id"]);
        AssertFalseMetadata(intake.BoundaryMetadata,
            "training_runner_invoked", "training_started", "checkpoint_mutated", "weights_produced", "model_upload_ready");
    }

    private static void AssertIndependentMeasurementGateContract(
        DesignConversionApprovedRuntimeDpoCandidateTrainingIntakePreflightResult result,
        DesignConversionApprovedDpoHandoffRow handoffRow)
    {
        Assert.StartsWith("dcm-approved-runtime-dpo-preflight-gate-", result.MeasurementGateId, StringComparison.Ordinal);
        Assert.StartsWith("dcm-approved-runtime-dpo-preflight-stable-intake-", result.StableIntakeIdRef, StringComparison.Ordinal);
        Assert.StartsWith("dcm-approved-runtime-dpo-preflight-idempotency-", result.IdempotencyKeyRef, StringComparison.Ordinal);
        Assert.Equal(
            BuildIndependentApprovedRuntimeDpoPreflightMeasurementGateContractRef(
                result.MeasurementGateId,
                result.MeasurementGateUnitId,
                handoffRow),
            result.MeasurementGateContractRef);
    }

    private static JsonlDesignConversionLocalArtifactDb NewDb(string name)
        => new(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), name));

    private static async Task Save(
        JsonlDesignConversionLocalArtifactDb db,
        DesignConversionLocalArtifactDbRecord record)
    {
        _ = await DesignConversionPostEvaluationEligibilityTestFixture
            .AppendEligibleRecordAsync(db, record);
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecord(
        string id,
        string unitId,
        double usefulness,
        string split = "train")
    {
        var example = DesignConversionDpoGeneratorTests.CreateExample(id, unitId, usefulness) with
        {
            Provenance = new DesignConversionDpoProvenance(
                SourceRequestId: "design-request-redacted",
                SourceKind: DesignConversionSourceKind.Figma.ToString(),
                SourceManifestRef: "manifest://design-conversion/source-redacted",
                ReviewerRef: "teacher:redacted",
                CreatedAtUtc: DateTimeOffset.UnixEpoch,
                Metadata: new Dictionary<string, string>
                {
                    ["split"] = split,
                    ["source_quality"] = "teacher-reviewed"
                })
        };
        var build = new DesignConversionDpoGenerator().BuildTrainReadyRecords([example]);
        Assert.True(build.IsSuccess, build.ErrorMessage);
        return build.Value!.AllRecords.Single();
    }

    private static DesignConversionDpoTrainingRecord WithRouteAdmissionBinding(
        DesignConversionDpoTrainingRecord record,
        string? tenantBound,
        string tenantIdRef,
        string scopeRef)
    {
        var metadata = new Dictionary<string, string>(record.Provenance.Metadata, StringComparer.Ordinal)
        {
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantIdRefMetadataKey] = tenantIdRef,
            [DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionScopeRefMetadataKey] = scopeRef
        };
        if (tenantBound is not null)
        {
            metadata[DesignConversionThinClientJobRuntime.ServerOwnedRouteAdmissionTenantBoundMetadataKey] =
                tenantBound;
        }
        return record with
        {
            Provenance = record.Provenance with { Metadata = metadata }
        };
    }

    private static string BuildRouteAdmissionScopeRef(string source)
        => "sha256:design-conversion-route-admission-scope:" +
           Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))
               .ToLowerInvariant();

    private static async Task<ApprovedRuntimeCandidateFixture> BuildApprovedRuntimeCandidateFixture(
        string id)
    {
        var inner = NewDb($"approved-runtime-preflight-{id}");
        var approval = await AppendAndApproveRuntimeCandidate(inner, id);
        var spy = LocalArtifactDbSpy.Wrap(inner);
        return new ApprovedRuntimeCandidateFixture(
            inner,
            spy,
            new DesignConversionDpoUsefulnessApi(spy.Db),
            approval);
    }

    private static async Task<DesignConversionDpoCandidateApprovalResult>
        AppendAndApproveRuntimeCandidate(
            JsonlDesignConversionLocalArtifactDb db,
            string id)
    {
        var trainingRecord = BuildRuntimePendingTrainingRecord(
            $"training-{id}",
            DesignConversionDpoUnits.CriticUnit,
            0.94);
        var record = BuildArtifactRecord(
            $"record-{id}",
            DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
            [trainingRecord],
            intakeReady: true);
        await Save(db, record);

        var approval = await new DesignConversionDpoUsefulnessApi(db)
            .ApproveRuntimeCreatedDpoCandidateAsync(
                record.RecordId,
                $"{trainingRecord.RecordId}:dpo_candidate",
                "reviewer:a4",
                "approved runtime candidate for post-approval preflight",
                DateTimeOffset.UnixEpoch.AddHours(4));
        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        return approval.Value!;
    }

    private static async Task<DesignConversionApprovedDpoHandoffRow> LoadApprovedHandoffRow(
        JsonlDesignConversionLocalArtifactDb db,
        DesignConversionDpoCandidateApprovalResult approval)
    {
        var handoff = await db.ExportApprovedRuntimeDpoCandidatesAsync(
            new DesignConversionLocalArtifactDbQuery(
                ApprovedTrainingOnly: true,
                LocalArtifactRecordId: approval.LocalArtifactRecordId,
                CandidateOrTrainingRecordId: approval.CandidateId),
            DateTimeOffset.UnixEpoch.AddHours(5));
        Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
        return Assert.Single(handoff.Value!.Rows);
    }

    private static DesignConversionDpoTrainingRecord BuildRuntimePendingTrainingRecord(
        string id,
        string unitId,
        double usefulness)
    {
        var record = BuildTrainingRecordWithComparison(id, unitId, usefulness);
        return record with
        {
            Provenance = record.Provenance with
            {
                SourceManifestRef = $"manifest://design-conversion/{id}",
                Metadata = WithRuntimePendingMetadata(record.Provenance.Metadata, includeCandidateOnly: true)
            },
            License = record.License with
            {
                Metadata = WithRuntimePendingMetadata(record.License.Metadata, includeCandidateOnly: false)
            }
        };
    }

    private static Dictionary<string, string> WithRuntimePendingMetadata(
        IReadOnlyDictionary<string, string> source,
        bool includeCandidateOnly)
    {
        var metadata = new Dictionary<string, string>(source, StringComparer.Ordinal)
        {
            ["approval_required"] = bool.TrueString,
            ["review_required"] = bool.TrueString,
            ["approval_status"] = "pending",
            ["approved_for_training"] = bool.FalseString,
            ["product_not_ready"] = bool.TrueString,
            ["model_readiness_claimed"] = bool.FalseString,
            ["held_out_not_proven"] = bool.TrueString,
            ["source_disjoint_not_proven"] = bool.TrueString
        };
        if (!includeCandidateOnly)
            return metadata;

        metadata["runtime_created_dpo_candidate"] = bool.TrueString;
        metadata["review_status"] = "runtime-candidate-review-pending";
        metadata["training_candidate_only"] = bool.TrueString;
        metadata["production_dpo"] = bool.FalseString;
        metadata["split"] = "runtime_training_candidate_review_pending";
        return metadata;
    }

    private static DesignConversionDpoTrainingRecord BuildTrainingRecordWithComparison(
        string id,
        string unitId,
        double usefulness)
    {
        var record = BuildTrainingRecord(id, unitId, usefulness);
        return record with
        {
            Chosen = record.Chosen with
            {
                QaResults = [BuildRenderQaResult($"{id}-chosen-comparison")]
            },
            Rejected = record.Rejected with
            {
                QaResults = [BuildRenderQaResult($"{id}-rejected-comparison")]
            }
        };
    }

    private static RenderQaResultRecord BuildRenderQaResult(string id)
    {
        var resolution = new RenderQaResolution("desktop", 1440, 900, 1);
        return new RenderQaResultRecord(
            ResultId: $"{id}:render-qa",
            RequestId: $"{id}:request",
            Expected: new RenderQaImageRecord(
                $"{id}:expected", "expected-design-render", resolution,
                $"artifact://design-conversion/{id}/expected.png", $"sha256:expected-{id}",
                new Dictionary<string, string> { ["role"] = "expected_design_render" }),
            Actual: new RenderQaImageRecord(
                $"{id}:actual", "actual-browser-render", resolution,
                $"artifact://design-conversion/{id}/actual.png", $"sha256:actual-{id}",
                new Dictionary<string, string> { ["role"] = "actual_browser_render" }),
            Verdict: new VisualVerdict(
                $"{id}:comparison", 0.91, [], [$"artifact://design-conversion/{id}/diff.png"], true),
            Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.09 },
            Warnings: []);
    }

    private static IReadOnlyList<PreflightCommandFields> BuildNonCanonicalCommands(
        DesignConversionDpoCandidateApprovalResult approval)
        =>
        [
            new(null!, approval.CandidateId, PreflightRequester, PreflightReason),
            new(approval.LocalArtifactRecordId, null!, PreflightRequester, PreflightReason),
            new(approval.LocalArtifactRecordId, approval.CandidateId, null!, PreflightReason),
            new(approval.LocalArtifactRecordId, approval.CandidateId, PreflightRequester, null!),
            new(" ", approval.CandidateId, PreflightRequester, PreflightReason),
            new(approval.LocalArtifactRecordId, "\t", PreflightRequester, PreflightReason),
            new(approval.LocalArtifactRecordId, approval.CandidateId, " ", PreflightReason),
            new(approval.LocalArtifactRecordId, approval.CandidateId, PreflightRequester, "\r\n"),
            new($" {approval.LocalArtifactRecordId}", approval.CandidateId, PreflightRequester, PreflightReason),
            new(approval.LocalArtifactRecordId, $"{approval.CandidateId} ", PreflightRequester, PreflightReason),
            new(approval.LocalArtifactRecordId, approval.CandidateId, $"{PreflightRequester} ", PreflightReason),
            new(approval.LocalArtifactRecordId, approval.CandidateId, PreflightRequester, $" {PreflightReason}")
        ];

    private static string BuildIndependentApprovedRuntimeDpoPreflightMeasurementGateContractRef(
        string gateId,
        string unitId,
        DesignConversionApprovedDpoHandoffRow row)
    {
        var evidenceRefs = BuildIndependentApprovedRuntimeDpoPreflightGateEvidenceRefs(row);
        var canonical = string.Concat(
            CanonicalFields(
                ("schema", "design-conversion-caller-admission-measurement-v1"),
                ("gate_id", gateId),
                ("unit_id", unitId),
                ("held_out_record_count", null),
                ("held_out_quality_score", null),
                ("runtime_ms", null),
                ("cost_usd", null),
                ("peak_ram_mb", null)),
            CanonicalCollection("held_out_evidence", evidenceRefs),
            CanonicalFields(
                ("source_disjoint", "false"),
                ("leakage_detected", null),
                ("hidden_determinism_detected", null),
                ("ablation_passed", null)),
            CanonicalCollection("integrity_evidence", evidenceRefs),
            CanonicalFields(
                ("required_quality", "1"),
                ("max_runtime_ms", "1"),
                ("max_cost_usd", "0"),
                ("max_peak_ram_mb", "1")));

        return "sha256:" + Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static IReadOnlyList<string> BuildIndependentApprovedRuntimeDpoPreflightGateEvidenceRefs(
        DesignConversionApprovedDpoHandoffRow row)
        => BuildIndependentApprovedRuntimeDpoPreflightPublicEvidenceRefs(row)
            .Select(reference => BuildIndependentMeasurementProjectIdentifierRef(
                "approved-runtime-dpo-preflight-evidence",
                reference))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> BuildIndependentApprovedRuntimeDpoPreflightPublicEvidenceRefs(
        DesignConversionApprovedDpoHandoffRow row)
        => (row.EvidenceRefs ?? [])
            .Concat(row.BoundaryMetadata.Select(pair => $"{pair.Key}={pair.Value}"))
            .Concat(
            [
                "approved_runtime_dpo_candidate_preflight:durable_handoff_row",
                "held_out_measurement:not_durably_proven",
                "source_disjoint:not_durably_proven",
                "training_mutation:not_started"
            ])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static string BuildIndependentMeasurementProjectIdentifierRef(string kind, string value)
        => "sha256:" + Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(
                    $"design-conversion-caller-admission:{kind}:{value}")))
            .ToLowerInvariant();

    private static string BuildIndependentHashRef(string prefix, string value)
    {
        var hash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)))
            .ToLowerInvariant();
        return $"{prefix}-{hash[..16]}";
    }

    private static string CanonicalFields(params (string Name, string? Value)[] fields)
        => string.Concat(fields.Select(field => CanonicalField(field.Name, field.Value)));

    private static string CanonicalCollection(
        string name,
        IReadOnlyList<string>? values)
    {
        if (values is null)
            return CanonicalField($"{name}.count", null);

        var items = values
            .Order(StringComparer.Ordinal)
            .Select((value, index) => CanonicalField($"{name}.{index.ToString(CultureInfo.InvariantCulture)}", value));
        return string.Concat(CanonicalField($"{name}.count", values.Count.ToString(CultureInfo.InvariantCulture)), string.Concat(items));
    }

    private static string CanonicalField(string name, string? value)
        => value is null
            ? $"{name.Length.ToString(CultureInfo.InvariantCulture)}:{name}=N;"
            : $"{name.Length.ToString(CultureInfo.InvariantCulture)}:{name}=V{value.Length.ToString(CultureInfo.InvariantCulture)}:{value};";

    private sealed record ApprovedRuntimeCandidateFixture(
        JsonlDesignConversionLocalArtifactDb Inner,
        LocalArtifactDbSpy Db,
        DesignConversionDpoUsefulnessApi Api,
        DesignConversionDpoCandidateApprovalResult Approval);

    private sealed record PreflightCommandFields(
        string LocalArtifactRecordId,
        string CandidateOrTrainingRecordId,
        string RequestedByRef,
        string Reason);

    private class LocalArtifactDbSpy : DispatchProxy
    {
        private IDesignConversionLocalArtifactDb _inner = null!;

        public IDesignConversionLocalArtifactDb Db { get; private set; } = null!;
        public int ExportApprovedRuntimeDpoCandidatesCallCount { get; private set; }
        public int BroadExportApprovedRuntimeDpoCandidatesCallCount { get; private set; }
        public int CreateTrainingIntakeCallCount { get; private set; }
        public int RunTrainingIntakePreflightCallCount { get; private set; }
        public int RunTrainingIntakeCallCount { get; private set; }
        public int RunTrainingIntakeAndPromoteLocalCallCount { get; private set; }
        public int QueryDisplayRowsCallCount { get; private set; }
        public DesignConversionLocalArtifactDbQuery? LastExportQuery { get; private set; }

        public static LocalArtifactDbSpy Wrap(IDesignConversionLocalArtifactDb inner)
        {
            var proxy = Create<IDesignConversionLocalArtifactDb, LocalArtifactDbSpy>();
            var spy = (LocalArtifactDbSpy)(object)proxy;
            spy._inner = inner;
            spy.Db = proxy;
            return spy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            switch (targetMethod?.Name)
            {
                case nameof(IDesignConversionLocalArtifactDb.ExportApprovedRuntimeDpoCandidatesAsync):
                    LastExportQuery = args?[0] as DesignConversionLocalArtifactDbQuery;
                    ExportApprovedRuntimeDpoCandidatesCallCount++;
                    if (LastExportQuery is null || !LastExportQuery.ApprovedTrainingOnly ||
                        string.IsNullOrWhiteSpace(LastExportQuery.LocalArtifactRecordId) ||
                        string.IsNullOrWhiteSpace(LastExportQuery.CandidateOrTrainingRecordId))
                        BroadExportApprovedRuntimeDpoCandidatesCallCount++;
                    break;
                case nameof(IDesignConversionLocalArtifactDb.CreateTrainingIntakeAsync):
                    CreateTrainingIntakeCallCount++;
                    break;
                case nameof(IDesignConversionLocalArtifactDb.RunTrainingIntakePreflightAsync):
                    RunTrainingIntakePreflightCallCount++;
                    break;
                case nameof(IDesignConversionLocalArtifactDb.RunTrainingIntakeAsync):
                    RunTrainingIntakeCallCount++;
                    break;
                case nameof(IDesignConversionLocalArtifactDb.RunTrainingIntakeAndPromoteLocalAsync):
                    RunTrainingIntakeAndPromoteLocalCallCount++;
                    break;
                case nameof(IDesignConversionLocalArtifactDb.QueryDisplayRowsAsync):
                    QueryDisplayRowsCallCount++;
                    break;
            }

            return targetMethod!.Invoke(_inner, args);
        }
    }

    private static DesignConversionLocalArtifactDbRecord BuildArtifactRecord(
        string recordId,
        string visualJudgeStatus,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records,
        bool intakeReady = false)
        => new(
            RecordId: recordId,
            DatabaseKind: JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
            JobId: $"{recordId}:job",
            RequestId: $"{recordId}:request",
            SourceKind: DesignConversionSourceKind.Figma.ToString(),
            Tier: "pro",
            SanitizedSourceRefs: intakeReady
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source_manifest_ref"] = $"manifest://design-conversion/{recordId}",
                    ["source_kind"] = "figma",
                    ["design_tree_payload_ref"] = $"sha256:design-tree-{recordId}",
                    ["original_tree_payload_ref"] = $"sha256:original-tree-{recordId}",
                    ["source_artifact:expected_desktop"] = $"sha256:expected-design-render-{recordId}",
                    ["source_artifact:actual_desktop"] = $"sha256:actual-browser-render-{recordId}"
                }
                : new Dictionary<string, string>
                {
                    ["source_manifest_ref"] = "file:///C:/Users/lubao/Documents/private.fig",
                    ["source_kind"] = "figma"
                },
            ConversionResult: new DesignConversionConversionResultArtifact(
                ResultId: $"{recordId}:conversion-result",
                HtmlRef: intakeReady
                    ? $"artifact://design-conversion/{recordId}/chosen.html"
                    : "<main data-user-id=\"luba\">exact_prompt private html</main>",
                HtmlSha256: intakeReady ? $"sha256:html-{recordId}" : string.Empty,
                CssRef: intakeReady
                    ? $"artifact://design-conversion/{recordId}/chosen.css"
                    : ".private{background:url('C:/Users/lubao/private.png');}",
                CssSha256: intakeReady ? $"sha256:css-{recordId}" : string.Empty,
                Metadata: new Dictionary<string, string>()),
            RenderArtifactRefs:
            [
                new DesignConversionRenderArtifactDbRef(
                    ViewportId: "desktop",
                    ArtifactId: $"{recordId}:render:desktop",
                    Kind: "browser_screenshot",
                    UriRef: intakeReady
                        ? $"artifact://design-conversion/{recordId}/render-desktop.png"
                        : "file:///C:/Users/lubao/private/render-desktop.png",
                    Sha256: intakeReady
                        ? $"sha256:render-desktop-{recordId}"
                        : "sha256:render-desktop",
                    ByteLength: 1234,
                    Width: 1440,
                    Height: 900,
                    Metadata: new Dictionary<string, string>())
            ],
            VisualJudgeStatus: new DesignConversionVisualJudgeArtifactDbStatus(
                Status: visualJudgeStatus,
                ProviderBoundary: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker,
                ExternalCallExecuted: false,
                EstimatedCostUsd: null,
                TierDecision: visualJudgeStatus == DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                    : DesignConversionVisualQaBoundaryPolicy.CostCapMarker,
                EvidenceRefs: [visualJudgeStatus, "visual_judge_status", "product_not_ready"],
                Metadata: new Dictionary<string, string>
                {
                    ["visual_judge_status"] = visualJudgeStatus
                }),
            DpoCandidates: records
                .Select(record => DesignConversionLocalArtifactDbDpoCandidate.FromTrainingRecord(record))
                .ToArray(),
            CreatedAtUtc: DateTimeOffset.UnixEpoch,
            EvidenceRefs:
            [
                "storage_training_artifact_db",
                "local_artifact_db",
                "conversion_result",
                "render_artifact_refs",
                "visual_judge_status",
                "dpo_candidate",
                "usefulness_score",
                "quarantine",
                "training_candidate_only",
                "product_not_ready"
            ],
            Metadata: new Dictionary<string, string>());
}
