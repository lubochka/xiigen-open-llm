// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticNlp;

// ─── In-memory fabric implementations (for tests and demos) ─────────────────

// InMemoryTextSource serves text from a pre-populated dictionary. Used by tests
// so they can run without touching the disk.
public sealed class InMemoryTextSource(Dictionary<string, string> initialTexts) : ITextSource
{
    // Internal copy of the texts so external mutation cannot leak in.
    private readonly Dictionary<string, string> _texts = new(initialTexts);

    // Look up the text by name; failure when absent so callers can tell the difference.
    public Task<OperationResult<string>> ReadTextAsync(string sourceName)
        => Task.FromResult(_texts.TryGetValue(sourceName, out var text)
            ? OperationResult<string>.Ok(text)
            : OperationResult<string>.Fail($"Text '{sourceName}' not found", "TEXT_NOT_FOUND"));

    // Return every key currently present in the in-memory dictionary.
    public Task<OperationResult<IReadOnlyList<string>>> ListTextsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)[.. _texts.Keys]));
}

// InMemorySemanticStorage keeps annotations in a process-local dictionary.
// Two threads writing the same key produce the last-writer-wins outcome, which
// is acceptable for tests where only one thread runs at a time.
public sealed class InMemorySemanticStorage : ISemanticStorage
{
    // Map of sentenceId → the annotation last written under that id.
    private readonly Dictionary<string, SemanticAnnotation> _annotations = [];

    // Save replaces any prior annotation under the same id (idempotent overwrite).
    public Task<OperationResult<bool>> SaveAnnotationAsync(string sentenceId, SemanticAnnotation annotation)
    {
        _annotations[sentenceId] = annotation;
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    // Load returns the saved annotation; failure when nothing has been written under that id.
    public Task<OperationResult<SemanticAnnotation>> LoadAnnotationAsync(string sentenceId)
        => Task.FromResult(_annotations.TryGetValue(sentenceId, out var annotation)
            ? OperationResult<SemanticAnnotation>.Ok(annotation)
            : OperationResult<SemanticAnnotation>.Fail(
                $"Annotation '{sentenceId}' not found", "ANNOTATION_NOT_FOUND"));
}

// InMemorySemanticEventLogger appends every StandardEvent to a public list so
// tests can assert that the right events fired in the right order.
public sealed class InMemorySemanticEventLogger : ISemanticEventLogger
{
    // Public list of recorded events — tests read this to verify event flow.
    public List<StandardEvent> Events { get; } = [];

    // Log appends to the list; this implementation never fails.
    public OperationResult<bool> Log(StandardEvent eventEnvelope)
    {
        Events.Add(eventEnvelope);
        return OperationResult<bool>.Ok(true);
    }
}
