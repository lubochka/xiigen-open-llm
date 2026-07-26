// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticNlp;

// ─── Subproject 32 — Universal Dependencies (UD) ────────────────────────────
// Universal Dependencies is a cross-language standard for syntactic
// annotation. Every word is assigned a head (the word it depends on) and a
// dependency relation (nsubj, obj, det, etc.). This subproject ships:
//   * a CoNLL-U parser that ingests the standard text format,
//   * a navigator that walks the dependency tree,
//   * a tiny rule-based parser that produces a tree from a raw sentence
//     using a few hand-written rules (good enough for the canonical demo
//     sentences and for the unit tests).

// ─── ConlluParser ───────────────────────────────────────────────────────────
// Parses CoNLL-U formatted blocks (one sentence at a time). The CoNLL-U
// format is tab-separated, with one token per line and ten fields per token.
// Lines starting with '#' are comments; we read the "# text = ..." comment
// to recover the surface sentence.
public sealed class ConlluParser
{
    public OperationResult<DependencyTree> ParseBlock(string conlluBlock)
    {
        if (string.IsNullOrWhiteSpace(conlluBlock))
            return OperationResult<DependencyTree>.Fail(
                "CoNLL-U block is empty", "EMPTY_INPUT");

        var tokens           = new List<DependencyToken>();
        string surfaceText   = string.Empty;

        // Split the block into individual lines. CoNLL-U separates sentences
        // by a blank line, so any single block normally has no blank lines.
        var lines = conlluBlock.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            // Comment lines start with '#'. Pull the surface text out if present.
            if (line.StartsWith('#'))
            {
                const string textPrefix = "# text = ";
                if (line.StartsWith(textPrefix))
                    surfaceText = line[textPrefix.Length..].Trim();
                continue;
            }

            // Token lines are tab-separated with at least 8 mandatory fields.
            var fields = line.Split('\t');
            if (fields.Length < 8) continue;

            // Skip multi-word ("1-2") and empty-node ("1.1") rows — only the
            // single-token rows participate in the dependency tree.
            if (fields[0].Contains('-') || fields[0].Contains('.')) continue;

            if (!int.TryParse(fields[0], out int tokenId)) continue;
            if (!int.TryParse(fields[6], out int headId)) headId = 0;

            tokens.Add(new DependencyToken
            {
                TokenId            = tokenId,
                OriginalForm       = fields[1],
                Lemma              = fields[2],
                UniversalPosTag    = fields[3],
                HeadTokenId        = headId,
                DependencyRelation = fields[7],
            });
        }

        if (tokens.Count == 0)
            return OperationResult<DependencyTree>.Fail(
                "No tokens found in the block", "NO_TOKENS");

        return OperationResult<DependencyTree>.Ok(new DependencyTree
        {
            Tokens           = tokens,
            OriginalSentence = surfaceText,
        });
    }
}

// ─── DependencyTreeNavigator ────────────────────────────────────────────────
// Helpers for walking the dependency tree once it is built.
public sealed class DependencyTreeNavigator
{
    // Return the children of the given token id (every token whose head equals it).
    public IReadOnlyList<DependencyToken> GetChildren(DependencyTree tree, int tokenId)
        => tree.Tokens.Where(token => token.HeadTokenId == tokenId).ToList();

    // Return the head of the given token, or null when the token is the root.
    public DependencyToken? GetHead(DependencyTree tree, int tokenId)
    {
        var token = tree.Tokens.FirstOrDefault(t => t.TokenId == tokenId);
        if (token is null || token.HeadTokenId == 0) return null;
        return tree.Tokens.FirstOrDefault(t => t.TokenId == token.HeadTokenId);
    }

    // Locate the root of the tree (head id == 0). Returns null when missing.
    public DependencyToken? GetRoot(DependencyTree tree)
        => tree.Tokens.FirstOrDefault(t => t.HeadTokenId == 0);
}

// ─── RuleBasedParser ────────────────────────────────────────────────────────
// Builds a small dependency tree from a raw sentence using simple positional
// rules. The first verb-looking token becomes the root; the noun-like word
// that precedes it becomes nsubj; the noun-like word that follows becomes
// obj; determiners attach to the closest following noun. Sufficient for
// canonical example sentences ("The cat chased the mouse").
public sealed class RuleBasedParser
{
    private static readonly HashSet<string> DeterminerWords = new(StringComparer.OrdinalIgnoreCase)
    { "the", "a", "an", "this", "that", "these", "those" };

    private readonly PredicateFinder _predicateFinder;

    public RuleBasedParser(PredicateFinder predicateFinder)
    {
        _predicateFinder = predicateFinder ?? throw new ArgumentNullException(nameof(predicateFinder));
    }

    public OperationResult<DependencyTree> Parse(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return OperationResult<DependencyTree>.Fail(
                "Sentence is empty", "EMPTY_SENTENCE");

        // Strip terminal punctuation but keep mid-sentence punctuation (for now we drop it).
        var rawTokens = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Trim('.', ',', '!', '?', ';', ':'))
            .Where(token => token.Length > 0)
            .ToList();

        if (rawTokens.Count == 0)
            return OperationResult<DependencyTree>.Fail("No usable tokens", "NO_TOKENS");

        // Find the root — the first verb in the sentence.
        var predicateResult = _predicateFinder.FindPredicatePositions(rawTokens);
        if (!predicateResult.IsSuccess)
            return predicateResult.MapError<DependencyTree>();

        int rootIndex = predicateResult.Value!.Count > 0 ? predicateResult.Value![0] : 0;

        // Identify nominal tokens by exclusion — anything that is not a verb and not
        // a determiner is treated as a noun for the purposes of nsubj / obj.
        var verbIndexes = predicateResult.Value!.ToHashSet();
        bool IsDeterminer(int index) => DeterminerWords.Contains(rawTokens[index]);
        bool IsNominal(int index)    => !verbIndexes.Contains(index) && !IsDeterminer(index);

        // Pick nsubj — the closest nominal token before the root.
        int nsubjIndex = -1;
        for (int tokenIndex = rootIndex - 1; tokenIndex >= 0; tokenIndex--)
            if (IsNominal(tokenIndex)) { nsubjIndex = tokenIndex; break; }

        // Pick obj — the closest nominal token after the root.
        int objIndex = -1;
        for (int tokenIndex = rootIndex + 1; tokenIndex < rawTokens.Count; tokenIndex++)
            if (IsNominal(tokenIndex)) { objIndex = tokenIndex; break; }

        // Build DependencyToken records. Token ids are 1-based per CoNLL-U.
        var tokens = new List<DependencyToken>(rawTokens.Count);
        for (int tokenIndex = 0; tokenIndex < rawTokens.Count; tokenIndex++)
        {
            string surface = rawTokens[tokenIndex];

            string upos;
            string deprel;
            int    headId;

            if (tokenIndex == rootIndex)
            {
                upos   = "VERB";
                deprel = "root";
                headId = 0;
            }
            else if (tokenIndex == nsubjIndex)
            {
                upos   = "NOUN";
                deprel = "nsubj";
                headId = rootIndex + 1;
            }
            else if (tokenIndex == objIndex)
            {
                upos   = "NOUN";
                deprel = "obj";
                headId = rootIndex + 1;
            }
            else if (IsDeterminer(tokenIndex))
            {
                // Determiner attaches to the nearest following nominal (nsubj or obj).
                int nominalAfter = -1;
                for (int candidate = tokenIndex + 1; candidate < rawTokens.Count; candidate++)
                    if (IsNominal(candidate)) { nominalAfter = candidate; break; }

                upos   = "DET";
                deprel = "det";
                headId = nominalAfter >= 0 ? nominalAfter + 1 : rootIndex + 1;
            }
            else
            {
                // Anything we cannot classify falls under "dep" — generic dependency.
                upos   = "X";
                deprel = "dep";
                headId = rootIndex + 1;
            }

            tokens.Add(new DependencyToken
            {
                TokenId            = tokenIndex + 1,
                OriginalForm       = surface,
                Lemma              = surface.ToLowerInvariant(),
                UniversalPosTag    = upos,
                HeadTokenId        = headId,
                DependencyRelation = deprel,
            });
        }

        return OperationResult<DependencyTree>.Ok(new DependencyTree
        {
            Tokens           = tokens,
            OriginalSentence = sentence,
        });
    }
}

// ─── UniversalDependenciesAnnotator ─────────────────────────────────────────
// Adapts the dependency tree to the SemanticAnnotation contract by emitting
// one SemanticToken per DependencyToken with the dependency relation as the
// role and "UD" as the family.
public sealed class UniversalDependenciesAnnotator
{
    private readonly RuleBasedParser _parser;

    public UniversalDependenciesAnnotator(RuleBasedParser parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public Task<OperationResult<SemanticAnnotation>> AnnotateAsync(string sentence)
    {
        var treeResult = _parser.Parse(sentence);
        if (!treeResult.IsSuccess)
            return Task.FromResult(treeResult.MapError<SemanticAnnotation>());

        var tree = treeResult.Value!;

        var tokens = new List<SemanticToken>(tree.Tokens.Count);
        for (int tokenIndex = 0; tokenIndex < tree.Tokens.Count; tokenIndex++)
        {
            var depToken = tree.Tokens[tokenIndex];
            tokens.Add(new SemanticToken
            {
                OriginalText     = depToken.OriginalForm,
                Position         = tokenIndex,
                SemanticFamily   = "UD",
                RoleWithinFamily = depToken.DependencyRelation,
            });
        }

        // The root of the tree (DEPREL == "root") is the predicate text we report.
        string? predicate = tree.Tokens
            .FirstOrDefault(t => t.DependencyRelation == "root")?.OriginalForm;

        return Task.FromResult(OperationResult<SemanticAnnotation>.Ok(new SemanticAnnotation
        {
            OriginalSentence = sentence,
            Tokens           = tokens,
            AlgorithmName    = "UniversalDependencies",
            PredicateText    = predicate,
        }));
    }
}

// ─── UniversalDependenciesService ───────────────────────────────────────────
public sealed class UniversalDependenciesService : BaseService, ISemanticAnnotator
{
    private readonly UniversalDependenciesAnnotator _annotator;

    public string AlgorithmName => "UniversalDependencies";

    public UniversalDependenciesService(
        UniversalDependenciesAnnotator annotator,
        ISemanticStorage               storage,
        ISemanticEventLogger           eventLogger)
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
            }));

        Idempotency.Record(operationId, new Dictionary<string, object>
        {
            ["annotation"] = result.Value!,
        });

        return result;
    }
}
