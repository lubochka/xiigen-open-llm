// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// StopSignalDetector: detect when task decomposition should stop.
// E33: baseline and enhanced both hit 100% after StratifiedSplit fix — problem solved at data level.
// This class encodes the four Depth-Calibration stop signals as explicit rules.
// These rules prevent infinite decomposition of already-atomic tasks.
public sealed class StopSignalDetector : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public StopSignalDetector(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Markers for stop signal 1: sub-tasks are sequential execution steps, not sub-tasks.
    private static readonly string[] SequentialStepMarkers =
        ["step 1", "step 2", "first,", "then,", "finally,", "next,", "after that,"];

    // Markers for stop signal 2: sub-tasks require unexplained technical context.
    private static readonly string[] TechnicalContextMarkers =
        ["jwt","ioc","di ","linq","roslyn","ienumerable","async/await","cancellationtoken",
         "nullable reference","expression tree","delegate","generic constraint"];

    // Markers for stop signal 3: single document produced once, no iteration.
    private static readonly string[] SingleDocumentMarkers =
        ["one document", "single page", "once", "produced once", "no iteration"];

    // Stop signal 4: no correction loop — no feedback or version history.
    // Detected by the ABSENCE of iteration markers.
    private static readonly string[] IterationMarkers =
        ["review", "feedback", "iterate", "again", "repeat", "version", "revision"];

    // Determine whether decomposition should stop at this sub-task level.
    // Returns true (stop) when any of the four signals is detected.
    public bool ShouldStop(string[] subTaskPredicates, int currentDepth)
    {
        string joined = string.Join(" ", subTaskPredicates).ToLowerInvariant();

        // Signal 1: next level = sequential steps (already at execution granularity).
        if (SequentialStepMarkers.Any(m => joined.Contains(m, StringComparison.Ordinal)))
            return true;

        // Signal 2: technical context required to interpret sub-tasks.
        if (TechnicalContextMarkers.Any(m => joined.Contains(m, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Signal 3: single document, no structural decomposition possible.
        if (SingleDocumentMarkers.Any(m => joined.Contains(m, StringComparison.Ordinal)))
            return true;

        // Signal 4: no correction loop — stop after the first decomposition level.
        bool hasIteration = IterationMarkers.Any(m => joined.Contains(m, StringComparison.Ordinal));
        if (!hasIteration && currentDepth >= 1)
            return true;

        return false;
    }
}
