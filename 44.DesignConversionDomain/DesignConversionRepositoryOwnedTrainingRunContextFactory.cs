// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// Repository-owned specification for one DesignConversion train/evaluate run.
/// Dataset fields name an already sealed DPO dataset; model fields name the
/// repository checkpoint line that may be loaded, resumed, and published later.
/// </summary>
public sealed record DesignConversionRepositoryOwnedTrainingRunContextSpec(
    string RepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string DatasetVersion,
    string ModelIdSlug,
    string ModelVersion,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    bool AllowColdStart,
    string? ColdStartReason,
    IReadOnlyList<string> ProvenanceRefs);

/// <summary>
/// Enabled repository-owned contexts that can be passed into DesignConversion
/// training after the saved DPO dataset has been reopened and checked.
/// </summary>
public sealed record DesignConversionRepositoryOwnedTrainingRunContexts(
    DesignConversionRepositoryOwnedDpoTransactionContext DpoTransactionContext,
    DesignConversionRepositoryOwnedModelCheckpointContext ModelCheckpointContext);

/// <summary>
/// Bridges a saved DesignConversion DPO dataset to the saved model checkpoint
/// policy. It does not create DPO rows, train a model, write weights, or create
/// runtime directories; it only proves that the next run can reuse repository
/// assets instead of starting from scratch.
/// </summary>
public static class DesignConversionRepositoryOwnedTrainingRunContextFactory
{
    public const string InvalidErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_TRAINING_RUN_CONTEXT_INVALID";

    public const string DpoDatasetUnavailableErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_TRAINING_RUN_CONTEXT_DPO_DATASET_UNAVAILABLE";

    public const string UnavailableErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_TRAINING_RUN_CONTEXT_UNAVAILABLE";

    private const string InvalidSafeMessage =
        "DesignConversion repository-owned training run context specification is invalid.";

    private const string DpoDatasetUnavailableSafeMessage =
        "DesignConversion repository-owned training run context requires an existing sealed DPO dataset in the repository.";

    private const string UnavailableSafeMessage =
        "DesignConversion repository-owned training run context could not safely open the sealed DPO dataset.";

    public static OperationResult<DesignConversionRepositoryOwnedTrainingRunContexts>
        Create(DesignConversionRepositoryOwnedTrainingRunContextSpec spec)
    {
        if (spec is null)
            return Invalid<DesignConversionRepositoryOwnedTrainingRunContexts>();

        try
        {
            var dpoAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateDpoDatasetAddress(
                    spec.RepositoryRoot,
                    spec.DomainSlug,
                    spec.DesignPatternSlug,
                    spec.DatasetVersion);
            if (!dpoAddress.IsSuccess || dpoAddress.Value is null)
                return Invalid<DesignConversionRepositoryOwnedTrainingRunContexts>();

            var modelAddress = RepositoryOwnedTrainingArtifactLayout
                .CreateModelCheckpointAddress(
                    spec.RepositoryRoot,
                    spec.DomainSlug,
                    spec.DesignPatternSlug,
                    spec.ModelIdSlug,
                    spec.ModelVersion,
                    step: 1);
            if (!modelAddress.IsSuccess || modelAddress.Value is null)
                return Invalid<DesignConversionRepositoryOwnedTrainingRunContexts>();

            var openedIdentity = RepositoryOwnedDpoDatasetManifestSealer
                .OpenManifestIdentityOnly(dpoAddress.Value);
            if (!openedIdentity.IsSuccess || openedIdentity.Value is null)
                return DpoDatasetUnavailable<
                    DesignConversionRepositoryOwnedTrainingRunContexts>();

            var requestIdentity = openedIdentity.Value.RequestIdentity;
            if (!RepositoryOwnedDpoDatasetManifestSealer
                    .IsValidRequestIdentity(requestIdentity))
            {
                return DpoDatasetUnavailable<
                    DesignConversionRepositoryOwnedTrainingRunContexts>();
            }

            var openedSealed = RepositoryOwnedDpoDatasetManifestSealer
                .OpenSealed(dpoAddress.Value, requestIdentity);
            if (!openedSealed.IsSuccess || openedSealed.Value is null)
            {
                // WHY: the manifest identity alone is not enough for training.
                // If a pair file was removed or rewritten, the next run must
                // stop instead of silently asking the open teacher to rebuild
                // data and hide the repository drift.
                return DpoDatasetUnavailable<
                    DesignConversionRepositoryOwnedTrainingRunContexts>();
            }

            var dpoContext =
                new DesignConversionRepositoryOwnedDpoTransactionContext(
                    Enabled: true,
                    dpoAddress.Value,
                    requestIdentity);
            var dpoValidation = dpoContext.Validate();
            if (!dpoValidation.IsSuccess)
                return dpoValidation.MapError<
                    DesignConversionRepositoryOwnedTrainingRunContexts>();

            var provenanceRefs = SnapshotProvenanceRefs(spec.ProvenanceRefs);
            if (provenanceRefs is null)
                return Invalid<DesignConversionRepositoryOwnedTrainingRunContexts>();

            // WHY: model address validation is done through the shared layout so
            // the factory cannot smuggle a caller-supplied output directory into
            // the training request. The returned request still names the model
            // line, not a concrete step that would be created here.
            var modelContext =
                new DesignConversionRepositoryOwnedModelCheckpointContext(
                    Enabled: true,
                    Request:
                    new DesignConversionRepositoryOwnedModelCheckpointRequest(
                        modelAddress.Value.RepositoryRoot,
                        modelAddress.Value.DomainSlug,
                        modelAddress.Value.DesignPatternSlug,
                        modelAddress.Value.ModelIdSlug,
                        modelAddress.Value.ModelVersion,
                        spec.RuntimeCompatibility,
                        spec.AllowColdStart,
                        spec.ColdStartReason,
                        provenanceRefs));
            var modelValidation = modelContext.Validate();
            if (!modelValidation.IsSuccess)
                return modelValidation.MapError<
                    DesignConversionRepositoryOwnedTrainingRunContexts>();

            return OperationResult<DesignConversionRepositoryOwnedTrainingRunContexts>
                .Ok(new DesignConversionRepositoryOwnedTrainingRunContexts(
                    dpoContext,
                    modelContext));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DesignConversionRepositoryOwnedTrainingRunContexts>
                .Fail(UnavailableSafeMessage, UnavailableErrorCode);
        }
    }

    private static IReadOnlyList<string>? SnapshotProvenanceRefs(
        IReadOnlyList<string>? provenanceRefs)
        => provenanceRefs is null
            ? null
            : Array.AsReadOnly(provenanceRefs.ToArray());

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(InvalidSafeMessage, InvalidErrorCode);

    private static OperationResult<T> DpoDatasetUnavailable<T>()
        => OperationResult<T>.Fail(
            DpoDatasetUnavailableSafeMessage,
            DpoDatasetUnavailableErrorCode);
}
