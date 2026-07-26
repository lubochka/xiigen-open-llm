// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticNlp;

// ─── Fabric interfaces (DNA rule 4 — interface contracts) ───────────────────
// Concrete implementations are injected through these interfaces so that
// services never instantiate concrete dependencies directly (fabric-first).

// ITextSource hides where input sentences come from — the algorithm sees only
// "give me the text named X". Implementations: in-memory dictionary (tests),
// file-system reader (production).
public interface ITextSource
{
    // Read the text stored under the given source name.
    // Returns failure when no entry with that name exists.
    Task<OperationResult<string>> ReadTextAsync(string sourceName);

    // List all source names available in this provider.
    Task<OperationResult<IReadOnlyList<string>>> ListTextsAsync();
}

// ISemanticLexicon hides where the algorithm's reference data lives.
// FrameNet stores frames; WordNet stores synsets — the same interface covers both.
public interface ISemanticLexicon
{
    // Find every frame name that the given word can activate (FrameNet only).
    // Returns an empty list when the word is not in any frame — never a failure.
    Task<OperationResult<IReadOnlyList<string>>> FindFramesForWordAsync(string word);

    // Find every synset id that contains the given word (WordNet only).
    // Returns an empty list when the word is not in any synset — never a failure.
    Task<OperationResult<IReadOnlyList<string>>> FindSynsetsForWordAsync(string word);

    // Return all words that belong to the given frame name or synset id.
    // Returns failure when the name does not exist in the lexicon.
    Task<OperationResult<IReadOnlyList<string>>> GetFrameMembersAsync(string frameOrSynsetName);
}

// ISemanticStorage persists annotations so they can be looked up later.
// Implementations: in-memory dictionary (tests), JSON-on-disk (production).
public interface ISemanticStorage
{
    // Save the annotation under a stable id (typically the operationId).
    Task<OperationResult<bool>> SaveAnnotationAsync(string sentenceId, SemanticAnnotation annotation);

    // Load a previously saved annotation by id; failure when absent.
    Task<OperationResult<SemanticAnnotation>> LoadAnnotationAsync(string sentenceId);
}

// ISemanticEventLogger records StandardEvent envelopes. The dispatcher and
// services emit events through this interface so observability stays decoupled.
public interface ISemanticEventLogger
{
    // Append the envelope to the audit log (in-memory list, file, or queue).
    OperationResult<bool> Log(StandardEvent eventEnvelope);
}

// ISemanticAnnotator is the polymorphic contract every per-algorithm service
// implements. The dispatcher (DNA rule 6) picks an annotator by name and calls
// AnnotateAsync — it does not know which algorithm runs underneath.
public interface ISemanticAnnotator
{
    // Stable name of this annotator: "FrameNet", "SRL", "WordNet", "AMR", "UD".
    string AlgorithmName { get; }

    // Run the algorithm against the given sentence and return a full annotation.
    // The same operationId returns the cached result on repeat calls.
    Task<OperationResult<SemanticAnnotation>> AnnotateAsync(string operationId, string sentence);
}
