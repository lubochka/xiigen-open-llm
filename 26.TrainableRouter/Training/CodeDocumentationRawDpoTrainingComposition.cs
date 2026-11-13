// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
namespace TrainableRouter.Training; // Keep raw input composition beside the existing training client.

using AgentInfrastructure; // Return typed refusal instead of throwing business errors.
using ContinuousLearning; // Reuse sealed dataset receipts and exact rich pairs.

/// <summary>Strict raw reopen → existing ModelServer request; this adapter neither judges nor trains.</summary>
public static class CodeDocumentationRawDpoTrainingComposition // Compose existing owners rather than creating another trainer.
{ // No model, optimizer, selection receipt or training grant is manufactured here.
    public static OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest> CreateRequest( // Expose the live operator entry point as a business result.
        CodeDocumentationRawDpoLessonSource source, CodeDocumentationRepositoryOwnedDpoDatasetAuthoringContext context, // Require the already admitted raw source and authoring identity.
        Uri baseUri, string trainingRunId, IReadOnlyList<string> provenanceRefs, TimeSpan timeout) // Leave endpoint and invocation identity to the operator.
    { // Only a successful real disk reopen may produce this request.
        var reopened = CodeDocumentationRawDpoLessonAuthoring.Reopen(source, context); // Revalidate source bytes, request identity and exact pair with no teacher factory.
        if (!reopened.IsSuccess || reopened.Value is null) return reopened.MapError<CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest>(); // Preserve the actual reader refusal.
        var dataset = reopened.Value; // Copy values from this verified reader result, never intake constants.
        var manifest = dataset.Manifest; // Retain the exact canonical manifest association.
        var receipt = new CodeDocumentationRepositoryOwnedDpoDatasetReceipt( // Use the existing receipt contract for the existing coordinator.
            RepositoryOwnedDpoDatasetResolutionSource.ExistingSealedRepositoryDataset, // This method strictly reopened an existing dataset.
            manifest.ManifestSha256, source.Address.ManifestRelativePath, manifest.PairCount, // Retain actual canonical hash, address and pair count.
            manifest.RequestIdentity.RequestIdentitySha256, manifest.OrderedPairs.ToArray()); // Preserve ordered pair paths and file hashes.
        var run = new CodeDocumentationTrainingRunResult( // Legacy transport carries the receipt and pair, with observations explicitly absent.
            0, 0, 0, 0, 0, 0, // No routing examples or correct outcomes were measured.
            0, 0, 0, 0, 0, 0, 0, // No routing or gate accuracy was measured.
            0, 0, 0, [], receipt, dataset.Pairs.ToArray(), []); // No routing resources/cases/pairs; preserve exact repository-owned pairs.
        var request = new CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest( // Enter the existing HTTP client without introducing training authority.
            baseUri, manifest.Domain, manifest.DesignPattern, manifest.DatasetVersion, trainingRunId, run, provenanceRefs, timeout) // Copy dataset coordinates from its real manifest.
        { RoutingDisposition = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.RoutingNotPerformed }; // Mark absent routing explicitly rather than pretending it completed.
        var validation = CodeDocumentationRepositoryOwnedDpoTrainingWireContract.ValidateRoutingDisposition(run, request.RoutingDisposition, manifest.Domain); // Apply the same wire invariant before returning.
        return validation.IsSuccess ? OperationResult<CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest>.Ok(request) // Return only the concrete composed request.
            : validation.MapError<CodeDocumentationRepositoryOwnedDpoTrainingModelServerRequest>(); // Propagate contradictions without a model call.
    } // The existing client/endpoint still validate URL, receipt, startup, lease, quality and publication.
} // Training and scientific acceptance remain separate downstream operations.
