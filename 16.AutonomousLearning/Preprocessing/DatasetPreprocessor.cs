// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AutonomousLearning;

using AgentInfrastructure;
using ContinuousLearning;
using SC29 = SemanticCompiler29;

// DatasetPreprocessor — applies the E20 gate then stores labeled training examples.
// E20 HARD RULE (non-negotiable): ISemanticCompiler29.Compile runs FIRST, BEFORE any
// normalization. Normalization destroys the language markers that StackDetector uses.
// Confirmed by E20 SCALE_UP experiment — reversing this order caused complete label failure.
public sealed class DatasetPreprocessor : BaseService
{
    // _compiler — injected ISemanticCompiler29 from 29.SemanticCompiler.
    // Never reimplemented here — Principle 4 (No Duplication).
    private readonly SC29.ISemanticCompiler29 _compiler;

    // _cache — injected IDomainDatasetCache from 14.ContinuousLearning.
    // Each successfully processed example is added to the cache for IncrementalTrainer.
    private readonly IDomainDatasetCache _cache;

    // Constructor: fabric-first injection of compiler and cache — no concrete types created here.
    public DatasetPreprocessor(
        ISessionStorage          storage,
        SC29.ISemanticCompiler29 compiler,
        IDomainDatasetCache      cache)
        : base(storage)
    {
        // Store compiler reference — called in Step 1 of every Process invocation.
        _compiler = compiler;
        // Store cache reference — example added in Step 4 after domain detection.
        _cache    = cache;
    }

    // Process — run the four-step pipeline for one raw text + source pair.
    // Step 1: StackDetector (E20 gate) — MUST run before Step 2.
    // Step 2: Normalization — ONLY after domain detection is complete.
    // Step 3: Build TrainingExample with detected domain.
    // Step 4: Add to IDomainDatasetCache for later IncrementalTrainer use.
    // Returns Fail for: empty text, LOW trust source, or compiler failure.
    public OperationResult<TrainingExample> Process(string rawText, SourceEntry source)
    {
        // Guard 1: empty text cannot produce a useful labeled example.
        if (string.IsNullOrWhiteSpace(rawText))
            return OperationResult<TrainingExample>.Fail(
                "Raw text is empty or whitespace — cannot label an empty example.",
                "EMPTY_TEXT");

        // Guard 2: LOW trust sources are excluded to keep dataset quality high.
        // MEDIUM and HIGH trust sources pass through to the pipeline.
        if (source.Trust == TrustLevel.Low)
            return OperationResult<TrainingExample>.Fail(
                $"Source '{source.Url}' has LOW trust and is excluded from the dataset.",
                "LOW_TRUST_SOURCE");

        // ── Step 1: StackDetector FIRST (E20 hard gate) ───────────────────────
        // ISemanticCompiler29 runs StackDetector internally as its first stage.
        // The raw, un-normalized text is passed — normalization has NOT happened yet.
        // Reversing this order destroys C# language markers before Detect() can see them.
        var compileResult = _compiler.Compile(new SC29.PipelineInput(rawText, FilePath: null));
        if (!compileResult.IsSuccess)
            return OperationResult<TrainingExample>.Fail(
                $"SemanticCompiler29 failed: {compileResult.ErrorMessage}",
                "COMPILER_FAILED");

        // Extract the domain string from the E18 RouteClassifier output.
        var domain = compileResult.Value!.Domain.ToString();

        // ── Step 2: Normalize AFTER StackDetector has run ─────────────────────
        // Only now that domain detection is complete can we safely alter the text.
        var normalizedText = Normalize(rawText);

        // ── Step 3: Build a TrainingExample with the compiler-assigned domain ──
        // Weight starts at 1.0 — DatasetWeighter applies D-09 formula later.
        var example = new TrainingExample(
            Prompt:     normalizedText,
            Response:   normalizedText,
            Domain:     domain,
            Weight:     1.0,
            CapturedAt: DateTimeOffset.UtcNow);

        // ── Step 4: Add to cache for IncrementalTrainer ────────────────────────
        // If the cache Add fails, still return the example — caching is non-blocking.
        _cache.Add(example);

        // Return the labeled example — caller can inspect domain and forward to trainer.
        return OperationResult<TrainingExample>.Ok(example);
    }

    // Normalize — apply text normalization ONLY after StackDetector has run (E20 rule).
    // Trims surrounding whitespace; collapses Windows CRLF (\r\n) to Unix LF (\n).
    private static string Normalize(string text)
        => text.Trim().Replace("\r\n", "\n");
}
