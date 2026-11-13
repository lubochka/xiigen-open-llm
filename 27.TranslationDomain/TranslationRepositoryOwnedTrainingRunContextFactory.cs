// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// Repository-owned specification for one Translation DPO train/evaluate run.
/// The dataset slugs identify an already sealed DPO dataset; model fields
/// identify where the runner may later load or publish compatible checkpoints.
/// </summary>
public sealed record TranslationRepositoryOwnedTrainingRunContextSpec(
    string RepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string DatasetVersion,
    string ModelIdSlug,
    string ModelVersion,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    bool AllowColdStart,
    string? ColdStartReason,
    IReadOnlyList<string> ProvenanceRefs,
    TranslationRepositoryOwnedDpoDatasetAuthoringContext?
        CleanMissAuthoringContext = null);

/// <summary>
/// Enabled repository-owned contexts that can be passed into Translation DPO
/// train/evaluate options after the sealed dataset has been reopened.
/// </summary>
public sealed record TranslationRepositoryOwnedTrainingRunContexts(
    TranslationRepositoryOwnedDpoDatasetContext DpoDatasetContext,
    TranslationRepositoryOwnedModelCheckpointContext ModelCheckpointContext);

/// <summary>
/// Bridges repository-owned Translation DPO dataset identity to model checkpoint
/// policy without creating datasets, weights, manifests, or runner outputs.
/// </summary>
public static class TranslationRepositoryOwnedTrainingRunContextFactory
{
    public const string InvalidErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_TRAINING_RUN_CONTEXT_INVALID";

    public const string DpoDatasetUnavailableErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_TRAINING_RUN_CONTEXT_DPO_DATASET_UNAVAILABLE";

    public const string UnavailableErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_TRAINING_RUN_CONTEXT_UNAVAILABLE";

    private const string InvalidSafeMessage =
        "Translation repository-owned training run context specification is invalid.";

    private const string DpoDatasetUnavailableSafeMessage =
        "Translation repository-owned training run context requires either an existing sealed DPO dataset or an explicit local Qwen clean-miss authoring context.";

    private const string UnavailableSafeMessage =
        "Translation repository-owned training run context could not safely open the sealed DPO dataset.";

    public static OperationResult<TranslationRepositoryOwnedTrainingRunContexts>
        Create(TranslationRepositoryOwnedTrainingRunContextSpec spec)
    {
        if (spec is null)
            return Invalid<TranslationRepositoryOwnedTrainingRunContexts>();

        try
        {
            var address = RepositoryOwnedTrainingArtifactLayout
                .CreateDpoDatasetAddress(
                    spec.RepositoryRoot,
                    spec.DomainSlug,
                    spec.DesignPatternSlug,
                    spec.DatasetVersion);
            if (!address.IsSuccess || address.Value is null)
                return Invalid<TranslationRepositoryOwnedTrainingRunContexts>();

            var authoringValidation = spec.CleanMissAuthoringContext?.Validate();
            if (authoringValidation is { IsSuccess: false })
            {
                return authoringValidation.MapError<
                    TranslationRepositoryOwnedTrainingRunContexts>();
            }

            var opened = RepositoryOwnedDpoDatasetManifestSealer
                .OpenManifestIdentityOnly(address.Value);
            RepositoryOwnedDpoDatasetRequestIdentity? requestIdentity = null;
            if (opened.IsSuccess && opened.Value is not null)
            {
                requestIdentity = opened.Value.RequestIdentity;
                if (!RepositoryOwnedDpoDatasetManifestSealer
                        .IsValidRequestIdentity(requestIdentity))
                {
                    return DpoDatasetUnavailable<
                        TranslationRepositoryOwnedTrainingRunContexts>();
                }

                if (spec.CleanMissAuthoringContext is { } authoring &&
                    (requestIdentity.OpenTeacher != authoring.OpenTeacher ||
                     requestIdentity.GenerationPolicy !=
                         authoring.GenerationPolicy ||
                     requestIdentity.Creator != authoring.Creator))
                {
                    return DpoDatasetUnavailable<
                        TranslationRepositoryOwnedTrainingRunContexts>();
                }
            }
            else if (File.Exists(address.Value.ManifestPath) ||
                     spec.CleanMissAuthoringContext is null)
            {
                // WHY: only a genuinely absent manifest is a clean miss.
                // Corrupt or partially written repository data must fail
                // closed instead of being overwritten by a new teacher run.
                return DpoDatasetUnavailable<
                    TranslationRepositoryOwnedTrainingRunContexts>();
            }

            // WHY: the sealed manifest is the only authority for the DPO
            // request identity at this stage. Recomputing it from slugs or
            // placeholders would silently train against a different request.
            var dpoContext = new TranslationRepositoryOwnedDpoDatasetContext(
                Enabled: true,
                Address: address.Value,
                RequestIdentity: requestIdentity,
                CleanMissAuthoringContext:
                    spec.CleanMissAuthoringContext);
            var dpoValidation = dpoContext.Validate();
            if (!dpoValidation.IsSuccess)
                return dpoValidation
                    .MapError<TranslationRepositoryOwnedTrainingRunContexts>();

            var provenanceRefs = SnapshotProvenanceRefs(spec.ProvenanceRefs);
            if (provenanceRefs is null)
                return Invalid<TranslationRepositoryOwnedTrainingRunContexts>();

            // WHY: a clean-miss has no trusted teacher/reference/config identity
            // to bind here. This factory only prepares a future model run after
            // the dataset sealer has already made those identities immutable.
            var modelContext =
                new TranslationRepositoryOwnedModelCheckpointContext(
                    Enabled: true,
                    Request: new TranslationRepositoryOwnedModelCheckpointRequest(
                        Address:
                            new TranslationRepositoryOwnedModelCheckpointAddress(
                                address.Value.RepositoryRoot,
                                address.Value.DomainSlug,
                                address.Value.DesignPatternSlug,
                                spec.ModelIdSlug,
                                spec.ModelVersion),
                        RuntimeCompatibility: spec.RuntimeCompatibility,
                        AllowColdStart: spec.AllowColdStart,
                        ColdStartReason: spec.ColdStartReason,
                        ProvenanceRefs: provenanceRefs));
            var modelValidation = modelContext.Validate();
            if (!modelValidation.IsSuccess)
                return modelValidation
                    .MapError<TranslationRepositoryOwnedTrainingRunContexts>();

            return OperationResult<TranslationRepositoryOwnedTrainingRunContexts>
                .Ok(new TranslationRepositoryOwnedTrainingRunContexts(
                    dpoContext,
                    modelContext));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<TranslationRepositoryOwnedTrainingRunContexts>
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
