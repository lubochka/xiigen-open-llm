namespace SemanticNlp;

// ─── Subproject 28 — FrameNet (frame semantics) ─────────────────────────────
// Charles Fillmore (1976) proposed that words cluster around conceptual
// situations called frames. The verbs "attack", "assault", "ambush" all
// activate the Attack frame; the frame defines roles such as Attacker, Victim,
// Weapon. This subproject implements a small but working FrameNet annotator:
// a lexicon, a frame detector, a role filler, and a service that hooks into
// the SemanticNlpDispatcher.

// ─── FrameNetLexicon ────────────────────────────────────────────────────────
// Two-way mapping between words and frames. Built from a Dictionary of
// frameName → list of words. Implements ISemanticLexicon so the same fabric
// pattern works for every algorithm in this subproject.
public sealed class FrameNetLexicon : ISemanticLexicon
{
    // Lookup: lowercase word → list of frames the word can activate.
    private readonly Dictionary<string, List<string>> _wordToFrames;

    // Lookup: frame name → list of member words (in their original case).
    private readonly Dictionary<string, List<string>> _frameToWords;

    // Build a lexicon directly from an in-memory dictionary. The dictionary
    // is copied defensively so the caller can mutate the original safely.
    public FrameNetLexicon(IReadOnlyDictionary<string, List<string>> framesData)
    {
        // Build the forward index frame → words.
        _frameToWords = framesData.ToDictionary(
            keyValue => keyValue.Key,
            keyValue => keyValue.Value.ToList());

        // Build the reverse index word → frames using case-insensitive keys.
        _wordToFrames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (frameName, words) in framesData)
        {
            foreach (var word in words)
            {
                // Normalize the word so callers do not need to match case.
                string normalizedWord = word.ToLowerInvariant();

                if (!_wordToFrames.TryGetValue(normalizedWord, out var frameList))
                {
                    frameList = [];
                    _wordToFrames[normalizedWord] = frameList;
                }

                // Avoid duplicate frame entries when the same word appears more than once.
                if (!frameList.Contains(frameName))
                    frameList.Add(frameName);
            }
        }
    }

    // Convenience factory that ships with a small but useful built-in frame set.
    // The default set covers the canonical FrameNet examples used by tests
    // (Attack, Commerce_buy, Motion, Statement, Communication).
    public static FrameNetLexicon CreateDefault() => new(new Dictionary<string, List<string>>
    {
        // Attack frame — verbs and nouns describing hostile action.
        ["Attack"]       = ["attack", "assault", "ambush", "bomb", "raid", "strike", "attacked"],

        // Commerce_buy frame — a buyer acquires goods.
        ["Commerce_buy"] = ["buy", "purchase", "acquire", "bought", "purchased"],

        // Motion frame — entity changes location.
        ["Motion"]       = ["move", "travel", "go", "run", "walk", "ran", "walked", "went"],

        // Statement frame — speaker conveys a message.
        ["Statement"]    = ["say", "tell", "state", "said", "told", "stated"],

        // Communication frame — broader speech-act umbrella.
        ["Communication"] = ["speak", "communicate", "discuss", "spoke", "discussed"],
    });

    // Return the frames that can be activated by the given word.
    // An empty list (not a failure) is returned when the word is unknown — that
    // is the normal case for any word outside the lexicon's vocabulary.
    public Task<OperationResult<IReadOnlyList<string>>> FindFramesForWordAsync(string word)
    {
        // Defensive null check — the word is required.
        if (string.IsNullOrEmpty(word))
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
                (IReadOnlyList<string>)Array.Empty<string>()));

        // Use the case-insensitive lookup built in the constructor.
        if (!_wordToFrames.TryGetValue(word, out var frames))
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
                (IReadOnlyList<string>)Array.Empty<string>()));

        // Return a snapshot copy so callers cannot mutate our internal state.
        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)frames.ToList()));
    }

    // FrameNet does not produce synsets — return an empty list per the contract.
    public Task<OperationResult<IReadOnlyList<string>>> FindSynsetsForWordAsync(string word)
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)Array.Empty<string>()));

    // List every word that belongs to a given frame.
    // Returns a failure when the frame name is not registered.
    public Task<OperationResult<IReadOnlyList<string>>> GetFrameMembersAsync(string frameName)
    {
        if (!_frameToWords.TryGetValue(frameName, out var words))
            return Task.FromResult(OperationResult<IReadOnlyList<string>>.Fail(
                $"Frame '{frameName}' not found in the lexicon", "FRAME_NOT_FOUND"));

        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok(
            (IReadOnlyList<string>)words.ToList()));
    }
}

// ─── FrameDetector ──────────────────────────────────────────────────────────
// Picks the most likely frame for a sentence. The heuristic is simple: every
// known word casts a vote for each frame it belongs to, and the frame with
// the most votes wins. The first sentence word that belongs to the winner
// becomes the trigger word reported back to the caller.
public sealed class FrameDetector
{
    // Lexicon used to look up frames for words. Injected via constructor.
    private readonly ISemanticLexicon _lexicon;

    public FrameDetector(ISemanticLexicon lexicon)
    {
        // Defensive contract assertion — the lexicon is mandatory.
        _lexicon = lexicon ?? throw new ArgumentNullException(nameof(lexicon));
    }

    // Detect the frame and the trigger word for the given token list.
    public async Task<OperationResult<(string FrameName, string TriggerWord)>>
        DetectFrameAsync(IReadOnlyList<string> tokens)
    {
        // Empty input is a business failure — return a clear error code.
        if (tokens is null || tokens.Count == 0)
            return OperationResult<(string, string)>.Fail(
                "Token list is empty — nothing to analyze", "EMPTY_TOKENS");

        // Tally of votes accumulated for each candidate frame.
        var frameVotes = new Dictionary<string, int>();

        // Walk every token and add one vote per frame it belongs to.
        foreach (var rawToken in tokens)
        {
            // Strip surrounding punctuation and lower-case before lookup.
            string cleanToken = rawToken.Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();
            if (cleanToken.Length == 0) continue;

            var framesResult = await _lexicon.FindFramesForWordAsync(cleanToken);
            if (!framesResult.IsSuccess) continue;

            foreach (var frameName in framesResult.Value!)
                frameVotes[frameName] = frameVotes.GetValueOrDefault(frameName) + 1;
        }

        // No vote registered — no frame could be selected.
        if (frameVotes.Count == 0)
            return OperationResult<(string, string)>.Fail(
                "No word in the sentence belongs to any known frame", "NO_FRAME_DETECTED");

        // Pick the frame with the highest vote count; ties broken by name to be deterministic.
        string winningFrame = frameVotes
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.Ordinal)
            .First().Key;

        // The trigger is the first token that belongs to the winning frame.
        string triggerWord = string.Empty;
        foreach (var rawToken in tokens)
        {
            string cleanToken = rawToken.Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();
            if (cleanToken.Length == 0) continue;

            var framesResult = await _lexicon.FindFramesForWordAsync(cleanToken);
            if (framesResult.IsSuccess && framesResult.Value!.Contains(winningFrame))
            {
                triggerWord = cleanToken;
                break;
            }
        }

        return OperationResult<(string, string)>.Ok((winningFrame, triggerWord));
    }
}

// ─── FrameElementFiller ─────────────────────────────────────────────────────
// Assigns a role label to every token relative to the trigger word. The role
// table is small and learning-grade — production FrameNet uses syntactic
// dependencies to do this; we use simple positional heuristics.
public sealed class FrameElementFiller
{
    // Per-frame role map: (token-before-trigger-role, token-after-trigger-role).
    private readonly IReadOnlyDictionary<string, (string BeforeTrigger, string AfterTrigger)> _frameRoles;

    // Default constructor seeds the canonical frame roles used by tests.
    public FrameElementFiller()
        : this(new Dictionary<string, (string, string)>
        {
            // Attack: agent before, victim after.
            ["Attack"]        = ("Attacker", "Victim"),
            // Commerce_buy: buyer before, goods after.
            ["Commerce_buy"]  = ("Buyer", "Goods"),
            // Motion: theme before (entity that moves), goal after.
            ["Motion"]        = ("Theme", "Goal"),
            // Statement: speaker before, message after.
            ["Statement"]     = ("Speaker", "Message"),
            // Communication: speaker before, addressee after.
            ["Communication"] = ("Speaker", "Addressee"),
        })
    { }

    // Constructor that accepts an arbitrary role table — used in tests.
    public FrameElementFiller(IReadOnlyDictionary<string, (string BeforeTrigger, string AfterTrigger)> roles)
    {
        _frameRoles = roles;
    }

    // Walk the tokens and emit a SemanticToken for each one. The token at
    // triggerIndex receives "Predicate"; tokens before/after pick up the role
    // pair declared for the frame. Punctuation is preserved with no role.
    public OperationResult<IReadOnlyList<SemanticToken>> FillRoles(
        IReadOnlyList<string> tokens,
        string                frameName,
        int                   triggerIndex)
    {
        if (tokens is null || tokens.Count == 0)
            return OperationResult<IReadOnlyList<SemanticToken>>.Fail(
                "Token list is empty", "EMPTY_TOKENS");

        if (triggerIndex < 0 || triggerIndex >= tokens.Count)
            return OperationResult<IReadOnlyList<SemanticToken>>.Fail(
                $"Trigger index {triggerIndex} out of range", "INVALID_TRIGGER_INDEX");

        // Look up the role pair; if the frame has no declared roles we still
        // emit Predicate for the trigger so downstream consumers know it.
        bool hasRoles = _frameRoles.TryGetValue(frameName, out var roles);

        var annotated = new List<SemanticToken>(tokens.Count);

        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            string surface = tokens[tokenIndex];

            // Decide the role for this position relative to the trigger.
            string? role;
            if (tokenIndex == triggerIndex)
                role = "Predicate";
            else if (hasRoles && tokenIndex < triggerIndex)
                role = roles.BeforeTrigger;
            else if (hasRoles && tokenIndex > triggerIndex)
                role = roles.AfterTrigger;
            else
                role = null;

            annotated.Add(new SemanticToken
            {
                OriginalText     = surface,
                Position         = tokenIndex,
                SemanticFamily   = frameName,
                RoleWithinFamily = role,
            });
        }

        return OperationResult<IReadOnlyList<SemanticToken>>.Ok(annotated);
    }
}

// ─── FrameNetAnnotator ──────────────────────────────────────────────────────
// Glues detector + filler into one pipeline: tokenize the sentence, run the
// detector, find the trigger position, fill the roles, return the annotation.
public sealed class FrameNetAnnotator
{
    private readonly FrameDetector       _detector;
    private readonly FrameElementFiller  _filler;

    public FrameNetAnnotator(FrameDetector detector, FrameElementFiller filler)
    {
        _detector = detector ?? throw new ArgumentNullException(nameof(detector));
        _filler   = filler   ?? throw new ArgumentNullException(nameof(filler));
    }

    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return OperationResult<SemanticAnnotation>.Fail(
                "Sentence is empty", "EMPTY_SENTENCE");

        // Whitespace tokenization is sufficient for this learning implementation.
        var tokens = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        // Detect the frame; if no frame matched, return an unlabelled annotation.
        var frameResult = await _detector.DetectFrameAsync(tokens);
        if (!frameResult.IsSuccess)
        {
            var unlabelled = tokens
                .Select((text, index) => new SemanticToken { OriginalText = text, Position = index })
                .ToList();

            return OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
            {
                OriginalSentence = sentence,
                Tokens           = unlabelled,
                AlgorithmName    = "FrameNet",
            });
        }

        var (frameName, triggerWord) = frameResult.Value;

        // Locate the trigger inside the token list (case- and punctuation-insensitive).
        int triggerIndex = -1;
        for (int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
        {
            string clean = tokens[tokenIndex].Trim('.', ',', '!', '?', ';', ':').ToLowerInvariant();
            if (clean == triggerWord)
            {
                triggerIndex = tokenIndex;
                break;
            }
        }

        // If the trigger word vanished after cleaning (should not happen) bail to position 0.
        if (triggerIndex < 0) triggerIndex = 0;

        var rolesResult = _filler.FillRoles(tokens, frameName, triggerIndex);
        if (!rolesResult.IsSuccess)
            return rolesResult.MapError<SemanticAnnotation>();

        return OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
        {
            OriginalSentence = sentence,
            Tokens           = rolesResult.Value!,
            AlgorithmName    = "FrameNet",
            FrameName        = frameName,
            PredicateText    = triggerWord,
        });
    }
}

// ─── FrameNetService ────────────────────────────────────────────────────────
// Service-layer wrapper: extends BaseService, implements ISemanticAnnotator,
// adds idempotency and storage save-before-event ordering (DNA rules 7 & 8).
public sealed class FrameNetService : BaseService, ISemanticAnnotator
{
    private readonly FrameNetAnnotator _annotator;

    public string AlgorithmName => "FrameNet";

    public FrameNetService(
        FrameNetAnnotator    annotator,
        ISemanticStorage     storage,
        ISemanticEventLogger eventLogger)
        : base(storage, eventLogger)
    {
        _annotator = annotator ?? throw new ArgumentNullException(nameof(annotator));
    }

    public async Task<OperationResult<SemanticAnnotation>> AnnotateAsync(
        string operationId, string sentence)
    {
        // DNA rule 7 — short-circuit when this operationId was already processed.
        if (Idempotency.TryGetResult(operationId, out var cached)
            && cached.TryGetValue("annotation", out var cachedObj)
            && cachedObj is SemanticAnnotation cachedAnnotation)
        {
            return OperationResult<SemanticAnnotation>.Ok(cachedAnnotation);
        }

        var annotationResult = await _annotator.AnnotateAsync(sentence);
        if (!annotationResult.IsSuccess) return annotationResult;

        // DNA rule 8 — persist before dispatching the completion event.
        await Storage.SaveAnnotationAsync(operationId, annotationResult.Value!);

        // Fire the standard event envelope (DNA rule 9).
        EventLogger.Log(new StandardEvent(
            EventType:   "semantic.annotation.completed",
            OperationId: operationId,
            OccurredAt:  DateTimeOffset.UtcNow,
            Payload:     new Dictionary<string, object>
            {
                ["algorithm"] = AlgorithmName,
                ["sentence"]  = sentence,
                ["frame"]     = annotationResult.Value!.FrameName ?? string.Empty,
            }));

        // Record the result for future idempotent calls.
        Idempotency.Record(operationId, new Dictionary<string, object>
        {
            ["annotation"] = annotationResult.Value!,
        });

        return annotationResult;
    }
}
