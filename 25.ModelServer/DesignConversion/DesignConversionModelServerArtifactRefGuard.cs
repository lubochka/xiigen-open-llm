// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using AgentInfrastructure;

internal static class DesignConversionModelServerArtifactRefGuard
{
    public const string ErrorCode = "DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED";

    public static OperationResult<bool> Validate(
        IReadOnlyList<DesignConversionModelServerArtifactRef> artifacts,
        string subject,
        IDesignConversionModelServerArtifactRefResolver? artifactRefResolver = null)
    {
        foreach (var artifact in artifacts)
        {
            if (string.IsNullOrWhiteSpace(artifact.ArtifactId) ||
                string.IsNullOrWhiteSpace(artifact.Kind) ||
                string.IsNullOrWhiteSpace(artifact.Uri) ||
                string.IsNullOrWhiteSpace(artifact.Sha256))
            {
                return OperationResult<bool>.Fail(
                    $"{subject} must include artifact id, kind, uri ref, and sha256 ref before visual proof.",
                    ErrorCode);
            }

            if (artifactRefResolver is not null)
            {
                var resolved = artifactRefResolver.Validate(artifact, subject);
                if (!resolved.IsSuccess)
                    return resolved;
                continue;
            }

            if (!artifact.Sha256.Trim().StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) ||
                !IsAllowedArtifactRefUri(artifact.Uri))
            {
                return OperationResult<bool>.Fail(
                    $"{subject} must use refs-only artifact or sha256 proof references before visual proof.",
                    ErrorCode);
            }
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool IsAllowedArtifactRefUri(string value)
        => value.Trim().StartsWith("artifact://", StringComparison.OrdinalIgnoreCase) ||
           value.Trim().StartsWith("sha256:", StringComparison.OrdinalIgnoreCase);
}
