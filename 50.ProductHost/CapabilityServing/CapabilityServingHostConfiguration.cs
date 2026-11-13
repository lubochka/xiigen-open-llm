// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProductHost.CapabilityServing;

public sealed record CapabilityServingHostConfiguration(
    string StateRoot,
    string CheckpointRoot,
    string SnapshotPath,
    string SessionDatabasePath)
{
    public const string StateRootArgumentName = "--capability-state-root";
    public const string StateRootEnvironmentName = "XIIGEN_PRODUCT_HOST_CAPABILITY_STATE_ROOT";
    public const string CheckpointDirectoryName = "checkpoints";
    public const string SnapshotFileName = "teaching-registry-snapshot.json";
    public const string SessionDatabaseFileName = "capability-serving-sessions.db";

    public static CapabilityServingHostConfiguration ResolveRequired(
        string[] args,
        Func<string, string?> readEnvironment)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(readEnvironment);

        var configuredByCli = false;
        string? configuredRoot = null;
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (string.Equals(argument, StateRootArgumentName, StringComparison.Ordinal))
            {
                if (configuredByCli)
                    throw new ArgumentException("Capability-serving state root was specified more than once.");
                if (index + 1 >= args.Length)
                    throw new ArgumentException("Capability-serving state root is required.");

                configuredRoot = args[++index];
                configuredByCli = true;
                continue;
            }

            var assignmentPrefix = StateRootArgumentName + "=";
            if (argument.StartsWith(assignmentPrefix, StringComparison.Ordinal))
            {
                if (configuredByCli)
                    throw new ArgumentException("Capability-serving state root was specified more than once.");

                configuredRoot = argument[assignmentPrefix.Length..];
                configuredByCli = true;
            }
        }

        configuredRoot = configuredByCli
            ? configuredRoot
            : readEnvironment(StateRootEnvironmentName);
        var stateRoot = NormalizeRequiredRoot(configuredRoot);
        var checkpointRoot = Path.Combine(stateRoot, CheckpointDirectoryName);
        var snapshotPath = Path.Combine(stateRoot, SnapshotFileName);
        var sessionDatabasePath = Path.Combine(stateRoot, SessionDatabaseFileName);

        EnsureExistingSafeDirectory(checkpointRoot, "Capability-serving checkpoint root is required.");
        EnsureExistingSafeFile(snapshotPath, "Capability-serving snapshot is required.");
        EnsureSafeFileOrMissing(
            sessionDatabasePath,
            "Capability-serving session database path is invalid.");
        return new CapabilityServingHostConfiguration(
            stateRoot,
            checkpointRoot,
            snapshotPath,
            sessionDatabasePath);
    }

    public bool IsOwnedCheckpointArtifact(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.IndexOf('\0') >= 0 ||
            path.Any(char.IsControl))
        {
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(path);
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            var rootPrefix = CheckpointRoot.EndsWith(Path.DirectorySeparatorChar)
                ? CheckpointRoot
                : CheckpointRoot + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootPrefix, comparison) ||
                !File.Exists(fullPath))
            {
                return false;
            }

            var attributes = File.GetAttributes(fullPath);
            return (attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) == 0 &&
                   ExistingPathChainIsSafe(Path.GetDirectoryName(fullPath)!);
        }
        catch (Exception exception) when (IsNonFatalPathException(exception))
        {
            return false;
        }
    }

    private static string NormalizeRequiredRoot(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.IndexOf('\0') >= 0 ||
            path.Any(char.IsControl))
        {
            throw new ArgumentException("Capability-serving state root is required.");
        }

        var trimmed = path.Trim();
        if (!Path.IsPathFullyQualified(trimmed))
            throw new ArgumentException("Capability-serving state root must be an absolute path.");

        string fullPath;
        try
        {
            fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(trimmed));
        }
        catch (Exception exception) when (IsNonFatalPathException(exception))
        {
            throw new ArgumentException("Capability-serving state root is invalid.", exception);
        }

        var volumeRoot = Path.TrimEndingDirectorySeparator(Path.GetPathRoot(fullPath) ?? string.Empty);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (string.Equals(fullPath, volumeRoot, comparison))
            throw new ArgumentException("Capability-serving state root must not be a volume root.");

        EnsureExistingSafeDirectory(fullPath, "Capability-serving state root is required.");
        return fullPath;
    }

    private static void EnsureOwnedDirectory(string path, string errorMessage)
    {
        try
        {
            if (File.Exists(path))
                throw new ArgumentException(errorMessage);

            Directory.CreateDirectory(path);
            EnsureExistingSafeDirectory(path, errorMessage);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalPathException(exception))
        {
            throw new ArgumentException(errorMessage, exception);
        }
    }

    private static void EnsureExistingSafeDirectory(string path, string errorMessage)
    {
        try
        {
            if (!Directory.Exists(path) || File.Exists(path))
                throw new ArgumentException(errorMessage);

            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0 ||
                !ExistingPathChainIsSafe(path))
            {
                throw new ArgumentException(errorMessage);
            }
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalPathException(exception))
        {
            throw new ArgumentException(errorMessage, exception);
        }
    }

    private static void EnsureExistingSafeFile(string path, string errorMessage)
    {
        try
        {
            if (!File.Exists(path) || Directory.Exists(path))
                throw new ArgumentException(errorMessage);

            var attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0 ||
                !ExistingPathChainIsSafe(Path.GetDirectoryName(path)!))
            {
                throw new ArgumentException(errorMessage);
            }
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalPathException(exception))
        {
            throw new ArgumentException(errorMessage, exception);
        }
    }

    private static void EnsureSafeFileOrMissing(string path, string errorMessage)
    {
        try
        {
            if (Directory.Exists(path))
                throw new ArgumentException(errorMessage);
            if (File.Exists(path))
            {
                var attributes = File.GetAttributes(path);
                if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                    throw new ArgumentException(errorMessage);
            }

            if (!ExistingPathChainIsSafe(Path.GetDirectoryName(path)!))
                throw new ArgumentException(errorMessage);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception exception) when (IsNonFatalPathException(exception))
        {
            throw new ArgumentException(errorMessage, exception);
        }
    }

    private static bool ExistingPathChainIsSafe(string path)
    {
        var current = new DirectoryInfo(Path.GetFullPath(path));
        while (current is not null)
        {
            if (!current.Exists)
                return false;
            if ((current.Attributes & FileAttributes.ReparsePoint) != 0)
                return false;
            current = current.Parent;
        }

        return true;
    }

    private static bool IsNonFatalPathException(Exception exception)
        => exception is ArgumentException
            or IOException
            or NotSupportedException
            or PathTooLongException
            or UnauthorizedAccessException;
}
