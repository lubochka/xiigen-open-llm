namespace SemanticNlp;

// ─── SemanticNlpDispatcher (DNA rule 6) ─────────────────────────────────────
// One dispatcher routes annotation requests to the right ISemanticAnnotator
// instead of having one HTTP controller (or equivalent) per algorithm. All
// callers go through Dispatch — they pass the algorithm name and the
// dispatcher looks up the matching annotator.
public sealed class SemanticNlpDispatcher
{
    // Map of algorithm name → annotator implementation. Built once in the
    // constructor; immutable for the lifetime of the dispatcher.
    private readonly Dictionary<string, ISemanticAnnotator> _annotators;

    // Optional event logger so the dispatcher can record routing decisions.
    private readonly ISemanticEventLogger _eventLogger;

    public SemanticNlpDispatcher(
        IEnumerable<ISemanticAnnotator> annotators,
        ISemanticEventLogger            eventLogger)
    {
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));

        _annotators = new Dictionary<string, ISemanticAnnotator>(StringComparer.OrdinalIgnoreCase);
        foreach (var annotator in annotators ?? Array.Empty<ISemanticAnnotator>())
        {
            if (string.IsNullOrEmpty(annotator.AlgorithmName))
                throw new ArgumentException("Annotator must declare AlgorithmName", nameof(annotators));
            _annotators[annotator.AlgorithmName] = annotator;
        }
    }

    // Names of all registered algorithms — used by callers that want to
    // discover what is available before issuing a request.
    public IReadOnlyList<string> AvailableAlgorithms => [.. _annotators.Keys];

    // Route the request to the named annotator and return its annotation.
    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(
        string algorithmName, string operationId, string sentence)
    {
        if (!_annotators.TryGetValue(algorithmName, out var annotator))
            return OperationResult<SemanticAnnotation>.Fail(
                $"Unknown algorithm '{algorithmName}'. Available: {string.Join(", ", _annotators.Keys)}",
                "ALGORITHM_NOT_FOUND");

        // Record the routing decision before delegating; useful for audit.
        _eventLogger.Log(new StandardEvent(
            EventType:   "semantic.dispatch.requested",
            OperationId: operationId,
            OccurredAt:  DateTimeOffset.UtcNow,
            Payload:     new Dictionary<string, object>
            {
                ["algorithm"] = algorithmName,
                ["sentence"]  = sentence,
            }));

        return await annotator.AnnotateAsync(operationId, sentence);
    }
}
