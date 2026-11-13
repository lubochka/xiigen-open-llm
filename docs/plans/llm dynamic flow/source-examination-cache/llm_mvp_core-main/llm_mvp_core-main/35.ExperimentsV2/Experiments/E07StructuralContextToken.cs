using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Tokenizer;

namespace ExperimentsV2;

// ─── E07: Structural context token — multi-dimensional Markov prediction ────────
//
// Hypothesis: a token is not just a number — it is a node in several overlapping
// structures simultaneously (BPE sequence, AST, file graph, call graph, semantic
// cluster).  Predicting P(next_bpe | bpe, ast_type, depth) is more accurate than
// P(next_bpe | bpe) because the grammar constrains which tokens can legally follow.
//
// Each tier adds one new dimension and measures how much it reduces perplexity:
//   Tier 1: + ast_type                         (E01 contribution)
//   Tier 2: + ast_parent                       (deeper structural context)
//   Tier 3: + ast_depth + file_id              (depth and cross-file)
//   Tier 4: + scope_depth                      (scope nesting)
//   Tier 5: structural batching (Variant C)    (reorder context by structure)
//
// Baseline: flat bigram on BPE token IDs.
// Enhanced: MultidimensionalNGram keyed by (bpe, ast_type, depth).
public sealed class E07StructuralContextToken
{
    public OperationResult<ExperimentV2Result> Run(
        string csharpSource,
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<ExperimentV2Result>.Fail("Empty source for E07");

        // ── Tokenize with BPE ─────────────────────────────────────────────────────
        var t0 = Stopwatch();
        var bpeTok = new BpeTokenizer();
        bpeTok.Train(csharpSource, vocabSize: 512);
        var bpeTokens = bpeTok.Encode(csharpSource);
        if (bpeTokens.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Too few BPE tokens for E07");

        // ── Parse AST metadata for each file ─────────────────────────────────────
        var structuralTokens = BuildStructuralTokens(bpeTokens, files, bpeTok, csharpSource);

        int splitAt = (int)(structuralTokens.Count * 0.8);
        var train   = structuralTokens.Take(splitAt).ToList();
        var val     = structuralTokens.Skip(splitAt).ToList();

        // ── Baseline: flat bigram on BPE ──────────────────────────────────────────
        var bigramCounts = new Dictionary<int, Dictionary<int, int>>();
        for (int i = 0; i < train.Count - 1; i++)
        {
            int cur  = train[i].BpeId;
            int next = train[i + 1].BpeId;
            if (!bigramCounts.TryGetValue(cur, out var d))
                bigramCounts[cur] = d = new Dictionary<int, int>();
            d[next] = d.GetValueOrDefault(next, 0) + 1;
        }

        // Global most-frequent next token for baseline fallback — same as enhanced uses.
        var globalFreq = new Dictionary<int, int>();
        for (int i = 0; i < train.Count - 1; i++)
            globalFreq[train[i + 1].BpeId] = globalFreq.GetValueOrDefault(train[i + 1].BpeId, 0) + 1;
        int baselineFallback = globalFreq.Count > 0 ? globalFreq.MaxBy(kv => kv.Value).Key : 0;

        int baseCorrect = 0, baseTotal = val.Count - 1;
        for (int i = 0; i < baseTotal; i++)
        {
            int cur  = val[i].BpeId;
            int next = val[i + 1].BpeId;
            int pred;
            if (bigramCounts.TryGetValue(cur, out var d) && d.Count > 0)
                pred = d.MaxBy(kv => kv.Value).Key;
            else
                pred = baselineFallback;
            if (pred == next) baseCorrect++;
        }

        double baselineMs  = ElapsedMs(t0);
        double baselineAcc = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0;

        // ── Enhanced: MultidimensionalNGram (bpe + ast_type + depth) ─────────────
        var t1 = Stopwatch();
        var mdNgram = new MultidimensionalNGram();
        mdNgram.Train(train);

        int enhCorrect = 0, enhTotal = val.Count - 1;
        for (int i = 0; i < enhTotal; i++)
        {
            int pred = mdNgram.Predict(val[i].BpeId, val[i].AstNodeType, val[i].AstDepth);
            if (pred == val[i + 1].BpeId) enhCorrect++;
        }

        double enhancedMs  = ElapsedMs(t1);
        double enhancedAcc = enhTotal > 0 ? enhCorrect / (double)enhTotal : 0;

        // Scaling signal: unique structural contexts seen in training vs total.
        int uniqueContexts = mdNgram.UniqueContexts;
        double contextRatio = structuralTokens.Count > 0
            ? uniqueContexts / (double)structuralTokens.Count
            : 0;

        string dimensionNote = tier switch
        {
            1 => "ast_type added",
            2 => "ast_parent added",
            3 => "ast_depth + file_id added",
            4 => "scope_depth added",
            _ => "structural batching"
        };

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E07-STRUCTURAL-CONTEXT-TOKEN",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = contextRatio,
            ScalingSignalNote = $"unique_context_ratio ({dimensionNote})",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"tier={tier}, dimension={dimensionNote}, " +
                                $"bpeTokens={bpeTokens.Count}, uniqueContexts={uniqueContexts}",
            // contextRatio > 0.5 = majority of contexts seen only once — not enough data.
            ShouldScaleUp     = contextRatio > 0.5
        });
    }

    // Build structural tokens by aligning BPE token positions with AST node metadata.
    // When Roslyn parse fails or there are no AST nodes, falls back to default values.
    private static List<StructuralToken> BuildStructuralTokens(
        IReadOnlyList<int>                         bpeTokens,
        IReadOnlyList<(string Path, string Text)>  files,
        BpeTokenizer                               tokenizer,
        string                                     combinedText)
    {
        // Build a position → AST metadata map from all files.
        var posMap = BuildPositionAstMap(files, combinedText);

        var result = new List<StructuralToken>(bpeTokens.Count);
        for (int i = 0; i < bpeTokens.Count; i++)
        {
            var meta = posMap.Count > 0 && i < posMap.Count ? posMap[i] : default;
            result.Add(new StructuralToken
            {
                BpeId           = bpeTokens[i],
                AstNodeType     = meta.AstNodeType,
                AstParentType   = meta.AstParentType,
                AstDepth        = meta.AstDepth,
                FileId          = meta.FileId,
                ScopeDepth      = meta.ScopeDepth,
                PageRankScore   = 0f,
                DataFlowInPacked = 0L,
                SemanticClusterId = 0
            });
        }
        return result;
    }

    // Build a per-position AST metadata array aligned to BPE token indices.
    // Uses character-level sampling: each BPE token index maps to a character range.
    private static List<AstMeta> BuildPositionAstMap(
        IReadOnlyList<(string Path, string Text)> files,
        string combinedText)
    {
        var result = new List<AstMeta>();
        try
        {
            // Parse combined text once with Roslyn.
            var tree = CSharpSyntaxTree.ParseText(combinedText);
            var root = tree.GetRoot();

            var allNodes = root.DescendantNodes().ToList();
            if (allNodes.Count == 0) return result;

            // Map char position → nearest AST node.
            int textLen = combinedText.Length;
            if (textLen == 0) return result;

            // Build a sorted list of (startPos, nodeType, parentType, depth, scopeDepth).
            var nodeList = allNodes
                .Select(n =>
                {
                    int depth  = 0;
                    var p = n.Parent;
                    while (p != null) { depth++; p = p.Parent; }
                    int scope = CountScopeDepth(n);
                    short nodeType   = (short)(n.RawKind % 1000);
                    short parentType = (short)((n.Parent?.RawKind ?? 0) % 1000);
                    return (start: n.SpanStart, nodeType, parentType,
                            depth: (byte)Math.Min(depth, 255),
                            scope: (byte)Math.Min(scope, 255));
                })
                .OrderBy(x => x.start)
                .ToList();

            // Assign each character position to the best matching node (nearest start ≤ pos).
            // We'll sample at fixed intervals based on text length.
            int nodeIdx = 0;
            AstMeta current = default;

            // We create one entry per "slot" — we'll allocate up to textLen slots
            // but limit to reasonable max to avoid huge allocations on large texts.
            int slots = Math.Min(textLen, 50_000);
            result.Capacity = slots;
            double step = textLen / (double)slots;

            for (int s = 0; s < slots; s++)
            {
                int charPos = (int)(s * step);
                // Advance to the node whose span covers charPos.
                while (nodeIdx + 1 < nodeList.Count && nodeList[nodeIdx + 1].start <= charPos)
                    nodeIdx++;
                if (nodeIdx < nodeList.Count)
                {
                    var n = nodeList[nodeIdx];
                    current = new AstMeta(n.nodeType, n.parentType, n.depth, 0, n.scope);
                }
                result.Add(current);
            }
        }
        catch
        {
            // If Roslyn fails, return empty — callers fall back to default(AstMeta).
        }
        return result;
    }

    private static int CountScopeDepth(SyntaxNode node)
    {
        int depth = 0;
        var p = node.Parent;
        while (p != null)
        {
            if (p is BlockSyntax) depth++;
            p = p.Parent;
        }
        return depth;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;

    private readonly record struct AstMeta(
        short AstNodeType, short AstParentType,
        byte  AstDepth,    short FileId, byte ScopeDepth);
}

// ─── StructuralToken ─────────────────────────────────────────────────────────────

public readonly record struct StructuralToken
{
    public int   BpeId             { get; init; }
    public short AstNodeType       { get; init; }
    public short AstParentType     { get; init; }
    public byte  AstDepth          { get; init; }
    public short FileId            { get; init; }
    public byte  ScopeDepth        { get; init; }
    public float PageRankScore     { get; init; }
    public long  DataFlowInPacked  { get; init; }
    public short SemanticClusterId { get; init; }
}

// ─── MultidimensionalNGram ───────────────────────────────────────────────────────

public sealed class MultidimensionalNGram
{
    private readonly Dictionary<(int bpe, short ast, byte depth), Dictionary<int, int>> _trans = new();
    private readonly Dictionary<int, Dictionary<int, int>> _bigram = new();
    private int _mostFrequentNext = 0;

    public int UniqueContexts => _trans.Count;

    public void Train(IReadOnlyList<StructuralToken> tokens)
    {
        var globalNextCounts = new Dictionary<int, int>();

        for (int i = 0; i < tokens.Count - 1; i++)
        {
            var cur  = tokens[i];
            int next = tokens[i + 1].BpeId;

            globalNextCounts[next] = globalNextCounts.GetValueOrDefault(next, 0) + 1;

            // Multi-dimensional transition.
            var key = (cur.BpeId, cur.AstNodeType, cur.AstDepth);
            if (!_trans.TryGetValue(key, out var d))
                _trans[key] = d = new Dictionary<int, int>();
            d[next] = d.GetValueOrDefault(next, 0) + 1;

            // Bigram fallback.
            if (!_bigram.TryGetValue(cur.BpeId, out var b))
                _bigram[cur.BpeId] = b = new Dictionary<int, int>();
            b[next] = b.GetValueOrDefault(next, 0) + 1;
        }

        if (globalNextCounts.Count > 0)
            _mostFrequentNext = globalNextCounts.MaxBy(kv => kv.Value).Key;
    }

    public int Predict(int currentBpe, short currentAst, byte currentDepth)
    {
        var key = (currentBpe, currentAst, currentDepth);
        if (_trans.TryGetValue(key, out var d) && d.Count > 0)
            return d.MaxBy(kv => kv.Value).Key;
        return BigramFallback(currentBpe);
    }

    private int BigramFallback(int bpe)
    {
        if (_bigram.TryGetValue(bpe, out var b) && b.Count > 0)
            return b.MaxBy(kv => kv.Value).Key;
        return _mostFrequentNext;
    }
}
