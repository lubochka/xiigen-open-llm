// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System;
using System.IO;

namespace LlmFromScratch.Tests;

/// <summary>
/// Shared, machine-neutral path resolver for the test suite. Every path is
/// derived at runtime from the repository layout or the current user profile,
/// so the tests resolve to byte-identical locations on the machine that owns
/// the checkout without embedding any hard-coded username or absolute path.
/// </summary>
internal static class TestMachineRoots
{
    private static string? cachedRepoRoot;

    /// <summary>
    /// The repository root, located by ascending from the test assembly's base
    /// directory until an ancestor holds both the <c>AGENTS.md</c> marker and
    /// the <c>LlmFromScratch.sln</c> solution file. Requiring both markers
    /// prevents the walk from latching onto an unrelated ancestor that happens
    /// to carry only one of them. The result is cached after the first lookup.
    /// </summary>
    public static string RepoRoot
    {
        get
        {
            if (cachedRepoRoot is not null)
            {
                return cachedRepoRoot;
            }

            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current is not null)
            {
                if (File.Exists(Path.Combine(current.FullName, "AGENTS.md"))
                    && File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln")))
                {
                    cachedRepoRoot = current.FullName;
                    return cachedRepoRoot;
                }

                current = current.Parent;
            }

            throw new InvalidOperationException(
                "Could not locate repository root ascending from " +
                $"'{AppContext.BaseDirectory}': no ancestor directory contains " +
                "both AGENTS.md and LlmFromScratch.sln.");
        }
    }

    /// <summary>
    /// Resolves a directory that sits beside the repository root — i.e. a
    /// sibling of <see cref="RepoRoot"/> sharing the same parent directory.
    /// </summary>
    /// <param name="name">The sibling directory name.</param>
    public static string SiblingRoot(string name) => Path.GetFullPath(Path.Combine(RepoRoot, "..", name));

    /// <summary>
    /// Root directory for adaptive-model checkpoints. Honours the
    /// <c>XIIGEN_ADAPTIVE_MODELS_ROOT</c> environment override when set;
    /// otherwise defaults to <c>.xiigen/adaptive-models</c> under the current
    /// user's profile directory.
    /// </summary>
    public static string AdaptiveModelsRoot
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("XIIGEN_ADAPTIVE_MODELS_ROOT");
            if (!string.IsNullOrEmpty(configured))
            {
                return configured;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".xiigen",
                "adaptive-models");
        }
    }

    /// <summary>
    /// Root directory for XIIGen documentation artifacts. Honours the
    /// <c>XIIGEN_DOCS_ROOT</c> environment override when set; otherwise defaults
    /// to <c>Documents/xiigen</c> under the current user's profile directory.
    /// </summary>
    public static string XiigenDocsRoot
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("XIIGEN_DOCS_ROOT");
            if (!string.IsNullOrEmpty(configured))
            {
                return configured;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Documents",
                "xiigen");
        }
    }
}
