namespace SemanticNlp;

// ─── Subproject 31 — Abstract Meaning Representation (AMR) ──────────────────
// AMR drops the surface words and keeps only the meaning, expressed as a
// directed graph of concepts and roles. The standard text representation is
// Penman notation: nested parenthesised expressions of the form
//   (variable / concept :role value)
// where value is either a literal, a variable reference, or a nested node.
// Example: (w / want-01 :ARG0 (b / boy) :ARG1 (g / girl))

// ─── AmrPenmanParser ────────────────────────────────────────────────────────
// Parses Penman-formatted strings into AmrGraph values. The parser is a
// hand-written recursive-descent reader; it accepts whitespace freely and
// reports parse failures via OperationResult instead of throwing.
public sealed class AmrPenmanParser
{
    // Top-level entry point. Returns an AmrGraph with the original sentence
    // field left empty — callers can fill it in if they want to track it.
    public OperationResult<AmrGraph> Parse(string penmanText)
    {
        if (string.IsNullOrWhiteSpace(penmanText))
            return OperationResult<AmrGraph>.Fail(
                "Penman text is empty", "EMPTY_INPUT");

        var nodes = new Dictionary<string, AmrNode>();
        var edges = new List<AmrEdge>();

        try
        {
            var (rootNode, _) = ParseNode(penmanText.Trim(), 0, nodes, edges);

            if (rootNode is null)
                return OperationResult<AmrGraph>.Fail(
                    "Failed to parse the root node", "PARSE_ERROR");

            return OperationResult<AmrGraph>.Ok(new AmrGraph
            {
                RootNode         = rootNode,
                Nodes            = nodes,
                Edges            = edges,
                OriginalSentence = string.Empty,
            });
        }
        catch (Exception ex)
        {
            // Catch-all for malformed input — convert into business-level failure.
            return OperationResult<AmrGraph>.Fail(
                $"Penman parse error: {ex.Message}", "PARSE_EXCEPTION");
        }
    }

    // Recursive parser. Reads one parenthesised expression starting at
    // 'position' and returns the parsed node plus the index after its
    // closing ')'. Subsequent characters belong to the parent context.
    private (AmrNode? Node, int NextPosition) ParseNode(
        string                  text,
        int                     position,
        Dictionary<string, AmrNode> nodes,
        List<AmrEdge>           edges)
    {
        position = SkipWhitespace(text, position);

        // Expect '(' to start the node.
        if (position >= text.Length || text[position] != '(')
            return (null, position);
        position++;

        position = SkipWhitespace(text, position);

        // Read variable name — terminated by whitespace or '/'.
        int variableStart = position;
        while (position < text.Length && !char.IsWhiteSpace(text[position]) && text[position] != '/')
            position++;
        string variableName = text[variableStart..position];

        // Skip whitespace and the '/' separator.
        position = SkipWhitespace(text, position);
        if (position < text.Length && text[position] == '/') position++;
        position = SkipWhitespace(text, position);

        // Read the concept — terminated by whitespace, ')' or ':'.
        int conceptStart = position;
        while (position < text.Length
               && !char.IsWhiteSpace(text[position])
               && text[position] != ')'
               && text[position] != ':')
            position++;
        string concept = text[conceptStart..position];

        // Build the node and register it in the graph index.
        var node = new AmrNode { VariableName = variableName, Concept = concept };
        nodes[variableName] = node;

        // Now read role/value pairs until we see the closing ')'.
        while (true)
        {
            position = SkipWhitespace(text, position);
            if (position >= text.Length || text[position] == ')') break;

            // Each role label starts with ':'. Anything else is malformed.
            if (text[position] != ':')
                throw new FormatException($"Expected role label at position {position}, got '{text[position]}'");

            position++;
            int labelStart = position;
            while (position < text.Length && !char.IsWhiteSpace(text[position]) && text[position] != '(')
                position++;
            string relationLabel = ":" + text[labelStart..position];

            position = SkipWhitespace(text, position);

            // The value is either a nested node or a bare token (variable / literal).
            if (position < text.Length && text[position] == '(')
            {
                var (childNode, nextPosition) = ParseNode(text, position, nodes, edges);
                position = nextPosition;
                if (childNode is not null)
                {
                    edges.Add(new AmrEdge
                    {
                        SourceVariable = variableName,
                        RelationLabel  = relationLabel,
                        TargetVariable = childNode.VariableName,
                    });
                }
            }
            else
            {
                int valueStart = position;
                while (position < text.Length && !char.IsWhiteSpace(text[position]) && text[position] != ')')
                    position++;
                string targetValue = text[valueStart..position];
                if (targetValue.Length > 0)
                {
                    edges.Add(new AmrEdge
                    {
                        SourceVariable = variableName,
                        RelationLabel  = relationLabel,
                        TargetVariable = targetValue,
                    });
                }
            }
        }

        // Consume the closing ')'.
        if (position < text.Length && text[position] == ')') position++;

        return (node, position);
    }

    // Move past any run of whitespace characters.
    private static int SkipWhitespace(string text, int position)
    {
        while (position < text.Length && char.IsWhiteSpace(text[position])) position++;
        return position;
    }
}

// ─── AmrPenmanSerializer ────────────────────────────────────────────────────
// Renders an AmrGraph back into Penman text. The serializer prints in the
// same nested form the parser accepts. Edges that target a known variable
// are printed as nested nodes; edges that target an unknown name are
// printed as bare values (constants / variable references).
public sealed class AmrPenmanSerializer
{
    public OperationResult<string> Serialize(AmrGraph graph)
    {
        if (graph is null)
            return OperationResult<string>.Fail("Graph is null", "NULL_GRAPH");

        // Group edges by their source variable for efficient lookup.
        var edgesBySource = graph.Edges
            .GroupBy(edge => edge.SourceVariable)
            .ToDictionary(group => group.Key, group => group.ToList());

        var visited = new HashSet<string>();
        var buffer  = new System.Text.StringBuilder();

        SerializeNode(graph.RootNode, edgesBySource, graph.Nodes, visited, buffer);

        return OperationResult<string>.Ok(buffer.ToString());
    }

    private static void SerializeNode(
        AmrNode                                node,
        Dictionary<string, List<AmrEdge>>      edgesBySource,
        IReadOnlyDictionary<string, AmrNode>   allNodes,
        HashSet<string>                        visited,
        System.Text.StringBuilder              buffer)
    {
        // Write the node's variable / concept block.
        buffer.Append('(').Append(node.VariableName).Append(" / ").Append(node.Concept);

        // Mark this variable as already expanded so cycles or shared subgraphs
        // do not get re-expanded; we print a bare reference instead.
        visited.Add(node.VariableName);

        if (edgesBySource.TryGetValue(node.VariableName, out var outgoing))
        {
            foreach (var edge in outgoing)
            {
                buffer.Append(' ').Append(edge.RelationLabel).Append(' ');

                // If the target is a known unvisited node, expand it inline.
                if (allNodes.TryGetValue(edge.TargetVariable, out var targetNode)
                    && !visited.Contains(edge.TargetVariable))
                {
                    SerializeNode(targetNode, edgesBySource, allNodes, visited, buffer);
                }
                else
                {
                    // Otherwise emit the literal target token.
                    buffer.Append(edge.TargetVariable);
                }
            }
        }

        buffer.Append(')');
    }
}

// ─── SimpleAmrBuilder ───────────────────────────────────────────────────────
// Builds a minimal AMR graph from a sentence using a verb-as-predicate rule.
// The first verb-looking token becomes the root concept (e.g. "want-01"); the
// first non-verb before it becomes ARG0; the first non-verb after it becomes
// ARG1. Sufficient to demonstrate AMR shape on canonical example sentences.
public sealed class SimpleAmrBuilder
{
    private readonly PredicateFinder _predicateFinder;

    public SimpleAmrBuilder(PredicateFinder predicateFinder)
    {
        _predicateFinder = predicateFinder ?? throw new ArgumentNullException(nameof(predicateFinder));
    }

    public OperationResult<AmrGraph> BuildFromSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return OperationResult<AmrGraph>.Fail(
                "Sentence is empty", "EMPTY_SENTENCE");

        var tokens = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Trim('.', ',', '!', '?', ';', ':'))
            .Where(token => token.Length > 0)
            .ToList();

        if (tokens.Count == 0)
            return OperationResult<AmrGraph>.Fail("No usable tokens", "NO_TOKENS");

        var predicateResult = _predicateFinder.FindPredicatePositions(tokens);
        if (!predicateResult.IsSuccess)
            return predicateResult.MapError<AmrGraph>();

        // Pick the first predicate; use the first token if none was detected.
        int rootIndex = predicateResult.Value!.Count > 0 ? predicateResult.Value![0] : 0;

        // Build the predicate concept as "lemma-01" — sense numbers are not derivable
        // from text alone, so we always pick "01" for this learning implementation.
        string rootConcept = tokens[rootIndex].ToLowerInvariant() + "-01";

        var rootNode = new AmrNode { VariableName = "p", Concept = rootConcept };
        var nodes    = new Dictionary<string, AmrNode> { ["p"] = rootNode };
        var edges    = new List<AmrEdge>();

        // ARG0 — first non-verb token before the predicate.
        int variableCounter = 0;
        for (int tokenIndex = 0; tokenIndex < rootIndex; tokenIndex++)
        {
            if (predicateResult.Value!.Contains(tokenIndex)) continue;

            string variable = $"a{variableCounter++}";
            nodes[variable] = new AmrNode
            {
                VariableName = variable,
                Concept      = tokens[tokenIndex].ToLowerInvariant(),
            };
            edges.Add(new AmrEdge
            {
                SourceVariable = "p",
                RelationLabel  = ":ARG0",
                TargetVariable = variable,
            });
            break;
        }

        // ARG1 — first non-verb token after the predicate.
        for (int tokenIndex = rootIndex + 1; tokenIndex < tokens.Count; tokenIndex++)
        {
            if (predicateResult.Value!.Contains(tokenIndex)) continue;

            string variable = $"a{variableCounter++}";
            nodes[variable] = new AmrNode
            {
                VariableName = variable,
                Concept      = tokens[tokenIndex].ToLowerInvariant(),
            };
            edges.Add(new AmrEdge
            {
                SourceVariable = "p",
                RelationLabel  = ":ARG1",
                TargetVariable = variable,
            });
            break;
        }

        return OperationResult<AmrGraph>.Ok(new AmrGraph
        {
            RootNode         = rootNode,
            Nodes            = nodes,
            Edges            = edges,
            OriginalSentence = sentence,
        });
    }
}

// ─── AmrAnnotator ───────────────────────────────────────────────────────────
// Adapts the AMR builder to the SemanticAnnotation contract by emitting one
// SemanticToken per AMR node, where the family is the algorithm name and the
// role is the AMR concept ("want-01", "boy", etc.).
public sealed class AmrAnnotator
{
    private readonly SimpleAmrBuilder    _builder;
    private readonly AmrPenmanSerializer _serializer;

    public AmrAnnotator(SimpleAmrBuilder builder, AmrPenmanSerializer serializer)
    {
        _builder    = builder    ?? throw new ArgumentNullException(nameof(builder));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public Task<OperationResult<SemanticAnnotation>> AnnotateAsync(string sentence)
    {
        var graphResult = _builder.BuildFromSentence(sentence);
        if (!graphResult.IsSuccess)
            return Task.FromResult(graphResult.MapError<SemanticAnnotation>());

        var graph = graphResult.Value!;

        // Tokenize the original sentence into surface words for the SemanticToken list.
        var surfaceTokens = sentence
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        // Build a quick lookup so we can attach the right concept to each token.
        var conceptByLower = graph.Nodes.Values
            .GroupBy(node => node.Concept.Split('-')[0])
            .ToDictionary(group => group.Key, group => group.First());

        var annotated = new List<SemanticToken>(surfaceTokens.Count);
        for (int tokenIndex = 0; tokenIndex < surfaceTokens.Count; tokenIndex++)
        {
            string surface = surfaceTokens[tokenIndex];
            string clean   = surface.Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();

            string? role;
            if (conceptByLower.TryGetValue(clean, out var node))
                role = node.Concept;
            else
                role = null;

            annotated.Add(new SemanticToken
            {
                OriginalText     = surface,
                Position         = tokenIndex,
                SemanticFamily   = "AMR",
                RoleWithinFamily = role,
            });
        }

        // Use the Penman serialization as the predicate text — it carries the full graph.
        var penman = _serializer.Serialize(graph);

        return Task.FromResult(OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
        {
            OriginalSentence = sentence,
            Tokens           = annotated,
            AlgorithmName    = "AMR",
            PredicateText    = penman.IsSuccess ? penman.Value : null,
            FrameName        = graph.RootNode.Concept,
        }));
    }
}

// ─── AmrService ─────────────────────────────────────────────────────────────
public sealed class AmrService : BaseService, ISemanticAnnotator
{
    private readonly AmrAnnotator _annotator;

    public string AlgorithmName => "AMR";

    public AmrService(
        AmrAnnotator         annotator,
        ISemanticStorage     storage,
        ISemanticEventLogger eventLogger)
        : base(storage, eventLogger)
    {
        _annotator = annotator ?? throw new ArgumentNullException(nameof(annotator));
    }

    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(
        string operationId, string sentence)
    {
        if (Idempotency.TryGetResult(operationId, out var cached)
            && cached.TryGetValue("annotation", out var cachedObj)
            && cachedObj is SemanticAnnotation cachedAnnotation)
        {
            return OperationResult<SemanticAnnotation>.Ok(cachedAnnotation);
        }

        var result = await _annotator.AnnotateAsync(sentence);
        if (!result.IsSuccess) return result;

        await Storage.SaveAnnotationAsync(operationId, result.Value!);

        EventLogger.Log(new StandardEvent(
            EventType:   "semantic.annotation.completed",
            OperationId: operationId,
            OccurredAt:  DateTimeOffset.UtcNow,
            Payload:     new Dictionary<string, object>
            {
                ["algorithm"] = AlgorithmName,
                ["sentence"]  = sentence,
                ["root"]      = result.Value!.FrameName ?? string.Empty,
            }));

        Idempotency.Record(operationId, new Dictionary<string, object>
        {
            ["annotation"] = result.Value!,
        });

        return result;
    }
}
