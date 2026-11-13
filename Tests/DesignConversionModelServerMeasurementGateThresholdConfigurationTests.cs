// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Globalization;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerMeasurementGateThresholdConfigurationTests
{
    private const string Prefix =
        "DESIGN_CONVERSION_MEASUREMENT_GATE_THRESHOLD_CONFIGURATION_";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LoadFromFile_ValidStrictJsonPreservesDecimalAndInt64Precision(bool includeBom)
    {
        using var fixture = new TemporaryDirectory();
        var path = fixture.PathFor("thresholds.json");
        var json =
            $$"""
            {
              "required_held_out_quality_score": 0.987654321,
              "max_runtime_ms": {{DesignConversionMeasurementAdmission.MaxReportedRuntimeMs.ToString(CultureInfo.InvariantCulture)}},
              "max_cost_usd": 0.1234567890123456789012345678,
              "max_peak_ram_mb": {{DesignConversionMeasurementAdmission.MaxReportedPeakRamMb.ToString(CultureInfo.InvariantCulture)}}
            }
            """;
        var payload = Encoding.UTF8.GetBytes(json);
        File.WriteAllBytes(
            path,
            includeBom
                ? [0xEF, 0xBB, 0xBF, .. payload]
                : payload);

        var loaded =
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path);

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(0.987654321d, loaded.Value!.RequiredHeldOutQualityScore, precision: 9);
        Assert.Equal(DesignConversionMeasurementAdmission.MaxReportedRuntimeMs, loaded.Value.MaxRuntimeMs);
        Assert.Equal(0.1234567890123456789012345678m, loaded.Value.MaxCostUsd);
        Assert.Equal(DesignConversionMeasurementAdmission.MaxReportedPeakRamMb, loaded.Value.MaxPeakRamMb);
        var domain = loaded.Value.ToDomainThresholds();
        Assert.Equal(loaded.Value.RequiredHeldOutQualityScore, domain.RequiredHeldOutQualityScore);
        Assert.Equal(loaded.Value.MaxRuntimeMs, domain.MaxRuntimeMs);
        Assert.Equal(loaded.Value.MaxCostUsd, domain.MaxCostUsd);
        Assert.Equal(loaded.Value.MaxPeakRamMb, domain.MaxPeakRamMb);
        Assert.DoesNotContain(path, loaded.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("0.987654321", loaded.Value.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(
            DesignConversionMeasurementAdmission.MaxReportedRuntimeMs.ToString(CultureInfo.InvariantCulture),
            loaded.Value.ToString(),
            StringComparison.Ordinal);
        Assert.Contains("[REDACTED]", loaded.Value.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t ")]
    public void LoadFromFile_MissingPathFailsWithStableCode(string? path)
    {
        var loaded =
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path);

        AssertFailure(loaded, "PATH_REQUIRED");
    }

    [Fact]
    public void LoadFromFile_InvalidMissingAndNonRegularPathsUseDistinctStableCodes()
    {
        using var fixture = new TemporaryDirectory();

        AssertFailure(
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(
                "invalid\0path"),
            "PATH_INVALID");
        AssertFailure(
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(
                fixture.PathFor("missing.json")),
            "FILE_NOT_FOUND");
        AssertFailure(
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(
                fixture.Root),
            "PATH_INVALID");
    }

    [Fact]
    public void LoadFromFile_OversizeAndConcurrentExclusiveOpenFailClosed()
    {
        using var fixture = new TemporaryDirectory();
        var oversize = fixture.PathFor("oversize.json");
        File.WriteAllBytes(
            oversize,
            Enumerable.Repeat(
                    (byte)' ',
                    DesignConversionModelServerMeasurementGateThresholdConfiguration.MaximumFileBytes + 1)
                .ToArray());
        AssertFailure(
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(oversize),
            "FILE_TOO_LARGE");

        var locked = fixture.PathFor("locked.json");
        File.WriteAllText(locked, ValidJson(), new UTF8Encoding(false));
        using var exclusive = new FileStream(
            locked,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.None);
        AssertFailure(
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(locked),
            "READ_FAILED");
    }

    [Fact]
    public void LoadFromFile_InvalidUtf8CommentsAndTrailingCommaAreJsonInvalid()
    {
        using var fixture = new TemporaryDirectory();
        var invalidUtf8 = fixture.PathFor("invalid-utf8.json");
        File.WriteAllBytes(invalidUtf8, [0x7B, 0xFF, 0x7D]);
        AssertFailure(
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(invalidUtf8),
            "JSON_INVALID");

        foreach (var (name, json) in new[]
                 {
                     ("comments.json", ValidJson().Replace("{", "{/*comment*/", StringComparison.Ordinal)),
                     ("trailing.json", ValidJson().Replace("\n}", ",\n}", StringComparison.Ordinal))
                 })
        {
            var path = fixture.PathFor(name);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            AssertFailure(
                DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path),
                "JSON_INVALID");
        }
    }

    [Theory]
    [MemberData(nameof(InvalidSchemaJson))]
    public void LoadFromFile_RejectsEverySchemaMutation(string json)
    {
        using var fixture = new TemporaryDirectory();
        var path = fixture.PathFor("schema.json");
        File.WriteAllText(path, json, new UTF8Encoding(false));

        var loaded =
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path);

        AssertFailure(loaded, "SCHEMA_INVALID");
    }

    [Theory]
    [MemberData(nameof(InvalidValueJson))]
    public void LoadFromFile_RejectsEveryNumericBoundaryMutation(string json)
    {
        using var fixture = new TemporaryDirectory();
        var path = fixture.PathFor("values.json");
        File.WriteAllText(path, json, new UTF8Encoding(false));

        var loaded =
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path);

        AssertFailure(loaded, "VALUE_INVALID");
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    public void LoadFromFile_AcceptsInclusiveQualityEndpointsAndZeroCost(double quality)
    {
        using var fixture = new TemporaryDirectory();
        var path = fixture.PathFor("boundary.json");
        File.WriteAllText(
            path,
            ValidJson(
                quality: quality.ToString(System.Globalization.CultureInfo.InvariantCulture),
                cost: "0"),
            new UTF8Encoding(false));

        var loaded =
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path);

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        Assert.Equal(quality, loaded.Value!.RequiredHeldOutQualityScore);
        Assert.Equal(0m, loaded.Value.MaxCostUsd);
    }

    [Theory]
    [MemberData(nameof(OverDomainValueJson))]
    public void LoadFromFile_RejectsOverDomainRuntimeCostAndRamWithoutLeakingValues(
        string json,
        string forbiddenValue)
    {
        using var fixture = new TemporaryDirectory("over-domain-private");
        var path = fixture.PathFor("thresholds-private-over-domain.json");
        File.WriteAllText(path, json, new UTF8Encoding(false));

        var loaded =
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path);

        AssertFailure(loaded, "VALUE_INVALID");
        var visible = string.Join("|", loaded.ErrorCode, loaded.ErrorMessage, loaded.ToString());
        Assert.DoesNotContain(path, visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(fixture.Root, visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("thresholds-private-over-domain", visible, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(forbiddenValue, visible, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LoadFromFile_RejectsLeafAndAncestorReparsePointsWhenSupported()
    {
        using var fixture = new TemporaryDirectory();
        var targetFile = fixture.PathFor("target.json");
        File.WriteAllText(targetFile, ValidJson(), new UTF8Encoding(false));
        var fileLink = fixture.PathFor("file-link.json");
        if (!TryCreateFileSymbolicLink(fileLink, targetFile))
            return;

        try
        {
            AssertFailure(
                DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(fileLink),
                "REPARSE_POINT_REJECTED");
        }
        finally
        {
            File.Delete(fileLink);
        }

        var targetDirectory = fixture.PathFor("target-directory");
        Directory.CreateDirectory(targetDirectory);
        File.WriteAllText(
            Path.Combine(targetDirectory, "thresholds.json"),
            ValidJson(),
            new UTF8Encoding(false));
        var directoryLink = fixture.PathFor("directory-link");
        if (!TryCreateDirectorySymbolicLink(directoryLink, targetDirectory))
            return;

        try
        {
            AssertFailure(
                DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(
                    Path.Combine(directoryLink, "thresholds.json")),
                "REPARSE_POINT_REJECTED");
        }
        finally
        {
            Directory.Delete(directoryLink);
        }
    }

    [Fact]
    public void LoadFromFile_FailuresNeverLeakPathContentOrValues()
    {
        using var fixture = new TemporaryDirectory("private-threshold-path");
        var path = fixture.PathFor("threshold-secret-771923.json");
        const string secretContent =
            """
            {
              "required_held_out_quality_score": 0.91,
              "max_runtime_ms": 772001,
              "max_cost_usd": 998.771923,
              "max_peak_ram_mb": 772002,
              "private_secret_771923": "do-not-leak"
            }
            """;
        File.WriteAllText(path, secretContent, new UTF8Encoding(false));

        var loaded =
            DesignConversionModelServerMeasurementGateThresholdConfiguration.LoadFromFile(path);
        AssertFailure(loaded, "SCHEMA_INVALID");

        var visible = string.Join("|", loaded.ErrorCode, loaded.ErrorMessage, loaded.ToString());
        foreach (var forbidden in new[]
                 {
                     path,
                     fixture.Root,
                     "threshold-secret-771923",
                     "private_secret_771923",
                     "do-not-leak",
                     "772001",
                     "998.771923",
                     "772002"
                 })
        {
            Assert.DoesNotContain(forbidden, visible, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static IEnumerable<object[]> InvalidSchemaJson()
    {
        yield return ["null"];
        yield return ["[]"];
        yield return
        [
            """
            {
              "required_held_out_quality_score": 0.9,
              "max_runtime_ms": 800,
              "max_cost_usd": 1.25
            }
            """
        ];
        yield return [ValidJson().Replace(
            "\n}",
            ",\n  \"unknown\": 1\n}",
            StringComparison.Ordinal)];
        yield return [ValidJson().Replace(
            "required_held_out_quality_score",
            "Required_Held_Out_Quality_Score",
            StringComparison.Ordinal)];
        yield return [ValidJson().Replace(
            "\"max_runtime_ms\": 800",
            "\"max_runtime_ms\": 800,\n  \"max_runtime_ms\": 801",
            StringComparison.Ordinal)];
        yield return [ValidJson().Replace(
            "\"max_cost_usd\": 1.25",
            "\"max_cost_usd\": null",
            StringComparison.Ordinal)];
        yield return [ValidJson().Replace(
            "\"max_peak_ram_mb\": 512",
            "\"max_peak_ram_mb\": \"512\"",
            StringComparison.Ordinal)];
        yield return [ValidJson().Replace(
            "\"required_held_out_quality_score\": 0.9",
            "\"required_held_out_quality_score\": true",
            StringComparison.Ordinal)];
    }

    public static IEnumerable<object[]> InvalidValueJson()
    {
        yield return [ValidJson(quality: "-0.0001")];
        yield return [ValidJson(quality: "1.0001")];
        yield return [ValidJson(quality: "1e400")];
        yield return [ValidJson(runtime: "0")];
        yield return [ValidJson(runtime: "-1")];
        yield return [ValidJson(runtime: "1.5")];
        yield return [ValidJson(runtime: (DesignConversionMeasurementAdmission.MaxReportedRuntimeMs + 1).ToString(CultureInfo.InvariantCulture))];
        yield return [ValidJson(cost: "-0.0001")];
        yield return [ValidJson(cost: (DesignConversionMeasurementAdmission.MaxReportedCostUsd + 0.0001m).ToString(CultureInfo.InvariantCulture))];
        yield return [ValidJson(cost: "1e1000")];
        yield return [ValidJson(ram: "0")];
        yield return [ValidJson(ram: "-1")];
        yield return [ValidJson(ram: "1.5")];
        yield return [ValidJson(ram: (DesignConversionMeasurementAdmission.MaxReportedPeakRamMb + 1).ToString(CultureInfo.InvariantCulture))];
    }

    public static IEnumerable<object[]> OverDomainValueJson()
    {
        var runtime = (DesignConversionMeasurementAdmission.MaxReportedRuntimeMs + 1).ToString(CultureInfo.InvariantCulture);
        var cost = (DesignConversionMeasurementAdmission.MaxReportedCostUsd + 0.0001m).ToString(CultureInfo.InvariantCulture);
        var ram = (DesignConversionMeasurementAdmission.MaxReportedPeakRamMb + 1).ToString(CultureInfo.InvariantCulture);
        yield return [ValidJson(runtime: runtime), runtime];
        yield return [ValidJson(cost: cost), cost];
        yield return [ValidJson(ram: ram), ram];
    }

    private static string ValidJson(
        string quality = "0.9",
        string runtime = "800",
        string cost = "1.25",
        string ram = "512")
        => $$"""
             {
               "required_held_out_quality_score": {{quality}},
               "max_runtime_ms": {{runtime}},
               "max_cost_usd": {{cost}},
               "max_peak_ram_mb": {{ram}}
             }
             """;

    private static void AssertFailure<T>(AgentInfrastructure.OperationResult<T> result, string suffix)
    {
        Assert.False(result.IsSuccess);
        Assert.Equal(Prefix + suffix, result.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    private static bool TryCreateFileSymbolicLink(string linkPath, string targetPath)
    {
        try
        {
            File.CreateSymbolicLink(linkPath, targetPath);
            return (File.GetAttributes(linkPath) & FileAttributes.ReparsePoint) != 0;
        }
        catch (Exception exception) when (IsRecognizedReparsePointCapabilityLimitation(exception))
        {
            return false;
        }
    }

    private static bool TryCreateDirectorySymbolicLink(string linkPath, string targetPath)
    {
        try
        {
            Directory.CreateSymbolicLink(linkPath, targetPath);
            return (File.GetAttributes(linkPath) & FileAttributes.ReparsePoint) != 0;
        }
        catch (Exception exception) when (IsRecognizedReparsePointCapabilityLimitation(exception))
        {
            return false;
        }
    }

    private static bool IsRecognizedReparsePointCapabilityLimitation(Exception exception)
        => exception is UnauthorizedAccessException or PlatformNotSupportedException or NotSupportedException
           || exception is IOException ioException
           && ioException.HResult is unchecked((int)0x80070522)
               or unchecked((int)0x80070032)
               or unchecked((int)0x80070001);

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory(string? marker = null)
        {
            Root = Path.Combine(
                Path.GetTempPath(),
                "design-conversion-measurement-threshold-tests",
                marker ?? "fixture",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string PathFor(string name) => Path.Combine(Root, name);

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
