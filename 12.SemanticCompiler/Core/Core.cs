// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// ─── Result envelope ─────────────────────────────────────────────────────────

public sealed class OperationResult<T>
{
    public bool   IsSuccess    { get; }
    public T?     Value        { get; }
    public string ErrorMessage { get; }
    public string ErrorCode    { get; }

    private OperationResult(bool ok, T? v, string msg, string code)
        => (IsSuccess, Value, ErrorMessage, ErrorCode) = (ok, v, msg, code);

    public static OperationResult<T> Ok(T value)
        => new(true, value, "", "");
    public static OperationResult<T> Fail(string message, string code = "ERROR")
        => new(false, default, message, code);
}

// ─── Stack labels ────────────────────────────────────────────────────────────

// Technology stack detected by StackDetector (E20).
public enum StackLabel { CSharp, TypeScript, Python, Generic }

// ─── Pipeline result ─────────────────────────────────────────────────────────

// Full result of running the SemanticCompiler pipeline on one input.
public sealed record CompilationResult(
    StackLabel        Stack,
    SemanticCompiler29.Domain Domain,
    IReadOnlyList<int> Predicates,       // E17 logical predicate sequence
    IReadOnlyList<string> Ops,           // E16 semantic op tokens
    IReadOnlyList<string> NormalizedWords,// E15 stemmed words
    IReadOnlyList<PromptSegment> Segments // E21 prompt segments
)
{
    // Optional E05 grammar-aware next-token predictions for C# source inputs.
    public IReadOnlyList<int>? PredictedNextKinds { get; init; }

    // Optional E09 structural KNN next-token predictions for C# source inputs.
    public IReadOnlyList<int> PredictedNextTokens { get; init; } = [];

    // Optional E13 canonical type prediction for the next method.
    public string? PredictedNextMethodType { get; init; }

    // Optional E14 file-level edit-action prediction.
    public string? PredictedFileAction { get; init; }
}

// One segment detected by the PromptSplitter (E21).
public sealed record PromptSegment(string Text, int WordStart, int WordEnd);

// Tagged word with POS (Part-Of-Speech) tag for translation (E25-E27).
public sealed record TaggedWord(string Text, int PosTag);

// ─── Main interface ───────────────────────────────────────────────────────────

// ISemanticCompiler is the production contract for the E01-E27 pipeline.
// Pipeline order (E20 CRITICAL): StackDetector → Domain → Normalize → Collapse → Extract.
public interface ISemanticCompiler
{
    // Detect technology stack from raw source (BEFORE normalization — E20 rule).
    OperationResult<StackLabel> DetectStack(string source);

    // Classify request domain from prompt text using predicate profile (E18).
    OperationResult<SemanticCompiler29.Domain> ClassifyDomain(string prompt);

    // Remove stopwords + stem all identifier words from C# source (E15).
    OperationResult<IReadOnlyList<string>> NormalizeSource(string csharpSource);

    // Collapse stemmed words to semantic op families (E16).
    OperationResult<IReadOnlyList<string>> CollapseToOps(IReadOnlyList<string> stemmedWords);

    // Extract logical predicate sequence from C# source using Roslyn (E17).
    OperationResult<IReadOnlyList<int>> ExtractPredicates(string csharpSource);

    // Split a natural-language prompt into task segments (E21).
    OperationResult<IReadOnlyList<PromptSegment>> SplitPrompt(string prompt);

    // Classify the stop signal for a sub-task (E33).
    OperationResult<int> DetectStopSignal(string taskDescription, bool hasIterations, bool hasTechTerms);

    // Retrieve best matching code template for predicate sequence (E23).
    OperationResult<string> BeautifyPredicates(IReadOnlyList<int> predicates, StackLabel stack);

    // Rank completion candidates by receiver-local call frequency (E02).
    OperationResult<IReadOnlyList<string>> RankCompletions(string receiver, IReadOnlyList<string> candidates);

    // Run the full pipeline and return combined CompilationResult.
    OperationResult<CompilationResult> Compile(string source);
}
