// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System;
using System.Collections.Generic;
using System.Linq;

namespace DesignConversionDomain;

public sealed record DesignConversionModelAuthoritySnapshot(
    bool TrainedModelAuthority,
    bool NotTrainedModelAuthority,
    string TrainedModelAuthorityValue,
    string NotTrainedModelAuthorityValue,
    string BoundarySource);

public static class DesignConversionModelAuthorityBoundary
{
    public const string TrainedModelAuthorityKey = "trained_model_authority";
    public const string NotTrainedModelAuthorityKey = "not_trained_model_authority";
    public const string TrainedModelAuthorityValue = "False";
    public const string NotTrainedModelAuthorityValue = "True";
    public const string ActiveTrainedModelAuthorityValue = "True";
    public const string ActiveNotTrainedModelAuthorityValue = "False";
    public const string BoundarySourceKey = "model_authority_boundary_source";
    public const string NoTrustedDirectDpoAuthoritySource = "no_trusted_direct_dpo_acceptance_authority";
    public const string TrustedDirectDpoRuntimeFlowAuthoritySource =
        "server_owned_direct_dpo_runtime_flow_acceptance_authority";

    public static DesignConversionModelAuthoritySnapshot NoTrustedAuthority { get; } = new(
        TrainedModelAuthority: false,
        NotTrainedModelAuthority: true,
        TrainedModelAuthorityValue,
        NotTrainedModelAuthorityValue,
        NoTrustedDirectDpoAuthoritySource);

    public static DesignConversionModelAuthoritySnapshot TrustedDirectDpoRuntimeFlowAuthority { get; } = new(
        TrainedModelAuthority: true,
        NotTrainedModelAuthority: false,
        ActiveTrainedModelAuthorityValue,
        ActiveNotTrainedModelAuthorityValue,
        TrustedDirectDpoRuntimeFlowAuthoritySource);

    public static DesignConversionModelAuthoritySnapshot FromTrustedDirectDpoAcceptanceAuthority(
        bool acceptanceAuthorityActive)
        => acceptanceAuthorityActive
            ? TrustedDirectDpoRuntimeFlowAuthority
            : NoTrustedAuthority;

    public static DesignConversionModelAuthoritySnapshot FromTrustedDirectDpoRuntimeEvidence(
        IReadOnlyList<string>? evidenceRefs)
        => (evidenceRefs ?? []).Contains(
            DesignConversionMachine.DpoPolicyDirectInferenceAcceptanceAuthorityEvidence,
            StringComparer.Ordinal)
            ? TrustedDirectDpoRuntimeFlowAuthority
            : NoTrustedAuthority;

    public static DesignConversionModelAuthoritySnapshot FromTrustedBoundaryMetadata(
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is not null &&
            metadata.TryGetValue(BoundarySourceKey, out var source) &&
            string.Equals(source, TrustedDirectDpoRuntimeFlowAuthoritySource, StringComparison.Ordinal) &&
            metadata.TryGetValue(TrainedModelAuthorityKey, out var trainedValue) &&
            metadata.TryGetValue(NotTrainedModelAuthorityKey, out var notTrainedValue) &&
            string.Equals(trainedValue, ActiveTrainedModelAuthorityValue, StringComparison.Ordinal) &&
            string.Equals(notTrainedValue, ActiveNotTrainedModelAuthorityValue, StringComparison.Ordinal))
        {
            return TrustedDirectDpoRuntimeFlowAuthority;
        }

        return NoTrustedAuthority;
    }

    public static void ApplyTrustedSnapshot(
        IDictionary<string, string> metadata,
        DesignConversionModelAuthoritySnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(snapshot);

        metadata[TrainedModelAuthorityKey] = snapshot.TrainedModelAuthorityValue;
        metadata[NotTrainedModelAuthorityKey] = snapshot.NotTrainedModelAuthorityValue;
        metadata[BoundarySourceKey] = snapshot.BoundarySource;
    }
}
