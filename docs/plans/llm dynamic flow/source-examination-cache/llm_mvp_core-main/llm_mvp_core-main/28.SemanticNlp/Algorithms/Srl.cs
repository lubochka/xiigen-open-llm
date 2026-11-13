namespace SemanticNlp;

// ─── Subproject 29 — Semantic Role Labeling (SRL) ───────────────────────────
// Every event in a sentence is anchored by a verb (the predicate). SRL finds
// the predicate and assigns each surrounding word a role relative to it:
//   A0  — Agent (the doer)
//   A1  — Theme / Patient (what is acted on)
//   A2  — Beneficiary (the indirect object)
//   AM-LOC — locative modifier (where)
//   AM-TMP — temporal modifier (when)
//   AM-MNR — manner modifier (how)
// Source for the rule heuristics: PropBank annotation guidelines.

// ─── PredicateFinder ────────────────────────────────────────────────────────
// Identifies which token positions are verbs/predicates. The full SRL pipeline
// in production uses a neural sequence labeller; this implementation uses a
// curated verb dictionary plus simple suffix heuristics so it works without
// any trained model.
public sealed class PredicateFinder
{
    // Common English verb suffixes that indicate a verb form even when the
    // verb is not in the dictionary (e.g. "containerize" → ends in "ize").
    private static readonly HashSet<string> CommonVerbSuffixes
        = new(StringComparer.OrdinalIgnoreCase) { "ed", "ing", "ize", "ise", "fy", "ate", "en" };

    // Vocabulary of known irregular verbs (and a few high-frequency regular ones)
    // injected at construction so callers can extend it.
    private readonly HashSet<string> _knownVerbs;

    public PredicateFinder(IEnumerable<string> verbVocabulary)
    {
        _knownVerbs = new HashSet<string>(verbVocabulary, StringComparer.OrdinalIgnoreCase);
    }

    // Convenience factory with a default vocabulary that covers the test sentences.
    public static PredicateFinder CreateDefault() => new([
        "be", "is", "are", "was", "were", "been", "being",
        "have", "has", "had",
        "do", "does", "did", "done",
        "go", "goes", "went", "gone",
        "say", "says", "said",
        "get", "gets", "got", "gotten",
        "make", "makes", "made",
        "see", "sees", "saw", "seen",
        "buy", "buys", "bought",
        "sell", "sells", "sold",
        "give", "gives", "gave", "given",
        "take", "takes", "took", "taken",
        "run", "runs", "ran",
        "walk", "walks", "walked",
        "attack", "attacks", "attacked",
        "want", "wants", "wanted",
        "believe", "believes", "believed",
        "tell", "tells", "told",
        "read", "reads",
        "write", "writes", "wrote", "written",
        "chase", "chases", "chased",
    ]);

    // Return the positions of every token that looks like a verb.
    public OperationResult<IReadOnlyList<int>> FindPredicatePositions(IReadOnlyList<string> tokens)
    {
        if (tokens is null || tokens.Count == 0)
            return OperationResult<IReadOnlyList<int>>.Fail(
                "Token list is empty", "EMPTY_TOKENS");

        var predicates = new List<int>();

        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            // Strip punctuation and lower-case before any vocabulary lookup.
            string clean = tokens[tokenIndex].Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();
            if (clean.Length == 0) continue;

            // Direct dictionary hit — definitely a verb.
            if (_knownVerbs.Contains(clean))
            {
                predicates.Add(tokenIndex);
                continue;
            }

            // Heuristic — a sufficiently long word ending in a common verb
            // suffix is treated as a verb candidate.
            bool looksLikeVerb = CommonVerbSuffixes.Any(suffix =>
                clean.Length > suffix.Length + 2 && clean.EndsWith(suffix));

            if (looksLikeVerb) predicates.Add(tokenIndex);
        }

        return OperationResult<IReadOnlyList<int>>.Ok(predicates);
    }
}

// ─── ArgumentLabeler ────────────────────────────────────────────────────────
// Given a sentence and the position of one predicate, label every other
// token with an SRL role. The rules are positional plus a few keyword
// modifiers (location/time/manner markers) — sufficient for the canonical
// example sentences and easy to extend with more rules.
public sealed class ArgumentLabeler
{
    // Words that introduce a location phrase. The token following any of
    // these gets an AM-LOC label. The marker itself is also AM-LOC so the
    // whole prepositional group is identifiable.
    private static readonly HashSet<string> LocationMarkers = new(StringComparer.OrdinalIgnoreCase)
    { "in", "on", "at", "from", "to", "near", "behind", "inside", "outside" };

    // Words that introduce a time phrase.
    private static readonly HashSet<string> TimeMarkers = new(StringComparer.OrdinalIgnoreCase)
    { "yesterday", "today", "tomorrow", "now", "then", "before", "after",
      "morning", "evening", "night", "year", "month", "week" };

    // Adverbs that describe how an action is performed.
    private static readonly HashSet<string> MannerMarkers = new(StringComparer.OrdinalIgnoreCase)
    { "quickly", "slowly", "carefully", "easily", "suddenly", "quietly", "loudly" };

    // Label every token relative to the predicate at predicateIndex.
    public OperationResult<IReadOnlyList<SemanticToken>> LabelArguments(
        IReadOnlyList<string> tokens,
        int                   predicateIndex)
    {
        if (tokens is null || tokens.Count == 0)
            return OperationResult<IReadOnlyList<SemanticToken>>.Fail(
                "Token list is empty", "EMPTY_TOKENS");

        if (predicateIndex < 0 || predicateIndex >= tokens.Count)
            return OperationResult<IReadOnlyList<SemanticToken>>.Fail(
                $"Predicate index {predicateIndex} out of range", "INVALID_PREDICATE_INDEX");

        var annotated = new List<SemanticToken>(tokens.Count);

        // Toggle that flips once we have walked past the predicate position;
        // it tells us whether the next non-modifier token is A0 or A1.
        bool pastPredicate = false;

        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            string surface = tokens[tokenIndex];
            string clean   = surface.Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();

            string? role;

            if (tokenIndex == predicateIndex)
            {
                // The predicate itself gets the verb label "V".
                role          = "V";
                pastPredicate = true;
            }
            else if (MannerMarkers.Contains(clean))
            {
                role = "AM-MNR (manner modifier — how the action happens)";
            }
            else if (TimeMarkers.Contains(clean))
            {
                role = "AM-TMP (temporal modifier — when the action happens)";
            }
            else if (LocationMarkers.Contains(clean))
            {
                role = "AM-LOC (locative modifier — where the action happens)";
            }
            else if (!pastPredicate)
            {
                // Token sits before the predicate — by default the agent.
                role = "A0 (Agent — who performs the action)";
            }
            else
            {
                // Token sits after the predicate — by default the theme.
                role = "A1 (Theme — what receives the action)";
            }

            annotated.Add(new SemanticToken
            {
                OriginalText     = surface,
                Position         = tokenIndex,
                SemanticFamily   = "SRL",
                RoleWithinFamily = role,
            });
        }

        return OperationResult<IReadOnlyList<SemanticToken>>.Ok(annotated);
    }
}

// ─── SrlAnnotator ───────────────────────────────────────────────────────────
// End-to-end pipeline: tokenize, find the first predicate, label arguments.
// When the sentence has no predicate the algorithm reports a graceful
// no-predicate annotation rather than an error.
public sealed class SrlAnnotator
{
    private readonly PredicateFinder   _predicateFinder;
    private readonly ArgumentLabeler   _argumentLabeler;

    public SrlAnnotator(PredicateFinder predicateFinder, ArgumentLabeler argumentLabeler)
    {
        _predicateFinder = predicateFinder ?? throw new ArgumentNullException(nameof(predicateFinder));
        _argumentLabeler = argumentLabeler ?? throw new ArgumentNullException(nameof(argumentLabeler));
    }

    public Task<OperationResult<SemanticAnnotation>> AnnotateAsync(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return Task.FromResult(OperationResult<SemanticAnnotation>.Fail(
                "Sentence is empty", "EMPTY_SENTENCE"));

        // Whitespace tokenization — sufficient for English example sentences.
        var tokens = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        var predicateResult = _predicateFinder.FindPredicatePositions(tokens);
        if (!predicateResult.IsSuccess)
            return Task.FromResult(predicateResult.MapError<SemanticAnnotation>());

        // No predicate found — emit the tokens with no roles, no error.
        if (predicateResult.Value!.Count == 0)
        {
            var unlabelled = tokens
                .Select((text, index) => new SemanticToken { OriginalText = text, Position = index })
                .ToList();

            return Task.FromResult(OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
            {
                OriginalSentence = sentence,
                Tokens           = unlabelled,
                AlgorithmName    = "SRL",
            }));
        }

        // Use the first predicate; multi-predicate sentences are out of scope here.
        int predicateIndex = predicateResult.Value![0];

        var labelResult = _argumentLabeler.LabelArguments(tokens, predicateIndex);
        if (!labelResult.IsSuccess)
            return Task.FromResult(labelResult.MapError<SemanticAnnotation>());

        return Task.FromResult(OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
        {
            OriginalSentence = sentence,
            Tokens           = labelResult.Value!,
            AlgorithmName    = "SRL",
            PredicateText    = tokens[predicateIndex],
        }));
    }
}

// ─── SrlService ─────────────────────────────────────────────────────────────
public sealed class SrlService : BaseService, ISemanticAnnotator
{
    private readonly SrlAnnotator _annotator;

    public string AlgorithmName => "SRL";

    public SrlService(
        SrlAnnotator         annotator,
        ISemanticStorage     storage,
        ISemanticEventLogger eventLogger)
        : base(storage, eventLogger)
    {
        _annotator = annotator ?? throw new ArgumentNullException(nameof(annotator));
    }

    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(
        string operationId, string sentence)
    {
        // Reuse cached output when this operationId was processed before.
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
                ["predicate"] = result.Value!.PredicateText ?? string.Empty,
            }));

        Idempotency.Record(operationId, new Dictionary<string, object>
        {
            ["annotation"] = result.Value!,
        });

        return result;
    }
}
