// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

internal static class RepositorySourceScanner
{
    private static readonly string[] AlwaysSkippedDirectoryNames =
    [
        ".git",
        ".tmp",
        ".clone",
        ".codex-test-temp",
        ".vs",
        "artifacts",
        "bin",
        "evidence",
        "node_modules",
        "obj",
        "packages",
        "Results",
        "TestResults"
    ];

    public static string[] EnumerateCSharpFiles(
        string repositoryRoot,
        IEnumerable<string>? additionallySkippedDirectoryNames = null)
        => EnumerateFiles(repositoryRoot, "*.cs", additionallySkippedDirectoryNames);

    public static string[] EnumerateFiles(
        string rootDirectory,
        string searchPattern,
        IEnumerable<string>? additionallySkippedDirectoryNames = null)
    {
        var skippedDirectoryNames = BuildSkippedDirectoryNameSet(additionallySkippedDirectoryNames);
        var pendingDirectories = new Stack<string>();
        var files = new List<string>();
        pendingDirectories.Push(Path.GetFullPath(rootDirectory));

        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = pendingDirectories.Pop();
            if (IsSkippedDirectory(currentDirectory, skippedDirectoryNames))
                continue;

            // Repository-wide tests must check source ownership, not the operating
            // system's ability to open leftover service folders from earlier runs.
            foreach (var file in EnumerateFilesInOneDirectory(currentDirectory, searchPattern))
                files.Add(file);

            foreach (var childDirectory in EnumerateChildDirectories(currentDirectory))
            {
                if (!IsSkippedDirectory(childDirectory, skippedDirectoryNames))
                    pendingDirectories.Push(childDirectory);
            }
        }

        return files
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static HashSet<string> BuildSkippedDirectoryNameSet(IEnumerable<string>? additionallySkippedDirectoryNames)
    {
        var skippedDirectoryNames = new HashSet<string>(AlwaysSkippedDirectoryNames, StringComparer.OrdinalIgnoreCase);
        if (additionallySkippedDirectoryNames is null)
            return skippedDirectoryNames;

        foreach (var directoryName in additionallySkippedDirectoryNames)
        {
            if (!string.IsNullOrWhiteSpace(directoryName))
                skippedDirectoryNames.Add(directoryName);
        }

        return skippedDirectoryNames;
    }

    private static bool IsSkippedDirectory(string directoryPath, HashSet<string> skippedDirectoryNames)
        => skippedDirectoryNames.Contains(Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));

    private static string[] EnumerateFilesInOneDirectory(string directoryPath, string searchPattern)
    {
        try
        {
            return Directory
                .EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly)
                .ToArray();
        }
        catch (Exception exception) when (IsSafeToSkipDuringRepositoryScan(exception))
        {
            return [];
        }
    }

    private static string[] EnumerateChildDirectories(string directoryPath)
    {
        try
        {
            return Directory
                .EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly)
                .ToArray();
        }
        catch (Exception exception) when (IsSafeToSkipDuringRepositoryScan(exception))
        {
            return [];
        }
    }

    private static bool IsSafeToSkipDuringRepositoryScan(Exception exception)
        => exception is UnauthorizedAccessException
            or DirectoryNotFoundException
            or IOException;
}
