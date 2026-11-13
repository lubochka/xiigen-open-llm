// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

/// <summary>
/// Publishes the full model produced by an incremental-training session as the
/// next sealed checkpoint in the canonical repository-owned model tree.
/// </summary>
public static class RepositoryOwnedIncrementalTrainingCheckpointPublisher
{
    public const string InvalidErrorCode =
        "REPOSITORY_OWNED_INCREMENTAL_TRAINING_CHECKPOINT_INVALID";

    public static OperationResult<RepositoryOwnedModelCheckpointManifest>
        PublishNext(
            IncrementalTrainingSession session,
            RepositoryOwnedDpoDatasetManifestIdentity dpoDatasetManifest,
            DateTimeOffset createdAtUtc,
            IReadOnlyList<string> provenanceRefs)
    {
        if (session is null
            || dpoDatasetManifest is null
            || provenanceRefs is null
            || session.CheckpointStart is null
            || session.CheckpointStart.RepositoryStartRequest is null)
        {
            return Invalid<RepositoryOwnedModelCheckpointManifest>();
        }

        var startRequest = session.CheckpointStart.RepositoryStartRequest;
        if (!CurrentDpoDatasetManifestAllowsPublication(
                startRequest,
                dpoDatasetManifest))
        {
            return DpoDatasetManifestMismatch<
                RepositoryOwnedModelCheckpointManifest>();
        }

        var lineage = BuildLineage(session, startRequest);
        if (!lineage.IsSuccess || lineage.Value is null)
            return Invalid<RepositoryOwnedModelCheckpointManifest>();
        var publicationLineage = lineage.Value;

        RepositoryOwnedModelCheckpointAddress address;
        try
        {
            var addressResult =
                RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
                    startRequest.RepositoryRoot,
                    startRequest.DomainSlug,
                    startRequest.DesignPatternSlug,
                    startRequest.ModelIdSlug,
                    startRequest.ModelVersion,
                    publicationLineage.Step);
            if (!addressResult.IsSuccess || addressResult.Value is null)
                return addressResult.MapError<
                    RepositoryOwnedModelCheckpointManifest>();
            address = addressResult.Value;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid<RepositoryOwnedModelCheckpointManifest>();
        }

        var request = new RepositoryOwnedModelCheckpointManifestRequest(
            ParentCheckpoint: publicationLineage.ParentCheckpoint,
            ColdStartReason: publicationLineage.ColdStartReason,
            DpoDatasetManifest: dpoDatasetManifest,
            RuntimeCompatibility: startRequest.RuntimeCompatibility,
            CreatedAtUtc: createdAtUtc,
            ProvenanceRefs: provenanceRefs);

        // WHY: a resumed session writes parent.Step+1 and the authorized
        // repository-owned cold start writes step 000001. Without that first
        // full-model checkpoint, the next process has no sealed model to load
        // and will start from zero again instead of continuing the chain.
        return RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
            address,
            session.Model.SaveCheckpointToStream,
            request);
    }

    private static OperationResult<CheckpointPublicationLineage> BuildLineage(
        IncrementalTrainingSession session,
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest)
    {
        switch (session.CheckpointStart!.Source)
        {
            case IncrementalTrainingCheckpointStartSource
                .RepositoryOwnedModelCheckpoint:
            {
                var parent = session.ResumeParentCheckpoint;
                if (parent is null
                    || session.CheckpointStart.ColdStart is not null
                    || !StartRequestMatchesParent(startRequest, parent))
                {
                    return Invalid<CheckpointPublicationLineage>();
                }

                try
                {
                    return OperationResult<CheckpointPublicationLineage>.Ok(
                        new CheckpointPublicationLineage(
                            checked(parent.Step + 1),
                            new RepositoryOwnedModelCheckpointParentIdentity(
                                parent.RepositoryRelativeManifestPath,
                                parent.Step,
                                parent.WeightsSha256,
                                parent.WeightsByteLength),
                            string.Empty));
                }
                catch (Exception exception)
                    when (SnapshotExceptionGraphClassifier.IsNonFatal(
                        exception))
                {
                    return Invalid<CheckpointPublicationLineage>();
                }
            }

            case IncrementalTrainingCheckpointStartSource
                .RetainedBaseCheckpointColdStart:
            {
                var coldStart = session.CheckpointStart.ColdStart;
                if (session.ResumeParentCheckpoint is not null
                    || coldStart is null
                    || !ReferenceEquals(
                        coldStart.RepositoryStartRequest,
                        startRequest)
                    || !startRequest.ColdStartPolicy.IsAuthorized
                    || !StringComparer.Ordinal.Equals(
                        startRequest.ColdStartPolicy.HumanReason,
                        coldStart.HumanReason))
                {
                    return Invalid<CheckpointPublicationLineage>();
                }

                return OperationResult<CheckpointPublicationLineage>.Ok(
                    new CheckpointPublicationLineage(
                        Step: 1,
                        ParentCheckpoint: null,
                        ColdStartReason: coldStart.HumanReason));
            }

            default:
                return Invalid<CheckpointPublicationLineage>();
        }
    }

    private static bool StartRequestMatchesParent(
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest,
        RepositoryOwnedModelCheckpointManifest parent)
        => StringComparer.Ordinal.Equals(
               startRequest.DomainSlug,
               parent.Domain)
           && StringComparer.Ordinal.Equals(
               startRequest.DesignPatternSlug,
               parent.DesignPattern)
           && StringComparer.Ordinal.Equals(
               startRequest.ModelIdSlug,
               parent.ModelId)
           && StringComparer.Ordinal.Equals(
               startRequest.ModelVersion,
               parent.Version)
           && RuntimeCompatibilityEquals(
               startRequest.RuntimeCompatibility,
               parent.RuntimeCompatibility);

    private static bool CurrentDpoDatasetManifestAllowsPublication(
        RepositoryOwnedIncrementalTrainingSessionStartRequest startRequest,
        RepositoryOwnedDpoDatasetManifestIdentity dpoDatasetManifest)
    {
        var currentDpoDatasetManifest =
            startRequest.CurrentDpoDatasetManifest;
        if (currentDpoDatasetManifest is null)
        {
            // WHY: a caller-only DPO identity is not session provenance. The
            // repository-owned checkpoint must prove which sealed dataset the
            // training session selected before any child bytes or owned staging
            // files can exist.
            return false;
        }

        // WHY: IncrementalTrainer already validates this binding on the normal
        // training path, but PublishNext is also a direct publication boundary.
        // A direct caller must not swap the session-bound current DPO dataset
        // between training and checkpoint sealing, so the publisher repeats the
        // same durable binding comparison (repository-relative manifest path
        // plus sealed manifest SHA-256) before any checkpoint writer can run.
        return DpoDatasetManifestIdentityEquals(
            currentDpoDatasetManifest,
            dpoDatasetManifest);
    }

    private static bool DpoDatasetManifestIdentityEquals(
        RepositoryOwnedDpoDatasetManifestIdentity left,
        RepositoryOwnedDpoDatasetManifestIdentity right)
        => left is not null
           && right is not null
           && StringComparer.Ordinal.Equals(
               left.RepositoryRelativeManifestPath,
               right.RepositoryRelativeManifestPath)
           && StringComparer.Ordinal.Equals(
               left.ManifestSha256,
               right.ManifestSha256);

    private static bool RuntimeCompatibilityEquals(
        RepositoryOwnedModelRuntimeCompatibility? left,
        RepositoryOwnedModelRuntimeCompatibility? right)
        => left is not null
           && right is not null
           && StringComparer.Ordinal.Equals(left.ConfigSha256, right.ConfigSha256)
           && StringComparer.Ordinal.Equals(
               left.TopologySha256,
               right.TopologySha256)
           && StringComparer.Ordinal.Equals(
               left.TokenizerSha256,
               right.TokenizerSha256)
           && StringComparer.Ordinal.Equals(
               left.TrainerSha256,
               right.TrainerSha256)
           && StringComparer.Ordinal.Equals(left.SchemaSha256, right.SchemaSha256)
           && StringComparer.Ordinal.Equals(
               left.CompatibilityKey,
               right.CompatibilityKey);

    private static OperationResult<T> Invalid<T>()
        => OperationResult<T>.Fail(
            "A repository-owned resumed or authorized cold-start training session is required before publishing the next checkpoint.",
            InvalidErrorCode);

    private static OperationResult<T> DpoDatasetManifestMismatch<T>()
        => OperationResult<T>.Fail(
            "The repository-owned checkpoint publisher cannot publish a child checkpoint without the exact sealed DPO dataset identity bound to the session start request.",
            InvalidErrorCode);

    private sealed record CheckpointPublicationLineage(
        int Step,
        RepositoryOwnedModelCheckpointParentIdentity? ParentCheckpoint,
        string ColdStartReason);
}
