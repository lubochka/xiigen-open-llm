// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;

public sealed record DesignConversionPostEvaluationTrainingEligibility(
    bool IsEligible,
    string Reason,
    string ErrorCode,
    string StateRef,
    long StateVersion)
{
    public static DesignConversionPostEvaluationTrainingEligibility Eligible(
        string stateRef,
        long stateVersion)
        => new(
            true,
            DesignConversionPostEvaluationTrainingEligibilityPolicy.EligibleReason,
            string.Empty,
            stateRef,
            stateVersion);

    public static DesignConversionPostEvaluationTrainingEligibility Denied(
        string reason,
        string errorCode,
        string? stateRef = null)
        => new(
            false,
            reason,
            errorCode,
            stateRef ?? string.Empty,
            0);
}

/// <summary>
/// The single durable authority used before a local artifact DPO candidate can
/// be approved, displayed as trainable, measured, exported, queued, executed,
/// activated, or promoted. Local record metadata is only a provisional
/// snapshot; the strict post-evaluation state store is authoritative.
/// </summary>
public sealed class DesignConversionPostEvaluationTrainingEligibilityPolicy
{
    public const string EligibleReason = "post_evaluation_state_committed";
    public const string MarkerSetIncompleteReason = "post_evaluation_marker_set_incomplete";
    public const string MetadataPersistenceStatusInvalidReason =
        "post_evaluation_metadata_persistence_status_invalid";
    public const string MetadataTrainingEligibilityInvalidReason =
        "post_evaluation_metadata_training_eligibility_invalid";
    public const string StateRefInvalidReason = "post_evaluation_state_ref_invalid";
    public const string StateReadFailedReason = "post_evaluation_state_read_failed";
    public const string StateMissingReason = "post_evaluation_state_missing";
    public const string StateNotCommittedReason = "post_evaluation_state_not_committed";
    public const string ArtifactRecordIdMismatchReason =
        "post_evaluation_artifact_record_id_mismatch";
    public const string CompletionBindingMissingReason =
        "post_evaluation_completion_binding_missing";
    public const string CompletionBindingKindInvalidReason =
        "post_evaluation_completion_binding_kind_invalid";
    public const string CompletionBindingRefMismatchReason =
        "post_evaluation_completion_binding_ref_mismatch";
    public const string LocalArtifactAuthorityInvalidReason =
        "post_evaluation_local_artifact_authority_invalid";
    public const string LedgerRecordIdMissingReason =
        "post_evaluation_ledger_record_id_missing";
    public const string EvaluatorUnavailableReason =
        "post_evaluation_eligibility_evaluator_unavailable";

    public const string MarkerSetIncompleteErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_MARKER_SET_INCOMPLETE";
    public const string MetadataSnapshotInvalidErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_METADATA_SNAPSHOT_INVALID";
    public const string StateRefInvalidErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_REF_INVALID";
    public const string StateReadFailedErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_ELIGIBILITY_READ_FAILED";
    public const string StateMissingErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_ELIGIBILITY_MISSING";
    public const string StateNotCommittedErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_NOT_COMMITTED";
    public const string StateIdentityMismatchErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_STATE_IDENTITY_MISMATCH";
    public const string CompletionBindingInvalidErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_COMPLETION_BINDING_INVALID";
    public const string EvaluatorUnavailableErrorCode =
        "DESIGN_CONVERSION_POST_EVALUATION_ELIGIBILITY_EVALUATOR_UNAVAILABLE";

    // A future offline migration may add an explicitly versioned legacy
    // discriminator. Absence of post-evaluation markers is intentionally not
    // grandfathered and this runtime does not emit a legacy eligibility marker.
    public const string LegacyMigrationSchemaMetadataKey =
        "post_evaluation_legacy_migration_schema";
    public const string LegacyMigrationSchemaV1 =
        "design-conversion-post-evaluation-legacy-migration-v1";

    private readonly IDesignConversionPostEvaluationStateStore _stateStore;

    public DesignConversionPostEvaluationTrainingEligibilityPolicy(
        IDesignConversionPostEvaluationStateStore stateStore)
        => _stateStore = stateStore ??
                         throw new ArgumentNullException(nameof(stateStore));

    public async Task<DesignConversionPostEvaluationTrainingEligibility> EvaluateAsync(
        DesignConversionLocalArtifactDbRecord? record,
        CancellationToken ct = default)
    {
        var results = await EvaluateManyAsync([record], ct)
            .ConfigureAwait(false);
        return results[0];
    }

    public async Task<IReadOnlyList<
        DesignConversionPostEvaluationTrainingEligibility>>
        EvaluateManyAsync(
            IReadOnlyList<DesignConversionLocalArtifactDbRecord?> records,
            CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        if (records.Count == 0)
            return [];

        var markers = records
            .Select(ValidateMarkerSnapshot)
            .ToArray();
        if (!markers.Any(marker => marker.Denied is null))
        {
            return markers
                .Select(marker => marker.Denied!)
                .ToArray();
        }

        OperationResult<IReadOnlyList<DesignConversionPostEvaluationState>>
            inventory;
        try
        {
            inventory = await _stateStore
                .ListAsync(ct)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            inventory =
                OperationResult<
                    IReadOnlyList<DesignConversionPostEvaluationState>>.Fail(
                    "Post-evaluation state inventory was cancelled.",
                    StateReadFailedErrorCode);
        }
        catch (Exception ex) when (
            ex is IOException or UnauthorizedAccessException or
                System.Security.SecurityException or
                System.Text.Json.JsonException or
                ArgumentException or InvalidOperationException)
        {
            inventory =
                OperationResult<
                    IReadOnlyList<DesignConversionPostEvaluationState>>.Fail(
                    "Post-evaluation state inventory could not be read.",
                    StateReadFailedErrorCode);
        }

        if (!inventory.IsSuccess)
        {
            return markers
                .Select(marker =>
                    marker.Denied ??
                    Denied(
                        StateReadFailedReason,
                        StateReadFailedErrorCode,
                        marker.StateRef))
                .ToArray();
        }

        var states = inventory.Value!
            .GroupBy(state => state.StateRef, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count() == 1 ? group.Single() : null,
                StringComparer.Ordinal);
        var results =
            new DesignConversionPostEvaluationTrainingEligibility[
                records.Count];
        for (var index = 0; index < records.Count; index++)
        {
            var marker = markers[index];
            if (marker.Denied is not null)
            {
                results[index] = marker.Denied;
                continue;
            }

            states.TryGetValue(marker.StateRef, out var state);
            results[index] = EvaluateResolvedState(
                records[index]!,
                marker.StateRef,
                state);
        }

        return results;
    }

    private static DesignConversionPostEvaluationTrainingEligibility
        EvaluateResolvedState(
            DesignConversionLocalArtifactDbRecord record,
            string stateRef,
            DesignConversionPostEvaluationState? state)
    {
        if (state is null)
        {
            return Denied(
                StateMissingReason,
                StateMissingErrorCode,
                stateRef);
        }

        if (state.TrainingEligibilityDenied)
        {
            return Denied(
                StateNotCommittedReason,
                StateNotCommittedErrorCode,
                stateRef);
        }

        if (!string.Equals(
                record.DatabaseKind,
                JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind,
                StringComparison.Ordinal))
        {
            return Denied(
                LocalArtifactAuthorityInvalidReason,
                CompletionBindingInvalidErrorCode,
                stateRef);
        }

        if (!string.Equals(
                state.PersistenceStatus,
                JsonDesignConversionPostEvaluationStateStore.PersistenceCommitted,
                StringComparison.Ordinal))
        {
            return Denied(
                StateNotCommittedReason,
                StateNotCommittedErrorCode,
                stateRef);
        }

        if (!string.Equals(state.ArtifactRecordId, record.RecordId, StringComparison.Ordinal))
        {
            return Denied(
                ArtifactRecordIdMismatchReason,
                StateIdentityMismatchErrorCode,
                stateRef);
        }

        if (string.IsNullOrWhiteSpace(state.CompletionBindingKind) ||
            string.IsNullOrWhiteSpace(state.CompletionBindingRef))
        {
            return Denied(
                CompletionBindingMissingReason,
                CompletionBindingInvalidErrorCode,
                stateRef);
        }

        if (!string.Equals(
                state.CompletionBindingKind,
                JsonDesignConversionPostEvaluationStateStore
                    .CompletionBindingLocalArtifactDbRecord,
                StringComparison.Ordinal))
        {
            return Denied(
                CompletionBindingKindInvalidReason,
                CompletionBindingInvalidErrorCode,
                stateRef);
        }

        if (!string.Equals(
                state.CompletionBindingRef,
                record.RecordId,
                StringComparison.Ordinal))
        {
            return Denied(
                CompletionBindingRefMismatchReason,
                CompletionBindingInvalidErrorCode,
                stateRef);
        }

        if (!JsonDesignConversionPostEvaluationStateStore
                .IsCanonicalNoReservationSentinel(state) &&
            string.IsNullOrWhiteSpace(state.LedgerRecordId))
        {
            return Denied(
                LedgerRecordIdMissingReason,
                StateIdentityMismatchErrorCode,
                stateRef);
        }

        return DesignConversionPostEvaluationTrainingEligibility.Eligible(
            stateRef,
            state.Version);
    }

    private static MarkerSnapshotValidation ValidateMarkerSnapshot(
        DesignConversionLocalArtifactDbRecord? record)
    {
        if (record is null)
        {
            return new MarkerSnapshotValidation(
                string.Empty,
                Denied(
                    MarkerSetIncompleteReason,
                    MarkerSetIncompleteErrorCode));
        }

        var metadata = record.Metadata;
        if (metadata is null ||
            !metadata.TryGetValue(
                JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey,
                out var stateRef) ||
            !metadata.TryGetValue(
                JsonDesignConversionPostEvaluationStateStore.PersistenceStatusMetadataKey,
                out var persistenceStatus) ||
            !metadata.TryGetValue(
                JsonDesignConversionPostEvaluationStateStore.TrainingEligibleMetadataKey,
                out var trainingEligible))
        {
            return new MarkerSnapshotValidation(
                string.Empty,
                Denied(
                    MarkerSetIncompleteReason,
                    MarkerSetIncompleteErrorCode,
                    stateRef: metadata is not null &&
                              metadata.TryGetValue(
                                  JsonDesignConversionPostEvaluationStateStore.StateRefMetadataKey,
                                  out var partialStateRef)
                        ? partialStateRef
                        : null));
        }

        if (!string.Equals(
                persistenceStatus,
                JsonDesignConversionPostEvaluationStateStore.PersistenceProvisional,
                StringComparison.Ordinal))
        {
            return new MarkerSnapshotValidation(
                stateRef,
                Denied(
                    MetadataPersistenceStatusInvalidReason,
                    MetadataSnapshotInvalidErrorCode,
                    stateRef));
        }

        if (!string.Equals(trainingEligible, bool.FalseString, StringComparison.Ordinal))
        {
            return new MarkerSnapshotValidation(
                stateRef,
                Denied(
                    MetadataTrainingEligibilityInvalidReason,
                    MetadataSnapshotInvalidErrorCode,
                    stateRef));
        }

        if (!JsonDesignConversionPostEvaluationStateStore.IsCanonicalStateRef(
                stateRef))
        {
            return new MarkerSnapshotValidation(
                stateRef,
                Denied(
                    StateRefInvalidReason,
                    StateRefInvalidErrorCode,
                    stateRef));
        }

        return new MarkerSnapshotValidation(stateRef, null);
    }

    private static DesignConversionPostEvaluationTrainingEligibility Denied(
        string reason,
        string errorCode,
        string? stateRef = null)
        => DesignConversionPostEvaluationTrainingEligibility.Denied(
            reason,
            errorCode,
            stateRef);

    private sealed record MarkerSnapshotValidation(
        string StateRef,
        DesignConversionPostEvaluationTrainingEligibility? Denied);
}
