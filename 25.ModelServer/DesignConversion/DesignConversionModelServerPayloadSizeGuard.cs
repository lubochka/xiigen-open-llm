// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Globalization;
using AgentInfrastructure;
using DesignConversionDomain;

public static class DesignConversionModelServerPayloadSizeGuard
{
    public const string ErrorCode = "DESIGN_CONVERSION_PAYLOAD_TOO_LARGE";
    public const int MaxRawPayloadChars = 262_144;
    public const int MaxReadyJsonChars = 65_536;
    public const int MaxScalarChars = 4_096;
    public const int MaxMetadataEntries = 64;
    public const int MaxMetadataKeyChars = 256;
    public const int MaxMetadataValueChars = 4_096;
    public const int MaxArtifactRefsPerKind = 16;
    public const int MaxArtifactRefsTotal = 32;

    public static OperationResult<bool> ValidateReadyJson(
        string? readyJson,
        string subject)
        => ValidateString(subject, "readyJson", readyJson, MaxReadyJsonChars);

    public static OperationResult<bool> ValidateImprovementLoopPackage(
        DesignConversionModelServerImprovementLoopLocalCallerPackage? package,
        string subject)
    {
        if (package is null)
            return OperationResult<bool>.Ok(true);

        var loopId = ValidateString(subject, "loopId", package.LoopId, MaxScalarChars);
        if (!loopId.IsSuccess)
            return loopId;

        var metadata = ValidateMetadata(subject, "metadata", package.Metadata);
        if (!metadata.IsSuccess)
            return metadata;

        return ValidatePackage(package.InitialPackage, subject + ".initialPackage");
    }

    public static OperationResult<bool> ValidateImprovementLoopRequest(
        DesignConversionModelServerImprovementLoopRequest? request,
        string subject)
    {
        if (request is null)
            return OperationResult<bool>.Ok(true);

        var loopId = ValidateString(subject, "loopId", request.LoopId, MaxScalarChars);
        if (!loopId.IsSuccess)
            return loopId;

        var metadata = ValidateMetadata(subject, "metadata", request.Metadata);
        if (!metadata.IsSuccess)
            return metadata;

        if (request.MeasurementGateRequest is not null)
        {
            var measurement = DesignConversionMeasurementAdmission.Capture(
                request.MeasurementGateRequest);
            if (!measurement.IsSuccess)
                return measurement.MapError<bool>();
        }

        return ValidatePackage(request.InitialPackage, subject + ".initialPackage");
    }

    public static OperationResult<bool> ValidatePackage(
        DesignConversionModelServerLocalCallerPackage? package,
        string subject)
    {
        if (package is null)
            return OperationResult<bool>.Ok(true);

        var requestId = ValidateString(subject, "requestId", package.RequestId, MaxScalarChars);
        if (!requestId.IsSuccess)
            return requestId;

        var sourceKind = ValidateString(subject, "sourceKind", package.SourceKind, MaxScalarChars);
        if (!sourceKind.IsSuccess)
            return sourceKind;

        var designTree = ValidatePayloadRef(subject, "designTree", package.DesignTree);
        if (!designTree.IsSuccess)
            return designTree;

        var originalTree = ValidatePayloadRef(subject, "originalTree", package.OriginalTree);
        if (!originalTree.IsSuccess)
            return originalTree;

        var html = ValidateString(subject, "html", package.Html, MaxRawPayloadChars);
        if (!html.IsSuccess)
            return html;

        var css = ValidateString(subject, "css", package.Css, MaxRawPayloadChars);
        if (!css.IsSuccess)
            return css;

        var artifacts = ValidateArtifactRefs(subject, package.Screenshots, package.DesignRenders);
        if (!artifacts.IsSuccess)
            return artifacts;

        var budget = ValidateBudget(subject, package.Budget);
        if (!budget.IsSuccess)
            return budget;

        return ValidateMetadata(subject, "metadata", package.Metadata);
    }

    public static OperationResult<bool> ValidateApiRequest(
        DesignConversionModelServerApiRequest? request,
        string subject)
    {
        if (request is null)
            return OperationResult<bool>.Ok(true);

        var requestId = ValidateString(subject, "requestId", request.RequestId, MaxScalarChars);
        if (!requestId.IsSuccess)
            return requestId;

        var sourceKind = ValidateString(subject, "sourceKind", request.SourceKind.ToString(), MaxScalarChars);
        if (!sourceKind.IsSuccess)
            return sourceKind;

        var designTree = ValidatePayloadRef(subject, "designTree", request.DesignTree);
        if (!designTree.IsSuccess)
            return designTree;

        var originalTree = ValidatePayloadRef(subject, "originalTree", request.OriginalTree);
        if (!originalTree.IsSuccess)
            return originalTree;

        var html = ValidateString(subject, "html", request.Html, MaxRawPayloadChars);
        if (!html.IsSuccess)
            return html;

        var css = ValidateString(subject, "css", request.Css, MaxRawPayloadChars);
        if (!css.IsSuccess)
            return css;

        var artifacts = ValidateArtifactRefs(subject, request.Screenshots, request.DesignRenders);
        if (!artifacts.IsSuccess)
            return artifacts;

        var budget = ValidateBudget(subject, request.Budget);
        if (!budget.IsSuccess)
            return budget;

        return ValidateMetadata(subject, "metadata", request.Metadata);
    }

    private static OperationResult<bool> ValidatePayloadRef(
        string subject,
        string field,
        DesignConversionModelServerPayloadRef? payloadRef)
    {
        if (payloadRef is null || string.IsNullOrWhiteSpace(payloadRef.Payload))
        {
            return string.Equals(field, "designTree", StringComparison.Ordinal)
                ? OperationResult<bool>.Fail(
                    "DesignConversion inline design tree payload is required.",
                    "DESIGN_CONVERSION_SOURCE_TREE_REQUIRED")
                : OperationResult<bool>.Fail(
                    "DesignConversion inline original tree payload is required.",
                    "DESIGN_CONVERSION_ORIGINAL_TREE_REQUIRED");
        }

        var refId = ValidateString(subject, field + ".refId", payloadRef.RefId, MaxScalarChars);
        if (!refId.IsSuccess)
            return refId;

        var kind = ValidateString(subject, field + ".kind", payloadRef.Kind, MaxScalarChars);
        if (!kind.IsSuccess)
            return kind;

        var sha = ValidateString(subject, field + ".sha256", payloadRef.Sha256, MaxScalarChars);
        if (!sha.IsSuccess)
            return sha;

        var payload = ValidateString(subject, field + ".payload", payloadRef.Payload, MaxRawPayloadChars);
        if (!payload.IsSuccess)
            return payload;

        var format = ValidateString(subject, field + ".format", payloadRef.Format, MaxScalarChars);
        if (!format.IsSuccess)
            return format;

        return ValidateMetadata(subject, field + ".metadata", payloadRef.Metadata);
    }

    private static OperationResult<bool> ValidateArtifactRefs(
        string subject,
        IReadOnlyList<DesignConversionModelServerArtifactRef>? screenshots,
        IReadOnlyList<DesignConversionModelServerArtifactRef>? designRenders)
    {
        var screenshotCount = screenshots?.Count ?? 0;
        var designRenderCount = designRenders?.Count ?? 0;
        if (screenshotCount > MaxArtifactRefsPerKind)
            return TooLarge(subject, "screenshots.count", screenshotCount, MaxArtifactRefsPerKind);
        if (designRenderCount > MaxArtifactRefsPerKind)
            return TooLarge(subject, "designRenders.count", designRenderCount, MaxArtifactRefsPerKind);

        var total = screenshotCount + designRenderCount;
        if (total > MaxArtifactRefsTotal)
            return TooLarge(subject, "artifactRefs.count", total, MaxArtifactRefsTotal);

        var screenshotRefs = ValidateArtifactRefList(subject, "screenshots", screenshots);
        if (!screenshotRefs.IsSuccess)
            return screenshotRefs;

        return ValidateArtifactRefList(subject, "designRenders", designRenders);
    }

    private static OperationResult<bool> ValidateArtifactRefList(
        string subject,
        string field,
        IReadOnlyList<DesignConversionModelServerArtifactRef>? artifacts)
    {
        if (artifacts is null)
            return OperationResult<bool>.Ok(true);

        for (var index = 0; index < artifacts.Count; index++)
        {
            var artifact = artifacts[index];
            var prefix = $"{field}[{index.ToString(CultureInfo.InvariantCulture)}]";
            var artifactId = ValidateString(subject, prefix + ".artifactId", artifact.ArtifactId, MaxScalarChars);
            if (!artifactId.IsSuccess)
                return artifactId;

            var kind = ValidateString(subject, prefix + ".kind", artifact.Kind, MaxScalarChars);
            if (!kind.IsSuccess)
                return kind;

            var uri = ValidateString(subject, prefix + ".uri", artifact.Uri, MaxScalarChars);
            if (!uri.IsSuccess)
                return uri;

            var sha = ValidateString(subject, prefix + ".sha256", artifact.Sha256, MaxScalarChars);
            if (!sha.IsSuccess)
                return sha;

            var metadata = ValidateMetadata(subject, prefix + ".metadata", artifact.Metadata);
            if (!metadata.IsSuccess)
                return metadata;
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateBudget(
        string subject,
        DesignConversionModelServerBudget? budget)
    {
        if (budget is null)
            return OperationResult<bool>.Ok(true);

        var userId = ValidateString(subject, "budget.userId", budget.UserId, MaxScalarChars);
        if (!userId.IsSuccess)
            return userId;

        var tier = ValidateString(subject, "budget.tier", budget.Tier, MaxScalarChars);
        if (!tier.IsSuccess)
            return tier;

        return ValidateMetadata(subject, "budget.metadata", budget.Metadata);
    }

    private static OperationResult<bool> ValidateMetadata(
        string subject,
        string field,
        IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null)
            return OperationResult<bool>.Ok(true);

        if (metadata.Count > MaxMetadataEntries)
            return TooLarge(subject, field + ".count", metadata.Count, MaxMetadataEntries);

        foreach (var pair in metadata)
        {
            var key = ValidateString(subject, field + ".key", pair.Key, MaxMetadataKeyChars);
            if (!key.IsSuccess)
                return key;

            var value = ValidateString(subject, field + "." + pair.Key, pair.Value, MaxMetadataValueChars);
            if (!value.IsSuccess)
                return value;
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateString(
        string subject,
        string field,
        string? value,
        int maxChars)
    {
        var length = value?.Length ?? 0;
        return length <= maxChars
            ? OperationResult<bool>.Ok(true)
            : TooLarge(subject, field, length, maxChars);
    }

    private static OperationResult<bool> TooLarge(
        string subject,
        string field,
        int actual,
        int max)
        => OperationResult<bool>.Fail(
            $"{subject} field '{field}' is too large: {actual.ToString(CultureInfo.InvariantCulture)} chars/items exceeds {max.ToString(CultureInfo.InvariantCulture)}.",
            ErrorCode);
}
