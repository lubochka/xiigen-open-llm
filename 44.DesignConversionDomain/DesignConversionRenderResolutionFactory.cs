// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

public static class DesignConversionRenderResolutionFactory
{
    public const string DeviceScaleFactorMetadataKey = "device_scale_factor";
    public const string InvalidDeviceScaleFactorErrorCode = "RENDER_RESOLUTION_DEVICE_SCALE_FACTOR_INVALID";
    public const string ConflictingDeviceScaleFactorErrorCode = "RENDER_RESOLUTION_DEVICE_SCALE_FACTOR_CONFLICT";
    public const string AmbiguousRequiredBrowserQaWidthErrorCode = "RENDER_RESOLUTION_REQUIRED_BROWSER_QA_WIDTH_AMBIGUOUS";
    public const string ServerOwnedMobileResolutionId = "server-owned-mobile-390x844";
    public const string ServerOwnedDesktopResolutionId = "server-owned-desktop-1440x900";
    public const string RequiredBrowserQaMobileResolutionId = "server-owned-browser-qa-mobile-360x800";
    public const string RequiredBrowserQaTabletResolutionId = "server-owned-browser-qa-tablet-768x1024";
    public const string RequiredBrowserQaLaptopResolutionId = "server-owned-browser-qa-laptop-1280x720";
    public const string RequiredBrowserQaDesktopResolutionId = "server-owned-browser-qa-desktop-1920x1080";
    internal const string CanonicalViewportIdMetadataKey = "viewport_id";
    internal const string ViewportIdConflictErrorCode = "RENDER_RESOLUTION_VIEWPORT_ID_CONFLICT";
    private const int ServerOwnedMobileWidth = 390;
    private const int ServerOwnedMobileHeight = 844;
    private const int ServerOwnedDesktopWidth = 1440;
    private const int ServerOwnedDesktopHeight = 900;
    private const int RequiredBrowserQaMobileWidth = 360;
    private const int RequiredBrowserQaMobileHeight = 800;
    private const int RequiredBrowserQaTabletWidth = 768;
    private const int RequiredBrowserQaTabletHeight = 1024;
    private const int RequiredBrowserQaLaptopWidth = 1280;
    private const int RequiredBrowserQaLaptopHeight = 720;
    private const int RequiredBrowserQaDesktopWidth = 1920;
    private const int RequiredBrowserQaDesktopHeight = 1080;
    private const double DefaultDeviceScaleFactor = 1.0;
    private const int MaxSafeIdentityLength = 64;
    private const string IdentityHashPrefix = "hash-";

    private static readonly HashSet<string> DeviceScaleFactorMetadataKeys = new(
        [
            DeviceScaleFactorMetadataKey,
            "deviceScaleFactor",
            "device_pixel_ratio",
            "devicePixelRatio"
        ],
        StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> ViewportIdentityMetadataKeys = new(
        [
            CanonicalViewportIdMetadataKey,
            "viewportId",
            "viewport_label",
            "viewportLabel"
        ],
        StringComparer.OrdinalIgnoreCase);

    public static OperationResult<IReadOnlyList<RenderQaResolution>> CreateGrouped(
        IEnumerable<DesignRenderArtifact>? artifacts)
    {
        var groupsByLogicalSize = new Dictionary<ResolutionGroupKey, ResolutionGroup>();
        var groupsInFirstSeenOrder = new List<ResolutionGroup>();

        foreach (var artifact in artifacts ?? [])
        {
            if (artifact is null || artifact.Width <= 0 || artifact.Height <= 0)
                continue;

            var explicitDeviceScaleFactors = new HashSet<double>();
            foreach (var pair in artifact.Metadata ?? new Dictionary<string, string>())
            {
                if (!DeviceScaleFactorMetadataKeys.Contains(pair.Key))
                    continue;

                if (!TryParseValidDeviceScaleFactor(
                        pair.Value,
                        artifact.Width,
                        artifact.Height,
                        out var deviceScaleFactor))
                {
                    return InvalidDeviceScaleFactor();
                }

                explicitDeviceScaleFactors.Add(deviceScaleFactor);
            }

            var callerIdentity = ResolveCallerViewportIdentity(artifact, explicitDeviceScaleFactors);
            if (!callerIdentity.IsSuccess)
            {
                return OperationResult<IReadOnlyList<RenderQaResolution>>.Fail(
                    callerIdentity.ErrorMessage,
                    callerIdentity.ErrorCode);
            }

            if (callerIdentity.Value.EmbeddedDeviceScaleFactor.HasValue)
                explicitDeviceScaleFactors.Add(callerIdentity.Value.EmbeddedDeviceScaleFactor.Value);

            var groupKey = new ResolutionGroupKey(
                artifact.Width,
                artifact.Height,
                callerIdentity.Value.IdentityToken ?? string.Empty);
            if (!groupsByLogicalSize.TryGetValue(groupKey, out var group))
            {
                group = new ResolutionGroup(
                    artifact.Width,
                    artifact.Height,
                    callerIdentity.Value.IdentityToken);
                groupsByLogicalSize.Add(groupKey, group);
                groupsInFirstSeenOrder.Add(group);
            }

            foreach (var deviceScaleFactor in explicitDeviceScaleFactors)
                group.ExplicitDeviceScaleFactors.Add(deviceScaleFactor);
        }

        foreach (var group in groupsInFirstSeenOrder)
        {
            if (group.ExplicitDeviceScaleFactors.Count > 1)
            {
                return OperationResult<IReadOnlyList<RenderQaResolution>>.Fail(
                    "DesignConversion render artifacts for one logical viewport specify conflicting device scale factors.",
                    ConflictingDeviceScaleFactorErrorCode);
            }
        }

        return OperationResult<IReadOnlyList<RenderQaResolution>>.Ok(
            groupsInFirstSeenOrder
                .Select(group =>
                {
                    var deviceScaleFactor = group.ExplicitDeviceScaleFactors.Count == 0
                        ? DefaultDeviceScaleFactor
                        : group.ExplicitDeviceScaleFactors.Single();
                    return new RenderQaResolution(
                        ResolutionId: ResolutionIdFor(
                            group.Width,
                            group.Height,
                            deviceScaleFactor,
                            group.CallerIdentityToken),
                        Width: group.Width,
                        Height: group.Height,
                        DeviceScaleFactor: deviceScaleFactor);
                })
                .ToArray());
    }

    public static OperationResult<IReadOnlyList<RenderQaResolution>> CreateModelServerMinimumResponsiveMatrix(
        IEnumerable<DesignRenderArtifact>? artifacts)
    {
        var sourceMatrix = CreateGrouped(artifacts);
        if (!sourceMatrix.IsSuccess)
            return sourceMatrix;

        if (sourceMatrix.Value!.Count == 0)
            return sourceMatrix;

        var distinctLogicalSizeCount = sourceMatrix.Value
            .Select(resolution => (resolution.Width, resolution.Height))
            .Distinct()
            .Count();
        if (distinctLogicalSizeCount != 1)
            return sourceMatrix;

        var source = sourceMatrix.Value[0];
        return OperationResult<IReadOnlyList<RenderQaResolution>>.Ok(
            sourceMatrix.Value.Append(ComplementaryServerOwnedResolution(source)).ToArray());
    }

    public static OperationResult<IReadOnlyList<RenderQaResolution>> CreateModelServerRequiredBrowserQaMatrix(
        IEnumerable<DesignRenderArtifact>? artifacts)
    {
        var sourceMatrix = CreateGrouped(artifacts);
        if (!sourceMatrix.IsSuccess)
            return sourceMatrix;

        if (sourceMatrix.Value!.Count == 0)
            return sourceMatrix;

        var requiredMatrix = new List<RenderQaResolution>();
        foreach (var fallback in RequiredBrowserQaResponsiveMatrix())
        {
            var sourceMatches = sourceMatrix.Value
                .Where(resolution => resolution.Width == fallback.Width)
                .ToArray();
            if (sourceMatches.Length > 1)
            {
                return OperationResult<IReadOnlyList<RenderQaResolution>>.Fail(
                    "DesignConversion required browser-QA width maps to multiple distinct caller viewports.",
                    AmbiguousRequiredBrowserQaWidthErrorCode);
            }

            requiredMatrix.Add(sourceMatches.Length == 1 ? sourceMatches[0] : fallback);
        }

        return OperationResult<IReadOnlyList<RenderQaResolution>>.Ok(requiredMatrix);
    }

    internal static bool HasViewportIdentityMetadata(IReadOnlyDictionary<string, string>? metadata)
        => (metadata ?? new Dictionary<string, string>())
            .Any(pair => IsViewportIdentityMetadataKey(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value));

    internal static bool IsViewportIdentityMetadataKey(string key)
        => ViewportIdentityMetadataKeys.Contains(key);

    internal static string FirstDeviceScaleFactorMetadata(IReadOnlyDictionary<string, string>? metadata)
        => FirstMetadata(metadata, DeviceScaleFactorMetadataKeys);

    internal static OperationResult<string> CanonicalResolutionIdForArtifact(
        DesignRenderArtifact artifact,
        RenderQaResolution resolution)
    {
        var identity = ResolveCallerViewportIdentity(
            artifact.Width,
            artifact.Height,
            artifact.Metadata,
            [resolution.DeviceScaleFactor]);
        if (!identity.IsSuccess)
        {
            return OperationResult<string>.Fail(
                identity.ErrorMessage,
                identity.ErrorCode);
        }

        return OperationResult<string>.Ok(ResolutionIdFor(
            artifact.Width,
            artifact.Height,
            resolution.DeviceScaleFactor,
            identity.Value.IdentityToken));
    }

    internal static OperationResult<ViewportIdentityResolution> ResolveCallerViewportIdentity(
        DesignRenderArtifact artifact,
        IReadOnlyCollection<double> explicitDeviceScaleFactors)
        => ResolveCallerViewportIdentity(
            artifact.Width,
            artifact.Height,
            artifact.Metadata,
            explicitDeviceScaleFactors);

    internal static OperationResult<ViewportIdentityResolution> ResolveCallerViewportIdentity(
        int width,
        int height,
        IReadOnlyDictionary<string, string>? metadata,
        IReadOnlyCollection<double> explicitDeviceScaleFactors)
    {
        var identityTokens = new HashSet<string>(StringComparer.Ordinal);
        var embeddedDeviceScaleFactors = new HashSet<double>();
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (!ViewportIdentityMetadataKeys.Contains(pair.Key))
                continue;

            var identity = ResolveViewportIdentityAlias(
                pair.Value,
                width,
                height,
                explicitDeviceScaleFactors);
            if (!identity.IsSuccess)
                return identity;
            if (!string.IsNullOrWhiteSpace(identity.Value.IdentityToken))
                identityTokens.Add(identity.Value.IdentityToken);
            if (identity.Value.EmbeddedDeviceScaleFactor.HasValue)
                embeddedDeviceScaleFactors.Add(identity.Value.EmbeddedDeviceScaleFactor.Value);
        }

        if (identityTokens.Count > 1)
            return ViewportIdConflict();

        if (embeddedDeviceScaleFactors.Count > 1)
            return DeviceScaleFactorConflict<ViewportIdentityResolution>();

        return OperationResult<ViewportIdentityResolution>.Ok(new ViewportIdentityResolution(
            identityTokens.Count == 0 ? null : identityTokens.Single(),
            embeddedDeviceScaleFactors.Count == 0 ? null : embeddedDeviceScaleFactors.Single()));
    }

    internal static bool ResolutionHasCallerIdentity(RenderQaResolution resolution)
        => TryParseCanonicalViewportId(resolution.ResolutionId, out var parsed) &&
           !string.IsNullOrWhiteSpace(parsed.IdentityToken);

    internal static string ResolutionIdFor(
        int width,
        int height,
        double deviceScaleFactor,
        string? callerIdentityToken)
    {
        var legacyId = ResolutionIdFor(width, height, deviceScaleFactor);
        return string.IsNullOrWhiteSpace(callerIdentityToken)
            ? legacyId
            : string.Create(CultureInfo.InvariantCulture, $"{legacyId}~{callerIdentityToken}");
    }

    internal static bool TryParseCanonicalViewportId(string? value, out CanonicalViewportId parsed)
    {
        parsed = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var text = value.Trim();
        const string prefix = "source-";
        if (!text.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        var widthStart = prefix.Length;
        var xIndex = text.IndexOf('x', widthStart);
        if (xIndex <= widthStart)
            return false;
        if (!int.TryParse(text[widthStart..xIndex], NumberStyles.None, CultureInfo.InvariantCulture, out var width) ||
            width <= 0)
        {
            return false;
        }

        var heightStart = xIndex + 1;
        var heightEnd = IndexOfNextCanonicalSeparator(text, heightStart);
        if (heightEnd < 0)
            heightEnd = text.Length;
        if (heightEnd <= heightStart ||
            !int.TryParse(text[heightStart..heightEnd], NumberStyles.None, CultureInfo.InvariantCulture, out var height) ||
            height <= 0)
        {
            return false;
        }

        double? deviceScaleFactor = null;
        string? identityToken = null;
        var position = heightEnd;
        if (position < text.Length && text[position] == '@')
        {
            var scaleStart = position + 1;
            var scaleEnd = text.IndexOf('~', scaleStart);
            if (scaleEnd < 0)
                scaleEnd = text.Length;
            if (scaleEnd <= scaleStart ||
                !double.TryParse(text[scaleStart..scaleEnd], NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedScale) ||
                !double.IsFinite(parsedScale) ||
                parsedScale <= 0)
            {
                return false;
            }

            deviceScaleFactor = parsedScale;
            position = scaleEnd;
        }

        if (position < text.Length && text[position] == '~')
        {
            var identityStart = position + 1;
            if (identityStart >= text.Length)
                return false;
            identityToken = text[identityStart..].Trim();
            if (string.IsNullOrWhiteSpace(identityToken))
                return false;
            position = text.Length;
        }

        if (position != text.Length)
            return false;

        parsed = new CanonicalViewportId(width, height, deviceScaleFactor, identityToken);
        return true;
    }

    private static bool TryParseValidDeviceScaleFactor(
        string? rawValue,
        int logicalWidth,
        int logicalHeight,
        out double deviceScaleFactor)
    {
        if (!double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out deviceScaleFactor) ||
            !double.IsFinite(deviceScaleFactor) ||
            deviceScaleFactor <= 0)
        {
            return false;
        }

        var physical = DesignConversionBrowserPhysicalScreenshotResolution.Resolve(
            new RenderQaResolution(
                ResolutionId: "metadata-dsf-probe",
                Width: logicalWidth,
                Height: logicalHeight,
                DeviceScaleFactor: deviceScaleFactor),
            InvalidDeviceScaleFactorErrorCode,
            "DesignConversion render resolution metadata");
        return physical.IsSuccess;
    }

    private static OperationResult<ViewportIdentityResolution> ResolveViewportIdentityAlias(
        string? rawValue,
        int width,
        int height,
        IReadOnlyCollection<double> explicitDeviceScaleFactors)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return OperationResult<ViewportIdentityResolution>.Ok(new ViewportIdentityResolution(null, null));

        var value = rawValue.Trim();
        if (TryParseCanonicalViewportId(value, out var parsed))
        {
            if (parsed.Width != width || parsed.Height != height)
                return ViewportIdConflict();

            if (parsed.DeviceScaleFactor.HasValue &&
                explicitDeviceScaleFactors.Count > 0 &&
                !explicitDeviceScaleFactors.Contains(parsed.DeviceScaleFactor.Value))
            {
                return DeviceScaleFactorConflict<ViewportIdentityResolution>();
            }

            return OperationResult<ViewportIdentityResolution>.Ok(new ViewportIdentityResolution(
                string.IsNullOrWhiteSpace(parsed.IdentityToken)
                ? null
                : SafeIdentityToken(parsed.IdentityToken),
                parsed.DeviceScaleFactor));
        }

        return OperationResult<ViewportIdentityResolution>.Ok(new ViewportIdentityResolution(
            SafeIdentityToken(value),
            null));
    }

    private static OperationResult<IReadOnlyList<RenderQaResolution>> InvalidDeviceScaleFactor()
        => OperationResult<IReadOnlyList<RenderQaResolution>>.Fail(
            "DesignConversion render viewport device scale factor is invalid.",
            InvalidDeviceScaleFactorErrorCode);

    private static OperationResult<T> DeviceScaleFactorConflict<T>()
        => OperationResult<T>.Fail(
            "DesignConversion render artifacts for one logical viewport specify conflicting device scale factors.",
            ConflictingDeviceScaleFactorErrorCode);

    private static OperationResult<ViewportIdentityResolution> ViewportIdConflict()
        => OperationResult<ViewportIdentityResolution>.Fail(
            "DesignConversion render artifacts specify conflicting viewport identity metadata.",
            ViewportIdConflictErrorCode);

    private static RenderQaResolution ComplementaryServerOwnedResolution(RenderQaResolution source)
    {
        var mobileDistance = Math.Abs(source.Width - ServerOwnedMobileWidth);
        var desktopDistance = Math.Abs(source.Width - ServerOwnedDesktopWidth);
        return desktopDistance >= mobileDistance
            ? ServerOwnedDesktopResolution()
            : ServerOwnedMobileResolution();
    }

    private static IReadOnlyList<RenderQaResolution> RequiredBrowserQaResponsiveMatrix()
        =>
        [
            RequiredBrowserQaMobileResolution(),
            RequiredBrowserQaTabletResolution(),
            RequiredBrowserQaLaptopResolution(),
            RequiredBrowserQaDesktopResolution()
        ];

    private static RenderQaResolution ServerOwnedMobileResolution()
        => new(
            ResolutionId: ServerOwnedMobileResolutionId,
            Width: ServerOwnedMobileWidth,
            Height: ServerOwnedMobileHeight,
            DeviceScaleFactor: DefaultDeviceScaleFactor);

    private static RenderQaResolution RequiredBrowserQaMobileResolution()
        => new(
            ResolutionId: RequiredBrowserQaMobileResolutionId,
            Width: RequiredBrowserQaMobileWidth,
            Height: RequiredBrowserQaMobileHeight,
            DeviceScaleFactor: DefaultDeviceScaleFactor);

    private static RenderQaResolution RequiredBrowserQaTabletResolution()
        => new(
            ResolutionId: RequiredBrowserQaTabletResolutionId,
            Width: RequiredBrowserQaTabletWidth,
            Height: RequiredBrowserQaTabletHeight,
            DeviceScaleFactor: DefaultDeviceScaleFactor);

    private static RenderQaResolution RequiredBrowserQaLaptopResolution()
        => new(
            ResolutionId: RequiredBrowserQaLaptopResolutionId,
            Width: RequiredBrowserQaLaptopWidth,
            Height: RequiredBrowserQaLaptopHeight,
            DeviceScaleFactor: DefaultDeviceScaleFactor);

    private static RenderQaResolution RequiredBrowserQaDesktopResolution()
        => new(
            ResolutionId: RequiredBrowserQaDesktopResolutionId,
            Width: RequiredBrowserQaDesktopWidth,
            Height: RequiredBrowserQaDesktopHeight,
            DeviceScaleFactor: DefaultDeviceScaleFactor);

    private static RenderQaResolution ServerOwnedDesktopResolution()
        => new(
            ResolutionId: ServerOwnedDesktopResolutionId,
            Width: ServerOwnedDesktopWidth,
            Height: ServerOwnedDesktopHeight,
            DeviceScaleFactor: DefaultDeviceScaleFactor);

    private static string ResolutionIdFor(int width, int height, double deviceScaleFactor)
    {
        var baseId = string.Create(CultureInfo.InvariantCulture, $"source-{width}x{height}");
        return deviceScaleFactor == DefaultDeviceScaleFactor
            ? baseId
            : string.Create(CultureInfo.InvariantCulture, $"{baseId}@{deviceScaleFactor:R}");
    }

    private static string SafeIdentityToken(string rawValue)
    {
        var value = rawValue.Trim();
        return IsSafeIdentityToken(value)
            ? value
            : IdentityHashPrefix + Convert.ToHexString(SHA256.HashData(
                    Encoding.UTF8.GetBytes("design-conversion-render-viewport-identity\n" + value)))
                .ToLowerInvariant()[..32];
    }

    private static bool IsSafeIdentityToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxSafeIdentityLength)
            return false;
        if (value.Contains("PRIVATE", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("SECRET", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("TOKEN", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return value.All(static ch =>
            char.IsAsciiLetterOrDigit(ch) ||
            ch is '.' or '_' or '-');
    }

    private static string FirstMetadata(
        IReadOnlyDictionary<string, string>? metadata,
        IReadOnlySet<string> keys)
    {
        foreach (var pair in metadata ?? new Dictionary<string, string>())
        {
            if (keys.Contains(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                return pair.Value.Trim();
        }

        return string.Empty;
    }

    private static int IndexOfNextCanonicalSeparator(string text, int start)
    {
        var scale = text.IndexOf('@', start);
        var identity = text.IndexOf('~', start);
        if (scale < 0)
            return identity;
        if (identity < 0)
            return scale;
        return Math.Min(scale, identity);
    }

    internal readonly record struct CanonicalViewportId(
        int Width,
        int Height,
        double? DeviceScaleFactor,
        string? IdentityToken);

    internal readonly record struct ViewportIdentityResolution(
        string? IdentityToken,
        double? EmbeddedDeviceScaleFactor);

    private readonly record struct ResolutionGroupKey(
        int Width,
        int Height,
        string CallerIdentityToken);

    private sealed class ResolutionGroup(int width, int height, string? callerIdentityToken)
    {
        public int Width { get; } = width;
        public int Height { get; } = height;
        public string? CallerIdentityToken { get; } = callerIdentityToken;
        public HashSet<double> ExplicitDeviceScaleFactors { get; } = [];
    }
}
