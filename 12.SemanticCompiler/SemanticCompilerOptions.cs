// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// Feature flags for SemanticCompilerFacade integrations.
public sealed class SemanticCompilerOptions
{
    // E10: strip XML docs, comments, strings, and blank lines before C# parsing.
    public bool UseRegexPreprocessor { get; init; } = true;

    // E09: run structural KNN next-token prediction when the predictor is trained.
    public bool UseStructuralKnn { get; init; } = false;

    // E13: predict the canonical type of the next method when MethodKnn is trained.
    public bool UseMethodKnn { get; init; } = true;

    // E14: predict a file-level edit action when FileActionKnn is trained.
    public bool UseFileActionKnn { get; init; } = false;

    // Disable optional experiment-backed steps for baseline comparisons.
    public static SemanticCompilerOptions BaselineOnly => new()
    {
        UseRegexPreprocessor = false,
        UseStructuralKnn = false,
        UseMethodKnn = false,
        UseFileActionKnn = false
    };

    // Production defaults preserve the current regex behavior and leave E09 opt-in.
    public static SemanticCompilerOptions Default => new();
}
