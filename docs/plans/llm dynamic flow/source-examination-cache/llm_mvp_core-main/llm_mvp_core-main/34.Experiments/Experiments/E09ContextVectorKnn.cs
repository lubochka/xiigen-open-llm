using Microsoft.CodeAnalysis.CSharp;
using VectorSearch;

namespace Experiments;

// ─── E09: 5D structural context vector + HNSW K-NN vs bigram on AST tokens ─────
//
// Hypothesis: representing each token's position in the AST as a 5-dimensional
// vector and retrieving K=5 nearest-neighbour contexts improves next-AST-node
// prediction over a plain bigram.
//
// Tokens  : Roslyn SyntaxNode kinds (same as E06 enhanced side).
// Baseline: bigram on AST node-kind tokens.
// Enhanced: HNSW K-NN (K=5) with voting on AST kind tokens, using 5D structural
//           vector [kind/200, parentKind/200, depth/20, posInParent/10, siblingCount/10].
// Fallback: if K-NN returns no candidates, bigram prediction.
public sealed class E09ContextVectorKnn
{
    private const int K = 5;

    public OperationResult<ExperimentResult> Run(string csharpSource, int scale = 1)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<ExperimentResult>.Fail("Empty C# source for E09");

        // ── Build AST token sequence with structural vectors ───────────────────
        var (tokens, vectors) = ExtractAstVectors(csharpSource);

        if (tokens.Count < 20)
            return OperationResult<ExperimentResult>.Fail("Too few AST nodes for E09");

        int split      = (int)(tokens.Count * 0.8);
        var trainToks  = tokens.Take(split).ToList();
        var valToks    = tokens.Skip(split).ToList();
        var trainVecs  = vectors.Take(split).ToList();
        var valVecs    = vectors.Skip(split).ToList();

        // ── Baseline: bigram on AST node kinds ────────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t0          = System.Diagnostics.Stopwatch.GetTimestamp();

        var bigram = new BigramMarkov();
        bigram.Train(trainToks);

        int baseCorrect = 0, baseTotal = valToks.Count - 1;
        for (int i = 0; i < baseTotal; i++)
        {
            var pred = bigram.Predict(valToks[i]);
            if (pred.IsSuccess && pred.Value == valToks[i + 1]) baseCorrect++;
        }
        double baselineAcc   = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0;
        double baselineMs    = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: HNSW K-NN ───────────────────────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t1          = System.Diagnostics.Stopwatch.GetTimestamp();

        // Build index: each training position i → vector for i, label = next AST kind token i+1
        var index       = new HnswIndex(m: 8, efConstruction: 50, efSearch: 20);
        var nextTokenAt = new Dictionary<string, int>();  // id → next token

        for (int i = 0; i < trainToks.Count - 1; i++)
        {
            string nodeId = i.ToString();
            index.Add(trainVecs[i], nodeId);
            nextTokenAt[nodeId] = trainToks[i + 1];
        }

        int enhCorrect = 0, enhTotal = valToks.Count - 1;
        for (int i = 0; i < enhTotal; i++)
        {
            int predicted = PredictKnn(index, valVecs[i], nextTokenAt, bigram, valToks[i]);
            if (predicted == valToks[i + 1]) enhCorrect++;
        }
        double enhancedAcc   = enhTotal > 0 ? enhCorrect / (double)enhTotal : 0;
        double enhancedMs    = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E09",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineAcc,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = tokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedAcc,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = tokens.Count
            },
            Notes = $"astNodes={tokens.Count}, kinds={tokens.Max() + 1}, k={K}, " +
                    $"trainSize={trainToks.Count - 1}, " +
                    $"baseAcc={baselineAcc * 100:F1}%, knnAcc={enhancedAcc * 100:F1}%"
        });
    }

    // Extract AST node kinds + 5D structural vectors for each node in the source.
    private static (List<int> tokens, List<float[]> vectors) ExtractAstVectors(string source)
    {
        var tree  = CSharpSyntaxTree.ParseText(source);
        var nodes = tree.GetRoot().DescendantNodes().ToList();

        if (nodes.Count == 0) return ([], []);

        var kinds   = nodes.Select(n => (int)n.Kind()).Distinct().OrderBy(x => x).ToList();
        var kindMap = kinds.Select((k, i) => (k, i)).ToDictionary(p => p.k, p => p.i);
        int numKinds = kinds.Count;

        var tokens  = new List<int>(nodes.Count);
        var vectors = new List<float[]>(nodes.Count);

        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            int kindId = kindMap[(int)node.Kind()];

            // Parent kind (0 if root)
            int parentKindId = 0;
            if (node.Parent != null && kindMap.TryGetValue((int)node.Parent.Kind(), out int pk))
                parentKindId = pk;

            // Depth in tree
            int depth = 0;
            var walker = node.Parent;
            while (walker != null) { depth++; walker = walker.Parent; }

            // Position among siblings
            int posInParent = 0, siblingCount = 0;
            if (node.Parent != null)
            {
                var siblings = node.Parent.ChildNodes().ToList();
                siblingCount = siblings.Count;
                posInParent  = Math.Max(0, siblings.IndexOf(node));
            }

            tokens.Add(kindId);
            vectors.Add(new float[]
            {
                kindId      / (float)Math.Max(1, numKinds),
                parentKindId / (float)Math.Max(1, numKinds),
                Math.Min(depth, 20) / 20.0f,
                Math.Min(posInParent,  10) / 10.0f,
                Math.Min(siblingCount, 10) / 10.0f,
            });
        }

        return (tokens, vectors);
    }

    // Predict next AST token using K-NN vote; fall back to bigram if index empty.
    private static int PredictKnn(
        HnswIndex index,
        float[] queryVec,
        Dictionary<string, int> nextTokenAt,
        BigramMarkov bigram,
        int curToken)
    {
        if (index.Count == 0)
        {
            var fb = bigram.Predict(curToken);
            return fb.IsSuccess ? fb.Value : 0;
        }

        var neighbours = index.Search(queryVec, K);
        if (neighbours.Count == 0)
        {
            var fb = bigram.Predict(curToken);
            return fb.IsSuccess ? fb.Value : 0;
        }

        // Vote: majority next token among K nearest
        var votes = new Dictionary<int, int>();
        foreach (var (id, _) in neighbours)
        {
            if (nextTokenAt.TryGetValue(id, out int next))
                votes[next] = votes.GetValueOrDefault(next, 0) + 1;
        }
        return votes.Count > 0 ? votes.MaxBy(kv => kv.Value).Key : 0;
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
