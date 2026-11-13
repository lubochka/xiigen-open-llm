// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ModelServer.DesignConversion;

using System.Security;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;

public sealed record DesignConversionModelServerMeasurementGateThresholdConfiguration(
    double RequiredHeldOutQualityScore,
    long MaxRuntimeMs,
    decimal MaxCostUsd,
    long MaxPeakRamMb)
{
    public const int MaximumFileBytes = 4096;
    public const string ErrorCodePrefix =
        "DESIGN_CONVERSION_MEASUREMENT_GATE_THRESHOLD_CONFIGURATION_";

    private const string PathRequiredCode = ErrorCodePrefix + "PATH_REQUIRED";
    private const string PathInvalidCode = ErrorCodePrefix + "PATH_INVALID";
    private const string FileNotFoundCode = ErrorCodePrefix + "FILE_NOT_FOUND";
    private const string ReparsePointRejectedCode = ErrorCodePrefix + "REPARSE_POINT_REJECTED";
    private const string FileTooLargeCode = ErrorCodePrefix + "FILE_TOO_LARGE";
    private const string ReadFailedCode = ErrorCodePrefix + "READ_FAILED";
    private const string JsonInvalidCode = ErrorCodePrefix + "JSON_INVALID";
    private const string SchemaInvalidCode = ErrorCodePrefix + "SCHEMA_INVALID";
    private const string ValueInvalidCode = ErrorCodePrefix + "VALUE_INVALID";

    private const string RequiredHeldOutQualityScoreKey = "required_held_out_quality_score";
    private const string MaxRuntimeMsKey = "max_runtime_ms";
    private const string MaxCostUsdKey = "max_cost_usd";
    private const string MaxPeakRamMbKey = "max_peak_ram_mb";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    public DesignConversionMeasurementGateThresholds ToDomainThresholds()
        => new(
            RequiredHeldOutQualityScore,
            MaxRuntimeMs,
            MaxCostUsd,
            MaxPeakRamMb);

    internal OperationResult<DesignConversionModelServerMeasurementGateThresholdConfiguration>
        ValidateForDomainBounds()
        => ValidateForDomainBounds(this);

    public static OperationResult<DesignConversionModelServerMeasurementGateThresholdConfiguration>
        LoadFromFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Fail(PathRequiredCode, "Measurement gate threshold configuration path is required.");

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception exception) when (
            exception is ArgumentException or NotSupportedException or PathTooLongException or SecurityException)
        {
            return Fail(PathInvalidCode, "Measurement gate threshold configuration path is invalid.");
        }

        try
        {
            if (Directory.Exists(fullPath))
                return Fail(PathInvalidCode, "Measurement gate threshold configuration path is not a regular file.");
            if (!File.Exists(fullPath))
                return Fail(FileNotFoundCode, "Measurement gate threshold configuration file was not found.");
            if (AnyExistingPathSegmentIsReparsePoint(fullPath))
                return Fail(ReparsePointRejectedCode, "Measurement gate threshold configuration reparse points are rejected.");

            var attributes = File.GetAttributes(fullPath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.Device)) != 0)
                return Fail(PathInvalidCode, "Measurement gate threshold configuration path is not a regular file.");
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return Fail(FileNotFoundCode, "Measurement gate threshold configuration file was not found.");
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or SecurityException or NotSupportedException)
        {
            return Fail(ReadFailedCode, "Measurement gate threshold configuration could not be read safely.");
        }

        byte[] bytes;
        try
        {
            using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: MaximumFileBytes,
                FileOptions.SequentialScan);
            var lengthBeforeRead = stream.Length;
            if (lengthBeforeRead > MaximumFileBytes)
                return Fail(FileTooLargeCode, "Measurement gate threshold configuration exceeds the size limit.");

            bytes = new byte[MaximumFileBytes + 1];
            var totalRead = 0;
            while (totalRead < bytes.Length)
            {
                var read = stream.Read(bytes, totalRead, bytes.Length - totalRead);
                if (read == 0)
                    break;
                totalRead += read;
            }

            if (totalRead > MaximumFileBytes || stream.Length > MaximumFileBytes)
                return Fail(FileTooLargeCode, "Measurement gate threshold configuration exceeds the size limit.");
            if (stream.Length != lengthBeforeRead || totalRead != lengthBeforeRead)
                return Fail(ReadFailedCode, "Measurement gate threshold configuration changed during the bounded read.");
            if (AnyExistingPathSegmentIsReparsePoint(fullPath))
                return Fail(ReparsePointRejectedCode, "Measurement gate threshold configuration reparse points are rejected.");

            Array.Resize(ref bytes, totalRead);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return Fail(FileNotFoundCode, "Measurement gate threshold configuration file was not found.");
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or SecurityException or NotSupportedException)
        {
            return Fail(ReadFailedCode, "Measurement gate threshold configuration could not be read safely.");
        }

        string json;
        try
        {
            var offset = HasUtf8Bom(bytes) ? 3 : 0;
            json = StrictUtf8.GetString(bytes, offset, bytes.Length - offset);
        }
        catch (DecoderFallbackException)
        {
            return Fail(JsonInvalidCode, "Measurement gate threshold configuration JSON encoding is invalid.");
        }

        try
        {
            using var document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 8
                });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return Fail(SchemaInvalidCode, "Measurement gate threshold configuration schema is invalid.");

            var seen = new HashSet<string>(StringComparer.Ordinal);
            JsonElement requiredHeldOutQualityScore = default;
            JsonElement maxRuntimeMs = default;
            JsonElement maxCostUsd = default;
            JsonElement maxPeakRamMb = default;
            foreach (var property in root.EnumerateObject())
            {
                if (!seen.Add(property.Name))
                    return Fail(SchemaInvalidCode, "Measurement gate threshold configuration schema is invalid.");

                switch (property.Name)
                {
                    case RequiredHeldOutQualityScoreKey:
                        requiredHeldOutQualityScore = property.Value;
                        break;
                    case MaxRuntimeMsKey:
                        maxRuntimeMs = property.Value;
                        break;
                    case MaxCostUsdKey:
                        maxCostUsd = property.Value;
                        break;
                    case MaxPeakRamMbKey:
                        maxPeakRamMb = property.Value;
                        break;
                    default:
                        return Fail(SchemaInvalidCode, "Measurement gate threshold configuration schema is invalid.");
                }
            }

            if (seen.Count != 4
                || !seen.Contains(RequiredHeldOutQualityScoreKey)
                || !seen.Contains(MaxRuntimeMsKey)
                || !seen.Contains(MaxCostUsdKey)
                || !seen.Contains(MaxPeakRamMbKey)
                || requiredHeldOutQualityScore.ValueKind != JsonValueKind.Number
                || maxRuntimeMs.ValueKind != JsonValueKind.Number
                || maxCostUsd.ValueKind != JsonValueKind.Number
                || maxPeakRamMb.ValueKind != JsonValueKind.Number)
            {
                return Fail(SchemaInvalidCode, "Measurement gate threshold configuration schema is invalid.");
            }

            if (!requiredHeldOutQualityScore.TryGetDouble(out var quality)
                || !maxRuntimeMs.TryGetInt64(out var runtimeMs)
                || !maxCostUsd.TryGetDecimal(out var costUsd)
                || !maxPeakRamMb.TryGetInt64(out var peakRamMb))
            {
                return Fail(ValueInvalidCode, "Measurement gate threshold configuration values are invalid.");
            }

            return ValidateForDomainBounds(
                new DesignConversionModelServerMeasurementGateThresholdConfiguration(
                    quality,
                    runtimeMs,
                    costUsd,
                    peakRamMb));
        }
        catch (JsonException)
        {
            return Fail(JsonInvalidCode, "Measurement gate threshold configuration JSON is invalid.");
        }
    }

    public override string ToString()
        => $"{nameof(DesignConversionModelServerMeasurementGateThresholdConfiguration)} {{ Configured = True, Values = [REDACTED] }}";

    private static bool HasUtf8Bom(IReadOnlyList<byte> bytes)
        => bytes.Count >= 3
           && bytes[0] == 0xEF
           && bytes[1] == 0xBB
           && bytes[2] == 0xBF;

    private static bool AnyExistingPathSegmentIsReparsePoint(string fullPath)
    {
        if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
            return true;

        var directory = new DirectoryInfo(Path.GetDirectoryName(fullPath)!);
        while (directory is not null)
        {
            if (directory.Exists && (directory.Attributes & FileAttributes.ReparsePoint) != 0)
                return true;
            directory = directory.Parent;
        }

        return false;
    }

    private static OperationResult<DesignConversionModelServerMeasurementGateThresholdConfiguration>
        ValidateForDomainBounds(DesignConversionModelServerMeasurementGateThresholdConfiguration configuration)
    {
        if (!double.IsFinite(configuration.RequiredHeldOutQualityScore)
            || configuration.RequiredHeldOutQualityScore < 0d
            || configuration.RequiredHeldOutQualityScore > 1d
            || configuration.MaxRuntimeMs < 1
            || configuration.MaxRuntimeMs > DesignConversionMeasurementAdmission.MaxReportedRuntimeMs
            || configuration.MaxCostUsd < 0m
            || configuration.MaxCostUsd > DesignConversionMeasurementAdmission.MaxReportedCostUsd
            || configuration.MaxPeakRamMb < 1
            || configuration.MaxPeakRamMb > DesignConversionMeasurementAdmission.MaxReportedPeakRamMb)
        {
            return Fail(ValueInvalidCode, "Measurement gate threshold configuration values are invalid.");
        }

        return OperationResult<DesignConversionModelServerMeasurementGateThresholdConfiguration>.Ok(configuration);
    }

    private static OperationResult<DesignConversionModelServerMeasurementGateThresholdConfiguration> Fail(
        string code,
        string message)
        => OperationResult<DesignConversionModelServerMeasurementGateThresholdConfiguration>.Fail(message, code);
}
