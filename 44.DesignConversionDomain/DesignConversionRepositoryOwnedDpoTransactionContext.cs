// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using ContinuousLearning;
using AgentInfrastructure;

/// <summary>
/// Carries the repository-owned DPO dataset transaction identity on the live
/// DesignConversion request path before teacher inference is allowed to run.
/// </summary>
public sealed record DesignConversionRepositoryOwnedDpoTransactionContext(
    bool Enabled,
    RepositoryOwnedDpoDatasetAddress? Address,
    RepositoryOwnedDpoDatasetRequestIdentity? RequestIdentity)
{
    // WHY: callers need one explicit value for legacy/disabled routes so old
    // tests and local deterministic experiments do not accidentally opt into
    // repository-owned fail-closed behaviour.
    public static DesignConversionRepositoryOwnedDpoTransactionContext Disabled { get; } =
        new(
            Enabled: false,
            Address: null,
            RequestIdentity: null);

    // WHY: the error is stable across API, coordinator, and teacher boundaries
    // so enabled reuse routes can fail closed before a fake or real teacher is
    // invoked.
    public const string InvalidErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_DPO_CONTEXT_INVALID";

    // WHY: the public result must not leak repository paths or request details;
    // this plain safe message explains the missing prerequisite.
    public const string InvalidSafeMessage =
        "DesignConversion repository-owned DPO transaction context is required and must be canonical before teacher inference.";

    // WHY: successful metadata must be hash/path-reference only so the model
    // server can prove threading without exposing raw paid references.
    public const string EnabledMetadataKey =
        "repository_owned_dpo_transaction_enabled";

    // WHY: reports and tests need to distinguish disabled legacy mode from an
    // enabled context that has been validated on the production path.
    public const string ValidationMetadataKey =
        "repository_owned_dpo_transaction_context_valid";

    // WHY: the address is represented by its repository-relative manifest path,
    // not by absolute local paths, to keep public telemetry portable.
    public const string ManifestRelativePathMetadataKey =
        "repository_owned_dpo_dataset_manifest_relative_path";

    // WHY: the identity fingerprint is already the canonical manifest authority
    // for matching paid-reference hashes, open teacher, policy, and creator.
    public const string RequestIdentitySha256MetadataKey =
        "repository_owned_dpo_request_identity_sha256";

    // WHY: every enabled route uses this one validation gate before inference
    // so invalid addresses cannot be treated as advisory metadata.
    public OperationResult<bool> Validate()
    {
        if (!Enabled)
            return OperationResult<bool>.Ok(true);

        if (Address is null
            || RequestIdentity is null
            || !RepositoryOwnedTrainingArtifactLayout
                .IsValidDpoDatasetAddress(Address)
            || !RepositoryOwnedDpoDatasetManifestSealer
                .IsValidRequestIdentity(RequestIdentity))
        {
            return OperationResult<bool>.Fail(
                InvalidSafeMessage,
                InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    // WHY: metadata emission is centralized so each boundary records the same
    // validated context without reformatting identity material differently.
    public IReadOnlyDictionary<string, string> ToBoundaryMetadata()
    {
        var validation = Validate();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [EnabledMetadataKey] = Enabled.ToString(),
            [ValidationMetadataKey] = validation.IsSuccess.ToString()
        };

        if (Enabled && Address is not null && RequestIdentity is not null)
        {
            metadata[ManifestRelativePathMetadataKey] =
                Address.ManifestRelativePath;
            metadata[RequestIdentitySha256MetadataKey] =
                RequestIdentity.RequestIdentitySha256;
        }

        return metadata;
    }
}
