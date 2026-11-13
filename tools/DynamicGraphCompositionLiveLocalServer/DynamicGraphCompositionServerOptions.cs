// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Globalization;
using System.Text.RegularExpressions;

namespace LlmFromScratch.DynamicGraphCompositionLiveLocalServer;

public sealed record DynamicGraphCompositionServerOptions
{
    public const string LoopbackUrl = "http://127.0.0.1:0";
    public const int MaxTenantIdLength = 128;
    public const int MaxIdempotencyKeyLength = 128;
    public const int MaxInputLength = 1024 * 1024;
    public const int MaxGraphNodes = 128;
    public const int MaxGraphEdges = 256;
    public const int MaxGraphDepth = 64;
    public const int MaxParallelBranches = 32;

    public required string TenantId { get; init; }
    public required string ReadyFile { get; init; }
    public string? StateDirectory { get; init; }
    public int MaxRequestBodyBytes { get; init; } = 1024 * 1024;
    public int MaxSessions { get; init; } = 64;
    public int MaxConcurrentCompositions { get; init; } = 2;
    public int MaxConcurrentRuns { get; init; } = 4;
    public TimeSpan SessionTtl { get; init; } = TimeSpan.FromSeconds(900);

    public static DynamicGraphCompositionServerOptions Parse(
        string[] args,
        Func<string, string?>? readEnvironment = null)
    {
        ArgumentNullException.ThrowIfNull(args);
        readEnvironment ??= Environment.GetEnvironmentVariable;
        var cli = ParseCli(args);

        string? Value(string name, string environmentName)
            => cli.TryGetValue(name, out var value) ? value : readEnvironment(environmentName);

        var tenantId = Value("--tenant-id", "XIIGEN_DYNAMIC_GRAPH_TENANT_ID");
        var readyFile = Value("--ready-file", "XIIGEN_DYNAMIC_GRAPH_READY_FILE");
        var stateDirectory = Value("--state-directory", "XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY");
        var options = new DynamicGraphCompositionServerOptions
        {
            TenantId = tenantId ?? string.Empty,
            ReadyFile = readyFile ?? string.Empty,
            StateDirectory = stateDirectory,
            MaxSessions = ParseInt(
                Value("--max-sessions", "XIIGEN_DYNAMIC_GRAPH_MAX_SESSIONS"), 64, "max sessions"),
            SessionTtl = TimeSpan.FromSeconds(ParseInt(
                Value("--session-ttl-seconds", "XIIGEN_DYNAMIC_GRAPH_SESSION_TTL_SECONDS"), 900, "session TTL")),
            MaxConcurrentCompositions = ParseInt(
                Value("--max-concurrent-compositions", "XIIGEN_DYNAMIC_GRAPH_MAX_CONCURRENT_COMPOSITIONS"), 2, "composition concurrency"),
            MaxConcurrentRuns = ParseInt(
                Value("--max-concurrent-runs", "XIIGEN_DYNAMIC_GRAPH_MAX_CONCURRENT_RUNS"), 4, "run concurrency"),
            MaxRequestBodyBytes = ParseInt(
                Value("--max-request-body-bytes", "XIIGEN_DYNAMIC_GRAPH_MAX_REQUEST_BODY_BYTES"), 1024 * 1024, "request body limit")
        };
        DynamicGraphCompositionServerOptions normalized;
        try
        {
            normalized = options with
            {
                ReadyFile = DynamicGraphCompositionFileSystemPolicy.NormalizeReadyFile(options.ReadyFile),
                StateDirectory = options.StateDirectory is null
                    ? null
                    : DynamicGraphCompositionFileSystemPolicy.NormalizeStateDirectory(options.StateDirectory)
            };
        }
        catch (DynamicGraphCompositionPersistenceException exception) when (
            DynamicGraphCompositionSessionPersistence.IsNonFatalException(exception))
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
        normalized.Validate();
        return normalized;
    }

    public static string? FindReadyFileForStartupCleanup(
        string[] args,
        Func<string, string?>? readEnvironment = null)
    {
        ArgumentNullException.ThrowIfNull(args);
        readEnvironment ??= Environment.GetEnvironmentVariable;
        var cli = ParseCli(args);
        var configuredReadyFile = cli.TryGetValue("--ready-file", out var readyFile)
            ? readyFile
            : readEnvironment("XIIGEN_DYNAMIC_GRAPH_READY_FILE");
        if (string.IsNullOrWhiteSpace(configuredReadyFile))
            return null;

        var configuredStateDirectory = cli.TryGetValue("--state-directory", out var stateDirectory)
            ? stateDirectory
            : readEnvironment("XIIGEN_DYNAMIC_GRAPH_STATE_DIRECTORY");
        try
        {
            var normalizedReadyFile =
                DynamicGraphCompositionFileSystemPolicy.NormalizeReadyFile(configuredReadyFile);
            var normalizedStateDirectory = configuredStateDirectory is null
                ? null
                : DynamicGraphCompositionFileSystemPolicy.NormalizeStateDirectory(configuredStateDirectory);
            DynamicGraphCompositionFileSystemPolicy.EnsureReadyFileOutsideState(
                normalizedReadyFile,
                normalizedStateDirectory);
            return normalizedReadyFile;
        }
        catch (DynamicGraphCompositionPersistenceException exception) when (
            DynamicGraphCompositionSessionPersistence.IsNonFatalException(exception))
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    public void Validate()
    {
        if (!IsSafeIdentifier(TenantId, 1, MaxTenantIdLength))
            throw new InvalidOperationException("A safe configured tenant id is required.");
        if (string.IsNullOrWhiteSpace(ReadyFile) || ReadyFile.IndexOf('\0') >= 0)
            throw new InvalidOperationException("A ready-file path is required.");
        if (StateDirectory is not null
            && (string.IsNullOrWhiteSpace(StateDirectory)
                || StateDirectory.IndexOf('\0') >= 0
                || !Path.IsPathFullyQualified(StateDirectory)))
        {
            throw new InvalidOperationException("StateDirectory must be an absolute path when persistence is enabled.");
        }
        try
        {
            var normalizedReadyFile = DynamicGraphCompositionFileSystemPolicy.NormalizeReadyFile(ReadyFile);
            var normalizedStateDirectory = StateDirectory is null
                ? null
                : DynamicGraphCompositionFileSystemPolicy.NormalizeStateDirectory(StateDirectory);
            DynamicGraphCompositionFileSystemPolicy.EnsureReadyFileOutsideState(
                normalizedReadyFile,
                normalizedStateDirectory);
        }
        catch (DynamicGraphCompositionPersistenceException exception) when (
            DynamicGraphCompositionSessionPersistence.IsNonFatalException(exception))
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
        if (MaxRequestBodyBytes is < 1024 or > 1024 * 1024)
            throw new InvalidOperationException("MaxRequestBodyBytes must be between 1 KiB and 1 MiB.");
        if (MaxSessions is < 1 or > 4096)
            throw new InvalidOperationException("MaxSessions must be between 1 and 4096.");
        if (MaxConcurrentCompositions is < 1 or > 64)
            throw new InvalidOperationException("MaxConcurrentCompositions must be between 1 and 64.");
        if (MaxConcurrentRuns is < 1 or > 64)
            throw new InvalidOperationException("MaxConcurrentRuns must be between 1 and 64.");
        if (SessionTtl < TimeSpan.FromSeconds(1) || SessionTtl > TimeSpan.FromHours(24))
            throw new InvalidOperationException("SessionTtl must be between one second and 24 hours.");
    }

    public static bool IsSafeIdentifier(string? value, int minLength, int maxLength)
        => value is not null
           && value.Length >= minLength
           && value.Length <= maxLength
           && Regex.IsMatch(value, "^[A-Za-z0-9._:-]+$", RegexOptions.CultureInvariant);

    private static Dictionary<string, string> ParseCli(string[] args)
    {
        var supported = new HashSet<string>(StringComparer.Ordinal)
        {
            "--tenant-id", "--ready-file", "--max-sessions", "--session-ttl-seconds",
            "--max-concurrent-compositions", "--max-concurrent-runs", "--max-request-body-bytes",
            "--state-directory"
        };
        if (args.Length % 2 != 0)
            throw new InvalidOperationException("Every server option requires one value.");

        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < args.Length; i += 2)
        {
            if (!supported.Contains(args[i]) || !result.TryAdd(args[i], args[i + 1]))
                throw new InvalidOperationException("An unsupported or duplicate server option was supplied.");
        }
        return result;
    }

    private static int ParseInt(string? value, int defaultValue, string name)
    {
        if (value is null)
            return defaultValue;
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            throw new InvalidOperationException($"The configured {name} must be an integer.");
        return parsed;
    }
}
