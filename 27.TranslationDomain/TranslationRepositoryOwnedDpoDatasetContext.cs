// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using AgentInfrastructure;
using ContinuousLearning;

/// <summary>
/// Carries the optional repository-owned DPO dataset identity for translation
/// train/evaluate runs. Disabled keeps the legacy seed/artifact route explicit.
/// </summary>
public sealed record TranslationRepositoryOwnedDpoDatasetContext(
    bool Enabled,
    RepositoryOwnedDpoDatasetAddress? Address,
    RepositoryOwnedDpoDatasetRequestIdentity? RequestIdentity,
    TranslationRepositoryOwnedDpoDatasetAuthoringContext?
        CleanMissAuthoringContext = null)
{
    public static TranslationRepositoryOwnedDpoDatasetContext Disabled { get; } =
        new(
            Enabled: false,
            Address: null,
            RequestIdentity: null,
            CleanMissAuthoringContext: null);

    public const string InvalidErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_DPO_CONTEXT_INVALID";

    public const string InvalidSafeMessage =
        "Translation repository-owned DPO dataset context is required and must point to a canonical address and request identity.";

    public OperationResult<bool> Validate()
    {
        if (!Enabled)
            return OperationResult<bool>.Ok(true);

        if (Address is null ||
            !RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                Address))
        {
            return OperationResult<bool>.Fail(
                InvalidSafeMessage,
                InvalidErrorCode);
        }

        var requestIdentityIsValid = RequestIdentity is not null &&
            RepositoryOwnedDpoDatasetManifestSealer.IsValidRequestIdentity(
                RequestIdentity);
        var authoringContextIsValid = CleanMissAuthoringContext is not null &&
            CleanMissAuthoringContext.Validate().IsSuccess;
        if (!requestIdentityIsValid && !authoringContextIsValid)
        {
            // WHY: an existing dataset is opened by its exact sealed identity,
            // while a genuinely missing dataset needs enough information to
            // build that identity before the first teacher call. Accepting
            // neither would silently revive the legacy paid-row projection.
            return OperationResult<bool>.Fail(
                InvalidSafeMessage,
                InvalidErrorCode);
        }

        if (RequestIdentity is not null && !requestIdentityIsValid)
        {
            return OperationResult<bool>.Fail(
                InvalidSafeMessage,
                InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }
}
