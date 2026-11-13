using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Tokenizer;

namespace ExperimentsV2;

// ─── E01: AST node types (Roslyn) vs BPE tokens — 3-gram prediction ──────────

// Hypothesis: SyntaxKind enum values (~200 types) form a much smaller vocabulary
// than BPE sub-word tokens (~500–2 000).  A 3-gram model over AST types should
// have lower perplexity and higher top-1 accuracy than a 3-gram over BPE tokens
// because the C# grammar constrains which node types can follow which.
//
// Baseline: BPE-tokenise the source, build a 3-gram, compute perplexity + top1.
// Enhanced: Roslyn parse → DescendantNodes() → Kind().ToString() → integer index
//           → build a 3-gram, compute perplexity + top1.
//
// Scaling signal: vocab_saturation_ratio = new unique token types on this tier /
//                 total unique types so far.  AST saturates first (grammar is finite).
public sealed class E01AstTokenizer
{
    // Run Tier 1 experiment on the provided C# source code string.
    // 'bpeVocabSize' caps the BPE vocabulary for training (256 = byte level only,
    // 512 = minimal merges).  Smaller caps keep training fast at Tier 1.
    public OperationResult<ExperimentV2Result> Run(
        string csharpSource,
        int    bpeVocabSize = 512,
        int    tier         = 1)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<ExperimentV2Result>.Fail("Empty source for E01");

        // ── Baseline: BPE 3-gram ─────────────────────────────────────────────────
        var t0 = Stopwatch();
        var tokenizer = new BpeTokenizer();
        tokenizer.Train(csharpSource, Math.Max(256, bpeVocabSize));
        var bpeTokens = tokenizer.Encode(csharpSource);
        if (bpeTokens.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail("Too few BPE tokens");

        int bpeSplit = (int)(bpeTokens.Count * 0.8);
        var bpeTrain = bpeTokens.Take(bpeSplit).ToList();
        var bpeVal   = bpeTokens.Skip(bpeSplit).ToList();

        var baseTrigram = new TrigramModelV2();
        baseTrigram.Train(bpeTrain);
        if (bpeVal.Count < 3) bpeVal = bpeTokens.TakeLast(3).ToList();
        var baseEval = baseTrigram.Evaluate(bpeVal);
        if (!baseEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Baseline eval: " + baseEval.ErrorMessage);

        double baselineMs = ElapsedMs(t0);
        (double bpePerp, double bpeTop1, _, _) = baseEval.Value!;

        // ── Enhanced: Roslyn AST 3-gram ──────────────────────────────────────────
        var t1 = Stopwatch();
        var astResult = ParseAstTokens(csharpSource);
        if (!astResult.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("AST parse: " + astResult.ErrorMessage);

        (List<int> astTokens, List<string> kinds) = astResult.Value!;
        if (astTokens.Count < 3)
            return OperationResult<ExperimentV2Result>.Fail("Too few AST nodes");

        int astSplit  = (int)(astTokens.Count * 0.8);
        var astTrain  = astTokens.Take(astSplit).ToList();
        var astVal    = astTokens.Skip(astSplit).ToList();
        if (astVal.Count < 3) astVal = astTokens.TakeLast(3).ToList();

        var enhTrigram = new TrigramModelV2();
        enhTrigram.Train(astTrain);
        var enhEval = enhTrigram.Evaluate(astVal);
        if (!enhEval.IsSuccess)
            return OperationResult<ExperimentV2Result>.Fail("Enhanced eval: " + enhEval.ErrorMessage);

        double enhancedMs = ElapsedMs(t1);
        (double astPerp, double astTop1, _, _) = enhEval.Value!;

        // Vocab saturation ratio: measures how close AST vocabulary is to saturation.
        // Ratio = 1 means fully saturated (no new types after train split).
        // Computed as distinct types in val / distinct types in train.
        int astTrainTypes = astTrain.Distinct().Count();
        int astValNewTypes = astVal.Distinct().Except(astTrain.Distinct()).Count();
        double satRatio = astTrainTypes == 0 ? 1.0 : astValNewTypes / (double)astTrainTypes;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId        = "E01-AST-TOKENIZER",
            Tier                = tier,
            BaselinePrimary     = bpePerp,
            EnhancedPrimary     = astPerp,
            PrimaryMetricName   = "perplexity_3gram",
            LowerIsBetter       = true,
            ScalingSignal       = satRatio,
            ScalingSignalNote   = "ast_saturation_ratio (lower = vocab more stable)",
            BaselineTimeMs      = baselineMs,
            EnhancedTimeMs      = enhancedMs,
            Notes               = $"bpeVocab={baseTrigram.VocabSize}, astTypes={kinds.Count}, " +
                                  $"bpeTop1={bpeTop1:P1}, astTop1={astTop1:P1}"
        });
    }

    // Parse C# source → normalised integer token IDs + list of distinct kind names.
    public static OperationResult<(List<int> tokens, List<string> distinctKinds)>
        ParseAstTokens(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var raw  = tree.GetRoot()
                       .DescendantNodes()
                       .Select(n => n.Kind().ToString())
                       .ToList();

        if (raw.Count == 0)
            return OperationResult<(List<int>, List<string>)>.Fail("No AST nodes found");

        var distinct    = raw.Distinct().OrderBy(s => s).ToList();
        var kindToIndex = distinct.Select((k, i) => (k, i)).ToDictionary(p => p.k, p => p.i);
        var tokens      = raw.Select(k => kindToIndex[k]).ToList();

        return OperationResult<(List<int>, List<string>)>.Ok((tokens, distinct));
    }

    private static long    Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double  ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
